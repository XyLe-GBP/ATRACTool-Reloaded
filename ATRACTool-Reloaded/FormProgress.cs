using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            switch (Common.Generic.ProcessFlag)
            {
                case 0: // Decode
                    {
                        Common.Generic.cts = new CancellationTokenSource();
                        var cToken = Common.Generic.cts.Token;
                        var p = new Progress<int>(UpdateProgress);

                        Generic.Result = await Task.Run(() => Decode_DoWork(p, cToken));
                        break;
                    }
                case 1: // Encode
                    {
                        Common.Generic.cts = new CancellationTokenSource();
                        var cToken = Common.Generic.cts.Token;
                        var p = new Progress<int>(UpdateProgress);

                        Generic.Result = await Task.Run(() => Encode_DoWork(p, cToken));
                        break;
                    }
                case 2: // Audio To Wave
                    {
                        Common.Generic.cts = new CancellationTokenSource();
                        var cToken = Common.Generic.cts.Token;
                        var p = new Progress<int>(UpdateProgress);

                        Generic.Result = await Task.Run(() => AudioConverter_ATW_DoWork(p, cToken));
                        break;
                    }
                case 3: // Wave To Audio
                    {
                        Common.Generic.cts = new CancellationTokenSource();
                        var cToken = Common.Generic.cts.Token;
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
                            pi.FileName = ".\\res\\at3tool.exe";
                            pi.Arguments = Generic.DecodeParamAT3.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at3tool ", "");
                            pi.WindowStyle = ProcessWindowStyle.Hidden;
                            pi.UseShellExecute = true;

                            ps = Process.Start(pi);

                            if (ps is null) { return false; }
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
                    case ".AT9":
                        {
                            pi.FileName = ".\\res\\at9tool.exe";
                            pi.Arguments = Generic.DecodeParamAT9.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at9tool ", "");
                            pi.WindowStyle = ProcessWindowStyle.Hidden;
                            pi.UseShellExecute = true;
                            ps = Process.Start(pi);

                            if (ps is null) { return false; }
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
                        return false;
                }
                return true;
            }
            else
            {
                foreach (var file in Generic.OpenFilePaths)
                {
                    FileInfo fi = new(file);

                    switch (fi.Extension.ToUpper())
                    {
                        case ".AT3":
                            {
                                pi.FileName = ".\\res\\at3tool.exe";
                                pi.Arguments = Generic.DecodeParamAT3.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp" + "\"").Replace("at3tool ", "");
                                pi.WindowStyle = ProcessWindowStyle.Hidden;
                                pi.UseShellExecute = true;
                                ps = Process.Start(pi);

                                if (ps is null) { return false; }
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
                        case ".AT9":
                            {
                                pi.FileName = ".\\res\\at9tool.exe";
                                pi.Arguments = Generic.DecodeParamAT9.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp" + "\"").Replace("at9tool ", "");
                                pi.WindowStyle = ProcessWindowStyle.Hidden;
                                pi.UseShellExecute = true;
                                ps = Process.Start(pi);

                                if (ps is null) { return false; }
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
                    case 0:
                        {
                            pi.FileName = ".\\res\\at3tool.exe";
                            pi.Arguments = Generic.EncodeParamAT3.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at3tool ", "");
                            pi.WindowStyle = ProcessWindowStyle.Hidden;
                            pi.UseShellExecute = true;
                            ps = Process.Start(pi);

                            if (ps is null) { return false; }
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
                    case 1:
                        {
                            pi.FileName = ".\\res\\at9tool.exe";
                            pi.Arguments = Generic.EncodeParamAT9.Replace("$InFile", "\"" + Generic.OpenFilePaths[0] + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp\" + fi2.Name + "\"").Replace("at9tool ", "");
                            pi.WindowStyle = ProcessWindowStyle.Hidden;
                            pi.UseShellExecute = true;
                            ps = Process.Start(pi);

                            if (ps is null) { return false; }
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
                        return false;
                }
                return true;
            }
            else
            {
                foreach (var file in Generic.OpenFilePaths)
                {
                    FileInfo fi = new(file);

                    switch (Generic.ATRACFlag)
                    {
                        case 0:
                            {
                                pi.FileName = ".\\res\\at3tool.exe";
                                pi.Arguments = Generic.EncodeParamAT3.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp" + "\"").Replace("at3tool ", "");
                                pi.WindowStyle = ProcessWindowStyle.Hidden;
                                pi.UseShellExecute = true;
                                ps = Process.Start(pi);

                                if (ps is null) { return false; }
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
                        case 1:
                            {
                                pi.FileName = ".\\res\\at9tool.exe";
                                pi.Arguments = Generic.EncodeParamAT9.Replace("$InFile", "\"" + file + "\"").Replace("$OutFile", "\"" + Directory.GetCurrentDirectory() + @"\_temp" + "\"").Replace("at9tool ", "");
                                pi.WindowStyle = ProcessWindowStyle.Hidden;
                                pi.UseShellExecute = true;
                                ps = Process.Start(pi);

                                if (ps is null) { return false; }
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
                            return false;
                    }
                }
                return true;
            }
        }
        private static bool AudioConverter_ATW_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            int length = Generic.OpenFilePaths.Length;

            if (length == 1)
            {
                p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                FileInfo fi = new(Common.Generic.SavePath);
                var source = new MediaFile { Filename = Common.Generic.OpenFilePaths[0] };
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
                foreach (var file in Common.Generic.OpenFilePaths)
                {
                    FileInfo fi = new(file);
                    var source = new MediaFile { Filename = file };
                    var output = new MediaFile { Filename = Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav" };

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

        private static bool AudioConverter_WTA_DoWork(IProgress<int> p, CancellationToken cToken)
        {
            int length = Generic.OpenFilePaths.Length;

            if (length == 1)
            {
                p.Report(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\_temp", "*").Length);
                FileInfo fi = new(Common.Generic.SavePath);
                var source = new MediaFile { Filename = Common.Generic.OpenFilePaths[0] };
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
                foreach (var file in Common.Generic.OpenFilePaths)
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
            switch (Common.Generic.ProcessFlag)
            {
                case 0:
                    progressBar_MainProgress.Value = p;
                    label_Status.Text = string.Format(Localization.StatusCaption, p, Common.Generic.OpenFilePaths.Length);
                    break;
                case 1:
                    progressBar_MainProgress.Value = p;
                    label_Status.Text = string.Format(Localization.StatusCaption, p, Common.Generic.OpenFilePaths.Length);
                    break;
                case 2:
                    progressBar_MainProgress.Value = p;
                    label_Status.Text = string.Format(Localization.StatusCaption, p, Common.Generic.OpenFilePaths.Length);
                    break;
            }
        }

        private void Button_Abort_Click(object sender, EventArgs e)
        {
            if (Common.Generic.cts != null)
            {
                Common.Generic.cts.Cancel();
                Close();
            }
            else
            {
                return;
            }
        }
    }
}
