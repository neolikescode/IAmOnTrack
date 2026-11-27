using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //A label above a rounded text box (used in the edit dialogs)
    public class InputField : Control
    {
        private readonly TextBox _input;
        private readonly string _label;
        private string _hint = "";

        //Small muted text on the right of the label (e.g. "60% still available")
        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value ?? "";
                Invalidate();
            }
        }

        public string Value
        {
            get { return _input.Text; }
            set { _input.Text = value; }
        }

        public InputField(string label)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            _label = label;
            BackColor = Theme.White;
            Height = 76;

            _input = new TextBox();
            _input.BorderStyle = BorderStyle.None;
            _input.Font = Theme.Font(15);
            _input.BackColor = Theme.White;
            _input.ForeColor = Theme.Charcoal;
            _input.GotFocus += (s, e) => Invalidate();
            _input.LostFocus += (s, e) => Invalidate();
            Controls.Add(_input);
        }

        public void FocusInput()
        {
            _input.Focus();
            _input.SelectAll();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_input == null) return;
            _input.SetBounds(16, 26 + (46 - _input.Height) / 2, Math.Max(20, Width - 32), _input.Height);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _input.Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            g.Clear(BackColor);

            Theme.Text(g, _label, Theme.Semibold(13.5f), new RectangleF(2, 0, Width, 22), Theme.Charcoal, Theme.SingleLine);
            if (_hint.Length > 0)
            {
                Theme.Text(g, _hint, Theme.Font(13), new RectangleF(0, 0, Width - 2, 22), Theme.Muted, Theme.SingleLine | TextFormatFlags.Right);
            }

            RectangleF box = new RectangleF(0.5f, 26.5f, Width - 1, 45);
            Theme.FillRounded(g, Theme.White, box, 12);
            using (var path = Theme.RoundedRect(box, 12))
            using (Pen pen = new Pen(_input.Focused ? Theme.Violet : Theme.Border, _input.Focused ? 1.6f : 1.2f))
            {
                g.DrawPath(pen, path);
            }
        }

        //Reads a number the way the user typed it (accepts both "12.5" and "12,5")
        public static bool TryParseNumber(string text, out double value)
        {
            string trimmed = (text ?? "").Trim();
            return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
                   double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
