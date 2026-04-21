using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using VAWCV5Tubod.Connection;

namespace VAWCV5Tubod
{
    public partial class Documents : Form
    {
        private const string GeneratedByFallbackName = "Unknown User";
        private const float GeneratedByGap = 8f;
        private const float PrintContentPadding = 10f;

        private UserControl? currentUserControl;
        private IntakeForm? intakeForm;
        private ReferralForm? referralForm;
        private readonly PrintDocument printDocument;
        private readonly string currentUserFullName;
        private readonly string currentUsername;
        private Bitmap? pendingPrintBitmap;

        public Documents()
            : this(string.Empty, string.Empty)
        {
        }

        public Documents(string currentUserFullName, string currentUsername = "")
        {
            this.currentUserFullName = currentUserFullName;
            this.currentUsername = currentUsername;
            printDocument = new PrintDocument();
            printDocument.OriginAtMargins = false;
            printDocument.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
            InitializeComponent();
            SetupEventHandlers();
            ShowIntakeForm();
        }

        private void SetupEventHandlers()
        {
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            button3.Click += button3_Click;
            button4.Click += button4_Click;
            textBox1.Leave += textBox1_Leave;
            textBox1.KeyDown += textBox1_KeyDown;
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        private void button1_Click(object? sender, EventArgs e)
        {
            try
            {
                pendingPrintBitmap?.Dispose();
                pendingPrintBitmap = CaptureCurrentControlBitmap();
                printDocument.DocumentName = BuildDocumentName();

                using PrintPreviewDialog previewDialog = new()
                {
                    Document = printDocument,
                    UseAntiAlias = true,
                    WindowState = FormWindowState.Maximized
                };

                previewDialog.ShowDialog(this);
                UserLogService.Log(
                    currentUsername,
                    "PrintDocument",
                    "documents",
                    ParseCaseIdOrZero(),
                    $"Opened print preview for {BuildDocumentName()}.");

                DisposePendingPrintBitmap();
            }
            catch (Exception ex)
            {
                DisposePendingPrintBitmap();
                MessageBox.Show(
                    $"Unable to print the current document.{Environment.NewLine}{ex.Message}",
                    "Print Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object? sender, EventArgs e)
        {
            try
            {
                using SaveFileDialog dialog = new()
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"{BuildDocumentName()}.pdf"
                };

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                using Bitmap bitmap = CaptureCurrentControlBitmap();
                ExportBitmapToPdf(bitmap, dialog.FileName);

                UserLogService.Log(
                    currentUsername,
                    "ExportDocument",
                    "documents",
                    ParseCaseIdOrZero(),
                    $"Exported {BuildDocumentName()} to PDF.");

                MessageBox.Show(
                    $"PDF exported successfully.{Environment.NewLine}{dialog.FileName}",
                    "Export Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to export the PDF.{Environment.NewLine}{ex.Message}",
                    "Export Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object? sender, EventArgs e)
        {
            ShowIntakeForm();
        }

        private void button4_Click(object? sender, EventArgs e)
        {
            ShowReferralForm();
        }

        private void textBox1_Leave(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                LoadCaseData();
            }
        }

        private void textBox1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            LoadCaseData();
        }

        private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            if (pendingPrintBitmap == null)
            {
                e.HasMorePages = false;
                return;
            }

            e.Graphics.PageUnit = GraphicsUnit.Display;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            RectangleF contentBounds = GetPrintableContentBounds(e);
            using Font generatedByFont = new("Arial", 11f, FontStyle.Bold);
            string generatedByText = BuildGeneratedByText();
            SizeF generatedBySize = e.Graphics.MeasureString(generatedByText, generatedByFont);
            float availableImageHeight = Math.Max(1f, contentBounds.Height - generatedBySize.Height - GeneratedByGap);
            float scale = Math.Min(
                contentBounds.Width / pendingPrintBitmap.Width,
                availableImageHeight / pendingPrintBitmap.Height);

            int drawWidth = (int)(pendingPrintBitmap.Width * scale);
            int drawHeight = (int)(pendingPrintBitmap.Height * scale);
            int drawX = (int)(contentBounds.Left + ((contentBounds.Width - drawWidth) / 2f));
            int drawY = (int)contentBounds.Top;
            float generatedByY = drawY + drawHeight + GeneratedByGap;

            e.Graphics.DrawImage(pendingPrintBitmap, drawX, drawY, drawWidth, drawHeight);
            e.Graphics.DrawString(generatedByText, generatedByFont, Brushes.Black, drawX, generatedByY);
            e.HasMorePages = false;
        }

        private void ShowIntakeForm()
        {
            intakeForm ??= new IntakeForm();
            ShowUserControl(intakeForm);
        }

        private void ShowReferralForm()
        {
            referralForm ??= new ReferralForm();
            ShowUserControl(referralForm);
        }

        private void ShowUserControl(UserControl userControl)
        {
            panel1.SuspendLayout();
            panel1.Controls.Clear();

            userControl.Location = new Point(0, 0);
            panel1.Controls.Add(userControl);
            panel1.AutoScroll = true;
            panel1.AutoScrollMinSize = new Size(userControl.Width, userControl.Height);

            currentUserControl = userControl;
            panel1.ResumeLayout();

            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                LoadCaseData();
            }
        }

        private void LoadCaseData()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                return;
            }

            if (!int.TryParse(textBox1.Text.Trim(), out int caseId))
            {
                MessageBox.Show(
                    "Please enter a valid numeric Case ID.",
                    "Invalid Case ID",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                textBox1.SelectAll();
                textBox1.Focus();
                return;
            }

            try
            {
                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                DataRow? caseRow = GetSingleRow(
                    connection,
                    """
                    SELECT *
                    FROM caselist
                    WHERE caseId = @id
                    LIMIT 1;
                    """,
                    "@id",
                    caseId);

                if (caseRow == null)
                {
                    MessageBox.Show(
                        $"Case ID {caseId} was not found.",
                        "Case Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                int complainantId = GetIntValue(caseRow, "compId");
                int respondentId = GetIntValue(caseRow, "respId");

                DataRow? complainantRow = complainantId > 0
                    ? GetSingleRow(
                        connection,
                        """
                        SELECT *
                        FROM complainant
                        WHERE compId = @id
                        LIMIT 1;
                        """,
                        "@id",
                        complainantId)
                    : null;

                DataRow? respondentRow = respondentId > 0
                    ? GetSingleRow(
                        connection,
                        """
                        SELECT *
                        FROM respondent
                        WHERE respId = @id
                        LIMIT 1;
                        """,
                        "@id",
                        respondentId)
                    : null;

                if (currentUserControl == intakeForm && intakeForm != null)
                {
                    intakeForm.PopulateCaseData(caseRow, complainantRow, respondentRow);
                }
                else if (currentUserControl == referralForm && referralForm != null)
                {
                    referralForm.PopulateCaseData(caseRow, complainantRow, respondentRow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load the case information.{Environment.NewLine}{ex.Message}",
                    "Load Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static DataRow? GetSingleRow(MySqlConnection connection, string query, string parameterName, int id)
        {
            using MySqlCommand command = new(query, connection);
            using MySqlDataAdapter adapter = new(command);

            command.Parameters.AddWithValue(parameterName, id);

            DataTable table = new();
            adapter.Fill(table);

            return table.Rows.Count > 0 ? table.Rows[0] : null;
        }

        private static int GetIntValue(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(row[columnName]);
        }

        private Bitmap CaptureCurrentControlBitmap()
        {
            if (currentUserControl == null)
            {
                throw new InvalidOperationException("There is no active document to print or export.");
            }

            Size captureSize = currentUserControl.Size;

            if (captureSize.Width <= 0 || captureSize.Height <= 0)
            {
                throw new InvalidOperationException("The active document does not have a printable size.");
            }

            Bitmap bitmap = new(captureSize.Width, captureSize.Height);
            currentUserControl.DrawToBitmap(bitmap, new Rectangle(Point.Empty, captureSize));
            return bitmap;
        }

        private void ExportBitmapToPdf(Bitmap bitmap, string filePath)
        {
            using MemoryStream imageStream = new();
            bitmap.Save(imageStream, ImageFormat.Png);

            using FileStream pdfStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            iTextSharp.text.Document document = new(iTextSharp.text.PageSize.A4, 24f, 24f, 24f, 24f);

            try
            {
                PdfWriter.GetInstance(document, pdfStream);
                document.Open();

                iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(imageStream.ToArray());
                float availableWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                float availableHeight = document.PageSize.Height - document.TopMargin - document.BottomMargin;

                pdfImage.ScaleToFit(availableWidth, availableHeight);
                pdfImage.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                document.Add(pdfImage);
            }
            finally
            {
                document.Close();
            }
        }

        private string BuildDocumentName()
        {
            string documentType = currentUserControl == referralForm ? "ReferralForm" : "IntakeForm";
            string caseId = string.IsNullOrWhiteSpace(textBox1.Text)
                ? "NoCaseId"
                : new string(textBox1.Text.Trim().Where(char.IsLetterOrDigit).ToArray());

            return $"{documentType}_{caseId}";
        }

        private int ParseCaseIdOrZero()
        {
            return int.TryParse(textBox1.Text.Trim(), out int caseId) ? caseId : 0;
        }

        private string BuildGeneratedByText()
        {
            string generatedByName = string.IsNullOrWhiteSpace(currentUserFullName)
                ? GeneratedByFallbackName
                : currentUserFullName.Trim();

            return $"Generated by: {generatedByName}";
        }

        private static RectangleF GetPrintableContentBounds(PrintPageEventArgs e)
        {
            RectangleF bounds = e.PageSettings.PrintableArea;

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                bounds = e.PageBounds;
            }

            float left = bounds.Left + PrintContentPadding;
            float top = bounds.Top + PrintContentPadding;
            float width = Math.Max(1f, bounds.Width - (PrintContentPadding * 2f));
            float height = Math.Max(1f, bounds.Height - (PrintContentPadding * 2f));

            return new RectangleF(left, top, width, height);
        }

        private void DisposePendingPrintBitmap()
        {
            pendingPrintBitmap?.Dispose();
            pendingPrintBitmap = null;
        }
    }
}
