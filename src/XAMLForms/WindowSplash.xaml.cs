using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    /// <summary>
    /// WindowSplash.xaml の相互作用ロジック
    /// </summary>
    public partial class WindowSplash : Window
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
        private const double DesignedSplashWidth = 800d;
        private const double DesignedSplashHeight = 400d;
        private const double MaxScreenCoverage = 0.9d;

        public WindowSplash()
        {
            InitializeComponent();
            FormMain.DebugInfo("[WindowSplash] Initialized.");
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            FormMain.DebugInfo("[WindowSplash] Load started.");
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            FileVersionInfo ver = FileVersionInfo.GetVersionInfo(System.Windows.Forms.Application.ExecutablePath);
            TextBlock_Version.Text = "Version " + (ver.FileVersion ?? "0.0.0.0");

            Config.Load(Common.xmlpath);
            Screen? screen = null;
            
            try
            {
                if (Screen.PrimaryScreen is not null)
                {
                    screen = Screen.PrimaryScreen;
                }
                else
                {
                    throw new NullReferenceException("PrimaryScreen is null.");
                }

                switch (Utils.GetBool("SplashImage", false))
                {
                    case true:
                        {
                            string splashImagePath = Utils.GetString("SplashImage_Path");
                            FormMain.DebugInfo($"[WindowSplash] Loading custom splash image. path={splashImagePath}");
                            using Bitmap cimg = new(splashImagePath);
                            ApplySplashImage(cimg, screen, "custom");
                            break;
                        }
                    case false:
                        {
                            FormMain.DebugInfo("[WindowSplash] Loading default splash image.");

                            using Bitmap cimg = new(Properties.Resources.SIE_White);
                            ApplySplashImage(cimg, screen, "default");

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                FormMain.DebugWarn($"[WindowSplash] Splash load failed. Falling back to default. error={ex.Message}");

                Generic.GlobalException = ex;
                using Bitmap cimg = new(Properties.Resources.SIE_White);
                ApplySplashImage(cimg, screen ?? Screen.PrimaryScreen, "default-fallback");
            }
            FormMain.DebugInfo("[WindowSplash] Load completed.");
        }

        private void ApplySplashImage(Bitmap bitmap, Screen? screen, string source)
        {
            SetSplashWindowBounds(screen);
            image.Source = BIMG.ToBitmapImage(bitmap);
            FormMain.DebugInfo($"[WindowSplash] Splash image applied. source={source}, imageSize={bitmap.Width}x{bitmap.Height}, windowSize={Width:0}x{Height:0}");
        }

        private void SetSplashWindowBounds(Screen? screen)
        {
            double scale = 1d;

            if (screen is not null)
            {
                Rectangle area = screen.WorkingArea;
                scale = Math.Min(1d, Math.Min(area.Width * MaxScreenCoverage / DesignedSplashWidth, area.Height * MaxScreenCoverage / DesignedSplashHeight));
                if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0d)
                {
                    scale = 1d;
                }

                Width = Math.Max(1d, DesignedSplashWidth * scale);
                Height = Math.Max(1d, DesignedSplashHeight * scale);
                Left = area.Left + (area.Width - Width) / 2d;
                Top = area.Top + (area.Height - Height) / 2d;
                return;
            }

            Width = DesignedSplashWidth;
            Height = DesignedSplashHeight;
            Left = 0d;
            Top = 0d;
        }

        public string ProgressMsg
        {
            set
            {
                TextBlock_Log.Text = value;
            }
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
    }

    internal static class BIMG
    {
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
