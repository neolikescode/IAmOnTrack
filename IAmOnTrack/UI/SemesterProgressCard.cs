using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Right panel, section 1: circular completion indicator + completed / remaining counts
    public class SemesterProgressCard : InfoCard
    {
        private const int RingSize = 112;

        private readonly CircularProgress _ring;
        private int _completed;
        private int _remaining;
        private string _semesterLabel = "";

        public SemesterProgressCard() : base(IconKind.Target, "Semester Progress", "Overall assessment completion")
        {
            _ring = new CircularProgress();
            _ring.Size = new Size(RingSize, RingSize);
            _ring.Thickness = 12;
            _ring.BackColor = Theme.White;
            Controls.Add(_ring);
        }

        public void SetProgress(CSemesterProgress progress, string semesterLabel)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }
            _completed = progress.Completed;
            _remaining = progress.Remaining;
            _semesterLabel = semesterLabel;
            _ring.Value = (float)progress.Percentage;
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (_ring == null)
            {
                return;
            }
            Rectangle b = BodyBounds;
            _ring.Location = new Point(b.X + 22, b.Y + ContentTop + 4);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle b = BodyBounds;
            if (b.Width < 250)
            {
                return;
            }

            int lx = _ring.Right + 18;
            int width = Math.Max(60, b.Right - 16 - lx);
            DrawLegendRow(g, lx, _ring.Top + 10, width, Theme.Mint, _completed, "completed");
            DrawLegendRow(g, lx, _ring.Top + 40, width, Theme.LavenderDeep, _remaining, "remaining");

            //semester pill
            if (_semesterLabel.Length > 0)
            {
                Font pillFont = Theme.Semibold(13);
                int textWidth = Theme.Measure(g, _semesterLabel, pillFont).Width;
                int pillWidth = Math.Min(width, textWidth + 48);
                Rectangle pill = new Rectangle(lx, _ring.Top + 74, pillWidth, 34);
                Theme.FillRounded(g, Theme.OffWhite, pill, 10);
                Icons.Draw(g, IconKind.Calendar, new RectangleF(pill.X + 10, pill.Y + 9, 16, 16), Theme.Charcoal, 1.9f);
                Theme.Text(g, _semesterLabel, pillFont, new RectangleF(pill.X + 32, pill.Y, pill.Width - 36, pill.Height), Theme.Charcoal, Theme.SingleLine);
            }
        }

        private void DrawLegendRow(Graphics g, int x, int y, int width, Color dot, int number, string label)
        {
            using (SolidBrush brush = new SolidBrush(dot))
            {
                g.FillEllipse(brush, x, y + 9, 12, 12);
            }
            Font numberFont = Theme.Font(15, FontStyle.Bold);
            string numberText = number.ToString();
            int numberWidth = Theme.Measure(g, numberText, numberFont).Width;
            Theme.Text(g, numberText, numberFont, new RectangleF(x + 22, y, numberWidth + 2, 30), Theme.Charcoal, Theme.SingleLine);
            Theme.Text(g, label, Theme.Font(14), new RectangleF(x + 22 + numberWidth + 5, y, Math.Max(10, width - 27 - numberWidth), 30), Theme.Muted, Theme.SingleLine);
        }
    }
}
