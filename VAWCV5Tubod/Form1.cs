using VAWCV5Tubod.Connection;
using VAWCV5Tubod.Domain;

namespace VAWCV5Tubod
{
    public partial class Form1 : Form
    {
        private readonly AuthenticationService authenticationService = new();
        private readonly RememberedLoginStore rememberedLoginStore = new();
        private bool isPasswordVisible;
        private bool isLoadingRememberedLogin;

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
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            Load += Form1_Load;
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

        private async void Form1_Load(object? sender, EventArgs e)
        {
            await RestoreRememberedLoginAsync();
        }

        private async Task RestoreRememberedLoginAsync()
        {
            RememberedLogin? rememberedLogin = rememberedLoginStore.Load();

            if (rememberedLogin is null)
            {
                return;
            }

            isLoadingRememberedLogin = true;
            checkBox1.Checked = true;
            textBox1.Text = rememberedLogin.Username;
            isLoadingRememberedLogin = false;

            ToggleLoginControls(false);

            try
            {
                Users? rememberedUser = await authenticationService.GetRememberedUserAsync(
                    rememberedLogin.UserId,
                    rememberedLogin.Username);

                if (rememberedUser is null)
                {
                    rememberedLoginStore.Clear();
                    checkBox1.Checked = false;
                    textBox1.Clear();
                    ToggleLoginControls(true);
                    return;
                }

                UserLogService.Log(
                    rememberedUser.Username,
                    "Login",
                    "users",
                    rememberedUser.UserId,
                    "Restored remembered login session.");

                OpenDashboard(rememberedUser);
            }
            catch
            {
                ToggleLoginControls(true);
                textBox2.Focus();
            }
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
                    UserLogService.Log(
                        username,
                        "LoginFailed",
                        "users",
                        0,
                        "Failed login attempt.");

                    MessageBox.Show(
                        "Invalid username or password. Please try again.",
                        "Login Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    textBox2.SelectAll();
                    textBox2.Focus();
                    return;
                }

                if (checkBox1.Checked)
                {
                    rememberedLoginStore.Save(user.UserId, user.Username);
                }
                else
                {
                    rememberedLoginStore.Clear();
                }

                UserLogService.Log(
                    user.Username,
                    "Login",
                    "users",
                    user.UserId,
                    "Logged in to the system.");

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
            checkBox1.Enabled = enabled;
            LoginButton.Enabled = enabled;
            UseWaitCursor = !enabled;
        }

        private void checkBox1_CheckedChanged(object? sender, EventArgs e)
        {
            if (!isLoadingRememberedLogin && !checkBox1.Checked)
            {
                rememberedLoginStore.Clear();
            }
        }

        private void OpenDashboard(Users user)
        {
            string middleInitial = NormalizeMiddleInitial(user.MiddleName);
            string fullName = string.Join(
                " ",
                new[] { user.FirstName.Trim(), middleInitial, user.LastName.Trim() }
                    .Where(part => !string.IsNullOrWhiteSpace(part)));

            LandingForm dashboard = new(fullName, user.Position, user.UserId, user.Username);
            dashboard.WindowState = FormWindowState.Maximized;
            dashboard.FormClosed += (_, _) =>
            {
                if (dashboard.IsLoggingOut)
                {
                    rememberedLoginStore.Clear();
                    checkBox1.Checked = false;
                    textBox2.Clear();
                    ToggleLoginControls(true);
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
