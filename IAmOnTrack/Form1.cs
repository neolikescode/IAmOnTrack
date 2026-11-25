using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using IAmOnTrack.UI;

namespace IAmOnTrack
{
    //The main dashboard window. Window chrome (borderless, resize, drag, maximise...) comes from
    //BorderlessForm; the visual layout is built in Form1.Layout.cs.
    public partial class Form1 : BorderlessForm
    {
        // ---- Existing application objects (shared with the Records form) ----
        private readonly CMarkTracker _tracker;
        private readonly CActivityLog _activityLog;
        private readonly CDashboardStatistics _statistics;

        //Default constructor used by Program.cs: the form owns a fresh tracker and activity log.
        public Form1() : this(new CMarkTracker(), new CActivityLog())
        {
        }

        //Lets another class (e.g. a future data-loading step) hand the dashboard an existing tracker
        public Form1(CMarkTracker tracker, CActivityLog activityLog)
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
            _statistics = new CDashboardStatistics(_tracker);

            InitializeComponent();
            BuildInterface();
            RefreshDashboard();
        }

        //The tracker and activity log are exposed so AssessmentForm / MarksForm can be given the same
        //objects later (e.g. new AssessmentForm(Tracker, ActivityLog)).
        public CMarkTracker Tracker
        {
            get { return _tracker; }
        }

        public CActivityLog ActivityLog
        {
            get { return _activityLog; }
        }

        protected override SearchBox GlobalSearch
        {
            get { return _header == null ? null : _header.Search; }
        }

        // ================== Dashboard data ==================

        //Re-reads the tracker and updates every number, ring and list on the dashboard
        public void RefreshDashboard()
        {
            try
            {
                DateTime now = DateTime.Now;
                int year = now.Year;
                int semester = CDashboardStatistics.GetSemesterForDate(now);

                _header.SetText(GetGreeting(now), CurrentUser.FirstName + ".", "Your academic progress, organised in one place.");

                _modulesCard.Value = _statistics.ModuleCount;
                _assessmentsCard.Value = _statistics.AssessmentCount;
                _marksCard.Value = _statistics.MarksRecordedCount;

                _semesterCard.SetProgress(_statistics.GetSemesterProgress(year, semester), "Semester " + semester + ", " + year);
                _performanceCard.SetAverage(_statistics.OverallAverage, _statistics.ModulesWithMarksCount);
                _nextStepsCard.SetSteps(_statistics.GetNextSteps());
                _activityCard.SetEntries(_activityLog.GetRecent(8));
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, "The dashboard could not be updated: " + ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, "The dashboard could not be updated: " + ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string GetGreeting(DateTime now)
        {
            if (now.Hour < 12)
            {
                return "Good morning.";
            }
            if (now.Hour < 18)
            {
                return "Good afternoon.";
            }
            return "Good evening.";
        }

        // ================== Navigation ==================

        private void AddAssessmentButton_Click(object sender, EventArgs e)
        {
            ShowChildForm(() => new AssessmentForm());
        }

        private void EnterMarksButton_Click(object sender, EventArgs e)
        {
            ShowChildForm(() => new MarksForm());
        }

        //Opens an existing form as a dialog, then refreshes the dashboard when it closes.
        //Expected exceptions are shown to the user here (never inside the model / logic classes).
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

            RefreshDashboard();
        }

        //Sidebar: Modules / Assessments / My Marks open the Records page on the matching tab
        private void Sidebar_Navigate(object sender, NavigationEventArgs e)
        {
            switch (e.Target)
            {
                case NavTarget.Modules:
                    ShowRecords(RecordsTab.Modules);
                    break;
                case NavTarget.Assessments:
                    ShowRecords(RecordsTab.Assessments);
                    break;
                case NavTarget.Marks:
                    ShowRecords(RecordsTab.Marks);
                    break;
            }
        }

        //Opens the Records page in the same window position and hides the dashboard behind it.
        //Both pages are given the SAME tracker and activity log, so there is only ever one set of data.
        private void ShowRecords(RecordsTab tab)
        {
            try
            {
                RecordsForm records = new RecordsForm(_tracker, _activityLog, tab);
                records.CopyWindowFrom(this);

                records.DashboardRequested += (s, e) =>
                {
                    CopyWindowFrom(records);
                    Show();
                    RefreshDashboard();
                    records.Close();
                };

                //closing the Records window with its X button closes the whole application
                records.FormClosed += (s, e) =>
                {
                    if (!records.ReturningToDashboard)
                    {
                        Close();
                    }
                };

                Hide();
                records.Show();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Show();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Show();
            }
        }
    }
}
