using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace VAWCV5Tubod
{
    public partial class IntakeForm : UserControl
    {
        public IntakeForm()
        {
            InitializeComponent();
        }

        public void PopulateCaseData(DataRow caseRow, DataRow? complainantRow, DataRow? respondentRow)
        {
            CaseTextBox.Text = GetDisplayText(GetDatabaseString(caseRow, "caseId"));
            ComplaintDate.Text = GetDateDisplayText(caseRow, "complaintDate");

            ComplainantFullName.Text = GetDisplayText(
                BuildFullName(
                    complainantRow,
                    GetDatabaseString(caseRow, "complainantFullname"),
                    "comp_lastname",
                    "comp_firstname",
                    "comp_middlename"));
            ComplainantContactNo.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_contactno"));
            ComplainantSex.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_sex"));
            ComplainantAge.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_age"));
            ComplainantAddress.Text = GetDisplayText(
                BuildAddress(complainantRow, "comp_purok", "comp_barangay", "comp_municipal", "comp_province"));
            ComplainantCivilStatus.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_civilstatus"));
            ComplainantReligion.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_religion"));
            ComplainantNationality.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_nationality"));
            ComplainantOccupation.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_occupation"));

            RespondentFullName.Text = GetDisplayText(
                BuildFullName(
                    respondentRow,
                    GetDatabaseString(caseRow, "respondentFullname"),
                    "resp_lastname",
                    "resp_firstname",
                    "resp_middlename"));
            RespondentContactNo.Text = GetDisplayText(GetDatabaseString(respondentRow, "resp_contactno"));
            RespondentSex.Text = GetDisplayText(GetDatabaseString(respondentRow, "resp_sex"));
            RespondentAge.Text = GetDisplayText(GetDatabaseString(respondentRow, "resp_age"));
            RespondentAddress.Text = GetDisplayText(
                BuildAddress(respondentRow, "resp_purok", "resp_barangay", "resp_municipal", "resp_province"));
            RespondentCivilStatus.Text = GetDisplayText(GetDatabaseString(respondentRow, "resp_civilstatus"));
            RespondentReligion.Text = GetDisplayText(GetDatabaseString(respondentRow, "resp_religion"));
            RespondentNationality.Text = GetDisplayText(GetDatabaseString(respondentRow, "resp_nationality"));
            RespondentOccupation.Text = GetDisplayText(GetDatabaseString(respondentRow, "resp_occupation"));
            RespondentRelationshipstoVictim.Text = GetDisplayText(GetDatabaseString(respondentRow, "resp_relationshiptocomplainant"));

            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
        }

        private static string GetDatabaseString(DataRow? row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(row[columnName])?.Trim() ?? string.Empty;
        }

        private static string GetDisplayText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private static string GetDateDisplayText(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return "-";
            }

            if (row[columnName] is DateTime dateValue)
            {
                return dateValue.ToString("MMMM d, yyyy");
            }

            return DateTime.TryParse(Convert.ToString(row[columnName]), out DateTime parsedDate)
                ? parsedDate.ToString("MMMM d, yyyy")
                : "-";
        }

        private static string BuildFullName(DataRow? row, string fallbackValue, string lastNameColumn, string firstNameColumn, string middleNameColumn)
        {
            if (row == null)
            {
                return fallbackValue;
            }

            string fullName = string.Join(
                " ",
                new[]
                {
                    GetDatabaseString(row, lastNameColumn),
                    GetDatabaseString(row, firstNameColumn),
                    GetDatabaseString(row, middleNameColumn)
                }.Where(value => !string.IsNullOrWhiteSpace(value)));

            return string.IsNullOrWhiteSpace(fullName) ? fallbackValue : fullName;
        }

        private static string BuildAddress(DataRow? row, params string[] columns)
        {
            if (row == null)
            {
                return string.Empty;
            }

            return string.Join(
                ", ",
                columns
                    .Select(column => GetDatabaseString(row, column))
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
        }
    }
}
