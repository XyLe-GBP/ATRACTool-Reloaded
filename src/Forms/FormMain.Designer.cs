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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_Sizetxt = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label_Formattxt = new System.Windows.Forms.Label();
            this.label_Filepath = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label_NotReaded = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeFileCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertAudioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.audioToWAVEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wAVEToAudioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loopPointCreationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutATRACToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.checkForUpdatesUToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button_Decode = new System.Windows.Forms.Button();
            this.button_Encode = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripDropDownButton_EF = new System.Windows.Forms.ToolStripDropDownButton();
            this.aTRAC3ATRAC3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aTRAC9ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_Sizetxt);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label_Formattxt);
            this.groupBox1.Controls.Add(this.label_Filepath);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // label_Sizetxt
            // 
            resources.ApplyResources(this.label_Sizetxt, "label_Sizetxt");
            this.label_Sizetxt.Name = "label_Sizetxt";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label_Formattxt
            // 
            resources.ApplyResources(this.label_Formattxt, "label_Formattxt");
            this.label_Formattxt.Name = "label_Formattxt";
            // 
            // label_Filepath
            // 
            this.label_Filepath.AutoEllipsis = true;
            resources.ApplyResources(this.label_Filepath, "label_Filepath");
            this.label_Filepath.Name = "label_Filepath";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label_NotReaded
            // 
            resources.ApplyResources(this.label_NotReaded, "label_NotReaded");
            this.label_NotReaded.Name = "label_NotReaded";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileFToolStripMenuItem,
            this.settingsSToolStripMenuItem,
            this.toolsTToolStripMenuItem,
            this.helpHToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileFToolStripMenuItem
            // 
            this.fileFToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileOToolStripMenuItem,
            this.closeFileCToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitXToolStripMenuItem});
            this.fileFToolStripMenuItem.Name = "fileFToolStripMenuItem";
            resources.ApplyResources(this.fileFToolStripMenuItem, "fileFToolStripMenuItem");
            // 
            // openFileOToolStripMenuItem
            // 
            this.openFileOToolStripMenuItem.Name = "openFileOToolStripMenuItem";
            resources.ApplyResources(this.openFileOToolStripMenuItem, "openFileOToolStripMenuItem");
            this.openFileOToolStripMenuItem.Click += new System.EventHandler(this.OpenFileOToolStripMenuItem_Click);
            // 
            // closeFileCToolStripMenuItem
            // 
            this.closeFileCToolStripMenuItem.Name = "closeFileCToolStripMenuItem";
            resources.ApplyResources(this.closeFileCToolStripMenuItem, "closeFileCToolStripMenuItem");
            this.closeFileCToolStripMenuItem.Click += new System.EventHandler(this.CloseFileCToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // exitXToolStripMenuItem
            // 
            this.exitXToolStripMenuItem.Name = "exitXToolStripMenuItem";
            resources.ApplyResources(this.exitXToolStripMenuItem, "exitXToolStripMenuItem");
            this.exitXToolStripMenuItem.Click += new System.EventHandler(this.ExitXToolStripMenuItem_Click);
            // 
            // settingsSToolStripMenuItem
            // 
            this.settingsSToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.convertSettingsToolStripMenuItem});
            this.settingsSToolStripMenuItem.Name = "settingsSToolStripMenuItem";
            resources.ApplyResources(this.settingsSToolStripMenuItem, "settingsSToolStripMenuItem");
            // 
            // convertSettingsToolStripMenuItem
            // 
            this.convertSettingsToolStripMenuItem.Name = "convertSettingsToolStripMenuItem";
            resources.ApplyResources(this.convertSettingsToolStripMenuItem, "convertSettingsToolStripMenuItem");
            this.convertSettingsToolStripMenuItem.Click += new System.EventHandler(this.ConvertSettingsToolStripMenuItem_Click);
            // 
            // toolsTToolStripMenuItem
            // 
            this.toolsTToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.convertAudioToolStripMenuItem,
            this.loopPointCreationToolStripMenuItem});
            this.toolsTToolStripMenuItem.Name = "toolsTToolStripMenuItem";
            resources.ApplyResources(this.toolsTToolStripMenuItem, "toolsTToolStripMenuItem");
            // 
            // convertAudioToolStripMenuItem
            // 
            this.convertAudioToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.audioToWAVEToolStripMenuItem,
            this.wAVEToAudioToolStripMenuItem});
            this.convertAudioToolStripMenuItem.Name = "convertAudioToolStripMenuItem";
            resources.ApplyResources(this.convertAudioToolStripMenuItem, "convertAudioToolStripMenuItem");
            this.convertAudioToolStripMenuItem.Click += new System.EventHandler(this.ConvertAudioToolStripMenuItem_Click);
            // 
            // audioToWAVEToolStripMenuItem
            // 
            this.audioToWAVEToolStripMenuItem.Name = "audioToWAVEToolStripMenuItem";
            resources.ApplyResources(this.audioToWAVEToolStripMenuItem, "audioToWAVEToolStripMenuItem");
            this.audioToWAVEToolStripMenuItem.Click += new System.EventHandler(this.AudioToWAVEToolStripMenuItem_Click);
            // 
            // wAVEToAudioToolStripMenuItem
            // 
            this.wAVEToAudioToolStripMenuItem.Name = "wAVEToAudioToolStripMenuItem";
            resources.ApplyResources(this.wAVEToAudioToolStripMenuItem, "wAVEToAudioToolStripMenuItem");
            this.wAVEToAudioToolStripMenuItem.Click += new System.EventHandler(this.WAVEToAudioToolStripMenuItem_Click);
            // 
            // loopPointCreationToolStripMenuItem
            // 
            this.loopPointCreationToolStripMenuItem.Name = "loopPointCreationToolStripMenuItem";
            resources.ApplyResources(this.loopPointCreationToolStripMenuItem, "loopPointCreationToolStripMenuItem");
            this.loopPointCreationToolStripMenuItem.Click += new System.EventHandler(this.LoopPointCreationToolStripMenuItem_Click);
            // 
            // helpHToolStripMenuItem
            // 
            this.helpHToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.helpHToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutATRACToolToolStripMenuItem,
            this.toolStripMenuItem2,
            this.checkForUpdatesUToolStripMenuItem});
            this.helpHToolStripMenuItem.Name = "helpHToolStripMenuItem";
            resources.ApplyResources(this.helpHToolStripMenuItem, "helpHToolStripMenuItem");
            // 
            // aboutATRACToolToolStripMenuItem
            // 
            this.aboutATRACToolToolStripMenuItem.Name = "aboutATRACToolToolStripMenuItem";
            resources.ApplyResources(this.aboutATRACToolToolStripMenuItem, "aboutATRACToolToolStripMenuItem");
            this.aboutATRACToolToolStripMenuItem.Click += new System.EventHandler(this.AboutATRACToolToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            // 
            // checkForUpdatesUToolStripMenuItem
            // 
            this.checkForUpdatesUToolStripMenuItem.Name = "checkForUpdatesUToolStripMenuItem";
            resources.ApplyResources(this.checkForUpdatesUToolStripMenuItem, "checkForUpdatesUToolStripMenuItem");
            this.checkForUpdatesUToolStripMenuItem.Click += new System.EventHandler(this.CheckForUpdatesUToolStripMenuItem_Click);
            // 
            // button_Decode
            // 
            resources.ApplyResources(this.button_Decode, "button_Decode");
            this.button_Decode.Name = "button_Decode";
            this.button_Decode.UseVisualStyleBackColor = true;
            this.button_Decode.Click += new System.EventHandler(this.Button_Decode_Click);
            // 
            // button_Encode
            // 
            resources.ApplyResources(this.button_Encode, "button_Encode");
            this.button_Encode.Name = "button_Encode";
            this.button_Encode.UseVisualStyleBackColor = true;
            this.button_Encode.Click += new System.EventHandler(this.Button_Encode_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_Status,
            this.toolStripDropDownButton_EF});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel_Status
            // 
            this.toolStripStatusLabel_Status.Name = "toolStripStatusLabel_Status";
            resources.ApplyResources(this.toolStripStatusLabel_Status, "toolStripStatusLabel_Status");
            // 
            // toolStripDropDownButton_EF
            // 
            this.toolStripDropDownButton_EF.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButton_EF.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_EF.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aTRAC3ATRAC3ToolStripMenuItem,
            this.aTRAC9ToolStripMenuItem});
            resources.ApplyResources(this.toolStripDropDownButton_EF, "toolStripDropDownButton_EF");
            this.toolStripDropDownButton_EF.Name = "toolStripDropDownButton_EF";
            // 
            // aTRAC3ATRAC3ToolStripMenuItem
            // 
            this.aTRAC3ATRAC3ToolStripMenuItem.Name = "aTRAC3ATRAC3ToolStripMenuItem";
            resources.ApplyResources(this.aTRAC3ATRAC3ToolStripMenuItem, "aTRAC3ATRAC3ToolStripMenuItem");
            this.aTRAC3ATRAC3ToolStripMenuItem.Click += new System.EventHandler(this.ATRAC3ATRAC3ToolStripMenuItem_Click);
            // 
            // aTRAC9ToolStripMenuItem
            // 
            this.aTRAC9ToolStripMenuItem.Name = "aTRAC9ToolStripMenuItem";
            resources.ApplyResources(this.aTRAC9ToolStripMenuItem, "aTRAC9ToolStripMenuItem");
            this.aTRAC9ToolStripMenuItem.Click += new System.EventHandler(this.ATRAC9ToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label_NotReaded);
            this.Controls.Add(this.button_Encode);
            this.Controls.Add(this.button_Decode);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormMain_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormMain_DragEnter);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}