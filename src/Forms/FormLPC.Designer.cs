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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLPC));
            trackBar_trk = new TrackBar();
            button_Play = new Button();
            button_Stop = new Button();
            button_OK = new Button();
            button_Cancel = new Button();
            label_Samples = new Label();
            timer_Reload = new System.Windows.Forms.Timer(components);
            label_Length = new Label();
            trackBar_Start = new TrackBar();
            trackBar_End = new TrackBar();
            label_LoopStartSamples = new Label();
            label_LoopEndSamples = new Label();
            button_SetStart = new Button();
            button_SetEnd = new Button();
            numericUpDown_LoopStart = new NumericUpDown();
            numericUpDown_LoopEnd = new NumericUpDown();
            checkBox_LoopEnable = new CheckBox();
            radioButton_at3 = new RadioButton();
            radioButton_at9 = new RadioButton();
            label_File = new Label();
            button_Prev = new Button();
            button_Next = new Button();
            label_Psamples = new Label();
            label_Plength = new Label();
            volumeSlider1 = new NAudio.Gui.VolumeSlider();
            label_Volume = new Label();
            button_LS_Current = new Button();
            button_LE_Current = new Button();
            ((System.ComponentModel.ISupportInitialize)trackBar_trk).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar_Start).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar_End).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_LoopStart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_LoopEnd).BeginInit();
            SuspendLayout();
            // 
            // trackBar_trk
            // 
            resources.ApplyResources(trackBar_trk, "trackBar_trk");
            trackBar_trk.Name = "trackBar_trk";
            trackBar_trk.TickStyle = TickStyle.Both;
            // 
            // button_Play
            // 
            resources.ApplyResources(button_Play, "button_Play");
            button_Play.Name = "button_Play";
            button_Play.UseVisualStyleBackColor = true;
            button_Play.Click += Button_Play_Click;
            // 
            // button_Stop
            // 
            resources.ApplyResources(button_Stop, "button_Stop");
            button_Stop.Name = "button_Stop";
            button_Stop.UseVisualStyleBackColor = true;
            button_Stop.Click += Button_Stop_Click;
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
            // label_Samples
            // 
            resources.ApplyResources(label_Samples, "label_Samples");
            label_Samples.Name = "label_Samples";
            // 
            // timer_Reload
            // 
            timer_Reload.Tick += Timer_Reload_Tick;
            // 
            // label_Length
            // 
            resources.ApplyResources(label_Length, "label_Length");
            label_Length.Name = "label_Length";
            // 
            // trackBar_Start
            // 
            resources.ApplyResources(trackBar_Start, "trackBar_Start");
            trackBar_Start.Name = "trackBar_Start";
            trackBar_Start.TickStyle = TickStyle.TopLeft;
            // 
            // trackBar_End
            // 
            resources.ApplyResources(trackBar_End, "trackBar_End");
            trackBar_End.Name = "trackBar_End";
            // 
            // label_LoopStartSamples
            // 
            resources.ApplyResources(label_LoopStartSamples, "label_LoopStartSamples");
            label_LoopStartSamples.Name = "label_LoopStartSamples";
            // 
            // label_LoopEndSamples
            // 
            resources.ApplyResources(label_LoopEndSamples, "label_LoopEndSamples");
            label_LoopEndSamples.Name = "label_LoopEndSamples";
            // 
            // button_SetStart
            // 
            resources.ApplyResources(button_SetStart, "button_SetStart");
            button_SetStart.Name = "button_SetStart";
            button_SetStart.UseVisualStyleBackColor = true;
            button_SetStart.Click += Button_SetStart_Click;
            // 
            // button_SetEnd
            // 
            resources.ApplyResources(button_SetEnd, "button_SetEnd");
            button_SetEnd.Name = "button_SetEnd";
            button_SetEnd.UseVisualStyleBackColor = true;
            button_SetEnd.Click += Button_SetEnd_Click;
            // 
            // numericUpDown_LoopStart
            // 
            resources.ApplyResources(numericUpDown_LoopStart, "numericUpDown_LoopStart");
            numericUpDown_LoopStart.Name = "numericUpDown_LoopStart";
            numericUpDown_LoopStart.ValueChanged += NumericUpDown_LoopStart_ValueChanged;
            // 
            // numericUpDown_LoopEnd
            // 
            resources.ApplyResources(numericUpDown_LoopEnd, "numericUpDown_LoopEnd");
            numericUpDown_LoopEnd.Name = "numericUpDown_LoopEnd";
            numericUpDown_LoopEnd.ValueChanged += NumericUpDown_LoopEnd_ValueChanged;
            // 
            // checkBox_LoopEnable
            // 
            resources.ApplyResources(checkBox_LoopEnable, "checkBox_LoopEnable");
            checkBox_LoopEnable.Name = "checkBox_LoopEnable";
            checkBox_LoopEnable.UseVisualStyleBackColor = true;
            checkBox_LoopEnable.CheckedChanged += CheckBox_LoopEnable_CheckedChanged;
            // 
            // radioButton_at3
            // 
            resources.ApplyResources(radioButton_at3, "radioButton_at3");
            radioButton_at3.Checked = true;
            radioButton_at3.Name = "radioButton_at3";
            radioButton_at3.TabStop = true;
            radioButton_at3.UseVisualStyleBackColor = true;
            radioButton_at3.CheckedChanged += RadioButton_at3_CheckedChanged;
            // 
            // radioButton_at9
            // 
            resources.ApplyResources(radioButton_at9, "radioButton_at9");
            radioButton_at9.Name = "radioButton_at9";
            radioButton_at9.TabStop = true;
            radioButton_at9.UseVisualStyleBackColor = true;
            radioButton_at9.CheckedChanged += RadioButton_at9_CheckedChanged;
            // 
            // label_File
            // 
            label_File.AutoEllipsis = true;
            resources.ApplyResources(label_File, "label_File");
            label_File.Name = "label_File";
            // 
            // button_Prev
            // 
            resources.ApplyResources(button_Prev, "button_Prev");
            button_Prev.Name = "button_Prev";
            button_Prev.UseVisualStyleBackColor = true;
            button_Prev.Click += Button_Prev_Click;
            // 
            // button_Next
            // 
            resources.ApplyResources(button_Next, "button_Next");
            button_Next.Name = "button_Next";
            button_Next.UseVisualStyleBackColor = true;
            button_Next.Click += Button_Next_Click;
            // 
            // label_Psamples
            // 
            resources.ApplyResources(label_Psamples, "label_Psamples");
            label_Psamples.Name = "label_Psamples";
            // 
            // label_Plength
            // 
            resources.ApplyResources(label_Plength, "label_Plength");
            label_Plength.Name = "label_Plength";
            // 
            // volumeSlider1
            // 
            resources.ApplyResources(volumeSlider1, "volumeSlider1");
            volumeSlider1.Name = "volumeSlider1";
            volumeSlider1.VolumeChanged += VolumeSlider1_VolumeChanged;
            // 
            // label_Volume
            // 
            resources.ApplyResources(label_Volume, "label_Volume");
            label_Volume.Name = "label_Volume";
            // 
            // button_LS_Current
            // 
            resources.ApplyResources(button_LS_Current, "button_LS_Current");
            button_LS_Current.Name = "button_LS_Current";
            button_LS_Current.UseVisualStyleBackColor = true;
            button_LS_Current.Click += Button_LS_Current_Click;
            // 
            // button_LE_Current
            // 
            resources.ApplyResources(button_LE_Current, "button_LE_Current");
            button_LE_Current.Name = "button_LE_Current";
            button_LE_Current.UseVisualStyleBackColor = true;
            button_LE_Current.Click += Button_LE_Current_Click;
            // 
            // FormLPC
            // 
            AcceptButton = button_OK;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ControlBox = false;
            Controls.Add(button_LE_Current);
            Controls.Add(button_LS_Current);
            Controls.Add(label_Volume);
            Controls.Add(volumeSlider1);
            Controls.Add(label_Plength);
            Controls.Add(label_Psamples);
            Controls.Add(button_Next);
            Controls.Add(button_Prev);
            Controls.Add(label_File);
            Controls.Add(radioButton_at9);
            Controls.Add(radioButton_at3);
            Controls.Add(checkBox_LoopEnable);
            Controls.Add(numericUpDown_LoopEnd);
            Controls.Add(numericUpDown_LoopStart);
            Controls.Add(trackBar_End);
            Controls.Add(button_SetEnd);
            Controls.Add(button_SetStart);
            Controls.Add(label_LoopEndSamples);
            Controls.Add(label_LoopStartSamples);
            Controls.Add(trackBar_trk);
            Controls.Add(label_Length);
            Controls.Add(label_Samples);
            Controls.Add(button_Cancel);
            Controls.Add(button_OK);
            Controls.Add(button_Stop);
            Controls.Add(button_Play);
            Controls.Add(trackBar_Start);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "FormLPC";
            FormClosed += FormLPC_FormClosed;
            Load += FormLPC_Load;
            Paint += FormLPC_Paint;
            ((System.ComponentModel.ISupportInitialize)trackBar_trk).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar_Start).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar_End).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_LoopStart).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_LoopEnd).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
        private NAudio.Gui.VolumeSlider volumeSlider1;
        private Label label_Volume;
        private Button button_LS_Current;
        private Button button_LE_Current;
    }
}