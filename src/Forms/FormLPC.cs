using NAudio.Wave;
using ATRACTool_Reloaded.Localizable;
using System.Text;

namespace ATRACTool_Reloaded
{
    public partial class FormLPC : Form
    {
        private readonly WaveOut wo = new();
        private AudioFileReader reader = null!;
        int bytePerSec;
        long Sample;
        long Start = 0, End = 0;
        int position;
        int length;
        TimeSpan time;
        bool mouseDown = false;

        public FormLPC()
        {
            InitializeComponent();

            trackBar_trk.Scroll += TrackBar_trk_Scroll;
            trackBar_Start.Scroll += TrackBar_Start_Scroll;
            trackBar_End.Scroll += TrackBar_End_Scroll;
            trackBar_trk.MouseDown += TrackBar_trk_MouseDown;
            trackBar_trk.MouseUp += TrackBar_trk_MouseUp;
            label_Samples.Text = Localization.SampleCaption + ": 0";
            label_Length.Text = Localization.LengthCaption + ": 00:00:00";
            label_LoopStartSamples.Text = "LoopStart: 0";
            label_LoopEndSamples.Text = "LoopEnd: 0";
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
            }
            else
            {
                if (Common.Generic.lpcreate != false)
                {
                    reader = new(Common.Generic.OpenFilePaths[Common.Generic.files]);

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
            }
        }

        private void Timer_Reload_Tick(object sender, EventArgs e)
        {
            if (!mouseDown) trackBar_trk.Value = (int)reader.CurrentTime.TotalMilliseconds;
            if (checkBox_LoopEnable.Checked == true && reader.CurrentTime >= TimeSpan.FromMilliseconds(trackBar_End.Value))
            {
                reader.CurrentTime = TimeSpan.FromMilliseconds(trackBar_Start.Value);
            }
            label_Length.Text = Localization.LengthCaption + ": " + time.ToString(@"hh\:mm\:ss");
            StringBuilder str = new(Localization.SampleCaption + ": " + Sample.ToString());
            label_Samples.Text = str.ToString();
        }

        private void Playback()
        {
            while (wo.PlaybackState != PlaybackState.Stopped)
            {
                position = (int)reader.Position / reader.WaveFormat.AverageBytesPerSecond;
                time = new(0, 0, position);
                Sample = reader.Position / reader.WaveFormat.BlockAlign;
            }
            if (wo.PlaybackState == PlaybackState.Stopped)
            {
                Invoke(new Action(() => button_Play.Text = Localization.PlayCaption));
                reader.Position = 0;
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
            reader.Position = 0;
            wo.Dispose();
            reader.Close();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
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

                Common.IniFile ini = new(@".\settings.ini");
                if (radioButton_at3.Checked == true)
                {
                    ini.WriteString("ATRAC3_SETTINGS", "LoopSound", "0");
                    ini.WriteString("ATRAC3_SETTINGS", "LoopPoint", "1");
                    ini.WriteString("ATRAC3_SETTINGS", "LoopStart_Samples", Start.ToString());
                    ini.WriteString("ATRAC3_SETTINGS", "LoopEnd_Samples", End.ToString());
                    ini.WriteString("ATRAC3_SETTINGS", "Param", "");
                    ini.WriteString("GENERIC", "LPCreateIndex", "0");
                }
                else
                {
                    ini.WriteString("ATRAC9_SETTINGS", "LoopSound", "0");
                    ini.WriteString("ATRAC9_SETTINGS", "LoopPoint", "1");
                    ini.WriteString("ATRAC9_SETTINGS", "LoopStart_Samples", Start.ToString());
                    ini.WriteString("ATRAC9_SETTINGS", "LoopEnd_Samples", End.ToString());
                    ini.WriteString("ATRAC9_SETTINGS", "Param", "");
                    ini.WriteString("GENERIC", "LPCreateIndex", "0");
                }
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
    }
}
