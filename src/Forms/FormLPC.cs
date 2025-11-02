using ATRACTool_Reloaded.Localizable;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormLPC : Form
    {
        private readonly WaveInEvent wi = new();
        private readonly WaveOutEvent wo = new();
        private MMDevice? mmDevice;
        private WasapiOut wasapiOut = null!;
        private AsioOut asioOut = null!;
        private readonly List<IDisposable> _disposables = new();
        WaveFileReader reader = null!;
        BufferedWaveProvider BufwaveProvider = null!;
        VolumeSampleProvider volumeSmplProvider = null!;
        PanningSampleProvider panSmplProvider = null!;
        long Sample, Start = 0, End = 0;
        int bytePerSec, position, length, smplrate, WASAPILatency = 0, WASAPIexLatency = 0, ASIOLatency = 0, UseThreads = 3;
        long totalsamples;
        uint btnpos;
        TimeSpan time;
        bool mouseDown = false, stopflag = false, IsPausedMoveTrackbar, SmoothSamples = false, IsPlaybackATRAC = false, IsEncodeSourceATRAC = false, IsMultiChannel = false, IsWASAPI = false, IsWASAPIex = false, IsASIO = false;
        float ScaleWidthTrk = 0f, ScaleWidthStart = 0f, ScaleWidthEnd = 0f;
        Point MainDefaultPoint = new(15, 88), StartDefaultPoint = new(15, 160), EndDefaultPoint = new(15, 32);

        Point labelTrk, labelStart, labelEnd;

        private volatile bool SLTAlive;
        ThreadStart StartPlaybackThread_s = null!;
        Thread Playback_s = null!;

        int[] bufferloop = new int[2];

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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        public long TotalSamples
        {
            get
            {
                return totalsamples;
            }
        }

        public uint ButtonPosition
        {
            get
            {
                return btnpos;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int[] BufferLoopPosition
        {
            get
            {
                return bufferloop;
            }
            set
            {
                bufferloop = value;
            }
        }


        public FormLPC(bool IsEnabledBtn)
        {
            InitializeComponent();

            labelTrk = MainDefaultPoint;
            labelStart = StartDefaultPoint;
            labelEnd = EndDefaultPoint;

            if (!IsEnabledBtn)
            {
                checkBox_LoopEnable.Checked = false;
                checkBox_LoopEnable.Enabled = true;
                FormBorderStyle = FormBorderStyle.None;
                button_OK.Enabled = false;
                button_OK.Visible = false;
                button_Cancel.Enabled = false;
                button_Cancel.Visible = false;
                radioButton_at3.Enabled = false;
                radioButton_at3.Visible = false;
                radioButton_at9.Enabled = false;
                radioButton_at9.Visible = false;
            }
            else
            {
                checkBox_LoopEnable.Checked = true;
                checkBox_LoopEnable.Enabled = false;
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
            //label_start.Visible = false;
            //label_end.Visible = false;
        }

        private void customTrackBar_End_Scroll(object? sender, EventArgs e)
        {
            //label_end.Text = customTrackBar_End.Value.ToString();
            //SetTrackbarEnd();
            //label_end.Location = new Point(labelEnd.X + (int)((customTrackBar_End.Value - customTrackBar_End.Minimum) * ScaleWidthEnd) - customTrackBar_End.Location.X - labelEnd.X, labelEnd.Y);
            numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
        }

        private void customTrackBar_Start_Scroll(object? sender, EventArgs e)
        {
            //label_start.Text = customTrackBar_Start.Value.ToString();
            //SetTrackbarStart();
            //label_start.Location = new Point(labelStart.X + (int)((customTrackBar_Start.Value - customTrackBar_Start.Minimum) * ScaleWidthStart) - customTrackBar_Start.Location.X - labelStart.X, labelStart.Y);
            numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
        }

        private void customTrackBar_Trk_Scroll(object? sender, EventArgs e)
        {
            //if (wo.PlaybackState == PlaybackState.Paused) IsPausedMoveTrackbar = true;
            //label_trk.Text = customTrackBar_Trk.Value.ToString();
            //SetTrackbarTrack();
            //label_trk.Location = new Point(labelTrk.X + (int)((customTrackBar_Trk.Value - customTrackBar_Trk.Minimum) * ScaleWidthTrk) - customTrackBar_Trk.Location.X - labelTrk.X, labelTrk.Y);

            reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Trk.Value);
            //Sample = reader.Position;
            //Sample = reader.Position / reader.BlockAlign;
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

            switch (bool.Parse(Config.Entry["ATRACEncodeSource"].Value))
            {
                case true:
                    IsEncodeSourceATRAC = true;
                    break;
                case false:
                    IsEncodeSourceATRAC = false;
                    break;
            }

            switch (uint.Parse(Config.Entry["LPCPlaybackMethod"].Value))
            {
                case 0:
                    {
                        IsWASAPI = false;
                        IsWASAPIex = false;
                        IsASIO = false;
                        break;
                    }
                case 1:
                    {
                        IsWASAPI = true;
                        IsWASAPIex = false;
                        IsASIO = false;
                        break;
                    }
                case 2:
                    {
                        IsWASAPI = false;
                        IsWASAPIex = true;
                        IsASIO = false;
                        break;
                    }
                case 3:
                    {
                        IsWASAPI = false;
                        IsWASAPIex = false;
                        IsASIO = true;
                        break;
                    }
                default:
                    {
                        IsWASAPI = false;
                        IsWASAPIex = false;
                        IsASIO = false;
                        break;
                    }
            }

            if (SmoothSamples)
            {
                if (IsWASAPI || IsWASAPIex)
                {
                    WASAPILatency = 0;
                }
                else if (IsASIO)
                {
                    ASIOLatency = 0;
                }
                else
                {
                    wo.DesiredLatency = 200; // 250
                    wo.NumberOfBuffers = 16; // 8
                }
            }
            else
            {
                if (IsWASAPI)
                {
                    WASAPILatency = int.Parse(Config.Entry["WASAPILatencySharedValue"].Value);
                }
                else if (IsWASAPIex)
                {
                    WASAPIexLatency = int.Parse(Config.Entry["WASAPILatencyExclusivedValue"].Value);
                }
                else if (IsASIO)
                {
                    ASIOLatency = 100;
                }
                else
                {
                    wo.DesiredLatency = int.Parse(Config.Entry["DirectSoundLatencyValue"].Value); // 250
                    wo.NumberOfBuffers = int.Parse(Config.Entry["DirectSoundBuffersValue"].Value); // 8
                }
            }

            UseThreads = int.Parse(Config.Entry["PlaybackThreadCount"].Value);

            Generic.IsLPCStreamingReloaded = true;
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
            else if (IsEncodeSourceATRAC && Generic.IsATRAC) // ATRACをエンコード用ソースとして読み込み
            {
                if (Common.Generic.pATRACOpenFilePaths.Length == 1) // 単一ファイル
                {
                    reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                    FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                    button_Prev.Enabled = false;
                    button_Next.Enabled = false;
                }
                else
                {
                    if (Generic.lpcreatev2 && Common.Generic.lpcreate != false) //　LPC有効
                    {
                        reader = new(Common.Generic.pATRACOpenFilePaths[Common.Generic.files]);
                        FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[Common.Generic.files]);
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
                    else // 複数ファイル
                    {
                        reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                        FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                        label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                        button_Prev.Enabled = false;
                        button_Next.Enabled = true;
                        btnpos = 1;
                    }
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
                    if (Generic.lpcreatev2 && Common.Generic.lpcreate != false)
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

            if (!PlaybackInit())
            {
                return;
            }

            _ = FormMain.FormMainInstance.Meta;

            Generic.IsLPCStreamingReloaded = false;

            if (!IsMultiChannel)
            {
                BufwaveProvider = new BufferedWaveProvider(reader.WaveFormat);
                BufwaveProvider.BufferDuration = TimeSpan.FromMilliseconds(500); // バッファの長さを設定
                                                                                 //wo.Init(BufwaveProvider);
                volumeSmplProvider = new VolumeSampleProvider(BufwaveProvider.ToSampleProvider());
            }

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
            customTrackBar_Start.Minimum = 0;
            customTrackBar_Start.Maximum = (int)reader.TotalTime.TotalMilliseconds;
            customTrackBar_End.Minimum = 0;
            customTrackBar_End.Maximum = (int)reader.TotalTime.TotalMilliseconds;

            ScaleWidthTrk = (float)customTrackBar_Trk.Size.Width / ((float)customTrackBar_Trk.Maximum - (float)customTrackBar_Trk.Minimum);
            ScaleWidthStart = (float)customTrackBar_Start.Size.Width / ((float)customTrackBar_Start.Maximum - (float)customTrackBar_Start.Minimum);
            ScaleWidthEnd = (float)customTrackBar_End.Size.Width / ((float)customTrackBar_End.Maximum - (float)customTrackBar_End.Minimum);

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

            smplrate = reader.WaveFormat.SampleRate;
            totalsamples = reader.SampleCount;
            SetLoopPointsWithATRACBuffer(reader.WaveFormat.SampleRate, 0);

            Generic.LPCTotalSamples = reader.SampleCount;

        }

        private IWaveProvider BuildOutputChain(WaveFileReader reader)
        {
            // まずは「Extensible含む何でも」→ float(ISampleProvider)
            var floatSP = BuildFloatFromWaveFileReader(reader);

            // 音量（多ch対応）
            //_sample = new SampleChannel(floatSP, true);
            ISampleProvider chain = floatSP;

            // パンは mono/stereo のみ（5.1/7.1 では無効にするのが無難）
            if (reader.WaveFormat.Channels <= 2)
            {
                panSmplProvider = new PanningSampleProvider(chain);
                chain = panSmplProvider;
            }
            else
            {
                panSmplProvider = null!;
            }

            // 出力ドライバが要求する IWaveProvider へ
            return chain.ToWaveProvider();
        }

        // 「WaveFileReader から必ず 32-bit float(ISampleProvider) を得る」ヘルパ
        private ISampleProvider BuildFloatFromWaveFileReader(WaveFileReader reader)
        {
            var wf = reader.WaveFormat;

            // 1) すでに float
            if (wf.Encoding == WaveFormatEncoding.IeeeFloat)
                return reader.ToSampleProvider();

            // 2) Extensible でも 16/24/32bit PCM なら “シム” → そのビット深度→float
            if (wf.Encoding == WaveFormatEncoding.Extensible)
            {
                IWaveProvider shim = new ExtensiblePcmShim(reader);
                switch (wf.BitsPerSample)
                {
                    case 16:
                        return new Wave16ToFloatProvider(shim).ToSampleProvider();
                    case 24:
                        return new WaveToSampleProvider(shim); // これ自体が ISampleProvider(float)
                    case 32:
                        return new WaveToSampleProvider(shim); // 同上
                    default:
                        throw new NotSupportedException($"Extensible {wf.BitsPerSample}bit は未対応です。");
                }
            }

            // 3) 素の PCM（非 Extensible）
            if (wf.Encoding == WaveFormatEncoding.Pcm)
            {
                switch (wf.BitsPerSample)
                {
                    case 16: return new Wave16ToFloatProvider(reader).ToSampleProvider();
                    case 24: return new WaveToSampleProvider(reader);
                    case 32: return new WaveToSampleProvider(reader);
                    default: throw new NotSupportedException($"PCM {wf.BitsPerSample}bit は未対応です。");
                }
            }

            // 4) それ以外（ADPCM / μ-law 等）は WaveFileReader ではなく MediaFoundationReader を使う
            throw new NotSupportedException("このWAVは圧縮コーデックを含むため、WaveFileReaderではなく MediaFoundationReader を使用してください。");
        }

        private bool PlaybackInit()
        {
            
            try
            {
                switch (reader.WaveFormat.Channels)
                {
                    case 1: // Mono
                        IsMultiChannel = false;
                        if (IsWASAPI || IsWASAPIex)
                        {
                            mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                            wasapiOut = new WasapiOut(mmDevice,
                                        IsWASAPIex ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared,
                                        false, IsWASAPIex ? WASAPIexLatency : WASAPILatency);
                            wasapiOut.Init(new WaveChannel32(reader));
                        }
                        else if (IsASIO)
                        {
                            asioOut = new();
                            asioOut.Init(new WaveChannel32(reader));
                        }
                        else
                        {
                            wo.Init(new WaveChannel32(reader));
                        }
                        return true;
                    case 2: // Stereo
                        IsMultiChannel = false;
                        if (IsWASAPI || IsWASAPIex)
                        {
                            mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                            wasapiOut = new WasapiOut(mmDevice,
                                        IsWASAPIex ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared,
                                        false, IsWASAPIex ? WASAPIexLatency : WASAPILatency);
                            wasapiOut.Init(new WaveChannel32(reader));
                        }
                        else if (IsASIO)
                        {
                            asioOut = new();
                            asioOut.Init(new WaveChannel32(reader));
                        }
                        else
                        {
                            wo.Init(new WaveChannel32(reader));
                        }
                        return true;
                    case 6: // 5.1ch
                        {
                            IsMultiChannel = true;
                            _disposables.Add(reader);
                            var output = BuildOutputChain(reader);
                            if (IsWASAPI || IsWASAPIex)
                            {
                                mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                                wasapiOut = new WasapiOut(mmDevice,
                                            IsWASAPIex ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared,
                                            false, IsWASAPIex ? WASAPIexLatency : WASAPILatency);
                                wasapiOut.Init(output);  // ← WaveChannel32 を使わない
                            }
                            else if (IsASIO)
                            {
                                asioOut = new();
                                asioOut.Init(output);
                            }
                            else
                            {
                                throw new NotSupportedException("This audio contains multiple channel information and cannot be played using DirectSound.\r\nPlease use WASAPI or ASIO.");
                            }
                            return true;
                        }
                    case 8: // 7.1ch
                        {
                            IsMultiChannel = true;
                            _disposables.Add(reader);
                            var output = BuildOutputChain(reader);
                            if (IsWASAPI || IsWASAPIex)
                            {
                                mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                                wasapiOut = new WasapiOut(mmDevice,
                                            IsWASAPIex ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared,
                                            false, IsWASAPIex ? WASAPIexLatency : WASAPILatency);
                                wasapiOut.Init(output);  // ← WaveChannel32 を使わない
                            }
                            else if (IsASIO)
                            {
                                asioOut = new();
                                asioOut.Init(output);
                            }
                            else
                            {
                                throw new NotSupportedException("This audio contains multiple channel information and cannot be played using DirectSound.\r\nPlease use WASAPI or ASIO.");
                            }
                            return true;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(this, string.Format(Localization.LPCUnsupportedFormatErrorCaption, Ex), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Generic.LPCException = true;
                return false;
            }
        }

        private async void Button_Play_Click(object sender, EventArgs e)
        {
            if (IsWASAPI || IsWASAPIex)
            {
                switch (wasapiOut.PlaybackState)
                {
                    case PlaybackState.Stopped:
                        bytePerSec = reader.WaveFormat.BitsPerSample / 8 * reader.WaveFormat.SampleRate * reader.WaveFormat.Channels;
                        length = (int)reader.Length / bytePerSec;

                        timer_Reload.Enabled = true;
                        wasapiOut.Play();
                        button_Play.Text = Localization.PauseCaption;
                        await Task.Run(Playback);
                        stopflag = false;
                        button_Stop.Enabled = true;
                        break;
                    case PlaybackState.Paused:
                        if (IsPausedMoveTrackbar)
                        {
                            wasapiOut.Stop();
                            reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Trk.Value);
                            wasapiOut.Play();
                            await Task.Run(Playback);
                            IsPausedMoveTrackbar = false;
                        }
                        else
                        {
                            wasapiOut.Play();
                        }

                        button_Play.Text = Localization.PauseCaption;
                        break;
                    case PlaybackState.Playing:
                        wasapiOut.Pause();
                        button_Play.Text = Localization.PlayCaption;
                        break;
                }
            }
            else if (IsASIO)
            {
                switch (asioOut.PlaybackState)
                {
                    case PlaybackState.Stopped:
                        bytePerSec = reader.WaveFormat.BitsPerSample / 8 * reader.WaveFormat.SampleRate * reader.WaveFormat.Channels;
                        length = (int)reader.Length / bytePerSec;

                        timer_Reload.Enabled = true;
                        asioOut.Play();
                        button_Play.Text = Localization.PauseCaption;
                        await Task.Run(Playback);
                        stopflag = false;
                        button_Stop.Enabled = true;
                        break;
                    case PlaybackState.Paused:
                        if (IsPausedMoveTrackbar)
                        {
                            asioOut.Stop();
                            reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Trk.Value);
                            asioOut.Play();
                            await Task.Run(Playback);
                            IsPausedMoveTrackbar = false;
                        }
                        else
                        {
                            asioOut.Play();
                        }

                        button_Play.Text = Localization.PauseCaption;
                        break;
                    case PlaybackState.Playing:
                        asioOut.Pause();
                        button_Play.Text = Localization.PlayCaption;
                        break;
                }
            }
            else
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
        }

        private void Button_Stop_Click(object sender, EventArgs e)
        {
            if (IsWASAPI || IsWASAPIex)
            {
                if (wasapiOut.PlaybackState != PlaybackState.Stopped)
                {
                    stopflag = true;
                    timer_Reload.Stop();
                    wasapiOut.Stop();
                    button_Play.Text = Localization.PlayCaption;
                    reader.Position = 0;
                    button_Stop.Enabled = false;
                    Resettrackbarlabels();
                }
            }
            else if (IsASIO)
            {
                if (asioOut.PlaybackState != PlaybackState.Stopped)
                {
                    stopflag = true;
                    timer_Reload.Stop();
                    asioOut.Stop();
                    button_Play.Text = Localization.PlayCaption;
                    reader.Position = 0;
                    button_Stop.Enabled = false;
                    Resettrackbarlabels();
                }
            }
            else
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

        }

        private void Timer_Reload_Tick(object sender, EventArgs e)
        {
            if (!mouseDown) customTrackBar_Trk.Value = (int)reader.CurrentTime.TotalMilliseconds;
            if (checkBox_LoopEnable.Checked == true && reader.CurrentTime >= TimeSpan.FromMilliseconds(customTrackBar_End.Value))
            {
                reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Start.Value);
                Sample = reader.Position / reader.BlockAlign;
            }

            if (IsWASAPI || IsWASAPIex)
            {
                if (reader.CurrentTime == reader.TotalTime)
                {
                    stopflag = true;
                    Sample = reader.SampleCount;
                    wasapiOut.Stop();
                    button_Play.Text = Localization.PlayCaption;
                    reader.Position = 0;
                    button_Stop.Enabled = false;
                    Resettrackbarlabels();
                }
                else if (reader.Position == 0 || customTrackBar_Trk.Value == 0)
                {
                    if (!stopflag)
                    {
                        wasapiOut.Stop();
                        button_Stop.Enabled = false;
                        Sample = 0;
                        reader.Position = 0;
                        wasapiOut.Play();
                        button_Play.Text = Localization.PauseCaption;
                        Task.Run(Playback);
                        button_Stop.Enabled = true;
                    }
                }
            }
            else if (IsASIO)
            {
                if (reader.CurrentTime == reader.TotalTime)
                {
                    stopflag = true;
                    Sample = reader.SampleCount;
                    asioOut.Stop();
                    button_Play.Text = Localization.PlayCaption;
                    reader.Position = 0;
                    button_Stop.Enabled = false;
                    Resettrackbarlabels();
                }
                else if (reader.Position == 0 || customTrackBar_Trk.Value == 0)
                {
                    if (!stopflag)
                    {
                        asioOut.Stop();
                        button_Stop.Enabled = false;
                        Sample = 0;
                        reader.Position = 0;
                        asioOut.Play();
                        button_Play.Text = Localization.PauseCaption;
                        Task.Run(Playback);
                        button_Stop.Enabled = true;
                    }
                }
            }
            else
            {
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
        }

        private void Playback()
        {
            if (IsWASAPI || IsWASAPIex)
            {
                object lockobj = new();
                lock (lockobj)
                {
                    SLTAlive = true;
                    StartPlaybackThread_s = new(StartPlaybackThread);
                    Playback_s = new(StartPlaybackThread_s)
                    {
                        Name = "WASAPIOut",
                        IsBackground = true,
                        Priority = ThreadPriority.AboveNormal
                    };
                    Playback_s.SetApartmentState(ApartmentState.STA);
                    Playback_s.Start();
                }
            }
            else if (IsASIO)
            {
                object lockobj = new();
                lock (lockobj)
                {
                    SLTAlive = true;
                    StartPlaybackThread_s = new(StartPlaybackThread);
                    Playback_s = new(StartPlaybackThread_s)
                    {
                        Name = "ASIOOut",
                        IsBackground = true,
                        Priority = ThreadPriority.AboveNormal
                    };
                    Playback_s.SetApartmentState(ApartmentState.STA);
                    Playback_s.Start();
                }
            }
            else
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
            }
        }

        private void StartPlaybackThread()
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = UseThreads }; // 例: 最大2スレッドを使用
            Parallel.ForEach(Infinite(), options, (ignored, loopState) =>
            {
                // 処理内容
                if (IsWASAPI || IsWASAPIex)
                {
                    while (wasapiOut.PlaybackState != PlaybackState.Stopped)
                    {
                        if (!SLTAlive) { return; }
                        position = (int)reader.Position / reader.WaveFormat.AverageBytesPerSecond;
                        time = new(0, 0, position);
                        Sample = reader.Position / reader.BlockAlign;
                    }
                }
                else if (IsASIO)
                {
                    while (asioOut.PlaybackState != PlaybackState.Stopped)
                    {
                        if (!SLTAlive) { return; }
                        position = (int)reader.Position / reader.WaveFormat.AverageBytesPerSecond;
                        time = new(0, 0, position);
                        Sample = reader.Position / reader.BlockAlign;
                    }
                }
                else
                {
                    while (wo.PlaybackState != PlaybackState.Stopped)
                    {
                        if (!SLTAlive) { return; }
                        position = (int)reader.Position / reader.WaveFormat.AverageBytesPerSecond;
                        time = new(0, 0, position);
                        Sample = reader.Position / reader.BlockAlign;
                    }
                }
                loopState.Stop();
            });
        }

        private IEnumerable<bool> Infinite()
        {
            while (true) yield return true;
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

            if (!Generic.lpcreatev2)
            {
                FormMain.FormMainInstance.textBox_LoopStart.Text = pos.ToString();

                if (Generic.IsOpenMulti)
                {
                    Generic.MultipleLoopStarts[btnpos - 1] = (int)pos;
                    if (Generic.MultipleLoopEnds[btnpos - 1] != 0)
                    {
                        Generic.MultipleFilesLoopOKFlags[btnpos - 1] = true;
                    }
                    else
                    {
                        Generic.MultipleFilesLoopOKFlags[btnpos - 1] = false;
                    }
                }
                else
                {
                    Generic.MultipleLoopStarts[0] = (int)pos;
                    if (Generic.MultipleLoopEnds[0] != 0)
                    {
                        Generic.MultipleFilesLoopOKFlags[0] = true;
                        Generic.LoopStartNG = false;
                    }
                    else
                    {
                        Generic.MultipleFilesLoopOKFlags[0] = false;
                        if (string.IsNullOrWhiteSpace(FormMain.FormMainInstance.textBox_LoopStart.Text))
                        {
                            Generic.LoopStartNG = true;
                        }
                        else
                        {
                            Generic.LoopStartNG = false;
                        }
                    }
                    Debug.WriteLine("MultipleFilesLoopOKFlags[]: ", string.Join(", ", Generic.MultipleFilesLoopOKFlags));
                    Debug.WriteLine("[Flag] LoopStartNG: ", string.Join(", ", Generic.LoopStartNG));
                }
            }


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

            if (!Generic.lpcreatev2)
            {
                FormMain.FormMainInstance.textBox_LoopEnd.Text = pos.ToString();

                if (Generic.IsOpenMulti)
                {
                    Generic.MultipleLoopEnds[btnpos - 1] = (int)pos;
                    if (Generic.MultipleLoopStarts[btnpos - 1] != 0)
                    {
                        Generic.MultipleFilesLoopOKFlags[btnpos - 1] = true;
                    }
                    else
                    {
                        label_LoopEndSamples.Text = string.Empty;
                        Generic.MultipleFilesLoopOKFlags[btnpos - 1] = false;
                    }
                }
                else
                {
                    Generic.MultipleLoopEnds[0] = (int)pos;
                    if (Generic.MultipleLoopStarts[0] != 0)
                    {
                        Generic.MultipleFilesLoopOKFlags[0] = true;
                        Generic.LoopEndNG = false;
                    }
                    else
                    {
                        label_LoopEndSamples.Text = string.Empty;
                        Generic.MultipleFilesLoopOKFlags[0] = false;
                        if (string.IsNullOrWhiteSpace(FormMain.FormMainInstance.textBox_LoopEnd.Text))
                        {
                            Generic.LoopEndNG = true;
                        }
                        else
                        {
                            Generic.LoopEndNG = false;
                        }
                    }
                    Debug.WriteLine("MultipleFilesLoopOKFlags[]: ", string.Join(", ", Generic.MultipleFilesLoopOKFlags));
                    Debug.WriteLine("[Flag] LoopEndNG: ", string.Join(", ", Generic.LoopEndNG));
                }
            }


        }


        private void FormLPC_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (timer_Reload.Enabled == true) timer_Reload.Enabled = false;
            SLTAlive = false;
            reader.Position = 0;

            if (wi is not null)
            {
                wi.StopRecording();
                wi.Dispose();
            }

            if (wo is not null)
            {
                wo.Stop();
                wo.Dispose();
            }

            if (wasapiOut is not null)
            {
                wasapiOut.Stop();
                wasapiOut.Dispose();
            }

            if (asioOut is not null)
            {
                asioOut.Stop();
                asioOut.Dispose();
            }

            reader.Close();
            reader = null!;
        }

        private void Button_Prev_Click(object sender, EventArgs e)
        {
            btnpos--;

            if (btnpos - 1 == uint.MaxValue)
            {
                btnpos++;
            }

            Debug.WriteLine("Prev btnpos: ", string.Format("{0}", btnpos));
            Debug.WriteLine("MultipleFilesLoopOKFlags[]: ", string.Join(", ", Generic.MultipleFilesLoopOKFlags));
            Debug.WriteLine("MultipleLoopStarts[]: ", string.Join(", ", Generic.MultipleLoopStarts));
            Debug.WriteLine("MultipleLoopEnds[]: ", string.Join(", ", Generic.MultipleLoopEnds));

            if (!Generic.MultipleFilesLoopOKFlags[btnpos] && Generic.IsLoopWarning && Generic.IsOpenMulti && checkBox_LoopEnable.Checked)
            {
                if (Generic.MultipleLoopStarts[btnpos] == 0 || Generic.MultipleLoopEnds[btnpos] == 0)
                {
                    DialogResult dr = MessageBox.Show(Localization.LoopWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr == DialogResult.No)
                    {
                        if (btnpos != Generic.OpenFilePaths.Length)
                        {
                            btnpos++;
                        }

                        return;
                    }
                }
            }

            Generic.IsLPCStreamingReloaded = true;
            string[] Paths, OriginPaths;
            if (IsPlaybackATRAC && Generic.IsATRAC)
            {
                Paths = Generic.pATRACOpenFilePaths;
                OriginPaths = Generic.OpenFilePaths;
            }
            else if (IsEncodeSourceATRAC && Generic.IsATRAC)
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

            if (IsWASAPI || IsWASAPIex)
            {
                wasapiOut.Stop();
            }
            else if (IsASIO)
            {
                asioOut.Stop();
            }
            else
            {
                wo.Stop();
            }

            button_Play.Text = Localization.PlayCaption;
            reader.Position = 0;
            reader.Close();
            button_Stop.Enabled = false;

            _ = FormMain.FormMainInstance.Meta;

            if (btnpos == 1)
            {
                reader = new(Paths[btnpos - 1]);
                if (!PlaybackInit())
                {
                    return;
                }
                ResetAFR();
                label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                FormMain.FormMainInstance.FPLabel = fiorig.Directory + @"\" + fiorig.Name;
                FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, FS / 1024, FS);
                button_Prev.Enabled = false;
                button_Next.Enabled = true;
            }
            else
            {
                reader = new(Paths[btnpos - 1]);
                if (!PlaybackInit())
                {
                    return;
                }
                ResetAFR();
                label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                FormMain.FormMainInstance.FPLabel = fiorig.Directory + @"\" + fiorig.Name;
                FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, FS / 1024, FS);
                button_Prev.Enabled = true;
                button_Next.Enabled = true;
            }

            SetLoopPointsWithATRACBuffer(reader.WaveFormat.SampleRate, btnpos - 1);
            SetLoopPointWithMultipleFiles(reader.WaveFormat.SampleRate, btnpos - 1);

            smplrate = reader.WaveFormat.SampleRate;
            totalsamples = reader.SampleCount;
            Generic.IsLPCStreamingReloaded = false;
        }

        private void Button_Next_Click(object sender, EventArgs e)
        {
            btnpos++;

            if (btnpos == Generic.OpenFilePaths.Length + 1)
            {
                btnpos--;
            }

            Debug.WriteLine("Next btnpos: ", string.Format("{0}", btnpos));
            Debug.WriteLine("MultipleFilesLoopOKFlags[]: ", string.Join(", ", Generic.MultipleFilesLoopOKFlags));
            Debug.WriteLine("MultipleLoopStarts[]: ", string.Join(", ", Generic.MultipleLoopStarts));
            Debug.WriteLine("MultipleLoopEnds[]: ", string.Join(", ", Generic.MultipleLoopEnds));

            if (!Generic.MultipleFilesLoopOKFlags[btnpos - 2] && Generic.IsLoopWarning && Generic.IsOpenMulti && checkBox_LoopEnable.Checked)
            {
                if (Generic.MultipleLoopStarts[btnpos - 2] == 0 || Generic.MultipleLoopEnds[btnpos - 2] == 0)
                {
                    DialogResult dr = MessageBox.Show(Localization.LoopWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr == DialogResult.No)
                    {
                        if (btnpos != 1)
                        {
                            btnpos--;
                        }
                        return;
                    }
                }
            }

            Generic.IsLPCStreamingReloaded = true;
            string[] Paths, OriginPaths;
            if (IsPlaybackATRAC && Generic.IsATRAC)
            {
                Paths = Generic.pATRACOpenFilePaths;
                OriginPaths = Generic.OpenFilePaths;
            }
            else if (IsEncodeSourceATRAC && Generic.IsATRAC)
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

            if (IsWASAPI || IsWASAPIex)
            {
                wasapiOut.Stop();
            }
            else if (IsASIO)
            {
                asioOut.Stop();
            }
            else
            {
                wo.Stop();
            }
            button_Play.Text = Localization.PlayCaption;
            reader.Position = 0;
            reader.Close();
            button_Stop.Enabled = false;
            Resettrackbarlabels();

            _ = FormMain.FormMainInstance.Meta;

            if (btnpos == Paths.Length)
            {
                reader = new(Paths[btnpos - 1]);
                if (!PlaybackInit())
                {
                    return;
                }
                ResetAFR();
                label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                FormMain.FormMainInstance.FPLabel = fiorig.Directory + @"\" + fiorig.Name;
                FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, FS / 1024, FS);
                button_Next.Enabled = false;
                button_Prev.Enabled = true;
            }
            else
            {
                reader = new(Paths[btnpos - 1]);
                if (!PlaybackInit())
                {
                    return;
                }
                ResetAFR();
                label_File.Text = fi.Name.Replace(fi.Extension, "") + " [" + reader.WaveFormat.BitsPerSample + "-bit," + reader.WaveFormat.SampleRate + "Hz]";
                FormMain.FormMainInstance.FPLabel = fiorig.Directory + @"\" + fiorig.Name;
                FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, FS / 1024, FS);
                button_Next.Enabled = true;
                button_Prev.Enabled = true;
            }

            SetLoopPointsWithATRACBuffer(reader.WaveFormat.SampleRate, btnpos - 1);
            SetLoopPointWithMultipleFiles(reader.WaveFormat.SampleRate, btnpos - 1);

            smplrate = reader.WaveFormat.SampleRate;
            totalsamples = reader.SampleCount;
            Generic.IsLPCStreamingReloaded = false;
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void VolumeSlider1_VolumeChanged(object sender, EventArgs e)
        {
            if (IsWASAPI || IsWASAPIex)
            {
                wasapiOut.Volume = volumeSlider1.Volume;
            }
            else if (IsASIO)
            {
                asioOut.Volume = volumeSlider1.Volume;
            }
            else
            {
                wo.Volume = volumeSlider1.Volume;
            }
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
            if (!Generic.IsOpenMulti) // Single
            {
                if (checkBox_LoopEnable.Checked != false)
                {
                    if (Generic.IsAT3LoopSound || Generic.IsAT3LoopPoint)
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            MessageBox.Show(this, Localization.AT3AT9LoopBeginToEndAlreadyEnabledWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ResetLoopEnable();
                            return;
                        }
                        else
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC3 / ATRAC3+")
                            {
                                MessageBox.Show(this, Localization.AT3LoopBeginToEndAlreadyEnabledWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetLoopEnable();
                                return;
                            }

                        }

                    }
                    else
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC9")
                            {
                                MessageBox.Show(this, Localization.AT9LoopBeginToEndAlreadyEnabledWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetLoopEnable();
                                return;
                            }
                        }
                    }

                    if (!Generic.lpcreatev2 && Generic.lpcreate != false)
                    {
                        MessageBox.Show(this, Localization.LPCreateAlreadyEnableWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ResetLoopEnable();
                        return;
                    }

                    if (!Generic.lpcreatev2)
                    {
                        FormMain.FormMainInstance.groupBox_Loop.Enabled = true;
                        FormMain.FormMainInstance.textBox_LoopStart.Enabled = true;
                        FormMain.FormMainInstance.textBox_LoopEnd.Enabled = true;
                        FormMain.FormMainInstance.label_LoopStart.Enabled = true;
                        FormMain.FormMainInstance.label_LoopEnd.Enabled = true;
                        FormMain.FormMainInstance.label_SSample.Enabled = true;
                        FormMain.FormMainInstance.label_ESample.Enabled = true;
                    }


                    customTrackBar_Start.Enabled = true;
                    customTrackBar_Start.Invalidate();
                    customTrackBar_End.Enabled = true;
                    customTrackBar_End.Invalidate();
                    numericUpDown_LoopStart.Enabled = true;
                    numericUpDown_LoopEnd.Enabled = true;
                    button_LS_Current.Enabled = true;
                    button_LE_Current.Enabled = true;
                    button_SetStart.Enabled = true;
                    button_SetEnd.Enabled = true;

                    label_start.Enabled = true;
                    label_end.Enabled = true;
                }
                else
                {
                    if (Generic.IsAT3LoopSound || Generic.IsAT3LoopPoint)
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                        }
                        else
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC3 / ATRAC3+")
                            {
                            }
                            else
                            {
                                if (Generic.IsLoopWarning && Generic.MultipleFilesLoopOKFlags[0] && FormMain.FormMainInstance.button_Encode.Enabled)
                                {
                                    DialogResult dr = MessageBox.Show(Localization.LoopingAlreadySetWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (dr == DialogResult.No)
                                    {
                                        checkBox_LoopEnable.Checked = true;
                                        return;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC9")
                            {
                            }
                            else
                            {
                                if (Generic.IsLoopWarning && Generic.MultipleFilesLoopOKFlags[0] && FormMain.FormMainInstance.button_Encode.Enabled)
                                {
                                    DialogResult dr = MessageBox.Show(Localization.LoopingAlreadySetWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (dr == DialogResult.No)
                                    {
                                        checkBox_LoopEnable.Checked = true;
                                        return;
                                    }
                                }
                            }
                        }
                    }



                    Generic.MultipleFilesLoopOKFlags[0] = false;
                    Generic.MultipleLoopStarts[0] = 0;
                    Generic.MultipleLoopEnds[0] = 0;

                    if (!Generic.lpcreatev2)
                    {
                        FormMain.FormMainInstance.groupBox_Loop.Enabled = false;
                        FormMain.FormMainInstance.textBox_LoopStart.Text = null;
                        FormMain.FormMainInstance.textBox_LoopStart.Enabled = false;
                        FormMain.FormMainInstance.textBox_LoopEnd.Text = null;
                        FormMain.FormMainInstance.textBox_LoopEnd.Enabled = false;
                        FormMain.FormMainInstance.label_LoopStart.Enabled = false;
                        FormMain.FormMainInstance.label_LoopEnd.Enabled = false;
                        FormMain.FormMainInstance.label_SSample.Enabled = false;
                        FormMain.FormMainInstance.label_ESample.Enabled = false;
                    }



                    customTrackBar_Start.Enabled = false;
                    customTrackBar_Start.Invalidate();
                    customTrackBar_End.Enabled = false;
                    customTrackBar_End.Invalidate();
                    numericUpDown_LoopStart.Enabled = false;
                    numericUpDown_LoopEnd.Enabled = false;
                    button_LS_Current.Enabled = false;
                    button_LE_Current.Enabled = false;
                    button_SetStart.Enabled = false;
                    button_SetEnd.Enabled = false;

                    label_LoopStartSamples.Text = string.Empty;
                    label_LoopEndSamples.Text = string.Empty;

                    label_start.Enabled = false;
                    label_end.Enabled = false;
                }
            }
            else // Multiple
            {
                if (checkBox_LoopEnable.Checked != false)
                {

                    if (!Generic.lpcreatev2 && Generic.lpcreate != false)
                    {
                        MessageBox.Show(this, Localization.LPCreateAlreadyEnableWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ResetLoopEnable();
                        return;
                    }

                    if (Generic.IsAT3LoopSound || Generic.IsAT3LoopPoint)
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            MessageBox.Show(this, Localization.AT3AT9LoopBeginToEndAlreadyEnabledWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ResetLoopEnable();
                            return;
                        }
                        else
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC3 / ATRAC3+")
                            {
                                MessageBox.Show(this, Localization.AT3LoopBeginToEndAlreadyEnabledWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetLoopEnable();
                                return;
                            }
                        }

                    }
                    else
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC9")
                            {
                                MessageBox.Show(this, Localization.AT9LoopBeginToEndAlreadyEnabledWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetLoopEnable();
                                return;
                            }
                        }
                    }

                    if (!Generic.lpcreatev2)
                    {
                        FormMain.FormMainInstance.groupBox_Loop.Enabled = true;
                        FormMain.FormMainInstance.textBox_LoopStart.Enabled = true;
                        FormMain.FormMainInstance.textBox_LoopEnd.Enabled = true;
                        FormMain.FormMainInstance.label_LoopStart.Enabled = true;
                        FormMain.FormMainInstance.label_LoopEnd.Enabled = true;
                        FormMain.FormMainInstance.label_SSample.Enabled = true;
                        FormMain.FormMainInstance.label_ESample.Enabled = true;
                    }

                    customTrackBar_Start.Enabled = true;
                    customTrackBar_Start.Invalidate();
                    customTrackBar_End.Enabled = true;
                    customTrackBar_End.Invalidate();
                    numericUpDown_LoopStart.Enabled = true;
                    numericUpDown_LoopEnd.Enabled = true;
                    button_LS_Current.Enabled = true;
                    button_LE_Current.Enabled = true;
                    button_SetStart.Enabled = true;
                    button_SetEnd.Enabled = true;

                    label_start.Enabled = true;
                    label_end.Enabled = true;
                }
                else
                {
                    if (Generic.IsAT3LoopSound || Generic.IsAT3LoopPoint)
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                        }
                        else
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC3 / ATRAC3+")
                            {
                            }
                            else
                            {
                                if (Generic.IsLoopWarning && Generic.MultipleFilesLoopOKFlags[ButtonPosition - 1] && FormMain.FormMainInstance.button_Encode.Enabled)
                                {
                                    DialogResult dr = MessageBox.Show(Localization.LoopingAlreadySetWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (dr == DialogResult.No)
                                    {
                                        checkBox_LoopEnable.Checked = true;
                                        return;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC9")
                            {
                            }
                            else
                            {
                                if (Generic.IsLoopWarning && Generic.MultipleFilesLoopOKFlags[ButtonPosition - 1] && FormMain.FormMainInstance.button_Encode.Enabled)
                                {
                                    DialogResult dr = MessageBox.Show(Localization.LoopingAlreadySetWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (dr == DialogResult.No)
                                    {
                                        checkBox_LoopEnable.Checked = true;
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    if (!Generic.lpcreatev2)
                    {
                        Generic.MultipleFilesLoopOKFlags[ButtonPosition - 1] = false;
                        Generic.MultipleLoopStarts[ButtonPosition - 1] = 0;
                        Generic.MultipleLoopEnds[ButtonPosition - 1] = 0;

                        FormMain.FormMainInstance.groupBox_Loop.Enabled = false;
                        FormMain.FormMainInstance.textBox_LoopStart.Enabled = false;
                        FormMain.FormMainInstance.textBox_LoopEnd.Enabled = false;
                        FormMain.FormMainInstance.label_LoopStart.Enabled = false;
                        FormMain.FormMainInstance.label_LoopEnd.Enabled = false;
                        FormMain.FormMainInstance.label_SSample.Enabled = false;
                        FormMain.FormMainInstance.label_ESample.Enabled = false;
                    }
                    else
                    {
                        Generic.MultipleFilesLoopOKFlags[0] = false;
                        Generic.MultipleLoopStarts[0] = 0;
                        Generic.MultipleLoopEnds[0] = 0;
                    }

                    customTrackBar_Start.Enabled = false;
                    customTrackBar_Start.Invalidate();
                    customTrackBar_End.Enabled = false;
                    customTrackBar_End.Invalidate();
                    numericUpDown_LoopStart.Enabled = false;
                    numericUpDown_LoopEnd.Enabled = false;
                    button_LS_Current.Enabled = false;
                    button_LE_Current.Enabled = false;
                    button_SetStart.Enabled = false;
                    button_SetEnd.Enabled = false;

                    label_LoopStartSamples.Text = string.Empty;
                    label_LoopEndSamples.Text = string.Empty;

                    label_start.Enabled = false;
                    label_end.Enabled = false;
                }
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

                if (IsWASAPI || IsWASAPIex)
                {
                    wasapiOut.Stop();
                }
                else if (IsASIO)
                {
                    asioOut.Stop();
                }
                else
                {
                    wo.Stop();
                }

                if (radioButton_at3.Checked == true)
                {
                    Generic.LPCSuffix = " -loop " + Start.ToString() + " " + End.ToString();
                    //Config.Entry["ATRAC3_LoopSound"].Value = "false";
                    //Config.Entry["ATRAC3_LoopPoint"].Value = "true";
                    //Config.Entry["ATRAC3_LoopStart_Samples"].Value = Start.ToString();
                    //Config.Entry["ATRAC3_LoopEnd_Samples"].Value = End.ToString();
                    //Config.Entry["ATRAC3_Params"].Value = "";
                    //Config.Entry["LPC_Create"].Value = "false";
                }
                else
                {
                    Generic.LPCSuffix = " -loop " + Start.ToString() + " " + End.ToString();
                    //Config.Entry["ATRAC9_LoopSound"].Value = "false";
                    //Config.Entry["ATRAC9_LoopPoint"].Value = "true";
                    //Config.Entry["ATRAC9_LoopStart_Samples"].Value = Start.ToString();
                    //Config.Entry["ATRAC9_LoopEnd_Samples"].Value = End.ToString();
                    //Config.Entry["ATRAC9_Params"].Value = "";
                    //Config.Entry["LPC_Create"].Value = "false";
                }
                //Config.Save(xmlpath);

                smplrate = reader.WaveFormat.SampleRate;
                if (Generic.lpcreatev2)
                {
                    checkBox_LoopEnable.Checked = false;
                    customTrackBar_Start.Enabled = false;
                    //customTrackBar_Start.Invalidate();
                    customTrackBar_End.Enabled = false;
                    //customTrackBar_End.Invalidate();
                    label_start.Enabled = false;
                    label_end.Enabled = false;
                }
                //using FormSettings form = new(true);
                //form.ShowDialog();

                Close();
            }
            else
            {
                if (IsWASAPI || IsWASAPIex)
                {
                    wasapiOut.Stop();
                }
                else if (IsASIO)
                {
                    asioOut.Stop();
                }
                else
                {
                    wo.Stop();
                }
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
                    MessageBox.Show(this, Localization.AT3LoopBeginToEndAlreadyEnabledWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show(this, Localization.AT9LoopBeginToEndAlreadyEnabledWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetLoopEnable();
                    return;
                }
            }
        }

        private void ResetLoopEnable()
        {
            checkBox_LoopEnable.Checked = false;
            customTrackBar_Start.Enabled = false;
            customTrackBar_Start.Invalidate();
            customTrackBar_End.Enabled = false;
            customTrackBar_End.Invalidate();
            numericUpDown_LoopStart.Enabled = false;
            numericUpDown_LoopEnd.Enabled = false;
            button_LS_Current.Enabled = false;
            button_LE_Current.Enabled = false;
            button_SetStart.Enabled = false;
            button_SetEnd.Enabled = false;
            label_LoopStartSamples.Text = string.Empty;
            label_LoopEndSamples.Text = string.Empty;

            FormMain.FormMainInstance.groupBox_Loop.Enabled = false;
            FormMain.FormMainInstance.textBox_LoopStart.Enabled = false;
            FormMain.FormMainInstance.textBox_LoopEnd.Enabled = false;
            FormMain.FormMainInstance.label_LoopStart.Enabled = false;
            FormMain.FormMainInstance.label_LoopEnd.Enabled = false;
            FormMain.FormMainInstance.label_SSample.Enabled = false;
            FormMain.FormMainInstance.label_ESample.Enabled = false;
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

            //ResetLoopEnable();
            Resettrackbarlabels();
        }

        private void Resettrackbarlabels()
        {
            ScaleWidthTrk = (float)customTrackBar_Trk.Size.Width / ((float)customTrackBar_Trk.Maximum - (float)customTrackBar_Trk.Minimum);
            ScaleWidthStart = (float)customTrackBar_Start.Size.Width / ((float)customTrackBar_Start.Maximum - (float)customTrackBar_Start.Minimum);
            ScaleWidthEnd = (float)customTrackBar_End.Size.Width / ((float)customTrackBar_End.Maximum - (float)customTrackBar_End.Minimum);
            labelTrk = MainDefaultPoint;
            labelStart = StartDefaultPoint;
            labelEnd = EndDefaultPoint;
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

        /// <summary>
        /// ATRACからループ情報を読み取りUIに反映
        /// </summary>
        /// <param name="samplerate">サンプリング周波数</param>
        /// <param name="pos">Current ButtonPosition (multiple files only)</param>
        private void SetLoopPointsWithATRACBuffer(int samplerate, uint pos = 0)
        {
            if (Generic.IsATRACLooped)
            {
                if (bufferloop[0] == 0 || bufferloop[1] == 0)
                {
                    if (Generic.IsOpenMulti)
                    {
                        Generic.MultipleFilesLoopOKFlags[pos] = false;
                        Generic.MultipleLoopStarts[pos] = 0;
                        Generic.MultipleLoopEnds[pos] = 0;
                    }
                    else
                    {
                        Generic.MultipleFilesLoopOKFlags[0] = false;
                        Generic.MultipleLoopStarts[0] = 0;
                        Generic.MultipleLoopEnds[0] = 0;
                        Generic.LoopNG = true;
                    }
                    return;
                }
                if (bufferloop[0] != 0 && bufferloop[1] != 0)
                {
                    if (Generic.IsOpenMulti)
                    {
                        Generic.MultipleFilesLoopOKFlags[pos] = true;
                        Generic.MultipleLoopStarts[pos] = bufferloop[0];
                        Generic.MultipleLoopEnds[pos] = bufferloop[1];
                    }
                    else
                    {
                        Generic.MultipleFilesLoopOKFlags[0] = true;
                        Generic.MultipleLoopStarts[0] = bufferloop[0];
                        Generic.MultipleLoopEnds[0] = bufferloop[1];
                    }
                }

                switch (samplerate)
                {
                    case 12000: // ATRAC9 Only
                        customTrackBar_Start.Value = (int)Math.Round(Generic.MultipleLoopStarts[pos] / 12.0, MidpointRounding.AwayFromZero);//value[0];
                        numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
                        customTrackBar_End.Value = (int)Math.Round(Generic.MultipleLoopEnds[pos] / 12.0, MidpointRounding.AwayFromZero);//value[1];
                        numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
                        break;
                    case 24000: // ATRAC9 Only
                        customTrackBar_Start.Value = (int)Math.Round(Generic.MultipleLoopStarts[pos] / 24.0, MidpointRounding.AwayFromZero);//value[0];
                        numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
                        customTrackBar_End.Value = (int)Math.Round(Generic.MultipleLoopEnds[pos] / 24.0, MidpointRounding.AwayFromZero);//value[1];
                        numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
                        break;
                    case 44100:
                        customTrackBar_Start.Value = (int)Math.Round(bufferloop[0] / 44.1, MidpointRounding.AwayFromZero);//value[0];
                        numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
                        customTrackBar_End.Value = (int)Math.Round(bufferloop[1] / 44.1, MidpointRounding.AwayFromZero);//value[1];
                        numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
                        break;
                    case 48000:
                        customTrackBar_Start.Value = (int)Math.Round(bufferloop[0] / 48.0, MidpointRounding.AwayFromZero);//value[0];
                        numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
                        customTrackBar_End.Value = (int)Math.Round(bufferloop[1] / 48.0, MidpointRounding.AwayFromZero);//value[1];
                        numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
                        break;
                    default:
                        break;
                }

                if (Generic.IsOpenMulti)
                {
                    if (Generic.MultipleFilesLoopOKFlags[pos] && !checkBox_LoopEnable.Checked)
                    {
                        checkBox_LoopEnable.Checked = true;
                        customTrackBar_Start.Enabled = true;
                        customTrackBar_Start.Invalidate();
                        customTrackBar_End.Enabled = true;
                        customTrackBar_End.Invalidate();
                        numericUpDown_LoopStart.Enabled = true;
                        numericUpDown_LoopEnd.Enabled = true;
                        button_LS_Current.Enabled = true;
                        button_LE_Current.Enabled = true;
                        button_SetEnd.Enabled = true;
                        button_SetStart.Enabled = true;
                        label_start.Enabled = true;
                        label_end.Enabled = true;
                    }
                    label_LoopStartSamples.Text = "LoopStart: " + Generic.MultipleLoopStarts[pos].ToString() + " " + Localizable.Localization.SampleCaption;
                    label_LoopEndSamples.Text = "LoopEnd: " + Generic.MultipleLoopEnds[pos].ToString() + " " + Localizable.Localization.SampleCaption;
                    FormMain.FormMainInstance.textBox_LoopStart.Text = Generic.MultipleLoopStarts[pos].ToString();
                    FormMain.FormMainInstance.textBox_LoopEnd.Text = Generic.MultipleLoopEnds[pos].ToString();
                }
                else
                {
                    label_LoopStartSamples.Text = "LoopStart: " + bufferloop[0].ToString() + " " + Localizable.Localization.SampleCaption;
                    label_LoopEndSamples.Text = "LoopEnd: " + bufferloop[1].ToString() + " " + Localizable.Localization.SampleCaption;
                    FormMain.FormMainInstance.textBox_LoopStart.Text = bufferloop[0].ToString();
                    FormMain.FormMainInstance.textBox_LoopEnd.Text = bufferloop[1].ToString();
                    Generic.LoopNG = false;
                }
            }
        }

        private void SetLoopPointWithMultipleFiles(int samplerate, uint pos)
        {
            if (!Generic.IsATRACLooped && Generic.IsOpenMulti && Generic.MultipleFilesLoopOKFlags[pos])
            {
                if (Generic.MultipleLoopStarts[pos] == 0 || Generic.MultipleLoopEnds[pos] == 0) { return; }

                switch (samplerate)
                {
                    case 12000: // ATRAC9 Only
                        customTrackBar_Start.Value = (int)Math.Round(Generic.MultipleLoopStarts[pos] / 12.0, MidpointRounding.AwayFromZero);//value[0];
                        numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
                        customTrackBar_End.Value = (int)Math.Round(Generic.MultipleLoopEnds[pos] / 12.0, MidpointRounding.AwayFromZero);//value[1];
                        numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
                        break;
                    case 24000: // ATRAC9 Only
                        customTrackBar_Start.Value = (int)Math.Round(Generic.MultipleLoopStarts[pos] / 24.0, MidpointRounding.AwayFromZero);//value[0];
                        numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
                        customTrackBar_End.Value = (int)Math.Round(Generic.MultipleLoopEnds[pos] / 24.0, MidpointRounding.AwayFromZero);//value[1];
                        numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
                        break;
                    case 44100:
                        customTrackBar_Start.Value = (int)Math.Round(Generic.MultipleLoopStarts[pos] / 44.1, MidpointRounding.AwayFromZero);//value[0];
                        numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
                        customTrackBar_End.Value = (int)Math.Round(Generic.MultipleLoopEnds[pos] / 44.1, MidpointRounding.AwayFromZero);//value[1];
                        numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
                        break;
                    case 48000:
                        customTrackBar_Start.Value = (int)Math.Round(Generic.MultipleLoopStarts[pos] / 48.0, MidpointRounding.AwayFromZero);//value[0];
                        numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
                        customTrackBar_End.Value = (int)Math.Round(Generic.MultipleLoopEnds[pos] / 48.0, MidpointRounding.AwayFromZero);//value[1];
                        numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
                        break;
                    default:
                        break;
                }

                if (Generic.MultipleFilesLoopOKFlags[pos] && !checkBox_LoopEnable.Checked)
                {
                    checkBox_LoopEnable.Checked = true;
                    customTrackBar_Start.Enabled = true;
                    customTrackBar_Start.Invalidate();
                    customTrackBar_End.Enabled = true;
                    customTrackBar_End.Invalidate();
                    numericUpDown_LoopStart.Enabled = true;
                    numericUpDown_LoopEnd.Enabled = true;
                    button_LS_Current.Enabled = true;
                    button_LE_Current.Enabled = true;
                    button_SetEnd.Enabled = true;
                    button_SetStart.Enabled = true;
                    label_start.Enabled = true;
                    label_end.Enabled = true;
                }
                label_LoopStartSamples.Text = "LoopStart: " + Generic.MultipleLoopStarts[pos].ToString() + " " + Localizable.Localization.SampleCaption;
                label_LoopEndSamples.Text = "LoopEnd: " + Generic.MultipleLoopEnds[pos].ToString() + " " + Localizable.Localization.SampleCaption;
                FormMain.FormMainInstance.textBox_LoopStart.Text = Generic.MultipleLoopStarts[pos].ToString();
                FormMain.FormMainInstance.textBox_LoopEnd.Text = Generic.MultipleLoopEnds[pos].ToString();
            }
            else if (!Generic.IsATRACLooped && Generic.IsOpenMulti && !Generic.MultipleFilesLoopOKFlags[pos])
            {
                if (checkBox_LoopEnable.Checked)
                {
                    checkBox_LoopEnable.Checked = false;
                    customTrackBar_Start.Enabled = false;
                    customTrackBar_Start.Invalidate();
                    customTrackBar_End.Enabled = false;
                    customTrackBar_End.Invalidate();
                    numericUpDown_LoopStart.Enabled = false;
                    numericUpDown_LoopEnd.Enabled = false;
                    button_LS_Current.Enabled = false;
                    button_LE_Current.Enabled = false;
                    button_SetEnd.Enabled = false;
                    button_SetStart.Enabled = false;
                    label_start.Enabled = false;
                    label_end.Enabled = false;
                }
                label_LoopStartSamples.Text = string.Empty;
                label_LoopEndSamples.Text = string.Empty;
                FormMain.FormMainInstance.textBox_LoopStart.Text = string.Empty;
                FormMain.FormMainInstance.textBox_LoopEnd.Text = string.Empty;
            }
        }
    }

    sealed class ExtensiblePcmShim : IWaveProvider
    {
        private readonly WaveStream _source;
        private readonly WaveFormat _pcmFormat;

        public ExtensiblePcmShim(WaveStream source)
        {
            _source = source;
            var wf = source.WaveFormat;
            _pcmFormat = new WaveFormat(wf.SampleRate, wf.BitsPerSample, wf.Channels);
        }

        public WaveFormat WaveFormat => _pcmFormat;

        public int Read(byte[] buffer, int offset, int count)
            => _source.Read(buffer, offset, count);
    }
}
