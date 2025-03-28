namespace PESOapps.Shared.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;

public class ExcelService
{
    public async Task<byte[]> GenerateExcelAsync(List<TupadBeneficiary> beneficiaries)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("TUPAD Beneficiaries");

            worksheet.Rows().Style.Alignment.WrapText = true;
            worksheet.Columns().AdjustToContents();

            worksheet.Style.Font.FontName = "Arial";
            worksheet.Style.Font.FontSize = 10;
            worksheet.Row(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Column("A").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column("J").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column("Q").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column("R").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column("S").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column("V").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            // Set Column Headers
            worksheet.Cell("B1").Value = "Name of Project:";
            worksheet.Cell("C1").Value = "TUPAD";
            worksheet.Cell("B2").Value = "DOLE Regional Office:";
            worksheet.Cell("C2").Value = "10";
            worksheet.Cell("B3").Value = "Province:";
            worksheet.Cell("C3").Value = "MISAMIS ORIENTAL";
            worksheet.Cell("B4").Value = "Municipality:";
            worksheet.Cell("B5").Value = "Barangay:";
            worksheet.Range("T1:U1").Merge();
            worksheet.Cell("T1").Value = "OSEC-FMS Form No. 4";
            worksheet.Range("A6:U6").Merge(); // Merge the range
            worksheet.Cell("A6").Value = "Profile of TUPAD Beneficiaries";
            worksheet.Cell("A6").Style.Font.Bold = true;
            worksheet.Cell("A6").Style.Font.FontSize = 14; // Set font size
            worksheet.Cell("A6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Center align
            worksheet.Cell("A6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; // Vertical align
            worksheet.Range("A7:A8").Merge();
            worksheet.Cell("A7").Value = "No.";
            worksheet.Cell("A7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("A7:A8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("B7:E7").Merge();
            worksheet.Cell("B7").Value = "Name of Beneficiary";
            worksheet.Cell("B7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("B7:E7").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("F7:F8").Merge();
            worksheet.Cell("F7").Value = "Birthdate¹ \r\n(YYYY/MM/DD)";
            worksheet.Cell("F7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("F7:F8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("G7:J7").Merge();
            worksheet.Cell("G7").Value = "Project Location";
            worksheet.Cell("G7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("G7:J7").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("K7:K8").Merge();
            worksheet.Cell("K7").Value = "Type of ID \r\n(e.g. SSS, Voter's ID)";
            worksheet.Cell("K7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("K7:K8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("L7:L8").Merge();
            worksheet.Cell("L7").Value = "ID Number";
            worksheet.Cell("L7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("L7:L8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("M7:M8").Merge();
            worksheet.Cell("M7").Value = "Contact No.";
            worksheet.Cell("M7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("M7:M8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("N7:N8").Merge();
            worksheet.Cell("N7").Value = "E-payment/Bank \r\nAccount No. \r\n(indicate the type of \r\naccount and no. as \r\napplicable)";
            worksheet.Cell("N7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Cell("N7").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("O7:O8").Merge();
            worksheet.Cell("O7").Value = "Type of Beneficiary³";
            worksheet.Cell("O7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("O7:O8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("P7:P8").Merge();
            worksheet.Cell("P7").Value = "Occupation⁴";
            worksheet.Cell("P7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("P7:P8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("Q7:Q8").Merge();
            worksheet.Cell("Q7").Value = "Sex⁵";
            worksheet.Cell("Q7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("Q7:Q8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("R7:R8").Merge();
            worksheet.Cell("R7").Value = "Civil \r\nStatus⁶";
            worksheet.Cell("R7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("R7:R8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("S7:S8").Merge();
            worksheet.Cell("S7").Value = "Age";
            worksheet.Cell("S7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("S7:S8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("T7:T8").Merge();
            worksheet.Cell("T7").Value = "Average \r\nmonthly income";
            worksheet.Cell("T7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("T7:T8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("U7:U8").Merge();
            worksheet.Cell("U7").Value = "Dependent⁷ \r\n(Name of Beneficiary\r\n of the Micro-insurance \r\nHolder)";
            worksheet.Cell("U7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("U7:U8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("V7:V8").Merge();
            worksheet.Cell("V7").Value = "Interested in \r\nwage employment \r\nor self- employment? \r\n(Yes/No?)";
            worksheet.Cell("V7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("V7:V8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("W7:W8").Merge();
            worksheet.Cell("W7").Value = "Skills \r\ntraining needed";
            worksheet.Cell("W7").Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
            worksheet.Range("W7:W8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


            // Table Headers
            string[] headers = {
                "", "First Name", "Middle Name", "Last Name", "Extension Name", "",
                "Barangay", "City/Municipality", "Province", "District", "", "ID Number",
                "Contact No.", "E-payment/Bank Account No.", "Type of Beneficiary", "Occupation", "Sex",
                "Civil Status", "Age", "Average Monthly Income", "Dependent (Name)",
                "Interested in Wage Employment (Yes/No)", "Skills Training Needed"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(8, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = false;
                cell.Style.Font.FontSize = 10;
                cell.Style.Font.FontName = "Arial";
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD6EE");
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center); // Center align
                cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center); // Vertically center
            }

            // Insert Data
            int row = 9;
            int number = 1;
            foreach (var beneficiary in beneficiaries)
            {
                worksheet.Cell(row, 1).Value = number++;
                worksheet.Cell(row, 2).Value = beneficiary.Firstname;
                worksheet.Cell(row, 3).Value = beneficiary.MiddleName;
                worksheet.Cell(row, 4).Value = beneficiary.Lastname;
                worksheet.Cell(row, 5).Value = beneficiary.ExtensionName;
                worksheet.Cell(row, 6).Value = beneficiary.Birthday;
                worksheet.Cell(row, 6).Value = beneficiary.FormattedBirthday;
                worksheet.Cell(row, 7).Value = beneficiary.Barangay;
                worksheet.Cell(row, 8).Value = beneficiary.Municipality;
                worksheet.Cell(row, 9).Value = beneficiary.Province;
                worksheet.Cell(row, 10).Value = beneficiary.District;
                worksheet.Cell(row, 11).Value = beneficiary.IDType;
                worksheet.Cell(row, 12).Value = beneficiary.IDNumber;
                worksheet.Cell(row, 13).Value = beneficiary.ContactNo;
                worksheet.Cell(row, 14).Value = beneficiary.Epayment;
                worksheet.Cell(row, 15).Value = beneficiary.TypeOfBenef;
                worksheet.Cell(row, 16).Value = beneficiary.Occupation;
                worksheet.Cell(row, 17).Value = beneficiary.Sex;
                worksheet.Cell(row, 18).Value = beneficiary.CivilStatus;
                worksheet.Cell(row, 19).Value = beneficiary.Age;
                worksheet.Cell(row, 20).Value = beneficiary.AverageIncome;
                worksheet.Cell(row, 21).Value = beneficiary.Dependent;
                worksheet.Cell(row, 22).Value = beneficiary.InterestWage;
                worksheet.Cell(row, 23).Value = beneficiary.SkillsTraining;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return await Task.FromResult(stream.ToArray());
            }
        }
    }
}

public class TupadBeneficiary
{
    public int ID { get; set; }
    public string? Firstname { get; set; }
    public string? MiddleName { get; set; }
    public string? Lastname { get; set; }
    public string? ExtensionName { get; set; }
    public string? Birthday { get; set; }
    public string FormattedBirthday =>
    DateTime.TryParse(Birthday, out DateTime date) ? date.ToString("yyyy/MM/dd") : Birthday ?? "";
    public string? Barangay { get; set; }
    public string? Municipality { get; set; }
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? IDType { get; set; }
    public string? IDNumber { get; set; }
    public string? ContactNo { get; set; }
    public string? Epayment { get; set; }
    public string? TypeOfBenef { get; set; }
    public string? Occupation { get; set; }
    public string? Sex { get; set; }
    public string? CivilStatus { get; set; }
    public string? Age { get; set; }
    public string? AverageIncome { get; set; }
    public string? Dependent { get; set; }
    public string? InterestWage { get; set; }
    public string? SkillsTraining { get; set; }
}
