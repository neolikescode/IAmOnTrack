using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //A "card": rounded corners, soft shadow, subtle hover effect. Other cards inherit from this.
    //BackColor must be set to the colour BEHIND the card (it fills the corners and the shadow area).
    public class RoundedPanel : Panel
    {
        //Space reserved around the card body for the shadow
        public static readonly Padding ShadowInset = new Padding(6, 3, 6, 9);

        private readonly Timer _hoverTimer;
        private float _hover;
        private bool _hovering;

        public int CornerRadius { get; set; }
        public Color CardColor { get; set; }
        public bool DrawBorder { get; set; }
        public bool HoverEffect { get; set; }
        public bool ShowShadow { get; set; }

        protected float HoverAmount
        {
            get { return _hover; }
        }

        //The visible rectangle of the card (everything inside the shadow inset)
        public Rectangle BodyBounds
        {
            get
            {
                return new Rectangle(
                    ShadowInset.Left,
                    ShadowInset.Top,
                    Math.Max(0, Width - ShadowInset.Horizontal),
                    Math.Max(0, Height - ShadowInset.Vertical));
            }
        }

        public RoundedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = Theme.OffWhite;
            CardColor = Theme.White;
            CornerRadius = 16;
            DrawBorder = true;
            HoverEffect = true;
            ShowShadow = true;
            Margin = new Padding(0);

            _hoverTimer = new Timer();
            _hoverTimer.Interval = 15;
            _hoverTimer.Tick += HoverTimer_Tick;
        }

        protected GraphicsPath CardPath()
        {
            return Theme.RoundedRect(BodyBounds, CornerRadius);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            Rectangle body = BodyBounds;
            if (body.Width < 4 || body.Height < 4)
            {
                return;
            }

            if (ShowShadow)
            {
                Theme.DrawShadow(g, body, CornerRadius, 6, (int)(6 + 6 * _hover), 3f);
            }

            using (GraphicsPath path = CardPath())
            using (SolidBrush brush = new SolidBrush(CardColor))
            {
                g.FillPath(brush, path);
                if (DrawBorder)
                {
                    using (Pen pen = new Pen(Theme.Blend(Theme.Border, Theme.LavenderDeep, _hover), 1f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }

        // ---- Hover handling (children count as "inside" the card) ----
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            HookHover(e.Control);
        }

        private void HookHover(Control control)
        {
            control.MouseEnter += (s, a) => UpdateHover();
            control.MouseLeave += (s, a) => UpdateHover();
            foreach (Control child in control.Controls)
            {
                HookHover(child);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateHover();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateHover();
        }

        private void UpdateHover()
        {
            if (!IsHandleCreated || IsDisposed)
            {
                return;
            }
            _hovering = HoverEffect && ClientRectangle.Contains(PointToClient(Cursor.Position));
            _hoverTimer.Start();
        }

        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            float target = _hovering ? 1f : 0f;
            _hover += (target - _hover) * 0.3f;
            if (Math.Abs(target - _hover) < 0.02f)
            {
                _hover = target;
                _hoverTimer.Stop();
            }
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hoverTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
