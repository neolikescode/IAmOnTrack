using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace IAmOnTrack
{
    public class CFileManager
    {
        private string folderName = @"C:\Users\USER\OneDrive\Documents\UFS Grades";
        public string FolderName
        {
            get { return folderName; }
            private set { folderName = value; }
        }

        private string _baseDirectory;

        public string BaseDirectory
        {
            get { return _baseDirectory; }
            private set { _baseDirectory = value; }
        }

        public CFileManager()
        {
            BaseDirectory = FolderName;
            //creates the directory if it doesn't already exist.
            Directory.CreateDirectory(BaseDirectory);
        }
        //This method creates the Directory
        public string CreateYearDirectory(int year)
        {
            string directoryPath = Path.Combine(_baseDirectory, year.ToString());
            Directory.CreateDirectory(directoryPath);
            return directoryPath;
        }
        //This method creates the Excel File
        public string CreateSemesterWorkbookPath(string yearDirectoryPath, int semester)
        {
            string excelFile = $"Semester {semester}.xlsx";
            string filePath = Path.Combine(yearDirectoryPath, excelFile);
            return filePath;
        }

        /*
                            ///EXCEL PORTION///
        */

        //This method creates or opens the Excel workbook we will be saving data to
        public XLWorkbook OpenOrCreateWorkbook(string filePath)
        {
            //Checks if the Excel workbook exists, if so it opens it, if not it ctreates the workbook 
            if (File.Exists(filePath))
            {
                return new XLWorkbook(filePath);
            }
            else
            {
                return new XLWorkbook();
            }
        }
        //this method creates or opens the Excel Worksheet
        public IXLWorksheet GetOrCreateWorksheet(XLWorkbook workbook, string moduleCode)
        {
            //Checks if the Excel worksheet exists, if so it opens it, if not it ctreates the worksheet 
            if (workbook.Worksheets.Contains(moduleCode))
            {
                return workbook.Worksheet(moduleCode);
            }
            else
            {
                return workbook.Worksheets.Add(moduleCode);
            }
        }
        //this method designs the header
        public void SetupWorksheetHeader(IXLWorksheet worksheet, CModules module)
        {
            // Title
            worksheet.Cell(1, 1).Value = "GRADE SORTER";

            // Module information
            worksheet.Cell(2, 1).Value = "Module Code:";
            worksheet.Cell(2, 2).Value = module.ModuleCode;

            worksheet.Cell(2, 4).Value = "Academic Year:";
            worksheet.Cell(2, 5).Value = module.AcademicYear;

            worksheet.Cell(3, 1).Value = "Module Name:";
            worksheet.Cell(3, 2).Value = module.ModuleName;

            worksheet.Cell(3, 4).Value = "Semester:";
            worksheet.Cell(3, 5).Value = module.Semester;

            // combines cells A1 through F1 into one larger cell for the title
            worksheet.Range("A1:F1").Merge();
        }
        //This method formats the Excel WorkSheet Table
        public void SetupAssessmentTable(IXLWorksheet worksheet)
        {
            worksheet.Cell(5, 1).Value = "Assessment";

            worksheet.Cell(5, 2).Value = "Weight (%)";

            worksheet.Cell(5, 3).Value = "Mark Obtained";

            worksheet.Cell(5, 4).Value = "Maximum Mark";

            worksheet.Cell(5, 5).Value = "Percentage";

            worksheet.Cell(5, 6).Value = "Weighted Contribution";
        }
        //This method enters the data from the other classes, into the excel workook
        public void PopulateAssessmentRows(IXLWorksheet worksheet, CModules module)
        {
            // Assessment records start at row 6
            int row = 6;

            // Loop through all assessments belonging to the module
            foreach (CAssessments assessment in module.Assessments)
            {
                // Insert assessment name and weight
                worksheet.Cell(row, 1).Value = assessment.AssessmentName;
                worksheet.Cell(row, 2).Value = assessment.AssessmentWeight;

                // Check whether a mark has been entered
                if (assessment.IsMark == true)
                {
                    // Insert the marks
                    worksheet.Cell(row, 3).Value = assessment.MarkObtained;
                    worksheet.Cell(row, 4).Value = assessment.MaximumMark;

                    double percentage = 0;
                    double contribution = 0;
                    if (assessment.MaximumMark > 0)
                    {
                        // Calculate students percentage
                        percentage = (assessment.MarkObtained / assessment.MaximumMark) * 100;
                        // Calculate students weighted contribution
                        contribution = (percentage * assessment.AssessmentWeight) / 100;
                    }

                    worksheet.Cell(row, 5).Value = percentage;
                    worksheet.Cell(row, 6).Value = contribution;
                }

                // Move to the next Excel row
                row++;
            }
        }
        //This method calculates and displays the summmary of the students work/marks
        public void SetupSemesterSummary(IXLWorksheet worksheet, CModules module)
        {
            double earnedContribution = 0;
            double completedAssessmentWeight = 0;
            foreach (CAssessments assessment in module.Assessments)
            {
                if (assessment.IsMark == true && assessment.MaximumMark > 0)
                {
                    // Calculate the assessment percentage
                    double percentage = 0;
                    percentage = (assessment.MarkObtained / assessment.MaximumMark) * 100;

                    // Calculate the weighted contribution
                    double contribution = (percentage * assessment.AssessmentWeight) / 100;

                    // Add to the earned semester contribution
                    earnedContribution += contribution;

                    // Add the assessment's weight to completed assessment weight
                    completedAssessmentWeight += assessment.AssessmentWeight;
                }
            }
            //calculate the performance across completed assessments:
            double performance = 0;
            //condition prevents division by zero when no assessments have been completed
            if (completedAssessmentWeight > 0)
            {
                performance = (earnedContribution / completedAssessmentWeight) * 100;
            }
            //Summary row now dynamically adjusts itself on the excel sheet
            int summaryRow = 6 + module.Assessments.Count;

            //Adding the summary content
            worksheet.Cell(summaryRow, 1).Value = "Earned Semester Contribution";
            worksheet.Cell(summaryRow, 2).Value = earnedContribution;
            worksheet.Cell(summaryRow + 1, 1).Value = "Performance Across Completed Assessments";
            worksheet.Cell(summaryRow + 1, 2).Value = performance;
        }
        //This method is the most important method
        //It is responsible for actually exporting/saving the data to the excel workbook
        public void ExportModule(CModules module)
        {
            //I am now attempting to create the directory that will store the Semester Workbook
            //We first get the year of the workbook we want (2026-2028)
            string dir = CreateYearDirectory(module.AcademicYear);

            //We then get the semester the student wants to make the workbook for
            string filePath = CreateSemesterWorkbookPath(dir, module.Semester);

            //CHECKS IF THE FILE ALREADY EXISTS
            bool fileExists = File.Exists(filePath);

            //Now we create/open the Excel Workbook
            using (XLWorkbook excel = OpenOrCreateWorkbook(filePath))
            {
                //We then create/get the specific worksheet we want to work in
                IXLWorksheet excelWorksheet = GetOrCreateWorksheet(excel, module.ModuleCode);

                //clears the existing worksheet before rebuilding it, to ensure old data is removed in Excel
                excelWorksheet.Clear();

                //Rebuilding the worksheet
                //Now that the old data is removed we will create the worksheet using the methods we created above for the Workbook
                //In this exact order below
                //writes the module information.
                SetupWorksheetHeader(excelWorksheet, module);
                //writes the column headings.
                SetupAssessmentTable(excelWorksheet);
                //inserts the assessment records.
                PopulateAssessmentRows(excelWorksheet, module);
                //calculates and inserts the summary.
                SetupSemesterSummary(excelWorksheet, module);

                //Save the data to the Excel File
                if (fileExists)
                {
                    // Save existing workbook
                    excel.Save();
                }
                else
                {
                    // Save new workbook using filePath
                    excel.SaveAs(filePath);
                }
            }
        }
    }
}

