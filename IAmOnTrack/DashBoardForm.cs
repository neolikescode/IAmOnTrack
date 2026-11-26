using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IAmOnTrack.UI;
namespace IAmOnTrack
{
    //The main dashboard window. It is borderless (custom title bar) but keeps resizing, minimise,
    //maximise / restore, close and drag. This file holds behaviour and window handling; the visual
    //layout is built in DashBoardForm.Layout.cs.
    public partial class DashBoardForm : Form
    {
        // ---- Placeholder profile details: the project has no user / settings system yet ----
        private const string UserFirstName = "Neo";
        private const string UserFullName = "Neo Kgatla";
        private const string UserRole = "Student";

        // ---- Win32 constants for the borderless window ----
        private const int ResizeBorder = 6;
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point point);

        // ---- Existing application objects (the dashboard only READS from these) ----
        private readonly CMarkTracker _tracker;
        private readonly CActivityLog _activityLog;
        private readonly CDashboardStatistics _statistics;

        private readonly WheelForwarder _wheelForwarder;

        //Default constructor used by Program.cs: the form owns a fresh tracker and activity log.
        public DashBoardForm() : this(new CMarkTracker(), new CActivityLog())
        {
        }

        //Lets another class (e.g. a future data-loading step) hand the dashboard an existing tracker
        public DashBoardForm(CMarkTracker tracker, CActivityLog activityLog)
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

            _wheelForwarder = new WheelForwarder();
            Application.AddMessageFilter(_wheelForwarder);
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

        // ================== Dashboard data ==================

        //Re-reads the tracker and updates every number, ring and list on the dashboard
        public void RefreshDashboard()
        {
            try
            {
                DateTime now = DateTime.Now;
                int year = now.Year;
                int semester = CDashboardStatistics.GetSemesterForDate(now);

                _header.SetText(GetGreeting(now), UserFirstName + ".", "Your academic progress, organised in one place.");

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

        // ================== Navigation to the existing forms ==================

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

        // ================== Custom window behaviour ==================

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX; //lets the taskbar minimise / restore a borderless window
                return cp;
            }
        }

        //The thin padding ring around the form belongs to the form itself, so it is where the
        //resize handles live. Returning the edge codes gives native resizing with the right cursors.
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST && WindowState == FormWindowState.Normal)
            {
                base.WndProc(ref m);
                if ((int)m.Result == HTCLIENT)
                {
                    int lp = unchecked((int)(long)m.LParam);
                    Point screenPoint = new Point((short)(lp & 0xFFFF), (short)((lp >> 16) & 0xFFFF));
                    m.Result = (IntPtr)HitTestEdges(PointToClient(screenPoint));
                }
                return;
            }
            base.WndProc(ref m);
        }

        private int HitTestEdges(Point p)
        {
            int corner = 16;
            bool left = p.X <= ResizeBorder;
            bool right = p.X >= ClientSize.Width - ResizeBorder;
            bool top = p.Y <= ResizeBorder;
            bool bottom = p.Y >= ClientSize.Height - ResizeBorder;

            if (top)
            {
                if (p.X <= corner) return HTTOPLEFT;
                if (p.X >= ClientSize.Width - corner) return HTTOPRIGHT;
                return HTTOP;
            }
            if (bottom)
            {
                if (p.X <= corner) return HTBOTTOMLEFT;
                if (p.X >= ClientSize.Width - corner) return HTBOTTOMRIGHT;
                return HTBOTTOM;
            }
            if (left)
            {
                if (p.Y <= corner) return HTTOPLEFT;
                if (p.Y >= ClientSize.Height - corner) return HTBOTTOMLEFT;
                return HTLEFT;
            }
            if (right)
            {
                if (p.Y <= corner) return HTTOPRIGHT;
                if (p.Y >= ClientSize.Height - corner) return HTBOTTOMRIGHT;
                return HTRIGHT;
            }
            return HTCLIENT;
        }

        //The ring is only needed in the normal state; when maximised the content fills the screen
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            bool maximised = WindowState == FormWindowState.Maximized;
            Padding = maximised ? Padding.Empty : new Padding(ResizeBorder);
            if (_maximizeButton != null)
            {
                _maximizeButton.Kind = maximised ? IconKind.Restore : IconKind.Maximize;
            }
            Invalidate();
        }

        //Paints the page background plus the navy strip behind the sidebar so the resize ring is seamless
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.OffWhite);
            using (SolidBrush navy = new SolidBrush(Theme.Navy))
            {
                e.Graphics.FillRectangle(navy, 0, 0, Padding.Left + SidebarPanel.PreferredWidth, Height);
            }
        }

        private void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                //keep the taskbar visible when maximising a borderless window
                MaximizedBounds = Screen.FromHandle(Handle).WorkingArea;
                WindowState = FormWindowState.Maximized;
            }
        }

        //Drag the window by the title bar (double-click toggles maximise)
        private void DragHandle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            if (e.Clicks == 2)
            {
                ToggleMaximize();
                return;
            }
            if (WindowState == FormWindowState.Maximized)
            {
                //restore, keeping the window under the cursor like a normal title bar
                Point cursor = Cursor.Position;
                float ratio = (float)(cursor.X - Bounds.X) / Math.Max(1, Bounds.Width);
                WindowState = FormWindowState.Normal;
                Location = new Point(cursor.X - (int)(Width * ratio), cursor.Y - 16);
            }
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        //Ctrl + K moves the cursor to the search box
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.K))
            {
                _header.Search.FocusInput();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Application.RemoveMessageFilter(_wheelForwarder);
            base.OnFormClosed(e);
        }

        //Custom-painted controls cannot take focus, so mouse-wheel messages are passed to the control under
        //the cursor, which lets the dashboard scroll when the window is small.
        private class WheelForwarder : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_MOUSEWHEEL)
                {
                    IntPtr target = WindowFromPoint(Cursor.Position);
                    if (target != IntPtr.Zero && target != m.HWnd && Control.FromHandle(target) != null)
                    {
                        SendMessage(target, m.Msg, m.WParam, m.LParam);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
