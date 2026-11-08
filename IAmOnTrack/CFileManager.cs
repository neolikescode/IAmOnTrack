using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Math;
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
            //Check whether the path is empty or contains only whitespace.
            if (string.IsNullOrWhiteSpace(folderName))
            {
                throw new ArgumentException("Base directory cannot be empty.", nameof(folderName));
            }
            //Check whether the path is an absolute path.
            if (!Path.IsPathRooted(folderName))
            {
                throw new ArgumentException("Base directory must be an absolute path.", nameof(folderName));
            }
            //creates the directory if all checks are met and it doesn't already exist.
            BaseDirectory = FolderName;
            Directory.CreateDirectory(BaseDirectory);
        }
        //This method creates the Directory
        public string CreateYearDirectory(int year)
        {
            //Ensures the academic year entered by the user is either 2026 or 2027 
            if(year < 2026 || year > 2027)
            {
                throw new ArgumentOutOfRangeException(nameof(year), year, "Academic year must be either 2026 or 2027");
            }
            //creates a new directory combining the base directory and the academic year entered and returns it
            string directoryPath = Path.Combine(_baseDirectory, year.ToString());
            Directory.CreateDirectory(directoryPath);
            return directoryPath;
        }
        //This method creates the Excel Filepath
        private string CreateSemesterWorkbookPath(string yearDirectoryPath, int semester)
        {
            //Checks to see if the folder path is not blank
            if (string.IsNullOrWhiteSpace(yearDirectoryPath))
            {
                throw new ArgumentException("Year directory path cannot be blank", nameof(yearDirectoryPath));
            }
            //Checks to see if the semester entered is either 1 or 2
            if(semester < 1 || semester > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(semester), semester, "Semester must be either Semester 1 or 2");
            }
            //If all checks are met, the excel file path is created
            string excelFile = $"Semester {semester}.xlsx";
            string excelFilePath = Path.Combine(yearDirectoryPath, excelFile);
            return excelFilePath;
        }

        /*
                            ///EXCEL PORTION///
        */

        //This method creates or opens the Excel workbook we will be saving data to
        //the 2nd parameter checks if the file exists and passes this info to the
        //Export method which then decides how the workbook will be saved 
        private XLWorkbook OpenOrCreateWorkbook(string filePath, out bool fileExists)
        {
            //Checks to see if the Excel File path is not blank
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be blank", nameof(filePath));
            }
            //Checks if the Excel workbook exists, if so it opens it, if not it ctreates the workbook 
            if (File.Exists(filePath))
            {
                fileExists = true;
                return new XLWorkbook(filePath);
            }
            else
            {
                fileExists = false;
                return new XLWorkbook();
            }
        }
        //this method creates or opens the Excel Worksheet
        private IXLWorksheet GetOrCreateWorksheet(XLWorkbook workbook, string moduleCode)
        {
            //Checks whether the workbook object is null
            if (workbook == null)
            {
                throw new ArgumentNullException(nameof(workbook), "Workbook cannot be null.");
            }
            //Checks to see if the module code is not null
            if (string.IsNullOrWhiteSpace(moduleCode))
            {
                throw new ArgumentException("Module Code cannot be null", nameof(moduleCode));
            }
            //Checks whether the module code exceeds excels maximum worksheet name length of 31 characters
            if (moduleCode.Length > 31)
            {
                throw new ArgumentException("Module code cannot exceed 31 characters.", nameof(moduleCode));
            }
            //This rejects any character that excel does not allow when naming worksheets
            if (moduleCode.IndexOfAny(new char[] { ':', '\\', '/', '?', '*', '[', ']' }) != -1)
            {
                throw new ArgumentException("Module code contains characters that are not allowed in Excel worksheet names.", nameof(moduleCode));
            }
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
        private void SetupWorksheetHeader(IXLWorksheet worksheet, CModules module)
        {
            //Checks whether the worksheet object is null
            if (worksheet == null)
            {
                throw new ArgumentNullException(nameof(worksheet), "Worksheet cannot be null.");
            }
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
        private void SetupAssessmentTable(IXLWorksheet worksheet)
        {
            //Checks whether the worksheet object is null
            if (worksheet == null)
            {
                throw new ArgumentNullException(nameof(worksheet), "Worksheet cannot be null.");
            }
            worksheet.Cell(5, 1).Value = "Assessment";

            worksheet.Cell(5, 2).Value = "Weight (%)";

            worksheet.Cell(5, 3).Value = "Mark Obtained";

            worksheet.Cell(5, 4).Value = "Maximum Mark";

            worksheet.Cell(5, 5).Value = "Percentage";

            worksheet.Cell(5, 6).Value = "Weighted Contribution";
        }
        //This method enters the data from the other classes, into the excel workook
        private void PopulateAssessmentRows(IXLWorksheet worksheet, CModules module)
        {
            //Checks whether the worksheet object is null
            if (worksheet == null)
            {
                throw new ArgumentNullException(nameof(worksheet), "Worksheet cannot be null.");
            }
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
        private void SetupSemesterSummary(IXLWorksheet worksheet, CModules module)
        {
            //Checks whether the worksheet object is null
            if (worksheet == null)
            {
                throw new ArgumentNullException(nameof(worksheet), "Worksheet cannot be null.");
            }
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
            //Checks to see if the module is null
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "Module cannot be null");
            }

            //Create the directory for the academic year
            string dir = CreateYearDirectory(module.AcademicYear);

            //Get the semester workbook path
            string filePath = CreateSemesterWorkbookPath(dir, module.Semester);

            //Create a unique temporary file path
            string temporaryFilePath = Path.Combine(
                dir,
                $"Semester {module.Semester}_{Guid.NewGuid():N}.tmp.xlsx"
            );

            bool fileExists;

            try
            {
                //Open or create the Excel workbook
                using (XLWorkbook excel = OpenOrCreateWorkbook(filePath, out fileExists))
                {
                    //Get or create the module worksheet
                    IXLWorksheet excelWorksheet = GetOrCreateWorksheet(excel, module.ModuleCode);

                    //Clear the existing worksheet before rebuilding it
                    excelWorksheet.Clear();

                    //Rebuild the worksheet
                    SetupWorksheetHeader(excelWorksheet, module);
                    SetupAssessmentTable(excelWorksheet);
                    PopulateAssessmentRows(excelWorksheet, module);
                    SetupSemesterSummary(excelWorksheet, module);

                    //Save the updated workbook to the temporary file
                    excel.SaveAs(temporaryFilePath);
                }

                //Replace the original workbook only after the temporary file has been saved
                if (fileExists)
                {
                    File.Replace(temporaryFilePath, filePath, null);
                }
                else
                {
                    //Move the temporary file to its final destination
                    File.Move(temporaryFilePath, filePath);
                }
            }
            finally
            {
                //Delete the temporary file if it still exists
                if (File.Exists(temporaryFilePath))
                {
                    File.Delete(temporaryFilePath);
                }
            }
        }
    }
}

