using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using NAudio.Wave;
using System.Diagnostics;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormProgress : Form
    {
        public FormProgress()
        {
            InitializeComponent();
        }

        private void FormProgress_Load(object sender, EventArgs e)
        {
            progressBar_MainProgress.Value = 0;
            progressBar_MainProgress.Minimum = 0;
            progressBar_MainProgress.Maximum = Generic.ProgressMax;
            RunTask();
        }

        private async void RunTask()
        {
            switch (Generic.ProcessFlag)
            {
                case 0: // Decode
                    {
                        Generic.cts = new CancellationTokenSource();
                        var cToken = Generic.cts.Token;
                        var p = new Progress<int>(UpdateProgress);

                        Generic.Result = await Task.Run(() => Decode_DoWork(p, cToken));
                        break;
                    }
                case 1: // Encode
                    {
                        Generic.cts = new CancellationTokenSource();
                        var cToken = Generic.cts.Token;
                        var p = new Progress<int>(UpdateProgress);

                        Generic.Result = await Task.Run(() => Encode_DoWork(p, cToken));
                        break;
                    }
                case 2: // Audio To Wave
                    {
                        Generic.cts = new CancellationTokenSource();
                        var cToken = Generic.cts.Token;
                        var p = new Progress<int>(UpdateProgress);

                        Generic.Result = await Task.Run(() => AudioConverter_ATW_DoWork(p, cToken));
                        break;
                    }
                case 3: // Wave To Audio
                    {
                        Generic.cts = new CancellationTokenSource();
                        var cToken = Generic.cts.Token;
                        var p = new Progress<int>(UpdateProgress);

                        Generic.Result = await Task.Run(() => AudioConverter_WTA_DoWork(p, cToken));
                        break;
                    }
                default:
                    break;
            }
            Close();
        }

        private static bool Decode_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            Process? ps = new();
            ProcessStartInfo pi = new();

            int length = Generic.OpenFilePaths.Length;

            if (length == 1)
            {
                FileInfo fi = new(Generic.OpenFilePaths[0]);
                FileInfo fi2 = new(Generic.SavePath);

                switch (fi.Extension.ToUpper())
                {
                    case ".AT3":
                        {
                            int console = Utils.GetIntForIniFile("ATRAC3_SETTINGS", "Console");
                            switch (console)
                            {
                                case 0: // PSP
                                    {
                                        pi.FileName = Generic.PSP_ATRAC3tool;
                                        pi.Arguments = Generic.DecodeParamAT3.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at3tool ", "");
                                        pi.UseShellExecute = false;
                                        pi.RedirectStandardOutput = true;
                                        pi.CreateNoWindow = true;

                                        ps = Process.Start(pi);

                                        if (ps is null) { return false; }

                                        Generic.Log = ps.StandardOutput;

                                        while (!ps.HasExited)
                                        {
                                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                            if (cToken.IsCancellationRequested == true)
                                            {
                                                if (!ps.HasExited)
                                                {
                                                    ps.Kill();
                                                }
                                                ps.Close();

                                                return false;
                                            }
                                            else if (ps.HasExited == true)
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                break;
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                                case 1: // PS3
                                    {
                                        pi.FileName = Generic.PS3_ATRAC3tool;
                                        pi.Arguments = Generic.DecodeParamAT3.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at3tool ", "");
                                        pi.UseShellExecute = false;
                                        pi.RedirectStandardOutput = true;
                                        pi.CreateNoWindow = true;

                                        ps = Process.Start(pi);

                                        if (ps is null) { return false; }

                                        Generic.Log = ps.StandardOutput;

                                        while (!ps.HasExited)
                                        {
                                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                            if (cToken.IsCancellationRequested == true)
                                            {
                                                if (!ps.HasExited)
                                                {
                                                    ps.Kill();
                                                }
                                                ps.Close();

                                                return false;
                                            }
                                            else if (ps.HasExited == true)
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                break;
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case ".AT9":
                        {
                            int console = Utils.GetIntForIniFile("ATRAC9_SETTINGS", "Console");
                            switch (console)
                            {
                                case 0: // PSV
                                    {
                                        pi.FileName = Generic.PSV_ATRAC9tool;
                                        pi.Arguments = Generic.DecodeParamAT9.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at9tool ", "");
                                        pi.UseShellExecute = false;
                                        pi.RedirectStandardOutput = true;
                                        pi.CreateNoWindow = true;

                                        ps = Process.Start(pi);

                                        if (ps is null) { return false; }

                                        Generic.Log = ps.StandardOutput;

                                        while (!ps.HasExited)
                                        {
                                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                            if (cToken.IsCancellationRequested == true)
                                            {
                                                if (!ps.HasExited)
                                                {
                                                    ps.Kill();
                                                }
                                                ps.Close();

                                                return false;
                                            }
                                            else if (ps.HasExited == true)
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                break;
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                                case 1: // PS4
                                    {
                                        pi.FileName = Generic.PS4_ATRAC9tool;
                                        pi.Arguments = Generic.DecodeParamAT9.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at9tool ", "");
                                        pi.UseShellExecute = false;
                                        pi.RedirectStandardOutput = true;
                                        pi.CreateNoWindow = true;

                                        ps = Process.Start(pi);

                                        if (ps is null) { return false; }

                                        Generic.Log = ps.StandardOutput;

                                        while (!ps.HasExited)
                                        {
                                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                            if (cToken.IsCancellationRequested == true)
                                            {
                                                if (!ps.HasExited)
                                                {
                                                    ps.Kill();
                                                }
                                                ps.Close();

                                                return false;
                                            }
                                            else if (ps.HasExited == true)
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                break;
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        return false;
                }
                return true;
            }
            else // multiple
            {
                foreach (var file in Generic.OpenFilePaths)
                {
                    FileInfo fi = new(file);

                    switch (fi.Extension.ToUpper())
                    {
                        case ".AT3":
                            {
                                int console = Utils.GetIntForIniFile("ATRAC3_SETTINGS", "Console");
                                switch (console)
                                {
                                    case 0: // PSP
                                        {
                                            pi.FileName = Generic.PSP_ATRAC3tool;
                                            pi.Arguments = Generic.DecodeParamAT3.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav") + "\"").Replace("at3tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            Generic.Log = ps.StandardOutput;

                                            while (!ps.HasExited)
                                            {
                                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                                if (cToken.IsCancellationRequested == true)
                                                {
                                                    if (!ps.HasExited)
                                                    {
                                                        ps.Kill();
                                                    }
                                                    ps.Close();

                                                    return false;
                                                }
                                                else if (ps.HasExited == true)
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    break;
                                                }
                                                else
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    continue;
                                                }
                                            }
                                        }
                                        break;
                                    case 1: // PS3
                                        {
                                            pi.FileName = Generic.PS3_ATRAC3tool;
                                            pi.Arguments = Generic.DecodeParamAT3.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav") + "\"").Replace("at3tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            Generic.Log = ps.StandardOutput;

                                            while (!ps.HasExited)
                                            {
                                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                                if (cToken.IsCancellationRequested == true)
                                                {
                                                    if (!ps.HasExited)
                                                    {
                                                        ps.Kill();
                                                    }
                                                    ps.Close();

                                                    return false;
                                                }
                                                else if (ps.HasExited == true)
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    break;
                                                }
                                                else
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    continue;
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case ".AT9":
                            {
                                int console = Utils.GetIntForIniFile("ATRAC9_SETTINGS", "Console");
                                switch (console)
                                {
                                    case 0: // PSV
                                        {
                                            pi.FileName = Generic.PSV_ATRAC9tool;
                                            pi.Arguments = Generic.DecodeParamAT9.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav") + "\"").Replace("at9tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            Generic.Log = ps.StandardOutput;

                                            while (!ps.HasExited)
                                            {
                                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                                if (cToken.IsCancellationRequested == true)
                                                {
                                                    if (!ps.HasExited)
                                                    {
                                                        ps.Kill();
                                                    }
                                                    ps.Close();

                                                    return false;
                                                }
                                                else if (ps.HasExited == true)
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    break;
                                                }
                                                else
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    continue;
                                                }
                                            }
                                        }
                                        break;
                                    case 1: // PS4
                                        {
                                            pi.FileName = Generic.PS4_ATRAC9tool;
                                            pi.Arguments = Generic.DecodeParamAT9.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav") + "\"").Replace("at9tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            Generic.Log = ps.StandardOutput;

                                            while (!ps.HasExited)
                                            {
                                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                                if (cToken.IsCancellationRequested == true)
                                                {
                                                    if (!ps.HasExited)
                                                    {
                                                        ps.Kill();
                                                    }
                                                    ps.Close();

                                                    return false;
                                                }
                                                else if (ps.HasExited == true)
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    break;
                                                }
                                                else
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    continue;
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        default:
                            return false;
                    }
                }
                return true;
            }
        }

        private static bool Encode_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            Process? ps = new();
            ProcessStartInfo pi = new();

            int length = Generic.OpenFilePaths.Length;

            if (length == 1)
            {
                FileInfo fi = new(Generic.OpenFilePaths[0]);
                FileInfo fi2 = new(Generic.SavePath);

                switch (Generic.ATRACFlag)
                {
                    case 0: // ATRAC3
                        {
                            int console = Utils.GetIntForIniFile("ATRAC3_SETTINGS", "Console");
                            switch (console)
                            {
                                case 0: // PSP
                                    {
                                        Generic.EncodeParamAT3 = Utils.GetStringForIniFile("ATRAC3_SETTINGS", "Param");
                                        pi.FileName = Generic.PSP_ATRAC3tool;
                                        pi.Arguments = Generic.EncodeParamAT3.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at3tool ", "");
                                        pi.UseShellExecute = false;
                                        pi.RedirectStandardOutput = true;
                                        pi.CreateNoWindow = true;

                                        ps = Process.Start(pi);

                                        if (ps is null) { return false; }

                                        Generic.Log = ps.StandardOutput;

                                        while (!ps.HasExited)
                                        {
                                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                            if (cToken.IsCancellationRequested == true)
                                            {
                                                if (!ps.HasExited)
                                                {
                                                    ps.Kill();
                                                }
                                                ps.Close();

                                                return false;
                                            }
                                            else if (ps.HasExited == true)
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                break;
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                                case 1: // PS3
                                    {
                                        Generic.EncodeParamAT3 = Utils.GetStringForIniFile("ATRAC3_SETTINGS", "Param");
                                        pi.FileName = Generic.PS3_ATRAC3tool;
                                        pi.Arguments = Generic.EncodeParamAT3.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at3tool ", "");
                                        pi.UseShellExecute = false;
                                        pi.RedirectStandardOutput = true;
                                        pi.CreateNoWindow = true;

                                        ps = Process.Start(pi);

                                        if (ps is null) { return false; }

                                        Generic.Log = ps.StandardOutput;

                                        while (!ps.HasExited)
                                        {
                                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                            if (cToken.IsCancellationRequested == true)
                                            {
                                                if (!ps.HasExited)
                                                {
                                                    ps.Kill();
                                                }
                                                ps.Close();

                                                return false;
                                            }
                                            else if (ps.HasExited == true)
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                break;
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }


                        }
                        break;
                    case 1: // ATRAC9
                        {
                            int console = Utils.GetIntForIniFile("ATRAC9_SETTINGS", "Console");
                            switch (console)
                            {
                                case 0: // PSV
                                    {
                                        Generic.EncodeParamAT9 = Utils.GetStringForIniFile("ATRAC9_SETTINGS", "Param");
                                        pi.FileName = Generic.PSV_ATRAC9tool;
                                        pi.Arguments = Generic.EncodeParamAT9.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at9tool ", "");
                                        pi.UseShellExecute = false;
                                        pi.RedirectStandardOutput = true;
                                        pi.CreateNoWindow = true;

                                        ps = Process.Start(pi);

                                        if (ps is null) { return false; }

                                        Generic.Log = ps.StandardOutput;

                                        while (!ps.HasExited)
                                        {
                                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                            if (cToken.IsCancellationRequested == true)
                                            {
                                                if (!ps.HasExited)
                                                {
                                                    ps.Kill();
                                                }
                                                ps.Close();

                                                return false;
                                            }
                                            else if (ps.HasExited == true)
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                break;
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                                case 1: // PS4
                                    {
                                        Generic.EncodeParamAT9 = Utils.GetStringForIniFile("ATRAC9_SETTINGS", "Param");
                                        pi.FileName = Generic.PS4_ATRAC9tool;
                                        pi.Arguments = Generic.EncodeParamAT9.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at9tool ", "");
                                        pi.UseShellExecute = false;
                                        pi.RedirectStandardOutput = true;
                                        pi.CreateNoWindow = true;

                                        ps = Process.Start(pi);

                                        if (ps is null) { return false; }

                                        Generic.Log = ps.StandardOutput;

                                        while (!ps.HasExited)
                                        {
                                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                            if (cToken.IsCancellationRequested == true)
                                            {
                                                if (!ps.HasExited)
                                                {
                                                    ps.Kill();
                                                }
                                                ps.Close();

                                                return false;
                                            }
                                            else if (ps.HasExited == true)
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                break;
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                                p.Report(files);
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        return false;
                }
                return true;
            }
            else // multiple
            {
                int fs = 0;
                foreach (var file in Generic.OpenFilePaths)
                {
                    FileInfo fi = new(file);

                    switch (Generic.ATRACFlag)
                    {
                        case 0: // ATRAC3
                            {
                                int console = Utils.GetIntForIniFile("ATRAC3_SETTINGS", "Console");
                                switch (console)
                                {
                                    case 0: // PSP
                                        {
                                            if (Generic.lpcreate != false)
                                            {
                                                Generic.lpcreatev2 = true;
                                                Generic.files = fs;
                                                using FormLPC form = new();
                                                form.ShowDialog();

                                                Generic.EncodeParamAT3 = Utils.GetStringForIniFile("ATRAC3_SETTINGS", "Param");
                                            }
                                            Generic.ATRACExt = ".at3";
                                            pi.FileName = Generic.PSP_ATRAC3tool;
                                            pi.Arguments = Generic.EncodeParamAT3.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".at3") + "\"").Replace("at3tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            Generic.Log = ps.StandardOutput;

                                            while (!ps.HasExited)
                                            {
                                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                                if (cToken.IsCancellationRequested == true)
                                                {
                                                    if (!ps.HasExited)
                                                    {
                                                        ps.Kill();
                                                    }
                                                    ps.Close();

                                                    return false;
                                                }
                                                else if (ps.HasExited == true)
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    break;
                                                }
                                                else
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    continue;
                                                }
                                            }
                                        }
                                        break;
                                    case 1: // PS3
                                        {
                                            if (Generic.lpcreate != false)
                                            {
                                                Generic.lpcreatev2 = true;
                                                Generic.files = fs;
                                                using FormLPC form = new();
                                                form.ShowDialog();

                                                Generic.EncodeParamAT3 = Utils.GetStringForIniFile("ATRAC3_SETTINGS", "Param");
                                            }
                                            Generic.ATRACExt = ".at3";
                                            pi.FileName = Generic.PS3_ATRAC3tool;
                                            pi.Arguments = Generic.EncodeParamAT3.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".at3") + "\"").Replace("at3tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            Generic.Log = ps.StandardOutput;

                                            while (!ps.HasExited)
                                            {
                                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                                if (cToken.IsCancellationRequested == true)
                                                {
                                                    if (!ps.HasExited)
                                                    {
                                                        ps.Kill();
                                                    }
                                                    ps.Close();

                                                    return false;
                                                }
                                                else if (ps.HasExited == true)
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    break;
                                                }
                                                else
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    continue;
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case 1: // ATRAC9
                            {
                                int console = Utils.GetIntForIniFile("ATRAC9_SETTINGS", "Console");
                                switch (console)
                                {
                                    case 0: // PSV
                                        {
                                            if (Generic.lpcreate != false)
                                            {
                                                Generic.lpcreatev2 = true;
                                                Generic.files = fs;
                                                using FormLPC form = new();
                                                form.ShowDialog();

                                                Generic.EncodeParamAT9 = Utils.GetStringForIniFile("ATRAC9_SETTINGS", "Param");
                                            }
                                            Generic.ATRACExt = ".at9";
                                            pi.FileName = Generic.PSV_ATRAC9tool;
                                            pi.Arguments = Generic.EncodeParamAT9.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".at9") + "\"").Replace("at9tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            Generic.Log = ps.StandardOutput;

                                            while (!ps.HasExited)
                                            {
                                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                                if (cToken.IsCancellationRequested == true)
                                                {
                                                    if (!ps.HasExited)
                                                    {
                                                        ps.Kill();
                                                    }
                                                    ps.Close();

                                                    return false;
                                                }
                                                else if (ps.HasExited == true)
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    break;
                                                }
                                                else
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    continue;
                                                }
                                            }
                                        }
                                        break;
                                    case 1: // PS4
                                        {
                                            if (Generic.lpcreate != false)
                                            {
                                                Generic.lpcreatev2 = true;
                                                Generic.files = fs;
                                                using FormLPC form = new();
                                                form.ShowDialog();

                                                Generic.EncodeParamAT9 = Utils.GetStringForIniFile("ATRAC9_SETTINGS", "Param");
                                            }
                                            Generic.ATRACExt = ".at9";
                                            pi.FileName = Generic.PS4_ATRAC9tool;
                                            pi.Arguments = Generic.EncodeParamAT9.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".at9") + "\"").Replace("at9tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            Generic.Log = ps.StandardOutput;

                                            while (!ps.HasExited)
                                            {
                                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                                if (cToken.IsCancellationRequested == true)
                                                {
                                                    if (!ps.HasExited)
                                                    {
                                                        ps.Kill();
                                                    }
                                                    ps.Close();

                                                    return false;
                                                }
                                                else if (ps.HasExited == true)
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    break;
                                                }
                                                else
                                                {
                                                    Thread.Sleep(100);
                                                    p.Report(files);
                                                    continue;
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                

                            }
                            break;
                        default:
                            return false;
                    }
                    fs++;
                }
                return true;
            }
        }

        private static bool AudioConverter_ATW_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            int length = Generic.OpenFilePaths.Length;
            switch (Generic.WTAmethod)
            {
                case 0: // 44100Hz
                    {
                        if (length == 1)
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            FileInfo fi = new(Generic.SavePath);
                            var source = new MediaFile { Filename = Generic.OpenFilePaths[0] };
                            var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name };

                            var co = new ConversionOptions
                            {
                                AudioSampleRate = AudioSampleRate.Hz44100,
                            };

                            using var engine = new Engine();
                            engine.Convert(source, output, co);

                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                            if (cToken.IsCancellationRequested == true)
                            {
                                return false;
                            }
                            else
                            {
                                Thread.Sleep(100);
                                p.Report(files);
                                return true;
                            }
                        }
                        else
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            foreach (var file in Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                var source = new MediaFile { Filename = file };
                                var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav" };

                                var co = new ConversionOptions
                                {
                                    AudioSampleRate = AudioSampleRate.Hz44100,
                                };

                                using var engine = new Engine();
                                engine.Convert(source, output, co);

                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                if (cToken.IsCancellationRequested == true)
                                {
                                    return false;
                                }
                                else
                                {
                                    Thread.Sleep(100);
                                    p.Report(files);
                                    continue;
                                }
                            }
                            return true;
                        }
                    }
                case 1: // 48000Hz
                    {
                        if (length == 1)
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            FileInfo fi = new(Generic.SavePath);
                            var source = new MediaFile { Filename = Generic.OpenFilePaths[0] };
                            var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name };

                            var co = new ConversionOptions
                            {
                                AudioSampleRate = AudioSampleRate.Hz48000,
                            };

                            using var engine = new Engine();
                            engine.Convert(source, output, co);

                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                            if (cToken.IsCancellationRequested == true)
                            {
                                return false;
                            }
                            else
                            {
                                Thread.Sleep(100);
                                p.Report(files);
                                return true;
                            }
                        }
                        else
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            foreach (var file in Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                var source = new MediaFile { Filename = file };
                                var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav" };

                                var co = new ConversionOptions
                                {
                                    AudioSampleRate = AudioSampleRate.Hz48000,
                                };

                                using var engine = new Engine();
                                engine.Convert(source, output, co);

                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                if (cToken.IsCancellationRequested == true)
                                {
                                    return false;
                                }
                                else
                                {
                                    Thread.Sleep(100);
                                    p.Report(files);
                                    continue;
                                }
                            }
                            return true;
                        }
                    }
                case 2: // 12000Hz
                    {
                        if (length == 1)
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            FileInfo fi = new(Generic.SavePath);
                            var source = new MediaFile { Filename = Generic.OpenFilePaths[0] };
                            var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name };

                            var co = new ConversionOptions
                            {
                                AudioSampleRate = (AudioSampleRate)12000,
                            };

                            using var engine = new Engine();
                            engine.Convert(source, output, co);

                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                            if (cToken.IsCancellationRequested == true)
                            {
                                return false;
                            }
                            else
                            {
                                Thread.Sleep(100);
                                p.Report(files);
                                return true;
                            }
                        }
                        else
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            foreach (var file in Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                var source = new MediaFile { Filename = file };
                                var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav" };

                                var co = new ConversionOptions
                                {
                                    AudioSampleRate = (AudioSampleRate)12000,
                                };

                                using var engine = new Engine();
                                engine.Convert(source, output, co);

                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                if (cToken.IsCancellationRequested == true)
                                {
                                    return false;
                                }
                                else
                                {
                                    Thread.Sleep(100);
                                    p.Report(files);
                                    continue;
                                }
                            }
                            return true;
                        }
                    }
                case 3: // 24000Hz
                    {
                        if (length == 1)
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            FileInfo fi = new(Generic.SavePath);
                            var source = new MediaFile { Filename = Generic.OpenFilePaths[0] };
                            var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name };

                            var co = new ConversionOptions
                            {
                                AudioSampleRate = (AudioSampleRate)24000,
                            };

                            using var engine = new Engine();
                            engine.Convert(source, output, co);

                            int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                            if (cToken.IsCancellationRequested == true)
                            {
                                return false;
                            }
                            else
                            {
                                Thread.Sleep(100);
                                p.Report(files);
                                return true;
                            }
                        }
                        else
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            foreach (var file in Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                var source = new MediaFile { Filename = file };
                                var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav" };

                                var co = new ConversionOptions
                                {
                                    AudioSampleRate = (AudioSampleRate)24000,
                                };

                                using var engine = new Engine();
                                engine.Convert(source, output, co);

                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                if (cToken.IsCancellationRequested == true)
                                {
                                    return false;
                                }
                                else
                                {
                                    Thread.Sleep(100);
                                    p.Report(files);
                                    continue;
                                }
                            }
                            return true;
                        }
                    }
                default:
                    return false;
            }

            
        }

        private static bool AudioConverter_WTA_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            int length = Generic.OpenFilePaths.Length;

            if (length == 1)
            {
                p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                FileInfo fi = new(Generic.SavePath);
                var source = new MediaFile { Filename = Generic.OpenFilePaths[0] };
                var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name };

                using var engine = new Engine();
                engine.Convert(source, output);

                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                if (cToken.IsCancellationRequested == true)
                {
                    return false;
                }
                else
                {
                    Thread.Sleep(100);
                    p.Report(files);
                    return true;
                }
            }
            else
            {
                p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                foreach (var file in Generic.OpenFilePaths)
                {
                    FileInfo fi = new(file);
                    var source = new MediaFile { Filename = file };
                    var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Generic.WTAFmt };

                    using var engine = new Engine();
                    engine.Convert(source, output);

                    int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                    if (cToken.IsCancellationRequested == true)
                    {
                        return false;
                    }
                    else
                    {
                        Thread.Sleep(100);
                        p.Report(files);
                        continue;
                    }
                }
                return true;
            }
        }

        private void UpdateProgress(int p)
        {
            switch (Generic.ProcessFlag)
            {
                case 0:
                    progressBar_MainProgress.Value = p;
                    label_Status.Text = string.Format(Localization.StatusCaption, p, Generic.OpenFilePaths.Length);
                    break;
                case 1:
                    progressBar_MainProgress.Value = p;
                    label_Status.Text = string.Format(Localization.StatusCaption, p, Generic.OpenFilePaths.Length);
                    break;
                case 2:
                    progressBar_MainProgress.Value = p;
                    label_Status.Text = string.Format(Localization.StatusCaption, p, Generic.OpenFilePaths.Length);
                    break;
            }
        }

        private void Button_Abort_Click(object sender, EventArgs e)
        {
            if (Generic.cts != null)
            {
                Generic.cts.Cancel();
                Close();
            }
            else
            {
                return;
            }
        }
    }
}
