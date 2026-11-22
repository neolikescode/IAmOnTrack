using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Right panel, section 2: overall average (or an empty state when nothing is marked yet)
    public class PerformanceCard : InfoCard
    {
        private double? _average;
        private int _moduleCount;

        public PerformanceCard() : base(IconKind.BarChartFilled, "Performance Overview", "Your current academic standing")
        {
        }

        public void SetAverage(double? average, int moduleCount)
        {
            _average = average;
            _moduleCount = moduleCount;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle b = BodyBounds;
            if (b.Width < 200 || b.Height < ContentTop + 60)
            {
                return;
            }

            Rectangle box = new Rectangle(b.X + 20, b.Y + ContentTop, b.Width - 40, b.Height - ContentTop - 20);
            Theme.FillRounded(g, Color.FromArgb(243, 240, 254), box, 14);

            Theme.Text(g, "Overall Average", Theme.Font(14), new RectangleF(box.X + 18, box.Y + 10, box.Width - 36, 20), Theme.Charcoal, Theme.SingleLine);

            if (_average.HasValue)
            {
                double value = Math.Max(0, Math.Min(100, _average.Value));
                string moduleText = _moduleCount + (_moduleCount == 1 ? " module" : " modules");
                Theme.Text(g, moduleText, Theme.Font(12), new RectangleF(box.X + 18, box.Y + 10, box.Width - 36, 20), Theme.Muted, Theme.SingleLine | System.Windows.Forms.TextFormatFlags.Right);
                Theme.Text(g, value.ToString("0.0") + "%", Theme.Font(30, FontStyle.Bold),
                    new RectangleF(box.X + 18, box.Y + 28, box.Width - 36, 40), Theme.Charcoal, Theme.SingleLine);

                //thin progress bar showing the average
                RectangleF track = new RectangleF(box.X + 18, box.Bottom - 18, box.Width - 36, 7);
                Theme.FillRounded(g, Color.White, track, 3.5f);
                if (value > 0)
                {
                    Theme.FillRounded(g, Theme.Violet, new RectangleF(track.X, track.Y, Math.Max(7f, (float)(track.Width * value / 100.0)), track.Height), 3.5f);
                }
            }
            else
            {
                Theme.Text(g, "\u2014", Theme.Font(30, FontStyle.Bold), new RectangleF(box.X + 18, box.Y + 28, 80, 40), Theme.Charcoal, Theme.SingleLine);
                Theme.Text(g, "Record a mark to see your average.", Theme.Font(12), new RectangleF(box.X + 18, box.Bottom - 26, box.Width - 90, 18), Theme.Muted, Theme.SingleLine);

                //faint decorative bars (an icon only, not data)
                Icons.Draw(g, IconKind.BarChartFilled, new RectangleF(box.Right - 66, box.Y + 18, 44, 44), Theme.WithAlpha(Theme.Violet, 70), 1.8f);
            }
        }
    }
}
