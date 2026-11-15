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
            //Cjcks wheteher the module is null
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
            foreach(CModules mod in _modules)
            {
                if(mod.ModuleCode == moduleCode && mod.AcademicYear == academicYear && mod.Semester == semester)
                {

                }
            }
        }
    }
}
