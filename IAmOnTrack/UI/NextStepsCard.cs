using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Right panel, section 3: guidance prompts that adapt to the user's progress
    public class NextStepsCard : InfoCard
    {
        private IReadOnlyList<CNextStep> _steps = new List<CNextStep>();

        public NextStepsCard() : base(IconKind.Calendar, "Upcoming / Next Steps", "Assessments and key actions")
        {
        }

        public void SetSteps(IReadOnlyList<CNextStep> steps)
        {
            _steps = steps ?? new List<CNextStep>();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle b = BodyBounds;
            if (b.Width < 220)
            {
                return;
            }

            int y = b.Y + ContentTop;
            foreach (CNextStep step in _steps)
            {
                if (y + 46 > b.Bottom - 8)
                {
                    break;
                }

                Rectangle tile = new Rectangle(b.X + 20, y, 46, 46);
                Theme.FillRounded(g, Theme.Lavender, tile, 12);
                IconKind icon = step.Kind == NextStepKind.AddAssessments ? IconKind.Plus : IconKind.BarChartFilled;
                Icons.Draw(g, icon, new RectangleF(tile.X + 11, tile.Y + 11, 24, 24), Theme.Violet, 2f);

                int textX = tile.Right + 14;
                int textWidth = Math.Max(60, b.Right - 8 - textX);
                Theme.Text(g, step.Title, Theme.Semibold(14), new RectangleF(textX, y + 2, textWidth, 22), Theme.Charcoal, Theme.SingleLine);
                Theme.Text(g, step.Description, Theme.Font(13), new RectangleF(textX, y + 24, textWidth, 20), Theme.Muted, Theme.SingleLine);
                y += 62;
            }
        }
    }
}
