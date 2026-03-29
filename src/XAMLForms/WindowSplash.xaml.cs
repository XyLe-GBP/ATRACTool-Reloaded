using System.Drawing.Imaging;
using System.IO;
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

        public WindowSplash()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            Config.Load(Common.xmlpath);
            Screen screen = null!;
            
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
                            Bitmap bimg = new(Properties.Resources.Splash);
                            Bitmap cimg = new(Utils.GetString("SplashImage_Path"));
                            using Graphics g = Graphics.FromImage(cimg);
                            g.DrawImage(bimg, 0, 0, cimg.Width, cimg.Height);
                            Width = cimg.Width;
                            Height = cimg.Height;
                            Left = (screen.Bounds.Width - Width) / 2;
                            Top = (screen.Bounds.Height - Height) / 2;

                            image.Source = BIMG.ToBitmapImage(cimg);
                            break;
                        }
                    case false:
                        {
                            ProgressBar_log.Margin = new Thickness(0, 455 - 80, 0, 0);
                            TextBlock_Log.Margin = new Thickness(0, 451 - 80, 0, 0);

                            Bitmap bimg = new(Properties.Resources.Splash);
                            Bitmap cimg = new(Properties.Resources.Splash_SIE_Default);
                            using Graphics g = Graphics.FromImage(cimg);
                            g.DrawImage(bimg, 0, 0, cimg.Width, cimg.Height);
                            Width = cimg.Width;
                            Height = cimg.Height;
                            Left = (screen.Bounds.Width - Width) / 2;
                            Top = (screen.Bounds.Height - Height) / 2;

                            image.Source = BIMG.ToBitmapImage(cimg);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                ProgressBar_log.Margin = new Thickness(0, 455 - 80, 0, 0);
                TextBlock_Log.Margin = new Thickness(0, 451 - 80, 0, 0);

                Generic.GlobalException = ex;
                Bitmap bimg = new(Properties.Resources.Splash);
                Bitmap cimg = new(Properties.Resources.Splash_SIE_Default);
                using Graphics g = Graphics.FromImage(cimg);
                g.DrawImage(bimg, 0, 0, cimg.Width, cimg.Height);
                Width = cimg.Width;
                Height = cimg.Height;
                Left = (screen.Bounds.Width - Width) / 2;
                Top = (screen.Bounds.Height - Height) / 2;

                image.Source = BIMG.ToBitmapImage(cimg);
            }
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
