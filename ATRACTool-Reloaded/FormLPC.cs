using NAudio.Wave;

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
            reader = new(Common.Generic.OpenFilePaths[0]);
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
        }

        private void button_Play_Click(object sender, EventArgs e)
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

        private void button_Stop_Click(object sender, EventArgs e)
        {
            if (wo.PlaybackState != PlaybackState.Stopped)
            {
                wo.Stop();
                button_Play.Text = Localization.PlayCaption;
                reader.Position = 0;
            }
        }

        private void timer_Reload_Tick(object sender, EventArgs e)
        {
            if (!mouseDown) trackBar_trk.Value = (int)reader.CurrentTime.TotalMilliseconds;
            if (checkBox_LoopEnable.Checked == true && reader.CurrentTime >= TimeSpan.FromMilliseconds(trackBar_End.Value))
            {
                reader.CurrentTime = TimeSpan.FromMilliseconds(trackBar_Start.Value);
            }
            label_Length.Text = Localization.LengthCaption + ": " + time.ToString(@"hh\:mm\:ss");
            label_Samples.Text = Localization.SampleCaption + ": " + Sample.ToString();
        }

        private void Playback()
        {
            while (wo.PlaybackState != PlaybackState.Stopped)
            {
                position = (int)reader.Position / reader.WaveFormat.AverageBytesPerSecond;
                time = new(0, 0, position);
                Sample = reader.Position / reader.WaveFormat.BlockAlign;
            }
        }

        private void FormLPC_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void button_SetStart_Click(object sender, EventArgs e)
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

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button_OK_Click(object sender, EventArgs e)
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
                }
                else
                {
                    ini.WriteString("ATRAC9_SETTINGS", "LoopSound", "0");
                    ini.WriteString("ATRAC9_SETTINGS", "LoopPoint", "1");
                    ini.WriteString("ATRAC9_SETTINGS", "LoopStart_Samples", Start.ToString());
                    ini.WriteString("ATRAC9_SETTINGS", "LoopEnd_Samples", End.ToString());
                    ini.WriteString("ATRAC9_SETTINGS", "Param", "");
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

        private void numericUpDown_LoopStart_ValueChanged(object sender, EventArgs e)
        {
            trackBar_Start.Value = (int)numericUpDown_LoopStart.Value;
        }

        private void numericUpDown_LoopEnd_ValueChanged(object sender, EventArgs e)
        {
            trackBar_End.Value = (int)numericUpDown_LoopEnd.Value;
        }

        private void button_SetEnd_Click(object sender, EventArgs e)
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
