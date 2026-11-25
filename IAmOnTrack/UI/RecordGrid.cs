using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    public enum CellStyle
    {
        Normal,
        Strong,
        Muted,
        PillSuccess,
        PillNeutral
    }

    [Flags]
    public enum RowActions
    {
        None = 0,
        View = 1,
        Edit = 2,
        Delete = 4
    }

    public class GridColumn
    {
        public string Header { get; private set; }
        public float Weight { get; private set; }
        public bool RightAligned { get; private set; }

        public GridColumn(string header, float weight, bool rightAligned = false)
        {
            Header = header;
            Weight = weight;
            RightAligned = rightAligned;
        }
    }

    public class GridCell
    {
        public string Text { get; private set; }
        public CellStyle Style { get; private set; }

        public GridCell(string text, CellStyle style = CellStyle.Normal)
        {
            Text = text ?? "";
            Style = style;
        }
    }

    //One table row. Tag holds the real record (a CModules or a CAssessmentRecord) - the grid never copies data.
    public class GridRow
    {
        public object Tag { get; private set; }
        public GridCell[] Cells { get; private set; }

        public GridRow(object tag, params GridCell[] cells)
        {
            Tag = tag;
            Cells = cells;
        }
    }

    public class GridActionEventArgs : EventArgs
    {
        public GridRow Row { get; private set; }
        public RowActions Action { get; private set; }

        public GridActionEventArgs(GridRow row, RowActions action)
        {
            Row = row;
            Action = action;
        }
    }

    //Custom-drawn data table: soft header, hover / selected rows, status pills and icon actions on every row.
    //Shows its own empty state (icon, message and an optional button) when there are no rows.
    public class RecordGrid : Control
    {
        private const int HeaderHeight = 44;
        private const int RowHeight = 50;
        private const int SidePad = 20;
        private const int ActionSize = 34;
        private const int ActionGap = 4;

        private readonly List<GridColumn> _columns = new List<GridColumn>();
        private readonly List<GridRow> _rows = new List<GridRow>();
        private readonly ToolTip _tip = new ToolTip();
        private readonly RoundedButton _emptyButton;

        private RowActions _actions = RowActions.View;
        private int _hoverRow = -1;
        private int _selectedRow = -1;
        private RowActions _hoverAction = RowActions.None;

        private IconKind _emptyIcon = IconKind.Inbox;
        private string _emptyTitle;
        private string _emptyMessage = "";

        public event EventHandler<GridActionEventArgs> ActionInvoked;
        public event EventHandler EmptyActionClicked;

        //How many rows fit in the current height (the form uses this as the page size)
        public int RowCapacity
        {
            get { return Math.Max(1, (Height - HeaderHeight) / RowHeight); }
        }

        public RowActions Actions
        {
            get { return _actions; }
            set
            {
                _actions = value;
                Invalidate();
            }
        }

        public GridRow SelectedRow
        {
            get { return _selectedRow >= 0 && _selectedRow < _rows.Count ? _rows[_selectedRow] : null; }
        }

        public RecordGrid()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            DoubleBuffered = true;
            BackColor = Theme.White;
            TabStop = true;

            _emptyButton = new RoundedButton();
            _emptyButton.BackColor = Theme.White;
            _emptyButton.Visible = false;
            _emptyButton.Size = new Size(190, 44);
            _emptyButton.Click += (s, e) => { if (EmptyActionClicked != null) EmptyActionClicked(this, EventArgs.Empty); };
            Controls.Add(_emptyButton);
        }

        // ---------------- data ----------------

        public void SetColumns(IEnumerable<GridColumn> columns)
        {
            _columns.Clear();
            _columns.AddRange(columns);
            Invalidate();
        }

        public void SetRows(IEnumerable<GridRow> rows)
        {
            GridRow previous = SelectedRow;
            _rows.Clear();
            _rows.AddRange(rows);

            //keep the same record selected after a refresh when it is still on screen
            _selectedRow = previous == null ? -1 : _rows.FindIndex(r => ReferenceEquals(r.Tag, previous.Tag));
            _hoverRow = -1;
            PositionEmptyButton();
            Invalidate();
        }

        //Describes what to show when there are no rows. actionText = null hides the button.
        public void SetEmptyState(IconKind icon, string title, string message, string actionText)
        {
            _emptyIcon = icon;
            _emptyTitle = title;
            _emptyMessage = message;
            _emptyButton.Text = actionText ?? "";
            _emptyButton.Visible = actionText != null && _rows.Count == 0;
            PositionEmptyButton();
            Invalidate();
        }

        private bool ShowingEmptyState
        {
            get { return _rows.Count == 0 && _emptyTitle != null; }
        }

        // ---------------- geometry ----------------

        private int ActionsWidth
        {
            get
            {
                int count = ActionList().Count;
                return count == 0 ? 0 : count * ActionSize + (count - 1) * ActionGap + 12;
            }
        }

        private List<RowActions> ActionList()
        {
            List<RowActions> list = new List<RowActions>();
            if ((_actions & RowActions.View) != 0) list.Add(RowActions.View);
            if ((_actions & RowActions.Edit) != 0) list.Add(RowActions.Edit);
            if ((_actions & RowActions.Delete) != 0) list.Add(RowActions.Delete);
            return list;
        }

        private Rectangle ActionRect(int row, int actionIndex, int actionCount)
        {
            int right = Width - SidePad + 4;
            int x = right - (actionCount - actionIndex) * ActionSize - (actionCount - 1 - actionIndex) * ActionGap;
            return new Rectangle(x, HeaderHeight + row * RowHeight + (RowHeight - ActionSize) / 2, ActionSize, ActionSize);
        }

        private float[] ColumnLefts(out float[] widths)
        {
            float available = Math.Max(100, Width - SidePad * 2 - ActionsWidth);
            float total = 0;
            foreach (GridColumn c in _columns) total += c.Weight;
            float[] lefts = new float[_columns.Count];
            widths = new float[_columns.Count];
            float x = SidePad;
            for (int i = 0; i < _columns.Count; i++)
            {
                widths[i] = available * _columns[i].Weight / Math.Max(0.01f, total);
                lefts[i] = x;
                x += widths[i];
            }
            return lefts;
        }

        private void PositionEmptyButton()
        {
            if (_emptyButton == null) return;
            int blockHeight = 70 + 14 + 26 + 8 + 44 + 22 + 44;
            int top = Math.Max(10, (Height - blockHeight) / 2);
            _emptyButton.Location = new Point((Width - _emptyButton.Width) / 2, top + 70 + 14 + 26 + 8 + 44 + 22);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PositionEmptyButton();
        }

        // ---------------- painting ----------------

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            if (ShowingEmptyState)
            {
                DrawEmptyState(g);
                return;
            }

            float[] widths;
            float[] lefts = ColumnLefts(out widths);

            //header band
            Theme.FillRounded(g, Color.FromArgb(247, 247, 252), new RectangleF(0, 4, Width, HeaderHeight - 8), 10);
            for (int c = 0; c < _columns.Count; c++)
            {
                TextFormatFlags flags = Theme.SingleLine | (_columns[c].RightAligned ? TextFormatFlags.Right : TextFormatFlags.Left);
                Theme.Text(g, _columns[c].Header.ToUpper(), Theme.Semibold(12), new RectangleF(lefts[c], 4, widths[c] - 12, HeaderHeight - 8), Theme.Muted, flags);
            }

            List<RowActions> actionList = ActionList();
            for (int r = 0; r < _rows.Count; r++)
            {
                int y = HeaderHeight + r * RowHeight;
                if (y + RowHeight > Height + RowHeight) break;

                bool selected = r == _selectedRow;
                if (selected)
                {
                    Theme.FillRounded(g, Theme.Lavender, new RectangleF(0, y + 2, Width, RowHeight - 4), 12);
                }
                else if (r == _hoverRow)
                {
                    Theme.FillRounded(g, Theme.LavenderCard, new RectangleF(0, y + 2, Width, RowHeight - 4), 12);
                }
                else if (r < _rows.Count - 1)
                {
                    using (Pen pen = new Pen(Theme.Border, 1f))
                    {
                        g.DrawLine(pen, SidePad, y + RowHeight - 1, Width - SidePad, y + RowHeight - 1);
                    }
                }

                GridRow row = _rows[r];
                for (int c = 0; c < _columns.Count && c < row.Cells.Length; c++)
                {
                    DrawCell(g, row.Cells[c], lefts[c], y, widths[c] - 12, _columns[c].RightAligned);
                }

                for (int a = 0; a < actionList.Count; a++)
                {
                    DrawAction(g, actionList[a], ActionRect(r, a, actionList.Count), r == _hoverRow && _hoverAction == actionList[a]);
                }
            }

            if (Focused && ShowFocusCues && _selectedRow >= 0)
            {
                using (var path = Theme.RoundedRect(new RectangleF(1, HeaderHeight + _selectedRow * RowHeight + 3, Width - 2, RowHeight - 6), 11))
                using (Pen pen = new Pen(Theme.Violet, 1.5f))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private void DrawCell(Graphics g, GridCell cell, float x, int rowTop, float width, bool rightAligned)
        {
            RectangleF area = new RectangleF(x, rowTop, Math.Max(10, width), RowHeight);

            if (cell.Style == CellStyle.PillSuccess || cell.Style == CellStyle.PillNeutral)
            {
                Font pillFont = Theme.Semibold(12.5f);
                int textWidth = Theme.Measure(g, cell.Text, pillFont).Width;
                float pillWidth = Math.Min(area.Width, textWidth + 30);
                float pillX = rightAligned ? area.Right - pillWidth : area.X;
                RectangleF pill = new RectangleF(pillX, rowTop + (RowHeight - 28) / 2f, pillWidth, 28);
                bool success = cell.Style == CellStyle.PillSuccess;
                Theme.FillRounded(g, success ? Theme.MintTint : Color.FromArgb(238, 238, 245), pill, 14);
                using (SolidBrush dot = new SolidBrush(success ? Theme.MintDeep : Theme.Muted))
                {
                    g.FillEllipse(dot, pill.X + 10, pill.Y + 11, 6, 6);
                }
                Theme.Text(g, cell.Text, pillFont, new RectangleF(pill.X + 20, pill.Y, pill.Width - 22, pill.Height),
                    success ? Color.FromArgb(31, 122, 88) : Theme.Muted, Theme.SingleLine);
                return;
            }

            Font font = cell.Style == CellStyle.Strong ? Theme.Semibold(14.5f) : Theme.Font(14);
            Color color = cell.Style == CellStyle.Muted ? Theme.Muted : Theme.Charcoal;
            TextFormatFlags flags = Theme.SingleLine | (rightAligned ? TextFormatFlags.Right : TextFormatFlags.Left);
            Theme.Text(g, cell.Text, font, area, color, flags);
        }

        private void DrawAction(Graphics g, RowActions action, Rectangle rect, bool hovering)
        {
            bool danger = action == RowActions.Delete;
            if (hovering)
            {
                Theme.FillRounded(g, danger ? Color.FromArgb(255, 235, 236) : Theme.White, rect, 10);
            }
            IconKind icon = action == RowActions.View ? IconKind.Eye : (action == RowActions.Edit ? IconKind.Edit : IconKind.Trash);
            Color color = hovering ? (danger ? Theme.Danger : Theme.Violet) : Color.FromArgb(150, 151, 170);
            Icons.Draw(g, icon, new RectangleF(rect.X + 8, rect.Y + 8, 18, 18), color, 1.9f);
        }

        private void DrawEmptyState(Graphics g)
        {
            int blockHeight = 70 + 14 + 26 + 8 + 44 + 22 + 44;
            int top = Math.Max(10, (Height - blockHeight) / 2);

            RectangleF circle = new RectangleF((Width - 70) / 2f, top, 70, 70);
            using (SolidBrush brush = new SolidBrush(Theme.Lavender))
            {
                g.FillEllipse(brush, circle);
            }
            Icons.Draw(g, _emptyIcon, new RectangleF(circle.X + 18, circle.Y + 18, 34, 34), Theme.Violet, 1.6f);

            Theme.Text(g, _emptyTitle, Theme.Semibold(18), new RectangleF(0, top + 84, Width, 26), Theme.Charcoal, Theme.Centered);
            int messageWidth = Math.Min(460, Width - 60);
            Theme.Text(g, _emptyMessage, Theme.Font(14), new RectangleF((Width - messageWidth) / 2f, top + 118, messageWidth, 44), Theme.Muted, Theme.WrappedCentered);
        }

        // ---------------- mouse ----------------

        private int RowAt(int y)
        {
            if (y < HeaderHeight) return -1;
            int row = (y - HeaderHeight) / RowHeight;
            return row < _rows.Count ? row : -1;
        }

        private RowActions ActionAt(Point p, int row)
        {
            if (row < 0) return RowActions.None;
            List<RowActions> list = ActionList();
            for (int a = 0; a < list.Count; a++)
            {
                if (ActionRect(row, a, list.Count).Contains(p)) return list[a];
            }
            return RowActions.None;
        }

        private static string ActionName(RowActions action)
        {
            switch (action)
            {
                case RowActions.View: return "View details";
                case RowActions.Edit: return "Edit";
                case RowActions.Delete: return "Delete";
                default: return "";
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int row = RowAt(e.Y);
            RowActions action = ActionAt(e.Location, row);
            if (row != _hoverRow || action != _hoverAction)
            {
                _hoverRow = row;
                if (action != _hoverAction && action != RowActions.None)
                {
                    _tip.Show(ActionName(action), this, e.X + 12, e.Y + 22, 1500);
                }
                _hoverAction = action;
                Cursor = action != RowActions.None ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverRow = -1;
            _hoverAction = RowActions.None;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int row = RowAt(e.Y);
            if (row < 0 || e.Button != MouseButtons.Left) return;

            _selectedRow = row;
            RowActions action = ActionAt(e.Location, row);
            Invalidate();
            if (action != RowActions.None)
            {
                Raise(_rows[row], action);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int row = RowAt(e.Y);
            if (row >= 0 && ActionAt(e.Location, row) == RowActions.None && (_actions & RowActions.View) != 0)
            {
                Raise(_rows[row], RowActions.View);
            }
        }

        // ---------------- keyboard ----------------

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_rows.Count == 0) return;

            switch (e.KeyCode)
            {
                case Keys.Down: _selectedRow = Math.Min(_rows.Count - 1, _selectedRow + 1); break;
                case Keys.Up: _selectedRow = Math.Max(0, _selectedRow < 0 ? 0 : _selectedRow - 1); break;
                case Keys.Home: _selectedRow = 0; break;
                case Keys.End: _selectedRow = _rows.Count - 1; break;
                case Keys.Enter:
                    if (_selectedRow >= 0 && (_actions & RowActions.View) != 0) Raise(_rows[_selectedRow], RowActions.View);
                    break;
                case Keys.F2:
                    if (_selectedRow >= 0 && (_actions & RowActions.Edit) != 0) Raise(_rows[_selectedRow], RowActions.Edit);
                    break;
                case Keys.Delete:
                    if (_selectedRow >= 0 && (_actions & RowActions.Delete) != 0) Raise(_rows[_selectedRow], RowActions.Delete);
                    break;
                default:
                    return;
            }
            e.Handled = true;
            Invalidate();
        }

        private void Raise(GridRow row, RowActions action)
        {
            if (ActionInvoked != null)
            {
                ActionInvoked(this, new GridActionEventArgs(row, action));
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
