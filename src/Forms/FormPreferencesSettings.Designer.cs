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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPreferencesSettings));
            radioButton_nml = new RadioButton();
            radioButton_spc = new RadioButton();
            groupBox1 = new GroupBox();
            textBox_Path = new TextBox();
            label1 = new Label();
            button_OK = new Button();
            button_Cancel = new Button();
            button_Browse = new Button();
            button_Clear = new Button();
            checkBox_Subfolder = new CheckBox();
            label2 = new Label();
            textBox_suffix = new TextBox();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            checkBox_DisablePreviewWarning = new CheckBox();
            checkBox_EnableATRACPlayback = new CheckBox();
            checkBox_Smoothsamples = new CheckBox();
            button_Splashimg = new Button();
            textBox_Splashimg = new TextBox();
            checkBox_Splashimg = new CheckBox();
            checkBox_Checkupdate = new CheckBox();
            tabPage2 = new TabPage();
            checkBox_ShowFolder = new CheckBox();
            tabPage3 = new TabPage();
            checkBox_ATRACEncodeSource = new CheckBox();
            checkBox_ForceConvertWaveOnly = new CheckBox();
            comboBox_Fixconvert = new ComboBox();
            checkBox_Fixconvert = new CheckBox();
            checkBox_FasterATRAC = new CheckBox();
            checkBox_Hidesplash = new CheckBox();
            checkBox_Oldmode = new CheckBox();
            toolTip_Description = new ToolTip(components);
            groupBox1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            SuspendLayout();
            // 
            // radioButton_nml
            // 
            resources.ApplyResources(radioButton_nml, "radioButton_nml");
            radioButton_nml.Name = "radioButton_nml";
            radioButton_nml.TabStop = true;
            toolTip_Description.SetToolTip(radioButton_nml, resources.GetString("radioButton_nml.ToolTip"));
            radioButton_nml.UseVisualStyleBackColor = true;
            radioButton_nml.CheckedChanged += RadioButton_nml_CheckedChanged;
            // 
            // radioButton_spc
            // 
            resources.ApplyResources(radioButton_spc, "radioButton_spc");
            radioButton_spc.Name = "radioButton_spc";
            radioButton_spc.TabStop = true;
            toolTip_Description.SetToolTip(radioButton_spc, resources.GetString("radioButton_spc.ToolTip"));
            radioButton_spc.UseVisualStyleBackColor = true;
            radioButton_spc.CheckedChanged += RadioButton_spc_CheckedChanged;
            // 
            // groupBox1
            // 
            resources.ApplyResources(groupBox1, "groupBox1");
            groupBox1.Controls.Add(radioButton_spc);
            groupBox1.Controls.Add(radioButton_nml);
            groupBox1.Name = "groupBox1";
            groupBox1.TabStop = false;
            toolTip_Description.SetToolTip(groupBox1, resources.GetString("groupBox1.ToolTip"));
            // 
            // textBox_Path
            // 
            resources.ApplyResources(textBox_Path, "textBox_Path");
            textBox_Path.Name = "textBox_Path";
            textBox_Path.ReadOnly = true;
            toolTip_Description.SetToolTip(textBox_Path, resources.GetString("textBox_Path.ToolTip"));
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            toolTip_Description.SetToolTip(label1, resources.GetString("label1.ToolTip"));
            // 
            // button_OK
            // 
            resources.ApplyResources(button_OK, "button_OK");
            button_OK.Name = "button_OK";
            toolTip_Description.SetToolTip(button_OK, resources.GetString("button_OK.ToolTip"));
            button_OK.UseVisualStyleBackColor = true;
            button_OK.Click += Button_OK_Click;
            // 
            // button_Cancel
            // 
            resources.ApplyResources(button_Cancel, "button_Cancel");
            button_Cancel.Name = "button_Cancel";
            toolTip_Description.SetToolTip(button_Cancel, resources.GetString("button_Cancel.ToolTip"));
            button_Cancel.UseVisualStyleBackColor = true;
            button_Cancel.Click += Button_Cancel_Click;
            // 
            // button_Browse
            // 
            resources.ApplyResources(button_Browse, "button_Browse");
            button_Browse.Name = "button_Browse";
            toolTip_Description.SetToolTip(button_Browse, resources.GetString("button_Browse.ToolTip"));
            button_Browse.UseVisualStyleBackColor = true;
            button_Browse.Click += Button_Browse_Click;
            // 
            // button_Clear
            // 
            resources.ApplyResources(button_Clear, "button_Clear");
            button_Clear.Name = "button_Clear";
            toolTip_Description.SetToolTip(button_Clear, resources.GetString("button_Clear.ToolTip"));
            button_Clear.UseVisualStyleBackColor = true;
            button_Clear.Click += Button_Clear_Click;
            // 
            // checkBox_Subfolder
            // 
            resources.ApplyResources(checkBox_Subfolder, "checkBox_Subfolder");
            checkBox_Subfolder.Name = "checkBox_Subfolder";
            toolTip_Description.SetToolTip(checkBox_Subfolder, resources.GetString("checkBox_Subfolder.ToolTip"));
            checkBox_Subfolder.UseVisualStyleBackColor = true;
            checkBox_Subfolder.CheckedChanged += CheckBox_Subfolder_CheckedChanged;
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            toolTip_Description.SetToolTip(label2, resources.GetString("label2.ToolTip"));
            // 
            // textBox_suffix
            // 
            resources.ApplyResources(textBox_suffix, "textBox_suffix");
            textBox_suffix.Name = "textBox_suffix";
            toolTip_Description.SetToolTip(textBox_suffix, resources.GetString("textBox_suffix.ToolTip"));
            // 
            // tabControl1
            // 
            resources.ApplyResources(tabControl1, "tabControl1");
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            toolTip_Description.SetToolTip(tabControl1, resources.GetString("tabControl1.ToolTip"));
            // 
            // tabPage1
            // 
            resources.ApplyResources(tabPage1, "tabPage1");
            tabPage1.Controls.Add(checkBox_DisablePreviewWarning);
            tabPage1.Controls.Add(checkBox_EnableATRACPlayback);
            tabPage1.Controls.Add(checkBox_Smoothsamples);
            tabPage1.Controls.Add(button_Splashimg);
            tabPage1.Controls.Add(textBox_Splashimg);
            tabPage1.Controls.Add(checkBox_Splashimg);
            tabPage1.Controls.Add(checkBox_Checkupdate);
            tabPage1.Name = "tabPage1";
            toolTip_Description.SetToolTip(tabPage1, resources.GetString("tabPage1.ToolTip"));
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // checkBox_DisablePreviewWarning
            // 
            resources.ApplyResources(checkBox_DisablePreviewWarning, "checkBox_DisablePreviewWarning");
            checkBox_DisablePreviewWarning.Name = "checkBox_DisablePreviewWarning";
            toolTip_Description.SetToolTip(checkBox_DisablePreviewWarning, resources.GetString("checkBox_DisablePreviewWarning.ToolTip"));
            checkBox_DisablePreviewWarning.UseVisualStyleBackColor = true;
            // 
            // checkBox_EnableATRACPlayback
            // 
            resources.ApplyResources(checkBox_EnableATRACPlayback, "checkBox_EnableATRACPlayback");
            checkBox_EnableATRACPlayback.Checked = true;
            checkBox_EnableATRACPlayback.CheckState = CheckState.Checked;
            checkBox_EnableATRACPlayback.Name = "checkBox_EnableATRACPlayback";
            toolTip_Description.SetToolTip(checkBox_EnableATRACPlayback, resources.GetString("checkBox_EnableATRACPlayback.ToolTip"));
            checkBox_EnableATRACPlayback.UseVisualStyleBackColor = true;
            // 
            // checkBox_Smoothsamples
            // 
            resources.ApplyResources(checkBox_Smoothsamples, "checkBox_Smoothsamples");
            checkBox_Smoothsamples.Name = "checkBox_Smoothsamples";
            toolTip_Description.SetToolTip(checkBox_Smoothsamples, resources.GetString("checkBox_Smoothsamples.ToolTip"));
            checkBox_Smoothsamples.UseVisualStyleBackColor = true;
            // 
            // button_Splashimg
            // 
            resources.ApplyResources(button_Splashimg, "button_Splashimg");
            button_Splashimg.Name = "button_Splashimg";
            toolTip_Description.SetToolTip(button_Splashimg, resources.GetString("button_Splashimg.ToolTip"));
            button_Splashimg.UseVisualStyleBackColor = true;
            button_Splashimg.Click += Button_Splashimg_Click;
            // 
            // textBox_Splashimg
            // 
            resources.ApplyResources(textBox_Splashimg, "textBox_Splashimg");
            textBox_Splashimg.Name = "textBox_Splashimg";
            textBox_Splashimg.ReadOnly = true;
            toolTip_Description.SetToolTip(textBox_Splashimg, resources.GetString("textBox_Splashimg.ToolTip"));
            // 
            // checkBox_Splashimg
            // 
            resources.ApplyResources(checkBox_Splashimg, "checkBox_Splashimg");
            checkBox_Splashimg.Name = "checkBox_Splashimg";
            toolTip_Description.SetToolTip(checkBox_Splashimg, resources.GetString("checkBox_Splashimg.ToolTip"));
            checkBox_Splashimg.UseVisualStyleBackColor = true;
            checkBox_Splashimg.CheckedChanged += CheckBox_Splashimg_CheckedChanged;
            // 
            // checkBox_Checkupdate
            // 
            resources.ApplyResources(checkBox_Checkupdate, "checkBox_Checkupdate");
            checkBox_Checkupdate.Checked = true;
            checkBox_Checkupdate.CheckState = CheckState.Checked;
            checkBox_Checkupdate.Name = "checkBox_Checkupdate";
            toolTip_Description.SetToolTip(checkBox_Checkupdate, resources.GetString("checkBox_Checkupdate.ToolTip"));
            checkBox_Checkupdate.UseVisualStyleBackColor = true;
            checkBox_Checkupdate.CheckedChanged += CheckBox_Checkupdate_CheckedChanged;
            // 
            // tabPage2
            // 
            resources.ApplyResources(tabPage2, "tabPage2");
            tabPage2.Controls.Add(checkBox_ShowFolder);
            tabPage2.Controls.Add(textBox_Path);
            tabPage2.Controls.Add(textBox_suffix);
            tabPage2.Controls.Add(groupBox1);
            tabPage2.Controls.Add(label2);
            tabPage2.Controls.Add(label1);
            tabPage2.Controls.Add(checkBox_Subfolder);
            tabPage2.Controls.Add(button_Browse);
            tabPage2.Controls.Add(button_Clear);
            tabPage2.Name = "tabPage2";
            toolTip_Description.SetToolTip(tabPage2, resources.GetString("tabPage2.ToolTip"));
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBox_ShowFolder
            // 
            resources.ApplyResources(checkBox_ShowFolder, "checkBox_ShowFolder");
            checkBox_ShowFolder.Checked = true;
            checkBox_ShowFolder.CheckState = CheckState.Checked;
            checkBox_ShowFolder.Name = "checkBox_ShowFolder";
            toolTip_Description.SetToolTip(checkBox_ShowFolder, resources.GetString("checkBox_ShowFolder.ToolTip"));
            checkBox_ShowFolder.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            resources.ApplyResources(tabPage3, "tabPage3");
            tabPage3.Controls.Add(checkBox_ATRACEncodeSource);
            tabPage3.Controls.Add(checkBox_ForceConvertWaveOnly);
            tabPage3.Controls.Add(comboBox_Fixconvert);
            tabPage3.Controls.Add(checkBox_Fixconvert);
            tabPage3.Controls.Add(checkBox_FasterATRAC);
            tabPage3.Controls.Add(checkBox_Hidesplash);
            tabPage3.Controls.Add(checkBox_Oldmode);
            tabPage3.Name = "tabPage3";
            toolTip_Description.SetToolTip(tabPage3, resources.GetString("tabPage3.ToolTip"));
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // checkBox_ATRACEncodeSource
            // 
            resources.ApplyResources(checkBox_ATRACEncodeSource, "checkBox_ATRACEncodeSource");
            checkBox_ATRACEncodeSource.Name = "checkBox_ATRACEncodeSource";
            toolTip_Description.SetToolTip(checkBox_ATRACEncodeSource, resources.GetString("checkBox_ATRACEncodeSource.ToolTip"));
            checkBox_ATRACEncodeSource.UseVisualStyleBackColor = true;
            // 
            // checkBox_ForceConvertWaveOnly
            // 
            resources.ApplyResources(checkBox_ForceConvertWaveOnly, "checkBox_ForceConvertWaveOnly");
            checkBox_ForceConvertWaveOnly.Name = "checkBox_ForceConvertWaveOnly";
            toolTip_Description.SetToolTip(checkBox_ForceConvertWaveOnly, resources.GetString("checkBox_ForceConvertWaveOnly.ToolTip"));
            checkBox_ForceConvertWaveOnly.UseVisualStyleBackColor = true;
            // 
            // comboBox_Fixconvert
            // 
            resources.ApplyResources(comboBox_Fixconvert, "comboBox_Fixconvert");
            comboBox_Fixconvert.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_Fixconvert.FormattingEnabled = true;
            comboBox_Fixconvert.Items.AddRange(new object[] { resources.GetString("comboBox_Fixconvert.Items"), resources.GetString("comboBox_Fixconvert.Items1"), resources.GetString("comboBox_Fixconvert.Items2"), resources.GetString("comboBox_Fixconvert.Items3") });
            comboBox_Fixconvert.Name = "comboBox_Fixconvert";
            toolTip_Description.SetToolTip(comboBox_Fixconvert, resources.GetString("comboBox_Fixconvert.ToolTip"));
            comboBox_Fixconvert.SelectedIndexChanged += ComboBox_Fixconvert_SelectedIndexChanged;
            // 
            // checkBox_Fixconvert
            // 
            resources.ApplyResources(checkBox_Fixconvert, "checkBox_Fixconvert");
            checkBox_Fixconvert.Name = "checkBox_Fixconvert";
            toolTip_Description.SetToolTip(checkBox_Fixconvert, resources.GetString("checkBox_Fixconvert.ToolTip"));
            checkBox_Fixconvert.UseVisualStyleBackColor = true;
            checkBox_Fixconvert.CheckedChanged += CheckBox_Fixconvert_CheckedChanged;
            // 
            // checkBox_FasterATRAC
            // 
            resources.ApplyResources(checkBox_FasterATRAC, "checkBox_FasterATRAC");
            checkBox_FasterATRAC.Name = "checkBox_FasterATRAC";
            toolTip_Description.SetToolTip(checkBox_FasterATRAC, resources.GetString("checkBox_FasterATRAC.ToolTip"));
            checkBox_FasterATRAC.UseVisualStyleBackColor = true;
            // 
            // checkBox_Hidesplash
            // 
            resources.ApplyResources(checkBox_Hidesplash, "checkBox_Hidesplash");
            checkBox_Hidesplash.Name = "checkBox_Hidesplash";
            toolTip_Description.SetToolTip(checkBox_Hidesplash, resources.GetString("checkBox_Hidesplash.ToolTip"));
            checkBox_Hidesplash.UseVisualStyleBackColor = true;
            // 
            // checkBox_Oldmode
            // 
            resources.ApplyResources(checkBox_Oldmode, "checkBox_Oldmode");
            checkBox_Oldmode.Name = "checkBox_Oldmode";
            toolTip_Description.SetToolTip(checkBox_Oldmode, resources.GetString("checkBox_Oldmode.ToolTip"));
            checkBox_Oldmode.UseVisualStyleBackColor = true;
            // 
            // FormPreferencesSettings
            // 
            AcceptButton = button_OK;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ControlBox = false;
            Controls.Add(tabControl1);
            Controls.Add(button_Cancel);
            Controls.Add(button_OK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "FormPreferencesSettings";
            toolTip_Description.SetToolTip(this, resources.GetString("$this.ToolTip"));
            Load += FormPreferencesSettings_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ResumeLayout(false);
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
        private CheckBox checkBox_Checkupdate;
        private TabPage tabPage3;
        private CheckBox checkBox_Hidesplash;
        private CheckBox checkBox_Oldmode;
        private Button button_Splashimg;
        private TextBox textBox_Splashimg;
        private CheckBox checkBox_Splashimg;
        private CheckBox checkBox_FasterATRAC;
        private ComboBox comboBox_Fixconvert;
        private CheckBox checkBox_Fixconvert;
        private CheckBox checkBox_ForceConvertWaveOnly;
        private CheckBox checkBox_Smoothsamples;
        private CheckBox checkBox_EnableATRACPlayback;
        private ToolTip toolTip_Description;
        private CheckBox checkBox_DisablePreviewWarning;
        private CheckBox checkBox_ATRACEncodeSource;
    }
}