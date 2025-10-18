namespace ATRACTool_Reloaded
{
    partial class FormAbout
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
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            pictureBox1 = new PictureBox();
            button_OK = new Button();
            label5 = new Label();
            linkLabel1 = new LinkLabel();
            label6 = new Label();
            pictureBox2 = new PictureBox();
            linkLabel2 = new LinkLabel();
            linkLabel3 = new LinkLabel();
            label7 = new Label();
            label8 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new Font("Yu Gothic UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(465, 51);
            label1.TabIndex = 0;
            label1.Text = "ATRACTool Reloaded\r\nOpen source ATRAC conversion utility.";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label2.Location = new Point(12, 169);
            label2.Name = "label2";
            label2.Size = new Size(465, 15);
            label2.TabIndex = 1;
            label2.Text = "SCEI ATRAC3plus Codec Tool, SCEI ATRAC9 Codec Tool";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label3.Location = new Point(12, 194);
            label3.Name = "label3";
            label3.Size = new Size(465, 15);
            label3.TabIndex = 2;
            label3.Text = "at3tool.exe: Copyright © 2010 - Sony Computer Entertainment Inc. All Rights Reserved.";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            label4.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label4.Location = new Point(12, 209);
            label4.Name = "label4";
            label4.Size = new Size(465, 15);
            label4.TabIndex = 3;
            label4.Text = "at9tool.exe: Copyright © 2012 - Sony Computer Entertainment Inc. All Rights Reserved.";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.SIE;
            pictureBox1.Location = new Point(115, 66);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(258, 93);
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // button_OK
            // 
            button_OK.Location = new Point(398, 403);
            button_OK.Name = "button_OK";
            button_OK.Size = new Size(75, 23);
            button_OK.TabIndex = 5;
            button_OK.Text = "Done!";
            button_OK.UseVisualStyleBackColor = true;
            button_OK.Click += button_OK_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(44, 272);
            label5.Name = "label5";
            label5.Size = new Size(98, 15);
            label5.TabIndex = 6;
            label5.Text = "This tool uses the";
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(139, 272);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(76, 15);
            linkLabel1.TabIndex = 7;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "MediaToolKit";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(211, 272);
            label6.Name = "label6";
            label6.Size = new Size(220, 15);
            label6.TabIndex = 8;
            label6.Text = ". by AydinAdn, FFmpeg wrapper for .NET";
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(27, 300);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(128, 128);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 9;
            pictureBox2.TabStop = false;
            pictureBox2.Paint += pictureBox2_Paint;
            // 
            // linkLabel2
            // 
            linkLabel2.AutoSize = true;
            linkLabel2.Font = new Font("Yu Gothic UI", 12F);
            linkLabel2.Location = new Point(190, 349);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new Size(107, 21);
            linkLabel2.TabIndex = 10;
            linkLabel2.TabStop = true;
            linkLabel2.Text = "XyLe's GitHub";
            linkLabel2.LinkClicked += linkLabel2_LinkClicked;
            // 
            // linkLabel3
            // 
            linkLabel3.AutoSize = true;
            linkLabel3.Font = new Font("Yu Gothic UI", 12F);
            linkLabel3.Location = new Point(326, 349);
            linkLabel3.Name = "linkLabel3";
            linkLabel3.Size = new Size(65, 21);
            linkLabel3.TabIndex = 11;
            linkLabel3.TabStop = true;
            linkLabel3.Text = "Website";
            linkLabel3.LinkClicked += linkLabel3_LinkClicked;
            // 
            // label7
            // 
            label7.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label7.Location = new Point(12, 225);
            label7.Name = "label7";
            label7.Size = new Size(465, 15);
            label7.TabIndex = 12;
            label7.Text = "ATRACTool.exe and ATRACTool-Reloaded.exe: Copyright © 2025 - XyLe. All Rights Reserved.";
            label7.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            label8.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label8.Location = new Point(12, 240);
            label8.Name = "label8";
            label8.Size = new Size(465, 15);
            label8.TabIndex = 13;
            label8.Text = "ApplicationUpdater (updater.exe): Copyright © 2025 - XyLe. All Rights Reserved.";
            label8.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FormAbout
            // 
            AcceptButton = button_OK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(489, 441);
            ControlBox = false;
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(linkLabel3);
            Controls.Add(linkLabel2);
            Controls.Add(pictureBox2);
            Controls.Add(label6);
            Controls.Add(linkLabel1);
            Controls.Add(label5);
            Controls.Add(button_OK);
            Controls.Add(pictureBox1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FormAbout";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About ATRACTool Reloaded";
            Load += FormAbout_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private PictureBox pictureBox1;
        private Button button_OK;
        private Label label5;
        private LinkLabel linkLabel1;
        private Label label6;
        private PictureBox pictureBox2;
        private LinkLabel linkLabel2;
        private LinkLabel linkLabel3;
        private Label label7;
        private Label label8;
    }
}