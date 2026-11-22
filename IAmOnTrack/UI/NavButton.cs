using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //One item in the dark sidebar. The selected item has a rounded electric-violet background.
    public class NavButton : Control
    {
        private readonly Timer _timer;
        private float _hover;
        private bool _hovering;
        private bool _selected;

        public IconKind Icon { get; set; }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                Invalidate();
            }
        }

        public NavButton(string text, IconKind icon)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable | ControlStyles.StandardClick, true);
            DoubleBuffered = true;
            Text = text;
            Icon = icon;
            Cursor = Cursors.Hand;
            Font = Theme.Semibold(15);
            BackColor = Theme.Navy;
            Height = 46;

            _timer = new Timer();
            _timer.Interval = 15;
            _timer.Tick += (s, e) =>
            {
                float target = _hovering ? 1f : 0f;
                _hover += (target - _hover) * 0.3f;
                if (Math.Abs(target - _hover) < 0.02f)
                {
                    _hover = target;
                    _timer.Stop();
                }
                Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            Color textColor;
            if (_selected)
            {
                Theme.FillRounded(g, Theme.Violet, new RectangleF(0, 0, Width, Height), 12);
                textColor = Color.White;
            }
            else
            {
                if (_hover > 0.01f)
                {
                    Theme.FillRounded(g, Theme.Blend(Theme.Navy, Theme.NavyHover, _hover), new RectangleF(0, 0, Width, Height), 12);
                }
                textColor = Theme.Blend(Theme.NavyText, Color.White, _hover);
            }

            Icons.Draw(g, Icon, new RectangleF(18, (Height - 22) / 2f, 22, 22), textColor, 1.7f);
            Theme.Text(g, Text, Font, new RectangleF(54, 0, Width - 62, Height), textColor, Theme.SingleLine);

            if (Focused && ShowFocusCues)
            {
                using (var path = Theme.RoundedRect(new RectangleF(1, 1, Width - 2, Height - 2), 12))
                using (Pen pen = new Pen(Theme.Violet, 2f))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hovering = true;
            _timer.Start();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovering = false;
            _timer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
