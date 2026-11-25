using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //"Showing 1-8 of 24 records" with Previous / Next buttons
    public class PagerBar : Control
    {
        private readonly RoundedButton _previous;
        private readonly RoundedButton _next;
        private string _summary = "";

        public event EventHandler PreviousClicked;
        public event EventHandler NextClicked;

        public PagerBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = Theme.White;
            Height = 56;

            _previous = CreateButton("Previous");
            _previous.LeadingIcon = IconKind.ChevronLeft;
            _previous.Click += (s, e) => { if (PreviousClicked != null) PreviousClicked(this, EventArgs.Empty); };

            _next = CreateButton("Next");
            _next.TrailingIcon = IconKind.ChevronRight;
            _next.Click += (s, e) => { if (NextClicked != null) NextClicked(this, EventArgs.Empty); };

            Controls.Add(_previous);
            Controls.Add(_next);
        }

        private RoundedButton CreateButton(string text)
        {
            RoundedButton button = new RoundedButton();
            button.Text = text;
            button.Font = Theme.Semibold(14);
            button.ButtonColor = Theme.Lavender;
            button.HoverColor = Theme.LavenderDeep;
            button.PressedColor = Theme.LavenderDeep;
            button.TextColor = Theme.Violet;
            button.BackColor = Theme.White;
            button.CornerRadius = 10;
            button.Size = new Size(108, 38);
            return button;
        }

        //lastShown is the 1-based number of the last row on this page
        public void SetState(int firstShown, int lastShown, int total, bool hasPrevious, bool hasNext)
        {
            if (total == 0)
            {
                _summary = "No records to show";
            }
            else
            {
                _summary = "Showing " + firstShown + "\u2013" + lastShown + " of " + total + (total == 1 ? " record" : " records");
            }
            _previous.Enabled = hasPrevious;
            _next.Enabled = hasNext;
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_next == null || _previous == null)
            {
                return;
            }
            int y = (Height - _next.Height) / 2;
            _next.Location = new Point(Width - _next.Width, y);
            _previous.Location = new Point(_next.Left - 10 - _previous.Width, y);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            Theme.Text(g, _summary, Theme.Font(14), new RectangleF(0, 0, Width - 240, Height), Theme.Muted, Theme.SingleLine);
        }
    }
}
