using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ATRACTool_Reloaded
{
    internal class Common
    {
        internal class Generic
        {
            public static CancellationTokenSource cts = null!;

            public static bool Result = false;

            /// <summary>
            /// psp at3tool path string (res\psp_at3tool.exe)
            /// </summary>
            public static string PSP_ATRAC3tool = Directory.GetCurrentDirectory() + @"\res\psp_at3tool.exe";
            /// <summary>
            /// ps3 at3tool path string (res\ps3_at3tool.exe)
            /// </summary>
            public static string PS3_ATRAC3tool = Directory.GetCurrentDirectory() + @"\res\ps3_at3tool.exe";
            /// <summary>
            /// psv at9tool path string (res\psv_at9tool.exe)
            /// </summary>
            public static string PSV_ATRAC9tool = Directory.GetCurrentDirectory() + @"\res\psv_at9tool.exe";
            /// <summary>
            /// ps4 at9tool path string (res\ps4_at9tool.exe)
            /// </summary>
            public static string PS4_ATRAC9tool = Directory.GetCurrentDirectory() + @"\res\ps4_at9tool.exe";
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
        }

        internal class Utils
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

            public static bool WriteStringForIniFile(string section, string key, string value, string file = @".\settings.ini")
            {
                IniFile ini = new(file);
                ini.WriteString(section, key, value);
                return true;
            }

            public static int GetIntForIniFile(string section, string key, int defaultvalue = 0, string file = @".\settings.ini")
            {
                IniFile ini = new(file);
                int i = ini.GetInt(section, key, defaultvalue);
                if (i != 0)
                {
                    return i;
                }
                else
                {
                    return 0;
                }
            }

            public static string GetStringForIniFile(string section, string key, string file = @".\settings.ini")
            {
                IniFile ini = new(file);
                string s = ini.GetString(section, key);
                if (s != "")
                {
                    return s;
                }
                else
                {
                    return "";
                }
            }

            public static void SetWTAFormat(int WTAFlag)
            {
                switch (WTAFlag)
                {
                    case 0: // AAC
                        Generic.WTAFmt = ".m4a";
                        break;
                    case 1: // AIFF
                        Generic.WTAFmt = ".aiff";
                        break;
                    case 2: // ALAC
                        Generic.WTAFmt = ".alac";
                        break;
                    case 3: // FLAC
                        Generic.WTAFmt = ".flac";
                        break;
                    case 4: // MP3
                        Generic.WTAFmt = ".mp3";
                        break;
                    case 5: // WAV
                        Generic.WTAFmt = ".wav";
                        break;
                    case 6: // OGG
                        Generic.WTAFmt = ".ogg";
                        break;
                    default:
                        Generic.WTAFmt = ".mp3";
                        break;
                }
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
        }

        internal class IniFile
        {
            [DllImport("kernel32.dll")]
            private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

            [DllImport("kernel32.dll")]
            private static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

            /// <summary>
            /// Ini ファイルのファイルパスを取得、設定します。
            /// </summary>
            public string FilePath { get; set; }

            /// <summary>
            /// インスタンスを初期化します。
            /// </summary>
            /// <param name="filePath">Ini ファイルのファイルパス</param>
            public IniFile(string filePath)
            {
                FilePath = filePath;
            }
            /// <summary>
            /// Ini ファイルから文字列を取得します。
            /// </summary>
            /// <param name="section">セクション名</param>
            /// <param name="key">項目名</param>
            /// <param name="defaultValue">値が取得できない場合の初期値</param>
            /// <returns></returns>
            public string GetString(string section, string key, string defaultValue = "")
            {
                var sb = new StringBuilder(1024);
                var r = GetPrivateProfileString(section, key, defaultValue, sb, (uint)sb.Capacity, FilePath);
                return sb.ToString();
            }
            /// <summary>
            /// Ini ファイルから整数を取得します。
            /// </summary>
            /// <param name="section">セクション名</param>
            /// <param name="key">項目名</param>
            /// <param name="defaultValue">値が取得できない場合の初期値</param>
            /// <returns></returns>
            public int GetInt(string section, string key, int defaultValue = 0)
            {
                return (int)GetPrivateProfileInt(section, key, defaultValue, FilePath);
            }
            /// <summary>
            /// Ini ファイルに文字列を書き込みます。
            /// </summary>
            /// <param name="section">セクション名</param>
            /// <param name="key">項目名</param>
            /// <param name="value">書き込む値</param>
            /// <returns></returns>
            public bool WriteString(string section, string key, string value)
            {
                return WritePrivateProfileString(section, key, value, FilePath);
            }
        }

        
    }
}
