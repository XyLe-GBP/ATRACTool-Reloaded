namespace ATRACTool_Reloaded
{
    partial class FormSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
            button_OK = new Button();
            button_Cancel = new Button();
            textBox_at3_cmd = new TextBox();
            textBox_at9_cmd = new TextBox();
            label1 = new Label();
            label2 = new Label();
            groupBox1 = new GroupBox();
            radioButton_PS3 = new RadioButton();
            label_at3_times = new Label();
            radioButton_PSP = new RadioButton();
            textBox_at3_looptimes = new TextBox();
            label6 = new Label();
            label_at3_nol = new Label();
            checkBox_at3_looptimes = new CheckBox();
            label_at3_samples = new Label();
            textBox_at3_loopend = new TextBox();
            textBox_at3_loopstart = new TextBox();
            label_at3_loopend = new Label();
            label_at3_loopstart = new Label();
            checkBox_at3_loopsound = new CheckBox();
            checkBox_at3_looppoint = new CheckBox();
            comboBox_at3_encmethod = new ComboBox();
            label3 = new Label();
            groupBox2 = new GroupBox();
            radioButton_PS4 = new RadioButton();
            radioButton_PSV = new RadioButton();
            label_at9_times = new Label();
            textBox_at9_looptimes = new TextBox();
            label_at9_samples = new Label();
            textBox_at9_loopend = new TextBox();
            textBox_at9_loopstart = new TextBox();
            label_at9_loopstart = new Label();
            checkBox_at9_looptimes = new CheckBox();
            button_at9_looplist = new Button();
            textBox_at9_looplist = new TextBox();
            checkBox_at9_looplist = new CheckBox();
            checkBox_at9_loopsound = new CheckBox();
            checkBox_at9_looppoint = new CheckBox();
            comboBox_at9_sampling = new ComboBox();
            label5 = new Label();
            comboBox_at9_bitrate = new ComboBox();
            label4 = new Label();
            label_at9_loopend = new Label();
            label7 = new Label();
            label_at9_nol = new Label();
            groupBox3 = new GroupBox();
            checkBox_LFE = new CheckBox();
            checkBox_wband = new CheckBox();
            checkBox_bex = new CheckBox();
            comboBox_at9_enctype = new ComboBox();
            label_at9_enctype = new Label();
            comboBox_at9_startband = new ComboBox();
            checkBox_at9_enctype = new CheckBox();
            comboBox_at9_useband = new ComboBox();
            label_at9_useband = new Label();
            label_at9_startband = new Label();
            checkBox_at9_advband = new CheckBox();
            checkBox_at9_dualenc = new CheckBox();
            checkBox_at9_supframe = new CheckBox();
            checkBox_at9_advanced = new CheckBox();
            checkBox_lpcreate = new CheckBox();
            toolTip_info = new ToolTip(components);
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // button_OK
            // 
            resources.ApplyResources(button_OK, "button_OK");
            button_OK.Name = "button_OK";
            toolTip_info.SetToolTip(button_OK, resources.GetString("button_OK.ToolTip"));
            button_OK.UseVisualStyleBackColor = true;
            button_OK.Click += Button_OK_Click;
            // 
            // button_Cancel
            // 
            resources.ApplyResources(button_Cancel, "button_Cancel");
            button_Cancel.Name = "button_Cancel";
            toolTip_info.SetToolTip(button_Cancel, resources.GetString("button_Cancel.ToolTip"));
            button_Cancel.UseVisualStyleBackColor = true;
            button_Cancel.Click += Button_Cancel_Click;
            // 
            // textBox_at3_cmd
            // 
            resources.ApplyResources(textBox_at3_cmd, "textBox_at3_cmd");
            textBox_at3_cmd.Name = "textBox_at3_cmd";
            toolTip_info.SetToolTip(textBox_at3_cmd, resources.GetString("textBox_at3_cmd.ToolTip"));
            textBox_at3_cmd.TextChanged += TextBox_at3_cmd_TextChanged;
            // 
            // textBox_at9_cmd
            // 
            resources.ApplyResources(textBox_at9_cmd, "textBox_at9_cmd");
            textBox_at9_cmd.Name = "textBox_at9_cmd";
            toolTip_info.SetToolTip(textBox_at9_cmd, resources.GetString("textBox_at9_cmd.ToolTip"));
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            toolTip_info.SetToolTip(label1, resources.GetString("label1.ToolTip"));
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            toolTip_info.SetToolTip(label2, resources.GetString("label2.ToolTip"));
            // 
            // groupBox1
            // 
            resources.ApplyResources(groupBox1, "groupBox1");
            groupBox1.Controls.Add(radioButton_PS3);
            groupBox1.Controls.Add(label_at3_times);
            groupBox1.Controls.Add(radioButton_PSP);
            groupBox1.Controls.Add(textBox_at3_looptimes);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(label_at3_nol);
            groupBox1.Controls.Add(checkBox_at3_looptimes);
            groupBox1.Controls.Add(label_at3_samples);
            groupBox1.Controls.Add(textBox_at3_loopend);
            groupBox1.Controls.Add(textBox_at3_loopstart);
            groupBox1.Controls.Add(label_at3_loopend);
            groupBox1.Controls.Add(label_at3_loopstart);
            groupBox1.Controls.Add(checkBox_at3_loopsound);
            groupBox1.Controls.Add(checkBox_at3_looppoint);
            groupBox1.Controls.Add(comboBox_at3_encmethod);
            groupBox1.Controls.Add(label3);
            groupBox1.Name = "groupBox1";
            groupBox1.TabStop = false;
            toolTip_info.SetToolTip(groupBox1, resources.GetString("groupBox1.ToolTip"));
            // 
            // radioButton_PS3
            // 
            resources.ApplyResources(radioButton_PS3, "radioButton_PS3");
            radioButton_PS3.Name = "radioButton_PS3";
            toolTip_info.SetToolTip(radioButton_PS3, resources.GetString("radioButton_PS3.ToolTip"));
            radioButton_PS3.UseVisualStyleBackColor = true;
            radioButton_PS3.CheckedChanged += RadioButton_PS3_CheckedChanged;
            // 
            // label_at3_times
            // 
            resources.ApplyResources(label_at3_times, "label_at3_times");
            label_at3_times.Name = "label_at3_times";
            toolTip_info.SetToolTip(label_at3_times, resources.GetString("label_at3_times.ToolTip"));
            // 
            // radioButton_PSP
            // 
            resources.ApplyResources(radioButton_PSP, "radioButton_PSP");
            radioButton_PSP.Checked = true;
            radioButton_PSP.Name = "radioButton_PSP";
            radioButton_PSP.TabStop = true;
            toolTip_info.SetToolTip(radioButton_PSP, resources.GetString("radioButton_PSP.ToolTip"));
            radioButton_PSP.UseVisualStyleBackColor = true;
            radioButton_PSP.CheckedChanged += RadioButton_PSP_CheckedChanged;
            // 
            // textBox_at3_looptimes
            // 
            resources.ApplyResources(textBox_at3_looptimes, "textBox_at3_looptimes");
            textBox_at3_looptimes.Name = "textBox_at3_looptimes";
            toolTip_info.SetToolTip(textBox_at3_looptimes, resources.GetString("textBox_at3_looptimes.ToolTip"));
            textBox_at3_looptimes.TextChanged += TextBox_at3_looptimes_TextChanged;
            textBox_at3_looptimes.KeyPress += TextBox_at3_looptimes_KeyPress;
            // 
            // label6
            // 
            resources.ApplyResources(label6, "label6");
            label6.Name = "label6";
            toolTip_info.SetToolTip(label6, resources.GetString("label6.ToolTip"));
            // 
            // label_at3_nol
            // 
            resources.ApplyResources(label_at3_nol, "label_at3_nol");
            label_at3_nol.Name = "label_at3_nol";
            toolTip_info.SetToolTip(label_at3_nol, resources.GetString("label_at3_nol.ToolTip"));
            // 
            // checkBox_at3_looptimes
            // 
            resources.ApplyResources(checkBox_at3_looptimes, "checkBox_at3_looptimes");
            checkBox_at3_looptimes.Name = "checkBox_at3_looptimes";
            toolTip_info.SetToolTip(checkBox_at3_looptimes, resources.GetString("checkBox_at3_looptimes.ToolTip"));
            checkBox_at3_looptimes.UseVisualStyleBackColor = true;
            checkBox_at3_looptimes.CheckedChanged += CheckBox_at3_looptimes_CheckedChanged;
            // 
            // label_at3_samples
            // 
            resources.ApplyResources(label_at3_samples, "label_at3_samples");
            label_at3_samples.Name = "label_at3_samples";
            toolTip_info.SetToolTip(label_at3_samples, resources.GetString("label_at3_samples.ToolTip"));
            // 
            // textBox_at3_loopend
            // 
            resources.ApplyResources(textBox_at3_loopend, "textBox_at3_loopend");
            textBox_at3_loopend.Name = "textBox_at3_loopend";
            toolTip_info.SetToolTip(textBox_at3_loopend, resources.GetString("textBox_at3_loopend.ToolTip"));
            textBox_at3_loopend.TextChanged += TextBox_at3_loopend_TextChanged;
            textBox_at3_loopend.KeyPress += TextBox_at3_loopend_KeyPress;
            // 
            // textBox_at3_loopstart
            // 
            resources.ApplyResources(textBox_at3_loopstart, "textBox_at3_loopstart");
            textBox_at3_loopstart.Name = "textBox_at3_loopstart";
            toolTip_info.SetToolTip(textBox_at3_loopstart, resources.GetString("textBox_at3_loopstart.ToolTip"));
            textBox_at3_loopstart.TextChanged += TextBox_at3_loopstart_TextChanged;
            textBox_at3_loopstart.KeyPress += TextBox_at3_loopstart_KeyPress;
            // 
            // label_at3_loopend
            // 
            resources.ApplyResources(label_at3_loopend, "label_at3_loopend");
            label_at3_loopend.Name = "label_at3_loopend";
            toolTip_info.SetToolTip(label_at3_loopend, resources.GetString("label_at3_loopend.ToolTip"));
            // 
            // label_at3_loopstart
            // 
            resources.ApplyResources(label_at3_loopstart, "label_at3_loopstart");
            label_at3_loopstart.Name = "label_at3_loopstart";
            toolTip_info.SetToolTip(label_at3_loopstart, resources.GetString("label_at3_loopstart.ToolTip"));
            // 
            // checkBox_at3_loopsound
            // 
            resources.ApplyResources(checkBox_at3_loopsound, "checkBox_at3_loopsound");
            checkBox_at3_loopsound.Name = "checkBox_at3_loopsound";
            toolTip_info.SetToolTip(checkBox_at3_loopsound, resources.GetString("checkBox_at3_loopsound.ToolTip"));
            checkBox_at3_loopsound.UseVisualStyleBackColor = true;
            checkBox_at3_loopsound.CheckedChanged += CheckBox_at3_loopsound_CheckedChanged;
            // 
            // checkBox_at3_looppoint
            // 
            resources.ApplyResources(checkBox_at3_looppoint, "checkBox_at3_looppoint");
            checkBox_at3_looppoint.Name = "checkBox_at3_looppoint";
            toolTip_info.SetToolTip(checkBox_at3_looppoint, resources.GetString("checkBox_at3_looppoint.ToolTip"));
            checkBox_at3_looppoint.UseVisualStyleBackColor = true;
            checkBox_at3_looppoint.CheckedChanged += CheckBox_at3_looppoint_CheckedChanged;
            // 
            // comboBox_at3_encmethod
            // 
            resources.ApplyResources(comboBox_at3_encmethod, "comboBox_at3_encmethod");
            comboBox_at3_encmethod.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_at3_encmethod.FormattingEnabled = true;
            comboBox_at3_encmethod.Items.AddRange(new object[] { resources.GetString("comboBox_at3_encmethod.Items"), resources.GetString("comboBox_at3_encmethod.Items1"), resources.GetString("comboBox_at3_encmethod.Items2"), resources.GetString("comboBox_at3_encmethod.Items3"), resources.GetString("comboBox_at3_encmethod.Items4"), resources.GetString("comboBox_at3_encmethod.Items5"), resources.GetString("comboBox_at3_encmethod.Items6"), resources.GetString("comboBox_at3_encmethod.Items7"), resources.GetString("comboBox_at3_encmethod.Items8"), resources.GetString("comboBox_at3_encmethod.Items9"), resources.GetString("comboBox_at3_encmethod.Items10"), resources.GetString("comboBox_at3_encmethod.Items11"), resources.GetString("comboBox_at3_encmethod.Items12"), resources.GetString("comboBox_at3_encmethod.Items13"), resources.GetString("comboBox_at3_encmethod.Items14"), resources.GetString("comboBox_at3_encmethod.Items15") });
            comboBox_at3_encmethod.Name = "comboBox_at3_encmethod";
            toolTip_info.SetToolTip(comboBox_at3_encmethod, resources.GetString("comboBox_at3_encmethod.ToolTip"));
            comboBox_at3_encmethod.SelectedIndexChanged += ComboBox_at3_encmethod_SelectedIndexChanged;
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Name = "label3";
            toolTip_info.SetToolTip(label3, resources.GetString("label3.ToolTip"));
            // 
            // groupBox2
            // 
            resources.ApplyResources(groupBox2, "groupBox2");
            groupBox2.Controls.Add(radioButton_PS4);
            groupBox2.Controls.Add(radioButton_PSV);
            groupBox2.Controls.Add(label_at9_times);
            groupBox2.Controls.Add(textBox_at9_looptimes);
            groupBox2.Controls.Add(label_at9_samples);
            groupBox2.Controls.Add(textBox_at9_loopend);
            groupBox2.Controls.Add(textBox_at9_loopstart);
            groupBox2.Controls.Add(label_at9_loopstart);
            groupBox2.Controls.Add(checkBox_at9_looptimes);
            groupBox2.Controls.Add(button_at9_looplist);
            groupBox2.Controls.Add(textBox_at9_looplist);
            groupBox2.Controls.Add(checkBox_at9_looplist);
            groupBox2.Controls.Add(checkBox_at9_loopsound);
            groupBox2.Controls.Add(checkBox_at9_looppoint);
            groupBox2.Controls.Add(comboBox_at9_sampling);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(comboBox_at9_bitrate);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(label_at9_loopend);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(label_at9_nol);
            groupBox2.Name = "groupBox2";
            groupBox2.TabStop = false;
            toolTip_info.SetToolTip(groupBox2, resources.GetString("groupBox2.ToolTip"));
            // 
            // radioButton_PS4
            // 
            resources.ApplyResources(radioButton_PS4, "radioButton_PS4");
            radioButton_PS4.Name = "radioButton_PS4";
            toolTip_info.SetToolTip(radioButton_PS4, resources.GetString("radioButton_PS4.ToolTip"));
            radioButton_PS4.UseVisualStyleBackColor = true;
            radioButton_PS4.CheckedChanged += RadioButton_PS4_CheckedChanged;
            // 
            // radioButton_PSV
            // 
            resources.ApplyResources(radioButton_PSV, "radioButton_PSV");
            radioButton_PSV.Checked = true;
            radioButton_PSV.Name = "radioButton_PSV";
            radioButton_PSV.TabStop = true;
            toolTip_info.SetToolTip(radioButton_PSV, resources.GetString("radioButton_PSV.ToolTip"));
            radioButton_PSV.UseVisualStyleBackColor = true;
            radioButton_PSV.CheckedChanged += RadioButton_PSV_CheckedChanged;
            // 
            // label_at9_times
            // 
            resources.ApplyResources(label_at9_times, "label_at9_times");
            label_at9_times.Name = "label_at9_times";
            toolTip_info.SetToolTip(label_at9_times, resources.GetString("label_at9_times.ToolTip"));
            // 
            // textBox_at9_looptimes
            // 
            resources.ApplyResources(textBox_at9_looptimes, "textBox_at9_looptimes");
            textBox_at9_looptimes.Name = "textBox_at9_looptimes";
            toolTip_info.SetToolTip(textBox_at9_looptimes, resources.GetString("textBox_at9_looptimes.ToolTip"));
            textBox_at9_looptimes.TextChanged += TextBox_at9_looptimes_TextChanged;
            textBox_at9_looptimes.KeyPress += TextBox_at9_looptimes_KeyPress;
            // 
            // label_at9_samples
            // 
            resources.ApplyResources(label_at9_samples, "label_at9_samples");
            label_at9_samples.Name = "label_at9_samples";
            toolTip_info.SetToolTip(label_at9_samples, resources.GetString("label_at9_samples.ToolTip"));
            // 
            // textBox_at9_loopend
            // 
            resources.ApplyResources(textBox_at9_loopend, "textBox_at9_loopend");
            textBox_at9_loopend.Name = "textBox_at9_loopend";
            toolTip_info.SetToolTip(textBox_at9_loopend, resources.GetString("textBox_at9_loopend.ToolTip"));
            textBox_at9_loopend.TextChanged += TextBox_at9_loopend_TextChanged;
            textBox_at9_loopend.KeyPress += TextBox_at9_loopend_KeyPress;
            // 
            // textBox_at9_loopstart
            // 
            resources.ApplyResources(textBox_at9_loopstart, "textBox_at9_loopstart");
            textBox_at9_loopstart.Name = "textBox_at9_loopstart";
            toolTip_info.SetToolTip(textBox_at9_loopstart, resources.GetString("textBox_at9_loopstart.ToolTip"));
            textBox_at9_loopstart.TextChanged += TextBox_at9_loopstart_TextChanged;
            textBox_at9_loopstart.KeyPress += TextBox_at9_loopstart_KeyPress;
            // 
            // label_at9_loopstart
            // 
            resources.ApplyResources(label_at9_loopstart, "label_at9_loopstart");
            label_at9_loopstart.Name = "label_at9_loopstart";
            toolTip_info.SetToolTip(label_at9_loopstart, resources.GetString("label_at9_loopstart.ToolTip"));
            // 
            // checkBox_at9_looptimes
            // 
            resources.ApplyResources(checkBox_at9_looptimes, "checkBox_at9_looptimes");
            checkBox_at9_looptimes.Name = "checkBox_at9_looptimes";
            toolTip_info.SetToolTip(checkBox_at9_looptimes, resources.GetString("checkBox_at9_looptimes.ToolTip"));
            checkBox_at9_looptimes.UseVisualStyleBackColor = true;
            checkBox_at9_looptimes.CheckedChanged += CheckBox_at9_looptimes_CheckedChanged;
            // 
            // button_at9_looplist
            // 
            resources.ApplyResources(button_at9_looplist, "button_at9_looplist");
            button_at9_looplist.Name = "button_at9_looplist";
            toolTip_info.SetToolTip(button_at9_looplist, resources.GetString("button_at9_looplist.ToolTip"));
            button_at9_looplist.UseVisualStyleBackColor = true;
            button_at9_looplist.Click += Button_at9_looplist_Click;
            // 
            // textBox_at9_looplist
            // 
            resources.ApplyResources(textBox_at9_looplist, "textBox_at9_looplist");
            textBox_at9_looplist.Name = "textBox_at9_looplist";
            textBox_at9_looplist.ReadOnly = true;
            toolTip_info.SetToolTip(textBox_at9_looplist, resources.GetString("textBox_at9_looplist.ToolTip"));
            // 
            // checkBox_at9_looplist
            // 
            resources.ApplyResources(checkBox_at9_looplist, "checkBox_at9_looplist");
            checkBox_at9_looplist.Name = "checkBox_at9_looplist";
            toolTip_info.SetToolTip(checkBox_at9_looplist, resources.GetString("checkBox_at9_looplist.ToolTip"));
            checkBox_at9_looplist.UseVisualStyleBackColor = true;
            checkBox_at9_looplist.CheckedChanged += CheckBox_at9_looplist_CheckedChanged;
            // 
            // checkBox_at9_loopsound
            // 
            resources.ApplyResources(checkBox_at9_loopsound, "checkBox_at9_loopsound");
            checkBox_at9_loopsound.Name = "checkBox_at9_loopsound";
            toolTip_info.SetToolTip(checkBox_at9_loopsound, resources.GetString("checkBox_at9_loopsound.ToolTip"));
            checkBox_at9_loopsound.UseVisualStyleBackColor = true;
            checkBox_at9_loopsound.CheckedChanged += CheckBox_at9_loopsound_CheckedChanged;
            // 
            // checkBox_at9_looppoint
            // 
            resources.ApplyResources(checkBox_at9_looppoint, "checkBox_at9_looppoint");
            checkBox_at9_looppoint.Name = "checkBox_at9_looppoint";
            toolTip_info.SetToolTip(checkBox_at9_looppoint, resources.GetString("checkBox_at9_looppoint.ToolTip"));
            checkBox_at9_looppoint.UseVisualStyleBackColor = true;
            checkBox_at9_looppoint.CheckedChanged += CheckBox_at9_looppoint_CheckedChanged;
            // 
            // comboBox_at9_sampling
            // 
            resources.ApplyResources(comboBox_at9_sampling, "comboBox_at9_sampling");
            comboBox_at9_sampling.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_at9_sampling.FormattingEnabled = true;
            comboBox_at9_sampling.Items.AddRange(new object[] { resources.GetString("comboBox_at9_sampling.Items"), resources.GetString("comboBox_at9_sampling.Items1"), resources.GetString("comboBox_at9_sampling.Items2") });
            comboBox_at9_sampling.Name = "comboBox_at9_sampling";
            toolTip_info.SetToolTip(comboBox_at9_sampling, resources.GetString("comboBox_at9_sampling.ToolTip"));
            comboBox_at9_sampling.SelectedIndexChanged += ComboBox_at9_sampling_SelectedIndexChanged;
            // 
            // label5
            // 
            resources.ApplyResources(label5, "label5");
            label5.Name = "label5";
            toolTip_info.SetToolTip(label5, resources.GetString("label5.ToolTip"));
            // 
            // comboBox_at9_bitrate
            // 
            resources.ApplyResources(comboBox_at9_bitrate, "comboBox_at9_bitrate");
            comboBox_at9_bitrate.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_at9_bitrate.FormattingEnabled = true;
            comboBox_at9_bitrate.Items.AddRange(new object[] { resources.GetString("comboBox_at9_bitrate.Items"), resources.GetString("comboBox_at9_bitrate.Items1"), resources.GetString("comboBox_at9_bitrate.Items2"), resources.GetString("comboBox_at9_bitrate.Items3"), resources.GetString("comboBox_at9_bitrate.Items4"), resources.GetString("comboBox_at9_bitrate.Items5"), resources.GetString("comboBox_at9_bitrate.Items6"), resources.GetString("comboBox_at9_bitrate.Items7"), resources.GetString("comboBox_at9_bitrate.Items8") });
            comboBox_at9_bitrate.Name = "comboBox_at9_bitrate";
            toolTip_info.SetToolTip(comboBox_at9_bitrate, resources.GetString("comboBox_at9_bitrate.ToolTip"));
            comboBox_at9_bitrate.SelectedIndexChanged += ComboBox_at9_bitrate_SelectedIndexChanged;
            // 
            // label4
            // 
            resources.ApplyResources(label4, "label4");
            label4.Name = "label4";
            toolTip_info.SetToolTip(label4, resources.GetString("label4.ToolTip"));
            // 
            // label_at9_loopend
            // 
            resources.ApplyResources(label_at9_loopend, "label_at9_loopend");
            label_at9_loopend.Name = "label_at9_loopend";
            toolTip_info.SetToolTip(label_at9_loopend, resources.GetString("label_at9_loopend.ToolTip"));
            // 
            // label7
            // 
            resources.ApplyResources(label7, "label7");
            label7.Name = "label7";
            toolTip_info.SetToolTip(label7, resources.GetString("label7.ToolTip"));
            // 
            // label_at9_nol
            // 
            resources.ApplyResources(label_at9_nol, "label_at9_nol");
            label_at9_nol.Name = "label_at9_nol";
            toolTip_info.SetToolTip(label_at9_nol, resources.GetString("label_at9_nol.ToolTip"));
            // 
            // groupBox3
            // 
            resources.ApplyResources(groupBox3, "groupBox3");
            groupBox3.Controls.Add(checkBox_LFE);
            groupBox3.Controls.Add(checkBox_wband);
            groupBox3.Controls.Add(checkBox_bex);
            groupBox3.Controls.Add(comboBox_at9_enctype);
            groupBox3.Controls.Add(label_at9_enctype);
            groupBox3.Controls.Add(comboBox_at9_startband);
            groupBox3.Controls.Add(checkBox_at9_enctype);
            groupBox3.Controls.Add(comboBox_at9_useband);
            groupBox3.Controls.Add(label_at9_useband);
            groupBox3.Controls.Add(label_at9_startband);
            groupBox3.Controls.Add(checkBox_at9_advband);
            groupBox3.Controls.Add(checkBox_at9_dualenc);
            groupBox3.Controls.Add(checkBox_at9_supframe);
            groupBox3.Controls.Add(checkBox_at9_advanced);
            groupBox3.Name = "groupBox3";
            groupBox3.TabStop = false;
            toolTip_info.SetToolTip(groupBox3, resources.GetString("groupBox3.ToolTip"));
            // 
            // checkBox_LFE
            // 
            resources.ApplyResources(checkBox_LFE, "checkBox_LFE");
            checkBox_LFE.Name = "checkBox_LFE";
            toolTip_info.SetToolTip(checkBox_LFE, resources.GetString("checkBox_LFE.ToolTip"));
            checkBox_LFE.UseVisualStyleBackColor = true;
            checkBox_LFE.CheckedChanged += CheckBox_LFE_CheckedChanged;
            // 
            // checkBox_wband
            // 
            resources.ApplyResources(checkBox_wband, "checkBox_wband");
            checkBox_wband.Name = "checkBox_wband";
            toolTip_info.SetToolTip(checkBox_wband, resources.GetString("checkBox_wband.ToolTip"));
            checkBox_wband.UseVisualStyleBackColor = true;
            checkBox_wband.CheckedChanged += CheckBox_wband_CheckedChanged;
            // 
            // checkBox_bex
            // 
            resources.ApplyResources(checkBox_bex, "checkBox_bex");
            checkBox_bex.Name = "checkBox_bex";
            toolTip_info.SetToolTip(checkBox_bex, resources.GetString("checkBox_bex.ToolTip"));
            checkBox_bex.UseVisualStyleBackColor = true;
            checkBox_bex.CheckedChanged += CheckBox_bex_CheckedChanged;
            // 
            // comboBox_at9_enctype
            // 
            resources.ApplyResources(comboBox_at9_enctype, "comboBox_at9_enctype");
            comboBox_at9_enctype.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_at9_enctype.FormattingEnabled = true;
            comboBox_at9_enctype.Items.AddRange(new object[] { resources.GetString("comboBox_at9_enctype.Items"), resources.GetString("comboBox_at9_enctype.Items1"), resources.GetString("comboBox_at9_enctype.Items2"), resources.GetString("comboBox_at9_enctype.Items3"), resources.GetString("comboBox_at9_enctype.Items4"), resources.GetString("comboBox_at9_enctype.Items5") });
            comboBox_at9_enctype.Name = "comboBox_at9_enctype";
            toolTip_info.SetToolTip(comboBox_at9_enctype, resources.GetString("comboBox_at9_enctype.ToolTip"));
            comboBox_at9_enctype.SelectedIndexChanged += ComboBox_at9_enctype_SelectedIndexChanged;
            // 
            // label_at9_enctype
            // 
            resources.ApplyResources(label_at9_enctype, "label_at9_enctype");
            label_at9_enctype.Name = "label_at9_enctype";
            toolTip_info.SetToolTip(label_at9_enctype, resources.GetString("label_at9_enctype.ToolTip"));
            // 
            // comboBox_at9_startband
            // 
            resources.ApplyResources(comboBox_at9_startband, "comboBox_at9_startband");
            comboBox_at9_startband.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_at9_startband.FormattingEnabled = true;
            comboBox_at9_startband.Items.AddRange(new object[] { resources.GetString("comboBox_at9_startband.Items"), resources.GetString("comboBox_at9_startband.Items1") });
            comboBox_at9_startband.Name = "comboBox_at9_startband";
            toolTip_info.SetToolTip(comboBox_at9_startband, resources.GetString("comboBox_at9_startband.ToolTip"));
            comboBox_at9_startband.SelectedIndexChanged += ComboBox_at9_startband_SelectedIndexChanged;
            // 
            // checkBox_at9_enctype
            // 
            resources.ApplyResources(checkBox_at9_enctype, "checkBox_at9_enctype");
            checkBox_at9_enctype.Name = "checkBox_at9_enctype";
            toolTip_info.SetToolTip(checkBox_at9_enctype, resources.GetString("checkBox_at9_enctype.ToolTip"));
            checkBox_at9_enctype.UseVisualStyleBackColor = true;
            checkBox_at9_enctype.CheckedChanged += CheckBox_at9_enctype_CheckedChanged;
            // 
            // comboBox_at9_useband
            // 
            resources.ApplyResources(comboBox_at9_useband, "comboBox_at9_useband");
            comboBox_at9_useband.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_at9_useband.FormattingEnabled = true;
            comboBox_at9_useband.Items.AddRange(new object[] { resources.GetString("comboBox_at9_useband.Items"), resources.GetString("comboBox_at9_useband.Items1"), resources.GetString("comboBox_at9_useband.Items2"), resources.GetString("comboBox_at9_useband.Items3"), resources.GetString("comboBox_at9_useband.Items4"), resources.GetString("comboBox_at9_useband.Items5"), resources.GetString("comboBox_at9_useband.Items6"), resources.GetString("comboBox_at9_useband.Items7"), resources.GetString("comboBox_at9_useband.Items8"), resources.GetString("comboBox_at9_useband.Items9"), resources.GetString("comboBox_at9_useband.Items10"), resources.GetString("comboBox_at9_useband.Items11"), resources.GetString("comboBox_at9_useband.Items12"), resources.GetString("comboBox_at9_useband.Items13"), resources.GetString("comboBox_at9_useband.Items14"), resources.GetString("comboBox_at9_useband.Items15") });
            comboBox_at9_useband.Name = "comboBox_at9_useband";
            toolTip_info.SetToolTip(comboBox_at9_useband, resources.GetString("comboBox_at9_useband.ToolTip"));
            comboBox_at9_useband.SelectedIndexChanged += ComboBox_at9_useband_SelectedIndexChanged;
            // 
            // label_at9_useband
            // 
            resources.ApplyResources(label_at9_useband, "label_at9_useband");
            label_at9_useband.Name = "label_at9_useband";
            toolTip_info.SetToolTip(label_at9_useband, resources.GetString("label_at9_useband.ToolTip"));
            // 
            // label_at9_startband
            // 
            resources.ApplyResources(label_at9_startband, "label_at9_startband");
            label_at9_startband.Name = "label_at9_startband";
            toolTip_info.SetToolTip(label_at9_startband, resources.GetString("label_at9_startband.ToolTip"));
            // 
            // checkBox_at9_advband
            // 
            resources.ApplyResources(checkBox_at9_advband, "checkBox_at9_advband");
            checkBox_at9_advband.Name = "checkBox_at9_advband";
            toolTip_info.SetToolTip(checkBox_at9_advband, resources.GetString("checkBox_at9_advband.ToolTip"));
            checkBox_at9_advband.UseVisualStyleBackColor = true;
            checkBox_at9_advband.CheckedChanged += CheckBox_at9_advband_CheckedChanged;
            // 
            // checkBox_at9_dualenc
            // 
            resources.ApplyResources(checkBox_at9_dualenc, "checkBox_at9_dualenc");
            checkBox_at9_dualenc.Name = "checkBox_at9_dualenc";
            toolTip_info.SetToolTip(checkBox_at9_dualenc, resources.GetString("checkBox_at9_dualenc.ToolTip"));
            checkBox_at9_dualenc.UseVisualStyleBackColor = true;
            checkBox_at9_dualenc.CheckedChanged += CheckBox_at9_dualenc_CheckedChanged;
            // 
            // checkBox_at9_supframe
            // 
            resources.ApplyResources(checkBox_at9_supframe, "checkBox_at9_supframe");
            checkBox_at9_supframe.Name = "checkBox_at9_supframe";
            toolTip_info.SetToolTip(checkBox_at9_supframe, resources.GetString("checkBox_at9_supframe.ToolTip"));
            checkBox_at9_supframe.UseVisualStyleBackColor = true;
            checkBox_at9_supframe.CheckedChanged += CheckBox_at9_supframe_CheckedChanged;
            // 
            // checkBox_at9_advanced
            // 
            resources.ApplyResources(checkBox_at9_advanced, "checkBox_at9_advanced");
            checkBox_at9_advanced.Name = "checkBox_at9_advanced";
            toolTip_info.SetToolTip(checkBox_at9_advanced, resources.GetString("checkBox_at9_advanced.ToolTip"));
            checkBox_at9_advanced.UseVisualStyleBackColor = true;
            checkBox_at9_advanced.CheckedChanged += CheckBox_at9_advanced_CheckedChanged;
            // 
            // checkBox_lpcreate
            // 
            resources.ApplyResources(checkBox_lpcreate, "checkBox_lpcreate");
            checkBox_lpcreate.Name = "checkBox_lpcreate";
            toolTip_info.SetToolTip(checkBox_lpcreate, resources.GetString("checkBox_lpcreate.ToolTip"));
            checkBox_lpcreate.UseVisualStyleBackColor = true;
            checkBox_lpcreate.CheckedChanged += CheckBox_lpcreate_CheckedChanged;
            // 
            // FormSettings
            // 
            AcceptButton = button_OK;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ControlBox = false;
            Controls.Add(checkBox_lpcreate);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBox_at9_cmd);
            Controls.Add(textBox_at3_cmd);
            Controls.Add(button_Cancel);
            Controls.Add(button_OK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FormSettings";
            toolTip_info.SetToolTip(this, resources.GetString("$this.ToolTip"));
            Load += FormSettings_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button_OK;
        private Button button_Cancel;
        private TextBox textBox_at3_cmd;
        private TextBox textBox_at9_cmd;
        private Label label1;
        private Label label2;
        private GroupBox groupBox1;
        private ComboBox comboBox_at3_encmethod;
        private Label label3;
        private GroupBox groupBox2;
        private Label label_at9_samples;
        private Label label_at9_loopend;
        private TextBox textBox_at9_loopend;
        private TextBox textBox_at9_loopstart;
        private Label label_at9_loopstart;
        private Button button_at9_looplist;
        private TextBox textBox_at9_looplist;
        private CheckBox checkBox_at9_looplist;
        private CheckBox checkBox_at9_loopsound;
        private CheckBox checkBox_at9_looppoint;
        private ComboBox comboBox_at9_sampling;
        private Label label5;
        private ComboBox comboBox_at9_bitrate;
        private Label label4;
        private CheckBox checkBox_at9_looptimes;
        private Label label_at3_times;
        private TextBox textBox_at3_looptimes;
        private Label label_at3_nol;
        private CheckBox checkBox_at3_looptimes;
        private Label label_at3_samples;
        private TextBox textBox_at3_loopend;
        private TextBox textBox_at3_loopstart;
        private Label label_at3_loopend;
        private Label label_at3_loopstart;
        private CheckBox checkBox_at3_loopsound;
        private CheckBox checkBox_at3_looppoint;
        private Label label_at9_times;
        private Label label_at9_nol;
        private TextBox textBox_at9_looptimes;
        private GroupBox groupBox3;
        private ComboBox comboBox_at9_enctype;
        private Label label_at9_enctype;
        private ComboBox comboBox_at9_startband;
        private CheckBox checkBox_at9_enctype;
        private ComboBox comboBox_at9_useband;
        private Label label_at9_useband;
        private Label label_at9_startband;
        private CheckBox checkBox_at9_advband;
        private CheckBox checkBox_at9_dualenc;
        private CheckBox checkBox_at9_supframe;
        private CheckBox checkBox_at9_advanced;
        private CheckBox checkBox_lpcreate;
        private RadioButton radioButton_PS3;
        private RadioButton radioButton_PSP;
        private Label label6;
        private RadioButton radioButton_PS4;
        private RadioButton radioButton_PSV;
        private Label label7;
        private CheckBox checkBox_LFE;
        private CheckBox checkBox_wband;
        private CheckBox checkBox_bex;
        private ToolTip toolTip_info;
    }
}