using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Shared behaviour for the app's custom (borderless) windows: resize edges, minimise, maximise / restore,
    //close, drag by the title bar, the navy strip behind the sidebar, and mouse-wheel forwarding.
    //Form1 (dashboard) and RecordsForm both inherit from this so they look and behave identically.
    public class BorderlessForm : Form
    {
        protected const int ResizeBorder = 6;

        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point point);

        private readonly WheelForwarder _wheelForwarder = new WheelForwarder();
        private WindowControlButton _maximizeButton;

        //The search box that Ctrl + K should focus (null = none)
        protected virtual SearchBox GlobalSearch
        {
            get { return null; }
        }

        protected BorderlessForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;
            Application.AddMessageFilter(_wheelForwarder);
        }

        //Builds the title bar (drag area + minimise / maximise / close). Docked to the top by the caller.
        protected Panel CreateTitleBar()
        {
            Panel titleBar = new Panel();
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 40;
            titleBar.BackColor = Theme.OffWhite;
            titleBar.MouseDown += DragHandle_MouseDown;

            WindowControlButton minimize = new WindowControlButton(IconKind.Minimize);
            minimize.Dock = DockStyle.Right;
            minimize.Click += (s, e) => WindowState = FormWindowState.Minimized;

            _maximizeButton = new WindowControlButton(IconKind.Maximize);
            _maximizeButton.Dock = DockStyle.Right;
            _maximizeButton.Click += (s, e) => ToggleMaximize();

            WindowControlButton close = new WindowControlButton(IconKind.Close);
            close.Dock = DockStyle.Right;
            close.Click += (s, e) => Close();

            //the control added LAST is docked first, so Close ends up on the far right
            titleBar.Controls.Add(minimize);
            titleBar.Controls.Add(_maximizeButton);
            titleBar.Controls.Add(close);
            return titleBar;
        }

        //Makes this window open with the same position / size / state as another one (used when switching pages)
        public void CopyWindowFrom(BorderlessForm other)
        {
            StartPosition = FormStartPosition.Manual;
            if (other.WindowState == FormWindowState.Maximized)
            {
                Bounds = other.RestoreBounds;
                MaximizedBounds = Screen.FromHandle(other.Handle).WorkingArea;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
                Bounds = other.Bounds;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX; //lets the taskbar minimise / restore a borderless window
                return cp;
            }
        }

        //The thin padding ring around the form belongs to the form itself, so it is where the
        //resize handles live. Returning the edge codes gives native resizing with the right cursors.
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST && WindowState == FormWindowState.Normal)
            {
                base.WndProc(ref m);
                if ((int)m.Result == HTCLIENT)
                {
                    int lp = unchecked((int)(long)m.LParam);
                    Point screenPoint = new Point((short)(lp & 0xFFFF), (short)((lp >> 16) & 0xFFFF));
                    m.Result = (IntPtr)HitTestEdges(PointToClient(screenPoint));
                }
                return;
            }
            base.WndProc(ref m);
        }

        private int HitTestEdges(Point p)
        {
            int corner = 16;
            bool left = p.X <= ResizeBorder;
            bool right = p.X >= ClientSize.Width - ResizeBorder;
            bool top = p.Y <= ResizeBorder;
            bool bottom = p.Y >= ClientSize.Height - ResizeBorder;

            if (top)
            {
                if (p.X <= corner) return HTTOPLEFT;
                if (p.X >= ClientSize.Width - corner) return HTTOPRIGHT;
                return HTTOP;
            }
            if (bottom)
            {
                if (p.X <= corner) return HTBOTTOMLEFT;
                if (p.X >= ClientSize.Width - corner) return HTBOTTOMRIGHT;
                return HTBOTTOM;
            }
            if (left)
            {
                if (p.Y <= corner) return HTTOPLEFT;
                if (p.Y >= ClientSize.Height - corner) return HTBOTTOMLEFT;
                return HTLEFT;
            }
            if (right)
            {
                if (p.Y <= corner) return HTTOPRIGHT;
                if (p.Y >= ClientSize.Height - corner) return HTBOTTOMRIGHT;
                return HTRIGHT;
            }
            return HTCLIENT;
        }

        //The ring is only needed in the normal state; when maximised the content fills the screen
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            bool maximised = WindowState == FormWindowState.Maximized;
            Padding = maximised ? Padding.Empty : new Padding(ResizeBorder);
            if (_maximizeButton != null)
            {
                _maximizeButton.Kind = maximised ? IconKind.Restore : IconKind.Maximize;
            }
            Invalidate();
        }

        //Paints the page background plus the navy strip behind the sidebar so the resize ring is seamless
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.OffWhite);
            using (SolidBrush navy = new SolidBrush(Theme.Navy))
            {
                e.Graphics.FillRectangle(navy, 0, 0, Padding.Left + SidebarPanel.PreferredWidth, Height);
            }
        }

        protected void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                //keep the taskbar visible when maximising a borderless window
                MaximizedBounds = Screen.FromHandle(Handle).WorkingArea;
                WindowState = FormWindowState.Maximized;
            }
        }

        //Drag the window by the title bar (double-click toggles maximise)
        protected void DragHandle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            if (e.Clicks == 2)
            {
                ToggleMaximize();
                return;
            }
            if (WindowState == FormWindowState.Maximized)
            {
                //restore, keeping the window under the cursor like a normal title bar
                Point cursor = Cursor.Position;
                float ratio = (float)(cursor.X - Bounds.X) / Math.Max(1, Bounds.Width);
                WindowState = FormWindowState.Normal;
                Location = new Point(cursor.X - (int)(Width * ratio), cursor.Y - 16);
            }
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        //Ctrl + K moves the cursor to the search box
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.K) && GlobalSearch != null)
            {
                GlobalSearch.FocusInput();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Application.RemoveMessageFilter(_wheelForwarder);
            base.OnFormClosed(e);
        }

        //Custom-painted controls cannot take focus, so mouse-wheel messages are passed to the control under
        //the cursor, which lets pages scroll when the window is small.
        private class WheelForwarder : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_MOUSEWHEEL)
                {
                    IntPtr target = WindowFromPoint(Cursor.Position);
                    if (target != IntPtr.Zero && target != m.HWnd && Control.FromHandle(target) != null)
                    {
                        SendMessage(target, m.Msg, m.WParam, m.LParam);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
