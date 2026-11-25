using System;
using System.Collections.Generic;
using System.Linq;

namespace IAmOnTrack
{
    //Business logic behind the Records form. It works on the SAME CMarkTracker the dashboard uses and
    //only wraps the existing methods (search / filter / update / delete / export). It never shows a MessageBox -
    //validation errors thrown by the existing models simply travel up to the form's event handlers.
    public class CAcademicRecords
    {
        private readonly CMarkTracker _tracker;
        private readonly CActivityLog _activityLog;
        private readonly CDashboardStatistics _statistics;

        //Overview figures (modules, assessments, marks, average) come from the existing statistics class
        public CDashboardStatistics Statistics
        {
            get { return _statistics; }
        }

        public CAcademicRecords(CMarkTracker tracker, CActivityLog activityLog)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException(nameof(tracker), "Mark tracker cannot be null");
            }
            if (activityLog == null)
            {
                throw new ArgumentNullException(nameof(activityLog), "Activity log cannot be null");
            }
            _tracker = tracker;
            _activityLog = activityLog;
            _statistics = new CDashboardStatistics(tracker);
        }

        // ---------------- Reading / filtering ----------------

        //Modules that match the year, semester, module and search filters.
        //(Assessment type and status only make sense for assessment rows, so they are ignored here.)
        public IReadOnlyList<CModules> GetModules(CRecordFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null");
            }
            return _tracker.Modules
                .Where(m => MatchesModule(m, filter))
                .Where(m => filter.SearchText.Length == 0 || ModuleMatchesSearch(m, filter.SearchText) ||
                            m.Assessments.Any(a => Contains(a.AssessmentName, filter.SearchText)))
                .OrderBy(m => m.AcademicYear).ThenBy(m => m.Semester).ThenBy(m => m.ModuleCode)
                .ToList();
        }

        //One row per assessment (with its module) that matches every filter
        public IReadOnlyList<CAssessmentRecord> GetAssessmentRecords(CRecordFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null");
            }

            List<CAssessmentRecord> result = new List<CAssessmentRecord>();
            foreach (CModules module in _tracker.Modules
                .Where(m => MatchesModule(m, filter))
                .OrderBy(m => m.AcademicYear).ThenBy(m => m.Semester).ThenBy(m => m.ModuleCode))
            {
                foreach (CAssessments assessment in module.Assessments)
                {
                    CAssessmentRecord record = new CAssessmentRecord(module, assessment);

                    if (filter.SearchText.Length > 0 && !ModuleMatchesSearch(module, filter.SearchText) &&
                        !Contains(assessment.AssessmentName, filter.SearchText))
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(filter.AssessmentType) &&
                        !string.Equals(record.AssessmentType, filter.AssessmentType, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (filter.Status == RecordStatus.Marked && !record.HasMark)
                    {
                        continue;
                    }
                    if (filter.Status == RecordStatus.Pending && record.HasMark)
                    {
                        continue;
                    }
                    result.Add(record);
                }
            }
            return result;
        }

        public IReadOnlyList<int> GetAcademicYears()
        {
            return _tracker.Modules.Select(m => m.AcademicYear).Distinct().OrderBy(y => y).ToList();
        }

        public IReadOnlyList<string> GetModuleCodes()
        {
            return _tracker.Modules.Select(m => m.ModuleCode).Distinct().OrderBy(c => c).ToList();
        }

        public IReadOnlyList<string> GetAssessmentTypes()
        {
            return _tracker.Modules
                .SelectMany(m => m.Assessments)
                .Select(a => CAssessmentRecord.DeriveType(a.AssessmentName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t)
                .ToList();
        }

        // ---------------- Changing data (existing model methods do the validation) ----------------

        public void UpdateAssessment(CAssessmentRecord record, string newName, double newWeight)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record), "Record cannot be null");
            }
            record.Module.UpdateAssessmentDetails(record.Assessment.AssessmentName, newName, newWeight);
            _activityLog.Record(ActivityKind.ModuleUpdated, "Updated assessment " + record.Assessment.AssessmentName + " in " + record.Module.ModuleCode);
        }

        public void UpdateMark(CAssessmentRecord record, double markObtained, double maximumMark)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record), "Record cannot be null");
            }
            CModules module = record.Module;
            _tracker.UpdateAssessmentMark(module.ModuleCode, module.AcademicYear, module.Semester,
                record.Assessment.AssessmentName, markObtained, maximumMark);
            _activityLog.Record(ActivityKind.MarksRecorded, "Recorded " + markObtained.ToString("0.##") + "/" + maximumMark.ToString("0.##") +
                " for " + record.Assessment.AssessmentName + " (" + module.ModuleCode + ")");
        }

        public void DeleteAssessment(CAssessmentRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record), "Record cannot be null");
            }
            string name = record.Assessment.AssessmentName;
            record.Module.RemoveAssessment(name);
            _activityLog.Record(ActivityKind.ModuleUpdated, "Removed assessment " + name + " from " + record.Module.ModuleCode);
        }

        public void DeleteModule(CModules module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "Module cannot be null");
            }
            _tracker.RemoveModule(module.ModuleCode, module.AcademicYear, module.Semester);
            _activityLog.Record(ActivityKind.ModuleUpdated, "Removed module " + module.ModuleCode);
        }

        // ---------------- Excel (uses the EXISTING CFileManager.ExportModule) ----------------

        //Exports every module matching the year / semester / module filters. Returns how many were exported.
        public int ExportModules(CRecordFilter filter, CFileManager fileManager)
        {
            if (fileManager == null)
            {
                throw new ArgumentNullException(nameof(fileManager), "File manager cannot be null");
            }
            int exported = 0;
            foreach (CModules module in GetModules(filter.ForExport()))
            {
                fileManager.ExportModule(module);
                exported++;
            }
            return exported;
        }

        // ---------------- helpers ----------------

        private static bool MatchesModule(CModules module, CRecordFilter filter)
        {
            if (filter.AcademicYear.HasValue && module.AcademicYear != filter.AcademicYear.Value)
            {
                return false;
            }
            if (filter.Semester.HasValue && module.Semester != filter.Semester.Value)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(filter.ModuleCode) &&
                !string.Equals(module.ModuleCode, filter.ModuleCode, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        private static bool ModuleMatchesSearch(CModules module, string text)
        {
            return Contains(module.ModuleCode, text) || Contains(module.ModuleName, text);
        }

        private static bool Contains(string source, string text)
        {
            return source.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
