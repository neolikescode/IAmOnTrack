using System;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Enter or change the mark obtained and the maximum mark for an assessment
    public class EditMarkDialog : ModernDialog
    {
        private readonly InputField _obtained;
        private readonly InputField _maximum;
        private readonly Action<double, double> _apply;

        public EditMarkDialog(string subtitle, double? currentMark, double currentMaximum, Action<double, double> apply)
            : base(currentMark.HasValue ? "Edit mark" : "Enter mark", subtitle, 24, "Save mark", "Cancel", 76 * 2 + 12, 480)
        {
            _apply = apply;

            _obtained = new InputField("Mark obtained");
            _obtained.Dock = DockStyle.Top;
            _obtained.Value = currentMark.HasValue ? currentMark.Value.ToString("0.##") : "";

            _maximum = new InputField("Maximum mark");
            _maximum.Dock = DockStyle.Top;
            _maximum.Value = currentMaximum > 0 ? currentMaximum.ToString("0.##") : "";

            Panel gap = new Panel();
            gap.Dock = DockStyle.Top;
            gap.Height = 12;

            ContentArea.Controls.Add(_maximum);
            ContentArea.Controls.Add(gap);
            ContentArea.Controls.Add(_obtained);
            Shown += (s, e) => _obtained.FocusInput();
        }

        protected override void OnPrimaryClick()
        {
            double obtained;
            double maximum;
            if (!InputField.TryParseNumber(_obtained.Value, out obtained) || !InputField.TryParseNumber(_maximum.Value, out maximum))
            {
                MessageBox.Show(this, "Please enter both marks as numbers, for example 45 and 50.", "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _apply(obtained, maximum);
                DialogResult = DialogResult.OK;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
