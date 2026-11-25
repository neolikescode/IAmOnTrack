using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Read-only details of a module or an assessment
    public class RecordDetailsDialog : ModernDialog
    {
        public RecordDetailsDialog(string title, string subtitle, IList<KeyValuePair<string, string>> fields)
            : base(title, subtitle, 24, "Close", null, fields.Count * KeyValueList.RowHeight + 4, 480)
        {
            KeyValueList list = new KeyValueList(fields);
            list.Dock = DockStyle.Fill;
            ContentArea.Controls.Add(list);
        }
    }
}
