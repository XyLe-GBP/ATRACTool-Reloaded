using ATRACTool_Reloaded.Localizable;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static ATRACTool_Reloaded.Common;
using static ATRACTool_Reloaded.Common.Constants;

namespace ATRACTool_Reloaded
{
    public partial class FormLPC : Form
    {
        private readonly WaveInEvent wi = new();
        private readonly WaveOutEvent wo = new();
        private MMDevice? mmDevice;
        private WasapiOut wasapiOut = null!;
        private AsioOut asioOut = null!;
        private string asioDriver = null!;
        private readonly List<IDisposable> _disposables = [];
        WaveFileReader reader = null!;
        BufferedWaveProvider BufwaveProvider = null!;
        VolumeSampleProvider volumeSmplProvider = null!;
        PanningSampleProvider panSmplProvider = null!;
        long Sample, Start = 0, End = 0;
        int bytePerSec, position, length, smplrate, WASAPILatency = 0, WASAPIexLatency = 0, UseThreads = 3;
        long totalsamples;
        uint btnpos;
        TimeSpan time;
        bool mouseDown = false, stopflag = false, IsPausedMoveTrackbar, SmoothSamples = false, IsPlaybackATRAC = false, IsEncodeSourceATRAC = false, IsMultiChannel = false, IsWASAPI = false, IsWASAPIex = false, IsASIO = false, UseParallel = false;
        float ScaleWidthTrk = 0f, ScaleWidthStart = 0f, ScaleWidthEnd = 0f;
        Point MainDefaultPoint = new(15, 88), StartDefaultPoint = new(15, 160), EndDefaultPoint = new(15, 32);

        Point labelTrk, labelStart, labelEnd;

        private volatile bool SLTAlive;
        ThreadStart StartPlaybackThread_s = null!;
        Thread Playback_s = null!;

        int[] bufferloop = new int[2];

        private static FormLPC _formLPCInstance = null!;
        /// <summary>
        /// FormLPC インスタンス
        /// </summary>
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

        /// <summary>
        /// FormLPCの現在のLoopStart
        /// </summary>
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

        /// <summary>
        /// FormLPCの現在のLoopEnd
        /// </summary>
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
            FormMain.DebugInfo("[FormLPC] Initialized.");

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

            customTrackBar_Trk.Scroll += CustomTrackBar_Trk_Scroll;
            customTrackBar_Start.Scroll += CustomTrackBar_Start_Scroll;
            customTrackBar_End.Scroll += CustomTrackBar_End_Scroll;
            customTrackBar_Trk.MouseDown += CustomTrackBar_Trk_MouseDown;
            customTrackBar_Trk.MouseUp += CustomTrackBar_Trk_MouseUp;
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
        }

        private void CustomTrackBar_End_Scroll(object? sender, EventArgs e)
        {
            numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
        }

        private void CustomTrackBar_Start_Scroll(object? sender, EventArgs e)
        {
            numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
        }

        private void CustomTrackBar_Trk_Scroll(object? sender, EventArgs e)
        {
            reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Trk.Value);
        }

        private void CustomTrackBar_Trk_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!mouseDown) return;
            mouseDown = false;
        }

        private void CustomTrackBar_Trk_MouseDown(object? sender, MouseEventArgs e)
        {
            if (reader == null) return;
            mouseDown = true;
        }

        private void FormLPC_Load(object sender, EventArgs e)
        {
            Config.Load(xmlpath);

            FormLPCInstance = this;

            // SmoothSamples
            SmoothSamples = Utils.GetBool("SmoothSamples", false);

            // ATRAC 再生可否
            IsPlaybackATRAC = Utils.GetBool("PlaybackATRAC", false);

            // ATRAC をエンコードソースとして扱うか
            IsEncodeSourceATRAC = Utils.GetBool("ATRACEncodeSource", false);

            // LPCPlaybackMethod
            LPCPlaybackMethodType playbackMethod = (LPCPlaybackMethodType)Utils.GetInt("LPCPlaybackMethod", 0);
            switch (playbackMethod)
            {
                case LPCPlaybackMethodType.DirectSound:
                    IsWASAPI = false;
                    IsWASAPIex = false;
                    IsASIO = false;
                    break;
                case LPCPlaybackMethodType.WasapiShared:
                    IsWASAPI = true;
                    IsWASAPIex = false;
                    IsASIO = false;
                    break;
                case LPCPlaybackMethodType.WasapiExclusive:
                    IsWASAPI = false;
                    IsWASAPIex = true;
                    IsASIO = false;
                    break;
                case LPCPlaybackMethodType.ASIO:
                    IsWASAPI = false;
                    IsWASAPIex = false;
                    IsASIO = true;
                    break;
                default:
                    IsWASAPI = false;
                    IsWASAPIex = false;
                    IsASIO = false;
                    break;
            }

            // ASIO ドライバ名取得
            string asioConfig = Utils.GetString("LPCUseASIODriver", string.Empty);
            if (string.IsNullOrWhiteSpace(asioConfig))
            {
                asioDriver = string.Empty;
                int lpcPlayback = Utils.GetInt("LPCPlaybackMethod", 0);
                int multiPlayback = Utils.GetInt("LPCMultipleStreamPlaybackMethod", 65535);

                if (lpcPlayback == 3 || multiPlayback == 2)
                {
                    MessageBox.Show(
                        "It is configured to play using ASIO, but no valid driver was found.\r\nIt will play using WASAPI exclusive mode instead.",
                        Localization.MSGBoxWarningCaption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    IsWASAPI = false;
                    IsWASAPIex = true;
                    IsASIO = false;
                }
            }
            else
            {
                asioDriver = asioConfig;
            }

            UseParallel = Utils.GetBool("UseParallelMethod", false);

            if (SmoothSamples)
            {
                if (IsWASAPI || IsWASAPIex)
                {
                    WASAPILatency = 0;
                }
                else if (IsASIO)
                {
                    //ASIOLatency = 0;
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
                    WASAPILatency = Utils.GetInt("WASAPILatencySharedValue", 0);
                }
                else if (IsWASAPIex)
                {
                    WASAPIexLatency = Utils.GetInt("WASAPILatencyExclusivedValue", 0);
                }
                else if (IsASIO)
                {
                    //ASIOLatency = 100;
                }
                else
                {
                    wo.DesiredLatency = Utils.GetInt("DirectSoundLatencyValue", 200);
                    wo.NumberOfBuffers = Utils.GetInt("DirectSoundBuffersValue", 16);
                }
            }

            UseThreads = Utils.GetInt("PlaybackThreadCount", 4);

            Generic.IsLPCStreamingReloaded = true;
            if (IsPlaybackATRAC && Generic.IsATRAC) // ATRAC再生機能有効
            {
                checkBox_LoopEnable.Enabled = false;
                radioButton_at3.Enabled = false;
                radioButton_at9.Enabled = false;

                if (Common.Generic.pATRACOpenFilePaths.Length == 1) // 単一ファイル
                {
                    reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                    //FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = Path.GetFileNameWithoutExtension(Generic.InputJobs[0].OriginPath) + GetCurrentReaderBitAndHzFromLabel(reader);
                    button_Prev.Enabled = false;
                    button_Next.Enabled = false;
                }
                else // 複数ファイル
                {
                    reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                    //FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = Path.GetFileNameWithoutExtension(Generic.InputJobs[0].OriginPath) + GetCurrentReaderBitAndHzFromLabel(reader);
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
                    //FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = Path.GetFileNameWithoutExtension(Generic.InputJobs[0].OriginPath) + GetCurrentReaderBitAndHzFromLabel(reader);
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
                        //FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                        label_File.Text = Path.GetFileNameWithoutExtension(Generic.InputJobs[0].OriginPath) + GetCurrentReaderBitAndHzFromLabel(reader);
                        button_Prev.Enabled = false;
                        button_Next.Enabled = true;
                        btnpos = 1;
                    }
                }
            }
            else // 通常ファイル
            {
                if (Common.Generic.OpenFilePaths.Length == 1) // 単一ファイル
                {
                    reader = new(Common.Generic.OpenFilePaths[0]);
                    //FileInfo fi = new(Common.Generic.OpenFilePaths[0]);
                    label_File.Text = Path.GetFileNameWithoutExtension(Generic.InputJobs[0].OriginPath) + GetCurrentReaderBitAndHzFromLabel(reader);
                    button_Prev.Enabled = false;
                    button_Next.Enabled = false;
                }
                else
                {
                    if (Generic.lpcreatev2 && Common.Generic.lpcreate != false) // LPC有効
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
                    else // 複数ファイル
                    {
                        reader = new(Common.Generic.OpenFilePaths[0]);
                        //FileInfo fi = new(Common.Generic.OpenFilePaths[0]);
                        label_File.Text = Path.GetFileNameWithoutExtension(Generic.InputJobs[0].OriginPath) + GetCurrentReaderBitAndHzFromLabel(reader);
                        button_Prev.Enabled = false;
                        button_Next.Enabled = true;
                        btnpos = 1;
                    }
                }
            }

            if (!PlaybackInit())
            {
                // 再生の初期化に失敗した場合は、このフォーム自体も閉じる
                Close();
                return;
            }

            _ = FormMain.FormMainInstance.Meta;

            Generic.IsLPCStreamingReloaded = false;

            if (!IsMultiChannel)
            {
                BufwaveProvider = new BufferedWaveProvider(reader.WaveFormat)
                {
                    BufferDuration = TimeSpan.FromMilliseconds(500) // バッファの長さを設定
                };
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

            int maxMs = GetSafeDurationMilliseconds();

            customTrackBar_Trk.Minimum = 0;
            customTrackBar_Trk.Maximum = maxMs;
            customTrackBar_Start.Minimum = 0;
            customTrackBar_Start.Maximum = maxMs;
            customTrackBar_End.Minimum = 0;
            customTrackBar_End.Maximum = maxMs;

            ScaleWidthTrk = (float)customTrackBar_Trk.Size.Width / ((float)customTrackBar_Trk.Maximum - (float)customTrackBar_Trk.Minimum);
            ScaleWidthStart = (float)customTrackBar_Start.Size.Width / ((float)customTrackBar_Start.Maximum - (float)customTrackBar_Start.Minimum);
            ScaleWidthEnd = (float)customTrackBar_End.Size.Width / ((float)customTrackBar_End.Maximum - (float)customTrackBar_End.Minimum);

            customTrackBar_Trk.TickFrequency = 1000;
            customTrackBar_Start.TickFrequency = 1000;
            customTrackBar_End.TickFrequency = 1000;
            numericUpDown_LoopStart.Minimum = 0;
            numericUpDown_LoopStart.Maximum = maxMs;
            numericUpDown_LoopStart.Increment = 1;
            numericUpDown_LoopEnd.Minimum = 0;
            numericUpDown_LoopEnd.Maximum = maxMs;
            numericUpDown_LoopEnd.Increment = 1;

            wo.Volume = volumeSlider1.Volume;

            label_trk.Text = "";
            label_start.Text = "";
            label_end.Text = "";

            int tb = GetSafeHalfDurationMillisecondsForLoopEnd();
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
        private static ISampleProvider BuildFloatFromWaveFileReader(WaveFileReader reader)
        {
            var wf = reader.WaveFormat;

            // 1) すでに float
            if (wf.Encoding == WaveFormatEncoding.IeeeFloat)
                return reader.ToSampleProvider();

            // 2) Extensible でも 16/24/32bit PCM なら “シム” → そのビット深度→float
            if (wf.Encoding == WaveFormatEncoding.Extensible)
            {
                IWaveProvider shim = new ExtensiblePcmShim(reader);
                return wf.BitsPerSample switch
                {
                    16 => new Wave16ToFloatProvider(shim).ToSampleProvider(),
                    24 => new WaveToSampleProvider(shim),// これ自体が ISampleProvider(float)
                    32 => new WaveToSampleProvider(shim),// 同上
                    _ => throw new NotSupportedException($"Extensible {wf.BitsPerSample}bit は未対応です。"),
                };
            }

            // 3) 素の PCM（非 Extensible）
            if (wf.Encoding == WaveFormatEncoding.Pcm)
            {
                return wf.BitsPerSample switch
                {
                    16 => new Wave16ToFloatProvider(reader).ToSampleProvider(),
                    24 => new WaveToSampleProvider(reader),
                    32 => new WaveToSampleProvider(reader),
                    _ => throw new NotSupportedException($"PCM {wf.BitsPerSample}bit は未対応です。"),
                };
            }

            // 4) それ以外（ADPCM / μ-law 等）は WaveFileReader ではなく MediaFoundationReader を使う
            throw new NotSupportedException("このWAVは圧縮コーデックを含むため、WaveFileReaderではなく MediaFoundationReader を使用してください。");
        }

        // 再生監視スレッドとタイマーを停止するヘルパー
        private void StopPlaybackLoop()
        {
            // Playback() → StartPlaybackThread() が回しているループの終了トリガ
            SLTAlive = false;

            // 進行状況更新用タイマーも止めておく
            if (timer_Reload.Enabled)
            {
                timer_Reload.Stop();
            }
        }

        /// <summary>
        /// WASAPI(共有/排他)の初期化を行う共通ヘルパー。
        /// 排他モードでフォーマット未対応の場合は共有モードにフォールバックする。
        /// </summary>
        private bool TryInitWasapi(IWaveProvider provider)
        {
            mmDevice = new MMDeviceEnumerator()
                .GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            // まずは現在の設定（共有 or 排他）で試す
            try
            {
                wasapiOut = new WasapiOut(
                    mmDevice,
                    IsWASAPIex ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared,
                    false,
                    IsWASAPIex ? WASAPIexLatency : WASAPILatency);

                wasapiOut.Init(provider);
                return true;
            }
            catch (COMException ex) when (IsWASAPIex && ex.HResult == unchecked((int)0x8889000A))
            {
                // 排他モードでフォーマット未対応の典型パターン (0x8889000A)
                // → 共有モードにフォールバック
                MessageBox.Show(
                    this,
                    "このオーディオ形式は WASAPI 排他モードでは再生できません。\r\n" +
                    "WASAPI 共有モードで再生します。",
                    Localization.MSGBoxWarningCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                try
                {
                    // 排他→共有 に切り替え
                    IsWASAPI = true;
                    IsWASAPIex = false;

                    wasapiOut = new WasapiOut(
                        mmDevice,
                        AudioClientShareMode.Shared,
                        false,
                        WASAPILatency);

                    wasapiOut.Init(provider);
                    return true;
                }
                catch (COMException)
                {
                    // 共有でもダメなら諦める
                    MessageBox.Show(
                        this,
                        "このオーディオ形式は WASAPI では再生できません。\r\n" +
                        "再生方法の設定を変更してください。",
                        Localization.MSGBoxErrorCaption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    // ★ 再生監視スレッド＆タイマーを即停止
                    StopPlaybackLoop();

                    // ★ メインフォームを「起動直後の状態」に戻す
                    FormMain.FormMainInstance.ResetToInitialState();

                    // false を返すことで、FormLPC_Load 側の
                    // 「if (!PlaybackInit()) { Close(); }」が実行され、
                    // この LPC フォーム自体も閉じられます。
                    return false;
                }
            }
            catch (COMException ex)
            {
                // その他の WASAPI 初期化エラー
                MessageBox.Show(
                    this,
                    "WASAPI の初期化に失敗しました。\r\n" + ex.Message,
                    Localization.MSGBoxErrorCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                // 念のため監視ループも止める
                StopPlaybackLoop();

                return false;
            }
        }

        /// <summary>
        /// 曲の長さの半分（ms）を計算し、
        /// TrackBar / NumericUpDown の上限を超えないようにクランプした値を返します。
        /// </summary>
        private int GetSafeHalfDurationMillisecondsForLoopEnd()
        {
            if (reader == null)
            {
                return 0;
            }

            // TotalMilliseconds は long で受ける（オーバーフロー対策）
            long halfMs = (long)(reader.TotalTime.TotalMilliseconds / 2.0);

            // TrackBar と NumericUpDown のどちらでも扱える範囲に合わせる
            int tbMax = customTrackBar_End.Maximum;
            int nudMax = (int)numericUpDown_LoopEnd.Maximum;
            int max = Math.Min(tbMax, nudMax);

            if (max <= 0)
            {
                return 0;
            }

            // 0 ～ max の範囲にクランプして int にキャスト
            if (halfMs < 0)
            {
                return 0;
            }
            if (halfMs > max)
            {
                return max;
            }

            return (int)halfMs;
        }

        /// <summary>
        /// 曲の全長（ms）を安全に int に収めて返す。
        /// TrackBar / NumericUpDown の Maximum に使う用。
        /// </summary>
        private int GetSafeDurationMilliseconds()
        {
            if (reader == null)
            {
                return 0;
            }

            double totalMs = reader.TotalTime.TotalMilliseconds;

            if (totalMs <= 0)
            {
                return 0;
            }

            if (totalMs > int.MaxValue)
            {
                return int.MaxValue;
            }

            return (int)totalMs;
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
                            var provider = new WaveChannel32(reader);
                            if (!TryInitWasapi(provider))
                            {
                                return false; // エラーを出しているのでそのまま抜ける
                            }
                        }
                        else if (IsASIO)
                        {
                            asioOut = new(asioDriver);
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
                            var provider = new WaveChannel32(reader);
                            if (!TryInitWasapi(provider))
                            {
                                return false; // エラーを出しているのでそのまま抜ける
                            }
                        }
                        else if (IsASIO)
                        {
                            asioOut = new(asioDriver);
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
                                if (!TryInitWasapi(output))
                                {
                                    return false;
                                }
                            }
                            else if (IsASIO)
                            {
                                asioOut = new(asioDriver);
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
                                if (!TryInitWasapi(output))
                                {
                                    return false;
                                }
                            }
                            else if (IsASIO)
                            {
                                asioOut = new(asioDriver);
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

        // 再生ボタンの動作を共通化するヘルパー
        private async Task HandlePlayButtonAsync(IWavePlayer output)
        {
            switch (output.PlaybackState)
            {
                case PlaybackState.Stopped:
                    // 再生時間や長さの計算（従来と同じ）
                    bytePerSec = reader.WaveFormat.BitsPerSample / 8
                                 * reader.WaveFormat.SampleRate
                                 * reader.WaveFormat.Channels;
                    length = (int)reader.Length / bytePerSec;

                    timer_Reload.Enabled = true;
                    output.Play();
                    button_Play.Text = Localization.PauseCaption;
                    await Task.Run(Playback);
                    stopflag = false;
                    button_Stop.Enabled = true;
                    break;

                case PlaybackState.Paused:
                    if (IsPausedMoveTrackbar)
                    {
                        // 一度止めて位置を移動してから再生し直す
                        output.Stop();
                        reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Trk.Value);
                        output.Play();
                        await Task.Run(Playback);
                        IsPausedMoveTrackbar = false;
                    }
                    else
                    {
                        output.Play();
                    }

                    button_Play.Text = Localization.PauseCaption;
                    break;

                case PlaybackState.Playing:
                    output.Pause();
                    button_Play.Text = Localization.PlayCaption;
                    break;
            }
        }

        private async void Button_Play_Click(object sender, EventArgs e)
        {
            if (IsWASAPI || IsWASAPIex)
            {
                await HandlePlayButtonAsync(wasapiOut);
            }
            else if (IsASIO)
            {
                await HandlePlayButtonAsync(asioOut);
            }
            else
            {
                await HandlePlayButtonAsync(wo);
            }
        }

        private void HandleStopButton(IWavePlayer output)
        {
            if (output.PlaybackState != PlaybackState.Stopped)
            {
                stopflag = true;
                timer_Reload.Stop();
                output.Stop();
                button_Play.Text = Localization.PlayCaption;
                reader.Position = 0;
                button_Stop.Enabled = false;
                Resettrackbarlabels();
            }
        }


        private void Button_Stop_Click(object sender, EventArgs e)
        {
            if (IsWASAPI || IsWASAPIex)
            {
                HandleStopButton(wasapiOut);
            }
            else if (IsASIO)
            {
                HandleStopButton(asioOut);
            }
            else
            {
                HandleStopButton(wo);
            }
        }

        /// <summary>
        /// Timer_Reload_Tick から呼び出す、再生終了／再生再開の共通処理。
        /// </summary>
        private void HandleTimerReloadForOutput(IWavePlayer output)
        {
            // 再生が最後まで到達したら停止処理
            if (reader.CurrentTime == reader.TotalTime)
            {
                stopflag = true;
                Sample = reader.SampleCount;

                output.Stop();
                button_Play.Text = Localization.PlayCaption;
                reader.Position = 0;
                button_Stop.Enabled = false;
                Resettrackbarlabels();
            }
            // 先頭付近まで戻っていて、ユーザーが Stop していない場合は再生し直す
            else if (reader.Position == 0 || customTrackBar_Trk.Value == 0)
            {
                if (!stopflag)
                {
                    output.Stop();
                    button_Stop.Enabled = false;
                    Sample = 0;
                    reader.Position = 0;

                    output.Play();
                    button_Play.Text = Localization.PauseCaption;

                    // 監視スレッドを再度起動（従来どおり）
                    Task.Run(Playback);
                    button_Stop.Enabled = true;
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

            // 出力デバイスごとの処理は共通ヘルパーに集約
            if (IsWASAPI || IsWASAPIex)
            {
                HandleTimerReloadForOutput(wasapiOut);
            }
            else if (IsASIO)
            {
                HandleTimerReloadForOutput(asioOut);
            }
            else
            {
                HandleTimerReloadForOutput(wo);
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
            SLTAlive = true;

            StartPlaybackThread_s = new(StartPlaybackThread);
            Playback_s = new(StartPlaybackThread_s)
            {
                Name = IsWASAPI || IsWASAPIex
                    ? "WASAPIOut"
                    : IsASIO
                        ? "ASIOOut"
                        : "waveOut",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };

            Playback_s.SetApartmentState(ApartmentState.STA);
            Playback_s.Start();
        }

        private void StartPlaybackThread()
        {
            try
            {
                while (SLTAlive)
                {
                    // 再生が止まっていたら監視スレッドも終了
                    PlaybackState state;

                    if (IsWASAPI || IsWASAPIex)
                    {
                        state = wasapiOut?.PlaybackState ?? PlaybackState.Stopped;
                    }
                    else if (IsASIO)
                    {
                        state = asioOut?.PlaybackState ?? PlaybackState.Stopped;
                    }
                    else
                    {
                        state = wo.PlaybackState;
                    }

                    if (state == PlaybackState.Stopped)
                    {
                        break;
                    }

                    // 再生位置・サンプル数を更新
                    position = (int)(reader.Position / (long)reader.WaveFormat.AverageBytesPerSecond);
                    time = new TimeSpan(0, 0, position);
                    Sample = reader.Position / reader.BlockAlign;

                    // CPU を休ませる（10〜20ms くらいなら十分）
                    Thread.Sleep(10);
                }
            }
            catch (ObjectDisposedException)
            {
                // フォームクローズ中に Dispose された場合の保険
            }
            catch
            {
                // 必要ならここでログ出力など
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
            if (pos == 0)
            {
                MessageBox.Show(this, "You cannot set the LoopStart value to zero.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Start = pos;

            if (!Generic.lpcreatev2)
            {
                // ★ Generic / FormMain 側の更新はコントローラに丸投げ
                LoopPointController.UpdateLoopStart(pos, btnpos);
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
                // ★ Generic / FormMain 側はコントローラに任せる
                LoopPointController.UpdateLoopEnd(pos, btnpos);

                // ラベルを消すロジックだけ LPC 内に残す（UI の話なので）
                if (Generic.IsOpenMulti)
                {
                    if (Generic.MultipleLoopStarts[btnpos - 1] == 0)
                    {
                        label_LoopEndSamples.Text = string.Empty;
                    }
                }
                else
                {
                    if (Generic.MultipleLoopStarts[0] == 0)
                    {
                        label_LoopEndSamples.Text = string.Empty;
                    }
                }
            }
        }


        private void FormLPC_FormClosed(object sender, FormClosedEventArgs e)
        {
            SLTAlive = false;

            if (timer_Reload.Enabled)
                timer_Reload.Enabled = false;

            // 読み取り位置を先頭に戻しておく
            if (reader is not null)
            {
                reader.Position = 0;
            }

            if (wi is not null)
            {
                wi.StopRecording();
                wi.Dispose();
            }

            if (wo is not null)
            {
                if (wo.PlaybackState != PlaybackState.Stopped)
                    wo.Stop();
                wo.Dispose();
            }

            if (wasapiOut is not null)
            {
                if (wasapiOut.PlaybackState != PlaybackState.Stopped)
                    wasapiOut.Stop();
                wasapiOut.Dispose();
            }

            if (asioOut is not null)
            {
                if (asioOut.PlaybackState != PlaybackState.Stopped)
                    asioOut.Stop();
                asioOut.Dispose();
            }

            // マルチチャンネル時に保持している追加の IDisposable を解放
            if (_disposables is not null && _disposables.Count > 0)
            {
                foreach (var d in _disposables)
                {
                    d?.Dispose();
                }
                _disposables.Clear();
            }

            // WASAPI のデバイスも解放
            mmDevice?.Dispose();
            mmDevice = null;

            // WaveFileReader を確実に解放
            if (reader is not null)
            {
                reader.Close();
                reader.Dispose();
                reader = null!;
            }

            FormMain.DebugInfo("[FormLPC] Closed.");
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
            FormMain.DebugInfo("Prev btnpos: " + string.Format("{0}", btnpos));
            FormMain.DebugInfo("MultipleFilesLoopOKFlags[]: " + string.Join(", ", Generic.MultipleFilesLoopOKFlags));
            FormMain.DebugInfo("MultipleLoopStarts[]: " + string.Join(", ", Generic.MultipleLoopStarts));
            FormMain.DebugInfo("MultipleLoopEnds[]: " + string.Join(", ", Generic.MultipleLoopEnds));

            // ▼ ループ警告ロジック：Generic 直読み → LoopPointController 経由に
            if (Generic.IsLoopWarning && Generic.IsOpenMulti && checkBox_LoopEnable.Checked)
            {
                var (start, end, ok) = LoopPointController.GetLoopState(btnpos + 1);

                if (!ok && (start == 0 || end == 0))
                {
                    DialogResult dr = MessageBox.Show(
                        Localization.LoopWarningCaption,
                        Localization.MSGBoxWarningCaption,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

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
                label_File.Text = Path.GetFileNameWithoutExtension(Generic.InputJobs[(int)btnpos - 1].OriginPath) + GetCurrentReaderBitAndHzFromLabel(reader);
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
                label_File.Text = Path.GetFileNameWithoutExtension(Generic.InputJobs[(int)btnpos - 1].OriginPath) + GetCurrentReaderBitAndHzFromLabel(reader);
                FormMain.FormMainInstance.FPLabel = fiorig.Directory + @"\" + fiorig.Name;
                FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, FS / 1024, FS);
                button_Prev.Enabled = true;
                button_Next.Enabled = true;
            }

            // ATRAC バッファに由来するループ情報は従来通り
            SetLoopPointsWithATRACBuffer(reader.WaveFormat.SampleRate, btnpos - 1);

            // ▼ MultipleFiles 用ループ表示処理は共通ヘルパーに置き換え
            ApplyLoopStateFromGeneric();

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
            FormMain.DebugInfo("Next btnpos: " + string.Format("{0}", btnpos));
            FormMain.DebugInfo("MultipleFilesLoopOKFlags[]: " + string.Join(", ", Generic.MultipleFilesLoopOKFlags));
            FormMain.DebugInfo("MultipleLoopStarts[]: " + string.Join(", ", Generic.MultipleLoopStarts));
            FormMain.DebugInfo("MultipleLoopEnds[]: " + string.Join(", ", Generic.MultipleLoopEnds));

            // ▼ 「今から離れる前のファイル」に対するループ警告
            if (Generic.IsLoopWarning && Generic.IsOpenMulti && checkBox_LoopEnable.Checked && btnpos != 1)
            {
                uint prevButton = (uint)(btnpos - 1);
                var (start, end, ok) = LoopPointController.GetLoopState(prevButton);

                if (!ok && (start == 0 || end == 0))
                {
                    DialogResult dr = MessageBox.Show(
                        Localization.LoopWarningCaption,
                        Localization.MSGBoxWarningCaption,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

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
                label_File.Text = Path.GetFileNameWithoutExtension(Generic.InputJobs[(int)btnpos - 1].OriginPath) + GetCurrentReaderBitAndHzFromLabel(reader);
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
                label_File.Text = Path.GetFileNameWithoutExtension(Generic.InputJobs[(int)btnpos - 1].OriginPath) + GetCurrentReaderBitAndHzFromLabel(reader);
                FormMain.FormMainInstance.FPLabel = fiorig.Directory + @"\" + fiorig.Name;
                FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, FS / 1024, FS);
                button_Next.Enabled = true;
                button_Prev.Enabled = true;
            }

            // ATRAC 由来のループは従来通り
            SetLoopPointsWithATRACBuffer(reader.WaveFormat.SampleRate, btnpos - 1);

            // MultipleFiles 用ループ表示は共通処理に任せる
            ApplyLoopStateFromGeneric();

            smplrate = reader.WaveFormat.SampleRate;
            totalsamples = reader.SampleCount;
            Generic.IsLPCStreamingReloaded = false;
        }

        /// <summary>
        /// 現在の btnpos / Generic のループ状態を LPC の UI に反映する。
        /// </summary>
        private void ApplyLoopStateFromGeneric()
        {
            // 現在のボタン（ファイル）のループ状態を取得
            var (startSamples, endSamples, isLoopOk) = LoopPointController.GetLoopState(btnpos);

            // 一旦ラベルはクリア
            label_LoopStartSamples.Text = string.Empty;
            label_LoopEndSamples.Text = string.Empty;

            // ループ未設定ならチェックを外して UI を初期化
            if (!isLoopOk || (startSamples == 0 && endSamples == 0))
            {
                int tb = GetSafeHalfDurationMillisecondsForLoopEnd();

                checkBox_LoopEnable.Checked = false;
                customTrackBar_Start.Value = 0;
                customTrackBar_End.Value = tb;
                numericUpDown_LoopStart.Value = 0;
                numericUpDown_LoopEnd.Value = tb;

                LoopPointController.SyncMainLoopTextFromGeneric(btnpos);
                return;
            }

            // サンプル数 → ミリ秒に変換（元の計算式に合わせる）
            int sampleRate = reader.WaveFormat.SampleRate;

            if (startSamples > 0)
            {
                Start = startSamples;
                int startMs = (int)(startSamples * 1000L / sampleRate);

                startMs = Math.Clamp(startMs, customTrackBar_Start.Minimum, customTrackBar_Start.Maximum);
                customTrackBar_Start.Value = startMs;

                decimal nudStart = Math.Clamp(startMs,
                    (int)numericUpDown_LoopStart.Minimum,
                    (int)numericUpDown_LoopStart.Maximum);
                numericUpDown_LoopStart.Value = nudStart;

                label_LoopStartSamples.Text = $"LoopStart: {startSamples} {Localization.SampleCaption}";
            }
            else
            {
                Start = 0;
                customTrackBar_Start.Value = 0;
                numericUpDown_LoopStart.Value = 0;
            }

            if (endSamples > 0)
            {
                End = endSamples;
                int endMs = (int)(endSamples * 1000L / sampleRate);

                endMs = Math.Clamp(endMs, customTrackBar_End.Minimum, customTrackBar_End.Maximum);
                customTrackBar_End.Value = endMs;

                decimal nudEnd = Math.Clamp(endMs,
                    (int)numericUpDown_LoopEnd.Minimum,
                    (int)numericUpDown_LoopEnd.Maximum);
                numericUpDown_LoopEnd.Value = nudEnd;

                label_LoopEndSamples.Text = $"LoopEnd: {endSamples} {Localization.SampleCaption}";
            }
            else
            {
                End = 0;
                customTrackBar_End.Value = 0;
                numericUpDown_LoopEnd.Value = 0;
            }

            // メインフォーム側のテキストボックスも Generic から同期
            LoopPointController.SyncMainLoopTextFromGeneric(btnpos);

            // ループ OK ならチェック ON & UI 有効化
            checkBox_LoopEnable.Checked = true;
            //EnableLoopUiControls();
            if (!IsPlaybackATRAC && Generic.IsATRAC)
            {
                EnableLoopUiControls();
            }
            else if (!Generic.IsATRAC)
            {
                EnableLoopUiControls();
            }
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
                if (checkBox_LoopEnable.Checked) // 有効化
                {
                    // --- 既存の競合チェック（AT3/AT9, LPC_CREATE） ---
                    if (Generic.IsAT3LoopSound || Generic.IsAT3LoopPoint)
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            MessageBox.Show(this,
                                Localization.AT3AT9LoopBeginToEndAlreadyEnabledWarning,
                                Localization.MSGBoxWarningCaption,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            ResetLoopEnable();
                            return;
                        }
                        else
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC3 / ATRAC3+")
                            {
                                MessageBox.Show(this,
                                    Localization.AT3LoopBeginToEndAlreadyEnabledWarning,
                                    Localization.MSGBoxWarningCaption,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
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
                                MessageBox.Show(this,
                                    Localization.AT9LoopBeginToEndAlreadyEnabledWarning,
                                    Localization.MSGBoxWarningCaption,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                ResetLoopEnable();
                                return;
                            }
                        }
                    }

                    if (!Generic.lpcreatev2 && Generic.lpcreate != false)
                    {
                        MessageBox.Show(this,
                            Localization.LPCreateAlreadyEnableWarning,
                            Localization.MSGBoxWarningCaption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        ResetLoopEnable();
                        return;
                    }

                    // --- Main 側のループ UI を有効化 ---
                    if (!Generic.lpcreatev2)
                    {
                        LoopPointController.EnableMainLoopUi();
                    }

                    // --- LPC 側コントロールをまとめて ON ---
                    EnableLoopUiControls();
                }
                else // 無効化
                {
                    // --- 既存の「ループ消してもいい？」警告ロジック ---
                    if (Generic.IsAT3LoopSound || Generic.IsAT3LoopPoint)
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            // 何もしない（従来どおり空ブロック）
                        }
                        else
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC3 / ATRAC3+")
                            {
                                // 何もしない（従来どおり空ブロック）
                            }
                            else
                            {
                                if (Generic.IsLoopWarning &&
                                    Generic.MultipleFilesLoopOKFlags[0] &&
                                    FormMain.FormMainInstance.button_Encode.Enabled)
                                {
                                    DialogResult dr = MessageBox.Show(
                                        Localization.LoopingAlreadySetWarningCaption,
                                        Localization.MSGBoxWarningCaption,
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning);
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
                                // 何もしない（従来どおり空ブロック）
                            }
                            else
                            {
                                if (Generic.IsLoopWarning &&
                                    Generic.MultipleFilesLoopOKFlags[0] &&
                                    FormMain.FormMainInstance.button_Encode.Enabled)
                                {
                                    DialogResult dr = MessageBox.Show(
                                        Localization.LoopingAlreadySetWarningCaption,
                                        Localization.MSGBoxWarningCaption,
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning);
                                    if (dr == DialogResult.No)
                                    {
                                        checkBox_LoopEnable.Checked = true;
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    // --- フラグのリセットはコントローラに任せる ---
                    LoopPointController.ResetLoop(0);

                    // --- Main 側 UI を OFF ---
                    if (!Generic.lpcreatev2)
                    {
                        LoopPointController.DisableMainLoopUi();
                    }

                    // --- LPC 側 UI を OFF ---
                    DisableLoopUiControls();

                    // ラベルは空にする
                    label_LoopStartSamples.Text = string.Empty;
                    label_LoopEndSamples.Text = string.Empty;
                }
            }
            else // Multiple
            {
                if (checkBox_LoopEnable.Checked) // 有効化
                {
                    if (!Generic.lpcreatev2 && Generic.lpcreate != false)
                    {
                        MessageBox.Show(this,
                            Localization.LPCreateAlreadyEnableWarning,
                            Localization.MSGBoxWarningCaption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        ResetLoopEnable();
                        return;
                    }

                    if (Generic.IsAT3LoopSound || Generic.IsAT3LoopPoint)
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            MessageBox.Show(this,
                                Localization.AT3AT9LoopBeginToEndAlreadyEnabledWarning,
                                Localization.MSGBoxWarningCaption,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            ResetLoopEnable();
                            return;
                        }
                        else
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC3 / ATRAC3+")
                            {
                                MessageBox.Show(this,
                                    Localization.AT3LoopBeginToEndAlreadyEnabledWarning,
                                    Localization.MSGBoxWarningCaption,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
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
                                MessageBox.Show(this,
                                    Localization.AT9LoopBeginToEndAlreadyEnabledWarning,
                                    Localization.MSGBoxWarningCaption,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                ResetLoopEnable();
                                return;
                            }
                        }
                    }



                    // --- LPC 側コントロール ON ---
                    if (!IsPlaybackATRAC && Generic.IsATRAC)
                    {
                        if (!Generic.lpcreatev2)
                        {
                            LoopPointController.EnableMainLoopUi();
                        }
                        EnableLoopUiControls();
                    }
                    else if (!Generic.IsATRAC)
                    {
                        if (!Generic.lpcreatev2)
                        {
                            LoopPointController.EnableMainLoopUi();
                        }
                        EnableLoopUiControls();
                    }

                }
                else // 無効化
                {
                    // 既存の「ループ消していい？」警告
                    if (Generic.IsAT3LoopSound || Generic.IsAT3LoopPoint)
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            // 何もしない
                        }
                        else
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC3 / ATRAC3+")
                            {
                                // 何もしない
                            }
                            else
                            {
                                if (Generic.IsLoopWarning &&
                                    Generic.MultipleFilesLoopOKFlags[ButtonPosition - 1] &&
                                    FormMain.FormMainInstance.button_Encode.Enabled)
                                {
                                    DialogResult dr = MessageBox.Show(
                                        Localization.LoopingAlreadySetWarningCaption,
                                        Localization.MSGBoxWarningCaption,
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning);
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
                                // 何もしない
                            }
                            else
                            {
                                if (Generic.IsLoopWarning &&
                                    Generic.MultipleFilesLoopOKFlags[ButtonPosition - 1] &&
                                    FormMain.FormMainInstance.button_Encode.Enabled)
                                {
                                    DialogResult dr = MessageBox.Show(
                                        Localization.LoopingAlreadySetWarningCaption,
                                        Localization.MSGBoxWarningCaption,
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning);
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
                        // このファイルのループ設定をリセット
                        LoopPointController.ResetLoop(ButtonPosition);

                        LoopPointController.DisableMainLoopUi();
                    }
                    else
                    {
                        // --- フラグのリセットはコントローラに任せる ---
                        LoopPointController.ResetLoop(0);
                    }

                    // LPC 側 UI を OFF
                    DisableLoopUiControls();

                    label_LoopStartSamples.Text = string.Empty;
                    label_LoopEndSamples.Text = string.Empty;
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
                }
                else
                {
                    Generic.LPCSuffix = " -loop " + Start.ToString() + " " + End.ToString();
                }

                smplrate = reader.WaveFormat.SampleRate;
                if (Generic.lpcreatev2)
                {
                    checkBox_LoopEnable.Checked = false;
                    customTrackBar_Start.Enabled = false;
                    customTrackBar_End.Enabled = false;
                    label_start.Enabled = false;
                    label_end.Enabled = false;
                }
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

        private void EnableLoopUiControls()
        {
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

        // ループ関連コントロールを無効化
        private void DisableLoopUiControls()
        {
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

            label_start.Enabled = false;
            label_end.Enabled = false;
        }

        private void ResetLoopEnable()
        {
            checkBox_LoopEnable.Checked = false;

            // LPC 側コントロールをまとめて OFF
            DisableLoopUiControls();

            // ラベルは空にする
            label_LoopStartSamples.Text = string.Empty;
            label_LoopEndSamples.Text = string.Empty;

            // ★ Main 側の Loop UI はコントローラに任せる
            LoopPointController.DisableMainLoopUi();
        }

        private void ResetAFR()
        {
            int maxMs = GetSafeDurationMilliseconds();

            customTrackBar_Trk.Minimum = 0;
            customTrackBar_Trk.Maximum = maxMs;
            customTrackBar_Start.Minimum = 0;
            customTrackBar_Start.Maximum = maxMs;
            customTrackBar_End.Minimum = 0;
            customTrackBar_End.Maximum = maxMs;
            customTrackBar_Trk.TickFrequency = 1000;
            customTrackBar_Start.TickFrequency = 1000;
            customTrackBar_End.TickFrequency = 1000;
            numericUpDown_LoopStart.Minimum = 0;
            numericUpDown_LoopStart.Maximum = maxMs;
            numericUpDown_LoopStart.Increment = 1;
            numericUpDown_LoopEnd.Minimum = 0;
            numericUpDown_LoopEnd.Maximum = maxMs;
            numericUpDown_LoopEnd.Increment = 1;
            wo.Volume = volumeSlider1.Volume;

            int tb = GetSafeHalfDurationMillisecondsForLoopEnd();
            numericUpDown_LoopStart.Value = 0;
            numericUpDown_LoopEnd.Value = tb;

            Generic.LPCTotalSamples = reader.SampleCount;

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
                        // ループ UI を共通メソッドで有効化
                        EnableLoopUiControls();
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

        private string GetCurrentReaderBitAndHzFromLabel(WaveFileReader reader)
        {
            if (reader is null)
            {
                return string.Empty;
            }

            return " [" + reader.WaveFormat.BitsPerSample + "-bit, " + reader.WaveFormat.SampleRate + "Hz]";
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
