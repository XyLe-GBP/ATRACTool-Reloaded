namespace ATRACTool_Reloaded
{
    partial class FormProgress
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgress));
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar_MainProgress = new System.Windows.Forms.ProgressBar();
            this.button_Abort = new System.Windows.Forms.Button();
            this.label_Status = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // progressBar_MainProgress
            // 
            resources.ApplyResources(this.progressBar_MainProgress, "progressBar_MainProgress");
            this.progressBar_MainProgress.Name = "progressBar_MainProgress";
            // 
            // button_Abort
            // 
            resources.ApplyResources(this.button_Abort, "button_Abort");
            this.button_Abort.Name = "button_Abort";
            this.button_Abort.UseVisualStyleBackColor = true;
            this.button_Abort.Click += new System.EventHandler(this.Button_Abort_Click);
            // 
            // label_Status
            // 
            resources.ApplyResources(this.label_Status, "label_Status");
            this.label_Status.Name = "label_Status";
            // 
            // FormProgress
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.label_Status);
            this.Controls.Add(this.button_Abort);
            this.Controls.Add(this.progressBar_MainProgress);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormProgress";
            this.Load += new System.EventHandler(this.FormProgress_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private ProgressBar progressBar_MainProgress;
        private Button button_Abort;
        private Label label_Status;
    }
}