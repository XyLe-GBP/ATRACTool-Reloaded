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
            groupBox_IO_Behavior = new GroupBox();
            textBox_Path = new TextBox();
            label_IO_Path = new Label();
            button_OK = new Button();
            button_Cancel = new Button();
            button_Browse = new Button();
            button_Clear = new Button();
            checkBox_Subfolder = new CheckBox();
            label_IO_SubfolderSuffix = new Label();
            textBox_suffix = new TextBox();
            tabControl1 = new TabControl();
            tabPageGeneral = new TabPage();
            checkBox_ATRACEncodeSource = new CheckBox();
            checkBox_Hidesplash = new CheckBox();
            checkBox_DisablePreviewWarning = new CheckBox();
            button_Splashimg = new Button();
            textBox_Splashimg = new TextBox();
            checkBox_Splashimg = new CheckBox();
            checkBox_Checkupdate = new CheckBox();
            tabPageIO = new TabPage();
            checkBox_SaveDeleteHzSuffix = new CheckBox();
            checkBox_IO_SaveSourcesnest = new CheckBox();
            checkBox_ShowFolder = new CheckBox();
            tabPageLPC = new TabPage();
            checkBox_EnableATRACPlayback = new CheckBox();
            checkBox_Smoothsamples = new CheckBox();
            comboBox_LPCMultisourcePlaybackmode = new ComboBox();
            label_LPC_MultiplesourcePlaybackmode = new Label();
            comboBox_LPCASIODriver = new ComboBox();
            label_LPC_ASIODriver = new Label();
            checkBox_MultisoundDontDS = new CheckBox();
            comboBox_LPCplayback = new ComboBox();
            label_LPC_PlaybackMode = new Label();
            tabPageAdvanced = new TabPage();
            comboBox_WASAPILatencysS = new ComboBox();
            comboBox_WASAPILatencysE = new ComboBox();
            comboBox_PlaybackThreadCounts = new ComboBox();
            comboBox_DSBuffers = new ComboBox();
            comboBox_DSLatencys = new ComboBox();
            label_WASAPILatencyE = new Label();
            label_WASAPILatencyS = new Label();
            label_Usingthreads = new Label();
            label_DSLatency = new Label();
            label_DSBuffers = new Label();
            checkBox_ForceConvertWaveOnly = new CheckBox();
            comboBox_Fixconvert = new ComboBox();
            checkBox_Fixconvert = new CheckBox();
            checkBox_FasterATRAC = new CheckBox();
            checkBox_Oldmode = new CheckBox();
            toolTip_Description = new ToolTip(components);
            groupBox_IO_Behavior.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPageGeneral.SuspendLayout();
            tabPageIO.SuspendLayout();
            tabPageLPC.SuspendLayout();
            tabPageAdvanced.SuspendLayout();
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
            // groupBox_IO_Behavior
            // 
            resources.ApplyResources(groupBox_IO_Behavior, "groupBox_IO_Behavior");
            groupBox_IO_Behavior.Controls.Add(radioButton_spc);
            groupBox_IO_Behavior.Controls.Add(radioButton_nml);
            groupBox_IO_Behavior.Name = "groupBox_IO_Behavior";
            groupBox_IO_Behavior.TabStop = false;
            toolTip_Description.SetToolTip(groupBox_IO_Behavior, resources.GetString("groupBox_IO_Behavior.ToolTip"));
            // 
            // textBox_Path
            // 
            resources.ApplyResources(textBox_Path, "textBox_Path");
            textBox_Path.Name = "textBox_Path";
            textBox_Path.ReadOnly = true;
            toolTip_Description.SetToolTip(textBox_Path, resources.GetString("textBox_Path.ToolTip"));
            // 
            // label_IO_Path
            // 
            resources.ApplyResources(label_IO_Path, "label_IO_Path");
            label_IO_Path.Name = "label_IO_Path";
            toolTip_Description.SetToolTip(label_IO_Path, resources.GetString("label_IO_Path.ToolTip"));
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
            // label_IO_SubfolderSuffix
            // 
            resources.ApplyResources(label_IO_SubfolderSuffix, "label_IO_SubfolderSuffix");
            label_IO_SubfolderSuffix.Name = "label_IO_SubfolderSuffix";
            toolTip_Description.SetToolTip(label_IO_SubfolderSuffix, resources.GetString("label_IO_SubfolderSuffix.ToolTip"));
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
            tabControl1.Controls.Add(tabPageGeneral);
            tabControl1.Controls.Add(tabPageIO);
            tabControl1.Controls.Add(tabPageLPC);
            tabControl1.Controls.Add(tabPageAdvanced);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            toolTip_Description.SetToolTip(tabControl1, resources.GetString("tabControl1.ToolTip"));
            // 
            // tabPageGeneral
            // 
            resources.ApplyResources(tabPageGeneral, "tabPageGeneral");
            tabPageGeneral.Controls.Add(checkBox_ATRACEncodeSource);
            tabPageGeneral.Controls.Add(checkBox_Hidesplash);
            tabPageGeneral.Controls.Add(checkBox_DisablePreviewWarning);
            tabPageGeneral.Controls.Add(button_Splashimg);
            tabPageGeneral.Controls.Add(textBox_Splashimg);
            tabPageGeneral.Controls.Add(checkBox_Splashimg);
            tabPageGeneral.Controls.Add(checkBox_Checkupdate);
            tabPageGeneral.Name = "tabPageGeneral";
            toolTip_Description.SetToolTip(tabPageGeneral, resources.GetString("tabPageGeneral.ToolTip"));
            tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // checkBox_ATRACEncodeSource
            // 
            resources.ApplyResources(checkBox_ATRACEncodeSource, "checkBox_ATRACEncodeSource");
            checkBox_ATRACEncodeSource.Name = "checkBox_ATRACEncodeSource";
            toolTip_Description.SetToolTip(checkBox_ATRACEncodeSource, resources.GetString("checkBox_ATRACEncodeSource.ToolTip"));
            checkBox_ATRACEncodeSource.UseVisualStyleBackColor = true;
            // 
            // checkBox_Hidesplash
            // 
            resources.ApplyResources(checkBox_Hidesplash, "checkBox_Hidesplash");
            checkBox_Hidesplash.Name = "checkBox_Hidesplash";
            toolTip_Description.SetToolTip(checkBox_Hidesplash, resources.GetString("checkBox_Hidesplash.ToolTip"));
            checkBox_Hidesplash.UseVisualStyleBackColor = true;
            // 
            // checkBox_DisablePreviewWarning
            // 
            resources.ApplyResources(checkBox_DisablePreviewWarning, "checkBox_DisablePreviewWarning");
            checkBox_DisablePreviewWarning.Name = "checkBox_DisablePreviewWarning";
            toolTip_Description.SetToolTip(checkBox_DisablePreviewWarning, resources.GetString("checkBox_DisablePreviewWarning.ToolTip"));
            checkBox_DisablePreviewWarning.UseVisualStyleBackColor = true;
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
            // tabPageIO
            // 
            resources.ApplyResources(tabPageIO, "tabPageIO");
            tabPageIO.Controls.Add(checkBox_SaveDeleteHzSuffix);
            tabPageIO.Controls.Add(checkBox_IO_SaveSourcesnest);
            tabPageIO.Controls.Add(checkBox_ShowFolder);
            tabPageIO.Controls.Add(textBox_Path);
            tabPageIO.Controls.Add(textBox_suffix);
            tabPageIO.Controls.Add(groupBox_IO_Behavior);
            tabPageIO.Controls.Add(label_IO_SubfolderSuffix);
            tabPageIO.Controls.Add(label_IO_Path);
            tabPageIO.Controls.Add(checkBox_Subfolder);
            tabPageIO.Controls.Add(button_Browse);
            tabPageIO.Controls.Add(button_Clear);
            tabPageIO.Name = "tabPageIO";
            toolTip_Description.SetToolTip(tabPageIO, resources.GetString("tabPageIO.ToolTip"));
            tabPageIO.UseVisualStyleBackColor = true;
            // 
            // checkBox_SaveDeleteHzSuffix
            // 
            resources.ApplyResources(checkBox_SaveDeleteHzSuffix, "checkBox_SaveDeleteHzSuffix");
            checkBox_SaveDeleteHzSuffix.Name = "checkBox_SaveDeleteHzSuffix";
            toolTip_Description.SetToolTip(checkBox_SaveDeleteHzSuffix, resources.GetString("checkBox_SaveDeleteHzSuffix.ToolTip"));
            checkBox_SaveDeleteHzSuffix.UseVisualStyleBackColor = true;
            // 
            // checkBox_IO_SaveSourcesnest
            // 
            resources.ApplyResources(checkBox_IO_SaveSourcesnest, "checkBox_IO_SaveSourcesnest");
            checkBox_IO_SaveSourcesnest.Name = "checkBox_IO_SaveSourcesnest";
            toolTip_Description.SetToolTip(checkBox_IO_SaveSourcesnest, resources.GetString("checkBox_IO_SaveSourcesnest.ToolTip"));
            checkBox_IO_SaveSourcesnest.UseVisualStyleBackColor = true;
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
            // tabPageLPC
            // 
            resources.ApplyResources(tabPageLPC, "tabPageLPC");
            tabPageLPC.Controls.Add(checkBox_EnableATRACPlayback);
            tabPageLPC.Controls.Add(checkBox_Smoothsamples);
            tabPageLPC.Controls.Add(comboBox_LPCMultisourcePlaybackmode);
            tabPageLPC.Controls.Add(label_LPC_MultiplesourcePlaybackmode);
            tabPageLPC.Controls.Add(comboBox_LPCASIODriver);
            tabPageLPC.Controls.Add(label_LPC_ASIODriver);
            tabPageLPC.Controls.Add(checkBox_MultisoundDontDS);
            tabPageLPC.Controls.Add(comboBox_LPCplayback);
            tabPageLPC.Controls.Add(label_LPC_PlaybackMode);
            tabPageLPC.Name = "tabPageLPC";
            toolTip_Description.SetToolTip(tabPageLPC, resources.GetString("tabPageLPC.ToolTip"));
            tabPageLPC.UseVisualStyleBackColor = true;
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
            checkBox_Smoothsamples.CheckedChanged += checkBox_Smoothsamples_CheckedChanged;
            // 
            // comboBox_LPCMultisourcePlaybackmode
            // 
            resources.ApplyResources(comboBox_LPCMultisourcePlaybackmode, "comboBox_LPCMultisourcePlaybackmode");
            comboBox_LPCMultisourcePlaybackmode.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_LPCMultisourcePlaybackmode.FormattingEnabled = true;
            comboBox_LPCMultisourcePlaybackmode.Items.AddRange(new object[] { resources.GetString("comboBox_LPCMultisourcePlaybackmode.Items"), resources.GetString("comboBox_LPCMultisourcePlaybackmode.Items1"), resources.GetString("comboBox_LPCMultisourcePlaybackmode.Items2") });
            comboBox_LPCMultisourcePlaybackmode.Name = "comboBox_LPCMultisourcePlaybackmode";
            toolTip_Description.SetToolTip(comboBox_LPCMultisourcePlaybackmode, resources.GetString("comboBox_LPCMultisourcePlaybackmode.ToolTip"));
            comboBox_LPCMultisourcePlaybackmode.SelectedIndexChanged += ComboBox_LPCMultisourcePlaybackmode_SelectedIndexChanged;
            // 
            // label_LPC_MultiplesourcePlaybackmode
            // 
            resources.ApplyResources(label_LPC_MultiplesourcePlaybackmode, "label_LPC_MultiplesourcePlaybackmode");
            label_LPC_MultiplesourcePlaybackmode.Name = "label_LPC_MultiplesourcePlaybackmode";
            toolTip_Description.SetToolTip(label_LPC_MultiplesourcePlaybackmode, resources.GetString("label_LPC_MultiplesourcePlaybackmode.ToolTip"));
            // 
            // comboBox_LPCASIODriver
            // 
            resources.ApplyResources(comboBox_LPCASIODriver, "comboBox_LPCASIODriver");
            comboBox_LPCASIODriver.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_LPCASIODriver.FormattingEnabled = true;
            comboBox_LPCASIODriver.Name = "comboBox_LPCASIODriver";
            toolTip_Description.SetToolTip(comboBox_LPCASIODriver, resources.GetString("comboBox_LPCASIODriver.ToolTip"));
            // 
            // label_LPC_ASIODriver
            // 
            resources.ApplyResources(label_LPC_ASIODriver, "label_LPC_ASIODriver");
            label_LPC_ASIODriver.Name = "label_LPC_ASIODriver";
            toolTip_Description.SetToolTip(label_LPC_ASIODriver, resources.GetString("label_LPC_ASIODriver.ToolTip"));
            // 
            // checkBox_MultisoundDontDS
            // 
            resources.ApplyResources(checkBox_MultisoundDontDS, "checkBox_MultisoundDontDS");
            checkBox_MultisoundDontDS.Checked = true;
            checkBox_MultisoundDontDS.CheckState = CheckState.Checked;
            checkBox_MultisoundDontDS.Name = "checkBox_MultisoundDontDS";
            toolTip_Description.SetToolTip(checkBox_MultisoundDontDS, resources.GetString("checkBox_MultisoundDontDS.ToolTip"));
            checkBox_MultisoundDontDS.UseVisualStyleBackColor = true;
            checkBox_MultisoundDontDS.CheckedChanged += CheckBox_MultisoundDontDS_CheckedChanged;
            // 
            // comboBox_LPCplayback
            // 
            resources.ApplyResources(comboBox_LPCplayback, "comboBox_LPCplayback");
            comboBox_LPCplayback.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_LPCplayback.FormattingEnabled = true;
            comboBox_LPCplayback.Items.AddRange(new object[] { resources.GetString("comboBox_LPCplayback.Items"), resources.GetString("comboBox_LPCplayback.Items1"), resources.GetString("comboBox_LPCplayback.Items2"), resources.GetString("comboBox_LPCplayback.Items3") });
            comboBox_LPCplayback.Name = "comboBox_LPCplayback";
            toolTip_Description.SetToolTip(comboBox_LPCplayback, resources.GetString("comboBox_LPCplayback.ToolTip"));
            comboBox_LPCplayback.SelectedIndexChanged += ComboBox_LPCplayback_SelectedIndexChanged;
            // 
            // label_LPC_PlaybackMode
            // 
            resources.ApplyResources(label_LPC_PlaybackMode, "label_LPC_PlaybackMode");
            label_LPC_PlaybackMode.Name = "label_LPC_PlaybackMode";
            toolTip_Description.SetToolTip(label_LPC_PlaybackMode, resources.GetString("label_LPC_PlaybackMode.ToolTip"));
            // 
            // tabPageAdvanced
            // 
            resources.ApplyResources(tabPageAdvanced, "tabPageAdvanced");
            tabPageAdvanced.Controls.Add(comboBox_WASAPILatencysS);
            tabPageAdvanced.Controls.Add(comboBox_WASAPILatencysE);
            tabPageAdvanced.Controls.Add(comboBox_PlaybackThreadCounts);
            tabPageAdvanced.Controls.Add(comboBox_DSBuffers);
            tabPageAdvanced.Controls.Add(comboBox_DSLatencys);
            tabPageAdvanced.Controls.Add(label_WASAPILatencyE);
            tabPageAdvanced.Controls.Add(label_WASAPILatencyS);
            tabPageAdvanced.Controls.Add(label_Usingthreads);
            tabPageAdvanced.Controls.Add(label_DSLatency);
            tabPageAdvanced.Controls.Add(label_DSBuffers);
            tabPageAdvanced.Controls.Add(checkBox_ForceConvertWaveOnly);
            tabPageAdvanced.Controls.Add(comboBox_Fixconvert);
            tabPageAdvanced.Controls.Add(checkBox_Fixconvert);
            tabPageAdvanced.Controls.Add(checkBox_FasterATRAC);
            tabPageAdvanced.Controls.Add(checkBox_Oldmode);
            tabPageAdvanced.Name = "tabPageAdvanced";
            toolTip_Description.SetToolTip(tabPageAdvanced, resources.GetString("tabPageAdvanced.ToolTip"));
            tabPageAdvanced.UseVisualStyleBackColor = true;
            // 
            // comboBox_WASAPILatencysS
            // 
            resources.ApplyResources(comboBox_WASAPILatencysS, "comboBox_WASAPILatencysS");
            comboBox_WASAPILatencysS.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_WASAPILatencysS.FormattingEnabled = true;
            comboBox_WASAPILatencysS.Items.AddRange(new object[] { resources.GetString("comboBox_WASAPILatencysS.Items"), resources.GetString("comboBox_WASAPILatencysS.Items1"), resources.GetString("comboBox_WASAPILatencysS.Items2"), resources.GetString("comboBox_WASAPILatencysS.Items3"), resources.GetString("comboBox_WASAPILatencysS.Items4"), resources.GetString("comboBox_WASAPILatencysS.Items5"), resources.GetString("comboBox_WASAPILatencysS.Items6"), resources.GetString("comboBox_WASAPILatencysS.Items7") });
            comboBox_WASAPILatencysS.Name = "comboBox_WASAPILatencysS";
            toolTip_Description.SetToolTip(comboBox_WASAPILatencysS, resources.GetString("comboBox_WASAPILatencysS.ToolTip"));
            // 
            // comboBox_WASAPILatencysE
            // 
            resources.ApplyResources(comboBox_WASAPILatencysE, "comboBox_WASAPILatencysE");
            comboBox_WASAPILatencysE.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_WASAPILatencysE.FormattingEnabled = true;
            comboBox_WASAPILatencysE.Items.AddRange(new object[] { resources.GetString("comboBox_WASAPILatencysE.Items"), resources.GetString("comboBox_WASAPILatencysE.Items1"), resources.GetString("comboBox_WASAPILatencysE.Items2"), resources.GetString("comboBox_WASAPILatencysE.Items3"), resources.GetString("comboBox_WASAPILatencysE.Items4"), resources.GetString("comboBox_WASAPILatencysE.Items5"), resources.GetString("comboBox_WASAPILatencysE.Items6"), resources.GetString("comboBox_WASAPILatencysE.Items7") });
            comboBox_WASAPILatencysE.Name = "comboBox_WASAPILatencysE";
            toolTip_Description.SetToolTip(comboBox_WASAPILatencysE, resources.GetString("comboBox_WASAPILatencysE.ToolTip"));
            // 
            // comboBox_PlaybackThreadCounts
            // 
            resources.ApplyResources(comboBox_PlaybackThreadCounts, "comboBox_PlaybackThreadCounts");
            comboBox_PlaybackThreadCounts.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_PlaybackThreadCounts.FormattingEnabled = true;
            comboBox_PlaybackThreadCounts.Items.AddRange(new object[] { resources.GetString("comboBox_PlaybackThreadCounts.Items"), resources.GetString("comboBox_PlaybackThreadCounts.Items1"), resources.GetString("comboBox_PlaybackThreadCounts.Items2"), resources.GetString("comboBox_PlaybackThreadCounts.Items3"), resources.GetString("comboBox_PlaybackThreadCounts.Items4"), resources.GetString("comboBox_PlaybackThreadCounts.Items5"), resources.GetString("comboBox_PlaybackThreadCounts.Items6"), resources.GetString("comboBox_PlaybackThreadCounts.Items7") });
            comboBox_PlaybackThreadCounts.Name = "comboBox_PlaybackThreadCounts";
            toolTip_Description.SetToolTip(comboBox_PlaybackThreadCounts, resources.GetString("comboBox_PlaybackThreadCounts.ToolTip"));
            // 
            // comboBox_DSBuffers
            // 
            resources.ApplyResources(comboBox_DSBuffers, "comboBox_DSBuffers");
            comboBox_DSBuffers.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_DSBuffers.FormattingEnabled = true;
            comboBox_DSBuffers.Items.AddRange(new object[] { resources.GetString("comboBox_DSBuffers.Items"), resources.GetString("comboBox_DSBuffers.Items1"), resources.GetString("comboBox_DSBuffers.Items2"), resources.GetString("comboBox_DSBuffers.Items3") });
            comboBox_DSBuffers.Name = "comboBox_DSBuffers";
            toolTip_Description.SetToolTip(comboBox_DSBuffers, resources.GetString("comboBox_DSBuffers.ToolTip"));
            // 
            // comboBox_DSLatencys
            // 
            resources.ApplyResources(comboBox_DSLatencys, "comboBox_DSLatencys");
            comboBox_DSLatencys.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_DSLatencys.FormattingEnabled = true;
            comboBox_DSLatencys.Items.AddRange(new object[] { resources.GetString("comboBox_DSLatencys.Items"), resources.GetString("comboBox_DSLatencys.Items1"), resources.GetString("comboBox_DSLatencys.Items2"), resources.GetString("comboBox_DSLatencys.Items3"), resources.GetString("comboBox_DSLatencys.Items4"), resources.GetString("comboBox_DSLatencys.Items5"), resources.GetString("comboBox_DSLatencys.Items6"), resources.GetString("comboBox_DSLatencys.Items7"), resources.GetString("comboBox_DSLatencys.Items8") });
            comboBox_DSLatencys.Name = "comboBox_DSLatencys";
            toolTip_Description.SetToolTip(comboBox_DSLatencys, resources.GetString("comboBox_DSLatencys.ToolTip"));
            // 
            // label_WASAPILatencyE
            // 
            resources.ApplyResources(label_WASAPILatencyE, "label_WASAPILatencyE");
            label_WASAPILatencyE.Name = "label_WASAPILatencyE";
            toolTip_Description.SetToolTip(label_WASAPILatencyE, resources.GetString("label_WASAPILatencyE.ToolTip"));
            // 
            // label_WASAPILatencyS
            // 
            resources.ApplyResources(label_WASAPILatencyS, "label_WASAPILatencyS");
            label_WASAPILatencyS.Name = "label_WASAPILatencyS";
            toolTip_Description.SetToolTip(label_WASAPILatencyS, resources.GetString("label_WASAPILatencyS.ToolTip"));
            // 
            // label_Usingthreads
            // 
            resources.ApplyResources(label_Usingthreads, "label_Usingthreads");
            label_Usingthreads.Name = "label_Usingthreads";
            toolTip_Description.SetToolTip(label_Usingthreads, resources.GetString("label_Usingthreads.ToolTip"));
            // 
            // label_DSLatency
            // 
            resources.ApplyResources(label_DSLatency, "label_DSLatency");
            label_DSLatency.Name = "label_DSLatency";
            toolTip_Description.SetToolTip(label_DSLatency, resources.GetString("label_DSLatency.ToolTip"));
            // 
            // label_DSBuffers
            // 
            resources.ApplyResources(label_DSBuffers, "label_DSBuffers");
            label_DSBuffers.Name = "label_DSBuffers";
            toolTip_Description.SetToolTip(label_DSBuffers, resources.GetString("label_DSBuffers.ToolTip"));
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
            groupBox_IO_Behavior.ResumeLayout(false);
            groupBox_IO_Behavior.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPageGeneral.ResumeLayout(false);
            tabPageGeneral.PerformLayout();
            tabPageIO.ResumeLayout(false);
            tabPageIO.PerformLayout();
            tabPageLPC.ResumeLayout(false);
            tabPageLPC.PerformLayout();
            tabPageAdvanced.ResumeLayout(false);
            tabPageAdvanced.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private RadioButton radioButton_nml;
        private RadioButton radioButton_spc;
        private GroupBox groupBox_IO_Behavior;
        private TextBox textBox_Path;
        private Label label_IO_Path;
        private Button button_OK;
        private Button button_Cancel;
        private Button button_Browse;
        private Button button_Clear;
        private CheckBox checkBox_Subfolder;
        private Label label_IO_SubfolderSuffix;
        private TextBox textBox_suffix;
        private TabControl tabControl1;
        private TabPage tabPageGeneral;
        private TabPage tabPageIO;
        private CheckBox checkBox_ShowFolder;
        private CheckBox checkBox_Checkupdate;
        private TabPage tabPageAdvanced;
        private CheckBox checkBox_Oldmode;
        private Button button_Splashimg;
        private TextBox textBox_Splashimg;
        private CheckBox checkBox_Splashimg;
        private CheckBox checkBox_FasterATRAC;
        private ComboBox comboBox_Fixconvert;
        private CheckBox checkBox_Fixconvert;
        private CheckBox checkBox_ForceConvertWaveOnly;
        private ToolTip toolTip_Description;
        private CheckBox checkBox_DisablePreviewWarning;
        private TabPage tabPageLPC;
        private ComboBox comboBox_LPCplayback;
        private Label label_LPC_PlaybackMode;
        private Label label_LPC_ASIODriver;
        private CheckBox checkBox_MultisoundDontDS;
        private ComboBox comboBox_LPCMultisourcePlaybackmode;
        private Label label_LPC_MultiplesourcePlaybackmode;
        private ComboBox comboBox_LPCASIODriver;
        private CheckBox checkBox_Hidesplash;
        private CheckBox checkBox_Smoothsamples;
        private CheckBox checkBox_EnableATRACPlayback;
        private CheckBox checkBox_ATRACEncodeSource;
        private CheckBox checkBox_IO_SaveSourcesnest;
        private Label label_WASAPILatencyE;
        private Label label_WASAPILatencyS;
        private Label label_Usingthreads;
        private Label label_DSLatency;
        private Label label_DSBuffers;
        private ComboBox comboBox_DSLatencys;
        private ComboBox comboBox_WASAPILatencysS;
        private ComboBox comboBox_WASAPILatencysE;
        private ComboBox comboBox_PlaybackThreadCounts;
        private ComboBox comboBox_DSBuffers;
        private CheckBox checkBox_SaveDeleteHzSuffix;
    }
}