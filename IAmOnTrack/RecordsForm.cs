using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using IAmOnTrack.UI;

namespace IAmOnTrack
{
    //Which tab the Records page shows (the numbers match the tab positions)
    public enum RecordsTab
    {
        All = 0,
        Modules = 1,
        Assessments = 2,
        Marks = 3
    }

    //The Academic Records page. It works on the SAME CMarkTracker / CActivityLog as the dashboard and
    //leaves searching, filtering, paging and editing to CAcademicRecords (business logic).
    public partial class RecordsForm : BorderlessForm
    {
        private enum EmptyAction
        {
            AddRecord,
            AddAssessment,
            EnterMarks,
            ClearFilters
        }

        private readonly CMarkTracker _tracker;
        private readonly CActivityLog _activityLog;
        private readonly CAcademicRecords _records;
        private readonly CRecordFilter _filter = new CRecordFilter();
        private readonly CPager _pager = new CPager();

        private CFileManager _fileManager;   //created only when the user exports (its constructor creates a folder)
        private RecordsTab _tab;
        private EmptyAction _emptyAction;

        //Raised when the user clicks "Dashboard" in the sidebar
        public event EventHandler DashboardRequested;

        //True when the window is being closed to go back to the dashboard (rather than to quit the app)
        public bool ReturningToDashboard { get; private set; }

        public RecordsForm() : this(new CMarkTracker(), new CActivityLog(), RecordsTab.All)
        {
        }

        public RecordsForm(CMarkTracker tracker, CActivityLog activityLog, RecordsTab startTab)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException(nameof(tracker), "Mark tracker cannot be null");
            }
            if (activityLog == null)
            {
                throw new ArgumentNullException(nameof(activityLog), "Activity log cannot be null");
            }

            _tracker = tracker;
            _activityLog = activityLog;
            _records = new CAcademicRecords(tracker, activityLog);

            InitializeComponent();
            BuildInterface();

            _tab = startTab;
            _tabs.SelectSilently((int)startTab);
            _sidebar.SetActive(NavTargetFor(startTab));
            RefreshRecords();
        }

        protected override SearchBox GlobalSearch
        {
            get { return _header == null ? null : _header.Search; }
        }

        // ================== Refreshing the page ==================

        //Re-reads the tracker and redraws the overview, filters and table
        public void RefreshRecords()
        {
            try
            {
                UpdateOverview();
                UpdateFilterOptions();
                ApplyView();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, "The records could not be loaded: " + ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, "The records could not be loaded: " + ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //The four overview cards and the tab counts - always the totals, not affected by filters
        private void UpdateOverview()
        {
            CDashboardStatistics stats = _records.Statistics;
            _modulesCard.Value = stats.ModuleCount;
            _assessmentsCard.Value = stats.AssessmentCount;
            _marksCard.Value = stats.MarksRecordedCount;

            double? average = stats.OverallAverage;
            _averageCard.ValueText = average.HasValue ? average.Value.ToString("0.0") + "%" : "\u2014";

            _tabs.SetCount((int)RecordsTab.All, stats.AssessmentCount);
            _tabs.SetCount((int)RecordsTab.Modules, stats.ModuleCount);
            _tabs.SetCount((int)RecordsTab.Assessments, stats.AssessmentCount);
            _tabs.SetCountText((int)RecordsTab.Marks, stats.MarksRecordedCount + "/" + stats.AssessmentCount); //marks recorded out of all assessments
        }

        //Fills the filter drop-downs from the real data, keeping the current choices when they still exist
        private void UpdateFilterOptions()
        {
            List<KeyValuePair<string, object>> years = new List<KeyValuePair<string, object>>();
            years.Add(new KeyValuePair<string, object>("All", null));
            foreach (int year in _records.GetAcademicYears())
            {
                years.Add(new KeyValuePair<string, object>(year.ToString(), year));
            }
            _yearFilter.SetItems(years, _filter.AcademicYear);

            List<KeyValuePair<string, object>> semesters = new List<KeyValuePair<string, object>>();
            semesters.Add(new KeyValuePair<string, object>("All", null));
            semesters.Add(new KeyValuePair<string, object>("Semester 1", 1));
            semesters.Add(new KeyValuePair<string, object>("Semester 2", 2));
            _semesterFilter.SetItems(semesters, _filter.Semester);

            List<KeyValuePair<string, object>> modules = new List<KeyValuePair<string, object>>();
            modules.Add(new KeyValuePair<string, object>("All", null));
            foreach (string code in _records.GetModuleCodes())
            {
                modules.Add(new KeyValuePair<string, object>(code, code));
            }
            _moduleFilter.SetItems(modules, _filter.ModuleCode);

            List<KeyValuePair<string, object>> types = new List<KeyValuePair<string, object>>();
            types.Add(new KeyValuePair<string, object>("All", null));
            foreach (string type in _records.GetAssessmentTypes())
            {
                types.Add(new KeyValuePair<string, object>(type, type));
            }
            _typeFilter.SetItems(types, _filter.AssessmentType);

            List<KeyValuePair<string, object>> statuses = new List<KeyValuePair<string, object>>();
            statuses.Add(new KeyValuePair<string, object>("All", RecordStatus.All));
            statuses.Add(new KeyValuePair<string, object>("Marked", RecordStatus.Marked));
            statuses.Add(new KeyValuePair<string, object>("Pending", RecordStatus.Pending));
            _statusFilter.SetItems(statuses, _filter.Status);

            SyncFilterFromControls(); //a choice that no longer exists (e.g. a deleted module) falls back to "All"
        }

        private void SyncFilterFromControls()
        {
            _filter.AcademicYear = (int?)_yearFilter.SelectedValue;
            _filter.Semester = (int?)_semesterFilter.SelectedValue;
            _filter.ModuleCode = (string)_moduleFilter.SelectedValue;
            _filter.AssessmentType = (string)_typeFilter.SelectedValue;
            _filter.Status = _statusFilter.SelectedValue == null ? RecordStatus.All : (RecordStatus)_statusFilter.SelectedValue;
        }

        //Applies the search + filters for the current tab and shows one page of rows
        private void ApplyView()
        {
            bool modulesTab = _tab == RecordsTab.Modules;

            //assessment type and status only apply to assessment rows
            _typeFilter.Enabled = !modulesTab;
            _statusFilter.Enabled = !modulesTab;

            _pager.PageSize = _grid.RowCapacity;
            List<GridRow> rows = new List<GridRow>();
            int total;

            if (modulesTab)
            {
                IReadOnlyList<CModules> modules = _records.GetModules(_filter);
                total = modules.Count;
                _pager.TotalItems = total;
                _grid.SetColumns(ModuleColumns());
                _grid.Actions = RowActions.View | RowActions.Delete;
                rows.AddRange(modules.Skip(_pager.FirstIndex).Take(_pager.PageSize).Select(BuildModuleRow));
            }
            else
            {
                IReadOnlyList<CAssessmentRecord> records = _records.GetAssessmentRecords(_filter);
                total = records.Count;
                _pager.TotalItems = total;
                _grid.SetColumns(ColumnsFor(_tab));
                _grid.Actions = _tab == RecordsTab.Marks ? (RowActions.View | RowActions.Edit)
                                                         : (RowActions.View | RowActions.Edit | RowActions.Delete);
                rows.AddRange(records.Skip(_pager.FirstIndex).Take(_pager.PageSize).Select(r => BuildAssessmentRow(r, _tab)));
            }

            _grid.SetRows(rows);
            if (total == 0)
            {
                ConfigureEmptyState();
            }
            else
            {
                _grid.SetEmptyState(IconKind.Inbox, null, "", null);
            }

            _pagerBar.SetState(total == 0 ? 0 : _pager.FirstIndex + 1, _pager.EndIndex, total, _pager.HasPrevious, _pager.HasNext);
        }

        // ================== Columns and rows ==================

        private static IEnumerable<GridColumn> ModuleColumns()
        {
            return new[]
            {
                new GridColumn("Module", 1.1f), new GridColumn("Name", 2.2f), new GridColumn("Year", 0.7f),
                new GridColumn("Semester", 1f), new GridColumn("Assessments", 1.1f),
                new GridColumn("Weight used", 1f, true), new GridColumn("Performance", 1.2f, true)
            };
        }

        private static IEnumerable<GridColumn> ColumnsFor(RecordsTab tab)
        {
            switch (tab)
            {
                case RecordsTab.Assessments:
                    return new[]
                    {
                        new GridColumn("Module", 1.1f), new GridColumn("Assessment", 1.8f), new GridColumn("Type", 1.3f),
                        new GridColumn("Weight", 0.9f, true), new GridColumn("Year / Semester", 1.5f), new GridColumn("Status", 1.2f)
                    };
                case RecordsTab.Marks:
                    return new[]
                    {
                        new GridColumn("Module", 1.1f), new GridColumn("Assessment", 1.8f), new GridColumn("Obtained", 1f, true),
                        new GridColumn("Maximum", 1f, true), new GridColumn("Percentage", 1.2f, true),
                        new GridColumn("Contribution", 1.2f, true), new GridColumn("Status", 1.2f)
                    };
                default:
                    return new[]
                    {
                        new GridColumn("Module", 1.1f), new GridColumn("Assessment", 1.8f), new GridColumn("Type", 1.3f),
                        new GridColumn("Mark", 1.2f), new GridColumn("Percentage", 1.2f, true), new GridColumn("Weight", 0.9f, true)
                    };
            }
        }

        private GridRow BuildModuleRow(CModules module)
        {
            double weightUsed = module.Assessments.Sum(a => a.AssessmentWeight);
            double? performance = _records.Statistics.GetModulePerformance(module);
            int marked = module.Assessments.Count(a => a.IsMark);

            return new GridRow(module,
                new GridCell(module.ModuleCode, CellStyle.Strong),
                new GridCell(module.ModuleName),
                new GridCell(module.AcademicYear.ToString()),
                new GridCell("Semester " + module.Semester),
                new GridCell(module.Assessments.Count + " (" + marked + " marked)"),
                new GridCell(FormatNumber(weightUsed) + "%"),
                performance.HasValue ? new GridCell(FormatPercent(performance.Value), CellStyle.Strong) : new GridCell("\u2014", CellStyle.Muted));
        }

        private GridRow BuildAssessmentRow(CAssessmentRecord record, RecordsTab tab)
        {
            CAssessments a = record.Assessment;
            GridCell module = new GridCell(record.Module.ModuleCode, CellStyle.Strong);
            GridCell name = new GridCell(a.AssessmentName);
            GridCell percentage = record.Percentage.HasValue ? new GridCell(FormatPercent(record.Percentage.Value), CellStyle.Strong) : new GridCell("\u2014", CellStyle.Muted);
            GridCell status = record.HasMark ? new GridCell("Marked", CellStyle.PillSuccess) : new GridCell("Pending", CellStyle.PillNeutral);
            GridCell weight = new GridCell(FormatNumber(a.AssessmentWeight) + "%");

            if (tab == RecordsTab.Assessments)
            {
                return new GridRow(record, module, name, new GridCell(record.AssessmentType, CellStyle.Muted), weight,
                    new GridCell(record.Module.AcademicYear + " \u00B7 Semester " + record.Module.Semester, CellStyle.Muted), status);
            }

            if (tab == RecordsTab.Marks)
            {
                GridCell obtained = record.HasMark ? new GridCell(FormatNumber(a.MarkObtained), CellStyle.Strong) : new GridCell("\u2014", CellStyle.Muted);
                GridCell maximum = record.HasMark ? new GridCell(FormatNumber(a.MaximumMark)) : new GridCell("\u2014", CellStyle.Muted);
                GridCell contribution = record.WeightedContribution.HasValue ? new GridCell(FormatPercent(record.WeightedContribution.Value)) : new GridCell("\u2014", CellStyle.Muted);
                return new GridRow(record, module, name, obtained, maximum, percentage, contribution, status);
            }

            GridCell mark = record.HasMark ? new GridCell(FormatNumber(a.MarkObtained) + "/" + FormatNumber(a.MaximumMark)) : new GridCell("Pending", CellStyle.PillNeutral);
            return new GridRow(record, module, name, new GridCell(record.AssessmentType, CellStyle.Muted), mark, percentage, weight);
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.##");
        }

        private static string FormatPercent(double value)
        {
            return value.ToString("0.00") + "%";
        }

        // ================== Empty states ==================

        private void ConfigureEmptyState()
        {
            bool assessmentTab = _tab != RecordsTab.Modules;

            if (_records.Statistics.ModuleCount == 0)
            {
                _emptyAction = EmptyAction.AddRecord;
                _grid.SetEmptyState(IconKind.Layers, "No modules registered yet",
                    "Your modules will appear here, together with their assessments and marks. Use Add Record to get started.", "Add Record");
            }
            else if (assessmentTab && _records.Statistics.AssessmentCount == 0)
            {
                _emptyAction = EmptyAction.AddAssessment;
                _grid.SetEmptyState(IconKind.Clipboard, "No assessments created yet",
                    "Create an assessment for one of your modules to start tracking your marks.", "Add Assessment");
            }
            else if (_tab == RecordsTab.Marks && _records.Statistics.MarksRecordedCount == 0 && !_filter.IsActive)
            {
                _emptyAction = EmptyAction.EnterMarks;
                _grid.SetEmptyState(IconKind.BarChart, "No marks recorded yet",
                    "Once you receive a result, enter it here to see your percentages and average.", "Enter Marks");
            }
            else
            {
                _emptyAction = EmptyAction.ClearFilters;
                _grid.SetEmptyState(IconKind.Search, "No records found",
                    "Nothing matches your current search or filters. Try different words, or clear the filters to see everything again.", "Clear search & filters");
            }
        }

        private void Grid_EmptyActionClicked(object sender, EventArgs e)
        {
            switch (_emptyAction)
            {
                case EmptyAction.AddRecord:
                    ShowAddMenu(_grid);
                    break;
                case EmptyAction.AddAssessment:
                    ShowChildForm(() => new AssessmentForm());
                    break;
                case EmptyAction.EnterMarks:
                    ShowChildForm(() => new MarksForm());
                    break;
                case EmptyAction.ClearFilters:
                    ClearFilters();
                    break;
            }
        }

        private void ClearFilters()
        {
            _filter.Clear();
            _header.Search.ClearQuery();
            _pager.Reset();
            RefreshRecords();
        }

        // ================== Search, tabs, filters, paging ==================

        private void Search_QueryChanged(object sender, EventArgs e)
        {
            _filter.SearchText = _header.Search.QueryText;
            _pager.Reset();
            ApplyView();
        }

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            _tab = (RecordsTab)_tabs.SelectedIndex;
            _sidebar.SetActive(NavTargetFor(_tab));
            _pager.Reset();
            ApplyView();
        }

        private void Filter_SelectionChanged(object sender, EventArgs e)
        {
            SyncFilterFromControls();
            _pager.Reset();
            ApplyView();
        }

        //The page size follows the window height, so re-page when the table is resized
        private void Grid_Resize(object sender, EventArgs e)
        {
            if (_pager.PageSize != _grid.RowCapacity)
            {
                ApplyView();
            }
        }

        private static NavTarget? NavTargetFor(RecordsTab tab)
        {
            switch (tab)
            {
                case RecordsTab.Modules: return NavTarget.Modules;
                case RecordsTab.Assessments: return NavTarget.Assessments;
                case RecordsTab.Marks: return NavTarget.Marks;
                default: return null;
            }
        }

        private void Sidebar_Navigate(object sender, NavigationEventArgs e)
        {
            switch (e.Target)
            {
                case NavTarget.Dashboard:
                    ReturningToDashboard = true;
                    if (DashboardRequested != null)
                    {
                        DashboardRequested(this, EventArgs.Empty);
                    }
                    break;
                case NavTarget.Modules:
                    _tabs.SelectedIndex = (int)RecordsTab.Modules;
                    break;
                case NavTarget.Assessments:
                    _tabs.SelectedIndex = (int)RecordsTab.Assessments;
                    break;
                case NavTarget.Marks:
                    _tabs.SelectedIndex = (int)RecordsTab.Marks;
                    break;
            }
        }

        // ================== Add (existing forms) ==================

        private void AddRecordButton_Click(object sender, EventArgs e)
        {
            ShowAddMenu(_header.PrimaryButton);
        }

        private void ShowAddMenu(Control anchor)
        {
            ContextMenuStrip menu = ModernMenu.Create(new[] { "Add Assessment", "Enter Marks" }, -1, index =>
            {
                if (index == 0)
                {
                    ShowChildForm(() => new AssessmentForm());
                }
                else
                {
                    ShowChildForm(() => new MarksForm());
                }
            });
            menu.MinimumSize = new System.Drawing.Size(Math.Max(anchor.Width, 190), 0);
            menu.Closed += (s, e) => menu.Dispose();
            menu.Show(anchor, new System.Drawing.Point(0, anchor.Height + 4));
        }

        //Opens one of the EXISTING entry forms as a dialog, then reloads the records when it closes
        private void ShowChildForm(Func<Form> createForm)
        {
            try
            {
                using (Form form = createForm())
                {
                    form.ShowDialog(this);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            RefreshRecords();
        }

        // ================== View / edit / delete ==================

        private void Grid_ActionInvoked(object sender, GridActionEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case RowActions.View:
                        ShowDetails(e.Row.Tag);
                        return;
                    case RowActions.Edit:
                        EditRecord(e.Row.Tag);
                        break;
                    case RowActions.Delete:
                        DeleteRecord(e.Row.Tag);
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            RefreshRecords();
        }

        private void ShowDetails(object tag)
        {
            List<KeyValuePair<string, string>> fields = new List<KeyValuePair<string, string>>();
            string title;
            string subtitle;

            CModules module = tag as CModules;
            CAssessmentRecord record = tag as CAssessmentRecord;

            if (module != null)
            {
                double? performance = _records.Statistics.GetModulePerformance(module);
                title = "Module details";
                subtitle = module.ModuleCode + " \u2014 " + module.ModuleName;
                fields.Add(Field("Module code", module.ModuleCode));
                fields.Add(Field("Module name", module.ModuleName));
                fields.Add(Field("Academic year", module.AcademicYear.ToString()));
                fields.Add(Field("Semester", module.Semester.ToString()));
                fields.Add(Field("Assessments", module.Assessments.Count + " (" + module.Assessments.Count(a => a.IsMark) + " marked)"));
                fields.Add(Field("Weight allocated", FormatNumber(module.Assessments.Sum(a => a.AssessmentWeight)) + "% of 100%"));
                fields.Add(Field("Performance so far", performance.HasValue ? FormatPercent(performance.Value) : "\u2014"));
            }
            else if (record != null)
            {
                CAssessments a = record.Assessment;
                title = "Assessment details";
                subtitle = record.Module.ModuleCode + " \u2014 " + a.AssessmentName;
                fields.Add(Field("Module", record.Module.ModuleCode + " \u2014 " + record.Module.ModuleName));
                fields.Add(Field("Assessment", a.AssessmentName));
                fields.Add(Field("Type", record.AssessmentType));
                fields.Add(Field("Year and semester", record.Module.AcademicYear + " \u00B7 Semester " + record.Module.Semester));
                fields.Add(Field("Weight", FormatNumber(a.AssessmentWeight) + "% of the semester mark"));
                fields.Add(Field("Status", record.HasMark ? "Marked" : "Pending"));
                fields.Add(Field("Mark", record.HasMark ? FormatNumber(a.MarkObtained) + " / " + FormatNumber(a.MaximumMark) : "Not recorded yet"));
                fields.Add(Field("Percentage", record.Percentage.HasValue ? FormatPercent(record.Percentage.Value) : "\u2014"));
                fields.Add(Field("Contribution", record.WeightedContribution.HasValue ? FormatPercent(record.WeightedContribution.Value) + " of the semester mark" : "\u2014"));
            }
            else
            {
                return;
            }

            using (RecordDetailsDialog dialog = new RecordDetailsDialog(title, subtitle, fields))
            {
                dialog.ShowDialog(this);
            }
        }

        private static KeyValuePair<string, string> Field(string label, string value)
        {
            return new KeyValuePair<string, string>(label, value);
        }

        //Modules cannot be edited because the existing CModules class has no method for changing its details
        private void EditRecord(object tag)
        {
            CAssessmentRecord record = tag as CAssessmentRecord;
            if (record == null)
            {
                return;
            }

            string subtitle = record.Module.ModuleCode + " \u2014 " + record.Assessment.AssessmentName;
            if (_tab == RecordsTab.Marks)
            {
                double? current = record.HasMark ? (double?)record.Assessment.MarkObtained : null;
                using (EditMarkDialog dialog = new EditMarkDialog(subtitle, current, record.Assessment.MaximumMark,
                    (obtained, maximum) => _records.UpdateMark(record, obtained, maximum)))
                {
                    dialog.ShowDialog(this);
                }
            }
            else
            {
                using (EditAssessmentDialog dialog = new EditAssessmentDialog(subtitle, record.Assessment.AssessmentName, record.Assessment.AssessmentWeight,
                    (name, weight) => _records.UpdateAssessment(record, name, weight)))
                {
                    dialog.ShowDialog(this);
                }
            }
        }

        private void DeleteRecord(object tag)
        {
            CModules module = tag as CModules;
            CAssessmentRecord record = tag as CAssessmentRecord;

            if (module != null)
            {
                string message = module.ModuleCode + " and its " + module.Assessments.Count + " assessment(s) will be removed from your records. " +
                                 "Excel files you already exported are not changed.";
                if (ConfirmDialog.Ask(this, "Delete module?", message, "Delete module", true))
                {
                    _records.DeleteModule(module);
                }
            }
            else if (record != null)
            {
                string message = record.Assessment.AssessmentName + " (" + record.Module.ModuleCode + ")" +
                                 (record.HasMark ? ", including its recorded mark," : "") + " will be removed. Excel files you already exported are not changed.";
                if (ConfirmDialog.Ask(this, "Delete assessment?", message, "Delete assessment", true))
                {
                    _records.DeleteAssessment(record);
                }
            }
        }

        // ================== Excel export (existing CFileManager) ==================

        private void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileManager == null)
                {
                    _fileManager = new CFileManager();
                }

                int exported = _records.ExportModules(_filter, _fileManager);
                if (exported == 0)
                {
                    MessageBox.Show(this, "There are no modules to export for the selected year, semester and module.", "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, "Exported " + exported + (exported == 1 ? " module" : " modules") + " to Excel.\n\nFolder: " + _fileManager.BaseDirectory,
                        "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(this, "The Excel file could not be written. Make sure it is not open in another program.\n\n" + ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(this, "IAmOnTrack does not have permission to save to the export folder.\n\n" + ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, "The export could not be completed: " + ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, "The export could not be completed: " + ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(this, "The export folder path is not supported: " + ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
