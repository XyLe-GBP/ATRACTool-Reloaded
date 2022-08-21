namespace ATRACTool_Reloaded
{
    partial class FormLPC
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLPC));
            this.trackBar_trk = new System.Windows.Forms.TrackBar();
            this.button_Play = new System.Windows.Forms.Button();
            this.button_Stop = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label_Samples = new System.Windows.Forms.Label();
            this.timer_Reload = new System.Windows.Forms.Timer(this.components);
            this.label_Length = new System.Windows.Forms.Label();
            this.trackBar_Start = new System.Windows.Forms.TrackBar();
            this.trackBar_End = new System.Windows.Forms.TrackBar();
            this.label_LoopStartSamples = new System.Windows.Forms.Label();
            this.label_LoopEndSamples = new System.Windows.Forms.Label();
            this.button_SetStart = new System.Windows.Forms.Button();
            this.button_SetEnd = new System.Windows.Forms.Button();
            this.numericUpDown_LoopStart = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_LoopEnd = new System.Windows.Forms.NumericUpDown();
            this.checkBox_LoopEnable = new System.Windows.Forms.CheckBox();
            this.radioButton_at3 = new System.Windows.Forms.RadioButton();
            this.radioButton_at9 = new System.Windows.Forms.RadioButton();
            this.label_File = new System.Windows.Forms.Label();
            this.button_Prev = new System.Windows.Forms.Button();
            this.button_Next = new System.Windows.Forms.Button();
            this.label_Psamples = new System.Windows.Forms.Label();
            this.label_Plength = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_trk)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Start)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_End)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_LoopStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_LoopEnd)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBar_trk
            // 
            resources.ApplyResources(this.trackBar_trk, "trackBar_trk");
            this.trackBar_trk.Name = "trackBar_trk";
            this.trackBar_trk.TickStyle = System.Windows.Forms.TickStyle.Both;
            // 
            // button_Play
            // 
            resources.ApplyResources(this.button_Play, "button_Play");
            this.button_Play.Name = "button_Play";
            this.button_Play.UseVisualStyleBackColor = true;
            this.button_Play.Click += new System.EventHandler(this.Button_Play_Click);
            // 
            // button_Stop
            // 
            resources.ApplyResources(this.button_Stop, "button_Stop");
            this.button_Stop.Name = "button_Stop";
            this.button_Stop.UseVisualStyleBackColor = true;
            this.button_Stop.Click += new System.EventHandler(this.Button_Stop_Click);
            // 
            // button_OK
            // 
            resources.ApplyResources(this.button_OK, "button_OK");
            this.button_OK.Name = "button_OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.Button_OK_Click);
            // 
            // button_Cancel
            // 
            resources.ApplyResources(this.button_Cancel, "button_Cancel");
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.Button_Cancel_Click);
            // 
            // label_Samples
            // 
            resources.ApplyResources(this.label_Samples, "label_Samples");
            this.label_Samples.Name = "label_Samples";
            // 
            // timer_Reload
            // 
            this.timer_Reload.Tick += new System.EventHandler(this.Timer_Reload_Tick);
            // 
            // label_Length
            // 
            resources.ApplyResources(this.label_Length, "label_Length");
            this.label_Length.Name = "label_Length";
            // 
            // trackBar_Start
            // 
            resources.ApplyResources(this.trackBar_Start, "trackBar_Start");
            this.trackBar_Start.Name = "trackBar_Start";
            this.trackBar_Start.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            // 
            // trackBar_End
            // 
            resources.ApplyResources(this.trackBar_End, "trackBar_End");
            this.trackBar_End.Name = "trackBar_End";
            // 
            // label_LoopStartSamples
            // 
            resources.ApplyResources(this.label_LoopStartSamples, "label_LoopStartSamples");
            this.label_LoopStartSamples.Name = "label_LoopStartSamples";
            // 
            // label_LoopEndSamples
            // 
            resources.ApplyResources(this.label_LoopEndSamples, "label_LoopEndSamples");
            this.label_LoopEndSamples.Name = "label_LoopEndSamples";
            // 
            // button_SetStart
            // 
            resources.ApplyResources(this.button_SetStart, "button_SetStart");
            this.button_SetStart.Name = "button_SetStart";
            this.button_SetStart.UseVisualStyleBackColor = true;
            this.button_SetStart.Click += new System.EventHandler(this.Button_SetStart_Click);
            // 
            // button_SetEnd
            // 
            resources.ApplyResources(this.button_SetEnd, "button_SetEnd");
            this.button_SetEnd.Name = "button_SetEnd";
            this.button_SetEnd.UseVisualStyleBackColor = true;
            this.button_SetEnd.Click += new System.EventHandler(this.Button_SetEnd_Click);
            // 
            // numericUpDown_LoopStart
            // 
            resources.ApplyResources(this.numericUpDown_LoopStart, "numericUpDown_LoopStart");
            this.numericUpDown_LoopStart.Name = "numericUpDown_LoopStart";
            this.numericUpDown_LoopStart.ValueChanged += new System.EventHandler(this.NumericUpDown_LoopStart_ValueChanged);
            // 
            // numericUpDown_LoopEnd
            // 
            resources.ApplyResources(this.numericUpDown_LoopEnd, "numericUpDown_LoopEnd");
            this.numericUpDown_LoopEnd.Name = "numericUpDown_LoopEnd";
            this.numericUpDown_LoopEnd.ValueChanged += new System.EventHandler(this.NumericUpDown_LoopEnd_ValueChanged);
            // 
            // checkBox_LoopEnable
            // 
            resources.ApplyResources(this.checkBox_LoopEnable, "checkBox_LoopEnable");
            this.checkBox_LoopEnable.Name = "checkBox_LoopEnable";
            this.checkBox_LoopEnable.UseVisualStyleBackColor = true;
            // 
            // radioButton_at3
            // 
            resources.ApplyResources(this.radioButton_at3, "radioButton_at3");
            this.radioButton_at3.Checked = true;
            this.radioButton_at3.Name = "radioButton_at3";
            this.radioButton_at3.TabStop = true;
            this.radioButton_at3.UseVisualStyleBackColor = true;
            // 
            // radioButton_at9
            // 
            resources.ApplyResources(this.radioButton_at9, "radioButton_at9");
            this.radioButton_at9.Name = "radioButton_at9";
            this.radioButton_at9.TabStop = true;
            this.radioButton_at9.UseVisualStyleBackColor = true;
            // 
            // label_File
            // 
            resources.ApplyResources(this.label_File, "label_File");
            this.label_File.AutoEllipsis = true;
            this.label_File.Name = "label_File";
            // 
            // button_Prev
            // 
            resources.ApplyResources(this.button_Prev, "button_Prev");
            this.button_Prev.Name = "button_Prev";
            this.button_Prev.UseVisualStyleBackColor = true;
            this.button_Prev.Click += new System.EventHandler(this.Button_Prev_Click);
            // 
            // button_Next
            // 
            resources.ApplyResources(this.button_Next, "button_Next");
            this.button_Next.Name = "button_Next";
            this.button_Next.UseVisualStyleBackColor = true;
            this.button_Next.Click += new System.EventHandler(this.Button_Next_Click);
            // 
            // label_Psamples
            // 
            resources.ApplyResources(this.label_Psamples, "label_Psamples");
            this.label_Psamples.Name = "label_Psamples";
            // 
            // label_Plength
            // 
            resources.ApplyResources(this.label_Plength, "label_Plength");
            this.label_Plength.Name = "label_Plength";
            // 
            // FormLPC
            // 
            this.AcceptButton = this.button_OK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ControlBox = false;
            this.Controls.Add(this.label_Plength);
            this.Controls.Add(this.label_Psamples);
            this.Controls.Add(this.button_Next);
            this.Controls.Add(this.button_Prev);
            this.Controls.Add(this.label_File);
            this.Controls.Add(this.radioButton_at9);
            this.Controls.Add(this.radioButton_at3);
            this.Controls.Add(this.checkBox_LoopEnable);
            this.Controls.Add(this.numericUpDown_LoopEnd);
            this.Controls.Add(this.numericUpDown_LoopStart);
            this.Controls.Add(this.trackBar_End);
            this.Controls.Add(this.button_SetEnd);
            this.Controls.Add(this.button_SetStart);
            this.Controls.Add(this.label_LoopEndSamples);
            this.Controls.Add(this.label_LoopStartSamples);
            this.Controls.Add(this.trackBar_trk);
            this.Controls.Add(this.label_Length);
            this.Controls.Add(this.label_Samples);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Stop);
            this.Controls.Add(this.button_Play);
            this.Controls.Add(this.trackBar_Start);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormLPC";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLPC_FormClosed);
            this.Load += new System.EventHandler(this.FormLPC_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormLPC_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_trk)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Start)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_End)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_LoopStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_LoopEnd)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TrackBar trackBar_trk;
        private Button button_Play;
        private Button button_Stop;
        private Button button_OK;
        private Button button_Cancel;
        private Label label_Samples;
        private System.Windows.Forms.Timer timer_Reload;
        private Label label_Length;
        private TrackBar trackBar_Start;
        private TrackBar trackBar_End;
        private Label label_LoopStartSamples;
        private Label label_LoopEndSamples;
        private Button button_SetStart;
        private Button button_SetEnd;
        private NumericUpDown numericUpDown_LoopStart;
        private NumericUpDown numericUpDown_LoopEnd;
        private CheckBox checkBox_LoopEnable;
        private RadioButton radioButton_at3;
        private RadioButton radioButton_at9;
        private Label label_File;
        private Button button_Prev;
        private Button button_Next;
        private Label label_Psamples;
        private Label label_Plength;
    }
}