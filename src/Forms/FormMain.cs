using System.Diagnostics;
using System.Net.NetworkInformation;
using ATRACTool_Reloaded.Localizable;
using ATRACTool_Reloaded.Properties;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormMain : Form
    {
        #region NetworkCommon
        private static readonly HttpClientHandler handler = new()
        {
            UseProxy = false,
            UseCookies = false
        };
        private static readonly HttpClient appUpdatechecker = new(handler);
        #endregion
        FormLPC FLPC;
        static FormSplash fs;
        static object lockobj;

        public FormMain()
        {
            InitializeComponent();
        }

        // 初期化

        private void FormMain_Load(object sender, EventArgs e)
        {
            FileVersionInfo ver = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            if (ver.FileVersion != null)
            {
                Text = "ATRACTool Rel ( build: " + ver.FileVersion.ToString() + "-Beta )";
            }

            if (!File.Exists(Common.xmlpath))
            {
                Common.Utils.InitConfig();
            }

            Common.Config.Load(Common.xmlpath);

            if (File.Exists(Directory.GetCurrentDirectory() + @"\updated.dat"))
            {
                TopMost = true;
                TopMost = false;
            }

            if (!bool.Parse(Config.Entry["HideSplash"].Value)) // Splash
            {
                lockobj = new object();

                lock (lockobj)
                {
                    ThreadStart tds = new(StartThread);
                    Thread thread = new(tds)
                    {
                        Name = "Splash",
                        IsBackground = true
                    };
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();

                    dmes d = new(ShowMessage);
                    fs?.Invoke(d, "Initializing...");
                    Thread.Sleep(1000);
                    foreach (var files in Directory.GetFiles(Directory.GetCurrentDirectory() + @"\res", "*", SearchOption.AllDirectories))
                    {
                        FileInfo fi = new(files);
                        if (fs != null)
                        {
                            fs.Invoke(d, string.Format(Localization.SplashFormFileCaption, fi.Name));
                            Thread.Sleep(50);
                        }
                    }

                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\_temp");
                    ResetStatus();

                    fs?.Invoke(d, Localization.SplashFormConfigCaption);

                    int ts = int.Parse(Config.Entry["ToolStrip"].Value);
                    string prm1 = Config.Entry["ATRAC3_Params"].Value, prm2 = Config.Entry["ATRAC9_Params"].Value;
                    if (ts != 65535)
                    {
                        switch (ts)
                        {
                            case 0:
                                Common.Generic.ATRACFlag = 0;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                aTRAC9ToolStripMenuItem.Checked = false;
                                toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                break;
                            case 1:
                                Common.Generic.ATRACFlag = 1;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = true;
                                toolStripDropDownButton_EF.Text = "ATRAC9";
                                break;
                        }
                    }
                    if (prm1 != "" || prm1 != null)
                    {
                        Common.Generic.EncodeParamAT3 = prm1;
                    }
                    else
                    {
                        Common.Generic.EncodeParamAT3 = "";
                    }
                    if (prm2 != "" || prm2 != null)
                    {
                        Common.Generic.EncodeParamAT9 = prm2;
                    }
                    else
                    {
                        Common.Generic.EncodeParamAT9 = "";
                    }
                    loopPointCreationToolStripMenuItem.Enabled = false;
                    Thread.Sleep(1000);

                    if (bool.Parse(Config.Entry["Oldmode"].Value))
                    {
                        fs?.Invoke(d, "Old mode is activated");
                        Thread.Sleep(500);
                        loopPointCreationToolStripMenuItem.Enabled = true;
                    }
                    else
                    {
                        loopPointCreationToolStripMenuItem.Enabled = false;
                    }

                    try
                    {
                        if (bool.Parse(Config.Entry["Check_Update"].Value))
                        {
                            fs?.Invoke(d, Localization.SplashFormUpdateCaption);
                            Thread.Sleep(500);
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\updated.dat"))
                            {
                                fs?.Invoke(d, Localization.SplashFormUpdatingCaption);
                                File.Delete(Directory.GetCurrentDirectory() + @"\updated.dat");
                                string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];
                                DirectoryInfo di = new(updpath + @"\updater-temp");
                                Common.Utils.RemoveReadonlyAttribute(di);
                                File.Delete(updpath + @"\updater.exe");
                                File.Delete(updpath + @"\atractool-rel.zip");
                                Common.Utils.DeleteDirectory(updpath + @"\updater-temp");

                                fs?.Invoke(d, Localization.SplashFormUpdatedCaption);
                                MessageBox.Show(fs, Localization.UpdateCompletedCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                var update = Task.Run(() => CheckForUpdatesForInit());
                                update.Wait();
                            }
                        }
                        else
                        {
                            fs?.Invoke(d, "Skip Update");
                            Thread.Sleep(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(fs, "An error occured.\n" + ex, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    
                    fs?.Invoke(d, "Starting...");
                    Thread.Sleep(1000);
                }

                CloseSplash();
            }
            else // No Splash
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\_temp");
                ResetStatus();

                int ts = int.Parse(Config.Entry["ToolStrip"].Value);
                string prm1 = Config.Entry["ATRAC3_Params"].Value, prm2 = Config.Entry["ATRAC9_Params"].Value;
                if (ts != 65535)
                {
                    switch (ts)
                    {
                        case 0:
                            Common.Generic.ATRACFlag = 0;
                            aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                            aTRAC9ToolStripMenuItem.Checked = false;
                            toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                            break;
                        case 1:
                            Common.Generic.ATRACFlag = 1;
                            aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                            aTRAC9ToolStripMenuItem.Checked = true;
                            toolStripDropDownButton_EF.Text = "ATRAC9";
                            break;
                    }
                }
                if (prm1 != "" || prm1 != null)
                {
                    Common.Generic.EncodeParamAT3 = prm1;
                }
                else
                {
                    Common.Generic.EncodeParamAT3 = "";
                }
                if (prm2 != "" || prm2 != null)
                {
                    Common.Generic.EncodeParamAT9 = prm2;
                }
                else
                {
                    Common.Generic.EncodeParamAT9 = "";
                }
                loopPointCreationToolStripMenuItem.Enabled = false;

                if (bool.Parse(Config.Entry["Check_Update"].Value))
                {
                    try
                    {
                        if (File.Exists(Directory.GetCurrentDirectory() + @"\updated.dat"))
                        {
                            File.Delete(Directory.GetCurrentDirectory() + @"\updated.dat");
                            string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];
                            DirectoryInfo di = new(updpath + @"\updater-temp");
                            Common.Utils.RemoveReadonlyAttribute(di);
                            File.Delete(updpath + @"\updater.exe");
                            File.Delete(updpath + @"\atractool-rel.zip");
                            Common.Utils.DeleteDirectory(updpath + @"\updater-temp");

                            MessageBox.Show(this, Localization.UpdateCompletedCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            var update = Task.Run(() => CheckForUpdatesForInit());
                            update.Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(fs, "An error occured.\n" + ex, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            Activate();

            if (Generic.GlobalException is not null)
            {
                MessageBox.Show(this, string.Format(Localization.UnExpectedCaption, Generic.GlobalException), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // メニュー項目

        /// <summary>
        /// ファイルを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = Localization.Filters,
                FilterIndex = 12,
                Title = Localization.OpenDialogTitle,
                Multiselect = true,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                closeFileCToolStripMenuItem.PerformClick();

                Utils.ATWCheck(Generic.IsATW);

                List<string> lst = new();
                foreach (string files in ofd.FileNames)
                {
                    lst.Add(files);
                }
                Common.Generic.OpenFilePaths = lst.ToArray();
                if (Common.Generic.OpenFilePaths.Length == 1) // Single
                {
                    FileInfo file = new(ofd.FileName);
                    long FileSize = file.Length;
                    ReadStatus();
                    label_Filepath.Text = ofd.FileName;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FileSize / 1024);
                    switch (file.Extension.ToUpper())
                    {
                        case ".WAV":
                            FormatSorter(true);
                            break;
                        case ".MP3":
                            FormatSorter(true, true);
                            break;
                        case ".M4A":
                            FormatSorter(true, true);
                            break;
                        case ".AAC":
                            FormatSorter(true, true);
                            break;
                        case ".FLAC":
                            FormatSorter(true, true);
                            break;
                        case ".ALAC":
                            FormatSorter(true, true);
                            break;
                        case ".AIFF":
                            FormatSorter(true, true);
                            break;
                        case ".OGG":
                            FormatSorter(true, true);
                            break;
                        case ".OPUS":
                            FormatSorter(true, true);
                            break;
                        case ".WMA":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    closeFileCToolStripMenuItem.Enabled = true;
                    return;
                }
                else // Multiple
                {
                    long FS = 0;
                    foreach (string file in Common.Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);
                        FS += fi.Length;
                    }

                    string Ft = "";
                    int count = 0;

                    foreach (var file in Common.Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);

                        if (count != 0)
                        {
                            if (Ft != fi.Extension)
                            {
                                MessageBox.Show(this, Localization.FileMixedErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                closeFileCToolStripMenuItem.Enabled = false;
                                toolStripDropDownButton_EF.Enabled = false;
                                toolStripDropDownButton_EF.Visible = false;
                                button_Decode.Enabled = false;
                                button_Encode.Enabled = false;
                                loopPointCreationToolStripMenuItem.Enabled = false;
                                return;
                            }
                        }
                        else
                        {
                            Ft = fi.Extension;
                        }
                        count++;
                    }

                    ReadStatus();
                    label_Filepath.Text = Localization.MultipleFilesCaption;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024);

                    closeFileCToolStripMenuItem.Enabled = true;

                    switch (Ft.ToUpper())
                    {
                        case ".WAV":
                            FormatSorter(true);
                            break;
                        case ".MP3":
                            FormatSorter(true, true);
                            break;
                        case ".M4A":
                            FormatSorter(true, true);
                            break;
                        case ".AAC":
                            FormatSorter(true, true);
                            break;
                        case ".FLAC":
                            FormatSorter(true, true);
                            break;
                        case ".ALAC":
                            FormatSorter(true, true);
                            break;
                        case ".AIFF":
                            FormatSorter(true, true);
                            break;
                        case ".OGG":
                            FormatSorter(true, true);
                            break;
                        case ".OPUS":
                            FormatSorter(true, true);
                            break;
                        case ".WMA":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    return;
                }
            }
            else
            {
                ResetStatus();
                return;
            }
        }

        private void CloseFileCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivateOrDeactivateLPC(false);
            ResetStatus();
        }

        private void ExitXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ConvertSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings formSettings = new();
            formSettings.ShowDialog();
            formSettings.Dispose();

            string prm1 = Config.Entry["ATRAC3_Params"].Value, prm2 = Config.Entry["ATRAC9_Params"].Value;
            bool lpc = bool.Parse(Config.Entry["LPC_Create"].Value);

            if (prm1 != "" || prm1 != null)
            {
                Common.Generic.EncodeParamAT3 = prm1;
            }
            else
            {
                Common.Generic.EncodeParamAT3 = "";
            }
            if (prm2 != "" || prm2 != null)
            {
                Common.Generic.EncodeParamAT9 = prm2;
            }
            else
            {
                Common.Generic.EncodeParamAT9 = "";
            }
            Common.Generic.lpcreate = lpc switch
            {
                false => false,
                true => true,
            };
        }

        private void ConvertAudioToolStripMenuItem_Click(object sender, EventArgs e)
        {



        }

        private void AboutATRACToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout formAbout = new();
            formAbout.ShowDialog();
            formAbout.Dispose();
        }

        private async void CheckForUpdatesUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    string hv = null!;

                    using Stream hcs = await Task.Run(() => Common.Network.GetWebStreamAsync(appUpdatechecker, Common.Network.GetUri("https://raw.githubusercontent.com/XyLe-GBP/ATRACTool-Reloaded/master/VERSIONINFO")));
                    using StreamReader hsr = new(hcs);
                    hv = await Task.Run(() => hsr.ReadToEndAsync());
                    Common.Generic.GitHubLatestVersion = hv[8..].Replace("\n", "");

                    FileVersionInfo ver = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

                    if (ver.FileVersion != null)
                    {
                        switch (ver.FileVersion.ToString().CompareTo(hv[8..].Replace("\n", "")))
                        {
                            case -1:
                                DialogResult dr = MessageBox.Show(Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + ver.FileVersion + "\n" + Localization.UpdateConfirmCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (dr == DialogResult.Yes)
                                {
                                    using FormUpdateApplicationType fuat = new();
                                    fuat.ShowDialog();

                                    Common.Generic.ProcessFlag = 4;
                                    Common.Generic.ProgressMax = 100;
                                    using FormProgress form = new();
                                    form.ShowDialog();

                                    if (Common.Generic.Result == false)
                                    {
                                        Common.Generic.cts.Dispose();
                                        MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }

                                    string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];
                                    File.Move(Directory.GetCurrentDirectory() + @"\res\updater.exe", updpath + @"\updater.exe");
                                    string wtext;
                                    switch (Common.Generic.ApplicationPortable)
                                    {
                                        case false:
                                            {
                                                wtext = Directory.GetCurrentDirectory() + "\r\nrelease";
                                            }
                                            break;
                                        case true:
                                            {
                                                wtext = Directory.GetCurrentDirectory() + "\r\nportable";
                                            }
                                            break;
                                    }
                                    File.WriteAllText(updpath + @"\updater.txt", wtext);
                                    File.Move(updpath + @"\updater.txt", updpath + @"\updater.dat");
                                    if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                    {
                                        File.Move(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip", updpath + @"\atractool-rel.zip");
                                    }

                                    ProcessStartInfo pi = new()
                                    {
                                        FileName = updpath + @"\updater.exe",
                                        Arguments = null,
                                        UseShellExecute = true,
                                        WindowStyle = ProcessWindowStyle.Normal,
                                    };
                                    Process.Start(pi);
                                    Close();
                                    return;
                                }
                                else
                                {
                                    DialogResult dr2 = MessageBox.Show(this, Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + ver.FileVersion + "\n" + Localization.SiteOpenCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (dr2 == DialogResult.Yes)
                                    {
                                        Common.Utils.OpenURI("https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases");
                                        return;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            case 0:
                                MessageBox.Show(this, Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + ver.FileVersion + "\n" + Localization.UptodateCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case 1:
                                throw new Exception(hv[8..].Replace("\n", "").ToString() + " < " + ver.FileVersion.ToString());
                        }
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, string.Format(Localization.UnExpectedCaption, ex.ToString()), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show(this, Localization.NetworkNotConnectedCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        // ステータスバー

        private void ATRAC3ATRAC3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.Entry["ToolStrip"].Value = "0";
            Config.Save(xmlpath);
            Common.Generic.ATRACFlag = 0;
            aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
            aTRAC9ToolStripMenuItem.Checked = false;
            toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";

        }

        private void ATRAC9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.Entry["ToolStrip"].Value = "1";
            Config.Save(xmlpath);
            Common.Generic.ATRACFlag = 1;
            aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
            aTRAC9ToolStripMenuItem.Checked = true;
            toolStripDropDownButton_EF.Text = "ATRAC9";
        }

        // ボタン

        /// <summary>
        /// Decode ATRAC File(s).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Decode_Click(object sender, EventArgs e)
        {
            Config.Load(xmlpath);

            bool manual = bool.Parse(Config.Entry["Save_IsManual"].Value);
            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");

            toolStripStatusLabel_Status.ForeColor = Color.FromArgb(0, 0, 0, 0);
            toolStripStatusLabel_Status.Text = "Decoding...";

            if (Common.Generic.OpenFilePaths.Length == 1) // 単一ファイル
            {
                switch (manual)
                {
                    case true: // 固定場所に保存
                        {
                            FileInfo fi = new(Generic.OpenFilePaths[0]);
                            string suffix = "";
                            switch (bool.Parse(Config.Entry["Save_IsSubfolder"].Value))
                            {
                                case true:
                                    {
                                        if (Config.Entry["Save_Subfolder_Suffix"].Value != "")
                                        {
                                            suffix = Config.Entry["Save_Subfolder_Suffix"].Value;
                                        }
                                        if (suffix != "")
                                        {
                                            if (!Directory.Exists(Config.Entry["Save_Isfolder"].Value + @"\" + suffix))
                                            {
                                                Directory.CreateDirectory(Config.Entry["Save_Isfolder"].Value + @"\" + suffix);
                                            }
                                        }

                                        Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                        Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav";
                                        Generic.ProgressMax = 1;
                                        break;
                                    }
                                case false:
                                    {
                                        Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                        Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav";
                                        Generic.ProgressMax = 1;
                                        break;
                                    }
                            }
                            break;
                        }
                    case false: // 通常保存
                        {
                            SaveFileDialog sfd = new()
                            {
                                FileName = Common.Utils.SFDRandomNumber(),
                                InitialDirectory = "",
                                Filter = Localization.WAVEFilter,
                                FilterIndex = 1,
                                Title = Localization.SaveDialogTitle,
                                OverwritePrompt = true,
                                RestoreDirectory = true
                            };
                            if (sfd.ShowDialog() == DialogResult.OK)
                            {
                                Common.Generic.SavePath = sfd.FileName;
                                Common.Generic.ProgressMax = 1;
                            }
                            else // Cancelled
                            {
                                ResetStatus();
                                return;
                            }
                            break;
                        }
                }


            }
            else // 複数ファイル
            {
                switch (manual)
                {
                    case true: // 固定場所に保存
                        {
                            string suffix = "";
                            switch (bool.Parse(Config.Entry["Save_IsSubfolder"].Value))
                            {
                                case true:
                                    {
                                        if (Config.Entry["Save_Subfolder_Suffix"].Value != "")
                                        {
                                            suffix = Config.Entry["Save_Subfolder_Suffix"].Value;
                                        }
                                        if (suffix != "")
                                        {
                                            if (!Directory.Exists(Config.Entry["Save_Isfolder"].Value + @"\" + suffix))
                                            {
                                                Directory.CreateDirectory(Config.Entry["Save_Isfolder"].Value + @"\" + suffix);
                                            }
                                        }

                                        Generic.FolderSavePath = Config.Entry["Save_Isfolder"].Value + @"\" + suffix;
                                        if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                        {
                                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                            if (dr == DialogResult.Yes)
                                            {
                                                Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                                        break;
                                    }
                                case false:
                                    {
                                        Generic.FolderSavePath = Config.Entry["Save_Isfolder"].Value;
                                        if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                        {
                                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                            if (dr == DialogResult.Yes)
                                            {
                                                Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                                        break;
                                    }
                            }
                            break;
                        }
                    case false: // 通常保存
                        {
                            FolderBrowserDialog fbd = new()
                            {
                                Description = Localization.FolderSaveDialogTitle,
                                RootFolder = Environment.SpecialFolder.MyDocuments,
                                SelectedPath = @"",
                            };
                            if (fbd.ShowDialog() == DialogResult.OK)
                            {
                                Common.Generic.FolderSavePath = fbd.SelectedPath;
                                if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                {
                                    DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (dr == DialogResult.Yes)
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                            }
                            else // Cancelled
                            {
                                ResetStatus();
                                return;
                            }
                            break;
                        }
                }

            }

            Common.Generic.ProcessFlag = 0;

            Form formProgress = new FormProgress();
            formProgress.ShowDialog();
            formProgress.Dispose();

            if (Common.Generic.Result == false) // 中断
            {
                Common.Generic.cts.Dispose();
                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                if (Common.Generic.OpenFilePaths.Length == 1) // 単一
                {
                    FileInfo fi = new(Common.Generic.SavePath);
                    Common.Generic.cts.Dispose();
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                    {
                        if (File.Exists(Common.Generic.SavePath))
                        {
                            File.Delete(Common.Generic.SavePath);
                        }
                        File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                        if (File.Exists(Common.Generic.SavePath))
                        {
                            if (fi.Length != 0) // OK
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.DecodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ResetStatus();
                                Utils.ShowFolder(Common.Generic.SavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                                return;
                            }
                            else // Error
                            {
                                File.Delete(Common.Generic.SavePath);
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.DecodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                        else // Exception
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }
                    else // Exception
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetStatus();
                        return;
                    }
                }
                else // 複数
                {
                    Common.Generic.cts.Dispose();
                    foreach (var file in Common.Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);
                        if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav")))
                        {
                            if (File.Exists(Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, ".wav")))
                            {
                                File.Delete(Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, ".wav"));
                            }
                            File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav"), Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, ".wav"));
                            continue;
                        }
                        else // Error
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }

                    if (Common.Generic.OpenFilePaths.Length == Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length) // OK
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetStatus();
                        Utils.ShowFolder(Common.Generic.FolderSavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                        return;
                    }
                    else // Error
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetStatus();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Encode ATRAC File(s).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Encode_Click(object sender, EventArgs e)
        {
            Config.Load(xmlpath);
            bool lpc = bool.Parse(Config.Entry["LPC_Create"].Value);
            bool manual = bool.Parse(Config.Entry["Save_IsManual"].Value);
            Common.Generic.lpcreate = lpc switch
            {
                false => false,
                true => true,
            };

            ActivateOrDeactivateLPC(false);

            if (Common.Generic.ATRACFlag == 0 || Common.Generic.ATRACFlag == 1)
            {
                if (Common.Generic.EncodeParamAT3 == "" || Common.Generic.EncodeParamAT3 == null || Common.Generic.EncodeParamAT9 == "" || Common.Generic.EncodeParamAT9 == null)
                {
                    // Param Error
                    MessageBox.Show(this, Localization.SettingsErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else // OK
                {
                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");

                    toolStripStatusLabel_Status.ForeColor = Color.FromArgb(0, 0, 0, 0);
                    toolStripStatusLabel_Status.Text = "Encoding...";

                    if (Common.Generic.OpenFilePaths.Length == 1)
                    {
                        if (manual != true) // 通常保存
                        {
                            switch (Common.Generic.ATRACFlag)
                            {
                                case 0:
                                    {
                                        SaveFileDialog sfd = new()
                                        {
                                            FileName = Common.Utils.SFDRandomNumber(),
                                            InitialDirectory = "",
                                            Filter = Localization.AT3Filter,
                                            FilterIndex = 1,
                                            Title = Localization.SaveDialogTitle,
                                            OverwritePrompt = true,
                                            RestoreDirectory = true
                                        };
                                        if (sfd.ShowDialog() == DialogResult.OK)
                                        {
                                            Common.Generic.SavePath = sfd.FileName;
                                            Common.Generic.ProgressMax = 1;
                                        }
                                        else // Cancelled
                                        {
                                            ResetStatus();
                                            return;
                                        }
                                        break;
                                    }
                                case 1:
                                    {
                                        SaveFileDialog sfd = new()
                                        {
                                            FileName = Common.Utils.SFDRandomNumber(),
                                            InitialDirectory = "",
                                            Filter = Localization.AT9Filter,
                                            FilterIndex = 1,
                                            Title = Localization.SaveDialogTitle,
                                            OverwritePrompt = true,
                                            RestoreDirectory = true
                                        };
                                        if (sfd.ShowDialog() == DialogResult.OK)
                                        {
                                            Common.Generic.SavePath = sfd.FileName;
                                            Common.Generic.ProgressMax = 1;
                                        }
                                        else // Cancelled
                                        {
                                            ResetStatus();
                                            return;
                                        }
                                        break;
                                    }
                            }
                        }
                        else // 固定場所に保存
                        {
                            FileInfo fi = new(Common.Generic.OpenFilePaths[0]);
                            string suffix = "";
                            switch (bool.Parse(Config.Entry["Save_IsSubfolder"].Value))
                            {
                                case true:
                                    {
                                        if (Config.Entry["Save_Subfolder_Suffix"].Value != "")
                                        {
                                            suffix = Config.Entry["Save_Subfolder_Suffix"].Value;
                                        }
                                        if (suffix != "")
                                        {
                                            if (!Directory.Exists(Config.Entry["Save_Isfolder"].Value + @"\" + suffix))
                                            {
                                                Directory.CreateDirectory(Config.Entry["Save_Isfolder"].Value + @"\" + suffix);
                                            }
                                        }

                                        switch (Generic.ATRACFlag)
                                        {
                                            case 0:
                                                Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".at3");
                                                Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".at3";
                                                Generic.ProgressMax = 1;
                                                break;
                                            case 1:
                                                Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".at9");
                                                Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + suffix + @"\" + fi.Name.Replace(fi.Extension, "") + ".at9";
                                                Generic.ProgressMax = 1;
                                                break;
                                        }
                                        break;
                                    }
                                case false:
                                    {
                                        switch (Generic.ATRACFlag)
                                        {
                                            case 0:
                                                Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".at3");
                                                Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".at3";
                                                Generic.ProgressMax = 1;
                                                break;
                                            case 1:
                                                Utils.CheckExistsFile(Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".at9");
                                                Generic.SavePath = Config.Entry["Save_Isfolder"].Value + @"\" + fi.Name.Replace(fi.Extension, "") + ".at9";
                                                Generic.ProgressMax = 1;
                                                break;
                                        }
                                        break;
                                    }
                            }
                        }

                    }
                    else // 複数のファイル
                    {
                        if (manual != true) // 通常保存
                        {
                            FolderBrowserDialog fbd = new()
                            {
                                Description = Localization.FolderSaveDialogTitle,
                                RootFolder = Environment.SpecialFolder.MyDocuments,
                                SelectedPath = @"",
                            };
                            if (fbd.ShowDialog() == DialogResult.OK)
                            {
                                Common.Generic.FolderSavePath = fbd.SelectedPath;
                                if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                {
                                    DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (dr == DialogResult.Yes)
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                            }
                            else // Cancelled
                            {
                                ResetStatus();
                                return;
                            }
                        }
                        else // 固定場所に保存
                        {
                            string suffix = "";
                            switch (bool.Parse(Config.Entry["Save_IsSubfolder"].Value))
                            {
                                case true:
                                    {
                                        if (Config.Entry["Save_Subfolder_Suffix"].Value != "")
                                        {
                                            suffix = Config.Entry["Save_Subfolder_Suffix"].Value;
                                        }
                                        if (suffix != "")
                                        {
                                            if (!Directory.Exists(Config.Entry["Save_Isfolder"].Value + @"\" + suffix))
                                            {
                                                Directory.CreateDirectory(Config.Entry["Save_Isfolder"].Value + @"\" + suffix);
                                            }
                                        }

                                        Generic.FolderSavePath = Config.Entry["Save_Isfolder"].Value + @"\" + suffix;
                                        if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                        {
                                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                            if (dr == DialogResult.Yes)
                                            {
                                                Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                                        break;
                                    }
                                case false:
                                    {
                                        Generic.FolderSavePath = Config.Entry["Save_Isfolder"].Value;
                                        if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                        {
                                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                            if (dr == DialogResult.Yes)
                                            {
                                                Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
                                        break;
                                    }
                            }

                        }

                    }

                    Common.Generic.ProcessFlag = 1;

                    Form formProgress = new FormProgress();
                    formProgress.ShowDialog();
                    formProgress.Dispose();

                    if (Common.Generic.lpcreatev2 != false)
                    {
                        Common.Generic.lpcreatev2 = false;
                    }

                    if (Common.Generic.Result == false)
                    {
                        Common.Generic.cts.Dispose();
                        MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        if (Common.Generic.OpenFilePaths.Length == 1) // 単一
                        {
                            FileInfo fi = new(Common.Generic.SavePath);
                            Common.Generic.cts.Dispose();
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                            {
                                if (File.Exists(Common.Generic.SavePath))
                                {
                                    File.Delete(Common.Generic.SavePath);
                                }
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                if (File.Exists(Common.Generic.SavePath))
                                {
                                    if (fi.Length != 0) // OK
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, Localization.EncodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        ResetStatus();
                                        Utils.ShowFolder(Common.Generic.SavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                                        return;
                                    }
                                    else // Error
                                    {
                                        File.Delete(Common.Generic.SavePath);
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.EncodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        ResetStatus();
                                        return;
                                    }
                                }
                                else // Exception
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }
                            }
                            else // Exception
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                        else // 複数
                        {
                            Common.Generic.cts.Dispose();
                            foreach (var file in Common.Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Common.Generic.ATRACExt)))
                                {
                                    if (File.Exists(Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, Common.Generic.ATRACExt)))
                                    {
                                        File.Delete(Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, Common.Generic.ATRACExt));
                                    }
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Common.Generic.ATRACExt), Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, Common.Generic.ATRACExt));
                                    if (File.Exists(Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, Common.Generic.ATRACExt)))
                                    {
                                        FileInfo fi2 = new(Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, Common.Generic.ATRACExt));
                                        if (fi2.Length != 0)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            File.Delete(fi2.FullName);
                                            continue;
                                        }
                                    }
                                    else // Error
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath + @"\");
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        ResetStatus();
                                        return;
                                    }
                                }
                                else // Exception
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }
                            }

                            if (Common.Generic.OpenFilePaths.Length == Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length) // OK
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.EncodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ResetStatus();
                                Utils.ShowFolder(Common.Generic.FolderSavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                                return;
                            }
                            else if (Common.Generic.OpenFilePaths.Length > Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length && Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0) // 一部変換失敗
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.EncodePartialCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetStatus();
                                Utils.ShowFolder(Common.Generic.FolderSavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                                return;
                            }
                            else // Error
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                // Select Error
                MessageBox.Show(this, Localization.EncodemethodErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private void ReadStatus()
        {
            AllowDrop = false;
            toolStripStatusLabel_Status.Text = Localization.ReadyCaption;
            toolStripStatusLabel_Status.ForeColor = Color.Green;
            label_NotReaded.Visible = false;
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            label_Filepath.Visible = true;
            label_Sizetxt.Visible = true;
            label_Formattxt.Visible = true;
        }

        private void ResetStatus()
        {
            AllowDrop = true;
            Utils.ATWCheck(Generic.IsATW);
            Common.Generic.OpenFilePaths = null!;
            Common.Generic.ProcessFlag = -1;
            Common.Generic.ProgressMax = -1;
            if (panel_Main.BackgroundImage is not null)
            {
                panel_Main.BackgroundImage.Dispose();
            }
            panel_Main.BackgroundImage = null!;
            button_Decode.Enabled = false;
            button_Encode.Enabled = false;
            toolStripStatusLabel_Status.Text = Localization.NotReadyCaption;
            toolStripStatusLabel_Status.ForeColor = Color.Red;
            label_NotReaded.Text = Localization.OpenFileCaption;
            label_NotReaded.Visible = true;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            label_Filepath.Visible = false;
            label_Sizetxt.Visible = false;
            label_Formattxt.Visible = false;
            toolStripDropDownButton_EF.Enabled = false;
            toolStripDropDownButton_EF.Visible = false;
            loopPointCreationToolStripMenuItem.Enabled = false;
            closeFileCToolStripMenuItem.Enabled = false;
        }

        private void AudioToWAVEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = Localization.ConverterFilters,
                FilterIndex = 15,
                Title = Localization.OpenDialogTitle,
                Multiselect = true,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<string> lst = new();
                foreach (string files in ofd.FileNames)
                {
                    lst.Add(files);
                }
                Common.Generic.OpenFilePaths = lst.ToArray();

                if (Common.Generic.OpenFilePaths.Length == 1) // Single
                {
                    if (bool.Parse(Config.Entry["FixedConvert"].Value)) // Fix
                    {
                        Common.Generic.WTAmethod = int.Parse(Config.Entry["ConvertType"].Value);
                        SaveFileDialog sfd = new()
                        {
                            FileName = Common.Utils.SFDRandomNumber(),
                            InitialDirectory = "",
                            Filter = Localization.WAVEFilter,
                            FilterIndex = 1,
                            Title = Localization.SaveDialogTitle,
                            OverwritePrompt = true,
                            RestoreDirectory = true
                        };
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            Common.Generic.SavePath = sfd.FileName;
                            Common.Generic.ProgressMax = 1;

                            Common.Generic.ProcessFlag = 2;

                            Form formProgress = new FormProgress();
                            formProgress.ShowDialog();
                            formProgress.Dispose();

                            if (Common.Generic.Result == false)
                            {
                                Common.Generic.cts.Dispose();
                                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            FileInfo fi = new(Common.Generic.SavePath);
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                            {
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ResetStatus();
                                Utils.ShowFolder(Common.Generic.SavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                                return;
                            }
                            else // Error
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                        else // Cancelled
                        {
                            return;
                        }
                    }
                    else // normal
                    {
                        using Form formAtWST = new FormAtWSelectTarget();
                        DialogResult dr = formAtWST.ShowDialog();
                        if (dr != DialogResult.Cancel && dr != DialogResult.None)
                        {
                            SaveFileDialog sfd = new()
                            {
                                FileName = Common.Utils.SFDRandomNumber(),
                                InitialDirectory = "",
                                Filter = Localization.WAVEFilter,
                                FilterIndex = 1,
                                Title = Localization.SaveDialogTitle,
                                OverwritePrompt = true,
                                RestoreDirectory = true
                            };
                            if (sfd.ShowDialog() == DialogResult.OK)
                            {
                                Common.Generic.SavePath = sfd.FileName;
                                Common.Generic.ProgressMax = 1;

                                Common.Generic.ProcessFlag = 2;

                                Form formProgress = new FormProgress();
                                formProgress.ShowDialog();
                                formProgress.Dispose();

                                if (Common.Generic.Result == false)
                                {
                                    Common.Generic.cts.Dispose();
                                    MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                FileInfo fi = new(Common.Generic.SavePath);
                                if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                                {
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    ResetStatus();
                                    Utils.ShowFolder(Common.Generic.SavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                                    return;
                                }
                                else // Error
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }
                            }
                            else // Cancelled
                            {
                                return;
                            }
                        }
                        else { return; }
                    }
                }
                else // Multiple
                {
                    if (bool.Parse(Config.Entry["FixedConvert"].Value)) // Fix
                    {
                        Common.Generic.WTAmethod = int.Parse(Config.Entry["ConvertType"].Value);
                        FolderBrowserDialog fbd = new()
                        {
                            Description = Localization.FolderSaveDialogTitle,
                            RootFolder = Environment.SpecialFolder.MyDocuments,
                            SelectedPath = @"",
                        };
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            Common.Generic.FolderSavePath = fbd.SelectedPath;
                            if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                            {
                                DialogResult dr2 = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (dr2 == DialogResult.Yes)
                                {
                                    Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                }
                                else
                                {
                                    return;
                                }
                            }
                            Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                            Common.Generic.ProcessFlag = 2;

                            Form formProgress = new FormProgress();
                            formProgress.ShowDialog();
                            formProgress.Dispose();

                            if (Common.Generic.Result == false)
                            {
                                Common.Generic.cts.Dispose();
                                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            foreach (var file in Common.Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav"))
                                {
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav", Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                }
                                else // Error
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }
                            }
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ResetStatus();
                            Utils.ShowFolder(Common.Generic.FolderSavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                            return;
                        }
                        else // Cancelled
                        {
                            return;
                        }
                    }
                    else // normal
                    {
                        using Form formAtWST = new FormAtWSelectTarget();
                        DialogResult dr = formAtWST.ShowDialog();
                        if (dr != DialogResult.Cancel && dr != DialogResult.None)
                        {
                            FolderBrowserDialog fbd = new()
                            {
                                Description = Localization.FolderSaveDialogTitle,
                                RootFolder = Environment.SpecialFolder.MyDocuments,
                                SelectedPath = @"",
                            };
                            if (fbd.ShowDialog() == DialogResult.OK)
                            {
                                Common.Generic.FolderSavePath = fbd.SelectedPath;
                                if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                                {
                                    DialogResult dr2 = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (dr2 == DialogResult.Yes)
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                                Common.Generic.ProcessFlag = 2;

                                Form formProgress = new FormProgress();
                                formProgress.ShowDialog();
                                formProgress.Dispose();

                                if (Common.Generic.Result == false)
                                {
                                    Common.Generic.cts.Dispose();
                                    MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                foreach (var file in Common.Generic.OpenFilePaths)
                                {
                                    FileInfo fi = new(file);
                                    if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav"))
                                    {
                                        File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav", Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                    }
                                    else // Error
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        ResetStatus();
                                        return;
                                    }
                                }
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ResetStatus();
                                Utils.ShowFolder(Common.Generic.FolderSavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                                return;
                            }
                            else // Cancelled
                            {
                                return;
                            }
                        }
                        else { return; }
                    }
                }
            }
            else // Cancelled
            {
                return;
            }
        }

        private void WAVEToAudioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = Localization.WAVEFilter,
                FilterIndex = 0,
                Title = Localization.OpenDialogTitle,
                Multiselect = true,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<string> lst = new();
                foreach (string files in ofd.FileNames)
                {
                    lst.Add(files);
                }
                Common.Generic.OpenFilePaths = lst.ToArray();

                if (Common.Generic.OpenFilePaths.Length == 1) // Single
                {
                    SaveFileDialog sfd = new()
                    {
                        FileName = Common.Utils.SFDRandomNumber(),
                        InitialDirectory = "",
                        Filter = Localization.ConverterFilters,
                        FilterIndex = 14,
                        Title = Localization.SaveDialogTitle,
                        OverwritePrompt = true,
                        RestoreDirectory = true
                    };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        Common.Generic.SavePath = sfd.FileName;
                        Common.Generic.ProgressMax = 1;

                        Common.Generic.ProcessFlag = 3;

                        Form formProgress = new FormProgress();
                        formProgress.ShowDialog();
                        formProgress.Dispose();

                        if (Common.Generic.Result == false)
                        {
                            Common.Generic.cts.Dispose();
                            MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        FileInfo fi = new(Common.Generic.SavePath);
                        if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                        {
                            File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ResetStatus();

                            Utils.ShowFolder(Common.Generic.SavePath, bool.Parse(Config.Entry["ShowFolder"].Value));

                            return;
                        }
                        else // Error
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }
                    else // Cancelled
                    {
                        return;
                    }
                }
                else // Multiple
                {
                    FolderBrowserDialog fbd = new()
                    {
                        Description = Localization.FolderSaveDialogTitle,
                        RootFolder = Environment.SpecialFolder.MyDocuments,
                        SelectedPath = @"",
                    };
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        Common.Generic.FolderSavePath = fbd.SelectedPath;
                        if (Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                        {
                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (dr == DialogResult.Yes)
                            {
                                Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath);
                            }
                            else
                            {
                                return;
                            }
                        }

                        Form formATWSelect = new FormATWSelect();
                        if (formATWSelect.ShowDialog() == DialogResult.OK)
                        {
                            Common.Utils.SetWTAFormat(Common.Generic.WTAFlag);
                            formATWSelect.Dispose();
                        }
                        else // Cancelled
                        {
                            return;
                        }

                        Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                        Common.Generic.ProcessFlag = 3;

                        Form formProgress = new FormProgress();
                        formProgress.ShowDialog();
                        formProgress.Dispose();

                        if (Common.Generic.Result == false)
                        {
                            Common.Generic.cts.Dispose();
                            MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        foreach (var file in Common.Generic.OpenFilePaths)
                        {
                            FileInfo fi = new(file);
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Common.Generic.WTAFmt))
                            {
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Common.Generic.WTAFmt, Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, "") + Common.Generic.WTAFmt);
                            }
                            else // Error
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.ConvertSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetStatus();

                        Utils.ShowFolder(Common.Generic.FolderSavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                        return;
                    }
                    else // Cancelled
                    {
                        return;
                    }
                }
            }
            else // Cancelled
            {
                return;
            }
        }

        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            Utils.ATWCheck(Generic.IsATW);

            if (e.Data != null)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var check in files)
                {
                    FileInfo file = new(check);
                    switch (file.Extension.ToUpper())
                    {
                        case ".WAV":
                            continue;
                        case ".MP3":
                            continue;
                        case ".M4A":
                            continue;
                        case ".AAC":
                            continue;
                        case ".AIFF":
                            continue;
                        case ".ALAC":
                            continue;
                        case ".FLAC":
                            continue;
                        case ".OGG":
                            continue;
                        case ".OPUS":
                            continue;
                        case ".WMA":
                            continue;
                        case ".AT3":
                            continue;
                        case ".AT9":
                            continue;
                        default:
                            MessageBox.Show(this, Localization.NotAllowedExtensionCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                    }
                }

                List<string> lst = new();
                foreach (string fp in files)
                {
                    lst.Add(fp);
                }
                Common.Generic.OpenFilePaths = lst.ToArray();

                if (Common.Generic.OpenFilePaths.Length == 1)
                {
                    FileInfo file = new(files[0]);
                    long FileSize = file.Length;
                    ReadStatus();
                    label_Filepath.Text = file.FullName;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FileSize / 1024);
                    switch (file.Extension.ToUpper())
                    {
                        case ".WAV":
                            FormatSorter(true);
                            break;
                        case ".MP3":
                            FormatSorter(true, true);
                            break;
                        case ".M4A":
                            FormatSorter(true, true);
                            break;
                        case ".AAC":
                            FormatSorter(true, true);
                            break;
                        case ".FLAC":
                            FormatSorter(true, true);
                            break;
                        case ".ALAC":
                            FormatSorter(true, true);
                            break;
                        case ".AIFF":
                            FormatSorter(true, true);
                            break;
                        case ".OGG":
                            FormatSorter(true, true);
                            break;
                        case ".OPUS":
                            FormatSorter(true, true);
                            break;
                        case ".WMA":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    closeFileCToolStripMenuItem.Enabled = true;
                    return;
                }
                else
                {
                    long FS = 0;
                    foreach (string file in files)
                    {
                        FileInfo fi = new(file);
                        FS += fi.Length;
                    }

                    string Ft = "";
                    int count = 0;

                    foreach (var file in Common.Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);

                        if (count != 0)
                        {
                            if (Ft != fi.Extension)
                            {
                                MessageBox.Show(this, Localization.FileMixedErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                closeFileCToolStripMenuItem.Enabled = false;
                                toolStripDropDownButton_EF.Enabled = false;
                                toolStripDropDownButton_EF.Visible = false;
                                button_Decode.Enabled = false;
                                button_Encode.Enabled = false;
                                loopPointCreationToolStripMenuItem.Enabled = false;
                                return;
                            }
                        }
                        else
                        {
                            Ft = fi.Extension;
                        }
                        count++;
                    }

                    ReadStatus();
                    label_Filepath.Text = Localization.MultipleFilesCaption;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024);

                    closeFileCToolStripMenuItem.Enabled = true;

                    switch (Ft.ToUpper())
                    {
                        case ".WAV":
                            FormatSorter(true);
                            break;
                        case ".MP3":
                            //Ft = ".wav";
                            FormatSorter(true, true);
                            break;
                        case ".M4A":
                            //Ft = ".wav";
                            FormatSorter(true, true);
                            break;
                        case ".AAC":
                            //Ft = ".wav";
                            FormatSorter(true, true);
                            break;
                        case ".FLAC":
                            //Ft = ".wav";
                            FormatSorter(true, true);
                            break;
                        case ".ALAC":
                            //Ft = ".wav";
                            FormatSorter(true, true);
                            break;
                        case ".AIFF":
                            //Ft = ".wav";
                            FormatSorter(true, true);
                            break;
                        case ".OGG":
                            //Ft = ".wav";
                            FormatSorter(true, true);
                            break;
                        case ".OPUS":
                            //Ft = ".wav";
                            FormatSorter(true, true);
                            break;
                        case ".WMA":
                            //Ft = ".wav";
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            ActivateOrDeactivateLPC(false);
            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
            Utils.ATWCheck(Generic.IsATW);
            Directory.Delete(Directory.GetCurrentDirectory() + @"\_temp");
        }

        private void LoopPointCreationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using FormLPC form = new(true);
            form.ShowDialog();
        }

        private async Task CheckForUpdatesForInit()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    string hv = null!;

                    using Stream hcs = await Task.Run(() => Common.Network.GetWebStreamAsync(appUpdatechecker, Common.Network.GetUri("https://raw.githubusercontent.com/XyLe-GBP/ATRACTool-Reloaded/master/VERSIONINFO")));
                    using StreamReader hsr = new(hcs);
                    hv = await Task.Run(() => hsr.ReadToEndAsync());
                    Common.Generic.GitHubLatestVersion = hv[8..].Replace("\n", "");

                    FileVersionInfo ver = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

                    if (ver.FileVersion != null)
                    {
                        switch (ver.FileVersion.ToString().CompareTo(hv[8..].Replace("\n", "")))
                        {
                            case -1:
                                DialogResult dr = MessageBox.Show(Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + ver.FileVersion + "\n" + Localization.UpdateConfirmCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (dr == DialogResult.Yes)
                                {
                                    using FormUpdateApplicationType fuat = new();
                                    fuat.ShowDialog();

                                    Common.Generic.ProcessFlag = 4;
                                    Common.Generic.ProgressMax = 100;
                                    using FormProgress form = new();
                                    form.ShowDialog();

                                    if (Common.Generic.Result == false)
                                    {
                                        Common.Generic.cts.Dispose();
                                        MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }

                                    string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];
                                    File.Move(Directory.GetCurrentDirectory() + @"\res\updater.exe", updpath + @"\updater.exe");
                                    string wtext;
                                    switch (Common.Generic.ApplicationPortable)
                                    {
                                        case false:
                                            {
                                                wtext = Directory.GetCurrentDirectory() + "\r\nrelease";
                                            }
                                            break;
                                        case true:
                                            {
                                                wtext = Directory.GetCurrentDirectory() + "\r\nportable";
                                            }
                                            break;
                                    }
                                    File.WriteAllText(updpath + @"\updater.txt", wtext);
                                    File.Move(updpath + @"\updater.txt", updpath + @"\updater.dat");
                                    if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                    {
                                        File.Move(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip", updpath + @"\atractool-rel.zip");
                                    }

                                    ProcessStartInfo pi = new()
                                    {
                                        FileName = updpath + @"\updater.exe",
                                        Arguments = null,
                                        UseShellExecute = true,
                                        WindowStyle = ProcessWindowStyle.Normal,
                                    };
                                    Process.Start(pi);
                                    Close();
                                    return;
                                }
                                else
                                {
                                    DialogResult dr2 = MessageBox.Show(Localization.LatestCaption + hv[8..].Replace("\n", "") + "\n" + Localization.CurrentCaption + ver.FileVersion + "\n" + Localization.SiteOpenCaption, Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (dr2 == DialogResult.Yes)
                                    {
                                        Common.Utils.OpenURI("https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases");
                                        return;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            case 0:
                                break;
                            case 1:
                                throw new Exception(hv[8..].Replace("\n", "").ToString() + " < " + ver.FileVersion.ToString());
                        }
                        return;
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Waveではない音声ファイルをWaveに変換する
        /// </summary>
        private bool AudioToWaveConvert()
        {
            if (Common.Generic.IsWave != true && Common.Generic.IsATRAC != true)
            {
                if (Common.Generic.OpenFilePaths.Length == 1) // 単一ファイル
                {
                    if (bool.Parse(Config.Entry["FixedConvert"].Value)) // Fix
                    {
                        FileInfo file = new(Common.Generic.OpenFilePaths[0]);
                        Common.Generic.WTAmethod = int.Parse(Config.Entry["ConvertType"].Value);

                        Common.Generic.SavePath = file.Directory + @"\" + file.Name + @".wav";
                        Common.Generic.ProgressMax = 1;

                        Common.Generic.ProcessFlag = 2;

                        Form formProgress = new FormProgress();
                        formProgress.ShowDialog();
                        formProgress.Dispose();

                        if (Common.Generic.Result == false)
                        {
                            Common.Generic.cts.Dispose();
                            MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ResetStatus();
                            return false;
                        }

                        FileInfo fi = new(Common.Generic.SavePath);
                        if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                        {
                            if (File.Exists(Common.Generic.SavePath))  // ファイルが既に存在する場合は削除してからMoveする
                            {
                                File.Delete(Common.Generic.SavePath);
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            }
                            else
                            {
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            }

                            if (File.Exists(Common.Generic.SavePath)) // ファイルが生成されているかどうか
                            {
                                Common.Generic.OpenFilePaths[0] = Common.Generic.SavePath;
                                label_Filepath.Text = Common.Generic.OpenFilePaths[0];
                            }
                            else // エラー
                            {
                                ResetStatus();
                                MessageBox.Show(Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }

                            switch (Common.Generic.WTAmethod)
                            {
                                case 0:
                                    Config.Entry["ATRAC3_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "0";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 0;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                    aTRAC9ToolStripMenuItem.Checked = false;
                                    toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                    break;
                                case 1:
                                    Config.Entry["ATRAC3_Console"].Value = "1";
                                    Config.Entry["ToolStrip"].Value = "0";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 0;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                    aTRAC9ToolStripMenuItem.Checked = false;
                                    toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                    break;
                                case 2:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                                case 3:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                            }
                            return true;
                        }
                        else // Error
                        {
                            ResetStatus();
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    else // normal
                    {
                        FileInfo file = new(Common.Generic.OpenFilePaths[0]);
                        using Form formAtWST = new FormAtWSelectTarget();
                        DialogResult dr = formAtWST.ShowDialog();
                        if (dr != DialogResult.Cancel && dr != DialogResult.None)
                        {
                            Common.Generic.SavePath = file.Directory + @"\" + file.Name + @".wav";
                            Common.Generic.ProgressMax = 1;

                            Common.Generic.ProcessFlag = 2;

                            Form formProgress = new FormProgress();
                            formProgress.ShowDialog();
                            formProgress.Dispose();

                            if (Common.Generic.Result == false)
                            {
                                Common.Generic.cts.Dispose();
                                MessageBox.Show(Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetStatus();
                                return false;
                            }

                            FileInfo fi = new(Common.Generic.SavePath);
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                            {
                                if (File.Exists(Common.Generic.SavePath))  // ファイルが既に存在する場合は削除してからMoveする
                                {
                                    File.Delete(Common.Generic.SavePath);
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                }
                                else
                                {
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                }

                                if (File.Exists(Common.Generic.SavePath)) // ファイルが生成されているかどうか
                                {
                                    Common.Generic.OpenFilePaths[0] = Common.Generic.SavePath;
                                    label_Filepath.Text = Common.Generic.OpenFilePaths[0];
                                }
                                else // エラー
                                {
                                    ResetStatus();
                                    MessageBox.Show(Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return false;
                                }

                                switch (Common.Generic.WTAmethod)
                                {
                                    case 0:
                                        Config.Entry["ATRAC3_Console"].Value = "0";
                                        Config.Entry["ToolStrip"].Value = "0";
                                        Config.Save(xmlpath);
                                        Common.Generic.ATRACFlag = 0;
                                        aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                        aTRAC9ToolStripMenuItem.Checked = false;
                                        toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                        break;
                                    case 1:
                                        Config.Entry["ATRAC3_Console"].Value = "1";
                                        Config.Entry["ToolStrip"].Value = "0";
                                        Config.Save(xmlpath);
                                        Common.Generic.ATRACFlag = 0;
                                        aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                        aTRAC9ToolStripMenuItem.Checked = false;
                                        toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                        break;
                                    case 2:
                                        Config.Entry["ATRAC9_Console"].Value = "0";
                                        Config.Entry["ToolStrip"].Value = "1";
                                        Config.Save(xmlpath);
                                        Common.Generic.ATRACFlag = 1;
                                        aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                        aTRAC9ToolStripMenuItem.Checked = true;
                                        toolStripDropDownButton_EF.Text = "ATRAC9";
                                        break;
                                    case 3:
                                        Config.Entry["ATRAC9_Console"].Value = "0";
                                        Config.Entry["ToolStrip"].Value = "1";
                                        Config.Save(xmlpath);
                                        Common.Generic.ATRACFlag = 1;
                                        aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                        aTRAC9ToolStripMenuItem.Checked = true;
                                        toolStripDropDownButton_EF.Text = "ATRAC9";
                                        break;
                                }
                                return true;
                            }
                            else // Error
                            {
                                ResetStatus();
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                        }
                        else
                        {
                            Generic.IsATW = false;
                            ResetStatus();
                            return false;
                        }
                    }
                }
                else // 複数ファイル
                {
                    if (bool.Parse(Config.Entry["FixedConvert"].Value)) // Fix
                    {
                        FileInfo fp = new(Common.Generic.OpenFilePaths[0]);
                        Common.Generic.WTAmethod = int.Parse(Config.Entry["ConvertType"].Value);

                        Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                        Common.Generic.ProcessFlag = 2;

                        Form formProgress = new FormProgress();
                        formProgress.ShowDialog();
                        formProgress.Dispose();

                        if (Common.Generic.Result == false)
                        {
                            Common.Generic.cts.Dispose();
                            MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ResetStatus();
                            return false;
                        }

                        int count = 0;
                        foreach (var file in Common.Generic.OpenFilePaths)
                        {
                            FileInfo fi = new(file);
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav"))
                            {
                                if (File.Exists(fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav"))  // ファイルが既に存在する場合は削除してからMoveする
                                {
                                    File.Delete(fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav", fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                }
                                else
                                {
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav", fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                }

                            }
                            else // Error
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return false;
                            }

                            if (File.Exists(fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav")) // ファイル存在確認
                            {
                                Common.Generic.OpenFilePaths[count] = fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav";
                                count++;
                            }
                            else // エラー
                            {
                                ResetStatus();
                                return false;
                            }
                        }
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");

                        switch (Common.Generic.WTAmethod)
                        {
                            case 0:
                                Config.Entry["ATRAC3_Console"].Value = "0";
                                Config.Entry["ToolStrip"].Value = "0";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 0;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                aTRAC9ToolStripMenuItem.Checked = false;
                                toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                break;
                            case 1:
                                Config.Entry["ATRAC3_Console"].Value = "1";
                                Config.Entry["ToolStrip"].Value = "0";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 0;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                aTRAC9ToolStripMenuItem.Checked = false;
                                toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                break;
                            case 2:
                                Config.Entry["ATRAC9_Console"].Value = "0";
                                Config.Entry["ToolStrip"].Value = "1";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 1;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = true;
                                toolStripDropDownButton_EF.Text = "ATRAC9";
                                break;
                            case 3:
                                Config.Entry["ATRAC9_Console"].Value = "0";
                                Config.Entry["ToolStrip"].Value = "1";
                                Config.Save(xmlpath);
                                Common.Generic.ATRACFlag = 1;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = true;
                                toolStripDropDownButton_EF.Text = "ATRAC9";
                                break;
                        }

                        return true;
                    }
                    else // normal
                    {
                        FileInfo fp = new(Common.Generic.OpenFilePaths[0]);
                        using Form formAtWST = new FormAtWSelectTarget();
                        DialogResult dr = formAtWST.ShowDialog();
                        if (dr != DialogResult.Cancel && dr != DialogResult.None)
                        {
                            Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                            Common.Generic.ProcessFlag = 2;

                            Form formProgress = new FormProgress();
                            formProgress.ShowDialog();
                            formProgress.Dispose();

                            if (Common.Generic.Result == false)
                            {
                                Common.Generic.cts.Dispose();
                                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetStatus();
                                return false;
                            }

                            int count = 0;
                            foreach (var file in Common.Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav"))
                                {
                                    if (File.Exists(fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav"))  // ファイルが既に存在する場合は削除してからMoveする
                                    {
                                        File.Delete(fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                        File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav", fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                    }
                                    else
                                    {
                                        File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + ".wav", fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav");
                                    }

                                }
                                else // Error
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return false;
                                }

                                if (File.Exists(fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav")) // ファイル存在確認
                                {
                                    Common.Generic.OpenFilePaths[count] = fp.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + ".wav";
                                    count++;
                                }
                                else // エラー
                                {
                                    ResetStatus();
                                    return false;
                                }
                            }
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");

                            switch (Common.Generic.WTAmethod)
                            {
                                case 0:
                                    Config.Entry["ATRAC3_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "0";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 0;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                    aTRAC9ToolStripMenuItem.Checked = false;
                                    toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                    break;
                                case 1:
                                    Config.Entry["ATRAC3_Console"].Value = "1";
                                    Config.Entry["ToolStrip"].Value = "0";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 0;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                    aTRAC9ToolStripMenuItem.Checked = false;
                                    toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                    break;
                                case 2:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                                case 3:
                                    Config.Entry["ATRAC9_Console"].Value = "0";
                                    Config.Entry["ToolStrip"].Value = "1";
                                    Config.Save(xmlpath);
                                    Common.Generic.ATRACFlag = 1;
                                    aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                    aTRAC9ToolStripMenuItem.Checked = true;
                                    toolStripDropDownButton_EF.Text = "ATRAC9";
                                    break;
                            }

                            return true;
                        }
                        else
                        {
                            Generic.IsATW = false;
                            ResetStatus();
                            return false;
                        }
                    }

                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ファイル形式に応じて動作を変更
        /// </summary>
        /// <param name="IsEncode">エンコード対象か否か</param>
        /// <param name="IsNotWave">Waveファイルか否か</param>
        private void FormatSorter(bool IsEncode, bool IsNotWave = false)
        {
            if (IsEncode != false)
            {
                if (IsNotWave != true) // Wave
                {
                    Common.Generic.IsWave = true;
                    Common.Generic.IsATRAC = false;
                    Generic.IsATW = false;
                    label_Formattxt.Text = Localization.WAVEFormatCaption;
                    toolStripDropDownButton_EF.Enabled = true;
                    toolStripDropDownButton_EF.Visible = true;
                    button_Decode.Enabled = false;
                    button_Encode.Enabled = true;
                    //loopPointCreationToolStripMenuItem.Enabled = true;
                    ActivateOrDeactivateLPC(true);
                }
                else // NotWave
                {
                    toolStripStatusLabel_Status.ForeColor = Color.Orange;
                    toolStripStatusLabel_Status.Text = Localization.InitializationCaption;
                    label_Formattxt.Text = Localization.InitializationCaption;
                    Common.Generic.IsWave = false;
                    Common.Generic.IsATRAC = false;
                    Generic.IsATW = true;
                    if (AudioToWaveConvert())
                    {
                        toolStripStatusLabel_Status.ForeColor = Color.Green;
                        toolStripStatusLabel_Status.Text = Localization.ReadyCaption;
                        ActivateOrDeactivateLPC(true);
                        label_Formattxt.Text = Localization.WAVEConvertedFormatCaption;
                        toolStripDropDownButton_EF.Enabled = true;
                        toolStripDropDownButton_EF.Visible = true;
                        button_Decode.Enabled = false;
                        button_Encode.Enabled = true;
                        //loopPointCreationToolStripMenuItem.Enabled = true;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else // ATRAC
            {
                if (bool.Parse(Config.Entry["FasterATRAC"].Value))
                {
                    panel_Main.BackgroundImage = Resources.SIEv2;
                    Common.Generic.IsWave = false;
                    Common.Generic.IsATRAC = true;
                    Generic.IsATW = false;
                    toolStripDropDownButton_EF.Enabled = false;
                    toolStripDropDownButton_EF.Visible = false;
                    button_Decode.Enabled = true;
                    button_Encode.Enabled = false;
                    loopPointCreationToolStripMenuItem.Enabled = false;
                    button_Decode.PerformClick();
                }
                else
                {
                    panel_Main.BackgroundImage = Resources.SIEv2;
                    Common.Generic.IsWave = false;
                    Common.Generic.IsATRAC = true;
                    Generic.IsATW = false;
                    toolStripDropDownButton_EF.Enabled = false;
                    toolStripDropDownButton_EF.Visible = false;
                    button_Decode.Enabled = true;
                    button_Encode.Enabled = false;
                    loopPointCreationToolStripMenuItem.Enabled = false;
                }
            }
        }

        #region SplashScreenCommon
        private static void StartThread()
        {
            fs = new FormSplash();
            Application.Run(fs);
        }


        private static void CloseSplash()
        {
            dop d = new(CloseForm);
            if (fs != null)
            {
                fs.Invoke(d);
            }
        }

        private delegate void dop();
        private static void CloseForm()
        {
            fs.Close();
        }

        private delegate void dmes(string message);
        private static void ShowMessage(string message)
        {
            fs.label_log.Text = message;
        }
        #endregion

        private void PreferencesMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using Form FSS = new FormPreferencesSettings();
            FSS.ShowDialog();
        }

        private void ActivateOrDeactivateLPC(bool flag)
        {
            if (Generic.OpenFilePaths is null) { return; }
            if (flag)
            {
                FLPC = new(false)
                {
                    TopLevel = false
                };
                panel_Main.Controls.Add(FLPC);
                FLPC.Show();
            }
            else
            {
                if (FLPC is not null && FLPC.Visible)
                {
                    FLPC.Close();
                }
            }

        }
    }
}