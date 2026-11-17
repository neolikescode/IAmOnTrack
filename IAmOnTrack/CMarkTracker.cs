using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAmOnTrack
{
    public class CMarkTracker
    {
        //List holds all the modules entered
        private List<CModules> _modules;
        public IReadOnlyList<CModules> Modules
        {
            //AsReadOnly ensures that External code can inspect the modules but cannot add or remove them through the returned collection.
            get { return _modules.AsReadOnly(); }
        }
        //default constructor
        public CMarkTracker()
        {
            _modules = new List<CModules>();
        }
        public void AddModule(CModules module)
        {
            //Checks wheteher the module is null
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "Module cannot be null");
            }
            //check whether a module with the same ModuleCode, AcademicYear and Semester already exists before adding it, using system.Linq
            if (_modules.Any(m =>m.ModuleCode == module.ModuleCode && m.AcademicYear == module.AcademicYear && m.Semester == module.Semester))
            {
                throw new InvalidOperationException("Cannot add duplicate modules");
            }
            _modules.Add(module);
        }
        public CModules FindModule(string moduleCode, int academicYear, int semester)
        {
            // Access the module's
            foreach (CModules mod in _modules)
            {
                if (mod.ModuleCode.ToUpper() == moduleCode.ToUpper() && mod.AcademicYear == academicYear && mod.Semester == semester)
                {
                    return mod;
                }
            }
            return null;
        }
        //This methods allows the application to add assessments to a module
        public void AddAssessment(string moduleCode,int academicYear, int semester, CAssessments assessment)
        {
            CModules mod = FindModule(moduleCode, academicYear, semester);
            //Checks to see if the module was found
            if (mod != null)
            {
                //if found then it Adds the assessment marks
                mod.AddAssessment(assessment);
            }
            else
            {
                throw new InvalidOperationException("Module not found.", nameof(assessment));
            }
        }
        //This methods allows the application to update an assessments marks
        public void UpdateAssessmentMark(string moduleCode,int academicYear,int semester,string assessmentName,double markObtained, double maximumMark)
        {
            CModules mod = FindModule(moduleCode, academicYear, semester);
            //Checks to see if the module was found
            if (mod != null)
            {
                //if found then it updates the assessment marks
                mod.UpdateAssessmentMark(assessmentName, markObtained, maximumMark);
            }
            else
            {
                throw new InvalidOperationException("Module not found.", nameof(assessment));
            }
        }
        //This methods creates a list (it can only retrieve info since it is an IReadOnly list) that will store modules that are specific to the semester
        public IReadOnlyList<CModules> GetModulesBySemester(int academicYear, int semester)
        {
            List<CModules> modules = new List<CModules>();
            //Loops through the entire modules list
            foreach(CModules mod in _modules)
            {
                //Checks if the module being looped through has the same acdemic year and semester if so, it adds the module to the new list
                if(mod.AcademicYear == academicYear && mod.Semester == semester)
                {
                    modules.Add(mod);
                }
            }
            return modules;
        }
        //This method gets all the modules depending on the year only not semester
        public IReadOnlyList<CModules> GetModulesByYear(int academicYear)
        {
            List<CModules> modules = new List<CModules>();
            foreach (CModules mod in _modules)
            {
                //Checks if the module being looped through has the same acdemic year, if so, it adds the module to the new list
                if (mod.AcademicYear == academicYear)
                {
                    modules.Add(mod);
                }
            }
            return modules;
        }
        // This method removes a module from the tracker
        public void RemoveModule(string moduleCode, int academicYear, int semester)
        {
            // Find the module that needs to be removed
            CModules mod = FindModule(moduleCode, academicYear, semester);

            // Check whether the module exists
            if (mod != null)
            {
                _modules.Remove(mod);
            }
            else
            {
                throw new InvalidOperationException("Module not found.");
            }
        }
    }
}
