using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    /// <summary>
    /// WindowATWSelectTarget.xaml の相互作用ロジック
    /// </summary>
    public partial class WindowATWSelectTarget : Window
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

        public WindowATWSelectTarget()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            Title = Localizable.XAMLWindowLocalization.WindowATWSelectTarget_Title;
            label_Output.Content = Localizable.XAMLWindowLocalization.WindowATWSelectTarget_OutputMethod;
            Button_Cancel.Content = Localizable.XAMLWindowLocalization.CancelCaption;

            Generic.WTAmethod = 0;
            combobox_Method.SelectedIndex = 5;
            combobox_Method.Items.Add(Localizable.XAMLWindowLocalization.Target8kHzCaption);
            combobox_Method.Items.Add(Localizable.XAMLWindowLocalization.Target12kHzCaption);
            combobox_Method.Items.Add(Localizable.XAMLWindowLocalization.Target16kHzCaption);
            combobox_Method.Items.Add(Localizable.XAMLWindowLocalization.Target24kHzCaption);
            combobox_Method.Items.Add(Localizable.XAMLWindowLocalization.Target32kHzCaption);
            combobox_Method.Items.Add(Localizable.XAMLWindowLocalization.Target44kHzCaption);
            combobox_Method.Items.Add(Localizable.XAMLWindowLocalization.Target48kHzCaption);
        }

        private void Combobox_Method_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Generic.WTAmethod = combobox_Method.SelectedIndex switch
            {
                0 => Constants.WTAType.Hz8000,
                1 => Constants.WTAType.Hz12000,
                2 => Constants.WTAType.Hz16000,
                3 => Constants.WTAType.Hz24000,
                4 => Constants.WTAType.Hz32000,
                5 => Constants.WTAType.Hz44100,
                6 => Constants.WTAType.Hz48000,
                _ => Constants.WTAType.Hz44100,
            };
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
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
