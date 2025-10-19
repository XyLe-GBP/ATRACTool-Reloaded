namespace ATRACTool_Reloaded
{
    partial class FormSelectWalkmanFormats
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSelectWalkmanFormats));
            comboBox_DecodeFormats = new ComboBox();
            label_DecodeFmt = new Label();
            label_OutputFmt = new Label();
            comboBox_OutputFormats = new ComboBox();
            button_OK = new Button();
            button_Cancel = new Button();
            SuspendLayout();
            // 
            // comboBox_DecodeFormats
            // 
            resources.ApplyResources(comboBox_DecodeFormats, "comboBox_DecodeFormats");
            comboBox_DecodeFormats.FormattingEnabled = true;
            comboBox_DecodeFormats.Items.AddRange(new object[] { resources.GetString("comboBox_DecodeFormats.Items"), resources.GetString("comboBox_DecodeFormats.Items1"), resources.GetString("comboBox_DecodeFormats.Items2") });
            comboBox_DecodeFormats.Name = "comboBox_DecodeFormats";
            // 
            // label_DecodeFmt
            // 
            resources.ApplyResources(label_DecodeFmt, "label_DecodeFmt");
            label_DecodeFmt.Name = "label_DecodeFmt";
            // 
            // label_OutputFmt
            // 
            resources.ApplyResources(label_OutputFmt, "label_OutputFmt");
            label_OutputFmt.Name = "label_OutputFmt";
            // 
            // comboBox_OutputFormats
            // 
            resources.ApplyResources(comboBox_OutputFormats, "comboBox_OutputFormats");
            comboBox_OutputFormats.FormattingEnabled = true;
            comboBox_OutputFormats.Items.AddRange(new object[] { resources.GetString("comboBox_OutputFormats.Items"), resources.GetString("comboBox_OutputFormats.Items1"), resources.GetString("comboBox_OutputFormats.Items2"), resources.GetString("comboBox_OutputFormats.Items3"), resources.GetString("comboBox_OutputFormats.Items4"), resources.GetString("comboBox_OutputFormats.Items5"), resources.GetString("comboBox_OutputFormats.Items6"), resources.GetString("comboBox_OutputFormats.Items7"), resources.GetString("comboBox_OutputFormats.Items8") });
            comboBox_OutputFormats.Name = "comboBox_OutputFormats";
            // 
            // button_OK
            // 
            resources.ApplyResources(button_OK, "button_OK");
            button_OK.Name = "button_OK";
            button_OK.UseVisualStyleBackColor = true;
            button_OK.Click += button_OK_Click;
            // 
            // button_Cancel
            // 
            resources.ApplyResources(button_Cancel, "button_Cancel");
            button_Cancel.Name = "button_Cancel";
            button_Cancel.UseVisualStyleBackColor = true;
            button_Cancel.Click += button_Cancel_Click;
            // 
            // FormSelectWalkmanFormats
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            ControlBox = false;
            Controls.Add(button_Cancel);
            Controls.Add(button_OK);
            Controls.Add(comboBox_DecodeFormats);
            Controls.Add(label_DecodeFmt);
            Controls.Add(label_OutputFmt);
            Controls.Add(comboBox_OutputFormats);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FormSelectWalkmanFormats";
            Load += FormSelectWalkmanFormats_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox comboBox_DecodeFormats;
        private Label label_DecodeFmt;
        private Label label_OutputFmt;
        private ComboBox comboBox_OutputFormats;
        private Button button_OK;
        private Button button_Cancel;
    }
}