using System;

namespace IAmOnTrack
{
    //Pagination maths for the records table (page numbers are 0-based internally)
    public class CPager
    {
        private int _pageSize = 8;
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                //DATA VALIDATION
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Page size must be at least 1");
                }
                _pageSize = value;
                Clamp();
            }
        }

        private int _totalItems;
        public int TotalItems
        {
            get { return _totalItems; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Total items cannot be negative");
                }
                _totalItems = value;
                Clamp();
            }
        }

        public int CurrentPage { get; private set; }

        public int PageCount
        {
            get { return Math.Max(1, (int)Math.Ceiling(_totalItems / (double)_pageSize)); }
        }

        //Index of the first item on this page, and the index just AFTER the last item
        public int FirstIndex
        {
            get { return CurrentPage * _pageSize; }
        }

        public int EndIndex
        {
            get { return Math.Min(_totalItems, FirstIndex + _pageSize); }
        }

        public bool HasPrevious
        {
            get { return CurrentPage > 0; }
        }

        public bool HasNext
        {
            get { return CurrentPage < PageCount - 1; }
        }

        public void Next()
        {
            if (HasNext)
            {
                CurrentPage++;
            }
        }

        public void Previous()
        {
            if (HasPrevious)
            {
                CurrentPage--;
            }
        }

        public void Reset()
        {
            CurrentPage = 0;
        }

        private void Clamp()
        {
            if (CurrentPage > PageCount - 1)
            {
                CurrentPage = PageCount - 1;
            }
        }
    }
}
