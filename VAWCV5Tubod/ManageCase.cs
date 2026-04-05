using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using VAWCV5Tubod.Connection;

namespace VAWCV5Tubod
{
    public partial class ManageCase : Form
    {
        private const string Ra9262ViolationText =
            "R.A. 9262: Anti Violence Against Women and their Children Act";

        private int loadedCaseId;
        private int loadedCompId;
        private int loadedRespId;
        private DateTime loadedComplaintDate;
        private bool formInitialized;

        public ManageCase()
        {
            InitializeComponent();
            ConfigureForm();
        }

        public bool LoadCaseForEdit(int caseId)
        {
            try
            {
                EnsureFormInitialized();

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                const string query = """
                    SELECT
                        c.caseId,
                        c.complaintDate,
                        c.compId,
                        c.respId,
                        c.violation,
                        c.subViolation,
                        c.subViolation2,
                        c.subViolation3,
                        c.subViolation4,
                        c.casestatus,
                        c.referredto,
                        c.placeofincident,
                        c.incidentdate,
                        c.incidentdescription,
                        comp.comp_image,
                        comp.comp_lastname,
                        comp.comp_firstname,
                        comp.comp_middlename,
                        comp.comp_sex,
                        comp.comp_birthdate,
                        comp.comp_purok,
                        comp.comp_contactno,
                        comp.comp_civilstatus,
                        comp.comp_religion,
                        comp.comp_occupation,
                        comp.comp_nationality,
                        comp.comp_eduattain,
                        resp.resp_img,
                        resp.resp_lastname,
                        resp.resp_firstname,
                        resp.resp_middlename,
                        resp.resp_sex,
                        resp.resp_birthdate,
                        resp.resp_purok,
                        resp.resp_contactno,
                        resp.resp_civilstatus,
                        resp.resp_religion,
                        resp.resp_occupation,
                        resp.resp_nationality,
                        resp.resp_eduattain,
                        resp.resp_relationshiptocomplainant
                    FROM caselist c
                    INNER JOIN complainant comp ON c.compId = comp.compId
                    INNER JOIN respondent resp ON c.respId = resp.respId
                    WHERE c.caseId = @caseId
                    LIMIT 1;
                    """;

                using MySqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@caseId", caseId);

                using MySqlDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show(
                        "The selected case record could not be found.",
                        "Case Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }

                PopulateForm(reader);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load the case for editing.{Environment.NewLine}{ex.Message}",
                    "Load Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private void ConfigureForm()
        {
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Click += pictureBox1_Click;
            imgComplainantbtn.Click += imgComplainantbtn_Click;
            guna2Button1.Click += guna2Button1_Click;
            updateCasebtn.Click += updateCasebtn_Click;
            FormClosed += ManageCase_FormClosed;

            updt_case_ra_violation.SelectedIndexChanged += updt_case_ra_violation_SelectedIndexChanged;
            updt_case_ra_violation.TextChanged += updt_case_ra_violation_TextChanged;
            updt_comp_birthdate.ValueChanged += updt_comp_birthdate_ValueChanged;
            updt_resp_birthdate.ValueChanged += updt_resp_birthdate_ValueChanged;

            updt_comp_image.SizeMode = PictureBoxSizeMode.Zoom;
            updt_resp_image.SizeMode = PictureBoxSizeMode.Zoom;
            updt_comp_age.ReadOnly = true;
            updt_resp_age.ReadOnly = true;

            AttachNumericOnlyHandlers(updt_comp_contactnumber);
            AttachNumericOnlyHandlers(updt_resp_contactnumber);
        }

        private void EnsureFormInitialized()
        {
            if (formInitialized)
            {
                return;
            }

            PopulateFixedOptions();
            LoadPurokOptions();
            ToggleRa9262SubViolations();
            UpdateAgeFields();
            formInitialized = true;
        }

        private void PopulateFixedOptions()
        {
            PopulateComboBoxItems(
                updt_case_ra_violation,
                "R.A. 9262: Anti Violence Against Women and their Children Act",
                "R.A. 8353: Anti-Rape Law of 1995",
                "R.A. 7877: Anti-Sexual Harrassment Act",
                "R.A. 9208/10364: Anti-Trafficking in Person Act of 2003");

            PopulateComboBoxItems(updt_case_status, "Active", "Pending", "Settled");
            PopulateComboBoxItems(updt_case_referred_to, "Not Yet", "CSWDO", "DSWD", "PNP", "COURT", "Hospital", "Other");
            PopulateComboBoxItems(
                updt_case_incident_place,
                "Home",
                "Religious Institutions",
                "Brothels and Similar Establishments",
                "Work",
                "Place of Medical Treatment",
                "School",
                "Transportation & Connecting Sites",
                "Commercial Places",
                "No Response",
                "Others");

            PopulateComboBoxItems(updt_comp_sex, "MALE", "FEMALE");
            PopulateComboBoxItems(updt_resp_sex, "MALE", "FEMALE");
            PopulateComboBoxItems(updt_comp_civilstatus, "Single", "Married", "Widowed", "Separated", "Annulled");
            PopulateComboBoxItems(updt_resp_civilstatus, "Single", "Married", "Widowed", "Separated", "Annulled");

            PopulateComboBoxItems(
                updt_comp_educationalattainment,
                "Pre-school",
                "Elementary (Primary Education)",
                "Junior High School",
                "Senior High School (SHS)",
                "Tertiary Education (Higher Education)",
                "Technical-Vocational Education and Training (TVET)",
                "Alternative Learning System (ALS)");

            PopulateComboBoxItems(
                updt_resp_educationalattainment,
                "Pre-school",
                "Elementary (Primary Education)",
                "Junior High School",
                "Senior High School (SHS)",
                "Tertiary Education (Higher Education)",
                "Technical-Vocational Education and Training (TVET)",
                "Alternative Learning System (ALS)");

            PopulateComboBoxItems(
                updt_resp_relationship_to_complainant,
                "Spouse (Asawa)",
                "Boyfriend (Nobyo)",
                "Ex-spouse / Ex-partner",
                "Ex-Boyfriend",
                "In-law (Biyenan, Hipag, Bayaw)",
                "Other Relatives (Tiyuhin, Tiyahin, Pamangkin)");
        }

        private static void PopulateComboBoxItems(ComboBox comboBox, params string[] items)
        {
            comboBox.BeginUpdate();
            comboBox.Items.Clear();
            comboBox.Items.AddRange(items.Cast<object>().ToArray());
            comboBox.EndUpdate();
        }

        private void LoadPurokOptions()
        {
            using MySqlConnection connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            const string query = """
                SELECT purok_name
                FROM purok
                WHERE purok_name IS NOT NULL AND TRIM(purok_name) <> ''
                ORDER BY purok_id;
                """;

            using MySqlCommand command = new(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();

            List<string> purokNames = new();

            while (reader.Read())
            {
                purokNames.Add(reader.GetString("purok_name"));
            }

            PopulatePurokComboBox(updt_comp_purok, purokNames);
            PopulatePurokComboBox(updt_resp_purok, purokNames);
        }

        private static void PopulatePurokComboBox(ComboBox comboBox, IEnumerable<string> purokNames)
        {
            comboBox.BeginUpdate();
            comboBox.Items.Clear();
            comboBox.Items.AddRange(purokNames.Cast<object>().ToArray());
            comboBox.EndUpdate();
        }

        private void PopulateForm(MySqlDataReader reader)
        {
            loadedCaseId = GetIntValue(reader, "caseId");
            loadedCompId = GetIntValue(reader, "compId");
            loadedRespId = GetIntValue(reader, "respId");
            loadedComplaintDate = GetDateValue(reader, "complaintDate");

            CaseId.Text = loadedCaseId.ToString();
            ComplaintDate.Text = loadedComplaintDate.ToString("MMMM d, yyyy");

            updt_case_ra_violation.Text = GetStringValue(reader, "violation");
            updt_case_status.Text = GetStringValue(reader, "casestatus");
            updt_case_referred_to.Text = GetStringValue(reader, "referredto");
            updt_case_incident_place.Text = GetStringValue(reader, "placeofincident");
            updt_case_incident_description.Text = GetStringValue(reader, "incidentdescription");
            updt_case_incident_date.Value = GetDateValue(reader, "incidentdate");

            SetSubViolationSelections(
                GetStringValue(reader, "subViolation"),
                GetStringValue(reader, "subViolation2"),
                GetStringValue(reader, "subViolation3"),
                GetStringValue(reader, "subViolation4"));

            updt_comp_lastname.Text = GetStringValue(reader, "comp_lastname");
            updt_comp_firstname.Text = GetStringValue(reader, "comp_firstname");
            updt_comp_middlename.Text = GetStringValue(reader, "comp_middlename");
            updt_comp_sex.Text = GetStringValue(reader, "comp_sex");
            updt_comp_birthdate.Value = GetDateValue(reader, "comp_birthdate");
            updt_comp_purok.Text = GetStringValue(reader, "comp_purok");
            updt_comp_contactnumber.Text = GetStringValue(reader, "comp_contactno");
            updt_comp_civilstatus.Text = GetStringValue(reader, "comp_civilstatus");
            updt_comp_religion.Text = GetStringValue(reader, "comp_religion");
            updt_comp_occupation.Text = GetStringValue(reader, "comp_occupation");
            updt_comp_nationality.Text = GetStringValue(reader, "comp_nationality");
            updt_comp_educationalattainment.Text = GetStringValue(reader, "comp_eduattain");

            updt_resp_lastname.Text = GetStringValue(reader, "resp_lastname");
            updt_resp_firstname.Text = GetStringValue(reader, "resp_firstname");
            updt_resp_middlename.Text = GetStringValue(reader, "resp_middlename");
            updt_resp_sex.Text = GetStringValue(reader, "resp_sex");
            updt_resp_birthdate.Value = GetDateValue(reader, "resp_birthdate");
            updt_resp_purok.Text = GetStringValue(reader, "resp_purok");
            updt_resp_contactnumber.Text = GetStringValue(reader, "resp_contactno");
            updt_resp_civilstatus.Text = GetStringValue(reader, "resp_civilstatus");
            updt_resp_religion.Text = GetStringValue(reader, "resp_religion");
            updt_resp_occupation.Text = GetStringValue(reader, "resp_occupation");
            updt_resp_nationality.Text = GetStringValue(reader, "resp_nationality");
            updt_resp_educationalattainment.Text = GetStringValue(reader, "resp_eduattain");
            updt_resp_relationship_to_complainant.Text = GetStringValue(reader, "resp_relationshiptocomplainant");

            ReplacePictureBoxImage(updt_comp_image, GetImageValue(reader, "comp_image"));
            ReplacePictureBoxImage(updt_resp_image, GetImageValue(reader, "resp_img"));

            UpdateAgeFields();
            ToggleRa9262SubViolations();
        }

        private void SetSubViolationSelections(params string[] subViolations)
        {
            string[] normalizedValues = subViolations
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToArray();

            updt_case_ra_sub_physical_abuse.Checked = normalizedValues.Contains(updt_case_ra_sub_physical_abuse.Text, StringComparer.OrdinalIgnoreCase);
            updt_case_ra_sub_psychological_abuse.Checked = normalizedValues.Contains(updt_case_ra_sub_psychological_abuse.Text, StringComparer.OrdinalIgnoreCase);
            updt_case_ra_sub_sexual_abuse.Checked = normalizedValues.Contains(updt_case_ra_sub_sexual_abuse.Text, StringComparer.OrdinalIgnoreCase);
            updt_case_ra_sub_economic_abuse.Checked = normalizedValues.Contains(updt_case_ra_sub_economic_abuse.Text, StringComparer.OrdinalIgnoreCase);
        }

        private static string GetStringValue(MySqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : Convert.ToString(reader.GetValue(ordinal)) ?? string.Empty;
        }

        private static int GetIntValue(MySqlDataReader reader, string columnName)
        {
            string value = GetStringValue(reader, columnName);
            return int.TryParse(value, out int parsedValue) ? parsedValue : 0;
        }

        private static DateTime GetDateValue(MySqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? AutoGeneratedCaseId.GetPhilippineToday() : Convert.ToDateTime(reader.GetValue(ordinal));
        }

        private static byte[]? GetImageValue(MySqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : (byte[])reader.GetValue(ordinal);
        }

        private static void ReplacePictureBoxImage(PictureBox pictureBox, byte[]? imageBytes)
        {
            Image? previousImage = pictureBox.Image;
            pictureBox.Image = imageBytes is null ? null : CreateImageFromBytes(imageBytes);
            previousImage?.Dispose();
        }

        private static Image CreateImageFromBytes(byte[] imageBytes)
        {
            using MemoryStream memoryStream = new(imageBytes);
            using Image originalImage = Image.FromStream(memoryStream);
            return new Bitmap(originalImage);
        }

        private void pictureBox1_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void imgComplainantbtn_Click(object? sender, EventArgs e)
        {
            BrowseAndLoadImage(updt_comp_image);
        }

        private void guna2Button1_Click(object? sender, EventArgs e)
        {
            BrowseAndLoadImage(updt_resp_image);
        }

        private void BrowseAndLoadImage(PictureBox targetPictureBox)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Title = "Select an image";
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                using FileStream stream = new(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                using Image originalImage = Image.FromStream(stream);
                Image newImage = new Bitmap(originalImage);
                Image? previousImage = targetPictureBox.Image;

                targetPictureBox.Image = newImage;
                previousImage?.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load the selected image.{Environment.NewLine}{ex.Message}",
                    "Image Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void updt_case_ra_violation_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ToggleRa9262SubViolations();
        }

        private void updt_case_ra_violation_TextChanged(object? sender, EventArgs e)
        {
            ToggleRa9262SubViolations();
        }

        private void updt_comp_birthdate_ValueChanged(object? sender, EventArgs e)
        {
            UpdateAgeText(updt_comp_birthdate, updt_comp_age);
        }

        private void updt_resp_birthdate_ValueChanged(object? sender, EventArgs e)
        {
            UpdateAgeText(updt_resp_birthdate, updt_resp_age);
        }

        private void UpdateAgeFields()
        {
            UpdateAgeText(updt_comp_birthdate, updt_comp_age);
            UpdateAgeText(updt_resp_birthdate, updt_resp_age);
        }

        private static void UpdateAgeText(DateTimePicker birthDatePicker, TextBox ageTextBox)
        {
            ageTextBox.Text = CalculateAge(birthDatePicker.Value).ToString();
        }

        private static int CalculateAge(DateTime birthDate)
        {
            DateTime today = AutoGeneratedCaseId.GetPhilippineToday();
            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return Math.Max(age, 0);
        }

        private bool IsRa9262Selected()
        {
            return string.Equals(
                updt_case_ra_violation.Text.Trim(),
                Ra9262ViolationText,
                StringComparison.Ordinal);
        }

        private void ToggleRa9262SubViolations()
        {
            bool allowSubViolations = IsRa9262Selected();

            SetRaSubViolationState(updt_case_ra_sub_physical_abuse, allowSubViolations);
            SetRaSubViolationState(updt_case_ra_sub_psychological_abuse, allowSubViolations);
            SetRaSubViolationState(updt_case_ra_sub_sexual_abuse, allowSubViolations);
            SetRaSubViolationState(updt_case_ra_sub_economic_abuse, allowSubViolations);
        }

        private static void SetRaSubViolationState(CheckBox checkBox, bool enabled)
        {
            checkBox.Enabled = enabled;

            if (!enabled)
            {
                checkBox.Checked = false;
            }
        }

        private static void AttachNumericOnlyHandlers(TextBox textBox)
        {
            textBox.KeyPress += NumericTextBox_KeyPress;
            textBox.TextChanged += NumericTextBox_TextChanged;
        }

        private static void NumericTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private static void NumericTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            string originalText = textBox.Text;
            string digitsOnly = new(originalText.Where(char.IsDigit).ToArray());

            if (originalText == digitsOnly)
            {
                return;
            }

            int selectionStart = textBox.SelectionStart;
            int removedCharactersBeforeCaret = originalText
                .Take(Math.Min(selectionStart, originalText.Length))
                .Count(character => !char.IsDigit(character));

            textBox.Text = digitsOnly;
            textBox.SelectionStart = Math.Max(0, selectionStart - removedCharactersBeforeCaret);
        }

        private void updateCasebtn_Click(object? sender, EventArgs e)
        {
            if (!ValidateRequiredInputs())
            {
                return;
            }

            try
            {
                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                using MySqlTransaction transaction = connection.BeginTransaction();

                UpdateComplainant(connection, transaction);
                UpdateRespondent(connection, transaction);
                UpdateCaseRecord(connection, transaction);

                transaction.Commit();

                MessageBox.Show(
                    "Case information was updated successfully.",
                    "Update Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to update the case.{Environment.NewLine}{ex.Message}",
                    "Update Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateComplainant(MySqlConnection connection, MySqlTransaction transaction)
        {
            const string query = """
                UPDATE complainant
                SET
                    comp_image = @comp_image,
                    comp_lastname = @lastname,
                    comp_firstname = @firstname,
                    comp_middlename = @middlename,
                    comp_sex = @sex,
                    comp_age = @age,
                    comp_birthdate = @birthdate,
                    comp_purok = @purok,
                    comp_barangay = @barangay,
                    comp_municipal = @municipal,
                    comp_province = @province,
                    comp_contactno = @contactno,
                    comp_civilstatus = @civilstatus,
                    comp_eduattain = @eduattain,
                    comp_religion = @religion,
                    comp_occupation = @occupation,
                    comp_nationality = @nationality
                WHERE compId = @compId;
                """;

            using MySqlCommand command = new(query, connection, transaction);
            command.Parameters.Add("@comp_image", MySqlDbType.LongBlob).Value =
                ConvertImageToBytes(updt_comp_image.Image) is byte[] imageBytes ? imageBytes : DBNull.Value;
            command.Parameters.AddWithValue("@lastname", updt_comp_lastname.Text.Trim());
            command.Parameters.AddWithValue("@firstname", updt_comp_firstname.Text.Trim());
            command.Parameters.AddWithValue("@middlename", updt_comp_middlename.Text.Trim());
            command.Parameters.AddWithValue("@sex", updt_comp_sex.Text.Trim());
            command.Parameters.AddWithValue("@age", ParseIntOrZero(updt_comp_age.Text));
            command.Parameters.AddWithValue("@birthdate", updt_comp_birthdate.Value);
            command.Parameters.AddWithValue("@purok", updt_comp_purok.Text.Trim());
            command.Parameters.AddWithValue("@barangay", "Tubod");
            command.Parameters.AddWithValue("@municipal", "Lakewood");
            command.Parameters.AddWithValue("@province", "Zamboanga Del Sur");
            command.Parameters.AddWithValue("@contactno", updt_comp_contactnumber.Text.Trim());
            command.Parameters.AddWithValue("@civilstatus", updt_comp_civilstatus.Text.Trim());
            command.Parameters.AddWithValue("@eduattain", updt_comp_educationalattainment.Text.Trim());
            command.Parameters.AddWithValue("@religion", updt_comp_religion.Text.Trim());
            command.Parameters.AddWithValue("@occupation", updt_comp_occupation.Text.Trim());
            command.Parameters.AddWithValue("@nationality", updt_comp_nationality.Text.Trim());
            command.Parameters.AddWithValue("@compId", loadedCompId);
            command.ExecuteNonQuery();
        }

        private void UpdateRespondent(MySqlConnection connection, MySqlTransaction transaction)
        {
            const string query = """
                UPDATE respondent
                SET
                    resp_img = @resp_img,
                    resp_lastname = @lastname,
                    resp_firstname = @firstname,
                    resp_middlename = @middlename,
                    resp_sex = @sex,
                    resp_age = @age,
                    resp_birthdate = @birthdate,
                    resp_purok = @purok,
                    resp_barangay = @barangay,
                    resp_municipal = @municipal,
                    resp_province = @province,
                    resp_contactno = @contactno,
                    resp_civilstatus = @civilstatus,
                    resp_eduattain = @eduattain,
                    resp_religion = @religion,
                    resp_occupation = @occupation,
                    resp_relationshiptocomplainant = @relationshiptocomplainant,
                    resp_nationality = @nationality
                WHERE respId = @respId;
                """;

            using MySqlCommand command = new(query, connection, transaction);
            command.Parameters.Add("@resp_img", MySqlDbType.LongBlob).Value =
                ConvertImageToBytes(updt_resp_image.Image) is byte[] imageBytes ? imageBytes : DBNull.Value;
            command.Parameters.AddWithValue("@lastname", updt_resp_lastname.Text.Trim());
            command.Parameters.AddWithValue("@firstname", updt_resp_firstname.Text.Trim());
            command.Parameters.AddWithValue("@middlename", updt_resp_middlename.Text.Trim());
            command.Parameters.AddWithValue("@sex", updt_resp_sex.Text.Trim());
            command.Parameters.AddWithValue("@age", ParseIntOrZero(updt_resp_age.Text));
            command.Parameters.AddWithValue("@birthdate", updt_resp_birthdate.Value);
            command.Parameters.AddWithValue("@purok", updt_resp_purok.Text.Trim());
            command.Parameters.AddWithValue("@barangay", "Tubod");
            command.Parameters.AddWithValue("@municipal", "Lakewood");
            command.Parameters.AddWithValue("@province", "Zamboanga Del Sur");
            command.Parameters.AddWithValue("@contactno", updt_resp_contactnumber.Text.Trim());
            command.Parameters.AddWithValue("@civilstatus", updt_resp_civilstatus.Text.Trim());
            command.Parameters.AddWithValue("@eduattain", updt_resp_educationalattainment.Text.Trim());
            command.Parameters.AddWithValue("@religion", updt_resp_religion.Text.Trim());
            command.Parameters.AddWithValue("@occupation", updt_resp_occupation.Text.Trim());
            command.Parameters.AddWithValue("@relationshiptocomplainant", updt_resp_relationship_to_complainant.Text.Trim());
            command.Parameters.AddWithValue("@nationality", updt_resp_nationality.Text.Trim());
            command.Parameters.AddWithValue("@respId", loadedRespId);
            command.ExecuteNonQuery();
        }

        private void UpdateCaseRecord(MySqlConnection connection, MySqlTransaction transaction)
        {
            const string query = """
                UPDATE caselist
                SET
                    complaintDate = @complaintDate,
                    compId = @compId,
                    complainantFullname = @complainantFullname,
                    respId = @respId,
                    respondentFullname = @respondentFullname,
                    violation = @violation,
                    subViolation = @subViolation,
                    subViolation2 = @subViolation2,
                    subViolation3 = @subViolation3,
                    subViolation4 = @subViolation4,
                    casestatus = @casestatus,
                    referredto = @referredto,
                    placeofincident = @placeofincident,
                    incidentdate = @incidentdate,
                    incidentdescription = @incidentdescription
                WHERE caseId = @caseId;
                """;

            using MySqlCommand command = new(query, connection, transaction);
            command.Parameters.AddWithValue("@complaintDate", loadedComplaintDate);
            command.Parameters.AddWithValue("@compId", loadedCompId);
            command.Parameters.AddWithValue("@complainantFullname", BuildFullName(updt_comp_lastname.Text, updt_comp_firstname.Text, updt_comp_middlename.Text));
            command.Parameters.AddWithValue("@respId", loadedRespId);
            command.Parameters.AddWithValue("@respondentFullname", BuildFullName(updt_resp_lastname.Text, updt_resp_firstname.Text, updt_resp_middlename.Text));
            command.Parameters.AddWithValue("@violation", updt_case_ra_violation.Text.Trim());
            command.Parameters.AddWithValue("@subViolation", GetCheckboxValueOrEmpty(updt_case_ra_sub_physical_abuse));
            command.Parameters.AddWithValue("@subViolation2", GetCheckboxValueOrEmpty(updt_case_ra_sub_psychological_abuse));
            command.Parameters.AddWithValue("@subViolation3", GetCheckboxValueOrEmpty(updt_case_ra_sub_sexual_abuse));
            command.Parameters.AddWithValue("@subViolation4", GetCheckboxValueOrEmpty(updt_case_ra_sub_economic_abuse));
            command.Parameters.AddWithValue("@casestatus", updt_case_status.Text.Trim());
            command.Parameters.AddWithValue("@referredto", updt_case_referred_to.Text.Trim());
            command.Parameters.AddWithValue("@placeofincident", updt_case_incident_place.Text.Trim());
            command.Parameters.AddWithValue("@incidentdate", updt_case_incident_date.Value);
            command.Parameters.AddWithValue("@incidentdescription", updt_case_incident_description.Text.Trim());
            command.Parameters.AddWithValue("@caseId", loadedCaseId);
            command.ExecuteNonQuery();
        }

        private bool ValidateRequiredInputs()
        {
            List<string> missingFields = new();

            ValidateRequiredText(updt_comp_lastname, "Complainant Last Name", missingFields);
            ValidateRequiredText(updt_comp_firstname, "Complainant First Name", missingFields);
            ValidateRequiredText(updt_comp_middlename, "Complainant Middle Name", missingFields);
            ValidateRequiredText(updt_comp_sex, "Complainant Sex", missingFields);
            ValidateRequiredText(updt_comp_age, "Complainant Age", missingFields);
            ValidateRequiredText(updt_comp_purok, "Complainant Purok", missingFields);
            ValidateRequiredText(updt_comp_contactnumber, "Complainant Contact Number", missingFields);
            ValidateRequiredText(updt_comp_civilstatus, "Complainant Civil Status", missingFields);
            ValidateRequiredText(updt_comp_religion, "Complainant Religion", missingFields);
            ValidateRequiredText(updt_comp_occupation, "Complainant Occupation", missingFields);
            ValidateRequiredText(updt_comp_nationality, "Complainant Nationality", missingFields);
            ValidateRequiredText(updt_comp_educationalattainment, "Complainant Educational Attainment", missingFields);

            ValidateRequiredText(updt_resp_lastname, "Respondent Last Name", missingFields);
            ValidateRequiredText(updt_resp_firstname, "Respondent First Name", missingFields);
            ValidateRequiredText(updt_resp_middlename, "Respondent Middle Name", missingFields);
            ValidateRequiredText(updt_resp_sex, "Respondent Sex", missingFields);
            ValidateRequiredText(updt_resp_age, "Respondent Age", missingFields);
            ValidateRequiredText(updt_resp_purok, "Respondent Purok", missingFields);
            ValidateRequiredText(updt_resp_contactnumber, "Respondent Contact Number", missingFields);
            ValidateRequiredText(updt_resp_civilstatus, "Respondent Civil Status", missingFields);
            ValidateRequiredText(updt_resp_religion, "Respondent Religion", missingFields);
            ValidateRequiredText(updt_resp_occupation, "Respondent Occupation", missingFields);
            ValidateRequiredText(updt_resp_nationality, "Respondent Nationality", missingFields);
            ValidateRequiredText(updt_resp_educationalattainment, "Respondent Educational Attainment", missingFields);
            ValidateRequiredText(updt_resp_relationship_to_complainant, "Relationship To Complainant", missingFields);

            ValidateRequiredText(updt_case_ra_violation, "R.A. Violation", missingFields);
            ValidateRequiredText(updt_case_status, "Case Status", missingFields);
            ValidateRequiredText(updt_case_referred_to, "Referred To", missingFields);
            ValidateRequiredText(updt_case_incident_place, "Place Of Incident", missingFields);
            ValidateRequiredText(updt_case_incident_description, "Incident Description", missingFields);

            if (IsRa9262Selected() &&
                !updt_case_ra_sub_physical_abuse.Checked &&
                !updt_case_ra_sub_psychological_abuse.Checked &&
                !updt_case_ra_sub_sexual_abuse.Checked &&
                !updt_case_ra_sub_economic_abuse.Checked)
            {
                missingFields.Add("At least one R.A. Sub Case Violation");
            }

            if (missingFields.Count == 0)
            {
                return true;
            }

            MessageBox.Show(
                "Please complete all required fields before updating:" +
                Environment.NewLine + Environment.NewLine +
                string.Join(Environment.NewLine, missingFields.Select(field => $"- {field}")),
                "Incomplete Form",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        private static void ValidateRequiredText(Control control, string fieldName, List<string> missingFields)
        {
            if (string.IsNullOrWhiteSpace(control.Text))
            {
                missingFields.Add(fieldName);
            }
        }

        private static int ParseIntOrZero(string value)
        {
            return int.TryParse(value, out int parsedValue) ? parsedValue : 0;
        }

        private static string BuildFullName(string lastName, string firstName, string middleName)
        {
            return string.Join(
                " ",
                new[] { lastName.Trim(), firstName.Trim(), middleName.Trim() }
                    .Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string GetCheckboxValueOrEmpty(CheckBox checkBox)
        {
            return checkBox.Checked ? checkBox.Text : string.Empty;
        }

        private static byte[]? ConvertImageToBytes(Image? image)
        {
            if (image is null)
            {
                return null;
            }

            using MemoryStream memoryStream = new();
            image.Save(memoryStream, ImageFormat.Png);
            return memoryStream.ToArray();
        }

        private void ManageCase_FormClosed(object? sender, FormClosedEventArgs e)
        {
            updt_comp_image.Image?.Dispose();
            updt_resp_image.Image?.Dispose();
        }
    }
}
