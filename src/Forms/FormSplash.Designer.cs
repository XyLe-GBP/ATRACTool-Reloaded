namespace ATRACTool_Reloaded
{
    partial class FormSplash
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
            label_log = new Label();
            progressBar1 = new ProgressBar();
            SuspendLayout();
            // 
            // label_log
            // 
            label_log.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label_log.AutoEllipsis = true;
            label_log.Font = new Font("Yu Gothic UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            label_log.Location = new Point(12, 130);
            label_log.Name = "label_log";
            label_log.Size = new Size(273, 21);
            label_log.TabIndex = 0;
            label_log.Text = "Loading...";
            label_log.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(12, 153);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(276, 5);
            progressBar1.TabIndex = 1;
            // 
            // FormSplash
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.SIE;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(300, 164);
            ControlBox = false;
            Controls.Add(progressBar1);
            Controls.Add(label_log);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormSplash";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Loading...";
            Load += FormSplash_Load;
            ResumeLayout(false);
        }

        #endregion

        public Label label_log;
        private ProgressBar progressBar1;
    }
}