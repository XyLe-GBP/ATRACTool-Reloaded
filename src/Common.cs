using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace ATRACTool_Reloaded
{
    public class Common
    {
        public static readonly string xmlpath = Directory.GetCurrentDirectory() + @"\app.config";
        public class Generic
        {
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
            /// デコードもしくはエンコードを判定するための変数
            /// </summary>
            public static int ProcessFlag = -1;
            public static int ProgressMax = 0;
            /// <summary>
            /// 複数のファイルをエンコードする際にループポイントを作成するかのフラグ
            /// </summary>
            public static bool lpcreate = false;
            public static bool lpcreatev2 = false;
            public static int files = 0;
            /// <summary>
            /// AT3もしくはAT9のどちらかを判定するための変数
            /// </summary>
            public static int ATRACFlag = -1;
            public static string ATRACExt = "";
            public static int TaskFlag = 0;
            public static bool IsWave = false;
            public static bool IsATRAC = false;
            /// <summary>
            /// 変換先の形式を判別するための変数
            /// </summary>
            public static int WTAFlag = -1;
            public static string[] OpenFilePaths = null!;
            public static string SavePath = null!;
            public static string FolderSavePath = null!;
            public static int WTAmethod = -1;
            public static string WTAFmt = null!;

            public static string DecodeParamAT3 = "at3tool -d $InFile $OutFile";
            public static string DecodeParamAT9 = "at9tool -d $InFile $OutFile";
            public static string EncodeParamAT3 = "";
            public static string EncodeParamAT9 = "";

            public static StreamReader Log = null!;

            /// <summary>
            /// ダウンロード機能用変数
            /// </summary>
            public static System.Net.WebClient downloadClient = null!;
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

            /// <summary>
            /// 指定したディレクトリ内のファイルも含めてディレクトリを削除する
            /// </summary>
            /// <param name="targetDirectoryPath">削除するディレクトリのパス</param>
            public static void DeleteDirectory(string targetDirectoryPath)
            {
                if (!Directory.Exists(targetDirectoryPath))
                {
                    return;
                }

                string[] filePaths = Directory.GetFiles(targetDirectoryPath);
                foreach (string filePath in filePaths)
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }

                string[] directoryPaths = Directory.GetDirectories(targetDirectoryPath);
                foreach (string directoryPath in directoryPaths)
                {
                    DeleteDirectory(directoryPath);
                }

                Directory.Delete(targetDirectoryPath, false);
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
                if (Config.Entry["ShowFolder"].Value == null) // 変換後にフォルダを表示 (bool)
                {
                    Config.Entry["ShowFolder"].Value = "true";
                }
                if (Config.Entry["ToolStrip"].Value == null) // ts (int)
                {
                    Config.Entry["ToolStrip"].Value = "0";
                }
                Config.Save(xmlpath);
            }
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
            public string Key { get; set; }
            /// <summary>
            /// 設定レコードの値
            /// </summary>
            public string Value { get; set; }
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
                    entry.Value = o;
                else
                {
                    if (Children == null)
                        Children = new List<ConfigEntry>();
                    entry = new ConfigEntry() { Key = key, Value = o };
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
