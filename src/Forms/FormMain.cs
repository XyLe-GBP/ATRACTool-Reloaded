using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using ATRACTool_Reloaded.Localizable;

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

            using FormSplash splash = new();
            splash.Show();
            splash.Refresh();

            foreach (var files in Directory.GetFiles(Directory.GetCurrentDirectory() + @"\res", "*", SearchOption.AllDirectories))
            {
                FileInfo fi = new(files);
                RefleshSplashForm(splash, string.Format(Localization.SplashFormFileCaption, fi.Name));
            }

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\_temp");
            ResetStatus();

            RefleshSplashForm(splash, Localization.SplashFormConfigCaption);

            int ts = Common.Utils.GetIntForIniFile("OTHERS", "ToolStrip", 65535);
            string prm1 = Common.Utils.GetStringForIniFile("ATRAC3_SETTINGS", "Param"), prm2 = Common.Utils.GetStringForIniFile("ATRAC9_SETTINGS", "Param");
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

            RefleshSplashForm(splash, Localization.SplashFormUpdateCaption);

            if (File.Exists(Directory.GetCurrentDirectory() + @"\updated.dat"))
            {
                RefleshSplashForm(splash, Localization.SplashFormUpdatingCaption);
                File.Delete(Directory.GetCurrentDirectory() + @"\updated.dat");
                string updpath = Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().LastIndexOf('\\')];
                File.Delete(updpath + @"\updater.exe");
                File.Delete(updpath + @"\atractool-rel.zip");
                Common.Utils.DeleteDirectory(updpath + @"\updater-temp");

                RefleshSplashForm(splash, Localization.SplashFormUpdatedCaption);
                MessageBox.Show(Localization.UpdateCompletedCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var update = Task.Run(() => CheckForUpdatesForInit());
                update.Wait();
            }

            splash.Close();
            Activate();
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
                FilterIndex = 11,
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
                    FileInfo file = new(ofd.FileName);
                    long FileSize = file.Length;
                    ReadStatus();
                    label_Filepath.Text = ofd.FileName;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FileSize / 1024);
                    switch (file.Extension.ToUpper())
                    {
                        case ".WAV":
                            label_Formattxt.Text = Localization.WAVEFormatCaption;
                            toolStripDropDownButton_EF.Enabled = true;
                            toolStripDropDownButton_EF.Visible = true;
                            button_Decode.Enabled = false;
                            button_Encode.Enabled = true;
                            loopPointCreationToolStripMenuItem.Enabled = true;
                            break;
                        case ".AT3":
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            toolStripDropDownButton_EF.Enabled = false;
                            toolStripDropDownButton_EF.Visible = false;
                            button_Decode.Enabled = true;
                            button_Encode.Enabled = false;
                            loopPointCreationToolStripMenuItem.Enabled = false;
                            break;
                        case ".AT9":
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            toolStripDropDownButton_EF.Enabled = false;
                            toolStripDropDownButton_EF.Visible = false;
                            button_Decode.Enabled = true;
                            button_Encode.Enabled = false;
                            loopPointCreationToolStripMenuItem.Enabled = false;
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
                            switch (fi.Extension.ToUpper())
                            {
                                case ".WAV":
                                    label_Formattxt.Text = Localization.WAVEFormatCaption;
                                    toolStripDropDownButton_EF.Enabled = true;
                                    toolStripDropDownButton_EF.Visible = true;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = true;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    break;
                                case ".AT3":
                                    label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    button_Decode.Enabled = true;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    break;
                                case ".AT9":
                                    label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    button_Decode.Enabled = true;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    break;
                            }
                        }
                        count++;
                    }

                    ReadStatus();
                    label_Filepath.Text = Localization.MultipleFilesCaption;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024);

                    closeFileCToolStripMenuItem.Enabled = true;
                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void CloseFileCToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

            string prm1 = Common.Utils.GetStringForIniFile("ATRAC3_SETTINGS", "Param"), prm2 = Common.Utils.GetStringForIniFile("ATRAC9_SETTINGS", "Param");
            int lpc = Common.Utils.GetIntForIniFile("GENERIC", "LPCreateIndex");

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
            if (lpc != 65535)
            {
                Common.Generic.lpcreate = lpc switch
                {
                    0 => false,
                    1 => true,
                    _ => false,
                };
            }
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
            Common.Utils.WriteStringForIniFile("OTHERS", "ToolStrip", "0");
            Common.Generic.ATRACFlag = 0;
            aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
            aTRAC9ToolStripMenuItem.Checked = false;
            toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
            
        }

        private void ATRAC9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Common.Utils.WriteStringForIniFile("OTHERS", "ToolStrip", "1");
            Common.Generic.ATRACFlag = 1;
            aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
            aTRAC9ToolStripMenuItem.Checked = true;
            toolStripDropDownButton_EF.Text = "ATRAC9";
        }

        // ボタン

        private void Button_Decode_Click(object sender, EventArgs e)
        {
            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");

            toolStripStatusLabel_Status.ForeColor = Color.FromArgb(0, 0, 0, 0);
            toolStripStatusLabel_Status.Text = "Decoding...";

            if (Common.Generic.OpenFilePaths.Length == 1)
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
            }
            else
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

            Common.Generic.ProcessFlag = 0;

            Form formProgress = new FormProgress();
            formProgress.ShowDialog();
            formProgress.Dispose();

            if (Common.Generic.Result == false)
            {
                Common.Generic.cts.Dispose();
                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                if (Common.Generic.OpenFilePaths.Length == 1)
                {
                    FileInfo fi = new(Common.Generic.SavePath);
                    Common.Generic.cts.Dispose();
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                    {
                        File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                        if (File.Exists(Common.Generic.SavePath))
                        {
                            if (fi.Length != 0)
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.DecodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ResetStatus();
                                Process.Start("EXPLORER.EXE", @"/select,""" + Common.Generic.SavePath + @"""");
                                return;
                            }
                            else
                            {
                                File.Delete(Common.Generic.SavePath);
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.DecodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                        else
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }
                    else
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetStatus();
                        return;
                    }
                }
                else
                {
                    Common.Generic.cts.Dispose();
                    foreach (var file in Common.Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);
                        if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav")))
                        {
                            File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, ".wav"), Common.Generic.FolderSavePath + @"\" + fi.Name.Replace(fi.Extension, ".wav"));
                            continue;
                        }
                        else
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }

                    if (Common.Generic.OpenFilePaths.Length == Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length)
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetStatus();
                        Process.Start("EXPLORER.EXE", Common.Generic.FolderSavePath);
                        return;
                    }
                    else
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetStatus();
                        return;
                    }
                }
            }
        }

        private void Button_Encode_Click(object sender, EventArgs e)
        {
            int lpc = Common.Utils.GetIntForIniFile("GENERIC", "LPCreateIndex");
            if (lpc != 65535)
            {
                Common.Generic.lpcreate = lpc switch
                {
                    0 => false,
                    1 => true,
                    _ => false,
                };
            }

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
                    else
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
                        if (Common.Generic.OpenFilePaths.Length == 1)
                        {
                            FileInfo fi = new(Common.Generic.SavePath);
                            Common.Generic.cts.Dispose();
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name))
                            {
                                File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name, Common.Generic.SavePath);
                                if (File.Exists(Common.Generic.SavePath))
                                {
                                    if (fi.Length != 0)
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, Localization.EncodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        ResetStatus();
                                        Process.Start("EXPLORER.EXE", @"/select,""" + Common.Generic.SavePath + @"""");
                                        return;
                                    }
                                    else
                                    {
                                        File.Delete(Common.Generic.SavePath);
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.EncodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        ResetStatus();
                                        return;
                                    }
                                }
                                else
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }
                            }
                            else
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return;
                            }
                        }
                        else
                        {
                            Common.Generic.cts.Dispose();
                            foreach (var file in Common.Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, Common.Generic.ATRACExt)))
                                {
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
                                    else
                                    {
                                        Common.Utils.DeleteDirectoryFiles(Common.Generic.FolderSavePath + @"\");
                                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                        MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        ResetStatus();
                                        return;
                                    }
                                }
                                else
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.EncodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return;
                                }
                            }

                            if (Common.Generic.OpenFilePaths.Length == Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length)
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.EncodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ResetStatus();
                                Process.Start("EXPLORER.EXE", Common.Generic.FolderSavePath);
                                return;
                            }
                            else if (Common.Generic.OpenFilePaths.Length > Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length && Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.AllDirectories).Length != 0)
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.EncodePartialCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetStatus();
                                Process.Start("EXPLORER.EXE", Common.Generic.FolderSavePath);
                                return;
                            }
                            else
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
            toolStripStatusLabel_Status.Text = Localization.ReadyCaption;
            toolStripStatusLabel_Status.ForeColor = Color.FromArgb(0, 0, 225, 0);
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
            Common.Generic.OpenFilePaths = null!;
            Common.Generic.ProcessFlag = -1;
            Common.Generic.ProgressMax = -1;
            button_Decode.Enabled = false;
            button_Encode.Enabled = false;
            toolStripStatusLabel_Status.Text = Localization.NotReadyCaption;
            toolStripStatusLabel_Status.ForeColor = Color.FromArgb(0, 255, 0, 0);
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
                                Process.Start("EXPLORER.EXE", @"/select,""" + Common.Generic.SavePath + @"""");
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
                    else
                    {
                        return;
                    }
                }
                else // Multiple
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
                            Process.Start("EXPLORER.EXE", @"/select,""" + Common.Generic.FolderSavePath + @"""");
                            return;
                        }
                        else // Cancelled
                        {
                            return;
                        }
                    }
                    else
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
                            Process.Start("EXPLORER.EXE", @"/select,""" + Common.Generic.SavePath + @"""");
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
                        Process.Start("EXPLORER.EXE", @"/select,""" + Common.Generic.FolderSavePath + @"""");
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
                            label_Formattxt.Text = Localization.WAVEFormatCaption;
                            toolStripDropDownButton_EF.Enabled = true;
                            toolStripDropDownButton_EF.Visible = true;
                            button_Decode.Enabled = false;
                            button_Encode.Enabled = true;
                            loopPointCreationToolStripMenuItem.Enabled = true;
                            break;
                        case ".AT3":
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            toolStripDropDownButton_EF.Enabled = false;
                            toolStripDropDownButton_EF.Visible = false;
                            button_Decode.Enabled = true;
                            button_Encode.Enabled = false;
                            loopPointCreationToolStripMenuItem.Enabled = false;
                            break;
                        case ".AT9":
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            toolStripDropDownButton_EF.Enabled = false;
                            toolStripDropDownButton_EF.Visible = false;
                            button_Decode.Enabled = true;
                            button_Encode.Enabled = false;
                            loopPointCreationToolStripMenuItem.Enabled = false;
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
                            switch (fi.Extension.ToUpper())
                            {
                                case ".WAV":
                                    label_Formattxt.Text = Localization.WAVEFormatCaption;
                                    toolStripDropDownButton_EF.Enabled = true;
                                    toolStripDropDownButton_EF.Visible = true;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = true;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    break;
                                case ".AT3":
                                    label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    button_Decode.Enabled = true;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    break;
                                case ".AT9":
                                    label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    button_Decode.Enabled = true;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    break;
                            }
                        }
                        count++;
                    }

                    ReadStatus();
                    label_Filepath.Text = Localization.MultipleFilesCaption;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024);

                    closeFileCToolStripMenuItem.Enabled = true;
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
            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
            Directory.Delete(Directory.GetCurrentDirectory() + @"\_temp");
        }

        private void LoopPointCreationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using FormLPC form = new();
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

        private static void RefleshSplashForm(FormSplash form, string text)
        {
            form.ProgressMsg = text;
            Application.DoEvents();
            Thread.Sleep(10);
        }
    }
}