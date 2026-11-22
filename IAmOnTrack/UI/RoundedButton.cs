using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Custom primary button: rounded, violet, white text, optional trailing icon (arrow),
    //animated hover and a pressed state. BackColor = the colour behind the button.
    public class RoundedButton : Control
    {
        private readonly Timer _timer;
        private float _hover;
        private bool _hovering;
        private bool _pressed;

        public Color ButtonColor { get; set; }
        public Color HoverColor { get; set; }
        public Color PressedColor { get; set; }
        public Color TextColor { get; set; }
        public int CornerRadius { get; set; }
        public IconKind TrailingIcon { get; set; }

        public RoundedButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable | ControlStyles.StandardClick, true);
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Font = Theme.Semibold(15);
            ButtonColor = Theme.Violet;
            HoverColor = Theme.VioletHover;
            PressedColor = Theme.VioletPressed;
            TextColor = Color.White;
            CornerRadius = 12;
            TrailingIcon = IconKind.None;
            BackColor = Theme.White;
            Size = new Size(200, 44);

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

            Color fill = _pressed ? PressedColor : Theme.Blend(ButtonColor, HoverColor, _hover);
            RectangleF r = new RectangleF(0, 0, Width, Height);
            Theme.FillRounded(g, fill, r, CornerRadius);

            if (Focused && ShowFocusCues)
            {
                RectangleF ring = r;
                ring.Inflate(-1, -1);
                using (var path = Theme.RoundedRect(ring, CornerRadius))
                using (Pen pen = new Pen(Theme.Lavender, 2f))
                {
                    g.DrawPath(pen, path);
                }
            }

            int iconSize = 16;
            int gap = 8;
            Size textSize = Theme.Measure(g, Text, Font);
            int groupWidth = textSize.Width + (TrailingIcon != IconKind.None ? gap + iconSize : 0);
            float x = (Width - groupWidth) / 2f;

            Theme.Text(g, Text, Font, new RectangleF(x, 0, textSize.Width + 2, Height), TextColor, Theme.SingleLine);

            if (TrailingIcon != IconKind.None)
            {
                float nudge = 2f * _hover; //arrow slides right slightly on hover
                RectangleF iconRect = new RectangleF(x + textSize.Width + gap + nudge, (Height - iconSize) / 2f, iconSize, iconSize);
                Icons.Draw(g, TrailingIcon, iconRect, TextColor, 2.2f);
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
            _pressed = false;
            _timer.Start();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _pressed = true;
                Focus();
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _pressed = false;
            Invalidate();
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

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
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
