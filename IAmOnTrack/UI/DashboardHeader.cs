using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Greeting, subtitle, search bar and notification button
    public class DashboardHeader : Control
    {
        private const int SideMargin = 24;

        private string _greeting = "";
        private string _name = "";
        private string _subtitle = "";

        public SearchBox Search { get; private set; }
        public IconButton Notifications { get; private set; }

        public DashboardHeader()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = Theme.OffWhite;
            Height = 92;

            Search = new SearchBox("Search modules, assessments...");
            Notifications = new IconButton(IconKind.Bell);
            Controls.Add(Search);
            Controls.Add(Notifications);
        }

        public void SetText(string greeting, string name, string subtitle)
        {
            _greeting = greeting;
            _name = name;
            _subtitle = subtitle;
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (Notifications == null || Search == null)
            {
                return;
            }
            int top = (Height - Notifications.Height) / 2 + 2;
            Notifications.Location = new Point(Width - SideMargin - Notifications.Width, top);

            //the greeting text keeps priority: the search box takes whatever width is left (160 - 400 px)
            int reservedForText = SideMargin + 20 + 400;
            int available = Width - reservedForText - SideMargin - Notifications.Width - 28;
            int searchWidth = Math.Max(160, Math.Min(400, available));
            Search.SetBounds(Notifications.Left - 14 - searchWidth, top, searchWidth, 48);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            //violet accent bar
            Theme.FillRounded(g, Theme.Violet, new RectangleF(SideMargin, 14, 5, Height - 28), 2.5f);

            int x = SideMargin + 20;
            Font big = Theme.Font(34, FontStyle.Bold);
            int greetingWidth = Theme.Measure(g, _greeting, big).Width;
            Theme.Text(g, _greeting, big, new RectangleF(x, 4, greetingWidth + 4, 48), Theme.Charcoal, Theme.SingleLine);
            Theme.Text(g, _name, big, new RectangleF(x + greetingWidth + 8, 4, 220, 48), Theme.Violet, Theme.SingleLine);
            Theme.Text(g, _subtitle, Theme.Font(16), new RectangleF(x, 52, Math.Max(100, Search.Left - x - 16), 26), Theme.Muted, Theme.SingleLine);
        }
    }
}
