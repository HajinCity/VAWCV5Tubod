using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using MySql.Data.MySqlClient;
using VAWCV5Tubod.Connection;

namespace VAWCV5Tubod
{
    public partial class SystemManagement : Form
    {
        private readonly string currentUserRole;
        private readonly int currentUserId;
        private DataTable? hotlineData;

        public SystemManagement()
            : this(string.Empty, string.Empty, 0)
        {
        }

        public SystemManagement(string currentUserFullName, string currentUserRole, int currentUserId)
        {
            InitializeComponent();
            this.currentUserRole = currentUserRole;
            this.currentUserId = currentUserId;
            ConfigureForm();
            Load += SystemManagement_Load;
        }

        private void ConfigureForm()
        {
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            button3.Click += button3_Click;
            button4.Click += button4_Click;
            button5.Click += button5_Click;
            button6.Click += button6_Click;
            button10.Click += button10_Click;
            textBox16.TextChanged += textBox16_TextChanged;

            ConfigureMiddleInitialTextBox(textBox3);
            ConfigureMiddleInitialTextBox(textBox10);

            textBox4.UseSystemPasswordChar = true;
            textBox5.UseSystemPasswordChar = true;
            textBox6.UseSystemPasswordChar = true;
            textBox11.UseSystemPasswordChar = true;
            textBox12.UseSystemPasswordChar = true;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.RowHeadersVisible = false;

            Column1.HeaderText = "Full Name";
            Column2.HeaderText = "Role";

            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.ReadOnly = true;
            dataGridView2.MultiSelect = false;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.RowHeadersVisible = false;

            dataGridView3.AllowUserToAddRows = false;
            dataGridView3.AllowUserToDeleteRows = false;
            dataGridView3.ReadOnly = true;
            dataGridView3.MultiSelect = false;
            dataGridView3.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView3.RowHeadersVisible = false;

            dataGridView4.AllowUserToAddRows = false;
            dataGridView4.AllowUserToDeleteRows = false;
            dataGridView4.ReadOnly = true;
            dataGridView4.MultiSelect = false;
            dataGridView4.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView4.AutoGenerateColumns = false;
            dataGridView4.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView4.RowHeadersVisible = false;

            dateTimePicker1.Value = DateTime.Today.AddDays(-30);
            dateTimePicker2.Value = DateTime.Today;
        }

        private void SystemManagement_Load(object? sender, EventArgs e)
        {
            BindCurrentUserInfo();
            LoadCurrentUserNameFields();
            ConfigureAdminFeaturesVisibility();
            LoadUserLogs();
            LoadHotlineNumbers();
            LoadPurokGrid();
            LoadRaArticles();
            LoadVawcHandbook();
        }

        private void BindCurrentUserInfo()
        {
            label6.Text = string.IsNullOrWhiteSpace(currentUserRole) ? "N/A" : currentUserRole;
            label7.Text = currentUserId > 0 ? currentUserId.ToString() : "N/A";
        }

        private void LoadCurrentUserNameFields()
        {
            if (currentUserId <= 0)
            {
                label5.Text = "N/A";
                return;
            }

            try
            {
                const string query = """
                    SELECT lastname, firstname, middlename, position
                    FROM users
                    WHERE userId = @userId
                    LIMIT 1;
                    """;

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                using MySqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@userId", currentUserId);

                using MySqlDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    label5.Text = "N/A";
                    return;
                }

                string lastName = Convert.ToString(reader["lastname"])?.Trim() ?? string.Empty;
                string firstName = Convert.ToString(reader["firstname"])?.Trim() ?? string.Empty;
                string middleName = Convert.ToString(reader["middlename"])?.Trim() ?? string.Empty;

                textBox1.Text = lastName;
                textBox2.Text = firstName;
                textBox3.Text = NormalizeMiddleInitial(middleName);

                label5.Text = BuildDisplayFullName(firstName, middleName, lastName);
                label6.Text = Convert.ToString(reader["position"])?.Trim() ?? label6.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load the current user information.{Environment.NewLine}{ex.Message}",
                    "Load Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigureAdminFeaturesVisibility()
        {
            bool isAdmin = string.Equals(currentUserRole, "Admin", StringComparison.OrdinalIgnoreCase);

            panel1.Visible = isAdmin;

            if (!isAdmin)
            {
                if (guna2TabControl1.TabPages.Contains(tabPage3))
                {
                    guna2TabControl1.TabPages.Remove(tabPage3);
                }

                return;
            }

            LoadUsersGrid();
        }

        private void LoadUsersGrid()
        {
            try
            {
                const string query = """
                    SELECT firstname, middlename, lastname, position
                    FROM users
                    ORDER BY lastname, firstname, middlename;
                    """;

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                using MySqlCommand command = new(query, connection);
                using MySqlDataAdapter adapter = new(command);

                DataTable usersTable = new();
                adapter.Fill(usersTable);

                dataGridView1.Rows.Clear();

                foreach (DataRow row in usersTable.Rows)
                {
                    int rowIndex = dataGridView1.Rows.Add();

                    string firstName = Convert.ToString(row["firstname"])?.Trim() ?? string.Empty;
                    string middleName = Convert.ToString(row["middlename"])?.Trim() ?? string.Empty;
                    string lastName = Convert.ToString(row["lastname"])?.Trim() ?? string.Empty;
                    string role = Convert.ToString(row["position"])?.Trim() ?? string.Empty;

                    dataGridView1.Rows[rowIndex].Cells["Column1"].Value = BuildDisplayFullName(firstName, middleName, lastName);
                    dataGridView1.Rows[rowIndex].Cells["Column2"].Value = role;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load user accounts.{Environment.NewLine}{ex.Message}",
                    "Load Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void button1_Click(object? sender, EventArgs e)
        {
            string lastName = textBox1.Text.Trim();
            string firstName = textBox2.Text.Trim();
            string middleInitial = NormalizeMiddleInitial(textBox3.Text);

            if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(middleInitial))
            {
                MessageBox.Show(
                    "Please complete the last name, first name, and middle initial.",
                    "Incomplete Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmation = MessageBox.Show(
                "Are you sure you entered the correct info?",
                "Confirm Save",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            try
            {
                const string query = """
                    UPDATE users
                    SET lastname = @lastname, firstname = @firstname, middlename = @middlename
                    WHERE userId = @userId;
                    """;

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                using MySqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@lastname", lastName);
                command.Parameters.AddWithValue("@firstname", firstName);
                command.Parameters.AddWithValue("@middlename", middleInitial);
                command.Parameters.AddWithValue("@userId", currentUserId);
                command.ExecuteNonQuery();

                textBox3.Text = middleInitial;
                label5.Text = BuildDisplayFullName(firstName, middleInitial, lastName);

                if (panel1.Visible)
                {
                    LoadUsersGrid();
                }

                MessageBox.Show(
                    "Account information updated successfully.",
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to save the account information.{Environment.NewLine}{ex.Message}",
                    "Save Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog dialog = new();
            dialog.Filter = "SQL Files (*.sql)|*.sql";
            dialog.FileName = $"vawcmanagementsystem_backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                MySqlConnectionStringBuilder connectionBuilder = new(DbConnectionFactory.CreateConnection().ConnectionString);
                string arguments =
                    $"--host={connectionBuilder.Server} " +
                    $"--port={connectionBuilder.Port} " +
                    $"--user={connectionBuilder.UserID} " +
                    $"--password={connectionBuilder.Password} " +
                    "--routines --triggers " +
                    $"--databases {connectionBuilder.Database}";

                using Process process = new();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "mysqldump",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                process.Start();

                string dumpContent = process.StandardOutput.ReadToEnd();
                string errorOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        string.IsNullOrWhiteSpace(errorOutput)
                            ? "mysqldump failed to create the backup."
                            : errorOutput.Trim());
                }

                File.WriteAllText(dialog.FileName, dumpContent, Encoding.UTF8);

                MessageBox.Show(
                    $"Backup completed successfully.{Environment.NewLine}{dialog.FileName}",
                    "Backup Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Backup failed. Make sure `mysqldump` is available in your MySQL installation or PATH." +
                    Environment.NewLine + Environment.NewLine +
                    ex.Message,
                    "Backup Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object? sender, EventArgs e)
        {
            if (currentUserId <= 0)
            {
                MessageBox.Show(
                    "No logged-in user was found for this password change request.",
                    "Change Password",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string oldPassword = textBox4.Text;
            string newPassword = textBox5.Text;
            string confirmPassword = textBox6.Text;

            if (string.IsNullOrWhiteSpace(oldPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show(
                    "Please complete the old password, new password, and confirm password fields.",
                    "Incomplete Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                MessageBox.Show(
                    "The new password and confirmation password do not match.",
                    "Password Mismatch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                textBox6.SelectAll();
                textBox6.Focus();
                return;
            }

            try
            {
                const string selectQuery = """
                    SELECT password
                    FROM users
                    WHERE userId = @userId
                    LIMIT 1;
                    """;

                const string updateQuery = """
                    UPDATE users
                    SET password = @password
                    WHERE userId = @userId;
                    """;

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                string storedPassword = string.Empty;

                using (MySqlCommand selectCommand = new(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@userId", currentUserId);
                    object? result = selectCommand.ExecuteScalar();
                    storedPassword = Convert.ToString(result) ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(storedPassword))
                {
                    MessageBox.Show(
                        "Unable to verify the current password for this account.",
                        "Change Password",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (!PasswordSecurity.Matches(oldPassword, storedPassword))
                {
                    MessageBox.Show(
                        "The old password you entered is incorrect.",
                        "Invalid Password",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    textBox4.SelectAll();
                    textBox4.Focus();
                    return;
                }

                using (MySqlCommand updateCommand = new(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@password", PasswordSecurity.HashPassword(newPassword));
                    updateCommand.Parameters.AddWithValue("@userId", currentUserId);
                    updateCommand.ExecuteNonQuery();
                }

                textBox4.Clear();
                textBox5.Clear();
                textBox6.Clear();

                MessageBox.Show(
                    "Your password has been changed successfully.",
                    "Password Updated",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to change the password.{Environment.NewLine}{ex.Message}",
                    "Change Password Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object? sender, EventArgs e)
        {
            if (!string.Equals(currentUserRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Only Admin users are allowed to create new user accounts.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string username = textBox7.Text.Trim();
            string lastName = textBox8.Text.Trim();
            string firstName = textBox9.Text.Trim();
            string middleInitial = NormalizeMiddleInitial(textBox10.Text);
            string role = comboBox1.Text.Trim();
            string password = textBox11.Text;
            string confirmPassword = textBox12.Text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(middleInitial) ||
                string.IsNullOrWhiteSpace(role) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show(
                    "Please complete all fields before saving the new user.",
                    "Incomplete Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, "Secretary", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Please choose either Admin or Secretary for the new user role.",
                    "Invalid Role",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                comboBox1.Focus();
                return;
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                MessageBox.Show(
                    "The password and confirmation password do not match.",
                    "Password Mismatch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                textBox12.SelectAll();
                textBox12.Focus();
                return;
            }

            try
            {
                const string existsQuery = """
                    SELECT COUNT(*)
                    FROM users
                    WHERE username = @username;
                    """;

                const string insertQuery = """
                    INSERT INTO users (username, password, lastname, firstname, middlename, position)
                    VALUES (@username, @password, @lastname, @firstname, @middlename, @position);
                    """;

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                using (MySqlCommand existsCommand = new(existsQuery, connection))
                {
                    existsCommand.Parameters.AddWithValue("@username", username);
                    int existingUsers = Convert.ToInt32(existsCommand.ExecuteScalar());

                    if (existingUsers > 0)
                    {
                        MessageBox.Show(
                            "That username is already being used. Please choose a different username.",
                            "Duplicate Username",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        textBox7.SelectAll();
                        textBox7.Focus();
                        return;
                    }
                }

                using (MySqlCommand insertCommand = new(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@username", username);
                    insertCommand.Parameters.AddWithValue("@password", PasswordSecurity.HashPassword(password));
                    insertCommand.Parameters.AddWithValue("@lastname", lastName);
                    insertCommand.Parameters.AddWithValue("@firstname", firstName);
                    insertCommand.Parameters.AddWithValue("@middlename", middleInitial);
                    insertCommand.Parameters.AddWithValue("@position", role);
                    insertCommand.ExecuteNonQuery();
                }

                ClearNewUserFields();
                LoadUsersGrid();

                MessageBox.Show(
                    "The new user account was created successfully.",
                    "User Created",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to create the new user account.{Environment.NewLine}{ex.Message}",
                    "Create Account Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object? sender, EventArgs e)
        {
            try
            {
                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                List<string> optimizedTables = new();

                using MySqlCommand tablesCommand = new("SHOW TABLES;", connection);
                using MySqlDataReader reader = tablesCommand.ExecuteReader();

                List<string> tableNames = new();

                while (reader.Read())
                {
                    string tableName = Convert.ToString(reader.GetValue(0)) ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(tableName))
                    {
                        tableNames.Add(tableName);
                    }
                }

                reader.Close();

                foreach (string tableName in tableNames)
                {
                    string escapedTableName = tableName.Replace("`", "``");
                    using MySqlCommand optimizeCommand = new($"OPTIMIZE TABLE `{escapedTableName}`;", connection);
                    optimizeCommand.ExecuteNonQuery();
                    optimizedTables.Add(tableName);
                }

                MessageBox.Show(
                    "Optimization completed for the following tables:" +
                    Environment.NewLine + Environment.NewLine +
                    string.Join(Environment.NewLine, optimizedTables.Select(table => $"- {table}")),
                    "Optimization Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Optimization failed.{Environment.NewLine}{ex.Message}",
                    "Optimization Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void textBox3_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            if (!char.IsLetter(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button6_Click(object? sender, EventArgs e)
        {
            LoadUserLogs(true);
        }

        private void button10_Click(object? sender, EventArgs e)
        {
            if (!string.Equals(currentUserRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Only Admin users are allowed to add new purok entries.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string purokName = textBox17.Text.Trim();

            if (string.IsNullOrWhiteSpace(purokName))
            {
                MessageBox.Show(
                    "Please enter the purok name before saving.",
                    "Incomplete Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                textBox17.Focus();
                return;
            }

            try
            {
                const string existsQuery = """
                    SELECT COUNT(*)
                    FROM purok
                    WHERE LOWER(TRIM(purok_name)) = LOWER(TRIM(@purokName));
                    """;

                const string insertQuery = """
                    INSERT INTO purok (purok_name)
                    VALUES (@purokName);
                    """;

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                using (MySqlCommand existsCommand = new(existsQuery, connection))
                {
                    existsCommand.Parameters.AddWithValue("@purokName", purokName);
                    int existingCount = Convert.ToInt32(existsCommand.ExecuteScalar());

                    if (existingCount > 0)
                    {
                        MessageBox.Show(
                            "That purok name already exists.",
                            "Duplicate Purok",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        textBox17.SelectAll();
                        textBox17.Focus();
                        return;
                    }
                }

                using (MySqlCommand insertCommand = new(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@purokName", purokName);
                    insertCommand.ExecuteNonQuery();
                }

                textBox17.Clear();
                LoadPurokGrid(true);

                MessageBox.Show(
                    "The purok was added successfully.",
                    "Purok Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to save the purok name.{Environment.NewLine}{ex.Message}",
                    "Save Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void textBox16_TextChanged(object? sender, EventArgs e)
        {
            FilterHotlineNumbers();
        }

        private void LoadUserLogs(bool showErrors = false)
        {
            try
            {
                string usernameLike = textBox13.Text.Trim();
                DateTime from = dateTimePicker1.Value.Date;
                DateTime to = dateTimePicker2.Value.Date.AddDays(1).AddTicks(-1);

                if (from > to)
                {
                    DateTime swap = from;
                    from = dateTimePicker2.Value.Date;
                    to = dateTimePicker1.Value.Date.AddDays(1).AddTicks(-1);
                }

                StringBuilder queryBuilder = new();
                queryBuilder.Append("""
                    SELECT log_id, username, action, entity_type, entity_id, description, timestamp
                    FROM user_logs
                    WHERE timestamp >= @from AND timestamp <= @to
                    """);

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                using MySqlCommand command = new(queryBuilder.ToString(), connection);
                command.Parameters.AddWithValue("@from", from);
                command.Parameters.AddWithValue("@to", to);

                if (!string.IsNullOrWhiteSpace(usernameLike))
                {
                    queryBuilder.Append(" AND username LIKE @username");
                    command.CommandText = queryBuilder.ToString();
                    command.Parameters.AddWithValue("@username", $"%{usernameLike}%");
                }

                queryBuilder.Append(" ORDER BY timestamp DESC");
                command.CommandText = queryBuilder.ToString();

                using MySqlDataAdapter adapter = new(command);
                DataTable logsTable = new();
                adapter.Fill(logsTable);

                dataGridView2.DataSource = logsTable;
            }
            catch (Exception ex)
            {
                if (!showErrors)
                {
                    return;
                }

                MessageBox.Show(
                    $"Unable to load the user logs.{Environment.NewLine}{ex.Message}",
                    "User Logs",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void LoadHotlineNumbers(bool showErrors = false)
        {
            try
            {
                const string query = """
                    SELECT hotline_id AS 'ID', agency_name AS 'Name', phone_number AS 'Contact Number'
                    FROM hotline_numbers
                    ORDER BY agency_name;
                    """;

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                using MySqlCommand command = new(query, connection);
                using MySqlDataAdapter adapter = new(command);

                hotlineData = new DataTable();
                adapter.Fill(hotlineData);
                FilterHotlineNumbers();
            }
            catch (Exception ex)
            {
                hotlineData = null;
                dataGridView3.DataSource = null;

                if (!showErrors)
                {
                    return;
                }

                MessageBox.Show(
                    $"Unable to load hotline numbers.{Environment.NewLine}{ex.Message}",
                    "Hotline Number",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void LoadPurokGrid(bool showErrors = false)
        {
            try
            {
                const string query = """
                    SELECT purok_name
                    FROM purok
                    WHERE purok_name IS NOT NULL AND TRIM(purok_name) <> ''
                    ORDER BY purok_id;
                    """;

                using MySqlConnection connection = DbConnectionFactory.CreateConnection();
                connection.Open();

                using MySqlCommand command = new(query, connection);
                using MySqlDataReader reader = command.ExecuteReader();

                dataGridView4.Rows.Clear();

                while (reader.Read())
                {
                    int rowIndex = dataGridView4.Rows.Add();
                    dataGridView4.Rows[rowIndex].Cells["Column3"].Value = reader.GetString("purok_name");
                }
            }
            catch (Exception ex)
            {
                dataGridView4.Rows.Clear();

                if (!showErrors)
                {
                    return;
                }

                MessageBox.Show(
                    $"Unable to load purok data.{Environment.NewLine}{ex.Message}",
                    "Purok Load Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void FilterHotlineNumbers()
        {
            if (hotlineData == null)
            {
                dataGridView3.DataSource = null;
                return;
            }

            string searchText = textBox16.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                dataGridView3.DataSource = hotlineData;
                return;
            }

            string normalizedSearch = searchText.ToLowerInvariant();

            var filteredRows = hotlineData.AsEnumerable()
                .Where(row =>
                    row.ItemArray.Any(value =>
                        Convert.ToString(value)?.ToLowerInvariant().Contains(normalizedSearch) == true));

            dataGridView3.DataSource = filteredRows.Any()
                ? filteredRows.CopyToDataTable()
                : hotlineData.Clone();
        }

        private void LoadRaArticles()
        {
            LoadArticleIntoPanel(panel2, "ra9262.txt");
            LoadArticleIntoPanel(panel3, "ra9208.txt");
            LoadArticleIntoPanel(panel4, "ra8353.txt");
            LoadArticleIntoPanel(panel5, "ra7877.txt");
            LoadArticleIntoPanel(panel6, "ra10364.txt");
        }

        private async void LoadVawcHandbook()
        {
            panel7.Controls.Clear();
            panel7.AutoScroll = false;

            string? handbookPath = ResolveArticlePath("Barangay-VAW-Desk-Handbook.pdf");

            if (handbookPath == null)
            {
                ShowHandbookMessage("Unable to find the handbook PDF file.");
                return;
            }

            try
            {
                Button openButton = new()
                {
                    Text = "Open Handbook PDF",
                    Width = 190,
                    Height = 40,
                    Top = 70,
                    Left = 20
                };

                openButton.Click += (_, _) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = handbookPath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Unable to open the handbook PDF.{Environment.NewLine}{ex.Message}",
                            "VAWC Handbook",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                };

                Panel buttonPanel = new()
                {
                    Dock = DockStyle.Top,
                    Height = 70
                };

                Label titleLabel = new()
                {
                    AutoSize = false,
                    Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold),
                    Text = "Barangay VAW Desk Handbook",
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 56
                };

                WebView2 handbookViewer = new()
                {
                    Dock = DockStyle.Fill,
                    BackColor = System.Drawing.Color.White
                };

                buttonPanel.Controls.Add(openButton);

                panel7.Controls.Add(handbookViewer);
                panel7.Controls.Add(buttonPanel);
                panel7.Controls.Add(titleLabel);

                await handbookViewer.EnsureCoreWebView2Async();
                handbookViewer.Source = new Uri(handbookPath);
            }
            catch (Exception ex)
            {
                ShowHandbookMessage(
                    "Unable to prepare the handbook viewer." +
                    Environment.NewLine + Environment.NewLine +
                    ex.Message);
            }
        }

        private void LoadArticleIntoPanel(Panel targetPanel, string articleFileName)
        {
            targetPanel.Controls.Clear();
            targetPanel.AutoScroll = true;

            RichTextBox articleViewer = new()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Font = new System.Drawing.Font("Arial", 10.5F),
                BackColor = System.Drawing.Color.White,
                DetectUrls = true
            };

            string? articlePath = ResolveArticlePath(articleFileName);

            if (articlePath == null)
            {
                articleViewer.Text = $"Unable to find the article file: {articleFileName}";
                targetPanel.Controls.Add(articleViewer);
                return;
            }

            try
            {
                articleViewer.Text = File.ReadAllText(articlePath);
            }
            catch (Exception ex)
            {
                articleViewer.Text = $"Unable to load the article file.{Environment.NewLine}{Environment.NewLine}{ex.Message}";
            }

            targetPanel.Controls.Add(articleViewer);
        }

        private static string? ResolveArticlePath(string articleFileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo? currentDirectory = new(baseDirectory);

            while (currentDirectory != null)
            {
                string candidatePath = Path.Combine(currentDirectory.FullName, "Articles", articleFileName);

                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }

                currentDirectory = currentDirectory.Parent;
            }

            return null;
        }

        private void ShowHandbookMessage(string message)
        {
            TextBox messageBox = new()
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Font = new System.Drawing.Font("Arial", 10.5F),
                BackColor = System.Drawing.Color.White,
                Text = message
            };

            panel7.Controls.Add(messageBox);
        }

        private void textBox3_TextChanged(object? sender, EventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            string normalizedValue = NormalizeMiddleInitial(textBox.Text);

            if (textBox.Text == normalizedValue)
            {
                return;
            }

            int selectionStart = textBox.SelectionStart;
            textBox.Text = normalizedValue;
            textBox.SelectionStart = Math.Min(selectionStart, textBox.Text.Length);
        }

        private void ConfigureMiddleInitialTextBox(TextBox textBox)
        {
            textBox.CharacterCasing = CharacterCasing.Upper;
            textBox.MaxLength = 1;
            textBox.KeyPress += textBox3_KeyPress;
            textBox.TextChanged += textBox3_TextChanged;
        }

        private void ClearNewUserFields()
        {
            textBox7.Clear();
            textBox8.Clear();
            textBox9.Clear();
            textBox10.Clear();
            comboBox1.SelectedIndex = -1;
            textBox11.Clear();
            textBox12.Clear();
        }

        private static string NormalizeMiddleInitial(string value)
        {
            char middleInitial = value
                .Where(char.IsLetter)
                .Select(char.ToUpperInvariant)
                .FirstOrDefault();

            return middleInitial == default ? string.Empty : middleInitial.ToString();
        }

        private static string BuildDisplayFullName(string firstName, string middleName, string lastName)
        {
            string normalizedMiddle = NormalizeMiddleInitial(middleName);

            return string.Join(
                " ",
                new[] { firstName.Trim(), normalizedMiddle, lastName.Trim() }
                    .Where(part => !string.IsNullOrWhiteSpace(part)));
        }
    }
}
