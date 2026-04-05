using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using VAWCV5Tubod.Connection;

namespace VAWCV5Tubod
{
    public partial class ViewDetails : Form
    {
        private const string Ra9262ViolationText = "R.A. 9262: Anti Violence Against Women and their Children Act";
        private const int VerticalMargin = 24;
        private const int TitleToPanelSpacing = 20;

        public ViewDetails()
        {
            InitializeComponent();
        }

        public ViewDetails(int caseId)
            : this()
        {
            if (!LoadCaseDetails(caseId))
            {
                Dispose();
            }
        }

        public bool LoadCaseDetails(int caseId)
        {
            try
            {
                DataRow? caseDetails = GetCaseDetails(caseId);

                if (caseDetails is null)
                {
                    MessageBox.Show(
                        "No case details were found for the selected record.",
                        "Case Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return false;
                }

                PopulateCaseDetails(caseDetails);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load the case details.{Environment.NewLine}{ex.Message}",
                    "Load Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private static DataRow? GetCaseDetails(int caseId)
        {
            const string query = """
                SELECT
                    c.caseId,
                    c.complaintDate,
                    c.complainantFullname,
                    c.respondentFullname,
                    c.violation,
                    c.subViolation,
                    c.subViolation2,
                    c.subViolation3,
                    c.subViolation4,
                    c.placeofincident,
                    c.incidentdescription,
                    comp.comp_lastname,
                    comp.comp_firstname,
                    comp.comp_middlename,
                    comp.comp_sex,
                    comp.comp_age,
                    comp.comp_purok,
                    comp.comp_barangay,
                    comp.comp_municipal,
                    comp.comp_province,
                    comp.comp_contactno,
                    comp.comp_civilstatus,
                    comp.comp_eduattain,
                    comp.comp_religion,
                    comp.comp_occupation,
                    comp.comp_nationality,
                    resp.resp_lastname,
                    resp.resp_firstname,
                    resp.resp_middlename,
                    resp.resp_sex,
                    resp.resp_age,
                    resp.resp_purok,
                    resp.resp_barangay,
                    resp.resp_municipal,
                    resp.resp_province,
                    resp.resp_contactno,
                    resp.resp_civilstatus,
                    resp.resp_eduattain,
                    resp.resp_religion,
                    resp.resp_occupation,
                    resp.resp_nationality,
                    resp.resp_relationshiptocomplainant
                FROM caselist c
                LEFT JOIN complainant comp ON c.compId = comp.compId
                LEFT JOIN respondent resp ON c.respId = resp.respId
                WHERE c.caseId = @caseId
                LIMIT 1;
                """;

            using MySqlConnection connection = DbConnectionFactory.CreateConnection();
            using MySqlCommand command = new(query, connection);
            using MySqlDataAdapter adapter = new(command);

            command.Parameters.AddWithValue("@caseId", caseId);

            DataTable dataTable = new();
            adapter.Fill(dataTable);

            return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
        }

        private void PopulateCaseDetails(DataRow row)
        {
            CaseId.Text = GetDisplayText(GetDatabaseString(row, "caseId"));
            complaintDate.Text = GetDateDisplayText(row, "complaintDate");
            case_ra_violation.Text = GetDisplayText(GetDatabaseString(row, "violation"));
            comp_subViolation.Text = BuildSubViolationText(row);
            case_incident_place.Text = GetDisplayText(GetDatabaseString(row, "placeofincident"));
            case_incident_description.Text = GetDisplayText(GetDatabaseString(row, "incidentdescription"));

            comp_fullname.Text = GetDisplayText(
                BuildFullName(
                    row,
                    "complainantFullname",
                    "comp_lastname",
                    "comp_firstname",
                    "comp_middlename"));
            comp_sex.Text = GetDisplayText(GetDatabaseString(row, "comp_sex"));
            comp_age.Text = GetDisplayText(GetDatabaseString(row, "comp_age"));
            comp_address.Text = GetDisplayText(
                BuildAddress(row, "comp_purok", "comp_barangay", "comp_municipal", "comp_province"));
            comp_contactnumber.Text = GetDisplayText(GetDatabaseString(row, "comp_contactno"));
            comp_civilstatus.Text = GetDisplayText(GetDatabaseString(row, "comp_civilstatus"));
            comp_occupation.Text = GetDisplayText(GetDatabaseString(row, "comp_occupation"));
            comp_nationality.Text = GetDisplayText(GetDatabaseString(row, "comp_nationality"));
            comp_religion.Text = GetDisplayText(GetDatabaseString(row, "comp_religion"));
            comp_educationalattainment.Text = GetDisplayText(GetDatabaseString(row, "comp_eduattain"));

            resp_fullname.Text = GetDisplayText(
                BuildFullName(
                    row,
                    "respondentFullname",
                    "resp_lastname",
                    "resp_firstname",
                    "resp_middlename"));
            resp_sex.Text = GetDisplayText(GetDatabaseString(row, "resp_sex"));
            resp_age.Text = GetDisplayText(GetDatabaseString(row, "resp_age"));
            resp_Address.Text = GetDisplayText(
                BuildAddress(row, "resp_purok", "resp_barangay", "resp_municipal", "resp_province"));
            resp_contactnumber.Text = GetDisplayText(GetDatabaseString(row, "resp_contactno"));
            resp_civilstatus.Text = GetDisplayText(GetDatabaseString(row, "resp_civilstatus"));
            resp_occupation.Text = GetDisplayText(GetDatabaseString(row, "resp_occupation"));
            resp_nationality.Text = GetDisplayText(GetDatabaseString(row, "resp_nationality"));
            resp_religion.Text = GetDisplayText(GetDatabaseString(row, "resp_religion"));
            resp_educationalattainment.Text = GetDisplayText(GetDatabaseString(row, "resp_eduattain"));
            resp_relationship_to_complainant.Text = GetDisplayText(GetDatabaseString(row, "resp_relationshiptocomplainant"));
        }

        private static string BuildSubViolationText(DataRow row)
        {
            List<string> subViolations =
            [
                GetDatabaseString(row, "subViolation"),
                GetDatabaseString(row, "subViolation2"),
                GetDatabaseString(row, "subViolation3"),
                GetDatabaseString(row, "subViolation4")
            ];

            subViolations = subViolations
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (subViolations.Count == 0)
            {
                return "-";
            }

            string violation = GetDatabaseString(row, "violation");

            if (string.Equals(violation, Ra9262ViolationText, StringComparison.OrdinalIgnoreCase))
            {
                return string.Join(", ", subViolations);
            }

            return subViolations[0];
        }

        private static string BuildFullName(DataRow row, string fallbackColumn, string lastNameColumn, string firstNameColumn, string middleNameColumn)
        {
            string fullName = string.Join(
                " ",
                new[]
                {
                    GetDatabaseString(row, lastNameColumn),
                    GetDatabaseString(row, firstNameColumn),
                    GetDatabaseString(row, middleNameColumn)
                }.Where(value => !string.IsNullOrWhiteSpace(value)));

            return !string.IsNullOrWhiteSpace(fullName)
                ? fullName
                : GetDatabaseString(row, fallbackColumn);
        }

        private static string BuildAddress(DataRow row, params string[] columnNames)
        {
            return string.Join(
                ", ",
                columnNames
                    .Select(columnName => GetDatabaseString(row, columnName))
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
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
                : GetDisplayText(Convert.ToString(row[columnName]));
        }

        private static string GetDatabaseString(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(row[columnName])?.Trim() ?? string.Empty;
        }

        private static string GetDisplayText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            CenterContentLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterContentLayout();
        }

        private void CenterContentLayout()
        {
            if (label1 is null || panel1 is null)
            {
                return;
            }

            int totalContentHeight = label1.Height + TitleToPanelSpacing + panel1.Height;
            int availableHeight = Math.Max(ClientSize.Height - (VerticalMargin * 2), 0);
            int top = VerticalMargin;

            if (availableHeight > totalContentHeight)
            {
                top += (availableHeight - totalContentHeight) / 2;
            }

            label1.Left = Math.Max((ClientSize.Width - label1.Width) / 2, 0);
            label1.Top = top;

            panel1.Left = Math.Max((ClientSize.Width - panel1.Width) / 2, 0);
            panel1.Top = label1.Bottom + TitleToPanelSpacing;
        }
    }
}
