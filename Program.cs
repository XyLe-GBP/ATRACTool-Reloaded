using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace ATRACTool_Reloaded
{
    internal static class Program
    {
        private const string SingleInstanceMutexName = "ATRACTool_Reloaded";
        private const string RestartParentPidEnvironmentVariable = "ATRACTOOL_RESTART_PARENT_PID";
        private static readonly TimeSpan RestartMutexWaitTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Threading.Mutex mutex = new(false, SingleInstanceMutexName);

            bool hasHandle = false;
            try
            {
                try
                {
                    TimeSpan mutexWaitTimeout = IsRestartLaunch() ? RestartMutexWaitTimeout : TimeSpan.Zero;
                    hasHandle = mutex.WaitOne(mutexWaitTimeout, false);
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

                Common.Utils.LoadOrCreateConfig();
                WpfBootstrap.Ensure();
                ModernUI.ModernWpfTheme.RefreshResources();
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

        public static bool RestartCurrentApplication()
        {
            try
            {
                ProcessStartInfo startInfo = new(Application.ExecutablePath)
                {
                    UseShellExecute = false,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };

                foreach (string arg in Environment.GetCommandLineArgs().Skip(1))
                {
                    startInfo.ArgumentList.Add(arg);
                }

                startInfo.Environment[RestartParentPidEnvironmentVariable] = Environment.ProcessId.ToString();
                Process.Start(startInfo);
                Application.Exit();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to restart the application.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private static bool IsRestartLaunch()
        {
            return int.TryParse(Environment.GetEnvironmentVariable(RestartParentPidEnvironmentVariable), out int parentPid)
                && parentPid > 0;
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