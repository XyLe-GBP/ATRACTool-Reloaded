using NAudio.Wave;
using ATRACTool_Reloaded.Localizable;
using System.Text;
using static ATRACTool_Reloaded.Common;
using NAudio.Utils;
using Microsoft.VisualBasic;
using NAudio.Wave.SampleProviders;
using System;

namespace ATRACTool_Reloaded
{
    public partial class FormLPC : Form
    {
        private readonly WaveInEvent wi = new();
        private readonly WaveOutEvent wo = new();
        WaveFileReader reader;
        BufferedWaveProvider BufwaveProvider;
        VolumeSampleProvider volumeSmplProvider;
        PanningSampleProvider panSmplProvider;
        long Sample, Start = 0, End = 0;
        int bytePerSec, position, length, btnpos, smplrate;
        TimeSpan time;
        bool mouseDown = false, stopflag = false, IsPausedMoveTrackbar, SmoothSamples = false, IsPlaybackATRAC = false;
        float ScaleWidthTrk = 0f, ScaleWidthStart = 0f, ScaleWidthEnd = 0f;
        Point labelTrk = new(15, 70), labelStart = new(15, 130), labelEnd = new(15, 10);

        private volatile bool SLTAlive;
        ThreadStart StartPlaybackThread_s;
        Thread Playback_s;

        private static FormLPC _formLPCInstance = null!;
        public static FormLPC FormLPCInstance
        {
            get
            {
                return _formLPCInstance;
            }
            set
            {
                _formLPCInstance = value;
            }
        }

        public string CautionLabel
        {
            set
            {
                label_previewwarn.Text = value;
            }
        }

        public string SampleLabel
        {
            get
            {
                return label_Psamples.Text;
            }
        }

        public bool LoopCheckEnable
        {
            get
            {
                return checkBox_LoopEnable.Checked;
            }
            set
            {
                checkBox_LoopEnable.Checked = value;
            }
        }

        public string LoopStartLabel
        {
            get
            {
                return label_LoopStartSamples.Text;
            }
        }

        public long LoopStart
        {
            get
            {
                return Start;
            }
        }

        public string LoopEndLabel
        {
            get
            {
                return label_LoopEndSamples.Text;
            }
        }

        public long LoopEnd
        {
            get
            {
                return End;
            }
        }

        public int SampleRate
        {
            get
            {
                return smplrate;
            }
        }

        public FormLPC(bool IsEnabledBtn)
        {
            InitializeComponent();

            if (!IsEnabledBtn)
            {
                FormBorderStyle = FormBorderStyle.None;
                button_OK.Enabled = false;
                button_OK.Visible = false;
                button_Cancel.Enabled = false;
                button_Cancel.Visible = false;
            }
            customTrackBar_Trk.Scroll += customTrackBar_Trk_Scroll;
            customTrackBar_Start.Scroll += customTrackBar_Start_Scroll;
            customTrackBar_End.Scroll += customTrackBar_End_Scroll;
            customTrackBar_Trk.MouseDown += customTrackBar_Trk_MouseDown;
            customTrackBar_Trk.MouseUp += customTrackBar_Trk_MouseUp;
            label_trk.Text = "0";
            label_start.Text = "0";
            label_end.Text = "0";
            label_Samples.Text = Localization.SampleCaption + ":";
            label_Psamples.Text = "0";
            label_Length.Text = Localization.LengthCaption + ":";
            label_Plength.Text = "00:00:00";
            label_LoopStartSamples.Text = "";
            label_LoopEndSamples.Text = "";
            label_previewwarn.Text = "";
            timer_Reload.Interval = 1;

            //label_trk.Visible = false;
            label_start.Visible = false;
            label_end.Visible = false;
        }

        private void customTrackBar_End_Scroll(object? sender, EventArgs e)
        {
            label_end.Text = customTrackBar_End.Value.ToString();
            SetTrackbarEnd();
            //label_end.Location = new Point(labelEnd.X + (int)((customTrackBar_End.Value - customTrackBar_End.Minimum) * ScaleWidthEnd) - customTrackBar_End.Location.X - labelEnd.X, labelEnd.Y);
            numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
        }

        private void customTrackBar_Start_Scroll(object? sender, EventArgs e)
        {
            label_start.Text = customTrackBar_Start.Value.ToString();
            SetTrackbarStart();
            //label_start.Location = new Point(labelStart.X + (int)((customTrackBar_Start.Value - customTrackBar_Start.Minimum) * ScaleWidthStart) - customTrackBar_Start.Location.X - labelStart.X, labelStart.Y);
            numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
        }

        private void customTrackBar_Trk_Scroll(object? sender, EventArgs e)
        {
            if (wo.PlaybackState == PlaybackState.Paused) IsPausedMoveTrackbar = true;
            label_trk.Text = customTrackBar_Trk.Value.ToString();
            SetTrackbarTrack();
            //label_trk.Location = new Point(labelTrk.X + (int)((customTrackBar_Trk.Value - customTrackBar_Trk.Minimum) * ScaleWidthTrk) - customTrackBar_Trk.Location.X - labelTrk.X, labelTrk.Y);

            reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Trk.Value);
            //Sample = reader.Position;
            Sample = reader.Position / reader.BlockAlign;
        }

        private void customTrackBar_Trk_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!mouseDown) return;
            mouseDown = false;
        }

        private void customTrackBar_Trk_MouseDown(object? sender, MouseEventArgs e)
        {
            if (reader == null) return;
            mouseDown = true;
        }

        private void FormLPC_Load(object sender, EventArgs e)
        {
            Config.Load(xmlpath);

            FormLPCInstance = this;

            switch (bool.Parse(Config.Entry["SmoothSamples"].Value))
            {
                case true:
                    SmoothSamples = true;
                    break;
                case false:
                    SmoothSamples = false;
                    break;
            }

            switch (bool.Parse(Config.Entry["PlaybackATRAC"].Value))
            {
                case true:
                    IsPlaybackATRAC = true;
                    break;
                case false:
                    IsPlaybackATRAC = false;
                    break;
            }

            if (SmoothSamples)
            {
                wo.DesiredLatency = 200; // 250
                wo.NumberOfBuffers = 16; // 8
            }
            else
            {
                wo.DesiredLatency = 300; // 250
                wo.NumberOfBuffers = 3; // 8
            }

            if (IsPlaybackATRAC && Generic.IsATRAC)
            {
                checkBox_LoopEnable.Enabled = false;
                radioButton_at3.Enabled = false;
                radioButton_at9.Enabled = false;
                if (Common.Generic.pATRACOpenFilePaths.Length == 1)
                {
                    reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                    FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                    button_Prev.Enabled = false;
                    button_Next.Enabled = false;
                }
                else
                {
                    reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                    FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                    button_Prev.Enabled = false;
                    button_Next.Enabled = true;
                    btnpos = 1;
                }
            }
            else
            {
                if (Common.Generic.OpenFilePaths.Length == 1)
                {
                    reader = new(Common.Generic.OpenFilePaths[0]);
                    FileInfo fi = new(Common.Generic.OpenFilePaths[0]);
                    label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                    button_Prev.Enabled = false;
                    button_Next.Enabled = false;
                }
                else
                {
                    if (Common.Generic.lpcreate != false)
                    {
                        reader = new(Common.Generic.OpenFilePaths[Common.Generic.files]);
                        FileInfo fi = new(Common.Generic.OpenFilePaths[Common.Generic.files]);
                        label_File.Text = fi.Name;
                        button_Prev.Enabled = false;
                        button_Next.Enabled = false;

                        switch (Common.Generic.ATRACFlag)
                        {
                            case 0:
                                checkBox_LoopEnable.Checked = true;
                                checkBox_LoopEnable.Enabled = false;
                                radioButton_at3.Checked = true;
                                radioButton_at9.Checked = false;
                                radioButton_at9.Enabled = false;
                                button_Cancel.Enabled = false;
                                break;
                            case 1:
                                checkBox_LoopEnable.Checked = true;
                                checkBox_LoopEnable.Enabled = false;
                                radioButton_at3.Checked = false;
                                radioButton_at3.Enabled = false;
                                radioButton_at9.Checked = true;
                                button_Cancel.Enabled = false;
                                break;
                        }
                    }
                    else
                    {
                        reader = new(Common.Generic.OpenFilePaths[0]);
                        FileInfo fi = new(Common.Generic.OpenFilePaths[0]);
                        label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                        button_Prev.Enabled = false;
                        button_Next.Enabled = true;
                        btnpos = 1;
                    }
                }
            }

            wo.Init(new WaveChannel32(reader));
            BufwaveProvider = new BufferedWaveProvider(reader.WaveFormat);
            BufwaveProvider.BufferDuration = TimeSpan.FromMilliseconds(500); // バッファの長さを設定
            //wo.Init(BufwaveProvider);
            volumeSmplProvider = new VolumeSampleProvider(BufwaveProvider.ToSampleProvider());
            if (reader.WaveFormat.Channels == 1)
            {
                panSmplProvider = new PanningSampleProvider(volumeSmplProvider);
                label_Pan.Enabled = true;
                panSlider1.Enabled = true;
            }
            else
            {
                label_Pan.Enabled = false;
                panSlider1.Enabled = false;
            }
            customTrackBar_Trk.Minimum = 0;
            customTrackBar_Trk.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            //customTrackBar_Trk.Maximum = (int)reader.TotalTime.TotalMinutes;
            customTrackBar_Start.Minimum = 0;
            customTrackBar_Start.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            customTrackBar_End.Minimum = 0;
            customTrackBar_End.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            ScaleWidthTrk = (float)customTrackBar_Trk.Size.Width / ((float)customTrackBar_Trk.Maximum - (float)customTrackBar_Trk.Minimum);
            ScaleWidthStart = (float)customTrackBar_Start.Size.Width / ((float)customTrackBar_Start.Maximum - (float)customTrackBar_Start.Minimum);
            ScaleWidthEnd = (float)customTrackBar_End.Size.Width / ((float)customTrackBar_End.Maximum - (float)customTrackBar_End.Minimum);
            //customTrackBar_End.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            customTrackBar_Trk.TickFrequency = 1000;
            customTrackBar_Start.TickFrequency = 1000;
            customTrackBar_End.TickFrequency = 1000;
            numericUpDown_LoopStart.Minimum = 0;
            numericUpDown_LoopStart.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            numericUpDown_LoopStart.Increment = 1;
            numericUpDown_LoopEnd.Minimum = 0;
            numericUpDown_LoopEnd.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            numericUpDown_LoopEnd.Increment = 1;
            wo.Volume = volumeSlider1.Volume;

            label_trk.Text = "";
            label_start.Text = "";
            label_end.Text = "";

            int tb = (int)reader.TotalTime.TotalMilliseconds / 2;
            numericUpDown_LoopEnd.Value = tb;

            Generic.LPCTotalSamples = reader.SampleCount;
        }

        private async void Button_Play_Click(object sender, EventArgs e)
        {
            switch (wo.PlaybackState)
            {
                case PlaybackState.Stopped:
                    bytePerSec = reader.WaveFormat.BitsPerSample / 8 * reader.WaveFormat.SampleRate * reader.WaveFormat.Channels;
                    length = (int)reader.Length / bytePerSec;

                    timer_Reload.Enabled = true;
                    wo.Play();
                    button_Play.Text = Localization.PauseCaption;
                    await Task.Run(Playback);
                    stopflag = false;
                    button_Stop.Enabled = true;
                    break;
                case PlaybackState.Paused:
                    if (IsPausedMoveTrackbar)
                    {
                        wo.Stop();
                        reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Trk.Value);
                        wo.Play();
                        await Task.Run(Playback);
                        IsPausedMoveTrackbar = false;
                    }
                    else
                    {
                        wo.Play();
                    }

                    button_Play.Text = Localization.PauseCaption;
                    break;
                case PlaybackState.Playing:
                    wo.Pause();
                    button_Play.Text = Localization.PlayCaption;
                    break;
            }
        }

        private void Button_Stop_Click(object sender, EventArgs e)
        {
            if (wo.PlaybackState != PlaybackState.Stopped)
            {
                stopflag = true;
                timer_Reload.Stop();
                wo.Stop();
                button_Play.Text = Localization.PlayCaption;
                reader.Position = 0;
                button_Stop.Enabled = false;
                Resettrackbarlabels();
            }
        }

        private void Timer_Reload_Tick(object sender, EventArgs e)
        {
            if (!mouseDown) customTrackBar_Trk.Value = (int)reader.CurrentTime.TotalMilliseconds;
            if (checkBox_LoopEnable.Checked == true && reader.CurrentTime >= TimeSpan.FromMilliseconds(customTrackBar_End.Value))
            {
                reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Start.Value);
                Sample = reader.Position / reader.BlockAlign;
                /*if (SmoothSamples)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        Sample = reader.Position / reader.BlockAlign;
                    });
                    //Sample = reader.Position / reader.BlockAlign;
                }
                else
                {
                    Sample = reader.Position / reader.BlockAlign;
                    //Sample = reader.CurrentTime.Ticks * reader.WaveFormat.SampleRate;
                }*/
            }

            if (reader.CurrentTime == reader.TotalTime)
            {
                stopflag = true;
                Sample = reader.SampleCount;
                wo.Stop();
                button_Play.Text = Localization.PlayCaption;
                reader.Position = 0;
                button_Stop.Enabled = false;
                Resettrackbarlabels();
            }
            else if (reader.Position == 0 || customTrackBar_Trk.Value == 0)
            {
                if (!stopflag)
                {
                    wo.Stop();
                    button_Stop.Enabled = false;
                    Sample = 0;
                    reader.Position = 0;
                    wo.Play();
                    button_Play.Text = Localization.PauseCaption;
                    Task.Run(Playback);
                    button_Stop.Enabled = true;
                }
            }

            SetTrackbarTrack();
            SetTrackbarStart();
            SetTrackbarEnd();
            StringBuilder str = new(Sample.ToString());

            label_trk.Text = customTrackBar_Trk.Value.ToString();
            label_start.Text = customTrackBar_Start.Value.ToString();
            label_end.Text = customTrackBar_End.Value.ToString();
            label_Length.Text = Localization.LengthCaption + ":";
            label_Plength.Text = time.ToString(@"hh\:mm\:ss");

            label_Samples.Text = Localization.SampleCaption + ":";
            label_Psamples.Text = str.ToString();

            /*if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    label_trk.Text = customTrackBar_Trk.Value.ToString();
                    label_start.Text = customTrackBar_Start.Value.ToString();
                    label_end.Text = customTrackBar_End.Value.ToString();
                    label_Length.Text = Localization.LengthCaption + ":";
                    label_Plength.Text = time.ToString(@"hh\:mm\:ss");

                    label_Samples.Text = Localization.SampleCaption + ":";
                    label_Psamples.Text = str.ToString();
                });
            }
            else
            {
                label_trk.Text = customTrackBar_Trk.Value.ToString();
                label_start.Text = customTrackBar_Start.Value.ToString();
                label_end.Text = customTrackBar_End.Value.ToString();
                label_Length.Text = Localization.LengthCaption + ":";
                label_Plength.Text = time.ToString(@"hh\:mm\:ss");

                label_Samples.Text = Localization.SampleCaption + ":";
                label_Psamples.Text = str.ToString();
            }*/
        }

        private void Playback()
        {
            object lockobj = new();
            lock (lockobj)
            {
                SLTAlive = true;
                StartPlaybackThread_s = new(StartPlaybackThread);
                Playback_s = new(StartPlaybackThread_s)
                {
                    Name = "waveOut",
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                Playback_s.SetApartmentState(ApartmentState.STA);
                Playback_s.Start();
            }

            /*while (wo.PlaybackState != PlaybackState.Stopped)
            {
                position = (int)reader.Position / reader.WaveFormat.AverageBytesPerSecond;
                time = new(0, 0, position);
                Sample = reader.Position / reader.BlockAlign + wo.GetPosition() / reader.BlockAlign;
            }*/
            //Sample = 0;
        }

        private void StartPlaybackThread()
        {
            while (wo.PlaybackState != PlaybackState.Stopped)
            {
                if (!SLTAlive) { return; }
                position = (int)reader.Position / reader.WaveFormat.AverageBytesPerSecond;
                time = new(0, 0, position);
                Sample = reader.Position / reader.BlockAlign;
                /*if (SmoothSamples)
                {
                    Sample = reader.Position / reader.BlockAlign + wo.GetPosition() / reader.BlockAlign;
                }
                else
                {
                    Sample = reader.Position / reader.BlockAlign;
                }*/
            }
        }

        private void FormLPC_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Button_SetStart_Click(object sender, EventArgs e)
        {
            long pos;
            TimeSpan oldc = reader.CurrentTime;
            reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Start.Value);
            pos = reader.Position / reader.WaveFormat.BlockAlign;
            reader.CurrentTime = oldc;
            label_LoopStartSamples.Text = "LoopStart: " + pos.ToString() + " " + Localization.SampleCaption;
            Start = pos;
        }

        private void FormLPC_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (timer_Reload.Enabled == true) timer_Reload.Enabled = false;
            SLTAlive = false;
            reader.Position = 0;
            wi.StopRecording();
            wi.Dispose();
            wo.Stop();
            wo.Dispose();
            reader.Close();
            reader = null!;
        }

        private void Button_Prev_Click(object sender, EventArgs e)
        {
            btnpos--;

            Generic.IsLPCStreamingReloaded = true;
            string[] Paths, OriginPaths;
            if (IsPlaybackATRAC && Generic.IsATRAC)
            {
                Paths = Generic.pATRACOpenFilePaths;
                OriginPaths = Generic.OpenFilePaths;
            }
            else
            {
                Paths = Generic.OpenFilePaths;
                OriginPaths = Generic.OriginOpenFilePaths;
            }

            FileInfo fi = new(Paths[btnpos - 1]);
            FileInfo fiorig = new(OriginPaths[btnpos - 1]);

            long FS = fiorig.Length;

            wo.Stop();
            button_Play.Text = Localization.PlayCaption;
            reader.Position = 0;
            reader.Close();
            button_Stop.Enabled = false;

            if (btnpos == 1)
            {
                reader = new(Paths[btnpos - 1]);
                wo.Init(new WaveChannel32(reader));
                ResetAFR();
                label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                FormMain.FormMainInstance.FPLabel = fiorig.Directory + @"\" + fiorig.Name;
                FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, FS / 1024);
                button_Prev.Enabled = false;
                button_Next.Enabled = true;
            }
            else
            {
                reader = new(Paths[btnpos - 1]);
                wo.Init(new WaveChannel32(reader));
                ResetAFR();
                label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                FormMain.FormMainInstance.FPLabel = fiorig.Directory + @"\" + fiorig.Name;
                FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, FS / 1024);
                button_Prev.Enabled = true;
                button_Next.Enabled = true;
            }
            Generic.IsLPCStreamingReloaded = false;
        }

        private void Button_Next_Click(object sender, EventArgs e)
        {
            btnpos++;

            Generic.IsLPCStreamingReloaded = true;
            string[] Paths, OriginPaths;
            if (IsPlaybackATRAC && Generic.IsATRAC)
            {
                Paths = Generic.pATRACOpenFilePaths;
                OriginPaths = Generic.OpenFilePaths;
            }
            else
            {
                Paths = Generic.OpenFilePaths;
                OriginPaths = Generic.OriginOpenFilePaths;
            }

            FileInfo fi = new(Paths[btnpos - 1]);
            FileInfo fiorig = new(OriginPaths[btnpos - 1]);

            long FS = fiorig.Length;

            wo.Stop();
            button_Play.Text = Localization.PlayCaption;
            reader.Position = 0;
            reader.Close();
            button_Stop.Enabled = false;
            Resettrackbarlabels();

            if (btnpos == Paths.Length)
            {
                reader = new(Paths[btnpos - 1]);
                wo.Init(new WaveChannel32(reader));
                ResetAFR();
                label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                FormMain.FormMainInstance.FPLabel = fiorig.Directory + @"\" + fiorig.Name;
                FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, FS / 1024);
                button_Next.Enabled = false;
                button_Prev.Enabled = true;
            }
            else
            {
                reader = new(Paths[btnpos - 1]);
                wo.Init(new WaveChannel32(reader));
                ResetAFR();
                label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                FormMain.FormMainInstance.FPLabel = fiorig.Directory + @"\" + fiorig.Name;
                FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, FS / 1024);
                button_Next.Enabled = true;
                button_Prev.Enabled = true;
            }
            Generic.IsLPCStreamingReloaded = false;
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void VolumeSlider1_VolumeChanged(object sender, EventArgs e)
        {
            wo.Volume = volumeSlider1.Volume;
        }

        private void Button_LS_Current_Click(object sender, EventArgs e)
        {
            customTrackBar_Start.Value = customTrackBar_Trk.Value;
            numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
        }

        private void Button_LE_Current_Click(object sender, EventArgs e)
        {
            customTrackBar_End.Value = customTrackBar_Trk.Value;
            numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
        }

        private void CheckBox_LoopEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_LoopEnable.Checked != false)
            {
                if (radioButton_at3.Checked)
                {
                    if (CheckLoopSoundEnabled(false))
                    {
                        MessageBox.Show(this, "If the item to loop from the beginning to the end of the sound source is enabled in the settings screen, the loop range setting will not be reflected.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ResetLoopEnable();
                        return;
                    }
                }
                else if (radioButton_at9.Checked)
                {
                    if (CheckLoopSoundEnabled(true))
                    {
                        MessageBox.Show(this, "If the item to loop from the beginning to the end of the sound source is enabled in the settings screen, the loop range setting will not be reflected.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ResetLoopEnable();
                        return;
                    }
                }

                customTrackBar_Start.Enabled = true;
                customTrackBar_End.Enabled = true;
                numericUpDown_LoopStart.Enabled = true;
                numericUpDown_LoopEnd.Enabled = true;
                button_LS_Current.Enabled = true;
                button_LE_Current.Enabled = true;
                button_SetStart.Enabled = true;
                button_SetEnd.Enabled = true;
            }
            else
            {
                customTrackBar_Start.Enabled = false;
                customTrackBar_End.Enabled = false;
                numericUpDown_LoopStart.Enabled = false;
                numericUpDown_LoopEnd.Enabled = false;
                button_LS_Current.Enabled = false;
                button_LE_Current.Enabled = false;
                button_SetStart.Enabled = false;
                button_SetEnd.Enabled = false;
            }
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            if (checkBox_LoopEnable.Checked == true)
            {
                if (Start == 0 || End == 0)
                {
                    MessageBox.Show(this, Localization.LoopNotSetCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Start == End)
                {
                    MessageBox.Show(this, "Incorrect loop value.\r\nLoop start and loop end values cannot be the same.", Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                double result = Math.Sign(End - Start);
                if (result == -1)
                {
                    MessageBox.Show(this, "Incorrect loop value.\r\nNegative value between loop start and loop end values.", Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (result == 1)
                {
                    long loopdistancevalue = End - Start;
                    if (loopdistancevalue <= 1000 && loopdistancevalue >= 0)
                    {
                        MessageBox.Show(this, "Incorrect loop value.\r\nThe interval between the loop start and loop end values must be greater than or equal to 1000.", Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(this, "Incorrect loop value.\r\nLoop start and loop end values cannot be the same.", Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                wo.Stop();

                if (radioButton_at3.Checked == true)
                {
                    Config.Entry["ATRAC3_LoopSound"].Value = "false";
                    Config.Entry["ATRAC3_LoopPoint"].Value = "true";
                    Config.Entry["ATRAC3_LoopStart_Samples"].Value = Start.ToString();
                    Config.Entry["ATRAC3_LoopEnd_Samples"].Value = End.ToString();
                    Config.Entry["ATRAC3_Params"].Value = "";
                    Config.Entry["LPC_Create"].Value = "false";
                }
                else
                {
                    Config.Entry["ATRAC9_LoopSound"].Value = "false";
                    Config.Entry["ATRAC9_LoopPoint"].Value = "true";
                    Config.Entry["ATRAC9_LoopStart_Samples"].Value = Start.ToString();
                    Config.Entry["ATRAC9_LoopEnd_Samples"].Value = End.ToString();
                    Config.Entry["ATRAC9_Params"].Value = "";
                    Config.Entry["LPC_Create"].Value = "false";
                }
                Config.Save(xmlpath);

                smplrate = reader.WaveFormat.SampleRate;
                using FormSettings form = new(true);
                form.ShowDialog();

                Close();
            }
            else
            {
                wo.Stop();
                Close();
            }
        }

        private void NumericUpDown_LoopStart_ValueChanged(object sender, EventArgs e)
        {
            customTrackBar_Start.Value = (int)numericUpDown_LoopStart.Value;
        }

        private void NumericUpDown_LoopEnd_ValueChanged(object sender, EventArgs e)
        {
            customTrackBar_End.Value = (int)numericUpDown_LoopEnd.Value;
        }

        private void Button_SetEnd_Click(object sender, EventArgs e)
        {
            long pos;
            TimeSpan oldc = reader.CurrentTime;
            reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_End.Value);
            pos = reader.Position / reader.WaveFormat.BlockAlign;
            reader.CurrentTime = oldc;
            label_LoopEndSamples.Text = "LoopEnd: " + pos.ToString() + " " + Localization.SampleCaption;
            End = pos;
        }

        private void ResetAFR()
        {
            customTrackBar_Trk.Minimum = 0;
            customTrackBar_Trk.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            customTrackBar_Start.Minimum = 0;
            customTrackBar_Start.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            customTrackBar_End.Minimum = 0;
            customTrackBar_End.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            customTrackBar_Trk.TickFrequency = 1000;
            customTrackBar_Start.TickFrequency = 1000;
            customTrackBar_End.TickFrequency = 1000;
            //customTrackBar_Trk.TickFrequency = 1;
            //customTrackBar_Start.TickFrequency = 1;
            //customTrackBar_End.TickFrequency = 1;
            numericUpDown_LoopStart.Minimum = 0;
            numericUpDown_LoopStart.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            numericUpDown_LoopStart.Increment = 1;
            numericUpDown_LoopEnd.Minimum = 0;
            numericUpDown_LoopEnd.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            numericUpDown_LoopEnd.Increment = 1;
            wo.Volume = volumeSlider1.Volume;

            int tb = (int)reader.TotalTime.TotalMilliseconds / 2;
            numericUpDown_LoopStart.Value = 0;
            numericUpDown_LoopEnd.Value = tb;

            Generic.LPCTotalSamples = reader.SampleCount;

            Resettrackbarlabels();
        }

        private static bool CheckLoopSoundEnabled(bool IsAT9)
        {
            Config.Load(Common.xmlpath);
            if (IsAT9)
            {
                if (bool.Parse(Config.Entry["ATRAC9_LoopSound"].Value))
                {
                    return true;
                }
                else { return false; }
            }
            else
            {
                if (bool.Parse(Config.Entry["ATRAC3_LoopSound"].Value))
                {
                    return true;
                }
                else { return false; }
            }
        }

        private void RadioButton_at3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_LoopEnable.Checked && radioButton_at3.Checked)
            {
                if (CheckLoopSoundEnabled(false))
                {
                    MessageBox.Show(this, "If the item to loop from the beginning to the end of the sound source is enabled in the settings screen, the loop range setting will not be reflected.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetLoopEnable();
                    return;
                }
            }
        }

        private void RadioButton_at9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_LoopEnable.Checked && radioButton_at9.Checked)
            {
                if (CheckLoopSoundEnabled(true))
                {
                    MessageBox.Show(this, "If the item to loop from the beginning to the end of the sound source is enabled in the settings screen, the loop range setting will not be reflected.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetLoopEnable();
                    return;
                }
            }
        }

        private void ResetLoopEnable()
        {
            checkBox_LoopEnable.Checked = false;
            customTrackBar_Start.Enabled = false;
            customTrackBar_End.Enabled = false;
            numericUpDown_LoopStart.Enabled = false;
            numericUpDown_LoopEnd.Enabled = false;
            button_LS_Current.Enabled = false;
            button_LE_Current.Enabled = false;
            button_SetStart.Enabled = false;
            button_SetEnd.Enabled = false;
        }

        private void customTrackBar_Trk_CursorChanged(object sender, EventArgs e)
        {

        }

        private void Resettrackbarlabels()
        {
            ScaleWidthTrk = (float)customTrackBar_Trk.Size.Width / ((float)customTrackBar_Trk.Maximum - (float)customTrackBar_Trk.Minimum);
            ScaleWidthStart = (float)customTrackBar_Start.Size.Width / ((float)customTrackBar_Start.Maximum - (float)customTrackBar_Start.Minimum);
            ScaleWidthEnd = (float)customTrackBar_End.Size.Width / ((float)customTrackBar_End.Maximum - (float)customTrackBar_End.Minimum);
            labelTrk = new(15, 70);
            labelStart = new(15, 130);
            labelEnd = new(15, 10);
        }

        private void SetTrackbarTrack()
        {
            if (customTrackBar_Trk.Value < customTrackBar_Trk.Maximum / 2)
            {
                label_trk.Location = new Point(labelTrk.X + (int)((customTrackBar_Trk.Value - customTrackBar_Trk.Minimum) * ScaleWidthTrk) - customTrackBar_Trk.Location.X - labelTrk.X + 9, labelTrk.Y);
            }
            else if (customTrackBar_Trk.Value > customTrackBar_Trk.Maximum / 2)
            {
                label_trk.Location = new Point(labelTrk.X + (int)((customTrackBar_Trk.Value - customTrackBar_Trk.Minimum) * ScaleWidthTrk) - customTrackBar_Trk.Location.X - labelTrk.X - 9, labelTrk.Y);
            }
            else if (customTrackBar_Trk.Value == customTrackBar_Trk.Maximum / 2)
            {
                label_trk.Location = new Point(labelTrk.X + (int)((customTrackBar_Trk.Value - customTrackBar_Trk.Minimum) * ScaleWidthTrk) - customTrackBar_Trk.Location.X - labelTrk.X, labelTrk.Y);
            }
            else
            {
                label_trk.Location = new Point(labelTrk.X + (int)((customTrackBar_Trk.Value - customTrackBar_Trk.Minimum) * ScaleWidthTrk) - customTrackBar_Trk.Location.X - labelTrk.X, labelTrk.Y);
            }
        }

        private void SetTrackbarStart()
        {
            if (customTrackBar_Start.Value < customTrackBar_Start.Maximum / 2)
            {
                label_start.Location = new Point(labelStart.X + (int)((customTrackBar_Start.Value - customTrackBar_Start.Minimum) * ScaleWidthStart) - customTrackBar_Start.Location.X - labelStart.X + 9, labelStart.Y);
            }
            else if (customTrackBar_Start.Value > customTrackBar_Start.Maximum / 2)
            {
                label_start.Location = new Point(labelStart.X + (int)((customTrackBar_Start.Value - customTrackBar_Start.Minimum) * ScaleWidthStart) - customTrackBar_Start.Location.X - labelStart.X - 9, labelStart.Y);
            }
            else if (customTrackBar_Start.Value == customTrackBar_Start.Maximum / 2)
            {
                label_start.Location = new Point(labelStart.X + (int)((customTrackBar_Start.Value - customTrackBar_Start.Minimum) * ScaleWidthStart) - customTrackBar_Start.Location.X - labelStart.X, labelStart.Y);
            }
            else
            {
                label_start.Location = new Point(labelStart.X + (int)((customTrackBar_Start.Value - customTrackBar_Start.Minimum) * ScaleWidthStart) - customTrackBar_Start.Location.X - labelStart.X, labelStart.Y);
            }
        }

        private void SetTrackbarEnd()
        {
            if (customTrackBar_End.Value < customTrackBar_End.Maximum / 2)
            {
                label_end.Location = new Point(labelEnd.X + (int)((customTrackBar_End.Value - customTrackBar_End.Minimum) * ScaleWidthEnd) - customTrackBar_End.Location.X - labelEnd.X + 9, labelEnd.Y);
            }
            else if (customTrackBar_End.Value > customTrackBar_End.Maximum / 2)
            {
                label_end.Location = new Point(labelEnd.X + (int)((customTrackBar_End.Value - customTrackBar_End.Minimum) * ScaleWidthEnd) - customTrackBar_End.Location.X - labelEnd.X - 9, labelEnd.Y);
            }
            else if (customTrackBar_End.Value == customTrackBar_End.Maximum / 2)
            {
                label_end.Location = new Point(labelEnd.X + (int)((customTrackBar_End.Value - customTrackBar_End.Minimum) * ScaleWidthEnd) - customTrackBar_End.Location.X - labelEnd.X, labelEnd.Y);
            }
            else
            {
                label_end.Location = new Point(labelEnd.X + (int)((customTrackBar_End.Value - customTrackBar_End.Minimum) * ScaleWidthEnd) - customTrackBar_End.Location.X - labelEnd.X, labelEnd.Y);
            }
        }

        private void PanSlider1_PanChanged(object sender, EventArgs e)
        {
            if (reader.WaveFormat.Channels != 1)
            {
                return;
            }
            panSmplProvider.Pan = panSlider1.Pan;
        }

        public void ATRACRadioButtonChanger(bool flag)
        {
            switch (flag)
            {
                case true:
                    radioButton_at3.Enabled = true;
                    radioButton_at9.Enabled = true;
                    checkBox_LoopEnable.Enabled = true;
                    break;
                case false:
                    radioButton_at3.Enabled = false;
                    radioButton_at9.Enabled = false;
                    checkBox_LoopEnable.Enabled = false;
                    break;
            }

        }

        private void customTrackBar1_Click(object sender, EventArgs e)
        {

        }

        private void customTrackBar_Trk_Click(object sender, EventArgs e)
        {

        }

        private void customTrackBar_Start_Click(object sender, EventArgs e)
        {

        }
    }
}
