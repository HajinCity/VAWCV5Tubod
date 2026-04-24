using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VAWCV5Tubod.Connection;

namespace VAWCV5Tubod
{
    public partial class LandingForm : Form
    {
        private Form? activeForm;
        private readonly string currentUserFullName;
        private readonly string currentUserPosition;
        private readonly string currentUsername;
        private readonly int currentUserId;
        public bool IsLoggingOut { get; private set; }

        public LandingForm(string fullName, string position, int userId, string username = "")
        {
            InitializeComponent();
            currentUserFullName = fullName;
            currentUserPosition = position;
            currentUsername = username;
            currentUserId = userId;
            label1.Text = fullName;
            label2.Text = position;
            openingForm(new Dashboard(currentUserFullName, currentUsername));
        }

        

        private void openingForm(Form childForm)
        {
            if (activeForm != null)
                activeForm.Close();

            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panel3.Controls.Add(childForm);
            panel3.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();

        }

        private void FileACaseBtn_Click_1(object sender, EventArgs e)
        {
            openingForm(new FileACase(currentUsername));
        }

        private void DashboardBtn_Click_1(object sender, EventArgs e)
        {
            openingForm(new Dashboard(currentUserFullName, currentUsername));
        }

        private void CaseListBtn_Click_1(object sender, EventArgs e)
        {
            openingForm(new CaseList(currentUserPosition, currentUsername));
        }

        private void ReportsBtn_Click_1(object sender, EventArgs e)
        {
            openingForm(new Documents(currentUserFullName, currentUsername));
        }

        private void SystemBtn_Click_1(object sender, EventArgs e)
        {
            openingForm(new SystemManagement(currentUserFullName, currentUserPosition, currentUserId, currentUsername));
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to log out?",
                "Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            IsLoggingOut = true;
            UserLogService.Log(
                currentUsername,
                "Logout",
                "users",
                currentUserId,
                "Logged out of the system.");

            if (activeForm != null && !activeForm.IsDisposed)
            {
                activeForm.Close();
                activeForm = null;
            }

            Close();
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
