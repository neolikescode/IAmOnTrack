using System;
using System.Collections.Generic;
using System.Linq;

namespace IAmOnTrack
{
    //Keeps the list of recent actions shown in the dashboard's "Recent Activity" card.
    //Nothing in the application calls Record() yet - once AssessmentForm / MarksForm save data they
    //can call it and the dashboard will show the entry automatically.
    public class CActivityLog
    {
        private List<CActivityEntry> _entries;

        public IReadOnlyList<CActivityEntry> Entries
        {
            //AsReadOnly lets other classes look at the entries but not add/remove them directly
            get { return _entries.AsReadOnly(); }
        }

        public CActivityLog()
        {
            _entries = new List<CActivityEntry>();
        }

        //Adds a new entry stamped with the current date and time
        public void Record(ActivityKind kind, string description)
        {
            _entries.Add(new CActivityEntry(kind, description, DateTime.Now));
        }

        //Returns the newest entries first, limited to 'count' items
        public IReadOnlyList<CActivityEntry> GetRecent(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be greater than 0.");
            }
            return _entries.OrderByDescending(e => e.Timestamp).Take(count).ToList();
        }
    }
}
