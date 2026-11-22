using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Circular progress ring with the percentage in the centre. The arc animates smoothly to new values.
    public class CircularProgress : Control
    {
        private readonly Timer _timer;
        private float _target;
        private float _display;

        public int Thickness { get; set; }

        //0 - 100
        public float Value
        {
            get { return _target; }
            set
            {
                _target = Math.Max(0f, Math.Min(100f, value));
                _timer.Start();
            }
        }

        public CircularProgress()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = Theme.White;
            Thickness = 14;
            Size = new Size(124, 124);

            _timer = new Timer();
            _timer.Interval = 15;
            _timer.Tick += (s, e) =>
            {
                _display += (_target - _display) * 0.18f;
                if (Math.Abs(_target - _display) < 0.1f)
                {
                    _display = _target;
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

            float inset = Thickness / 2f + 1f;
            RectangleF ring = new RectangleF(inset, inset, Width - inset * 2, Height - inset * 2);
            if (ring.Width < 10 || ring.Height < 10)
            {
                return;
            }

            using (Pen track = new Pen(Theme.TrackGrey, Thickness))
            {
                g.DrawEllipse(track, ring);
            }

            if (_display > 0.4f)
            {
                using (Pen arc = new Pen(Theme.Violet, Thickness))
                {
                    arc.StartCap = LineCap.Round;
                    arc.EndCap = LineCap.Round;
                    g.DrawArc(arc, ring, -90f, 360f * _display / 100f);
                }
            }

            string label = Math.Round(_display) + "%";
            Theme.Text(g, label, Theme.Font(26, FontStyle.Bold), new RectangleF(0, 0, Width, Height), Theme.Charcoal, Theme.Centered);
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
