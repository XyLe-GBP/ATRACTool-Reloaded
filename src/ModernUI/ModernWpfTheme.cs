using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ATRACTool_Reloaded.ModernUI
{
    internal static class ModernWpfTheme
    {
        private const int DwmwaUseImmersiveDarkMode = 20;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attribute,
            ref int attributeValue,
            int attributeSize);

        public static void Apply()
        {
            RefreshResources();
        }

        public static void RefreshResources()
        {
            var app = System.Windows.Application.Current;
            if (app == null)
            {
                return;
            }

            if (app.Dispatcher.CheckAccess())
            {
                ApplyTheme(app);
                return;
            }

            app.Dispatcher.BeginInvoke(new Action(() => ApplyTheme(app)));
        }

        private static void ApplyTheme(System.Windows.Application app)
        {
            ApplyResources(app.Resources);

            foreach (Window window in app.Windows)
            {
                ApplyWindowChrome(window);
            }
        }

        private static void ApplyResources(System.Windows.ResourceDictionary resources)
        {
            bool dark = ModernTheme.IsDarkMode;

            resources["WindowBrush"] = Brush(dark ? 0xFF202124 : 0xFFF6F7F9);
            resources["SurfaceBrush"] = Brush(dark ? 0xFF2B2D30 : 0xFFFFFFFF);
            resources["SurfaceHoverBrush"] = Brush(dark ? 0xFF3B4046 : 0xFFDCE6F1);
            resources["SurfacePressedBrush"] = Brush(dark ? 0xFF484E56 : 0xFFCAD8E7);
            resources["ButtonBrush"] = Brush(dark ? 0xFF383D45 : 0xFFFFFFFF);
            resources["BorderBrush"] = Brush(dark ? 0xFF585C63 : 0xFFD2D6DC);
            resources["TextBrush"] = Brush(dark ? 0xFFF2F3F5 : 0xFF1F2328);
            resources["MutedTextBrush"] = Brush(dark ? 0xFFB0B6BE : 0xFF57606A);
            resources["AccentBrush"] = Brush(0xFF0078D4);
            resources["DisabledBrush"] = Brush(dark ? 0xFF3F4248 : 0xFFCDD2D8);
            resources["DisabledBorderBrush"] = Brush(dark ? 0xFF52565E : 0xFFA4ABB3);
            resources["DisabledTextBrush"] = Brush(dark ? 0xFF828891 : 0xFF6C737B);
        }

        private static void ApplyWindowChrome(Window window)
        {
            window.SourceInitialized -= Window_SourceInitialized;
            window.SourceInitialized += Window_SourceInitialized;

            IntPtr handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            try
            {
                int darkMode = ModernTheme.IsDarkMode ? 1 : 0;
                DwmSetWindowAttribute(
                    handle,
                    DwmwaUseImmersiveDarkMode,
                    ref darkMode,
                    sizeof(int));
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }

        private static void Window_SourceInitialized(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                ApplyWindowChrome(window);
            }
        }

        private static SolidColorBrush Brush(uint argb)
        {
            SolidColorBrush brush = new(System.Windows.Media.Color.FromArgb(
                (byte)(argb >> 24),
                (byte)(argb >> 16),
                (byte)(argb >> 8),
                (byte)argb));
            brush.Freeze();
            return brush;
        }
    }
}
