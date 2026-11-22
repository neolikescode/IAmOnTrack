using System;
using System.Drawing;

namespace IAmOnTrack.UI
{
    //Shared base for the right-hand panel cards: draws the icon + title + subtitle header.
    public class InfoCard : RoundedPanel
    {
        protected const int ContentTop = 76;

        private readonly IconKind _icon;
        private readonly string _title;
        private readonly string _subtitle;

        public InfoCard(IconKind icon, string title, string subtitle)
        {
            _icon = icon;
            _title = title;
            _subtitle = subtitle;
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle b = BodyBounds;
            if (b.Width < 4)
            {
                return;
            }

            Icons.Draw(g, _icon, new RectangleF(b.X + 20, b.Y + 20, 26, 26), Theme.Violet, 1.8f);
            Theme.Text(g, _title, Theme.Semibold(17), new RectangleF(b.X + 56, b.Y + 17, b.Width - 76, 24), Theme.Charcoal, Theme.SingleLine);
            Theme.Text(g, _subtitle, Theme.Font(13), new RectangleF(b.X + 56, b.Y + 41, b.Width - 76, 20), Theme.Muted, Theme.SingleLine);
        }
    }
}
