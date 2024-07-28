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
            label4 = new Label();
            label_Formattxt = new Label();
            label_Filepath = new Label();
            label3 = new Label();
            label2 = new Label();
            label_NotReaded = new Label();
            menuStrip1 = new MenuStrip();
            fileFToolStripMenuItem = new ToolStripMenuItem();
            openFileOToolStripMenuItem = new ToolStripMenuItem();
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
            loopPointCreationToolStripMenuItem = new ToolStripMenuItem();
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
            groupBox1.SuspendLayout();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            panel_Control.SuspendLayout();
            panel_Main.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label_Sizetxt);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label_Formattxt);
            groupBox1.Controls.Add(label_Filepath);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            resources.ApplyResources(groupBox1, "groupBox1");
            groupBox1.Name = "groupBox1";
            groupBox1.TabStop = false;
            // 
            // label_Sizetxt
            // 
            resources.ApplyResources(label_Sizetxt, "label_Sizetxt");
            label_Sizetxt.Name = "label_Sizetxt";
            // 
            // label4
            // 
            resources.ApplyResources(label4, "label4");
            label4.Name = "label4";
            // 
            // label_Formattxt
            // 
            resources.ApplyResources(label_Formattxt, "label_Formattxt");
            label_Formattxt.Name = "label_Formattxt";
            // 
            // label_Filepath
            // 
            label_Filepath.AutoEllipsis = true;
            resources.ApplyResources(label_Filepath, "label_Filepath");
            label_Filepath.Name = "label_Filepath";
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // label_NotReaded
            // 
            resources.ApplyResources(label_NotReaded, "label_NotReaded");
            label_NotReaded.Name = "label_NotReaded";
            label_NotReaded.Click += label_NotReaded_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileFToolStripMenuItem, settingsSToolStripMenuItem, toolsTToolStripMenuItem, helpHToolStripMenuItem });
            resources.ApplyResources(menuStrip1, "menuStrip1");
            menuStrip1.Name = "menuStrip1";
            // 
            // fileFToolStripMenuItem
            // 
            fileFToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openFileOToolStripMenuItem, closeFileCToolStripMenuItem, toolStripMenuItem1, exitXToolStripMenuItem });
            fileFToolStripMenuItem.Name = "fileFToolStripMenuItem";
            resources.ApplyResources(fileFToolStripMenuItem, "fileFToolStripMenuItem");
            // 
            // openFileOToolStripMenuItem
            // 
            openFileOToolStripMenuItem.Name = "openFileOToolStripMenuItem";
            resources.ApplyResources(openFileOToolStripMenuItem, "openFileOToolStripMenuItem");
            openFileOToolStripMenuItem.Click += OpenFileOToolStripMenuItem_Click;
            // 
            // closeFileCToolStripMenuItem
            // 
            closeFileCToolStripMenuItem.Name = "closeFileCToolStripMenuItem";
            resources.ApplyResources(closeFileCToolStripMenuItem, "closeFileCToolStripMenuItem");
            closeFileCToolStripMenuItem.Click += CloseFileCToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // exitXToolStripMenuItem
            // 
            resources.ApplyResources(exitXToolStripMenuItem, "exitXToolStripMenuItem");
            exitXToolStripMenuItem.ForeColor = Color.Red;
            exitXToolStripMenuItem.Name = "exitXToolStripMenuItem";
            exitXToolStripMenuItem.Click += ExitXToolStripMenuItem_Click;
            // 
            // settingsSToolStripMenuItem
            // 
            settingsSToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { convertSettingsToolStripMenuItem, saveMethodSettingsMToolStripMenuItem });
            settingsSToolStripMenuItem.Name = "settingsSToolStripMenuItem";
            resources.ApplyResources(settingsSToolStripMenuItem, "settingsSToolStripMenuItem");
            // 
            // convertSettingsToolStripMenuItem
            // 
            convertSettingsToolStripMenuItem.Name = "convertSettingsToolStripMenuItem";
            resources.ApplyResources(convertSettingsToolStripMenuItem, "convertSettingsToolStripMenuItem");
            convertSettingsToolStripMenuItem.Click += ConvertSettingsToolStripMenuItem_Click;
            // 
            // saveMethodSettingsMToolStripMenuItem
            // 
            saveMethodSettingsMToolStripMenuItem.Name = "saveMethodSettingsMToolStripMenuItem";
            resources.ApplyResources(saveMethodSettingsMToolStripMenuItem, "saveMethodSettingsMToolStripMenuItem");
            saveMethodSettingsMToolStripMenuItem.Click += PreferencesMToolStripMenuItem_Click;
            // 
            // toolsTToolStripMenuItem
            // 
            toolsTToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { convertAudioToolStripMenuItem, loopPointCreationToolStripMenuItem });
            toolsTToolStripMenuItem.Name = "toolsTToolStripMenuItem";
            resources.ApplyResources(toolsTToolStripMenuItem, "toolsTToolStripMenuItem");
            // 
            // convertAudioToolStripMenuItem
            // 
            convertAudioToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { audioToWAVEToolStripMenuItem, wAVEToAudioToolStripMenuItem });
            convertAudioToolStripMenuItem.Name = "convertAudioToolStripMenuItem";
            resources.ApplyResources(convertAudioToolStripMenuItem, "convertAudioToolStripMenuItem");
            // 
            // audioToWAVEToolStripMenuItem
            // 
            audioToWAVEToolStripMenuItem.Name = "audioToWAVEToolStripMenuItem";
            resources.ApplyResources(audioToWAVEToolStripMenuItem, "audioToWAVEToolStripMenuItem");
            audioToWAVEToolStripMenuItem.Click += AudioToWAVEToolStripMenuItem_Click;
            // 
            // wAVEToAudioToolStripMenuItem
            // 
            wAVEToAudioToolStripMenuItem.Name = "wAVEToAudioToolStripMenuItem";
            resources.ApplyResources(wAVEToAudioToolStripMenuItem, "wAVEToAudioToolStripMenuItem");
            wAVEToAudioToolStripMenuItem.Click += WAVEToAudioToolStripMenuItem_Click;
            // 
            // loopPointCreationToolStripMenuItem
            // 
            resources.ApplyResources(loopPointCreationToolStripMenuItem, "loopPointCreationToolStripMenuItem");
            loopPointCreationToolStripMenuItem.Name = "loopPointCreationToolStripMenuItem";
            loopPointCreationToolStripMenuItem.Click += LoopPointCreationToolStripMenuItem_Click;
            // 
            // helpHToolStripMenuItem
            // 
            helpHToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
            helpHToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutATRACToolToolStripMenuItem, toolStripMenuItem2, checkForUpdatesUToolStripMenuItem });
            helpHToolStripMenuItem.Name = "helpHToolStripMenuItem";
            resources.ApplyResources(helpHToolStripMenuItem, "helpHToolStripMenuItem");
            // 
            // aboutATRACToolToolStripMenuItem
            // 
            aboutATRACToolToolStripMenuItem.Name = "aboutATRACToolToolStripMenuItem";
            resources.ApplyResources(aboutATRACToolToolStripMenuItem, "aboutATRACToolToolStripMenuItem");
            aboutATRACToolToolStripMenuItem.Click += AboutATRACToolToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(toolStripMenuItem2, "toolStripMenuItem2");
            // 
            // checkForUpdatesUToolStripMenuItem
            // 
            checkForUpdatesUToolStripMenuItem.Name = "checkForUpdatesUToolStripMenuItem";
            resources.ApplyResources(checkForUpdatesUToolStripMenuItem, "checkForUpdatesUToolStripMenuItem");
            checkForUpdatesUToolStripMenuItem.Click += CheckForUpdatesUToolStripMenuItem_Click;
            // 
            // button_Decode
            // 
            resources.ApplyResources(button_Decode, "button_Decode");
            button_Decode.Name = "button_Decode";
            button_Decode.UseVisualStyleBackColor = true;
            button_Decode.Click += Button_Decode_Click;
            // 
            // button_Encode
            // 
            resources.ApplyResources(button_Encode, "button_Encode");
            button_Encode.Name = "button_Encode";
            button_Encode.UseVisualStyleBackColor = true;
            button_Encode.Click += Button_Encode_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel_Status, toolStripDropDownButton_EF, toolStripStatusLabel_EncMethod });
            statusStrip1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            resources.ApplyResources(statusStrip1, "statusStrip1");
            statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel_Status
            // 
            toolStripStatusLabel_Status.Name = "toolStripStatusLabel_Status";
            resources.ApplyResources(toolStripStatusLabel_Status, "toolStripStatusLabel_Status");
            // 
            // toolStripDropDownButton_EF
            // 
            toolStripDropDownButton_EF.Alignment = ToolStripItemAlignment.Right;
            toolStripDropDownButton_EF.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton_EF.DropDownItems.AddRange(new ToolStripItem[] { aTRAC3ATRAC3ToolStripMenuItem, aTRAC9ToolStripMenuItem, toolStripMenuItem3, walkmanToolStripMenuItem });
            resources.ApplyResources(toolStripDropDownButton_EF, "toolStripDropDownButton_EF");
            toolStripDropDownButton_EF.Name = "toolStripDropDownButton_EF";
            // 
            // aTRAC3ATRAC3ToolStripMenuItem
            // 
            aTRAC3ATRAC3ToolStripMenuItem.Name = "aTRAC3ATRAC3ToolStripMenuItem";
            resources.ApplyResources(aTRAC3ATRAC3ToolStripMenuItem, "aTRAC3ATRAC3ToolStripMenuItem");
            aTRAC3ATRAC3ToolStripMenuItem.Click += ATRAC3ATRAC3ToolStripMenuItem_Click;
            // 
            // aTRAC9ToolStripMenuItem
            // 
            aTRAC9ToolStripMenuItem.Name = "aTRAC9ToolStripMenuItem";
            resources.ApplyResources(aTRAC9ToolStripMenuItem, "aTRAC9ToolStripMenuItem");
            aTRAC9ToolStripMenuItem.Click += ATRAC9ToolStripMenuItem_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            resources.ApplyResources(toolStripMenuItem3, "toolStripMenuItem3");
            // 
            // walkmanToolStripMenuItem
            // 
            walkmanToolStripMenuItem.Name = "walkmanToolStripMenuItem";
            resources.ApplyResources(walkmanToolStripMenuItem, "walkmanToolStripMenuItem");
            walkmanToolStripMenuItem.Click += walkmanToolStripMenuItem_Click;
            // 
            // toolStripStatusLabel_EncMethod
            // 
            resources.ApplyResources(toolStripStatusLabel_EncMethod, "toolStripStatusLabel_EncMethod");
            toolStripStatusLabel_EncMethod.Name = "toolStripStatusLabel_EncMethod";
            // 
            // panel_Control
            // 
            panel_Control.Controls.Add(button_Encode);
            panel_Control.Controls.Add(button_Decode);
            resources.ApplyResources(panel_Control, "panel_Control");
            panel_Control.Name = "panel_Control";
            // 
            // panel_Main
            // 
            resources.ApplyResources(panel_Main, "panel_Main");
            panel_Main.Controls.Add(label_NotReaded);
            panel_Main.Name = "panel_Main";
            // 
            // FormMain
            // 
            AllowDrop = true;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox1);
            Controls.Add(panel_Main);
            Controls.Add(panel_Control);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "FormMain";
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
        private Button button_Encode;
        private Label label_NotReaded;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel_Status;
        private ToolStripDropDownButton toolStripDropDownButton_EF;
        private ToolStripMenuItem aTRAC3ATRAC3ToolStripMenuItem;
        private ToolStripMenuItem aTRAC9ToolStripMenuItem;
        private Label label_Filepath;
        private Label label3;
        private Label label2;
        private Label label_Formattxt;
        private Label label_Sizetxt;
        private Label label4;
        private ToolStripMenuItem audioToWAVEToolStripMenuItem;
        private ToolStripMenuItem wAVEToAudioToolStripMenuItem;
        private ToolStripMenuItem loopPointCreationToolStripMenuItem;
        private ToolStripMenuItem saveMethodSettingsMToolStripMenuItem;
        private Panel panel_Control;
        private Panel panel_Main;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem walkmanToolStripMenuItem;
        private ToolStripStatusLabel toolStripStatusLabel_EncMethod;
    }
}