using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static ATRACTool_Reloaded.FormMain;
using Brushes = System.Windows.Media.Brushes;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        const int GWL_STYLE = -16;
        const int WS_SYSMENU = 0x80000;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            _windowHandle = handle;
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
        private const double BackgroundOpacity = 0.5;
        private static readonly Uri DefaultBackgroundImageUri = new("pack://application:,,,/Properties/SIE_Default.png", UriKind.Absolute);

        private enum DebugLogFilter
        {
            Info = 0,
            Warn = 1,
            Error = 2,
            All = 3
        }

        private readonly Queue<FormMain.DebugLogEntry> _logEntries = new(MaxLogLines);
        private DebugLogFilter _currentFilter = DebugLogFilter.All;
        private bool _loadingOptions;

        public static bool DebugFunctionsEnabled { get; private set; } = true;

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
        private HwndSource? _hwndSource;

        public WindowDebug()
        {
            InitializeComponent();

            _timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
        }

        private IntPtr _lastMainHandle;
        private IntPtr _windowHandle;

        public bool PlaceBehindMain(IntPtr mainHandle)
        {
            _lastMainHandle = mainHandle;

            IntPtr debugHandle = _windowHandle;
            if (debugHandle == IntPtr.Zero && Dispatcher.CheckAccess())
            {
                debugHandle = new WindowInteropHelper(this).Handle;
                _windowHandle = debugHandle;
            }

            if (debugHandle == IntPtr.Zero)
            {
                if (!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished)
                {
                    Dispatcher.BeginInvoke(new Action(() => PlaceBehindMain(mainHandle)));
                }

                return false;
            }

            if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
            {
                return false;
            }

            IntPtr insertAfter = mainHandle != IntPtr.Zero ? mainHandle : new IntPtr(1);
            return SetWindowPos(debugHandle, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            _hwndSource?.AddHook(WndProc);

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
            LoadDebugOptions();
            if (_lastMainHandle != IntPtr.Zero)
            {
                PlaceBehindMain(_lastMainHandle);
            }

            _timer?.Start();
        }

        private void CheckBox_DebugFunc_Changed(object sender, RoutedEventArgs e)
        {
            DebugFunctionsEnabled = checkBox_DebugFunc.IsChecked == true;
            FormMain.DebugInfo(DebugFunctionsEnabled
                ? "[WindowDebug] Debug functions enabled."
                : "[WindowDebug] Debug functions disabled.");
        }

        private void ComboBox_LogFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (richText_Message == null)
            {
                return;
            }

            _currentFilter = comboBox_LogFilter.SelectedIndex switch
            {
                0 => DebugLogFilter.Info,
                1 => DebugLogFilter.Warn,
                2 => DebugLogFilter.Error,
                _ => DebugLogFilter.All
            };

            RebuildLogDocument();
            FormMain.DebugInfo($"[WindowDebug] Log filter changed. filter={_currentFilter}");
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
            if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
            {
                return;
            }

            var snap = FormMain.GetDebugHandleSnapshotSafe();

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
            _logEntries.Enqueue(entry);

            while (_logEntries.Count > MaxLogLines)
            {
                FormMain.DebugLogEntry oldEntry = _logEntries.Dequeue();
                if (MatchesFilter(oldEntry))
                    RemoveFirstLogBlock();
            }

            if (!MatchesFilter(entry))
            {
                return;
            }

            AppendLogBlock(entry, scrollToEnd: true);
        }

        private void RemoveFirstLogBlock()
        {
            FlowDocument? document = richText_Message.Document;
            Block? firstBlock = document?.Blocks.FirstBlock;
            if (document is null || firstBlock is null)
                return;

            if (firstBlock is Paragraph paragraph)
                paragraph.Inlines.Clear();

            document.Blocks.Remove(firstBlock);
        }

        private bool MatchesFilter(FormMain.DebugLogEntry entry)
        {
            return _currentFilter switch
            {
                DebugLogFilter.Info => entry.Level == FormMain.DebugLogLevel.Info,
                DebugLogFilter.Warn => entry.Level == FormMain.DebugLogLevel.Warn,
                DebugLogFilter.Error => entry.Level == FormMain.DebugLogLevel.Error,
                _ => true
            };
        }

        private void RebuildLogDocument()
        {
            var doc = richText_Message.Document ??= new FlowDocument();
            while (doc.Blocks.FirstBlock is Block firstBlock)
            {
                if (firstBlock is Paragraph paragraph)
                {
                    paragraph.Inlines.Clear();
                }

                doc.Blocks.Remove(firstBlock);
            }

            foreach (FormMain.DebugLogEntry entry in _logEntries)
            {
                if (MatchesFilter(entry))
                {
                    AppendLogBlock(entry, scrollToEnd: false);
                }
            }

            richText_Message.ScrollToEnd();
        }

        private void AppendLogBlock(FormMain.DebugLogEntry entry, bool scrollToEnd)
        {
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
                Block? firstBlock = doc.Blocks.FirstBlock;
                if (firstBlock is null)
                    break;

                if (firstBlock is Paragraph paragraph)
                    paragraph.Inlines.Clear();

                doc.Blocks.Remove(firstBlock);
            }

            if (scrollToEnd)
            {
                richText_Message.ScrollToEnd();
            }
        }

        private void LoadDebugOptions()
        {
            _loadingOptions = true;
            try
            {
                checkBox_BackgroundImage.IsChecked = Common.Utils.GetBool("WindowDebug_BackgroundImage", false);
                textBox_BackgroundImage.Text = Common.Utils.GetString("WindowDebug_BackgroundImage_Path", string.Empty);
                UpdateBackgroundOptionControls();
                ApplyBackgroundImage();
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[WindowDebug] Failed to load debug options. error={ex.Message}");
                ApplyDefaultBackgroundImage();
            }
            finally
            {
                _loadingOptions = false;
            }
        }

        private void CheckBox_BackgroundImage_Changed(object sender, RoutedEventArgs e)
        {
            if (_loadingOptions)
            {
                UpdateBackgroundOptionControls();
                return;
            }

            if (checkBox_BackgroundImage.IsChecked == true && string.IsNullOrWhiteSpace(textBox_BackgroundImage.Text))
            {
                if (!TrySelectBackgroundImage())
                {
                    _loadingOptions = true;
                    checkBox_BackgroundImage.IsChecked = false;
                    _loadingOptions = false;
                }
            }

            UpdateBackgroundOptionControls();
            SaveBackgroundImageSettings();
            ApplyBackgroundImage();
        }

        private void Button_BackgroundBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (!TrySelectBackgroundImage())
            {
                return;
            }

            _loadingOptions = true;
            checkBox_BackgroundImage.IsChecked = true;
            _loadingOptions = false;

            UpdateBackgroundOptionControls();
            SaveBackgroundImageSettings();
            ApplyBackgroundImage();
        }

        private void Button_BackgroundClear_Click(object sender, RoutedEventArgs e)
        {
            textBox_BackgroundImage.Clear();
            _loadingOptions = true;
            checkBox_BackgroundImage.IsChecked = false;
            _loadingOptions = false;

            UpdateBackgroundOptionControls();
            SaveBackgroundImageSettings();
            ApplyBackgroundImage();
            FormMain.DebugInfo("[WindowDebug] Custom background image cleared.");
        }

        private bool TrySelectBackgroundImage()
        {
            OpenFileDialog dialog = new()
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|All Files|*.*",
                Title = "Select background image"
            };

            if (!string.IsNullOrWhiteSpace(textBox_BackgroundImage.Text))
            {
                string currentPath = textBox_BackgroundImage.Text;
                string? currentDirectory = Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrWhiteSpace(currentDirectory) && Directory.Exists(currentDirectory))
                {
                    dialog.InitialDirectory = currentDirectory;
                }
            }

            bool? result = dialog.ShowDialog(this);
            if (result != true)
            {
                return false;
            }

            textBox_BackgroundImage.Text = dialog.FileName;
            FormMain.DebugInfo($"[WindowDebug] Custom background image selected. path={dialog.FileName}");
            return true;
        }

        private void UpdateBackgroundOptionControls()
        {
            bool enabled = checkBox_BackgroundImage.IsChecked == true;
            textBox_BackgroundImage.IsEnabled = enabled;
            button_BackgroundBrowse.IsEnabled = enabled;
            button_BackgroundClear.IsEnabled = enabled && !string.IsNullOrWhiteSpace(textBox_BackgroundImage.Text);
        }

        private void SaveBackgroundImageSettings()
        {
            try
            {
                bool enabled = checkBox_BackgroundImage.IsChecked == true;
                Common.Config.Entry["WindowDebug_BackgroundImage"].Value = enabled.ToString().ToLowerInvariant();
                Common.Config.Entry["WindowDebug_BackgroundImage_Path"].Value = textBox_BackgroundImage.Text.Trim();
                Common.Config.Save(Common.xmlpath);
                FormMain.DebugInfo($"[WindowDebug] Background image settings saved. enabled={enabled}");
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[WindowDebug] Failed to save background image settings. error={ex.Message}");
            }
        }

        private void ApplyBackgroundImage()
        {
            bool enabled = checkBox_BackgroundImage.IsChecked == true;
            string imagePath = textBox_BackgroundImage.Text.Trim();
            if (!enabled)
            {
                ApplyDefaultBackgroundImage();
                return;
            }

            if (!File.Exists(imagePath))
            {
                FormMain.DebugWarn($"[WindowDebug] Custom background image not found. path={imagePath}");
                ApplyDefaultBackgroundImage();
                return;
            }

            try
            {
                tabControl_Root.Background = new ImageBrush(LoadBitmapImage(imagePath))
                {
                    Opacity = BackgroundOpacity,
                    Stretch = Stretch.UniformToFill
                };
            }
            catch (Exception ex)
            {
                FormMain.DebugError($"[WindowDebug] Failed to apply custom background image. path={imagePath}, error={ex.Message}");
                ApplyDefaultBackgroundImage();
            }
        }

        private void ApplyDefaultBackgroundImage()
        {
            tabControl_Root.Background = new ImageBrush(new BitmapImage(DefaultBackgroundImageUri))
            {
                Opacity = BackgroundOpacity,
                Stretch = Stretch.UniformToFill
            };
        }

        private static BitmapImage LoadBitmapImage(string path)
        {
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private void Timer_Tick(object? sender, EventArgs e)
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
            _timer?.Stop();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_timer is not null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                _timer = null;
            }

            _hwndSource?.RemoveHook(WndProc);
            _hwndSource = null;
            _logEntries.Clear();
        }
    }
}
