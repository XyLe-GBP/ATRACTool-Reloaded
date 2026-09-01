using ATRACTool_Reloaded.Localizable;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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
        WaveFileReader reader = null!;
        BufferedWaveProvider BufwaveProvider = null!;
        VolumeSampleProvider volumeSmplProvider = null!;
        PanningSampleProvider panSmplProvider = null!;
        long Sample, Start = 0, End = 0;
        long? loopStartSampleDisplayOverride, loopEndSampleDisplayOverride;
        int bytePerSec, position, length, smplrate, WASAPILatency = 0, WASAPIexLatency = 0, UseThreads = 3;
        long totalsamples;
        uint btnpos;
        TimeSpan time;
        bool mouseDown = false, stopflag = false, IsPausedMoveTrackbar, SmoothSamples = false, IsPlaybackATRAC = false, IsEncodeSourceATRAC = false, IsPlaybackNus3Bank = false, IsMultiChannel = false, IsWASAPI = false, IsWASAPIex = false, IsASIO = false, UseParallel = false, applyingExternalLoopState = false;
        private volatile bool _isClosing;
        float ScaleWidthTrk = 0f, ScaleWidthStart = 0f, ScaleWidthEnd = 0f;
        Point MainDefaultPoint = new(15, 88), StartDefaultPoint = new(15, 160), EndDefaultPoint = new(15, 32);
        private const int TrackOverlayTextTopOffset = 17;
        private const int EndLabelFileGap = 2;
        private const int StartLabelXOffset = -5;
        private const int StartLabelYOffset = -1;
        private const int EndLabelXOffset = -3;
        private const int EndLabelYOffset = 5;

        Point labelTrk, labelStart, labelEnd;

        private volatile bool SLTAlive;
        private readonly object _playbackMonitorSync = new();
        private Thread? _playbackMonitorThread;

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
            ModernUI.ModernTheme.Apply(this);
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
            ConfigureTrackValueLabel();
            ConfigureLoopPointValueLabels();
            label_start.Text = BuildPositionText(0);
            label_end.Text = BuildPositionText(0);
            label_Samples.Text = Localization.SampleCaption + ":";
            label_Psamples.Text = "0";
            label_Length.Text = Localization.LengthCaption + ":";
            label_Plength.Text = "00:00:00";
            label_LoopStartSamples.Text = "";
            label_LoopEndSamples.Text = "";
            label_previewwarn.Text = "";
            timer_Reload.Interval = 15;
        }

        private bool IsNus3BankPlaybackActive()
        {
            return IsPlaybackNus3Bank &&
                Generic.IsNus3Bank &&
                Generic.IsPlaybackNus3Bank &&
                Generic.pATRACOpenFilePaths is { Length: > 0 };
        }

        private bool ShouldUseDecodedPreviewPaths()
        {
            return IsNus3BankPlaybackActive() ||
                (IsPlaybackATRAC && Generic.IsATRAC) ||
                (IsEncodeSourceATRAC && Generic.IsATRAC);
        }

        private string[] GetLpcPlaybackPaths()
        {
            return ShouldUseDecodedPreviewPaths()
                ? Generic.pATRACOpenFilePaths
                : Generic.OpenFilePaths;
        }

        private string[] GetLpcOriginPaths()
        {
            if (IsNus3BankPlaybackActive() && Generic.Nus3BankPlaybackOriginPaths is { Length: > 0 })
                return Generic.Nus3BankPlaybackOriginPaths;

            if ((IsPlaybackATRAC && Generic.IsATRAC) || (IsEncodeSourceATRAC && Generic.IsATRAC))
                return Generic.OpenFilePaths;

            return Generic.OriginOpenFilePaths;
        }

        private int GetLpcPathCount()
        {
            return GetLpcPlaybackPaths()?.Length ?? 0;
        }

        private static string GetIndexedPath(string[]? paths, int index, string fallback)
        {
            if (paths is not null && index >= 0 && index < paths.Length && !string.IsNullOrWhiteSpace(paths[index]))
                return paths[index];

            return fallback;
        }

        private string BuildLpcDisplayLabel(int index, WaveFileReader currentReader)
        {
            string originPath;
            if (IsNus3BankPlaybackActive())
            {
                originPath = GetIndexedPath(Generic.Nus3BankPlaybackOriginPaths, index, GetIndexedPath(Generic.pATRACOpenFilePaths, index, string.Empty));
            }
            else if (Generic.InputJobs is not null && index >= 0 && index < Generic.InputJobs.Count)
            {
                originPath = Generic.InputJobs[index].OriginPath;
            }
            else
            {
                originPath = GetIndexedPath(GetLpcOriginPaths(), index, GetIndexedPath(GetLpcPlaybackPaths(), index, string.Empty));
            }

            string name = string.IsNullOrWhiteSpace(originPath)
                ? string.Empty
                : Path.GetFileNameWithoutExtension(originPath);

            return name + GetCurrentReaderBitAndHzFromLabel(currentReader);
        }

        private static string GetNus3BankPlaybackSourcePath()
        {
            if (Generic.OpenFilePaths is null || Generic.OpenFilePaths.Length == 0)
                return string.Empty;

            foreach (string path in Generic.OpenFilePaths)
            {
                if (Nus3BankFile.HasNus3BankExtension(path))
                    return path;
            }

            return Generic.OpenFilePaths[0] ?? string.Empty;
        }

        private void UpdateMainFileLabelsForCurrentPlayback(FileInfo fallbackOrigin)
        {
            FileInfo displayFile = fallbackOrigin;

            if (IsNus3BankPlaybackActive())
            {
                string sourcePath = GetNus3BankPlaybackSourcePath();
                if (!string.IsNullOrWhiteSpace(sourcePath))
                    displayFile = new FileInfo(sourcePath);
            }

            long fileSize = displayFile.Exists
                ? displayFile.Length
                : fallbackOrigin.Exists
                    ? fallbackOrigin.Length
                    : 0;

            FormMain.FormMainInstance.FPLabel = displayFile.FullName;
            FormMain.FormMainInstance.FSLabel = string.Format(Localization.FileSizeCaption, fileSize / 1024, fileSize);
        }

        public string ExecuteDebugFunction(string functionName)
        {
            if (!FormMain.AreDebugFunctionsEnabled)
            {
                return "Debug functions are disabled.";
            }

            string normalized = (functionName ?? "status").Trim().ToLowerInvariant();
            return normalized switch
            {
                "" or "status" => BuildDebugStatus(),
                "stop-playback" => ExecuteDebugStopPlayback(),
                _ => $"Unknown FormLPC debug function: {functionName}"
            };
        }

        private string BuildDebugStatus()
        {
            string playbackState = IsWASAPI || IsWASAPIex
                ? wasapiOut?.PlaybackState.ToString() ?? "null"
                : IsASIO
                    ? asioOut?.PlaybackState.ToString() ?? "null"
                    : wo?.PlaybackState.ToString() ?? "null";

            return $"FormLPC: closing={_isClosing}, timer={timer_Reload.Enabled}, reader={(reader is null ? "null" : "ready")}, playback={playbackState}, loop={checkBox_LoopEnable.Checked}, sample={Sample}";
        }

        private string ExecuteDebugStopPlayback()
        {
            StopPlaybackLoop();
            try
            {
                if (IsWASAPI || IsWASAPIex)
                {
                    wasapiOut?.Stop();
                }
                else if (IsASIO)
                {
                    asioOut?.Stop();
                }
                else
                {
                    wo?.Stop();
                }
            }
            catch (ObjectDisposedException)
            {
            }

            return "FormLPC playback stopped.";
        }

        private void ConfigureTrackValueLabel()
        {
            labelTrk = new Point(
                label_trk.Left - customTrackBar_Trk.Left,
                label_trk.Top - customTrackBar_Trk.Top);
            MainDefaultPoint = labelTrk;
            label_trk.Visible = false;
            int overlayTop = Math.Max(0, labelTrk.Y - TrackOverlayTextTopOffset);
            customTrackBar_Trk.OverlayTextTop = overlayTop;
            customTrackBar_Trk.OverlayTextSize = new Size(
                label_trk.Width,
                Math.Max(label_trk.Height, customTrackBar_Trk.Height - overlayTop));
            customTrackBar_Trk.OverlayTextFont = label_trk.Font;
            customTrackBar_Trk.OverlayText = BuildPositionText(customTrackBar_Trk.Value);
        }

        private void ConfigureLoopPointValueLabels()
        {
            int labelEndBottom = label_end.Bottom;
            int labelHeight = Math.Max(label_start.Height, label_end.Height) * 2 + 4;
            label_start.Height = labelHeight;
            label_end.Height = labelHeight;
            label_end.Top = Math.Max(label_File.Bottom + EndLabelFileGap, labelEndBottom - label_end.Height);
            StartDefaultPoint = new Point(StartDefaultPoint.X, label_start.Top + StartLabelYOffset);
            EndDefaultPoint = new Point(EndDefaultPoint.X, label_end.Top + EndLabelYOffset);
            labelStart = StartDefaultPoint;
            labelEnd = EndDefaultPoint;
            label_start.TextAlign = ContentAlignment.MiddleCenter;
            label_end.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void UpdateLoopPointValueLabels()
        {
            SetTrackbarStart();
            SetTrackbarEnd();
            label_start.Text = BuildPositionText(customTrackBar_Start.Value, loopStartSampleDisplayOverride);
            label_end.Text = BuildPositionText(customTrackBar_End.Value, loopEndSampleDisplayOverride);
        }

        private string BuildPositionText(int milliseconds, long? exactSamples = null)
        {
            return $"{milliseconds}{Environment.NewLine}{exactSamples ?? MillisecondsToSamples(milliseconds)}";
        }

        private long MillisecondsToSamples(int milliseconds)
        {
            if (smplrate <= 0)
            {
                return 0;
            }

            long samples = milliseconds * (long)smplrate / 1000L;
            return totalsamples > 0
                ? Math.Clamp(samples, 0L, totalsamples)
                : Math.Max(0L, samples);
        }

        private void CustomTrackBar_End_Scroll(object? sender, EventArgs e)
        {
            loopEndSampleDisplayOverride = null;
            numericUpDown_LoopEnd.Value = customTrackBar_End.Value;
            UpdateLoopPointValueLabels();
        }

        private void CustomTrackBar_Start_Scroll(object? sender, EventArgs e)
        {
            loopStartSampleDisplayOverride = null;
            numericUpDown_LoopStart.Value = customTrackBar_Start.Value;
            UpdateLoopPointValueLabels();
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
            FormMain.DebugInfo("[FormLPC] Load started.");
            Config.Load(xmlpath);

            FormLPCInstance = this;

            // SmoothSamples
            SmoothSamples = Utils.GetBool("SmoothSamples", false);

            // ATRAC 再生可否
            IsPlaybackATRAC = Utils.GetBool("PlaybackATRAC", false);
            IsPlaybackNus3Bank = Utils.GetBool("PlaybackNus3Bank", true);

            // ATRAC をエンコードソースとして扱ぁE��
            IsEncodeSourceATRAC = Utils.GetBool("ATRACEncodeSource", false);
            FormMain.DebugInfo($"[FormLPC] Config loaded. smoothSamples={SmoothSamples}, playbackAtrac={IsPlaybackATRAC}, playbackNus3Bank={IsPlaybackNus3Bank}, encodeSourceAtrac={IsEncodeSourceATRAC}");

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

            // ASIO ドライバ名取征E
            string asioConfig = Utils.GetString("LPCUseASIODriver", string.Empty);
            if (string.IsNullOrWhiteSpace(asioConfig))
            {
                asioDriver = string.Empty;
                int lpcPlayback = Utils.GetInt("LPCPlaybackMethod", 0);
                int multiPlayback = Utils.GetInt("LPCMultipleStreamPlaybackMethod", 65535);

                if (lpcPlayback == 3 || multiPlayback == 2)
                {
                    FormMain.DebugWarn("[FormLPC] ASIO playback requested but no driver is configured. Falling back to WASAPI exclusive.");
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
            if (ShouldDisableLoopEnableForAtracEncodeSourceOnly())
            {
                checkBox_LoopEnable.Enabled = false;
            }

            if (IsNus3BankPlaybackActive())
            {
                FormMain.DebugInfo("[FormLPC] Loading NUS3BANK playback preview.");
                checkBox_LoopEnable.Checked = false;
                checkBox_LoopEnable.Enabled = false;
                radioButton_at3.Enabled = false;
                radioButton_at9.Enabled = false;
                DisableLoopUiControls();

                string[] paths = GetLpcPlaybackPaths();
                reader = new(paths[0]);
                label_File.Text = BuildLpcDisplayLabel(0, reader);
                button_Prev.Enabled = false;
                button_Next.Enabled = paths.Length > 1;
                if (paths.Length > 1)
                    btnpos = 1;
            }
            else if (IsPlaybackATRAC && Generic.IsATRAC) // ATRAC再生機�E有効
            {
                FormMain.DebugInfo($"[FormLPC] Loading ATRAC playback preview. files={Common.Generic.pATRACOpenFilePaths.Length}");
                checkBox_LoopEnable.Enabled = false;
                radioButton_at3.Enabled = false;
                radioButton_at9.Enabled = false;

                if (Common.Generic.pATRACOpenFilePaths.Length == 1) // 単一ファイル
                {
                    reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                    //FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = BuildLpcDisplayLabel(0, reader);
                    button_Prev.Enabled = false;
                    button_Next.Enabled = false;
                }
                else // 褁E��ファイル
                {
                    reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                    //FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = BuildLpcDisplayLabel(0, reader);
                    button_Prev.Enabled = false;
                    button_Next.Enabled = true;
                    btnpos = 1;
                }
            }
            else if (IsEncodeSourceATRAC && Generic.IsATRAC) // ATRACをエンコード用ソースとして読み込み
            {
                FormMain.DebugInfo($"[FormLPC] Loading ATRAC encode source preview. files={Common.Generic.pATRACOpenFilePaths.Length}");
                if (Common.Generic.pATRACOpenFilePaths.Length == 1) // 単一ファイル
                {
                    reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                    //FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = BuildLpcDisplayLabel(0, reader);
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
                    else // 褁E��ファイル
                    {
                        reader = new(Common.Generic.pATRACOpenFilePaths[0]);
                        //FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                        label_File.Text = BuildLpcDisplayLabel(0, reader);
                        button_Prev.Enabled = false;
                        button_Next.Enabled = true;
                        btnpos = 1;
                    }
                }
            }
            else // 通常ファイル
            {
                FormMain.DebugInfo($"[FormLPC] Loading normal preview. files={Common.Generic.OpenFilePaths.Length}");
                if (Common.Generic.OpenFilePaths.Length == 1) // 単一ファイル
                {
                    reader = new(Common.Generic.OpenFilePaths[0]);
                    //FileInfo fi = new(Common.Generic.OpenFilePaths[0]);
                    label_File.Text = BuildLpcDisplayLabel(0, reader);
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
                    else // 褁E��ファイル
                    {
                        reader = new(Common.Generic.OpenFilePaths[0]);
                        //FileInfo fi = new(Common.Generic.OpenFilePaths[0]);
                        label_File.Text = BuildLpcDisplayLabel(0, reader);
                        button_Prev.Enabled = false;
                        button_Next.Enabled = true;
                        btnpos = 1;
                    }
                }
            }

            if (!PlaybackInit())
            {
                FormMain.DebugError("[FormLPC] Playback initialization failed. Closing LPC form.");
                // 再生の初期化に失敗した場合�E、このフォーム自体も閉じめE
                Close();
                return;
            }

            _ = FormMain.FormMainInstance.Meta;

            Generic.IsLPCStreamingReloaded = false;

            if (!IsMultiChannel)
            {
                BufwaveProvider = new BufferedWaveProvider(reader.WaveFormat)
                {
                    BufferDuration = TimeSpan.FromMilliseconds(500) // バッファの長さを設宁E
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
            customTrackBar_Trk.OverlayText = string.Empty;
            label_start.Text = "";
            label_end.Text = "";

            int tb = GetSafeHalfDurationMillisecondsForLoopEnd();
            numericUpDown_LoopEnd.Value = tb;

            smplrate = reader.WaveFormat.SampleRate;
            totalsamples = reader.SampleCount;
            if (IsNus3BankPlaybackActive())
            {
                ApplyLoopStateFromGenericSilently();
            }
            else
            {
                SetLoopPointsWithATRACBuffer(reader.WaveFormat.SampleRate, 0);
            }

            Generic.LPCTotalSamples = reader.SampleCount;
            RefreshTrackbarVisuals();
            FormMain.DebugInfo($"[FormLPC] Load completed. file={label_File.Text}, channels={reader.WaveFormat.Channels}, sampleRate={reader.WaveFormat.SampleRate}, samples={reader.SampleCount}");

        }

        private IWaveProvider BuildOutputChain(WaveFileReader reader)
        {
            // まず�E「Extensible含む何でも」�E float(ISampleProvider)
            var floatSP = BuildFloatFromWaveFileReader(reader);

            // 音量（多ch対応！E
            //_sample = new SampleChannel(floatSP, true);
            ISampleProvider chain = floatSP;

            // パンは mono/stereo のみ�E�E.1/7.1 では無効にするのが無難�E�E
            if (reader.WaveFormat.Channels <= 2)
            {
                panSmplProvider = new PanningSampleProvider(chain);
                chain = panSmplProvider;
            }
            else
            {
                panSmplProvider = null!;
            }

            // 出力ドライバが要求すめEIWaveProvider へ
            return chain.ToWaveProvider();
        }

        // 「WaveFileReader から忁E�� 32-bit float(ISampleProvider) を得る」�EルチE
        private static ISampleProvider BuildFloatFromWaveFileReader(WaveFileReader reader)
        {
            var wf = reader.WaveFormat;

            // 1) すでに float
            if (wf.Encoding == WaveFormatEncoding.IeeeFloat)
                return reader.ToSampleProvider();

            // 2) Extensible でめE16/24/32bit PCM なめE“シム EↁEそ�Eビット深度→float
            if (wf.Encoding == WaveFormatEncoding.Extensible)
            {
                IWaveProvider shim = new ExtensiblePcmShim(reader);
                return wf.BitsPerSample switch
                {
                    16 => new Wave16ToFloatProvider(shim).ToSampleProvider(),
                    24 => new WaveToSampleProvider(shim),// これ自体が ISampleProvider(float)
                    32 => new WaveToSampleProvider(shim),// 同丁E
                    _ => throw new NotSupportedException($"Extensible {wf.BitsPerSample}bit is not supported."),
                };
            }

            // 3) 素の PCM�E�非 Extensible�E�E
            if (wf.Encoding == WaveFormatEncoding.Pcm)
            {
                return wf.BitsPerSample switch
                {
                    16 => new Wave16ToFloatProvider(reader).ToSampleProvider(),
                    24 => new WaveToSampleProvider(reader),
                    32 => new WaveToSampleProvider(reader),
                    _ => throw new NotSupportedException($"PCM {wf.BitsPerSample}bit is not supported."),
                };
            }

            // 4) それ以外！EDPCM / μ-law 等）�E WaveFileReader ではなぁEMediaFoundationReader を使ぁE
            throw new NotSupportedException("This WAV uses a compressed codec. Please use MediaFoundationReader instead of WaveFileReader.");
        }

        // 再生監視スレチE��とタイマ�Eを停止するヘルパ�E
        private void StopPlaybackLoop()
        {
            // Playback() ↁEStartPlaybackThread() が回してぁE��ループ�E終亁E��リガ
            SLTAlive = false;

            // 進行状況更新用タイマ�Eも止めておく
            try
            {
                if (timer_Reload.Enabled)
                {
                    timer_Reload.Stop();
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        /// <summary>
        /// WASAPI(共朁E排仁Eの初期化を行う共通�Eルパ�E、E
        /// 排他モードでフォーマット未対応�E場合�E共有モードにフォールバックする、E
        /// </summary>
        private bool TryInitWasapi(IWaveProvider provider)
        {
            FormMain.DebugInfo($"[FormLPC] WASAPI initialization started. exclusive={IsWASAPIex}, shared={IsWASAPI}");
            using (var enumerator = new MMDeviceEnumerator())
            {
                mmDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }

            // まず�E現在の設定（�E朁Eor 排他）で試ぁE
            try
            {
                wasapiOut = new WasapiOut(
                    mmDevice,
                    IsWASAPIex ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared,
                    false,
                    IsWASAPIex ? WASAPIexLatency : WASAPILatency);

                wasapiOut.Init(provider);
                FormMain.DebugInfo($"[FormLPC] WASAPI initialization completed. shareMode={(IsWASAPIex ? "Exclusive" : "Shared")}");
                return true;
            }
            catch (COMException ex) when (IsWASAPIex && ex.HResult == unchecked((int)0x8889000A))
            {
                FormMain.DebugWarn($"[FormLPC] WASAPI exclusive format unsupported. Falling back to shared. hresult=0x{ex.HResult:X8}");
                // 排他モードでフォーマット未対応�E典型パターン (0x8889000A)
                // ↁE共有モードにフォールバック
                MessageBox.Show(
                    this,
                    "This audio format cannot be played in WASAPI exclusive mode.\r\n" +
                    "Playback will use WASAPI shared mode.",
                    Localization.MSGBoxWarningCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                try
                {
                    // 排他�E共朁Eに刁E��替ぁE
                    IsWASAPI = true;
                    IsWASAPIex = false;

                    try { wasapiOut?.Dispose(); } catch (Exception disposeEx)
                    {
                        FormMain.DebugWarn($"[FormLPC] Failed to dispose unsupported WASAPI output. error={disposeEx.Message}");
                    }

                    wasapiOut = new WasapiOut(
                        mmDevice,
                        AudioClientShareMode.Shared,
                        false,
                        WASAPILatency);

                    wasapiOut.Init(provider);
                    FormMain.DebugInfo("[FormLPC] WASAPI shared fallback completed.");
                    return true;
                }
                catch (COMException sharedEx)
                {
                    FormMain.DebugError($"[FormLPC] WASAPI shared fallback failed. error={sharedEx}");
                    // 共有でもダメなら諦める
                    MessageBox.Show(
                        this,
                        "This audio format cannot be played with WASAPI.\r\n" +
                        "Please change the playback method setting.",
                        Localization.MSGBoxErrorCaption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    // ☁E再生監視スレチE���E�E��イマ�Eを即停止
                    StopPlaybackLoop();
                    DisposeReloadableAudioOutputs();

                    // ☁Eメインフォームを「起動直後�E状態」に戻ぁE
                    FormMain.FormMainInstance.ResetToInitialState();

                    // false を返すことで、FormLPC_Load 側の
                    // 「if (!PlaybackInit()) { Close(); }」が実行され、E
                    // こ�E LPC フォーム自体も閉じられます、E
                    return false;
                }
            }
            catch (COMException ex)
            {
                FormMain.DebugError($"[FormLPC] WASAPI initialization failed. error={ex}");
                // そ�E他�E WASAPI 初期化エラー
                MessageBox.Show(
                    this,
                    "WASAPI の初期化に失敗しました、Er\n" + ex.Message,
                    Localization.MSGBoxErrorCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                // 念のため監視ループも止める
                StopPlaybackLoop();
                DisposeReloadableAudioOutputs();

                return false;
            }
        }

        /// <summary>
        /// 曲の長さ�E半�E�E�Es�E�を計算し、E
        /// TrackBar / NumericUpDown の上限を趁E��なぁE��ぁE��クランプした値を返します、E
        /// </summary>
        private int GetSafeHalfDurationMillisecondsForLoopEnd()
        {
            if (reader == null)
            {
                return 0;
            }

            // TotalMilliseconds は long で受ける（オーバ�Eフロー対策！E
            long halfMs = (long)(reader.TotalTime.TotalMilliseconds / 2.0);

            // TrackBar と NumericUpDown のどちらでも扱える篁E��に合わせる
            int tbMax = customTrackBar_End.Maximum;
            int nudMax = (int)numericUpDown_LoopEnd.Maximum;
            int max = Math.Min(tbMax, nudMax);

            if (max <= 0)
            {
                return 0;
            }

            // 0 �E�Emax の篁E��にクランプして int にキャスチE
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
        /// 曲の全長�E�Es�E�を安�Eに int に収めて返す、E
        /// TrackBar / NumericUpDown の Maximum に使ぁE��、E
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
                DisposeReloadableAudioOutputs();
                FormMain.DebugInfo($"[FormLPC] PlaybackInit started. channels={reader.WaveFormat.Channels}, sampleRate={reader.WaveFormat.SampleRate}, bits={reader.WaveFormat.BitsPerSample}, wasapi={IsWASAPI}, wasapiExclusive={IsWASAPIex}, asio={IsASIO}");
                switch (reader.WaveFormat.Channels)
                {
                    case 1: // Mono
                        IsMultiChannel = false;
                        if (IsWASAPI || IsWASAPIex)
                        {
                            var provider = new WaveChannel32(reader);
                            if (!TryInitWasapi(provider))
                            {
                                return false; // エラーを�EしてぁE��のでそ�Eまま抜けめE
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
                                return false; // エラーを�EしてぁE��のでそ�Eまま抜けめE
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
                DisposeReloadableAudioOutputs();
                FormMain.DebugError($"[FormLPC] PlaybackInit failed. error={Ex}");
                MessageBox.Show(this, string.Format(Localization.LPCUnsupportedFormatErrorCaption, Ex), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Generic.LPCException = true;
                return false;
            }
        }

        // 再生ボタンの動作を共通化するヘルパ�E
        private void HandlePlayButton(IWavePlayer output)
        {
            switch (output.PlaybackState)
            {
                case PlaybackState.Stopped:
                    FormMain.DebugInfo($"[FormLPC] Playback started. file={label_File.Text}, positionMs={customTrackBar_Trk.Value}");
                    // 再生時間めE��さ�E計算（従来と同じ�E�E
                    bytePerSec = reader.WaveFormat.BitsPerSample / 8
                                 * reader.WaveFormat.SampleRate
                                 * reader.WaveFormat.Channels;
                    length = (int)reader.Length / bytePerSec;

                    timer_Reload.Enabled = true;
                    output.Play();
                    button_Play.Text = Localization.PauseCaption;
                    StartPlaybackMonitor();
                    stopflag = false;
                    button_Stop.Enabled = true;
                    break;

                case PlaybackState.Paused:
                    try
                    {
                        if (IsPausedMoveTrackbar)
                        {
                            FormMain.DebugInfo($"[FormLPC] Playback resumed after seek. positionMs={customTrackBar_Trk.Value}");
                            // 一度止めて位置を移動してから再生し直ぁE
                            output.Stop();
                            reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Trk.Value);
                            output.Play();
                            StartPlaybackMonitor();
                            IsPausedMoveTrackbar = false;
                        }
                        else
                        {
                            FormMain.DebugInfo($"[FormLPC] Playback resumed. positionMs={customTrackBar_Trk.Value}");
                            output.Play();
                        }

                        button_Play.Text = Localization.PauseCaption;
                        break;
                    }
                    catch (NAudio.MmException)
                    {
                        FormMain.DebugWarn("[FormLPC] Playback resume failed with MME error.");
                        output.Stop();

                        button_Play.Text = "MME Detect.\r\nPlay Again";
                        break;
                    }

                case PlaybackState.Playing:
                    FormMain.DebugInfo($"[FormLPC] Playback paused. positionMs={customTrackBar_Trk.Value}");
                    output.Pause();
                    button_Play.Text = Localization.PlayCaption;
                    break;
            }
        }

        private void Button_Play_Click(object sender, EventArgs e)
        {
            if (IsWASAPI || IsWASAPIex)
            {
                HandlePlayButton(wasapiOut);
            }
            else if (IsASIO)
            {
                HandlePlayButton(asioOut);
            }
            else
            {
                HandlePlayButton(wo);
            }
        }

        private void HandleStopButton(IWavePlayer output)
        {
            if (output.PlaybackState != PlaybackState.Stopped)
            {
                FormMain.DebugInfo($"[FormLPC] Playback stopped. file={label_File.Text}, positionMs={customTrackBar_Trk.Value}");
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
        /// Timer_Reload_Tick から呼び出す、�E生終亁E���E生�E開�E共通�E琁E��E
        /// </summary>
        private void HandleTimerReloadForOutput(IWavePlayer output)
        {
            if (_isClosing || reader is null || output is null)
            {
                return;
            }

            // 再生が最後まで到達したら停止処琁E
            if (reader.CurrentTime == reader.TotalTime)
            {
                FormMain.DebugInfo($"[FormLPC] Playback reached end. file={label_File.Text}");
                stopflag = true;
                Sample = reader.SampleCount;

                output.Stop();
                button_Play.Text = Localization.PlayCaption;
                reader.Position = 0;
                button_Stop.Enabled = false;
                Resettrackbarlabels();
            }
            // 先頭付近まで戻ってぁE��、ユーザーぁEStop してぁE��ぁE��合�E再生し直ぁE
            else if (reader.Position == 0 || customTrackBar_Trk.Value == 0)
            {
                if (!stopflag)
                {
                    FormMain.DebugInfo($"[FormLPC] Playback restarted from beginning. file={label_File.Text}");
                    output.Stop();
                    button_Stop.Enabled = false;
                    Sample = 0;
                    reader.Position = 0;

                    output.Play();
                    button_Play.Text = Localization.PauseCaption;

                    // 監視スレチE��を�E度起動（従来どおり�E�E
                    StartPlaybackMonitor();
                    button_Stop.Enabled = true;
                }
            }
        }

        private void Timer_Reload_Tick(object? sender, EventArgs e)
        {
            if (_isClosing || reader is null || IsDisposed || Disposing)
            {
                StopPlaybackLoop();
                return;
            }

            try
            {
                if (!mouseDown) customTrackBar_Trk.Value = (int)reader.CurrentTime.TotalMilliseconds;
                if (checkBox_LoopEnable.Checked == true && reader.CurrentTime >= TimeSpan.FromMilliseconds(customTrackBar_End.Value))
                {
                    reader.CurrentTime = TimeSpan.FromMilliseconds(customTrackBar_Start.Value);
                    Sample = reader.Position / reader.BlockAlign;
                }

                // 出力デバイスごとの処琁E�E共通�Eルパ�Eに雁E��E
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
                UpdateLoopPointValueLabels();
                StringBuilder str = new(Sample.ToString());

                label_trk.Text = BuildPositionText(customTrackBar_Trk.Value);
                customTrackBar_Trk.OverlayText = label_trk.Text;
                label_Length.Text = Localization.LengthCaption + ":";
                label_Plength.Text = time.ToString(@"hh\:mm\:ss");

                label_Samples.Text = Localization.SampleCaption + ":";
                label_Psamples.Text = str.ToString();
            }
            catch (ObjectDisposedException)
            {
                StopPlaybackLoop();
            }
        }

        private void StartPlaybackMonitor()
        {
            lock (_playbackMonitorSync)
            {
                SLTAlive = true;
                if (_playbackMonitorThread is { IsAlive: true })
                    return;

                _playbackMonitorThread = new Thread(StartPlaybackThread)
                {
                    Name = IsWASAPI || IsWASAPIex
                        ? "WASAPIOutMonitor"
                        : IsASIO
                            ? "ASIOOutMonitor"
                            : "WaveOutMonitor",
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                };

                _playbackMonitorThread.Start();
            }
        }

        private void DisposeReloadableAudioOutputs()
        {
            if (wasapiOut is not null)
            {
                try
                {
                    if (wasapiOut.PlaybackState != PlaybackState.Stopped)
                        wasapiOut.Stop();
                }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[FormLPC] Failed to stop WASAPI output during cleanup. error={ex.Message}");
                }

                try { wasapiOut.Dispose(); }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[FormLPC] Failed to dispose WASAPI output. error={ex.Message}");
                }
                wasapiOut = null!;
            }

            if (asioOut is not null)
            {
                try
                {
                    if (asioOut.PlaybackState != PlaybackState.Stopped)
                        asioOut.Stop();
                }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[FormLPC] Failed to stop ASIO output during cleanup. error={ex.Message}");
                }

                try { asioOut.Dispose(); }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[FormLPC] Failed to dispose ASIO output. error={ex.Message}");
                }
                asioOut = null!;
            }

            try { mmDevice?.Dispose(); }
            catch (Exception ex)
            {
                FormMain.DebugWarn($"[FormLPC] Failed to dispose audio endpoint. error={ex.Message}");
            }
            mmDevice = null;
        }

        private void WaitForPlaybackMonitorExit()
        {
            Thread? monitor;
            lock (_playbackMonitorSync)
            {
                monitor = _playbackMonitorThread;
            }

            if (monitor is null || monitor == Thread.CurrentThread || !monitor.IsAlive)
                return;

            try
            {
                monitor.Join(250);
            }
            catch (ThreadStateException)
            {
            }
        }

        private void StartPlaybackThread()
        {
            try
            {
                while (SLTAlive)
                {
                    if (_isClosing || reader is null)
                    {
                        break;
                    }

                    // 再生が止まってぁE��ら監視スレチE��も終亁E
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

                    if (_isClosing || reader is null)
                    {
                        break;
                    }

                    // 再生位置・サンプル数を更新
                    position = (int)(reader.Position / (long)reader.WaveFormat.AverageBytesPerSecond);
                    time = new TimeSpan(0, 0, position);
                    Sample = reader.Position / reader.BlockAlign;

                    // CPU を休ませる�E�E0、E0ms くらぁE��ら十刁E��E
                    Thread.Sleep(10);
                }
            }
            catch (ObjectDisposedException)
            {
                // フォームクローズ中に Dispose された場合�E保険
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[FormLPC] Playback monitor failed. error={ex}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isClosing = true;
            StopPlaybackLoop();
            WaitForPlaybackMonitorExit();
            base.OnFormClosing(e);
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
                FormMain.DebugWarn("[FormLPC] Loop start rejected: zero sample.");
                MessageBox.Show(this, "You cannot set the LoopStart value to zero.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Start = pos;
            loopStartSampleDisplayOverride = pos;
            FormMain.DebugInfo($"[FormLPC] Loop start set. sample={Start}, buttonIndex={btnpos}");

            if (!Generic.lpcreatev2)
            {
                // ☁EGeneric / FormMain 側の更新はコントローラに丸投げ
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
            loopEndSampleDisplayOverride = pos;
            FormMain.DebugInfo($"[FormLPC] Loop end set. sample={End}, buttonIndex={btnpos}");

            if (!Generic.lpcreatev2)
            {
                // ☁EGeneric / FormMain 側はコントローラに任せる
                LoopPointController.UpdateLoopEnd(pos, btnpos);

                // ラベルを消すロジチE��だぁELPC 冁E��残す�E�EI の話なので�E�E
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
            _isClosing = true;
            StopPlaybackLoop();

            try
            {
                timer_Reload.Tick -= Timer_Reload_Tick;
            }
            catch (ObjectDisposedException)
            {
            }

            // 読み取り位置を�E頭に戻しておく
            try
            {
                if (reader is not null)
                {
                    reader.Position = 0;
                }
            }
            catch (ObjectDisposedException)
            {
            }

            if (wi is not null)
            {
                try { wi.StopRecording(); } catch (ObjectDisposedException) { }
                try { wi.Dispose(); } catch (ObjectDisposedException) { }
            }

            if (wo is not null)
            {
                try
                {
                    if (wo.PlaybackState != PlaybackState.Stopped)
                        wo.Stop();
                }
                catch (ObjectDisposedException) { }
                try { wo.Dispose(); } catch (ObjectDisposedException) { }
            }

            DisposeReloadableAudioOutputs();

            // WaveFileReader を確実に解放
            if (reader is not null)
            {
                try { reader.Dispose(); }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[FormLPC] Failed to dispose WAV reader. error={ex.Message}");
                }
                reader = null!;
            }

            FormMain.DebugInfo("[FormLPC] Closed.");
        }

        private void Button_Prev_Click(object sender, EventArgs e)
        {
            FormMain.DebugInfo($"[FormLPC] Previous file requested. currentButtonIndex={btnpos}");
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

            // ▼ ループ警告ロジチE���E�Generic 直読み ↁELoopPointController 経由に
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
                        FormMain.DebugWarn($"[FormLPC] Previous file cancelled by loop warning. buttonIndex={btnpos}");
                        if (btnpos != (uint)GetLpcPathCount())
                        {
                            btnpos++;
                        }

                        return;
                    }
                }
            }

            Generic.IsLPCStreamingReloaded = true;
            string[] Paths = GetLpcPlaybackPaths();
            string[] OriginPaths = GetLpcOriginPaths();
            int pathIndex = (int)btnpos - 1;

            FileInfo fi = new(Paths[pathIndex]);
            FileInfo fiorig = new(GetIndexedPath(OriginPaths, pathIndex, Paths[pathIndex]));

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
                reader = new(Paths[pathIndex]);
                if (!PlaybackInit())
                {
                    return;
                }
                ResetAFR();
                label_File.Text = BuildLpcDisplayLabel((int)btnpos - 1, reader);
                UpdateMainFileLabelsForCurrentPlayback(fiorig);
                button_Prev.Enabled = false;
                button_Next.Enabled = true;
            }
            else
            {
                reader = new(Paths[pathIndex]);
                if (!PlaybackInit())
                {
                    return;
                }
                ResetAFR();
                label_File.Text = BuildLpcDisplayLabel((int)btnpos - 1, reader);
                UpdateMainFileLabelsForCurrentPlayback(fiorig);
                button_Prev.Enabled = true;
                button_Next.Enabled = true;
            }

            // ATRAC バッファに由来するループ情報は従来通り
            SetLoopPointsWithATRACBuffer(reader.WaveFormat.SampleRate, btnpos - 1);

            // ▼ MultipleFiles 用ループ表示処琁E�E共通�Eルパ�Eに置き換ぁE
            ApplyLoopStateFromGenericSilently();

            smplrate = reader.WaveFormat.SampleRate;
            totalsamples = reader.SampleCount;
            Generic.IsLPCStreamingReloaded = false;
            RefreshTrackbarVisuals();
            FormMain.DebugInfo($"[FormLPC] Previous file loaded. buttonIndex={btnpos}, file={label_File.Text}");
        }

        private void Button_Next_Click(object sender, EventArgs e)
        {
            FormMain.DebugInfo($"[FormLPC] Next file requested. currentButtonIndex={btnpos}");
            btnpos++;

            if (btnpos == (uint)GetLpcPathCount() + 1)
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

            // ▼ 「今から離れる前�Eファイル」に対するループ警呁E
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
                        FormMain.DebugWarn($"[FormLPC] Next file cancelled by loop warning. buttonIndex={btnpos}");
                        if (btnpos != 1)
                        {
                            btnpos--;
                        }
                        return;
                    }
                }
            }

            Generic.IsLPCStreamingReloaded = true;
            string[] Paths = GetLpcPlaybackPaths();
            string[] OriginPaths = GetLpcOriginPaths();
            int pathIndex = (int)btnpos - 1;

            FileInfo fi = new(Paths[pathIndex]);
            FileInfo fiorig = new(GetIndexedPath(OriginPaths, pathIndex, Paths[pathIndex]));

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

            if (btnpos == (uint)Paths.Length)
            {
                reader = new(Paths[pathIndex]);
                if (!PlaybackInit())
                {
                    return;
                }
                ResetAFR();
                label_File.Text = BuildLpcDisplayLabel((int)btnpos - 1, reader);
                UpdateMainFileLabelsForCurrentPlayback(fiorig);
                button_Next.Enabled = false;
                button_Prev.Enabled = true;
            }
            else
            {
                reader = new(Paths[pathIndex]);
                if (!PlaybackInit())
                {
                    return;
                }
                ResetAFR();
                label_File.Text = BuildLpcDisplayLabel((int)btnpos - 1, reader);
                UpdateMainFileLabelsForCurrentPlayback(fiorig);
                button_Next.Enabled = true;
                button_Prev.Enabled = true;
            }

            // ATRAC 由来のループ�E従来通り
            SetLoopPointsWithATRACBuffer(reader.WaveFormat.SampleRate, btnpos - 1);

            // MultipleFiles 用ループ表示は共通�E琁E��任せる
            ApplyLoopStateFromGenericSilently();

            smplrate = reader.WaveFormat.SampleRate;
            totalsamples = reader.SampleCount;
            Generic.IsLPCStreamingReloaded = false;
            RefreshTrackbarVisuals();
            FormMain.DebugInfo($"[FormLPC] Next file loaded. buttonIndex={btnpos}, file={label_File.Text}");
        }

        private void ApplyLoopStateFromGenericSilently()
        {
            applyingExternalLoopState = true;
            try
            {
                ApplyLoopStateFromGeneric();
            }
            finally
            {
                applyingExternalLoopState = false;
            }
        }

        /// <summary>
        /// 現在の btnpos / Generic のループ状態を LPC の UI に反映する、E
        /// </summary>
        private void ApplyLoopStateFromGeneric()
        {
            // 現在のボタン�E�ファイル�E��Eループ状態を取征E
            var (startSamples, endSamples, isLoopOk) = LoopPointController.GetLoopState(btnpos);

            // 一旦ラベルはクリア
            label_LoopStartSamples.Text = string.Empty;
            label_LoopEndSamples.Text = string.Empty;

            // ループ未設定ならチェチE��を外して UI を�E期化
            if (!isLoopOk || (startSamples == 0 && endSamples == 0))
            {
                loopStartSampleDisplayOverride = null;
                loopEndSampleDisplayOverride = null;
                int tb = GetSafeHalfDurationMillisecondsForLoopEnd();

                checkBox_LoopEnable.Checked = false;
                customTrackBar_Start.Value = 0;
                customTrackBar_End.Value = tb;
                numericUpDown_LoopStart.Value = 0;
                numericUpDown_LoopEnd.Value = tb;

                LoopPointController.SyncMainLoopTextFromGeneric(btnpos);
                LockNus3BankPreviewLoopControls();
                return;
            }

            // サンプル数 ↁEミリ秒に変換�E��Eの計算式に合わせる�E�E
            int sampleRate = reader.WaveFormat.SampleRate;
            loopStartSampleDisplayOverride = startSamples > 0 ? startSamples : null;
            loopEndSampleDisplayOverride = endSamples > 0 ? endSamples : null;

            if (startSamples > 0)
            {
                Start = startSamples;
                int startMs = SamplesToMilliseconds(startSamples, sampleRate);

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
                int endMs = SamplesToMilliseconds(endSamples, sampleRate);

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

            // メインフォーム側のチE��スト�EチE��スめEGeneric から同期
            LoopPointController.SyncMainLoopTextFromGeneric(btnpos);

            // ルーチEOK ならチェチE�� ON & UI 有効匁E
            checkBox_LoopEnable.Checked = true;
            if (IsNus3BankPlaybackActive())
            {
                LockNus3BankPreviewLoopControls();
                return;
            }

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

        public void RefreshLoopStateFromGeneric()
        {
            if (IsDisposed || reader is null)
                return;

            applyingExternalLoopState = true;
            try
            {
                ApplyLoopStateFromGeneric();

                var (_, _, isLoopOk) = LoopPointController.GetLoopState(btnpos);
                if (isLoopOk)
                {
                    checkBox_LoopEnable.Enabled = true;
                    checkBox_LoopEnable.Checked = true;
                    if (IsNus3BankPlaybackActive())
                    {
                        LockNus3BankPreviewLoopControls();
                        return;
                    }

                    if (!Generic.lpcreatev2)
                        LoopPointController.EnableMainLoopUi();
                    EnableLoopUiControls();
                }
            }
            finally
            {
                applyingExternalLoopState = false;
            }
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            FormMain.DebugWarn("[FormLPC] Cancelled.");
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
            if (applyingExternalLoopState)
            {
                if (IsNus3BankPlaybackActive())
                {
                    LockNus3BankPreviewLoopControls();
                    return;
                }

                if (checkBox_LoopEnable.Checked)
                {
                    if (!Generic.lpcreatev2)
                        LoopPointController.EnableMainLoopUi();
                    EnableLoopUiControls();
                }
                else
                {
                    if (!Generic.lpcreatev2)
                        LoopPointController.DisableMainLoopUi();
                    DisableLoopUiControls();
                }

                return;
            }

            FormMain.DebugInfo($"[FormLPC] Loop enable changed. enabled={checkBox_LoopEnable.Checked}, multiple={Generic.IsOpenMulti}, buttonIndex={btnpos}");
            if (!Generic.IsOpenMulti) // Single
            {
                if (checkBox_LoopEnable.Checked) // 有効匁E
                {
                    // --- 既存�E競合チェチE���E�ET3/AT9, LPC_CREATE�E�E---
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

                    if (ShouldDisableLoopEnableForAtracEncodeSourceOnly())
                    {
                        checkBox_LoopEnable.Checked = false;
                        checkBox_LoopEnable.Enabled = false;
                        DisableLoopUiControls();
                        return;
                    }

                    if (ShouldShowLpCreateAlreadyEnabledWarning())
                    {
                        MessageBox.Show(this,
                            Localization.LPCreateAlreadyEnableWarning,
                            Localization.MSGBoxWarningCaption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        ResetLoopEnable();
                        return;
                    }

                    if (ShouldLockLoopEnableForAtracPreview())
                    {
                        checkBox_LoopEnable.Enabled = false;
                    }

                    // --- Main 側のルーチEUI を有効匁E---
                    if (!Generic.lpcreatev2)
                    {
                        LoopPointController.EnableMainLoopUi();
                    }

                    // --- LPC 側コントロールをまとめて ON ---
                    EnableLoopUiControls();
                }
                else // 無効匁E
                {
                    // --- 既存�E「ループ消してもいぁE��」警告ロジチE�� ---
                    if (Generic.IsAT3LoopSound || Generic.IsAT3LoopPoint)
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            // 何もしなぁE��従来どおり空ブロチE���E�E
                        }
                        else
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC3 / ATRAC3+")
                            {
                                // 何もしなぁE��従来どおり空ブロチE���E�E
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
                                // 何もしなぁE��従来どおり空ブロチE���E�E
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

                    // --- フラグのリセチE��はコントローラに任せる ---
                    LoopPointController.ResetLoop(0);

                    // --- Main 側 UI めEOFF ---
                    if (!Generic.lpcreatev2)
                    {
                        LoopPointController.DisableMainLoopUi();
                    }

                    // --- LPC 側 UI めEOFF ---
                    DisableLoopUiControls();

                    // ラベルは空にする
                    label_LoopStartSamples.Text = string.Empty;
                    label_LoopEndSamples.Text = string.Empty;
                }
            }
            else // Multiple
            {
                if (checkBox_LoopEnable.Checked) // 有効匁E
                {
                    if (ShouldDisableLoopEnableForAtracEncodeSourceOnly())
                    {
                        checkBox_LoopEnable.Checked = false;
                        checkBox_LoopEnable.Enabled = false;
                        DisableLoopUiControls();
                        return;
                    }

                    if (ShouldShowLpCreateAlreadyEnabledWarning())
                    {
                        MessageBox.Show(this,
                            Localization.LPCreateAlreadyEnableWarning,
                            Localization.MSGBoxWarningCaption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        ResetLoopEnable();
                        return;
                    }

                    if (ShouldLockLoopEnableForAtracPreview())
                    {
                        checkBox_LoopEnable.Enabled = false;
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
                else // 無効匁E
                {
                    // 既存�E「ループ消してぁE���E�」警呁E
                    if (Generic.IsAT3LoopSound || Generic.IsAT3LoopPoint)
                    {
                        if (Generic.IsAT9LoopSound || Generic.IsAT9LoopPoint)
                        {
                            // 何もしなぁE
                        }
                        else
                        {
                            if (FormMain.FormMainInstance.toolStripDropDownButton_EF.Text == "ATRAC3 / ATRAC3+")
                            {
                                // 何もしなぁE
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
                                // 何もしなぁE
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
                        // こ�Eファイルのループ設定をリセチE��
                        LoopPointController.ResetLoop(ButtonPosition);

                        LoopPointController.DisableMainLoopUi();
                    }
                    else
                    {
                        // --- フラグのリセチE��はコントローラに任せる ---
                        LoopPointController.ResetLoop(0);
                    }

                    // LPC 側 UI めEOFF
                    DisableLoopUiControls();

                    label_LoopStartSamples.Text = string.Empty;
                    label_LoopEndSamples.Text = string.Empty;
                }
            }
        }

        private void LockNus3BankPreviewLoopControls()
        {
            if (!IsNus3BankPlaybackActive())
                return;

            checkBox_LoopEnable.Enabled = false;
            DisableLoopUiControls();
            if (!Generic.lpcreatev2)
                LoopPointController.DisableMainLoopUi();
        }

        private bool ShouldShowLpCreateAlreadyEnabledWarning()
        {
            return !Generic.lpcreatev2 &&
                Generic.lpcreate &&
                !ShouldIgnoreLpCreateWarningForAtracPreview();
        }

        private bool ShouldIgnoreLpCreateWarningForAtracPreview()
        {
            return ShouldLockLoopEnableForAtracPreview();
        }

        private bool ShouldLockLoopEnableForAtracPreview()
        {
            return Generic.IsATRAC &&
                IsEncodeSourceATRAC &&
                IsPlaybackATRAC &&
                Generic.lpcreate;
        }

        private bool ShouldDisableLoopEnableForAtracEncodeSourceOnly()
        {
            return Generic.IsATRAC &&
                IsEncodeSourceATRAC &&
                !IsPlaybackATRAC &&
                Generic.lpcreate;
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            if (checkBox_LoopEnable.Checked == true)
            {
                if (Start == 0 || End == 0)
                {
                    FormMain.DebugWarn($"[FormLPC] OK blocked: loop point is not set. start={Start}, end={End}");
                    MessageBox.Show(this, Localization.LoopNotSetCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Start == End)
                {
                    FormMain.DebugWarn($"[FormLPC] OK blocked: loop start equals end. start={Start}, end={End}");
                    MessageBox.Show(this, "Incorrect loop value.\r\nLoop start and loop end values cannot be the same.", Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                double result = Math.Sign(End - Start);
                if (result == -1)
                {
                    FormMain.DebugWarn($"[FormLPC] OK blocked: loop end is before start. start={Start}, end={End}");
                    MessageBox.Show(this, "Incorrect loop value.\r\nNegative value between loop start and loop end values.", Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (result == 1)
                {
                    long loopdistancevalue = End - Start;
                    if (loopdistancevalue <= 1000 && loopdistancevalue >= 0)
                    {
                        FormMain.DebugWarn($"[FormLPC] OK blocked: loop interval too short. start={Start}, end={End}, interval={loopdistancevalue}");
                        MessageBox.Show(this, "Incorrect loop value.\r\nThe interval between the loop start and loop end values must be greater than or equal to 1000.", Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    FormMain.DebugWarn($"[FormLPC] OK blocked: invalid loop result. start={Start}, end={End}");
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
                FormMain.DebugInfo($"[FormLPC] OK. loopEnabled=true, start={Start}, end={End}, suffix={Generic.LPCSuffix}");
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
                FormMain.DebugInfo("[FormLPC] OK. loopEnabled=false");
                Close();
            }
        }

        private void NumericUpDown_LoopStart_ValueChanged(object sender, EventArgs e)
        {
            if (!applyingExternalLoopState)
                loopStartSampleDisplayOverride = null;
            customTrackBar_Start.Value = (int)numericUpDown_LoopStart.Value;
            UpdateLoopPointValueLabels();
        }

        private void NumericUpDown_LoopEnd_ValueChanged(object sender, EventArgs e)
        {
            if (!applyingExternalLoopState)
                loopEndSampleDisplayOverride = null;
            customTrackBar_End.Value = (int)numericUpDown_LoopEnd.Value;
            UpdateLoopPointValueLabels();
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

        // ループ関連コントロールを無効匁E
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

            // ☁EMain 側の Loop UI はコントローラに任せる
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

        private void RefreshTrackbarVisuals()
        {
            Resettrackbarlabels();
            UpdateLoopPointValueLabels();
            customTrackBar_Trk.Invalidate();
            customTrackBar_Start.Invalidate();
            customTrackBar_End.Invalidate();
        }

        private void SetTrackbarTrack()
        {
            customTrackBar_Trk.Invalidate();
        }

        private void SetTrackbarStart()
        {
            if (customTrackBar_Start.Value < customTrackBar_Start.Maximum / 2)
            {
                label_start.Location = new Point(labelStart.X + (int)((customTrackBar_Start.Value - customTrackBar_Start.Minimum) * ScaleWidthStart) - customTrackBar_Start.Location.X - labelStart.X + 9 + StartLabelXOffset, labelStart.Y);
            }
            else if (customTrackBar_Start.Value > customTrackBar_Start.Maximum / 2)
            {
                label_start.Location = new Point(labelStart.X + (int)((customTrackBar_Start.Value - customTrackBar_Start.Minimum) * ScaleWidthStart) - customTrackBar_Start.Location.X - labelStart.X - 9 + StartLabelXOffset, labelStart.Y);
            }
            else if (customTrackBar_Start.Value == customTrackBar_Start.Maximum / 2)
            {
                label_start.Location = new Point(labelStart.X + (int)((customTrackBar_Start.Value - customTrackBar_Start.Minimum) * ScaleWidthStart) - customTrackBar_Start.Location.X - labelStart.X + StartLabelXOffset, labelStart.Y);
            }
            else
            {
                label_start.Location = new Point(labelStart.X + (int)((customTrackBar_Start.Value - customTrackBar_Start.Minimum) * ScaleWidthStart) - customTrackBar_Start.Location.X - labelStart.X + StartLabelXOffset, labelStart.Y);
            }
        }

        private void SetTrackbarEnd()
        {
            if (customTrackBar_End.Value < customTrackBar_End.Maximum / 2)
            {
                label_end.Location = new Point(labelEnd.X + (int)((customTrackBar_End.Value - customTrackBar_End.Minimum) * ScaleWidthEnd) - customTrackBar_End.Location.X - labelEnd.X + 9 + EndLabelXOffset, labelEnd.Y);
            }
            else if (customTrackBar_End.Value > customTrackBar_End.Maximum / 2)
            {
                label_end.Location = new Point(labelEnd.X + (int)((customTrackBar_End.Value - customTrackBar_End.Minimum) * ScaleWidthEnd) - customTrackBar_End.Location.X - labelEnd.X - 9 + EndLabelXOffset, labelEnd.Y);
            }
            else if (customTrackBar_End.Value == customTrackBar_End.Maximum / 2)
            {
                label_end.Location = new Point(labelEnd.X + (int)((customTrackBar_End.Value - customTrackBar_End.Minimum) * ScaleWidthEnd) - customTrackBar_End.Location.X - labelEnd.X + EndLabelXOffset, labelEnd.Y);
            }
            else
            {
                label_end.Location = new Point(labelEnd.X + (int)((customTrackBar_End.Value - customTrackBar_End.Minimum) * ScaleWidthEnd) - customTrackBar_End.Location.X - labelEnd.X + EndLabelXOffset, labelEnd.Y);
            }
        }

        private static int ClampTrackBarValue(src.Controls.CustomTrackBar trackBar, int value)
        {
            if (value < trackBar.Minimum)
                return trackBar.Minimum;

            if (value > trackBar.Maximum)
                return trackBar.Maximum;

            return value;
        }

        private static decimal ClampNumericUpDownValue(NumericUpDown numericUpDown, int value)
        {
            decimal decimalValue = value;

            if (decimalValue < numericUpDown.Minimum)
                return numericUpDown.Minimum;

            if (decimalValue > numericUpDown.Maximum)
                return numericUpDown.Maximum;

            return decimalValue;
        }

        public void SetLoopStartMilliseconds(int startMs)
        {
            startMs = ClampTrackBarValue(customTrackBar_Start, startMs);
            customTrackBar_Start.Value = startMs;
            numericUpDown_LoopStart.Value = ClampNumericUpDownValue(numericUpDown_LoopStart, startMs);
        }

        public void SetLoopEndMilliseconds(int endMs)
        {
            endMs = ClampTrackBarValue(customTrackBar_End, endMs);
            customTrackBar_End.Value = endMs;
            numericUpDown_LoopEnd.Value = ClampNumericUpDownValue(numericUpDown_LoopEnd, endMs);
        }

        private void SetLoopUiMilliseconds(int startMs, int endMs, long? exactStartSamples = null, long? exactEndSamples = null)
        {
            bool previousExternalState = applyingExternalLoopState;
            applyingExternalLoopState = true;
            try
            {
                loopStartSampleDisplayOverride = exactStartSamples;
                loopEndSampleDisplayOverride = exactEndSamples;
                SetLoopStartMilliseconds(startMs);
                SetLoopEndMilliseconds(endMs);
            }
            finally
            {
                applyingExternalLoopState = previousExternalState;
            }
            UpdateLoopPointValueLabels();
        }

        private static int SamplesToMilliseconds(long samples, int sampleRate)
        {
            if (samples <= 0 || sampleRate <= 0)
                return 0;

            double milliseconds = samples * 1000.0 / sampleRate;
            return milliseconds >= int.MaxValue
                ? int.MaxValue
                : (int)Math.Round(milliseconds, MidpointRounding.AwayFromZero);
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
            if (IsNus3BankPlaybackActive())
            {
                FormMain.DebugInfo($"[FormLPC] ATRAC buffer loop sync skipped for NUS3BANK preview. buttonIndex={btnpos}");
                return;
            }

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

                long exactStartSamples = Generic.IsOpenMulti ? Generic.MultipleLoopStarts[pos] : bufferloop[0];
                long exactEndSamples = Generic.IsOpenMulti ? Generic.MultipleLoopEnds[pos] : bufferloop[1];
                SetLoopUiMilliseconds(
                    SamplesToMilliseconds(exactStartSamples, samplerate),
                    SamplesToMilliseconds(exactEndSamples, samplerate),
                    exactStartSamples,
                    exactEndSamples);

                if (Generic.IsOpenMulti)
                {
                    if (Generic.MultipleFilesLoopOKFlags[pos] && !checkBox_LoopEnable.Checked)
                    {
                        checkBox_LoopEnable.Checked = true;
                        // ルーチEUI を�E通メソチE��で有効匁E
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
