using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Rounded search field with a magnifier icon, placeholder text and a "Ctrl + K" hint.
    //NOTE: this only provides the input box - no searching is implemented yet.
    public class SearchBox : Control
    {
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private readonly TextBox _input;
        private readonly string _placeholder;

        public string PlaceholderText
        {
            get { return _placeholder; }
        }

        public string QueryText
        {
            get { return _input.Text; }
        }

        public SearchBox(string placeholder)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            _placeholder = placeholder;
            BackColor = Theme.OffWhite;
            Height = 48;

            _input = new TextBox();
            _input.BorderStyle = BorderStyle.None;
            _input.Font = Theme.Font(15);
            _input.BackColor = Theme.White;
            _input.ForeColor = Theme.Charcoal;
            _input.GotFocus += (s, e) => Invalidate();
            _input.LostFocus += (s, e) => Invalidate();
            _input.TextChanged += (s, e) => Invalidate();
            _input.HandleCreated += (s, e) => ApplyPlaceholder();
            Controls.Add(_input);
        }

        //Native placeholder ("cue banner") text. A failure here is purely cosmetic, so it must never crash the app.
        private void ApplyPlaceholder()
        {
            try
            {
                SendMessage(_input.Handle, EM_SETCUEBANNER, IntPtr.Zero, _placeholder);
            }
            catch (EntryPointNotFoundException)
            {
            }
            catch (DllNotFoundException)
            {
            }
        }

        //Used by the Ctrl + K shortcut
        public void FocusInput()
        {
            _input.Focus();
            _input.SelectAll();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_input == null)
            {
                return;
            }
            int chipSpace = Width >= 260 ? 92 : 16;
            _input.SetBounds(48, (Height - _input.Height) / 2, Math.Max(40, Width - 48 - chipSpace), _input.Height);
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

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
            bool focused = _input.Focused;
            Theme.FillRounded(g, Theme.White, r, 14);
            using (var path = Theme.RoundedRect(r, 14))
            using (Pen pen = new Pen(focused ? Theme.Violet : Theme.Border, focused ? 1.6f : 1f))
            {
                g.DrawPath(pen, path);
            }

            Icons.Draw(g, IconKind.Search, new RectangleF(16, (Height - 22) / 2f, 22, 22), focused ? Theme.Violet : Theme.Muted, 1.7f);

            if (!focused && _input.TextLength == 0 && Width >= 260)
            {
                RectangleF chip = new RectangleF(Width - 82, (Height - 28) / 2f, 68, 28);
                Theme.FillRounded(g, Theme.OffWhite, chip, 8);
                Theme.Text(g, "Ctrl + K", Theme.Semibold(12), chip, Theme.Muted, Theme.Centered);
            }
        }
    }
}
