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
        public List<CModules> Modules
        {
            get { return _modules; }
            private set { _modules = value; }
        }
        //default constructor
        public CMarkTracker()
        {
            Modules = new List<CModules>();
        }
    
    }
}
