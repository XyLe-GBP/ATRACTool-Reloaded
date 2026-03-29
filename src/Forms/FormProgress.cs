using ATRACTool_Reloaded.Localizable;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static ATRACTool_Reloaded.Common;

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

        public FormProgress()
        {
            InitializeComponent();

            FormMain.DebugInfo("[FormProgress] Initialized.");
        }

        private void FormProgress_Load(object sender, EventArgs e)
        {
            _formProgressInstance = this;

            timer_interval.Interval = 1000;
            progressBar_MainProgress.Value = 0;
            progressBar_MainProgress.Minimum = 0;
            progressBar_MainProgress.Maximum = Generic.ProgressMax;
            RunTask();
        }

        private async void RunTask()
        {
            // すべての処理で共通の CTS / Progress を使う
            Generic.cts = new CancellationTokenSource();
            var cToken = Generic.cts.Token;
            var progress = new Progress<int>(UpdateProgress);
            try
            {
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
                FormMain.DebugError(ex.Message);
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
            FileInfo fi = new(Generic.OpenFilePaths[0]);
            FileInfo fi2;

            bool playbackAtrac = Utils.GetBool("PlaybackATRAC", false);
            bool fasterAtrac = Utils.GetBool("FasterATRAC", false);
            bool atracEncodeSource = Utils.GetBool("ATRACEncodeSource", false);

            if (playbackAtrac && Generic.IsATRAC && !fasterAtrac)
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

            string ext = fi.Extension.ToUpperInvariant();

            if (ext == ".AT3")
            {
                string tool = Atrac3Tools[atrac3Console];
                if (!RunAtracTool(tool, Generic.DecodeParamAT3, Generic.OpenFilePaths[0], outPath, p, cToken)) return false;
            }
            else if (ext == ".AT9")
            {
                string tool = Atrac9Tools[atrac9Console];
                if (!RunAtracTool(tool, Generic.DecodeParamAT9, Generic.OpenFilePaths[0], outPath, p, cToken)) return false;
            }
            else if (ext == ".OMA")
            {
                // OMA → WAV は FFmpeg(MediaToolkit) 経由で _temp に吐く
                string ffpath = Path.Combine(Directory.GetCurrentDirectory(), "res", "ffmpeg.exe");
                if (!File.Exists(ffpath))
                {
                    FormMain.DebugError("ffmpeg.exe not found: " + ffpath);
                    return false;
                }

                // outPath が .wav 以外なら .wav に補正
                string outWav = outPath;
                if (!string.Equals(Path.GetExtension(outWav), ".wav", StringComparison.OrdinalIgnoreCase))
                    outWav = Path.ChangeExtension(outWav, ".wav");

                using var engine = new Engine(ffpath);

                // 失敗したら false（上位で Encode/Decode エラー扱い）
                if (!RunMediaToolkitDecodeToWav(engine, Generic.OpenFilePaths[0], outWav, p, cToken))
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

            string ffpath = Path.Combine(Directory.GetCurrentDirectory(), "res", "ffmpeg.exe");
            Engine? ffEngine = null;
            if (File.Exists(ffpath))
            {
                ffEngine = new Engine(ffpath);
            }

            for (int i = 0; i < Generic.OpenFilePaths.Length; i++)
            {
                string file = Generic.OpenFilePaths[i];
                FileInfo fi = new(file);

                // Origin があるならそちら優先（InputJobs 導入済みなら確実）
                string originKey =
                    (Generic.InputJobs != null && Generic.InputJobs.Count == Generic.OpenFilePaths.Length)
                        ? Generic.InputJobs[i].OriginPath
                        : file;

                string outPath = Utils.MakeTempUniquePath(TempDirectory, originKey, i, ".wav");

                switch (fi.Extension.ToUpperInvariant())
                {
                    case ".AT3":
                        {
                            switch (atrac3Console)
                            {
                                case Constants.ATRAC3ConsoleType.PSP:
                                    if (!RunAtracTool(Generic.PSP_ATRAC3tool, Generic.DecodeParamAT3, file, outPath, p, cToken)) return false;
                                    break;
                                case Constants.ATRAC3ConsoleType.PS3:
                                    if (!RunAtracTool(Generic.PS3_ATRAC3tool, Generic.DecodeParamAT3, file, outPath, p, cToken)) return false;
                                    break;
                            }
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
                            if (ffEngine == null)
                            {
                                FormMain.DebugError("ffmpeg.exe not found (OMA decode requires it): " + ffpath);
                                return false;
                            }

                            string outPath2 = Path.Combine(TempDirectory, fi.Name.Replace(fi.Extension, ".wav"));

                            if (!RunMediaToolkitDecodeToWav(ffEngine, file, outPath2, p, cToken))
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
                            string outPath2 = Path.Combine(TempDirectory, fi.Name.Replace(fi.Extension, ".wav"));

                            if (!RunMediaToolkitDecodeToWav(ffEngine, file, outPath2, p, cToken))
                                return false;

                            break;
                        }
                    default:
                        return false;
                }
            }
            ffEngine?.Dispose();

            return true;
        }

        private static bool Encode_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            Config.Load(xmlpath);

            int length = Generic.OpenFilePaths.Length;

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

            string atrac3Params = Utils.GetString("ATRAC3_Params", string.Empty);
            string atrac9Params = Utils.GetString("ATRAC9_Params", string.Empty);

            FileInfo fi, fi2;

            if (atracEncodeSource && Generic.IsATRAC)
            {
                fi = new(Generic.pATRACOpenFilePaths[0]);
                fi2 = new(Generic.pATRACSavePath);
            }
            else
            {
                fi = new(Generic.OpenFilePaths[0]);
                fi2 = new(Generic.SavePath);
            }


            switch (Generic.ATRACFlag)
            {
                case 0: // ATRAC3
                    {
                        var atrac3Console = (Constants.ATRAC3ConsoleType)Utils.GetInt("ATRAC3_Console", (int)Constants.ATRAC3ConsoleType.PSP);

                        // 出力先パス（単一ファイルなのでそのまま fi2.Name を使う）
                        string outPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "_temp",
                            fi2.Name);

                        switch (atrac3Console)
                        {
                            case Constants.ATRAC3ConsoleType.PSP: // PSP
                                {
                                    if (mloop)
                                    {
                                        Generic.EncodeParamAT3 = atrac3Params;
                                        Generic.EncodeParamAT3 += " -loop " + Generic.MultipleLoopStarts[0] + " " + Generic.MultipleLoopEnds[0];
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
                                            Generic.EncodeParamAT3 = atrac3Params;
                                            Generic.EncodeParamAT3 += Generic.LPCSuffix;
                                        }
                                        else
                                        {
                                            Generic.EncodeParamAT3 = atrac3Params;
                                        }
                                    }


                                    if (!RunAtracTool(
                                    Generic.PSP_ATRAC3tool,
                                    Generic.EncodeParamAT3,
                                    Generic.OpenFilePaths[0],
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
                                    if (mloop)
                                    {
                                        Generic.EncodeParamAT3 = atrac3Params;
                                        Generic.EncodeParamAT3 += " -loop " + Generic.MultipleLoopStarts[0] + " " + Generic.MultipleLoopEnds[0];
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
                                            Generic.EncodeParamAT3 = atrac3Params;
                                            Generic.EncodeParamAT3 += Generic.LPCSuffix;
                                        }
                                        else
                                        {
                                            Generic.EncodeParamAT3 = atrac3Params;
                                        }
                                    }


                                    if (!RunAtracTool(
                                    Generic.PS3_ATRAC3tool,
                                    Generic.EncodeParamAT3,
                                    Generic.OpenFilePaths[0],
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
                case 1: // ATRAC9
                    {
                        var atrac9Console = (Constants.ATRAC9ConsoleType)Utils.GetInt("ATRAC9_Console", (int)Constants.ATRAC9ConsoleType.PSV);

                        string outPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "_temp",
                            fi2.Name);

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
                                    Generic.OpenFilePaths[0],
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
                                    Generic.OpenFilePaths[0],
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
                case 2: // Walkman
                    {
                        string outPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "_temp",
                            fi2.Name);

                        var job = Common.Generic.InputJobs[0];
                        bool unattended = Utils.GetBool("Walkman_Unattended", false);

                        // unattended でなければ、ここでメタデータ設定画面を出せる（必要なときだけ）
                        if (!unattended)
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
                    return false;
            }
            return true;
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

            var atrac3Console = (Constants.ATRAC3ConsoleType)Utils.GetInt("ATRAC3_Console", (int)Constants.ATRAC3ConsoleType.PSP);
            var atrac9Console = (Constants.ATRAC9ConsoleType)Utils.GetInt("ATRAC9_Console", (int)Constants.ATRAC9ConsoleType.PSV);

            string[] fp;

            if (atracEncodeSource && Generic.IsATRAC)
            {
                fp = Generic.pATRACOpenFilePaths;
            }
            else
            {
                fp = Generic.OpenFilePaths;
            }

            // Walkman 用：毎回フォームを出さず、必要なら「最初に1回だけ」出す
            bool walkmanUnattended = Utils.GetBool("Walkman_Unattended", false);
            bool walkmanEveryFmt = Utils.GetBool("Walkman_FixSongInformation", false);

            if (Generic.ATRACFlag == 2 && !walkmanEveryFmt && !walkmanUnattended)
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

            // ここで確定した Walkman_Params を掴んでおく（フォームが保存した後）
            string walkmanParamTemplate = Utils.GetString("Walkman_Params", Generic.EncodeParamWalkman);

            if (Generic.ATRACFlag == 2)
            {
                var jobs = Common.Generic.InputJobs;
                if (jobs == null || jobs.Count == 0)
                    return false;

                // 念のため：OpenFilePaths と jobs 数がズレていたらここで止める
                if (jobs.Count != Common.Generic.OpenFilePaths.Length)
                {
                    FormMain.DebugError($"InputJobs mismatch. jobs={jobs.Count}, OpenFilePaths={Common.Generic.OpenFilePaths.Length}");
                    return false;
                }

                for (int i = 0; i < jobs.Count; i++)
                {
                    var job = jobs[i];

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
                }

                return true; // ★ここが重要：外側 foreach に入らない
            }

            foreach (var file in fp)
            {
                bool mloop = Generic.MultipleFilesLoopOKFlags[mpfloop];
                FileInfo fi = new(file);

                switch (Generic.ATRACFlag)
                {
                    case 0: // ATRAC3
                        {
                            switch (atrac3Console)
                            {
                                case Constants.ATRAC3ConsoleType.PSP: // PSP
                                    {
                                        Generic.EncodeParamAT3 = BuildAtracEncodeParam(
                                            atrac3Params,
                                            mloop,
                                            Generic.MultipleLoopStarts[mpfloop],
                                            Generic.MultipleLoopEnds[mpfloop],
                                            Generic.lpcreate,
                                            fs);


                                        Generic.ATRACExt = ".at3";
                                        string outPath = Path.Combine(TempDirectory, fi.Name.Replace(fi.Extension, Generic.ATRACExt));

                                        if (!RunAtracToolWithValidation(
                                                Generic.PSP_ATRAC3tool,
                                                Generic.EncodeParamAT3,
                                                file,
                                                outPath,
                                                fi.Name,
                                                p,
                                                cToken))
                                        {
                                            return false;
                                        }
                                    }
                                    break;
                                case Constants.ATRAC3ConsoleType.PS3: // PS3
                                    {
                                        Generic.EncodeParamAT3 = BuildAtracEncodeParam(
                                            atrac3Params,
                                            mloop,
                                            Generic.MultipleLoopStarts[mpfloop],
                                            Generic.MultipleLoopEnds[mpfloop],
                                            Generic.lpcreate,
                                            fs);


                                        Generic.ATRACExt = ".at3";
                                        string outPath = Path.Combine(TempDirectory, fi.Name.Replace(fi.Extension, Generic.ATRACExt));

                                        if (!RunAtracToolWithValidation(
                                                Generic.PS3_ATRAC3tool,
                                                Generic.EncodeParamAT3,
                                                file,
                                                outPath,
                                                fi.Name,
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
                    case 1: // ATRAC9
                        {
                            switch (atrac9Console)
                            {
                                case Constants.ATRAC9ConsoleType.PSV: // PSV
                                    {
                                        Generic.EncodeParamAT9 = BuildAtracEncodeParam(
                                            atrac9Params,
                                            mloop,
                                            Generic.MultipleLoopStarts[mpfloop],
                                            Generic.MultipleLoopEnds[mpfloop],
                                            Generic.lpcreate,
                                            fs);


                                        Generic.ATRACExt = ".at9";
                                        string outPath = Path.Combine(TempDirectory, fi.Name.Replace(fi.Extension, Generic.ATRACExt));

                                        if (!RunAtracToolWithValidation(
                                                Generic.PSV_ATRAC9tool,
                                                Generic.EncodeParamAT9,
                                                file,
                                                outPath,
                                                fi.Name,
                                                p,
                                                cToken))
                                        {
                                            return false;
                                        }
                                    }
                                    break;
                                case Constants.ATRAC9ConsoleType.PS4: // PS4
                                    {
                                        Generic.EncodeParamAT9 = BuildAtracEncodeParam(
                                            atrac9Params,
                                            mloop,
                                            Generic.MultipleLoopStarts[mpfloop],
                                            Generic.MultipleLoopEnds[mpfloop],
                                            Generic.lpcreate,
                                            fs);


                                        Generic.ATRACExt = ".at9";
                                        string outPath = Path.Combine(TempDirectory, fi.Name.Replace(fi.Extension, Generic.ATRACExt));

                                        if (!RunAtracToolWithValidation(
                                                Generic.PS4_ATRAC9tool,
                                                Generic.EncodeParamAT9,
                                                file,
                                                outPath,
                                                fi.Name,
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

                            break;
                        }
                    case 2: // Walkman
                        {
                            
                        }
                        break;
                    default:
                        return false;
                }
                fs++;
                mpfloop++;
            }
            return true;
        }

        /// <summary>
        /// ATRAC 関連ツールを起動して終了まで待機する共通ヘルパー。
        /// $InFile / $OutFile プレースホルダ展開と、
        /// at3tool / at9tool / traconv のプレフィックス削除もここで行う。
        /// </summary>
        private static bool RunAtracTool(
            string toolPath,
            string parameterTemplate,
            string inputFile,
            string outputFile,
            IProgress<int> progress,
            CancellationToken cToken)
        {
            var args = parameterTemplate
                .Replace("$InFile", "\"" + inputFile + "\"")
                .Replace("$OutFile", "\"" + outputFile + "\"")
                .Replace("at3tool ", "")
                .Replace("at9tool ", "")
                .Replace("traconv ", "");

            var pi = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var ps = Process.Start(pi);
            if (ps is null)
            {
                return false;
            }

            // デバッグ用ログはこれまで通り Generic.Log に保持
            Generic.Log = ps.StandardOutput;

            return WaitForProcessExit(ps, progress, cToken);
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
                return false;
            }

            try
            {
                var fi = new FileInfo(outputFile);
                if (fi.Exists && fi.Length == 0 && Generic.Log != null)
                {
                    string text = "'" + originalFileName + "' " + Utils.LogSplit(Generic.Log);
                    MessageBox.Show(
                        text,
                        Localization.MSGBoxErrorCaption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch
            {
                // ファイルアクセスで何か起きても、ここでは無視（変換自体は完了している）
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
        private static bool WaitForProcessExit(Process ps, IProgress<int> progress, CancellationToken cToken)
        {
            string tempPath = TempDirectory;

            while (!ps.HasExited)
            {
                if (cToken.IsCancellationRequested)
                {
                    try
                    {
                        if (!ps.HasExited)
                            ps.Kill(true);
                    }
                    catch { }
                    finally
                    {
                        ps.Close();
                    }
                    return false;
                }

                // 疑似進捗
                if (Directory.Exists(tempPath))
                {
                    int files = Directory.GetFiles(tempPath, "*").Length;
                    progress.Report(files);
                }

                Thread.Sleep(50); // CPU に優しい
            }

            // 正常終了
            ps.Close();
            return true;
        }

        private static bool WaitForConversion(Task conversionTask, IProgress<int> progress, CancellationToken cancellationToken)
        {
            string tempDir = TempDirectory;
            bool cancelled = false;

            while (!conversionTask.IsCompleted)
            {
                // キャンセルは「フラグとして覚えておくだけ」
                if (cancellationToken.IsCancellationRequested)
                {
                    cancelled = true;
                }

                // 疑似進捗更新
                if (Directory.Exists(tempDir))
                {
                    int files = Directory.GetFiles(tempDir, "*").Length;
                    progress.Report(files);
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
                throw ex.GetBaseException();
            }

            // キャンセルフラグが立っていたら false（上位で「中止」として扱う）
            return !cancelled;
        }

        private static bool AudioConverter_ATW_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            //int length = Generic.OpenFilePaths.Length;
            int length = (Generic.InputJobs != null && Generic.InputJobs.Count > 0) ? Generic.InputJobs.Count : Generic.OpenFilePaths.Length;
            string ffpath = Path.Combine(Directory.GetCurrentDirectory(), "res", "ffmpeg.exe");

            // WTAType から使うサンプルレートを決定
            AudioSampleRate sampleRate = GetSampleRateFromWtaType(Generic.WTAmethod);

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

            // 進捗初期値
            if (Directory.Exists(tempDir))
                p.Report(Directory.GetFiles(tempDir, "*").Length);

            // ★ InputJobs が未構築なら、OpenFilePaths/OriginOpenFilePaths から最低限構築しておく（保険）
            if (Generic.InputJobs == null || Generic.InputJobs.Count == 0)
            {
                // ここはあなたの Common.Generic.BuildInputJobsFromPaths(...) 相当を呼ぶ
                // 例: Common.Generic.BuildInputJobsFromPaths(Generic.OpenFilePaths, Generic.OriginOpenFilePaths);
                Generic.BuildInputJobsFromPaths(Generic.OpenFilePaths, Generic.OriginOpenFilePaths);
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

                var task = MTK_ConvertAsync(engine, source, output, co);
                if (!WaitForConversion(task, p, cToken)) return false;

                // ★ 成功したら WorkPath を wav に更新
                Generic.InputJobs[0].WorkPath = outPath;
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

                    var task = MTK_ConvertAsync(engine, source, output, co);
                    if (!WaitForConversion(task, p, cToken)) return false;

                    // ★ 各ジョブの WorkPath を更新
                    Generic.InputJobs[i].WorkPath = outPath;
                }
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
            return WaitForConversion(task, p, cToken);
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

            // 最初の進捗通知
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
                    // もともとの実装と同じ: _temp\SavePathのファイル名
                    Filename = Path.Combine(tempDir, fi.Name)
                };

                var co = new ConversionOptions
                {
                    AudioSampleRate = AudioSampleRate.Hz44100,
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
                    var task = MTK_ConvertAsync(engine, source, output, co);

                    if (!WaitForConversion(task, p, cToken))
                    {
                        return false;
                    }
                }

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

                // 実際のダウンロード（同期メソッド内なので GetAwaiter().GetResult()）
                DownloadFileWithProgressAsync(uri, targetPath, p, cToken)
                    .GetAwaiter()
                    .GetResult();

                if (cToken.IsCancellationRequested)
                {
                    return false;
                }

                // ダウンロード完了後は「処理中」表示に戻す
                Invoke(new Action(() => Text = Localization.ProcessingCaption));
                Invoke(new Action(() => label_Status.Text = Localization.ProcessingCaption));
                Invoke(new Action(() => button_Abort.Enabled = false));

                FormMain.DebugInfo("Download Completed.");
                return true;
            }
            catch (OperationCanceledException)
            {
                FormMain.DebugWarn("Operation Cancelled.");
                // キャンセルされた場合は false を返して中止扱い
                return false;
            }
            catch (Exception ex)
            {
                FormMain.DebugError(ex.Message);
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

            using var response = await httpClient.GetAsync(
                uri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;

            // 保存先フォルダが無ければ作る
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
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
                    if (p > progressBar_MainProgress.Maximum)
                    {
                        break;
                    }
                    progressBar_MainProgress.Value = p;
                    label_Status.Text = string.Format(Localization.StatusCaption, p, Generic.OpenFilePaths.Length);
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
                Close();
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
                var tagFile = TagLib.File.Create(inputFileForTags);
                var tag = tagFile.Tag;

                string? title = string.IsNullOrWhiteSpace(tag.Title) ? null : tag.Title;
                string? artist = (tag.Performers?.Length ?? 0) > 0 ? tag.Performers[0] : null;
                string? album = string.IsNullOrWhiteSpace(tag.Album) ? null : tag.Album;
                string? genre = (tag.Genres?.Length ?? 0) > 0 ? tag.Genres[0] : null;

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
            FormMain.DebugInfo("[FormProgress] Closed.");
        }
    }
}
