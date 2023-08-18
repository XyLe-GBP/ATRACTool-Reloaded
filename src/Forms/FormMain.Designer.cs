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
            resources.ApplyResources(groupBox1, "groupBox1");
            groupBox1.Controls.Add(label_Sizetxt);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label_Formattxt);
            groupBox1.Controls.Add(label_Filepath);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
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
            resources.ApplyResources(label_Filepath, "label_Filepath");
            label_Filepath.AutoEllipsis = true;
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
            // 
            // menuStrip1
            // 
            resources.ApplyResources(menuStrip1, "menuStrip1");
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileFToolStripMenuItem, settingsSToolStripMenuItem, toolsTToolStripMenuItem, helpHToolStripMenuItem });
            menuStrip1.Name = "menuStrip1";
            // 
            // fileFToolStripMenuItem
            // 
            resources.ApplyResources(fileFToolStripMenuItem, "fileFToolStripMenuItem");
            fileFToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openFileOToolStripMenuItem, closeFileCToolStripMenuItem, toolStripMenuItem1, exitXToolStripMenuItem });
            fileFToolStripMenuItem.Name = "fileFToolStripMenuItem";
            // 
            // openFileOToolStripMenuItem
            // 
            resources.ApplyResources(openFileOToolStripMenuItem, "openFileOToolStripMenuItem");
            openFileOToolStripMenuItem.Name = "openFileOToolStripMenuItem";
            openFileOToolStripMenuItem.Click += OpenFileOToolStripMenuItem_Click;
            // 
            // closeFileCToolStripMenuItem
            // 
            resources.ApplyResources(closeFileCToolStripMenuItem, "closeFileCToolStripMenuItem");
            closeFileCToolStripMenuItem.Name = "closeFileCToolStripMenuItem";
            closeFileCToolStripMenuItem.Click += CloseFileCToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            resources.ApplyResources(toolStripMenuItem1, "toolStripMenuItem1");
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            // 
            // exitXToolStripMenuItem
            // 
            resources.ApplyResources(exitXToolStripMenuItem, "exitXToolStripMenuItem");
            exitXToolStripMenuItem.Name = "exitXToolStripMenuItem";
            exitXToolStripMenuItem.Click += ExitXToolStripMenuItem_Click;
            // 
            // settingsSToolStripMenuItem
            // 
            resources.ApplyResources(settingsSToolStripMenuItem, "settingsSToolStripMenuItem");
            settingsSToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { convertSettingsToolStripMenuItem, saveMethodSettingsMToolStripMenuItem });
            settingsSToolStripMenuItem.Name = "settingsSToolStripMenuItem";
            // 
            // convertSettingsToolStripMenuItem
            // 
            resources.ApplyResources(convertSettingsToolStripMenuItem, "convertSettingsToolStripMenuItem");
            convertSettingsToolStripMenuItem.Name = "convertSettingsToolStripMenuItem";
            convertSettingsToolStripMenuItem.Click += ConvertSettingsToolStripMenuItem_Click;
            // 
            // saveMethodSettingsMToolStripMenuItem
            // 
            resources.ApplyResources(saveMethodSettingsMToolStripMenuItem, "saveMethodSettingsMToolStripMenuItem");
            saveMethodSettingsMToolStripMenuItem.Name = "saveMethodSettingsMToolStripMenuItem";
            saveMethodSettingsMToolStripMenuItem.Click += PreferencesMToolStripMenuItem_Click;
            // 
            // toolsTToolStripMenuItem
            // 
            resources.ApplyResources(toolsTToolStripMenuItem, "toolsTToolStripMenuItem");
            toolsTToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { convertAudioToolStripMenuItem, loopPointCreationToolStripMenuItem });
            toolsTToolStripMenuItem.Name = "toolsTToolStripMenuItem";
            // 
            // convertAudioToolStripMenuItem
            // 
            resources.ApplyResources(convertAudioToolStripMenuItem, "convertAudioToolStripMenuItem");
            convertAudioToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { audioToWAVEToolStripMenuItem, wAVEToAudioToolStripMenuItem });
            convertAudioToolStripMenuItem.Name = "convertAudioToolStripMenuItem";
            convertAudioToolStripMenuItem.Click += ConvertAudioToolStripMenuItem_Click;
            // 
            // audioToWAVEToolStripMenuItem
            // 
            resources.ApplyResources(audioToWAVEToolStripMenuItem, "audioToWAVEToolStripMenuItem");
            audioToWAVEToolStripMenuItem.Name = "audioToWAVEToolStripMenuItem";
            audioToWAVEToolStripMenuItem.Click += AudioToWAVEToolStripMenuItem_Click;
            // 
            // wAVEToAudioToolStripMenuItem
            // 
            resources.ApplyResources(wAVEToAudioToolStripMenuItem, "wAVEToAudioToolStripMenuItem");
            wAVEToAudioToolStripMenuItem.Name = "wAVEToAudioToolStripMenuItem";
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
            resources.ApplyResources(helpHToolStripMenuItem, "helpHToolStripMenuItem");
            helpHToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
            helpHToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutATRACToolToolStripMenuItem, toolStripMenuItem2, checkForUpdatesUToolStripMenuItem });
            helpHToolStripMenuItem.Name = "helpHToolStripMenuItem";
            // 
            // aboutATRACToolToolStripMenuItem
            // 
            resources.ApplyResources(aboutATRACToolToolStripMenuItem, "aboutATRACToolToolStripMenuItem");
            aboutATRACToolToolStripMenuItem.Name = "aboutATRACToolToolStripMenuItem";
            aboutATRACToolToolStripMenuItem.Click += AboutATRACToolToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            resources.ApplyResources(toolStripMenuItem2, "toolStripMenuItem2");
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            // 
            // checkForUpdatesUToolStripMenuItem
            // 
            resources.ApplyResources(checkForUpdatesUToolStripMenuItem, "checkForUpdatesUToolStripMenuItem");
            checkForUpdatesUToolStripMenuItem.Name = "checkForUpdatesUToolStripMenuItem";
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
            resources.ApplyResources(statusStrip1, "statusStrip1");
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel_Status, toolStripDropDownButton_EF });
            statusStrip1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel_Status
            // 
            resources.ApplyResources(toolStripStatusLabel_Status, "toolStripStatusLabel_Status");
            toolStripStatusLabel_Status.Name = "toolStripStatusLabel_Status";
            // 
            // toolStripDropDownButton_EF
            // 
            resources.ApplyResources(toolStripDropDownButton_EF, "toolStripDropDownButton_EF");
            toolStripDropDownButton_EF.Alignment = ToolStripItemAlignment.Right;
            toolStripDropDownButton_EF.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton_EF.DropDownItems.AddRange(new ToolStripItem[] { aTRAC3ATRAC3ToolStripMenuItem, aTRAC9ToolStripMenuItem });
            toolStripDropDownButton_EF.Name = "toolStripDropDownButton_EF";
            // 
            // aTRAC3ATRAC3ToolStripMenuItem
            // 
            resources.ApplyResources(aTRAC3ATRAC3ToolStripMenuItem, "aTRAC3ATRAC3ToolStripMenuItem");
            aTRAC3ATRAC3ToolStripMenuItem.Name = "aTRAC3ATRAC3ToolStripMenuItem";
            aTRAC3ATRAC3ToolStripMenuItem.Click += ATRAC3ATRAC3ToolStripMenuItem_Click;
            // 
            // aTRAC9ToolStripMenuItem
            // 
            resources.ApplyResources(aTRAC9ToolStripMenuItem, "aTRAC9ToolStripMenuItem");
            aTRAC9ToolStripMenuItem.Name = "aTRAC9ToolStripMenuItem";
            aTRAC9ToolStripMenuItem.Click += ATRAC9ToolStripMenuItem_Click;
            // 
            // panel_Control
            // 
            resources.ApplyResources(panel_Control, "panel_Control");
            panel_Control.Controls.Add(button_Encode);
            panel_Control.Controls.Add(button_Decode);
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
            resources.ApplyResources(this, "$this");
            AllowDrop = true;
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
    }
}