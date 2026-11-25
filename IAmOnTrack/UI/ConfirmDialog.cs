using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //"Are you sure?" confirmation. Pass dangerous = true for destructive actions such as delete.
    public class ConfirmDialog : ModernDialog
    {
        public ConfirmDialog(string title, string message, string confirmText, bool dangerous)
            : base(title, message, 66, confirmText, "Cancel", 0, 460)
        {
            if (dangerous)
            {
                PrimaryButton.ButtonColor = Theme.Danger;
                PrimaryButton.HoverColor = Color.FromArgb(214, 60, 66);
                PrimaryButton.PressedColor = Color.FromArgb(190, 50, 56);
            }
        }

        //Shows the dialog and returns true when the user confirmed
        public static bool Ask(IWin32Window owner, string title, string message, string confirmText, bool dangerous)
        {
            using (ConfirmDialog dialog = new ConfirmDialog(title, message, confirmText, dangerous))
            {
                return dialog.ShowDialog(owner) == DialogResult.OK;
            }
        }
    }
}
