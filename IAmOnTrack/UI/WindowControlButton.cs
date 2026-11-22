using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Minimize / Maximize / Restore / Close buttons for the custom title bar
    public class WindowControlButton : Control
    {
        private bool _hovering;
        private bool _pressed;
        private IconKind _kind;

        //Which glyph to draw (Minimize, Maximize, Restore or Close)
        public IconKind Kind
        {
            get { return _kind; }
            set
            {
                _kind = value;
                Invalidate();
            }
        }

        public WindowControlButton(IconKind kind)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.StandardClick, true);
            DoubleBuffered = true;
            _kind = kind;
            Cursor = Cursors.Default;
            BackColor = Theme.OffWhite;
            Size = new Size(48, 40);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            bool isClose = _kind == IconKind.Close;
            Color glyph = Theme.Charcoal;

            if (_hovering)
            {
                Color hoverFill = isClose ? Theme.Danger : Theme.Lavender;
                if (_pressed)
                {
                    hoverFill = isClose ? Color.FromArgb(200, 55, 60) : Theme.LavenderDeep;
                }
                using (SolidBrush brush = new SolidBrush(hoverFill))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
                if (isClose)
                {
                    glyph = Color.White;
                }
            }

            Icons.Draw(g, _kind, new RectangleF((Width - 14) / 2f, (Height - 14) / 2f, 14, 14), glyph, 1.9f);
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
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _pressed = false;
            Invalidate();
        }
    }
}
