using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using VAWCV5Tubod.Connection;

namespace VAWCV5Tubod
{
    public partial class ForgotPasswordEntry : Form
    {
        private bool isNewPasswordVisible;
        private bool isConfirmPasswordVisible;

        public ForgotPasswordEntry()
            : this(string.Empty)
        {
        }

        public ForgotPasswordEntry(string username)
        {
            InitializeComponent();
            textBox1.Text = username;
            ConfigureForm();
        }

        private void ConfigureForm()
        {
            isNewPasswordVisible = false;
            isConfirmPasswordVisible = false;

            ApplyPasswordVisibility();

            pictureBox1.Cursor = Cursors.Hand;
            pictureBox2.Cursor = Cursors.Hand;
            pictureBox3.Cursor = Cursors.Hand;

            pictureBox1.Click += pictureBox1_Click;
            pictureBox2.Click += pictureBox2_Click;
            pictureBox3.Click += pictureBox3_Click;
            LoginButton.Click += LoginButton_Click;
            AcceptButton = LoginButton;
        }

        private void ApplyPasswordVisibility()
        {
            textBox2.UseSystemPasswordChar = !isNewPasswordVisible;
            textBox3.UseSystemPasswordChar = !isConfirmPasswordVisible;

            pictureBox2.Image = isNewPasswordVisible
                ? Properties.Resources.Eye
                : Properties.Resources.Closed_Eye;

            pictureBox3.Image = isConfirmPasswordVisible
                ? Properties.Resources.Eye
                : Properties.Resources.Closed_Eye;
        }

        private void pictureBox1_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox2_Click(object? sender, EventArgs e)
        {
            isNewPasswordVisible = !isNewPasswordVisible;
            ApplyPasswordVisibility();
        }

        private void pictureBox3_Click(object? sender, EventArgs e)
        {
            isConfirmPasswordVisible = !isConfirmPasswordVisible;
            ApplyPasswordVisibility();
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string newPassword = textBox2.Text;
            string confirmPassword = textBox3.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "Username is required.",
                    "Missing Username",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show(
                    "Please enter and confirm the new password.",
                    "Missing Password",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                MessageBox.Show(
                    "The password confirmation does not match.",
                    "Password Mismatch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                textBox3.SelectAll();
                textBox3.Focus();
                return;
            }

            try
            {
                if (!UpdatePassword(username, newPassword))
                {
                    MessageBox.Show(
                        "The username you entered was not found.",
                        "Update Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                UserLogService.Log(
                    username,
                    "ForgotPassword",
                    "users",
                    0,
                    $"Password reset through forgot password for {username}.");

                MessageBox.Show(
                    "The password was changed successfully.",
                    "Password Updated",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to change the password.{Environment.NewLine}{ex.Message}",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static bool UpdatePassword(string username, string newPassword)
        {
            const string query = """
                UPDATE users
                SET password = @password
                WHERE username = @username AND position = 'Secretary';
                """;

            using MySqlConnection connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            using MySqlCommand command = new(query, connection);
            command.Parameters.Add("@password", MySqlDbType.VarChar).Value = PasswordSecurity.HashPassword(newPassword);
            command.Parameters.Add("@username", MySqlDbType.VarChar).Value = username;

            return command.ExecuteNonQuery() > 0;
        }        
    }
}
