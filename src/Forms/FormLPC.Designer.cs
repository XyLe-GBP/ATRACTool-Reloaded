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
            button_Play = new Button();
            button_Stop = new Button();
            button_OK = new Button();
            button_Cancel = new Button();
            label_Samples = new Label();
            timer_Reload = new System.Windows.Forms.Timer(components);
            label_Length = new Label();
            label_LoopStartSamples = new Label();
            label_LoopEndSamples = new Label();
            button_SetStart = new Button();
            button_SetEnd = new Button();
            button_RestoreOriginalLoop = new Button();
            numericUpDown_LoopStart = new NumericUpDown();
            numericUpDown_LoopEnd = new NumericUpDown();
            checkBox_LoopEnable = new CheckBox();
            label_File = new Label();
            button_Prev = new Button();
            button_Next = new Button();
            label_Psamples = new Label();
            label_Plength = new Label();
            volumeSlider1 = new NAudio.Gui.VolumeSlider();
            label_Volume = new Label();
            button_LS_Current = new Button();
            button_LE_Current = new Button();
            label_start = new Label();
            label_trk = new Label();
            label_end = new Label();
            panSlider1 = new NAudio.Gui.PanSlider();
            label_Pan = new Label();
            button_PanCenter = new Button();
            customTrackBar_End = new ATRACTool_Reloaded.src.Controls.CustomTrackBar();
            customTrackBar_Trk = new ATRACTool_Reloaded.src.Controls.CustomTrackBar();
            customTrackBar_Start = new ATRACTool_Reloaded.src.Controls.CustomTrackBar();
            label_previewwarn = new Label();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_LoopStart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_LoopEnd).BeginInit();
            SuspendLayout();
            // 
            // button_Play
            // 
            button_Play.BackColor = Color.White;
            button_Play.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_Play.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_Play.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            resources.ApplyResources(button_Play, "button_Play");
            button_Play.ForeColor = Color.FromArgb(31, 35, 40);
            button_Play.Name = "button_Play";
            button_Play.UseVisualStyleBackColor = false;
            button_Play.Click += Button_Play_Click;
            // 
            // button_Stop
            // 
            button_Stop.BackColor = Color.White;
            button_Stop.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_Stop.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_Stop.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            resources.ApplyResources(button_Stop, "button_Stop");
            button_Stop.ForeColor = Color.FromArgb(31, 35, 40);
            button_Stop.Name = "button_Stop";
            button_Stop.UseVisualStyleBackColor = false;
            button_Stop.Click += Button_Stop_Click;
            // 
            // button_OK
            // 
            button_OK.BackColor = Color.White;
            button_OK.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_OK.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_OK.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            resources.ApplyResources(button_OK, "button_OK");
            button_OK.ForeColor = Color.FromArgb(31, 35, 40);
            button_OK.Name = "button_OK";
            button_OK.UseVisualStyleBackColor = false;
            button_OK.Click += Button_OK_Click;
            // 
            // button_Cancel
            // 
            button_Cancel.BackColor = Color.White;
            button_Cancel.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_Cancel.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_Cancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            resources.ApplyResources(button_Cancel, "button_Cancel");
            button_Cancel.ForeColor = Color.FromArgb(31, 35, 40);
            button_Cancel.Name = "button_Cancel";
            button_Cancel.UseVisualStyleBackColor = false;
            button_Cancel.Click += Button_Cancel_Click;
            // 
            // label_Samples
            // 
            resources.ApplyResources(label_Samples, "label_Samples");
            label_Samples.BackColor = Color.FromArgb(246, 247, 249);
            label_Samples.ForeColor = Color.FromArgb(31, 35, 40);
            label_Samples.Name = "label_Samples";
            // 
            // timer_Reload
            // 
            timer_Reload.Tick += Timer_Reload_Tick;
            // 
            // label_Length
            // 
            resources.ApplyResources(label_Length, "label_Length");
            label_Length.BackColor = Color.FromArgb(246, 247, 249);
            label_Length.ForeColor = Color.FromArgb(31, 35, 40);
            label_Length.Name = "label_Length";
            // 
            // label_LoopStartSamples
            // 
            resources.ApplyResources(label_LoopStartSamples, "label_LoopStartSamples");
            label_LoopStartSamples.BackColor = Color.FromArgb(246, 247, 249);
            label_LoopStartSamples.ForeColor = Color.FromArgb(31, 35, 40);
            label_LoopStartSamples.Name = "label_LoopStartSamples";
            // 
            // label_LoopEndSamples
            // 
            resources.ApplyResources(label_LoopEndSamples, "label_LoopEndSamples");
            label_LoopEndSamples.BackColor = Color.FromArgb(246, 247, 249);
            label_LoopEndSamples.ForeColor = Color.FromArgb(31, 35, 40);
            label_LoopEndSamples.Name = "label_LoopEndSamples";
            // 
            // button_SetStart
            // 
            button_SetStart.BackColor = Color.White;
            resources.ApplyResources(button_SetStart, "button_SetStart");
            button_SetStart.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_SetStart.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_SetStart.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            button_SetStart.ForeColor = Color.FromArgb(31, 35, 40);
            button_SetStart.Name = "button_SetStart";
            button_SetStart.UseVisualStyleBackColor = false;
            button_SetStart.Click += Button_SetStart_Click;
            // 
            // button_SetEnd
            // 
            button_SetEnd.BackColor = Color.White;
            resources.ApplyResources(button_SetEnd, "button_SetEnd");
            button_SetEnd.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_SetEnd.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_SetEnd.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            button_SetEnd.ForeColor = Color.FromArgb(31, 35, 40);
            button_SetEnd.Name = "button_SetEnd";
            button_SetEnd.UseVisualStyleBackColor = false;
            button_SetEnd.Click += Button_SetEnd_Click;
            // 
            // button_RestoreOriginalLoop
            // 
            button_RestoreOriginalLoop.BackColor = Color.White;
            resources.ApplyResources(button_RestoreOriginalLoop, "button_RestoreOriginalLoop");
            button_RestoreOriginalLoop.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_RestoreOriginalLoop.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_RestoreOriginalLoop.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            button_RestoreOriginalLoop.ForeColor = Color.FromArgb(31, 35, 40);
            button_RestoreOriginalLoop.Name = "button_RestoreOriginalLoop";
            button_RestoreOriginalLoop.UseVisualStyleBackColor = false;
            button_RestoreOriginalLoop.Click += Button_RestoreOriginalLoop_Click;
            // 
            // numericUpDown_LoopStart
            // 
            numericUpDown_LoopStart.BackColor = Color.White;
            resources.ApplyResources(numericUpDown_LoopStart, "numericUpDown_LoopStart");
            numericUpDown_LoopStart.ForeColor = Color.FromArgb(31, 35, 40);
            numericUpDown_LoopStart.Name = "numericUpDown_LoopStart";
            numericUpDown_LoopStart.ValueChanged += NumericUpDown_LoopStart_ValueChanged;
            // 
            // numericUpDown_LoopEnd
            // 
            numericUpDown_LoopEnd.BackColor = Color.White;
            resources.ApplyResources(numericUpDown_LoopEnd, "numericUpDown_LoopEnd");
            numericUpDown_LoopEnd.ForeColor = Color.FromArgb(31, 35, 40);
            numericUpDown_LoopEnd.Name = "numericUpDown_LoopEnd";
            numericUpDown_LoopEnd.ValueChanged += NumericUpDown_LoopEnd_ValueChanged;
            // 
            // checkBox_LoopEnable
            // 
            resources.ApplyResources(checkBox_LoopEnable, "checkBox_LoopEnable");
            checkBox_LoopEnable.BackColor = Color.FromArgb(246, 247, 249);
            checkBox_LoopEnable.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            checkBox_LoopEnable.FlatAppearance.CheckedBackColor = Color.FromArgb(0, 120, 212);
            checkBox_LoopEnable.ForeColor = Color.FromArgb(31, 35, 40);
            checkBox_LoopEnable.Name = "checkBox_LoopEnable";
            checkBox_LoopEnable.UseVisualStyleBackColor = false;
            checkBox_LoopEnable.CheckedChanged += CheckBox_LoopEnable_CheckedChanged;
            // 
            // label_File
            // 
            label_File.AutoEllipsis = true;
            label_File.BackColor = Color.FromArgb(246, 247, 249);
            resources.ApplyResources(label_File, "label_File");
            label_File.ForeColor = Color.FromArgb(31, 35, 40);
            label_File.Name = "label_File";
            // 
            // button_Prev
            // 
            button_Prev.BackColor = Color.White;
            button_Prev.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_Prev.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_Prev.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            resources.ApplyResources(button_Prev, "button_Prev");
            button_Prev.ForeColor = Color.FromArgb(31, 35, 40);
            button_Prev.Name = "button_Prev";
            button_Prev.UseVisualStyleBackColor = false;
            button_Prev.Click += Button_Prev_Click;
            // 
            // button_Next
            // 
            button_Next.BackColor = Color.White;
            button_Next.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_Next.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_Next.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            resources.ApplyResources(button_Next, "button_Next");
            button_Next.ForeColor = Color.FromArgb(31, 35, 40);
            button_Next.Name = "button_Next";
            button_Next.UseVisualStyleBackColor = false;
            button_Next.Click += Button_Next_Click;
            // 
            // label_Psamples
            // 
            label_Psamples.BackColor = Color.FromArgb(246, 247, 249);
            resources.ApplyResources(label_Psamples, "label_Psamples");
            label_Psamples.ForeColor = Color.FromArgb(31, 35, 40);
            label_Psamples.Name = "label_Psamples";
            // 
            // label_Plength
            // 
            label_Plength.BackColor = Color.FromArgb(246, 247, 249);
            resources.ApplyResources(label_Plength, "label_Plength");
            label_Plength.ForeColor = Color.FromArgb(31, 35, 40);
            label_Plength.Name = "label_Plength";
            // 
            // volumeSlider1
            // 
            volumeSlider1.ForeColor = Color.FromArgb(31, 35, 40);
            resources.ApplyResources(volumeSlider1, "volumeSlider1");
            volumeSlider1.Name = "volumeSlider1";
            volumeSlider1.VolumeChanged += VolumeSlider1_VolumeChanged;
            // 
            // label_Volume
            // 
            resources.ApplyResources(label_Volume, "label_Volume");
            label_Volume.BackColor = Color.FromArgb(246, 247, 249);
            label_Volume.ForeColor = Color.FromArgb(31, 35, 40);
            label_Volume.Name = "label_Volume";
            // 
            // button_LS_Current
            // 
            button_LS_Current.BackColor = Color.White;
            resources.ApplyResources(button_LS_Current, "button_LS_Current");
            button_LS_Current.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_LS_Current.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_LS_Current.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            button_LS_Current.ForeColor = Color.FromArgb(31, 35, 40);
            button_LS_Current.Name = "button_LS_Current";
            button_LS_Current.UseVisualStyleBackColor = false;
            button_LS_Current.Click += Button_LS_Current_Click;
            // 
            // button_LE_Current
            // 
            button_LE_Current.BackColor = Color.White;
            resources.ApplyResources(button_LE_Current, "button_LE_Current");
            button_LE_Current.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_LE_Current.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_LE_Current.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            button_LE_Current.ForeColor = Color.FromArgb(31, 35, 40);
            button_LE_Current.Name = "button_LE_Current";
            button_LE_Current.UseVisualStyleBackColor = false;
            button_LE_Current.Click += Button_LE_Current_Click;
            // 
            // label_start
            // 
            label_start.BackColor = Color.FromArgb(246, 247, 249);
            resources.ApplyResources(label_start, "label_start");
            label_start.ForeColor = Color.FromArgb(31, 35, 40);
            label_start.Name = "label_start";
            // 
            // label_trk
            // 
            label_trk.BackColor = Color.FromArgb(246, 247, 249);
            resources.ApplyResources(label_trk, "label_trk");
            label_trk.ForeColor = Color.FromArgb(31, 35, 40);
            label_trk.Name = "label_trk";
            // 
            // label_end
            // 
            label_end.BackColor = Color.FromArgb(246, 247, 249);
            resources.ApplyResources(label_end, "label_end");
            label_end.ForeColor = Color.FromArgb(31, 35, 40);
            label_end.Name = "label_end";
            // 
            // panSlider1
            // 
            panSlider1.ForeColor = Color.FromArgb(31, 35, 40);
            resources.ApplyResources(panSlider1, "panSlider1");
            panSlider1.Name = "panSlider1";
            panSlider1.Pan = 0F;
            panSlider1.PanChanged += PanSlider1_PanChanged;
            // 
            // label_Pan
            // 
            resources.ApplyResources(label_Pan, "label_Pan");
            label_Pan.BackColor = Color.FromArgb(246, 247, 249);
            label_Pan.ForeColor = Color.FromArgb(31, 35, 40);
            label_Pan.Name = "label_Pan";
            // 
            // button_PanCenter
            // 
            button_PanCenter.BackColor = Color.White;
            resources.ApplyResources(button_PanCenter, "button_PanCenter");
            button_PanCenter.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);
            button_PanCenter.FlatAppearance.MouseDownBackColor = Color.FromArgb(202, 216, 231);
            button_PanCenter.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 241);
            button_PanCenter.ForeColor = Color.FromArgb(31, 35, 40);
            button_PanCenter.Name = "button_PanCenter";
            button_PanCenter.UseVisualStyleBackColor = false;
            button_PanCenter.Click += Button_PanCenter_Click;
            // 
            // customTrackBar_End
            // 
            customTrackBar_End.BackgroundColor = SystemColors.Control;
            customTrackBar_End.DraggedThumbColor = Color.DarkRed;
            resources.ApplyResources(customTrackBar_End, "customTrackBar_End");
            customTrackBar_End.ForeColor = Color.FromArgb(31, 35, 40);
            customTrackBar_End.Maximum = 100;
            customTrackBar_End.Minimum = 0;
            customTrackBar_End.Name = "customTrackBar_End";
            customTrackBar_End.Orientation = Orientation.Horizontal;
            customTrackBar_End.OverlayText = "";
            customTrackBar_End.OverlayTextSize = new Size(0, 0);
            customTrackBar_End.OverlayTextTop = 0;
            customTrackBar_End.Shape = src.Controls.CustomTrackBar.ThumbShape.DownArrow;
            customTrackBar_End.ShowLPCSamples = true;
            customTrackBar_End.ShowTicks = true;
            customTrackBar_End.ThumbColor = Color.Red;
            customTrackBar_End.ThumbHeight = 22;
            customTrackBar_End.ThumbSize = 10;
            customTrackBar_End.ThumbWidth = 8;
            customTrackBar_End.TickColor = Color.Black;
            customTrackBar_End.TickFrequency = 10;
            customTrackBar_End.TickPos = src.Controls.CustomTrackBar.TickPosition.Below;
            customTrackBar_End.TickSize = 5;
            customTrackBar_End.TrackColor = Color.Gainsboro;
            customTrackBar_End.TrackThickness = 8;
            customTrackBar_End.Value = 0;
            customTrackBar_End.Scroll += CustomTrackBar_End_Scroll;
            // 
            // customTrackBar_Trk
            // 
            customTrackBar_Trk.BackgroundColor = SystemColors.Control;
            customTrackBar_Trk.DraggedThumbColor = Color.DarkBlue;
            customTrackBar_Trk.ForeColor = Color.FromArgb(31, 35, 40);
            resources.ApplyResources(customTrackBar_Trk, "customTrackBar_Trk");
            customTrackBar_Trk.Maximum = 100;
            customTrackBar_Trk.Minimum = 0;
            customTrackBar_Trk.Name = "customTrackBar_Trk";
            customTrackBar_Trk.Orientation = Orientation.Horizontal;
            customTrackBar_Trk.OverlayText = "";
            customTrackBar_Trk.OverlayTextSize = new Size(0, 0);
            customTrackBar_Trk.OverlayTextTop = 0;
            customTrackBar_Trk.Shape = src.Controls.CustomTrackBar.ThumbShape.Rectangle;
            customTrackBar_Trk.ShowLPCSamples = true;
            customTrackBar_Trk.ShowTicks = true;
            customTrackBar_Trk.ThumbColor = Color.DodgerBlue;
            customTrackBar_Trk.ThumbHeight = 63;
            customTrackBar_Trk.ThumbSize = 10;
            customTrackBar_Trk.ThumbWidth = 4;
            customTrackBar_Trk.TickColor = Color.Black;
            customTrackBar_Trk.TickFrequency = 10;
            customTrackBar_Trk.TickPos = src.Controls.CustomTrackBar.TickPosition.Both;
            customTrackBar_Trk.TickSize = 5;
            customTrackBar_Trk.TrackColor = Color.Gainsboro;
            customTrackBar_Trk.TrackThickness = 30;
            customTrackBar_Trk.Value = 0;
            customTrackBar_Trk.Scroll += CustomTrackBar_Trk_Scroll;
            // 
            // customTrackBar_Start
            // 
            customTrackBar_Start.BackgroundColor = SystemColors.Control;
            customTrackBar_Start.DraggedThumbColor = Color.DarkGreen;
            resources.ApplyResources(customTrackBar_Start, "customTrackBar_Start");
            customTrackBar_Start.ForeColor = Color.FromArgb(31, 35, 40);
            customTrackBar_Start.Maximum = 100;
            customTrackBar_Start.Minimum = 0;
            customTrackBar_Start.Name = "customTrackBar_Start";
            customTrackBar_Start.Orientation = Orientation.Horizontal;
            customTrackBar_Start.OverlayText = "";
            customTrackBar_Start.OverlayTextSize = new Size(0, 0);
            customTrackBar_Start.OverlayTextTop = 0;
            customTrackBar_Start.Shape = src.Controls.CustomTrackBar.ThumbShape.UpArrow;
            customTrackBar_Start.ShowLPCSamples = true;
            customTrackBar_Start.ShowTicks = true;
            customTrackBar_Start.ThumbColor = Color.Green;
            customTrackBar_Start.ThumbHeight = 25;
            customTrackBar_Start.ThumbSize = 10;
            customTrackBar_Start.ThumbWidth = 8;
            customTrackBar_Start.TickColor = Color.Black;
            customTrackBar_Start.TickFrequency = 10;
            customTrackBar_Start.TickPos = src.Controls.CustomTrackBar.TickPosition.Above;
            customTrackBar_Start.TickSize = 5;
            customTrackBar_Start.TrackColor = Color.Gainsboro;
            customTrackBar_Start.TrackThickness = 10;
            customTrackBar_Start.Value = 0;
            customTrackBar_Start.Scroll += CustomTrackBar_Start_Scroll;
            // 
            // label_previewwarn
            // 
            resources.ApplyResources(label_previewwarn, "label_previewwarn");
            label_previewwarn.BackColor = Color.FromArgb(246, 247, 249);
            label_previewwarn.ForeColor = Color.FromArgb(31, 35, 40);
            label_previewwarn.Name = "label_previewwarn";
            // 
            // FormLPC
            // 
            AcceptButton = button_OK;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ControlBox = false;
            Controls.Add(label_end);
            Controls.Add(label_LoopStartSamples);
            Controls.Add(label_LoopEndSamples);
            Controls.Add(label_previewwarn);
            Controls.Add(label_trk);
            Controls.Add(customTrackBar_Start);
            Controls.Add(customTrackBar_Trk);
            Controls.Add(customTrackBar_End);
            Controls.Add(button_PanCenter);
            Controls.Add(label_Pan);
            Controls.Add(panSlider1);
            Controls.Add(label_start);
            Controls.Add(button_LE_Current);
            Controls.Add(button_LS_Current);
            Controls.Add(label_Volume);
            Controls.Add(volumeSlider1);
            Controls.Add(label_Plength);
            Controls.Add(label_Psamples);
            Controls.Add(button_Next);
            Controls.Add(button_Prev);
            Controls.Add(label_File);
            Controls.Add(checkBox_LoopEnable);
            Controls.Add(numericUpDown_LoopEnd);
            Controls.Add(numericUpDown_LoopStart);
            Controls.Add(button_RestoreOriginalLoop);
            Controls.Add(button_SetEnd);
            Controls.Add(button_SetStart);
            Controls.Add(label_Length);
            Controls.Add(label_Samples);
            Controls.Add(button_Cancel);
            Controls.Add(button_OK);
            Controls.Add(button_Stop);
            Controls.Add(button_Play);
            DoubleBuffered = true;
            ForeColor = Color.FromArgb(31, 35, 40);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "FormLPC";
            FormClosed += FormLPC_FormClosed;
            Load += FormLPC_Load;
            Paint += FormLPC_Paint;
            ((System.ComponentModel.ISupportInitialize)numericUpDown_LoopStart).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_LoopEnd).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button_Play;
        private Button button_Stop;
        private Button button_OK;
        private Button button_Cancel;
        private Label label_Samples;
        private System.Windows.Forms.Timer timer_Reload;
        private Label label_Length;
        private Label label_LoopStartSamples;
        private Label label_LoopEndSamples;
        private Button button_SetStart;
        private Button button_SetEnd;
        private Button button_RestoreOriginalLoop;
        private Label label_File;
        private Button button_Prev;
        private Button button_Next;
        private Label label_Plength;
        private NAudio.Gui.VolumeSlider volumeSlider1;
        private Label label_Volume;
        private Button button_LS_Current;
        private Button button_LE_Current;
        private Label label_start;
        private Label label_trk;
        private Label label_end;
        private NAudio.Gui.PanSlider panSlider1;
        private Label label_Pan;
        private Button button_PanCenter;
        private src.Controls.CustomTrackBar customTrackBar_Trk;
        internal Label label_Psamples;
        private Label label_previewwarn;
        internal CheckBox checkBox_LoopEnable;
        internal NumericUpDown numericUpDown_LoopStart;
        internal NumericUpDown numericUpDown_LoopEnd;
        internal src.Controls.CustomTrackBar customTrackBar_End;
        internal src.Controls.CustomTrackBar customTrackBar_Start;
    }
}
