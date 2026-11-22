using System;

namespace IAmOnTrack
{
    //The kind of action a "next step" prompt is guiding the user towards
    public enum NextStepKind
    {
        AddAssessments,
        RecordMarks
    }

    //One prompt shown in the dashboard's "Upcoming / Next Steps" card
    public class CNextStep
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Next step title cannot be blank.");
                }
                _title = value.Trim();
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Next step description cannot be blank.");
                }
                _description = value.Trim();
            }
        }

        private NextStepKind _kind;
        public NextStepKind Kind
        {
            get { return _kind; }
            private set { _kind = value; }
        }

        public CNextStep(NextStepKind kind, string title, string description)
        {
            Kind = kind;
            Title = title;
            Description = description;
        }
    }
}
