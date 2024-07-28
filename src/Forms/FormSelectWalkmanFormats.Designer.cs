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
            comboBox_DecodeFormats.FormattingEnabled = true;
            comboBox_DecodeFormats.Items.AddRange(new object[] { "[WAVE] PCM (.wav)", "[AAC-LC] Advanced Audio Coding (.3gp)", "[HE-AAC] Advanced Audio Coding (.3gp)" });
            comboBox_DecodeFormats.Location = new Point(12, 27);
            comboBox_DecodeFormats.Name = "comboBox_DecodeFormats";
            comboBox_DecodeFormats.Size = new Size(452, 23);
            comboBox_DecodeFormats.TabIndex = 7;
            // 
            // label_DecodeFmt
            // 
            label_DecodeFmt.AutoSize = true;
            label_DecodeFmt.ImeMode = ImeMode.NoControl;
            label_DecodeFmt.Location = new Point(12, 9);
            label_DecodeFmt.Name = "label_DecodeFmt";
            label_DecodeFmt.Size = new Size(88, 15);
            label_DecodeFmt.TabIndex = 6;
            label_DecodeFmt.Text = "Decode format:";
            // 
            // label_OutputFmt
            // 
            label_OutputFmt.AutoSize = true;
            label_OutputFmt.ImeMode = ImeMode.NoControl;
            label_OutputFmt.Location = new Point(12, 9);
            label_OutputFmt.Name = "label_OutputFmt";
            label_OutputFmt.Size = new Size(192, 15);
            label_OutputFmt.TabIndex = 5;
            label_OutputFmt.Text = "ATRAC3 / ATRAC3+ Output format:";
            // 
            // comboBox_OutputFormats
            // 
            comboBox_OutputFormats.FormattingEnabled = true;
            comboBox_OutputFormats.Items.AddRange(new object[] { "[PCM] OpenMG Audio (.oma)", "[ATRAC3] OpenMG Audio (.oma)", "[ATRAC3] OpenMG Audio (.omg)", "[ATRAC3] ATRAC Advanced Lossless [132kHz](.oma)", "[ATRAC3] Video Clip (.kdr)", "[ATRAC3+] OpenMG Audio (.oma)", "[ATRAC3+] OpenMG Audio (.omg)", "[ATRAC3+] ATRAC Advanced Lossless [352kHz](.oma)", "[ATRAC3+] Video Clip (.kdr)" });
            comboBox_OutputFormats.Location = new Point(12, 27);
            comboBox_OutputFormats.Name = "comboBox_OutputFormats";
            comboBox_OutputFormats.Size = new Size(452, 23);
            comboBox_OutputFormats.TabIndex = 4;
            // 
            // button_OK
            // 
            button_OK.Location = new Point(389, 56);
            button_OK.Name = "button_OK";
            button_OK.Size = new Size(75, 23);
            button_OK.TabIndex = 8;
            button_OK.Text = "OK";
            button_OK.UseVisualStyleBackColor = true;
            button_OK.Click += button_OK_Click;
            // 
            // button_Cancel
            // 
            button_Cancel.Location = new Point(309, 56);
            button_Cancel.Name = "button_Cancel";
            button_Cancel.Size = new Size(75, 23);
            button_Cancel.TabIndex = 9;
            button_Cancel.Text = "Cancel";
            button_Cancel.UseVisualStyleBackColor = true;
            button_Cancel.Click += button_Cancel_Click;
            // 
            // FormSelectWalkmanFormats
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(477, 91);
            ControlBox = false;
            Controls.Add(button_Cancel);
            Controls.Add(button_OK);
            Controls.Add(comboBox_DecodeFormats);
            Controls.Add(label_DecodeFmt);
            Controls.Add(label_OutputFmt);
            Controls.Add(comboBox_OutputFormats);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FormSelectWalkmanFormats";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FormSelectWalkmanFormats";
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