namespace ATRACTool_Reloaded
{
    partial class FormPreferencesSettings
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
            this.radioButton_nml = new System.Windows.Forms.RadioButton();
            this.radioButton_spc = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_Path = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_Browse = new System.Windows.Forms.Button();
            this.button_Clear = new System.Windows.Forms.Button();
            this.checkBox_Subfolder = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_suffix = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.checkBox_ShowFolder = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButton_nml
            // 
            this.radioButton_nml.AutoSize = true;
            this.radioButton_nml.Location = new System.Drawing.Point(6, 22);
            this.radioButton_nml.Name = "radioButton_nml";
            this.radioButton_nml.Size = new System.Drawing.Size(181, 19);
            this.radioButton_nml.TabIndex = 0;
            this.radioButton_nml.TabStop = true;
            this.radioButton_nml.Text = "Always ask for a save location";
            this.radioButton_nml.UseVisualStyleBackColor = true;
            this.radioButton_nml.CheckedChanged += new System.EventHandler(this.RadioButton_nml_CheckedChanged);
            // 
            // radioButton_spc
            // 
            this.radioButton_spc.AutoSize = true;
            this.radioButton_spc.Location = new System.Drawing.Point(198, 22);
            this.radioButton_spc.Name = "radioButton_spc";
            this.radioButton_spc.Size = new System.Drawing.Size(160, 19);
            this.radioButton_spc.TabIndex = 1;
            this.radioButton_spc.TabStop = true;
            this.radioButton_spc.Text = "Output to specified folder";
            this.radioButton_spc.UseVisualStyleBackColor = true;
            this.radioButton_spc.CheckedChanged += new System.EventHandler(this.RadioButton_spc_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton_spc);
            this.groupBox1.Controls.Add(this.radioButton_nml);
            this.groupBox1.Location = new System.Drawing.Point(7, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(365, 55);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Behavior when saving a file";
            // 
            // textBox_Path
            // 
            this.textBox_Path.Enabled = false;
            this.textBox_Path.Location = new System.Drawing.Point(47, 69);
            this.textBox_Path.Name = "textBox_Path";
            this.textBox_Path.ReadOnly = true;
            this.textBox_Path.Size = new System.Drawing.Size(230, 23);
            this.textBox_Path.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Enabled = false;
            this.label1.Location = new System.Drawing.Point(7, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Path:";
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(232, 217);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.Button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(313, 217);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.Button_Cancel_Click);
            // 
            // button_Browse
            // 
            this.button_Browse.Enabled = false;
            this.button_Browse.Location = new System.Drawing.Point(283, 69);
            this.button_Browse.Name = "button_Browse";
            this.button_Browse.Size = new System.Drawing.Size(26, 23);
            this.button_Browse.TabIndex = 7;
            this.button_Browse.Text = "...";
            this.button_Browse.UseVisualStyleBackColor = true;
            this.button_Browse.Click += new System.EventHandler(this.Button_Browse_Click);
            // 
            // button_Clear
            // 
            this.button_Clear.Enabled = false;
            this.button_Clear.Location = new System.Drawing.Point(315, 69);
            this.button_Clear.Name = "button_Clear";
            this.button_Clear.Size = new System.Drawing.Size(57, 23);
            this.button_Clear.TabIndex = 8;
            this.button_Clear.Text = "Clear";
            this.button_Clear.UseVisualStyleBackColor = true;
            this.button_Clear.Click += new System.EventHandler(this.Button_Clear_Click);
            // 
            // checkBox_Subfolder
            // 
            this.checkBox_Subfolder.AutoSize = true;
            this.checkBox_Subfolder.Enabled = false;
            this.checkBox_Subfolder.Location = new System.Drawing.Point(13, 98);
            this.checkBox_Subfolder.Name = "checkBox_Subfolder";
            this.checkBox_Subfolder.Size = new System.Drawing.Size(117, 19);
            this.checkBox_Subfolder.TabIndex = 9;
            this.checkBox_Subfolder.Text = "Create subfolders";
            this.checkBox_Subfolder.UseVisualStyleBackColor = true;
            this.checkBox_Subfolder.CheckedChanged += new System.EventHandler(this.CheckBox_Subfolder_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Enabled = false;
            this.label2.Location = new System.Drawing.Point(7, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 15);
            this.label2.TabIndex = 10;
            this.label2.Text = "Subfolder suffix:";
            // 
            // textBox_suffix
            // 
            this.textBox_suffix.Enabled = false;
            this.textBox_suffix.Location = new System.Drawing.Point(101, 121);
            this.textBox_suffix.Name = "textBox_suffix";
            this.textBox_suffix.Size = new System.Drawing.Size(271, 23);
            this.textBox_suffix.TabIndex = 11;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(2, 1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(390, 210);
            this.tabControl1.TabIndex = 12;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(382, 182);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(155, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "No options.";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.checkBox_ShowFolder);
            this.tabPage2.Controls.Add(this.textBox_Path);
            this.tabPage2.Controls.Add(this.textBox_suffix);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.checkBox_Subfolder);
            this.tabPage2.Controls.Add(this.button_Browse);
            this.tabPage2.Controls.Add(this.button_Clear);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(382, 182);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "IO";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBox_ShowFolder
            // 
            this.checkBox_ShowFolder.AutoSize = true;
            this.checkBox_ShowFolder.Checked = true;
            this.checkBox_ShowFolder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_ShowFolder.Location = new System.Drawing.Point(13, 151);
            this.checkBox_ShowFolder.Name = "checkBox_ShowFolder";
            this.checkBox_ShowFolder.Size = new System.Drawing.Size(241, 19);
            this.checkBox_ShowFolder.TabIndex = 12;
            this.checkBox_ShowFolder.Text = "Open destination folder after completion";
            this.checkBox_ShowFolder.UseVisualStyleBackColor = true;
            // 
            // FormPreferencesSettings
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(394, 251);
            this.ControlBox = false;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FormPreferencesSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Preferences";
            this.Load += new System.EventHandler(this.FormPreferencesSettings_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private RadioButton radioButton_nml;
        private RadioButton radioButton_spc;
        private GroupBox groupBox1;
        private TextBox textBox_Path;
        private Label label1;
        private Button button_OK;
        private Button button_Cancel;
        private Button button_Browse;
        private Button button_Clear;
        private CheckBox checkBox_Subfolder;
        private Label label2;
        private TextBox textBox_suffix;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private CheckBox checkBox_ShowFolder;
        private Label label3;
    }
}