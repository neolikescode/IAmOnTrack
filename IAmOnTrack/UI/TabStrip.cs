using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Category tabs: the selected tab is a soft-lavender pill with violet text; each tab can show a count.
    public class TabStrip : Control
    {
        private readonly List<string> _titles = new List<string>();
        private readonly List<string> _counts = new List<string>();
        private int _selectedIndex;
        private int _hoverIndex = -1;

        public event EventHandler SelectedIndexChanged;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value < 0 || value >= _titles.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Tab index is not valid");
                }
                if (value != _selectedIndex)
                {
                    _selectedIndex = value;
                    Invalidate();
                    if (SelectedIndexChanged != null)
                    {
                        SelectedIndexChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public TabStrip(params string[] titles)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            DoubleBuffered = true;
            BackColor = Theme.White;
            Height = 48;
            foreach (string title in titles)
            {
                _titles.Add(title);
                _counts.Add("0");
            }
        }

        //Select a tab without raising SelectedIndexChanged (used when the page opens)
        public void SelectSilently(int index)
        {
            _selectedIndex = Math.Max(0, Math.Min(_titles.Count - 1, index));
            Invalidate();
        }

        public void SetCount(int index, int count)
        {
            SetCountText(index, count.ToString());
        }

        //For badges that are not a plain number (e.g. "7/13")
        public void SetCountText(int index, string text)
        {
            _counts[index] = text;
            Invalidate();
        }

        private Rectangle TabBounds(Graphics g, int index)
        {
            int x = 0;
            for (int i = 0; i <= index; i++)
            {
                int width = Theme.Measure(g, _titles[i], Theme.Semibold(14.5f)).Width + 28 + CountWidth(g, i);
                if (i == index)
                {
                    return new Rectangle(x, 4, width, Height - 8);
                }
                x += width + 6;
            }
            return Rectangle.Empty;
        }

        private int CountWidth(Graphics g, int index)
        {
            return Theme.Measure(g, _counts[index], Theme.Semibold(12)).Width + 18 + 8;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            for (int i = 0; i < _titles.Count; i++)
            {
                Rectangle tab = TabBounds(g, i);
                bool selected = i == _selectedIndex;

                if (selected)
                {
                    Theme.FillRounded(g, Theme.Lavender, tab, 12);
                }
                else if (i == _hoverIndex)
                {
                    Theme.FillRounded(g, Theme.OffWhite, tab, 12);
                }

                Color textColor = selected ? Theme.Violet : Theme.Muted;
                int titleWidth = Theme.Measure(g, _titles[i], Theme.Semibold(14.5f)).Width;
                Theme.Text(g, _titles[i], Theme.Semibold(14.5f), new RectangleF(tab.X + 14, tab.Y, titleWidth + 2, tab.Height), textColor, Theme.SingleLine);

                //count badge
                string count = _counts[i].ToString();
                int countWidth = Theme.Measure(g, count, Theme.Semibold(12)).Width;
                RectangleF badge = new RectangleF(tab.X + 14 + titleWidth + 8, tab.Y + (tab.Height - 22) / 2f, countWidth + 14, 22);
                Theme.FillRounded(g, selected ? Theme.White : Theme.OffWhite, badge, 11);
                Theme.Text(g, count, Theme.Semibold(12), badge, selected ? Theme.Violet : Theme.Muted, Theme.Centered);

                if (Focused && selected && ShowFocusCues)
                {
                    using (var path = Theme.RoundedRect(new RectangleF(tab.X + 1, tab.Y + 1, tab.Width - 2, tab.Height - 2), 12))
                    using (Pen pen = new Pen(Theme.Violet, 1.5f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }

        private int HitTest(Point p)
        {
            using (Graphics g = CreateGraphics())
            {
                for (int i = 0; i < _titles.Count; i++)
                {
                    if (TabBounds(g, i).Contains(p))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int hit = HitTest(e.Location);
            if (hit != _hoverIndex)
            {
                _hoverIndex = hit;
                Cursor = hit >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int hit = HitTest(e.Location);
            if (hit >= 0)
            {
                Focus();
                SelectedIndex = hit;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Left || keyData == Keys.Right || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Right && _selectedIndex < _titles.Count - 1)
            {
                SelectedIndex = _selectedIndex + 1;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Left && _selectedIndex > 0)
            {
                SelectedIndex = _selectedIndex - 1;
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
    }
}
