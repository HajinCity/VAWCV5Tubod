using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using VAWCV5Tubod.Connection;

namespace VAWCV5Tubod
{
    public partial class ForgotPasswordChangePass : Form
    {
        public ForgotPasswordChangePass()
        {
            InitializeComponent();
            ConfigureForm();
        }

        private void ConfigureForm()
        {
            LoginButton.Click += LoginButton_Click;
            AcceptButton = LoginButton;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "Please enter a username first.",
                    "Missing Username",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!TryValidateSecretaryUsername(username, out string errorMessage))
                {
                    UserLogService.Log(
                        username,
                        "ForgotPasswordDenied",
                        "users",
                        0,
                        errorMessage);

                    MessageBox.Show(
                        errorMessage,
                        "Forgot Password",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                using ForgotPasswordEntry forgotPasswordEntry = new(username);
                Hide();
                forgotPasswordEntry.ShowDialog(this);
                Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to continue the forgot-password request.{Environment.NewLine}{ex.Message}",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static bool TryValidateSecretaryUsername(string username, out string errorMessage)
        {
            const string query = """
                SELECT position
                FROM users
                WHERE username = @username
                LIMIT 1;
                """;

            using MySqlConnection connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            using MySqlCommand command = new(query, connection);
            command.Parameters.Add("@username", MySqlDbType.VarChar).Value = username;

            object? result = command.ExecuteScalar();

            if (result is null || result == DBNull.Value)
            {
                errorMessage = "The username you entered was not found.";
                return false;
            }

            string position = Convert.ToString(result)?.Trim() ?? string.Empty;

            if (!string.Equals(position, "Secretary", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Only users with the Secretary role can reset a password here.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
