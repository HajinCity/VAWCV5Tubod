using VAWCV5Tubod.Connection;
using VAWCV5Tubod.Domain;

namespace VAWCV5Tubod
{
    public partial class Form1 : Form
    {
        private readonly AuthenticationService authenticationService = new();
        private bool isPasswordVisible;

        public Form1()
        {
            InitializeComponent();
            ConfigureLoginForm();
        }

        private void ConfigureLoginForm()
        {
            isPasswordVisible = false;
            ApplyPasswordVisibility();

            pictureBox2.Cursor = Cursors.Hand;
            pictureBox2.Click += pictureBox2_Click;
            fpWindows.Cursor = Cursors.Hand;
            fpWindows.Click += fpWindows_Click;
            LoginButton.Click += LoginButton_Click;
            AcceptButton = LoginButton;
        }

        private void ApplyPasswordVisibility()
        {
            textBox2.UseSystemPasswordChar = !isPasswordVisible;
            pictureBox2.Image = isPasswordVisible
                ? Properties.Resources.Eye
                : Properties.Resources.Closed_Eye;
        }

        private void pictureBox2_Click(object? sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            ApplyPasswordVisibility();
        }

        private async void LoginButton_Click(object? sender, EventArgs e)
        {
            await LoginAsync();
        }

        private async Task LoginAsync()
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Please enter both your username and password.",
                    "Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            ToggleLoginControls(false);

            try
            {
                var user = await authenticationService.AuthenticateAsync(username, password);

                if (user == null)
                {
                    MessageBox.Show(
                        "Invalid username or password. Please try again.",
                        "Login Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    textBox2.SelectAll();
                    textBox2.Focus();
                    return;
                }

                OpenDashboard(user);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Database Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to complete the login request.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                {
                    ToggleLoginControls(true);
                }
            }
        }

        private void ToggleLoginControls(bool enabled)
        {
            textBox1.Enabled = enabled;
            textBox2.Enabled = enabled;
            pictureBox2.Enabled = enabled;
            LoginButton.Enabled = enabled;
            UseWaitCursor = !enabled;
        }

        private void OpenDashboard(Users user)
        {
            string middleInitial = NormalizeMiddleInitial(user.MiddleName);
            string fullName = string.Join(
                " ",
                new[] { user.FirstName.Trim(), middleInitial, user.LastName.Trim() }
                    .Where(part => !string.IsNullOrWhiteSpace(part)));

            LandingForm dashboard = new(fullName, user.Position, user.UserId);
            dashboard.WindowState = FormWindowState.Maximized;
            dashboard.FormClosed += (_, _) =>
            {
                if (dashboard.IsLoggingOut)
                {
                    textBox2.Clear();
                    Show();
                    WindowState = FormWindowState.Normal;
                    Activate();
                    textBox1.Focus();
                    return;
                }

                Close();
            };
            dashboard.Show();
            Hide();
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void fpWindows_Click(object? sender, EventArgs e)
        {
            using ForgotPasswordChangePass forgotPasswordChangePass = new();
            forgotPasswordChangePass.ShowDialog(this);
        }

        private static string NormalizeMiddleInitial(string value)
        {
            char middleInitial = value
                .Where(char.IsLetter)
                .Select(char.ToUpperInvariant)
                .FirstOrDefault();

            return middleInitial == default ? string.Empty : middleInitial.ToString();
        }
    }
}
