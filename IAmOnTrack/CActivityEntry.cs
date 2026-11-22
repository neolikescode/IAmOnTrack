using System;

namespace IAmOnTrack
{
    //The different kinds of actions that can appear in the Recent Activity list
    public enum ActivityKind
    {
        AssessmentAdded,
        MarksRecorded,
        ModuleUpdated
    }

    //Represents ONE action the user performed (e.g. "Added assessment Test 1 to CSIS1614")
    public class CActivityEntry
    {
        private ActivityKind _kind;
        public ActivityKind Kind
        {
            get { return _kind; }
            private set
            {
                //DATA VALIDATION
                if (!Enum.IsDefined(typeof(ActivityKind), value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Activity kind is not valid.");
                }
                _kind = value;
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            private set
            {
                //DATA VALIDATION
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Activity description cannot be blank.");
                }
                _description = value.Trim();
            }
        }

        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get { return _timestamp; }
            private set { _timestamp = value; }
        }

        public CActivityEntry(ActivityKind kind, string description, DateTime timestamp)
        {
            Kind = kind;
            Description = description;
            Timestamp = timestamp;
        }
    }
}
