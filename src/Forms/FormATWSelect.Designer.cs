namespace ATRACTool_Reloaded
{
    partial class FormATWSelect
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormATWSelect));
            button_OK = new Button();
            button_Cancel = new Button();
            label1 = new Label();
            comboBox_fmt = new ComboBox();
            SuspendLayout();
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
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // comboBox_fmt
            // 
            resources.ApplyResources(comboBox_fmt, "comboBox_fmt");
            comboBox_fmt.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_fmt.FormattingEnabled = true;
            comboBox_fmt.Items.AddRange(new object[] { resources.GetString("comboBox_fmt.Items"), resources.GetString("comboBox_fmt.Items1"), resources.GetString("comboBox_fmt.Items2"), resources.GetString("comboBox_fmt.Items3"), resources.GetString("comboBox_fmt.Items4"), resources.GetString("comboBox_fmt.Items5"), resources.GetString("comboBox_fmt.Items6") });
            comboBox_fmt.Name = "comboBox_fmt";
            // 
            // FormATWSelect
            // 
            AcceptButton = button_OK;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ControlBox = false;
            Controls.Add(comboBox_fmt);
            Controls.Add(label1);
            Controls.Add(button_Cancel);
            Controls.Add(button_OK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FormATWSelect";
            Load += FormATWSelect_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button button_OK;
        private Button button_Cancel;
        private Label label1;
        private ComboBox comboBox_fmt;
    }
}