using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System.ComponentModel;
using System.Diagnostics;
using ATRACTool_Reloaded.Localizable;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormProgress : Form
    {
        public FormProgress()
        {
            InitializeComponent();
        }

        private int DownloadProgress = 0;
        private string DownloadStatus = "";

        private void FormProgress_Load(object sender, EventArgs e)
        {
            timer_interval.Interval = 3000;
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
                case 4: // Update Program
                    {
                        Text = Localization.ProcessingCaption;
                        label1.Text = Localization.DownloadStatusCaption;
                        label_Status.Text = Localization.InitializationCaption;
                        Generic.cts = new CancellationTokenSource();
                        var cToken = Generic.cts.Token;
                        var p = new Progress<int>(UpdateProgress);

                        Generic.Result = await Task.Run(() => Download_DoWork(p, cToken));
                        break;
                    }
                default:
                    Close();
                    break;
            }
            timer_interval.Enabled = true;
        }

        private static bool Decode_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            Config.Load(xmlpath);
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
                            int console = int.Parse(Config.Entry["ATRAC3_Console"].Value);
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
                                            p.Report(files);
                                            continue;
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
                                            p.Report(files);
                                            continue;
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
                            int console = int.Parse(Config.Entry["ATRAC9_Console"].Value);
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
                                            p.Report(files);
                                            continue;
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
                                            p.Report(files);
                                            continue;
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
                                int console = int.Parse(Config.Entry["ATRAC3_Console"].Value);
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
                                                p.Report(files);
                                                continue;
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
                                                p.Report(files);
                                                continue;
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
                                int console = int.Parse(Config.Entry["ATRAC9_Console"].Value);
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
                                                p.Report(files);
                                                continue;
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
                                                p.Report(files);
                                                continue;
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
            Config.Load(xmlpath);
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
                            int console = int.Parse(Config.Entry["ATRAC3_Console"].Value);
                            switch (console)
                            {
                                case 0: // PSP
                                    {
                                        Generic.EncodeParamAT3 = Config.Entry["ATRAC3_Params"].Value;
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
                                            p.Report(files);
                                            continue;
                                        }
                                    }
                                    break;
                                case 1: // PS3
                                    {
                                        Generic.EncodeParamAT3 = Config.Entry["ATRAC3_Params"].Value;
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
                                            p.Report(files);
                                            continue;
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
                            int console = int.Parse(Config.Entry["ATRAC9_Console"].Value);
                            switch (console)
                            {
                                case 0: // PSV
                                    {
                                        Generic.EncodeParamAT9 = Config.Entry["ATRAC9_Params"].Value;
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
                                            p.Report(files);
                                            continue;
                                        }
                                    }
                                    break;
                                case 1: // PS4
                                    {
                                        Generic.EncodeParamAT9 = Config.Entry["ATRAC9_Params"].Value;
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
                                            p.Report(files);
                                            continue;
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
                                int console = int.Parse(Config.Entry["ATRAC3_Console"].Value);
                                switch (console)
                                {
                                    case 0: // PSP
                                        {
                                            if (Generic.lpcreate != false)
                                            {
                                                Generic.lpcreatev2 = true;
                                                Generic.files = fs;
                                                using FormLPC form = new(true);
                                                form.ShowDialog();

                                                Generic.EncodeParamAT3 = Config.Entry["ATRAC3_Params"].Value;
                                            }
                                            Generic.ATRACExt = ".at3";
                                            pi.FileName = Generic.PSP_ATRAC3tool;
                                            pi.Arguments = Generic.EncodeParamAT3.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".at3") + "\"").Replace("at3tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            var log = ps.StandardOutput;

                                            FileInfo fi2 = new(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Generic.ATRACExt));

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
                                                p.Report(files);
                                                continue;
                                            }

                                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Generic.ATRACExt)))
                                            {
                                                if (fi2.Length == 0 && log != null)
                                                {
                                                    string text = "'" + fi.Name + "' " + Utils.LogSplit(log);
                                                    MessageBox.Show(string.Format("{0}", text), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                                                using FormLPC form = new(true);
                                                form.ShowDialog();

                                                Generic.EncodeParamAT3 = Config.Entry["ATRAC3_Params"].Value;
                                            }
                                            Generic.ATRACExt = ".at3";
                                            pi.FileName = Generic.PS3_ATRAC3tool;
                                            pi.Arguments = Generic.EncodeParamAT3.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".at3") + "\"").Replace("at3tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            var log = ps.StandardOutput;

                                            FileInfo fi2 = new(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Generic.ATRACExt));

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
                                                p.Report(files);
                                                continue;
                                            }

                                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Generic.ATRACExt)))
                                            {
                                                if (fi2.Length == 0 && log != null)
                                                {
                                                    string text = "'" + fi.Name + "' " + Utils.LogSplit(log);
                                                    MessageBox.Show(string.Format("{0}", text), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                                int console = int.Parse(Config.Entry["ATRAC9_Console"].Value);
                                switch (console)
                                {
                                    case 0: // PSV
                                        {
                                            if (Generic.lpcreate != false)
                                            {
                                                Generic.lpcreatev2 = true;
                                                Generic.files = fs;
                                                using FormLPC form = new(true);
                                                form.ShowDialog();

                                                Generic.EncodeParamAT9 = Config.Entry["ATRAC9_Params"].Value;
                                            }
                                            Generic.ATRACExt = ".at9";
                                            pi.FileName = Generic.PSV_ATRAC9tool;
                                            pi.Arguments = Generic.EncodeParamAT9.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".at9") + "\"").Replace("at9tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            var log = ps.StandardOutput;

                                            FileInfo fi2 = new(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Generic.ATRACExt));

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
                                                p.Report(files);
                                                continue;
                                            }

                                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Generic.ATRACExt)))
                                            {
                                                if (fi2.Length == 0 && log != null)
                                                {
                                                    string text = "'" + fi.Name + "' " + Utils.LogSplit(log);
                                                    MessageBox.Show(string.Format("{0}", text), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                                                using FormLPC form = new(true);
                                                form.ShowDialog();

                                                Generic.EncodeParamAT9 = Config.Entry["ATRAC9_Params"].Value;
                                            }
                                            Generic.ATRACExt = ".at9";
                                            pi.FileName = Generic.PS4_ATRAC9tool;
                                            pi.Arguments = Generic.EncodeParamAT9.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".at9") + "\"").Replace("at9tool ", "");
                                            pi.UseShellExecute = false;
                                            pi.RedirectStandardOutput = true;
                                            pi.CreateNoWindow = true;

                                            ps = Process.Start(pi);

                                            if (ps is null) { return false; }

                                            var log = ps.StandardOutput;

                                            FileInfo fi2 = new(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Generic.ATRACExt));

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
                                                p.Report(files);
                                                continue;
                                            }

                                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Generic.ATRACExt)))
                                            {
                                                if (fi2.Length == 0 && log != null)
                                                {
                                                    string text = "'" + fi.Name + "' " + Utils.LogSplit(log);
                                                    MessageBox.Show(string.Format("{0}", text), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                            var task = MTK_ConvertAsync(engine, source, output, co);

                            while (task.IsCompleted != true)
                            {
                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                if (cToken.IsCancellationRequested == true)
                                {
                                    return false;
                                }
                                else
                                {
                                    p.Report(files);
                                    continue;
                                }
                            }
                            return true;
                        }
                        else
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            int count = 0;
                            foreach (var file in Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                var source = new MediaFile { Filename = file };
                                var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav" };

                                var co = new ConversionOptions
                                {
                                    AudioSampleRate = AudioSampleRate.Hz44100,
                                };

                                using var engine = new Engine();
                                var task = MTK_ConvertAsync(engine, source, output, co);
                                count++;

                                while (task.IsCompleted != true)
                                {
                                    int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                    if (cToken.IsCancellationRequested == true)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        p.Report(files);
                                        continue;
                                    }
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
                            var task = MTK_ConvertAsync(engine, source, output, co);

                            while (task.IsCompleted != true)
                            {
                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                if (cToken.IsCancellationRequested == true)
                                {
                                    return false;
                                }
                                else
                                {
                                    p.Report(files);
                                    continue;
                                }
                            }

                            return true;
                        }
                        else
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            foreach (var file in Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                var source = new MediaFile { Filename = file };
                                var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav" };

                                var co = new ConversionOptions
                                {
                                    AudioSampleRate = AudioSampleRate.Hz48000,
                                };

                                using var engine = new Engine();
                                var task = MTK_ConvertAsync(engine, source, output, co);

                                while (task.IsCompleted != true)
                                {
                                    int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                    if (cToken.IsCancellationRequested == true)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        p.Report(files);
                                        continue;
                                    }
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
                            var task = MTK_ConvertAsync(engine, source, output, co);

                            while (task.IsCompleted != true)
                            {
                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                if (cToken.IsCancellationRequested == true)
                                {
                                    return false;
                                }
                                else
                                {
                                    p.Report(files);
                                    continue;
                                }
                            }

                            return true;
                        }
                        else
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            foreach (var file in Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                var source = new MediaFile { Filename = file };
                                var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav" };

                                var co = new ConversionOptions
                                {
                                    AudioSampleRate = (AudioSampleRate)12000,
                                };

                                using var engine = new Engine();
                                var task = MTK_ConvertAsync(engine, source, output, co);

                                while (task.IsCompleted != true)
                                {
                                    int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                    if (cToken.IsCancellationRequested == true)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        p.Report(files);
                                        continue;
                                    }
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
                            var task = MTK_ConvertAsync(engine, source, output, co);

                            while (task.IsCompleted != true)
                            {
                                int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                if (cToken.IsCancellationRequested == true)
                                {
                                    return false;
                                }
                                else
                                {
                                    p.Report(files);
                                    continue;
                                }
                            }

                            return true;
                        }
                        else
                        {
                            p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                            foreach (var file in Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                var source = new MediaFile { Filename = file };
                                var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav" };

                                var co = new ConversionOptions
                                {
                                    AudioSampleRate = (AudioSampleRate)24000,
                                };

                                using var engine = new Engine();
                                var task = MTK_ConvertAsync(engine, source, output, co);

                                while (task.IsCompleted != true)
                                {
                                    int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                                    if (cToken.IsCancellationRequested == true)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        p.Report(files);
                                        continue;
                                    }
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
                var co = new ConversionOptions
                {
                    AudioSampleRate = AudioSampleRate.Hz44100,
                };

                using var engine = new Engine();
                var task = MTK_ConvertAsync(engine, source, output, co);

                while (task.IsCompleted != true)
                {
                    int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                    if (cToken.IsCancellationRequested == true)
                    {
                        return false;
                    }
                    else
                    {
                        p.Report(files);
                        continue;
                    }
                }

                return true;
            }
            else
            {
                p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                foreach (var file in Generic.OpenFilePaths)
                {
                    FileInfo fi = new(file);
                    var source = new MediaFile { Filename = file };
                    var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Generic.WTAFmt };
                    var co = new ConversionOptions
                    {
                        AudioSampleRate = AudioSampleRate.Hz44100,
                    };

                    using var engine = new Engine();
                    var task = MTK_ConvertAsync(engine, source, output, co);

                    while (task.IsCompleted != true)
                    {
                        int files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length;

                        if (cToken.IsCancellationRequested == true)
                        {
                            return false;
                        }
                        else
                        {
                            p.Report(files);
                            continue;
                        }
                    }
                }
                return true;
            }
        }

        private bool Download_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            if (Generic.downloadClient == null)
            {
#pragma warning disable SYSLIB0014 // 型またはメンバーが旧型式です
                Generic.downloadClient = new System.Net.WebClient();
#pragma warning restore SYSLIB0014 // 型またはメンバーが旧型式です
                Generic.downloadClient.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(DownloadClient_DownloadProgressChanged);
                Generic.downloadClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadClient_DownloadFileCompleted);
            }

            Invoke(new Action(() => Text = Localization.FormDownloadingCaption));
            var task = Download(cToken);

            while (task.IsCompleted != true)
            {
                if (task.Result == false)
                {
                    return false;
                }
            }
            if (cToken.IsCancellationRequested == true)
            {
                return false;
            }
            else
            {
                Invoke(new Action(() => Text = Localization.ProcessingCaption));
                Invoke(new Action(() => label_Status.Text = Localization.ProcessingCaption));
                Invoke(new Action(() => button_Abort.Enabled = false));
            }

            return true;
        }

        private async Task<bool> Download(CancellationToken cToken)
        {
            Uri uri;

            switch (Generic.ApplicationPortable)
            {
                case false:
                    {
                        uri = new("https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases/download/v" + Generic.GitHubLatestVersion + "/atractool-rel-release.zip");
                    }
                    break;
                case true:
                    {
                        uri = new("https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases/download/v" + Generic.GitHubLatestVersion + "/atractool-rel-portable.zip");
                    }
                    break;
            }

            switch (Generic.ApplicationPortable)
            {
                case false: // release
                    {
                        Generic.downloadClient.DownloadFileAsync(uri, Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip");
                    }
                    break;
                case true: // portable
                    {
                        Generic.downloadClient.DownloadFileAsync(uri, Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip");
                    }
                    break;
            }
            Generic.IsDownloading = true;

            while (Generic.downloadClient.IsBusy)
            {
                if (cToken.IsCancellationRequested == true)
                {
                    return await Task.FromResult(false);
                }
                Invoke(new Action(() => progressBar_MainProgress.Value = DownloadProgress));
                Invoke(new Action(() => label_Status.Text = DownloadStatus));
            }
            return await Task.FromResult(true);
        }

        private void DownloadClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (Generic.IsDownloading == true)
            {
                progressBar_MainProgress.Maximum = 100;
                Generic.IsDownloading = false;
            }
            DownloadProgress = e.ProgressPercentage;
            DownloadStatus = string.Format(Localization.DownloadingCaption, e.ProgressPercentage, e.TotalBytesToReceive / 1024, e.BytesReceived / 1024);
        }

        private void DownloadClient_DownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled) // Cancelled
            {
                Generic.downloadClient!.Dispose();
            }
            else if (e.Error != null) // Error
            {
                Generic.downloadClient!.Dispose();
            }
            else
            {
                Generic.downloadClient!.Dispose();
            }
        }

        private void UpdateProgress(int p)
        {
            switch (Generic.ProcessFlag)
            {
                default:
                    progressBar_MainProgress.Value = p;
                    label_Status.Text = string.Format(Localization.StatusCaption, p, Generic.OpenFilePaths.Length);
                    break;
            }
        }

        private void Button_Abort_Click(object sender, EventArgs e)
        {
            if (Generic.cts != null)
            {
                if (Generic.downloadClient.IsBusy)
                {
                    DialogResult dr = MessageBox.Show(Localization.DownloadAbortConfirmCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr == DialogResult.Yes)
                    {
                        Generic.cts.Cancel();
                        Generic.downloadClient.CancelAsync();
                        Close();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    Generic.cts.Cancel();
                    Close();
                }
            }
        }

        private static async Task MTK_ConvertAsync(Engine engine, MediaFile source, MediaFile dest, ConversionOptions co)
        {
            await Task.Run(() => engine.Convert(source, dest, co));
        }

        private void Timer_interval_Tick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
