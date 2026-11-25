using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Page title + subtitle on the left; search box and primary action button on the right
    public class PageHeader : Control
    {
        private const int SideMargin = 24;

        private string _title = "";
        private string _subtitle = "";

        public SearchBox Search { get; private set; }
        public RoundedButton PrimaryButton { get; private set; }

        public PageHeader(string searchPlaceholder, string buttonText)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = Theme.OffWhite;
            Height = 92;

            Search = new SearchBox(searchPlaceholder);
            PrimaryButton = new RoundedButton();
            PrimaryButton.Text = buttonText;
            PrimaryButton.LeadingIcon = IconKind.Plus;
            PrimaryButton.BackColor = Theme.OffWhite;
            PrimaryButton.Font = Theme.Semibold(15);
            PrimaryButton.CornerRadius = 14;
            PrimaryButton.Size = new Size(154, 48);
            Controls.Add(Search);
            Controls.Add(PrimaryButton);
        }

        public void SetText(string title, string subtitle)
        {
            _title = title;
            _subtitle = subtitle;
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (Search == null || PrimaryButton == null)
            {
                return;
            }
            int top = (Height - 48) / 2 + 2;
            PrimaryButton.Location = new Point(Width - SideMargin - PrimaryButton.Width, top);

            //the title keeps priority: the search box takes whatever width is left (160 - 400 px)
            int reserved = SideMargin + 20 + 520;
            int available = Width - reserved - SideMargin - PrimaryButton.Width - 28;
            int searchWidth = Math.Max(160, Math.Min(400, available));
            Search.SetBounds(PrimaryButton.Left - 14 - searchWidth, top, searchWidth, 48);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            Theme.FillRounded(g, Theme.Violet, new RectangleF(SideMargin, 14, 5, Height - 28), 2.5f);
            int x = SideMargin + 20;
            Theme.Text(g, _title, Theme.Font(34, FontStyle.Bold), new RectangleF(x, 4, Math.Max(100, Search.Left - x - 16), 48), Theme.Charcoal, Theme.SingleLine);
            Theme.Text(g, _subtitle, Theme.Font(16), new RectangleF(x, 52, Math.Max(100, Search.Left - x - 16), 26), Theme.Muted, Theme.SingleLine);
        }
    }
}
