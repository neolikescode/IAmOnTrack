using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Base for the small pop-up windows (details / edit / confirm): borderless, rounded, white, with a
    //title, a short description, a content area and primary / secondary buttons.
    public class ModernDialog : Form
    {
        private const int SidePad = 28;

        private readonly string _title;
        private readonly string _subtitle;
        private readonly int _subtitleHeight;

        protected Panel ContentArea { get; private set; }
        protected RoundedButton PrimaryButton { get; private set; }
        protected RoundedButton SecondaryButton { get; private set; }

        protected ModernDialog(string title, string subtitle, int subtitleHeight, string primaryText,
                               string secondaryText, int contentHeight, int width)
        {
            _title = title;
            _subtitle = subtitle;
            _subtitleHeight = subtitleHeight;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            BackColor = Theme.White;
            KeyPreview = true;

            int contentTop = 60 + subtitleHeight + 14;
            ClientSize = new Size(width, contentTop + contentHeight + 22 + 44 + SidePad);

            ContentArea = new Panel();
            ContentArea.BackColor = Theme.White;
            ContentArea.SetBounds(SidePad, contentTop, width - SidePad * 2, contentHeight);
            Controls.Add(ContentArea);

            int buttonTop = ClientSize.Height - SidePad - 44;
            PrimaryButton = new RoundedButton();
            PrimaryButton.Text = primaryText;
            PrimaryButton.BackColor = Theme.White;
            //the main button grows to fit its text (e.g. "Delete assessment")
            int primaryWidth = Math.Max(132, TextRenderer.MeasureText(primaryText, Theme.Semibold(15)).Width + 52);
            PrimaryButton.SetBounds(width - SidePad - primaryWidth, buttonTop, primaryWidth, 44);
            PrimaryButton.Click += (s, e) => OnPrimaryClick();
            Controls.Add(PrimaryButton);

            if (secondaryText != null)
            {
                SecondaryButton = new RoundedButton();
                SecondaryButton.Text = secondaryText;
                SecondaryButton.BackColor = Theme.White;
                SecondaryButton.ButtonColor = Theme.Lavender;
                SecondaryButton.HoverColor = Theme.LavenderDeep;
                SecondaryButton.PressedColor = Theme.LavenderDeep;
                SecondaryButton.TextColor = Theme.Violet;
                SecondaryButton.SetBounds(PrimaryButton.Left - 10 - 112, buttonTop, 112, 44);
                SecondaryButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
                Controls.Add(SecondaryButton);
            }
        }

        //Called when the main button is pressed. Derived dialogs validate here and only close when it succeeds.
        protected virtual void OnPrimaryClick()
        {
            DialogResult = DialogResult.OK;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                return true;
            }
            if (keyData == Keys.Enter)
            {
                OnPrimaryClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            using (var path = Theme.RoundedRect(new RectangleF(0, 0, Width, Height), 18))
            {
                Region = new Region(path);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Theme.HighQuality(g);

            using (var path = Theme.RoundedRect(new RectangleF(0.5f, 0.5f, Width - 1, Height - 1), 18))
            using (Pen pen = new Pen(Theme.LavenderDeep, 1.5f))
            {
                g.DrawPath(pen, path);
            }

            Theme.Text(g, _title, Theme.Font(22, FontStyle.Bold), new RectangleF(SidePad, 24, Width - SidePad * 2, 32), Theme.Charcoal, Theme.SingleLine);
            Theme.Text(g, _subtitle, Theme.Font(14), new RectangleF(SidePad, 60, Width - SidePad * 2, _subtitleHeight), Theme.Muted, Theme.Wrapped);
        }
    }
}
