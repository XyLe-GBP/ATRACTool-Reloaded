using NAudio.Wave;
using ATRACTool_Reloaded.Localizable;
using System.Text;
using static ATRACTool_Reloaded.Common;
using NAudio.Wave.SampleProviders;
using System.Diagnostics;

namespace ATRACTool_Reloaded
{
    public partial class FormLPC : Form
    {
        private readonly WaveIn wi = new();
        private readonly WaveOut wo = new();
        private ISampleProvider isp = null!;
        WaveFileReader reader;
        //private AudioFileReader reader = null!;
        long Sample, Start = 0, End = 0;
        float[] SampleF_L, SampleF_R;
        int bytePerSec, position, length, btnpos;
        TimeSpan time;
        bool mouseDown = false;

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
            trackBar_trk.Scroll += TrackBar_trk_Scroll;
            trackBar_Start.Scroll += TrackBar_Start_Scroll;
            trackBar_End.Scroll += TrackBar_End_Scroll;
            trackBar_trk.MouseDown += TrackBar_trk_MouseDown;
            trackBar_trk.MouseUp += TrackBar_trk_MouseUp;
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
            numericUpDown_LoopEnd.Value = trackBar_End.Value;
        }

        private void TrackBar_Start_Scroll(object? sender, EventArgs e)
        {
            numericUpDown_LoopStart.Value = trackBar_Start.Value;
        }

        private void TrackBar_trk_Scroll(object? sender, EventArgs e)
        {
            reader.CurrentTime = TimeSpan.FromMilliseconds(trackBar_trk.Value);
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

            wo.Init(reader);
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
                    Task.Run(() => Playback());
                    button_Stop.Enabled = true;
                    break;
                case PlaybackState.Paused:
                    wo.Play();
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
                wo.Stop();
                button_Play.Text = Localization.PlayCaption;
                reader.Position = 0;
                button_Stop.Enabled = false;
            }
        }

        private void Timer_Reload_Tick(object sender, EventArgs e)
        {
            if (!mouseDown) trackBar_trk.Value = (int)reader.CurrentTime.TotalMilliseconds;
            if (checkBox_LoopEnable.Checked == true && reader.CurrentTime >= TimeSpan.FromMilliseconds(trackBar_End.Value))
            {
                reader.CurrentTime = TimeSpan.FromMilliseconds(trackBar_Start.Value);
            }
            if (reader.CurrentTime == reader.TotalTime)
            {
                wo.Stop();
                button_Play.Text = Localization.PlayCaption;
                reader.Position = 0;
                button_Stop.Enabled = false;
            }
            label_Length.Text = Localization.LengthCaption + ":";
            label_Plength.Text = time.ToString(@"hh\:mm\:ss");
            StringBuilder str = new(Sample.ToString());
            label_Samples.Text = Localization.SampleCaption + ":";
            label_Psamples.Text = str.ToString();
        }

        private void Playback()
        {
            //long rp = 0;
            while (wo.PlaybackState != PlaybackState.Stopped)
            {
                /*long FasterPos = wo.GetPosition() / wo.OutputWaveFormat.BlockAlign;
                long ReaderPos = reader.Position / reader.WaveFormat.BlockAlign;
                int ct = (int)reader.CurrentTime.TotalMilliseconds;
                if (wo.PlaybackState == PlaybackState.Paused)
                {
                    if (mouseDown)
                    {
                        Sample = ReaderPos;
                        rp = ReaderPos;
                    }
                }
                if (wo.PlaybackState == PlaybackState.Playing)
                {
                    if (mouseDown)
                    {
                        if (ct == 0)
                        {
                            wo.Stop();
                        }
                        Sample = ReaderPos;
                        rp = ReaderPos;
                    }
                    else
                    {
                        if (rp < FasterPos)
                        {
                            Sample = FasterPos - rp;
                        }
                        else if (rp > FasterPos)
                        {
                            Sample = FasterPos + rp;
                        }
                        else
                        {
                            Sample = FasterPos + rp;
                        }
                    }
                }*/
                position = (int)reader.Position / reader.WaveFormat.AverageBytesPerSecond;
                time = new(0, 0, position);
                if (wo.PlaybackState == PlaybackState.Paused)
                {
                    Sample = reader.Position / reader.WaveFormat.BlockAlign;
                }
                else
                {
                    //SampleF_L = new float[reader.Length / reader.WaveFormat.BlockAlign];
                    //float[] smpl = reader.ReadNextSampleFrame();
                    Sample = reader.Position / reader.WaveFormat.BlockAlign;
                }

            }
        }

        private void SampleCalc()
        {
            SampleF_L = new float[reader.Length / reader.WaveFormat.BlockAlign];
            SampleF_R = new float[reader.Length / reader.WaveFormat.BlockAlign];

            for (int i = 0; i < SampleF_L.Length; i++)
            {
                float[] smpl = reader.ReadNextSampleFrame();
                SampleF_L[i] = smpl[0];
                SampleF_R[i] = smpl[1];
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
            FileInfo fi = new(Common.Generic.OpenFilePaths[btnpos - 1]);

            wo.Stop();
            button_Play.Text = Localization.PlayCaption;
            reader.Position = 0;
            reader.Close();
            button_Stop.Enabled = false;

            if (btnpos == 1)
            {
                reader = new(Common.Generic.OpenFilePaths[btnpos - 1]);
                wo.Init(reader);
                ResetAFR();
                label_File.Text = fi.Name + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                button_Prev.Enabled = false;
                button_Next.Enabled = true;
            }
            else
            {
                reader = new(Common.Generic.OpenFilePaths[btnpos - 1]);
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
            FileInfo fi = new(Common.Generic.OpenFilePaths[btnpos - 1]);

            wo.Stop();
            button_Play.Text = Localization.PlayCaption;
            reader.Position = 0;
            reader.Close();
            button_Stop.Enabled = false;

            if (btnpos == Common.Generic.OpenFilePaths.Length)
            {
                reader = new(Common.Generic.OpenFilePaths[btnpos - 1]);
                wo.Init(reader);
                ResetAFR();
                label_File.Text = fi.Name + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                button_Next.Enabled = false;
                button_Prev.Enabled = true;
            }
            else
            {
                reader = new(Common.Generic.OpenFilePaths[btnpos - 1]);
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

        private void radioButton_at3_CheckedChanged(object sender, EventArgs e)
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

        private void radioButton_at9_CheckedChanged(object sender, EventArgs e)
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
    }
}
