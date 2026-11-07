using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			set 
			{ 
				_moduleCode = value; 
			}
		}

		private string _moduleName;

		public string ModuleName
        {
			get { return _moduleName; }
			set 
			{
				
				_moduleName = value; 
			}
		}
		private int _academicYear;

		public int AcademicYear
        {
			get { return _academicYear; }
			set { _academicYear = value; }
		}
		private int _semester;

		public int Semester
        {
			get { return _semester; }
			set { _semester = value; }
		}

		// list to hold all the assessments added to the module by the user
		private List<CAssessments> _assessments;
        public List<CAssessments> Assessments
        {
            get { return _assessments; }
            private set { _assessments = value; }
        }
        //default constructor
        public CModules()
		{
            _assessments = new List<CAssessments>();
		}
	}
}
