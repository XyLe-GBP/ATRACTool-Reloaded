using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static ATRACTool_Reloaded.FormMain;
using Brushes = System.Windows.Media.Brushes;

namespace ATRACTool_Reloaded
{
    /// <summary>
    /// WindowDebug.xaml の相互作用ロジック
    /// </summary>
    public partial class WindowDebug : Window
    {
        #region "最大化・最小化・閉じるボタンの非表示設定"

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int GWL_STYLE = -16;
        const int WS_SYSMENU = 0x80000;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(handle, GWL_STYLE);
            style = style & (~WS_SYSMENU);
            SetWindowLong(handle, GWL_STYLE, style);
        }

        #endregion
        const int WM_SYSKEYDOWN = 0x0104;
        const int VK_F4 = 0x73;
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_CLOSE = 0xF060;

        private const int MaxLogLines = 2000;

        private static WindowDebug _WindowDebugInstance = null!;
        public static WindowDebug WindowDebugInstance
        {
            get
            {
                return _WindowDebugInstance;
            }
            set
            {
                _WindowDebugInstance = value;
            }
        }

        private DispatcherTimer? _timer;

        public WindowDebug()
        {
            InitializeComponent();

            _timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (_, __) => RefleshCurrentInstanceInfo();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            FileVersionInfo ver = FileVersionInfo.GetVersionInfo(System.Windows.Forms.Application.ExecutablePath);
            if (ver.FileVersion != null)
            {
                label_Version.Content = "ATRACTool-Reloaded ( build: " + ver.FileVersion.ToString() + " ) [Win Application]";
            }
            else
            {
                label_Version.Content = "ATRACTool-Reloaded ( build: 0.0.0.0 ) [Win Application]";
            }

            RefleshCurrentInstanceInfo();

            _timer?.Start();
        }

        /*public void RefleshCurrentInstanceInfo()
        {
            var snap = FormMain.GetDebugHandleSnapshot();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                label_MainInstance.Content = $"PID: {snap.ProcessId}  CPU: {snap.CpuPercent:0.0}%";

                var sb = new System.Text.StringBuilder();
                foreach (var fi in snap.Forms)
                {
                    sb.AppendLine($"{fi.Name}: {fi.Hwnd}  (PID:{fi.Pid} / TID:{fi.Tid})  " +
                                  $"Visible:{fi.Visible} Enabled:{fi.Enabled} State:{fi.WindowState}");
                }

                textBlock_SubInstances.Text = sb.ToString();
            }));
        }*/

        public void RefleshCurrentInstanceInfo()
        {
            var snap = FormMain.GetDebugHandleSnapshot();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // ---- WinForms情報 ----
                label_MainInstance.Content = $"PID: {snap.ProcessId}  CPU: {snap.CpuPercent:0.0}%";

                var sb = new System.Text.StringBuilder();

                sb.AppendLine("=== WinForms ===");
                foreach (var fi in snap.Forms)
                {
                    sb.AppendLine($"{fi.Name}: {fi.Hwnd}  (PID:{fi.Pid} / TID:{fi.Tid})  " +
                                  $"Visible:{fi.Visible} Enabled:{fi.Enabled} State:{fi.WindowState}");
                }

                // ---- WPF情報 ----
                sb.AppendLine();
                sb.AppendLine("=== WPF ===");

                var wpfInfos = WpfBootstrap.Invoke(() =>
                {
                    var app = System.Windows.Application.Current;
                    if (app == null) return new List<DebugFormInfo>();

                    return app.Windows
                        .OfType<Window>()
                        .Select(MakeWpfWindowInfo)
                        .ToList();
                });

                if (wpfInfos != null)
                {
                    foreach (var w in wpfInfos)
                    {
                        if (w == null) continue;
                        var wi = w;

                        sb.AppendLine($"{wi.Name}: {wi.Hwnd}  (PID:{wi.Pid} / TID:{wi.Tid})  " +
                                      $"Visible:{wi.Visible} Enabled:{wi.Enabled} State:{wi.WindowState}");
                    }
                }
                else
                {
                    sb.AppendLine("Application.Current == null (WPF Application 未初期化)");
                }

                textBlock_SubInstances.Text = sb.ToString();
            }));
        }

        public void AppendLog(FormMain.DebugLogEntry entry)
        {
            // RichTextBox の Document を使う
            var doc = richText_Message.Document ??= new FlowDocument();

            // 1行=1 Paragraph にする（行ごとの色分けが容易）
            var p = new Paragraph
            {
                Margin = new Thickness(0)
            };

            // タイムスタンプ（例: 2025-12-14 16:12:34.123）
            string ts = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            p.Inlines.Add(new Run($"[{ts}] "));

            // レベル表示
            string levelText = entry.Level.ToString().ToUpperInvariant();
            var levelRun = new Run($"[{levelText}] ")
            {
                Foreground = entry.Level switch
                {
                    FormMain.DebugLogLevel.Warn => Brushes.DarkOrange,
                    FormMain.DebugLogLevel.Error => Brushes.Red,
                    _ => Brushes.DodgerBlue,
                },
                FontWeight = FontWeights.SemiBold
            };
            p.Inlines.Add(levelRun);

            // 本文
            var msgRun = new Run(entry.Message)
            {
                Foreground = Brushes.Black
            };
            p.Inlines.Add(msgRun);

            doc.Blocks.Add(p);

            // 行数制限（増えすぎ防止）
            while (doc.Blocks.Count > MaxLogLines)
            {
                doc.Blocks.Remove(doc.Blocks.FirstBlock);
            }

            richText_Message.ScrollToEnd();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            RefleshCurrentInstanceInfo();
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if ((msg == WM_SYSKEYDOWN) &&
                (wParam.ToInt32() == VK_F4))
            {
                handled = true;
            }
            if ((msg == WM_SYSCOMMAND) &&
                (wParam.ToInt32() == SC_CLOSE))
            {
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _timer?.Stop();
        }
    }
}
