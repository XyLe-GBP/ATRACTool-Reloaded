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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgress));
            label1 = new Label();
            progressBar_MainProgress = new ProgressBar();
            button_Abort = new Button();
            label_Status = new Label();
            timer_interval = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // progressBar_MainProgress
            // 
            resources.ApplyResources(progressBar_MainProgress, "progressBar_MainProgress");
            progressBar_MainProgress.Name = "progressBar_MainProgress";
            // 
            // button_Abort
            // 
            resources.ApplyResources(button_Abort, "button_Abort");
            button_Abort.Name = "button_Abort";
            button_Abort.UseVisualStyleBackColor = true;
            button_Abort.Click += Button_Abort_Click;
            // 
            // label_Status
            // 
            resources.ApplyResources(label_Status, "label_Status");
            label_Status.Name = "label_Status";
            // 
            // timer_interval
            // 
            timer_interval.Tick += Timer_interval_Tick;
            // 
            // FormProgress
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            ControlBox = false;
            Controls.Add(label_Status);
            Controls.Add(button_Abort);
            Controls.Add(progressBar_MainProgress);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FormProgress";
            Load += FormProgress_Load;
            ResumeLayout(false);

        }

        #endregion

        private Label label1;
        private ProgressBar progressBar_MainProgress;
        private Button button_Abort;
        private Label label_Status;
        private System.Windows.Forms.Timer timer_interval;
    }
}