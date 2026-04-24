using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using VAWCV5Tubod.Connection;

namespace VAWCV5Tubod
{
    public partial class CensusData : UserControl
    {
        private static readonly Color[] DashboardColors =
        {
            Color.FromArgb(46, 134, 171),
            Color.FromArgb(162, 59, 114),
            Color.FromArgb(241, 143, 1),
            Color.FromArgb(199, 62, 29),
            Color.FromArgb(109, 89, 122),
            Color.FromArgb(72, 149, 239)
        };

        private readonly Label noRecordsLabel = new Label();
        private bool hasReportData;

        public string CurrentUserFullName { get; set; } = string.Empty;
        public string CurrentUsername { get; set; } = string.Empty;

        public CensusData()
        {
            InitializeComponent();
            ConfigureDashboard();
            InitializeDateRange();
            LoadCensusData();
        }

        private void ConfigureDashboard()
        {
            panelCharts.AutoScroll = true;
            btnRefresh.Click += btnRefresh_Click;
            btnExport.Click += btnExport_Click;
            btnPrint.Click += btnPrint_Click;

            noRecordsLabel.Dock = DockStyle.Fill;
            noRecordsLabel.TextAlign = ContentAlignment.MiddleCenter;
            noRecordsLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            noRecordsLabel.ForeColor = DashboardColors[0];
            noRecordsLabel.BackColor = Color.White;
            noRecordsLabel.Visible = false;
            panelCharts.Controls.Add(noRecordsLabel);
            noRecordsLabel.BringToFront();
        }

        private void InitializeDateRange()
        {
            dtpStartDate.Value = new DateTime(DateTime.Now.Year, 1, 1);
            dtpEndDate.Value = DateTime.Now.Date;
        }

        private void LoadCensusData()
        {
            hasReportData = GetTotalCasesForSelectedPeriod() > 0;
            LoadKpiData();

            if (!hasReportData)
            {
                ConfigureEmptyDashboard();
                ShowNoRecordsMessage("No records found for the selected period.");
                return;
            }

            HideNoRecordsMessage();
            LoadMonthlyTrends();
            LoadViolationTypes();
            LoadAgeDistribution();
            LoadGeographicDistribution();
            LoadCaseStatus();
            LoadRelationshipAnalysis();
            LoadEducationLevel();
            LoadOccupation();
            LoadCivilStatus();
        }

        private int GetTotalCasesForSelectedPeriod()
        {
            string query = @"
                SELECT COUNT(*) AS total_cases
                FROM caselist
                " + GetDateRangeFilter("WHERE ");

            DataTable table = ExecuteQuery(query);
            return table.Rows.Count == 0 ? 0 : Convert.ToInt32(table.Rows[0]["total_cases"]);
        }

        private void LoadKpiData()
        {
            string query = @"
                SELECT COUNT(*) AS total_cases,
                       SUM(CASE WHEN casestatus = 'Active' OR casestatus = 'Pending' THEN 1 ELSE 0 END) AS active_cases,
                       SUM(CASE WHEN casestatus = 'Settled' THEN 1 ELSE 0 END) AS resolved_cases
                FROM caselist
                " + GetDateRangeFilter("WHERE ");

            DataTable table = ExecuteQuery(query);
            if (table.Rows.Count == 0)
            {
                lblTotalCasesValue.Text = "0";
                lblActiveCasesValue.Text = "0";
                lblResolvedCasesValue.Text = "0";
                return;
            }

            DataRow row = table.Rows[0];
            lblTotalCasesValue.Text = Convert.ToInt32(row["total_cases"]).ToString("N0");
            lblActiveCasesValue.Text = Convert.ToInt32(row["active_cases"] == DBNull.Value ? 0 : row["active_cases"]).ToString("N0");
            lblResolvedCasesValue.Text = Convert.ToInt32(row["resolved_cases"] == DBNull.Value ? 0 : row["resolved_cases"]).ToString("N0");
        }

        private void LoadMonthlyTrends()
        {
            ConfigureCartesianChart(chartMonthlyTrends, "MONTHLY TRENDS CHART", "Month", "Number of Cases");

            Series series = CreateSeries("Cases Reported", SeriesChartType.Line, DashboardColors[0]);
            series.BorderWidth = 3;
            series.MarkerStyle = MarkerStyle.Circle;
            series.MarkerSize = 8;

            string query = @"
                SELECT MONTH(complaintDate) AS month_num,
                       MONTHNAME(complaintDate) AS month_name,
                       COUNT(*) AS case_count
                FROM caselist
                " + GetDateRangeFilter("WHERE ") + @"
                GROUP BY MONTH(complaintDate), MONTHNAME(complaintDate)
                ORDER BY month_num";

            DataTable table = ExecuteQuery(query);
            foreach (DataRow row in table.Rows)
            {
                series.Points.AddXY(Convert.ToString(row["month_name"]), Convert.ToInt32(row["case_count"]));
            }

            chartMonthlyTrends.Series.Add(series);
        }

        private void LoadViolationTypes()
        {
            ConfigureCircularChart(chartViolationTypes, "VIOLATION TYPES", SeriesChartType.Pie);
            PopulateCircularChart(
                chartViolationTypes,
                "Violation Types",
                @"
                SELECT CASE
                           WHEN violation LIKE '%9262%' THEN 'R.A. 9262'
                           WHEN violation LIKE '%8353%' THEN 'R.A. 8353'
                           WHEN violation LIKE '%7877%' THEN 'R.A. 7877'
                           WHEN violation LIKE '%9208%' OR violation LIKE '%10364%' THEN 'R.A. 9208/10364'
                           ELSE 'Other'
                       END AS category,
                       COUNT(*) AS case_count
                FROM caselist
                " + GetDateRangeFilter("WHERE ") + @"
                GROUP BY category
                ORDER BY case_count DESC");
        }
        private void LoadAgeDistribution()
        {
            ConfigureCartesianChart(chartAgeDistribution, "AGE DISTRIBUTION", "Age Groups", "Number of Cases");

            Series series = CreateSeries("Age Groups", SeriesChartType.Bar, DashboardColors[0]);
            string query = @"
                SELECT CASE
                           WHEN comp.comp_age < 18 THEN 'Under 18'
                           WHEN comp.comp_age BETWEEN 18 AND 25 THEN '18-25'
                           WHEN comp.comp_age BETWEEN 26 AND 35 THEN '26-35'
                           WHEN comp.comp_age BETWEEN 36 AND 45 THEN '36-45'
                           WHEN comp.comp_age BETWEEN 46 AND 55 THEN '46-55'
                           WHEN comp.comp_age > 55 THEN 'Over 55'
                           ELSE 'Unknown'
                       END AS category,
                       COUNT(*) AS case_count
                FROM caselist c
                INNER JOIN complainant comp ON c.compId = comp.compId
                " + GetDateRangeFilter("WHERE c.") + @"
                GROUP BY category
                ORDER BY case_count DESC";

            DataTable table = ExecuteQuery(query);
            foreach (DataRow row in table.Rows)
            {
                int index = series.Points.AddXY(Convert.ToString(row["category"]), Convert.ToInt32(row["case_count"]));
                series.Points[index].Label = "#VALY";
            }

            chartAgeDistribution.Series.Add(series);
        }

        private void LoadGeographicDistribution()
        {
            ConfigureCartesianChart(chartGeographicDistribution, "GEOGRAPHIC DISTRIBUTION", "Purok / Barangay", "Number of Cases");

            Series series = CreateSeries("Geographic Distribution", SeriesChartType.Column, DashboardColors[1]);
            string query = @"
                SELECT CONCAT(comp.comp_purok, ' - ', comp.comp_barangay) AS category,
                       COUNT(*) AS case_count
                FROM caselist c
                INNER JOIN complainant comp ON c.compId = comp.compId
                " + GetDateRangeFilter("WHERE c.") + @"
                AND comp.comp_purok IS NOT NULL AND TRIM(comp.comp_purok) <> ''
                GROUP BY comp.comp_purok, comp.comp_barangay
                ORDER BY case_count DESC
                LIMIT 10";

            DataTable table = ExecuteQuery(query);
            foreach (DataRow row in table.Rows)
            {
                int index = series.Points.AddXY(Convert.ToString(row["category"]), Convert.ToInt32(row["case_count"]));
                series.Points[index].Label = "#VALY";
            }

            chartGeographicDistribution.Series.Add(series);
        }
        private void LoadCaseStatus()
        {
            ConfigureCircularChart(chartCaseStatus, "CASE STATUS", SeriesChartType.Doughnut);
            PopulateCircularChart(
                chartCaseStatus,
                "Case Status",
                @"
                SELECT CASE
                           WHEN casestatus LIKE '%Pending%' THEN 'Pending'
                           WHEN casestatus LIKE '%Dismissed%' THEN 'Dismissed'
                           WHEN casestatus LIKE '%Finished%' OR casestatus LIKE '%Completed%' THEN 'Finished / Completed'
                           WHEN casestatus LIKE '%Stop%' THEN 'Stop'
                           ELSE casestatus
                       END AS category,
                       COUNT(*) AS case_count
                FROM caselist
                " + GetDateRangeFilter("WHERE ") + @"
                AND casestatus IS NOT NULL AND TRIM(casestatus) <> ''
                GROUP BY category
                ORDER BY case_count DESC");
        }
        private void LoadRelationshipAnalysis()
        {
            ConfigureCartesianChart(chartRelationshipAnalysis, "RELATIONSHIP ANALYSIS", "Relationship Type", "Number of Cases");

            Series maleSeries = CreateSeries("Male Respondents", SeriesChartType.StackedBar, DashboardColors[0]);
            Series femaleSeries = CreateSeries("Female Respondents", SeriesChartType.StackedBar, DashboardColors[1]);

            string query = @"
                SELECT resp.resp_relationshiptocomplainant AS relationship_type,
                       resp.resp_sex AS respondent_gender,
                       COUNT(*) AS case_count
                FROM caselist c
                INNER JOIN respondent resp ON c.respId = resp.respId
                " + GetDateRangeFilter("WHERE c.") + @"
                AND resp.resp_relationshiptocomplainant IS NOT NULL
                AND TRIM(resp.resp_relationshiptocomplainant) <> ''
                GROUP BY resp.resp_relationshiptocomplainant, resp.resp_sex
                ORDER BY relationship_type";

            DataTable table = ExecuteQuery(query);
            DataTable maleData = table.Clone();
            DataTable femaleData = table.Clone();

            foreach (DataRow row in table.Rows)
            {
                string gender = Convert.ToString(row["respondent_gender"]) ?? string.Empty;
                if (gender.StartsWith("M", StringComparison.OrdinalIgnoreCase))
                {
                    maleData.ImportRow(row);
                }
                else
                {
                    femaleData.ImportRow(row);
                }
            }

            foreach (DataRow row in maleData.Rows)
            {
                maleSeries.Points.AddXY(Convert.ToString(row["relationship_type"]), Convert.ToInt32(row["case_count"]));
            }

            foreach (DataRow row in femaleData.Rows)
            {
                femaleSeries.Points.AddXY(Convert.ToString(row["relationship_type"]), Convert.ToInt32(row["case_count"]));
            }

            chartRelationshipAnalysis.Series.Add(maleSeries);
            chartRelationshipAnalysis.Series.Add(femaleSeries);
        }
        private void LoadEducationLevel()
        {
            ConfigureCartesianChart(chartEducationLevel, "EDUCATION LEVEL", "Education Level", "Number of Cases");

            Series series = CreateSeries("Education Level", SeriesChartType.Bar, DashboardColors[2]);
            string query = @"
                SELECT CASE
                           WHEN comp.comp_eduattain LIKE '%Pre-school%' OR comp.comp_eduattain LIKE '%Preschool%' THEN 'Pre-school'
                           WHEN comp.comp_eduattain LIKE '%Elementary%' OR comp.comp_eduattain LIKE '%Primary%' THEN 'Elementary'
                           WHEN comp.comp_eduattain LIKE '%Junior High%' OR comp.comp_eduattain LIKE '%JHS%' THEN 'Junior High School'
                           WHEN comp.comp_eduattain LIKE '%Senior High%' OR comp.comp_eduattain LIKE '%SHS%' THEN 'Senior High School'
                           WHEN comp.comp_eduattain LIKE '%Tertiary%' OR comp.comp_eduattain LIKE '%College%' OR comp.comp_eduattain LIKE '%University%' THEN 'Tertiary Education'
                           WHEN comp.comp_eduattain LIKE '%Technical%' OR comp.comp_eduattain LIKE '%Vocational%' OR comp.comp_eduattain LIKE '%TVET%' THEN 'Technical / Vocational'
                           WHEN comp.comp_eduattain LIKE '%Alternative%' OR comp.comp_eduattain LIKE '%ALS%' THEN 'ALS'
                           ELSE comp.comp_eduattain
                       END AS category,
                       COUNT(*) AS case_count
                FROM caselist c
                INNER JOIN complainant comp ON c.compId = comp.compId
                " + GetDateRangeFilter("WHERE c.") + @"
                AND comp.comp_eduattain IS NOT NULL AND TRIM(comp.comp_eduattain) <> ''
                GROUP BY category
                ORDER BY case_count DESC";

            DataTable table = ExecuteQuery(query);
            foreach (DataRow row in table.Rows)
            {
                int index = series.Points.AddXY(Convert.ToString(row["category"]), Convert.ToInt32(row["case_count"]));
                series.Points[index].Label = "#VALY";
            }

            chartEducationLevel.Series.Add(series);
        }

        private void LoadOccupation()
        {
            ConfigureCartesianChart(chartOccupation, "OCCUPATION", "Occupation", "Number of Cases");

            Series series = CreateSeries("Occupation", SeriesChartType.Bar, DashboardColors[3]);
            string query = @"
                SELECT comp.comp_occupation AS category,
                       COUNT(*) AS case_count
                FROM caselist c
                INNER JOIN complainant comp ON c.compId = comp.compId
                " + GetDateRangeFilter("WHERE c.") + @"
                AND comp.comp_occupation IS NOT NULL AND TRIM(comp.comp_occupation) <> ''
                GROUP BY comp.comp_occupation
                ORDER BY case_count DESC
                LIMIT 10";

            DataTable table = ExecuteQuery(query);
            foreach (DataRow row in table.Rows)
            {
                int index = series.Points.AddXY(Convert.ToString(row["category"]), Convert.ToInt32(row["case_count"]));
                series.Points[index].Label = "#VALY";
            }

            chartOccupation.Series.Add(series);
        }

        private void LoadCivilStatus()
        {
            ConfigureCircularChart(chartCivilStatus, "COMPLAINANT CIVIL STATUS DISTRIBUTION", SeriesChartType.Pie);
            PopulateCircularChart(
                chartCivilStatus,
                "Civil Status",
                @"
                SELECT comp.comp_civilstatus AS category,
                       COUNT(*) AS case_count
                FROM caselist c
                INNER JOIN complainant comp ON c.compId = comp.compId
                " + GetDateRangeFilter("WHERE c.") + @"
                AND comp.comp_civilstatus IS NOT NULL AND TRIM(comp.comp_civilstatus) <> ''
                GROUP BY comp.comp_civilstatus
                ORDER BY case_count DESC");
        }

        private DataTable ExecuteQuery(string query)
        {
            using (MySqlConnection connection = DbConnectionFactory.CreateConnection())
            using (MySqlCommand command = new MySqlCommand(query, connection))
            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private string GetDateRangeFilter(string prefix)
        {
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1);
            return string.Format(
                "{0}complaintDate >= '{1:yyyy-MM-dd HH:mm:ss}' AND complaintDate <= '{2:yyyy-MM-dd HH:mm:ss}'",
                prefix,
                startDate,
                endDate);
        }

        private static void ApplyChartChrome(Chart chart, string title)
        {
            chart.BackColor = Color.White;
            chart.BorderlineColor = Color.LightGray;
            chart.BorderlineWidth = 1;
            chart.Titles.Clear();
            chart.Titles.Add(title);
            chart.Titles[0].Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            chart.Titles[0].ForeColor = DashboardColors[0];
        }

        private void ConfigureEmptyDashboard()
        {
            ConfigureCartesianChart(chartMonthlyTrends, "MONTHLY TRENDS CHART", "Month", "Number of Cases");
            ConfigureCircularChart(chartViolationTypes, "VIOLATION TYPES", SeriesChartType.Pie);
            ConfigureCartesianChart(chartAgeDistribution, "AGE DISTRIBUTION", "Age Groups", "Number of Cases");
            ConfigureCartesianChart(chartGeographicDistribution, "GEOGRAPHIC DISTRIBUTION", "Purok / Barangay", "Number of Cases");
            ConfigureCircularChart(chartCaseStatus, "CASE STATUS", SeriesChartType.Doughnut);
            ConfigureCartesianChart(chartRelationshipAnalysis, "RELATIONSHIP ANALYSIS", "Relationship Type", "Number of Cases");
            ConfigureCartesianChart(chartEducationLevel, "EDUCATION LEVEL", "Education Level", "Number of Cases");
            ConfigureCartesianChart(chartOccupation, "OCCUPATION", "Occupation", "Number of Cases");
            ConfigureCircularChart(chartCivilStatus, "COMPLAINANT CIVIL STATUS DISTRIBUTION", SeriesChartType.Pie);
        }

        private void ShowNoRecordsMessage(string message)
        {
            foreach (Chart chart in GetDashboardCharts())
            {
                chart.Visible = false;
            }

            noRecordsLabel.Text = message;
            noRecordsLabel.Visible = true;
            noRecordsLabel.BringToFront();
        }

        private void HideNoRecordsMessage()
        {
            noRecordsLabel.Visible = false;

            foreach (Chart chart in GetDashboardCharts())
            {
                chart.Visible = true;
            }
        }

        private Chart[] GetDashboardCharts()
        {
            return
            [
                chartMonthlyTrends,
                chartViolationTypes,
                chartAgeDistribution,
                chartGeographicDistribution,
                chartCaseStatus,
                chartRelationshipAnalysis,
                chartEducationLevel,
                chartOccupation,
                chartCivilStatus
            ];
        }

        private void ConfigureCartesianChart(Chart chart, string title, string xTitle, string yTitle)
        {
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            ChartArea area = new ChartArea("MainArea");
            area.AxisX.Title = xTitle;
            area.AxisY.Title = yTitle;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.Interval = 1;
            chart.ChartAreas.Add(area);
            ApplyChartChrome(chart, title);
        }

        private void ConfigureCircularChart(Chart chart, string title, SeriesChartType chartType)
        {
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            chart.ChartAreas.Add(new ChartArea("MainArea"));
            Legend legend = new Legend("Legend1");
            legend.Docking = Docking.Right;
            legend.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            chart.Legends.Add(legend);

            Series series = CreateSeries(title, chartType, DashboardColors[0]);
            series.Legend = "Legend1";
            chart.Series.Add(series);
            ApplyChartChrome(chart, title);
        }

        private static Series CreateSeries(string name, SeriesChartType type, Color color)
        {
            Series series = new Series(name);
            series.ChartArea = "MainArea";
            series.ChartType = type;
            series.Color = color;
            return series;
        }

        private void PopulateCircularChart(Chart chart, string name, string query)
        {
            Series series = chart.Series[0];
            series.Name = name;
            series.Points.Clear();

            DataTable table = ExecuteQuery(query);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int pointIndex = series.Points.AddXY(Convert.ToString(row["category"]), Convert.ToInt32(row["case_count"]));
                DataPoint point = series.Points[pointIndex];
                point.Color = DashboardColors[i % DashboardColors.Length];
                point.Label = "#VALY";
                point.LabelForeColor = Color.White;
                point.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                point.LegendText = point.AxisLabel;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadCensusData();
            string message = hasReportData
                ? "Census data refreshed successfully."
                : "No records found for the selected period.";

            MessageBox.Show(message, "Census Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv";
                dialog.FileName = "census-report.xlsx";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();
                    if (extension == ".xlsx")
                    {
                        ExportToExcel(dialog.FileName);
                    }
                    else
                    {
                        ExportToCsv(dialog.FileName);
                    }

                    UserLogService.Log(
                        CurrentUsername,
                        "ExportCensusDashboard",
                        "census_dashboard",
                        0,
                        $"Exported census dashboard for {BuildSelectedDateRangeDescription()}.");
                }
            }
        }

        private void ExportToExcel(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet summarySheet = package.Workbook.Worksheets.Add("Summary");
                ExcelWorksheet chartsSheet = package.Workbook.Worksheets.Add("Charts");

                BuildSummarySheet(summarySheet);
                BuildChartsSheet(chartsSheet);

                summarySheet.Cells[summarySheet.Dimension.Address].AutoFitColumns();
                chartsSheet.View.ShowGridLines = false;

                package.SaveAs(new FileInfo(filePath));
            }

            string message = hasReportData
                ? "Excel report exported successfully with charts included."
                : "Excel report exported successfully. No records were found for the selected period.";

            MessageBox.Show(message, "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportToCsv(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.WriteLine("VAWC Census Dashboard");
                writer.WriteLine("Generated On,{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                writer.WriteLine("Date Range,{0} to {1}", dtpStartDate.Value.ToString("yyyy-MM-dd"), dtpEndDate.Value.ToString("yyyy-MM-dd"));
                writer.WriteLine();
                writer.WriteLine("Metric,Value");
                writer.WriteLine("Total Cases,{0}", lblTotalCasesValue.Text);
                writer.WriteLine("Active and Pending Cases,{0}", lblActiveCasesValue.Text);
                writer.WriteLine("Settled Cases,{0}", lblResolvedCasesValue.Text);

                if (!hasReportData)
                {
                    writer.WriteLine();
                    writer.WriteLine("No records found for the selected period.");
                }
            }

            string message = hasReportData
                ? "Census report exported successfully."
                : "Census report exported successfully. No records were found for the selected period.";

            MessageBox.Show(message, "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BuildSummarySheet(ExcelWorksheet sheet)
        {
            sheet.Cells["A1"].Value = "VAWC Census Dashboard";
            sheet.Cells["A1:D1"].Merge = true;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.Font.Size = 18;
            sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            sheet.Cells["A2"].Value = "Generated On";
            sheet.Cells["B2"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            sheet.Cells["A3"].Value = "Date Range";
            sheet.Cells["B3"].Value = dtpStartDate.Value.ToString("yyyy-MM-dd") + " to " + dtpEndDate.Value.ToString("yyyy-MM-dd");

            sheet.Cells["A5"].Value = "Metric";
            sheet.Cells["B5"].Value = "Value";
            using (ExcelRange range = sheet.Cells["A5:B5"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(DashboardColors[0]);
                range.Style.Font.Color.SetColor(Color.White);
            }

            sheet.Cells["A6"].Value = "Total Cases";
            sheet.Cells["B6"].Value = lblTotalCasesValue.Text;
            sheet.Cells["A7"].Value = "Active and Pending Cases";
            sheet.Cells["B7"].Value = lblActiveCasesValue.Text;
            sheet.Cells["A8"].Value = "Settled Cases";
            sheet.Cells["B8"].Value = lblResolvedCasesValue.Text;

            if (!hasReportData)
            {
                sheet.Cells["A10"].Value = "No records found for the selected period.";
                sheet.Cells["A10:D10"].Merge = true;
                sheet.Cells["A10"].Style.Font.Bold = true;
                sheet.Cells["A10"].Style.Font.Size = 12;
                sheet.Cells["A10"].Style.Font.Color.SetColor(DashboardColors[1]);
                return;
            }

            WriteTableFromQuery(sheet, "A10", "Violation Types", @"
                SELECT CASE
                           WHEN violation LIKE '%9262%' THEN 'R.A. 9262'
                           WHEN violation LIKE '%8353%' THEN 'R.A. 8353'
                           WHEN violation LIKE '%7877%' THEN 'R.A. 7877'
                           WHEN violation LIKE '%9208%' OR violation LIKE '%10364%' THEN 'R.A. 9208/10364'
                           ELSE 'Other'
                       END AS Category,
                       COUNT(*) AS Count
                FROM caselist
                " + GetDateRangeFilter("WHERE ") + @"
                GROUP BY Category
                ORDER BY Count DESC");

            WriteTableFromQuery(sheet, "D10", "Case Status", @"
                SELECT CASE
                           WHEN casestatus LIKE '%Pending%' THEN 'Pending'
                           WHEN casestatus LIKE '%Dismissed%' THEN 'Dismissed'
                           WHEN casestatus LIKE '%Finished%' OR casestatus LIKE '%Completed%' THEN 'Finished / Completed'
                           WHEN casestatus LIKE '%Stop%' THEN 'Stop'
                           ELSE casestatus
                       END AS Category,
                       COUNT(*) AS Count
                FROM caselist
                " + GetDateRangeFilter("WHERE ") + @"
                AND casestatus IS NOT NULL AND TRIM(casestatus) <> ''
                GROUP BY Category
                ORDER BY Count DESC");
        }

        private void WriteTableFromQuery(ExcelWorksheet sheet, string startCell, string title, string query)
        {
            DataTable table = ExecuteQuery(query);
            ExcelCellAddress start = new ExcelCellAddress(startCell);

            sheet.Cells[start.Row, start.Column].Value = title;
            sheet.Cells[start.Row, start.Column, start.Row, start.Column + 1].Merge = true;
            sheet.Cells[start.Row, start.Column].Style.Font.Bold = true;
            sheet.Cells[start.Row, start.Column].Style.Font.Size = 12;

            sheet.Cells[start.Row + 1, start.Column].Value = "Category";
            sheet.Cells[start.Row + 1, start.Column + 1].Value = "Count";
            using (ExcelRange headerRange = sheet.Cells[start.Row + 1, start.Column, start.Row + 1, start.Column + 1])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 230, 241));
            }

            int rowIndex = start.Row + 2;
            foreach (DataRow row in table.Rows)
            {
                sheet.Cells[rowIndex, start.Column].Value = Convert.ToString(row["Category"]);
                sheet.Cells[rowIndex, start.Column + 1].Value = Convert.ToInt32(row["Count"]);
                rowIndex++;
            }
        }

        private void BuildChartsSheet(ExcelWorksheet sheet)
        {
            sheet.Cells["A1"].Value = "Census Dashboard Charts";
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.Font.Size = 18;

            if (!hasReportData)
            {
                sheet.Cells["A3"].Value = "No records found for the selected period.";
                sheet.Cells["A3:H3"].Merge = true;
                sheet.Cells["A3"].Style.Font.Bold = true;
                sheet.Cells["A3"].Style.Font.Size = 14;
                sheet.Cells["A3"].Style.Font.Color.SetColor(DashboardColors[1]);
                return;
            }

            AddChartImage(sheet, chartMonthlyTrends, "Monthly Trends", 3, 1);
            AddChartImage(sheet, chartViolationTypes, "Violation Types", 22, 1);
            AddChartImage(sheet, chartAgeDistribution, "Age Distribution", 22, 9);
            AddChartImage(sheet, chartCaseStatus, "Case Status", 41, 1);
            AddChartImage(sheet, chartRelationshipAnalysis, "Relationship Analysis", 41, 9);
            AddChartImage(sheet, chartEducationLevel, "Education Level", 60, 1);
            AddChartImage(sheet, chartGeographicDistribution, "Geographic Distribution", 60, 9);
            AddChartImage(sheet, chartCivilStatus, "Civil Status", 79, 1);
            AddChartImage(sheet, chartOccupation, "Occupation", 79, 9);
        }

        private void AddChartImage(ExcelWorksheet sheet, Chart chart, string title, int row, int column)
        {
            sheet.Cells[row, column].Value = title;
            sheet.Cells[row, column].Style.Font.Bold = true;
            sheet.Cells[row, column].Style.Font.Size = 12;

            using (MemoryStream stream = new MemoryStream())
            {
                chart.SaveImage(stream, ChartImageFormat.Png);
                stream.Position = 0;

                ExcelPicture picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString("N"), stream);
                picture.SetPosition(row, 4, column - 1, 4);
                picture.SetSize(520, 260);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            using (PrintDocument printDocument = new PrintDocument())
            {
                printDocument.PrintPage += PrintDocument_PrintPage;
                using (PrintPreviewDialog preview = new PrintPreviewDialog())
                {
                    preview.Document = printDocument;
                    preview.WindowState = FormWindowState.Maximized;
                    preview.ShowDialog();
                }

                UserLogService.Log(
                    CurrentUsername,
                    "PrintCensusDashboard",
                    "census_dashboard",
                    0,
                    $"Opened print preview for census dashboard for {BuildSelectedDateRangeDescription()}.");
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            using (Bitmap bitmap = CapturePrintableDashboardBitmap())
            {
                float scaleX = (e.PageBounds.Width - 40F) / bitmap.Width;
                float scaleY = (e.PageBounds.Height - 40F) / bitmap.Height;
                float scale = Math.Min(scaleX, scaleY);

                int width = (int)(bitmap.Width * scale);
                int height = (int)(bitmap.Height * scale);
                int x = (e.PageBounds.Width - width) / 2;
                e.Graphics.DrawImage(bitmap, x, 20, width, height);
                PrintWatermarkHelper.Draw(e.Graphics, new RectangleF(x, 20, width, height));
            }
        }

        private Bitmap CapturePrintableDashboardBitmap()
        {
            Label printDateRangeLabel = CreatePrintDateRangeLabel();
            Label generatedByLabel = CreateGeneratedByLabel();
            bool refreshVisible = btnRefresh.Visible;
            bool printVisible = btnPrint.Visible;
            bool exportVisible = btnExport.Visible;
            bool startPickerVisible = dtpStartDate.Visible;
            bool endPickerVisible = dtpEndDate.Visible;
            bool startLabelVisible = lblStartDate.Visible;
            bool endLabelVisible = lblEndDate.Visible;
            bool titleVisible = lblTitle.Visible;

            try
            {
                lblTitle.Visible = false;
                btnRefresh.Visible = false;
                btnPrint.Visible = false;
                btnExport.Visible = false;
                dtpStartDate.Visible = false;
                dtpEndDate.Visible = false;
                lblStartDate.Visible = false;
                lblEndDate.Visible = false;

                panelHeader.Controls.Add(printDateRangeLabel);
                printDateRangeLabel.BringToFront();
                panelCharts.Controls.Add(generatedByLabel);
                generatedByLabel.BringToFront();
                panelHeader.PerformLayout();
                panelCharts.PerformLayout();
                Refresh();

                Bitmap bitmap = new Bitmap(Width, Height);
                DrawToBitmap(bitmap, new Rectangle(0, 0, Width, Height));
                return bitmap;
            }
            finally
            {
                panelHeader.Controls.Remove(printDateRangeLabel);
                printDateRangeLabel.Dispose();

                panelCharts.Controls.Remove(generatedByLabel);
                generatedByLabel.Dispose();

                btnRefresh.Visible = refreshVisible;
                btnPrint.Visible = printVisible;
                btnExport.Visible = exportVisible;
                dtpStartDate.Visible = startPickerVisible;
                dtpEndDate.Visible = endPickerVisible;
                lblStartDate.Visible = startLabelVisible;
                lblEndDate.Visible = endLabelVisible;
                lblTitle.Visible = titleVisible;
            }
        }

        private Label CreatePrintDateRangeLabel()
        {
            Label printDateRangeLabel = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 18F, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = $"From: {dtpStartDate.Value:MMMM d, yyyy}    To: {dtpEndDate.Value:MMMM d, yyyy}"
            };

            int labelWidth = 760;
            printDateRangeLabel.Size = new Size(labelWidth, 40);
            printDateRangeLabel.Location = new Point((panelHeader.Width - labelWidth) / 2, 20);

            return printDateRangeLabel;
        }

        private Label CreateGeneratedByLabel()
        {
            string generatedByName = string.IsNullOrWhiteSpace(CurrentUserFullName)
                ? "Unknown User"
                : CurrentUserFullName.Trim();

            Label generatedByLabel = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 11F, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"Generated by: {generatedByName}"
            };

            generatedByLabel.Size = new Size(panelCharts.Width - 80, 34);
            generatedByLabel.Location = new Point(40, panelCharts.Height - generatedByLabel.Height - 12);

            return generatedByLabel;
        }

        private string BuildSelectedDateRangeDescription()
        {
            return $"{dtpStartDate.Value:MMMM d, yyyy} to {dtpEndDate.Value:MMMM d, yyyy}";
        }
    }
}
