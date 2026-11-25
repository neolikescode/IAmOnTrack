using System;
using System.Text.RegularExpressions;

namespace IAmOnTrack
{
    //Pairs an EXISTING assessment with the EXISTING module it belongs to. CAssessments does not know which
    //module owns it, so the Records form needs this pairing to show "module + assessment" rows.
    //It stores references only - no data is copied, so edits to the real objects show up immediately.
    public class CAssessmentRecord
    {
        private readonly CModules _module;
        private readonly CAssessments _assessment;

        public CModules Module
        {
            get { return _module; }
        }

        public CAssessments Assessment
        {
            get { return _assessment; }
        }

        public CAssessmentRecord(CModules module, CAssessments assessment)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "Module cannot be null");
            }
            if (assessment == null)
            {
                throw new ArgumentNullException(nameof(assessment), "Assessment cannot be null");
            }
            _module = module;
            _assessment = assessment;
        }

        public bool HasMark
        {
            get { return _assessment.IsMark; }
        }

        //The existing model has no "type" field, so the type is taken from the assessment name
        //by removing a trailing number: "Practical 1" -> "Practical", "Test 2" -> "Test", "Exam" -> "Exam"
        public string AssessmentType
        {
            get { return DeriveType(_assessment.AssessmentName); }
        }

        //mark obtained / maximum mark * 100, or null while the assessment is still pending
        public double? Percentage
        {
            get
            {
                if (!_assessment.IsMark || _assessment.MaximumMark <= 0)
                {
                    return null;
                }
                return (_assessment.MarkObtained / _assessment.MaximumMark) * 100;
            }
        }

        //How many percentage points of the module's semester mark this assessment contributes
        //(same calculation as the "Weighted Contribution" column in the Excel export)
        public double? WeightedContribution
        {
            get
            {
                double? percentage = Percentage;
                if (!percentage.HasValue)
                {
                    return null;
                }
                return (percentage.Value * _assessment.AssessmentWeight) / 100;
            }
        }

        public static string DeriveType(string assessmentName)
        {
            if (string.IsNullOrWhiteSpace(assessmentName))
            {
                return "Other";
            }
            string type = Regex.Replace(assessmentName.Trim(), @"[\s\-#]*\d+\s*$", "").Trim(' ', '-');
            return type.Length == 0 ? "Other" : type;
        }
    }
}
