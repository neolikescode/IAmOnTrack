using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			set 
			{ 

				_assessmentName = value; 
			}
		}
		private double _assessmentWeight;

		public double AssessmentWeight
        {
			get { return _assessmentWeight; }
            set
            {

                _assessmentWeight = value; 
			}
		}
		private double _markObtained;

		public double MarkObtained
        {
			get { return _markObtained; }
            set
            {

                _markObtained = value; 
			}
		}
		private double _maximumMark;
		public double MaximumMark
        {
			get { return _maximumMark; }
            set
            {

               _maximumMark = value; 
			}
		}
		private bool _isMark;
		public bool IsMark
        {
			get { return _isMark; }
            set { _isMark = value;}
		}
		//default constructor
		public CAssessments()
		{

		}
	}
}
