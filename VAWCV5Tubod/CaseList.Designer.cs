namespace VAWCV5Tubod
{
    partial class CaseList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            label1 = new Label();
            guna2GradientPanel1 = new Guna.UI2.WinForms.Guna2GradientPanel();
            textBox1 = new TextBox();
            label40 = new Label();
            panel1 = new Panel();
            dataGridView1 = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            btnView = new DataGridViewImageColumn();
            btnEdit = new DataGridViewImageColumn();
            guna2GradientPanel1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 26.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(149, 41);
            label1.TabIndex = 3;
            label1.Text = "Caselist";
            // 
            // guna2GradientPanel1
            // 
            guna2GradientPanel1.BorderRadius = 15;
            guna2GradientPanel1.Controls.Add(textBox1);
            guna2GradientPanel1.Controls.Add(label40);
            guna2GradientPanel1.CustomizableEdges = customizableEdges1;
            guna2GradientPanel1.FillColor = Color.FromArgb(255, 214, 255);
            guna2GradientPanel1.FillColor2 = Color.FromArgb(200, 182, 255);
            guna2GradientPanel1.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            guna2GradientPanel1.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            guna2GradientPanel1.Location = new Point(12, 80);
            guna2GradientPanel1.Name = "guna2GradientPanel1";
            guna2GradientPanel1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2GradientPanel1.Size = new Size(1112, 125);
            guna2GradientPanel1.TabIndex = 9;
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Arial", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox1.Location = new Point(393, 44);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(477, 25);
            textBox1.TabIndex = 49;
            // 
            // label40
            // 
            label40.AutoSize = true;
            label40.BackColor = Color.Transparent;
            label40.Font = new Font("Arial", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label40.Location = new Point(30, 48);
            label40.Name = "label40";
            label40.Size = new Size(358, 18);
            label40.TabIndex = 48;
            label40.Text = "Search (Case ID, Complainant/Respondent Name)";
            // 
            // panel1
            // 
            panel1.Controls.Add(dataGridView1);
            panel1.Font = new Font("Arial", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            panel1.Location = new Point(12, 246);
            panel1.Name = "panel1";
            panel1.Size = new Size(1112, 438);
            panel1.TabIndex = 10;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5, btnView, btnEdit });
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(1112, 438);
            dataGridView1.TabIndex = 0;
            // 
            // Column1
            // 
            Column1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            Column1.HeaderText = "Case ID";
            Column1.Name = "Column1";
            Column1.Width = 289;
            // 
            // Column2
            // 
            Column2.HeaderText = "Complaint Date";
            Column2.Name = "Column2";
            // 
            // Column3
            // 
            Column3.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Column3.HeaderText = "Complainant";
            Column3.Name = "Column3";
            // 
            // Column4
            // 
            Column4.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Column4.HeaderText = "Respondent";
            Column4.Name = "Column4";
            // 
            // Column5
            // 
            Column5.HeaderText = "Case Violaion";
            Column5.Name = "Column5";
            // 
            // btnView
            // 
            btnView.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            btnView.HeaderText = "";
            btnView.Image = Properties.Resources.Group_29;
            btnView.ImageLayout = DataGridViewImageCellLayout.Zoom;
            btnView.Name = "btnView";
            btnView.Width = 21;
            // 
            // btnEdit
            // 
            btnEdit.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            btnEdit.HeaderText = "";
            btnEdit.Image = Properties.Resources.Group_30;
            btnEdit.ImageLayout = DataGridViewImageCellLayout.Zoom;
            btnEdit.Name = "btnEdit";
            btnEdit.Width = 21;
            // 
            // CaseList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1136, 730);
            Controls.Add(panel1);
            Controls.Add(guna2GradientPanel1);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "CaseList";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CaseList";
            guna2GradientPanel1.ResumeLayout(false);
            guna2GradientPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Guna.UI2.WinForms.Guna2GradientPanel guna2GradientPanel1;
        private TextBox textBox1;
        private Label label40;
        private Panel panel1;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewImageColumn btnView;
        private DataGridViewImageColumn btnEdit;
    }
}