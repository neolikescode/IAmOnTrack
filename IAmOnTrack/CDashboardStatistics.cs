using System;
using System.Collections.Generic;
using System.Linq;

namespace IAmOnTrack
{
    //Business logic for the dashboard. It READS from the existing CMarkTracker and calculates the
    //numbers shown on the main form. It never shows a MessageBox and never touches any controls.
    public class CDashboardStatistics
    {
        private readonly CMarkTracker _tracker;

        public CDashboardStatistics(CMarkTracker tracker)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException(nameof(tracker), "Mark tracker cannot be null");
            }
            _tracker = tracker;
        }

        //Number of registered modules
        public int ModuleCount
        {
            get { return _tracker.Modules.Count; }
        }

        //Number of registered assessments across every module
        public int AssessmentCount
        {
            get { return _tracker.Modules.Sum(m => m.Assessments.Count); }
        }

        //Number of assessments that already have a mark entered
        public int MarksRecordedCount
        {
            get { return _tracker.Modules.Sum(m => m.Assessments.Count(a => a.IsMark)); }
        }

        //Number of modules that have at least one marked assessment
        public int ModulesWithMarksCount
        {
            get { return _tracker.Modules.Count(m => GetModulePerformance(m).HasValue); }
        }

        //Performance across completed assessments for ONE module (0 - 100), or null if nothing is marked yet.
        //This follows the same formula CFileManager uses for the "Performance Across Completed Assessments"
        //row in the Excel summary: earned contribution / completed weight * 100
        public double? GetModulePerformance(CModules module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "Module cannot be null");
            }

            double earnedContribution = 0;
            double completedWeight = 0;

            foreach (CAssessments assessment in module.Assessments)
            {
                if (assessment.IsMark && assessment.MaximumMark > 0)
                {
                    double percentage = (assessment.MarkObtained / assessment.MaximumMark) * 100;
                    earnedContribution += (percentage * assessment.AssessmentWeight) / 100;
                    completedWeight += assessment.AssessmentWeight;
                }
            }

            //prevents division by zero when no assessments have been completed
            if (completedWeight <= 0)
            {
                return null;
            }
            return (earnedContribution / completedWeight) * 100;
        }

        //Overall academic average = the average of every module's performance (modules without marks are skipped).
        //Returns null when no marks have been recorded at all, so the UI can show an empty state.
        public double? OverallAverage
        {
            get
            {
                List<double> performances = new List<double>();
                foreach (CModules module in _tracker.Modules)
                {
                    double? performance = GetModulePerformance(module);
                    if (performance.HasValue)
                    {
                        performances.Add(performance.Value);
                    }
                }
                if (performances.Count == 0)
                {
                    return null;
                }
                return performances.Average();
            }
        }

        //Completed vs remaining assessments for the modules of one semester
        public CSemesterProgress GetSemesterProgress(int academicYear, int semester)
        {
            int completed = 0;
            int remaining = 0;

            foreach (CModules module in _tracker.GetModulesBySemester(academicYear, semester))
            {
                foreach (CAssessments assessment in module.Assessments)
                {
                    if (assessment.IsMark)
                    {
                        completed++;
                    }
                    else
                    {
                        remaining++;
                    }
                }
            }
            return new CSemesterProgress(completed, remaining);
        }

        //Works out which semester a date falls in. January - June = Semester 1, July - December = Semester 2
        public static int GetSemesterForDate(DateTime date)
        {
            return date.Month <= 6 ? 1 : 2;
        }

        //Guidance prompts that change depending on how far the user has got
        public IReadOnlyList<CNextStep> GetNextSteps()
        {
            List<CNextStep> steps = new List<CNextStep>();

            if (ModuleCount == 0 || AssessmentCount == 0)
            {
                steps.Add(new CNextStep(NextStepKind.AddAssessments, "Add your modules and assessments", "Set up your academic structure."));
                steps.Add(new CNextStep(NextStepKind.RecordMarks, "Record your marks", "Start tracking your performance."));
            }
            else
            {
                int waiting = AssessmentCount - MarksRecordedCount;
                if (waiting > 0)
                {
                    string noun = waiting == 1 ? "assessment" : "assessments";
                    steps.Add(new CNextStep(NextStepKind.RecordMarks, "Record marks for " + waiting + " " + noun, "These are still waiting for a mark."));
                }
                else
                {
                    steps.Add(new CNextStep(NextStepKind.AddAssessments, "Add your next assessment", "Every assessment so far has a mark."));
                }
            }
            return steps;
        }
    }
}
