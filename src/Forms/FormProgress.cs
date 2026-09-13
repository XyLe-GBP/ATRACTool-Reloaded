using ATRACTool_Reloaded.Localizable;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using NAudio.Wave;
using System.Buffers;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using static ATRACTool_Reloaded.Common;
using Localization = ATRACTool_Reloaded.Localizable.Localization;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ATRACTool_Reloaded
{
    public partial class FormProgress : Form
    {
        private static FormProgress _formProgressInstance = null!;
        public static FormProgress FormProgressInstance
        {
            get
            {
                return _formProgressInstance;
            }
            set
            {
                _formProgressInstance = value;
            }
        }

        private static string TempDirectory => Path.Combine(Directory.GetCurrentDirectory(), "_temp");

        private static readonly Dictionary<Constants.ATRAC3ConsoleType, string> Atrac3Tools =
        new()
        {
            { Constants.ATRAC3ConsoleType.PSP, Generic.PSP_ATRAC3tool },
            { Constants.ATRAC3ConsoleType.PS3, Generic.PS3_ATRAC3tool },
        };

        private static readonly Dictionary<Constants.ATRAC9ConsoleType, string> Atrac9Tools =
        new()
        {
            { Constants.ATRAC9ConsoleType.PSV, Generic.PSV_ATRAC9tool },
            { Constants.ATRAC9ConsoleType.PS4, Generic.PS4_ATRAC9tool },
        };

        private static readonly int[,] PsxAdpcmCoefficients =
        {
            { 0, 0 },
            { 60, 0 },
            { 115, -52 },
            { 98, -55 },
            { 122, -60 },
        };

        private const int ProgressDirectoryPollIntervalMs = 250;

        private sealed class IvagInfo
        {
            public required int Channels { get; init; }
            public required int SampleRate { get; init; }
            public required int SampleCount { get; init; }
            public required int DataOffset { get; init; }
            public required int ChannelDataSize { get; init; }
            public required int FrameGroups { get; init; }
        }

        private sealed class Nus3BankEncodedStreamSource
        {
            public required string EncodedPath { get; init; }
            public required string OriginPath { get; init; }
            public required string StreamName { get; init; }
            public Nus3RiffLoopPoints? LoopPoints { get; init; }
        }

        private sealed class Nus3BankDecodeWorkItem
        {
            public required Nus3SubfileCodec Codec { get; init; }
            public required string EncodedPath { get; init; }
            public required string WavPath { get; init; }
        }

        private sealed class MultipleEncodeItem
        {
            public required string InputPath { get; init; }
            public required string OriginPath { get; init; }
            public required string StreamName { get; init; }
            public required int SourceIndex { get; init; }
        }

        public FormProgress()
        {
            InitializeComponent();
            ModernUI.ModernTheme.Apply(this);

            FormMain.DebugInfo("[FormProgress] Initialized.");
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
                "" or "status" => $"FormProgress: process={Generic.ProcessFlag}, result={Generic.Result}, progress={progressBar_MainProgress.Value}/{progressBar_MainProgress.Maximum}, timer={timer_interval.Enabled}",
                "cancel" => ExecuteDebugCancel(),
                _ => $"Unknown FormProgress debug function: {functionName}"
            };
        }

        private static string ExecuteDebugCancel()
        {
            if (Generic.cts is null)
            {
                return "No cancellation token source is available.";
            }

            if (!Generic.cts.IsCancellationRequested)
            {
                Generic.cts.Cancel();
            }

            return "FormProgress cancellation requested.";
        }

        private void FormProgress_Load(object sender, EventArgs e)
        {
            _formProgressInstance = this;

            timer_interval.Interval = 1000;
            progressBar_MainProgress.Value = 0;
            progressBar_MainProgress.Minimum = 0;
            progressBar_MainProgress.Maximum = Generic.ProgressMax;
            FormMain.DebugInfo($"[FormProgress] Load. process={Generic.ProcessFlag}, progressMax={Generic.ProgressMax}");
            RunTask();
        }

        private async void RunTask()
        {
            // すべての処理で共通の CTS / Progress を使う
            Generic.Result = false;
            Generic.cts = new CancellationTokenSource();
            var cToken = Generic.cts.Token;
            var progress = new Progress<int>(UpdateProgress);
            try
            {
                FormMain.DebugInfo($"[FormProgress] RunTask started. process={Generic.ProcessFlag}");
                switch (Generic.ProcessFlag)
                {
                    case Constants.ProcessType.Decode: // Decode
                        {
                            FormMain.DebugInfo("Decode started.");

                            Generic.Result = await Task.Run(() => Decode_DoWork(progress, cToken), cToken);
                            break;
                        }
                    case Constants.ProcessType.Encode: // Encode
                        {
                            FormMain.DebugInfo("Encode started.");

                            if (Generic.lpcreate != false)
                            {
                                label_Status.Text = "Editing with Loop Point Creator...";
                            }
                            else
                            {
                                label_Status.Text = "Encoding...";
                            }

                            Generic.Result = await Task.Run(() => Encode_DoWork(progress, cToken), cToken);
                            break;
                        }
                    case Constants.ProcessType.AudioToWave: // Audio To Wave
                        {
                            FormMain.DebugInfo("ATW started.");

                            Generic.Result = await Task.Run(() => AudioConverter_ATW_DoWork(progress, cToken), cToken);
                            break;
                        }
                    case Constants.ProcessType.WaveToAudio: // Wave To Audio
                        {
                            FormMain.DebugInfo("WTA started.");

                            Generic.Result = await Task.Run(() => AudioConverter_WTA_DoWork(progress, cToken), cToken);
                            break;
                        }
                    case Constants.ProcessType.Update: // Update Program
                        {
                            FormMain.DebugInfo("Update started.");

                            Text = Localization.ProcessingCaption;
                            label1.Text = Localization.DownloadStatusCaption;
                            label_Status.Text = Localization.InitializationCaption;

                            Generic.Result = await Task.Run(() => Download_DoWork(progress, cToken), cToken);
                            break;
                        }
                    default:
                        FormMain.DebugWarn($"[FormProgress] Unknown process flag. process={Generic.ProcessFlag}");
                        Close();
                        return;
                }
            }
            catch (OperationCanceledException)
            {
                FormMain.DebugWarn("Operation Cancelled.");
                // ダウンロードなどでキャンセルされた場合：
                // ここでは単に Result=false として扱う
                Generic.Result = false;
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[FormProgress] RunTask failed. process={Generic.ProcessFlag}, error={ex}");
                // 予期せぬ例外はメッセージボックスで通知してから Result=false
                MessageBox.Show(
                    this,
                    ex.Message,
                    Localization.MSGBoxErrorCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Generic.Result = false;
            }
            finally
            {
                FormMain.DebugInfo($"[FormProgress] RunTask completed. process={Generic.ProcessFlag}, result={Generic.Result}");
                // どのパスでも最後にタイマー起動（一定時間後に自動で閉じる）
                timer_interval.Enabled = true;
                // UIスレッドで確実に閉じる（ShowDialog を戻す）
                /*try
                {
                    if (IsHandleCreated)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            // 呼び出し側が Result を見て判断しているなら DialogResult は任意だが、
                            // OK/Cancel を付けるとデバッグが楽になります。
                            DialogResult = Generic.Result ? DialogResult.OK : DialogResult.Cancel;
                            Close();
                        }));
                    }
                    else
                    {
                        // 念のため
                        DialogResult = Generic.Result ? DialogResult.OK : DialogResult.Cancel;
                        Close();
                    }
                }
                catch
                {
                    // 最終的に Close だけは試す
                    try { Close(); } catch { }
                }*/
            }
        }

        private static bool Decode_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            Config.Load(xmlpath);
            int length = Generic.OpenFilePaths.Length;
            FormMain.DebugInfo($"[Decode] Dispatch. files={length}, nus3bank={Generic.IsNus3Bank}, playbackNus3bank={Generic.IsPlaybackNus3Bank}, extractEmbedded={Generic.Nus3BankExtractEmbedded}");

            if (length == 1)
            {
                return DecodeSingleFile(p, cToken);
            }
            else // multiple
            {
                return DecodeMultipleFiles(p, cToken);
            }
        }

        /// <summary>
        /// 単一ファイルのATRACデコード処理
        /// </summary>
        private static bool DecodeSingleFile(IProgress<int> p, CancellationToken cToken)
        {
            FormMain.DebugInfo($"[Decode] Single file started. input={Generic.OpenFilePaths[0]}");
            FileInfo fi = new(Generic.OpenFilePaths[0]);
            string ext = fi.Extension.ToUpperInvariant();
            FormMain.DebugInfo($"[Decode] Single file format detected. ext={ext}");

            if (Nus3BankFile.HasNus3BankExtension(fi.FullName))
            {
                FormMain.DebugInfo("This file is NUSound.");
                string? singleOutPath = null;
                if (!Generic.Nus3BankDecodeToFolder)
                {
                    FileInfo target = ResolveSingleDecodeTarget();
                    singleOutPath = Path.Combine(TempDirectory, target.Name);
                }

                return DecodeNus3Bank(Generic.OpenFilePaths[0], singleOutPath, p, cToken);
            }

            FileInfo fi2;

            bool playbackAtrac = Utils.GetBool("PlaybackATRAC", false);
            bool fasterAtrac = Utils.GetBool("FasterATRAC", false);
            bool atracEncodeSource = Utils.GetBool("ATRACEncodeSource", false);

            if (Generic.IsPlaybackConversion && Generic.IsATRAC)
            {
                fi2 = new(Generic.pATRACSavePath);
            }
            else if (playbackAtrac && Generic.IsATRAC && !fasterAtrac)
            {
                fi2 = new(Generic.pATRACSavePath);
            }
            else if (playbackAtrac && Generic.IsATRAC && fasterAtrac)
            {
                fi2 = new(Generic.SavePath);
            }
            else
            {
                fi2 = atracEncodeSource
                    ? new FileInfo(Generic.pATRACSavePath)
                    : new FileInfo(Generic.SavePath);
            }

            var atrac3Console = (Constants.ATRAC3ConsoleType)Utils.GetInt("ATRAC3_Console", (int)Constants.ATRAC3ConsoleType.PSP);
            var atrac9Console = (Constants.ATRAC9ConsoleType)Utils.GetInt("ATRAC9_Console", (int)Constants.ATRAC9ConsoleType.PSV);
            string outPath = Path.Combine(TempDirectory, fi2.Name);

            if (ext == ".AEA")
            {
                if (!DecodeMiniDiscAtrac1ToWave(Generic.OpenFilePaths[0], outPath, p, cToken))
                    return false;
            }
            else if (ext == ".AT3")
            {
                Constants.ATRAC3ConsoleType decodeConsole = ResolveAtrac3DecoderConsole(Generic.OpenFilePaths[0], atrac3Console);
                string tool = Atrac3Tools[decodeConsole];
                if (!RunAtracTool(tool, Generic.DecodeParamAT3, Generic.OpenFilePaths[0], outPath, p, cToken)) return false;
            }
            else if (ext == ".AT9")
            {
                string tool = Atrac9Tools[atrac9Console];
                if (!RunAtracTool(tool, Generic.DecodeParamAT9, Generic.OpenFilePaths[0], outPath, p, cToken)) return false;
            }
            else if (ext == ".OMA")
            {
                string outWav = outPath;
                if (!string.Equals(Path.GetExtension(outWav), ".wav", StringComparison.OrdinalIgnoreCase))
                    outWav = Path.ChangeExtension(outWav, ".wav");

                if (!DecodeWalkmanOmaToWave(Generic.OpenFilePaths[0], outWav, p, cToken))
                    return false;
            }
            else if (ext == ".OMG")
            {
                // 参考：OMG は OpenMG/DRM の可能性が高く、FFmpeg で読めないことが多いです。
                // 非DRMなら読めるケースもあるので、一旦試すならOMAと同じでOK。
                string ffpath = Path.Combine(Directory.GetCurrentDirectory(), "res", "ffmpeg.exe");
                if (!File.Exists(ffpath))
                {
                    FormMain.DebugError("ffmpeg.exe not found: " + ffpath);
                    return false;
                }

                string outWav = outPath;
                if (!string.Equals(Path.GetExtension(outWav), ".wav", StringComparison.OrdinalIgnoreCase))
                    outWav = Path.ChangeExtension(outWav, ".wav");

                using var engine = new Engine(ffpath);

                if (!RunMediaToolkitDecodeToWav(engine, Generic.OpenFilePaths[0], outWav, p, cToken))
                    return false;
            }
            else
            {
                FormMain.DebugWarn("DecodeSingleFile: unsupported extension: " + ext);
                return false;
            }
            FormMain.DebugInfo($"[Decode] Single file completed. input={Generic.OpenFilePaths[0]}");
            return true;
        }

        /// <summary>
        /// 複数ファイルのATRACデコード処理
        /// </summary>
        /*private static bool DecodeMultipleFiles(IProgress<int> p, CancellationToken cToken)
        {
            var atrac3Console = (Constants.ATRAC3ConsoleType)Utils.GetInt("ATRAC3_Console", (int)Constants.ATRAC3ConsoleType.PSP);
            var atrac9Console = (Constants.ATRAC9ConsoleType)Utils.GetInt("ATRAC9_Console", (int)Constants.ATRAC9ConsoleType.PSV);

            foreach (var file in Generic.OpenFilePaths)
            {
                FileInfo fi = new(file);

                switch (fi.Extension.ToUpper())
                {
                    case ".AT3":
                        {
                            string outPath = Path.Combine(TempDirectory, fi.Name.Replace(fi.Extension, ".wav"));

                            switch (atrac3Console)
                            {
                                case Constants.ATRAC3ConsoleType.PSP: // PSP
                                    {
                                        if (!RunAtracTool(
                                            Generic.PSP_ATRAC3tool,
                                            Generic.DecodeParamAT3,
                                            file,
                                            outPath,
                                            p,
                                            cToken))
                                        {
                                            return false;
                                        }
                                    }
                                    break;
                                case Constants.ATRAC3ConsoleType.PS3: // PS3
                                    {
                                        if (!RunAtracTool(
                                            Generic.PS3_ATRAC3tool,
                                            Generic.DecodeParamAT3,
                                            file,
                                            outPath,
                                            p,
                                            cToken))
                                        {
                                            return false;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case ".AT9":
                        {
                            string outPath = Path.Combine(TempDirectory, fi.Name.Replace(fi.Extension, ".wav"));

                            switch (atrac9Console)
                            {
                                case Constants.ATRAC9ConsoleType.PSV: // PSV
                                    {
                                        if (!RunAtracTool(
                                            Generic.PSV_ATRAC9tool,
                                            Generic.DecodeParamAT9,
                                            file,
                                            outPath,
                                            p,
                                            cToken))
                                        {
                                            return false;
                                        }
                                    }
                                    break;
                                case Constants.ATRAC9ConsoleType.PS4: // PS4
                                    {
                                        if (!RunAtracTool(
                                            Generic.PS4_ATRAC9tool,
                                            Generic.DecodeParamAT9,
                                            file,
                                            outPath,
                                            p,
                                            cToken))
                                        {
                                            return false;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }*/
        private static bool DecodeMultipleFiles(IProgress<int> p, CancellationToken cToken)
        {
            var atrac3Console = (Constants.ATRAC3ConsoleType)Utils.GetInt("ATRAC3_Console", (int)Constants.ATRAC3ConsoleType.PSP);
            var atrac9Console = (Constants.ATRAC9ConsoleType)Utils.GetInt("ATRAC9_Console", (int)Constants.ATRAC9ConsoleType.PSV);
            FormMain.DebugInfo($"[Decode] Multiple files started. files={Generic.OpenFilePaths.Length}, atrac3Console={atrac3Console}, atrac9Console={atrac9Console}");

            if (ShouldUseParallelPlaybackDecode())
                return DecodeMultiplePlaybackFilesInParallel(p, cToken, atrac3Console, atrac9Console);

            string ffpath = Path.Combine(Directory.GetCurrentDirectory(), "res", "ffmpeg.exe");
            using Engine? ffEngine = File.Exists(ffpath) ? new Engine(ffpath) : null;
            if (ffEngine != null)
            {
                FormMain.DebugInfo($"[Decode] FFmpeg engine prepared. path={ffpath}");
            }
            else
            {
                FormMain.DebugWarn($"[Decode] FFmpeg not found. path={ffpath}");
            }

            for (int i = 0; i < Generic.OpenFilePaths.Length; i++)
            {
                string file = Generic.OpenFilePaths[i];
                FileInfo fi = new(file);
                FormMain.DebugInfo($"[Decode] File started. index={i + 1}/{Generic.OpenFilePaths.Length}, ext={fi.Extension}, input={file}");

                // Origin があるならそちら優先（InputJobs 導入済みなら確実）
                string originKey =
                    (Generic.InputJobs != null && Generic.InputJobs.Count == Generic.OpenFilePaths.Length)
                        ? Generic.InputJobs[i].OriginPath
                        : file;

                string outPath = Utils.MakeTempUniquePath(TempDirectory, originKey, i, ".wav");

                switch (fi.Extension.ToUpperInvariant())
                {
                    case ".AEA":
                        {
                            if (!DecodeMiniDiscAtrac1ToWave(file, outPath, p, cToken))
                                return false;
                        }
                        break;

                    case ".AT3":
                        {
                            Constants.ATRAC3ConsoleType decodeConsole = ResolveAtrac3DecoderConsole(file, atrac3Console);
                            if (!RunAtracTool(Atrac3Tools[decodeConsole], Generic.DecodeParamAT3, file, outPath, p, cToken)) return false;
                        }
                        break;

                    case ".AT9":
                        {
                            switch (atrac9Console)
                            {
                                case Constants.ATRAC9ConsoleType.PSV:
                                    if (!RunAtracTool(Generic.PSV_ATRAC9tool, Generic.DecodeParamAT9, file, outPath, p, cToken)) return false;
                                    break;
                                case Constants.ATRAC9ConsoleType.PS4:
                                    if (!RunAtracTool(Generic.PS4_ATRAC9tool, Generic.DecodeParamAT9, file, outPath, p, cToken)) return false;
                                    break;
                            }
                        }
                        break;
                    case ".OMA":
                        {
                            if (!DecodeWalkmanOmaToWave(file, outPath, p, cToken))
                                return false;

                            break;
                        }
                    case ".OMG":
                        {
                            if (ffEngine == null)
                            {
                                FormMain.DebugError("ffmpeg.exe not found (OMG decode requires it): " + ffpath);
                                return false;
                            }

                            // DRM だと失敗する可能性が高いです（失敗時は false で止める）
                            if (!RunMediaToolkitDecodeToWav(ffEngine, file, outPath, p, cToken))
                                return false;

                            break;
                        }
                    case ".NUS3BANK":
                    case ".NUB2":
                        {
                            if (!DecodeNus3Bank(file, null, p, cToken))
                                return false;

                            break;
                        }
                    default:
                        FormMain.DebugWarn($"[Decode] Unsupported extension. input={file}, ext={fi.Extension}");
                        return false;
                }
                FormMain.DebugInfo($"[Decode] File completed. index={i + 1}/{Generic.OpenFilePaths.Length}, input={file}");
            }
            FormMain.DebugInfo($"[Decode] Multiple files completed. files={Generic.OpenFilePaths.Length}");
            return true;
        }

        private static bool ShouldUseParallelPlaybackDecode()
        {
            if (!Generic.IsPlaybackConversion ||
                !Utils.GetBool("UseParallelMethod", false) ||
                Generic.OpenFilePaths.Length < 2)
            {
                return false;
            }

            return Generic.OpenFilePaths.All(path =>
            {
                string extension = Path.GetExtension(path);
                return extension.Equals(".aea", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".at3", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".at9", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".oma", StringComparison.OrdinalIgnoreCase);
            });
        }

        private static bool DecodeMultiplePlaybackFilesInParallel(
            IProgress<int> progress,
            CancellationToken cToken,
            Constants.ATRAC3ConsoleType atrac3Console,
            Constants.ATRAC9ConsoleType atrac9Console)
        {
            int threadCount = Math.Clamp(Utils.GetInt("PlaybackThreadCount", 3) + 1, 1, 8);
            int failed = 0;
            var options = new ParallelOptions
            {
                CancellationToken = cToken,
                MaxDegreeOfParallelism = threadCount
            };

            FormMain.DebugInfo($"[Decode] Parallel playback preview decode started. files={Generic.OpenFilePaths.Length}, threads={threadCount}");
            try
            {
                Parallel.ForEach(
                    Enumerable.Range(0, Generic.OpenFilePaths.Length),
                    options,
                    (index, state) =>
                    {
                        if (Volatile.Read(ref failed) != 0)
                        {
                            state.Stop();
                            return;
                        }

                        try
                        {
                            if (!DecodePlaybackFileAtIndex(index, progress, cToken, atrac3Console, atrac9Console))
                            {
                                Interlocked.Exchange(ref failed, 1);
                                state.Stop();
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            FormMain.DebugError($"[Decode] Parallel playback preview decode failed. index={index + 1}, error={ex}");
                            Interlocked.Exchange(ref failed, 1);
                            state.Stop();
                        }
                    });
            }
            catch (OperationCanceledException)
            {
                FormMain.DebugWarn("[Decode] Parallel playback preview decode cancelled.");
                return false;
            }

            bool result = Volatile.Read(ref failed) == 0;
            FormMain.DebugInfo($"[Decode] Parallel playback preview decode completed. files={Generic.OpenFilePaths.Length}, result={result}");
            return result;
        }

        private static bool DecodePlaybackFileAtIndex(
            int index,
            IProgress<int> progress,
            CancellationToken cToken,
            Constants.ATRAC3ConsoleType atrac3Console,
            Constants.ATRAC9ConsoleType atrac9Console)
        {
            cToken.ThrowIfCancellationRequested();
            string file = Generic.OpenFilePaths[index];
            string originKey = Generic.InputJobs != null && Generic.InputJobs.Count == Generic.OpenFilePaths.Length
                ? Generic.InputJobs[index].OriginPath
                : file;
            string outputPath = Utils.MakeTempUniquePath(TempDirectory, originKey, index, ".wav");
            string extension = Path.GetExtension(file).ToUpperInvariant();
            FormMain.DebugInfo($"[Decode] Parallel file started. index={index + 1}/{Generic.OpenFilePaths.Length}, ext={extension}, input={file}");

            bool result = extension switch
            {
                ".AEA" => DecodeMiniDiscAtrac1ToWave(file, outputPath, progress, cToken),
                ".AT3" => RunAtracTool(
                    Atrac3Tools[ResolveAtrac3DecoderConsole(file, atrac3Console)],
                    Generic.DecodeParamAT3,
                    file,
                    outputPath,
                    progress,
                    cToken),
                ".AT9" => RunAtracTool(
                    Atrac9Tools[atrac9Console],
                    Generic.DecodeParamAT9,
                    file,
                    outputPath,
                    progress,
                    cToken),
                ".OMA" => DecodeWalkmanOmaToWave(file, outputPath, progress, cToken),
                _ => false
            };

            if (result && (!File.Exists(outputPath) || new FileInfo(outputPath).Length == 0))
            {
                FormMain.DebugError($"[Decode] Parallel playback output validation failed. index={index + 1}, output={outputPath}");
                result = false;
            }

            FormMain.DebugInfo($"[Decode] Parallel file completed. index={index + 1}/{Generic.OpenFilePaths.Length}, result={result}, input={file}");
            return result;
        }

        private static FileInfo ResolveSingleDecodeTarget()
        {
            bool playbackAtrac = Utils.GetBool("PlaybackATRAC", false);
            bool fasterAtrac = Utils.GetBool("FasterATRAC", false);
            bool atracEncodeSource = Utils.GetBool("ATRACEncodeSource", false);

            if (playbackAtrac && Generic.IsATRAC && !fasterAtrac)
                return new FileInfo(Generic.pATRACSavePath);

            if (playbackAtrac && Generic.IsATRAC && fasterAtrac)
                return new FileInfo(Generic.SavePath);

            return atracEncodeSource
                ? new FileInfo(Generic.pATRACSavePath)
                : new FileInfo(Generic.SavePath);
        }

        private static bool DecodeNus3Bank(string file, string? singleOutputPath, IProgress<int> p, CancellationToken cToken)
        {
            var decodeItems = new List<Nus3BankDecodeWorkItem>();
            int completed = 0;
            int targetCount;

            using (var bank = Nus3BankFile.Load(file))
            {
                var targetTones = Generic.Nus3BankExtractEmbedded
                    ? bank.ExtractableTones.ToList()
                    : bank.DecodableWaveTones.ToList();
                if (targetTones.Count == 0)
                {
                    FormMain.DebugWarn(Localization.NotNUSoundDecodeTarget + file);
                    return false;
                }

                targetCount = targetTones.Count;
                bool useSingleOutput = !Generic.Nus3BankExtractEmbedded && targetTones.Count == 1 && !string.IsNullOrWhiteSpace(singleOutputPath);

                foreach (var tone in targetTones)
                {
                    if (cToken.IsCancellationRequested)
                        return false;

                    string encodedExt = GetEmbeddedSubfileExtension(tone.Codec);
                    string encodedPath = Path.Combine(TempDirectory, bank.MakeToneOutputName(tone, encodedExt));
                    string wavPath = useSingleOutput
                        ? singleOutputPath!
                        : Path.Combine(TempDirectory, bank.MakeToneOutputName(tone, ".wav"));

                    if (Generic.Nus3BankExtractEmbedded)
                    {
                        bank.ExtractToneToFile(tone, encodedPath);
                        completed++;
                        p.Report(completed);
                        continue;
                    }

                    if (tone.Codec == Nus3SubfileCodec.PcmWave)
                    {
                        bank.ExtractToneToFile(tone, wavPath);
                        if (Generic.IsPlaybackNus3Bank)
                            Generic.Nus3BankPlaybackTempPaths.Add(wavPath);

                        completed++;
                        p.Report(completed);
                        continue;
                    }

                    bank.ExtractToneToFile(tone, encodedPath);
                    decodeItems.Add(new Nus3BankDecodeWorkItem
                    {
                        Codec = tone.Codec,
                        EncodedPath = encodedPath,
                        WavPath = wavPath,
                    });
                }
            }

            var atrac3Console = (Constants.ATRAC3ConsoleType)Utils.GetInt("ATRAC3_Console", (int)Constants.ATRAC3ConsoleType.PSP);
            var atrac9Console = (Constants.ATRAC9ConsoleType)Utils.GetInt("ATRAC9_Console", (int)Constants.ATRAC9ConsoleType.PSV);

            foreach (Nus3BankDecodeWorkItem item in decodeItems)
            {
                if (cToken.IsCancellationRequested)
                    return false;

                bool decoded = item.Codec == Nus3SubfileCodec.Ivag
                    ? DecodeNus3BankIvagTone(item.EncodedPath, item.WavPath, p, cToken, () => completed)
                    : DecodeNus3BankAtracTone(item.Codec, item.EncodedPath, item.WavPath, atrac3Console, atrac9Console, p, cToken, () => completed);

                if (!decoded)
                    return false;

                if (Generic.IsPlaybackNus3Bank)
                    Generic.Nus3BankPlaybackTempPaths.Add(item.WavPath);

                try
                {
                    if (File.Exists(item.EncodedPath))
                        File.Delete(item.EncodedPath);
                }
                catch
                {
                    // Best effort cleanup only.
                }

                completed++;
                p.Report(completed);
            }

            Generic.Nus3BankOutputCount += targetCount;
            return true;
        }

        private static string GetEmbeddedSubfileExtension(Nus3SubfileCodec codec)
        {
            return codec switch
            {
                Nus3SubfileCodec.Atrac3 => ".at3",
                Nus3SubfileCodec.Atrac9 => ".at9",
                Nus3SubfileCodec.PcmWave => ".wav",
                Nus3SubfileCodec.RiffUnknown => ".riff",
                Nus3SubfileCodec.Bnsf => ".bnsf",
                Nus3SubfileCodec.Ivag => ".ivag",
                _ => ".bin",
            };
        }

        private static bool DecodeNus3BankIvagTone(
            string ivagPath,
            string wavPath,
            IProgress<int> p,
            CancellationToken cToken,
            Func<int>? progressValueProvider = null)
        {
            string tempWavPath = wavPath + ".tmp";

            try
            {
                using var ivagStream = new FileStream(
                    ivagPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete,
                    81920,
                    FileOptions.SequentialScan);
                if (!TryReadIvagInfo(ivagStream, out var info, out string error))
                {
                    FormMain.DebugWarn($"NUS3/NUB2 IVAG decode skipped: {error}");
                    return false;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(wavPath)!);
                TryDeleteFile(tempWavPath);
                WriteIvagPcmWav(ivagStream, info, tempWavPath, p, cToken, progressValueProvider);

                if (cToken.IsCancellationRequested)
                    return false;

                TryDeleteFile(wavPath);
                File.Move(tempWavPath, wavPath, overwrite: true);
                return File.Exists(wavPath) && new FileInfo(wavPath).Length > 0;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                FormMain.DebugWarn($"NUS3/NUB2 IVAG decode failed: {ex.Message}");
                return false;
            }
            finally
            {
                TryDeleteFile(tempWavPath);
            }
        }

        private static bool TryReadIvagInfo(Stream ivag, out IvagInfo info, out string error)
        {
            info = null!;
            error = string.Empty;

            if (!ivag.CanRead || !ivag.CanSeek || ivag.Length < 0x80)
            {
                error = "The stream does not start with IVAG.";
                return false;
            }

            Span<byte> header = stackalloc byte[0x30];
            ivag.Position = 0;
            if (!TryReadExactly(ivag, header) || !header[..4].SequenceEqual("IVAG"u8))
            {
                error = "The stream does not start with IVAG.";
                return false;
            }

            uint channelsRaw = ReadU32BE(header, 0x08);
            uint sampleRateRaw = ReadU32BE(header, 0x0C);
            uint sampleCountRaw = ReadU32BE(header, 0x10);
            uint dataOffsetRaw = ReadU32BE(header, 0x28);
            uint channelDataSizeRaw = ReadU32BE(header, 0x2C);

            if (channelsRaw is < 1 or > 8 || channelsRaw > int.MaxValue)
            {
                error = $"Unsupported IVAG channel count: {channelsRaw}.";
                return false;
            }

            if (sampleRateRaw is < 1000 or > 384000 || sampleRateRaw > int.MaxValue)
            {
                error = $"Unsupported IVAG sample rate: {sampleRateRaw}.";
                return false;
            }

            int channels = (int)channelsRaw;
            int sampleRate = (int)sampleRateRaw;
            int minimumDataOffset = 0x40 + channels * 0x40;
            if (dataOffsetRaw < minimumDataOffset || dataOffsetRaw > int.MaxValue || dataOffsetRaw >= ivag.Length)
            {
                error = $"Invalid IVAG data offset: 0x{dataOffsetRaw:X}.";
                return false;
            }

            Span<byte> vagHeader = stackalloc byte[4];
            for (int channel = 0; channel < channels; channel++)
            {
                int vagHeaderOffset = 0x40 + channel * 0x40;
                ivag.Position = vagHeaderOffset;
                if (!TryReadExactly(ivag, vagHeader) || !vagHeader.SequenceEqual("VAGp"u8))
                {
                    error = $"IVAG channel {channel} does not contain a VAGp header.";
                    return false;
                }
            }

            int dataOffset = (int)dataOffsetRaw;
            long availableData = ivag.Length - dataOffset;
            long maxChannelDataSizeLong = (availableData / channels) & ~0x0FL;
            int maxChannelDataSize = (int)Math.Min(maxChannelDataSizeLong, int.MaxValue & ~0x0F);
            int channelDataSize = channelDataSizeRaw <= int.MaxValue
                ? (int)channelDataSizeRaw
                : maxChannelDataSize;
            if (channelDataSize <= 0 || channelDataSize > maxChannelDataSize)
                channelDataSize = maxChannelDataSize;
            channelDataSize &= ~0x0F;

            if (channelDataSize <= 0)
            {
                error = "IVAG does not contain complete ADPCM frames.";
                return false;
            }

            int frameGroups = channelDataSize / 0x10;
            int sampleCapacity = (int)Math.Min((long)frameGroups * 28, int.MaxValue);
            int sampleCount = sampleCountRaw <= int.MaxValue
                ? (int)sampleCountRaw
                : sampleCapacity;
            if (sampleCount <= 0 || sampleCount > sampleCapacity)
                sampleCount = sampleCapacity;

            info = new IvagInfo
            {
                Channels = channels,
                SampleRate = sampleRate,
                SampleCount = sampleCount,
                DataOffset = dataOffset,
                ChannelDataSize = channelDataSize,
                FrameGroups = frameGroups,
            };
            return true;
        }

        private static void WriteIvagPcmWav(
            Stream ivag,
            IvagInfo info,
            string wavPath,
            IProgress<int> progress,
            CancellationToken cToken,
            Func<int>? progressValueProvider)
        {
            long dataSize = (long)info.SampleCount * info.Channels * sizeof(short);
            if (dataSize > uint.MaxValue - 36u)
                throw new InvalidDataException(Localization.DecodedIVAGLargeSize);

            using var fs = new FileStream(
                wavPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.SequentialScan);
            using var writer = new BinaryWriter(fs, Encoding.ASCII, leaveOpen: false);
            WriteWaveHeader(writer, info.Channels, info.SampleRate, (uint)dataSize);

            int[] previous1 = new int[info.Channels];
            int[] previous2 = new int[info.Channels];
            short[][] decoded = new short[info.Channels][];
            for (int channel = 0; channel < info.Channels; channel++)
                decoded[channel] = new short[28];

            Span<byte> compressedFrames = stackalloc byte[8 * 0x10];
            Span<byte> pcmFrames = stackalloc byte[28 * 8 * sizeof(short)];
            ivag.Position = info.DataOffset;
            int samplesWritten = 0;
            for (int group = 0; group < info.FrameGroups && samplesWritten < info.SampleCount; group++)
            {
                if ((group & 0x1FF) == 0)
                {
                    cToken.ThrowIfCancellationRequested();
                    if (Directory.Exists(TempDirectory))
                        progress.Report(progressValueProvider?.Invoke() ?? CountTopDirectoryFiles(TempDirectory));
                }

                Span<byte> currentCompressedFrames = compressedFrames[..(info.Channels * 0x10)];
                if (!TryReadExactly(ivag, currentCompressedFrames))
                    throw new EndOfStreamException("IVAG ended before all ADPCM frames were decoded.");

                for (int channel = 0; channel < info.Channels; channel++)
                {
                    DecodePsxAdpcmFrame(
                        currentCompressedFrames.Slice(channel * 0x10, 0x10),
                        ref previous1[channel],
                        ref previous2[channel],
                        decoded[channel]);
                }

                int samplesThisGroup = Math.Min(28, info.SampleCount - samplesWritten);
                int pcmOffset = 0;
                for (int sample = 0; sample < samplesThisGroup; sample++)
                {
                    for (int channel = 0; channel < info.Channels; channel++)
                    {
                        BinaryPrimitives.WriteInt16LittleEndian(
                            pcmFrames.Slice(pcmOffset, sizeof(short)),
                            decoded[channel][sample]);
                        pcmOffset += sizeof(short);
                    }
                }
                writer.Write(pcmFrames[..pcmOffset]);

                samplesWritten += samplesThisGroup;
            }
        }

        private static void DecodePsxAdpcmFrame(ReadOnlySpan<byte> block, ref int previous1, ref int previous2, short[] output)
        {
            int shift = block[0] & 0x0F;
            int predictor = (block[0] >> 4) & 0x0F;
            if (predictor >= PsxAdpcmCoefficients.GetLength(0))
                predictor = 0;

            int coefficient1 = PsxAdpcmCoefficients[predictor, 0];
            int coefficient2 = PsxAdpcmCoefficients[predictor, 1];

            for (int i = 0; i < 28; i++)
            {
                int packed = block[2 + (i / 2)];
                int nibble = (i & 1) == 0
                    ? packed & 0x0F
                    : packed >> 4;
                if (nibble >= 8)
                    nibble -= 16;

                int sample = (nibble << 12) >> shift;
                sample += (previous1 * coefficient1 + previous2 * coefficient2 + 32) >> 6;
                sample = Math.Clamp(sample, short.MinValue, short.MaxValue);

                output[i] = (short)sample;
                previous2 = previous1;
                previous1 = sample;
            }
        }

        private static bool TryReadExactly(Stream stream, Span<byte> buffer)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int read = stream.Read(buffer[offset..]);
                if (read <= 0)
                    return false;

                offset += read;
            }

            return true;
        }

        private static uint ReadU32BE(ReadOnlySpan<byte> data, int offset)
        {
            return BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, sizeof(uint)));
        }

        private static void WriteWaveHeader(BinaryWriter writer, int channels, int sampleRate, uint dataSize)
        {
            ushort bitsPerSample = 16;
            ushort blockAlign = checked((ushort)(channels * bitsPerSample / 8));
            uint byteRate = checked((uint)(sampleRate * blockAlign));

            WriteAscii(writer, "RIFF");
            writer.Write(checked(36u + dataSize));
            WriteAscii(writer, "WAVE");
            WriteAscii(writer, "fmt ");
            writer.Write(16u);
            writer.Write((ushort)1);
            writer.Write((ushort)channels);
            writer.Write((uint)sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
            WriteAscii(writer, "data");
            writer.Write(dataSize);
        }

        private static void WriteAscii(BinaryWriter writer, string value)
        {
            writer.Write(Encoding.ASCII.GetBytes(value));
        }

        private static bool DecodeNus3BankAtracTone(
            Nus3SubfileCodec codec,
            string encodedPath,
            string wavPath,
            Constants.ATRAC3ConsoleType atrac3Console,
            Constants.ATRAC9ConsoleType atrac9Console,
            IProgress<int> p,
            CancellationToken cToken,
            Func<int>? progressValueProvider = null)
        {
            Constants.ATRAC3ConsoleType selectedAtrac3Console = codec == Nus3SubfileCodec.Atrac3
                ? ResolveAtrac3DecoderConsole(encodedPath, atrac3Console)
                : atrac3Console;
            IEnumerable<string> tools = codec == Nus3SubfileCodec.Atrac9
                ? BuildToolFallbackList(Atrac9Tools, atrac9Console)
                : BuildToolFallbackList(Atrac3Tools, selectedAtrac3Console);
            string parameter = codec == Nus3SubfileCodec.Atrac9
                ? Generic.DecodeParamAT9
                : Generic.DecodeParamAT3;

            foreach (string tool in tools)
            {
                TryDeleteFile(wavPath);
                FormMain.DebugInfo($"NUS3/NUB2 decode: {codec} using {Path.GetFileName(tool)}");

                bool ran;
                try
                {
                    ran = RunAtracTool(
                        tool,
                        parameter,
                        encodedPath,
                        wavPath,
                        p,
                        cToken,
                        progressValueProvider,
                        forceLoopExpandedDecodeNormalization: true);
                }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"NUS3/NUB2 decode failed to start {Path.GetFileName(tool)}: {ex.Message}");
                    ran = false;
                }

                if (cToken.IsCancellationRequested)
                    return false;

                if (ran && File.Exists(wavPath) && new FileInfo(wavPath).Length > 0)
                    return true;

                string log = ReadCurrentProcessLog();
                if (!string.IsNullOrWhiteSpace(log))
                    FormMain.DebugWarn($"NUS3/NUB2 decode failed with {Path.GetFileName(tool)}: {log}");
                else
                    FormMain.DebugWarn($"NUS3/NUB2 decode failed with {Path.GetFileName(tool)}.");
            }

            TryDeleteFile(wavPath);
            return false;
        }

        private static Constants.ATRAC3ConsoleType ResolveAtrac3DecoderConsole(
            string path,
            Constants.ATRAC3ConsoleType configuredConsole)
        {
            Constants.ATRAC3ConsoleType fallback = Atrac3Tools.ContainsKey(configuredConsole)
                ? configuredConsole
                : Constants.ATRAC3ConsoleType.PSP;

            if (!Nus3BankFile.TryReadRiffWaveSampleRate(path, out int sampleRate))
            {
                FormMain.DebugWarn($"[Decode] ATRAC3 sample rate could not be detected. Using configured decoder. input={path}, decoder={fallback}");
                return fallback;
            }

            Constants.ATRAC3ConsoleType? detectedConsole = sampleRate switch
            {
                44100 => Constants.ATRAC3ConsoleType.PSP,
                48000 => Constants.ATRAC3ConsoleType.PS3,
                _ => null,
            };

            if (!detectedConsole.HasValue)
            {
                FormMain.DebugWarn($"[Decode] Unsupported ATRAC3 sample rate. Using configured decoder. input={path}, sampleRate={sampleRate}, decoder={fallback}");
                return fallback;
            }

            FormMain.DebugInfo($"[Decode] ATRAC3 decoder auto-detected. input={path}, sampleRate={sampleRate}, decoder={detectedConsole.Value}");
            return detectedConsole.Value;
        }

        private static IEnumerable<string> BuildToolFallbackList<T>(IReadOnlyDictionary<T, string> tools, T selected)
            where T : notnull
        {
            if (tools.TryGetValue(selected, out string? selectedTool))
                yield return selectedTool;

            foreach (var pair in tools)
            {
                if (!EqualityComparer<T>.Default.Equals(pair.Key, selected))
                    yield return pair.Value;
            }
        }

        private static string ReadCurrentProcessLog()
        {
            try
            {
                if (Generic.Log is null)
                    return string.Empty;

                return Utils.LogSplit(Generic.Log) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }
        }

        private static bool Encode_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            Config.Load(xmlpath);

            int length = Generic.OpenFilePaths.Length;
            FormMain.DebugInfo($"[Encode] Dispatch. files={length}, atracFlag={Generic.ATRACFlag}, nus3bankOutput={Generic.Nus3BankEncodeOutput}");

            if (length == 1) // Single
            {
                return EncodeSingleFile(p, cToken);
            }
            else // multiple
            {
                return EncodeMultipleFiles(p, cToken);
            }
        }

        /// <summary>
        /// 単一ファイルのATRACエンコード処理
        /// </summary>
        private static bool EncodeSingleFile(IProgress<int> p, CancellationToken cToken)
        {
            bool mloop = Generic.MultipleFilesLoopOKFlags[0];
            bool atracEncodeSource = Utils.GetBool("ATRACEncodeSource", false);
            FormMain.DebugInfo($"[Encode] Single file started. atracFlag={Generic.ATRACFlag}, loop={mloop}, atracEncodeSource={atracEncodeSource}");

            string atrac3Params = Utils.GetString("ATRAC3_Params", string.Empty);
            string atrac9Params = Utils.GetString("ATRAC9_Params", string.Empty);

            FileInfo fi, fi2;

            if (atracEncodeSource && Generic.IsATRAC)
            {
                fi = new(Generic.pATRACOpenFilePaths[0]);
                fi2 = new(Generic.SavePath);
            }
            else
            {
                fi = new(Generic.OpenFilePaths[0]);
                fi2 = new(Generic.SavePath);
            }

            if (Generic.IsMiniDisc)
                return EncodeMiniDiscSingle(fi, fi2, p, cToken);

            switch (Generic.ATRACFlag)
            {
                case 0: // ATRAC3
                    {
                        var atrac3Console = (Constants.ATRAC3ConsoleType)Utils.GetInt("ATRAC3_Console", (int)Constants.ATRAC3ConsoleType.PSP);
                        if (!Atrac3Tools.TryGetValue(atrac3Console, out string? atrac3Tool))
                        {
                            FormMain.DebugError($"[Encode] Unsupported ATRAC3 console selected. console={atrac3Console}");
                            return false;
                        }

                        bool isAtrac3Ps3 = atrac3Console == Constants.ATRAC3ConsoleType.PS3;
                        int targetSamplingRate = isAtrac3Ps3 ? 48000 : 44100;
                        string encoderInputPath = fi.FullName;
                        int sourceSamplingRate = targetSamplingRate;
                        if (!TryPrepareAtracEncodeInput(
                                fi.FullName,
                                fi.FullName,
                                0,
                                targetSamplingRate,
                                "ATRAC3",
                                out encoderInputPath,
                                out sourceSamplingRate))
                        {
                            return false;
                        }

                        string? resampledInputPath = string.Equals(encoderInputPath, fi.FullName, StringComparison.OrdinalIgnoreCase)
                            ? null
                            : encoderInputPath;
                        string outPath = BuildSingleAtracTempOutputPath(fi2, ".at3");

                        try
                        {
                            Generic.EncodeParamAT3 = BuildAtracEncodeParam(
                                atrac3Params,
                                mloop,
                                Generic.MultipleLoopStarts.Length > 0 ? Generic.MultipleLoopStarts[0] : 0,
                                Generic.MultipleLoopEnds.Length > 0 ? Generic.MultipleLoopEnds[0] : 0,
                                Generic.lpcreate,
                                fileIndex: 0);

                            if (sourceSamplingRate != targetSamplingRate &&
                                !TryScaleLoopInEncodeParam(ref Generic.EncodeParamAT3, sourceSamplingRate, targetSamplingRate, "ATRAC3", fi.FullName))
                            {
                                FormMain.DebugError($"[Encode] ATRAC3 loop scaling failed. rate={sourceSamplingRate}->{targetSamplingRate}, input={fi.FullName}");
                                return false;
                            }

                            if (!RunAtracTool(
                                    atrac3Tool,
                                    Generic.EncodeParamAT3,
                                    encoderInputPath,
                                    outPath,
                                    p,
                                    cToken))
                            {
                                return false;
                            }

                            if (!TryNormalizeAtrac3LoopMetadata(outPath, Generic.EncodeParamAT3, isAtrac3Ps3))
                                return false;

                            if (!WrapSingleNus3BankIfRequested(
                                    outPath,
                                    fi2,
                                    fi,
                                    BuildNus3RiffLoopPoints(outPath, Generic.EncodeParamAT3, isAtrac9: false, isAtrac3Ps3: isAtrac3Ps3, sourceInputPath: fi.FullName)))
                            {
                                return false;
                            }
                        }
                        finally
                        {
                            if (resampledInputPath is not null)
                                TryDeleteFile(resampledInputPath);
                        }
                    }
                    break;
                case 1: // ATRAC9
                    {
                        var atrac9Console = (Constants.ATRAC9ConsoleType)Utils.GetInt("ATRAC9_Console", (int)Constants.ATRAC9ConsoleType.PSV);

                        string outPath = BuildSingleAtracTempOutputPath(fi2, ".at9");

                        switch (atrac9Console)
                        {
                            case Constants.ATRAC9ConsoleType.PSV: // PSV
                                {
                                    if (mloop)
                                    {
                                        Generic.EncodeParamAT9 = atrac9Params;
                                        Generic.EncodeParamAT9 += " -loop " + Generic.MultipleLoopStarts[0] + " " + Generic.MultipleLoopEnds[0];
                                    }
                                    else
                                    {
                                        if (Generic.lpcreate != false)
                                        {
                                            Generic.lpcreatev2 = true;
                                            using FormLPC form = new(true);
                                            FormProgressInstance.Invoke(new Action(() => FormProgressInstance.Enabled = false));
                                            form.ShowDialog();
                                            FormProgressInstance.Invoke(new Action(() => FormProgressInstance.Enabled = true));
                                            Generic.EncodeParamAT9 = atrac9Params;
                                            Generic.EncodeParamAT9 += Generic.LPCSuffix;
                                        }
                                        else
                                        {
                                            Generic.EncodeParamAT9 = atrac9Params;
                                        }
                                    }


                                    if (!RunAtracTool(
                                    Generic.PSV_ATRAC9tool,
                                    Generic.EncodeParamAT9,
                                    fi.FullName,
                                    outPath,
                                    p,
                                    cToken))
                                    {
                                        return false;
                                    }
                                }
                                break;
                            case Constants.ATRAC9ConsoleType.PS4: // PS4
                                {
                                    if (mloop)
                                    {
                                        Generic.EncodeParamAT9 = atrac9Params;
                                        Generic.EncodeParamAT9 += " -loop " + Generic.MultipleLoopStarts[0] + " " + Generic.MultipleLoopEnds[0];
                                    }
                                    else
                                    {
                                        if (Generic.lpcreate != false)
                                        {
                                            Generic.lpcreatev2 = true;
                                            using FormLPC form = new(true);
                                            FormProgressInstance.Invoke(new Action(() => FormProgressInstance.Enabled = false));
                                            form.ShowDialog();
                                            FormProgressInstance.Invoke(new Action(() => FormProgressInstance.Enabled = true));
                                            Generic.EncodeParamAT9 = atrac9Params;
                                            Generic.EncodeParamAT9 += Generic.LPCSuffix;
                                        }
                                        else
                                        {
                                            Generic.EncodeParamAT9 = atrac9Params;
                                        }
                                    }


                                    if (!RunAtracTool(
                                    Generic.PS4_ATRAC9tool,
                                    Generic.EncodeParamAT9,
                                    fi.FullName,
                                    outPath,
                                    p,
                                    cToken))
                                    {
                                        return false;
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                        if (!WrapSingleNus3BankIfRequested(
                                outPath,
                                fi2,
                                fi,
                                BuildNus3RiffLoopPoints(outPath, Generic.EncodeParamAT9, isAtrac9: true, isAtrac3Ps3: false, sourceInputPath: fi.FullName)))
                        {
                            return false;
                        }
                    }
                    break;
                case 2: // Walkman
                    {
                        string outPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "_temp",
                            fi2.Name);

                        var job = Common.Generic.InputJobs[0];
                        bool unattended = Utils.GetBool("Walkman_Unattended", false);
                        bool useConfiguredMetadata = Utils.GetBool("Walkman_FixSongInformation", false);

                        if (useConfiguredMetadata)
                            Common.Utils.ApplyConfiguredWalkmanMeta(job);

                        // 入力メタデータを使う場合だけ、必要に応じて確認画面を表示する。
                        if (!useConfiguredMetadata && !unattended)
                        {
                            FormMain.DebugInfo("Walkman_Params after form: " + Utils.GetString("Walkman_Params", ""));
                            Common.Generic.CurrentWalkmanInputFile = Generic.OriginOpenFilePaths[0];
                            FormMain.DebugInfo("Loading File: " + Generic.OriginOpenFilePaths[0]);

                            // ★UIスレッドで ShowDialog し、その戻り値を取得
                            var dr = (DialogResult)FormProgressInstance.Invoke(new Func<DialogResult>(() =>
                            {
                                using var form = new FormWalkmanInformations(job);
                                FormProgressInstance.Enabled = false;
                                try
                                {
                                    return form.ShowDialog(FormProgressInstance);
                                }
                                finally
                                {
                                    FormProgressInstance.Enabled = true;
                                }
                            }));

                            // ★OK以外は全体中止にする
                            if (dr != DialogResult.OK)
                            {
                                try { Common.Generic.cts?.Cancel(); } catch { }
                                return false; // ←ここで上位に伝播できる
                            }
                            /*FormProgressInstance.Invoke(new Action(() =>
                            {
                                using var form = new FormWalkmanInformations(job);
                                FormProgressInstance.Enabled = false;
                                if (form.ShowDialog(FormProgressInstance) != DialogResult.OK)
                                {
                                    FormProgressInstance.Enabled = true;
                                    return;
                                }
                            }));*/
                        }

                        string walkmanParam = Common.Utils.BuildTraConvArgsForJob(
                            job,
                            job.WorkPath,   // WAV or 元ファイル
                            outPath
                        );


                        //string baseParam = Utils.GetString("Walkman_Params", Generic.EncodeParamWalkman);
                        // タグは元ファイル（Origin）から取得
                        //string walkmanParam = BuildWalkmanParamForFile(baseParam, Generic.OriginOpenFilePaths[0]);
                        // タグをパラメータに反映（大小文字の問題を後述の修正2で解決）
                        // ここは「元ファイル」があるならそちらを渡すのが理想
                        //string tagSourcePath = Generic.OriginOpenFilePaths[0];
                        //walkmanParam = BuildWalkmanParamForFile(walkmanParam, tagSourcePath);
                        FormMain.DebugInfo("walkmanParam: " + walkmanParam);

                        if (!RunAtracTool(
                            Generic.Walkman_TraConv,
                            walkmanParam,
                            job.WorkPath,
                            outPath,
                            p,
                            cToken))
                        {
                            return false;
                        }
                        break;
                    }
                default:
                    FormMain.DebugWarn($"[Encode] Unsupported ATRAC flag for single encode. atracFlag={Generic.ATRACFlag}");
                    return false;
            }
            FormMain.DebugInfo($"[Encode] Single file completed. input={fi.FullName}, output={fi2.FullName}");
            return true;
        }

        private static string BuildSingleAtracTempOutputPath(FileInfo requestedOutput, string atracExtension)
        {
            string outputName = Nus3BankFile.HasNus3BankExtension(requestedOutput.FullName)
                ? Path.ChangeExtension(requestedOutput.Name, atracExtension)
                : requestedOutput.Name;

            return Path.Combine(TempDirectory, outputName);
        }

        private static bool EncodeMiniDiscSingle(FileInfo input, FileInfo requestedOutput, IProgress<int> progress, CancellationToken cToken)
        {
            var profile = GetMiniDiscEncodeProfile(Generic.MiniDiscEncodeMode);
            string encoderInputPath = input.FullName;
            if (!TryPrepareAtracEncodeInput(
                    input.FullName,
                    input.FullName,
                    0,
                    44100,
                    profile.Name,
                    out encoderInputPath,
                    out _))
            {
                return false;
            }

            string? resampledInputPath = string.Equals(encoderInputPath, input.FullName, StringComparison.OrdinalIgnoreCase)
                ? null
                : encoderInputPath;
            string outPath = Path.Combine(TempDirectory, Path.ChangeExtension(requestedOutput.Name, profile.Extension));
            Generic.ATRACExt = profile.Extension;

            try
            {
                FormMain.DebugInfo($"[MiniDisc] Single encode started. mode={Generic.MiniDiscEncodeMode}, input={input.FullName}, output={outPath}");
                return RunMiniDiscTool(profile.ToolPath, profile.Name, profile.Parameters, encoderInputPath, outPath, progress, cToken);
            }
            finally
            {
                if (resampledInputPath is not null)
                    TryDeleteFile(resampledInputPath);
            }
        }

        private static bool EncodeMiniDiscMultiple(string[] inputPaths, IProgress<int> progress, CancellationToken cToken)
        {
            var profile = GetMiniDiscEncodeProfile(Generic.MiniDiscEncodeMode);
            Generic.ATRACExt = profile.Extension;

            for (int i = 0; i < inputPaths.Length; i++)
            {
                cToken.ThrowIfCancellationRequested();
                string inputPath = inputPaths[i];
                string originPath = Generic.InputJobs.Count == inputPaths.Length
                    ? Generic.InputJobs[i].OriginPath
                    : inputPath;
                string encoderInputPath = inputPath;

                if (!TryPrepareAtracEncodeInput(
                        inputPath,
                        originPath,
                        i,
                        44100,
                        profile.Name,
                        out encoderInputPath,
                        out _))
                {
                    return false;
                }

                string? resampledInputPath = string.Equals(encoderInputPath, inputPath, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : encoderInputPath;
                string outPath = Utils.MakeTempUniquePath(TempDirectory, originPath, i, profile.Extension);

                try
                {
                    FormMain.DebugInfo($"[MiniDisc] File started. index={i + 1}/{inputPaths.Length}, mode={Generic.MiniDiscEncodeMode}, input={inputPath}, output={outPath}");
                    if (!RunMiniDiscTool(profile.ToolPath, profile.Name, profile.Parameters, encoderInputPath, outPath, progress, cToken))
                        return false;
                }
                finally
                {
                    if (resampledInputPath is not null)
                        TryDeleteFile(resampledInputPath);
                }
            }

            FormMain.DebugInfo($"[MiniDisc] Multiple encode completed. files={inputPaths.Length}, mode={Generic.MiniDiscEncodeMode}");
            return true;
        }

        private static (string ToolPath, string Parameters, string Extension, string Name) GetMiniDiscEncodeProfile(Constants.MiniDiscMode mode)
        {
            return mode switch
            {
                Constants.MiniDiscMode.SP => (Generic.ATRAC1tool, "-e atrac1 -i $InFile -o $OutFile", ".aea", "MiniDisc SP (ATRAC1)"),
                Constants.MiniDiscMode.LP4 => (Generic.PSP_ATRAC3tool, "at3tool -e -br 66 $InFile $OutFile", ".at3", "MiniDisc LP4 (ATRAC3 66 kbps)"),
                _ => (Generic.PSP_ATRAC3tool, "at3tool -e -br 132 $InFile $OutFile", ".at3", "MiniDisc LP2 (ATRAC3 132 kbps)")
            };
        }

        private static bool WrapSingleNus3BankIfRequested(string encodedPath, FileInfo requestedOutput, FileInfo sourceInput, Nus3RiffLoopPoints? loopPoints)
        {
            if (!Nus3BankFile.HasNus3BankExtension(requestedOutput.FullName))
                return true;

            if (!File.Exists(encodedPath) || new FileInfo(encodedPath).Length == 0)
                return false;

            string outputPath = Path.Combine(TempDirectory, requestedOutput.Name);
            return WriteNus3BankFromEncodedFile(encodedPath, outputPath, sourceInput.FullName, loopPoints);
        }

        private static bool AddMultipleNus3BankStreamIfRequested(List<Nus3BankEncodedStreamSource>? streams, string encodedPath, string sourceInputPath, string originPath, string streamName, string encodeParam, bool isAtrac9, bool isAtrac3Ps3)
        {
            if (!Generic.Nus3BankEncodeOutput)
                return true;

            if (streams is null || !File.Exists(encodedPath) || new FileInfo(encodedPath).Length == 0)
                return false;

            streams.Add(new Nus3BankEncodedStreamSource
            {
                EncodedPath = encodedPath,
                OriginPath = originPath,
                StreamName = streamName,
                LoopPoints = BuildNus3RiffLoopPoints(encodedPath, encodeParam, isAtrac9, isAtrac3Ps3, sourceInputPath),
            });

            return true;
        }

        private static bool WriteMultiStreamNus3Bank(IReadOnlyList<Nus3BankEncodedStreamSource> streams)
        {
            if (streams.Count == 0 || string.IsNullOrWhiteSpace(Generic.SavePath))
                return false;

            string outputName = Path.GetFileName(Generic.SavePath);
            if (string.IsNullOrWhiteSpace(outputName))
                outputName = "output.nus3bank";
            if (!Nus3BankFile.HasNus3BankExtension(outputName))
                outputName = Path.ChangeExtension(outputName, ".nus3bank");

            string outputPath = Path.Combine(TempDirectory, outputName);
            string bankName = Path.GetFileNameWithoutExtension(outputName);
            Nus3BankBuildFlavor flavor = Nus3BankFile.HasNub2Extension(outputName)
                ? Nus3BankBuildFlavor.Nub2
                : Nus3BankBuildFlavor.Nus3Bank;

            try
            {
                var bankStreams = new List<Nus3BankEncodeStream>(streams.Count);
                foreach (Nus3BankEncodedStreamSource stream in streams)
                {
                    if (!File.Exists(stream.EncodedPath) || new FileInfo(stream.EncodedPath).Length == 0)
                        return false;

                    bankStreams.Add(new Nus3BankEncodeStream
                    {
                        Name = stream.StreamName,
                        DataPath = stream.EncodedPath,
                        LoopPoints = stream.LoopPoints,
                    });
                }

                Nus3BankFile.WriteMultiToneBank(bankStreams, outputPath, bankName, flavor);

                foreach (Nus3BankEncodedStreamSource stream in streams)
                {
                    if (File.Exists(stream.EncodedPath))
                        File.Delete(stream.EncodedPath);
                }
            }
            catch (Exception ex)
            {
                FormMain.DebugError("NUS3BANK multi encode failed: " + ex.Message);
                return false;
            }

            Generic.ATRACExt = flavor == Nus3BankBuildFlavor.Nub2 ? ".nub2" : ".nus3bank";
            return File.Exists(outputPath) && new FileInfo(outputPath).Length > 0;
        }

        private static string ResolveMultipleEncodeOriginPath(string workPath, int index, int inputCount)
        {
            if (Generic.InputJobs.Count == inputCount && index >= 0 && index < Generic.InputJobs.Count)
                return Generic.InputJobs[index].OriginPath;

            InputJob? job = Generic.InputJobs.FirstOrDefault(j => string.Equals(j.WorkPath, workPath, StringComparison.OrdinalIgnoreCase));
            return job?.OriginPath ?? workPath;
        }

        private static List<MultipleEncodeItem> BuildMultipleEncodeItems(string[] inputPaths)
        {
            if (Generic.Nus3BankEncodeOutput && Generic.Nus3BankEncodeStreamSettings.Count > 0)
            {
                var ordered = new List<MultipleEncodeItem>();
                foreach (Nus3BankEncodeStreamSetting setting in Generic.Nus3BankEncodeStreamSettings)
                {
                    if (!TryResolveNus3BankEncodeInputPath(setting.SourceIndex, inputPaths, out string inputPath, out string originPath))
                        continue;

                    ordered.Add(new MultipleEncodeItem
                    {
                        InputPath = inputPath,
                        OriginPath = originPath,
                        StreamName = string.IsNullOrWhiteSpace(setting.StreamName)
                            ? Path.GetFileNameWithoutExtension(originPath)
                            : setting.StreamName.Trim(),
                        SourceIndex = setting.SourceIndex,
                    });
                }

                if (ordered.Count > 0)
                    return ordered;
            }

            var fallback = new List<MultipleEncodeItem>(inputPaths.Length);
            for (int i = 0; i < inputPaths.Length; i++)
            {
                string originPath = ResolveMultipleEncodeOriginPath(inputPaths[i], i, inputPaths.Length);
                fallback.Add(new MultipleEncodeItem
                {
                    InputPath = inputPaths[i],
                    OriginPath = originPath,
                    StreamName = Path.GetFileNameWithoutExtension(originPath),
                    SourceIndex = i,
                });
            }

            return fallback;
        }

        private static bool TryResolveNus3BankEncodeInputPath(int sourceIndex, string[] inputPaths, out string inputPath, out string originPath)
        {
            inputPath = string.Empty;
            originPath = string.Empty;

            if (sourceIndex >= 0 && sourceIndex < Generic.InputJobs.Count)
            {
                InputJob job = Generic.InputJobs[sourceIndex];
                originPath = string.IsNullOrWhiteSpace(job.OriginPath) ? job.WorkPath : job.OriginPath;
                inputPath = job.WorkPath;

                if (!string.IsNullOrWhiteSpace(inputPath) && File.Exists(inputPath))
                    return true;
            }

            if (sourceIndex >= 0 && sourceIndex < inputPaths.Length)
            {
                inputPath = inputPaths[sourceIndex];
                originPath = ResolveMultipleEncodeOriginPath(inputPath, sourceIndex, inputPaths.Length);
                return true;
            }

            return false;
        }

        private static bool WriteNus3BankFromEncodedFile(string encodedPath, string outputPath, string sourceInputPath, Nus3RiffLoopPoints? loopPoints)
        {
            if (!File.Exists(encodedPath) || new FileInfo(encodedPath).Length == 0)
                return false;

            string bankName = Path.GetFileNameWithoutExtension(outputPath);
            string toneName = Path.GetFileNameWithoutExtension(sourceInputPath);

            try
            {
                Nus3BankBuildFlavor flavor = Nus3BankFile.HasNub2Extension(outputPath)
                    ? Nus3BankBuildFlavor.Nub2
                    : Nus3BankBuildFlavor.Nus3Bank;

                Nus3BankFile.WriteSingleToneBank(encodedPath, outputPath, bankName, toneName, loopPoints, flavor);
                File.Delete(encodedPath);
            }
            catch (Exception ex)
            {
                FormMain.DebugError("NUS3BANK encode failed: " + ex.Message);
                return false;
            }

            return File.Exists(outputPath) && new FileInfo(outputPath).Length > 0;
        }

        private static Nus3RiffLoopPoints? BuildNus3RiffLoopPoints(string encodedPath, string encodeParam, bool isAtrac9, bool isAtrac3Ps3, string? sourceInputPath = null)
        {
            // at9tool converts loop positions to the output sample rate and writes them to smpl.
            // Preserve that chunk so a 44.1 kHz source is not overwritten with unscaled values.
            if (isAtrac9 && Nus3BankFile.TryReadRiffLoopChunk(encodedPath, out uint encodedLoopStart, out uint encodedLoopEnd))
            {
                FormMain.DebugInfo($"[NUS3BANK] Preserving ATRAC9 loop metadata. rawStart={encodedLoopStart}, rawEnd={encodedLoopEnd}, input={encodedPath}");
                return null;
            }

            if (!TryGetLoopPointSamples(encodeParam, out int loopStart, out int loopEnd))
                return null;

            if (isAtrac9 &&
                !string.IsNullOrWhiteSpace(sourceInputPath) &&
                Nus3BankFile.TryReadRiffWaveSampleRate(sourceInputPath, out int sourceSampleRate) &&
                Nus3BankFile.TryReadRiffWaveSampleRate(encodedPath, out int outputSampleRate) &&
                sourceSampleRate != outputSampleRate)
            {
                if (!TryScaleLoopPoints(loopStart, loopEnd, sourceSampleRate, outputSampleRate, out int scaledStart, out int scaledEnd))
                {
                    FormMain.DebugWarn($"[NUS3BANK] ATRAC9 loop scaling failed. start={loopStart}, end={loopEnd}, sourceRate={sourceSampleRate}, outputRate={outputSampleRate}");
                    return null;
                }

                FormMain.DebugInfo($"[NUS3BANK] ATRAC9 loop metadata scaled for fallback. start={loopStart}->{scaledStart}, end={loopEnd}->{scaledEnd}, rate={sourceSampleRate}->{outputSampleRate}");
                loopStart = scaledStart;
                loopEnd = scaledEnd;
            }

            (int startAdjustment, int endAdjustment) = isAtrac9
                ? GetAtrac9LoopAdjustments(encodedPath)
                : isAtrac3Ps3
                    ? (3271, 3270)
                    : (2459, 2458);

            var loopPoints = new Nus3RiffLoopPoints(loopStart, loopEnd, startAdjustment, endAdjustment);
            if (!isAtrac9 &&
                loopPoints.TryGetRiffLoop(out uint expectedLoopStart, out uint expectedLoopEnd) &&
                Nus3BankFile.TryReadRiffLoopChunk(encodedPath, out uint normalizedLoopStart, out uint normalizedLoopEnd) &&
                normalizedLoopStart == expectedLoopStart &&
                normalizedLoopEnd == expectedLoopEnd)
            {
                FormMain.DebugInfo($"[NUS3BANK] Preserving normalized ATRAC3 loop metadata. rawStart={normalizedLoopStart}, rawEnd={normalizedLoopEnd}, input={encodedPath}");
                return null;
            }

            return loopPoints;
        }

        private static bool TryNormalizeAtrac3LoopMetadata(string encodedPath, string encodeParam, bool isAtrac3Ps3)
        {
            if (!TryGetLoopPointSamples(encodeParam, out int loopStart, out int loopEnd))
                return true;

            (int startAdjustment, int endAdjustment) = isAtrac3Ps3
                ? (3271, 3270)
                : (2459, 2458);
            var loopPoints = new Nus3RiffLoopPoints(loopStart, loopEnd, startAdjustment, endAdjustment);
            if (!loopPoints.TryGetRiffLoop(out uint rawLoopStart, out uint rawLoopEnd))
            {
                FormMain.DebugError($"[Encode] ATRAC3 loop metadata is invalid. start={loopStart}, end={loopEnd}, input={encodedPath}");
                return false;
            }

            if (!Nus3BankFile.TryWriteRiffLoopChunk(encodedPath, loopPoints))
            {
                FormMain.DebugError($"[Encode] ATRAC3 loop metadata normalization failed. rawStart={rawLoopStart}, rawEnd={rawLoopEnd}, input={encodedPath}");
                return false;
            }

            FormMain.DebugInfo($"[Encode] ATRAC3 loop metadata normalized. rawStart={rawLoopStart}, rawEnd={rawLoopEnd}, console={(isAtrac3Ps3 ? "PS3" : "PSP")}, input={encodedPath}");
            return true;
        }

        private static bool TryScaleLoopPoints(int loopStart, int loopEnd, int sourceSampleRate, int outputSampleRate, out int scaledStart, out int scaledEnd)
        {
            scaledStart = 0;
            scaledEnd = 0;
            if (loopStart < 0 || loopEnd <= loopStart || sourceSampleRate <= 0 || outputSampleRate <= 0)
                return false;

            long convertedStart = ((long)loopStart * outputSampleRate + sourceSampleRate / 2L) / sourceSampleRate;
            long convertedEnd = ((long)loopEnd * outputSampleRate + sourceSampleRate / 2L) / sourceSampleRate;
            if (convertedStart > int.MaxValue || convertedEnd > int.MaxValue || convertedEnd <= convertedStart)
                return false;

            scaledStart = (int)convertedStart;
            scaledEnd = (int)convertedEnd;
            return true;
        }

        private static string OverrideAtrac9SamplingRate(string parameters, int samplingRate)
        {
            const string samplingRatePattern = @"(?<!\S)-fs\s+\d+";
            string replacement = $"-fs {samplingRate}";
            var regex = new Regex(samplingRatePattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(parameters))
                return regex.Replace(parameters, replacement, 1);

            int inputPlaceholderIndex = parameters.IndexOf("$InFile", StringComparison.Ordinal);
            return inputPlaceholderIndex >= 0
                ? parameters.Insert(inputPlaceholderIndex, replacement + " ")
                : parameters.TrimEnd() + " " + replacement;
        }

        private static bool TryScaleLoopInEncodeParam(
            ref string parameters,
            int sourceSamplingRate,
            int targetSamplingRate,
            string codecName,
            string inputPath)
        {
            if (!TryGetLoopPointSamples(parameters, out int loopStart, out int loopEnd))
                return true;

            if (!TryScaleLoopPoints(loopStart, loopEnd, sourceSamplingRate, targetSamplingRate, out int scaledStart, out int scaledEnd))
                return false;

            var regex = new Regex(@"((?:^|\s)-loop\s+)-?\d+\s+-?\d+", RegexOptions.IgnoreCase);
            parameters = regex.Replace(parameters, $"${{1}}{scaledStart} {scaledEnd}", 1);
            FormMain.DebugInfo($"[Encode] {codecName} loop points scaled. rate={sourceSamplingRate}->{targetSamplingRate}, start={loopStart}->{scaledStart}, end={loopEnd}->{scaledEnd}, input={inputPath}");
            return true;
        }

        private static bool TryPrepareAtracEncodeInput(
            string inputPath,
            string originPath,
            int fileIndex,
            int targetSamplingRate,
            string codecName,
            out string encoderInputPath,
            out int sourceSamplingRate)
        {
            encoderInputPath = inputPath;
            sourceSamplingRate = targetSamplingRate;

            if (!Nus3BankFile.TryReadRiffWaveSampleRate(inputPath, out sourceSamplingRate))
            {
                FormMain.DebugWarn($"[Encode] {codecName} input sample rate could not be read. Encoding the original input. input={inputPath}, targetRate={targetSamplingRate}");
                return true;
            }

            if (sourceSamplingRate == targetSamplingRate)
                return true;

            string resampleKey = $"{Path.GetFileNameWithoutExtension(originPath)}__{codecName.ToLowerInvariant()}_{targetSamplingRate}";
            string resampledPath = Utils.MakeTempUniquePath(
                TempDirectory,
                resampleKey,
                fileIndex,
                ".wav");

            TryDeleteFile(resampledPath);
            try
            {
                string resamplerName = ResampleWaveFileForAtrac(inputPath, resampledPath, targetSamplingRate, codecName);

                if (!Nus3BankFile.TryReadRiffWaveSampleRate(resampledPath, out int actualSamplingRate) ||
                    actualSamplingRate != targetSamplingRate)
                {
                    FormMain.DebugError($"[Encode] {codecName} resampling validation failed. input={inputPath}, output={resampledPath}, expectedRate={targetSamplingRate}, actualRate={actualSamplingRate}");
                    TryDeleteFile(resampledPath);
                    return false;
                }

                encoderInputPath = resampledPath;
                FormMain.DebugInfo($"[Encode] {codecName} input resampled. engine={resamplerName}, input={inputPath}, output={resampledPath}, rate={sourceSamplingRate}->{targetSamplingRate}");
                return true;
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[Encode] {codecName} input resampling failed. input={inputPath}, targetRate={targetSamplingRate}, error={ex}");
                TryDeleteFile(resampledPath);
                return false;
            }
        }

        private static string ResampleWaveFileForAtrac(string inputPath, string outputPath, int targetSamplingRate, string codecName)
        {
            try
            {
                using var reader = new WaveFileReader(inputPath);
                var outputFormat = new WaveFormat(targetSamplingRate, 16, reader.WaveFormat.Channels);
                using var resampler = new MediaFoundationResampler(reader, outputFormat)
                {
                    ResamplerQuality = 60,
                };
                WaveFileWriter.CreateWaveFile(outputPath, resampler);
                return "MediaFoundation(q=60)";
            }
            catch (Exception ex)
            {
                TryDeleteFile(outputPath);
                FormMain.DebugWarn($"[Encode] {codecName} high-quality resampler unavailable; using WDL fallback. input={inputPath}, targetRate={targetSamplingRate}, error={ex.Message}");
            }

            using (var reader = new WaveFileReader(inputPath))
            {
                var resampler = new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(reader.ToSampleProvider(), targetSamplingRate);
                WaveFileWriter.CreateWaveFile16(outputPath, resampler);
            }

            return "WDL(fallback)";
        }

        private static (int Start, int End) GetAtrac9LoopAdjustments(string encodedPath)
        {
            if (!Nus3BankFile.TryReadRiffWaveSampleRate(encodedPath, out int sampleRate))
                return (256, 255);

            return sampleRate switch
            {
                12000 => (64, 63),
                24000 => (128, 127),
                _ => (256, 255),
            };
        }

        private static bool TryGetLoopPointSamples(string encodeParam, out int loopStart, out int loopEnd)
        {
            loopStart = 0;
            loopEnd = 0;

            Match match = Regex.Match(encodeParam ?? string.Empty, @"(?:^|\s)-loop\s+(-?\d+)\s+(-?\d+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return false;

            return int.TryParse(match.Groups[1].Value, out loopStart) &&
                   int.TryParse(match.Groups[2].Value, out loopEnd) &&
                   loopEnd > loopStart;
        }

        /// <summary>
        /// 複数ファイルのATRACエンコード処理
        /// </summary>
        private static bool EncodeMultipleFiles(IProgress<int> p, CancellationToken cToken)
        {
            int fs = 0;
            int mpfloop = 0;

            bool atracEncodeSource = Utils.GetBool("ATRACEncodeSource", false);

            string atrac3Params = Utils.GetString("ATRAC3_Params", string.Empty);
            string atrac9Params = Utils.GetString("ATRAC9_Params", string.Empty);
            List<Nus3BankEncodedStreamSource>? nus3BankStreams = Generic.Nus3BankEncodeOutput ? [] : null;

            var atrac3Console = (Constants.ATRAC3ConsoleType)Utils.GetInt("ATRAC3_Console", (int)Constants.ATRAC3ConsoleType.PSP);
            var atrac9Console = (Constants.ATRAC9ConsoleType)Utils.GetInt("ATRAC9_Console", (int)Constants.ATRAC9ConsoleType.PSV);

            if (Generic.Nus3BankEncodeOutput)
            {
                int selectedSamplingRate = Generic.Nus3BankEncodeSamplingRate;
                if (Generic.ATRACFlag == 0)
                {
                    selectedSamplingRate = selectedSamplingRate is 44100 or 48000
                        ? selectedSamplingRate
                        : 44100;
                    atrac3Console = selectedSamplingRate == 48000
                        ? Constants.ATRAC3ConsoleType.PS3
                        : Constants.ATRAC3ConsoleType.PSP;
                }
                else if (Generic.ATRACFlag == 1)
                {
                    selectedSamplingRate = selectedSamplingRate is 48000 or 24000 or 12000
                        ? selectedSamplingRate
                        : 48000;
                    atrac9Params = OverrideAtrac9SamplingRate(atrac9Params, selectedSamplingRate);
                }

                FormMain.DebugInfo($"[NUS3BANK] Multiple encode format selected. atracFlag={Generic.ATRACFlag}, samplingRate={selectedSamplingRate}, atrac3Console={atrac3Console}, atrac9Console={atrac9Console}");
            }

            string[] fp;

            if (atracEncodeSource && Generic.IsATRAC)
            {
                fp = Generic.pATRACOpenFilePaths;
            }
            else
            {
                fp = Generic.OpenFilePaths;
            }
            FormMain.DebugInfo($"[Encode] Multiple files started. files={fp.Length}, atracFlag={Generic.ATRACFlag}, atrac3Console={atrac3Console}, atrac9Console={atrac9Console}, nus3bankOutput={Generic.Nus3BankEncodeOutput}");

            if (Generic.IsMiniDisc)
                return EncodeMiniDiscMultiple(fp, p, cToken);

            // Walkman metadata is either fixed for all files, inherited without
            // prompting, or confirmed per file.
            bool walkmanUnattended = Utils.GetBool("Walkman_Unattended", false);
            bool useConfiguredWalkmanMetadata = Utils.GetBool("Walkman_FixSongInformation", false);

            if (Generic.ATRACFlag == 2 && useConfiguredWalkmanMetadata)
            {
                foreach (var job in Common.Generic.InputJobs)
                    Common.Utils.ApplyConfiguredWalkmanMeta(job);
            }

            if (Generic.ATRACFlag == 2 && !useConfiguredWalkmanMetadata && !walkmanUnattended)
            {
                for (int i = 0; i < Common.Generic.InputJobs.Count; i++)
                {
                    var job = Generic.InputJobs[i];

                    var dr = (DialogResult)FormProgressInstance.Invoke(new Func<DialogResult>(() =>
                    {
                        using var form = new FormWalkmanInformations(job);
                        FormProgressInstance.Enabled = false;
                        try
                        {
                            return form.ShowDialog(FormProgressInstance);
                        }
                        finally
                        {
                            FormProgressInstance.Enabled = true;
                        }
                    }));

                    // ★OK以外なら即座に全体中止
                    if (dr != DialogResult.OK)
                    {
                        try { Common.Generic.cts?.Cancel(); } catch { }
                        return false;
                    }
                    /*FormProgressInstance.Invoke(new Action(() =>
                    {
                        using var form = new FormWalkmanInformations(job);
                        FormProgressInstance.Enabled = false;
                        if (form.ShowDialog(FormProgressInstance) != DialogResult.OK)
                        {
                            // 途中キャンセルなら中断
                            FormProgressInstance.Enabled = true;
                            return;
                        }
                        FormProgressInstance.Enabled = true;
                    }));*/
                }
            }

            if (Generic.ATRACFlag == 2)
            {
                var jobs = Common.Generic.InputJobs;
                if (jobs == null || jobs.Count == 0)
                {
                    FormMain.DebugWarn("[Encode] Walkman multiple encode skipped: no input jobs.");
                    return false;
                }

                // 念のため：OpenFilePaths と jobs 数がズレていたらここで止める
                if (jobs.Count != Common.Generic.OpenFilePaths.Length)
                {
                    FormMain.DebugError($"InputJobs mismatch. jobs={jobs.Count}, OpenFilePaths={Common.Generic.OpenFilePaths.Length}");
                    return false;
                }

                for (int i = 0; i < jobs.Count; i++)
                {
                    var job = jobs[i];
                    FormMain.DebugInfo($"[Encode] Walkman file started. index={i + 1}/{jobs.Count}, input={job.WorkPath}, origin={job.OriginPath}");

                    // 入力は WorkPath（WAV化済みなら WAV）
                    string inFile = job.WorkPath;

                    // 拡張子は Walkman 用（.oma など）
                    Common.Generic.ATRACExt = Common.Generic.WalkmanMultiConvExt;

                    // 出力 temp は衝突回避で一意名にする（※最終出力時にハッシュを落とす設計のままでOK）
                    string outPath = Common.Utils.MakeTempUniquePath(
                        TempDirectory,
                        job.OriginPath, // 衝突回避のキー
                        i,
                        Common.Generic.ATRACExt
                    );

                    // ★ここが本題：job.Meta から traconv 引数を毎回生成（--FileType/Jacket 含む）
                    string args = Common.Utils.BuildTraConvArgsForJob(job, inFile, outPath);

                    // デバッグ：ジャケットが入っているか確認（任意だが強く推奨）
                    FormMain.DebugInfo("Walkman args[" + i + "]: " + args);

                    // 表示名（ラベル等）はハッシュ無し＝Origin のファイル名を使う
                    string displayName = Path.GetFileName(job.OriginPath);

                    if (!RunAtracToolWithValidation(
                            Generic.Walkman_TraConv,
                            args,
                            inFile,
                            outPath,
                            displayName,
                            p,
                            cToken))
                    {
                        return false;
                    }
                    FormMain.DebugInfo($"[Encode] Walkman file completed. index={i + 1}/{jobs.Count}, output={outPath}");
                }

                FormMain.DebugInfo($"[Encode] Walkman multiple files completed. files={jobs.Count}");
                return true; // ★ここが重要：外側 foreach に入らない
            }

            List<MultipleEncodeItem> encodeItems = BuildMultipleEncodeItems(fp);
            foreach (MultipleEncodeItem encodeItem in encodeItems)
            {
                int sourceIndex = encodeItem.SourceIndex;
                int loopIndex = sourceIndex >= 0 && sourceIndex < Generic.MultipleFilesLoopOKFlags.Length ? sourceIndex : mpfloop;
                bool mloop = loopIndex < Generic.MultipleFilesLoopOKFlags.Length && Generic.MultipleFilesLoopOKFlags[loopIndex];
                string file = encodeItem.InputPath;
                string originPath = encodeItem.OriginPath;
                string streamName = encodeItem.StreamName;
                FormMain.DebugInfo($"[Encode] File started. index={fs + 1}/{encodeItems.Count}, atracFlag={Generic.ATRACFlag}, loop={mloop}, input={file}, origin={originPath}");

                switch (Generic.ATRACFlag)
                {
                    case 0: // ATRAC3
                        {
                            if (!Atrac3Tools.TryGetValue(atrac3Console, out string? atrac3Tool))
                            {
                                FormMain.DebugError($"[Encode] Unsupported ATRAC3 console selected. console={atrac3Console}");
                                return false;
                            }

                            bool isAtrac3Ps3 = atrac3Console == Constants.ATRAC3ConsoleType.PS3;
                            int targetSamplingRate = isAtrac3Ps3 ? 48000 : 44100;
                            int loopStart = loopIndex < Generic.MultipleLoopStarts.Length ? Generic.MultipleLoopStarts[loopIndex] : 0;
                            int loopEnd = loopIndex < Generic.MultipleLoopEnds.Length ? Generic.MultipleLoopEnds[loopIndex] : 0;
                            string encoderInputPath = file;
                            int sourceSamplingRate = targetSamplingRate;

                            if (!TryPrepareAtracEncodeInput(
                                    file,
                                    originPath,
                                    fs,
                                    targetSamplingRate,
                                    "ATRAC3",
                                    out encoderInputPath,
                                    out sourceSamplingRate))
                            {
                                return false;
                            }

                            string? resampledInputPath = string.Equals(encoderInputPath, file, StringComparison.OrdinalIgnoreCase)
                                ? null
                                : encoderInputPath;

                            try
                            {
                                Generic.EncodeParamAT3 = BuildAtracEncodeParam(
                                    atrac3Params,
                                    mloop,
                                    loopStart,
                                    loopEnd,
                                    Generic.lpcreate,
                                    sourceIndex);

                                if (sourceSamplingRate != targetSamplingRate &&
                                    !TryScaleLoopInEncodeParam(ref Generic.EncodeParamAT3, sourceSamplingRate, targetSamplingRate, "ATRAC3", file))
                                {
                                    FormMain.DebugError($"[Encode] ATRAC3 loop scaling failed. rate={sourceSamplingRate}->{targetSamplingRate}, input={file}");
                                    return false;
                                }

                                Generic.ATRACExt = ".at3";
                                string outPath = Utils.MakeTempUniquePath(
                                    TempDirectory,
                                    originPath,
                                    fs,
                                    Generic.ATRACExt);

                                if (!RunAtracToolWithValidation(
                                        atrac3Tool,
                                        Generic.EncodeParamAT3,
                                        encoderInputPath,
                                        outPath,
                                        Path.GetFileName(originPath),
                                        p,
                                        cToken))
                                {
                                    return false;
                                }

                                if (!TryNormalizeAtrac3LoopMetadata(outPath, Generic.EncodeParamAT3, isAtrac3Ps3))
                                    return false;

                                if (!AddMultipleNus3BankStreamIfRequested(nus3BankStreams, outPath, file, originPath, streamName, Generic.EncodeParamAT3, isAtrac9: false, isAtrac3Ps3: isAtrac3Ps3))
                                    return false;
                            }
                            finally
                            {
                                if (resampledInputPath is not null)
                                    TryDeleteFile(resampledInputPath);
                            }
                        }
                        break;
                    case 1: // ATRAC9
                        {
                            if (!Atrac9Tools.TryGetValue(atrac9Console, out string? atrac9Tool))
                            {
                                FormMain.DebugError($"[NUS3BANK] Unsupported ATRAC9 console selected. console={atrac9Console}");
                                return false;
                            }

                            int targetSamplingRate = Generic.Nus3BankEncodeSamplingRate is 48000 or 24000 or 12000
                                ? Generic.Nus3BankEncodeSamplingRate
                                : 48000;
                            int loopStart = loopIndex < Generic.MultipleLoopStarts.Length ? Generic.MultipleLoopStarts[loopIndex] : 0;
                            int loopEnd = loopIndex < Generic.MultipleLoopEnds.Length ? Generic.MultipleLoopEnds[loopIndex] : 0;
                            string encoderInputPath = file;
                            int sourceSamplingRate = targetSamplingRate;

                            if (Generic.Nus3BankEncodeOutput &&
                                targetSamplingRate != 48000 &&
                                !TryPrepareAtracEncodeInput(
                                    file,
                                    originPath,
                                    fs,
                                    targetSamplingRate,
                                    "ATRAC9",
                                    out encoderInputPath,
                                    out sourceSamplingRate))
                            {
                                return false;
                            }

                            string? resampledInputPath = string.Equals(encoderInputPath, file, StringComparison.OrdinalIgnoreCase)
                                ? null
                                : encoderInputPath;

                            try
                            {
                                if (mloop && sourceSamplingRate != targetSamplingRate)
                                {
                                    if (!TryScaleLoopPoints(loopStart, loopEnd, sourceSamplingRate, targetSamplingRate, out loopStart, out loopEnd))
                                    {
                                        FormMain.DebugError($"[NUS3BANK] ATRAC9 loop scaling failed. start={loopStart}, end={loopEnd}, rate={sourceSamplingRate}->{targetSamplingRate}, input={file}");
                                        return false;
                                    }

                                    FormMain.DebugInfo($"[NUS3BANK] ATRAC9 loop points scaled. rate={sourceSamplingRate}->{targetSamplingRate}, start={loopStart}, end={loopEnd}, input={file}");
                                }

                                Generic.EncodeParamAT9 = BuildAtracEncodeParam(
                                    atrac9Params,
                                    mloop,
                                    loopStart,
                                    loopEnd,
                                    Generic.lpcreate,
                                    sourceIndex);

                                if (!mloop &&
                                    sourceSamplingRate != targetSamplingRate &&
                                    !TryScaleLoopInEncodeParam(ref Generic.EncodeParamAT9, sourceSamplingRate, targetSamplingRate, "ATRAC9", file))
                                {
                                    FormMain.DebugError($"[NUS3BANK] ATRAC9 LPC loop scaling failed. rate={sourceSamplingRate}->{targetSamplingRate}, input={file}");
                                    return false;
                                }

                                Generic.ATRACExt = ".at9";
                                string outPath = Utils.MakeTempUniquePath(
                                    TempDirectory,
                                    originPath,
                                    fs,
                                    Generic.ATRACExt);

                                if (!RunAtracToolWithValidation(
                                        atrac9Tool,
                                        Generic.EncodeParamAT9,
                                        encoderInputPath,
                                        outPath,
                                        Path.GetFileName(originPath),
                                        p,
                                        cToken))
                                {
                                    return false;
                                }

                                if (!AddMultipleNus3BankStreamIfRequested(nus3BankStreams, outPath, file, originPath, streamName, Generic.EncodeParamAT9, isAtrac9: true, isAtrac3Ps3: false))
                                    return false;
                            }
                            finally
                            {
                                if (resampledInputPath is not null)
                                    TryDeleteFile(resampledInputPath);
                            }

                            break;
                        }
                    case 2: // Walkman
                        {
                            
                        }
                        break;
                    default:
                        FormMain.DebugWarn($"[Encode] Unsupported ATRAC flag for multiple encode. atracFlag={Generic.ATRACFlag}");
                        return false;
                }
                FormMain.DebugInfo($"[Encode] File completed. index={fs + 1}/{encodeItems.Count}, input={file}");
                fs++;
                mpfloop++;
            }

            if (Generic.Nus3BankEncodeOutput)
            {
                if (nus3BankStreams is null)
                    return false;

                return WriteMultiStreamNus3Bank(nus3BankStreams);
            }

            FormMain.DebugInfo($"[Encode] Multiple files completed. files={encodeItems.Count}");
            return true;
        }

        /// <summary>
        /// ATRAC 関連ツールを起動して終了まで待機する共通ヘルパー。
        /// $InFile / $OutFile プレースホルダ展開と、
        /// at3tool / at9tool / traconv のプレフィックス削除もここで行う。
        /// </summary>
        private static bool RunMiniDiscTool(
            string toolPath,
            string operation,
            string parameterTemplate,
            string inputFile,
            string outputFile,
            IProgress<int> progress,
            CancellationToken cToken)
        {
            if (!File.Exists(toolPath))
            {
                FormMain.DebugError($"[MiniDisc] Required tool not found. operation={operation}, path={toolPath}");
                return false;
            }

            bool isAtracdenc = Path.GetFileName(toolPath)
                .Equals("atracdenc.exe", StringComparison.OrdinalIgnoreCase);
            bool isDecode = Regex.IsMatch(parameterTemplate, @"(?:^|\s)-d(?:\s|$)", RegexOptions.IgnoreCase);
            bool stageAtracdencInput = isAtracdenc && RequiresAtracdencPathStaging(inputFile);
            string toolInputFile = inputFile;
            string toolOutputFile = outputFile;
            if (isAtracdenc)
            {
                Directory.CreateDirectory(TempDirectory);
                if (stageAtracdencInput)
                {
                    string inputExtension = isDecode ? ".aea" : ".wav";
                    toolInputFile = Path.Combine(TempDirectory, $"mdinput_{Guid.NewGuid():N}{inputExtension}");
                    FormMain.DebugInfo($"[MiniDisc] Using ASCII staging input for atracdenc. operation={operation}, source={inputFile}, staged={toolInputFile}");
                }

                string outputExtension = isDecode ? ".wav" : ".aea";
                toolOutputFile = Path.Combine(TempDirectory, $"mdtool_{Guid.NewGuid():N}{outputExtension}");
                FormMain.DebugInfo($"[MiniDisc] Using ASCII staging output for atracdenc. operation={operation}, staged={toolOutputFile}, destination={outputFile}");
            }

            TryDeleteFile(outputFile);
            if (isAtracdenc)
                TryDeleteFile(toolOutputFile);
            if (stageAtracdencInput)
                TryDeleteFile(toolInputFile);

            try
            {
                if (stageAtracdencInput)
                    File.Copy(inputFile, toolInputFile, overwrite: true);

                string arguments = parameterTemplate
                    .Replace("$InFile", "\"" + toolInputFile + "\"")
                    .Replace("$OutFile", "\"" + toolOutputFile + "\"")
                    .Replace("at3tool ", "")
                    .Replace("atracdenc ", "");
                using Process? process = Process.Start(new ProcessStartInfo
                {
                    FileName = toolPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true
                });
                if (process is null || !WaitForProcessExit(process, Path.GetFileName(toolPath), progress, cToken))
                    return false;

                if (!File.Exists(toolOutputFile) || new FileInfo(toolOutputFile).Length == 0)
                {
                    FormMain.DebugError($"[MiniDisc] Tool output validation failed. operation={operation}, input={inputFile}, output={toolOutputFile}");
                    return false;
                }

                if (isAtracdenc)
                {
                    string? destinationDirectory = Path.GetDirectoryName(outputFile);
                    if (!string.IsNullOrWhiteSpace(destinationDirectory))
                        Directory.CreateDirectory(destinationDirectory);

                    File.Move(toolOutputFile, outputFile, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[MiniDisc] Tool execution failed. operation={operation}, error={ex}");
                return false;
            }
            finally
            {
                if (isAtracdenc)
                    TryDeleteFile(toolOutputFile);
                if (stageAtracdencInput)
                    TryDeleteFile(toolInputFile);
            }

            if (!File.Exists(outputFile) || new FileInfo(outputFile).Length == 0)
            {
                FormMain.DebugError($"[MiniDisc] Output validation failed. operation={operation}, input={inputFile}, output={outputFile}");
                TryDeleteFile(outputFile);
                return false;
            }

            return true;
        }

        private static bool RequiresAtracdencPathStaging(string path)
        {
            if (path.Length >= 240)
                return true;

            foreach (char character in path)
            {
                if (character > 0x7F)
                    return true;
            }

            return false;
        }

        private static bool DecodeMiniDiscAtrac1ToWave(
            string inputFile,
            string outputFile,
            IProgress<int> progress,
            CancellationToken cToken)
        {
            if (!RunMiniDiscTool(
                    Generic.ATRAC1tool,
                    "decode SP",
                    "-d -i $InFile -o $OutFile",
                    inputFile,
                    outputFile,
                    progress,
                    cToken))
            {
                return false;
            }

            return TryFinalizeAndValidateMiniDiscWave(inputFile, outputFile);
        }

        private static bool DecodeWalkmanOmaToWave(
            string inputFile,
            string outputFile,
            IProgress<int> progress,
            CancellationToken cToken)
        {
            if (!File.Exists(Generic.Walkman_TraConv))
            {
                FormMain.DebugError($"[Walkman] TraConv was not found. path={Generic.Walkman_TraConv}");
                return false;
            }

            TryDeleteFile(outputFile);
            if (!RunAtracTool(
                    Generic.Walkman_TraConv,
                    Generic.DecodeParamWalkman,
                    inputFile,
                    outputFile,
                    progress,
                    cToken))
            {
                return false;
            }

            try
            {
                using var reader = new WaveFileReader(outputFile);
                if (reader.WaveFormat.Channels <= 0 || reader.WaveFormat.SampleRate <= 0 || reader.Length <= 0)
                    throw new InvalidDataException("Decoded OMA does not contain playable PCM data.");

                FormMain.DebugInfo($"[Walkman] OMA decoded for playback. input={inputFile}, output={outputFile}, channels={reader.WaveFormat.Channels}, sampleRate={reader.WaveFormat.SampleRate}, bytes={reader.Length}");
                return true;
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[Walkman] OMA decoded WAV validation failed. input={inputFile}, output={outputFile}, error={ex}");
                TryDeleteFile(outputFile);
                return false;
            }
        }

        /// <summary>
        /// atracdenc 0.2.3 for Windows leaves the RIFF and data chunk sizes at
        /// zero after ATRAC1 decoding. The PCM payload and fmt chunk are valid,
        /// so repair those two size fields before NAudio opens the preview.
        /// </summary>
        private static bool TryFinalizeAndValidateMiniDiscWave(string inputFile, string outputFile)
        {
            try
            {
                using (var stream = new FileStream(outputFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    if (stream.Length < 44 || stream.Length - 8 > uint.MaxValue)
                        throw new InvalidDataException($"Decoded WAV size is invalid: {stream.Length} bytes.");

                    Span<byte> riffHeader = stackalloc byte[12];
                    stream.ReadExactly(riffHeader);
                    if (!riffHeader[..4].SequenceEqual("RIFF"u8) ||
                        !riffHeader[8..12].SequenceEqual("WAVE"u8))
                    {
                        throw new InvalidDataException("Decoded output does not contain a RIFF/WAVE header.");
                    }

                    bool fmtFound = false;
                    bool dataFound = false;
                    long chunkOffset = 12;
                    Span<byte> chunkHeader = stackalloc byte[8];
                    Span<byte> sizeBuffer = stackalloc byte[sizeof(uint)];
                    while (chunkOffset + 8 <= stream.Length)
                    {
                        stream.Position = chunkOffset;
                        stream.ReadExactly(chunkHeader);
                        uint declaredSize = BinaryPrimitives.ReadUInt32LittleEndian(chunkHeader[4..8]);

                        if (chunkHeader[..4].SequenceEqual("fmt "u8))
                        {
                            if (declaredSize < 16 || chunkOffset + 8 + declaredSize > stream.Length)
                                throw new InvalidDataException("Decoded WAV contains an invalid fmt chunk.");

                            fmtFound = true;
                        }
                        else if (chunkHeader[..4].SequenceEqual("data"u8))
                        {
                            if (!fmtFound)
                                throw new InvalidDataException("Decoded WAV has no fmt chunk before its audio data.");

                            long dataOffset = chunkOffset + 8;
                            long availableDataSize = stream.Length - dataOffset;
                            if (availableDataSize <= 0 || availableDataSize > uint.MaxValue)
                                throw new InvalidDataException($"Decoded WAV data size is invalid: {availableDataSize} bytes.");

                            if (declaredSize == 0)
                            {
                                stream.Position = chunkOffset + 4;
                                BinaryPrimitives.WriteUInt32LittleEndian(sizeBuffer, checked((uint)availableDataSize));
                                stream.Write(sizeBuffer);
                            }
                            else if (dataOffset + declaredSize > stream.Length)
                            {
                                throw new InvalidDataException("Decoded WAV data chunk exceeds the file length.");
                            }

                            dataFound = true;
                            break;
                        }

                        long nextChunkOffset = checked(chunkOffset + 8 + declaredSize + (declaredSize & 1));
                        if (nextChunkOffset <= chunkOffset || nextChunkOffset > stream.Length)
                            throw new InvalidDataException("Decoded WAV contains an invalid chunk layout.");

                        chunkOffset = nextChunkOffset;
                    }

                    if (!fmtFound || !dataFound)
                        throw new InvalidDataException("Decoded WAV is missing a required fmt or data chunk.");

                    stream.Position = 4;
                    BinaryPrimitives.WriteUInt32LittleEndian(sizeBuffer, checked((uint)(stream.Length - 8)));
                    stream.Write(sizeBuffer);
                    stream.Flush(flushToDisk: true);
                }

                using var reader = new WaveFileReader(outputFile);
                if (reader.WaveFormat.Channels <= 0 || reader.WaveFormat.SampleRate <= 0 || reader.Length <= 0)
                    throw new InvalidDataException("Decoded WAV has no playable PCM data.");

                FormMain.DebugInfo($"[MiniDisc] ATRAC1 decoded WAV finalized. input={inputFile}, output={outputFile}, channels={reader.WaveFormat.Channels}, sampleRate={reader.WaveFormat.SampleRate}, bytes={reader.Length}");
                return true;
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[MiniDisc] ATRAC1 decoded WAV validation failed. input={inputFile}, output={outputFile}, error={ex}");
                TryDeleteFile(outputFile);
                return false;
            }
        }

        private static bool RunAtracTool(
            string toolPath,
            string parameterTemplate,
            string inputFile,
            string outputFile,
            IProgress<int> progress,
            CancellationToken cToken,
            Func<int>? progressValueProvider = null,
            bool forceLoopExpandedDecodeNormalization = false)
        {
            bool normalizeLoopExpandedDecode =
                forceLoopExpandedDecodeNormalization ||
                IsAtracDecodeCommand(parameterTemplate, inputFile, outputFile);
            var args = parameterTemplate
                .Replace("$InFile", "\"" + inputFile + "\"")
                .Replace("$OutFile", "\"" + outputFile + "\"")
                .Replace("at3tool ", "")
                .Replace("at9tool ", "")
                .Replace("traconv ", "")
                .Replace("atracdenc ", "");
            if (normalizeLoopExpandedDecode)
                args = UseSingleLoopRepeatForDecode(args);

            var pi = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            FormMain.DebugInfo($"[ExternalTool] Start. tool={Path.GetFileName(toolPath)}, input={inputFile}, output={outputFile}, args={args}");
            Process? ps;
            try
            {
                ps = Process.Start(pi);
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[ExternalTool] Start failed. tool={toolPath}, error={ex}");
                throw;
            }

            if (ps is null)
            {
                FormMain.DebugError($"[ExternalTool] Start returned null. tool={toolPath}");
                return false;
            }

            // デバッグ用ログはこれまで通り Generic.Log に保持
            Generic.Log = ps.StandardOutput;

            if (!WaitForProcessExit(ps, Path.GetFileName(toolPath), progress, cToken, progressValueProvider))
                return false;

            if (normalizeLoopExpandedDecode &&
                !TryTrimLoopExpandedAtracDecode(
                    inputFile,
                    outputFile,
                    allowLossyNestedRepeatChain:
                        forceLoopExpandedDecodeNormalization ||
                        Path.GetExtension(inputFile).Equals(".at9", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return true;
        }

        private static string UseSingleLoopRepeatForDecode(string arguments)
        {
            const string repeatPattern = @"(?<!\S)-repeat\s+\d+";
            if (Regex.IsMatch(arguments, repeatPattern, RegexOptions.IgnoreCase))
                return Regex.Replace(arguments, repeatPattern, "-repeat 1", RegexOptions.IgnoreCase);

            var decodeOption = new Regex(@"(?<!\S)-d(?!\S)", RegexOptions.IgnoreCase);
            return decodeOption.Replace(arguments, "$0 -repeat 1", 1);
        }

        private static bool IsAtracDecodeCommand(string parameterTemplate, string inputFile, string outputFile)
        {
            string inputExtension = Path.GetExtension(inputFile);
            return (inputExtension.Equals(".at3", StringComparison.OrdinalIgnoreCase) ||
                    inputExtension.Equals(".at9", StringComparison.OrdinalIgnoreCase)) &&
                Path.GetExtension(outputFile).Equals(".wav", StringComparison.OrdinalIgnoreCase) &&
                Regex.IsMatch(parameterTemplate, @"(?:^|\s)-d(?:\s|$)", RegexOptions.IgnoreCase);
        }

        private static bool TryTrimLoopExpandedAtracDecode(
            string encodedPath,
            string decodedWavePath,
            bool allowLossyNestedRepeatChain = false)
        {
            if (!Nus3BankFile.TryReadRiffLoopChunk(encodedPath, out uint rawLoopStart, out uint rawLoopEnd) ||
                !Nus3BankFile.TryReadRiffFactSampleCount(encodedPath, out uint expectedSampleFrames))
            {
                return true;
            }

            if (!TryGetDecodedLoopRange(encodedPath, rawLoopStart, rawLoopEnd, out long loopStart, out long loopEnd))
                return true;

            string tempPath = decodedWavePath + ".looptrim-" + Guid.NewGuid().ToString("N") + ".wav";
            try
            {
                long actualSampleFrames;
                using (var reader = new WaveFileReader(decodedWavePath))
                {
                    actualSampleFrames = reader.SampleCount;
                }

                long loopFrames = loopEnd - loopStart;
                if (loopFrames <= 0)
                    return true;

                if (actualSampleFrames > expectedSampleFrames)
                {
                    long expandedFrames = actualSampleFrames - expectedSampleFrames;
                    long removeStart = expandedFrames == loopFrames && loopEnd + expandedFrames <= actualSampleFrames
                        ? loopEnd
                        : expectedSampleFrames;

                    RewriteWaveWithoutFrameRange(decodedWavePath, tempPath, removeStart, expandedFrames);
                    File.Move(tempPath, decodedWavePath, overwrite: true);
                    actualSampleFrames = expectedSampleFrames;
                    FormMain.DebugInfo($"[Decode] Removed decoder loop repeat. input={encodedPath}, output={decodedWavePath}, remove={removeStart}+{expandedFrames}, samples={actualSampleFrames + expandedFrames}->{actualSampleFrames}");
                }

                bool removeNestedRepeatChain =
                    allowLossyNestedRepeatChain &&
                    Path.GetExtension(encodedPath).Equals(".at9", StringComparison.OrdinalIgnoreCase);
                double minimumCorrelation = removeNestedRepeatChain ? 0.90 : 0.98;
                int maximumRepeatRemovals = removeNestedRepeatChain ? 16 : 1;
                int removedRepeatCount = 0;

                while (removedRepeatCount < maximumRepeatRemovals &&
                       actualSampleFrames >= loopEnd + loopFrames &&
                       actualSampleFrames - loopFrames >= loopEnd)
                {
                    double correlation = MeasureWaveRangeCorrelation(decodedWavePath, loopStart, loopEnd, loopEnd);
                    FormMain.DebugInfo($"[Decode] Nested loop check. input={encodedPath}, loop={loopStart}-{loopEnd}, samples={actualSampleFrames}, correlation={correlation:F6}, threshold={minimumCorrelation:F2}");
                    if (correlation < minimumCorrelation)
                        break;

                    RewriteWaveWithoutFrameRange(decodedWavePath, tempPath, loopEnd, loopFrames);
                    File.Move(tempPath, decodedWavePath, overwrite: true);
                    long normalizedSampleFrames = actualSampleFrames - loopFrames;
                    using var nestedValidationReader = new WaveFileReader(decodedWavePath);
                    if (nestedValidationReader.SampleCount != normalizedSampleFrames)
                        throw new InvalidDataException($"Normalized WAV has {nestedValidationReader.SampleCount} samples; expected {normalizedSampleFrames}.");

                    removedRepeatCount++;
                    FormMain.DebugInfo($"[Decode] Removed nested loop-expanded PCM range. input={encodedPath}, output={decodedWavePath}, remove={loopEnd}+{loopFrames}, samples={actualSampleFrames}->{normalizedSampleFrames}, correlation={correlation:F6}, repeat={removedRepeatCount}");
                    actualSampleFrames = normalizedSampleFrames;
                }

                using var validationReader = new WaveFileReader(decodedWavePath);
                if (validationReader.SampleCount != actualSampleFrames)
                    throw new InvalidDataException($"Normalized WAV has {validationReader.SampleCount} samples; expected {actualSampleFrames}.");

                return true;
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[Decode] Failed to normalize loop-expanded PCM. input={encodedPath}, output={decodedWavePath}, error={ex}");
                return false;
            }
            finally
            {
                TryDeleteFile(tempPath);
            }
        }

        private static bool TryGetDecodedLoopRange(
            string encodedPath,
            uint rawLoopStart,
            uint rawLoopEnd,
            out long loopStart,
            out long loopEnd)
        {
            loopStart = 0;
            loopEnd = 0;
            string extension = Path.GetExtension(encodedPath);
            (int startAdjustment, int endAdjustment) = extension.Equals(".at9", StringComparison.OrdinalIgnoreCase)
                ? GetAtrac9LoopAdjustments(encodedPath)
                : Nus3BankFile.TryReadRiffWaveSampleRate(encodedPath, out int sampleRate) && sampleRate == 48000
                    ? (3271, 3270)
                    : (2459, 2458);

            loopStart = (long)rawLoopStart - startAdjustment;
            loopEnd = (long)rawLoopEnd - endAdjustment;
            return loopStart >= 0 && loopEnd > loopStart;
        }

        private static void RewriteWaveWithoutFrameRange(
            string sourcePath,
            string tempPath,
            long removeStartFrame,
            long removeFrameCount)
        {
            TryDeleteFile(tempPath);
            using var reader = new WaveFileReader(sourcePath);
            long removeEndFrame = checked(removeStartFrame + removeFrameCount);
            if (removeStartFrame < 0 || removeFrameCount <= 0 || removeEndFrame > reader.SampleCount)
                throw new InvalidDataException($"Invalid WAV removal range: {removeStartFrame}+{removeFrameCount}/{reader.SampleCount}.");

            int blockAlign = reader.BlockAlign;
            int bufferSize = 81920 - (81920 % blockAlign);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                using var writer = new WaveFileWriter(tempPath, reader.WaveFormat);
                CopyWaveBytes(reader, writer, checked(removeStartFrame * blockAlign), buffer, bufferSize);
                reader.Position = checked(removeEndFrame * blockAlign);
                CopyWaveBytes(reader, writer, checked((reader.SampleCount - removeEndFrame) * blockAlign), buffer, bufferSize);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static void CopyWaveBytes(
            WaveFileReader reader,
            WaveFileWriter writer,
            long byteCount,
            byte[] buffer,
            int bufferSize)
        {
            long remaining = byteCount;
            while (remaining > 0)
            {
                int requested = (int)Math.Min(bufferSize, remaining);
                int read = reader.Read(buffer, 0, requested);
                if (read <= 0)
                    throw new EndOfStreamException("Decoded WAV ended while removing a loop-expanded range.");

                writer.Write(buffer, 0, read);
                remaining -= read;
            }
        }

        private static double MeasureWaveRangeCorrelation(
            string wavePath,
            long sourceStartFrame,
            long sourceEndFrame,
            long comparisonStartFrame)
        {
            using var reader = new WaveFileReader(wavePath);
            if (reader.WaveFormat.BitsPerSample != 16)
                return 0;

            long sourceFrames = sourceEndFrame - sourceStartFrame;
            if (sourceFrames <= 0 || comparisonStartFrame < 0 || comparisonStartFrame + sourceFrames > reader.SampleCount)
                return 0;

            int windowFrames = (int)Math.Min(8192, Math.Max(1024, sourceFrames / 16));
            int windowBytes = checked(windowFrames * reader.BlockAlign);
            byte[] sourceBuffer = ArrayPool<byte>.Shared.Rent(windowBytes);
            byte[] comparisonBuffer = ArrayPool<byte>.Shared.Rent(windowBytes);
            double correlationSum = 0;
            int comparedWindows = 0;
            try
            {
                foreach (double fraction in new[] { 0.15, 0.5, 0.85 })
                {
                    long offset = Math.Min(sourceFrames - windowFrames, Math.Max(0, (long)(sourceFrames * fraction)));
                    int sourceBytes = ReadWaveFrames(reader, sourceStartFrame + offset, windowFrames, sourceBuffer);
                    int comparisonBytes = ReadWaveFrames(reader, comparisonStartFrame + offset, windowFrames, comparisonBuffer);
                    if (sourceBytes != comparisonBytes || sourceBytes < sizeof(short))
                        continue;

                    double dot = 0;
                    double sourceEnergy = 0;
                    double comparisonEnergy = 0;
                    for (int i = 0; i + 1 < sourceBytes; i += sizeof(short))
                    {
                        short first = BinaryPrimitives.ReadInt16LittleEndian(sourceBuffer.AsSpan(i, sizeof(short)));
                        short second = BinaryPrimitives.ReadInt16LittleEndian(comparisonBuffer.AsSpan(i, sizeof(short)));
                        dot += (double)first * second;
                        sourceEnergy += (double)first * first;
                        comparisonEnergy += (double)second * second;
                    }

                    if (sourceEnergy > 0 && comparisonEnergy > 0)
                    {
                        correlationSum += dot / Math.Sqrt(sourceEnergy * comparisonEnergy);
                        comparedWindows++;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sourceBuffer);
                ArrayPool<byte>.Shared.Return(comparisonBuffer);
            }

            return comparedWindows == 0 ? 0 : correlationSum / comparedWindows;
        }

        private static int ReadWaveFrames(
            WaveFileReader reader,
            long startFrame,
            int frameCount,
            byte[] buffer)
        {
            int byteCount = checked(frameCount * reader.BlockAlign);
            if (buffer.Length < byteCount)
                throw new ArgumentException("The WAV read buffer is too small.", nameof(buffer));

            reader.Position = checked(startFrame * reader.BlockAlign);
            int offset = 0;
            while (offset < byteCount)
            {
                int read = reader.Read(buffer, offset, byteCount - offset);
                if (read <= 0)
                    break;
                offset += read;
            }

            return offset;
        }

        /// <summary>
        /// ATRAC 関連ツールを起動して終了まで待機し、
        /// 出力ファイルが 0 バイトの場合はログを表示する共通ヘルパー。
        /// （複数ファイルエンコード用）
        /// </summary>
        private static bool RunAtracToolWithValidation(
            string toolPath,
            string parameterTemplate,
            string inputFile,
            string outputFile,
            string originalFileName,
            IProgress<int> progress,
            CancellationToken cToken)
        {
            // まず通常の実行（RunAtracTool）を使う
            bool ok = RunAtracTool(
                toolPath,
                parameterTemplate,
                inputFile,
                outputFile,
                progress,
                cToken);

            if (!ok)
            {
                // キャンセルなど
                FormMain.DebugWarn($"[ExternalTool] Validation skipped because process failed or was cancelled. tool={Path.GetFileName(toolPath)}, output={outputFile}");
                return false;
            }

            try
            {
                var fi = new FileInfo(outputFile);
                if (fi.Exists && fi.Length == 0 && Generic.Log != null)
                {
                    FormMain.DebugError($"[ExternalTool] Output file is empty. output={outputFile}");
                    string text = "'" + originalFileName + "' " + Utils.LogSplit(Generic.Log);
                    MessageBox.Show(
                        text,
                        Localization.MSGBoxErrorCaption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // ファイルアクセスで何か起きても、ここでは無視（変換自体は完了している）
                FormMain.DebugWarn($"[ExternalTool] Output validation failed. output={outputFile}, error={ex.Message}");
            }

            return true;
        }

        /// <summary>
        /// ATRAC エンコード用のパラメータを構築する共通メソッド。
        /// ループポイント設定／LPC 画面／単体・複数 すべて対応。
        /// </summary>
        private static string BuildAtracEncodeParam(
            string baseParams,
            bool useLoopPoint,
            int loopStart,
            int loopEnd,
            bool useLpcEditor,
            int fileIndex)
        {
            string param = baseParams;

            if (useLoopPoint)
            {
                // ループポイント追加
                param += $" -loop {loopStart} {loopEnd}";
            }
            else
            {
                // LPC 画面を使う？
                if (useLpcEditor)
                {
                    Generic.lpcreatev2 = true;
                    Generic.files = fileIndex;

                    using FormLPC form = new(true);
                    FormProgressInstance.Invoke(new Action(() => FormProgressInstance.Enabled = false));
                    form.ShowDialog();
                    FormProgressInstance.Invoke(new Action(() => FormProgressInstance.Enabled = true));

                    param += Generic.LPCSuffix;
                }
            }

            return param;
        }

        /// <summary>
        /// 外部プロセスが終了するまで監視し、
        /// 進捗（_tempフォルダのファイル数）を更新し、
        /// キャンセルが来たらプロセスを安全に殺して false を返す。
        /// </summary>
        private static bool WaitForProcessExit(Process ps, string processLabel, IProgress<int> progress, CancellationToken cToken, Func<int>? progressValueProvider = null)
        {
            string tempPath = TempDirectory;
            string processInfo = BuildProcessInfo(ps, processLabel);
            Stopwatch progressTimer = Stopwatch.StartNew();

            try
            {
                while (!ps.HasExited)
                {
                    if (cToken.IsCancellationRequested)
                    {
                        FormMain.DebugWarn($"[ExternalTool] Cancellation requested. process={processInfo}");
                        KillProcessTreeAndWait(ps, processInfo);
                        return false;
                    }

                    // 疑似進捗
                    if (progressTimer.ElapsedMilliseconds >= ProgressDirectoryPollIntervalMs && Directory.Exists(tempPath))
                    {
                        int progressValue = progressValueProvider?.Invoke() ?? CountTopDirectoryFiles(tempPath);
                        progress.Report(progressValue);
                        progressTimer.Restart();
                    }

                    Thread.Sleep(50); // CPU に優しい
                }

                try { ps.WaitForExit(); } catch { }
                try
                {
                    int exitCode = ps.ExitCode;
                    if (exitCode == 0)
                    {
                        FormMain.DebugInfo($"[ExternalTool] Completed. process={processInfo}, exitCode={exitCode}");
                    }
                    else
                    {
                        FormMain.DebugWarn($"[ExternalTool] Completed with non-zero exit code. process={processInfo}, exitCode={exitCode}");
                    }
                }
                catch (Exception ex)
                {
                    FormMain.DebugWarn($"[ExternalTool] Exit code unavailable. process={processInfo}, error={ex.Message}");
                }
                return true;
            }
            finally
            {
                try { ps.Close(); } catch { }
            }
        }

        private static string BuildProcessInfo(Process ps, string fallbackName)
        {
            string name = string.IsNullOrWhiteSpace(fallbackName) ? "process" : fallbackName;
            try
            {
                if (!string.IsNullOrWhiteSpace(ps.ProcessName))
                    name = ps.ProcessName;
            }
            catch
            {
            }

            try
            {
                return $"{name}, id={ps.Id}";
            }
            catch
            {
                return name;
            }
        }

        private static void ReportTempFileCount(IProgress<int> progress, string tempDir)
        {
            if (!Directory.Exists(tempDir))
                return;

            progress.Report(CountTopDirectoryFiles(tempDir));
        }

        private static int CountTopDirectoryFiles(string path)
        {
            try
            {
                return Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly).Count();
            }
            catch
            {
                return 0;
            }
        }

        private static void KillProcessTreeAndWait(Process ps, string processInfo)
        {
            try
            {
                if (!ps.HasExited)
                {
                    FormMain.DebugWarn($"[ExternalTool] Killing process tree. process={processInfo}");
                    ps.Kill(entireProcessTree: true);
                }
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[ExternalTool] Failed to kill process tree. error={ex.Message}");
            }

            try
            {
                ps.WaitForExit(5000);
            }
            catch (Exception ex)
            {
                FormMain.DebugWarn($"[ExternalTool] Wait after kill failed. error={ex.Message}");
            }
        }

        private static bool WaitForConversion(Task conversionTask, IProgress<int> progress, CancellationToken cancellationToken)
        {
            string tempDir = TempDirectory;
            bool cancelled = false;
            Stopwatch progressTimer = Stopwatch.StartNew();

            while (!conversionTask.IsCompleted)
            {
                // キャンセルは「フラグとして覚えておくだけ」
                if (cancellationToken.IsCancellationRequested)
                {
                    if (!cancelled)
                    {
                        FormMain.DebugWarn("[MediaToolkit] Cancellation requested.");
                    }
                    cancelled = true;
                }

                // 疑似進捗更新
                if (progressTimer.ElapsedMilliseconds >= ProgressDirectoryPollIntervalMs && Directory.Exists(tempDir))
                {
                    int files = CountTopDirectoryFiles(tempDir);
                    progress.Report(files);
                    progressTimer.Restart();
                }

                // CPU を休ませる
                Thread.Sleep(100);
            }

            // ここに来た時点で変換タスクは完了済み（成功 or 例外）
            try
            {
                // 例外があればここで投げられる
                conversionTask.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // MediaToolkit 内部での例外などは上位にそのまま投げる
                FormMain.DebugError($"[MediaToolkit] Conversion failed. error={ex.GetBaseException()}");
                throw;
            }

            // キャンセルフラグが立っていたら false（上位で「中止」として扱う）
            if (cancelled)
            {
                FormMain.DebugWarn("[MediaToolkit] Conversion completed after cancellation request.");
            }
            return !cancelled;
        }

        private static bool AudioConverter_ATW_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            //int length = Generic.OpenFilePaths.Length;
            int length = (Generic.InputJobs != null && Generic.InputJobs.Count > 0) ? Generic.InputJobs.Count : Generic.OpenFilePaths.Length;
            string ffpath = Path.Combine(Directory.GetCurrentDirectory(), "res", "ffmpeg.exe");

            // WTAType から使うサンプルレートを決定
            AudioSampleRate sampleRate = GetSampleRateFromWtaType(Generic.WTAmethod);
            FormMain.DebugInfo($"[ATW] Dispatch. files={length}, sampleRate={sampleRate}, ffmpeg={ffpath}");

            // 共通処理に委譲
            return ConvertAudioToWave(p, cToken, ffpath, sampleRate, length);
        }

        private static AudioSampleRate GetSampleRateFromWtaType(Constants.WTAType method)
        {
            return method switch
            {
                Constants.WTAType.Hz44100 => AudioSampleRate.Hz44100,
                Constants.WTAType.Hz48000 => AudioSampleRate.Hz48000,
                Constants.WTAType.Hz8000 => (AudioSampleRate)8000,
                Constants.WTAType.Hz12000 => (AudioSampleRate)12000,
                Constants.WTAType.Hz16000 => (AudioSampleRate)16000,
                Constants.WTAType.Hz24000 => (AudioSampleRate)24000,
                Constants.WTAType.Hz32000 => (AudioSampleRate)32000,
                _ => AudioSampleRate.Hz44100, // フォールバック
            };
        }

        private static bool ConvertAudioToWave(
    IProgress<int> p,
    CancellationToken cToken,
    string ffmpegPath,
    AudioSampleRate sampleRate,
    int length)
        {
            string tempDir = TempDirectory;
            FormMain.DebugInfo($"[ATW] ConvertAudioToWave started. files={length}, sampleRate={sampleRate}");

            // 進捗初期値
            ReportTempFileCount(p, tempDir);

            // ★ InputJobs が未構築なら、OpenFilePaths/OriginOpenFilePaths から最低限構築しておく（保険）
            if (Generic.InputJobs == null || Generic.InputJobs.Count == 0)
            {
                FormMain.DebugWarn("[ATW] InputJobs missing. Rebuilding from OpenFilePaths.");
                // ここはあなたの Common.Generic.BuildInputJobsFromPaths(...) 相当を呼ぶ
                // 例: Common.Generic.BuildInputJobsFromPaths(Generic.OpenFilePaths, Generic.OriginOpenFilePaths);
                Generic.BuildInputJobsFromPaths(Generic.OpenFilePaths, Generic.OriginOpenFilePaths);
            }

            if (Generic.InputJobs == null || Generic.InputJobs.Count == 0)
            {
                FormMain.DebugError("[ATW] ConvertAudioToWave failed: no input jobs.");
                return false;
            }

            // ★ Engine はループ外で 1 回だけ生成（毎回 new Engine すると重い）
            using var engine = new Engine(ffmpegPath);

            if (length == 1)
            {
                // 入力は WorkPath
                string inPath = Generic.InputJobs[0].WorkPath;

                // 出力名は現行踏襲（SavePath の名前を使う）
                FileInfo fiOutName = new(Generic.SavePath);
                string outPath = Path.Combine(tempDir, fiOutName.Name);
                //string outPath = BuildUniqueWavPath(tempDir, inPath);

                var source = new MediaFile { Filename = inPath };
                var output = new MediaFile { Filename = outPath };

                var co = new ConversionOptions { AudioSampleRate = sampleRate };

                FormMain.DebugInfo($"[ATW] File started. index=1/1, input={inPath}, output={outPath}");
                var task = MTK_ConvertAsync(engine, source, output, co);
                if (!WaitForConversion(task, p, cToken)) return false;

                // ★ 成功したら WorkPath を wav に更新
                Generic.InputJobs[0].WorkPath = outPath;
                FormMain.DebugInfo($"[ATW] File completed. index=1/1, output={outPath}");
                return true;
            }
            else
            {
                for (int i = 0; i < Generic.InputJobs.Count; i++)
                {
                    string inPath = Generic.InputJobs[i].WorkPath;
                    FileInfo fi = new(inPath);

                    /*string outPath = Path.Combine(
                        tempDir,
                        fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav"
                    );*/
                    string outPath = BuildUniqueWavPath(tempDir, inPath);

                    var source = new MediaFile { Filename = inPath };
                    var output = new MediaFile { Filename = outPath };
                    var co = new ConversionOptions { AudioSampleRate = sampleRate };

                    FormMain.DebugInfo($"[ATW] File started. index={i + 1}/{Generic.InputJobs.Count}, input={inPath}, output={outPath}");
                    var task = MTK_ConvertAsync(engine, source, output, co);
                    if (!WaitForConversion(task, p, cToken)) return false;

                    // ★ 各ジョブの WorkPath を更新
                    Generic.InputJobs[i].WorkPath = outPath;
                    FormMain.DebugInfo($"[ATW] File completed. index={i + 1}/{Generic.InputJobs.Count}, output={outPath}");
                }
                FormMain.DebugInfo($"[ATW] ConvertAudioToWave completed. files={Generic.InputJobs.Count}");
                return true;
            }
        }

        private static bool RunMediaToolkitDecodeToWav(
    Engine engine,
    string inFile,
    string outWav,
    IProgress<int> p,
    CancellationToken cToken)
        {
            // outWav は必ず .wav に寄せる
            if (!string.Equals(Path.GetExtension(outWav), ".wav", StringComparison.OrdinalIgnoreCase))
                outWav = Path.ChangeExtension(outWav, ".wav");

            // 出力先ディレクトリ
            Directory.CreateDirectory(Path.GetDirectoryName(outWav)!);
            FormMain.DebugInfo($"[MediaToolkit] Decode to WAV started. input={inFile}, output={outWav}");

            var source = new MediaFile { Filename = inFile };
            var output = new MediaFile { Filename = outWav };

            // デコードなので基本は co なしでも良いですが、安定のため明示しておきます
            // （不要なら消してもOK）
            var co = new ConversionOptions
            {
                // サンプルレートを固定したいなら指定（好み）
                // AudioSampleRate = AudioSampleRate.Hz44100,
            };

            var task = MTK_ConvertAsync(engine, source, output, co);

            // 既存の疑似進捗＋キャンセル判定を流用
            // WaitForConversion は「キャンセル要求が来たら false」を返す設計
            bool result = WaitForConversion(task, p, cToken);
            FormMain.DebugInfo($"[MediaToolkit] Decode to WAV completed. result={result}, output={outWav}");
            return result;
        }

        private static string BuildUniqueWavPath(string tempDir, string inPath)
        {
            // 例: song_8A1F2C3D_atw.wav のようにする（suffix は既存踏襲）
            string baseName = Path.GetFileNameWithoutExtension(inPath);

            // フルパスでハッシュ化（同名でもディレクトリが違えば別になる）
            byte[] bytes = SHA1.HashData(Encoding.UTF8.GetBytes(inPath));
            string h = Convert.ToHexString(bytes).Substring(0, 8); // 8桁で十分

            return Path.Combine(tempDir, $"{baseName}_{h}{Utils.ATWSuffix()}.wav");
        }

        /*private static bool ConvertAudioToWave(
    IProgress<int> p,
    CancellationToken cToken,
    string ffmpegPath,
    AudioSampleRate sampleRate,
    int length)
        {
            string tempDir = TempDirectory;

            // 最初の進捗通知（元のコードの p.Report(...) 相当）
            if (Directory.Exists(tempDir))
            {
                p.Report(Directory.GetFiles(tempDir, "*").Length);
            }

            if (length == 1)
            {
                FileInfo fi = new(Generic.SavePath);

                var source = new MediaFile { Filename = Generic.OpenFilePaths[0] };
                var output = new MediaFile
                {
                    Filename = Path.Combine(tempDir, fi.Name)
                };

                var co = new ConversionOptions
                {
                    AudioSampleRate = sampleRate,
                };

                using var engine = new Engine(ffmpegPath);
                var task = MTK_ConvertAsync(engine, source, output, co);

                return WaitForConversion(task, p, cToken);
            }
            else
            {
                foreach (var file in Generic.OpenFilePaths)
                {
                    FileInfo fi = new(file);

                    var source = new MediaFile { Filename = file };
                    var output = new MediaFile
                    {
                        Filename = Path.Combine(
                            tempDir,
                            fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav")
                    };

                    var co = new ConversionOptions
                    {
                        AudioSampleRate = sampleRate,
                    };

                    using var engine = new Engine(ffmpegPath);
                    var task = MTK_ConvertAsync(engine, source, output, co);

                    if (!WaitForConversion(task, p, cToken))
                    {
                        return false;
                    }
                }

                return true;
            }
        }*/

        private static bool AudioConverter_WTA_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            int length = Generic.OpenFilePaths.Length;
            string ffpath = Path.Combine(Directory.GetCurrentDirectory(), "res", "ffmpeg.exe");
            FormMain.DebugInfo($"[WTA] Dispatch. files={length}, format={Generic.WTAFmt}, ffmpeg={ffpath}");

            return ConvertWaveToAudio(p, cToken, ffpath, length);
        }

        /// <summary>
        /// Wave → Audio（WTA）の変換処理を共通化したメソッド。
        /// 単一／複数ファイルを処理し、_temp 配下に出力します。
        /// </summary>
        private static bool ConvertWaveToAudio(
            IProgress<int> p,
            CancellationToken cToken,
            string ffmpegPath,
            int length)
        {
            string tempDir = TempDirectory;
            FormMain.DebugInfo($"[WTA] ConvertWaveToAudio started. files={length}, format={Generic.WTAFmt}");

            // 最初の進捗通知
            ReportTempFileCount(p, tempDir);

            if (length == 1)
            {
                FileInfo fi = new(Generic.SavePath);

                var source = new MediaFile { Filename = Generic.OpenFilePaths[0] };
                var output = new MediaFile
                {
                    // もともとの実装と同じ: _temp\SavePathのファイル名
                    Filename = Path.Combine(tempDir, fi.Name)
                };

                var co = new ConversionOptions
                {
                    AudioSampleRate = AudioSampleRate.Hz44100,
                };

                using var engine = new Engine(ffmpegPath);
                FormMain.DebugInfo($"[WTA] File started. index=1/1, input={source.Filename}, output={output.Filename}");
                var task = MTK_ConvertAsync(engine, source, output, co);

                bool result = WaitForConversion(task, p, cToken);
                FormMain.DebugInfo($"[WTA] File completed. index=1/1, result={result}, output={output.Filename}");
                return result;
            }
            else
            {
                for (int i = 0; i < Generic.OpenFilePaths.Length; i++)
                {
                    var file = Generic.OpenFilePaths[i];
                    FileInfo fi = new(file);

                    var source = new MediaFile { Filename = file };
                    var output = new MediaFile
                    {
                        // 元の: _temp\<元のファイル名> + Generic.WTAFmt
                        Filename = Path.Combine(
                            tempDir,
                            fi.Name.Replace(fi.Extension, "") + Generic.WTAFmt)
                    };

                    var co = new ConversionOptions
                    {
                        AudioSampleRate = AudioSampleRate.Hz44100,
                    };

                    using var engine = new Engine(ffmpegPath);
                    FormMain.DebugInfo($"[WTA] File started. index={i + 1}/{Generic.OpenFilePaths.Length}, input={source.Filename}, output={output.Filename}");
                    var task = MTK_ConvertAsync(engine, source, output, co);

                    if (!WaitForConversion(task, p, cToken))
                    {
                        return false;
                    }
                    FormMain.DebugInfo($"[WTA] File completed. index={i + 1}/{Generic.OpenFilePaths.Length}, output={output.Filename}");
                }

                FormMain.DebugInfo($"[WTA] ConvertWaveToAudio completed. files={Generic.OpenFilePaths.Length}");
                return true;
            }
        }

        private bool Download_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            // タイトルを「ダウンロード中」に
            Invoke(new Action(() => Text = Localization.FormDownloadingCaption));

            try
            {
                // ダウンロードURLを決定
                Uri uri;
                string targetPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    @"res",
                    "atractool-rel.zip");

                if (Generic.ApplicationPortable)
                {
                    uri = new Uri(
                        "https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases/download/v"
                        + Generic.GitHubLatestVersion
                        + "/atractool-rel-portable.zip");
                }
                else
                {
                    uri = new Uri(
                        "https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases/download/v"
                        + Generic.GitHubLatestVersion
                        + "/atractool-rel-release.zip");
                }

                FormMain.DebugInfo($"[Update] Download started. portable={Generic.ApplicationPortable}, version={Generic.GitHubLatestVersion}, uri={uri}, target={targetPath}");
                // 実際のダウンロード（同期メソッド内なので GetAwaiter().GetResult()）
                DownloadFileWithProgressAsync(uri, targetPath, p, cToken)
                    .GetAwaiter()
                    .GetResult();

                if (cToken.IsCancellationRequested)
                {
                    FormMain.DebugWarn("[Update] Download cancelled after transfer.");
                    return false;
                }

                // ダウンロード完了後は「処理中」表示に戻す
                Invoke(new Action(() => Text = Localization.ProcessingCaption));
                Invoke(new Action(() => label_Status.Text = Localization.ProcessingCaption));
                Invoke(new Action(() => button_Abort.Enabled = false));

                FormMain.DebugInfo($"[Update] Download completed. target={targetPath}");
                return true;
            }
            catch (OperationCanceledException)
            {
                FormMain.DebugWarn("[Update] Download cancelled.");
                // キャンセルされた場合は false を返して中止扱い
                return false;
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[Update] Download failed. error={ex}");
                // 通信エラーなど
                Invoke(new Action(() =>
                {
                    MessageBox.Show(
                        this,
                        ex.Message,
                        Localization.MSGBoxErrorCaption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }));
                return false;
            }
        }

        private static async Task DownloadFileWithProgressAsync(
    Uri uri,
    string destinationPath,
    IProgress<int> progress,
    CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            FormMain.DebugInfo($"[Download] HTTP request started. uri={uri}");

            using var response = await httpClient.GetAsync(
                uri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            FormMain.DebugInfo($"[Download] HTTP response received. status={(int)response.StatusCode}, totalBytes={(totalBytes.HasValue ? totalBytes.Value.ToString() : "unknown")}");

            // 保存先フォルダが無ければ作る
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                FormMain.DebugInfo($"[Download] Created destination directory. path={directory}");
            }

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(
                destinationPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 8192,
                useAsync: true);

            var buffer = new byte[8192];
            long totalRead = 0;
            int read;

            Generic.IsDownloading = true;

            try
            {
                while ((read = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    totalRead += read;

                    if (totalBytes.HasValue && totalBytes.Value > 0)
                    {
                        int percent = (int)(totalRead * 100L / totalBytes.Value);
                        if (percent > 100) percent = 100;

                        // 既存の Generic.* をそのまま再利用
                        Generic.DownloadProgress = percent;
                        Generic.DownloadedStatus = string.Format(
                            Localization.DownloadingCaption,
                            percent,
                            totalBytes.Value / 1024,
                            totalRead / 1024);

                        progress.Report(percent);
                    }
                }
                FormMain.DebugInfo($"[Download] File write completed. path={destinationPath}, bytes={totalRead}");
            }
            finally
            {
                Generic.IsDownloading = false;
            }
        }

        private void UpdateProgress(int p)
        {
            switch (Generic.ProcessFlag)
            {
                case Constants.ProcessType.Update: // Update Program (Download)
                    progressBar_MainProgress.Maximum = 100;
                    if (p < progressBar_MainProgress.Minimum) p = progressBar_MainProgress.Minimum;
                    if (p > progressBar_MainProgress.Maximum) p = progressBar_MainProgress.Maximum;
                    progressBar_MainProgress.Value = p;

                    // DownloadedStatus は DownloadFileWithProgressAsync で更新
                    label_Status.Text = Generic.DownloadedStatus;
                    break;
                default:
                    int total = progressBar_MainProgress.Maximum;
                    if (total < 1)
                    {
                        int openFileCount = Generic.OpenFilePaths?.Length ?? 0;
                        total = Math.Max(Math.Max(Generic.ProgressMax, openFileCount), p);
                        if (total < 1)
                            total = 1;

                        progressBar_MainProgress.Maximum = total;
                    }

                    if (p < progressBar_MainProgress.Minimum) p = progressBar_MainProgress.Minimum;
                    if (p > progressBar_MainProgress.Maximum) p = progressBar_MainProgress.Maximum;

                    progressBar_MainProgress.Value = p;
                    label_Status.Text = string.Format(Localization.StatusCaption, p, total);
                    break;
            }
        }

        private void Button_Abort_Click(object sender, EventArgs e)
        {
            if (Generic.cts != null)
            {
                // ダウンロード中は確認ダイアログを出す
                if (Generic.ProcessFlag == Constants.ProcessType.Update)
                {
                    DialogResult dr = MessageBox.Show(
                        Localization.DownloadAbortConfirmCaption,
                        Localization.MSGBoxConfirmCaption,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (dr != DialogResult.Yes)
                    {
                        return;
                    }
                }

                Generic.cts.Cancel();
                button_Abort.Enabled = false;
                label_Status.Text = Localization.CancelledCaption;
            }
        }

        private static async Task MTK_ConvertAsync(Engine engine, MediaFile source, MediaFile dest, ConversionOptions co)
        {
            await Task.Run(() => engine.Convert(source, dest, co));
        }

        private static string BuildWalkmanParamForFile(string baseTemplate, string inputFileForTags)
        {
            try
            {
                using var tagFile = TagLib.File.Create(inputFileForTags);
                var tag = tagFile.Tag;
                var performers = tag.Performers ?? Array.Empty<string>();
                var genres = tag.Genres ?? Array.Empty<string>();

                string? title = string.IsNullOrWhiteSpace(tag.Title) ? null : tag.Title;
                string? artist = performers.Length > 0 ? performers[0] : null;
                string? album = string.IsNullOrWhiteSpace(tag.Album) ? null : tag.Album;
                string? genre = genres.Length > 0 ? genres[0] : null;

                // 年やトラック番号は、traconv側の実オプション名に合わせる必要があります。
                // ここでは既存GUIの項目に合わせて例示：Release / TrackNumber
                string? release = tag.Year > 0 ? tag.Year.ToString() : null;
                string? trackNo = tag.Track > 0 ? tag.Track.ToString() : null;

                string param = baseTemplate;

                if (title != null) param = ReplaceOrAppend(param, "--Title", title);
                if (artist != null) param = ReplaceOrAppend(param, "--Artist", artist);
                if (album != null) param = ReplaceOrAppend(param, "--Album", album);
                if (genre != null) param = ReplaceOrAppend(param, "--Genre", genre);
                if (release != null) param = ReplaceOrAppend(param, "--Release", release);
                if (trackNo != null) param = ReplaceOrAppend(param, "--TrackNumber", trackNo);

                return param;
            }
            catch
            {
                return baseTemplate;
            }
        }

        private static string AppendSwitch(string param, string key, string value)
        {
            // 値に " が入っていても壊れないようにエスケープ
            string escaped = value.Replace("\"", "\\\"");
            return param + $" {key} \"{escaped}\"";
        }

        private static string RemoveSwitch(string param, string key)
        {
            // 「--Key "xxx"」も「--Key xxx」も削除対象にする
            var rx = new System.Text.RegularExpressions.Regex(
                $"{System.Text.RegularExpressions.Regex.Escape(key)}\\s+(?:\"[^\"]*\"|\\S+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return rx.Replace(param, "").Trim();
        }

        private static string ReplaceOrAppend(string param, string key, string value)
        {
            var removeRx = new Regex(
        $"{Regex.Escape(key)}\\s+.*?(?=\\s--|$)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

            param = removeRx.Replace(param, "").Trim();

            // 正規形で追加
            string escaped = value.Replace("\"", "\\\"");
            return $"{param} {key} \"{escaped}\"";
        }

        private void Timer_interval_Tick(object sender, EventArgs e)
        {
            timer_interval.Enabled = false;
            DialogResult = Generic.Result ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        private void FormProgress_FormClosed(object sender, FormClosedEventArgs e)
        {
            Generic.ProcessFlag = Constants.ProcessType.None;
            FormMain.DebugInfo("[FormProgress] Closed.");
        }
    }
}
