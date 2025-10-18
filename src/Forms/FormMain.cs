using ATRACTool_Reloaded.Localizable;
using ATRACTool_Reloaded.Properties;
using NAudio.Gui;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
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
        FormLPC? FLPC;
        static FormSplash? fs;
        static object? lockobj;

        private static FormMain _formMainInstance = null!;
        public static FormMain FormMainInstance
        {
            get
            {
                return _formMainInstance;
            }
            set
            {
                _formMainInstance = value;
            }
        }

        public bool Meta
        {
            get
            {
                SetMetaDatas();
                return true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FPLabel
        {
            get
            {
                return label_Filepath.Text;
            }
            set
            {
                label_Filepath.Text = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FSLabel
        {
            get
            {
                return label_Sizetxt.Text;
            }
            set
            {
                label_Sizetxt.Text = value;
            }
        }

        public FormMain()
        {
            InitializeComponent();
        }

        // 初期化

        /// <summary>
        /// フォームのロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_Load(object sender, EventArgs e)
        {
            FileVersionInfo ver = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            Text = "ATRACTool Rel";

            if (Directory.Exists(Directory.GetCurrentDirectory() + @"\_temp\"))
            {
                Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp\");
            }
            if (Directory.Exists(Directory.GetCurrentDirectory() + @"\_tempAudio"))
            {
                Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_tempAudio");
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

            FormMainInstance = this;

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

                    Dmes d = new(ShowMessage);
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
                    string prm1 = Config.Entry["ATRAC3_Params"].Value, prm2 = Config.Entry["ATRAC9_Params"].Value, prm3 = Config.Entry["Walkman_Params"].Value;
                    if (ts != 65535)
                    {
                        switch (ts)
                        {
                            case 0:
                                Common.Generic.ATRACFlag = 0;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                                aTRAC9ToolStripMenuItem.Checked = false;
                                walkmanToolStripMenuItem.Checked = false;
                                toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                                EncodeMethodIsATRAC(true);
                                break;
                            case 1:
                                Common.Generic.ATRACFlag = 1;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = true;
                                walkmanToolStripMenuItem.Checked = false;
                                toolStripDropDownButton_EF.Text = "ATRAC9";
                                EncodeMethodIsATRAC(true);
                                break;
                            case 2:
                                Common.Generic.ATRACFlag = 2;
                                aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                                aTRAC9ToolStripMenuItem.Checked = false;
                                walkmanToolStripMenuItem.Checked = true;
                                toolStripDropDownButton_EF.Text = "Walkman";
                                EncodeMethodIsATRAC(false);
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
                    if (prm3 != "" || prm3 != null)
                    {
                        Common.Generic.EncodeParamWalkman = prm3;
                    }
                    else
                    {
                        Common.Generic.EncodeParamWalkman = "";
                    }
                    if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_EveryFmt_OutputFmt"].Value))
                    {
                        Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.oma)|*.oma;";
                    }
                    else
                    {
                        switch (int.Parse(Config.Entry["Walkman_EveryFmt_OutputFmt"].Value))
                        {
                            case 0:
                                Common.Generic.WalkmanEveryFilter = "PCM ATRAC (*.oma)|*.oma;";
                                break;
                            case 1:
                                Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.oma)|*.oma;";
                                break;
                            case 2:
                                Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.omg)|*.omg;";
                                break;
                            case 3:
                                Common.Generic.WalkmanEveryFilter = "ATRAC3 Advanced Lossless (*.oma)|*.oma;";
                                break;
                            case 4:
                                Common.Generic.WalkmanEveryFilter = "ATRAC3 Video Clip (*.kdr)|*.kdr;";
                                break;
                            case 5:
                                Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3+ (*.oma)|*.oma;";
                                break;
                            case 6:
                                Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3+ (*.omg)|*.omg;";
                                break;
                            case 7:
                                Common.Generic.WalkmanEveryFilter = "ATRAC3+ Advanced Lossless (*.oma)|*.oma;";
                                break;
                            case 8:
                                Common.Generic.WalkmanEveryFilter = "ATRAC3+ Video Clip (*.kdr)|*.kdr;";
                                break;
                        }
                    }

                    switch (bool.Parse(Config.Entry["LPC_Create"].Value))
                    {
                        case true:
                            Generic.lpcreate = true;
                            break;
                        case false:
                            Generic.lpcreate = false;
                            break;
                    }

                    switch (int.Parse(Config.Entry["ATRAC3_Console"].Value))
                    {
                        case 0:
                            {
                                Generic.IsAT3PS3 = false;
                                break;
                            }
                        case 1:
                            {
                                Generic.IsAT3PS3 = true;
                                break;
                            }
                        default:
                            {
                                Generic.IsAT3PS3 = false;
                                break;
                            }
                    }

                    switch (int.Parse(Config.Entry["ATRAC9_Console"].Value))
                    {
                        case 0:
                            {
                                Generic.IsAT9PS4 = false;
                                break;
                            }
                        case 1:
                            {
                                Generic.IsAT9PS4 = true;
                                break;
                            }
                        default:
                            {
                                Generic.IsAT9PS4 = false;
                                break;
                            }
                    }

                    switch (bool.Parse(Config.Entry["ATRAC3_LoopPoint"].Value))
                    {
                        case true:
                            {
                                Generic.IsAT3LoopPoint = true;
                                break;
                            }
                        case false:
                            {
                                Generic.IsAT3LoopPoint = false;
                                break;
                            }
                    }

                    switch (bool.Parse(Config.Entry["ATRAC3_LoopSound"].Value))
                    {
                        case true:
                            {
                                Generic.IsAT3LoopSound = true;
                                break;
                            }
                        case false:
                            {
                                Generic.IsAT3LoopSound = false;
                                break;
                            }
                    }

                    switch (bool.Parse(Config.Entry["ATRAC9_LoopPoint"].Value))
                    {
                        case true:
                            {
                                Generic.IsAT9LoopPoint = true;
                                break;
                            }
                        case false:
                            {
                                Generic.IsAT9LoopPoint = false;
                                break;
                            }
                    }

                    switch (bool.Parse(Config.Entry["ATRAC9_LoopSound"].Value))
                    {
                        case true:
                            {
                                Generic.IsAT9LoopSound = true;
                                break;
                            }
                        case false:
                            {
                                Generic.IsAT9LoopSound = false;
                                break;
                            }
                    }

                    loopPointCreationToolStripMenuItem.Enabled = false;
                    Thread.Sleep(1000);

                    if (bool.Parse(Config.Entry["Oldmode"].Value))
                    {
                        fs?.Invoke(d, "Legacy mode is activated");
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
                                var update = Task.Run(CheckForUpdatesForInit);
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
                string prm1 = Config.Entry["ATRAC3_Params"].Value, prm2 = Config.Entry["ATRAC9_Params"].Value, prm3 = Config.Entry["Walkman_Params"].Value;
                if (ts != 65535)
                {
                    switch (ts)
                    {
                        case 0:
                            Common.Generic.ATRACFlag = 0;
                            aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
                            aTRAC9ToolStripMenuItem.Checked = false;
                            walkmanToolStripMenuItem.Checked = false;
                            toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
                            EncodeMethodIsATRAC(true);
                            break;
                        case 1:
                            Common.Generic.ATRACFlag = 1;
                            aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                            aTRAC9ToolStripMenuItem.Checked = true;
                            walkmanToolStripMenuItem.Checked = false;
                            toolStripDropDownButton_EF.Text = "ATRAC9";
                            EncodeMethodIsATRAC(true);
                            break;
                        case 2:
                            Common.Generic.ATRACFlag = 2;
                            aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
                            aTRAC9ToolStripMenuItem.Checked = false;
                            walkmanToolStripMenuItem.Checked = true;
                            toolStripDropDownButton_EF.Text = "Walkman";
                            EncodeMethodIsATRAC(false);
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
                if (prm3 != "" || prm3 != null)
                {
                    Common.Generic.EncodeParamWalkman = prm3;
                }
                else
                {
                    Common.Generic.EncodeParamWalkman = "";
                }
                if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_EveryFmt_OutputFmt"].Value))
                {
                    Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.oma)|*.oma;";
                }
                else
                {
                    switch (int.Parse(Config.Entry["Walkman_EveryFmt_OutputFmt"].Value))
                    {
                        case 0:
                            Common.Generic.WalkmanEveryFilter = "PCM ATRAC (*.oma)|*.oma;";
                            break;
                        case 1:
                            Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.oma)|*.oma;";
                            break;
                        case 2:
                            Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.omg)|*.omg;";
                            break;
                        case 3:
                            Common.Generic.WalkmanEveryFilter = "ATRAC3 Advanced Lossless (*.oma)|*.oma;";
                            break;
                        case 4:
                            Common.Generic.WalkmanEveryFilter = "ATRAC3 Video Clip (*.kdr)|*.kdr;";
                            break;
                        case 5:
                            Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3+ (*.oma)|*.oma;";
                            break;
                        case 6:
                            Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3+ (*.omg)|*.omg;";
                            break;
                        case 7:
                            Common.Generic.WalkmanEveryFilter = "ATRAC3+ Advanced Lossless (*.oma)|*.oma;";
                            break;
                        case 8:
                            Common.Generic.WalkmanEveryFilter = "ATRAC3+ Video Clip (*.kdr)|*.kdr;";
                            break;
                    }
                }

                if (bool.Parse(Config.Entry["LPC_Create"].Value))
                {
                    Generic.lpcreate = true;
                }
                else
                {
                    Generic.lpcreate = false;
                }

                switch (int.Parse(Config.Entry["ATRAC3_Console"].Value))
                {
                    case 0:
                        {
                            Generic.IsAT3PS3 = false;
                            break;
                        }
                    case 1:
                        {
                            Generic.IsAT3PS3 = true;
                            break;
                        }
                    default:
                        {
                            Generic.IsAT3PS3 = false;
                            break;
                        }
                }

                switch (int.Parse(Config.Entry["ATRAC9_Console"].Value))
                {
                    case 0:
                        {
                            Generic.IsAT9PS4 = false;
                            break;
                        }
                    case 1:
                        {
                            Generic.IsAT9PS4 = true;
                            break;
                        }
                    default:
                        {
                            Generic.IsAT9PS4 = false;
                            break;
                        }
                }

                switch (bool.Parse(Config.Entry["ATRAC3_LoopPoint"].Value))
                {
                    case true:
                        {
                            Generic.IsAT3LoopPoint = true;
                            break;
                        }
                    case false:
                        {
                            Generic.IsAT3LoopPoint = false;
                            break;
                        }
                }

                switch (bool.Parse(Config.Entry["ATRAC3_LoopSound"].Value))
                {
                    case true:
                        {
                            Generic.IsAT3LoopSound = true;
                            break;
                        }
                    case false:
                        {
                            Generic.IsAT3LoopSound = false;
                            break;
                        }
                }

                switch (bool.Parse(Config.Entry["ATRAC9_LoopPoint"].Value))
                {
                    case true:
                        {
                            Generic.IsAT9LoopPoint = true;
                            break;
                        }
                    case false:
                        {
                            Generic.IsAT9LoopPoint = false;
                            break;
                        }
                }

                switch (bool.Parse(Config.Entry["ATRAC9_LoopSound"].Value))
                {
                    case true:
                        {
                            Generic.IsAT9LoopSound = true;
                            break;
                        }
                    case false:
                        {
                            Generic.IsAT9LoopSound = false;
                            break;
                        }
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
                            var update = Task.Run(CheckForUpdatesForInit);
                            update.Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(fs, "An error occured.\n" + ex, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

            }

            toolStripStatusLabel_EncMethod.Alignment = ToolStripItemAlignment.Right;



            if (ver.FileVersion != null)
            {
                Text = "ATRACTool Rel ( build: " + ver.FileVersion.ToString() + "-Beta )";
            }
            else
            {
                Text = "ATRACTool Rel";
            }

            panel_Main.BackgroundImage = Resources.SIEv2;
            Activate();

            if (!Utils.OpenMGCheck64() && !Utils.OpenMGCheck64_32())
            {
                MessageBox.Show(this, "There are no libraries installed on this PC to process OpenMG.\r\nTo generate files for Walkman, Sony Media Library Earth must be installed.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

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

                List<string> lst = [.. ofd.FileNames];
                Generic.OpenFilePaths = lst.ToArray();
                Generic.OriginOpenFilePaths = lst.ToArray();

                if (Generic.OpenFilePaths.Length == 1) // Single
                {
                    Generic.IsOpenMulti = false;

                    Generic.MultipleLoopStarts = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleLoopEnds = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleFilesLoopOKFlags = new bool[Generic.OpenFilePaths.Length];

                    Generic.ATRACMetadataBuffers = new int[3];

                    FileInfo file = new(ofd.FileName);
                    long FileSize = file.Length;
                    if (FileSize >= uint.MaxValue)
                    {
                        MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ReadStatus();
                    label_Filepath.Text = ofd.FileName;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FileSize / 1024, FileSize);

                    closeFileCToolStripMenuItem.Enabled = true;

                    switch (file.Extension.ToUpper())
                    {
                        case ".WAV":
                            if (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                            {
                                FormatSorter(true, true);
                            }
                            else
                            {
                                FormatSorter(true);
                            }
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
                            Generic.ReadedATRACFlag = 0;
                            Utils.ReadMetadatas(Generic.OpenFilePaths[0], Generic.ATRACMetadataBuffers);
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            Generic.ReadedATRACFlag = 1;
                            Utils.ReadMetadatas(Generic.OpenFilePaths[0], Generic.ATRACMetadataBuffers);
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    return;
                }
                else // Multiple
                {
                    Generic.IsOpenMulti = true;

                    long Filesizes = 0;
                    FileInfo fs = new(Generic.OpenFilePaths[0]);
                    long FS = fs.Length;
                    foreach (string file in Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);

                        if (fi.Length >= uint.MaxValue)
                        {
                            MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else
                        {
                            Filesizes += fi.Length;
                        }
                    }

                    string Ft = "";
                    int count = 0, wavcount = 0;
                    List<string> multiextlst = new();

                    foreach (var file in Common.Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);

                        if (count != 0)
                        {
                            if (Ft == ".AT3" || Ft == ".AT9")
                            {
                                if (Ft != fi.Extension.ToUpper())
                                {
                                    MessageBox.Show(this, Localization.FileMixedWithATRACCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    closeFileCToolStripMenuItem.Enabled = false;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    toolStripStatusLabel_EncMethod.Enabled = false;
                                    toolStripStatusLabel_EncMethod.Visible = false;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    return;
                                }
                            }
                            else
                            {
                                if (fi.Extension.ToUpper() == ".AT3" || fi.Extension.ToUpper() == ".AT9")
                                {
                                    MessageBox.Show(this, Localization.FileMixedWithATRACCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    closeFileCToolStripMenuItem.Enabled = false;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    toolStripStatusLabel_EncMethod.Enabled = false;
                                    toolStripStatusLabel_EncMethod.Visible = false;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    return;
                                }
                                if (count == Generic.OpenFilePaths.Length - 1)
                                {
                                    if (wavcount == Generic.OpenFilePaths.Length - 1)
                                    {
                                        if (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                                        {
                                            Ft = ".NOT";
                                        }
                                        else
                                        {
                                            Ft = ".WAV";
                                        }

                                    }
                                    else
                                    {
                                        Ft = ".NOT";
                                    }
                                }
                                else
                                {
                                    if (fi.Extension.ToUpper() == Ft)
                                    {
                                        Ft = fi.Extension.ToUpper();
                                        wavcount++;
                                    }
                                    else if (fi.Extension.ToUpper() != Ft)
                                    {
                                        Ft = fi.Extension.ToUpper();
                                    }
                                    else
                                    {
                                        Ft = fi.Extension.ToUpper();
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (fi.Extension.ToUpper() == ".WAV")
                            {
                                Ft = fi.Extension.ToUpper();
                                multiextlst.Add(file);
                                wavcount++;
                                count++;
                                continue;
                            }
                            else if (fi.Extension.ToUpper() == ".AT3" || fi.Extension.ToUpper() == ".AT9")
                            {
                                Ft = fi.Extension.ToUpper();
                            }
                            else
                            {
                                Ft = fi.Extension.ToUpper();
                                multiextlst.Add(file);
                            }
                        }

                        count++;
                    }

                    ReadStatus();
                    //label_Filepath.Text = Localization.MultipleFilesCaption;
                    label_Filepath.Text = Generic.OpenFilePaths[0];
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024, FS);

                    closeFileCToolStripMenuItem.Enabled = true;

                    Generic.MultipleLoopStarts = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleLoopEnds = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleFilesLoopOKFlags = new bool[Generic.OpenFilePaths.Length];

                    Generic.ATRACMultiMetadataBuffer = new int[Generic.OpenFilePaths.Length, 3];

                    switch (Ft.ToUpper())
                    {
                        case ".WAV":
                            FormatSorter(true);
                            break;
                        case ".NOT":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            Generic.ReadedATRACFlag = 0;
                            Utils.ReadMetadatasMulti(Generic.OpenFilePaths, Generic.ATRACMultiMetadataBuffer);
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            Generic.ReadedATRACFlag = 1;
                            Utils.ReadMetadatasMulti(Generic.OpenFilePaths, Generic.ATRACMultiMetadataBuffer);
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    return;
                }
            }
            else
            {
                ActivateOrDeactivateLPC(false);
                ResetStatus();
                return;
            }
        }

        /// <summary>
        /// ファイルを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseFileCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivateOrDeactivateLPC(false);
            if (bool.Parse(Config.Entry["PlaybackATRAC"].Value) || bool.Parse(Config.Entry["ATRACEncodeSource"].Value))
            {
                Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp\");
            }
            ResetStatus();
        }

        /// <summary>
        /// 終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 変換設定ダイアログ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string prm1 = Config.Entry["ATRAC3_Params"].Value, prm2 = Config.Entry["ATRAC9_Params"].Value, prm3 = Config.Entry["Walkman_Params"].Value;

            using Form FSS = new FormSettings(false);
            FSS.ShowDialog();

            if (!string.IsNullOrWhiteSpace(prm1))
            {
                Common.Generic.EncodeParamAT3 = prm1;
            }
            else
            {
                Common.Generic.EncodeParamAT3 = "";
            }
            if (!string.IsNullOrWhiteSpace(prm2))
            {
                Common.Generic.EncodeParamAT9 = prm2;
            }
            else
            {
                Common.Generic.EncodeParamAT9 = "";
            }
            if (!string.IsNullOrWhiteSpace(prm3))
            {
                Common.Generic.EncodeParamWalkman = prm3;
            }
            else
            {
                Common.Generic.EncodeParamWalkman = "";
            }

            switch (bool.Parse(Config.Entry["LPC_Create"].Value))
            {
                case true:
                    Generic.lpcreate = true;
                    if (FormLPC.FormLPCInstance is not null) FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    break;
                case false:
                    Generic.lpcreate = false;
                    if (FormLPC.FormLPCInstance is not null) FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                    break;
            }

            switch (int.Parse(Config.Entry["ATRAC3_Console"].Value))
            {
                case 0:
                    {
                        Generic.IsAT3PS3 = false;
                        break;
                    }
                case 1:
                    {
                        Generic.IsAT3PS3 = true;
                        break;
                    }
                default:
                    {
                        Generic.IsAT3PS3 = false;
                        break;
                    }
            }

            switch (int.Parse(Config.Entry["ATRAC9_Console"].Value))
            {
                case 0:
                    {
                        Generic.IsAT9PS4 = false;
                        break;
                    }
                case 1:
                    {
                        Generic.IsAT9PS4 = true;
                        break;
                    }
                default:
                    {
                        Generic.IsAT9PS4 = false;
                        break;
                    }
            }

            switch (bool.Parse(Config.Entry["ATRAC3_LoopPoint"].Value))
            {
                case true:
                    {
                        Generic.IsAT3LoopPoint = true;
                        break;
                    }
                case false:
                    {
                        Generic.IsAT3LoopPoint = false;
                        break;
                    }
            }

            switch (bool.Parse(Config.Entry["ATRAC3_LoopSound"].Value))
            {
                case true:
                    {
                        Generic.IsAT3LoopSound = true;
                        break;
                    }
                case false:
                    {
                        Generic.IsAT3LoopSound = false;
                        break;
                    }
            }

            switch (bool.Parse(Config.Entry["ATRAC9_LoopPoint"].Value))
            {
                case true:
                    {
                        Generic.IsAT9LoopPoint = true;
                        break;
                    }
                case false:
                    {
                        Generic.IsAT9LoopPoint = false;
                        break;
                    }
            }

            switch (bool.Parse(Config.Entry["ATRAC9_LoopSound"].Value))
            {
                case true:
                    {
                        Generic.IsAT9LoopSound = true;
                        break;
                    }
                case false:
                    {
                        Generic.IsAT9LoopSound = false;
                        break;
                    }
            }

            if (FormLPC.FormLPCInstance is not null)
            {
                if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                {
                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3/ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                }
                else if (Generic.lpcreate)
                {
                    FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                }
                else
                {
                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                    }
                }
            }
            else
            {

            }
        }

        /// <summary>
        /// バージョン情報ダイアログの表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutATRACToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout formAbout = new();
            formAbout.ShowDialog();
            formAbout.Dispose();
        }

        /// <summary>
        /// アップデート確認
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CheckForUpdatesUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    string hv = null!;

                    using Stream hcs = await Task.Run(() => Common.Network.GetWebStreamAsync(appUpdatechecker, Common.Network.GetUri("https://raw.githubusercontent.com/XyLe-GBP/ATRACTool-Reloaded/master/VERSIONINFO")));
                    using StreamReader hsr = new(hcs);
                    hv = await Task.Run(hsr.ReadToEndAsync);
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

                                    if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                    {
                                        File.Delete(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip");
                                    }

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

                                    if (File.Exists(updpath + @"\updater.exe"))
                                    {
                                        File.Delete(updpath + @"\updater.exe");
                                    }
                                    if (Directory.Exists(updpath + @"\updater-temp"))
                                    {
                                        Common.Utils.DeleteDirectory(updpath + @"\updater-temp");
                                    }
                                    if (File.Exists(updpath + @"\atractool-rel.zip"))
                                    {
                                        File.Delete(updpath + @"\atractool-rel.zip");
                                    }

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

        /// <summary>
        /// (Task) アプリケーション起動時のアップデート確認
        /// </summary>
        /// <returns></returns>
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

                                    if (File.Exists(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip"))
                                    {
                                        File.Delete(Directory.GetCurrentDirectory() + @"\res\atractool-rel.zip");
                                    }

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

                                    if (File.Exists(updpath + @"\updater.exe"))
                                    {
                                        File.Delete(updpath + @"\updater.exe");
                                    }
                                    if (Directory.Exists(updpath + @"\updater-temp"))
                                    {
                                        Common.Utils.DeleteDirectory(updpath + @"\updater-temp");
                                    }
                                    if (File.Exists(updpath + @"\atractool-rel.zip"))
                                    {
                                        File.Delete(updpath + @"\atractool-rel.zip");
                                    }

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

        // ステータスバー

        private void ATRAC3ATRAC3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.Entry["ToolStrip"].Value = "0";
            Config.Save(xmlpath);
            Common.Generic.ATRACFlag = 0;
            aTRAC3ATRAC3ToolStripMenuItem.Checked = true;
            aTRAC9ToolStripMenuItem.Checked = false;
            walkmanToolStripMenuItem.Checked = false;
            toolStripDropDownButton_EF.Text = "ATRAC3 / ATRAC3+";
            EncodeMethodIsATRAC(true);
            if (FormLPC.FormLPCInstance is not null)
            {
                FormLPC.FormLPCInstance.checkBox_LoopEnable.Enabled = true;
                if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                {
                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3/ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                }
                else if (Generic.lpcreate)
                {
                    FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                }
                else
                {
                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                    }
                }
            }
        }

        private void ATRAC9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.Entry["ToolStrip"].Value = "1";
            Config.Save(xmlpath);
            Common.Generic.ATRACFlag = 1;
            aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
            aTRAC9ToolStripMenuItem.Checked = true;
            walkmanToolStripMenuItem.Checked = false;
            toolStripDropDownButton_EF.Text = "ATRAC9";
            EncodeMethodIsATRAC(true);
            if (FormLPC.FormLPCInstance is not null)
            {
                FormLPC.FormLPCInstance.checkBox_LoopEnable.Enabled = true;
                if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                {
                    if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3/ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                }
                else if (Generic.lpcreate)
                {
                    FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                }
                else
                {
                    if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                    {
                        FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                    }
                    else
                    {
                        FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                    }

                }
            }
        }

        private void walkmanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.Entry["ToolStrip"].Value = "2";
            Config.Save(xmlpath);
            Common.Generic.ATRACFlag = 2;
            aTRAC3ATRAC3ToolStripMenuItem.Checked = false;
            aTRAC9ToolStripMenuItem.Checked = false;
            walkmanToolStripMenuItem.Checked = true;
            toolStripDropDownButton_EF.Text = "Walkman";
            EncodeMethodIsATRAC(false);
            if (FormLPC.FormLPCInstance is not null)
            {
                FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = false;
                FormLPC.FormLPCInstance.checkBox_LoopEnable.Enabled = false;
                FormLPC.FormLPCInstance.CautionLabel = "Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
            }
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
            bool IsFasterATRAC = bool.Parse(Config.Entry["FasterATRAC"].Value);
            bool IsPlayingATRAC = bool.Parse(Config.Entry["PlaybackATRAC"].Value);

            if (IsPlayingATRAC && Generic.IsATRAC && !IsFasterATRAC) // ATRAC再生が有効
            {
                if (manual)
                {
                    MessageBox.Show(this, Localization.ATRACPlaybackEnabledSpecLocationWarning, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                ActivateOrDeactivateLPC(false);

                if (Generic.pATRACOpenFilePaths.Length == 1) // 単一
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
                        if (File.Exists(sfd.FileName))
                        {
                            File.Delete(sfd.FileName);
                        }
                        File.Move(Generic.pATRACOpenFilePaths[0], sfd.FileName);
                        if (File.Exists(sfd.FileName))
                        {
                            MessageBox.Show(this, Localization.DecodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ResetStatus();
                            Utils.ShowFolder(sfd.FileName, bool.Parse(Config.Entry["ShowFolder"].Value));
                            return;
                        }
                        else
                        {
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.DecodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
                    }
                    else // Cancelled
                    {
                        ResetStatus();
                        return;
                    }
                }
                else // 複数
                {
                    FolderBrowserDialog fbd = new()
                    {
                        Description = Localization.FolderSaveDialogTitle,
                        RootFolder = Environment.SpecialFolder.MyDocuments,
                        SelectedPath = @"",
                    };
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        int AcceptFile = 0, ErrorFile = 0;
                        if (Directory.GetFiles(fbd.SelectedPath, "*", SearchOption.AllDirectories).Length != 0)
                        {
                            DialogResult dr = MessageBox.Show(this, Localization.AlreadyExistsCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (dr == DialogResult.Yes)
                            {
                                Common.Utils.DeleteDirectoryFiles(fbd.SelectedPath);
                            }
                            else
                            {
                                return;
                            }
                        }
                        foreach (var file in Generic.pATRACOpenFilePaths)
                        {
                            FileInfo fi = new(file);
                            File.Move(file, fbd.SelectedPath + @"\" + fi.Name.Replace(fi.Extension, ".wav"));
                            if (File.Exists(fbd.SelectedPath + @"\" + fi.Name.Replace(fi.Extension, ".wav")))
                            {
                                AcceptFile++;
                                continue;
                            }
                            else
                            {
                                ErrorFile++;
                                continue;
                            }
                        }
                        MessageBox.Show(this, string.Format(Localization.DecodeSuccessCaption + "\nSuccess: {0} Files\nError: {1} Files", AcceptFile, ErrorFile), Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetStatus();
                        Utils.ShowFolder(fbd.SelectedPath, bool.Parse(Config.Entry["ShowFolder"].Value));
                        return;
                    }
                    else // Cancelled
                    {
                        ResetStatus();
                        return;
                    }
                }
            }
            else if (IsFasterATRAC && IsPlayingATRAC && Generic.IsATRAC)
            {
                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
            }
            else // ATRAC再生が無効
            {
                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
            }

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

            if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested) // 中断
            {
                Common.Generic.cts.Dispose();
                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                ResetStatus();
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

                    if (Common.Generic.OpenFilePaths.Length == Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.TopDirectoryOnly).Length) // OK
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetStatus();
                        Utils.ShowFolder(Common.Generic.FolderSavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                        return;
                    }
                    else if (Common.Generic.OpenFilePaths.Length > Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.TopDirectoryOnly).Length && Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.TopDirectoryOnly).Length != 0) // 一部変換失敗
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodePartialCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            bool iseveryfmt = bool.Parse(Config.Entry["Walkman_EveryFmt"].Value);
            bool atracencsource = bool.Parse(Config.Entry["ATRACEncodeSource"].Value);
            Common.Generic.lpcreate = lpc switch
            {
                false => false,
                true => true,
            };

            if (Common.Generic.ATRACFlag == 0 || Common.Generic.ATRACFlag == 1 || Generic.ATRACFlag == 2)
            {
                if (string.IsNullOrWhiteSpace(Generic.EncodeParamAT3) || string.IsNullOrWhiteSpace(Generic.EncodeParamAT9) || string.IsNullOrWhiteSpace(Generic.EncodeParamWalkman))
                {
                    // Param Error
                    MessageBox.Show(this, Localization.SettingsErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else // OK
                {
                    ActivateOrDeactivateLPC(false);

                    if (atracencsource && Generic.IsATRAC)
                    {
                        uint error = 0;
                        string ct = Utils.SFDRandomNumber();
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\_ATRACEnc" + ct);
                        Generic.ATRACEncodeSourceTempPath = Directory.GetCurrentDirectory() + @"\_ATRACEnc" + ct;

                        List<string> list = [];
                        foreach (string path in Generic.pATRACOpenFilePaths)
                        {
                            FileInfo fi = new(path);
                            File.Move(path, Generic.ATRACEncodeSourceTempPath + @"\" + fi.Name.Replace(".ata", ".wav"));
                            if (File.Exists(Generic.ATRACEncodeSourceTempPath + @"\" + fi.Name.Replace(".ata", ".wav")))
                            {
                                list.Add(Generic.ATRACEncodeSourceTempPath + @"\" + fi.Name.Replace(".ata", ".wav"));
                            }
                            else
                            {
                                error++;
                            }
                        }
                        if (error != 0)
                        {
                            MessageBox.Show(this, string.Format(Localization.UnExpectedCaption, "File move failed. [" + error + " Files]"), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        Generic.pATRACOpenFilePaths = list.ToArray();
                    }
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
                                case 2:
                                    {
                                        if (!iseveryfmt)
                                        {
                                            SaveFileDialog sfd = new()
                                            {
                                                FileName = Common.Utils.SFDRandomNumber(),
                                                InitialDirectory = "",
                                                Filter = Common.Generic.WalkmanEveryFilter,
                                                FilterIndex = 0,
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
                                        else
                                        {
                                            SaveFileDialog sfd = new()
                                            {
                                                FileName = Common.Utils.SFDRandomNumber(),
                                                InitialDirectory = "",
                                                Filter = Localization.WalkmanFilter,
                                                FilterIndex = 0,
                                                Title = Localization.SaveDialogTitle,
                                                OverwritePrompt = true,
                                                RestoreDirectory = true
                                            };
                                            if (sfd.ShowDialog() == DialogResult.OK)
                                            {
                                                FormSetWalkmanInformations formSetWalkmanInformations = new();
                                                if (formSetWalkmanInformations.ShowDialog() == DialogResult.OK)
                                                {
                                                    Common.Generic.SavePath = sfd.FileName;
                                                    Common.Generic.ProgressMax = 1;
                                                }
                                                else
                                                {
                                                    ResetStatus();
                                                    return;
                                                }
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
                        if (bool.Parse(Config.Entry["ATRAC3_LoopPoint"].Value) || bool.Parse(Config.Entry["ATRAC9_LoopPoint"].Value))
                        {
                            MessageBox.Show(this, Localization.MultipleLoopPointErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetStatus();
                            return;
                        }
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

                                if (Generic.ATRACFlag == 2) // Walkman
                                {
                                    FormSelectWalkmanFormats formSelectWalkmanFormats = new(true);
                                    if (formSelectWalkmanFormats.ShowDialog() == DialogResult.OK)
                                    {
                                        SetWalkmanMultiConvertFormats(true, int.Parse(Generic.WalkmanMultiConvFmt));
                                    }
                                    else { return; }
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
                        Config.Entry["ATRAC3_LoopPoint"].Value = "false";
                        Config.Entry["ATRAC3_LoopStart_Samples"].Value = "";
                        Config.Entry["ATRAC3_LoopEnd_Samples"].Value = "";
                        Config.Entry["ATRAC9_LoopPoint"].Value = "false";
                        Config.Entry["ATRAC9_LoopStart_Samples"].Value = "";
                        Config.Entry["ATRAC9_LoopEnd_Samples"].Value = "";
                        Config.Save(xmlpath);
                        Generic.LPCSuffix = string.Empty;
                        Common.Generic.lpcreatev2 = false;
                    }

                    if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested)
                    {
                        Common.Generic.cts.Dispose();
                        MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        ResetStatus();
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

                            if (Common.Generic.OpenFilePaths.Length == Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.TopDirectoryOnly).Length) // OK
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.EncodeSuccessCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ResetStatus();
                                Utils.ShowFolder(Common.Generic.FolderSavePath, bool.Parse(Config.Entry["ShowFolder"].Value));
                                return;
                            }
                            else if (Common.Generic.OpenFilePaths.Length > Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.TopDirectoryOnly).Length && Directory.GetFiles(Common.Generic.FolderSavePath, "*", SearchOption.TopDirectoryOnly).Length != 0) // 一部変換失敗
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

        /// <summary>
        /// 詳細設定ダイアログ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreferencesMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool oldSSValue = bool.Parse(Config.Entry["SmoothSamples"].Value);
            using Form FSS = new FormPreferencesSettings();
            FSS.ShowDialog();

            if (Generic.IsConfigError)
            {
                MessageBox.Show(this, "The configuration file is corrupt. Delete the configuration file and restart the application.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                File.Delete(Common.xmlpath);
                Application.Restart();
                return;
            }

            if (oldSSValue != bool.Parse(Config.Entry["SmoothSamples"].Value))
            {
                ActivateOrDeactivateLPC(false);
                ActivateOrDeactivateLPC(true);
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 旧関数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoopPointCreationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using FormLPC form = new(true);
            form.ShowDialog();
        }

        /// <summary>
        /// ファイルを読み込んだ際のボタン等の動作
        /// </summary>
        private void ReadStatus()
        {
            AllowDrop = false;
            toolStripStatusLabel_Status.Text = Localization.ReadyCaption;
            toolStripStatusLabel_Status.ForeColor = Color.Green;
            label_NotReaded.Visible = false;
            label_File.Visible = true;
            label_Format.Visible = true;
            label_Size.Visible = true;
            label_Filepath.Visible = true;
            label_Sizetxt.Visible = true;
            label_Formattxt.Visible = true;
        }

        /// <summary>
        /// ファイルを閉じたときにUIや変数をリセットする
        /// </summary>
        private void ResetStatus()
        {
            AllowDrop = true;
            if (Generic.IsATWCancelled)
            {
                Utils.ATWCheck(Generic.IsATW, true);
                Generic.IsATWCancelled = false;
            }
            else
            {
                Utils.ATWCheck(Generic.IsATW);
            }
            Generic.IsWave = false;
            Generic.IsATRAC = false;
            Generic.IsATRACLooped = false;
            Generic.ReadedATRACFlag = -1;

            Generic.OpenFilePaths = null!;
            Generic.pATRACOpenFilePaths = null!;
            Generic.OriginOpenFilePaths = null!;
            Generic.SavePath = null!;
            Generic.FolderSavePath = null!;
            Generic.pATRACSavePath = null!;
            Generic.pATRACFolderSavePath = null!;

            Generic.ProcessFlag = -1;
            Generic.ProgressMax = -1;
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
            label_File.Visible = false;
            label_Format.Visible = false;
            label_Size.Visible = false;
            label_Filepath.Visible = false;
            label_Sizetxt.Visible = false;
            label_Formattxt.Visible = false;
            toolStripDropDownButton_EF.Enabled = false;
            toolStripDropDownButton_EF.Visible = false;
            toolStripStatusLabel_EncMethod.Enabled = false;
            toolStripStatusLabel_EncMethod.Visible = false;
            loopPointCreationToolStripMenuItem.Enabled = false;
            closeFileCToolStripMenuItem.Enabled = false;
            groupBox_Loop.Enabled = false;
            textBox_LoopStart.Text = string.Empty;
            textBox_LoopEnd.Text = string.Empty;

            Generic.MultipleFilesLoopOKFlags = [];
            Generic.MultipleLoopStarts = [];
            Generic.MultipleLoopEnds = [];
            Generic.ATRACMetadataBuffers = [];
            Generic.ATRACMultiMetadataBuffer = null!;

            if (bool.Parse(Config.Entry["ATRACEncodeSource"].Value))
            {
                Utils.DeleteDirectory(Generic.ATRACEncodeSourceTempPath);
                Generic.ATRACEncodeSourceTempPath = null!;
            }
        }

        /// <summary>
        /// サポートされている任意のファイルをWaveに変換する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Waveファイルをサポートされている任意のオーディオに変換する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// ドラッグアンドドロップ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// ドラッグアンドドロップでファイルを読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            Utils.ATWCheck(Generic.IsATW);

            if (e.Data != null)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop)!;

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
                Generic.OpenFilePaths = lst.ToArray();
                Generic.OriginOpenFilePaths = lst.ToArray();

                if (Generic.OpenFilePaths.Length == 1) // Single
                {
                    Generic.IsOpenMulti = false;

                    Generic.MultipleLoopStarts = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleLoopEnds = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleFilesLoopOKFlags = new bool[Generic.OpenFilePaths.Length];

                    Generic.ATRACMetadataBuffers = new int[3];

                    FileInfo file = new(files[0]);
                    long FileSize = file.Length;
                    if (FileSize >= uint.MaxValue)
                    {
                        MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ReadStatus();
                    label_Filepath.Text = file.FullName;
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FileSize / 1024, FileSize);
                    switch (file.Extension.ToUpper())
                    {
                        case ".WAV":
                            if (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                            {
                                FormatSorter(true, true);
                            }
                            else
                            {
                                FormatSorter(true);
                            }
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
                            Generic.ReadedATRACFlag = 0;
                            Utils.ReadMetadatas(Generic.OpenFilePaths[0], Generic.ATRACMetadataBuffers);
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            Generic.ReadedATRACFlag = 1;
                            Utils.ReadMetadatas(Generic.OpenFilePaths[0], Generic.ATRACMetadataBuffers);
                            label_Formattxt.Text = Localization.ATRAC9FormatCaption;
                            FormatSorter(false);
                            break;
                    }

                    closeFileCToolStripMenuItem.Enabled = true;
                    return;
                }
                else // 複数ファイル
                {
                    Generic.IsOpenMulti = true;

                    long Filesizes = 0;
                    FileInfo fs = new(files[0]);
                    long FS = fs.Length;
                    foreach (string file in files)
                    {
                        FileInfo fi = new(file);

                        if (fi.Length >= uint.MaxValue)
                        {
                            MessageBox.Show(this, Localization.FilesizeLargeCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else
                        {
                            Filesizes += fi.Length;
                        }
                    }

                    string Ft = "";
                    int count = 0, wavcount = 0;
                    List<string> multiextlst = new();

                    foreach (var file in Common.Generic.OpenFilePaths)
                    {
                        FileInfo fi = new(file);

                        if (count != 0)
                        {
                            if (Ft == ".AT3" || Ft == ".AT9")
                            {
                                if (Ft != fi.Extension.ToUpper())
                                {
                                    MessageBox.Show(this, Localization.FileMixedWithATRACCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    closeFileCToolStripMenuItem.Enabled = false;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    toolStripStatusLabel_EncMethod.Enabled = false;
                                    toolStripStatusLabel_EncMethod.Visible = false;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    return;
                                }
                            }
                            else
                            {
                                if (fi.Extension.ToUpper() == ".AT3" || fi.Extension.ToUpper() == ".AT9")
                                {
                                    MessageBox.Show(this, Localization.FileMixedWithATRACCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    closeFileCToolStripMenuItem.Enabled = false;
                                    toolStripDropDownButton_EF.Enabled = false;
                                    toolStripDropDownButton_EF.Visible = false;
                                    toolStripStatusLabel_EncMethod.Enabled = false;
                                    toolStripStatusLabel_EncMethod.Visible = false;
                                    button_Decode.Enabled = false;
                                    button_Encode.Enabled = false;
                                    loopPointCreationToolStripMenuItem.Enabled = false;
                                    return;
                                }
                                if (count == Generic.OpenFilePaths.Length - 1)
                                {
                                    if (wavcount == Generic.OpenFilePaths.Length - 1)
                                    {
                                        if (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                                        {
                                            Ft = ".NOT";
                                        }
                                        else
                                        {
                                            Ft = ".WAV";
                                        }

                                    }
                                    else
                                    {
                                        Ft = ".NOT";
                                    }
                                }
                                else
                                {
                                    if (fi.Extension.ToUpper() == Ft)
                                    {
                                        Ft = fi.Extension.ToUpper();
                                        wavcount++;
                                    }
                                    else if (fi.Extension.ToUpper() != Ft)
                                    {
                                        Ft = fi.Extension.ToUpper();
                                    }
                                    else
                                    {
                                        Ft = fi.Extension.ToUpper();
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (fi.Extension.ToUpper() == ".WAV")
                            {
                                Ft = fi.Extension.ToUpper();
                                multiextlst.Add(file);
                                wavcount++;
                                count++;
                                continue;
                            }
                            else if (fi.Extension.ToUpper() == ".AT3" || fi.Extension.ToUpper() == ".AT9")
                            {
                                Ft = fi.Extension.ToUpper();
                            }
                            else
                            {
                                Ft = fi.Extension.ToUpper();
                                multiextlst.Add(file);
                            }
                        }

                        count++;
                    }

                    ReadStatus();
                    //label_Filepath.Text = Localization.MultipleFilesCaption;
                    label_Filepath.Text = Generic.OpenFilePaths[0];
                    label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024, FS);

                    closeFileCToolStripMenuItem.Enabled = true;

                    Generic.MultipleLoopStarts = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleLoopEnds = new int[Generic.OpenFilePaths.Length];
                    Generic.MultipleFilesLoopOKFlags = new bool[Generic.OpenFilePaths.Length];

                    Generic.ATRACMultiMetadataBuffer = new int[Generic.OpenFilePaths.Length, 3];

                    switch (Ft.ToUpper())
                    {
                        case ".WAV":
                            FormatSorter(true);
                            break;
                        case ".NOT":
                            FormatSorter(true, true);
                            break;
                        case ".AT3":
                            Generic.ReadedATRACFlag = 0;
                            Utils.ReadMetadatasMulti(Generic.OpenFilePaths, Generic.ATRACMultiMetadataBuffer);
                            label_Formattxt.Text = Localization.ATRAC3FormatCaption;
                            FormatSorter(false);
                            break;
                        case ".AT9":
                            Generic.ReadedATRACFlag = 1;
                            Utils.ReadMetadatasMulti(Generic.OpenFilePaths, Generic.ATRACMultiMetadataBuffer);
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

        /// <summary>
        /// フォーム終了後の後処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            ActivateOrDeactivateLPC(false);
            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
            Utils.ATWCheck(Generic.IsATW);
            Directory.Delete(Directory.GetCurrentDirectory() + @"\_temp");
        }

        /// <summary>
        /// Waveではない音声ファイルをWaveに変換する
        /// </summary>
        private bool AudioToWaveConvert()
        {
            if (walkmanToolStripMenuItem.Checked)
            {
                walkmanToolStripMenuItem.Checked = false;
            }
            if (Common.Generic.IsWave != true && Common.Generic.IsATRAC != true)
            {
                string TempAudioDir;
                if (Directory.Exists(Directory.GetCurrentDirectory() + @"\_tempAudio"))
                {
                    TempAudioDir = Directory.GetCurrentDirectory() + @"\_tempAudio";
                }
                else
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\_tempAudio");
                    TempAudioDir = Directory.GetCurrentDirectory() + @"\_tempAudio";
                }

                if (Common.Generic.OpenFilePaths.Length == 1) // 単一ファイル
                {
                    if (bool.Parse(Config.Entry["FixedConvert"].Value)) // Fix
                    {
                        FileInfo file = new(Common.Generic.OpenFilePaths[0]);
                        Common.Generic.WTAmethod = int.Parse(Config.Entry["ConvertType"].Value);

                        //Common.Generic.SavePath = file.Directory + @"\" + file.Name + @".wav";
                        Common.Generic.SavePath = TempAudioDir + @"\" + file.Name.Replace(file.Extension, "") + @".wav";
                        Common.Generic.ProgressMax = 1;

                        Common.Generic.ProcessFlag = 2;

                        Form formProgress = new FormProgress();
                        formProgress.ShowDialog();
                        formProgress.Dispose();

                        if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested)
                        {
                            Generic.IsATWCancelled = true;
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
                                label_Filepath.Text = Common.Generic.OpenFilePaths[0];
                                Common.Generic.OpenFilePaths[0] = Common.Generic.SavePath;
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
                            //Common.Generic.SavePath = file.Directory + @"\" + file.Name + @".wav";
                            Common.Generic.SavePath = TempAudioDir + @"\" + file.Name + @".wav";
                            Common.Generic.ProgressMax = 1;

                            Common.Generic.ProcessFlag = 2;

                            Form formProgress = new FormProgress();
                            formProgress.ShowDialog();
                            formProgress.Dispose();

                            if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested)
                            {
                                Generic.IsATWCancelled = true;
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
                                    label_Filepath.Text = Common.Generic.OpenFilePaths[0];
                                    Common.Generic.OpenFilePaths[0] = Common.Generic.SavePath;
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
                    Generic.IsOpenMulti = true;
                    if (bool.Parse(Config.Entry["FixedConvert"].Value)) // Fix
                    {
                        //FileInfo fp = new(Common.Generic.OpenFilePaths[0]);
                        Common.Generic.WTAmethod = int.Parse(Config.Entry["ConvertType"].Value);

                        Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                        Common.Generic.ProcessFlag = 2;

                        Form formProgress = new FormProgress();
                        formProgress.ShowDialog();
                        formProgress.Dispose();

                        if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested)
                        {
                            Generic.IsATWCancelled = true;
                            Common.Generic.cts.Dispose();
                            MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ResetStatus();
                            return false;
                        }

                        int count = 0;
                        label_Filepath.Text = Generic.OpenFilePaths[0];

                        foreach (var file in Common.Generic.OpenFilePaths)
                        {
                            FileInfo fi = new(file);
                            if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav"))
                            {
                                if (File.Exists(TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav"))  // ファイルが既に存在する場合は削除してからMoveする
                                {
                                    File.Delete(TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav");
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav", TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav");
                                }
                                else
                                {
                                    File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav", TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav");
                                }

                            }
                            else // Error
                            {
                                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetStatus();
                                return false;
                            }

                            if (File.Exists(TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav")) // ファイル存在確認
                            {
                                Common.Generic.OpenFilePaths[count] = TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav";
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

                        FileInfo fisize = new(label_Filepath.Text);
                        long FS = fisize.Length;
                        label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024);

                        return true;
                    }
                    else // normal
                    {
                        //FileInfo fp = new(Common.Generic.OpenFilePaths[0]);
                        using Form formAtWST = new FormAtWSelectTarget();
                        DialogResult dr = formAtWST.ShowDialog();
                        if (dr != DialogResult.Cancel && dr != DialogResult.None)
                        {
                            Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;

                            Common.Generic.ProcessFlag = 2;

                            Form formProgress = new FormProgress();
                            formProgress.ShowDialog();
                            formProgress.Dispose();

                            if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested)
                            {
                                Generic.IsATWCancelled = true;
                                Common.Generic.cts.Dispose();
                                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ResetStatus();
                                return false;
                            }

                            int count = 0;
                            label_Filepath.Text = Generic.OpenFilePaths[0];

                            foreach (var file in Common.Generic.OpenFilePaths)
                            {
                                FileInfo fi = new(file);
                                if (File.Exists(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav"))
                                {
                                    if (File.Exists(TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav"))  // ファイルが既に存在する場合は削除してからMoveする
                                    {
                                        File.Delete(TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav");
                                        File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav", TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav");
                                    }
                                    else
                                    {
                                        File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav", TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav");
                                    }

                                }
                                else // Error
                                {
                                    Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                                    MessageBox.Show(this, Localization.ConvertErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ResetStatus();
                                    return false;
                                }

                                if (File.Exists(TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav")) // ファイル存在確認
                                {
                                    Common.Generic.OpenFilePaths[count] = TempAudioDir + @"\" + fi.Name.Replace(fi.Extension, "") + Utils.ATWSuffix() + ".wav";
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

                            FileInfo fisize = new(label_Filepath.Text);
                            long FS = fisize.Length;
                            label_Sizetxt.Text = string.Format(Localization.FileSizeCaption, FS / 1024, FS);

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
                    toolStripStatusLabel_EncMethod.Enabled = true;
                    toolStripStatusLabel_EncMethod.Visible = true;
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
                        toolStripStatusLabel_EncMethod.Enabled = true;
                        toolStripStatusLabel_EncMethod.Visible = true;
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
                Config.Load(xmlpath);

                bool faster_atrac = bool.Parse(Config.Entry["FasterATRAC"].Value);
                bool play_atrac = bool.Parse(Config.Entry["PlaybackATRAC"].Value);
                bool encodesource_atrac = bool.Parse(Config.Entry["ATRACEncodeSource"].Value);

                if (faster_atrac && play_atrac)
                {
                    panel_Main.BackgroundImage = Resources.SIEv2;
                    Common.Generic.IsWave = false;
                    Common.Generic.IsATRAC = true;
                    Generic.IsATW = false;
                    toolStripDropDownButton_EF.Enabled = false;
                    toolStripDropDownButton_EF.Visible = false;
                    toolStripStatusLabel_EncMethod.Enabled = false;
                    toolStripStatusLabel_EncMethod.Visible = false;
                    button_Decode.Enabled = true;
                    button_Encode.Enabled = false;
                    loopPointCreationToolStripMenuItem.Enabled = false;
                    button_Decode.PerformClick();
                }
                else if (!faster_atrac && play_atrac)
                {
                    panel_Main.BackgroundImage = Resources.SIEv2;
                    Common.Generic.IsWave = false;
                    Common.Generic.IsATRAC = true;
                    Generic.IsATW = false;
                    toolStripDropDownButton_EF.Enabled = false;
                    toolStripDropDownButton_EF.Visible = false;
                    toolStripStatusLabel_EncMethod.Enabled = false;
                    toolStripStatusLabel_EncMethod.Visible = false;
                    button_Decode.Enabled = true;
                    button_Encode.Enabled = false;
                    loopPointCreationToolStripMenuItem.Enabled = false;
                    if (PlaybackATRACConvert())
                    {
                        Generic.IsPlaybackATRAC = true;
                        ActivateOrDeactivateLPC(true);
                    }
                    else
                    {
                        ResetStatus();
                        return;
                    }
                }
                else if (faster_atrac && !play_atrac)
                {
                    panel_Main.BackgroundImage = Resources.SIEv2;
                    Common.Generic.IsWave = false;
                    Common.Generic.IsATRAC = true;
                    Generic.IsATW = false;
                    toolStripDropDownButton_EF.Enabled = false;
                    toolStripDropDownButton_EF.Visible = false;
                    toolStripStatusLabel_EncMethod.Enabled = false;
                    toolStripStatusLabel_EncMethod.Visible = false;
                    button_Decode.Enabled = true;
                    button_Encode.Enabled = false;
                    loopPointCreationToolStripMenuItem.Enabled = false;
                    button_Decode.PerformClick();
                }
                else
                {
                    if (encodesource_atrac) // ATRACをエンコード用ソースとして読み込み
                    {
                        toolStripStatusLabel_Status.Text = Localization.InitializationCaption;
                        label_Formattxt.Text = Localization.InitializationCaption;
                        Common.Generic.IsWave = false;
                        Common.Generic.IsATRAC = true;
                        Generic.IsATW = false;
                        if (PlaybackATRACConvert())
                        {
                            toolStripStatusLabel_Status.ForeColor = Color.Green;
                            toolStripStatusLabel_Status.Text = Localization.ReadyCaption;
                            ActivateOrDeactivateLPC(true);
                            label_Formattxt.Text = Localization.WAVEConvertedFormatCaption;
                            toolStripDropDownButton_EF.Enabled = true;
                            toolStripDropDownButton_EF.Visible = true;
                            toolStripStatusLabel_EncMethod.Enabled = true;
                            toolStripStatusLabel_EncMethod.Visible = true;
                            button_Decode.Enabled = false;
                            button_Encode.Enabled = true;
                            //SetMetaDatas();
                            //loopPointCreationToolStripMenuItem.Enabled = true;
                        }
                        else
                        {
                            ResetStatus();
                            return;
                        }
                    }
                    else
                    {
                        panel_Main.BackgroundImage = Resources.SIEv2;
                        Common.Generic.IsWave = false;
                        Common.Generic.IsATRAC = true;
                        Generic.IsATW = false;
                        toolStripDropDownButton_EF.Enabled = false;
                        toolStripDropDownButton_EF.Visible = false;
                        toolStripStatusLabel_EncMethod.Enabled = false;
                        toolStripStatusLabel_EncMethod.Visible = false;
                        button_Decode.Enabled = true;
                        button_Encode.Enabled = false;
                        loopPointCreationToolStripMenuItem.Enabled = false;
                        if (bool.Parse(Config.Entry["PlaybackATRAC"].Value))
                        {
                            if (PlaybackATRACConvert())
                            {
                                Generic.IsPlaybackATRAC = true;
                                ActivateOrDeactivateLPC(true);
                            }
                            else
                            {
                                ResetStatus();
                                return;
                            }
                        }
                    }

                }
            }
        }

        private bool PlaybackATRACConvert()
        {
            if (Common.Generic.OpenFilePaths.Length == 1) // 単一ファイル
            {
                FileInfo CurrentFi = new(Generic.OpenFilePaths[0]);
                Common.Generic.pATRACSavePath = Directory.GetCurrentDirectory() + @"\_temp\" + CurrentFi.Name.Replace(CurrentFi.Extension, ".ata");
                List<string> lst = [Generic.pATRACSavePath];
                Generic.pATRACOpenFilePaths = lst.ToArray();
                Common.Generic.ProgressMax = 1;
            }
            else // 複数ファイル
            {
                Common.Generic.pATRACFolderSavePath = Directory.GetCurrentDirectory() + @"\_temp\";
                Common.Generic.ProgressMax = Common.Generic.OpenFilePaths.Length;
            }

            Common.Generic.ProcessFlag = 0;

            Form formProgress = new FormProgress();
            formProgress.ShowDialog();
            formProgress.Dispose();

            if (Common.Generic.Result == false || Generic.cts.IsCancellationRequested) // 中断
            {
                Common.Generic.cts.Dispose();
                MessageBox.Show(this, Localization.CancelledCaption, Localization.MSGBoxAbortedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                return false;
            }
            else
            {
                if (Common.Generic.OpenFilePaths.Length == 1) // 単一
                {
                    FileInfo fi = new(Common.Generic.pATRACSavePath);
                    Common.Generic.cts.Dispose();
                    if (File.Exists(Common.Generic.pATRACSavePath))
                    {
                        if (fi.Length != 0) // OK
                        {
                            return true;
                        }
                        else // Error
                        {
                            File.Delete(Common.Generic.pATRACSavePath);
                            Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                            MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.DecodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    else // Exception
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else // 複数
                {
                    Common.Generic.cts.Dispose();

                    foreach (var file in Generic.OpenFilePaths)
                    {
                        FileInfo CurrentFi = new(file);
                        File.Move(Directory.GetCurrentDirectory() + @"\_temp\" + CurrentFi.Name.Replace(CurrentFi.Extension, ".wav"), Directory.GetCurrentDirectory() + @"\_temp\" + CurrentFi.Name.Replace(CurrentFi.Extension, ".ata"));
                    }

                    if (Common.Generic.OpenFilePaths.Length == Directory.GetFiles(Common.Generic.pATRACFolderSavePath, "*", SearchOption.AllDirectories).Length) // OK
                    {
                        List<string> lst = new();
                        List<string> lst2 = new();
                        foreach (var file in Generic.OpenFilePaths)
                        {
                            FileInfo CurrentFi = new(file);
                            Common.Generic.pATRACSavePath = Directory.GetCurrentDirectory() + @"\_temp\" + CurrentFi.Name.Replace(CurrentFi.Extension, ".ata");
                            lst.Add(Generic.pATRACSavePath);
                        }
                        Generic.pATRACOpenFilePaths = lst.ToArray();

                        foreach (var file in Generic.pATRACOpenFilePaths)
                        {
                            FileInfo fi = new(file);
                            if (fi.Length != 0)
                            {
                                lst2.Add(file);
                                continue;
                            }
                            else
                            {
                                File.Delete(file);
                                continue;
                            }
                        }

                        Generic.pATRACOpenFilePaths = null!;
                        if (lst.Count == lst2.Count) // OK
                        {
                            label_Filepath.Text = Generic.OpenFilePaths[0];
                            Generic.pATRACOpenFilePaths = lst2.ToArray();
                            return true;
                        }
                        else if (lst.Count > lst2.Count && lst2.Count != 0) // 一部変換失敗
                        {
                            FileInfo fp = new(Generic.OpenFilePaths[0]);
                            FileInfo ls = new(lst2[0]);
                            label_Filepath.Text = fp.Directory + @"\" + ls.Name.Replace(ls.Extension, fp.Extension);
                            Generic.pATRACOpenFilePaths = lst2.ToArray();
                            MessageBox.Show(this, Localization.DecodePartialCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        else
                        {
                            MessageBox.Show(this, string.Format("{0}\n\nLog: {1}", Localization.DecodeErrorCaption, Common.Utils.LogSplit(Common.Generic.Log)), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    else // Error
                    {
                        Common.Utils.DeleteDirectoryFiles(Directory.GetCurrentDirectory() + @"\_temp");
                        MessageBox.Show(this, Localization.DecodeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
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
            Dop d = new(CloseForm);
            fs?.Invoke(d);
        }

        private delegate void Dop();
        private static void CloseForm()
        {
            fs?.Close();
        }

        private delegate void Dmes(string message);
        private static void ShowMessage(string message)
        {
            fs!.label_log.Text = message;
        }
        #endregion

        /// <summary>
        /// LPCの処理
        /// </summary>
        /// <param name="flag">フラグ (true or false)</param>
        private void ActivateOrDeactivateLPC(bool flag)
        {
            if (bool.Parse(Config.Entry["PlaybackATRAC"].Value))
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
                    if (!Generic.IsPlaybackATRAC)
                    {
                        if (Common.Generic.IsWalkman)
                        {
                            FormLPC.FormLPCInstance.LoopCheckEnable = false;
                        }
                        if (!bool.Parse(Config.Entry["DisablePreviewWarning"].Value))
                        {
                            if (!Common.Generic.IsWalkman)
                            {
                                if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                                {
                                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                                    {
                                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3/ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                        MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    else
                                    {
                                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                        MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }

                                }
                                else
                                {
                                    if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                                    {
                                        FormLPC.FormLPCInstance.CautionLabel = "[ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                        MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    else
                                    {
                                        if (Generic.lpcreate)
                                        {
                                            FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                        }
                                        else
                                        {
                                            FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                                        }
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        Generic.IsPlaybackATRAC = false;
                    }
                }
                else
                {
                    if (FLPC is not null && FLPC.Visible)
                    {
                        FLPC.Close();
                    }
                }
            }
            else
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



                    if (Common.Generic.IsWalkman)
                    {
                        FormLPC.FormLPCInstance.LoopCheckEnable = false;
                    }
                    if (!bool.Parse(Config.Entry["DisablePreviewWarning"].Value))
                    {
                        if (!Common.Generic.IsWalkman)
                        {
                            if (Generic.IsAT3LoopPoint || Generic.IsAT3LoopSound)
                            {
                                if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                                {
                                    FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3/ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                    MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    FormLPC.FormLPCInstance.CautionLabel = "[ATRAC3] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                    MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }

                            }
                            else
                            {
                                if (Generic.IsAT9LoopPoint || Generic.IsAT9LoopSound)
                                {
                                    FormLPC.FormLPCInstance.CautionLabel = "[ATRAC9] Caution: The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                    MessageBox.Show(this, Localization.PreviewWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    if (Generic.lpcreate)
                                    {
                                        FormLPC.FormLPCInstance.CautionLabel = "LPC Enabled. The settings made on this screen are for confirmation only and will not be reflected in the actual output.";
                                    }
                                    else
                                    {
                                        FormLPC.FormLPCInstance.CautionLabel = string.Empty;
                                    }
                                }
                            }
                        }
                    }


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

        public void SetMetaDatas()
        {
            if (Generic.IsATRAC && Generic.OpenFilePaths.Length == 1)
            {
                int[] loopbuf = new int[2];
                if (Utils.GetATRACLooped(Generic.ATRACMetadataBuffers, loopbuf))
                {
                    Generic.IsATRACLooped = true;
                    FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = true;
                    FormLPC.FormLPCInstance.BufferLoopPosition = loopbuf;
                }
                else
                {
                    Generic.IsATRACLooped = false;
                }
                // UI META Labels
            }
            else if (Generic.IsATRAC && Generic.IsLPCStreamingReloaded || Generic.IsATRAC && Generic.OpenFilePaths.Length > 1)
            {
                uint pos = FormLPC.FormLPCInstance.ButtonPosition;
                //bool looped = Utils.SearchATRACSampleChunk(Generic.pATRACOpenFilePaths[pos - 1]);
                int[] data = new int[2];
                if (Utils.GetATRACLoopedMulti(Generic.pATRACOpenFilePaths, Generic.ATRACMultiMetadataBuffer, data, pos - 1))
                {
                    Generic.IsATRACLooped = true;
                    FormLPC.FormLPCInstance.checkBox_LoopEnable.Checked = true;
                    FormLPC.FormLPCInstance.BufferLoopPosition = data;
                }
                else
                {
                    Generic.IsATRACLooped = false;
                }
                // UI META Labels
            }
            else if (!Generic.IsATRAC && Generic.IsLPCStreamingReloaded && Generic.OpenFilePaths.Length > 1)
            {
                uint pos = FormLPC.FormLPCInstance.ButtonPosition;
                // UI Labels (Not META)
            }
            else
            {
                /*label_Filename.Text = "FILE_NAME: " + GetFileName(OriginalPaths[0]);
                label_OrigFilepath.Text = "FILE_PATH: " + OriginalPaths[0];
                label_id.Text = "STREAM_ID: -";
                label_NStream.Text = "NUMBER_OF_STREAMS: -";
                label_Totalstream.Text = "STREAM_TOTAL: " + GetStreamTotals(OpenFilePaths[0]).ToString() + " (FILE_SIZE: " + GetFileSize(OpenFilePaths[0]).ToString() + " )";
                label_lsPosition.Text = "LOOP_START: -";
                label_lePosition.Text = "LOOP_END: -";
                label_Volume_Info.Text = "VOLUME: -";
                label_Channel_Info.Text = "CHANNELS: -";
                label_Samplerate_Info.Text = "SAMPLE_LATES: -";*/
            }
        }

        private void label_NotReaded_Click(object sender, EventArgs e)
        {

        }

        private void EncodeMethodIsATRAC(bool flag)
        {
            switch (flag)
            {
                case true:
                    FLPC?.ATRACRadioButtonChanger(true);
                    Common.Generic.IsWalkman = false;
                    break;
                case false:
                    FLPC?.ATRACRadioButtonChanger(false);
                    Common.Generic.IsWalkman = true;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deencflag">フラグ (true:Encode, false:Decode)</param>
        /// <param name="swit">switch式</param>
        private void SetWalkmanMultiConvertFormats(bool deencflag, int swit)
        {
            if (!deencflag)
            {
                switch (swit)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (swit)
                {
                    case 0: // PCM
                        Generic.WalkmanMultiConvExt = ".oma";
                        break;
                    case 1: // ATRAC3 OMA
                        Generic.WalkmanMultiConvExt = ".oma";
                        break;
                    case 2: // ATRAC3 OMG
                        Generic.WalkmanMultiConvExt = ".omg";
                        break;
                    case 3: // ATRAC3 AL
                        Generic.WalkmanMultiConvExt = ".oma";
                        break;
                    case 4: // ATRAC3 KDR
                        Generic.WalkmanMultiConvExt = ".kdr";
                        break;
                    case 5: // ATRAC3+ OMA
                        Generic.WalkmanMultiConvExt = ".oma";
                        break;
                    case 6: // ATRAC3+ OMG
                        Generic.WalkmanMultiConvExt = ".omg";
                        break;
                    case 7: // ATRAC3+ AL
                        Generic.WalkmanMultiConvExt = ".oma";
                        break;
                    case 8 - 9: // ATRAC3+ KDR
                        Generic.WalkmanMultiConvExt = ".kdr";
                        break;
                    default:
                        break;
                }
            }
        }

        private void toolStripDropDownButton_EF_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void textBox_LoopStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }

        }

        private void textBox_LoopEnd_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }

        }

        private void textBox_LoopStart_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_LoopStart.Text))
            {
                return;
            }
            if (FormLPC.FormLPCInstance.TotalSamples < int.Parse(textBox_LoopStart.Text))
            {
                textBox_LoopStart.Text = textBox_LoopStart.Text.Remove(textBox_LoopStart.TextLength - 1);
                return;
            }
            else
            {
                switch (FormLPC.FormLPCInstance.SampleRate)
                {
                    case 12000: // ATRAC9 Only
                        FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 12.0, MidpointRounding.AwayFromZero);//value[0];
                        FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                        break;
                    case 24000: // ATRAC9 Only
                        FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 24.0, MidpointRounding.AwayFromZero);//value[0];
                        FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                        break;
                    case 44100:
                        FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 44.1, MidpointRounding.AwayFromZero);//value[0];
                        FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                        break;
                    case 48000:
                        FormLPC.FormLPCInstance.customTrackBar_Start.Value = (int)Math.Round(int.Parse(textBox_LoopStart.Text) / 48.0, MidpointRounding.AwayFromZero);//value[0];
                        FormLPC.FormLPCInstance.numericUpDown_LoopStart.Value = FormLPC.FormLPCInstance.customTrackBar_Start.Value;
                        break;
                    default:
                        break;
                }
                FormLPC.FormLPCInstance.customTrackBar_Start.Invalidate();
            }

        }

        private void textBox_LoopEnd_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_LoopEnd.Text))
            {
                return;
            }
            if (FormLPC.FormLPCInstance.TotalSamples < int.Parse(textBox_LoopEnd.Text))
            {
                textBox_LoopEnd.Text = textBox_LoopEnd.Text.Remove(textBox_LoopEnd.TextLength - 1);
                return;
            }
            else
            {
                switch (FormLPC.FormLPCInstance.SampleRate)
                {
                    case 12000: // ATRAC9 Only
                        FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 12.0, MidpointRounding.AwayFromZero);//value[1];
                        FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                        break;
                    case 24000: // ATRAC9 Only
                        FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 24.0, MidpointRounding.AwayFromZero);//value[1];
                        FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                        break;
                    case 44100:
                        FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 44.1, MidpointRounding.AwayFromZero);//value[1];
                        FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                        break;
                    case 48000:
                        FormLPC.FormLPCInstance.customTrackBar_End.Value = (int)Math.Round(int.Parse(textBox_LoopEnd.Text) / 48.0, MidpointRounding.AwayFromZero);//value[1];
                        FormLPC.FormLPCInstance.numericUpDown_LoopEnd.Value = FormLPC.FormLPCInstance.customTrackBar_End.Value;
                        break;
                    default:
                        break;
                }
                FormLPC.FormLPCInstance.customTrackBar_End.Invalidate();
            }
        }

        private void groupBox_Loop_Enter(object sender, EventArgs e)
        {

        }
    }
}