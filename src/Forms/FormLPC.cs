using ATRACTool_Reloaded.Localizable;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Runtime.InteropServices;
using static ATRACTool_Reloaded.Common;
using static ATRACTool_Reloaded.Common.Constants;

namespace ATRACTool_Reloaded
{
    public partial class FormLPC : Form
    {
        private WaveOut wo = new();
        private MMDevice? mmDevice;
        private WasapiPlayer wasapiPlayer = null!;
        private AsioOut asioOut = null!;
        private string asioDriver = null!;
        WaveFileReader reader = null!;
        private WaveChannel32? waveChannel;
        private VolumeSampleProvider? asioVolumeProvider;
        long Sample, Start = 0, End = 0;
        long? loopStartSampleDisplayOverride, loopEndSampleDisplayOverride;
        int bytePerSec, position, length, smplrate, WASAPILatency = 0, WASAPIexLatency = 0, UseThreads = 3;
        private int _waveOutDesiredLatency = 200;
        private int _waveOutBufferCount = 16;
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
        private const int SmoothPlaybackTimerIntervalMs = 8;
        private const int StandardPlaybackTimerIntervalMs = 15;
        private const int StandardPlaybackUiRefreshIntervalMs = 100;
        private const int DefaultWasapiLatencyMs = 50;
        private const int UnsupportedAudioFormatHResult = unchecked((int)0x8889000A);

        private long _lastPlaybackUiRefreshTick;
        private long _smoothPlaybackAnchorSample;
        private long _smoothPlaybackAnchorTimestamp;
        private long _lastObservedReaderSample;
        private bool _smoothPlaybackPositionInitialized;

        Point labelTrk, labelStart, labelEnd;

        private SwitchableWaveProvider? _activePlaybackProvider;
        private PlaybackOutputMode? _activeOutputMode;
        private bool _waveOutInitialized;

        int[] bufferloop = new int[2];
        private int[] originalLoopStarts = [];
        private int[] originalLoopEnds = [];
        private bool[] originalLoopFlags = [];

        private enum PlaybackOutputMode
        {
            WaveOut,
            WasapiShared,
            WasapiExclusive,
            Asio,
        }

        private sealed class SwitchableWaveProvider : IWaveProvider
        {
            private IWaveProvider _source;

            public SwitchableWaveProvider(IWaveProvider source)
            {
                _source = source;
                WaveFormat = source.WaveFormat;
            }

            public WaveFormat WaveFormat { get; }

            public bool CanSwitchTo(IWaveProvider source)
            {
                WaveFormat candidate = source.WaveFormat;
                bool basicFormatMatches = WaveFormat.Encoding == candidate.Encoding &&
                    WaveFormat.SampleRate == candidate.SampleRate &&
                    WaveFormat.Channels == candidate.Channels &&
                    WaveFormat.BitsPerSample == candidate.BitsPerSample &&
                    WaveFormat.BlockAlign == candidate.BlockAlign &&
                    WaveFormat.AverageBytesPerSecond == candidate.AverageBytesPerSecond &&
                    WaveFormat.ExtraSize == candidate.ExtraSize;

                if (!basicFormatMatches)
                    return false;

                if (WaveFormat is WaveFormatExtensible currentExtensible &&
                    candidate is WaveFormatExtensible candidateExtensible)
                {
                    return currentExtensible.SubFormat == candidateExtensible.SubFormat &&
                        currentExtensible.ValidBitsPerSample == candidateExtensible.ValidBitsPerSample &&
                        currentExtensible.ChannelMask == candidateExtensible.ChannelMask;
                }

                if (WaveFormat is WaveFormatExtensible || candidate is WaveFormatExtensible)
                    return false;

                if (WaveFormat is WaveFormatExtraData currentExtra &&
                    candidate is WaveFormatExtraData candidateExtra)
                {
                    return currentExtra.ExtraData.AsSpan().SequenceEqual(candidateExtra.ExtraData);
                }

                return WaveFormat is not WaveFormatExtraData && candidate is not WaveFormatExtraData;
            }

            public void SwitchTo(IWaveProvider source)
            {
                if (!CanSwitchTo(source))
                    throw new InvalidOperationException("The playback formats are not compatible.");

                Interlocked.Exchange(ref _source, source);
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                return Volatile.Read(ref _source).Read(buffer.AsSpan(offset, count));
            }

            public int Read(Span<byte> buffer)
            {
                return Volatile.Read(ref _source).Read(buffer);
            }
        }

        private sealed class WaveFormatOverrideProvider : IWaveProvider
        {
            private readonly IWaveProvider source;

            public WaveFormatOverrideProvider(IWaveProvider source, WaveFormat waveFormat)
            {
                this.source = source;
                WaveFormat = waveFormat;
            }

            public WaveFormat WaveFormat { get; }

            public int Read(byte[] buffer, int offset, int count)
            {
                return source.Read(buffer.AsSpan(offset, count));
            }

            public int Read(Span<byte> buffer)
            {
                return source.Read(buffer);
            }
        }

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
            timer_Reload.Interval = StandardPlaybackTimerIntervalMs;
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

        private bool TryOpenPlaybackReader(string path)
        {
            if (!TryCreatePlaybackReader(path, out WaveFileReader? openedReader))
            {
                Generic.LPCException = true;
                return false;
            }

            reader = openedReader!;
            return true;
        }

        private bool TryCreatePlaybackReader(string path, out WaveFileReader? openedReader)
        {
            openedReader = null;
            try
            {
                openedReader = new WaveFileReader(path);
                return true;
            }
            catch (Exception ex)
            {
                Generic.IsLPCStreamingReloaded = false;
                FormMain.DebugError($"[FormLPC] Playback WAV could not be opened. path={path}, error={ex}");
                MessageBox.Show(
                    this,
                    $"{Localization.DecodeErrorCaption}\r\n\r\n{ex.Message}",
                    Localization.MSGBoxErrorCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool PlaybackInitialized { get; private set; }

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
                ? wasapiPlayer?.PlaybackState.ToString() ?? "null"
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
                    wasapiPlayer?.Stop();
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
            ResetPlaybackPositionTracking(reader.Position / Math.Max(reader.BlockAlign, 1));
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

        private void ApplyPlaybackMethod(LPCPlaybackMethodType playbackMethod)
        {
            IsWASAPI = playbackMethod == LPCPlaybackMethodType.WasapiShared;
            IsWASAPIex = playbackMethod == LPCPlaybackMethodType.WasapiExclusive;
            IsASIO = playbackMethod == LPCPlaybackMethodType.ASIO;
        }

        private void ApplyMultiChannelPlaybackSettings()
        {
            if (reader.WaveFormat.Channels <= 2)
                return;

            if (!Utils.GetBool("LPCMultipleStreamAlwaysWASAPIorASIO", true))
            {
                FormMain.DebugInfo(
                    $"[FormLPC] Multi-channel playback override is disabled. " +
                    $"channels={reader.WaveFormat.Channels}, mode={GetPlaybackOutputMode()}");
                return;
            }

            int configuredMethod = Utils.GetInt("LPCMultipleStreamPlaybackMethod", 0);
            LPCPlaybackMethodType playbackMethod = configuredMethod switch
            {
                0 => LPCPlaybackMethodType.WasapiShared,
                1 => LPCPlaybackMethodType.WasapiExclusive,
                2 => LPCPlaybackMethodType.ASIO,
                _ => LPCPlaybackMethodType.WasapiShared,
            };

            if (configuredMethod is < 0 or > 2)
            {
                FormMain.DebugWarn(
                    $"[FormLPC] Invalid multi-channel playback method ignored. " +
                    $"value={configuredMethod}, fallback={playbackMethod}");
            }

            ApplyPlaybackMethod(playbackMethod);
            FormMain.DebugInfo(
                $"[FormLPC] Multi-channel playback override applied. " +
                $"channels={reader.WaveFormat.Channels}, mode={playbackMethod}");
        }

        private void EnsureAsioDriverAvailable()
        {
            if (!IsASIO || !string.IsNullOrWhiteSpace(asioDriver))
                return;

            FormMain.DebugWarn("[FormLPC] ASIO playback requested but no driver is configured. Falling back to WASAPI exclusive.");
            MessageBox.Show(
                this,
                "It is configured to play using ASIO, but no valid driver was found.\r\n" +
                "It will play using WASAPI exclusive mode instead.",
                Localization.MSGBoxWarningCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            ApplyPlaybackMethod(LPCPlaybackMethodType.WasapiExclusive);
        }

        private void ApplyConfiguredPlaybackSettings()
        {
            int configuredMethod = Utils.GetInt("LPCPlaybackMethod", 0);
            LPCPlaybackMethodType playbackMethod = Enum.IsDefined(typeof(LPCPlaybackMethodType), configuredMethod)
                ? (LPCPlaybackMethodType)configuredMethod
                : LPCPlaybackMethodType.DirectSound;

            if (playbackMethod != (LPCPlaybackMethodType)configuredMethod)
            {
                FormMain.DebugWarn(
                    $"[FormLPC] Invalid playback method ignored. " +
                    $"value={configuredMethod}, fallback={playbackMethod}");
            }

            ApplyPlaybackMethod(playbackMethod);
            ApplyMultiChannelPlaybackSettings();
            EnsureAsioDriverAvailable();
        }

        private void FormLPC_Load(object sender, EventArgs e)
        {
            FormMain.DebugInfo("[FormLPC] Load started.");
            PlaybackInitialized = false;
            Config.Load(xmlpath);

            FormLPCInstance = this;

            // SmoothSamples
            SmoothSamples = Utils.GetBool("SmoothSamples", false);
            timer_Reload.Interval = SmoothSamples
                ? SmoothPlaybackTimerIntervalMs
                : StandardPlaybackTimerIntervalMs;

            // ATRAC 再生可否（MiniDisc SP / AEA は既存ATRAC設定から分離）
            IsPlaybackATRAC = Generic.IsMiniDiscAtrac1Input
                ? Utils.GetBool("PlaybackMiniDisc", true)
                : Utils.GetBool("PlaybackATRAC", false);
            IsPlaybackNus3Bank = Utils.GetBool("PlaybackNus3Bank", true);

            // ATRAC をエンコードソースとして扱ぁE��
            IsEncodeSourceATRAC = Utils.GetBool("ATRACEncodeSource", false);
            FormMain.DebugInfo($"[FormLPC] Config loaded. smoothSamples={SmoothSamples}, playbackAtrac={IsPlaybackATRAC}, playbackNus3Bank={IsPlaybackNus3Bank}, encodeSourceAtrac={IsEncodeSourceATRAC}");

            // ASIO ドライバ名取征E
            asioDriver = Utils.GetString("LPCUseASIODriver", string.Empty);

            UseParallel = Utils.GetBool("UseParallelMethod", false);

            if (SmoothSamples)
            {
                WASAPILatency = 0;
                WASAPIexLatency = 0;
                _waveOutDesiredLatency = 200;
                _waveOutBufferCount = 16;
            }
            else
            {
                WASAPILatency = GetValidatedPlaybackSetting(
                    "WASAPILatencySharedValue",
                    0,
                    value => value is >= 0 and <= 350 && value % 50 == 0);
                WASAPIexLatency = GetValidatedPlaybackSetting(
                    "WASAPILatencyExclusivedValue",
                    0,
                    value => value is >= 0 and <= 350 && value % 50 == 0);
                _waveOutDesiredLatency = GetValidatedPlaybackSetting(
                    "DirectSoundLatencyValue",
                    200,
                    value => value is >= 100 and <= 500 && value % 50 == 0);
                _waveOutBufferCount = GetValidatedPlaybackSetting(
                    "DirectSoundBuffersValue",
                    16,
                    value => value is 8 or 16 or 24 or 32);
            }

            ApplyWaveOutSettings();
            FormMain.DebugInfo(
                $"[FormLPC] Playback tuning loaded. smoothSamples={SmoothSamples}, " +
                $"waveOutLatencyMs={_waveOutDesiredLatency}, waveOutBuffers={_waveOutBufferCount}, " +
                $"wasapiSharedLatencyMs={WASAPILatency}, wasapiExclusiveLatencyMs={WASAPIexLatency}");

            UseThreads = Math.Clamp(Utils.GetInt("PlaybackThreadCount", 3) + 1, 1, 8);

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
                DisableLoopUiControls();

                string[] paths = GetLpcPlaybackPaths();
                if (!TryOpenPlaybackReader(paths[0])) return;
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
                if (Common.Generic.pATRACOpenFilePaths.Length == 1) // 単一ファイル
                {
                    if (!TryOpenPlaybackReader(Common.Generic.pATRACOpenFilePaths[0])) return;
                    //FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = BuildLpcDisplayLabel(0, reader);
                    button_Prev.Enabled = false;
                    button_Next.Enabled = false;
                }
                else // 褁E��ファイル
                {
                    if (!TryOpenPlaybackReader(Common.Generic.pATRACOpenFilePaths[0])) return;
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
                    if (!TryOpenPlaybackReader(Common.Generic.pATRACOpenFilePaths[0])) return;
                    //FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[0]);
                    label_File.Text = BuildLpcDisplayLabel(0, reader);
                    button_Prev.Enabled = false;
                    button_Next.Enabled = false;
                }
                else
                {
                    if (Generic.lpcreatev2 && Common.Generic.lpcreate != false) //　LPC有効
                    {
                        if (!TryOpenPlaybackReader(Common.Generic.pATRACOpenFilePaths[Common.Generic.files])) return;
                        FileInfo fi = new(Common.Generic.pATRACOpenFilePaths[Common.Generic.files]);
                        label_File.Text = fi.Name;
                        button_Prev.Enabled = false;
                        button_Next.Enabled = false;

                        switch (Common.Generic.ATRACFlag)
                        {
                            case 0:
                                checkBox_LoopEnable.Checked = true;
                                checkBox_LoopEnable.Enabled = false;
                                button_Cancel.Enabled = false;
                                break;
                            case 1:
                                checkBox_LoopEnable.Checked = true;
                                checkBox_LoopEnable.Enabled = false;
                                button_Cancel.Enabled = false;
                                break;
                        }
                    }
                    else // 褁E��ファイル
                    {
                        if (!TryOpenPlaybackReader(Common.Generic.pATRACOpenFilePaths[0])) return;
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
                    if (!TryOpenPlaybackReader(Common.Generic.OpenFilePaths[0])) return;
                    //FileInfo fi = new(Common.Generic.OpenFilePaths[0]);
                    label_File.Text = BuildLpcDisplayLabel(0, reader);
                    button_Prev.Enabled = false;
                    button_Next.Enabled = false;
                }
                else
                {
                    if (Generic.lpcreatev2 && Common.Generic.lpcreate != false) // LPC有効
                    {
                        if (!TryOpenPlaybackReader(Common.Generic.OpenFilePaths[Common.Generic.files])) return;
                        FileInfo fi = new(Common.Generic.OpenFilePaths[Common.Generic.files]);
                        label_File.Text = fi.Name;
                        button_Prev.Enabled = false;
                        button_Next.Enabled = false;

                        switch (Common.Generic.ATRACFlag)
                        {
                            case 0:
                                checkBox_LoopEnable.Checked = true;
                                checkBox_LoopEnable.Enabled = false;
                                button_Cancel.Enabled = false;
                                break;
                            case 1:
                                checkBox_LoopEnable.Checked = true;
                                checkBox_LoopEnable.Enabled = false;
                                button_Cancel.Enabled = false;
                                break;
                        }
                    }
                    else // 褁E��ファイル
                    {
                        if (!TryOpenPlaybackReader(Common.Generic.OpenFilePaths[0])) return;
                        //FileInfo fi = new(Common.Generic.OpenFilePaths[0]);
                        label_File.Text = BuildLpcDisplayLabel(0, reader);
                        button_Prev.Enabled = false;
                        button_Next.Enabled = true;
                        btnpos = 1;
                    }
                }
            }

            ApplyConfiguredPlaybackSettings();

            if (!PlaybackInit())
            {
                FormMain.DebugError("[FormLPC] Playback initialization failed. Closing LPC form.");
                Generic.LPCException = true;
                return;
            }

            _ = FormMain.FormMainInstance.Meta;

            Generic.IsLPCStreamingReloaded = false;

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
            ResetPlaybackPositionTracking(0);
            if (IsNus3BankPlaybackActive())
            {
                ApplyLoopStateFromGenericSilently();
            }
            else
            {
                SetLoopPointsWithATRACBuffer(reader.WaveFormat.SampleRate, 0);
            }
            CaptureOriginalLoopState();
            UpdateRestoreOriginalLoopButtonState();

            Generic.LPCTotalSamples = reader.SampleCount;
            RefreshTrackbarVisuals();
            PlaybackInitialized = true;
            FormMain.DebugInfo($"[FormLPC] Load completed. file={label_File.Text}, channels={reader.WaveFormat.Channels}, sampleRate={reader.WaveFormat.SampleRate}, samples={reader.SampleCount}");

        }

        private IWaveProvider BuildOutputChain(WaveFileReader reader)
        {
            // まず�E「Extensible含む何でも」�E float(ISampleProvider)
            return BuildFloatFromWaveFileReader(reader).ToWaveProvider();
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
        private WasapiPlayer CreateWasapiPlayer(bool exclusive, int configuredLatency, int channels)
        {
            if (mmDevice is null)
                throw new InvalidOperationException("The WASAPI playback device is unavailable.");

            int effectiveLatency = configuredLatency > 0
                ? configuredLatency
                : DefaultWasapiLatencyMs;

            var builder = new WasapiPlayerBuilder()
                .WithDevice(mmDevice)
                .WithLatency(effectiveLatency)
                .WithEventSync()
                .WithMmcssThreadPriority(exclusive ? "Pro Audio" : "Audio");

            if (exclusive)
            {
                builder.WithExclusiveMode();
            }
            else
            {
                builder.WithSharedMode();

                // A configured value of zero represented the low-latency path in the old settings.
                // NAudio 3 can now request the device's native low-latency engine period directly.
                if (configuredLatency <= 0 && channels <= 2)
                    builder.WithLowLatency(false);
            }

            return builder.Build();
        }

        private void LogWasapiConfiguration(string shareMode, int requestedLatency)
        {
            FormMain.DebugInfo(
                $"[FormLPC] WASAPI initialization completed. shareMode={shareMode}, " +
                $"requestedLatency={requestedLatency}, actualLatency={wasapiPlayer.LatencyMilliseconds}, " +
                $"lowLatency={wasapiPlayer.LowLatencyActive}");

            if (!string.IsNullOrWhiteSpace(wasapiPlayer.LowLatencyUnavailableReason))
            {
                FormMain.DebugWarn(
                    $"[FormLPC] WASAPI low-latency mode was unavailable. " +
                    $"reason={wasapiPlayer.LowLatencyUnavailableReason}");
            }
        }

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
                int configuredLatency = IsWASAPIex ? WASAPIexLatency : WASAPILatency;
                wasapiPlayer = CreateWasapiPlayer(IsWASAPIex, configuredLatency, provider.WaveFormat.Channels);

                WasapiPlaybackCapability capability = wasapiPlayer.GetPlaybackCapability(provider.WaveFormat);
                if (!capability.Supported)
                {
                    throw new COMException(
                        capability.Reason ?? "The audio format is unsupported in WASAPI exclusive mode.",
                        UnsupportedAudioFormatHResult);
                }

                wasapiPlayer.Init(provider);
                wasapiPlayer.Volume = volumeSlider1.Volume;
                LogWasapiConfiguration(IsWASAPIex ? "Exclusive" : "Shared", configuredLatency);
                return true;
            }
            catch (COMException ex) when (IsWASAPIex && ex.HResult == UnsupportedAudioFormatHResult)
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

                    try { wasapiPlayer?.Dispose(); } catch (Exception disposeEx)
                    {
                        FormMain.DebugWarn($"[FormLPC] Failed to dispose unsupported WASAPI output. error={disposeEx.Message}");
                    }

                    wasapiPlayer = CreateWasapiPlayer(false, WASAPILatency, provider.WaveFormat.Channels);
                    wasapiPlayer.Init(provider);
                    wasapiPlayer.Volume = volumeSlider1.Volume;
                    LogWasapiConfiguration("Shared fallback", WASAPILatency);
                    return true;
                }
                catch (Exception sharedEx)
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

                    // false を返すことで、FormLPC_Load 側の
                    // 「if (!PlaybackInit()) { Close(); }」が実行され、E
                    // こ�E LPC フォーム自体も閉じられます、E
                    return false;
                }
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[FormLPC] WASAPI initialization failed. error={ex}");
                // そ�E他�E WASAPI 初期化エラー
                MessageBox.Show(
                    this,
                    "WASAPI の初期化に失敗しました。\r\n" + ex.Message,
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

        private static int GetValidatedPlaybackSetting(
            string key,
            int fallback,
            Func<int, bool> isValid)
        {
            int value = Utils.GetInt(key, fallback);
            if (isValid(value))
                return value;

            FormMain.DebugWarn($"[FormLPC] Invalid playback setting ignored. key={key}, value={value}, fallback={fallback}");
            return fallback;
        }

        private void ApplyWaveOutSettings()
        {
            wo.BufferMilliseconds = Math.Max(
                1,
                (_waveOutDesiredLatency + _waveOutBufferCount - 1) / _waveOutBufferCount);
            wo.NumberOfBuffers = _waveOutBufferCount;
        }

        private PlaybackOutputMode GetPlaybackOutputMode()
        {
            if (IsWASAPIex)
                return PlaybackOutputMode.WasapiExclusive;
            if (IsWASAPI)
                return PlaybackOutputMode.WasapiShared;
            if (IsASIO)
                return PlaybackOutputMode.Asio;
            return PlaybackOutputMode.WaveOut;
        }

        private bool IsPlaybackOutputReady(PlaybackOutputMode mode)
        {
            return mode switch
            {
                PlaybackOutputMode.WasapiShared or PlaybackOutputMode.WasapiExclusive => wasapiPlayer is not null,
                PlaybackOutputMode.Asio => asioOut is not null,
                PlaybackOutputMode.WaveOut => _waveOutInitialized,
                _ => false,
            };
        }

        private IWaveProvider CreateWasapiMultiChannelProvider()
        {
            WaveFormat sourceFormat = reader.WaveFormat;
            if (sourceFormat.Encoding == WaveFormatEncoding.Extensible)
            {
                FormMain.DebugInfo(
                    $"[FormLPC] Preserving the source WAVEFORMATEXTENSIBLE for WASAPI. " +
                    $"channels={sourceFormat.Channels}, bits={sourceFormat.BitsPerSample}");
                return reader;
            }

            if (sourceFormat.Encoding is not (WaveFormatEncoding.Pcm or WaveFormatEncoding.IeeeFloat))
                return BuildOutputChain(reader);

            Speakers speakerLayout = sourceFormat.Channels switch
            {
                4 => Speakers.Quad,
                6 => Speakers.Surround51,
                8 => Speakers.Surround71,
                _ => Speakers.None,
            };
            var extensibleFormat = new WaveFormatExtensible(
                sourceFormat.SampleRate,
                sourceFormat.BitsPerSample,
                sourceFormat.Channels,
                sourceFormat.Encoding == WaveFormatEncoding.IeeeFloat,
                sourceFormat.BitsPerSample,
                speakerLayout);

            FormMain.DebugInfo(
                $"[FormLPC] Added a channel mask for WASAPI multi-channel playback. " +
                $"channels={sourceFormat.Channels}, layout={speakerLayout}");
            return new WaveFormatOverrideProvider(reader, extensibleFormat);
        }

        private IWaveProvider CreatePlaybackProvider()
        {
            IWaveProvider source;
            if (reader.WaveFormat.Channels <= 2)
            {
                waveChannel = new WaveChannel32(reader);
                waveChannel.Pan = panSlider1.Pan;
                source = waveChannel;
            }
            else
            {
                waveChannel = null;
                source = IsWASAPI || IsWASAPIex
                    ? CreateWasapiMultiChannelProvider()
                    : BuildOutputChain(reader);
            }

            if (!IsASIO)
            {
                asioVolumeProvider = null;
                return source;
            }

            asioVolumeProvider = new VolumeSampleProvider(source.ToSampleProvider())
            {
                Volume = volumeSlider1.Volume,
            };
            return asioVolumeProvider.ToWaveProvider();
        }

        private void UpdatePanControlsForPlayback()
        {
            bool panSupported = reader is not null &&
                reader.WaveFormat.Channels <= 2 &&
                waveChannel is not null;

            label_Pan.Enabled = panSupported;
            panSlider1.Enabled = panSupported;
            button_PanCenter.Enabled = panSupported;

            if (panSupported)
                waveChannel!.Pan = panSlider1.Pan;
        }

        private bool TryReusePlaybackOutput(IWaveProvider source)
        {
            PlaybackOutputMode requestedMode = GetPlaybackOutputMode();
            if (_activePlaybackProvider is null ||
                _activeOutputMode != requestedMode ||
                !IsPlaybackOutputReady(requestedMode) ||
                !_activePlaybackProvider.CanSwitchTo(source))
            {
                return false;
            }

            _activePlaybackProvider.SwitchTo(source);
            FormMain.DebugInfo($"[FormLPC] Playback output reused. mode={requestedMode}, sampleRate={source.WaveFormat.SampleRate}, channels={source.WaveFormat.Channels}");
            return true;
        }

        private void DisposePlaybackOutputForReinitialization()
        {
            DisposeReloadableAudioOutputs();

            if (_waveOutInitialized)
            {
                try
                {
                    if (wo.PlaybackState != PlaybackState.Stopped)
                        wo.Stop();
                }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[FormLPC] Failed to stop WaveOut during reinitialization. error={ex.Message}");
                }

                try { wo.Dispose(); }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[FormLPC] Failed to dispose WaveOut during reinitialization. error={ex.Message}");
                }

                wo = new WaveOut();
                ApplyWaveOutSettings();
                _waveOutInitialized = false;
            }

            _activePlaybackProvider = null;
            _activeOutputMode = null;
        }

        private bool PlaybackInit()
        {
            try
            {
                FormMain.DebugInfo($"[FormLPC] PlaybackInit started. channels={reader.WaveFormat.Channels}, sampleRate={reader.WaveFormat.SampleRate}, bits={reader.WaveFormat.BitsPerSample}, wasapi={IsWASAPI}, wasapiExclusive={IsWASAPIex}, asio={IsASIO}");
                if (reader.WaveFormat.Channels is not (1 or 2 or 4 or 6 or 8))
                    return false;

                IsMultiChannel = reader.WaveFormat.Channels > 2;
                if (IsMultiChannel && !IsWASAPI && !IsWASAPIex && !IsASIO)
                {
                    throw new NotSupportedException(
                        "This audio contains multiple channel information and cannot be played using DirectSound.\r\n" +
                        "Please use WASAPI or ASIO.");
                }

                IWaveProvider source = CreatePlaybackProvider();
                if (TryReusePlaybackOutput(source))
                {
                    UpdatePanControlsForPlayback();
                    return true;
                }

                DisposePlaybackOutputForReinitialization();
                var switchableProvider = new SwitchableWaveProvider(source);

                if (IsWASAPI || IsWASAPIex)
                {
                    if (!TryInitWasapi(switchableProvider))
                        return false;
                }
                else if (IsASIO)
                {
                    asioOut = new(asioDriver);
                    asioOut.Init(switchableProvider);
                }
                else
                {
                    _waveOutInitialized = true;
                    wo.Init(switchableProvider);
                    wo.Volume = volumeSlider1.Volume;
                }

                _activePlaybackProvider = switchableProvider;
                _activeOutputMode = GetPlaybackOutputMode();
                UpdatePanControlsForPlayback();
                FormMain.DebugInfo($"[FormLPC] Playback output initialized. mode={_activeOutputMode}, sampleRate={source.WaveFormat.SampleRate}, channels={source.WaveFormat.Channels}");
                return true;
            }
            catch (Exception Ex)
            {
                DisposePlaybackOutputForReinitialization();
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

                    ResetPlaybackPositionTracking(reader.Position / Math.Max(reader.BlockAlign, 1));
                    timer_Reload.Enabled = true;
                    output.Play();
                    button_Play.Text = Localization.PauseCaption;
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
                            ResetPlaybackPositionTracking(reader.Position / Math.Max(reader.BlockAlign, 1));
                            output.Play();
                            IsPausedMoveTrackbar = false;
                        }
                        else
                        {
                            FormMain.DebugInfo($"[FormLPC] Playback resumed. positionMs={customTrackBar_Trk.Value}");
                            ResetPlaybackPositionTracking(Sample);
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
                    ResetPlaybackPositionTracking(Sample);
                    button_Play.Text = Localization.PlayCaption;
                    break;
            }
        }

        private void Button_Play_Click(object sender, EventArgs e)
        {
            if (IsWASAPI || IsWASAPIex)
            {
                HandlePlayButton(wasapiPlayer);
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
                ResetPlaybackPositionTracking(0);
                UpdatePlaybackPositionUi(0);
                Resettrackbarlabels();
            }
        }


        private void Button_Stop_Click(object sender, EventArgs e)
        {
            if (IsWASAPI || IsWASAPIex)
            {
                HandleStopButton(wasapiPlayer);
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
            if (reader.Position >= reader.Length)
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
            else if (reader.Position == 0 && output.PlaybackState == PlaybackState.Stopped)
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
                long currentPosition = reader.Position;
                bool looped = false;
                if (checkBox_LoopEnable.Checked)
                {
                    long loopStartPosition = GetLoopBytePosition(customTrackBar_Start.Value, loopStartSampleDisplayOverride);
                    long loopEndPosition = GetLoopBytePosition(customTrackBar_End.Value, loopEndSampleDisplayOverride);
                    if (loopEndPosition > loopStartPosition && currentPosition >= loopEndPosition)
                    {
                        reader.Position = loopStartPosition;
                        currentPosition = loopStartPosition;
                        looped = true;
                    }
                }

                // 出力デバイスごとの処琁E�E共通�Eルパ�Eに雁E��E
                if (IsWASAPI || IsWASAPIex)
                {
                    HandleTimerReloadForOutput(wasapiPlayer);
                }
                else if (IsASIO)
                {
                    HandleTimerReloadForOutput(asioOut);
                }
                else
                {
                    HandleTimerReloadForOutput(wo);
                }

                currentPosition = reader.Position;
                int blockAlign = Math.Max(reader.BlockAlign, 1);
                long actualSample = currentPosition / blockAlign;
                PlaybackState playbackState = GetActivePlaybackOutput()?.PlaybackState ?? PlaybackState.Stopped;
                long displayedSample = SmoothSamples
                    ? GetSmoothPlaybackSample(actualSample, playbackState, looped)
                    : actualSample;

                if (!ShouldRefreshPlaybackUi())
                {
                    Sample = actualSample;
                    return;
                }

                UpdatePlaybackPositionUi(displayedSample);
            }
            catch (ObjectDisposedException)
            {
                StopPlaybackLoop();
            }
        }

        private bool ShouldRefreshPlaybackUi()
        {
            if (SmoothSamples)
                return true;

            long now = Environment.TickCount64;
            if (_lastPlaybackUiRefreshTick != 0 &&
                now - _lastPlaybackUiRefreshTick < StandardPlaybackUiRefreshIntervalMs)
            {
                return false;
            }

            _lastPlaybackUiRefreshTick = now;
            return true;
        }

        private long GetSmoothPlaybackSample(long actualSample, PlaybackState playbackState, bool forceReset)
        {
            actualSample = ClampPlaybackSample(actualSample);
            long now = Stopwatch.GetTimestamp();

            if (!_smoothPlaybackPositionInitialized || forceReset || actualSample < _lastObservedReaderSample)
            {
                ResetPlaybackPositionTracking(actualSample, now);
                return actualSample;
            }

            _lastObservedReaderSample = actualSample;
            if (playbackState != PlaybackState.Playing)
            {
                _smoothPlaybackAnchorSample = Sample;
                _smoothPlaybackAnchorTimestamp = now;
                return Sample;
            }

            double elapsedSeconds = (now - _smoothPlaybackAnchorTimestamp) / (double)Stopwatch.Frequency;
            long elapsedSamples = (long)Math.Round(elapsedSeconds * Math.Max(smplrate, 1));
            long predictedSample = ClampPlaybackSample(_smoothPlaybackAnchorSample + elapsedSamples);

            // WaveFileReader.Position advances when audio buffers are filled. Keep the
            // interpolated display behind that position so it cannot run ahead of audio data.
            return Math.Min(Math.Max(predictedSample, Sample), actualSample);
        }

        private void ResetPlaybackPositionTracking(long sample)
        {
            ResetPlaybackPositionTracking(sample, Stopwatch.GetTimestamp());
        }

        private void ResetPlaybackPositionTracking(long sample, long timestamp)
        {
            sample = ClampPlaybackSample(sample);
            Sample = sample;
            _smoothPlaybackAnchorSample = sample;
            _smoothPlaybackAnchorTimestamp = timestamp;
            _lastObservedReaderSample = sample;
            _smoothPlaybackPositionInitialized = true;
            _lastPlaybackUiRefreshTick = 0;
        }

        private long ClampPlaybackSample(long sample)
        {
            return totalsamples > 0
                ? Math.Clamp(sample, 0L, totalsamples)
                : Math.Max(0L, sample);
        }

        private void UpdatePlaybackPositionUi(long displayedSample)
        {
            Sample = ClampPlaybackSample(displayedSample);
            int sampleRate = Math.Max(smplrate, 1);
            double elapsedMilliseconds = Sample * 1000.0 / sampleRate;
            position = (int)Math.Min(elapsedMilliseconds / 1000.0, int.MaxValue);
            time = TimeSpan.FromMilliseconds(elapsedMilliseconds);

            if (!mouseDown)
            {
                int trackMilliseconds = (int)Math.Clamp(
                    Math.Round(elapsedMilliseconds, MidpointRounding.AwayFromZero),
                    customTrackBar_Trk.Minimum,
                    customTrackBar_Trk.Maximum);
                customTrackBar_Trk.Value = trackMilliseconds;
            }

            long trackSample = mouseDown
                ? MillisecondsToSamples(customTrackBar_Trk.Value)
                : Sample;
            string trackText = BuildPositionText(customTrackBar_Trk.Value, trackSample);
            if (!string.Equals(label_trk.Text, trackText, StringComparison.Ordinal))
            {
                label_trk.Text = trackText;
                customTrackBar_Trk.OverlayText = trackText;
            }

            string elapsedText = time.ToString(@"hh\:mm\:ss");
            if (!string.Equals(label_Plength.Text, elapsedText, StringComparison.Ordinal))
                label_Plength.Text = elapsedText;

            string sampleText = Sample.ToString();
            if (!string.Equals(label_Psamples.Text, sampleText, StringComparison.Ordinal))
                label_Psamples.Text = sampleText;
        }

        private IWavePlayer? GetActivePlaybackOutput()
        {
            return _activeOutputMode switch
            {
                PlaybackOutputMode.WasapiShared or PlaybackOutputMode.WasapiExclusive => wasapiPlayer,
                PlaybackOutputMode.Asio => asioOut,
                PlaybackOutputMode.WaveOut when _waveOutInitialized => wo,
                _ => null,
            };
        }

        private long GetLoopBytePosition(int milliseconds, long? exactSamples)
        {
            int blockAlign = Math.Max(reader.BlockAlign, 1);
            long position = exactSamples.HasValue
                ? exactSamples.Value * blockAlign
                : (long)(milliseconds * (double)reader.WaveFormat.AverageBytesPerSecond / 1000.0);

            position = Math.Clamp(position, 0L, reader.Length);
            return position - position % blockAlign;
        }

        private void ResumePlaybackAfterTrackSwitch()
        {
            IWavePlayer? output = GetActivePlaybackOutput();

            if (output is null)
            {
                FormMain.DebugWarn("[FormLPC] Automatic playback could not resume because the output is unavailable.");
                return;
            }

            HandlePlayButton(output);
            FormMain.DebugInfo($"[FormLPC] Automatic playback resumed after track switch. buttonIndex={btnpos}");
        }

        private void DisposeReloadableAudioOutputs()
        {
            if (wasapiPlayer is not null)
            {
                try
                {
                    if (wasapiPlayer.PlaybackState != PlaybackState.Stopped)
                        wasapiPlayer.Stop();
                }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[FormLPC] Failed to stop WASAPI output during cleanup. error={ex.Message}");
                }

                try { wasapiPlayer.Dispose(); }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[FormLPC] Failed to dispose WASAPI output. error={ex.Message}");
                }
                wasapiPlayer = null!;
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isClosing = true;
            StopPlaybackLoop();
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
            _activePlaybackProvider = null;
            _activeOutputMode = null;
            _waveOutInitialized = false;
            waveChannel = null;
            asioVolumeProvider = null;

            if (ReferenceEquals(FormLPCInstance, this))
                FormLPCInstance = null!;

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
            SwitchPlaybackTrack(-1);
        }

        private void Button_Next_Click(object sender, EventArgs e)
        {
            SwitchPlaybackTrack(1);
        }

        private void SwitchPlaybackTrack(int direction)
        {
            string action = direction < 0 ? "Previous" : "Next";
            long startedAt = Stopwatch.GetTimestamp();
            string[] paths = GetLpcPlaybackPaths();
            int trackCount = paths.Length;
            int currentIndex = btnpos > 0 ? (int)btnpos - 1 : 0;
            int targetIndex = currentIndex + Math.Sign(direction);

            if (trackCount == 0 || targetIndex < 0 || targetIndex >= trackCount)
                return;

            FormMain.DebugInfo($"[FormLPC] {action} file requested. currentButtonIndex={btnpos}, targetButtonIndex={targetIndex + 1}");

            if (Generic.IsLoopWarning && Generic.IsOpenMulti && checkBox_LoopEnable.Checked)
            {
                var (start, end, ok) = LoopPointController.GetLoopState((uint)(currentIndex + 1));
                if (!ok && (start == 0 || end == 0))
                {
                    DialogResult result = MessageBox.Show(
                        Localization.LoopWarningCaption,
                        Localization.MSGBoxWarningCaption,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        FormMain.DebugWarn($"[FormLPC] {action} file cancelled by loop warning. currentButtonIndex={btnpos}");
                        return;
                    }
                }
            }

            string[] originPaths = GetLpcOriginPaths();
            FileInfo originalFile = new(GetIndexedPath(originPaths, targetIndex, paths[targetIndex]));
            if (!TryCreatePlaybackReader(paths[targetIndex], out WaveFileReader? nextReader))
                return;

            WaveFileReader? previousReader = reader;
            bool resumeAfterSwitch = timer_Reload.Enabled && !stopflag;
            Generic.IsLPCStreamingReloaded = true;
            try
            {
                StopPlaybackLoop();
                try
                {
                    if (_activeOutputMode is PlaybackOutputMode.WasapiShared or PlaybackOutputMode.WasapiExclusive)
                        wasapiPlayer?.Stop();
                    else if (_activeOutputMode == PlaybackOutputMode.Asio)
                        asioOut?.Stop();
                    else if (_waveOutInitialized)
                        wo.Stop();
                }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[FormLPC] Playback stop failed during track switch. error={ex.Message}");
                    DisposePlaybackOutputForReinitialization();
                }

                button_Play.Text = Localization.PlayCaption;
                button_Stop.Enabled = false;

                btnpos = (uint)(targetIndex + 1);
                reader = nextReader!;
                nextReader = null;
                ApplyConfiguredPlaybackSettings();

                _ = FormMain.FormMainInstance.Meta;
                bool initialized = PlaybackInit();
                previousReader.Dispose();
                previousReader = null;
                if (!initialized)
                    return;

                ResetAFR();
                label_File.Text = BuildLpcDisplayLabel(targetIndex, reader);
                UpdateMainFileLabelsForCurrentPlayback(originalFile);
                button_Prev.Enabled = targetIndex > 0;
                button_Next.Enabled = targetIndex < trackCount - 1;

                SetLoopPointsWithATRACBuffer(reader.WaveFormat.SampleRate, btnpos - 1);
                ApplyLoopStateFromGenericSilently();
                UpdateRestoreOriginalLoopButtonState();

                smplrate = reader.WaveFormat.SampleRate;
                totalsamples = reader.SampleCount;
                ResetPlaybackPositionTracking(0);
                RefreshTrackbarVisuals();
                if (resumeAfterSwitch)
                    ResumePlaybackAfterTrackSwitch();

                var (loopStart, loopEnd, loopOk) = LoopPointController.GetLoopState(btnpos);
                double elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
                FormMain.DebugInfo($"[FormLPC] {action} file loaded. buttonIndex={btnpos}, file={label_File.Text}, loop={loopOk}, loopStart={loopStart}, loopEnd={loopEnd}, elapsedMs={elapsedMs:F1}");
            }
            finally
            {
                nextReader?.Dispose();
                previousReader?.Dispose();
                Generic.IsLPCStreamingReloaded = false;
            }
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

        private static bool IsLoopUnsupportedEncodeMethod()
        {
            return Generic.IsMiniDisc || Generic.IsWalkman;
        }

        private void ApplyUnsupportedEncodeLoopUi()
        {
            bool previousExternalState = applyingExternalLoopState;
            applyingExternalLoopState = true;
            try
            {
                loopStartSampleDisplayOverride = null;
                loopEndSampleDisplayOverride = null;
                checkBox_LoopEnable.Checked = false;
                checkBox_LoopEnable.Enabled = false;
                label_LoopStartSamples.Text = string.Empty;
                label_LoopEndSamples.Text = string.Empty;
                button_RestoreOriginalLoop.Enabled = false;
                DisableLoopUiControls();
                if (!Generic.lpcreatev2)
                    LoopPointController.DisableMainLoopUi();
            }
            finally
            {
                applyingExternalLoopState = previousExternalState;
            }

            string method = Generic.IsMiniDisc ? "MiniDisc" : "Walkman";
            FormMain.DebugInfo($"[FormLPC] Loop UI suppressed for unsupported encoding method. method={method}");
        }

        /// <summary>
        /// 現在の btnpos / Generic のループ状態を LPC の UI に反映する、E
        /// </summary>
        private void ApplyLoopStateFromGeneric()
        {
            if (IsLoopUnsupportedEncodeMethod())
            {
                ApplyUnsupportedEncodeLoopUi();
                return;
            }

            UpdateRestoreOriginalLoopButtonState();

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

                if (IsLoopUnsupportedEncodeMethod())
                    return;

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
                if (wasapiPlayer is not null)
                    wasapiPlayer.Volume = volumeSlider1.Volume;
            }
            else if (IsASIO)
            {
                if (asioVolumeProvider is not null)
                    asioVolumeProvider.Volume = volumeSlider1.Volume;
            }
            else if (_waveOutInitialized)
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

        private void Button_RestoreOriginalLoop_Click(object sender, EventArgs e)
        {
            int index = GetCurrentLoopStateIndex();
            if (!HasOriginalLoopState(index))
            {
                FormMain.DebugWarn($"[FormLPC] Original loop restore skipped: no original loop. buttonIndex={btnpos}, index={index}");
                UpdateRestoreOriginalLoopButtonState();
                return;
            }

            int start = originalLoopStarts[index];
            int end = originalLoopEnds[index];
            LoopPointController.UpdateLoopPointsBySourceIndex(index, start, end);
            bufferloop[0] = start;
            bufferloop[1] = end;
            Generic.IsATRACLooped = true;
            ApplyLoopStateFromGenericSilently();
            FormMain.DebugInfo($"[FormLPC] Original loop restored. buttonIndex={btnpos}, index={index}, start={start}, end={end}");
        }

        private void CaptureOriginalLoopState()
        {
            originalLoopStarts = Generic.MultipleLoopStarts?.ToArray() ?? [];
            originalLoopEnds = Generic.MultipleLoopEnds?.ToArray() ?? [];
            originalLoopFlags = Generic.MultipleFilesLoopOKFlags?.ToArray() ?? [];
            FormMain.DebugInfo($"[FormLPC] Original loop state captured. tracks={originalLoopFlags.Length}, looped={originalLoopFlags.Count(flag => flag)}");
        }

        private int GetCurrentLoopStateIndex()
        {
            bool indexed = Generic.IsOpenMulti ||
                (Generic.IsNus3Bank && Generic.IsPlaybackNus3Bank && Generic.pATRACOpenFilePaths is { Length: > 1 });
            return indexed ? Math.Max(0, (int)btnpos - 1) : 0;
        }

        private bool HasOriginalLoopState(int index)
        {
            return index >= 0 &&
                index < originalLoopStarts.Length &&
                index < originalLoopEnds.Length &&
                index < originalLoopFlags.Length &&
                originalLoopFlags[index] &&
                originalLoopStarts[index] > 0 &&
                originalLoopEnds[index] > originalLoopStarts[index];
        }

        private void UpdateRestoreOriginalLoopButtonState()
        {
            int index = GetCurrentLoopStateIndex();
            button_RestoreOriginalLoop.Enabled =
                !Generic.lpcreatev2 &&
                !IsLoopUnsupportedEncodeMethod() &&
                !IsNus3BankPlaybackActive() &&
                HasOriginalLoopState(index);
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
                    wasapiPlayer.Stop();
                }
                else if (IsASIO)
                {
                    asioOut.Stop();
                }
                else
                {
                    wo.Stop();
                }

                Generic.LPCSuffix = " -loop " + Start.ToString() + " " + End.ToString();

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
                    wasapiPlayer.Stop();
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
            if (reader is null || reader.WaveFormat.Channels > 2)
                return;

            if (waveChannel is not null)
                waveChannel.Pan = panSlider1.Pan;
        }

        private void Button_PanCenter_Click(object sender, EventArgs e)
        {
            panSlider1.Pan = 0F;
            if (waveChannel is not null)
                waveChannel.Pan = 0F;

            FormMain.DebugInfo("[FormLPC] Pan reset to center.");
        }

        public void SetLoopEditingAvailable(bool available)
        {
            checkBox_LoopEnable.Enabled = available;
        }

        /// <summary>
        /// ATRACからループ情報を読み取りUIに反映
        /// </summary>
        /// <param name="samplerate">サンプリング周波数</param>
        /// <param name="pos">Current ButtonPosition (multiple files only)</param>
        private void SetLoopPointsWithATRACBuffer(int samplerate, uint pos = 0)
        {
            if (IsLoopUnsupportedEncodeMethod())
            {
                ApplyUnsupportedEncodeLoopUi();
                return;
            }

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

        public int Read(Span<byte> buffer)
            => _source.Read(buffer);
    }
}
