using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ATRACTool_Reloaded
{
    /// <summary>
    /// WindowAbout.xaml の相互作用ロジック
    /// </summary>
    public partial class WindowAbout : Window
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

        public WindowAbout()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                BitmapImage img = new(new Uri("https://avatars.githubusercontent.com/u/59692068?v=4", UriKind.RelativeOrAbsolute));
                Image_GitHub.Source = img;
            }
        }

        private static void OpenUrl(string uri)
        {
            if (uri == null) return;

            Uri uricheck = new(uri);
            if (uricheck.Scheme != Uri.UriSchemeHttp && uricheck.Scheme != Uri.UriSchemeHttps) return;

            Process.Start(new ProcessStartInfo(uri)
            {
                UseShellExecute = true
            });
        }

        private void HL_GitHub_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            OpenUrl(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void HL_Website_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            OpenUrl(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void HL_Donate_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            OpenUrl(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
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
}
