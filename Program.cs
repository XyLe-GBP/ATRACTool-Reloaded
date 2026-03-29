using System.IO;
using System.Windows.Threading;

namespace ATRACTool_Reloaded
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = "ATRACTool_Reloaded";
            System.Threading.Mutex mutex = new(false, mutexName);

            bool hasHandle = false;
            try
            {
                try
                {
                    hasHandle = mutex.WaitOne(0, false);
                }
                catch (System.Threading.AbandonedMutexException)
                {
                    hasHandle = true;
                }
                if (hasHandle == false)
                {
                    MessageBox.Show("Multiple launch of applications is not allowed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + @"\res\psp_at3tool.exe"))
                {
                    MessageBox.Show("The required file 'psp_at3tool.exe' does not exist.\nClose the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + @"\res\ps3_at3tool.exe"))
                {
                    MessageBox.Show("The required file 'ps3_at3tool.exe' does not exist.\nClose the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + @"\res\psv_at9tool.exe"))
                {
                    MessageBox.Show("The required file 'psv_at9tool.exe' does not exist.\nClose the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + @"\res\ps4_at9tool.exe"))
                {
                    MessageBox.Show("The required file 'ps4_at9tool.exe' does not exist.\nClose the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + @"\res\updater.exe"))
                {
                    MessageBox.Show("The required file 'updater.exe' does not exist.\nClose the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                WpfBootstrap.Ensure();
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                ApplicationConfiguration.Initialize();
                Application.Run(new FormMain());
            }
            finally
            {
                if (hasHandle)
                {
                    mutex.ReleaseMutex();
                }
                mutex.Close();
            }
        }
    }

    internal static class WpfBootstrap
    {
        private static bool _initialized;
        public static System.Windows.Application? App { get; private set; }
        public static Dispatcher? Dispatcher { get; private set; }

        public static void Ensure()
        {
            if (_initialized) return;

            // ここは WinForms の UI スレッド (STA) で呼ぶ前提
            App = System.Windows.Application.Current ?? new System.Windows.Application
            {
                ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown
            };

            Dispatcher = App.Dispatcher;
            _initialized = true;
        }

        public static void Invoke(Action action)
        {
            if (Dispatcher == null)
                throw new InvalidOperationException("WPF Dispatcher が初期化されていません。WpfBootstrap.Ensure() を先に呼んでください。");

            if (Dispatcher.CheckAccess())
                action();
            else
                Dispatcher.Invoke(action);
        }

        public static T Invoke<T>(Func<T> func)
        {
            if (Dispatcher == null)
                throw new InvalidOperationException("WPF Dispatcher が初期化されていません。WpfBootstrap.Ensure() を先に呼んでください。");

            return Dispatcher.CheckAccess() ? func() : Dispatcher.Invoke(func);
        }
    }
}