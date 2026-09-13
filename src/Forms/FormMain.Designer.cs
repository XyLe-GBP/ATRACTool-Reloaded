namespace ATRACTool_Reloaded
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            groupBox1 = new GroupBox();
            label_Sizetxt = new Label();
            label_Size = new Label();
            label_Formattxt = new Label();
            label_Filepath = new Label();
            label_Format = new Label();
            label_File = new Label();
            textBox_LoopEnd = new TextBox();
            textBox_LoopStart = new TextBox();
            label_NotReaded = new Label();
            menuStrip1 = new MenuStrip();
            fileFToolStripMenuItem = new ToolStripMenuItem();
            openFileOToolStripMenuItem = new ToolStripMenuItem();
            filesToolStripMenuItem = new ToolStripMenuItem();
            folderToolStripMenuItem = new ToolStripMenuItem();
            closeFileCToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            exitXToolStripMenuItem = new ToolStripMenuItem();
            settingsSToolStripMenuItem = new ToolStripMenuItem();
            convertSettingsToolStripMenuItem = new ToolStripMenuItem();
            saveMethodSettingsMToolStripMenuItem = new ToolStripMenuItem();
            toolsTToolStripMenuItem = new ToolStripMenuItem();
            convertAudioToolStripMenuItem = new ToolStripMenuItem();
            audioToWAVEToolStripMenuItem = new ToolStripMenuItem();
            wAVEToAudioToolStripMenuItem = new ToolStripMenuItem();
            helpHToolStripMenuItem = new ToolStripMenuItem();
            aboutATRACToolToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            checkForUpdatesUToolStripMenuItem = new ToolStripMenuItem();
            button_Decode = new Button();
            button_Encode = new Button();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel_Status = new ToolStripStatusLabel();
            toolStripDropDownButton_EF = new ToolStripDropDownButton();
            aTRAC3ATRAC3ToolStripMenuItem = new ToolStripMenuItem();
            aTRAC9ToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripSeparator();
            walkmanToolStripMenuItem = new ToolStripMenuItem();
            toolStripStatusLabel_EncMethod = new ToolStripStatusLabel();
            panel_Control = new Panel();
            panel_Main = new Panel();
            label_LoopStart = new Label();
            label_LoopEnd = new Label();
            label_SSample = new Label();
            label_ESample = new Label();
            groupBox_Loop = new GroupBox();
            groupBox1.SuspendLayout();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            panel_Control.SuspendLayout();
            panel_Main.SuspendLayout();
            groupBox_Loop.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.FromArgb(246, 247, 249);
            groupBox1.Controls.Add(label_Sizetxt);
            groupBox1.Controls.Add(label_Size);
            groupBox1.Controls.Add(label_Formattxt);
            groupBox1.Controls.Add(label_Filepath);
            groupBox1.Controls.Add(label_Format);
            groupBox1.Controls.Add(label_File);
            groupBox1.ForeColor = Color.FromArgb(31, 35, 40);
            resources.ApplyResources(groupBox1, "groupBox1");
            groupBox1.Name = "groupBox1";
            groupBox1.TabStop = false;
            // 
            // label_Sizetxt
            // 
            resources.ApplyResources(label_Sizetxt, "label_Sizetxt");
            label_Sizetxt.BackColor = Color.FromArgb(246, 247, 249);
            label_Sizetxt.ForeColor = Color.FromArgb(31, 35, 40);
            label_Sizetxt.Name = "label_Sizetxt";
            // 
            // label_Size
            // 
            resources.ApplyResources(label_Size, "label_Size");
            label_Size.BackColor = Color.FromArgb(246, 247, 249);
            label_Size.ForeColor = Color.FromArgb(31, 35, 40);
            label_Size.Name = "label_Size";
            // 
            // label_Formattxt
            // 
            label_Formattxt.BackColor = Color.FromArgb(246, 247, 249);
            label_Formattxt.ForeColor = Color.FromArgb(31, 35, 40);
            resources.ApplyResources(label_Formattxt, "label_Formattxt");
            label_Formattxt.Name = "label_Formattxt";
            // 
            // label_Filepath
            // 
            label_Filepath.AutoEllipsis = true;
            label_Filepath.BackColor = Color.FromArgb(246, 247, 249);
            label_Filepath.ForeColor = Color.FromArgb(31, 35, 40);
            resources.ApplyResources(label_Filepath, "label_Filepath");
            label_Filepath.Name = "label_Filepath";
            // 
            // label_Format
            // 
            resources.ApplyResources(label_Format, "label_Format");
            label_Format.BackColor = Color.FromArgb(246, 247, 249);
            label_Format.ForeColor = Color.FromArgb(31, 35, 40);
            label_Format.Name = "label_Format";
            // 
            // label_File
            // 
            resources.ApplyResources(label_File, "label_File");
            label_File.BackColor = Color.FromArgb(246, 247, 249);
            label_File.ForeColor = Color.FromArgb(31, 35, 40);
            label_File.Name = "label_File";
            // 
            // textBox_LoopEnd
            // 
            textBox_LoopEnd.BackColor = Color.White;
            resources.ApplyResources(textBox_LoopEnd, "textBox_LoopEnd");
            textBox_LoopEnd.ForeColor = Color.FromArgb(31, 35, 40);
            textBox_LoopEnd.Name = "textBox_LoopEnd";
            textBox_LoopEnd.TextChanged += TextBox_LoopEnd_TextChanged;
            textBox_LoopEnd.KeyPress += TextBox_LoopEnd_KeyPress;
            // 
            // textBox_LoopStart
            // 
            textBox_LoopStart.BackColor = Color.White;
            resources.ApplyResources(textBox_LoopStart, "textBox_LoopStart");
            textBox_LoopStart.ForeColor = Color.FromArgb(31, 35, 40);
            textBox_LoopStart.Name = "textBox_LoopStart";
            textBox_LoopStart.TextChanged += TextBox_LoopStart_TextChanged;
            textBox_LoopStart.KeyPress += TextBox_LoopStart_KeyPress;
            // 
            // label_NotReaded
            // 
            label_NotReaded.BackColor = Color.FromArgb(246, 247, 249);
            resources.ApplyResources(label_NotReaded, "label_NotReaded");
            label_NotReaded.ForeColor = Color.FromArgb(31, 35, 40);
            label_NotReaded.Name = "label_NotReaded";
            label_NotReaded.Click += label_NotReaded_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = Color.White;
            menuStrip1.ForeColor = Color.FromArgb(31, 35, 40);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileFToolStripMenuItem, settingsSToolStripMenuItem, toolsTToolStripMenuItem, helpHToolStripMenuItem });
            resources.ApplyResources(menuStrip1, "menuStrip1");
            menuStrip1.Name = "menuStrip1";
            // 
            // fileFToolStripMenuItem
            // 
            fileFToolStripMenuItem.BackColor = Color.White;
            fileFToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openFileOToolStripMenuItem, closeFileCToolStripMenuItem, toolStripMenuItem1, exitXToolStripMenuItem });
            fileFToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            fileFToolStripMenuItem.Name = "fileFToolStripMenuItem";
            resources.ApplyResources(fileFToolStripMenuItem, "fileFToolStripMenuItem");
            // 
            // openFileOToolStripMenuItem
            // 
            openFileOToolStripMenuItem.BackColor = Color.White;
            openFileOToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { filesToolStripMenuItem, folderToolStripMenuItem });
            openFileOToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            openFileOToolStripMenuItem.Name = "openFileOToolStripMenuItem";
            resources.ApplyResources(openFileOToolStripMenuItem, "openFileOToolStripMenuItem");
            // 
            // filesToolStripMenuItem
            // 
            filesToolStripMenuItem.BackColor = Color.White;
            filesToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            filesToolStripMenuItem.Name = "filesToolStripMenuItem";
            resources.ApplyResources(filesToolStripMenuItem, "filesToolStripMenuItem");
            filesToolStripMenuItem.Click += FilesToolStripMenuItem_Click;
            // 
            // folderToolStripMenuItem
            // 
            folderToolStripMenuItem.BackColor = Color.White;
            folderToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            folderToolStripMenuItem.Name = "folderToolStripMenuItem";
            resources.ApplyResources(folderToolStripMenuItem, "folderToolStripMenuItem");
            folderToolStripMenuItem.Click += FolderToolStripMenuItem_Click;
            // 
            // closeFileCToolStripMenuItem
            // 
            closeFileCToolStripMenuItem.BackColor = Color.White;
            closeFileCToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            closeFileCToolStripMenuItem.Name = "closeFileCToolStripMenuItem";
            resources.ApplyResources(closeFileCToolStripMenuItem, "closeFileCToolStripMenuItem");
            closeFileCToolStripMenuItem.Click += CloseFileCToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.BackColor = Color.White;
            toolStripMenuItem1.ForeColor = Color.FromArgb(31, 35, 40);
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // exitXToolStripMenuItem
            // 
            exitXToolStripMenuItem.BackColor = Color.White;
            resources.ApplyResources(exitXToolStripMenuItem, "exitXToolStripMenuItem");
            exitXToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            exitXToolStripMenuItem.Name = "exitXToolStripMenuItem";
            exitXToolStripMenuItem.Click += ExitXToolStripMenuItem_Click;
            // 
            // settingsSToolStripMenuItem
            // 
            settingsSToolStripMenuItem.BackColor = Color.White;
            settingsSToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { convertSettingsToolStripMenuItem, saveMethodSettingsMToolStripMenuItem });
            settingsSToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            settingsSToolStripMenuItem.Name = "settingsSToolStripMenuItem";
            resources.ApplyResources(settingsSToolStripMenuItem, "settingsSToolStripMenuItem");
            // 
            // convertSettingsToolStripMenuItem
            // 
            convertSettingsToolStripMenuItem.BackColor = Color.White;
            convertSettingsToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            convertSettingsToolStripMenuItem.Name = "convertSettingsToolStripMenuItem";
            resources.ApplyResources(convertSettingsToolStripMenuItem, "convertSettingsToolStripMenuItem");
            convertSettingsToolStripMenuItem.Click += ConvertSettingsToolStripMenuItem_Click;
            // 
            // saveMethodSettingsMToolStripMenuItem
            // 
            saveMethodSettingsMToolStripMenuItem.BackColor = Color.White;
            saveMethodSettingsMToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            saveMethodSettingsMToolStripMenuItem.Name = "saveMethodSettingsMToolStripMenuItem";
            resources.ApplyResources(saveMethodSettingsMToolStripMenuItem, "saveMethodSettingsMToolStripMenuItem");
            saveMethodSettingsMToolStripMenuItem.Click += PreferencesMToolStripMenuItem_Click;
            // 
            // toolsTToolStripMenuItem
            // 
            toolsTToolStripMenuItem.BackColor = Color.White;
            toolsTToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { convertAudioToolStripMenuItem });
            toolsTToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            toolsTToolStripMenuItem.Name = "toolsTToolStripMenuItem";
            resources.ApplyResources(toolsTToolStripMenuItem, "toolsTToolStripMenuItem");
            // 
            // convertAudioToolStripMenuItem
            // 
            convertAudioToolStripMenuItem.BackColor = Color.White;
            convertAudioToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { audioToWAVEToolStripMenuItem, wAVEToAudioToolStripMenuItem });
            convertAudioToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            convertAudioToolStripMenuItem.Name = "convertAudioToolStripMenuItem";
            resources.ApplyResources(convertAudioToolStripMenuItem, "convertAudioToolStripMenuItem");
            // 
            // audioToWAVEToolStripMenuItem
            // 
            audioToWAVEToolStripMenuItem.BackColor = Color.White;
            audioToWAVEToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            audioToWAVEToolStripMenuItem.Name = "audioToWAVEToolStripMenuItem";
            resources.ApplyResources(audioToWAVEToolStripMenuItem, "audioToWAVEToolStripMenuItem");
            audioToWAVEToolStripMenuItem.Click += AudioToWAVEToolStripMenuItem_Click;
            // 
            // wAVEToAudioToolStripMenuItem
            // 
            wAVEToAudioToolStripMenuItem.BackColor = Color.White;
            wAVEToAudioToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            wAVEToAudioToolStripMenuItem.Name = "wAVEToAudioToolStripMenuItem";
            resources.ApplyResources(wAVEToAudioToolStripMenuItem, "wAVEToAudioToolStripMenuItem");
            wAVEToAudioToolStripMenuItem.Click += WAVEToAudioToolStripMenuItem_Click;
            // 
            // helpHToolStripMenuItem
            // 
            helpHToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
            helpHToolStripMenuItem.BackColor = Color.White;
            helpHToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutATRACToolToolStripMenuItem, toolStripMenuItem2, checkForUpdatesUToolStripMenuItem });
            helpHToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            helpHToolStripMenuItem.Name = "helpHToolStripMenuItem";
            resources.ApplyResources(helpHToolStripMenuItem, "helpHToolStripMenuItem");
            // 
            // aboutATRACToolToolStripMenuItem
            // 
            aboutATRACToolToolStripMenuItem.BackColor = Color.White;
            aboutATRACToolToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            aboutATRACToolToolStripMenuItem.Name = "aboutATRACToolToolStripMenuItem";
            resources.ApplyResources(aboutATRACToolToolStripMenuItem, "aboutATRACToolToolStripMenuItem");
            aboutATRACToolToolStripMenuItem.Click += AboutATRACToolToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.BackColor = Color.White;
            toolStripMenuItem2.ForeColor = Color.FromArgb(31, 35, 40);
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(toolStripMenuItem2, "toolStripMenuItem2");
            // 
            // checkForUpdatesUToolStripMenuItem
            // 
            checkForUpdatesUToolStripMenuItem.BackColor = Color.White;
            checkForUpdatesUToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            checkForUpdatesUToolStripMenuItem.Name = "checkForUpdatesUToolStripMenuItem";
            resources.ApplyResources(checkForUpdatesUToolStripMenuItem, "checkForUpdatesUToolStripMenuItem");
            checkForUpdatesUToolStripMenuItem.Click += CheckForUpdatesUToolStripMenuItem_Click;
            // 
            // button_Decode
            // 
            button_Decode.BackColor = Color.White;
            resources.ApplyResources(button_Decode, "button_Decode");
            button_Decode.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_Decode.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_Decode.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            button_Decode.ForeColor = Color.FromArgb(31, 35, 40);
            button_Decode.Name = "button_Decode";
            button_Decode.UseVisualStyleBackColor = false;
            button_Decode.Click += Button_Decode_Click;
            // 
            // button_Encode
            // 
            button_Encode.BackColor = Color.White;
            resources.ApplyResources(button_Encode, "button_Encode");
            button_Encode.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_Encode.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_Encode.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            button_Encode.ForeColor = Color.FromArgb(31, 35, 40);
            button_Encode.Name = "button_Encode";
            button_Encode.UseVisualStyleBackColor = false;
            button_Encode.Click += Button_Encode_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.White;
            statusStrip1.ForeColor = Color.FromArgb(31, 35, 40);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel_Status, toolStripDropDownButton_EF, toolStripStatusLabel_EncMethod });
            statusStrip1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            resources.ApplyResources(statusStrip1, "statusStrip1");
            statusStrip1.Name = "statusStrip1";
            statusStrip1.SizingGrip = false;
            // 
            // toolStripStatusLabel_Status
            // 
            toolStripStatusLabel_Status.BackColor = Color.White;
            toolStripStatusLabel_Status.ForeColor = Color.FromArgb(31, 35, 40);
            toolStripStatusLabel_Status.Name = "toolStripStatusLabel_Status";
            resources.ApplyResources(toolStripStatusLabel_Status, "toolStripStatusLabel_Status");
            // 
            // toolStripDropDownButton_EF
            // 
            toolStripDropDownButton_EF.Alignment = ToolStripItemAlignment.Right;
            toolStripDropDownButton_EF.BackColor = Color.White;
            toolStripDropDownButton_EF.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton_EF.DropDownItems.AddRange(new ToolStripItem[] { aTRAC3ATRAC3ToolStripMenuItem, aTRAC9ToolStripMenuItem, toolStripMenuItem3, walkmanToolStripMenuItem });
            toolStripDropDownButton_EF.ForeColor = Color.FromArgb(31, 35, 40);
            resources.ApplyResources(toolStripDropDownButton_EF, "toolStripDropDownButton_EF");
            toolStripDropDownButton_EF.Name = "toolStripDropDownButton_EF";
            // 
            // aTRAC3ATRAC3ToolStripMenuItem
            // 
            aTRAC3ATRAC3ToolStripMenuItem.BackColor = Color.White;
            aTRAC3ATRAC3ToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            aTRAC3ATRAC3ToolStripMenuItem.Name = "aTRAC3ATRAC3ToolStripMenuItem";
            resources.ApplyResources(aTRAC3ATRAC3ToolStripMenuItem, "aTRAC3ATRAC3ToolStripMenuItem");
            aTRAC3ATRAC3ToolStripMenuItem.Click += ATRAC3ATRAC3ToolStripMenuItem_Click;
            // 
            // aTRAC9ToolStripMenuItem
            // 
            aTRAC9ToolStripMenuItem.BackColor = Color.White;
            aTRAC9ToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            aTRAC9ToolStripMenuItem.Name = "aTRAC9ToolStripMenuItem";
            resources.ApplyResources(aTRAC9ToolStripMenuItem, "aTRAC9ToolStripMenuItem");
            aTRAC9ToolStripMenuItem.Click += ATRAC9ToolStripMenuItem_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.BackColor = Color.White;
            toolStripMenuItem3.ForeColor = Color.FromArgb(31, 35, 40);
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            resources.ApplyResources(toolStripMenuItem3, "toolStripMenuItem3");
            // 
            // walkmanToolStripMenuItem
            // 
            walkmanToolStripMenuItem.BackColor = Color.White;
            walkmanToolStripMenuItem.ForeColor = Color.FromArgb(31, 35, 40);
            walkmanToolStripMenuItem.Name = "walkmanToolStripMenuItem";
            resources.ApplyResources(walkmanToolStripMenuItem, "walkmanToolStripMenuItem");
            walkmanToolStripMenuItem.Click += walkmanToolStripMenuItem_Click;
            // 
            // toolStripStatusLabel_EncMethod
            // 
            toolStripStatusLabel_EncMethod.BackColor = Color.White;
            resources.ApplyResources(toolStripStatusLabel_EncMethod, "toolStripStatusLabel_EncMethod");
            toolStripStatusLabel_EncMethod.ForeColor = Color.FromArgb(108, 115, 123);
            toolStripStatusLabel_EncMethod.Name = "toolStripStatusLabel_EncMethod";
            // 
            // panel_Control
            // 
            panel_Control.BackColor = Color.FromArgb(246, 247, 249);
            panel_Control.Controls.Add(button_Encode);
            panel_Control.Controls.Add(button_Decode);
            panel_Control.ForeColor = Color.FromArgb(31, 35, 40);
            resources.ApplyResources(panel_Control, "panel_Control");
            panel_Control.Name = "panel_Control";
            // 
            // panel_Main
            // 
            resources.ApplyResources(panel_Main, "panel_Main");
            panel_Main.BackColor = Color.FromArgb(246, 247, 249);
            panel_Main.Controls.Add(label_NotReaded);
            panel_Main.ForeColor = Color.FromArgb(31, 35, 40);
            panel_Main.Name = "panel_Main";
            // 
            // label_LoopStart
            // 
            resources.ApplyResources(label_LoopStart, "label_LoopStart");
            label_LoopStart.BackColor = Color.FromArgb(246, 247, 249);
            label_LoopStart.ForeColor = Color.FromArgb(31, 35, 40);
            label_LoopStart.Name = "label_LoopStart";
            // 
            // label_LoopEnd
            // 
            resources.ApplyResources(label_LoopEnd, "label_LoopEnd");
            label_LoopEnd.BackColor = Color.FromArgb(246, 247, 249);
            label_LoopEnd.ForeColor = Color.FromArgb(31, 35, 40);
            label_LoopEnd.Name = "label_LoopEnd";
            // 
            // label_SSample
            // 
            resources.ApplyResources(label_SSample, "label_SSample");
            label_SSample.BackColor = Color.FromArgb(246, 247, 249);
            label_SSample.ForeColor = Color.FromArgb(31, 35, 40);
            label_SSample.Name = "label_SSample";
            // 
            // label_ESample
            // 
            resources.ApplyResources(label_ESample, "label_ESample");
            label_ESample.BackColor = Color.FromArgb(246, 247, 249);
            label_ESample.ForeColor = Color.FromArgb(31, 35, 40);
            label_ESample.Name = "label_ESample";
            // 
            // groupBox_Loop
            // 
            groupBox_Loop.BackColor = Color.FromArgb(246, 247, 249);
            groupBox_Loop.Controls.Add(label_ESample);
            groupBox_Loop.Controls.Add(label_LoopEnd);
            groupBox_Loop.Controls.Add(label_SSample);
            groupBox_Loop.Controls.Add(label_LoopStart);
            groupBox_Loop.Controls.Add(textBox_LoopStart);
            groupBox_Loop.Controls.Add(textBox_LoopEnd);
            resources.ApplyResources(groupBox_Loop, "groupBox_Loop");
            groupBox_Loop.ForeColor = Color.FromArgb(31, 35, 40);
            groupBox_Loop.Name = "groupBox_Loop";
            groupBox_Loop.TabStop = false;
            // 
            // FormMain
            // 
            AllowDrop = true;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox_Loop);
            Controls.Add(groupBox1);
            Controls.Add(panel_Main);
            Controls.Add(panel_Control);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            DoubleBuffered = true;
            ForeColor = Color.FromArgb(31, 35, 40);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "FormMain";
            FormClosing += FormMain_FormClosing;
            FormClosed += FormMain_FormClosed;
            Load += FormMain_Load;
            DragDrop += FormMain_DragDrop;
            DragEnter += FormMain_DragEnter;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            panel_Control.ResumeLayout(false);
            panel_Main.ResumeLayout(false);
            groupBox_Loop.ResumeLayout(false);
            groupBox_Loop.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileFToolStripMenuItem;
        private ToolStripMenuItem openFileOToolStripMenuItem;
        private ToolStripMenuItem closeFileCToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem exitXToolStripMenuItem;
        private ToolStripMenuItem settingsSToolStripMenuItem;
        private ToolStripMenuItem convertSettingsToolStripMenuItem;
        private ToolStripMenuItem toolsTToolStripMenuItem;
        private ToolStripMenuItem convertAudioToolStripMenuItem;
        private ToolStripMenuItem helpHToolStripMenuItem;
        private ToolStripMenuItem aboutATRACToolToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem checkForUpdatesUToolStripMenuItem;
        private Button button_Decode;
        private Label label_NotReaded;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel_Status;
        private ToolStripMenuItem aTRAC3ATRAC3ToolStripMenuItem;
        private ToolStripMenuItem aTRAC9ToolStripMenuItem;
        private Label label_Format;
        private Label label_File;
        private Label label_Formattxt;
        private Label label_Sizetxt;
        private Label label_Size;
        private ToolStripMenuItem audioToWAVEToolStripMenuItem;
        private ToolStripMenuItem wAVEToAudioToolStripMenuItem;
        private ToolStripMenuItem saveMethodSettingsMToolStripMenuItem;
        private Panel panel_Control;
        private Panel panel_Main;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem walkmanToolStripMenuItem;
        private ToolStripStatusLabel toolStripStatusLabel_EncMethod;
        internal Label label_Filepath;
        internal TextBox textBox_LoopEnd;
        internal TextBox textBox_LoopStart;
        internal Button button_Encode;
        internal Label label_LoopStart;
        internal Label label_LoopEnd;
        internal Label label_SSample;
        internal Label label_ESample;
        internal GroupBox groupBox_Loop;
        internal ToolStripDropDownButton toolStripDropDownButton_EF;
        private ToolStripMenuItem filesToolStripMenuItem;
        private ToolStripMenuItem folderToolStripMenuItem;
    }
}
