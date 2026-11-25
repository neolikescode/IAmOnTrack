using System;

namespace IAmOnTrack
{
    public enum RecordStatus
    {
        All,
        Marked,
        Pending
    }

    //Everything the user can filter the Records form by. A null / empty value means "no filter".
    public class CRecordFilter
    {
        private string _searchText = "";
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value == null ? "" : value.Trim(); }
        }

        public int? AcademicYear { get; set; }

        private int? _semester;
        public int? Semester
        {
            get { return _semester; }
            set
            {
                //DATA VALIDATION
                if (value.HasValue && (value.Value < 1 || value.Value > 2))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Semester must be 1 or 2");
                }
                _semester = value;
            }
        }

        public string ModuleCode { get; set; }
        public string AssessmentType { get; set; }
        public RecordStatus Status { get; set; }

        //True when at least one search / filter is applied
        public bool IsActive
        {
            get
            {
                return SearchText.Length > 0 || AcademicYear.HasValue || Semester.HasValue ||
                       !string.IsNullOrEmpty(ModuleCode) || !string.IsNullOrEmpty(AssessmentType) ||
                       Status != RecordStatus.All;
            }
        }

        //Only the year / semester / module part (used when exporting to Excel, where search text is irrelevant)
        public CRecordFilter ForExport()
        {
            CRecordFilter copy = new CRecordFilter();
            copy.AcademicYear = AcademicYear;
            copy.Semester = Semester;
            copy.ModuleCode = ModuleCode;
            return copy;
        }

        public void Clear()
        {
            SearchText = "";
            AcademicYear = null;
            Semester = null;
            ModuleCode = null;
            AssessmentType = null;
            Status = RecordStatus.All;
        }
    }
}
