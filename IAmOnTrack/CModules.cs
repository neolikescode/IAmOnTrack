using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAmOnTrack
{
    public class CModules
    {
		// 3 data fields and properties that will store and handle
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
		// list to hold all the assessments added to the module by the user
		public static List<CAssessments> assessments;
		//default constructor
		public CModules()
		{
            assessments = new List<CAssessments>();
		}
	}
}
