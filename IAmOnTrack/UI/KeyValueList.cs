using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Read-only "label ........ value" list used by the details dialog
    public class KeyValueList : Control
    {
        public const int RowHeight = 38;

        private readonly List<KeyValuePair<string, string>> _rows = new List<KeyValuePair<string, string>>();

        public KeyValueList(IEnumerable<KeyValuePair<string, string>> rows)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = Theme.White;
            _rows.AddRange(rows);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            for (int i = 0; i < _rows.Count; i++)
            {
                int y = i * RowHeight;
                Theme.Text(g, _rows[i].Key, Theme.Font(14), new RectangleF(0, y, Width * 0.42f, RowHeight), Theme.Muted, Theme.SingleLine);
                Theme.Text(g, _rows[i].Value, Theme.Semibold(14.5f), new RectangleF(Width * 0.42f, y, Width * 0.58f, RowHeight), Theme.Charcoal, Theme.SingleLine);
                if (i < _rows.Count - 1)
                {
                    using (Pen pen = new Pen(Theme.Border))
                    {
                        g.DrawLine(pen, 0, y + RowHeight - 1, Width, y + RowHeight - 1);
                    }
                }
            }
        }
    }
}
