using System;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Edit an assessment's name and weight. The dialog only collects the values: the supplied 'apply' action
    //calls the existing model (which validates and throws). Expected errors are shown in a MessageBox and the
    //dialog stays open so the user can correct them.
    public class EditAssessmentDialog : ModernDialog
    {
        private readonly InputField _name;
        private readonly InputField _weight;
        private readonly Action<string, double> _apply;

        public EditAssessmentDialog(string subtitle, string currentName, double currentWeight, Action<string, double> apply)
            : base("Edit assessment", subtitle, 24, "Save changes", "Cancel", 76 * 2 + 12, 480)
        {
            _apply = apply;

            _name = new InputField("Assessment name");
            _name.Dock = DockStyle.Top;
            _name.Value = currentName;

            _weight = new InputField("Weight (% of the semester mark)");
            _weight.Dock = DockStyle.Top;
            _weight.Value = currentWeight.ToString("0.##");

            Panel gap = new Panel();
            gap.Dock = DockStyle.Top;
            gap.Height = 12;

            //controls docked to the top are stacked in reverse order of adding
            ContentArea.Controls.Add(_weight);
            ContentArea.Controls.Add(gap);
            ContentArea.Controls.Add(_name);
            Shown += (s, e) => _name.FocusInput();
        }

        protected override void OnPrimaryClick()
        {
            double weight;
            if (!InputField.TryParseNumber(_weight.Value, out weight))
            {
                MessageBox.Show(this, "Please enter the weight as a number, for example 20.", "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _weight.FocusInput();
                return;
            }

            try
            {
                _apply(_name.Value, weight);
                DialogResult = DialogResult.OK;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (FormatException ex)
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
