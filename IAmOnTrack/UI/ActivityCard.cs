using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //"Recent Activity" card. Shows a professional empty state until entries exist.
    public class ActivityCard : RoundedPanel
    {
        private IReadOnlyList<CActivityEntry> _entries = new List<CActivityEntry>();

        public ActivityCard()
        {
            CornerRadius = 16;
        }

        public void SetEntries(IReadOnlyList<CActivityEntry> entries)
        {
            _entries = entries ?? new List<CActivityEntry>();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle b = BodyBounds;
            if (b.Width < 240 || b.Height < 100)
            {
                return;
            }

            //header + divider
            Icons.Draw(g, IconKind.Clock, new RectangleF(b.X + 22, b.Y + 17, 26, 26), Theme.Violet, 1.8f);
            Theme.Text(g, "Recent Activity", Theme.Semibold(18), new RectangleF(b.X + 58, b.Y + 15, b.Width - 90, 30), Theme.Charcoal, Theme.SingleLine);
            using (Pen divider = new Pen(Theme.Border, 1f))
            {
                g.DrawLine(divider, b.X + 22, b.Y + 58, b.Right - 22, b.Y + 58);
            }

            Rectangle content = new Rectangle(b.X + 22, b.Y + 60, b.Width - 44, b.Height - 60 - 8);

            if (_entries.Count == 0)
            {
                DrawEmptyState(g, content);
            }
            else
            {
                DrawEntries(g, content);
            }
        }

        private void DrawEmptyState(Graphics g, Rectangle area)
        {
            int blockHeight = 36 + 10 + 24 + 4 + 40;
            int y = area.Y + Math.Max(4, (area.Height - blockHeight) / 2);

            Icons.Draw(g, IconKind.Inbox, new RectangleF(area.X + (area.Width - 38) / 2f, y, 38, 38), Theme.Muted, 1.4f);
            Theme.Text(g, "No activity yet", Theme.Semibold(16), new RectangleF(area.X, y + 46, area.Width, 24), Theme.Charcoal, Theme.Centered);
            Theme.Text(g, "Your recent actions will appear here, such as added assessments or recorded marks.", Theme.Font(13.5f),
                new RectangleF(area.X + 20, y + 72, area.Width - 40, 40), Theme.Muted, Theme.WrappedCentered);
        }

        private void DrawEntries(Graphics g, Rectangle area)
        {
            int y = area.Y + 8;
            foreach (CActivityEntry entry in _entries)
            {
                if (y + 44 > area.Bottom)
                {
                    break;
                }

                Rectangle tile = new Rectangle(area.X, y, 40, 40);
                Theme.FillRounded(g, Theme.Lavender, tile, 12);
                Icons.Draw(g, IconFor(entry.Kind), new RectangleF(tile.X + 10, tile.Y + 10, 20, 20), Theme.Violet, 1.9f);

                string when = FormatTime(entry.Timestamp);
                int whenWidth = Theme.Measure(g, when, Theme.Font(13)).Width + 4;
                Theme.Text(g, when, Theme.Font(13), new RectangleF(area.Right - whenWidth, y, whenWidth, 40), Theme.Muted, Theme.SingleLine);
                Theme.Text(g, entry.Description, Theme.Semibold(14.5f), new RectangleF(tile.Right + 14, y, area.Width - 40 - 14 - whenWidth - 10, 40), Theme.Charcoal, Theme.SingleLine);
                y += 50;
            }
        }

        private static IconKind IconFor(ActivityKind kind)
        {
            switch (kind)
            {
                case ActivityKind.AssessmentAdded: return IconKind.DocPlus;
                case ActivityKind.MarksRecorded: return IconKind.BarChartFilled;
                default: return IconKind.Layers;
            }
        }

        private static string FormatTime(DateTime timestamp)
        {
            TimeSpan age = DateTime.Now - timestamp;
            if (age.TotalMinutes < 1) return "Just now";
            if (age.TotalMinutes < 60) return (int)age.TotalMinutes + " min ago";
            if (age.TotalHours < 24) return (int)age.TotalHours + " h ago";
            return timestamp.ToString("d MMM");
        }
    }
}
