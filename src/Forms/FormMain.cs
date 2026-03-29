using ATRACTool_Reloaded.Localizable;
using ATRACTool_Reloaded.Properties;
using NAudio.Gui;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormMain : Form
    {
        #region NetworkCommon
        private static readonly HttpClientHandler handler = new()
        {
            UseProxy = false,
            UseCookies = false
        };
        private static readonly HttpClient appUpdatechecker = new(handler);
        #endregion
        FormLPC? FLPC;
        //static FormSplash? fs;
        static WindowSplash? fsWPF;
        static object? lockobj;

        static WindowDebug? windowDebug;

        private volatile bool _isClosing = false;
        private int _closingCancelIssued = 0;

        private static readonly ConcurrentQueue<DebugLogEntry> _debugMsgQueue = new();
        private static readonly ManualResetEventSlim _debugReady = new(false);

        public static void DebugInfo(string message) => EnqueueLog(DebugLogLevel.Info, message);
        public static void DebugWarn(string message) => EnqueueLog(DebugLogLevel.Warn, message);
        public static void DebugError(string message) => EnqueueLog(DebugLogLevel.Error, message);
        private static TimeSpan _lastCpuTime = TimeSpan.Zero;
        private static long _lastCpuTick = 0;


        private static FormMain _formMainInstance = null!;
        public static FormMain FormMainInstance
        {
            get
            {
                return _formMainInstance;
            }
            set
            {
                _formMainInstance = value;
            }
        }

        public bool Meta
        {
            get
            {
                SetMetaDatas();
                return true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FPLabel
        {
            get
            {
                return label_Filepath.Text;
            }
            set
            {
                label_Filepath.Text = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FSLabel
        {
            get
            {
                return label_Sizetxt.Text;
            }
            set
            {
                label_Sizetxt.Text = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WindowDebug DebugWindowInst
        {
            get
            {
                if (windowDebug is not null)
                {
                    return windowDebug;
                }
                return null!;
            }
        }

        public FormMain()
        {
            InitializeComponent();
        }

        // 初期化

        /// <summary>
        /// フォームのロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                FileVersionInfo ver = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
                Text = "ATRACTool Rel";

                if (Directory.Exists(Directory.GetCurrentDirectory() + @"\_temp\"))
                {
                    Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp\");
                }
                if (Directory.Exists(Directory.GetCurrentDirectory() + @"\_tempAudio"))
                {
                    Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_tempAudio");
                }

                if (!File.Exists(Common.xmlpath))
                {
                    Common.Utils.InitConfig();
                }

                Common.Config.Load(Common.xmlpath);

                if (File.Exists(Directory.GetCurrentDirectory() + @"\updated.dat"))
                {
                    TopMost = true;
                    TopMost = false;
                }

                FormMainInstance = this;

                bool hideSplash = Utils.GetBool("HideSplash", false);
                if (!hideSplash) // スプラッシュスクリーンあり
                {
                    lockobj = new object();

                    lock (lockobj)
                    {
                        ThreadStart tds = new(StartThread);
                        Thread thread = new(tds)
                        {
                            Name = "Splash",
                            IsBackground = true
                        };
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();

                        Dmes d = new(ShowMessage);
                        Thread.Sleep(4000);
                        fsWPF?.Dispatcher.Invoke(d, "Initializing...");
                        Thread.Sleep(2000);

                        foreach (var files in Directory.GetFiles(Directory.GetCurrentDirectory() + @"\res", "*", SearchOption.AllDirectories))
                        {
                            FileInfo fi = new(files);
                            if (fsWPF != null)
                            {
                                fsWPF?.Dispatcher.Invoke(d, string.Format(Localization.SplashFormFileCaption, fi.Name));
                                Thread.Sleep(50);
                            }
                        }

                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\_temp");
                        ResetStatus();

                        Thread.Sleep(2000);
                        fsWPF?.Dispatcher.Invoke(d, Localization.SplashFormConfigCaption);
                        Thread.Sleep(2000);

                        int ts = Utils.GetInt("ToolStrip", 65535);
                        //string prm1 = Config.Entry["ATRAC3_Params"].Value, prm2 = Config.Entry["ATRAC9_Params"].Value, prm3 = Config.Entry["Walkman_Params"].Value;
                        if (ts != 65535)
                        {
                            switch (ts)
                            {
                                case 0:
                                    Common.Generic.ATRACFlag = 0;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                    aTRAC9ToolStripMenuItem.Checked = false;
                                    walkmanToolStripMenuItem.Checked = false;
                                    toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                    EncodeMethodIsATRAC(true);
                                    break;
                                case 1:
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    walkmanToolStripMenuItem.Checked = false;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    EncodeMethodIsATRAC(true);
                                    break;
                                case 2:
                                    Common.Generic.ATRACFlag = 2;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = false;
                                    walkmanToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "Walkman";
                                    EncodeMethodIsATRAC(false);
                                    break;
                            }
                        }

                        Common.Generic.EncodeParamAT3 = Utils.GetString("ATRAC3_Params", string.Empty);
                        Common.Generic.EncodeParamAT9 = Utils.GetString("ATRAC9_Params", string.Empty);
                        Common.Generic.EncodeParamWalkman = Utils.GetString("Walkman_Params", string.Empty);

                        int wOutFmt = Utils.GetInt("Walkman_EveryFmt_OutputFmt", 1);
                        switch (wOutFmt)
                        {
                            case 0:
                                Common.Generic.WalkmanEveryFilter = "PCM ATRAC (*.oma)|*.oma;";
                                break;
                            case 1:
                                Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.oma)|*.oma;";
                                break;
                            case 2:
                                Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.omg)|*.omg;";
                                break;
                            case 3:
                                Common.Generic.WalkmanEveryFilter = "ATRAC3 Advanced Lossless (*.oma)|*.oma;";
                                break;
                            case 4:
                                Common.Generic.WalkmanEveryFilter = "ATRAC3 Video Clip (*.kdr)|*.kdr;";
                                break;
                            case 5:
                                Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3+ (*.oma)|*.oma;";
                                break;
                            case 6:
                                Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3+ (*.omg)|*.omg;";
                                break;
                            case 7:
                                Common.Generic.WalkmanEveryFilter = "ATRAC3+ Advanced Lossless (*.oma)|*.oma;";
                                break;
                            case 8:
                                Common.Generic.WalkmanEveryFilter = "ATRAC3+ Video Clip (*.kdr)|*.kdr;";
                                break;
                        }

                        Generic.lpcreate = Utils.GetBool("LPC_Create", false);

                        int at3Console = Utils.GetInt("ATRAC3_Console", 0);
                        Generic.IsAT3PS3 = (at3Console == 1);

                        int at9Console = Utils.GetInt("ATRAC9_Console", 0);
                        Generic.IsAT9PS4 = (at9Console == 1);

                        Generic.IsAT3LoopPoint = Utils.GetBool("ATRAC3_LoopPoint", false);
                        Generic.IsAT3LoopSound = Utils.GetBool("ATRAC3_LoopSound", false);

                        Generic.IsAT9LoopPoint = Utils.GetBool("ATRAC9_LoopPoint", false);
                        Generic.IsAT9LoopSound = Utils.GetBool("ATRAC9_LoopSound", false);

                        loopPointCreationToolStripMenuItem.Enabled = false;
                        Thread.Sleep(1000);

                        bool Legacymode = Utils.GetBool("Oldmode", false);
                        if (Legacymode)
                        {
                            fsWPF?.Dispatcher.Invoke(d, "Legacy mode is activated");
                            Thread.Sleep(500);
                            loopPointCreationToolStripMenuItem.Enabled = true;
                        }
                        else
                        {
                            loopPointCreationToolStripMenuItem.Enabled = false;
                        }

                        bool Debugmode = Utils.GetBool("Debugmode", false);
                        if (Debugmode)
                        {
                            fsWPF?.Dispatcher.Invoke(d, "Debug mode is activated");
                            Thread.Sleep(500);
                        }

                        try
                        {
                            bool chkUpdate = Utils.GetBool("Check_Update", true);
                            if (chkUpdate)
                            {
                                fsWPF?.Dispatcher.Invoke(d, Localization.SplashFormUpdateCaption);
                                Thread.Sleep(500);
                                if (File.Exists(Directory.GetCurrentDirectory() + @"\updated.dat"))
                                {
                                    fsWPF?.Dispatcher.Invoke(d, Localization.SplashFormUpdatingCaption);
                                    File.Delete(Directory.GetCurrentDirectory() + @"\updated.dat");
                                    string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];
                                    DirectoryInfo di = new(updpath + @"\updater-temp");
                                    Common.Utils.RemoveReadonlyAttribute(di);
                                    File.Delete(updpath + @"\updater.exe");
                                    File.Delete(updpath + @"\atractool-rel.zip");
                                    Common.Utils.DeleteDirectory(updpath + @"\updater-temp");

                                    fsWPF?.Dispatcher.Invoke(d, Localization.SplashFormUpdatedCaption);
                                    MessageBox.Show(this, Localization.UpdateCompletedCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    var update = Task.Run(CheckForUpdatesForInit);
                                    update.Wait();
                                }
                                fsWPF?.Dispatcher.Invoke(d, "Update check completed.");
                                Thread.Sleep(500);
                            }
                            else
                            {
                                fsWPF?.Dispatcher.Invoke(d, "Skip Update");
                                Thread.Sleep(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, "An error occured.\n" + ex, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        if (Debugmode)
                        {
                            fsWPF?.Dispatcher.Invoke(d, "Initialize Debug window...");
                            Thread.Sleep(800);
                            InitDebugWindow();
                            fsWPF?.Dispatcher.Invoke(d, "Debug window initialize Completed.");
                            Thread.Sleep(500);
                        }

                        fsWPF?.Dispatcher.Invoke(d, "Starting...");
                        Thread.Sleep(800);
                    }

                    CloseSplash();
                }
                else // スプラッシュスクリーンなし
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\_temp");
                    ResetStatus();

                    int ts = Utils.GetInt("ToolStrip", 65535);
                    //string prm1 = Config.Entry["ATRAC3_Params"].Value, prm2 = Config.Entry["ATRAC9_Params"].Value, prm3 = Config.Entry["Walkman_Params"].Value;
                    if (ts != 65535)
                    {
                        switch (ts)
                        {
                            case 0:
                                Common.Generic.ATRACFlag = 0;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                aTRAC9ToolStripMenuItem.Checked = false;
                                walkmanToolStripMenuItem.Checked = false;
                                toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                EncodeMethodIsATRAC(true);
                                break;
                            case 1:
                                Common.Generic.ATRACFlag = 1;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = true;
                                walkmanToolStripMenuItem.Checked = false;
                                toolStripDropDownButton_EF.Text = "ATRAC9";
                                EncodeMethodIsATRAC(true);
                                break;
                            case 2:
                                Common.Generic.ATRACFlag = 2;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = false;
                                walkmanToolStripMenuItem.Checked = true;
                                toolStripDropDownButton_EF.Text = "Walkman";
                                EncodeMethodIsATRAC(false);
                                break;
                        }
                    }

                    Common.Generic.EncodeParamAT3 = Utils.GetString("ATRAC3_Params", string.Empty);
                    Common.Generic.EncodeParamAT9 = Utils.GetString("ATRAC9_Params", string.Empty);
                    Common.Generic.EncodeParamWalkman = Utils.GetString("Walkman_Params", string.Empty);

                    int wOutFmt = Utils.GetInt("Walkman_EveryFmt_OutputFmt", 1);
                    switch (wOutFmt)
                    {
                        case 0:
                            Common.Generic.WalkmanEveryFilter = "PCM ATRAC (*.oma)|*.oma;";
                            break;
                        case 1:
                            Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.oma)|*.oma;";
                            break;
                        case 2:
                            Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.omg)|*.omg;";
                            break;
                        case 3:
                            Common.Generic.WalkmanEveryFilter = "ATRAC3 Advanced Lossless (*.oma)|*.oma;";
                            break;
                        case 4:
                            Common.Generic.WalkmanEveryFilter = "ATRAC3 Video Clip (*.kdr)|*.kdr;";
                            break;
                        case 5:
                            Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3+ (*.oma)|*.oma;";
                            break;
                        case 6:
                            Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3+ (*.omg)|*.omg;";
                            break;
                        case 7:
                            Common.Generic.WalkmanEveryFilter = "ATRAC3+ Advanced Lossless (*.oma)|*.oma;";
                            break;
                        case 8:
                            Common.Generic.WalkmanEveryFilter = "ATRAC3+ Video Clip (*.kdr)|*.kdr;";
                            break;
                    }


                    Generic.lpcreate = Utils.GetBool("LPC_Create", false);

                    int at3Console = Utils.GetInt("ATRAC3_Console", 0);
                    Generic.IsAT3PS3 = (at3Console == 1);
                    int at9Console = Utils.GetInt("ATRAC9_Console", 0);
                    Generic.IsAT9PS4 = (at9Console == 1);

                    Generic.IsAT3LoopPoint = Utils.GetBool("ATRAC3_LoopPoint", false);
                    Generic.IsAT3LoopSound = Utils.GetBool("ATRAC3_LoopSound", false);

                    Generic.IsAT9LoopPoint = Utils.GetBool("ATRAC9_LoopPoint", false);
                    Generic.IsAT9LoopSound = Utils.GetBool("ATRAC9_LoopSound", false);

                    loopPointCreationToolStripMenuItem.Enabled = false;

                    bool Legacymode = Utils.GetBool("Oldmode", false);
                    if (Legacymode)
                    {
                        loopPointCreationToolStripMenuItem.Enabled = true;
                    }
                    else
                    {
                        loopPointCreationToolStripMenuItem.Enabled = false;
                    }

                    bool chkUpdate = Utils.GetBool("Check_Update", true);
                    if (chkUpdate)
                    {
                        try
                        {
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\updated.dat"))
                            {
                                File.Delete(Directory.GetCurrentDirectory() + @"\updated.dat");
                                string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];
                                DirectoryInfo di = new(updpath + @"\updater-temp");
                                Common.Utils.RemoveReadonlyAttribute(di);
                                File.Delete(updpath + @"\updater.exe");
                                File.Delete(updpath + @"\atractool-rel.zip");
                                Common.Utils.DeleteDirectory(updpath + @"\updater-temp");

                                MessageBox.Show(this, Localization.UpdateCompletedCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                var update = Task.Run(CheckForUpdatesForInit);
                                update.Wait();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, "An error occured.\n" + ex, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }

                    bool Debugmode = Utils.GetBool("Debugmode", false);
                    if (Debugmode)
                    {
                        InitDebugWindow();
                        Thread.Sleep(500);
                    }
                }

                toolStripStatusLabel_EncMethod.Alignment = ToolStripItemAlignment.Right;



                if (ver.FileVersion != null)
                {
                    Text = "ATRACTool Rel ( build: " + ver.FileVersion.ToString() + "-Beta )";
                }
                else
                {
                    Text = "ATRACTool Rel";
                }

                panel_Main.BackgroundImage = Resources.SIEv2;
                Activate();

                if (!Utils.OpenMGCheck64() && !Utils.OpenMGCheck64_32())
                {
                    MessageBox.Show(this, "There are no libraries installed on this PC to process OpenMG.\r\nTo generate files for Walkman, Sony Media Library Earth must be installed.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (Generic.GlobalException is not null)
                {
                    MessageBox.Show(this, string.Format(Localization.UnExpectedCaption, Generic.GlobalException), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Utils.CreateExceptionLog(ex, true, this);
                Close();
            }
        }

        #region SplashScreenCommon
        private static void StartThread()
        {
            //fs = new FormSplash();
            //Application.Run(fs);
            fsWPF = new WindowSplash();
            WpfWindowRegistry.Register(fsWPF);
            fsWPF.ShowDialog();
        }


        private static void CloseSplash()
        {
            Dop d = new(CloseForm);
            //fs?.Invoke(d);
            fsWPF?.Dispatcher.Invoke(d);
        }

        private delegate void Dop();
        private static void CloseForm()
        {
            //fs?.Close();
            fsWPF?.Close();
        }

        private delegate void Dmes(string message);
        private static void ShowMessage(string message)
        {
            //fs!.label_log.Text = message;
            fsWPF!.TextBlock_Log.Text = message;
        }
        #endregion

        #region WindowDebugCommon
        public enum DebugLogLevel
        {
            Info,
            Warn,
            Error
        }

        public readonly struct DebugLogEntry
        {
            public DebugLogEntry(DebugLogLevel level, string message, DateTime timestamp)
            {
                Level = level;
                Message = message ?? "";
                Timestamp = timestamp;
            }

            public DebugLogLevel Level { get; }
            public string Message { get; }
            public DateTime Timestamp { get; }
        }

        private void InitDebugWindow()
        {
            ThreadStart tds = new(RunDebugWindow);
            Thread thread = new(tds)
            {
                Name = "DebugWindow",
                IsBackground = true
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            DWMSG d = new(DebugMessage);
            DebugMessage(thread.Name + " (TID: " + thread.ManagedThreadId.ToString() + " ) started.");
        }
        public static void RunDebugWindow()
        {
            windowDebug = new WindowDebug();
            WindowDebug.WindowDebugInstance = windowDebug;

            windowDebug.Closed += (_, __) =>
            {
                _debugReady.Reset(); // 次回起動に備える
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            };

            windowDebug.Show();

            // ★ここで「生成完了」とみなす
            _debugReady.Set();

            // ★起動前に溜まったメッセージを吐く
            FlushQueuedDebugMessages();

            Dispatcher.Run();
        }

        private void CloseDebug()
        {
            CDW cdw = new(CloseDebugWindow);
            windowDebug?.Dispatcher.Invoke(cdw);
        }

        public delegate void CDW();
        public static void CloseDebugWindow()
        {
            windowDebug?.Close();
        }

        public delegate void DWMSG(string message);
        // 互換（既存コードを壊さない）
        private static void DebugMessage(string message) => DebugInfo(message);
        public static void DebugMessageAppend(string message) => DebugInfo(message);

        private static void EnqueueLog(DebugLogLevel level, string message)
        {
            _debugMsgQueue.Enqueue(new DebugLogEntry(level, message, DateTime.Now));

            if (!_debugReady.IsSet) return; // 生成前は溜める
            FlushQueuedDebugMessages();
        }

        private static void FlushQueuedDebugMessages()
        {
            var wd = windowDebug;
            if (wd == null) return;

            wd.Dispatcher.BeginInvoke(new Action(() =>
            {
                while (_debugMsgQueue.TryDequeue(out var entry))
                {
                    wd.AppendLog(entry); // ★WindowDebug側へ
                }
            }));
        }

        public sealed class DebugFormInfo
        {
            public string Name { get; init; } = "";
            public string Hwnd { get; init; } = "null";
            public int Pid { get; init; }
            public int Tid { get; init; }
            public bool Visible { get; init; }
            public bool Enabled { get; init; }
            public string WindowState { get; init; } = "";
            public bool HandleCreated { get; init; }
        }

        public sealed class DebugHandleSnapshot
        {
            public int ProcessId { get; init; } // 全体プロセスID
            public double CpuPercent { get; init; }   // ★追加
            public List<DebugFormInfo> Forms { get; init; } = new();
            public string Main { get; init; } = "null";
            public string LPC { get; init; } = "null";
            public string Progress { get; init; } = "null";
            public string Settings { get; init; } = "null";
            public string Preferences { get; init; } = "null";
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // ★ 追加：OpenForms から型で探して Handle を返す
        private static string GetHandleTextFromOpenForms(Type formType)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f == null || f.IsDisposed) continue;
                if (f.GetType() != formType) continue;

                // Handle が未作成なら「未作成」と分かる値にしておく（null と区別）
                return f.IsHandleCreated ? "HWND: " + f.Handle.ToString() : "not created";
            }
            return "null";
        }

        public static DebugHandleSnapshot GetDebugHandleSnapshot()
        {
            var fm = FormMainInstance;
            if (fm is null || fm.IsDisposed)
                return new DebugHandleSnapshot();

            if (fm.InvokeRequired)
            {
                var flpc = FormLPC.FormLPCInstance;
                if (flpc is null || flpc.IsDisposed)
                    return (DebugHandleSnapshot)fm.Invoke(new Func<DebugHandleSnapshot>(GetDebugHandleSnapshot));

                if (!fm.IsDisposed && fm is not null)
                {
                    return (DebugHandleSnapshot)fm.Invoke(new Func<DebugHandleSnapshot>(GetDebugHandleSnapshot));
                    //return new DebugHandleSnapshot(); // or return _lastSnapshotCache;
                }
            }

            // ===== WinForms UI スレッド =====
            var snap = new DebugHandleSnapshot
            {
                ProcessId = Environment.ProcessId,
                CpuPercent = GetProcessCpuPercent()
            };

            foreach (Form f in Application.OpenForms)
            {
                if (f == null || f.IsDisposed) continue;
                snap.Forms.Add(MakeFormInfo(f));
            }

            return snap;
        }

        public static DebugFormInfo MakeFormInfo(Form f)
        {
            string hwndText;
            int pid = 0;
            int tid = 0;

            if (f.IsHandleCreated)
            {
                hwndText = "HWND: " + f.Handle.ToString();

                uint upid;
                uint utid = GetWindowThreadProcessId(f.Handle, out upid);
                pid = unchecked((int)upid);
                tid = unchecked((int)utid);
            }
            else
            {
                hwndText = "not created";
            }

            return new DebugFormInfo
            {
                Name = f.GetType().Name,
                Hwnd = hwndText,
                Pid = pid,
                Tid = tid,
                Visible = f.Visible,
                Enabled = f.Enabled,
                WindowState = f.WindowState.ToString(),
                HandleCreated = f.IsHandleCreated
            };
        }

        public static DebugFormInfo MakeWpfWindowInfo(System.Windows.Window w)
        {
            string hwndText;
            int pid = 0;
            int tid = 0;

            IntPtr hwnd = new WindowInteropHelper(w).Handle;

            if (hwnd != IntPtr.Zero)
            {
                hwndText = "HWND: " + hwnd.ToString();

                uint upid;
                uint utid = GetWindowThreadProcessId(hwnd, out upid);
                pid = unchecked((int)upid);
                tid = unchecked((int)utid);
            }
            else
            {
                hwndText = "not created";
            }

            return new DebugFormInfo
            {
                Name = "[WPF] " + w.GetType().Name,
                Hwnd = hwndText,
                Pid = pid,
                Tid = tid,
                Visible = w.IsVisible,
                Enabled = w.IsEnabled,
                WindowState = w.WindowState.ToString(),
                HandleCreated = hwnd != IntPtr.Zero
            };
        }

        private static double GetProcessCpuPercent()
        {
            var p = Process.GetCurrentProcess();

            // 初回
            if (_lastCpuTick == 0)
            {
                _lastCpuTick = Environment.TickCount64;
                _lastCpuTime = p.TotalProcessorTime;
                return 0;
            }

            long nowTick = Environment.TickCount64;
            TimeSpan nowCpu = p.TotalProcessorTime;

            double elapsedMs = nowTick - _lastCpuTick;
            double cpuMs = (nowCpu - _lastCpuTime).TotalMilliseconds;

            _lastCpuTick = nowTick;
            _lastCpuTime = nowCpu;

            if (elapsedMs <= 0) return 0;

            // 100% = 1コア相当。全コア換算に合わせる
            double cpu = (cpuMs / elapsedMs) * 100.0 / Environment.ProcessorCount;
            if (cpu < 0) cpu = 0;
            if (cpu > 100) cpu = 100;
            return cpu;
        }
        #endregion

        public void ResetToInitialState()
        {
            // LPC 関連 UI の無効化・状態リセット
            ActivateOrDeactivateLPC(false);

            // ATRAC 再生 / エンコードソースを使っている場合は _temp を掃除
            if (Utils.GetBool("PlaybackATRAC", false) || Utils.GetBool("ATRACEncodeSource", false))
            {
                Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp\");
            }

            // ラベルや状態を「ファイル未読み込み」の状態へ戻す
            ResetStatus();
        }

        /// <summary>
        /// ファイルを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseFileCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetToInitialState();
        }

        /// <summary>
        /// 終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*bool Debugmode = Utils.GetBool("Debugmode", false);
            if (Debugmode)
            {
                CloseDebug();
            }*/
            Close();
        }

        /// <summary>
        /// 変換設定ダイアログ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using Form FSS = new FormSettings(false);
            FSS.ShowDialog();

            // ダイアログで更新された最新の値をここで読み直す
            string prm1 = Config.Entry["ATRAC3_Params"].Value;
            string prm2 = Config.Entry["ATRAC9_Params"].Value;
            string prm3 = Config.Entry["Walkman_Params"].Value;

            // ATRAC3
            Common.Generic.EncodeParamAT3 = string.IsNullOrWhiteSpace(prm1)
                ? string.Empty
                : prm1;

            // ATRAC9
            Common.Generic.EncodeParamAT9 = string.IsNullOrWhiteSpace(prm2)
                ? string.Empty
                : prm2;

            // Walkman
            Common.Generic.EncodeParamWalkman = string.IsNullOrWhiteSpace(prm3)
                ? string.Empty
                : prm3;

            bool lpc = Utils.GetBool("LPC_Create", false);
            switch (lpc)
            {
                case true:
                    Generic.lpcreate = true;
                    if (FormLPC.FormLPCInstance is not null) FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    break;
                case false:
                    Generic.lpcreate = false;
                    if (FormLPC.FormLPCInstance is not null) FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                    break;
            }

            int at3Console = Utils.GetInt("ATRAC3_Console", 0);
            Generic.IsAT3PS3 = (at3Console == 1);
            int at9Console = Utils.GetInt("ATRAC9_Console", 0);
            Generic.IsAT9PS4 = (at9Console == 1);

            Generic.IsAT3LoopPoint = Utils.GetBool("ATRAC3_LoopPoint", false);
            Generic.IsAT3LoopSound = Utils.GetBool("ATRAC3_LoopSound", false);

            Generic.IsAT9LoopPoint = Utils.GetBool("ATRAC9_LoopPoint", false);
            Generic.IsAT9LoopSound = Utils.GetBool("ATRAC9_LoopSound", false);

            if (FormLPC.FormLPCInstance is not null)
            {
                if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                {
                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3/ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                }
                else if (Generic.lpcreate)
                {
                    FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                }
                else
                {
                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// バージョン情報ダイアログの表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutATRACToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*FormAbout formAbout = new();
            formAbout.ShowDialog();
            formAbout.Dispose();*/
            WindowAbout window = new();
            WpfWindowRegistry.Register(window);
            window.ShowDialog();
        }

        /// <summary>
        /// アップデート確認
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CheckForUpdatesUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool IsDebug = Utils.GetBool("Debugmode");

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (IsDebug)
                {
                    MessageBox.Show(Localization.DebugmodeEnableWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    try
                    {
                        string hv = null!;

                        using Stream hcs = await Task.Run(() => Common.Network.GetWebStreamAsync(appUpdatechecker, Common.Network.GetUri("https://raw.githubusercontent.com/XyLe-GBP/ATRACTool-Reloaded/master/VERSIONINFO")));
                        using StreamReader hsr = new(hcs);
                        hv = await Task.Run(() => hsr.ReadToEndAsync());
                        Common.Generic.GitHubLatestVersion = hv[8..].Replace("\n", "");

                        string dummyver = "1.23.4567.890";

                        switch (dummyver.CompareTo(hv[8..].Replace("\n", "")))
                        {
                            case -1:
                                {
                                    DialogResult dr = MessageBox.Show(Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + dummyver + "\n" + Localization.UpdateConfirmCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (dr == DialogResult.Yes)
                                    {
                                        WindowUpdateApplicationType fuat = new();
                                        WpfWindowRegistry.Register(fuat);
                                        bool? fuatdr = fuat.ShowDialog();

                                        switch (fuatdr)
                                        {
                                            case true:
                                                break;
                                            case false:
                                                MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                return;
                                        }

                                        if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                        {
                                            File.Delete(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip");
                                        }

                                        Common.Generic.ProcessFlag = Constants.ProcessType.Update;
                                        Common.Generic.ProgressMax = 100;
                                        using FormProgress form = new();
                                        form.ShowDialog();

                                        if (Common.Generic.Result == false)
                                        {
                                            Common.Generic.cts.Dispose();
                                            MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            return;
                                        }

                                        DialogResult dr2 = MessageBox.Show(Localization.DebugModeUpdateConfirmCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                        if (dr2 == DialogResult.Yes)
                                        {
                                            string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];

                                            if (File.Exists(updpath + @"\updater.exe"))
                                            {
                                                File.Delete(updpath + @"\updater.exe");
                                            }
                                            if (Directory.Exists(updpath + @"\updater-temp"))
                                            {
                                                Common.Utils.DeleteDirectory(updpath + @"\updater-temp");
                                            }
                                            if (File.Exists(updpath + @"\atractool-rel.zip"))
                                            {
                                                File.Delete(updpath + @"\atractool-rel.zip");
                                            }

                                            File.Move(Directory.GetCurrentDirectory() + @"\res\updater.exe", updpath + @"\updater.exe");
                                            string wtext;
                                            switch (Common.Generic.ApplicationPortable)
                                            {
                                                case false:
                                                    {
                                                        wtext = Directory.GetCurrentDirectory() + "\r\nrelease";
                                                    }
                                                    break;
                                                case true:
                                                    {
                                                        wtext = Directory.GetCurrentDirectory() + "\r\nportable";
                                                    }
                                                    break;
                                            }
                                            File.WriteAllText(updpath + @"\updater.txt", wtext);
                                            File.Move(updpath + @"\updater.txt", updpath + @"\updater.dat");
                                            if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                            {
                                                File.Move(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip", updpath + @"\atractool-rel.zip");
                                            }

                                            ProcessStartInfo pi = new()
                                            {
                                                FileName = updpath + @"\updater.exe",
                                                Arguments = null,
                                                UseShellExecute = true,
                                                WindowStyle = ProcessWindowStyle.Normal,
                                            };
                                            Process.Start(pi);
                                            Close();
                                            return;
                                        }
                                        else
                                        {
                                            if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                            {
                                                File.Delete(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip");
                                            }
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        DialogResult dr2 = MessageBox.Show(Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + dummyver + "\n" + Localization.SiteOpenCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                        if (dr2 == DialogResult.Yes)
                                        {
                                            Common.Utils.OpenURI("https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases");
                                            return;
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                }
                            case 0:
                                break;
                            case 1:
                                throw new Exception(hv[8..].Replace("\n", "").ToString() + " < " + dummyver + "\nあんたバカぁ？");
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
                else // No Debugmode
                {
                    try
                    {
                        string hv = null!;

                        using Stream hcs = await Task.Run(() => Common.Network.GetWebStreamAsync(appUpdatechecker, Common.Network.GetUri("https://raw.githubusercontent.com/XyLe-GBP/ATRACTool-Reloaded/master/VERSIONINFO")));
                        using StreamReader hsr = new(hcs);
                        hv = await Task.Run(hsr.ReadToEndAsync);
                        Common.Generic.GitHubLatestVersion = hv[8..].Replace("\n", "");

                        FileVersionInfo ver = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

                        if (ver.FileVersion != null)
                        {
                            switch (ver.FileVersion.ToString().CompareTo(hv[8..].Replace("\n", "")))
                            {
                                case -1:
                                    DialogResult dr = MessageBox.Show(Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + ver.FileVersion + "\n" + Localization.UpdateConfirmCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (dr == DialogResult.Yes)
                                    {
                                        WindowUpdateApplicationType fuat = new();
                                        bool? fuatdr = fuat.ShowDialog();

                                        switch (fuatdr)
                                        {
                                            case true:
                                                break;
                                            case false:
                                                MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                return;
                                        }

                                        if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                        {
                                            File.Delete(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip");
                                        }

                                        Common.Generic.ProcessFlag = Constants.ProcessType.Update;
                                        Common.Generic.ProgressMax = 100;
                                        using FormProgress form = new();
                                        form.ShowDialog();

                                        if (Common.Generic.Result == false)
                                        {
                                            Common.Generic.cts.Dispose();
                                            MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            return;
                                        }

                                        string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];

                                        if (File.Exists(updpath + @"\updater.exe"))
                                        {
                                            File.Delete(updpath + @"\updater.exe");
                                        }
                                        if (Directory.Exists(updpath + @"\updater-temp"))
                                        {
                                            Common.Utils.DeleteDirectory(updpath + @"\updater-temp");
                                        }
                                        if (File.Exists(updpath + @"\atractool-rel.zip"))
                                        {
                                            File.Delete(updpath + @"\atractool-rel.zip");
                                        }

                                        File.Move(Directory.GetCurrentDirectory() + @"\res\updater.exe", updpath + @"\updater.exe");
                                        string wtext;
                                        switch (Common.Generic.ApplicationPortable)
                                        {
                                            case false:
                                                {
                                                    wtext = Directory.GetCurrentDirectory() + "\r\nrelease";
                                                }
                                                break;
                                            case true:
                                                {
                                                    wtext = Directory.GetCurrentDirectory() + "\r\nportable";
                                                }
                                                break;
                                        }
                                        File.WriteAllText(updpath + @"\updater.txt", wtext);
                                        File.Move(updpath + @"\updater.txt", updpath + @"\updater.dat");
                                        if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                        {
                                            File.Move(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip", updpath + @"\atractool-rel.zip");
                                        }

                                        ProcessStartInfo pi = new()
                                        {
                                            FileName = updpath + @"\updater.exe",
                                            Arguments = null,
                                            UseShellExecute = true,
                                            WindowStyle = ProcessWindowStyle.Normal,
                                        };
                                        Process.Start(pi);
                                        Close();
                                        return;
                                    }
                                    else
                                    {
                                        DialogResult dr2 = MessageBox.Show(this, Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + ver.FileVersion + "\n" + Localization.SiteOpenCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                        if (dr2 == DialogResult.Yes)
                                        {
                                            Common.Utils.OpenURI("https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases");
                                            return;
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                case 0:
                                    MessageBox.Show(this, Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + ver.FileVersion + "\n" + Localization.UptodateCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    break;
                                case 1:
                                    throw new Exception(hv[8..].Replace("\n", "").ToString() + " < " + ver.FileVersion.ToString() + "\nあんたバカぁ？");
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, string.Format(Localization.UnExpectedCaption, ex.ToString()), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show(this, Localization.NetworkNotConnectedCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary>
        /// (Task) アプリケーション起動時のアップデート確認
        /// </summary>
        /// <returns></returns>
        private async Task CheckForUpdatesForInit()
        {
            bool IsDebug = Utils.GetBool("Debugmode");

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (IsDebug)
                {
                    MessageBox.Show(Localization.DebugmodeEnableWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    try
                    {
                        string hv = null!;

                        using Stream hcs = await Task.Run(() => Common.Network.GetWebStreamAsync(appUpdatechecker, Common.Network.GetUri("https://raw.githubusercontent.com/XyLe-GBP/ATRACTool-Reloaded/master/VERSIONINFO")));
                        using StreamReader hsr = new(hcs);
                        hv = await Task.Run(() => hsr.ReadToEndAsync());
                        Common.Generic.GitHubLatestVersion = hv[8..].Replace("\n", "");

                        string dummyver = "1.23.4567.890";

                        switch (dummyver.CompareTo(hv[8..].Replace("\n", "")))
                        {
                            case -1:
                                {
                                    DialogResult dr = MessageBox.Show(Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + dummyver + "\n" + Localization.UpdateConfirmCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (dr == DialogResult.Yes)
                                    {
                                        using FormUpdateApplicationType fuat = new();
                                        DialogResult fuatdr = fuat.ShowDialog();

                                        switch (fuatdr)
                                        {
                                            case DialogResult.OK:
                                                break;
                                            case DialogResult.Cancel:
                                                MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                return;
                                        }

                                        if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                        {
                                            File.Delete(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip");
                                        }

                                        Common.Generic.ProcessFlag = Constants.ProcessType.Update;
                                        Common.Generic.ProgressMax = 100;
                                        using FormProgress form = new();
                                        form.ShowDialog();

                                        if (Common.Generic.Result == false)
                                        {
                                            Common.Generic.cts.Dispose();
                                            MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            return;
                                        }

                                        DialogResult dr2 = MessageBox.Show(Localization.DebugModeUpdateConfirmCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                        if (dr2 == DialogResult.Yes)
                                        {
                                            string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];

                                            if (File.Exists(updpath + @"\updater.exe"))
                                            {
                                                File.Delete(updpath + @"\updater.exe");
                                            }
                                            if (Directory.Exists(updpath + @"\updater-temp"))
                                            {
                                                Common.Utils.DeleteDirectory(updpath + @"\updater-temp");
                                            }
                                            if (File.Exists(updpath + @"\atractool-rel.zip"))
                                            {
                                                File.Delete(updpath + @"\atractool-rel.zip");
                                            }

                                            File.Move(Directory.GetCurrentDirectory() + @"\res\updater.exe", updpath + @"\updater.exe");
                                            string wtext;
                                            switch (Common.Generic.ApplicationPortable)
                                            {
                                                case false:
                                                    {
                                                        wtext = Directory.GetCurrentDirectory() + "\r\nrelease";
                                                    }
                                                    break;
                                                case true:
                                                    {
                                                        wtext = Directory.GetCurrentDirectory() + "\r\nportable";
                                                    }
                                                    break;
                                            }
                                            File.WriteAllText(updpath + @"\updater.txt", wtext);
                                            File.Move(updpath + @"\updater.txt", updpath + @"\updater.dat");
                                            if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                            {
                                                File.Move(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip", updpath + @"\atractool-rel.zip");
                                            }

                                            ProcessStartInfo pi = new()
                                            {
                                                FileName = updpath + @"\updater.exe",
                                                Arguments = null,
                                                UseShellExecute = true,
                                                WindowStyle = ProcessWindowStyle.Normal,
                                            };
                                            Process.Start(pi);
                                            Close();
                                            return;
                                        }
                                        else
                                        {
                                            if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                            {
                                                File.Delete(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip");
                                            }
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        DialogResult dr2 = MessageBox.Show(Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + dummyver + "\n" + Localization.SiteOpenCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                        if (dr2 == DialogResult.Yes)
                                        {
                                            Common.Utils.OpenURI("https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases");
                                            return;
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                }
                            case 0:
                                break;
                            case 1:
                                throw new Exception(hv[8..].Replace("\n", "").ToString() + " < " + dummyver + "\nあんたバカぁ？");
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
                else // No Debugmode
                {
                    try
                    {
                        string hv = null!;

                        using Stream hcs = await Task.Run(() => Common.Network.GetWebStreamAsync(appUpdatechecker, Common.Network.GetUri("https://raw.githubusercontent.com/XyLe-GBP/ATRACTool-Reloaded/master/VERSIONINFO")));
                        using StreamReader hsr = new(hcs);
                        hv = await Task.Run(() => hsr.ReadToEndAsync());
                        Common.Generic.GitHubLatestVersion = hv[8..].Replace("\n", "");

                        FileVersionInfo ver = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

                        if (ver.FileVersion != null)
                        {
                            switch (ver.FileVersion.ToString().CompareTo(hv[8..].Replace("\n", "")))
                            {
                                case -1:
                                    DialogResult dr = MessageBox.Show(Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + ver.FileVersion + "\n" + Localization.UpdateConfirmCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (dr == DialogResult.Yes)
                                    {
                                        using FormUpdateApplicationType fuat = new();
                                        DialogResult fuatdr = fuat.ShowDialog();

                                        switch (fuatdr)
                                        {
                                            case DialogResult.OK:
                                                break;
                                            case DialogResult.Cancel:
                                                MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                return;
                                        }

                                        if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                        {
                                            File.Delete(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip");
                                        }

                                        Common.Generic.ProcessFlag = Constants.ProcessType.Update;
                                        Common.Generic.ProgressMax = 100;
                                        using FormProgress form = new();
                                        form.ShowDialog();

                                        if (Common.Generic.Result == false)
                                        {
                                            Common.Generic.cts.Dispose();
                                            MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            return;
                                        }
                                        string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];

                                        if (File.Exists(updpath + @"\updater.exe"))
                                        {
                                            File.Delete(updpath + @"\updater.exe");
                                        }
                                        if (Directory.Exists(updpath + @"\updater-temp"))
                                        {
                                            Common.Utils.DeleteDirectory(updpath + @"\updater-temp");
                                        }
                                        if (File.Exists(updpath + @"\atractool-rel.zip"))
                                        {
                                            File.Delete(updpath + @"\atractool-rel.zip");
                                        }

                                        File.Move(Directory.GetCurrentDirectory() + @"\res\updater.exe", updpath + @"\updater.exe");
                                        string wtext;
                                        switch (Common.Generic.ApplicationPortable)
                                        {
                                            case false:
                                                {
                                                    wtext = Directory.GetCurrentDirectory() + "\r\nrelease";
                                                }
                                                break;
                                            case true:
                                                {
                                                    wtext = Directory.GetCurrentDirectory() + "\r\nportable";
                                                }
                                                break;
                                        }
                                        File.WriteAllText(updpath + @"\updater.txt", wtext);
                                        File.Move(updpath + @"\updater.txt", updpath + @"\updater.dat");
                                        if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                        {
                                            File.Move(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip", updpath + @"\atractool-rel.zip");
                                        }

                                        ProcessStartInfo pi = new()
                                        {
                                            FileName = updpath + @"\updater.exe",
                                            Arguments = null,
                                            UseShellExecute = true,
                                            WindowStyle = ProcessWindowStyle.Normal,
                                        };
                                        Process.Start(pi);
                                        Close();
                                        return;
                                    }
                                    else
                                    {
                                        DialogResult dr2 = MessageBox.Show(Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + ver.FileVersion + "\n" + Localization.SiteOpenCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                        if (dr2 == DialogResult.Yes)
                                        {
                                            Common.Utils.OpenURI("https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases");
                                            return;
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                case 0:
                                    break;
                                case 1:
                                    throw new Exception(hv[8..].Replace("\n", "").ToString() + " < " + ver.FileVersion.ToString() + "\nあんたバカぁ？");
                            }
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }
            else
            {
                return;
            }
        }

        // ステータスバー

        private void ATRAC3ATRAC3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.Entry["ToolStrip"].Value = "0";
            Config.Save(xmlpath);
            Common.Generic.ATRACFlag = 0;
            aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
            aTRAC9ToolStripMenuItem.Checked = false;
            walkmanToolStripMenuItem.Checked = false;
            toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
            EncodeMethodIsATRAC(true);
            if (FormLPC.FormLPCInstance is not null)
            {
                FormLPC.FormLPCInstance.checkBox_LoopEnable.Enabled = true;
                if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                {
                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3/ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                }
                else if (Generic.lpcreate)
                {
                    FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                }
                else
                {
                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                    }
                }
            }
        }

        private void ATRAC9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.Entry["ToolStrip"].Value = "1";
            Config.Save(xmlpath);
            Common.Generic.ATRACFlag = 1;
            aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
            aTRAC9ToolStripMenuItem.Checked = true;
            walkmanToolStripMenuItem.Checked = false;
            toolStripDropDownButton_EF.Text = "ATRAC9";
            EncodeMethodIsATRAC(true);
            if (FormLPC.FormLPCInstance is not null)
            {
                FormLPC.FormLPCInstance.checkBox_LoopEnable.Enabled = true;
                if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                {
                    if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3/ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                }
                else if (Generic.lpcreate)
                {
                    FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                }
                else
                {
                    if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                    }

                }
            }
        }

        private void walkmanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.Entry["ToolStrip"].Value = "2";
            Config.Save(xmlpath);
            Common.Generic.ATRACFlag = 2;
            aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
            aTRAC9ToolStripMenuItem.Checked = false;
            walkmanToolStripMenuItem.Checked = true;
            toolStripDropDownButton_EF.Text = "Walkman";
            EncodeMethodIsATRAC(false);
            if (FormLPC.FormLPCInstance is not null)
            {
                FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                FormLPC.FormLPCInstance.checkBox_LoopEnable.Enabled = false;
                FormLPC.FormLPCInstance.CautionLabel = "Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
            }
        }

        // ボタン

        /// <summary>
        /// Decode ATRAC File(s).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Decode_Click(object sender, EventArgs e)
        {
            Config.Load(xmlpath);

            bool manual = Utils.GetBool("Save_IsManual", false);
            bool IsFasterATRAC = Utils.GetBool("FasterATRAC", false);
            bool IsPlayingATRAC = Utils.GetBool("PlaybackATRAC", false);

            if (IsPlayingATRAC && Generic.IsATRAC && !IsFasterATRAC) // ATRAC再生が有効
            {
                if (manual)
                {
                    MessageBox.Show(this, Localization.ATRACPlaybackEnabledSpecLocationWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                ActivateOrDeactivateLPC(false);

                if (Generic.pATRACOpenFilePaths.Length == 1) // 単一
                {
                    SaveFileDialog sfd = new()
                    {
                        FileName = Common.Utils.SFDRandomNumber(),
                        InitialDirectory = "",
                        Filter = Localization.WAVEFilter,
                        FilterIndex = 1,
                        Title = Localization.SaveDialogTitle,
                        OverwritePrompt = true,
                        RestoreDirectory = true
                    };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        if (File.Exists(sfd.FileName))
                        {
                            File.Delete(sfd.FileName);
                        }
                        File.Move(Generic.pATRACOpenFilePaths[0], sfd.FileName);
                        if (File.Exists(sfd.FileName))
                        {
                            MessageBox.Show(this, Localization.DecodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Utils.ShowFolder(sfd.FileName, Utils.GetBool("ShowFolder", true));
                            ResetStatus();
                            return;
                        }
                        else
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.DecodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }
                    else // Cancelled
                    {
                        ResetStatus();
                        return;
                    }
                }
                else // 複数
                {
                    FolderBrowserDialog fbd = new()
                    {
                        Description = Localization.FolderSaveDialogTitle,
                        RootFolder = Environment.SpecialFolder.MyDocuments,
                        SelectedPath = @"",
                    };
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        int AcceptFile = 0, ErrorFile = 0;
                        if (Directory.GetFiles(fbd.SelectedPath, "*", SearchOption.AllDirectories).Length != 0)
                        {
                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (dr == DialogResult.Yes)
                            {
                                try
                                {
                                    if (Utils.GetBool("Save_NestFolderSource", false) && Generic.IsLoadFolder)
                                    {
                                        // フォルダ自体は残す。中身だけ削除を試みる。
                                        // ロックされているものは残るが、後続で別名出力するので問題なし。
                                        Common.Utils.TryDeleteDirectoryContents(fbd.SelectedPath);
                                        Directory.CreateDirectory(fbd.SelectedPath); // 既にあってもOK
                                    }
                                    else
                                    {
                                        // フォルダ自体は残す。中身だけ削除を試みる。
                                        // ロックされているものは残るが、後続で別名出力するので問題なし。
                                        Common.Utils.TryDeleteDirectoryContents(fbd.SelectedPath);
                                        Directory.CreateDirectory(fbd.SelectedPath); // 既にあってもOK
                                    }
                                }
                                catch (Exception ex)
                                {
                                    FormMain.DebugError("Exception occured: " + ex);
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                        uint count = 0, fcount = 1;
                        foreach (var file in Generic.pATRACOpenFilePaths)
                        {
                            if (Utils.GetBool("Save_NestFolderSource", false) && Generic.IsLoadFolder)
                            {
                                FileInfo fi = new(Generic.InputJobs[(int)count].OriginPath);
                                DirectoryInfo di = new(Generic.OriginOpenFilePaths[count]);

                                if (di.Parent is not null && !Directory.Exists(fbd.SelectedPath + @"\" + di.Parent.Name))
                                {
                                    Directory.CreateDirectory(fbd.SelectedPath + @"\" + di.Parent.Name);
                                }

                                if (File.Exists(fbd.SelectedPath + @"\" + di.Parent!.Name + @"\" + fi.Name.Replace(fi.Extension, ".wav")))
                                {
                                    File.Move(file, fbd.SelectedPath + @"\" + di.Parent!.Name + @"\" + fi.Name.Replace(fi.Extension, "(" + fcount + ").wav"));

                                    if (File.Exists(fbd.SelectedPath + @"\" + di.Parent.Name + @"\" + fi.Name.Replace(fi.Extension, "(" + fcount + ").wav")))
                                    {
                                        AcceptFile++;
                                        count++;
                                        fcount++;
                                        continue;
                                    }
                                    else
                                    {
                                        ErrorFile++;
                                        count++;
                                        continue;
                                    }
                                }
                                else
                                {
                                    File.Move(file, fbd.SelectedPath + @"\" + di.Parent!.Name + @"\" + fi.Name.Replace(fi.Extension, ".wav"));

                                    if (File.Exists(fbd.SelectedPath + @"\" + di.Parent.Name + @"\" + fi.Name.Replace(fi.Extension, ".wav")))
                                    {
                                        AcceptFile++;
                                        count++;
                                        continue;
                                    }
                                    else
                                    {
                                        ErrorFile++;
                                        count++;
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                FileInfo fi = new(Generic.InputJobs[(int)count].OriginPath);

                                if (File.Exists(fbd.SelectedPath + @"\" + fi.Name.Replace(fi.Extension, ".wav")))
                                {
                                    File.Move(file, fbd.SelectedPath + @"\" + fi.Name.Replace(fi.Extension, "(" + fcount + ").wav"));

                                    if (File.Exists(fbd.SelectedPath + @"\" + fi.Name.Replace(fi.Extension, "(" + fcount + ").wav")))
                                    {
                                        AcceptFile++;
                                        fcount++;
                                        continue;
                                    }
                                    else
                                    {
                                        ErrorFile++;
                                        continue;
                                    }
                                }
                                else
                                {
                                    File.Move(file, fbd.SelectedPath + @"\" + fi.Name.Replace(fi.Extension, ".wav"));

                                    if (File.Exists(fbd.SelectedPath + @"\" + fi.Name.Replace(fi.Extension, ".wav")))
                                    {
                                        AcceptFile++;
                                        continue;
                                    }
                                    else
                                    {
                                        ErrorFile++;
                                        continue;
                                    }
                                }
                            }
                        }
                        MessageBox.Show(this, string.Format(Localization.DecodeSuccessCaption + "\nSuccess: {0} Files\nError: {1} Files", AcceptFile, ErrorFile), Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Utils.ShowFolder(fbd.SelectedPath, Utils.GetBool("ShowFolder", true));
                        ResetStatus();
                        return;
                    }
                    else // Cancelled
                    {
                        ResetStatus();
                        return;
                    }
                }
            }
            else if (IsFasterATRAC && IsPlayingATRAC && Generic.IsATRAC)
            {
                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
            }
            else // ATRAC再生が無効
            {
                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
            }

            toolStripStatusLabel_Status.ForeColor = Color.FromArgb(0, 0, 0, 0);
            toolStripStatusLabel_Status.Text = "Decoding...";

            if (Common.Generic.OpenFilePaths.Length == 1) // 単一ファイル
            {
                switch (manual)
                {
                    case true: // 固定場所に保存
                        {
                            FileInfo fi = new(Generic.OpenFilePaths[0]);
                            string suffix = "";
                            switch (bool.Parse(Config.Entry["Save_IsSubfolder"].Value))
                            {
                                case true:
                                    {
                                        if (Config.Entry["Save_Subfolder_Suffix"].Value != "")
                                        {
                                            suffix = Config.Entry["Save_Subfolder_Suffix"].Value;
                                        }
                                        if (suffix != "")
                                        {
                                            if (!Directory.Exists(Config.Entry["Save_Isfolder"].Value + @"\" + suffix))
                                            {
                                                Directory.CreateDirectory(Config.Entry["Save_Isfolder"].Value + @"\" + suffix);
                                            }
                                        }

                                        Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                        Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav";
                                        Generic.ProgressMax = 1;
                                        break;
                                    }
                                case false:
                                    {
                                        Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                        Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav";
                                        Generic.ProgressMax = 1;
                                        break;
                                    }
                            }
                            break;
                        }
                    case false: // 通常保存
                        {
                            SaveFileDialog sfd = new()
                            {
                                FileName = Common.Utils.SFDRandomNumber(),
                                InitialDirectory = "",
                                Filter = Localization.WAVEFilter,
                                FilterIndex = 1,
                                Title = Localization.SaveDialogTitle,
                                OverwritePrompt = true,
                                RestoreDirectory = true
                            };
                            if (sfd.ShowDialog() == DialogResult.OK)
                            {
                                Common.Generic.SavePath = sfd.FileName;
                                Common.Generic.ProgressMax = 1;
                            }
                            else // Cancelled
                            {
                                ResetStatus();
                                return;
                            }
                            break;
                        }
                }


            }
            else // 複数ファイル
            {
                switch (manual)
                {
                    case true: // 固定場所に保存
                        {
                            string suffix = "";
                            switch (bool.Parse(Config.Entry["Save_IsSubfolder"].Value))
                            {
                                case true:
                                    {
                                        if (Config.Entry["Save_Subfolder_Suffix"].Value != "")
                                        {
                                            suffix = Config.Entry["Save_Subfolder_Suffix"].Value;
                                        }
                                        if (suffix != "")
                                        {
                                            if (!Directory.Exists(Config.Entry["Save_Isfolder"].Value + @"\" + suffix))
                                            {
                                                Directory.CreateDirectory(Config.Entry["Save_Isfolder"].Value + @"\" + suffix);
                                            }
                                        }

                                        Generic.FolderSavePath = Config.Entry["Save_Isfolder"].Value + @"\" + suffix;
                                        if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                        {
                                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                            if (dr == DialogResult.Yes)
                                            {
                                                if (Utils.GetBool("Save_NestFolderSource", false) && Generic.IsLoadFolder)
                                                {
                                                    Common.Utils.DeleteDirectory(Common.Generic.FolderSavePath);
                                                    Directory.CreateDirectory(Common.Generic.FolderSavePath);
                                                }
                                                else
                                                {
                                                    Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                                        break;
                                    }
                                case false:
                                    {
                                        Generic.FolderSavePath = Config.Entry["Save_Isfolder"].Value;
                                        if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                        {
                                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                            if (dr == DialogResult.Yes)
                                            {
                                                if (Utils.GetBool("Save_NestFolderSource", false) && Generic.IsLoadFolder)
                                                {
                                                    Common.Utils.DeleteDirectory(Common.Generic.FolderSavePath);
                                                    Directory.CreateDirectory(Common.Generic.FolderSavePath);
                                                }
                                                else
                                                {
                                                    Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                                        break;
                                    }
                            }
                            break;
                        }
                    case false: // 通常保存
                        {
                            FolderBrowserDialog fbd = new()
                            {
                                Description = Localization.FolderSaveDialogTitle,
                                RootFolder = Environment.SpecialFolder.MyDocuments,
                                SelectedPath = @"",
                            };
                            if (fbd.ShowDialog() == DialogResult.OK)
                            {
                                Common.Generic.FolderSavePath = fbd.SelectedPath;
                                if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                {
                                    DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (dr == DialogResult.Yes)
                                    {
                                        if (Utils.GetBool("Save_NestFolderSource", false) && Generic.IsLoadFolder)
                                        {
                                            Common.Utils.DeleteDirectory(Common.Generic.FolderSavePath);
                                            Directory.CreateDirectory(Common.Generic.FolderSavePath);
                                        }
                                        else
                                        {
                                            Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                        }
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                            }
                            else // Cancelled
                            {
                                ResetStatus();
                                return;
                            }
                            break;
                        }
                }

            }

            Common.Generic.ProcessFlag = 0;

            Form formProgress = new FormProgress();
            formProgress.ShowDialog();
            formProgress.Dispose();

            if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested) // 中断
            {
                Common.Generic.cts.Dispose();
                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                ResetStatus();
                return;
            }
            else
            {
                if (Common.Generic.OpenFilePaths.Length == 1) // 単一
                {
                    FileInfo fi = new(Common.Generic.SavePath);
                    Common.Generic.cts.Dispose();
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                    {
                        if (File.Exists(Common.Generic.SavePath))
                        {
                            File.Delete(Common.Generic.SavePath);
                        }
                        File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                        if (File.Exists(Common.Generic.SavePath))
                        {
                            if (fi.Length != 0) // OK
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.DecodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Utils.ShowFolder(Common.Generic.SavePath, Utils.GetBool("ShowFolder", true));
                                ResetStatus();
                                return;
                            }
                            else // Error
                            {
                                File.Delete(Common.Generic.SavePath);
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.DecodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                        else // Exception
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }
                    else // Exception
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetStatus();
                        return;
                    }
                }
                else // 複数
                {
                    Common.Generic.cts.Dispose();

                    if (Utils.GetBool("Save_NestFolderSource", false) && Generic.IsLoadFolder) // ネスト方式のフォルダを読み込み、保存時にソースと同じネスト方式で保存
                    {
                        uint count = 0;
                        foreach (var file in Common.Generic.OpenFilePaths)
                        {
                            FileInfo fi = new(file);
                            DirectoryInfo di = new(Generic.OriginOpenFilePaths[count]);

                            if (di.Parent is not null && !Directory.Exists(Common.Generic.FolderSavePath + @"\" + di.Parent.Name))
                            {
                                Directory.CreateDirectory(Common.Generic.FolderSavePath + @"\" + di.Parent.Name);
                            }

                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav")))
                            {
                                if (di.Parent is not null && File.Exists(Common.Generic.FolderSavePath + @"\" + di.Parent.Name + @"\" + fi.Name.Replace(fi.Extension, ".wav")))
                                {
                                    File.Delete(Common.Generic.FolderSavePath + @"\" + di.Parent.Name + @"\" + fi.Name.Replace(fi.Extension, ".wav"));
                                }
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav"), Common.Generic.FolderSavePath + @"\" + di.Parent!.Name + @"\" + fi.Name.Replace(fi.Extension, ".wav"));
                                count++;
                                continue;
                            }
                            else // Error
                            {
                                Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath + @"\");
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }

                        if (Common.Generic.OpenFilePaths.Length == Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.TopDirectoryOnly).Length) // OK
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Utils.ShowFolder(Common.Generic.FolderSavePath, Utils.GetBool("ShowFolder", true));
                            ResetStatus();
                            return;
                        }
                        else if (Common.Generic.OpenFilePaths.Length > Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length && Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0) // 一部変換失敗
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodePartialCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            Utils.ShowFolder(Common.Generic.FolderSavePath, Utils.GetBool("ShowFolder", true));
                            ResetStatus();
                            return;
                        }
                        else // Error
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }
                    else // オプション無効
                    {
                        foreach (var file in Common.Generic.OpenFilePaths)
                        {
                            FileInfo fi = new(file);
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav")))
                            {
                                if (File.Exists(Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, ".wav")))
                                {
                                    File.Delete(Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, ".wav"));
                                }
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav"), Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, ".wav"));
                                continue;
                            }
                            else // Error
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }

                        if (Common.Generic.OpenFilePaths.Length == Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.TopDirectoryOnly).Length) // OK
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Utils.ShowFolder(Common.Generic.FolderSavePath, Utils.GetBool("ShowFolder", true));
                            ResetStatus();
                            return;
                        }
                        else if (Common.Generic.OpenFilePaths.Length > Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.TopDirectoryOnly).Length && Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.TopDirectoryOnly).Length != 0) // 一部変換失敗
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodePartialCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            Utils.ShowFolder(Common.Generic.FolderSavePath, Utils.GetBool("ShowFolder", true));
                            ResetStatus();
                            return;
                        }
                        else // Error
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Encode ATRAC File(s).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Encode_Click(object sender, EventArgs e)
        {
            Config.Load(xmlpath);
            bool lpc = bool.Parse(Config.Entry["LPC_Create"].Value);
            bool manual = bool.Parse(Config.Entry["Save_IsManual"].Value);
            bool iseveryfmt = bool.Parse(Config.Entry["Walkman_EveryFmt"].Value);
            bool atracencsource = bool.Parse(Config.Entry["ATRACEncodeSource"].Value);
            Common.Generic.lpcreate = lpc switch
            {
                false => false,
                true => true,
            };

            if (Generic.ATRACFlag == 0 || Generic.ATRACFlag == 1)
            {
                if (Utils.CheckATRACFormatError(FormLPC.FormLPCInstance.SampleRate))
                {
                    MessageBox.Show(this, Localization.UnsupportedFormatErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (Common.Generic.ATRACFlag == 0 || Common.Generic.ATRACFlag == 1 || Generic.ATRACFlag == 2)
            {
                if (string.IsNullOrWhiteSpace(Generic.EncodeParamAT3) || string.IsNullOrWhiteSpace(Generic.EncodeParamAT9) || string.IsNullOrWhiteSpace(Generic.EncodeParamWalkman))
                {
                    // Param Error
                    MessageBox.Show(this, Localization.SettingsErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else // OK
                {
                    ActivateOrDeactivateLPC(false);

                    if (atracencsource && Generic.IsATRAC)
                    {
                        uint error = 0;
                        string ct = Utils.SFDRandomNumber();
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\_ATRACEnc" + ct);
                        Generic.ATRACEncodeSourceTempPath = Directory.GetCurrentDirectory() + @"\_ATRACEnc" + ct;

                        List<string> list = [];
                        foreach (string path in Generic.pATRACOpenFilePaths)
                        {
                            FileInfo fi = new(path);
                            File.Move(path, Generic.ATRACEncodeSourceTempPath + @"\" + fi.Name.Replace(".ata", ".wav"));
                            if (File.Exists(Generic.ATRACEncodeSourceTempPath + @"\" + fi.Name.Replace(".ata", ".wav")))
                            {
                                list.Add(Generic.ATRACEncodeSourceTempPath + @"\" + fi.Name.Replace(".ata", ".wav"));
                            }
                            else
                            {
                                error++;
                            }
                        }
                        if (error != 0)
                        {
                            MessageBox.Show(this, string.Format(Localization.UnExpectedCaption, "File move failed. [" + error + " Files]"), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        Generic.pATRACOpenFilePaths = list.ToArray();
                    }
                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");

                    toolStripStatusLabel_Status.ForeColor = Color.FromArgb(0, 0, 0, 0);
                    toolStripStatusLabel_Status.Text = "Encoding...";

                    if (Common.Generic.OpenFilePaths.Length == 1)
                    {
                        if (manual != true) // 通常保存
                        {
                            switch (Common.Generic.ATRACFlag)
                            {
                                case 0:
                                    {
                                        SaveFileDialog sfd = new()
                                        {
                                            FileName = Common.Utils.SFDRandomNumber(),
                                            InitialDirectory = "",
                                            Filter = Localization.AT3Filter,
                                            FilterIndex = 1,
                                            Title = Localization.SaveDialogTitle,
                                            OverwritePrompt = true,
                                            RestoreDirectory = true
                                        };
                                        if (sfd.ShowDialog() == DialogResult.OK)
                                        {
                                            Common.Generic.SavePath = sfd.FileName;
                                            Common.Generic.ProgressMax = 1;
                                        }
                                        else // Cancelled
                                        {
                                            ResetStatus();
                                            return;
                                        }
                                        break;
                                    }
                                case 1:
                                    {
                                        SaveFileDialog sfd = new()
                                        {
                                            FileName = Common.Utils.SFDRandomNumber(),
                                            InitialDirectory = "",
                                            Filter = Localization.AT9Filter,
                                            FilterIndex = 1,
                                            Title = Localization.SaveDialogTitle,
                                            OverwritePrompt = true,
                                            RestoreDirectory = true
                                        };
                                        if (sfd.ShowDialog() == DialogResult.OK)
                                        {
                                            Common.Generic.SavePath = sfd.FileName;
                                            Common.Generic.ProgressMax = 1;
                                        }
                                        else // Cancelled
                                        {
                                            ResetStatus();
                                            return;
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        if (!iseveryfmt)
                                        {
                                            SaveFileDialog sfd = new()
                                            {
                                                FileName = Common.Utils.SFDRandomNumber(),
                                                InitialDirectory = "",
                                                Filter = Common.Generic.WalkmanEveryFilter,
                                                FilterIndex = 0,
                                                Title = Localization.SaveDialogTitle,
                                                OverwritePrompt = true,
                                                RestoreDirectory = true
                                            };
                                            if (sfd.ShowDialog() == DialogResult.OK)
                                            {
                                                Common.Generic.SavePath = sfd.FileName;
                                                Common.Generic.ProgressMax = 1;
                                            }
                                            else // Cancelled
                                            {
                                                ResetStatus();
                                                return;
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            SaveFileDialog sfd = new()
                                            {
                                                FileName = Common.Utils.SFDRandomNumber(),
                                                InitialDirectory = "",
                                                Filter = Localization.WalkmanFilter,
                                                FilterIndex = 0,
                                                Title = Localization.SaveDialogTitle,
                                                OverwritePrompt = true,
                                                RestoreDirectory = true
                                            };
                                            if (sfd.ShowDialog() == DialogResult.OK)
                                            {
                                                FormSetWalkmanInformations formSetWalkmanInformations = new();
                                                if (formSetWalkmanInformations.ShowDialog() == DialogResult.OK)
                                                {
                                                    Common.Generic.SavePath = sfd.FileName;
                                                    Common.Generic.ProgressMax = 1;
                                                }
                                                else
                                                {
                                                    ResetStatus();
                                                    return;
                                                }
                                            }
                                            else // Cancelled
                                            {
                                                ResetStatus();
                                                return;
                                            }
                                            break;
                                        }
                                    }
                            }
                        }
                        else // 固定場所に保存
                        {
                            FileInfo fi = new(Common.Generic.OpenFilePaths[0]);
                            string suffix = "";
                            switch (bool.Parse(Config.Entry["Save_IsSubfolder"].Value))
                            {
                                case true:
                                    {
                                        if (Config.Entry["Save_Subfolder_Suffix"].Value != "")
                                        {
                                            suffix = Config.Entry["Save_Subfolder_Suffix"].Value;
                                        }
                                        if (suffix != "")
                                        {
                                            if (!Directory.Exists(Config.Entry["Save_Isfolder"].Value + @"\" + suffix))
                                            {
                                                Directory.CreateDirectory(Config.Entry["Save_Isfolder"].Value + @"\" + suffix);
                                            }
                                        }

                                        switch (Generic.ATRACFlag)
                                        {
                                            case 0:
                                                Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".at3");
                                                Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".at3";
                                                Generic.ProgressMax = 1;
                                                break;
                                            case 1:
                                                Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".at9");
                                                Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".at9";
                                                Generic.ProgressMax = 1;
                                                break;
                                        }
                                        break;
                                    }
                                case false:
                                    {
                                        switch (Generic.ATRACFlag)
                                        {
                                            case 0:
                                                Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".at3");
                                                Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".at3";
                                                Generic.ProgressMax = 1;
                                                break;
                                            case 1:
                                                Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".at9");
                                                Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".at9";
                                                Generic.ProgressMax = 1;
                                                break;
                                        }
                                        break;
                                    }
                            }
                        }

                    }
                    else // 複数のファイル
                    {
                        if (bool.Parse(Config.Entry["ATRAC3_LoopPoint"].Value) || bool.Parse(Config.Entry["ATRAC9_LoopPoint"].Value))
                        {
                            MessageBox.Show(this, Localization.MultipleLoopPointErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                        if (manual != true) // 通常保存
                        {
                            FolderBrowserDialog fbd = new()
                            {
                                Description = Localization.FolderSaveDialogTitle,
                                RootFolder = Environment.SpecialFolder.MyDocuments,
                                SelectedPath = @"",
                            };
                            if (fbd.ShowDialog() == DialogResult.OK)
                            {
                                Common.Generic.FolderSavePath = fbd.SelectedPath;
                                if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0 || Directory.GetDirectories(Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                {
                                    DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (dr == DialogResult.Yes)
                                    {
                                        if (Utils.GetBool("Save_NestFolderSource", false) && Generic.IsLoadFolder)
                                        {
                                            Common.Utils.DeleteDirectory(Common.Generic.FolderSavePath);
                                            Directory.CreateDirectory(Common.Generic.FolderSavePath);
                                        }
                                        else
                                        {
                                            Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                        }
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }

                                if (Generic.ATRACFlag == 2) // Walkman
                                {
                                    if (iseveryfmt)
                                    {
                                        FormSelectWalkmanFormats formSelectWalkmanFormats = new(true);
                                        if (formSelectWalkmanFormats.ShowDialog() == DialogResult.OK)
                                        {
                                            SetWalkmanMultiConvertFormats(true, int.Parse(Generic.WalkmanMultiConvFmt));
                                        }
                                        else { return; }
                                    }
                                    else
                                    {
                                        SetWalkmanMultiConvertFormats(true, Utils.GetInt("Walkman_EveryFmt_OutputFmt"));
                                    }
                                }

                                Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                            }
                            else // Cancelled
                            {
                                ResetStatus();
                                return;
                            }
                        }
                        else // 固定場所に保存
                        {
                            string suffix = "";
                            switch (bool.Parse(Config.Entry["Save_IsSubfolder"].Value))
                            {
                                case true:
                                    {
                                        if (Config.Entry["Save_Subfolder_Suffix"].Value != "")
                                        {
                                            suffix = Config.Entry["Save_Subfolder_Suffix"].Value;
                                        }
                                        if (suffix != "")
                                        {
                                            if (!Directory.Exists(Config.Entry["Save_Isfolder"].Value + @"\" + suffix))
                                            {
                                                Directory.CreateDirectory(Config.Entry["Save_Isfolder"].Value + @"\" + suffix);
                                            }
                                        }

                                        Generic.FolderSavePath = Config.Entry["Save_Isfolder"].Value + @"\" + suffix;
                                        if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0 || Directory.GetDirectories(Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                        {
                                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                            if (dr == DialogResult.Yes)
                                            {
                                                if (Utils.GetBool("Save_NestFolderSource", false) && Generic.IsLoadFolder)
                                                {
                                                    Common.Utils.DeleteDirectory(Common.Generic.FolderSavePath);
                                                    Directory.CreateDirectory(Common.Generic.FolderSavePath);
                                                }
                                                else
                                                {
                                                    Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                                        break;
                                    }
                                case false:
                                    {
                                        Generic.FolderSavePath = Config.Entry["Save_Isfolder"].Value;
                                        if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0 || Directory.GetDirectories(Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                        {
                                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                            if (dr == DialogResult.Yes)
                                            {
                                                if (Utils.GetBool("Save_NestFolderSource", false) && Generic.IsLoadFolder)
                                                {
                                                    Common.Utils.DeleteDirectory(Common.Generic.FolderSavePath);
                                                    Directory.CreateDirectory(Common.Generic.FolderSavePath);
                                                }
                                                else
                                                {
                                                    Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                                        break;
                                    }
                            }

                        }

                    }

                    Common.Generic.ProcessFlag = Constants.ProcessType.Encode;

                    Form formProgress = new FormProgress();
                    formProgress.ShowDialog();
                    formProgress.Dispose();

                    if (Common.Generic.lpcreatev2 != false)
                    {
                        Config.Entry["ATRAC3_LoopPoint"].Value = "false";
                        Config.Entry["ATRAC3_LoopStart_Samples"].Value = "";
                        Config.Entry["ATRAC3_LoopEnd_Samples"].Value = "";
                        Config.Entry["ATRAC9_LoopPoint"].Value = "false";
                        Config.Entry["ATRAC9_LoopStart_Samples"].Value = "";
                        Config.Entry["ATRAC9_LoopEnd_Samples"].Value = "";
                        Config.Save(xmlpath);
                        Generic.LPCSuffix = string.Empty;
                        Common.Generic.lpcreatev2 = false;
                    }

                    if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested)
                    {
                        Common.Generic.cts.Dispose();
                        MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        ResetStatus();
                        return;
                    }
                    else
                    {
                        if (Common.Generic.OpenFilePaths.Length == 1) // 単一
                        {
                            FileInfo fi = new(Common.Generic.SavePath);
                            Common.Generic.cts.Dispose();
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                            {
                                if (File.Exists(Common.Generic.SavePath))
                                {
                                    File.Delete(Common.Generic.SavePath);
                                }
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                if (File.Exists(Common.Generic.SavePath))
                                {
                                    if (fi.Length != 0) // OK
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, Localization.EncodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        Utils.ShowFolder(Common.Generic.SavePath, Utils.GetBool("ShowFolder", true));
                                        ResetStatus();
                                        return;
                                    }
                                    else // Error
                                    {
                                        File.Delete(Common.Generic.SavePath);
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.EncodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        ResetStatus();
                                        return;
                                    }
                                }
                                else // Exception
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }
                            }
                            else // Exception
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                        else // 複数
                        {
                            Common.Generic.cts.Dispose();

                            if (Utils.GetBool("Save_NestFolderSource", false) && Generic.IsLoadFolder) // ネスト保存
                            {
                                string tempDir = Path.Combine(Directory.GetCurrentDirectory(), "_temp");
                                int accept = 0, error = 0;

                                var jobs = Common.Generic.InputJobs;
                                if (jobs == null || jobs.Count == 0)
                                {
                                    Common.Utils.DeleteDirectoryFiles(tempDir);
                                    MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption,
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }

                                // ルート（フォルダ読み込み時に保存しておくのが理想）
                                // 既に Common.Generic.LoadFolderRootPath を持っているならそれを優先
                                string? root = Common.Generic.LoadFolderRootPath;
                                if (string.IsNullOrWhiteSpace(root))
                                {
                                    // 取れない場合のフォールバック：最初のOriginの親を root とする（最低限の動作保証）
                                    root = Path.GetDirectoryName(jobs[0].OriginPath);
                                }
                                root ??= "";

                                for (int i = 0; i < jobs.Count; i++)
                                {
                                    var job = jobs[i];

                                    // temp 側の実ファイル名（__0000 等の一意 suffix を含む）
                                    string tempOut = Common.Utils.MakeTempUniquePath(
                                        tempDir,
                                        job.OriginPath,
                                        i,
                                        Common.Generic.ATRACExt);

                                    if (!File.Exists(tempOut))
                                    {
                                        error++;
                                        continue;
                                    }

                                    // 相対パス（root からの相対）→ 出力先フォルダ構造を再現
                                    string rel = job.OriginPath;
                                    if (!string.IsNullOrWhiteSpace(root))
                                    {
                                        try { rel = Path.GetRelativePath(root, job.OriginPath); } catch { rel = Path.GetFileName(job.OriginPath); }
                                    }

                                    string? relDir = Path.GetDirectoryName(rel);
                                    string destDir = string.IsNullOrWhiteSpace(relDir)
                                        ? Common.Generic.FolderSavePath
                                        : Path.Combine(Common.Generic.FolderSavePath, relDir);

                                    if (!Directory.Exists(destDir))
                                        Directory.CreateDirectory(destDir);

                                    // 保存先ファイル名（ハッシュ無しで、元ファイル名ベース）
                                    string baseName = Path.GetFileNameWithoutExtension(job.OriginPath);
                                    string destPath = Utils.MakeUniqueDestPath(destDir, baseName, Common.Generic.ATRACExt);

                                    try
                                    {
                                        // 既存があれば上書きしたいなら削除（MakeUniqueDestPath 方式なら通常不要だが安全のため）
                                        if (File.Exists(destPath))
                                            File.Delete(destPath);

                                        File.Move(tempOut, destPath);

                                        // 0 byte は失敗扱いで削除
                                        var fi2 = new FileInfo(destPath);
                                        if (fi2.Length == 0)
                                        {
                                            File.Delete(destPath);
                                            error++;
                                            continue;
                                        }

                                        accept++;
                                    }
                                    catch
                                    {
                                        error++;
                                    }
                                }

                                // temp 掃除
                                Common.Utils.DeleteDirectoryFiles(tempDir);

                                // 旧コードは「出力ファイル数==入力数」を条件にしていましたが、
                                // ネスト＋衝突回避(連番)だと数一致に意味が薄いので accept/error で通知します
                                if (accept > 0 && error == 0)
                                {
                                    MessageBox.Show(this, Localization.EncodeSuccessCaption, Localization.MSGBoxSuccessCaption,
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (accept > 0 && error > 0)
                                {
                                    MessageBox.Show(this, Localization.EncodePartialCaption, Localization.MSGBoxWarningCaption,
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption,
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }

                                Utils.ShowFolder(Common.Generic.FolderSavePath, Utils.GetBool("ShowFolder", true));
                                ResetStatus();
                                return;
                            }
                            else // オプション無効
                            {
                                // ★InputJobs を正とする
                                var jobs = Common.Generic.InputJobs;
                                if (jobs == null || jobs.Count == 0)
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption,
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }

                                for (int i = 0; i < jobs.Count; i++)
                                {
                                    var job = jobs[i];

                                    // temp 側の “実際の出力ファイル名” を、FormProgress と同じ規則で作る
                                    // ※あなたの実装で out が "__0000" になる以上、ここも同じにする必要があります
                                    string tempOut = Common.Utils.MakeTempUniquePath(
                                        Directory.GetCurrentDirectory() + @"\_temp",
                                        job.OriginPath,
                                        i,
                                        Common.Generic.ATRACExt);

                                    if (!File.Exists(tempOut))
                                    {
                                        // ここで見つからないなら、FormProgress で使っている outPath 規則と不一致
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption,
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        ResetStatus();
                                        return;
                                    }

                                    // 最終保存名は “元ファイル名（suffix無し）” を基本にする
                                    string baseName = Path.GetFileNameWithoutExtension(job.OriginPath) + Common.Generic.ATRACExt;
                                    string destOut = Path.Combine(Common.Generic.FolderSavePath, baseName);

                                    // 同名衝突時は連番
                                    destOut = Common.Utils.MakeNonCollidingPath(destOut);

                                    if (File.Exists(destOut))
                                        File.Delete(destOut);

                                    File.Move(tempOut, destOut);

                                    // 0byte の失敗ファイルは削除
                                    var fi2 = new FileInfo(destOut);
                                    if (fi2.Length == 0)
                                    {
                                        File.Delete(destOut);
                                    }
                                }

                                // ここまで来たら成功扱い
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.EncodeSuccessCaption, Localization.MSGBoxSuccessCaption,
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Utils.ShowFolder(Common.Generic.FolderSavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                                ResetStatus();
                                return;
                            }

                        }
                    }
                }
            }
            else
            {
                // Select Error
                MessageBox.Show(this, Localization.EncodemethodErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        /// <summary>
        /// 詳細設定ダイアログ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreferencesMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool oldSSValue = bool.Parse(Config.Entry["SmoothSamples"].Value);
            uint oldPlaybackValue = uint.Parse(Config.Entry["LPCPlaybackMethod"].Value);
            uint oldMPlaybackValue = uint.Parse(Config.Entry["LPCMultipleStreamPlaybackMethod"].Value);
            bool oldAlwaysWASAPIorASIOPlaybackValue = bool.Parse(Config.Entry["LPCMultipleStreamAlwaysWASAPIorASIO"].Value);
            uint oldDSBuffer = uint.Parse(Config.Entry["DirectSoundBuffers"].Value);
            uint oldDSBufferVal = uint.Parse(Config.Entry["DirectSoundBuffersValue"].Value);
            uint oldDSLatency = uint.Parse(Config.Entry["DirectSoundLatency"].Value);
            uint oldDSLatencyVal = uint.Parse(Config.Entry["DirectSoundLatencyValue"].Value);
            uint oldWASAPISLatency = uint.Parse(Config.Entry["WASAPILatencyShared"].Value);
            uint oldWASAPISLatencyVal = uint.Parse(Config.Entry["WASAPILatencySharedValue"].Value);
            uint oldWASAPIELatency = uint.Parse(Config.Entry["WASAPILatencyExclusived"].Value);
            uint oldWASAPIELatencyVal = uint.Parse(Config.Entry["WASAPILatencyExclusivedValue"].Value);
            uint oldThreadCount = uint.Parse(Config.Entry["PlaybackThreadCount"].Value);
            bool oldParallelUseVal = bool.Parse(Config.Entry["UseParallelMethod"].Value);

            using Form FSS = new FormPreferencesSettings();
            FSS.ShowDialog();

            if (Generic.IsConfigError)
            {
                MessageBox.Show(this, "The configuration file is corrupt. Delete the configuration file and restart the application.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                File.Delete(Common.xmlpath);
                Application.Restart();
                return;
            }

            if (oldSSValue != bool.Parse(Config.Entry["SmoothSamples"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldPlaybackValue != uint.Parse(Config.Entry["LPCPlaybackMethod"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldMPlaybackValue != uint.Parse(Config.Entry["LPCMultipleStreamPlaybackMethod"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldAlwaysWASAPIorASIOPlaybackValue != bool.Parse(Config.Entry["LPCMultipleStreamAlwaysWASAPIorASIO"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldDSBuffer != uint.Parse(Config.Entry["DirectSoundBuffers"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldDSBufferVal != uint.Parse(Config.Entry["DirectSoundBuffersValue"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldDSLatency != uint.Parse(Config.Entry["DirectSoundLatency"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldDSLatencyVal != uint.Parse(Config.Entry["DirectSoundLatencyValue"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldWASAPISLatency != uint.Parse(Config.Entry["WASAPILatencyShared"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldWASAPISLatencyVal != uint.Parse(Config.Entry["WASAPILatencySharedValue"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldWASAPIELatency != uint.Parse(Config.Entry["WASAPILatencyExclusived"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldWASAPIELatencyVal != uint.Parse(Config.Entry["WASAPILatencyExclusivedValue"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldThreadCount != uint.Parse(Config.Entry["PlaybackThreadCount"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else if (oldParallelUseVal != bool.Parse(Config.Entry["UseParallelMethod"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
                CheckLPCException();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 旧関数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoopPointCreationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using FormLPC form = new(true);
            form.ShowDialog();
        }

        /// <summary>
        /// ファイルを読み込んだ際のボタン等の動作
        /// </summary>
        private void ReadStatus()
        {
            AllowDrop = false;
            toolStripStatusLabel_Status.Text = Localization.ReadyCaption;
            toolStripStatusLabel_Status.ForeColor = Color.Green;
            label_NotReaded.Visible = false;
            label_File.Visible = true;
            label_Format.Visible = true;
            label_Size.Visible = true;
            label_Filepath.Visible = true;
            label_Sizetxt.Visible = true;
            label_Formattxt.Visible = true;
        }

        /// <summary>
        /// ファイルを閉じたときにUIや変数をリセットする
        /// </summary>
        private void ResetStatus()
        {
            AllowDrop = true;
            if (Generic.IsATWCancelled)
            {
                Utils.ATWCheck(Generic.IsATW, true);
                Generic.IsATWCancelled = false;
            }
            else
            {
                Utils.ATWCheck(Generic.IsATW);
            }
            Generic.IsWave = false;
            Generic.IsATRAC = false;
            Generic.IsATRACLooped = false;
            Generic.ReadedATRACFlag = -1;

            Generic.OpenFilePaths = null!;
            Generic.pATRACOpenFilePaths = null!;
            Generic.OriginOpenFilePaths = null!;
            Generic.FolderOpenPaths = null!;
            Generic.SubFolderOpenPaths = null!;
            Generic.IsLoadFolder = false;

            Generic.SavePath = null!;
            Generic.FolderSavePath = null!;
            Generic.pATRACSavePath = null!;
            Generic.pATRACFolderSavePath = null!;

            Generic.ProcessFlag = Constants.ProcessType.None;
            Generic.ProgressMax = -1;
            /*if (panel_Main.BackgroundImage is not null)
            {
                panel_Main.BackgroundImage.Dispose();
            }
            panel_Main.BackgroundImage = null!;*/
            button_Decode.Enabled = false;
            button_Encode.Enabled = false;
            toolStripStatusLabel_Status.Text = Localization.NotReadyCaption;
            toolStripStatusLabel_Status.ForeColor = Color.Red;
            label_NotReaded.Text = Localization.OpenFileCaption;
            label_NotReaded.Visible = true;
            label_File.Visible = false;
            label_Format.Visible = false;
            label_Size.Visible = false;
            label_Filepath.Visible = false;
            label_Sizetxt.Visible = false;
            label_Formattxt.Visible = false;
            toolStripDropDownButton_EF.Enabled = false;
            toolStripDropDownButton_EF.Visible = false;
            toolStripStatusLabel_EncMethod.Enabled = false;
            toolStripStatusLabel_EncMethod.Visible = false;
            loopPointCreationToolStripMenuItem.Enabled = false;
            closeFileCToolStripMenuItem.Enabled = false;
            groupBox_Loop.Enabled = false;
            textBox_LoopStart.Text = string.Empty;
            textBox_LoopEnd.Text = string.Empty;

            Generic.MultipleFilesLoopOKFlags = [];
            Generic.MultipleLoopStarts = [];
            Generic.MultipleLoopEnds = [];
            Generic.ATRACMetadataBuffers = [];
            Generic.ATRACMultiMetadataBuffer = null!;

            if (bool.Parse(Config.Entry["ATRACEncodeSource"].Value))
            {
                Utils.DeleteDirectory(Generic.ATRACEncodeSourceTempPath);
                Generic.ATRACEncodeSourceTempPath = null!;
            }
        }

        /// <summary>
        /// サポートされている任意のファイルをWaveに変換する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioToWAVEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = Localization.ConverterFilters,
                FilterIndex = 15,
                Title = Localization.OpenDialogTitle,
                Multiselect = true,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<string> lst = new();
                foreach (string files in ofd.FileNames)
                {
                    lst.Add(files);
                }
                Common.Generic.OpenFilePaths = lst.ToArray();

                if (Common.Generic.OpenFilePaths.Length == 1) // Single
                {
                    if (bool.Parse(Config.Entry["FixedConvert"].Value)) // Fix
                    {
                        Common.Generic.WTAmethod = (Constants.WTAType)Utils.GetInt("ConvertType", 0);//int.Parse(Config.Entry["ConvertType"].Value);
                        SaveFileDialog sfd = new()
                        {
                            FileName = Common.Utils.SFDRandomNumber(),
                            InitialDirectory = "",
                            Filter = Localization.WAVEFilter,
                            FilterIndex = 1,
                            Title = Localization.SaveDialogTitle,
                            OverwritePrompt = true,
                            RestoreDirectory = true
                        };
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            Common.Generic.SavePath = sfd.FileName;
                            Common.Generic.ProgressMax = 1;

                            Common.Generic.ProcessFlag = Constants.ProcessType.AudioToWave;

                            Form formProgress = new FormProgress();
                            formProgress.ShowDialog();
                            formProgress.Dispose();

                            if (Common.Generic.Result == false)
                            {
                                Common.Generic.cts.Dispose();
                                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            FileInfo fi = new(Common.Generic.SavePath);
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                            {
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Utils.ShowFolder(Common.Generic.SavePath, Utils.GetBool("ShowFolder", true));
                                ResetStatus();
                                return;
                            }
                            else // Error
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                        else // Cancelled
                        {
                            return;
                        }
                    }
                    else // normal
                    {
                        //using Form formAtWST = new FormAtWSelectTarget();
                        //DialogResult dr = formAtWST.ShowDialog();
                        WindowATWSelectTarget WATWST = new();
                        WpfWindowRegistry.Register(WATWST);
                        bool? dr = WATWST.ShowDialog();

                        //if (dr != DialogResult.Cancel && dr != DialogResult.None)
                        if (dr != false && dr != null)
                        {
                            SaveFileDialog sfd = new()
                            {
                                FileName = Common.Utils.SFDRandomNumber(),
                                InitialDirectory = "",
                                Filter = Localization.WAVEFilter,
                                FilterIndex = 1,
                                Title = Localization.SaveDialogTitle,
                                OverwritePrompt = true,
                                RestoreDirectory = true
                            };
                            if (sfd.ShowDialog() == DialogResult.OK)
                            {
                                Common.Generic.SavePath = sfd.FileName;
                                Common.Generic.ProgressMax = 1;

                                Common.Generic.ProcessFlag = Constants.ProcessType.AudioToWave;

                                Form formProgress = new FormProgress();
                                formProgress.ShowDialog();
                                formProgress.Dispose();

                                if (Common.Generic.Result == false)
                                {
                                    Common.Generic.cts.Dispose();
                                    MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                FileInfo fi = new(Common.Generic.SavePath);
                                if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                                {
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    Utils.ShowFolder(Common.Generic.SavePath, Utils.GetBool("ShowFolder", true));
                                    ResetStatus();
                                    return;
                                }
                                else // Error
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }
                            }
                            else // Cancelled
                            {
                                return;
                            }
                        }
                        else { return; }
                    }
                }
                else // Multiple
                {
                    if (bool.Parse(Config.Entry["FixedConvert"].Value)) // Fix
                    {
                        Common.Generic.WTAmethod = (Constants.WTAType)Utils.GetInt("ConvertType", 0);//int.Parse(Config.Entry["ConvertType"].Value);
                        FolderBrowserDialog fbd = new()
                        {
                            Description = Localization.FolderSaveDialogTitle,
                            RootFolder = Environment.SpecialFolder.MyDocuments,
                            SelectedPath = @"",
                        };
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            Common.Generic.FolderSavePath = fbd.SelectedPath;
                            if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                            {
                                DialogResult dr2 = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (dr2 == DialogResult.Yes)
                                {
                                    Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                }
                                else
                                {
                                    return;
                                }
                            }
                            Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                            Common.Generic.ProcessFlag = Constants.ProcessType.AudioToWave;

                            Form formProgress = new FormProgress();
                            formProgress.ShowDialog();
                            formProgress.Dispose();

                            if (Common.Generic.Result == false)
                            {
                                Common.Generic.cts.Dispose();
                                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            foreach (var file in Common.Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav"))
                                {
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav", Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                }
                                else // Error
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }
                            }
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Utils.ShowFolder(Common.Generic.FolderSavePath, Utils.GetBool("ShowFolder", true));
                            ResetStatus();
                            return;
                        }
                        else // Cancelled
                        {
                            return;
                        }
                    }
                    else // normal
                    {
                        //using Form formAtWST = new FormAtWSelectTarget();
                        //DialogResult dr = formAtWST.ShowDialog();
                        WindowATWSelectTarget WATWST = new();
                        WpfWindowRegistry.Register(WATWST);
                        bool? dr = WATWST.ShowDialog();

                        //if (dr != DialogResult.Cancel && dr != DialogResult.None)
                        if (dr != false && dr != null)
                        {
                            FolderBrowserDialog fbd = new()
                            {
                                Description = Localization.FolderSaveDialogTitle,
                                RootFolder = Environment.SpecialFolder.MyDocuments,
                                SelectedPath = @"",
                            };
                            if (fbd.ShowDialog() == DialogResult.OK)
                            {
                                Common.Generic.FolderSavePath = fbd.SelectedPath;
                                if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                {
                                    DialogResult dr2 = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (dr2 == DialogResult.Yes)
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                                Common.Generic.ProcessFlag = Constants.ProcessType.AudioToWave;

                                Form formProgress = new FormProgress();
                                formProgress.ShowDialog();
                                formProgress.Dispose();

                                if (Common.Generic.Result == false)
                                {
                                    Common.Generic.cts.Dispose();
                                    MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                foreach (var file in Common.Generic.OpenFilePaths)
                                {
                                    FileInfo fi = new(file);
                                    if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav"))
                                    {
                                        File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav", Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                    }
                                    else // Error
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        ResetStatus();
                                        return;
                                    }
                                }
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Utils.ShowFolder(Common.Generic.FolderSavePath, Utils.GetBool("ShowFolder", true));
                                ResetStatus();
                                return;
                            }
                            else // Cancelled
                            {
                                return;
                            }
                        }
                        else { return; }
                    }
                }
            }
            else // Cancelled
            {
                return;
            }
        }

        /// <summary>
        /// Waveファイルをサポートされている任意のオーディオに変換する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WAVEToAudioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = Localization.WAVEFilter,
                FilterIndex = 0,
                Title = Localization.OpenDialogTitle,
                Multiselect = true,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<string> lst = new();
                foreach (string files in ofd.FileNames)
                {
                    lst.Add(files);
                }
                Common.Generic.OpenFilePaths = lst.ToArray();

                if (Common.Generic.OpenFilePaths.Length == 1) // Single
                {
                    SaveFileDialog sfd = new()
                    {
                        FileName = Common.Utils.SFDRandomNumber(),
                        InitialDirectory = "",
                        Filter = Localization.ConverterFilters,
                        FilterIndex = 14,
                        Title = Localization.SaveDialogTitle,
                        OverwritePrompt = true,
                        RestoreDirectory = true
                    };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        Common.Generic.SavePath = sfd.FileName;
                        Common.Generic.ProgressMax = 1;

                        Common.Generic.ProcessFlag = Constants.ProcessType.WaveToAudio;

                        Form formProgress = new FormProgress();
                        formProgress.ShowDialog();
                        formProgress.Dispose();

                        if (Common.Generic.Result == false)
                        {
                            Common.Generic.cts.Dispose();
                            MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        FileInfo fi = new(Common.Generic.SavePath);
                        if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                        {
                            File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Utils.ShowFolder(Common.Generic.SavePath, Utils.GetBool("ShowFolder", true));
                            ResetStatus();
                            return;
                        }
                        else // Error
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }
                    else // Cancelled
                    {
                        return;
                    }
                }
                else // Multiple
                {
                    FolderBrowserDialog fbd = new()
                    {
                        Description = Localization.FolderSaveDialogTitle,
                        RootFolder = Environment.SpecialFolder.MyDocuments,
                        SelectedPath = @"",
                    };
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        Common.Generic.FolderSavePath = fbd.SelectedPath;
                        if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                        {
                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (dr == DialogResult.Yes)
                            {
                                Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                            }
                            else
                            {
                                return;
                            }
                        }

                        WindowATWSelect WATW = new();
                        WpfWindowRegistry.Register(WATW);
                        if (WATW.ShowDialog() == true)
                        {
                            Utils.SetWTAFormat(Common.Generic.WTAFlag);
                        }
                        else
                        {
                            return;
                        }

                        /*Form formATWSelect = new FormATWSelect();
                        if (formATWSelect.ShowDialog() == DialogResult.OK)
                        {
                            Common.Utils.SetWTAFormat(Common.Generic.WTAFlag);
                            formATWSelect.Dispose();
                        }
                        else // Cancelled
                        {
                            return;
                        }*/

                        Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                        Common.Generic.ProcessFlag = Constants.ProcessType.WaveToAudio;

                        Form formProgress = new FormProgress();
                        formProgress.ShowDialog();
                        formProgress.Dispose();

                        if (Common.Generic.Result == false)
                        {
                            Common.Generic.cts.Dispose();
                            MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        foreach (var file in Common.Generic.OpenFilePaths)
                        {
                            FileInfo fi = new(file);
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Common.Generic.WTAFmt))
                            {
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Common.Generic.WTAFmt, Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, "") + Common.Generic.WTAFmt);
                            }
                            else // Error
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Utils.ShowFolder(Common.Generic.FolderSavePath, Utils.GetBool("ShowFolder", true));
                        ResetStatus();
                        return;
                    }
                    else // Cancelled
                    {
                        return;
                    }
                }
            }
            else // Cancelled
            {
                return;
            }
        }

        /// <summary>
        /// ファイルを選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = Localization.Filters,
                FilterIndex = 12,
                Title = Localization.OpenDialogTitle,
                Multiselect = true,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                closeFileCToolStripMenuItem.PerformClick();

                Utils.ATWCheck(Generic.IsATW);

                List<string> lst = [.. ofd.FileNames];
                Generic.OpenFilePaths = lst.ToArray();
                Generic.OriginOpenFilePaths = lst.ToArray();
                Generic.BuildInputJobsFromPaths(Generic.OpenFilePaths, Generic.OriginOpenFilePaths);
                //BuildInputJobsFromPaths(ofd.FileNames);

                if (Generic.OpenFilePaths.Length == 1) // Single
                {
                    Generic.IsOpenMulti = false;

                    Generic.MultipleLoopStarts = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleLoopEnds = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleFilesLoopOKFlags = new bool[Generic.OpenFilePaths.Length];

                    Generic.ATRACMetadataBuffers = new int[3];

                    FileInfo file = new(ofd.FileName);
                    long FileSize = file.Length;
                    if (FileSize >= uint.MaxValue)
                    {
                        MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ReadStatus();
                    label_Filepath.Text = ofd.FileName;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FileSize / 1024, FileSize);

                    closeFileCToolStripMenuItem.Enabled = true;

                    switch (file.Extension.ToUpper())
                    {
                        case ".WAV":
                            if (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                            {
                                FormatSorter(true, true);
                            }
                            else
                            {
                                FormatSorter(true);
                            }
                            break;
                        case ".MP3":
                            FormatSorter(true, true);
                            break;
                        case ".M4A":
                            FormatSorter(true, true);
                            break;
                        case ".AAC":
                            FormatSorter(true, true);
                            break;
                        case ".FLAC":
                            FormatSorter(true, true);
                            break;
                        case ".ALAC":
                            FormatSorter(true, true);
                            break;
                        case ".AIFF":
                            FormatSorter(true, true);
                            break;
                        case ".OGG":
                            FormatSorter(true, true);
                            break;
                        case ".OPUS":
                            FormatSorter(true, true);
                            break;
                        case ".WMA":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            Generic.ReadedATRACFlag = 0;
                            Utils.ReadMetadatas(Generic.OpenFilePaths[0], Generic.ATRACMetadataBuffers);
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            Generic.ReadedATRACFlag = 1;
                            Utils.ReadMetadatas(Generic.OpenFilePaths[0], Generic.ATRACMetadataBuffers);
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    return;
                }
                else // Multiple
                {
                    Generic.IsOpenMulti = true;

                    long Filesizes = 0;
                    FileInfo fs = new(Generic.OpenFilePaths[0]);
                    long FS = fs.Length;
                    foreach (string file in Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);

                        if (fi.Length >= uint.MaxValue)
                        {
                            MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else
                        {
                            Filesizes += fi.Length;
                        }
                    }

                    string Ft = "";
                    int count = 0, wavcount = 0;
                    List<string> multiextlst = new();

                    foreach (var file in Common.Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);

                        if (count != 0)
                        {
                            if (Ft == ".AT3" || Ft == ".AT9")
                            {
                                if (Ft != fi.Extension.ToUpper())
                                {
                                    MessageBox.Show(this, Localization.FileMixedWithATRACCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    closeFileCToolStripMenuItem.Enabled = false;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    toolStripStatusLabel_EncMethod.Enabled = false;
                                    toolStripStatusLabel_EncMethod.Visible = false;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    return;
                                }
                            }
                            else
                            {
                                if (fi.Extension.ToUpper() == ".AT3" || fi.Extension.ToUpper() == ".AT9")
                                {
                                    MessageBox.Show(this, Localization.FileMixedWithATRACCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    closeFileCToolStripMenuItem.Enabled = false;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    toolStripStatusLabel_EncMethod.Enabled = false;
                                    toolStripStatusLabel_EncMethod.Visible = false;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    return;
                                }
                                if (count == Generic.OpenFilePaths.Length - 1)
                                {
                                    if (wavcount == Generic.OpenFilePaths.Length - 1)
                                    {
                                        if (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                                        {
                                            Ft = ".NOT";
                                        }
                                        else
                                        {
                                            Ft = ".WAV";
                                        }

                                    }
                                    else
                                    {
                                        Ft = ".NOT";
                                    }
                                }
                                else
                                {
                                    if (fi.Extension.ToUpper() == Ft)
                                    {
                                        Ft = fi.Extension.ToUpper();
                                        wavcount++;
                                    }
                                    else if (fi.Extension.ToUpper() != Ft)
                                    {
                                        Ft = fi.Extension.ToUpper();
                                    }
                                    else
                                    {
                                        Ft = fi.Extension.ToUpper();
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (fi.Extension.ToUpper() == ".WAV")
                            {
                                Ft = fi.Extension.ToUpper();
                                multiextlst.Add(file);
                                wavcount++;
                                count++;
                                continue;
                            }
                            else if (fi.Extension.ToUpper() == ".AT3" || fi.Extension.ToUpper() == ".AT9")
                            {
                                Ft = fi.Extension.ToUpper();
                            }
                            else
                            {
                                Ft = fi.Extension.ToUpper();
                                multiextlst.Add(file);
                            }
                        }

                        count++;
                    }

                    ReadStatus();
                    //label_Filepath.Text = Localization.MultipleFilesCaption;
                    label_Filepath.Text = Generic.OpenFilePaths[0];
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024, FS);

                    closeFileCToolStripMenuItem.Enabled = true;

                    Generic.MultipleLoopStarts = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleLoopEnds = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleFilesLoopOKFlags = new bool[Generic.OpenFilePaths.Length];

                    Generic.ATRACMultiMetadataBuffer = new int[Generic.OpenFilePaths.Length, 3];

                    switch (Ft.ToUpper())
                    {
                        case ".WAV":
                            FormatSorter(true);
                            break;
                        case ".NOT":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            Generic.ReadedATRACFlag = 0;
                            Utils.ReadMetadatasMulti(Generic.OpenFilePaths, Generic.ATRACMultiMetadataBuffer);
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            Generic.ReadedATRACFlag = 1;
                            Utils.ReadMetadatasMulti(Generic.OpenFilePaths, Generic.ATRACMultiMetadataBuffer);
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    return;
                }
            }
            else
            {
                ActivateOrDeactivateLPC(false);
                ResetStatus();
                return;
            }
        }

        /// <summary>
        /// フォルダーを選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new()
            {
                Description = Localization.FolderLoadDialogTitle,
                RootFolder = Environment.SpecialFolder.MyDocuments,
                Multiselect = true,
                SelectedPath = @"",
            };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                foreach (var check in fbd.SelectedPaths)
                {
                    if (Directory.Exists(check))
                    {
                        if (Generic.IsLoadFolder)
                        {
                            MessageBox.Show(this, Localization.NotAllowedMultiFolderCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Generic.IsLoadFolder = false;
                            return;
                        }
                        var files = Utils.GetFolderAllFiles(check);
                        Generic.OpenFilePaths = files;
                        Generic.OriginOpenFilePaths = files;
                        
                        //Utils.GetFolderAllFiles(check);
                        // 一次情報確定
                        //Common.Generic.BuildInputJobsFromPaths(Generic.OpenFilePaths, Generic.OriginOpenFilePaths);
                        Generic.IsLoadFolder = true;
                        Generic.LoadFolderRootPath = fbd.SelectedPath;
                        Generic.BuildInputJobsFromPaths(Generic.OpenFilePaths, Generic.OriginOpenFilePaths);

                        continue;
                    }
                    else
                    {
                        return;
                    }

                }

                if (Generic.OpenFilePaths.Length == 1) // Single
                {
                    Generic.IsOpenMulti = false;

                    Generic.MultipleLoopStarts = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleLoopEnds = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleFilesLoopOKFlags = new bool[Generic.OpenFilePaths.Length];

                    Generic.ATRACMetadataBuffers = new int[3];

                    FileInfo file = new(Generic.OpenFilePaths[0]);
                    long FileSize = file.Length;
                    if (FileSize >= uint.MaxValue)
                    {
                        MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ReadStatus();
                    label_Filepath.Text = file.FullName;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FileSize / 1024, FileSize);

                    closeFileCToolStripMenuItem.Enabled = true;

                    switch (file.Extension.ToUpper())
                    {
                        case ".WAV":
                            if (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                            {
                                FormatSorter(true, true);
                            }
                            else
                            {
                                FormatSorter(true);
                            }
                            break;
                        case ".MP3":
                            FormatSorter(true, true);
                            break;
                        case ".M4A":
                            FormatSorter(true, true);
                            break;
                        case ".AAC":
                            FormatSorter(true, true);
                            break;
                        case ".FLAC":
                            FormatSorter(true, true);
                            break;
                        case ".ALAC":
                            FormatSorter(true, true);
                            break;
                        case ".AIFF":
                            FormatSorter(true, true);
                            break;
                        case ".OGG":
                            FormatSorter(true, true);
                            break;
                        case ".OPUS":
                            FormatSorter(true, true);
                            break;
                        case ".WMA":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            Generic.ReadedATRACFlag = 0;
                            Utils.ReadMetadatas(Generic.OpenFilePaths[0], Generic.ATRACMetadataBuffers);
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            Generic.ReadedATRACFlag = 1;
                            Utils.ReadMetadatas(Generic.OpenFilePaths[0], Generic.ATRACMetadataBuffers);
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    return;
                }
                else // 複数ファイル
                {
                    Generic.IsOpenMulti = true;

                    long FS;
                    if (Generic.IsLoadFolder)
                    {
                        long Filesizes = 0;
                        FileInfo fs = new(Generic.OpenFilePaths[0]);
                        FS = fs.Length;
                        foreach (string file in Generic.OpenFilePaths)
                        {
                            FileInfo fi = new(file);

                            if (fi.Length >= uint.MaxValue)
                            {
                                MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            else
                            {
                                Filesizes += fi.Length;
                            }
                        }
                    }
                    else
                    {
                        long Filesizes = 0;
                        FileInfo fs = new(Generic.OpenFilePaths[0]);
                        FS = fs.Length;
                        foreach (string file in Generic.OpenFilePaths)
                        {
                            FileInfo fi = new(file);

                            if (fi.Length >= uint.MaxValue)
                            {
                                MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            else
                            {
                                Filesizes += fi.Length;
                            }
                        }
                    }



                    string Ft = "";
                    int count = 0, wavcount = 0;
                    List<string> multiextlst = new();

                    foreach (var file in Common.Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);

                        if (count != 0)
                        {
                            if (Ft == ".AT3" || Ft == ".AT9")
                            {
                                if (Ft != fi.Extension.ToUpper())
                                {
                                    MessageBox.Show(this, Localization.FileMixedWithATRACCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    closeFileCToolStripMenuItem.Enabled = false;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    toolStripStatusLabel_EncMethod.Enabled = false;
                                    toolStripStatusLabel_EncMethod.Visible = false;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    return;
                                }
                            }
                            else
                            {
                                if (fi.Extension.ToUpper() == ".AT3" || fi.Extension.ToUpper() == ".AT9")
                                {
                                    MessageBox.Show(this, Localization.FileMixedWithATRACCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    closeFileCToolStripMenuItem.Enabled = false;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    toolStripStatusLabel_EncMethod.Enabled = false;
                                    toolStripStatusLabel_EncMethod.Visible = false;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    return;
                                }
                                if (count == Generic.OpenFilePaths.Length - 1)
                                {
                                    if (wavcount == Generic.OpenFilePaths.Length - 1)
                                    {
                                        if (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                                        {
                                            Ft = ".NOT";
                                        }
                                        else
                                        {
                                            Ft = ".WAV";
                                        }

                                    }
                                    else
                                    {
                                        Ft = ".NOT";
                                    }
                                }
                                else
                                {
                                    if (fi.Extension.ToUpper() == Ft)
                                    {
                                        Ft = fi.Extension.ToUpper();
                                        wavcount++;
                                    }
                                    else if (fi.Extension.ToUpper() != Ft)
                                    {
                                        Ft = fi.Extension.ToUpper();
                                    }
                                    else
                                    {
                                        Ft = fi.Extension.ToUpper();
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (fi.Extension.ToUpper() == ".WAV")
                            {
                                Ft = fi.Extension.ToUpper();
                                multiextlst.Add(file);
                                wavcount++;
                                count++;
                                continue;
                            }
                            else if (fi.Extension.ToUpper() == ".AT3" || fi.Extension.ToUpper() == ".AT9")
                            {
                                Ft = fi.Extension.ToUpper();
                            }
                            else
                            {
                                Ft = fi.Extension.ToUpper();
                                multiextlst.Add(file);
                            }
                        }

                        count++;
                    }

                    ReadStatus();
                    //label_Filepath.Text = Localization.MultipleFilesCaption;
                    label_Filepath.Text = Generic.OpenFilePaths[0];
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024, FS);

                    closeFileCToolStripMenuItem.Enabled = true;

                    Generic.MultipleLoopStarts = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleLoopEnds = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleFilesLoopOKFlags = new bool[Generic.OpenFilePaths.Length];

                    Generic.ATRACMultiMetadataBuffer = new int[Generic.OpenFilePaths.Length, 3];

                    switch (Ft.ToUpper())
                    {
                        case ".WAV":
                            FormatSorter(true);
                            break;
                        case ".NOT":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            Generic.ReadedATRACFlag = 0;
                            Utils.ReadMetadatasMulti(Generic.OpenFilePaths, Generic.ATRACMultiMetadataBuffer);
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            Generic.ReadedATRACFlag = 1;
                            Utils.ReadMetadatasMulti(Generic.OpenFilePaths, Generic.ATRACMultiMetadataBuffer);
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    return;
                }
            }
            else // Cancelled
            {
                ActivateOrDeactivateLPC(false);
                ResetStatus();
                return;
            }
        }

        /// <summary>
        /// ドラッグアンドドロップ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// ドラッグアンドドロップでファイルを読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            Utils.ATWCheck(Generic.IsATW);
            bool fileflag = false;
            if (e.Data != null)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop)!;

                foreach (var check in files)
                {
                    if (Directory.Exists(check))
                    {
                        if (fileflag)
                        {
                            MessageBox.Show(this, Localization.NotAllowedFolderWFileCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Generic.IsLoadFolder = false;
                            return;
                        }
                        if (Generic.IsLoadFolder)
                        {
                            MessageBox.Show(this, Localization.NotAllowedMultiFolderCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Generic.IsLoadFolder = false;
                            return;
                        }
                        //Utils.GetFolderAllFiles(check);
                        var folderfiles = Utils.GetFolderAllFiles(check);
                        Generic.OpenFilePaths = folderfiles;
                        Generic.OriginOpenFilePaths = folderfiles;
                        
                        Generic.IsLoadFolder = true;
                        Generic.LoadFolderRootPath = check;
                        Generic.BuildInputJobsFromPaths(Generic.OpenFilePaths, Generic.OriginOpenFilePaths);
                        continue;
                    }
                    else
                    {
                        if (Generic.IsLoadFolder)
                        {
                            MessageBox.Show(this, Localization.NotAllowedFolderWFileCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Generic.IsLoadFolder = false;
                            return;
                        }
                        fileflag = true;

                        FileInfo file = new(check);
                        switch (file.Extension.ToUpper())
                        {
                            case ".WAV":
                                continue;
                            case ".MP3":
                                continue;
                            case ".M4A":
                                continue;
                            case ".AAC":
                                continue;
                            case ".AIFF":
                                continue;
                            case ".ALAC":
                                continue;
                            case ".FLAC":
                                continue;
                            case ".OGG":
                                continue;
                            case ".OPUS":
                                continue;
                            case ".WMA":
                                continue;
                            case ".AT3":
                                continue;
                            case ".AT9":
                                continue;
                            default:
                                {
                                    MessageBox.Show(this, Localization.NotAllowedExtensionCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                        }
                    }

                }

                if (Generic.IsLoadFolder)
                {
                    //Generic.OriginOpenFilePaths = Generic.OpenFilePaths;
                    Generic.OriginOpenFilePaths = Generic.OpenFilePaths.ToArray();
                }
                else
                {
                    List<string> lst = [.. files];
                    Generic.OpenFilePaths = lst.ToArray();
                    Generic.OriginOpenFilePaths = lst.ToArray();
                }
                Generic.BuildInputJobsFromPaths(Generic.OpenFilePaths, Generic.OriginOpenFilePaths);

                if (Generic.OpenFilePaths.Length == 1) // Single
                {
                    Generic.IsOpenMulti = false;

                    Generic.MultipleLoopStarts = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleLoopEnds = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleFilesLoopOKFlags = new bool[Generic.OpenFilePaths.Length];

                    Generic.ATRACMetadataBuffers = new int[3];

                    FileInfo file = new(files[0]);
                    long FileSize = file.Length;
                    if (FileSize >= uint.MaxValue)
                    {
                        MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ReadStatus();
                    label_Filepath.Text = file.FullName;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FileSize / 1024, FileSize);

                    closeFileCToolStripMenuItem.Enabled = true;

                    switch (file.Extension.ToUpper())
                    {
                        case ".WAV":
                            if (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                            {
                                FormatSorter(true, true);
                            }
                            else
                            {
                                FormatSorter(true);
                            }
                            break;
                        case ".MP3":
                            FormatSorter(true, true);
                            break;
                        case ".M4A":
                            FormatSorter(true, true);
                            break;
                        case ".AAC":
                            FormatSorter(true, true);
                            break;
                        case ".FLAC":
                            FormatSorter(true, true);
                            break;
                        case ".ALAC":
                            FormatSorter(true, true);
                            break;
                        case ".AIFF":
                            FormatSorter(true, true);
                            break;
                        case ".OGG":
                            FormatSorter(true, true);
                            break;
                        case ".OPUS":
                            FormatSorter(true, true);
                            break;
                        case ".WMA":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            Generic.ReadedATRACFlag = 0;
                            Utils.ReadMetadatas(Generic.OpenFilePaths[0], Generic.ATRACMetadataBuffers);
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            Generic.ReadedATRACFlag = 1;
                            Utils.ReadMetadatas(Generic.OpenFilePaths[0], Generic.ATRACMetadataBuffers);
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    return;
                }
                else // 複数ファイル
                {
                    Generic.IsOpenMulti = true;

                    long FS;
                    if (Generic.IsLoadFolder)
                    {
                        long Filesizes = 0;
                        FileInfo fs = new(Generic.OpenFilePaths[0]);
                        FS = fs.Length;
                        foreach (string file in Generic.OpenFilePaths)
                        {
                            FileInfo fi = new(file);

                            if (fi.Length >= uint.MaxValue)
                            {
                                MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            else
                            {
                                Filesizes += fi.Length;
                            }
                        }
                    }
                    else
                    {
                        long Filesizes = 0;
                        FileInfo fs = new(files[0]);
                        FS = fs.Length;
                        foreach (string file in files)
                        {
                            FileInfo fi = new(file);

                            if (fi.Length >= uint.MaxValue)
                            {
                                MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            else
                            {
                                Filesizes += fi.Length;
                            }
                        }
                    }



                    string Ft = "";
                    int count = 0, wavcount = 0;
                    List<string> multiextlst = new();

                    foreach (var file in Common.Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);

                        if (count != 0)
                        {
                            if (Ft == ".AT3" || Ft == ".AT9")
                            {
                                if (Ft != fi.Extension.ToUpper())
                                {
                                    MessageBox.Show(this, Localization.FileMixedWithATRACCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    closeFileCToolStripMenuItem.Enabled = false;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    toolStripStatusLabel_EncMethod.Enabled = false;
                                    toolStripStatusLabel_EncMethod.Visible = false;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    return;
                                }
                            }
                            else
                            {
                                if (fi.Extension.ToUpper() == ".AT3" || fi.Extension.ToUpper() == ".AT9")
                                {
                                    MessageBox.Show(this, Localization.FileMixedWithATRACCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    closeFileCToolStripMenuItem.Enabled = false;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    toolStripStatusLabel_EncMethod.Enabled = false;
                                    toolStripStatusLabel_EncMethod.Visible = false;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    return;
                                }
                                if (count == Generic.OpenFilePaths.Length - 1)
                                {
                                    if (wavcount == Generic.OpenFilePaths.Length - 1)
                                    {
                                        if (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                                        {
                                            Ft = ".NOT";
                                        }
                                        else
                                        {
                                            Ft = ".WAV";
                                        }

                                    }
                                    else
                                    {
                                        Ft = ".NOT";
                                    }
                                }
                                else
                                {
                                    if (fi.Extension.ToUpper() == Ft)
                                    {
                                        Ft = fi.Extension.ToUpper();
                                        wavcount++;
                                    }
                                    else if (fi.Extension.ToUpper() != Ft)
                                    {
                                        Ft = fi.Extension.ToUpper();
                                    }
                                    else
                                    {
                                        Ft = fi.Extension.ToUpper();
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (fi.Extension.ToUpper() == ".WAV")
                            {
                                Ft = fi.Extension.ToUpper();
                                multiextlst.Add(file);
                                wavcount++;
                                count++;
                                continue;
                            }
                            else if (fi.Extension.ToUpper() == ".AT3" || fi.Extension.ToUpper() == ".AT9")
                            {
                                Ft = fi.Extension.ToUpper();
                            }
                            else
                            {
                                Ft = fi.Extension.ToUpper();
                                multiextlst.Add(file);
                            }
                        }

                        count++;
                    }

                    ReadStatus();
                    label_Filepath.Text = Generic.OpenFilePaths[0];
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024, FS);

                    closeFileCToolStripMenuItem.Enabled = true;

                    Generic.MultipleLoopStarts = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleLoopEnds = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleFilesLoopOKFlags = new bool[Generic.OpenFilePaths.Length];

                    Generic.ATRACMultiMetadataBuffer = new int[Generic.OpenFilePaths.Length, 3];

                    switch (Ft.ToUpper())
                    {
                        case ".WAV":
                            FormatSorter(true);
                            break;
                        case ".NOT":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            Generic.ReadedATRACFlag = 0;
                            Utils.ReadMetadatasMulti(Generic.OpenFilePaths, Generic.ATRACMultiMetadataBuffer);
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            Generic.ReadedATRACFlag = 1;
                            Utils.ReadMetadatasMulti(Generic.OpenFilePaths, Generic.ATRACMultiMetadataBuffer);
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _isClosing = true;

            bool isBusy = Common.Generic.ProcessFlag != Constants.ProcessType.None
                  && Common.Generic.cts != null
                  && !Common.Generic.cts.IsCancellationRequested;

            if (isBusy)
            {
                // ★まず閉じるのを止める（デッドロック/再帰防止）
                e.Cancel = true;

                if (Interlocked.Exchange(ref _closingCancelIssued, 1) == 0)
                {
                    try { Common.Generic.cts!.Cancel(); } catch { }
                }

                // すぐ戻る。停止完了後に自分で Close する
                return;
            }

            // ★Cancel は一度だけ（再入防止）
            if (Interlocked.Exchange(ref _closingCancelIssued, 1) == 0)
            {
                try
                {
                    var cts = Common.Generic.cts;
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                }
                catch
                {
                    // StackOverflow は catch 不可。ここは通常例外だけ握る
                }
            }

            /*bool Debugmode = Utils.GetBool("Debugmode", false);
            if (Debugmode)
            {
                CloseDebug();
            }*/

            try
            {
                // デバッグウインドウが生きているなら閉じる（同期Invokeは避ける）
                var wd = windowDebug;
                if (wd != null)
                {
                    wd.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try { wd.Close(); } catch { }
                    }));
                }

                // 以後ログを吐かないようにする
                _debugReady.Reset();
                while (_debugMsgQueue.TryDequeue(out _)) { }
            }
            catch { }


            //base.OnFormClosing(e);
        }

        /// <summary>
        /// フォーム終了後の後処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            ActivateOrDeactivateLPC(false);
            Utils.ATWCheck(Generic.IsATW);

            string tempDir = Path.Combine(Directory.GetCurrentDirectory(), "_temp");

            try
            {
                if (Directory.Exists(tempDir))
                {
                    Common.Utils.DeleteDirectoryFiles(tempDir);

                    // 念のため、空でなくても再帰削除したい場合は true を付ける
                    Directory.Delete(tempDir, true);
                }
            }
            catch (Exception ex)
            {
                // 終了処理なので、致命的でない限りログだけに留めるのが無難
                Debug.WriteLine("Failed to clean temp dir: " + ex);
            }

            string ffPath = Path.Combine(Directory.GetCurrentDirectory(), @"res\ffmpeg.exe");

            try
            {
                if (File.Exists(ffPath))
                {
                    File.Delete(ffPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to delete ffmpeg.exe: " + ex);
            }
        }

        /// <summary>
        /// Waveではない音声ファイルをWaveに変換する
        /// </summary>
        private bool AudioToWaveConvert()
        {
            if (walkmanToolStripMenuItem.Checked)
            {
                walkmanToolStripMenuItem.Checked = false;
            }
            if (Common.Generic.IsWave != true && Common.Generic.IsATRAC != true)
            {
                string TempAudioDir;
                if (Directory.Exists(Directory.GetCurrentDirectory() + @"\_tempAudio"))
                {
                    TempAudioDir = Directory.GetCurrentDirectory() + @"\_tempAudio";
                }
                else
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\_tempAudio");
                    TempAudioDir = Directory.GetCurrentDirectory() + @"\_tempAudio";
                }

                if (Common.Generic.OpenFilePaths.Length == 1) // 単一ファイル
                {
                    if (bool.Parse(Config.Entry["FixedConvert"].Value)) // Fix
                    {
                        FileInfo file = new(Common.Generic.OpenFilePaths[0]);
                        Common.Generic.WTAmethod = (Constants.WTAType)Utils.GetInt("ConvertType", 0);//int.Parse(Config.Entry["ConvertType"].Value);

                        //Common.Generic.SavePath = file.Directory + @"\" + file.Name + @".wav";
                        Common.Generic.SavePath = TempAudioDir + @"\" + file.Name.Replace(file.Extension, "") + @".wav";
                        Common.Generic.ProgressMax = 1;

                        Common.Generic.ProcessFlag = Constants.ProcessType.AudioToWave;

                        Form formProgress = new FormProgress();
                        formProgress.ShowDialog();
                        formProgress.Dispose();

                        if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested)
                        {
                            Generic.IsATWCancelled = true;
                            Common.Generic.cts.Dispose();
                            MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ResetStatus();
                            return false;
                        }

                        FileInfo fi = new(Common.Generic.SavePath);
                        if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                        {
                            if (File.Exists(Common.Generic.InputJobs[0].WorkPath))
                            {
                                // UI表示は元ファイル名（ハッシュ無し）
                                label_Filepath.Text = Common.Generic.InputJobs[0].OriginPath;

                                // ★最終出力名は Origin 由来（ハッシュ無し）
                                string originBase = Path.GetFileNameWithoutExtension(Common.Generic.InputJobs[0].OriginPath);
                                string destName = $"{originBase}{Utils.ATWSuffix()}.wav";
                                string destPath = Path.Combine(TempAudioDir, destName);

                                // 同名衝突は (2) 方式で回避（ハッシュは付けない）
                                destPath = Common.Utils.MakeUniquePath(destPath);

                                // _temp の実ファイル（ハッシュ付きでもOK）を移動
                                File.Move(Common.Generic.InputJobs[0].WorkPath, destPath);

                                // WorkPath を最終出力に更新（以後はハッシュ無し名になる）
                                Common.Generic.InputJobs[0].WorkPath = destPath;

                                // OpenFilePaths へ反映（後続処理は WAV を参照する）
                                Common.Generic.SyncPathsFromInputJobs();

                                // _temp 掃除
                                Common.Utils.DeleteDirectoryFiles(Path.Combine(Directory.GetCurrentDirectory(), "_temp"));
                            }
                            else
                            {
                                ResetStatus();
                                MessageBox.Show(Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }

                            switch (Common.Generic.WTAmethod)
                            {
                                case Constants.WTAType.Hz44100:
                                    Config.Entry["ATRAC3_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "0";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 0;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                    aTRAC9ToolStripMenuItem.Checked = false;
                                    toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                    break;
                                case Constants.WTAType.Hz48000:
                                    Config.Entry["ATRAC3_Console"].Value = "1";
                                    Config.Entry["ToolStrip"].Value = "0";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 0;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                    aTRAC9ToolStripMenuItem.Checked = false;
                                    toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                    break;
                                case Constants.WTAType.Hz8000:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                                case Constants.WTAType.Hz12000:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                                case Constants.WTAType.Hz16000:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                                case Constants.WTAType.Hz24000:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                                case Constants.WTAType.Hz32000:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                            }
                            return true;
                        }
                        else // Error
                        {
                            ResetStatus();
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    else // normal
                    {
                        FileInfo file = new(Common.Generic.OpenFilePaths[0]);
                        //using Form formAtWST = new FormAtWSelectTarget();
                        //DialogResult dr = formAtWST.ShowDialog();
                        WindowATWSelectTarget WATWST = new();
                        WpfWindowRegistry.Register(WATWST);
                        bool? dr = WATWST.ShowDialog();

                        //if (dr != DialogResult.Cancel && dr != DialogResult.None)
                        if (dr != false && dr != null)
                        {
                            //Common.Generic.SavePath = file.Directory + @"\" + file.Name + @".wav";
                            Common.Generic.SavePath = TempAudioDir + @"\" + file.Name + @".wav";
                            Common.Generic.ProgressMax = 1;

                            Common.Generic.ProcessFlag = Constants.ProcessType.AudioToWave;

                            Form formProgress = new FormProgress();
                            formProgress.ShowDialog();
                            formProgress.Dispose();

                            if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested)
                            {
                                Generic.IsATWCancelled = true;
                                Common.Generic.cts.Dispose();
                                MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetStatus();
                                return false;
                            }

                            FileInfo fi = new(Common.Generic.SavePath);
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                            {
                                if (File.Exists(Common.Generic.InputJobs[0].WorkPath))
                                {
                                    // UI表示は元ファイル名（ハッシュ無し）
                                    label_Filepath.Text = Common.Generic.InputJobs[0].OriginPath;

                                    // ★最終出力名は Origin 由来（ハッシュ無し）
                                    string originBase = Path.GetFileNameWithoutExtension(Common.Generic.InputJobs[0].OriginPath);
                                    string destName = $"{originBase}{Utils.ATWSuffix()}.wav";
                                    string destPath = Path.Combine(TempAudioDir, destName);

                                    // 同名衝突は (2) 方式で回避（ハッシュは付けない）
                                    destPath = Common.Utils.MakeUniquePath(destPath);

                                    // _temp の実ファイル（ハッシュ付きでもOK）を移動
                                    File.Move(Common.Generic.InputJobs[0].WorkPath, destPath);

                                    // WorkPath を最終出力に更新（以後はハッシュ無し名になる）
                                    Common.Generic.InputJobs[0].WorkPath = destPath;

                                    // OpenFilePaths へ反映（後続処理は WAV を参照する）
                                    Common.Generic.SyncPathsFromInputJobs();

                                    // _temp 掃除
                                    Common.Utils.DeleteDirectoryFiles(Path.Combine(Directory.GetCurrentDirectory(), "_temp"));
                                }
                                else
                                {
                                    ResetStatus();
                                    MessageBox.Show(Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption,
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return false;
                                }

                                switch (Common.Generic.WTAmethod)
                                {
                                    case Constants.WTAType.Hz44100:
                                        Config.Entry["ATRAC3_Console"].Value = "0";
                                        Config.Entry["ToolStrip"].Value = "0";
                                        Config.Save(xmlpath);
                                        Common.Generic.ATRACFlag = 0;
                                        aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                        aTRAC9ToolStripMenuItem.Checked = false;
                                        toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                        break;
                                    case Constants.WTAType.Hz48000:
                                        Config.Entry["ATRAC3_Console"].Value = "1";
                                        Config.Entry["ToolStrip"].Value = "0";
                                        Config.Save(xmlpath);
                                        Common.Generic.ATRACFlag = 0;
                                        aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                        aTRAC9ToolStripMenuItem.Checked = false;
                                        toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                        break;
                                    case Constants.WTAType.Hz12000:
                                        Config.Entry["ATRAC9_Console"].Value = "0";
                                        Config.Entry["ToolStrip"].Value = "1";
                                        Config.Save(xmlpath);
                                        Common.Generic.ATRACFlag = 1;
                                        aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                        aTRAC9ToolStripMenuItem.Checked = true;
                                        toolStripDropDownButton_EF.Text = "ATRAC9";
                                        break;
                                    case Constants.WTAType.Hz24000:
                                        Config.Entry["ATRAC9_Console"].Value = "0";
                                        Config.Entry["ToolStrip"].Value = "1";
                                        Config.Save(xmlpath);
                                        Common.Generic.ATRACFlag = 1;
                                        aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                        aTRAC9ToolStripMenuItem.Checked = true;
                                        toolStripDropDownButton_EF.Text = "ATRAC9";
                                        break;
                                }
                                return true;
                            }
                            else // Error
                            {
                                ResetStatus();
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                        }
                        else
                        {
                            Generic.IsATW = false;
                            ResetStatus();
                            return false;
                        }
                    }
                }
                else // 複数ファイル
                {
                    // 念のため：InputJobs が未構築なら構築する
                    if (Common.Generic.InputJobs.Count == 0)
                    {
                        Common.Generic.BuildInputJobsFromPaths(Common.Generic.OpenFilePaths, Common.Generic.OriginOpenFilePaths);
                    }

                    // 念のため：長さ不一致は即エラー（ズレたまま進む方が危険）
                    if (Common.Generic.InputJobs.Count != Common.Generic.OpenFilePaths.Length)
                    {
                        MessageBox.Show("Internal error: InputJobs and OpenFilePaths length mismatch.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    Generic.IsOpenMulti = true;
                    if (bool.Parse(Config.Entry["FixedConvert"].Value)) // Fix
                    {
                        //FileInfo fp = new(Common.Generic.OpenFilePaths[0]);
                        Common.Generic.WTAmethod = (Constants.WTAType)Utils.GetInt("ConvertType", 0);

                        Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                        Common.Generic.ProcessFlag = Constants.ProcessType.AudioToWave;

                        Form formProgress = new FormProgress();
                        formProgress.ShowDialog();
                        formProgress.Dispose();

                        if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested)
                        {
                            Generic.IsATWCancelled = true;
                            Common.Generic.cts.Dispose();
                            MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ResetStatus();
                            return false;
                        }

                        label_Filepath.Text = Generic.OpenFilePaths[0];

                        for (int i = 0; i < Common.Generic.InputJobs.Count; i++)
                        {
                            // _temp 内の実ファイル（ハッシュ付きでもOK）
                            string tempWav = Common.Generic.InputJobs[i].WorkPath;
                            if (!File.Exists(tempWav))
                            {
                                MessageBox.Show(this, $"Temp wav not found: {tempWav}", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }

                            // ★最終出力名は Origin から作る（ハッシュ無し）
                            string originBase = Path.GetFileNameWithoutExtension(Common.Generic.InputJobs[i].OriginPath);
                            string destName = $"{originBase}{Utils.ATWSuffix()}.wav";  // 例: song_atw.wav
                            string dest = Path.Combine(TempAudioDir, destName);

                            // 同名衝突は "(2)" などで回避（ハッシュは付けない）
                            dest = Common.Utils.MakeUniquePath(dest);

                            File.Move(tempWav, dest);

                            // WorkPath を最終出力へ更新（以後はハッシュ無し名になる）
                            Common.Generic.InputJobs[i].WorkPath = dest;
                        }

                        Generic.SyncPathsFromInputJobs();

                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");

                        switch (Common.Generic.WTAmethod)
                        {
                            case Constants.WTAType.Hz44100:
                                Config.Entry["ATRAC3_Console"].Value = "0";
                                Config.Entry["ToolStrip"].Value = "0";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 0;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                aTRAC9ToolStripMenuItem.Checked = false;
                                toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                break;
                            case Constants.WTAType.Hz48000:
                                Config.Entry["ATRAC3_Console"].Value = "1";
                                Config.Entry["ToolStrip"].Value = "0";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 0;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                aTRAC9ToolStripMenuItem.Checked = false;
                                toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                break;
                            case Constants.WTAType.Hz8000:
                                Config.Entry["ATRAC9_Console"].Value = "0";
                                Config.Entry["ToolStrip"].Value = "1";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 1;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = true;
                                toolStripDropDownButton_EF.Text = "ATRAC9";
                                break;
                            case Constants.WTAType.Hz12000:
                                Config.Entry["ATRAC9_Console"].Value = "0";
                                Config.Entry["ToolStrip"].Value = "1";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 1;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = true;
                                toolStripDropDownButton_EF.Text = "ATRAC9";
                                break;
                            case Constants.WTAType.Hz16000:
                                Config.Entry["ATRAC9_Console"].Value = "0";
                                Config.Entry["ToolStrip"].Value = "1";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 1;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = true;
                                toolStripDropDownButton_EF.Text = "ATRAC9";
                                break;
                            case Constants.WTAType.Hz24000:
                                Config.Entry["ATRAC9_Console"].Value = "0";
                                Config.Entry["ToolStrip"].Value = "1";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 1;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = true;
                                toolStripDropDownButton_EF.Text = "ATRAC9";
                                break;
                            case Constants.WTAType.Hz32000:
                                Config.Entry["ATRAC9_Console"].Value = "0";
                                Config.Entry["ToolStrip"].Value = "1";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 1;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = true;
                                toolStripDropDownButton_EF.Text = "ATRAC9";
                                break;
                        }

                        FileInfo fisize = new(label_Filepath.Text);
                        long FS = fisize.Length;
                        label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024, FS);

                        return true;
                    }
                    else // normal
                    {
                        //FileInfo fp = new(Common.Generic.OpenFilePaths[0]);
                        //using Form formAtWST = new FormAtWSelectTarget();
                        //DialogResult dr = formAtWST.ShowDialog();
                        WindowATWSelectTarget WATWST = new();
                        WpfWindowRegistry.Register(WATWST);
                        bool? dr = WATWST.ShowDialog();

                        //if (dr != DialogResult.Cancel && dr != DialogResult.None)
                        if (dr != false && dr != null)
                        {
                            Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                            Common.Generic.ProcessFlag = Constants.ProcessType.AudioToWave;

                            Form formProgress = new FormProgress();
                            formProgress.ShowDialog();
                            formProgress.Dispose();

                            if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested)
                            {
                                Generic.IsATWCancelled = true;
                                Common.Generic.cts.Dispose();
                                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetStatus();
                                return false;
                            }

                            label_Filepath.Text = Generic.OpenFilePaths[0];

                            for (int i = 0; i < Common.Generic.InputJobs.Count; i++)
                            {
                                // _temp 内の実ファイル（ハッシュ付きでもOK）
                                string tempWav = Common.Generic.InputJobs[i].WorkPath;
                                if (!File.Exists(tempWav))
                                {
                                    MessageBox.Show(this, $"Temp wav not found: {tempWav}", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return false;
                                }

                                // ★最終出力名は Origin から作る（ハッシュ無し）
                                string originBase = Path.GetFileNameWithoutExtension(Common.Generic.InputJobs[i].OriginPath);
                                string destName = $"{originBase}{Utils.ATWSuffix()}.wav";  // 例: song_atw.wav
                                string dest = Path.Combine(TempAudioDir, destName);

                                // 同名衝突は "(2)" などで回避（ハッシュは付けない）
                                dest = Common.Utils.MakeUniquePath(dest);

                                File.Move(tempWav, dest);

                                // WorkPath を最終出力へ更新（以後はハッシュ無し名になる）
                                Common.Generic.InputJobs[i].WorkPath = dest;
                            }
                            Generic.SyncPathsFromInputJobs();

                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");

                            switch (Common.Generic.WTAmethod)
                            {
                                case Constants.WTAType.Hz44100:
                                    Config.Entry["ATRAC3_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "0";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 0;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                    aTRAC9ToolStripMenuItem.Checked = false;
                                    toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                    break;
                                case Constants.WTAType.Hz48000:
                                    Config.Entry["ATRAC3_Console"].Value = "1";
                                    Config.Entry["ToolStrip"].Value = "0";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 0;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                    aTRAC9ToolStripMenuItem.Checked = false;
                                    toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                    break;
                                case Constants.WTAType.Hz8000:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                                case Constants.WTAType.Hz12000:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                                case Constants.WTAType.Hz16000:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                                case Constants.WTAType.Hz24000:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                                case Constants.WTAType.Hz32000:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                            }

                            FileInfo fisize = new(label_Filepath.Text);
                            long FS = fisize.Length;
                            label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024, FS);

                            return true;
                        }
                        else
                        {
                            Generic.IsATW = false;
                            ResetStatus();
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

        /// <summary>
        /// ファイル形式に応じて動作を変更
        /// </summary>
        /// <param name="IsEncode">エンコード対象か否か</param>
        /// <param name="IsNotWave">Waveファイルか否か</param>
        private void FormatSorter(bool IsEncode, bool IsNotWave = false)
        {
            if (IsEncode != false)
            {
                if (IsNotWave != true) // Wave
                {
                    Common.Generic.IsWave = true;
                    Common.Generic.IsATRAC = false;
                    Generic.IsATW = false;
                    ActivateOrDeactivateLPC(true);
                    label_Formattxt.Text = Localization.WAVEFormatCaption;
                    toolStripDropDownButton_EF.Enabled = true;
                    toolStripDropDownButton_EF.Visible = true;
                    toolStripStatusLabel_EncMethod.Enabled = true;
                    toolStripStatusLabel_EncMethod.Visible = true;
                    button_Decode.Enabled = false;
                    button_Encode.Enabled = true;
                    CheckLPCException();
                }
                else // NotWave
                {
                    toolStripStatusLabel_Status.ForeColor = Color.Orange;
                    toolStripStatusLabel_Status.Text = Localization.InitializationCaption;
                    label_Formattxt.Text = Localization.InitializationCaption;
                    Common.Generic.IsWave = false;
                    Common.Generic.IsATRAC = false;
                    Generic.IsATW = true;
                    if (AudioToWaveConvert())
                    {
                        toolStripStatusLabel_Status.ForeColor = Color.Green;
                        toolStripStatusLabel_Status.Text = Localization.ReadyCaption;
                        ActivateOrDeactivateLPC(true);
                        label_Formattxt.Text = Localization.WAVEConvertedFormatCaption;
                        toolStripDropDownButton_EF.Enabled = true;
                        toolStripDropDownButton_EF.Visible = true;
                        toolStripStatusLabel_EncMethod.Enabled = true;
                        toolStripStatusLabel_EncMethod.Visible = true;
                        button_Decode.Enabled = false;
                        button_Encode.Enabled = true;
                        CheckLPCException();
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else // ATRAC
            {
                Config.Load(xmlpath);

                bool faster_atrac = bool.Parse(Config.Entry["FasterATRAC"].Value);
                bool play_atrac = bool.Parse(Config.Entry["PlaybackATRAC"].Value);
                bool encodesource_atrac = bool.Parse(Config.Entry["ATRACEncodeSource"].Value);

                if (faster_atrac && play_atrac)
                {
                    panel_Main.BackgroundImage = Resources.SIEv2;
                    Common.Generic.IsWave = false;
                    Common.Generic.IsATRAC = true;
                    Generic.IsATW = false;
                    toolStripDropDownButton_EF.Enabled = false;
                    toolStripDropDownButton_EF.Visible = false;
                    toolStripStatusLabel_EncMethod.Enabled = false;
                    toolStripStatusLabel_EncMethod.Visible = false;
                    button_Decode.Enabled = true;
                    button_Encode.Enabled = false;
                    loopPointCreationToolStripMenuItem.Enabled = false;
                    button_Decode.PerformClick();
                }
                else if (!faster_atrac && play_atrac)
                {
                    panel_Main.BackgroundImage = Resources.SIEv2;
                    Common.Generic.IsWave = false;
                    Common.Generic.IsATRAC = true;
                    Generic.IsATW = false;
                    toolStripDropDownButton_EF.Enabled = false;
                    toolStripDropDownButton_EF.Visible = false;
                    toolStripStatusLabel_EncMethod.Enabled = false;
                    toolStripStatusLabel_EncMethod.Visible = false;
                    button_Decode.Enabled = true;
                    button_Encode.Enabled = false;
                    loopPointCreationToolStripMenuItem.Enabled = false;
                    if (PlaybackATRACConvert())
                    {
                        Generic.IsPlaybackATRAC = true;
                        ActivateOrDeactivateLPC(true);
                        CheckLPCException();
                    }
                    else
                    {
                        ResetStatus();
                        return;
                    }
                }
                else if (faster_atrac && !play_atrac)
                {
                    panel_Main.BackgroundImage = Resources.SIEv2;
                    Common.Generic.IsWave = false;
                    Common.Generic.IsATRAC = true;
                    Generic.IsATW = false;
                    toolStripDropDownButton_EF.Enabled = false;
                    toolStripDropDownButton_EF.Visible = false;
                    toolStripStatusLabel_EncMethod.Enabled = false;
                    toolStripStatusLabel_EncMethod.Visible = false;
                    button_Decode.Enabled = true;
                    button_Encode.Enabled = false;
                    loopPointCreationToolStripMenuItem.Enabled = false;
                    button_Decode.PerformClick();
                }
                else
                {
                    if (encodesource_atrac) // ATRACをエンコード用ソースとして読み込み
                    {
                        toolStripStatusLabel_Status.Text = Localization.InitializationCaption;
                        label_Formattxt.Text = Localization.InitializationCaption;
                        Common.Generic.IsWave = false;
                        Common.Generic.IsATRAC = true;
                        Generic.IsATW = false;
                        if (PlaybackATRACConvert())
                        {
                            toolStripStatusLabel_Status.ForeColor = Color.Green;
                            toolStripStatusLabel_Status.Text = Localization.ReadyCaption;
                            ActivateOrDeactivateLPC(true);
                            label_Formattxt.Text = Localization.WAVEConvertedFormatCaption;
                            toolStripDropDownButton_EF.Enabled = true;
                            toolStripDropDownButton_EF.Visible = true;
                            toolStripStatusLabel_EncMethod.Enabled = true;
                            toolStripStatusLabel_EncMethod.Visible = true;
                            button_Decode.Enabled = false;
                            button_Encode.Enabled = true;
                            CheckLPCException();
                        }
                        else
                        {
                            ResetStatus();
                            return;
                        }
                    }
                    else
                    {
                        panel_Main.BackgroundImage = Resources.SIEv2;
                        Common.Generic.IsWave = false;
                        Common.Generic.IsATRAC = true;
                        Generic.IsATW = false;
                        toolStripDropDownButton_EF.Enabled = false;
                        toolStripDropDownButton_EF.Visible = false;
                        toolStripStatusLabel_EncMethod.Enabled = false;
                        toolStripStatusLabel_EncMethod.Visible = false;
                        button_Decode.Enabled = true;
                        button_Encode.Enabled = false;
                        loopPointCreationToolStripMenuItem.Enabled = false;
                        if (bool.Parse(Config.Entry["PlaybackATRAC"].Value))
                        {
                            if (PlaybackATRACConvert())
                            {
                                Generic.IsPlaybackATRAC = true;
                                ActivateOrDeactivateLPC(true);
                                CheckLPCException();
                            }
                            else
                            {
                                ResetStatus();
                                return;
                            }
                        }
                    }

                }
            }
        }

        private bool PlaybackATRACConvert()
        {
            string tempDir = Path.Combine(Directory.GetCurrentDirectory(), "_temp");

            if (Common.Generic.OpenFilePaths.Length == 1) // 単一ファイル
            {
                string originKey =
                    (Common.Generic.InputJobs != null && Common.Generic.InputJobs.Count == 1)
                        ? Common.Generic.InputJobs[0].OriginPath
                        : Common.Generic.OpenFilePaths[0];

                // 一意名の ata を使う（単一でも方式統一）
                Common.Generic.pATRACSavePath = Common.Utils.MakeTempUniquePath(tempDir, originKey, 0, ".ata");

                Generic.pATRACOpenFilePaths = new[] { Common.Generic.pATRACSavePath };
                Common.Generic.ProgressMax = 1;
            }
            else // 複数ファイル
            {
                Common.Generic.pATRACFolderSavePath = tempDir;
                Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
            }

            Common.Generic.ProcessFlag = Constants.ProcessType.Decode;

            Form formProgress = new FormProgress();
            formProgress.ShowDialog();
            formProgress.Dispose();

            if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested) // 中断
            {
                Common.Generic.cts.Dispose();
                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Common.Utils.DeleteDirectoryFiles(tempDir);
                return false;
            }
            else
            {
                if (Common.Generic.OpenFilePaths.Length == 1) // 単一
                {
                    FileInfo fi = new(Common.Generic.pATRACSavePath);
                    Common.Generic.cts.Dispose();
                    if (File.Exists(Common.Generic.pATRACSavePath))
                    {
                        if (fi.Length != 0) // OK
                        {
                            return true;
                        }
                        else // Error
                        {
                            File.Delete(Common.Generic.pATRACSavePath);
                            Common.Utils.DeleteDirectoryFiles(tempDir);
                            MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.DecodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    else // Exception
                    {
                        Common.Utils.DeleteDirectoryFiles(tempDir);
                        MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else // 複数
                {
                    Common.Generic.cts.Dispose();

                    // 1) wav -> ata に移動（存在チェック付き）
                    for (int i = 0; i < Common.Generic.OpenFilePaths.Length; i++)
                    {
                        string originKey =
                            (Common.Generic.InputJobs != null && Common.Generic.InputJobs.Count == Common.Generic.OpenFilePaths.Length)
                                ? Common.Generic.InputJobs[i].OriginPath
                                : Common.Generic.OpenFilePaths[i];

                        string tempWav = Common.Utils.MakeTempUniquePath(tempDir, originKey, i, ".wav");
                        string tempAta = Common.Utils.MakeTempUniquePath(tempDir, originKey, i, ".ata");

                        if (!File.Exists(tempWav))
                        {
                            // 変換失敗 or 既に消えた
                            continue;
                        }

                        // 既存があれば上書き回避（念のため）
                        if (File.Exists(tempAta)) File.Delete(tempAta);

                        File.Move(tempWav, tempAta);
                    }

                    // 2) “実在する ata” を検査して pATRACOpenFilePaths を確定
                    var ok = new List<string>();
                    for (int i = 0; i < Common.Generic.OpenFilePaths.Length; i++)
                    {
                        string originKey =
                            (Common.Generic.InputJobs != null && Common.Generic.InputJobs.Count == Common.Generic.OpenFilePaths.Length)
                                ? Common.Generic.InputJobs[i].OriginPath
                                : Common.Generic.OpenFilePaths[i];

                        string tempAta = Common.Utils.MakeTempUniquePath(tempDir, originKey, i, ".ata");

                        if (!File.Exists(tempAta))
                            continue;

                        try
                        {
                            var fi = new FileInfo(tempAta);
                            if (fi.Length > 0)
                            {
                                ok.Add(tempAta);
                            }
                            else
                            {
                                File.Delete(tempAta);
                            }
                        }
                        catch
                        {
                            // 競合（削除済み等）はスキップ
                        }
                    }

                    if (ok.Count == 0)
                    {
                        Common.Utils.DeleteDirectoryFiles(tempDir);
                        MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.DecodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)),
                            Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // 3) pATRACOpenFilePaths を確定（ここで旧方式で作り直さない）
                    Generic.pATRACOpenFilePaths = ok.ToArray();

                    // 表示は “元のファイル名” を使う（tempの一意名は表示しない）
                    label_Filepath.Text = Common.Generic.OpenFilePaths[0];

                    if (ok.Count == Common.Generic.OpenFilePaths.Length)
                    {
                        return true; // 全成功
                    }
                    else
                    {
                        MessageBox.Show(this, Localization.DecodePartialCaption, Localization.MSGBoxWarningCaption,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return true; // 一部成功でも進める設計なら true
                    }
                }
            }
        }

        /// <summary>
        /// LPCの処理
        /// </summary>
        /// <param name="flag">フラグ (true or false)</param>
        private void ActivateOrDeactivateLPC(bool flag)
        {
            if (bool.Parse(Config.Entry["PlaybackATRAC"].Value))
            {
                if (Generic.OpenFilePaths is null) { return; }
                if (flag)
                {
                    FLPC = new(false)
                    {
                        TopLevel = false
                    };
                    panel_Main.Controls.Add(FLPC);
                    FLPC.Show();
                    if (!Generic.IsPlaybackATRAC)
                    {
                        if (Common.Generic.IsWalkman)
                        {
                            FormLPC.FormLPCInstance.LoopCheckEnable = false;
                        }
                        if (!bool.Parse(Config.Entry["DisablePreviewWarning"].Value))
                        {
                            if (!Common.Generic.IsWalkman)
                            {
                                if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                                {
                                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                                    {
                                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3/ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                        MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    else
                                    {
                                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                        MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }

                                }
                                else
                                {
                                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                                    {
                                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                        MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    else
                                    {
                                        if (Generic.lpcreate)
                                        {
                                            FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                        }
                                        else
                                        {
                                            FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                                        }
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        Generic.IsPlaybackATRAC = false;
                    }
                }
                else
                {
                    if (FLPC is not null && FLPC.Visible)
                    {
                        FLPC.Close();
                    }
                }
            }
            else
            {
                if (Generic.OpenFilePaths is null) { return; }
                if (flag)
                {
                    FLPC = new(false)
                    {
                        TopLevel = false
                    };
                    panel_Main.Controls.Add(FLPC);
                    FLPC.Show();

                    if (Common.Generic.IsWalkman)
                    {
                        FormLPC.FormLPCInstance.LoopCheckEnable = false;
                    }
                    if (!bool.Parse(Config.Entry["DisablePreviewWarning"].Value))
                    {
                        if (!Common.Generic.IsWalkman)
                        {
                            if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                            {
                                if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                                {
                                    FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3/ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                    MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                    MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }

                            }
                            else
                            {
                                if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                                {
                                    FormLPC.FormLPCInstance.CautionLabel = "[ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                    MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    if (Generic.lpcreate)
                                    {
                                        FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                    }
                                    else
                                    {
                                        FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                                    }
                                }
                            }
                        }
                    }

                    if (Generic.LPCException)
                    {
                        FLPC.Close();
                    }
                }
                else
                {
                    if (FLPC is not null && FLPC.Visible)
                    {
                        FLPC.Close();
                    }
                }
            }
        }

        private void CheckLPCException()
        {
            if (Generic.LPCException)
            {
                Generic.LPCException = false;
                ResetStatus();
                return;
            }
        }

        public static void SetMetaDatas()
        {
            if (Generic.IsATRAC && Generic.OpenFilePaths.Length == 1)
            {
                int[] loopbuf = new int[2];
                if (Utils.GetATRACLooped(Generic.ATRACMetadataBuffers, loopbuf))
                {
                    Generic.IsATRACLooped = true;
                    FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = true;
                    FormLPC.FormLPCInstance.BufferLoopPosition = loopbuf;
                }
                else
                {
                    Generic.IsATRACLooped = false;
                }
                // UI META Labels
            }
            else if (Generic.IsATRAC && Generic.IsLPCStreamingReloaded || Generic.IsATRAC && Generic.OpenFilePaths.Length > 1)
            {
                uint pos = FormLPC.FormLPCInstance.ButtonPosition;
                //bool looped = Utils.SearchATRACSampleChunk(Generic.pATRACOpenFilePaths[pos - 1]);
                int[] data = new int[2];
                if (Utils.GetATRACLoopedMulti(Generic.pATRACOpenFilePaths, Generic.ATRACMultiMetadataBuffer, data, pos - 1))
                {
                    Generic.IsATRACLooped = true;
                    FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = true;
                    FormLPC.FormLPCInstance.BufferLoopPosition = data;
                }
                else
                {
                    Generic.IsATRACLooped = false;
                }
                // UI META Labels
            }
            else if (!Generic.IsATRAC && Generic.IsLPCStreamingReloaded && Generic.OpenFilePaths.Length > 1)
            {
                uint pos = FormLPC.FormLPCInstance.ButtonPosition;
                // UI Labels (Not META)
            }
            else
            {
                /*label_Filename.Text = "FILE_NAME: " + GetFileName(OriginalPaths[0]);
                label_OrigFilepath.Text = "FILE_PATH: " + OriginalPaths[0];
                label_id.Text = "STREAM_ID: -";
                label_NStream.Text = "NUMBER_OF_STREAMS: -";
                label_Totalstream.Text = "STREAM_TOTAL: " + GetStreamTotals(OpenFilePaths[0]).ToString() + " (FILE_SIZE: " + GetFileSize(OpenFilePaths[0]).ToString() + " )";
                label_lsPosition.Text = "LOOP_START: -";
                label_lePosition.Text = "LOOP_END: -";
                label_Volume_Info.Text = "VOLUME: -";
                label_Channel_Info.Text = "CHANNELS: -";
                label_Samplerate_Info.Text = "SAMPLE_LATES: -";*/
            }
        }

        private void label_NotReaded_Click(object sender, EventArgs e)
        {

        }

        private void EncodeMethodIsATRAC(bool flag)
        {
            switch (flag)
            {
                case true:
                    FLPC?.ATRACRadioButtonChanger(true);
                    Common.Generic.IsWalkman = false;
                    break;
                case false:
                    FLPC?.ATRACRadioButtonChanger(false);
                    Common.Generic.IsWalkman = true;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deencflag">フラグ (true:Encode, false:Decode)</param>
        /// <param name="swit">switch式</param>
        private static void SetWalkmanMultiConvertFormats(bool deencflag, int swit)
        {
            if (!deencflag)
            {
                switch (swit)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (swit)
                {
                    case 0: // PCM
                        Generic.WalkmanMultiConvExt = ".oma";
                        break;
                    case 1: // ATRAC3 OMA
                        Generic.WalkmanMultiConvExt = ".oma";
                        break;
                    case 2: // ATRAC3 OMG
                        Generic.WalkmanMultiConvExt = ".omg";
                        break;
                    case 3: // ATRAC3 AL
                        Generic.WalkmanMultiConvExt = ".oma";
                        break;
                    case 4: // ATRAC3 KDR
                        Generic.WalkmanMultiConvExt = ".kdr";
                        break;
                    case 5: // ATRAC3+ OMA
                        Generic.WalkmanMultiConvExt = ".oma";
                        break;
                    case 6: // ATRAC3+ OMG
                        Generic.WalkmanMultiConvExt = ".omg";
                        break;
                    case 7: // ATRAC3+ AL
                        Generic.WalkmanMultiConvExt = ".oma";
                        break;
                    case 8 - 9: // ATRAC3+ KDR
                        Generic.WalkmanMultiConvExt = ".kdr";
                        break;
                    default:
                        break;
                }
            }
        }

        private void TextBox_LoopStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }

        }

        private void TextBox_LoopEnd_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }

        }

        private void TextBox_LoopStart_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_LoopStart.Text))
            {
                return;
            }
            if (!Generic.IsLPCStreamingReloaded)
            {
                if (FormLPC.FormLPCInstance.TotalSamples < int.Parse(textBox_LoopStart.Text))
                {
                    textBox_LoopStart.Text = textBox_LoopStart.Text.Remove(textBox_LoopStart.TextLength - 1);
                    return;
                }
            }

            switch (FormLPC.FormLPCInstance.SampleRate)
            {
                case 8000: // ATRAC9 Only
                    FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 8.0, MidpointRounding.AwayFromZero);//value[0];
                    FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                    break;
                case 12000: // ATRAC9 Only
                    FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 12.0, MidpointRounding.AwayFromZero);//value[0];
                    FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                    break;
                case 16000: // ATRAC9 Only
                    FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 16.0, MidpointRounding.AwayFromZero);//value[0];
                    FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                    break;
                case 24000: // ATRAC9 Only
                    FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 24.0, MidpointRounding.AwayFromZero);//value[0];
                    FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                    break;
                case 32000: // ATRAC9 Only
                    FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 32.0, MidpointRounding.AwayFromZero);//value[0];
                    FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                    break;
                case 44100:
                    FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 44.1, MidpointRounding.AwayFromZero);//value[0];
                    FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                    break;
                case 48000:
                    FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 48.0, MidpointRounding.AwayFromZero);//value[0];
                    FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                    break;
                default:
                    break;
            }
            FormLPC.FormLPCInstance.customTrackBar_Start.Invalidate();
        }

        private void TextBox_LoopEnd_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_LoopEnd.Text))
            {
                return;
            }
            if (!Generic.IsLPCStreamingReloaded)
            {
                if (FormLPC.FormLPCInstance.TotalSamples < int.Parse(textBox_LoopEnd.Text))
                {
                    textBox_LoopEnd.Text = textBox_LoopEnd.Text.Remove(textBox_LoopEnd.TextLength - 1);
                    return;
                }
            }

            switch (FormLPC.FormLPCInstance.SampleRate)
            {
                case 8000: // ATRAC9 Only
                    FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 8.0, MidpointRounding.AwayFromZero);//value[1];
                    FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                    break;
                case 12000: // ATRAC9 Only
                    FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 12.0, MidpointRounding.AwayFromZero);//value[1];
                    FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                    break;
                case 16000: // ATRAC9 Only
                    FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 16.0, MidpointRounding.AwayFromZero);//value[1];
                    FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                    break;
                case 24000: // ATRAC9 Only
                    FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 24.0, MidpointRounding.AwayFromZero);//value[1];
                    FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                    break;
                case 32000: // ATRAC9 Only
                    FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 32.0, MidpointRounding.AwayFromZero);//value[1];
                    FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                    break;
                case 44100:
                    FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 44.1, MidpointRounding.AwayFromZero);//value[1];
                    FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                    break;
                case 48000:
                    FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 48.0, MidpointRounding.AwayFromZero);//value[1];
                    FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                    break;
                default:
                    break;
            }
            FormLPC.FormLPCInstance.customTrackBar_End.Invalidate();
        }
    }
}