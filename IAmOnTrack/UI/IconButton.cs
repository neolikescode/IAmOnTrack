using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Small square button with just an icon (used for the notification bell)
    public class IconButton : Control
    {
        private bool _hovering;

        public IconKind Icon { get; set; }

        public IconButton(IconKind icon)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.StandardClick, true);
            DoubleBuffered = true;
            Icon = icon;
            Cursor = Cursors.Hand;
            BackColor = Theme.OffWhite;
            Size = new Size(48, 48);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
            Theme.FillRounded(g, _hovering ? Theme.Lavender : Theme.White, r, 14);
            using (var path = Theme.RoundedRect(r, 14))
            using (Pen pen = new Pen(_hovering ? Theme.LavenderDeep : Theme.Border, 1f))
            {
                g.DrawPath(pen, path);
            }
            Icons.Draw(g, Icon, new RectangleF((Width - 22) / 2f, (Height - 22) / 2f, 22, 22), _hovering ? Theme.Violet : Theme.Charcoal, 1.7f);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hovering = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovering = false;
            Invalidate();
        }
    }
}
