using System;
using System.Collections.Generic;
using System.Windows.Forms;
using IAmOnTrack.UI;

namespace IAmOnTrack
{
    //"Enter marks" pop-up: choose a module and one of its assessments, then record the mark obtained and the maximum mark.
    //The existing CAssessments.EnterMark method validates the numbers.
    public partial class MarksForm : ModernDialog
    {
        private readonly CAcademicRecords _records;
        private readonly SelectField _module;
        private readonly SelectField _assessment;
        private readonly InputField _obtained;
        private readonly InputField _maximum;

        public MarksForm() : this(new CMarkTracker(), new CActivityLog())
        {
        }

        public MarksForm(CMarkTracker tracker, CActivityLog activityLog)
            : base("Enter marks", "Record your obtained mark for an existing assessment.", 24, "Save mark", "Cancel", FieldsHeight(4), 500)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException(nameof(tracker), "Mark tracker cannot be null");
            }
            if (activityLog == null)
            {
                throw new ArgumentNullException(nameof(activityLog), "Activity log cannot be null");
            }
            _records = new CAcademicRecords(tracker, activityLog);

            InitializeComponent();

            _module = new SelectField("Module");
            _module.Placeholder = "No modules yet \u2013 add one from the Records page";
            _module.SelectionChanged += (s, e) => LoadAssessments();

            _assessment = new SelectField("Assessment");
            _assessment.SelectionChanged += (s, e) => LoadMarkFields();

            _obtained = new InputField("Mark obtained");
            _maximum = new InputField("Maximum mark");

            StackFields(_module, _assessment, _obtained, _maximum);
            LoadModules();
            Shown += (s, e) => { if (_assessment.ItemCount > 0) _obtained.FocusInput(); };
        }

        private void LoadModules()
        {
            List<KeyValuePair<string, object>> items = new List<KeyValuePair<string, object>>();
            foreach (CModules module in _records.GetModules(new CRecordFilter()))
            {
                items.Add(new KeyValuePair<string, object>(
                    module.ModuleCode + " \u2014 " + module.ModuleName + "  (" + module.AcademicYear + " \u00B7 S" + module.Semester + ")", module));
            }
            _module.SetItems(items, null);
            LoadAssessments();
        }

        //Lists the chosen module's assessments, preferring one that has no mark yet
        private void LoadAssessments()
        {
            CModules module = _module.SelectedValue as CModules;
            List<KeyValuePair<string, object>> items = new List<KeyValuePair<string, object>>();
            CAssessments firstPending = null;

            if (module != null)
            {
                foreach (CAssessments assessment in module.Assessments)
                {
                    string status = assessment.IsMark
                        ? "marked " + assessment.MarkObtained.ToString("0.##") + "/" + assessment.MaximumMark.ToString("0.##")
                        : "pending";
                    items.Add(new KeyValuePair<string, object>(assessment.AssessmentName + "  \u00B7  " + status, assessment));
                    if (firstPending == null && !assessment.IsMark)
                    {
                        firstPending = assessment;
                    }
                }
            }

            _assessment.Placeholder = module == null ? "Choose a module first" : "This module has no assessments yet";
            _assessment.SetItems(items, firstPending);
            LoadMarkFields();
        }

        //Pre-fills the mark boxes from the chosen assessment
        private void LoadMarkFields()
        {
            CAssessments assessment = _assessment.SelectedValue as CAssessments;
            if (assessment == null)
            {
                _obtained.Value = "";
                _maximum.Value = "";
                return;
            }
            _obtained.Value = assessment.IsMark ? assessment.MarkObtained.ToString("0.##") : "";
            _maximum.Value = assessment.MaximumMark.ToString("0.##");
        }

        protected override void OnPrimaryClick()
        {
            CModules module = _module.SelectedValue as CModules;
            CAssessments assessment = _assessment.SelectedValue as CAssessments;
            if (module == null || assessment == null)
            {
                MessageBox.Show(this, "Please choose a module and one of its assessments first.", "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double obtained;
            double maximum;
            if (!InputField.TryParseNumber(_obtained.Value, out obtained) || !InputField.TryParseNumber(_maximum.Value, out maximum))
            {
                MessageBox.Show(this, "Please enter both marks as numbers, for example 45 and 50.", "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _records.UpdateMark(new CAssessmentRecord(module, assessment), obtained, maximum);
                DialogResult = DialogResult.OK;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
