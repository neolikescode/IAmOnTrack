using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Fixed-width Midnight Navy sidebar: logo, navigation, user profile and version.
    //Only "Dashboard" is functional at this stage; the other items are visual placeholders.
    public class SidebarPanel : Panel
    {
        public const int PreferredWidth = 248;

        private readonly ToolTip _tip = new ToolTip();
        private readonly NavButton[] _navigation;
        private readonly NavButton _modulesButton;
        private readonly NavButton _assessmentsButton;
        private readonly NavButton _marksButton;
        private string _displayName = "";
        private string _roleText = "";

        public NavButton DashboardButton { get; private set; }

        //Raised when the user presses the mouse on the logo area (so the window can be dragged from there)
        public event MouseEventHandler DragRequested;

        //Raised when the user clicks Dashboard, Modules, Assessments or My Marks
        public event EventHandler<NavigationEventArgs> Navigate;

        public SidebarPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = Theme.Navy;
            Width = PreferredWidth;

            DashboardButton = new NavButton("Dashboard", IconKind.Home);
            DashboardButton.Selected = true;
            NavButton modules = new NavButton("Modules", IconKind.Layers);
            NavButton assessments = new NavButton("Assessments", IconKind.Clipboard);
            NavButton marks = new NavButton("My Marks", IconKind.BarChart);
            NavButton data = new NavButton("Data & Files", IconKind.Folder);
            NavButton settings = new NavButton("Settings", IconKind.Gear);

            _navigation = new[] { DashboardButton, modules, assessments, marks, data, settings };
            _modulesButton = modules;
            _assessmentsButton = assessments;
            _marksButton = marks;

            foreach (NavButton button in _navigation)
            {
                Controls.Add(button);
            }

            //Data & Files and Settings are still visual placeholders
            _tip.SetToolTip(data, "Coming soon");
            _tip.SetToolTip(settings, "Coming soon");

            DashboardButton.Click += (s, e) => RaiseNavigate(NavTarget.Dashboard);
            modules.Click += (s, e) => RaiseNavigate(NavTarget.Modules);
            assessments.Click += (s, e) => RaiseNavigate(NavTarget.Assessments);
            marks.Click += (s, e) => RaiseNavigate(NavTarget.Marks);
        }

        private void RaiseNavigate(NavTarget target)
        {
            if (Navigate != null)
            {
                Navigate(this, new NavigationEventArgs(target));
            }
        }

        //Highlights the item for the page that is currently open
        public void SetActive(NavTarget? target)
        {
            DashboardButton.Selected = target == NavTarget.Dashboard;
            _modulesButton.Selected = target == NavTarget.Modules;
            _assessmentsButton.Selected = target == NavTarget.Assessments;
            _marksButton.Selected = target == NavTarget.Marks;
        }

        public void SetUser(string displayName, string roleText)
        {
            _displayName = displayName ?? "";
            _roleText = roleText ?? "";
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_navigation == null)
            {
                return;
            }
            int x = 16;
            int width = Math.Max(40, Width - 32);
            int[] tops = { 116, 168, 220, 272, 354, 406 };
            for (int i = 0; i < _navigation.Length; i++)
            {
                _navigation[i].SetBounds(x, tops[i], width, 46);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Y < 100 && DragRequested != null)
            {
                DragRequested(this, e);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(Theme.Navy);

            //logo block
            Icons.DrawLogo(g, new RectangleF(22, 26, 46, 46));
            Theme.Text(g, "IAmOnTrack", Theme.Font(22, FontStyle.Bold), new RectangleF(80, 24, Width - 90, 30), Color.White, Theme.SingleLine);
            Theme.Text(g, "Academic Marks Manager", Theme.Font(12), new RectangleF(80, 52, Width - 90, 20), Theme.NavyMuted, Theme.SingleLine);

            //divider between the academic items and "Data & Files"
            using (Pen pen = new Pen(Color.FromArgb(46, 255, 255, 255), 1f))
            {
                g.DrawLine(pen, 28, 338, Width - 28, 338);
            }

            //user profile card
            Rectangle card = new Rectangle(14, Height - 116, Width - 28, 66);
            Theme.FillRounded(g, Theme.NavyRaised, card, 16);
            Rectangle avatar = new Rectangle(card.X + 14, card.Y + 11, 44, 44);
            using (SolidBrush brush = new SolidBrush(Theme.Violet))
            {
                g.FillEllipse(brush, avatar);
            }
            Theme.Text(g, Initials(_displayName), Theme.Font(15, FontStyle.Bold), avatar, Color.White, Theme.Centered);
            Theme.Text(g, _displayName, Theme.Semibold(15), new RectangleF(avatar.Right + 12, card.Y + 12, card.Width - 44 - 14 - 12 - 38, 22), Color.White, Theme.SingleLine);
            Theme.Text(g, _roleText, Theme.Font(13), new RectangleF(avatar.Right + 12, card.Y + 34, card.Width - 44 - 14 - 12 - 38, 20), Theme.NavyMuted, Theme.SingleLine);
            Icons.Draw(g, IconKind.Chevron, new RectangleF(card.Right - 34, card.Y + 23, 20, 20), Theme.NavyMuted, 2f);

            //version
            Theme.Text(g, "IAmOnTrack v1.0", Theme.Font(13), new RectangleF(26, Height - 40, Width - 40, 22), Theme.NavyMuted, Theme.SingleLine);
        }

        private static string Initials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "?";
            }
            string[] parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = parts[0].Substring(0, 1);
            if (parts.Length > 1)
            {
                initials += parts[parts.Length - 1].Substring(0, 1);
            }
            return initials.ToUpper();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
