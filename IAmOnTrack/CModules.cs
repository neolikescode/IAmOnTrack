using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IAmOnTrack
{
    public class CModules
    {
		// 5 data fields and properties that will store and handle
		// information about the Module the user selects
        private string _moduleCode;
		public string ModuleCode
        {
			get { return _moduleCode; }
            private set
            {
                // 1. Prevent null or empty text
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Module Code cannot be blank.");
                }

                // 2. Define the exact pattern: 
                // ^         = start of string
                // [a-zA-Z]{4} = exactly 4 alphabetic characters
                // \d{4}     = exactly 4 digits (numbers)
                // $         = end of string
                string moduleCodePattern = @"^[a-zA-Z]{4}\d{4}$";

                // 3. Validate against the pattern
                if (!Regex.IsMatch(value.Trim().ToUpper(), moduleCodePattern))
                {
                    throw new FormatException("Module Code must consist of exactly 4 letters followed by 4 numbers (e.g., CSIS1624).");
                }

                // 4. Save the value
                _moduleCode = value.Trim().ToUpper();
			}
		}

		private string _moduleName;

		public string ModuleName
        {
			get { return _moduleName; }
            private set
            {
						//DATA VALIDATION

				//to prevent empty text from being entered
				if(string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException("Module Name cannot be blank");
				} 
				string makePattern = @"^[a-zA-Z\s-]+$";
                //Checks if the text the user entered only contains letters, spaces, or hyphens.
                if (!Regex.IsMatch(value.Trim(), makePattern))
				{
                    throw new FormatException("Module Name can only contain letters, spaces, or hyphens.");
                }
				_moduleName = value.Trim(); 
			}
		}
		private int _academicYear;

		public int AcademicYear
        {
			get { return _academicYear; }
            private set
            {
							//DATA VALIDATION
				//Checks to see if the academic year is not less than 2026 or greater than 2027
				// will edit later next year to include 2028
				if(value < 2026 || value > 2027)
				{
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Academic year must be between 2026 and 2027.");
                }
				_academicYear = value; 
			}
		}
		private int _semester;

		public int Semester
        {
			get { return _semester; }
            private set
            {
                //DATA VALIDATION
                //Checks to see if the semester entered is either 1 or 2
                if (value < 1 || value > 2)
                {
					throw new ArgumentOutOfRangeException(nameof(value), value, "Semester must between 1 and 2");
                }
                _semester = value; 
			}
		}

		// list to hold all the assessments added to the module by the user
		private List<CAssessments> _assessments;
		//List is made read only to prevent external classes from modifying it and adding null/blank assessments through the getter
        public IReadOnlyList<CAssessments> Assessments
        {
            //AsReadOnly ensures that External code can inspect the assessments but cannot add or remove them through the returned collection.
            get { return _assessments.AsReadOnly(); }
        }
        //parameterized constructor that accepts the module code, name, semester and academic year
        public CModules(string moduleCode, string moduleName, int academicYear, int semester)
		{
            ModuleCode = moduleCode;
            ModuleName = moduleName;
            AcademicYear = academicYear;
            Semester = semester;
            _assessments = new List<CAssessments>();
		}
        //This method checks to see the assessment object being added is not null and if its not it adds it to the Aseessments list
        public void AddAssessment(CAssessments assessment)
        {
            //This method checks to see the assessment object being added is not null and if its not it adds it to the Assessments list
            if (assessment == null)
            {
                throw new ArgumentNullException(nameof(assessment), "Assessment cannot be null");
            }

            //Check whether an assessment with the same name already exists using system.Linq
            if (_assessments.Any(a => a.AssessmentName.Equals(assessment.AssessmentName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Cannot add duplicate assessments");
            }

            //Calculate the combined assessment weight before adding the new assessment
            double totalWeight = _assessments.Sum(a => a.AssessmentWeight) + assessment.AssessmentWeight;

            //Check whether the combined assessment weight exceeds 100%
            if (totalWeight > 100)
            {
                throw new InvalidOperationException("Combined assessment weights cannot exceed 100%");
            }

            _assessments.Add(assessment);
        }
        // Searches for an existing assessment by name and returns it
        public CAssessments FindAssessment(string assessmentName)
        {
            // Access the module's assessments
            foreach (CAssessments assess in _assessments)
            {
                // Searches the list for the assessment name, ignoring case differences
                if (string.Equals(assess.AssessmentName, assessmentName, StringComparison.OrdinalIgnoreCase))
                {
                    return assess;
                }
            }
            // Return null if no matching assessment is found
            return null;
        }
        public void UpdateAssessmentMark(string assessmentName, double markObtained, double maximumMark)
        {
            CAssessments assess = FindAssessment(assessmentName);
            if(assess != null)
            {
               assess.EnterMark(markObtained, maximumMark);
            }
            else
            {
                throw new ArgumentNullException("Assessment not found", nameof(assess));
            }
        }
    }
}
