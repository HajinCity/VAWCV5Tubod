using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using VAWCV5Tubod.Connection;

namespace VAWCV5Tubod
{
    public partial class CaseList : Form
    {
        private DataTable caseData = new();
        private bool caseDataLoaded;
        private readonly string currentUserPosition;
        private readonly string currentUsername;

        public CaseList()
            : this(string.Empty, string.Empty)
        {
        }

        public CaseList(string currentUserPosition, string currentUsername = "")
        {
            InitializeComponent();
            this.currentUserPosition = currentUserPosition;
            this.currentUsername = currentUsername;
            textBox1.TextChanged += textBox1_TextChanged;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (caseDataLoaded)
            {
                return;
            }

            ConfigureCaseGrid();
            LoadCaseData();
            caseDataLoaded = true;
        }

        private void ConfigureCaseGrid()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.CellClick -= dataGridView1_CellClick;
            dataGridView1.CellClick += dataGridView1_CellClick;

            btnEdit.Width = 50;
            btnView.Width = 50;
        }

        private void LoadCaseData()
        {
            try
            {
                const string query = """
                    SELECT DISTINCT
                        caseId,
                        complaintDate,
                        complainantFullname,
                        respondentFullname,
                        violation
                    FROM caselist
                    ORDER BY caseId DESC;
                    """;

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                using MySqlCommand command = new(query, connection);
                using MySqlDataAdapter adapter = new(command);

                caseData = new DataTable();
                adapter.Fill(caseData);

                FilterData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading case data: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void PopulateCaseGrid(DataTable source)
        {
            dataGridView1.Rows.Clear();

            foreach (DataRow row in source.Rows)
            {
                int rowIndex = dataGridView1.Rows.Add();

                dataGridView1.Rows[rowIndex].Cells["Column1"].Value = row["caseId"];
                dataGridView1.Rows[rowIndex].Cells["Column2"].Value = row["complaintDate"];
                dataGridView1.Rows[rowIndex].Cells["Column3"].Value = row["complainantFullname"];
                dataGridView1.Rows[rowIndex].Cells["Column4"].Value = row["respondentFullname"];
                dataGridView1.Rows[rowIndex].Cells["Column5"].Value = row["violation"];
            }
        }

        private void FilterData()
        {
            if (caseData == null || caseData.Rows.Count == 0)
            {
                dataGridView1.Rows.Clear();
                return;
            }

            string searchText = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                PopulateCaseGrid(caseData);
                return;
            }

            DataRow[] filteredRows = caseData.AsEnumerable()
                .Where(row =>
                    (Convert.ToString(row["caseId"])?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (Convert.ToString(row["complainantFullname"])?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (Convert.ToString(row["respondentFullname"])?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (Convert.ToString(row["violation"])?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToArray();

            DataTable filteredData = caseData.Clone();

            foreach (DataRow row in filteredRows)
            {
                filteredData.ImportRow(row);
            }

            PopulateCaseGrid(filteredData);
        }

        private void textBox1_TextChanged(object? sender, EventArgs e)
        {
            FilterData();
        }

        private void dataGridView1_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;

            if (!TryGetSelectedCaseId(e.RowIndex, out int caseId))
            {
                return;
            }

            if (string.Equals(columnName, "btnView", StringComparison.Ordinal))
            {
                OpenViewDetails(caseId);
                return;
            }

            if (string.Equals(columnName, "btnEdit", StringComparison.Ordinal))
            {
                OpenManageCase(caseId);
            }
        }

        private bool TryGetSelectedCaseId(int rowIndex, out int caseId)
        {
            string caseIdText = Convert.ToString(dataGridView1.Rows[rowIndex].Cells["Column1"].Value)?.Trim() ?? string.Empty;

            if (int.TryParse(caseIdText, out caseId))
            {
                return true;
            }

            MessageBox.Show(
                "Unable to use the selected case because the Case ID is invalid.",
                "Invalid Case ID",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        private void OpenViewDetails(int caseId)
        {
            using ViewDetails viewDetails = new();

            if (!viewDetails.LoadCaseDetails(caseId))
            {
                return;
            }

            viewDetails.ShowDialog(this);
        }

        private void OpenManageCase(int caseId)
        {
            if (!string.Equals(currentUserPosition, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                UserLogService.Log(
                    currentUsername,
                    "UnauthorizedAccess",
                    "ManageCase",
                    caseId,
                    "User attempted to open ManageCase form.");

                MessageBox.Show(
                    "Only users with the Admin role can edit case records.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using ManageCase manageCase = new(currentUsername);

            if (!manageCase.LoadCaseForEdit(caseId))
            {
                return;
            }

            if (manageCase.ShowDialog(this) == DialogResult.OK)
            {
                LoadCaseData();
            }
        }
    }
}
