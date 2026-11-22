using System;

namespace IAmOnTrack
{
    //Holds how many assessments of a semester are completed (have a mark) and how many are remaining
    public class CSemesterProgress
    {
        private int _completed;
        public int Completed
        {
            get { return _completed; }
            private set
            {
                //DATA VALIDATION
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Completed assessments cannot be negative.");
                }
                _completed = value;
            }
        }

        private int _remaining;
        public int Remaining
        {
            get { return _remaining; }
            private set
            {
                //DATA VALIDATION
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Remaining assessments cannot be negative.");
                }
                _remaining = value;
            }
        }

        public int Total
        {
            get { return Completed + Remaining; }
        }

        //Completion percentage (0 - 100). Returns 0 when there are no assessments, to avoid dividing by zero
        public double Percentage
        {
            get
            {
                if (Total == 0)
                {
                    return 0;
                }
                return (Completed * 100.0) / Total;
            }
        }

        public CSemesterProgress(int completed, int remaining)
        {
            Completed = completed;
            Remaining = remaining;
        }
    }
}
