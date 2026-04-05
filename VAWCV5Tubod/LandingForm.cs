using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VAWCV5Tubod
{
    public partial class LandingForm : Form
    {
        private Form? activeForm;
        private readonly string currentUserFullName;
        private readonly string currentUserPosition;
        private readonly int currentUserId;
        public bool IsLoggingOut { get; private set; }

        public LandingForm(string fullName, string position, int userId)
        {
            InitializeComponent();
            currentUserFullName = fullName;
            currentUserPosition = position;
            currentUserId = userId;
            label1.Text = fullName;
            label2.Text = position;
            openingForm(new Dashboard());
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
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
            openingForm(new FileACase());
        }

        private void DashboardBtn_Click_1(object sender, EventArgs e)
        {
            openingForm(new Dashboard());
        }

        private void CaseListBtn_Click_1(object sender, EventArgs e)
        {
            openingForm(new CaseList(currentUserPosition));
        }

        private void ReportsBtn_Click_1(object sender, EventArgs e)
        {
            openingForm(new Documents());
        }

        private void SystemBtn_Click_1(object sender, EventArgs e)
        {
            openingForm(new SystemManagement(currentUserFullName, currentUserPosition, currentUserId));
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
