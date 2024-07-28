using NAudio.Wave;
using ATRACTool_Reloaded.Localizable;
using System.Text;
using static ATRACTool_Reloaded.Common;
using NAudio.Utils;
using Microsoft.VisualBasic;
using NAudio.Wave.SampleProviders;

namespace ATRACTool_Reloaded
{
    public partial class FormLPC : Form
    {
        private readonly WaveIn wi = new();
        private readonly WaveOut wo = new();
        WaveFileReader reader;
        BufferedWaveProvider BufwaveProvider;
        VolumeSampleProvider volumeSmplProvider;
        PanningSampleProvider panSmplProvider;
        long Sample, Start = 0, End = 0;
        int bytePerSec, position, length, btnpos;
        TimeSpan time;
        bool mouseDown = false, stopflag = false, IsPausedMoveTrackbar, SmoothSamples = false, IsPlaybackATRAC = false;
        float ScaleWidthTrk = 0f, ScaleWidthStart = 0f, ScaleWidthEnd = 0f;
        Point labelTrk = new(15, 90), labelStart = new(15, 130), labelEnd = new(15, 10);

        public FormLPC(bool IsEnabledBtn)
        {
            InitializeComponent();

            //wo.DesiredLatency = 280;

            if (!IsEnabledBtn)
            {
                FormBorderStyle = FormBorderStyle.None;
                button_OK.Enabled = false;
                button_OK.Visible = false;
                button_Cancel.Enabled = false;
                button_Cancel.Visible = false;
            }
            trackBar_trk.Scroll += TrackBar_trk_Scroll;
            trackBar_Start.Scroll += TrackBar_Start_Scroll;
            trackBar_End.Scroll += TrackBar_End_Scroll;
            trackBar_trk.MouseDown += TrackBar_trk_MouseDown;
            trackBar_trk.MouseUp += TrackBar_trk_MouseUp;
            label_trk.Text = "0";
            label_start.Text = "0";
            label_end.Text = "0";
            label_Samples.Text = Localization.SampleCaption + ":";
            label_Psamples.Text = "0";
            label_Length.Text = Localization.LengthCaption + ":";
            label_Plength.Text = "00:00:00";
            label_LoopStartSamples.Text = "";
            label_LoopEndSamples.Text = "";
            timer_Reload.Interval = 1;
        }

        private void TrackBar_End_Scroll(object? sender, EventArgs e)
        {
            label_end.Text = trackBar_End.Value.ToString();
            SetTrackbarEnd();
            //label_end.Location = new Point(labelEnd.X + (int)((trackBar_End.Value - trackBar_End.Minimum) * ScaleWidthEnd) - trackBar_End.Location.X - labelEnd.X, labelEnd.Y);
            numericUpDown_LoopEnd.Value = trackBar_End.Value;
        }

        private void TrackBar_Start_Scroll(object? sender, EventArgs e)
        {
            label_start.Text = trackBar_Start.Value.ToString();
            SetTrackbarStart();
            //label_start.Location = new Point(labelStart.X + (int)((trackBar_Start.Value - trackBar_Start.Minimum) * ScaleWidthStart) - trackBar_Start.Location.X - labelStart.X, labelStart.Y);
            numericUpDown_LoopStart.Value = trackBar_Start.Value;
        }

        private void TrackBar_trk_Scroll(object? sender, EventArgs e)
        {
            if (wo.PlaybackState == PlaybackState.Paused) IsPausedMoveTrackbar = true;
            label_trk.Text = trackBar_trk.Value.ToString();
            SetTrackbarTrack();
            //label_trk.Location = new Point(labelTrk.X + (int)((trackBar_trk.Value - trackBar_trk.Minimum) * ScaleWidthTrk) - trackBar_trk.Location.X - labelTrk.X, labelTrk.Y);

            reader.CurrentTime = TimeSpan.FromMilliseconds(trackBar_trk.Value);
            Sample = reader.Position;
        }

        private void TrackBar_trk_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!mouseDown) return;
            mouseDown = false;
        }

        private void TrackBar_trk_MouseDown(object? sender, MouseEventArgs e)
        {
            if (reader == null) return;
            mouseDown = true;
        }

        private void FormLPC_Load(object sender, EventArgs e)
        {
            Config.Load(xmlpath);

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

            if (IsPlaybackATRAC && Generic.IsATRAC)
            {
                checkBox_LoopEnable.Enabled = false;
                radioButton_at3.Enabled = false;
                radioButton_at9.Enabled = false;
                if (Common.Generic.pATRACOpenFilePaths.Length == 1)
                {
                    reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                    FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = fi.Name + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                    button_Prev.Enabled = false;
                    button_Next.Enabled = false;
                }
                else
                {
                    reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                    FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = fi.Name + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
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
                    label_File.Text = fi.Name + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
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
                        label_File.Text = fi.Name + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                        button_Prev.Enabled = false;
                        button_Next.Enabled = true;
                        btnpos = 1;
                    }
                }
            }

            wo.Init(reader);
            BufwaveProvider = new BufferedWaveProvider(reader.WaveFormat);
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
            trackBar_trk.Minimum = 0;
            trackBar_trk.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            trackBar_Start.Minimum = 0;
            trackBar_Start.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            trackBar_End.Minimum = 0;
            trackBar_End.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            ScaleWidthTrk = (float)trackBar_trk.Size.Width / ((float)trackBar_trk.Maximum - (float)trackBar_trk.Minimum);
            ScaleWidthStart = (float)trackBar_Start.Size.Width / ((float)trackBar_Start.Maximum - (float)trackBar_Start.Minimum);
            ScaleWidthEnd = (float)trackBar_End.Size.Width / ((float)trackBar_End.Maximum - (float)trackBar_End.Minimum);
            trackBar_End.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            trackBar_trk.TickFrequency = 1;
            trackBar_Start.TickFrequency = 1;
            trackBar_End.TickFrequency = 1;
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
        }

        private void Button_Play_Click(object sender, EventArgs e)
        {
            switch (wo.PlaybackState)
            {
                case PlaybackState.Stopped:
                    bytePerSec = reader.WaveFormat.BitsPerSample / 8 * reader.WaveFormat.SampleRate * reader.WaveFormat.Channels;
                    length = (int)reader.Length / bytePerSec;

                    timer_Reload.Enabled = true;
                    wo.Play();
                    button_Play.Text = Localization.PauseCaption;
                    Task.Run(Playback);
                    stopflag = false;
                    button_Stop.Enabled = true;
                    break;
                case PlaybackState.Paused:
                    if (IsPausedMoveTrackbar)
                    {
                        wo.Stop();
                        reader.CurrentTime = TimeSpan.FromMilliseconds(trackBar_trk.Value);
                        wo.Play();
                        Task.Run(Playback);
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
                wo.Stop();
                button_Play.Text = Localization.PlayCaption;
                reader.Position = 0;
                button_Stop.Enabled = false;
                Resettrackbarlabels();
            }
        }

        private void Timer_Reload_Tick(object sender, EventArgs e)
        {
            if (!mouseDown) trackBar_trk.Value = (int)reader.CurrentTime.TotalMilliseconds;
            if (checkBox_LoopEnable.Checked == true && reader.CurrentTime >= TimeSpan.FromMilliseconds(trackBar_End.Value))
            {
                reader.CurrentTime = TimeSpan.FromMilliseconds(trackBar_Start.Value);
                if (SmoothSamples)
                {
                    Sample = reader.Position / reader.BlockAlign;
                }
                else
                {
                    Sample = reader.Position / reader.BlockAlign;
                    //Sample = reader.CurrentTime.Ticks * reader.WaveFormat.SampleRate;
                }
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
            else if (reader.Position == 0 || trackBar_trk.Value == 0)
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

            label_trk.Text = trackBar_trk.Value.ToString();
            label_start.Text = trackBar_Start.Value.ToString();
            label_end.Text = trackBar_End.Value.ToString();
            label_Length.Text = Localization.LengthCaption + ":";
            label_Plength.Text = time.ToString(@"hh\:mm\:ss");
            StringBuilder str = new(Sample.ToString());
            label_Samples.Text = Localization.SampleCaption + ":";
            label_Psamples.Text = str.ToString();
        }

        private void Playback()
        {
            object lockobj = new();
            lock (lockobj)
            {
                ThreadStart tds = new(StartPlaybackThread);
                Thread thread = new(tds)
                {
                    Name = "waveOut",
                    IsBackground = true
                };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
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
                position = (int)reader.Position / reader.WaveFormat.AverageBytesPerSecond;
                time = new(0, 0, position);
                if (SmoothSamples)
                {
                    Sample = reader.Position / reader.BlockAlign + wo.GetPosition() / reader.BlockAlign;
                }
                else
                {
                    Sample = reader.Position / reader.BlockAlign;
                }
            }
        }

        private void FormLPC_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Button_SetStart_Click(object sender, EventArgs e)
        {
            long pos;
            TimeSpan oldc = reader.CurrentTime;
            reader.CurrentTime = TimeSpan.FromMilliseconds(trackBar_Start.Value);
            pos = reader.Position / reader.WaveFormat.BlockAlign;
            reader.CurrentTime = oldc;
            label_LoopStartSamples.Text = "LoopStart: " + pos.ToString() + " " + Localization.SampleCaption;
            Start = pos;
        }

        private void FormLPC_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (timer_Reload.Enabled == true) timer_Reload.Enabled = false;
            reader.Position = 0;
            wi.Dispose();
            wo.Stop();
            wo.Dispose();
            reader.Close();
            reader = null!;
        }

        private void Button_Prev_Click(object sender, EventArgs e)
        {
            btnpos--;

            string[] Paths;
            if (IsPlaybackATRAC && Generic.IsATRAC)
            {
                Paths = Generic.pATRACOpenFilePaths;
            }
            else
            {
                Paths = Generic.OpenFilePaths;
            }

            FileInfo fi = new(Paths[btnpos - 1]);

            wo.Stop();
            button_Play.Text = Localization.PlayCaption;
            reader.Position = 0;
            reader.Close();
            button_Stop.Enabled = false;

            if (btnpos == 1)
            {
                reader = new(Paths[btnpos - 1]);
                wo.Init(reader);
                ResetAFR();
                label_File.Text = fi.Name + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                button_Prev.Enabled = false;
                button_Next.Enabled = true;
            }
            else
            {
                reader = new(Paths[btnpos - 1]);
                wo.Init(reader);
                ResetAFR();
                label_File.Text = fi.Name + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                button_Prev.Enabled = true;
                button_Next.Enabled = true;
            }
        }

        private void Button_Next_Click(object sender, EventArgs e)
        {
            btnpos++;

            string[] Paths;
            if (IsPlaybackATRAC && Generic.IsATRAC)
            {
                Paths = Generic.pATRACOpenFilePaths;
            }
            else
            {
                Paths = Generic.OpenFilePaths;
            }

            FileInfo fi = new(Paths[btnpos - 1]);

            wo.Stop();
            button_Play.Text = Localization.PlayCaption;
            reader.Position = 0;
            reader.Close();
            button_Stop.Enabled = false;
            Resettrackbarlabels();

            if (btnpos == Paths.Length)
            {
                reader = new(Paths[btnpos - 1]);
                wo.Init(reader);
                ResetAFR();
                label_File.Text = fi.Name + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                button_Next.Enabled = false;
                button_Prev.Enabled = true;
            }
            else
            {
                reader = new(Paths[btnpos - 1]);
                wo.Init(reader);
                ResetAFR();
                label_File.Text = fi.Name + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                button_Next.Enabled = true;
                button_Prev.Enabled = true;
            }
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
            trackBar_Start.Value = trackBar_trk.Value;
            numericUpDown_LoopStart.Value = trackBar_Start.Value;
        }

        private void Button_LE_Current_Click(object sender, EventArgs e)
        {
            trackBar_End.Value = trackBar_trk.Value;
            numericUpDown_LoopEnd.Value = trackBar_End.Value;
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

                trackBar_Start.Enabled = true;
                trackBar_End.Enabled = true;
                numericUpDown_LoopStart.Enabled = true;
                numericUpDown_LoopEnd.Enabled = true;
                button_LS_Current.Enabled = true;
                button_LE_Current.Enabled = true;
                button_SetStart.Enabled = true;
                button_SetEnd.Enabled = true;
            }
            else
            {
                trackBar_Start.Enabled = false;
                trackBar_End.Enabled = false;
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

                using FormSettings form = new();
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
            trackBar_Start.Value = (int)numericUpDown_LoopStart.Value;
        }

        private void NumericUpDown_LoopEnd_ValueChanged(object sender, EventArgs e)
        {
            trackBar_End.Value = (int)numericUpDown_LoopEnd.Value;
        }

        private void Button_SetEnd_Click(object sender, EventArgs e)
        {
            long pos;
            TimeSpan oldc = reader.CurrentTime;
            reader.CurrentTime = TimeSpan.FromMilliseconds(trackBar_End.Value);
            pos = reader.Position / reader.WaveFormat.BlockAlign;
            reader.CurrentTime = oldc;
            label_LoopEndSamples.Text = "LoopEnd: " + pos.ToString() + " " + Localization.SampleCaption;
            End = pos;
        }

        private void ResetAFR()
        {
            trackBar_trk.Minimum = 0;
            trackBar_trk.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            trackBar_Start.Minimum = 0;
            trackBar_Start.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            trackBar_End.Minimum = 0;
            trackBar_End.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            trackBar_trk.TickFrequency = 1;
            trackBar_Start.TickFrequency = 1;
            trackBar_End.TickFrequency = 1;
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
            trackBar_Start.Enabled = false;
            trackBar_End.Enabled = false;
            numericUpDown_LoopStart.Enabled = false;
            numericUpDown_LoopEnd.Enabled = false;
            button_LS_Current.Enabled = false;
            button_LE_Current.Enabled = false;
            button_SetStart.Enabled = false;
            button_SetEnd.Enabled = false;
        }

        private void trackBar_trk_CursorChanged(object sender, EventArgs e)
        {

        }

        private void Resettrackbarlabels()
        {
            ScaleWidthTrk = (float)trackBar_trk.Size.Width / ((float)trackBar_trk.Maximum - (float)trackBar_trk.Minimum);
            ScaleWidthStart = (float)trackBar_Start.Size.Width / ((float)trackBar_Start.Maximum - (float)trackBar_Start.Minimum);
            ScaleWidthEnd = (float)trackBar_End.Size.Width / ((float)trackBar_End.Maximum - (float)trackBar_End.Minimum);
            labelTrk = new(15, 90);
            labelStart = new(15, 130);
            labelEnd = new(15, 10);
        }

        private void SetTrackbarTrack()
        {
            if (trackBar_trk.Value < trackBar_trk.Maximum / 2)
            {
                label_trk.Location = new Point(labelTrk.X + (int)((trackBar_trk.Value - trackBar_trk.Minimum) * ScaleWidthTrk) - trackBar_trk.Location.X - labelTrk.X + 9, labelTrk.Y);
            }
            else if (trackBar_trk.Value > trackBar_trk.Maximum / 2)
            {
                label_trk.Location = new Point(labelTrk.X + (int)((trackBar_trk.Value - trackBar_trk.Minimum) * ScaleWidthTrk) - trackBar_trk.Location.X - labelTrk.X - 9, labelTrk.Y);
            }
            else if (trackBar_trk.Value == trackBar_trk.Maximum / 2)
            {
                label_trk.Location = new Point(labelTrk.X + (int)((trackBar_trk.Value - trackBar_trk.Minimum) * ScaleWidthTrk) - trackBar_trk.Location.X - labelTrk.X, labelTrk.Y);
            }
            else
            {
                label_trk.Location = new Point(labelTrk.X + (int)((trackBar_trk.Value - trackBar_trk.Minimum) * ScaleWidthTrk) - trackBar_trk.Location.X - labelTrk.X, labelTrk.Y);
            }
        }

        private void SetTrackbarStart()
        {
            if (trackBar_Start.Value < trackBar_Start.Maximum / 2)
            {
                label_start.Location = new Point(labelStart.X + (int)((trackBar_Start.Value - trackBar_Start.Minimum) * ScaleWidthStart) - trackBar_Start.Location.X - labelStart.X + 9, labelStart.Y);
            }
            else if (trackBar_Start.Value > trackBar_Start.Maximum / 2)
            {
                label_start.Location = new Point(labelStart.X + (int)((trackBar_Start.Value - trackBar_Start.Minimum) * ScaleWidthStart) - trackBar_Start.Location.X - labelStart.X - 9, labelStart.Y);
            }
            else if (trackBar_Start.Value == trackBar_Start.Maximum / 2)
            {
                label_start.Location = new Point(labelStart.X + (int)((trackBar_Start.Value - trackBar_Start.Minimum) * ScaleWidthStart) - trackBar_Start.Location.X - labelStart.X, labelStart.Y);
            }
            else
            {
                label_start.Location = new Point(labelStart.X + (int)((trackBar_Start.Value - trackBar_Start.Minimum) * ScaleWidthStart) - trackBar_Start.Location.X - labelStart.X, labelStart.Y);
            }
        }

        private void SetTrackbarEnd()
        {
            if (trackBar_End.Value < trackBar_End.Maximum / 2)
            {
                label_end.Location = new Point(labelEnd.X + (int)((trackBar_End.Value - trackBar_End.Minimum) * ScaleWidthEnd) - trackBar_End.Location.X - labelEnd.X + 9, labelEnd.Y);
            }
            else if (trackBar_End.Value > trackBar_End.Maximum / 2)
            {
                label_end.Location = new Point(labelEnd.X + (int)((trackBar_End.Value - trackBar_End.Minimum) * ScaleWidthEnd) - trackBar_End.Location.X - labelEnd.X - 9, labelEnd.Y);
            }
            else if (trackBar_End.Value == trackBar_End.Maximum / 2)
            {
                label_end.Location = new Point(labelEnd.X + (int)((trackBar_End.Value - trackBar_End.Minimum) * ScaleWidthEnd) - trackBar_End.Location.X - labelEnd.X, labelEnd.Y);
            }
            else
            {
                label_end.Location = new Point(labelEnd.X + (int)((trackBar_End.Value - trackBar_End.Minimum) * ScaleWidthEnd) - trackBar_End.Location.X - labelEnd.X, labelEnd.Y);
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
                    break;
                case false:
                    radioButton_at3.Enabled = false;
                    radioButton_at9.Enabled = false;
                    break;
            }

        }
    }
}
