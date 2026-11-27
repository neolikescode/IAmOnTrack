using System;
using System.Collections.Generic;
using System.Windows.Forms;
using IAmOnTrack.UI;

namespace IAmOnTrack
{
    //"Add module" pop-up. The existing CModules class validates the values and CMarkTracker rejects duplicates;
    //this form only collects the input and shows any problem in a MessageBox.
    public partial class ModuleForm : ModernDialog
    {
        private readonly CAcademicRecords _records;
        private readonly InputField _code;
        private readonly InputField _name;
        private readonly SelectField _year;
        private readonly SelectField _semester;

        //The module that was created (null if the user cancelled)
        public CModules CreatedModule { get; private set; }

        public ModuleForm() : this(new CMarkTracker(), new CActivityLog())
        {
        }

        public ModuleForm(CMarkTracker tracker, CActivityLog activityLog)
            : base("Add module", "Register a module so you can add its assessments and marks.", 24, "Add module", "Cancel", FieldsHeight(4), 500)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException(nameof(tracker), "Mark tracker cannot be null");
            }
            if (activityLog == null)
            {
                throw new ArgumentNullException(nameof(activityLog), "Activity log cannot be null");
            }
            _records = new CAcademicRecords(tracker, activityLog);

            InitializeComponent();

            _code = new InputField("Module code");
            _code.Hint = "4 letters + 4 numbers, e.g. CSIS1624";
            _name = new InputField("Module name");

            _year = new SelectField("Academic year");
            _year.SetItems(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("2026", 2026),
                new KeyValuePair<string, object>("2027", 2027)
            }, DateTime.Now.Year);

            _semester = new SelectField("Semester");
            _semester.SetItems(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Semester 1", 1),
                new KeyValuePair<string, object>("Semester 2", 2)
            }, CDashboardStatistics.GetSemesterForDate(DateTime.Now));

            StackFields(_code, _name, _year, _semester);
            Shown += (s, e) => _code.FocusInput();
        }

        protected override void OnPrimaryClick()
        {
            try
            {
                CreatedModule = _records.AddModule(_code.Value, _name.Value, (int)_year.SelectedValue, (int)_semester.SelectedValue);
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
