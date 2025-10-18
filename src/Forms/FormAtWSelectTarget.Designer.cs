namespace ATRACTool_Reloaded
{
    partial class FormAtWSelectTarget
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAtWSelectTarget));
            label1 = new Label();
            comboBox_method = new ComboBox();
            button_OK = new Button();
            button_Cancel = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // comboBox_method
            // 
            resources.ApplyResources(comboBox_method, "comboBox_method");
            comboBox_method.FormattingEnabled = true;
            comboBox_method.Items.AddRange(new object[] { resources.GetString("comboBox_method.Items"), resources.GetString("comboBox_method.Items1"), resources.GetString("comboBox_method.Items2"), resources.GetString("comboBox_method.Items3"), resources.GetString("comboBox_method.Items4"), resources.GetString("comboBox_method.Items5"), resources.GetString("comboBox_method.Items6") });
            comboBox_method.Name = "comboBox_method";
            comboBox_method.SelectedIndexChanged += ComboBox_method_SelectedIndexChanged;
            // 
            // button_OK
            // 
            resources.ApplyResources(button_OK, "button_OK");
            button_OK.Name = "button_OK";
            button_OK.UseVisualStyleBackColor = true;
            button_OK.Click += Button_OK_Click;
            // 
            // button_Cancel
            // 
            resources.ApplyResources(button_Cancel, "button_Cancel");
            button_Cancel.Name = "button_Cancel";
            button_Cancel.UseVisualStyleBackColor = true;
            button_Cancel.Click += Button_Cancel_Click;
            // 
            // FormAtWSelectTarget
            // 
            AcceptButton = button_OK;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ControlBox = false;
            Controls.Add(button_Cancel);
            Controls.Add(button_OK);
            Controls.Add(comboBox_method);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "FormAtWSelectTarget";
            Load += FormAtWSelectTarget_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label label1;
        private ComboBox comboBox_method;
        private Button button_OK;
        private Button button_Cancel;
    }
}