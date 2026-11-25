using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //A rounded filter box ("Semester  All v"). Clicking it opens a styled menu of choices.
    public class FilterDropdown : Control
    {
        private readonly string _caption;
        private readonly List<KeyValuePair<string, object>> _items = new List<KeyValuePair<string, object>>();
        private int _selectedIndex;
        private bool _hovering;

        public event EventHandler SelectionChanged;

        public FilterDropdown(string caption)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            DoubleBuffered = true;
            _caption = caption;
            Cursor = Cursors.Hand;
            BackColor = Theme.White;
            Height = 42;
            Width = 170;
            Margin = new Padding(0, 0, 12, 8);
        }

        //The value of the chosen item (null for the "All" choice)
        public object SelectedValue
        {
            get { return _items.Count == 0 ? null : _items[_selectedIndex].Value; }
        }

        //Replaces the choices, keeping the current choice if it still exists
        public void SetItems(IList<KeyValuePair<string, object>> items, object keepValue)
        {
            _items.Clear();
            _items.AddRange(items);
            _selectedIndex = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                if (Equals(_items[i].Value, keepValue))
                {
                    _selectedIndex = i;
                    break;
                }
            }

            //size the box so the longest choice fits
            using (Graphics g = CreateGraphics())
            {
                int widest = 0;
                foreach (KeyValuePair<string, object> item in _items)
                {
                    widest = Math.Max(widest, Theme.Measure(g, item.Key, Theme.Semibold(14)).Width);
                }
                Width = 16 + Theme.Measure(g, _caption, Theme.Font(13)).Width + 10 + widest + 14 + 16 + 12;
            }
            Invalidate();
        }

        private void OpenMenu()
        {
            if (!Enabled || _items.Count == 0)
            {
                return;
            }
            List<string> names = new List<string>();
            foreach (KeyValuePair<string, object> item in _items)
            {
                names.Add(item.Key);
            }
            ContextMenuStrip menu = ModernMenu.Create(names, _selectedIndex, index =>
            {
                if (index != _selectedIndex)
                {
                    _selectedIndex = index;
                    Invalidate();
                    if (SelectionChanged != null)
                    {
                        SelectionChanged(this, EventArgs.Empty);
                    }
                }
            });
            menu.MinimumSize = new Size(Width, 0);
            menu.Closed += (s, e) => menu.Dispose();
            menu.Show(this, new Point(0, Height + 4));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
            Color fill = !Enabled ? Theme.OffWhite : (_hovering ? Theme.LavenderCard : Theme.White);
            Theme.FillRounded(g, fill, r, 12);
            using (var path = Theme.RoundedRect(r, 12))
            using (Pen pen = new Pen(Focused && ShowFocusCues ? Theme.Violet : (_hovering ? Theme.LavenderDeep : Theme.Border), 1.2f))
            {
                g.DrawPath(pen, path);
            }

            Color captionColor = Enabled ? Theme.Muted : Color.FromArgb(170, 171, 184);
            Color valueColor = Enabled ? Theme.Charcoal : Color.FromArgb(170, 171, 184);
            int captionWidth = Theme.Measure(g, _caption, Theme.Font(13)).Width;
            Theme.Text(g, _caption, Theme.Font(13), new RectangleF(16, 0, captionWidth + 2, Height), captionColor, Theme.SingleLine);

            string value = _items.Count == 0 ? "" : _items[_selectedIndex].Key;
            Theme.Text(g, value, Theme.Semibold(14), new RectangleF(16 + captionWidth + 10, 0, Width - 16 - captionWidth - 10 - 34, Height), valueColor, Theme.SingleLine);
            Icons.Draw(g, IconKind.Chevron, new RectangleF(Width - 30, (Height - 16) / 2f, 16, 16), captionColor, 2f);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Focus();
            OpenMenu();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Down || keyData == Keys.Up || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.Down)
            {
                OpenMenu();
                e.Handled = true;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hovering = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovering = false;
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
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
