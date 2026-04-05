using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace VAWCV5Tubod
{
    public partial class ReferralForm : UserControl
    {
        public ReferralForm()
        {
            InitializeComponent();
        }

        public void PopulateCaseData(DataRow caseRow, DataRow? complainantRow, DataRow? respondentRow)
        {
            textBox1.Text = GetDisplayText(GetDatabaseString(caseRow, "caseId"));
            textBox2.Text = GetDateDisplayText(caseRow, "complaintDate");
            textBox12.Text = GetDisplayText(GetDatabaseString(caseRow, "referredto"));
            textBox13.Clear();

            Complainant_Name.Text = GetDisplayText(
                BuildFullName(
                    complainantRow,
                    GetDatabaseString(caseRow, "complainantFullname"),
                    "comp_lastname",
                    "comp_firstname",
                    "comp_middlename"));
            comp_age.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_age"));
            comp_sex.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_sex"));
            comp_address.Text = GetDisplayText(
                BuildAddress(complainantRow, "comp_purok", "comp_barangay", "comp_municipal", "comp_province"));
            comp_civilstatus.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_civilstatus"));
            comp_contactno.Text = GetDisplayText(GetDatabaseString(complainantRow, "comp_contactno"));

            textBox3.Text = BuildReferralReason(caseRow);
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            textBox7.Clear();
            textBox8.Clear();
            textBox9.Clear();
            textBox10.Clear();
            textBox11.Clear();
        }

        private static string BuildReferralReason(DataRow row)
        {
            string violation = GetDatabaseString(row, "violation");
            string incidentDescription = GetDatabaseString(row, "incidentdescription");

            if (!string.IsNullOrWhiteSpace(violation) && !string.IsNullOrWhiteSpace(incidentDescription))
            {
                return $"{violation} - {incidentDescription}";
            }

            if (!string.IsNullOrWhiteSpace(incidentDescription))
            {
                return incidentDescription;
            }

            return violation;
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
