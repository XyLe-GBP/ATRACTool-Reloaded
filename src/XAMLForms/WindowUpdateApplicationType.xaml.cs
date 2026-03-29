using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ATRACTool_Reloaded
{
    /// <summary>
    /// WindowUpdateApplicationType.xaml の相互作用ロジック
    /// </summary>
    public partial class WindowUpdateApplicationType : Window
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

        public WindowUpdateApplicationType()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            Title = Localizable.XAMLWindowLocalization.WindowUpdateApplicationType_Title;

            comboBox_type.Items.Clear();
            comboBox_type.Items.Add(Localizable.XAMLWindowLocalization.WindowUpdateApplicationType_Combobox_Release);
            comboBox_type.Items.Add(Localizable.XAMLWindowLocalization.WindowUpdateApplicationType_Combobox_Portable);
            comboBox_type.SelectedIndex = 0;

            label_Type.Content = Localizable.XAMLWindowLocalization.WindowUpdateApplicationType_Type;

            Button_Cancel.Content = Localizable.XAMLWindowLocalization.CancelCaption;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_type.SelectedIndex != 1)
            {
                Common.Generic.ApplicationPortable = false;
            }
            else
            {
                Common.Generic.ApplicationPortable = true;
            }
            DialogResult = true;
            Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
