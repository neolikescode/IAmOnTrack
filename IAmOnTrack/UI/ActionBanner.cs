using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //The dark violet "What would you like to do today?" section holding the two action cards
    public class ActionBanner : RoundedPanel
    {
        public ActionCard AddAssessmentCard { get; private set; }
        public ActionCard EnterMarksCard { get; private set; }

        public ActionBanner()
        {
            CardColor = Theme.Banner;
            CornerRadius = 20;
            DrawBorder = false;
            HoverEffect = false;
            Padding = new Padding(ShadowInset.Left + 14, ShadowInset.Top + 92, ShadowInset.Right + 14, ShadowInset.Bottom + 8);

            AddAssessmentCard = new ActionCard(IconKind.DocPlus, "Add Assessment",
                "Create a new assessment, link it to a module and set its weighting.",
                "Add Assessment", Theme.LavenderCard, Theme.LavenderDeep, Theme.Violet);
            AddAssessmentCard.BackColor = Theme.Banner;
            AddAssessmentCard.Dock = DockStyle.Fill;

            EnterMarksCard = new ActionCard(IconKind.BarChartFilled, "Enter Marks",
                "Record your obtained marks for existing assessments.",
                "Enter Marks", Theme.MintCard, Theme.MintTint, Theme.MintDeep);
            EnterMarksCard.BackColor = Theme.Banner;
            EnterMarksCard.Dock = DockStyle.Fill;

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.BackColor = Theme.Banner;
            grid.ColumnCount = 2;
            grid.RowCount = 1;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            grid.Controls.Add(AddAssessmentCard, 0, 0);
            grid.Controls.Add(EnterMarksCard, 1, 0);
            Controls.Add(grid);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle b = BodyBounds;
            if (b.Width < 300)
            {
                return;
            }

            //faint graduation-cap watermark in the top-right corner
            using (var clip = CardPath())
            {
                g.SetClip(clip);
                Icons.Draw(g, IconKind.GradCap, new RectangleF(b.Right - 128, b.Y + 4, 100, 100), Theme.WithAlpha(Color.FromArgb(159, 137, 255), 55), 1.6f);
                g.ResetClip();
            }

            Theme.Text(g, "What would you like to do today?", Theme.Font(26, FontStyle.Bold), new RectangleF(b.X + 24, b.Y + 18, b.Width - 180, 38), Color.White, Theme.SingleLine);
            Theme.Text(g, "Add new assessments, record your marks, and keep track of your performance.", Theme.Font(14),
                new RectangleF(b.X + 24, b.Y + 58, b.Width - 48, 22), Color.FromArgb(222, 215, 255), Theme.SingleLine);
        }
    }
}
