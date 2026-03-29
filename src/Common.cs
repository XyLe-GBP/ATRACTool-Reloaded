using ATRACTool_Reloaded.Localizable;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static ATRACTool_Reloaded.Common.Constants;

namespace ATRACTool_Reloaded
{
    public class Common
    {
        public static readonly string xmlpath = Directory.GetCurrentDirectory() + @"\app.config";

        internal static class AssemblyState
        {
            public const bool IsDebug =
        #if DEBUG
            true;
        #else
            false;
        #endif
        }

        public static class Constants
        {
            public enum ProcessType
            {
                None = -1,
                Decode = 0,
                Encode = 1,
                AudioToWave = 2,
                WaveToAudio = 3,
                Update = 4,
            }

            public enum WTAType
            {
                Hz8000 = 0,
                Hz12000 = 1,
                Hz16000 = 2,
                Hz24000 = 3,
                Hz32000 = 4,
                Hz44100 = 5,
                Hz48000 = 6,
            }

            public enum WalkmanFormatType
            {
                ATRAC3_OMA = 0,
                ATRAC3_OMG = 1,
                ATRAC3_AL = 2,
                ATRAC3_KDR = 3,
                ATRAC3plus_OMA = 4,
                ATRAC3plus_OMG = 5,
                ATRAC3plus_AL = 6,
                ATRAC3plus_KDR = 7,
            }

            public enum ATRAC3ConsoleType
            {
                PSP = 0,
                PS3 = 1
            }

            public enum ATRAC9ConsoleType
            {
                PSV = 0,
                PS4 = 1
            }

            public enum LPCPlaybackMethodType
            {
                DirectSound = 0,
                WasapiShared = 1,
                WasapiExclusive = 2,
                ASIO = 3
            }
        }

        public sealed class WalkmanMeta
        {
            public string Title { get; set; } = "";
            public string SortTitle { get; set; } = "";
            public string Artist { get; set; } = "";
            public string SortArtist { get; set; } = "";
            public string Album { get; set; } = "";
            public string SortAlbum { get; set; } = "";
            public string AlbumArtist { get; set; } = "";
            public string SortAlbumArtist { get; set; } = "";
            public string Genre { get; set; } = "";
            public string Composer { get; set; } = "";
            public string Lyricist { get; set; } = "";
            public string TrackNumber { get; set; } = "";
            public string TotalTracks { get; set; } = "";
            public string ReleaseYear { get; set; } = "";
            public string Import { get; set; } = "";

            // ★必須：--FileType（未指定だとPCM16になりタグが付かない）
            public string FileType { get; set; } = "";   // 例: "OMA3" / "OMAP" 等

            // ★Jacket
            public string JacketPath { get; set; } = ""; // 画像ファイルパス
            public string JacketMode { get; set; } = "Auto"; // Auto/Picture/Text/Delete (traconv仕様に合わせる)

            public string LyricsMode { get; set; } = "";
            public string LyricsPath { get; set; } = "";

            public string LinerNotesMode { get; set; } = "";
            public string LinerNotesPath { get; set; } = "";

            public WalkmanMeta Clone()
            {
                return (WalkmanMeta)MemberwiseClone();
            }

            public void ClearTagFieldsKeepRequired()
            {
                // タグ類だけ消す
                Title = "";
                SortTitle = "";
                Artist = "";
                SortArtist = "";
                Album = "";
                SortAlbum = "";
                AlbumArtist = "";
                SortAlbumArtist = "";
                Genre = "";
                Composer = "";
                Lyricist = "";
                TrackNumber = "";
                TotalTracks = "";
                ReleaseYear = "";
                Import = "";
                JacketPath = "";

                // Lyrics/LinerNotes を持っているなら同様に
                LyricsPath = "";
                LinerNotesPath = "";

                // ★重要：FileType は残す（OMA3 が必須という要件）
                if (string.IsNullOrWhiteSpace(FileType))
                    FileType = "OMA3";

                // JacketMode は Picture 強制でも良いが、JacketPath を消すので Auto でOK
                JacketMode = "Auto";
            }
        }

        public sealed class InputJob
        {
            public int Index { get; init; }
            // メタデータ取得に使う “本来の入力”
            public string OriginPath { get; init; } = "";

            // 実際に traconv に渡す “作業用入力”（WAV化したらこちらが変わる）
            public string WorkPath { get; set; } = "";

            // フォルダ入力時に、出力側で構造を再現するための情報（任意）
            public string? RootFolder { get; init; } = "";
            public string? RelativePath { get; init; } = "";
            // 表示名 (ハッシュを含めないようにする)
            public string DisplayName { get; init; } = "";

            public bool UserCancelled { get; set; } = false;

            public WalkmanMeta Meta { get; } = new WalkmanMeta();
        }

        public class Generic
        {
            /// <summary>
            /// インデックス整合の“唯一の真実”。Origin(元)とWork(処理用/WAV化後など)をペアで保持する。
            /// </summary>
            public static List<InputJob> InputJobs { get; set; } = [];
            public static string? LoadFolderRootPath = null;

            public static void BuildInputJobsFromPaths(string[] openPaths, string[] originPaths)
            {
                if (openPaths is null) throw new ArgumentNullException(nameof(openPaths));
                if (originPaths is null) throw new ArgumentNullException(nameof(originPaths));
                if (openPaths.Length != originPaths.Length)
                    throw new InvalidOperationException($"Open/Origin length mismatch: {openPaths.Length} vs {originPaths.Length}");

                InputJobs.Clear();

                for (int i = 0; i < openPaths.Length; i++)
                {
                    var job = new InputJob
                    {
                        Index = i,
                        OriginPath = originPaths[i],
                        WorkPath = openPaths[i],
                        RootFolder = LoadFolderRootPath,
                        RelativePath = (!string.IsNullOrEmpty(LoadFolderRootPath)) ? Path.GetRelativePath(LoadFolderRootPath, originPaths[i]) : Path.GetFileName(originPaths[i]),
                        DisplayName = Path.GetFileName(originPaths[i]),
                    };

                    // ★ここで Walkman メタを初期投入
                    Utils.PopulateWalkmanMetaFromOrigin(job);

                    InputJobs.Add(job);
                }
            }

            /// <summary>
            /// InputJobs → Open/Origin 配列へ同期（必要に応じて既存コード互換のため）
            /// </summary>
            public static void SyncPathsFromInputJobs()
            {
                OpenFilePaths = InputJobs.Select(j => j.WorkPath).ToArray();
                OriginOpenFilePaths = InputJobs.Select(j => j.OriginPath).ToArray();
            }

            /// <summary>
            /// 既存コードで OpenFilePaths を直接差し替えた場合に、InputJobs 側へ反映する
            /// </summary>
            public static void SyncJobsWorkPathsFromOpenFilePaths()
            {
                if (OpenFilePaths is null) return;
                if (InputJobs.Count != OpenFilePaths.Length)
                    throw new InvalidOperationException($"InputJobs/OpenFilePaths length mismatch: {InputJobs.Count} vs {OpenFilePaths.Length}");

                for (int i = 0; i < OpenFilePaths.Length; i++)
                {
                    InputJobs[i].WorkPath = OpenFilePaths[i];
                }
            }

            public static CancellationTokenSource cts = null!;

            public static bool Result = false;

            /// <summary>
            /// psp at3tool path string (res\psp_at3tool.exe)
            /// </summary>
            public static readonly string PSP_ATRAC3tool = Directory.GetCurrentDirectory() + @"\res\psp_at3tool.exe";
            /// <summary>
            /// ps3 at3tool path string (res\ps3_at3tool.exe)
            /// </summary>
            public static readonly string PS3_ATRAC3tool = Directory.GetCurrentDirectory() + @"\res\ps3_at3tool.exe";
            /// <summary>
            /// psv at9tool path string (res\psv_at9tool.exe)
            /// </summary>
            public static readonly string PSV_ATRAC9tool = Directory.GetCurrentDirectory() + @"\res\psv_at9tool.exe";
            /// <summary>
            /// ps4 at9tool path string (res\ps4_at9tool.exe)
            /// </summary>
            public static readonly string PS4_ATRAC9tool = Directory.GetCurrentDirectory() + @"\res\ps4_at9tool.exe";
            /// <summary>
            /// walkman traconv path string (res\traconv.exe)
            /// </summary>
            public static readonly string Walkman_TraConv = Directory.GetCurrentDirectory() + @"\res\traconv.exe";
            /// <summary>
            /// Progressフォームの動作を判定するための変数
            /// </summary>
            public static ProcessType ProcessFlag;
            //public static sbyte ProcessFlag = -1;
            public static int ProgressMax = 0;
            
            /// <summary>
            /// エンコードする際にループポイントを作成するかのフラグ
            /// </summary>
            public static bool lpcreate = false;
            public static string LPCSuffix = string.Empty;
            /// <summary>
            /// 変換設定ダイアログ用ループフラグ
            /// </summary>
            public static bool lpcreatev2 = false;
            public static int files = 0;

            public static bool[] MultipleFilesLoopOKFlags = [];
            public static int[] MultipleLoopStarts = [];
            public static int[] MultipleLoopEnds = [];
            public static bool LoopStartNG = false;
            public static bool LoopEndNG = false;
            public static bool IsLoopWarning = true;

            public static bool IsAT3LoopSound = false;
            public static bool IsAT9LoopSound = false;
            public static bool IsAT3LoopPoint = false;
            public static bool IsAT9LoopPoint = false;

            public static sbyte ReadedATRACFlag = -1;
            /// <summary>
            /// Toolstrip AT3,AT9,Walkmanのどれかを判定するための変数
            /// </summary>
            public static sbyte ATRACFlag = -1;
            public static string ATRACExt = "";
            public static sbyte TaskFlag = 0;
            public static bool IsWave = false;
            public static bool IsATRAC = false;
            public static bool IsWalkman = false;
            public static bool IsATRACLooped = false;
            public static bool LoopNG = false;

            /// <summary>
            /// AT3の変換メソッド判別
            /// </summary>
            public static bool IsAT3PS3 = false;
            public static ATRAC3ConsoleType ATRAC3ConsoleType;
            /// <summary>
            /// AT9の変換メソッド判別
            /// </summary>
            public static bool IsAT9PS4 = false;
            public static ATRAC9ConsoleType ATRAC9ConsoleType;
            /// <summary>
            /// ファイルをWaveに変換したかどうかを判別するための変数
            /// </summary>
            public static bool IsATW = false;
            public static bool IsATWCancelled = false;
            /// <summary>
            /// 変換先の形式を判別するための変数
            /// </summary>
            public static sbyte WTAFlag = -1;
            /// <summary>
            /// 開いたファイルパスの格納用
            /// </summary>
            public static string[] OpenFilePaths = null!;

            public static string[] FolderOpenPaths = null!;
            public static string[] SubFolderOpenPaths = null!;
            /// <summary>
            /// フォルダーを読み込んだ場合はtrue
            /// </summary>
            public static bool IsLoadFolder = false;
            /// <summary>
            /// 開いたATRACパスの格納用
            /// </summary>
            public static string[] pATRACOpenFilePaths = null!;
            public static string[] ATAOpenFilePaths = null!;
            /// <summary>
            /// デフォルトのソースファイルパスの格納用
            /// </summary>
            public static string[] OriginOpenFilePaths = null!;

            //public static string[] OpenFilePathsWithMultiExt = null!;
            public static bool IsOpenMulti = false;
            public static string SavePath = null!;
            public static string FolderSavePath = "";
            public static string pATRACSavePath = null!;
            public static string pATRACFolderSavePath = null!;

            //public static int WTAmethod = -1;
            public static WTAType WTAmethod;
            public static string WTAFmt = null!;
            

            public static string DecodeParamAT3 = "at3tool -d $InFile $OutFile";
            public static string DecodeParamAT9 = "at9tool -d $InFile $OutFile";
            public static string DecodeParamWalkman = "traconv --Convert $InFile $OutFile";
            public static string EncodeParamAT3 = "";
            public static string EncodeParamAT3_OLD = "";
            public static string EncodeParamAT9 = "";
            public static string EncodeParamAT9_OLD = "";
            public static string EncodeParamWalkman = "";
            public static string WalkmanEveryFilter = "";
            public static string WalkmanMultiConvFmt = "";
            public static string WalkmanMultiConvExt = "";

            public static string? CurrentWalkmanInputFile;

            public static StreamReader Log = null!;
            public static Exception GlobalException = null!;
            public static Exception CommonException = null!;
            public static bool LPCException = false;

            public static bool IsLPCStreamingReloaded = false;
            public static long LPCTotalSamples = 0;
            public static LPCPlaybackMethodType LPCPlaybackMethod;

            public static bool IsPlaybackATRAC = false;
            /// <summary>
            /// ATRACメタデータ情報設定
            /// </summary>
            public static int[] ATRACMetadataBuffers = null!;
            /// <summary>
            /// ATRACメタデータ情報設定 (複数ファイル)
            /// </summary>
            public static int[,] ATRACMultiMetadataBuffer = null!;
            public static string ATRACEncodeSourceTempPath = "";
            /// <summary>
            /// 設定ファイルエラー検知用変数 
            /// </summary>
            public static bool IsConfigError = false;
            /// <summary>
            /// ダウンロード機能用変数
            /// </summary>
            public static bool IsDownloading = false;
            public static string DownloadedStatus = "";
            public static int DownloadProgress = 0;

            public static bool ApplicationPortable = false;
            public static string? GitHubLatestVersion;
        }

        public class Utils
        {
            /// <summary>
            /// Process.Start: Open URI for .NET
            /// </summary>
            /// <param name="URI">http://~ または https://~ から始まるウェブサイトのURL</param>
            public static void OpenURI(string URI)
            {
                try
                {
                    Process.Start(URI);
                }
                catch
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        //Windowsのとき  
                        URI = URI.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {URI}") { CreateNoWindow = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        //Linuxのとき  
                        Process.Start("xdg-open", URI);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        //Macのとき  
                        Process.Start("open", URI);
                    }
                    else
                    {
                        throw;
                    }
                }

                return;
            }

            /// <summary>
            /// 現在の時刻を取得する
            /// </summary>
            /// <returns>YYYY-MM-DD-HH-MM-SS (例：2000-01-01-00-00-00)</returns>
            public static string SFDRandomNumber()
            {
                DateTime dt = DateTime.Now;
                return dt.Year + "-" + dt.Month + "-" + dt.Day + "-" + dt.Hour + "-" + dt.Minute + "-" + dt.Second;
            }

            public static string[] GetFolderAllFiles(string folderPath)
            {
                // 必要ならフィルタ（拡張子制限）をここでかける
                return Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories).ToArray();
            }

            /*public static void GetFolderAllFiles(string FolderPath)
            {
                Generic.FolderOpenPaths = FolderPath.Split('/');
                uint filecount = 0, subfoldercount = 0;

                // フォルダ内のすべてのファイルを取得
                string[] files = Directory.GetFiles(FolderPath);
                List<string> lst = [.. files];
                if (Generic.SubFolderOpenPaths is not null && Generic.SubFolderOpenPaths.Length != 0)
                {
                    Generic.OpenFilePaths = Generic.OpenFilePaths.Concat(lst.ToArray()).ToArray();
                    Generic.OriginOpenFilePaths = Generic.OriginOpenFilePaths.Concat(lst.ToArray()).ToArray();
                }
                else
                {
                    Generic.OpenFilePaths = lst.ToArray();
                    Generic.OriginOpenFilePaths = lst.ToArray();
                }

                foreach (string file in files)
                {
                    Debug.WriteLine($"ファイル: {Path.GetFileName(file)}");
                    filecount++;
                }
                
                // フォルダ内のすべてのサブディレクトリを取得
                string[] directories = Directory.GetDirectories(FolderPath);
                List<string> dlst = [.. directories];

                if (Generic.SubFolderOpenPaths is not null && directories.Length != 0)
                {
                    
                    Generic.SubFolderOpenPaths = Generic.SubFolderOpenPaths.Concat(dlst.ToArray()).ToArray();
                }
                else
                {
                    if (dlst.Count != 0)
                    {
                        Generic.SubFolderOpenPaths = dlst.ToArray();
                    }
                    
                }
                    

                foreach (string directory in directories)
                {
                    Debug.WriteLine($"サブフォルダ: {Path.GetFileName(directory)}");
                    subfoldercount++;
                    // 再帰的に呼び出す
                    GetFolderAllFiles(directory);
                }
            }*/

            /// <summary>
            /// 指定したディレクトリ内のファイルも含めてディレクトリを削除する
            /// </summary>
            /// <param name="targetDirectoryPath">削除するディレクトリのパス</param>
            public static void DeleteDirectory(string targetDirectoryPath)
            {
                if (string.IsNullOrWhiteSpace(targetDirectoryPath)) return;
                if (!Directory.Exists(targetDirectoryPath)) return;

                // 再試行回数と間隔（現実的に効く設定）
                const int maxRetry = 20;
                const int delayMs = 50;

                // まず属性を正規化（ディレクトリも含めて）
                try { RemoveReadonlyAttribute(new DirectoryInfo(targetDirectoryPath)); } catch { /* ignore */ }

                for (int attempt = 1; attempt <= maxRetry; attempt++)
                {
                    try
                    {
                        // ファイルを先に消す（下位から消す）
                        foreach (var filePath in Directory.EnumerateFiles(targetDirectoryPath, "*", SearchOption.AllDirectories))
                        {
                            try
                            {
                                File.SetAttributes(filePath, FileAttributes.Normal);
                                File.Delete(filePath);
                            }
                            catch (IOException ioe)
                            {
                                // ロック等。次のリトライへ
                                FormMain.DebugError($"IOException: {filePath}\n{ioe}");
                            }
                            catch (UnauthorizedAccessException uae)
                            {
                                // アクセス権や一時ロック。次のリトライへ
                                FormMain.DebugError($"UnauthorizedAccessException: {filePath}\n{uae}");
                            }
                        }

                        // ディレクトリを消す（再帰）
                        Directory.Delete(targetDirectoryPath, recursive: true);
                        return; // 成功
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(delayMs);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Thread.Sleep(delayMs);
                    }
                }

                // ここまで来たら、かなり頑固に掴まれている。
                // 例外にして呼び出し側で MessageBox などに出した方が原因究明が早いです。
                throw new IOException($"Failed to delete directory after retries: {targetDirectoryPath}");
            }

            public static void TryDeleteDirectoryContents(string dir)
            {
                if (!Directory.Exists(dir)) return;

                // ファイルを消せるだけ消す（ロックは無視）
                foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        // 他プロセスが掴んでいる → 残す
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // 権限/ロック → 残す
                    }
                }

                // 空になったディレクトリだけ消す（下から順に）
                var subDirs = Directory.EnumerateDirectories(dir, "*", SearchOption.AllDirectories)
                    .OrderByDescending(d => d.Length);
                foreach (var d in subDirs)
                {
                    try { Directory.Delete(d, recursive: false); } catch { /* 残す */ }
                }
            }

            /// <summary>
            /// 指定したディレクトリ内のファイルのみを削除する
            /// </summary>
            /// <param name="targetDirectoryPath">削除するディレクトリのパス</param>
            public static void DeleteDirectoryFiles(string targetDirectoryPath)
            {
                if (!Directory.Exists(targetDirectoryPath))
                {
                    return;
                }

                DirectoryInfo di = new(targetDirectoryPath);
                FileInfo[] fi = di.GetFiles();
                foreach (var file in fi)
                {
                    file.Delete();
                }
                return;
            }

            /// <summary>
            /// ファイル、フォルダの属性を解除
            /// </summary>
            /// <param name="dirInfo"></param>
            public static void RemoveReadonlyAttribute(DirectoryInfo dirInfo)
            {
                //基のフォルダの属性を変更
                if ((dirInfo.Attributes & FileAttributes.ReadOnly) ==
                    FileAttributes.ReadOnly)
                    dirInfo.Attributes = FileAttributes.Normal;
                //フォルダ内のすべてのファイルの属性を変更
                foreach (FileInfo fi in dirInfo.GetFiles())
                    if ((fi.Attributes & FileAttributes.ReadOnly) ==
                        FileAttributes.ReadOnly)
                        fi.Attributes = FileAttributes.Normal;
                //サブフォルダの属性を回帰的に変更
                foreach (DirectoryInfo di in dirInfo.GetDirectories())
                    RemoveReadonlyAttribute(di);
            }

            public static void SetWTAFormat(int WTAFlag)
            {
                Generic.WTAFmt = WTAFlag switch
                {
                    // AAC
                    0 => ".m4a",
                    // AIFF
                    1 => ".aiff",
                    // ALAC
                    2 => ".alac",
                    // FLAC
                    3 => ".flac",
                    // MP3
                    4 => ".mp3",
                    // WAV
                    5 => ".wav",
                    // OGG
                    6 => ".ogg",
                    _ => ".mp3",
                };
            }

            public static string LogSplit(StreamReader streamReader)
            {
                string read = streamReader.ReadToEnd();
                int pos = read.IndexOf("SCEI");
                if (pos != -1)
                {
                    return read[..pos];
                }
                else
                {
                    return null!;
                }
            }

            /// <summary>
            /// 完了後に保存先のフォルダを開く
            /// </summary>
            /// <param name="Fullpath">フォルダのフルパス</param>
            /// <param name="Flag">フラグ</param>
            public static void ShowFolder(string Fullpath, bool Flag = true)
            {
                if (Flag != false)
                {
                    Process.Start("EXPLORER.EXE", @"/select,""" + Fullpath + @"""");
                    return;
                }
                else
                {
                    return;
                }
            }

            /// <summary>
            /// 該当ファイルが存在する場合は削除
            /// </summary>
            /// <param name="path">ファイルの場所</param>
            public static void CheckExistsFile(string path)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return;
                }
                else
                {
                    return;
                }
            }

            /// <summary>
            /// IsATWがtrueならWaveに変換したファイルを削除
            /// </summary>
            /// <param name="flag">Generic.IsATW</param>
            public static void ATWCheck(bool flag, bool IsCancelled = false)
            {
                switch (flag)
                {
                    case true:
                        {
                            if (IsCancelled)
                            {
                                if (Directory.Exists(Directory.GetCurrentDirectory() + @"\_tempAudio"))
                                {
                                    Directory.Delete(Directory.GetCurrentDirectory() + @"\_tempAudio");
                                }
                                Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            }
                            else
                            {
                                foreach (var file in Generic.OpenFilePaths)
                                {
                                    if (File.Exists(file))
                                    {
                                        File.Delete(file);
                                    }
                                }
                                if (Directory.Exists(Directory.GetCurrentDirectory() + @"\_tempAudio"))
                                {
                                    Directory.Delete(Directory.GetCurrentDirectory() + @"\_tempAudio");
                                }
                            }
                            break;
                        }
                    case false:
                        {
                            break;
                        }
                }
                Generic.IsOpenMulti = false;
                Generic.IsATW = false;
                return;
            }

            public static string ATWSuffix()
            {
                Config.Load(xmlpath);
                if (bool.Parse(Config.Entry["Save_DeleteHzSuffix"].Value))
                {
                    return string.Empty;
                }
                else
                {
                    return Generic.WTAmethod switch
                    {
                        WTAType.Hz44100 => "_44.1k",
                        WTAType.Hz48000 => "_48k",
                        WTAType.Hz8000 => "_8k",
                        WTAType.Hz12000 => "_12k",
                        WTAType.Hz16000 => "_16k",
                        WTAType.Hz24000 => "_24k",
                        WTAType.Hz32000 => "_32k",
                        _ => string.Empty,
                    };
                }
            }

            public static string MakeUniquePath(string fullPath)
            {
                if (!File.Exists(fullPath)) return fullPath;

                string dir = Path.GetDirectoryName(fullPath)!;
                string name = Path.GetFileNameWithoutExtension(fullPath);
                string ext = Path.GetExtension(fullPath);

                for (int i = 2; i < 10000; i++)
                {
                    string cand = Path.Combine(dir, $"{name} ({i}){ext}");
                    if (!File.Exists(cand)) return cand;
                }

                return Path.Combine(dir, $"{name}_{Guid.NewGuid():N}{ext}");
            }

            public static string MakeTempUniqueName(string originPathOrName, int index, string newExt)
            {
                // originPathOrName はフルパスでもファイル名でもOK
                string stem = Path.GetFileNameWithoutExtension(originPathOrName);

                // 連番で一意化（ハッシュは使わない）
                return $"{stem}__{index:D4}{newExt}";
            }

            public static string MakeTempUniquePath(string tempDir, string originPathOrName, int index, string newExt)
            {
                return Path.Combine(tempDir, MakeTempUniqueName(originPathOrName, index, newExt));
            }

            private static string MakeTempBaseName(string originPath)
            {
                // 表示用ではない（内部ファイル名専用）
                var stem = Path.GetFileNameWithoutExtension(originPath);

                using var sha1 = System.Security.Cryptography.SHA1.Create();
                var bytes = System.Text.Encoding.UTF8.GetBytes(originPath);
                var hash = Convert.ToHexString(sha1.ComputeHash(bytes)).Substring(0, 8);

                return $"{stem}__{hash}";
            }

            public static string MakeNonCollidingPath(string desiredPath)
            {
                if (!File.Exists(desiredPath)) return desiredPath;

                string dir = Path.GetDirectoryName(desiredPath)!;
                string name = Path.GetFileNameWithoutExtension(desiredPath);
                string ext = Path.GetExtension(desiredPath);

                int n = 1;
                while (true)
                {
                    string p = Path.Combine(dir, $"{name}({n}){ext}");
                    if (!File.Exists(p)) return p;
                    n++;
                }
            }

            public static string EnsureDirectory(string dir)
            {
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return dir;
            }

            public static string MakeUniqueDestPath(string destDir, string baseName, string ext)
            {
                string candidate = Path.Combine(destDir, baseName + ext);
                if (!File.Exists(candidate)) return candidate;

                for (int i = 1; ; i++)
                {
                    string c = Path.Combine(destDir, $"{baseName}({i}){ext}");
                    if (!File.Exists(c)) return c;
                }
            }

            /// <summary>
            /// 32bit環境、または64bit環境で64bitアプリケーションのインストールした場合
            /// </summary>
            /// <returns></returns>
            public static bool OpenMGCheck64()
            {
                List<string> ret = [];

                string uninstall_path = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
                Microsoft.Win32.RegistryKey uninstall = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(uninstall_path, false)!;
                if (uninstall != null)
                {
                    foreach (string subKey in uninstall.GetSubKeyNames())
                    {
                        string appName = null!;
                        Microsoft.Win32.RegistryKey appkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(uninstall_path + "\\" + subKey, false)!;

                        if (appkey.GetValue("DisplayName") != null)
                            appName = appkey.GetValue("DisplayName")!.ToString()!;
                        else
                            appName = subKey;

                        ret.Add(appName);
                    }

                    foreach (var item in ret)
                    {
                        if (item is not null && item.Contains("Sony Media Library Earth"))
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// 64bit環境で32bitアプリケーションをインストールした場合
            /// </summary>
            /// <returns></returns>
            public static bool OpenMGCheck64_32()
            {
                List<string> ret = [];

                string uninstall_path = "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
                Microsoft.Win32.RegistryKey uninstall = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(uninstall_path, false)!;
                if (uninstall != null)
                {
                    foreach (string subKey in uninstall.GetSubKeyNames())
                    {
                        string appName = null!;
                        Microsoft.Win32.RegistryKey appkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(uninstall_path + "\\" + subKey, false)!;

                        if (appkey.GetValue("DisplayName") != null)
                            appName = appkey.GetValue("DisplayName")!.ToString()!;
                        else
                            appName = subKey;

                        ret.Add(appName);
                    }

                    foreach (var item in ret)
                    {
                        if (item is not null && item.Contains("Sony Media Library Earth"))
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }

            public static void CreateExceptionLog(Exception ex, bool FolderOpen, IWin32Window? owner = null)
            {
                string logname = @"Exception_" + Utils.SFDRandomNumber() + @".log";
                GenerateLog(logname, Utils.SFDRandomNumber() + @": " + ex.Message + "\n\nSource:\n" + ex.Source + "\n\nStack trace:\n" + ex.StackTrace + "\n\nEOF.\n");
                MessageBox.Show(owner, "An unexpected error has occurred.\nThe log file was output to the same location as the application directory.", Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowFolder(Directory.GetCurrentDirectory() + @"\Logs\" + logname, FolderOpen);
            }

            public static void GenerateLog(string Logfilename, string Logs)
            {
                if (string.IsNullOrWhiteSpace(Logfilename) || string.IsNullOrWhiteSpace(Logs))
                {
                    return;
                }
                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\Logs\"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Logs\");
                }
                using var stream = new StreamWriter(Directory.GetCurrentDirectory() + @"\Logs\" + Logfilename, true, Encoding.UTF8) { AutoFlush = true };
                stream.WriteLine(Logs);
                stream.Close();

                return;
            }

            public static void PictureboxImageDispose(PictureBox pictureBox)
            {
                if (pictureBox.Image is not null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }
            }

            public static bool GetATRACLooped(int[] ATRACBuffers, int[] LoopsBuffers)
            {
                if (ATRACBuffers.Length != 3 || LoopsBuffers.Length != 2)
                {
                    return false;
                }

                if (ATRACBuffers[0] != 0 && ATRACBuffers[1] != 0)
                {
                    LoopsBuffers[0] = ATRACBuffers[0];
                    LoopsBuffers[1] = ATRACBuffers[1];
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static bool GetATRACLoopedMulti(string[] ATRACFiles, int[,] MultiATRACBuffers, int[] LoopsBuffers, uint pos)
            {
                //uint count = 0, error = 0;
                if (MultiATRACBuffers.Length != ATRACFiles.Length * 3 || LoopsBuffers.Length != 2)
                {
                    return false;
                }

                if (MultiATRACBuffers[pos, 0] != 0 && MultiATRACBuffers[pos, 1] != 0)
                {
                    LoopsBuffers[0] = MultiATRACBuffers[pos, 0];
                    LoopsBuffers[1] = MultiATRACBuffers[pos, 1];
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static bool ReadMetadatas(string ATRACfile, int[] buffers)
            {
                try
                {
                    if (buffers.Length != 3)
                    {
                        return false;
                    }
                    
                    using var fs = new FileStream(ATRACfile, FileMode.Open);
                    using var br = new BinaryReader(fs);

                    br.BaseStream.Seek(24, SeekOrigin.Begin);
                    byte[] rsamplelate = br.ReadBytes(2);

                    byte[] chunk;
                    if (Generic.ReadedATRACFlag == 0 && !Generic.IsAT3PS3)
                    {
                        br.BaseStream.Seek(88, SeekOrigin.Begin);
                        chunk = br.ReadBytes(4);
                    }
                    else
                    {
                        br.BaseStream.Seek(92, SeekOrigin.Begin);
                        chunk = br.ReadBytes(4);
                    }

                    string chunkstr = Encoding.GetEncoding("US-ASCII").GetString(chunk);

                    if (!string.IsNullOrWhiteSpace(chunkstr) && chunkstr == "smpl") // SampleChunk Found
                    {
                        if (Generic.ReadedATRACFlag == 0) // ATRAC3
                        {
                            if (Generic.IsAT3PS3) // PS3
                            {
                                br.BaseStream.Seek(144, SeekOrigin.Begin);
                                byte[] rloop_start = br.ReadBytes(4);

                                br.BaseStream.Seek(148, SeekOrigin.Begin);
                                byte[] rloop_end = br.ReadBytes(4);

                                buffers[0] = BitConverter.ToInt32(rloop_start, 0) - 3271;
                                buffers[1] = BitConverter.ToInt32(rloop_end, 0) - 3270;
                                buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                            }
                            else // PSP
                            {
                                br.BaseStream.Seek(140, SeekOrigin.Begin);
                                byte[] rloop_start = br.ReadBytes(4);

                                br.BaseStream.Seek(144, SeekOrigin.Begin);
                                byte[] rloop_end = br.ReadBytes(4);

                                buffers[0] = BitConverter.ToInt32(rloop_start, 0) - 2459;
                                buffers[1] = BitConverter.ToInt32(rloop_end, 0) - 2458;
                                buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                            }
                        }
                        else if (Generic.ReadedATRACFlag == 1) // ATRAC9
                        {
                            br.BaseStream.Seek(144, SeekOrigin.Begin);
                            byte[] rloop_start = br.ReadBytes(4);

                            br.BaseStream.Seek(148, SeekOrigin.Begin);
                            byte[] rloop_end = br.ReadBytes(4);

                            if (Generic.IsAT9PS4) // PS4
                            {
                                switch (BitConverter.ToUInt16(rsamplelate, 0))
                                {
                                    case 12000:
                                        {
                                            buffers[0] = BitConverter.ToInt32(rloop_start, 0) - 64;
                                            buffers[1] = BitConverter.ToInt32(rloop_end, 0) - 63;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    case 24000:
                                        {
                                            buffers[0] = BitConverter.ToInt32(rloop_start, 0) - 128;
                                            buffers[1] = BitConverter.ToInt32(rloop_end, 0) - 127;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    case 48000:
                                        {
                                            buffers[0] = BitConverter.ToInt32(rloop_start, 0) - 256;
                                            buffers[1] = BitConverter.ToInt32(rloop_end, 0) - 255;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    default:
                                        {
                                            return false;
                                        }
                                }
                            }
                            else // PSV
                            {
                                switch (BitConverter.ToUInt16(rsamplelate, 0))
                                {
                                    case 12000:
                                        {
                                            buffers[0] = BitConverter.ToInt32(rloop_start, 0) - 64;
                                            buffers[1] = BitConverter.ToInt32(rloop_end, 0) - 63;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    case 24000:
                                        {
                                            buffers[0] = BitConverter.ToInt32(rloop_start, 0) - 128;
                                            buffers[1] = BitConverter.ToInt32(rloop_end, 0) - 127;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    case 48000:
                                        {
                                            buffers[0] = BitConverter.ToInt32(rloop_start, 0) - 256;
                                            buffers[1] = BitConverter.ToInt32(rloop_end, 0) - 255;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    default:
                                        {
                                            return false;
                                        }
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(chunkstr) && chunkstr == "data") // SampleChunk not found
                    {
                        if (Generic.ReadedATRACFlag == 0) // ATRAC3
                        {
                            if (Generic.IsAT3PS3) // PS3
                            {
                                buffers[0] = 0;
                                buffers[1] = 0;
                                buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                            }
                            else // PSP
                            {
                                buffers[0] = 0;
                                buffers[1] = 0;
                                buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                            }
                        }
                        else if (Generic.ReadedATRACFlag == 1) // ATRAC9
                        {
                            if (Generic.IsAT9PS4) // PS4
                            {
                                switch (BitConverter.ToUInt16(rsamplelate, 0))
                                {
                                    case 12000:
                                        {
                                            buffers[0] = 0;
                                            buffers[1] = 0;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    case 24000:
                                        {
                                            buffers[0] = 0;
                                            buffers[1] = 0;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    case 48000:
                                        {
                                            buffers[0] = 0;
                                            buffers[1] = 0;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    default:
                                        {
                                            return false;
                                        }
                                }
                            }
                            else // PSV
                            {
                                switch (BitConverter.ToUInt16(rsamplelate, 0))
                                {
                                    case 12000:
                                        {
                                            buffers[0] = 0;
                                            buffers[1] = 0;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    case 24000:
                                        {
                                            buffers[0] = 0;
                                            buffers[1] = 0;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    case 48000:
                                        {
                                            buffers[0] = 0;
                                            buffers[1] = 0;
                                            buffers[2] = BitConverter.ToUInt16(rsamplelate, 0);
                                            break;
                                        }
                                    default:
                                        {
                                            return false;
                                        }
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Generic.CommonException = e;
                    Utils.CreateExceptionLog(e, true);
                    return false;
                }
            }

            /// <summary>
            /// ATRACの生データを読み取りバッファに保存
            /// </summary>
            /// <param name="ATRACfile">ATRACファイル</param>
            /// <param name="mbuffer">多次元配列バッファ</param>
            /// <returns></returns>
            public static bool ReadMetadatasMulti(string[] ATRACfile, int[,] mbuffer)
            {
                try
                {
                    if (mbuffer.Length != ATRACfile.Length * 3)
                    {
                        return false;
                    }

                    uint count = 0, error = 0;
                    foreach (var file in ATRACfile)
                    {
                        using var fs = new FileStream(file, FileMode.Open);
                        using var br = new BinaryReader(fs);

                        br.BaseStream.Seek(24, SeekOrigin.Begin);
                        byte[] rsamplelate = br.ReadBytes(2);

                        byte[] chunk;
                        if (Generic.ReadedATRACFlag == 0 && !Generic.IsAT3PS3)
                        {
                            br.BaseStream.Seek(88, SeekOrigin.Begin);
                            chunk = br.ReadBytes(4);
                        }
                        else
                        {
                            br.BaseStream.Seek(92, SeekOrigin.Begin);
                            chunk = br.ReadBytes(4);
                        }

                        string chunkstr = Encoding.GetEncoding("US-ASCII").GetString(chunk);
                        if (!string.IsNullOrWhiteSpace(chunkstr) && chunkstr == "smpl") // SampleChunk Found
                        {
                            if (Generic.ReadedATRACFlag == 0) // ATRAC3
                            {
                                if (Generic.IsAT3PS3) // PS3
                                {
                                    br.BaseStream.Seek(144, SeekOrigin.Begin);
                                    byte[] rloop_start = br.ReadBytes(4);

                                    br.BaseStream.Seek(148, SeekOrigin.Begin);
                                    byte[] rloop_end = br.ReadBytes(4);

                                    mbuffer[count, 0] = BitConverter.ToInt32(rloop_start, 0) - 3271;
                                    mbuffer[count, 1] = BitConverter.ToInt32(rloop_end, 0) - 3270;
                                    mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                }
                                else // PSP
                                {
                                    br.BaseStream.Seek(140, SeekOrigin.Begin);
                                    byte[] rloop_start = br.ReadBytes(4);

                                    br.BaseStream.Seek(144, SeekOrigin.Begin);
                                    byte[] rloop_end = br.ReadBytes(4);

                                    mbuffer[count, 0] = BitConverter.ToInt32(rloop_start, 0) - 2459;
                                    mbuffer[count, 1] = BitConverter.ToInt32(rloop_end, 0) - 2458;
                                    mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                }
                            }
                            else if (Generic.ReadedATRACFlag == 1) // ATRAC9
                            {
                                br.BaseStream.Seek(144, SeekOrigin.Begin);
                                byte[] rloop_start = br.ReadBytes(4);

                                br.BaseStream.Seek(148, SeekOrigin.Begin);
                                byte[] rloop_end = br.ReadBytes(4);

                                if (Generic.IsAT9PS4) // PS4
                                {
                                    switch (BitConverter.ToUInt16(rsamplelate, 0))
                                    {
                                        case 12000:
                                            {
                                                mbuffer[count, 0] = BitConverter.ToInt32(rloop_start, 0) - 64;
                                                mbuffer[count, 1] = BitConverter.ToInt32(rloop_end, 0) - 63;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        case 24000:
                                            {
                                                mbuffer[count, 0] = BitConverter.ToInt32(rloop_start, 0) - 128;
                                                mbuffer[count, 1] = BitConverter.ToInt32(rloop_end, 0) - 127;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        case 48000:
                                            {
                                                mbuffer[count, 0] = BitConverter.ToInt32(rloop_start, 0) - 256;
                                                mbuffer[count, 1] = BitConverter.ToInt32(rloop_end, 0) - 255;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        default:
                                            {
                                                error++;
                                                break;
                                            }
                                    }
                                }
                                else // PSV
                                {
                                    switch (BitConverter.ToUInt16(rsamplelate, 0))
                                    {
                                        case 12000:
                                            {
                                                mbuffer[count, 0] = BitConverter.ToInt32(rloop_start, 0) - 64;
                                                mbuffer[count, 1] = BitConverter.ToInt32(rloop_end, 0) - 63;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        case 24000:
                                            {
                                                mbuffer[count, 0] = BitConverter.ToInt32(rloop_start, 0) - 128;
                                                mbuffer[count, 1] = BitConverter.ToInt32(rloop_end, 0) - 127;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        case 48000:
                                            {
                                                mbuffer[count, 0] = BitConverter.ToInt32(rloop_start, 0) - 256;
                                                mbuffer[count, 1] = BitConverter.ToInt32(rloop_end, 0) - 255;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        default:
                                            {
                                                error++;
                                                break;
                                            }
                                    }
                                }
                            }
                            else
                            {
                                error++;
                            }
                            count++;
                        }
                        else if (!string.IsNullOrWhiteSpace(chunkstr) && chunkstr == "data") // SampleChunk not found
                        {
                            if (Generic.ReadedATRACFlag == 0) // ATRAC3
                            {
                                if (Generic.IsAT3PS3) // PS3
                                {
                                    mbuffer[count, 0] = 0;
                                    mbuffer[count, 1] = 0;
                                    mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                }
                                else // PSP
                                {
                                    mbuffer[count, 0] = 0;
                                    mbuffer[count, 1] = 0;
                                    mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                }
                            }
                            else if (Generic.ReadedATRACFlag == 1) // ATRAC9
                            {
                                if (Generic.IsAT9PS4) // PS4
                                {
                                    switch (BitConverter.ToUInt16(rsamplelate, 0))
                                    {
                                        case 12000:
                                            {
                                                mbuffer[count, 0] = 0;
                                                mbuffer[count, 1] = 0;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        case 24000:
                                            {
                                                mbuffer[count, 0] = 0;
                                                mbuffer[count, 1] = 0;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        case 48000:
                                            {
                                                mbuffer[count, 0] = 0;
                                                mbuffer[count, 1] = 0;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        default:
                                            {
                                                error++;
                                                break;
                                            }
                                    }
                                }
                                else // PSV
                                {
                                    switch (BitConverter.ToUInt16(rsamplelate, 0))
                                    {
                                        case 12000:
                                            {
                                                mbuffer[count, 0] = 0;
                                                mbuffer[count, 1] = 0;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        case 24000:
                                            {
                                                mbuffer[count, 0] = 0;
                                                mbuffer[count, 1] = 0;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        case 48000:
                                            {
                                                mbuffer[count, 0] = 0;
                                                mbuffer[count, 1] = 0;
                                                mbuffer[count, 2] = BitConverter.ToUInt16(rsamplelate, 0);
                                                break;
                                            }
                                        default:
                                            {
                                                error++;
                                                break;
                                            }
                                    }
                                }
                            }
                            else
                            {
                                error++;
                            }
                            count++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    
                    if (error != 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                        
                }
                catch (Exception e)
                {
                    Generic.CommonException = e;
                    Utils.CreateExceptionLog(e, true);
                    return false;
                }
            }

            /// <summary>
            /// ATRAC変換時にエラーがないかチェック
            /// </summary>
            /// <param name="sourceSampleRate">今現在読み込まれているファイルのサンプリング周波数</param>
            /// <returns>false: エラーなし<br />true: エラーあり</returns>
            public static bool CheckATRACFormatError(int sourceSampleRate)
            {
                try
                {
                    Config.Load(xmlpath);
                    int ConfigAT9SampleRate = Utils.GetInt("ATRAC9_SamplingValue");

                    if (Generic.ReadedATRACFlag == 0) // ATRAC3
                    {
                        if (Generic.IsAT3PS3) // PS3
                        {
                            if (sourceSampleRate == 48000)
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else // PSP
                        {
                            if (sourceSampleRate == 44100)
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    else if (Generic.ReadedATRACFlag == 1) // ATRAC9
                    {
                        if (Generic.IsAT9PS4) // PS4
                        {
                            switch (sourceSampleRate)
                            {
                                case 8000:
                                    if (ConfigAT9SampleRate == 48000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 12000:
                                    if (ConfigAT9SampleRate == 48000 || ConfigAT9SampleRate == 12000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 16000:
                                    if (ConfigAT9SampleRate == 48000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 24000:
                                    if (ConfigAT9SampleRate == 48000 || ConfigAT9SampleRate == 24000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 32000:
                                    if (ConfigAT9SampleRate == 48000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 44100:
                                    if (ConfigAT9SampleRate == 48000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 48000:
                                    if (ConfigAT9SampleRate == 48000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                default:
                                    return true;
                            }
                        }
                        else // PSV
                        {
                            switch (sourceSampleRate)
                            {
                                case 8000:
                                    if (ConfigAT9SampleRate == 48000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 12000:
                                    if (ConfigAT9SampleRate == 48000 || ConfigAT9SampleRate == 12000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 16000:
                                    if (ConfigAT9SampleRate == 48000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 24000:
                                    if (ConfigAT9SampleRate == 48000 || ConfigAT9SampleRate == 24000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 32000:
                                    if (ConfigAT9SampleRate == 48000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 44100:
                                    if (ConfigAT9SampleRate == 48000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                case 48000:
                                    if (ConfigAT9SampleRate == 48000)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                default:
                                    return true;
                            }
                        }
                    }
                    else
                    {
                        if (Generic.ATRACFlag == 0)
                        {
                            if (Generic.IsAT3PS3) // PS3
                            {
                                if (sourceSampleRate == 48000)
                                {
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                            else // PSP
                            {
                                if (sourceSampleRate == 44100)
                                {
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                        }
                        else if (Generic.ATRACFlag == 1)
                        {
                            if (Generic.IsAT9PS4) // PS4
                            {
                                switch (sourceSampleRate)
                                {
                                    case 8000:
                                        if (ConfigAT9SampleRate == 48000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 12000:
                                        if (ConfigAT9SampleRate == 48000 || ConfigAT9SampleRate == 12000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 16000:
                                        if (ConfigAT9SampleRate == 48000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 24000:
                                        if (ConfigAT9SampleRate == 48000 || ConfigAT9SampleRate == 24000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 32000:
                                        if (ConfigAT9SampleRate == 48000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 44100:
                                        if (ConfigAT9SampleRate == 48000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 48000:
                                        if (ConfigAT9SampleRate == 48000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    default:
                                        return true;
                                }
                            }
                            else // PSV
                            {
                                switch (sourceSampleRate)
                                {
                                    case 8000:
                                        if (ConfigAT9SampleRate == 48000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 12000:
                                        if (ConfigAT9SampleRate == 48000 || ConfigAT9SampleRate == 12000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 16000:
                                        if (ConfigAT9SampleRate == 48000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 24000:
                                        if (ConfigAT9SampleRate == 48000 || ConfigAT9SampleRate == 24000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 32000:
                                        if (ConfigAT9SampleRate == 48000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 44100:
                                        if (ConfigAT9SampleRate == 48000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    case 48000:
                                        if (ConfigAT9SampleRate == 48000)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    default:
                                        return true;
                                }
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.CreateExceptionLog(ex, true);
                    return true;
                }

            }

            /// <summary>
            /// 設定ファイルに全てを書き出す
            /// </summary>
            public static void InitConfig()
            {
                if (Config.Entry["ATRAC3_Console"].Value == null) // ATRAC3 コンソール (int)
                {
                    Config.Entry["ATRAC3_Console"].Value = "0";
                }
                if (Config.Entry["ATRAC9_Console"].Value == null) // ATRAC9 コンソール (int)
                {
                    Config.Entry["ATRAC9_Console"].Value = "0";
                }
                if (Config.Entry["ATRAC3_Bitrate"].Value == null) // ATRAC3 ビットレート (int)
                {
                    Config.Entry["ATRAC3_Bitrate"].Value = "7";
                }
                if (Config.Entry["ATRAC9_Bitrate"].Value == null) // ATRAC9 ビットレート (int)
                {
                    Config.Entry["ATRAC9_Bitrate"].Value = "7";
                }
                if (Config.Entry["ATRAC9_Sampling"].Value == null) // ATRAC9 サンプリング周波数 (int)
                {
                    Config.Entry["ATRAC9_Sampling"].Value = "2";
                }
                if (Config.Entry["ATRAC9_SamplingValue"].Value == null) // ATRAC9 サンプリング周波数値 (int)
                {
                    Config.Entry["ATRAC9_SamplingValue"].Value = "48000";
                }
                if (Config.Entry["ATRAC3_LoopSound"].Value == null)　// ATRAC3 ループ (bool)
                {
                    Config.Entry["ATRAC3_LoopSound"].Value = "false";
                }
                if (Config.Entry["ATRAC3_LoopPoint"].Value == null) // ATRAC3 ループポイント (bool)
                {
                    Config.Entry["ATRAC3_LoopPoint"].Value = "false";
                }
                if (Config.Entry["ATRAC3_LoopStart_Samples"].Value == null) // ATRAC3 ループスタート (string)
                {
                    Config.Entry["ATRAC3_LoopStart_Samples"].Value = "";
                }
                if (Config.Entry["ATRAC3_LoopEnd_Samples"].Value == null) // ATRAC3 ループエンド (string)
                {
                    Config.Entry["ATRAC3_LoopEnd_Samples"].Value = "";
                }
                if (Config.Entry["ATRAC3_LoopTime"].Value == null) // ATRAC3 ループ回数指定 (bool)
                {
                    Config.Entry["ATRAC3_LoopTime"].Value = "false";
                }
                if (Config.Entry["ATRAC3_LoopTimes"].Value == null) // ATRAC3 ループ回数 (string)
                {
                    Config.Entry["ATRAC3_LoopTimes"].Value = "";
                }
                if (Config.Entry["ATRAC9_LoopSound"].Value == null) // ATRAC9 ループ (bool)
                {
                    Config.Entry["ATRAC9_LoopSound"].Value = "false";
                }
                if (Config.Entry["ATRAC9_LoopPoint"].Value == null)  // ATRAC9 ループポイント (bool)
                {
                    Config.Entry["ATRAC9_LoopPoint"].Value = "false";
                }
                if (Config.Entry["ATRAC9_LoopStart_Samples"].Value == null) // ATRAC9 ループスタート (string)
                {
                    Config.Entry["ATRAC9_LoopStart_Samples"].Value = "";
                }
                if (Config.Entry["ATRAC9_LoopEnd_Samples"].Value == null) // ATRAC9 ループエンド (string)
                {
                    Config.Entry["ATRAC9_LoopEnd_Samples"].Value = "";
                }
                if (Config.Entry["ATRAC9_LoopTime"].Value == null) // ATRAC9 ループ回数指定 (bool)
                {
                    Config.Entry["ATRAC9_LoopTime"].Value = "false";
                }
                if (Config.Entry["ATRAC9_LoopTimes"].Value == null) // ATRAC9 ループ回数 (string)
                {
                    Config.Entry["ATRAC9_LoopTimes"].Value = "";
                }
                if (Config.Entry["ATRAC9_LoopList"].Value == null) // ATRAC9 ループリスト使用 (bool)
                {
                    Config.Entry["ATRAC9_LoopList"].Value = "false";
                }
                if (Config.Entry["ATRAC9_LoopListFile"].Value == null) // ATRAC9 ループリストファイル (string)
                {
                    Config.Entry["ATRAC9_LoopListFile"].Value = "";
                }
                if (Config.Entry["ATRAC9_Advanced"].Value == null) // ATRAC9 高度な設定 (bool)
                {
                    Config.Entry["ATRAC9_Advanced"].Value = "false";
                }
                if (Config.Entry["ATRAC9_EncodeType"].Value == null) // ATARC9 エンコード方式指定 (bool)
                {
                    Config.Entry["ATRAC9_EncodeType"].Value = "false";
                }
                if (Config.Entry["ATRAC9_EncodeTypeIndex"].Value == null) // ATARC9 エンコード方式 (int)
                {
                    Config.Entry["ATRAC9_EncodeTypeIndex"].Value = "";
                }
                if (Config.Entry["ATRAC9_AdvancedBand"].Value == null) // ATRAC9 高度なバンド (bool)
                {
                    Config.Entry["ATRAC9_AdvancedBand"].Value = "false";
                }
                if (Config.Entry["ATRAC9_NbandsIndex"].Value == null) // ATRAC9 NBand (int)
                {
                    Config.Entry["ATRAC9_NbandsIndex"].Value = "";
                }
                if (Config.Entry["ATRAC9_IsbandIndex"].Value == null) // ATRAC9 Isband (int)
                {
                    Config.Entry["ATRAC9_IsbandIndex"].Value = "";
                }
                if (Config.Entry["ATRAC9_DualEncode"].Value == null) // ATRAC9 デュアルエンコードモード (bool)
                {
                    Config.Entry["ATRAC9_DualEncode"].Value = "false";
                }
                if (Config.Entry["ATRAC9_SuperFrameEncode"].Value == null) // ATRAC9 スーパーフレームエンコード (bool)
                {
                    Config.Entry["ATRAC9_SuperFrameEncode"].Value = "false";
                }
                if (Config.Entry["ATRAC9_WideBand"].Value == null) // ATRAC9 WideBand (bool)
                {
                    Config.Entry["ATRAC9_WideBand"].Value = "false";
                }
                if (Config.Entry["ATRAC9_BandExtension"].Value == null) // ATRAC9 BandExtension (bool)
                {
                    Config.Entry["ATRAC9_BandExtension"].Value = "false";
                }
                if (Config.Entry["ATRAC9_LFE_SuperLowCut"].Value == null) // ATRAC9 LFE (bool)
                {
                    Config.Entry["ATRAC9_LFE_SuperLowCut"].Value = "false";
                }
                if (Config.Entry["LPC_Create"].Value == null) // ループポイントクリエーター (bool)
                {
                    Config.Entry["LPC_Create"].Value = "false";
                }
                if (Config.Entry["ATRAC3_Params"].Value == null) // ATRAC3 引数 (string)
                {
                    Config.Entry["ATRAC3_Params"].Value = "at3tool -e -br 128 $InFile $OutFile";
                }
                if (Config.Entry["ATRAC9_Params"].Value == null) // ATRAC9 引数 (string)
                {
                    Config.Entry["ATRAC9_Params"].Value = "at9tool -e -br 168 -fs 48000 $InFile $OutFile";
                }
                if (Config.Entry["Walkman_Params"].Value == null) // Walkman 引数 (string)
                {
                    Config.Entry["Walkman_Params"].Value = "traconv --Convert --FileType OMA3 --BitRate -1 --Output $OutFile $InFile";
                }
                if (Config.Entry["Walkman_EveryFmt"].Value == null) // Walkman 常に形式固定
                {
                    Config.Entry["Walkman_EveryFmt"].Value = "false";
                }
                if (Config.Entry["Walkman_Unattended"].Value == null) // Walkman 無人（確認ダイアログを出さない）
                {
                    Config.Entry["Walkman_Unattended"].Value = "false";
                }
                if (Config.Entry["Walkman_EveryFmt_OutputFmt"].Value == null) // Walkman 出力フォーマット
                {
                    Config.Entry["Walkman_EveryFmt_OutputFmt"].Value = "1";
                }
                if (Config.Entry["Walkman_EveryFmt_DecodeFmt"].Value == null) // Walkman デコードフォーマット
                {
                    Config.Entry["Walkman_EveryFmt_DecodeFmt"].Value = "0";
                }
                if (Config.Entry["Walkman_FixSongInformation"].Value == null) // Walkman 楽曲情報固定
                {
                    Config.Entry["Walkman_FixSongInformation"].Value = "false";
                }
                if (Config.Entry["Walkman_FileType"].Value == null) // Walkman ファイル形式
                {
                    Config.Entry["Walkman_FileType"].Value = "OMA3";
                }
                if (Config.Entry["Walkman_Bitrate"].Value == null) // Walkman ビットレート
                {
                    Config.Entry["Walkman_Bitrate"].Value = "";
                }
                if (Config.Entry["Walkman_Title"].Value == null) // Walkman タイトル
                {
                    Config.Entry["Walkman_Title"].Value = "";
                }
                if (Config.Entry["Walkman_SortTitle"].Value == null) // Walkman SortTitle
                {
                    Config.Entry["Walkman_SortTitle"].Value = "";
                }
                if (Config.Entry["Walkman_SubTitle"].Value == null) // Walkman SubTitle
                {
                    Config.Entry["Walkman_SubTitle"].Value = "";
                }
                if (Config.Entry["Walkman_SortSubTitle"].Value == null) // Walkman SortSubTitle
                {
                    Config.Entry["Walkman_SortSubTitle"].Value = "";
                }
                if (Config.Entry["Walkman_Artist"].Value == null) // Walkman Artist
                {
                    Config.Entry["Walkman_Artist"].Value = "";
                }
                if (Config.Entry["Walkman_SortArtist"].Value == null) // Walkman SortArtist
                {
                    Config.Entry["Walkman_SortArtist"].Value = "";
                }
                if (Config.Entry["Walkman_ArtistURL"].Value == null) // Walkman ArtistURL
                {
                    Config.Entry["Walkman_ArtistURL"].Value = "";
                }
                if (Config.Entry["Walkman_Album"].Value == null) // Walkman Album
                {
                    Config.Entry["Walkman_Album"].Value = "";
                }
                if (Config.Entry["Walkman_SortAlbum"].Value == null) // Walkman SortAlbum
                {
                    Config.Entry["Walkman_SortAlbum"].Value = "";
                }
                if (Config.Entry["Walkman_AlbumArtist"].Value == null) // Walkman AlbumArtist
                {
                    Config.Entry["Walkman_AlbumArtist"].Value = "";
                }
                if (Config.Entry["Walkman_SortAlbumArtist"].Value == null) // Walkman SortAlbumArtist
                {
                    Config.Entry["Walkman_SortAlbumArtist"].Value = "";
                }
                if (Config.Entry["Walkman_Genre"].Value == null) // Walkman Genre
                {
                    Config.Entry["Walkman_Genre"].Value = "";
                }
                if (Config.Entry["Walkman_Composer"].Value == null) // Walkman Composer
                {
                    Config.Entry["Walkman_Composer"].Value = "";
                }
                if (Config.Entry["Walkman_Lyricist"].Value == null) // Walkman Lyricist
                {
                    Config.Entry["Walkman_Lyricist"].Value = "";
                }
                if (Config.Entry["Walkman_TrackNumber"].Value == null) // Walkman TrackNumber
                {
                    Config.Entry["Walkman_TrackNumber"].Value = "";
                }
                if (Config.Entry["Walkman_TotalTracks"].Value == null) // Walkman TotalTracks
                {
                    Config.Entry["Walkman_TotalTracks"].Value = "";
                }
                if (Config.Entry["Walkman_Release"].Value == null) // Walkman Release
                {
                    Config.Entry["Walkman_Release"].Value = "";
                }
                if (Config.Entry["Walkman_Import"].Value == null) // Walkman Import
                {
                    Config.Entry["Walkman_Import"].Value = "";
                }
                if (Config.Entry["Walkman_Duration"].Value == null) // Walkman Duration
                {
                    Config.Entry["Walkman_Duration"].Value = "";
                }
                if (Config.Entry["Walkman_MilliSecond"].Value == null) // Walkman MilliSecond
                {
                    Config.Entry["Walkman_MilliSecond"].Value = "";
                }
                if (Config.Entry["Walkman_Lyrics"].Value == null) // Walkman Lyrics
                {
                    Config.Entry["Walkman_Lyrics"].Value = "";
                }
                if (Config.Entry["Walkman_LyricsMode"].Value == null) // Walkman LyricsMode
                {
                    Config.Entry["Walkman_LyricsMode"].Value = "3";
                }
                if (Config.Entry["Walkman_LinerNotes"].Value == null) // Walkman LinerNotes
                {
                    Config.Entry["Walkman_LinerNotes"].Value = "";
                }
                if (Config.Entry["Walkman_LinerNotesMode"].Value == null) // Walkman LinerNotesMode
                {
                    Config.Entry["Walkman_LinerNotesMode"].Value = "3";
                }
                if (Config.Entry["Walkman_Jacket"].Value == null) // Walkman Jacket
                {
                    Config.Entry["Walkman_Jacket"].Value = "";
                }
                if (Config.Entry["Walkman_JacketMode"].Value == null) // Walkman JacketMode
                {
                    Config.Entry["Walkman_JacketMode"].Value = "3";
                }

                // 設定ダイアログ

                // General 項目
                if (Config.Entry["Check_Update"].Value == null) // アップデートを確認 (bool)
                {
                    Config.Entry["Check_Update"].Value = "true";
                }
                if (Config.Entry["HideSplash"].Value == null) // スプラッシュスクリーンを無効化 (bool)
                {
                    Config.Entry["HideSplash"].Value = "false";
                }
                if (Config.Entry["DisablePreviewWarning"].Value == null) // 警告メッセージ無効化 (bool)
                {
                    Config.Entry["DisablePreviewWarning"].Value = "false";
                }
                if (Config.Entry["ATRACEncodeSource"].Value == null) // ATRACをエンコード用ソースとして読み込み (bool)
                {
                    Config.Entry["ATRACEncodeSource"].Value = "false";
                }
                if (Config.Entry["SplashImage"].Value == null) // スプラッシュスクリーン画像 (bool)
                {
                    Config.Entry["SplashImage"].Value = "false";
                }
                if (Config.Entry["SplashImage_Path"].Value == null) // スプラッシュスクリーン画像パス (string)
                {
                    Config.Entry["SplashImage_Path"].Value = "";
                }

                // IO 項目
                if (Config.Entry["Save_IsManual"].Value == null) // ファイル保存方法 (bool)
                {
                    Config.Entry["Save_IsManual"].Value = "false";
                }
                if (Config.Entry["Save_Isfolder"].Value == null) // ファイル保存フォルダ (string)
                {
                    Config.Entry["Save_Isfolder"].Value = "";
                }
                if (Config.Entry["Save_IsSubfolder"].Value == null) // サブフォルダ作成 (bool)
                {
                    Config.Entry["Save_IsSubfolder"].Value = "false";
                }
                if (Config.Entry["Save_Subfolder_Suffix"].Value == null) // サブフォルダ接尾辞 (string)
                {
                    Config.Entry["Save_Subfolder_Suffix"].Value = "";
                }
                if (Config.Entry["Save_NestFolderSource"].Value == null) // ネスト方式のフォルダを読み込み、保存時にソースと同じネスト方式で保存 (bool)
                {
                    Config.Entry["Save_NestFolderSource"].Value = "false";
                }
                if (Config.Entry["Save_DeleteHzSuffix"].Value == null) // ファイル保存時、ファイル名の最後に追加されるHz表記を無効にする (bool)
                {
                    Config.Entry["Save_DeleteHzSuffix"].Value = "false";
                }
                if (Config.Entry["ShowFolder"].Value == null) // 変換後にフォルダを表示 (bool)
                {
                    Config.Entry["ShowFolder"].Value = "true";
                }

                // LPC 項目
                if (Config.Entry["LPCPlaybackMethod"].Value == null) // LPC 再生方法 (uint)
                {
                    Config.Entry["LPCPlaybackMethod"].Value = "0";
                }
                if (Config.Entry["LPCUseASIODriver"].Value == null) // LPC 使用するASIOドライバ (string)
                {
                    Config.Entry["LPCUseASIODriver"].Value = "";
                }
                if (Config.Entry["LPCMultipleStreamAlwaysWASAPIorASIO"].Value == null) // LPC マルチオーディオ再生時、常にWASAPIまたはASIO使用 (bool)
                {
                    Config.Entry["LPCMultipleStreamAlwaysWASAPIorASIO"].Value = "true";
                }
                if (Config.Entry["LPCMultipleStreamPlaybackMethod"].Value == null) // LPC マルチオーディオファイル再生方法 (uint)
                {
                    Config.Entry["LPCMultipleStreamPlaybackMethod"].Value = "0";
                }
                if (Config.Entry["SmoothSamples"].Value == null) // サンプル値の更新を滑らかにする (bool)
                {
                    Config.Entry["SmoothSamples"].Value = "true";
                }
                if (Config.Entry["PlaybackATRAC"].Value == null) // ATRAC読み込み時の再生インターフェースの有効 (bool)
                {
                    Config.Entry["PlaybackATRAC"].Value = "true";
                }

                // Advanced 項目
                if (Config.Entry["FasterATRAC"].Value == null) // ATRAC即時変換 (bool)
                {
                    Config.Entry["FasterATRAC"].Value = "false";
                }
                if (Config.Entry["FixedConvert"].Value == null) // 形式固定 (bool)
                {
                    Config.Entry["FixedConvert"].Value = "false";
                }
                if (Config.Entry["ConvertType"].Value == null) // 形式固定有効化時の形式 (int)
                {
                    Config.Entry["ConvertType"].Value = "";
                }
                if (Config.Entry["ForceConvertWaveOnly"].Value == null) // waveファイルのみの読み込みでも変換を強制する (bool)
                {
                    Config.Entry["ForceConvertWaveOnly"].Value = "false";
                }

                if (Config.Entry["DirectSoundBuffers"].Value == null) // DirectSound使用時のバッファ (uint)
                {
                    Config.Entry["DirectSoundBuffers"].Value = "1";
                }
                if (Config.Entry["DirectSoundBuffersValue"].Value == null) // DirectSound使用時のバッファ値 (uint)
                {
                    Config.Entry["DirectSoundBuffersValue"].Value = "16";
                }
                if (Config.Entry["DirectSoundLatency"].Value == null) // DirectSound使用時のレイテンシ (uint)
                {
                    Config.Entry["DirectSoundLatency"].Value = "2";
                }
                if (Config.Entry["DirectSoundLatencyValue"].Value == null) // DirectSound使用時のレイテンシ値 (uint)
                {
                    Config.Entry["DirectSoundLatencyValue"].Value = "200";
                }
                if (Config.Entry["WASAPILatencyShared"].Value == null) // WASAPI(共有)使用時のレイテンシ (uint)
                {
                    Config.Entry["WASAPILatencyShared"].Value = "0";
                }
                if (Config.Entry["WASAPILatencySharedValue"].Value == null) // WASAPI(共有)使用時のレイテンシ値 (uint)
                {
                    Config.Entry["WASAPILatencySharedValue"].Value = "0";
                }
                if (Config.Entry["WASAPILatencyExclusived"].Value == null) // WASAPI(排他)使用時のレイテンシ (uint)
                {
                    Config.Entry["WASAPILatencyExclusived"].Value = "0";
                }
                if (Config.Entry["WASAPILatencyExclusivedValue"].Value == null) // WASAPI(排他)使用時のレイテンシ値 (uint)
                {
                    Config.Entry["WASAPILatencyExclusivedValue"].Value = "0";
                }
                if (Config.Entry["UseParallelMethod"].Value == null) // 再生にParallel.ForEachを使用する (bool)
                {
                    Config.Entry["UseParallelMethod"].Value = "false";
                }
                if (Config.Entry["PlaybackThreadCount"].Value == null) // 再生時に使用するスレッド数 (uint)
                {
                    Config.Entry["PlaybackThreadCount"].Value = "3";
                }

                if (Config.Entry["Oldmode"].Value == null) // 従来のモード (bool)
                {
                    Config.Entry["Oldmode"].Value = "false";
                }
                if (Config.Entry["Debugmode"].Value == null) // デバッグモード (bool)
                {
                    Config.Entry["Debugmode"].Value = "false";
                }

                // その他
                if (Config.Entry["ToolStrip"].Value == null) // メイン画面ToolStrip (int)
                {
                    Config.Entry["ToolStrip"].Value = "0";
                }

                Config.Save(xmlpath);
            }

            /// <summary>
            /// Config から bool を安全に取得する。キーがなかったり値が壊れていたら defaultValue。
            /// </summary>
            public static bool GetBool(string key, bool defaultValue = false)
            {
                try
                {
                    if (!Config.Entry.Exists(key))
                        return defaultValue;

                    var raw = Config.Entry[key].Value;
                    if (string.IsNullOrWhiteSpace(raw))
                        return defaultValue;

                    return bool.TryParse(raw, out var v) ? v : defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }

            /// <summary>
            /// 階層キー版 (Config.Entry["Group","Key"] 用)
            /// </summary>
            public static bool GetBool(string[] keys, bool defaultValue = false)
            {
                try
                {
                    if (!Config.Entry.Exists(keys))
                        return defaultValue;

                    var raw = Config.Entry[keys].Value;
                    if (string.IsNullOrWhiteSpace(raw))
                        return defaultValue;

                    return bool.TryParse(raw, out var v) ? v : defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }

            /// <summary>
            /// Config から int を安全に取得する。
            /// </summary>
            public static int GetInt(string key, int defaultValue = 0)
            {
                try
                {
                    if (!Config.Entry.Exists(key))
                        return defaultValue;

                    var raw = Config.Entry[key].Value;
                    if (string.IsNullOrWhiteSpace(raw))
                        return defaultValue;

                    return int.TryParse(raw, out var v) ? v : defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }

            public static int GetInt(string[] keys, int defaultValue = 0)
            {
                try
                {
                    if (!Config.Entry.Exists(keys))
                        return defaultValue;

                    var raw = Config.Entry[keys].Value;
                    if (string.IsNullOrWhiteSpace(raw))
                        return defaultValue;

                    return int.TryParse(raw, out var v) ? v : defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }

            /// <summary>
            /// Config から string を安全に取得する。空や null のときは defaultValue。
            /// </summary>
            public static string GetString(string key, string defaultValue = "")
            {
                try
                {
                    if (!Config.Entry.Exists(key))
                        return defaultValue;

                    var raw = Config.Entry[key].Value;
                    return string.IsNullOrWhiteSpace(raw) ? defaultValue : raw;
                }
                catch
                {
                    return defaultValue;
                }
            }

            public static string GetString(string[] keys, string defaultValue = "")
            {
                try
                {
                    if (!Config.Entry.Exists(keys))
                        return defaultValue;

                    var raw = Config.Entry[keys].Value;
                    return string.IsNullOrWhiteSpace(raw) ? defaultValue : raw;
                }
                catch
                {
                    return defaultValue;
                }
            }
            private static string Q(string s)
            {
                // traconv へ渡す用：ダブルクォートはエスケープ
                return "\"" + (s ?? "").Replace("\"", "\\\"") + "\"";
            }

            public static string QuoteArg(string value)
            {
                // traconv の引数として安全にクォート
                // " を \" にする（ProcessStartInfo.Arguments 前提の定番）
                value ??= "";
                value = value.Replace("\"", "\\\"");
                return $"\"{value}\"";
            }

            private static string BuildStrOpt(string key, string? value)
            {
                if (string.IsNullOrWhiteSpace(value)) return "";
                return " " + key + " " + Q(value.Trim());
            }

            public static string BuildStrOptQ(string optName, string? value)
            {
                value = value?.Trim() ?? "";
                if (value.Length == 0) return "";
                return $" {optName} {QuoteArg(value)}";
            }

            public static string BuildNumOpt(string optName, string? value, int min = int.MinValue, int max = int.MaxValue)
            {
                value = value?.Trim() ?? "";
                if (value.Length == 0) return "";

                if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n))
                    return ""; // 数値でないなら付与しない（ここは要件次第でエラー扱いも可）

                if (n < min || n > max)
                    return ""; // 範囲外は付与しない

                return $" {optName} \"{n}\"";
            }

            public sealed class SimpleTags
            {
                public string? Title;
                public string? Artist;
                public string? Album;
                public string? Genre;
                public string? TrackNumber;
                public string? TotalTracks;
            }

            /// <summary>
            /// ID3からタグ情報を取得
            /// </summary>
            /// <param name="path"></param>
            /// <param name="tags"></param>
            /// <returns></returns>
            public static bool TryReadId3v2Mp3(string path, out SimpleTags tags)
            {
                tags = new SimpleTags();

                try
                {
                    if (!File.Exists(path)) return false;
                    if (!path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)) return false;

                    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var br = new BinaryReader(fs);

                    // ID3 header: "ID3" + ver(2) + flags(1) + size(4 synchsafe)
                    var head = br.ReadBytes(10);
                    if (head.Length < 10) return false;
                    if (head[0] != (byte)'I' || head[1] != (byte)'D' || head[2] != (byte)'3') return false;

                    int tagSize =
                        ((head[6] & 0x7F) << 21) |
                        ((head[7] & 0x7F) << 14) |
                        ((head[8] & 0x7F) << 7) |
                        (head[9] & 0x7F);

                    if (tagSize <= 0) return false;

                    long tagEnd = 10L + tagSize;
                    while (fs.Position + 10 <= tagEnd)
                    {
                        byte[] frameHeader = br.ReadBytes(10);
                        if (frameHeader.Length < 10) break;

                        string id = Encoding.ASCII.GetString(frameHeader, 0, 4);
                        int size = (frameHeader[4] << 24) | (frameHeader[5] << 16) | (frameHeader[6] << 8) | frameHeader[7];
                        if (size <= 0 || fs.Position + size > tagEnd) { fs.Position = tagEnd; break; }

                        byte[] payload = br.ReadBytes(size);
                        if (payload.Length < 2) continue;

                        // テキストフレームは先頭1バイトが encoding
                        byte enc = payload[0];
                        string text = DecodeId3Text(enc, payload.AsSpan(1)).Trim('\0', ' ', '\r', '\n', '\t');

                        switch (id)
                        {
                            case "TIT2": tags.Title = text; break;
                            case "TPE1": tags.Artist = text; break;
                            case "TALB": tags.Album = text; break;
                            case "TCON": tags.Genre = text; break;
                            case "TRCK":
                                // "3/12" 形式あり
                                var parts = text.Split('/');
                                tags.TrackNumber = parts.Length >= 1 ? parts[0] : text;
                                tags.TotalTracks = parts.Length >= 2 ? parts[1] : tags.TotalTracks;
                                break;
                        }
                    }

                    return !(string.IsNullOrWhiteSpace(tags.Title) &&
                             string.IsNullOrWhiteSpace(tags.Artist) &&
                             string.IsNullOrWhiteSpace(tags.Album) &&
                             string.IsNullOrWhiteSpace(tags.Genre) &&
                             string.IsNullOrWhiteSpace(tags.TrackNumber));
                }
                catch
                {
                    return false;
                }
            }

            private static string DecodeId3Text(byte encoding, ReadOnlySpan<byte> data)
            {
                // 0: ISO-8859-1, 1: UTF-16 with BOM, 2: UTF-16BE, 3: UTF-8
                return encoding switch
                {
                    0 => Encoding.GetEncoding("ISO-8859-1").GetString(data),
                    1 => Encoding.Unicode.GetString(data),
                    2 => Encoding.BigEndianUnicode.GetString(data),
                    3 => Encoding.UTF8.GetString(data),
                    _ => Encoding.UTF8.GetString(data),
                };
            }

            public static bool TryReadTagsAny(string path, out SimpleTags tags)
            {
                tags = new SimpleTags();

                try
                {
                    if (!File.Exists(path)) return false;

                    using var f = TagLib.File.Create(path);
                    var t = f.Tag;

                    tags.Title = string.IsNullOrWhiteSpace(t.Title) ? null : t.Title;
                    tags.Artist = (t.Performers?.Length ?? 0) > 0 ? t.Performers[0] : null;
                    tags.Album = string.IsNullOrWhiteSpace(t.Album) ? null : t.Album;
                    tags.Genre = (t.Genres?.Length ?? 0) > 0 ? t.Genres[0] : null;

                    if (t.Track > 0) tags.TrackNumber = t.Track.ToString();
                    if (t.TrackCount > 0) tags.TotalTracks = t.TrackCount.ToString();
                    // 年（Release）は TagLib 側では Year
                    // 必要なら tags.Release = t.Year > 0 ? t.Year.ToString() : null;

                    return !(string.IsNullOrWhiteSpace(tags.Title) &&
                             string.IsNullOrWhiteSpace(tags.Artist) &&
                             string.IsNullOrWhiteSpace(tags.Album) &&
                             string.IsNullOrWhiteSpace(tags.Genre) &&
                             string.IsNullOrWhiteSpace(tags.TrackNumber));
                }
                catch
                {
                    return false;
                }
            }

            public static void PopulateWalkmanMetaFromOrigin(InputJob job)
            {
                try
                {
                    using var tfile = TagLib.File.Create(job.OriginPath);

                    job.Meta.Title = tfile.Tag.Title ?? "";
                    job.Meta.Artist = (tfile.Tag.Performers?.Length > 0) ? tfile.Tag.Performers[0] : "";
                    job.Meta.Album = tfile.Tag.Album ?? "";
                    job.Meta.Genre = (tfile.Tag.Genres?.Length > 0) ? tfile.Tag.Genres[0] : "";
                    job.Meta.TrackNumber = (tfile.Tag.Track != 0) ? tfile.Tag.Track.ToString() : "";
                    job.Meta.ReleaseYear = (tfile.Tag.Year != 0) ? tfile.Tag.Year.ToString() : "";

                    // ★ジャケット抽出（既に手動指定されているなら上書きしない方が安全）
                    if (string.IsNullOrWhiteSpace(job.Meta.JacketPath))
                    {
                        string tempJacketDir = Path.Combine(Directory.GetCurrentDirectory(), "_temp", "jacket");
                        string? jacket = WalkmanJacketExtractor.TryExtractEmbeddedJacketToTemp(job.OriginPath, tempJacketDir, job.Index);
                        if (!string.IsNullOrWhiteSpace(jacket))
                        {
                            job.Meta.JacketPath = jacket;
                            if (string.IsNullOrWhiteSpace(job.Meta.JacketMode))
                                job.Meta.JacketMode = "Picture"; // or "Auto"（あなたのtraconv仕様に合わせて）
                        }
                    }
                }
                catch
                {
                    // タグ取得不可は空のまま（WAV/未知形式/破損など）
                }
            }

            public static string BuildTraConvMetaArgs(Common.WalkmanMeta meta)
            {
                var sb = new StringBuilder();

                sb.Append(BuildStrOptQ("--Title", meta.Title));
                sb.Append(BuildStrOptQ("--Artist", meta.Artist));
                sb.Append(BuildStrOptQ("--Album", meta.Album));
                sb.Append(BuildStrOptQ("--Genre", meta.Genre));

                // TrackNumber は 1～9999 程度に制限（必要なら変更）
                sb.Append(BuildNumOpt("--TrackNumber", meta.TrackNumber, 1, 9999));

                // ReleaseYear は年（1～9999）
                sb.Append(BuildNumOpt("--Release", meta.ReleaseYear, 1, 9999));

                return sb.ToString();
            }

            public static string BuildTraConvArgsForJob(InputJob job, string inFile, string outFile)
            {
                if (job == null) throw new ArgumentNullException(nameof(job));
                if (job.Meta == null) throw new InvalidOperationException("job.Meta is null.");
                if (string.IsNullOrWhiteSpace(inFile)) throw new ArgumentException("inFile is empty.", nameof(inFile));
                if (string.IsNullOrWhiteSpace(outFile)) throw new ArgumentException("outFile is empty.", nameof(outFile));

                var meta = job.Meta;

                // ★FileType 必須（未指定＝PCM16化＆タグ無しになり得る）
                string fileType = meta.FileType?.Trim() ?? "";
                if (fileType.Length == 0)
                {
                    // 既存の設定キー名に合わせてください（例）
                    fileType = Utils.GetString("Walkman_FileType", "").Trim();
                }
                if (fileType.Length == 0)
                {
                    throw new InvalidOperationException("Walkman FileType is required. Set job.Meta.FileType (or config Walkman_FileType).");
                }

                // Bitrate は既存運用（未指定なら -1）
                string bitrate = Utils.GetString("Walkman_Bitrate", "-1").Trim();
                if (bitrate.Length == 0) bitrate = "-1";

                // Import は任意（未指定なら今日）
                string import = Utils.GetString("Walkman_Import", DateTime.Now.ToString("yyyy/MM/dd")).Trim();

                var sb = new StringBuilder();
                sb.Append("--Convert");

                sb.Append(BuildStrOpt("--FileType", fileType));
                sb.Append(" --Bitrate ").Append(bitrate);

                sb.Append(BuildStrOpt("--Title", meta.Title));
                sb.Append(BuildStrOpt("--SortTitle", meta.SortTitle));
                sb.Append(BuildStrOpt("--Artist", meta.Artist));
                sb.Append(BuildStrOpt("--SortArtist", meta.SortArtist));
                sb.Append(BuildStrOpt("--Album", meta.Album));
                sb.Append(BuildStrOpt("--SortAlbum", meta.SortAlbum));
                sb.Append(BuildStrOpt("--AlbumArtist", meta.AlbumArtist));
                sb.Append(BuildStrOpt("--SortAlbumArtist", meta.SortAlbumArtist));
                sb.Append(BuildStrOpt("--Genre", meta.Genre));
                sb.Append(BuildStrOpt("--Composer", meta.Composer));
                sb.Append(BuildStrOpt("--Lyricist", meta.Lyricist));

                // 数値化しない：空なら出さない
                sb.Append(BuildStrOpt("--TrackNumber", meta.TrackNumber));
                sb.Append(BuildStrOpt("--TotalTracks", meta.TotalTracks));

                // Release は年文字列（あなたのMeta設計通り）
                sb.Append(BuildStrOpt("--Release", meta.ReleaseYear));

                sb.Append(BuildStrOpt("--Import", import));

                // Jacket（任意）
                if (!string.IsNullOrWhiteSpace(meta.JacketPath))
                {
                    sb.Append(BuildStrOpt("--Jacket", meta.JacketPath));
                    if (!string.IsNullOrWhiteSpace(meta.JacketMode))
                        sb.Append(" --JacketMode ").Append(meta.JacketMode.Trim());
                    else
                        sb.Append(" --JacketMode Picture");
                }
                else
                {
                    // Jacket無しでも Mode は Auto を入れておきたいならここ
                    sb.Append(" --JacketMode Auto");
                }

                if (!string.IsNullOrWhiteSpace(meta.LyricsPath))
                {
                    sb.Append(BuildStrOpt("--Lyrics", meta.LyricsPath));
                    if (!string.IsNullOrWhiteSpace(meta.LyricsMode))
                        sb.Append(" --LyricsMode ").Append(meta.LyricsMode.Trim());
                    else
                        sb.Append(" --LyricsMode Auto");
                }
                else
                {
                    sb.Append(" --LyricsMode Auto");
                }

                if (!string.IsNullOrWhiteSpace(meta.LinerNotesPath))
                {
                    sb.Append(BuildStrOpt("--LinerNotes", meta.LinerNotesPath));
                    if (!string.IsNullOrWhiteSpace(meta.LinerNotesMode))
                        sb.Append(" --LinerNotesMode ").Append(meta.LinerNotesMode.Trim());
                    else
                        sb.Append(" --LinerNotesMode Auto");
                }
                else
                {
                    sb.Append(" --LinerNotesMode Auto");
                }

                // 既存踏襲（必要なら）
                //sb.Append(" --LyricsMode Auto --LinerNotesMode Auto");

                // 入出力（※RunAtracToolWithValidation に任せるなら $InFile/$OutFile でもよい）
                sb.Append(" --Output ").Append(QuoteArg(outFile)).Append(' ').Append(QuoteArg(inFile));

                return sb.ToString();
            }
            // 既に同等があるならそれを使ってOK
            private static string Quote(string s) => "\"" + (s ?? "").Replace("\"", "\\\"") + "\"";


        }

        public static class WalkmanJacketExtractor
        {
            public static string? TryExtractEmbeddedJacketToTemp(string originPath, string tempDir, int index = 0)
            {
                if (string.IsNullOrWhiteSpace(originPath) || !System.IO.File.Exists(originPath))
                    return null;

                Directory.CreateDirectory(tempDir);

                try
                {
                    // ★ 明示的に TagLib.File
                    using var tf = TagLib.File.Create(originPath);

                    var pics = tf.Tag.Pictures;
                    if (pics == null || pics.Length == 0)
                        return null;

                    var pic =
                        pics.FirstOrDefault(p => p.Type == TagLib.PictureType.FrontCover)
                        ?? pics[0];

                    if (pic?.Data == null || pic.Data.Count == 0)
                        return null;

                    string ext = MimeToExtension(pic.MimeType);
                    if (string.IsNullOrEmpty(ext))
                        ext = GuessExtensionFromHeader(pic.Data.Data) ?? ".jpg";

                    string baseName = Path.GetFileNameWithoutExtension(originPath);
                    string hash = ShortHash(originPath + "|" + index);

                    string outPath = Path.Combine(tempDir, $"{baseName}__jkt_{hash}{ext}");

                    // ★ 明示的に System.IO.File
                    //System.IO.File.WriteAllBytes(outPath, pic.Data.Data);

                    //return outPath;

                    System.IO.File.WriteAllBytes(outPath, pic.Data.Data);

                    // ★traconv 向けに正規化して返す
                    var normalized = NormalizeJacketForTraConv(outPath, maxSize: 600);
                    if (!string.IsNullOrWhiteSpace(normalized) && !string.Equals(normalized, outPath, StringComparison.OrdinalIgnoreCase))
                    {
                        // 最適化前を削除（残す必要なし）
                        try { System.IO.File.Delete(outPath); } catch { /* 無視 */ }
                        return normalized;
                    }

                    return outPath;
                }
                catch
                {
                    return null;
                }
            }

            private static string MimeToExtension(string? mime)
            {
                mime = (mime ?? "").ToLowerInvariant();
                return mime switch
                {
                    "image/jpeg" or "image/jpg" => ".jpg",
                    "image/png" => ".png",
                    "image/gif" => ".gif",
                    "image/bmp" => ".bmp",
                    "image/webp" => ".webp",
                    _ => ""
                };
            }

            private static string? GuessExtensionFromHeader(byte[] data)
            {
                if (data.Length >= 8 &&
                    data[0] == 0x89 && data[1] == 0x50 &&
                    data[2] == 0x4E && data[3] == 0x47)
                    return ".png";

                if (data.Length >= 3 &&
                    data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
                    return ".jpg";

                return null;
            }

            private static string ShortHash(string s)
            {
                using var sha1 = SHA1.Create();
                var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(s));
                return BitConverter.ToString(bytes, 0, 8).Replace("-", "").ToLowerInvariant();
            }

            public static string? NormalizeJacketForTraConv(string srcPath, int maxSize = 600)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(srcPath) || !System.IO.File.Exists(srcPath))
                        return null;

                    // traconv 安定化のため、常に JPG に寄せる（PNG透明などが必要なら PNG にしてもOK）
                    string dir = Path.GetDirectoryName(srcPath)!;
                    string name = Path.GetFileNameWithoutExtension(srcPath);
                    string outPath = Path.Combine(dir, $"{name}__traconv.jpg");

                    using var fs = new FileStream(srcPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var img = Image.FromStream(fs);

                    int w = img.Width;
                    int h = img.Height;

                    // リサイズ判定
                    double scale = 1.0;
                    if (w > maxSize || h > maxSize)
                    {
                        scale = Math.Min((double)maxSize / w, (double)maxSize / h);
                    }

                    int nw = Math.Max(1, (int)Math.Round(w * scale));
                    int nh = Math.Max(1, (int)Math.Round(h * scale));

                    using var bmp = new Bitmap(nw, nh);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.DrawImage(img, 0, 0, nw, nh);
                    }

                    // JPG（品質 90）
                    var enc = ImageCodecInfo.GetImageEncoders().First(e => e.MimeType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase));
                    var ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

                    bmp.Save(outPath, enc, ep);

                    return System.IO.File.Exists(outPath) ? outPath : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public static class WpfWindowRegistry
        {
            private static readonly HashSet<WeakReference<System.Windows.Window>> _windows = new();

            public static void Register(System.Windows.Window w)
            {
                Cleanup();
                _windows.Add(new WeakReference<System.Windows.Window>(w));
                w.Closed += (_, __) => Cleanup();
            }

            public static IReadOnlyList<System.Windows.Window> Windows
            {
                get
                {
                    Cleanup();
                    return _windows
                        .Select(r => r.TryGetTarget(out var w) ? w : null)
                        .Where(w => w != null)
                        .ToList()!;
                }
            }

            public static T? Find<T>() where T : System.Windows.Window
                => Windows.OfType<T>().FirstOrDefault();

            private static void Cleanup()
                => _windows.RemoveWhere(r => !r.TryGetTarget(out _));
        }

        /// <summary>
        /// ネットワーク系関数
        /// </summary>
        internal class Network
        {
            /// <summary>
            /// 文字列をURIに変換
            /// </summary>
            /// <param name="uri">URI文字列</param>
            /// <returns></returns>
            public static Uri GetUri(string uri)
            {
                return new Uri(uri);
            }

            public static Stream GetWebStream(HttpClient httpClient, Uri uri)
            {
                return httpClient.GetStreamAsync(uri).Result;
            }

            public static async Task<Stream> GetWebStreamAsync(HttpClient httpClient, Uri uri)
            {
                return await httpClient.GetStreamAsync(uri);
            }

            public static async Task<Image> GetWebImageAsync(HttpClient httpClient, Uri uri)
            {
                using Stream stream = await GetWebStreamAsync(httpClient, uri);
                return Image.FromStream(stream);
            }
        }

        public class Config
        {
            /// <summary>
            /// ルートエントリ
            /// </summary>
            public static ConfigEntry Entry = new() { Key = "ConfigRoot" };
            public static void Load(string filename)
            {
                if (!File.Exists(filename))
                    return;
                var xmlSerializer = new XmlSerializer(typeof(ConfigEntry));
                using var streamReader = new StreamReader(filename, Encoding.UTF8);
                using var xmlReader = XmlReader.Create(streamReader, new XmlReaderSettings() { CheckCharacters = false });
                Entry = (ConfigEntry)xmlSerializer.Deserialize(xmlReader)!; // （3）
            }
            public static void Save(string filename)
            {
                var serializer = new XmlSerializer(typeof(ConfigEntry));
                using var streamWriter = new StreamWriter(filename, false, Encoding.UTF8);
                serializer.Serialize(streamWriter, Entry);
            }
        }

        /// <summary>
        /// ConfigEntryクラス。設定の1レコード
        /// </summary>
        public class ConfigEntry
        {
            /// <summary>
            /// 設定レコードののキー
            /// </summary>
            public string Key { get; set; } = null!;
            /// <summary>
            /// 設定レコードの値
            /// </summary>
            public string Value { get; set; } = null!;
            /// <summary>
            /// 子アイテム
            /// </summary>
            public List<ConfigEntry>? Children { get; set; }
            /// <summary>
            /// キーを指定して子アイテムからConfigEntryを取得します。
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public ConfigEntry Get(string key)
            {
                var entry = Children?.FirstOrDefault(rec => rec.Key == key);
                if (entry == null)
                {
                    if (Children == null)
                        Children = new List<ConfigEntry>();
                    entry = new ConfigEntry() { Key = key };
                    Children.Add(entry);
                }
                return entry;
            }
            /// <summary>
            /// 子アイテムにConfigEntryを追加します。
            /// </summary>
            /// <param name="key">キー</param>
            /// <param name="o">設定値</param>
            public void Add(string key, string? o)
            {
                ConfigEntry? entry = Children?.FirstOrDefault(rec => rec.Key == key);
                if (entry != null)
                    entry.Value = o!;
                else
                {
                    if (Children == null)
                        Children = new List<ConfigEntry>();
                    entry = new ConfigEntry() { Key = key, Value = o! };
                    Children.Add(entry);
                }
            }
            /// <summary>
            /// 子アイテムからConfigEntryを取得します。存在しなければ新しいConfigEntryが作成されます。
            /// </summary>
            /// <param name="key">キー</param>
            /// <returns></returns>
            public ConfigEntry this[string key]
            {
                set => Add(key, null);
                get => Get(key);
            }
            /// <summary>
            /// 子アイテムからConfigEntryを取得します。存在しなければ新しいConfigEntryが作成されます。
            /// </summary>
            /// <param name="keys">キー、カンマで区切って階層指定します</param>
            /// <returns></returns>
            public ConfigEntry this[params string[] keys]
            {
                set
                {
                    ConfigEntry entry = this;
                    for (int i = 0; i < keys.Length; i++)
                    {
                        entry = entry[keys[i]];
                    }
                }
                get
                {
                    ConfigEntry entry = this;
                    for (int i = 0; i < keys.Length; i++)
                    {
                        entry = entry[keys[i]];
                    }
                    return entry;
                }
            }

            /// <summary>
            /// 指定したキーが子アイテムに存在するか調べます。再帰的調査はされません。
            /// </summary>
            /// <param name="key">キー</param>
            /// <returns>キーが存在すればTrue</returns>
            public bool Exists(string key) => Children?.Any(c => c.Key == key) ?? false;
            /// <summary>
            /// 指定したキーが子アイテムに存在するか調べます。階層をまたいだ指定をします。
            /// </summary>
            /// <param name="keys">キー、カンマで区切って階層指定します。</param>
            /// <returns>キーが存在すればTrue</returns>
            public bool Exists(params string[] keys)
            {
                ConfigEntry entry = this;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (entry.Exists(keys[i]) == false)
                        return false;
                    entry = entry[keys[i]];
                }
                return true;
            }
        }

    }
}
