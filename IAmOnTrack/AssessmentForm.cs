using System;
using System.Collections.Generic;
using System.Windows.Forms;
using IAmOnTrack.UI;

namespace IAmOnTrack
{
    //"Add assessment" pop-up: choose a module, name the assessment and set its weighting.
    //Validation (blank names, duplicates, weights over 100%) comes from the existing model classes.
    public partial class AssessmentForm : ModernDialog
    {
        private readonly CMarkTracker _tracker;
        private readonly CActivityLog _activityLog;
        private readonly CAcademicRecords _records;
        private readonly SelectField _module;
        private readonly InputField _name;
        private readonly InputField _weight;
        private readonly InputField _maximum;

        public AssessmentForm() : this(new CMarkTracker(), new CActivityLog())
        {
        }

        public AssessmentForm(CMarkTracker tracker, CActivityLog activityLog)
            : base("Add assessment", "Create a new assessment, link it to a module and set its weighting.", 24, "Add assessment", "Cancel", FieldsHeight(4), 500)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException(nameof(tracker), "Mark tracker cannot be null");
            }
            if (activityLog == null)
            {
                throw new ArgumentNullException(nameof(activityLog), "Activity log cannot be null");
            }
            _tracker = tracker;
            _activityLog = activityLog;
            _records = new CAcademicRecords(tracker, activityLog);

            InitializeComponent();

            _module = new SelectField("Module");
            _module.Placeholder = "No modules yet \u2013 add one first";
            _module.ActionText = "+ New module";
            _module.SelectionChanged += (s, e) => UpdateWeightHint();
            _module.ActionClicked += NewModule_Clicked;

            _name = new InputField("Assessment name");
            _weight = new InputField("Weight (% of the semester mark)");

            //the model needs a maximum mark when an assessment is created; it can be changed when entering the mark
            _maximum = new InputField("Maximum mark");
            _maximum.Value = "100";

            StackFields(_module, _name, _weight, _maximum);
            RefreshModules(null);
            Shown += (s, e) => { if (_module.ItemCount > 0) _name.FocusInput(); };
        }

        private void RefreshModules(CModules select)
        {
            List<KeyValuePair<string, object>> items = new List<KeyValuePair<string, object>>();
            foreach (CModules module in _records.GetModules(new CRecordFilter()))
            {
                items.Add(new KeyValuePair<string, object>(
                    module.ModuleCode + " \u2014 " + module.ModuleName + "  (" + module.AcademicYear + " \u00B7 S" + module.Semester + ")", module));
            }
            _module.SetItems(items, select);
            UpdateWeightHint();
        }

        //Shows how much of the chosen module's 100% is still available
        private void UpdateWeightHint()
        {
            CModules module = _module.SelectedValue as CModules;
            _weight.Hint = module == null ? "" : _records.GetRemainingWeight(module).ToString("0.##") + "% still available";
        }

        private void NewModule_Clicked(object sender, EventArgs e)
        {
            using (ModuleForm form = new ModuleForm(_tracker, _activityLog))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshModules(form.CreatedModule);
                    _name.FocusInput();
                }
            }
        }

        protected override void OnPrimaryClick()
        {
            CModules module = _module.SelectedValue as CModules;
            if (module == null)
            {
                MessageBox.Show(this, "Please choose a module first. If you have none yet, use \"+ New module\".", "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double weight;
            double maximum;
            if (!InputField.TryParseNumber(_weight.Value, out weight) || !InputField.TryParseNumber(_maximum.Value, out maximum))
            {
                MessageBox.Show(this, "Please enter the weight and the maximum mark as numbers, for example 20 and 100.", "IAmOnTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _records.AddAssessment(module, _name.Value, weight, maximum);
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
