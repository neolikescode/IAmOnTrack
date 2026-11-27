using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //A label above a rounded drop-down box (used in the Add / Enter forms). Optional action link on the label row.
    public class SelectField : Control
    {
        private const int BoxTop = 26;
        private const int BoxHeight = 45;

        private readonly string _label;
        private readonly List<KeyValuePair<string, object>> _items = new List<KeyValuePair<string, object>>();
        private int _selectedIndex = -1;
        private bool _hovering;
        private string _actionText;

        //Shown (muted) when there is nothing to choose, e.g. "No modules yet"
        public string Placeholder { get; set; }

        //Optional violet link on the right of the label, e.g. "+ New module"
        public string ActionText
        {
            get { return _actionText; }
            set
            {
                _actionText = value;
                Invalidate();
            }
        }

        public event EventHandler SelectionChanged;
        public event EventHandler ActionClicked;

        public object SelectedValue
        {
            get { return _selectedIndex < 0 ? null : _items[_selectedIndex].Value; }
        }

        public int ItemCount
        {
            get { return _items.Count; }
        }

        public SelectField(string label)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            DoubleBuffered = true;
            _label = label;
            Placeholder = "Select...";
            BackColor = Theme.White;
            Height = 76;
        }

        //Replaces the choices and selects 'select' (or the first item). Does not raise SelectionChanged.
        public void SetItems(IList<KeyValuePair<string, object>> items, object select)
        {
            _items.Clear();
            _items.AddRange(items);
            _selectedIndex = _items.Count == 0 ? -1 : 0;
            for (int i = 0; i < _items.Count; i++)
            {
                if (select != null && Equals(_items[i].Value, select))
                {
                    _selectedIndex = i;
                    break;
                }
            }
            Invalidate();
        }

        private Rectangle ActionRect(Graphics g)
        {
            if (string.IsNullOrEmpty(_actionText))
            {
                return Rectangle.Empty;
            }
            int width = Theme.Measure(g, _actionText, Theme.Semibold(13)).Width + 4;
            return new Rectangle(Width - width - 2, 0, width, 22);
        }

        private void OpenMenu()
        {
            if (_items.Count == 0)
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
            menu.Show(this, new Point(0, BoxTop + BoxHeight + 4));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            Theme.Text(g, _label, Theme.Semibold(13.5f), new RectangleF(2, 0, Width, 22), Theme.Charcoal, Theme.SingleLine);
            Rectangle action = ActionRect(g);
            if (!action.IsEmpty)
            {
                Theme.Text(g, _actionText, Theme.Semibold(13), action, Theme.Violet, Theme.SingleLine | TextFormatFlags.Right);
            }

            RectangleF box = new RectangleF(0.5f, BoxTop + 0.5f, Width - 1, BoxHeight);
            bool focused = Focused && ShowFocusCues;
            Theme.FillRounded(g, _hovering ? Theme.LavenderCard : Theme.White, box, 12);
            using (var path = Theme.RoundedRect(box, 12))
            using (Pen pen = new Pen(focused ? Theme.Violet : (_hovering ? Theme.LavenderDeep : Theme.Border), focused ? 1.6f : 1.2f))
            {
                g.DrawPath(pen, path);
            }

            bool hasValue = _selectedIndex >= 0;
            string text = hasValue ? _items[_selectedIndex].Key : Placeholder;
            Theme.Text(g, text, Theme.Font(15), new RectangleF(16, BoxTop, Width - 56, BoxHeight), hasValue ? Theme.Charcoal : Theme.Muted, Theme.SingleLine);
            Icons.Draw(g, IconKind.Chevron, new RectangleF(Width - 36, BoxTop + (BoxHeight - 18) / 2f, 18, 18), Theme.Muted, 2f);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            using (Graphics g = CreateGraphics())
            {
                if (ActionRect(g).Contains(e.Location) && ActionClicked != null)
                {
                    ActionClicked(this, EventArgs.Empty);
                    return;
                }
            }
            if (e.Y >= BoxTop)
            {
                OpenMenu();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool overBox = e.Y >= BoxTop && _items.Count > 0;
            bool overAction = false;
            using (Graphics g = CreateGraphics())
            {
                overAction = ActionRect(g).Contains(e.Location);
            }
            Cursor = (overBox || overAction) ? Cursors.Hand : Cursors.Default;
            if (overBox != _hovering)
            {
                _hovering = overBox;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovering = false;
            Invalidate();
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
