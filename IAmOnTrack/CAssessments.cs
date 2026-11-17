using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IAmOnTrack
{
    public class CAssessments
    {
        //Five private data fields and 4 properties that will store and handle
		//all the information about the assessment the user wants to enter
        private string _assessmentName;
		public string AssessmentName
        {
			get { return _assessmentName; }
            private set
            {

                //DATA VALIDATION

                //to prevent empty text from being entered
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Assessment Name cannot be blank");
                }
                string makePattern = @"^[a-zA-Z0-9\s-]+$";
                //Checks if the text the user entered only contains letters, spaces, or hyphens.
                if (!Regex.IsMatch(value.Trim(), makePattern))
                {
                    throw new FormatException("Assessment Name can only contain letters, numbers, spaces, or hyphens.");
                }
                _assessmentName = value.Trim();
			}
		}
		private double _assessmentWeight;

		public double AssessmentWeight
        {
			get { return _assessmentWeight; }
            private set
            {
                //DATA VALIDATION
                if (value <= 0 || value > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Assessment weight cannot be greater than 100 or less than or equal to 0");
                }
                _assessmentWeight = value; 
			}
		}
        private double _maximumMark;
        public double MaximumMark
        {
            get { return _maximumMark; }
            private set
            {
                //DATA VALIDATION
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Maximum mark possible cannot be less than or equal to 0");
                }
                _maximumMark = value;
            }
        }
        private double _markObtained;

		public double MarkObtained
        {
			get { return _markObtained; }
            private set
            {
                //DATA VALIDATION
                if(value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Mark obtained cannot be a negative number");
                }
                _markObtained = value; 
			}
		}
		
		private bool _isMark;
		public bool IsMark
        {
			get { return _isMark; }
            //set to private to prevent other classes from setting it to true or false
            private set { _isMark = value;}
		}
        //Parameterized constructor that will be used when user has not recieved their mark obtained
        public CAssessments(string assessmentName, double assessmentWeight, double assessmentMaximumMark)
        {
            AssessmentName = assessmentName;
            AssessmentWeight = assessmentWeight;
            MaximumMark = assessmentMaximumMark;
            IsMark = false;
        }
        //Parameterized constructor that will be used when user has recieved their mark obtained
        public CAssessments(string assessmentName, double assessmentWeight, double assessmentMaximumMark, double assessmentMarkObtained)
        {
            AssessmentName = assessmentName;
            AssessmentWeight = assessmentWeight;
            EnterMark(assessmentMarkObtained, assessmentMaximumMark);
        }
        //This method recieves the Mark obtained and Maximum mark of an assessment (it adds or updates the marks)
        //in order to prevent the user from entering a mark obtained that is greater than the maximum value
        public void EnterMark(double markObtained, double maximumMark)
        { 
            if (maximumMark > 0)
            {
                if(markObtained < 0 || markObtained > maximumMark)
                {
                    throw new ArgumentOutOfRangeException(nameof(markObtained), markObtained, "Mark obtained cannot be less than 0 or greater than the maximum number");
                }
                MaximumMark = maximumMark;
                MarkObtained = markObtained;
                IsMark = true;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(maximumMark), maximumMark, "Maximum mark cannot be less or equal to 0");
            }
            
        }
        // This method (updates/ allows you to edit) the assessment name and weight
        public void UpdateAssessmentDetails(string assessmentName, double assessmentWeight)
        {
            //since the AssessmentName and AssessmentWeight setters are private, we need a controlled method to update them
            AssessmentName = assessmentName;
            AssessmentWeight = assessmentWeight;
        }
    }
}
