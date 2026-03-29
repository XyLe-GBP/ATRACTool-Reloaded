using ATRACTool_Reloaded.Localizable;
using Microsoft.VisualBasic;
using NAudio.Wave;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormPreferencesSettings : Form
    {
        private static FormPreferencesSettings _formPreferencesSettingsInstance = null!;
        public static FormPreferencesSettings FormPreferencesSettingsInstance
        {
            get
            {
                return _formPreferencesSettingsInstance;
            }
            set
            {
                _formPreferencesSettingsInstance = value;
            }
        }

        public FormPreferencesSettings()
        {
            InitializeComponent();

            FormMain.DebugInfo("[FormPreferencesSettings] Initialized.");
        }

        private static string Path = null!;
        private static bool FirstLoad = true, ASIODriverNotFound = true;

        private void FormPreferencesSettings_Load(object sender, EventArgs e)
        {
            comboBox_Fixconvert.SelectedIndex = 0;

            Config.Load(xmlpath);

            FirstLoad = true;
            try
            {
                // 更新チェック
                checkBox_Checkupdate.Checked = Utils.GetBool("Check_Update", true);

                // スムーズサンプル
                bool smooth = Utils.GetBool("SmoothSamples", false);
                checkBox_Smoothsamples.Checked = smooth;

                bool latencyControlsEnabled = !smooth;
                label_DSBuffers.Enabled = latencyControlsEnabled;
                label_DSLatency.Enabled = latencyControlsEnabled;
                label_WASAPILatencyS.Enabled = latencyControlsEnabled;
                label_WASAPILatencyE.Enabled = latencyControlsEnabled;
                comboBox_DSBuffers.Enabled = latencyControlsEnabled;
                comboBox_DSLatencys.Enabled = latencyControlsEnabled;
                comboBox_WASAPILatencysS.Enabled = latencyControlsEnabled;
                comboBox_WASAPILatencysE.Enabled = latencyControlsEnabled;

                // ATRAC 再生 / プレビュー警告
                checkBox_EnableATRACPlayback.Checked = Utils.GetBool("PlaybackATRAC", false);
                checkBox_DisablePreviewWarning.Checked = Utils.GetBool("DisablePreviewWarning", false);

                // スプラッシュ画像
                bool useSplashImage = Utils.GetBool("SplashImage", false);
                checkBox_Splashimg.Checked = useSplashImage;
                textBox_Splashimg.Enabled = useSplashImage;
                button_Splashimg.Enabled = useSplashImage;

                if (useSplashImage)
                {
                    string splashPath = Utils.GetString("SplashImage_Path", string.Empty);
                    textBox_Splashimg.Text = splashPath;
                }
                else
                {
                    textBox_Splashimg.Text = string.Empty;
                }

                // 旧モード / スプラッシュ非表示 / 高速ATRAC
                checkBox_Oldmode.Checked = Utils.GetBool("Oldmode", false);
                checkBox_Hidesplash.Checked = Utils.GetBool("HideSplash", false);
                checkBox_FasterATRAC.Checked = Utils.GetBool("FasterATRAC", false);

                // 固定変換
                bool fixedConvert = Utils.GetBool("FixedConvert", false);
                checkBox_Fixconvert.Checked = fixedConvert;
                comboBox_Fixconvert.Enabled = fixedConvert;

                if (fixedConvert)
                {
                    int convertType = Utils.GetInt("ConvertType", 0);
                    if (convertType < 0 || convertType >= comboBox_Fixconvert.Items.Count)
                    {
                        convertType = 0;
                    }
                    comboBox_Fixconvert.SelectedIndex = convertType;
                }

                // WAV 強制変換 / ATRAC エンコードソース
                checkBox_ForceConvertWaveOnly.Checked = Utils.GetBool("ForceConvertWaveOnly", false);
                checkBox_ATRACEncodeSource.Checked = Utils.GetBool("ATRACEncodeSource", false);

                // 保存先（通常 / 指定フォルダ）
                bool saveIsManual = Utils.GetBool("Save_IsManual", false);
                if (saveIsManual)
                {
                    radioButton_spc.Checked = true;
                    radioButton_nml.Checked = false;

                    label_IO_Path.Enabled = true;
                    button_Clear.Enabled = true;
                    button_Browse.Enabled = true;
                    textBox_Path.Enabled = true;
                    checkBox_Subfolder.Enabled = true;

                    string saveFolder = Utils.GetString("Save_Isfolder", string.Empty);
                    textBox_Path.Text = string.IsNullOrEmpty(saveFolder) ? null : saveFolder;

                    bool saveIsSubfolder = Utils.GetBool("Save_IsSubfolder", false);
                    if (saveIsSubfolder)
                    {
                        label_IO_SubfolderSuffix.Enabled = true;
                        textBox_suffix.Enabled = true;
                        checkBox_Subfolder.Checked = true;

                        string suffix = Utils.GetString("Save_Subfolder_Suffix", string.Empty);
                        textBox_suffix.Text = string.IsNullOrEmpty(suffix) ? null : suffix;
                    }
                    else
                    {
                        label_IO_SubfolderSuffix.Enabled = false;
                        textBox_suffix.Text = null;
                        textBox_suffix.Enabled = false;
                        checkBox_Subfolder.Checked = false;
                    }
                }
                else
                {
                    radioButton_spc.Checked = false;
                    radioButton_nml.Checked = true;

                    label_IO_Path.Enabled = false;
                    label_IO_SubfolderSuffix.Enabled = false;
                    button_Clear.Enabled = false;
                    button_Browse.Enabled = false;
                    textBox_Path.Text = null;
                    textBox_Path.Enabled = false;
                    textBox_suffix.Text = null;
                    textBox_suffix.Enabled = false;
                    checkBox_Subfolder.Checked = false;
                    checkBox_Subfolder.Enabled = false;
                }

                // 保存設定その他
                checkBox_IO_SaveSourcesnest.Checked = Utils.GetBool("Save_NestFolderSource", false);
                checkBox_SaveDeleteHzSuffix.Checked = Utils.GetBool("Save_DeleteHzSuffix", false);
                checkBox_ShowFolder.Checked = Utils.GetBool("ShowFolder", true);

                // LPC 再生方法
                int lpcPlaybackMethod = Utils.GetInt("LPCPlaybackMethod", 0);
                switch (lpcPlaybackMethod)
                {
                    case 0:
                        comboBox_LPCplayback.SelectedIndex = 0;
                        label_LPC_ASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Enabled = false;
                        break;
                    case 1:
                        comboBox_LPCplayback.SelectedIndex = 1;
                        label_LPC_ASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Enabled = false;
                        break;
                    case 2:
                        comboBox_LPCplayback.SelectedIndex = 2;
                        label_LPC_ASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Enabled = false;
                        break;
                    case 3:
                        comboBox_LPCplayback.SelectedIndex = 3;
                        label_LPC_ASIODriver.Enabled = true;
                        comboBox_LPCASIODriver.Enabled = true;
                        break;
                    default:
                        comboBox_LPCplayback.SelectedIndex = 0;
                        label_LPC_ASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Enabled = false;
                        break;
                }

                // ASIO ドライバ名
                string lpcAsioDriver = Utils.GetString("LPCUseASIODriver", string.Empty);
                if (string.IsNullOrWhiteSpace(lpcAsioDriver))
                {
                    label_LPC_ASIODriver.Enabled = false;
                    comboBox_LPCASIODriver.Enabled = false;
                }
                else
                {
                    if (ASIODriverNotFound)
                    {
                        label_LPC_ASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Enabled = false;
                    }
                    else
                    {
                        int count = 0;
                        foreach (var obj in comboBox_LPCASIODriver.Items!)
                        {
                            if (lpcAsioDriver == comboBox_LPCASIODriver.Items[count]!.ToString())
                            {
                                comboBox_LPCASIODriver.SelectedIndex = count;
                                break;
                            }
                            count++;
                        }
                    }
                }

                // マルチストリーム設定
                bool lpcMultiAlwaysWasapiOrAsio = Utils.GetBool("LPCMultipleStreamAlwaysWASAPIorASIO", false);
                checkBox_MultisoundDontDS.Checked = lpcMultiAlwaysWasapiOrAsio;

                int lpcMultiPlaybackMethod = Utils.GetInt("LPCMultipleStreamPlaybackMethod", 0);
                switch (lpcMultiPlaybackMethod)
                {
                    case 0:
                        comboBox_LPCMultisourcePlaybackmode.SelectedIndex = 0;
                        label_LPC_MultiplesourcePlaybackmode.Enabled = true;
                        comboBox_LPCMultisourcePlaybackmode.Enabled = true;
                        break;
                    case 1:
                        comboBox_LPCMultisourcePlaybackmode.SelectedIndex = 1;
                        label_LPC_MultiplesourcePlaybackmode.Enabled = true;
                        comboBox_LPCMultisourcePlaybackmode.Enabled = true;
                        break;
                    case 2:
                        comboBox_LPCMultisourcePlaybackmode.SelectedIndex = 2;
                        label_LPC_MultiplesourcePlaybackmode.Enabled = true;
                        comboBox_LPCMultisourcePlaybackmode.Enabled = true;
                        break;
                    default:
                        comboBox_LPCMultisourcePlaybackmode.SelectedIndex = 0;
                        label_LPC_MultiplesourcePlaybackmode.Enabled = false;
                        comboBox_LPCMultisourcePlaybackmode.Enabled = false;
                        break;
                }

                // レイテンシ等のコンボボックス（範囲チェック付き）
                int dsBuffers = Utils.GetInt("DirectSoundBuffers", 0);
                if (dsBuffers < 0 || dsBuffers >= comboBox_DSBuffers.Items.Count) dsBuffers = 0;
                comboBox_DSBuffers.SelectedIndex = dsBuffers;

                int dsLatency = Utils.GetInt("DirectSoundLatency", 0);
                if (dsLatency < 0 || dsLatency >= comboBox_DSLatencys.Items.Count) dsLatency = 0;
                comboBox_DSLatencys.SelectedIndex = dsLatency;

                int wspShared = Utils.GetInt("WASAPILatencyShared", 0);
                if (wspShared < 0 || wspShared >= comboBox_WASAPILatencysS.Items.Count) wspShared = 0;
                comboBox_WASAPILatencysS.SelectedIndex = wspShared;

                int wspExclusive = Utils.GetInt("WASAPILatencyExclusived", 0);
                if (wspExclusive < 0 || wspExclusive >= comboBox_WASAPILatencysE.Items.Count) wspExclusive = 0;
                comboBox_WASAPILatencysE.SelectedIndex = wspExclusive;

                int playbackThreads = Utils.GetInt("PlaybackThreadCount", 0);
                if (playbackThreads < 0 || playbackThreads >= comboBox_PlaybackThreadCounts.Items.Count) playbackThreads = 0;
                comboBox_PlaybackThreadCounts.SelectedIndex = playbackThreads;

                // Parallel.ForEachを使用
                bool Parallelmethod = Utils.GetBool("UseParallelMethod", false);
                checkBox_Usepal.Checked = Parallelmethod;

                // デバッグモード
                if (AssemblyState.IsDebug)
                {
                    bool DebugMode = Utils.GetBool("Debugmode", false);
                    checkBox_debug.Checked = DebugMode;
                }
                else
                {
#pragma warning disable CS0162 // 到達できないコードが検出されました
                    checkBox_debug.Checked = false;
#pragma warning restore CS0162 // 到達できないコードが検出されました
                    checkBox_debug.Enabled = false;
                }

                FirstLoad = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, string.Format("An error has occurred.\n{0}\nThe configuration file is incorrect.", ex), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Generic.IsConfigError = true;
                Close();
            }
        }

        private void RadioButton_nml_CheckedChanged(object sender, EventArgs e)
        {
            label_IO_Path.Enabled = false;
            label_IO_SubfolderSuffix.Enabled = false;
            button_Clear.Enabled = false;
            button_Browse.Enabled = false;
            textBox_Path.Text = null;
            textBox_Path.Enabled = false;
            textBox_suffix.Text = null;
            textBox_suffix.Enabled = false;
            checkBox_Subfolder.Checked = false;
            checkBox_Subfolder.Enabled = false;
        }

        private void RadioButton_spc_CheckedChanged(object sender, EventArgs e)
        {
            label_IO_Path.Enabled = true;
            button_Clear.Enabled = true;
            button_Browse.Enabled = true;
            textBox_Path.Text = null;
            textBox_Path.Enabled = true;
            checkBox_Subfolder.Enabled = true;
        }

        private void Button_Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new()
            {
                Description = Localization.FolderSaveDialogTitle,
                RootFolder = Environment.SpecialFolder.MyDocuments,
                SelectedPath = @"",
            };

            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                textBox_Path.Text = fbd.SelectedPath;
                Path = fbd.SelectedPath;
            }
        }

        private void Button_Clear_Click(object sender, EventArgs e)
        {
            textBox_Path.Text = null;
        }

        private void CheckBox_Subfolder_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_Subfolder.Checked != true)
            {
                label_IO_SubfolderSuffix.Enabled = false;
                textBox_suffix.Text = null;
                textBox_suffix.Enabled = false;
            }
            else
            {
                label_IO_SubfolderSuffix.Enabled = true;
                textBox_suffix.Enabled = true;
            }
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            if (checkBox_Splashimg.Checked == true && string.IsNullOrEmpty(textBox_Splashimg.Text))
            {
                MessageBox.Show(this, Localization.SplashPathErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (radioButton_spc.Checked == true && textBox_Path.Text == "")
            {
                MessageBox.Show(this, Localization.SpecificPathErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (radioButton_spc.Checked == true && checkBox_Subfolder.Checked == true && textBox_suffix.Text == "")
            {
                MessageBox.Show(this, Localization.SpecificSubfolderErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string oldsplashimgpath = Config.Entry["SplashImage_Path"].Value;


            if (checkBox_Checkupdate.Checked != true)
            {
                Config.Entry["Check_Update"].Value = "false";
            }
            else
            {
                Config.Entry["Check_Update"].Value = "true";
            }

            if (checkBox_Smoothsamples.Checked != true)
            {
                Config.Entry["SmoothSamples"].Value = "false";
            }
            else
            {
                Config.Entry["SmoothSamples"].Value = "true";
            }

            if (checkBox_EnableATRACPlayback.Checked != true)
            {
                Config.Entry["PlaybackATRAC"].Value = "false";
            }
            else
            {
                Config.Entry["PlaybackATRAC"].Value = "true";
            }

            if (checkBox_DisablePreviewWarning.Checked != true)
            {
                Config.Entry["DisablePreviewWarning"].Value = "false";
            }
            else
            {
                Config.Entry["DisablePreviewWarning"].Value = "true";
            }

            if (checkBox_Splashimg.Checked != true)
            {
                Config.Entry["SplashImage"].Value = "false";
                Config.Entry["SplashImage_Path"].Value = "";
            }
            else
            {
                Config.Entry["SplashImage"].Value = "true";
                Config.Entry["SplashImage_Path"].Value = textBox_Splashimg.Text;
            }

            if (checkBox_Oldmode.Checked != true)
            {
                Config.Entry["Oldmode"].Value = "false";
            }
            else
            {
                Config.Entry["Oldmode"].Value = "true";
            }

            if (checkBox_debug.Checked != true)
            {
                Config.Entry["Debugmode"].Value = "false";
            }
            else
            {
                if (AssemblyState.IsDebug)
                {
                    Config.Entry["Debugmode"].Value = "true";
                }
                else
                {
#pragma warning disable CS0162 // 到達できないコードが検出されました
                    Config.Entry["Debugmode"].Value = "false";
#pragma warning restore CS0162 // 到達できないコードが検出されました
                }
            }

            if (checkBox_Hidesplash.Checked != true)
            {
                Config.Entry["HideSplash"].Value = "false";
            }
            else
            {
                Config.Entry["HideSplash"].Value = "true";
            }

            if (checkBox_FasterATRAC.Checked != true)
            {
                Config.Entry["FasterATRAC"].Value = "false";
            }
            else
            {
                Config.Entry["FasterATRAC"].Value = "true";
            }

            if (checkBox_Fixconvert.Checked != true)
            {
                Config.Entry["FixedConvert"].Value = "false";
                Config.Entry["ConvertType"].Value = "";
            }
            else
            {
                Config.Entry["FixedConvert"].Value = "true";
                Config.Entry["ConvertType"].Value = comboBox_Fixconvert.SelectedIndex.ToString();
            }

            if (checkBox_ForceConvertWaveOnly.Checked != true)
            {
                Config.Entry["ForceConvertWaveOnly"].Value = "false";
            }
            else
            {
                Config.Entry["ForceConvertWaveOnly"].Value = "true";
            }

            if (checkBox_ATRACEncodeSource.Checked != true)
            {
                Config.Entry["ATRACEncodeSource"].Value = "false";
            }
            else
            {
                Config.Entry["ATRACEncodeSource"].Value = "true";
            }

            if (radioButton_nml.Checked != true)
            {
                Config.Entry["Save_IsManual"].Value = "true";
            }
            else
            {
                Config.Entry["Save_IsManual"].Value = "false";
            }

            if (checkBox_Subfolder.Checked != true)
            {
                Config.Entry["Save_IsSubfolder"].Value = "false";
            }
            else
            {
                Config.Entry["Save_IsSubfolder"].Value = "true";
            }

            if (Path != "")
            {
                Config.Entry["Save_Isfolder"].Value = Path;
            }
            else
            {
                Config.Entry["Save_Isfolder"].Value = "";
            }

            if (textBox_suffix.Text != "")
            {
                Config.Entry["Save_Subfolder_Suffix"].Value = textBox_suffix.Text;
            }
            else
            {
                Config.Entry["Save_Subfolder_Suffix"].Value = "";
            }

            if (checkBox_IO_SaveSourcesnest.Checked)
            {
                Config.Entry["Save_NestFolderSource"].Value = "true";
            }
            else
            {
                Config.Entry["Save_NestFolderSource"].Value = "false";
            }

            if (checkBox_SaveDeleteHzSuffix.Checked)
            {
                Config.Entry["Save_DeleteHzSuffix"].Value = "true";
            }
            else
            {
                Config.Entry["Save_DeleteHzSuffix"].Value = "false";
            }

            if (checkBox_ShowFolder.Checked != true)
            {
                Config.Entry["ShowFolder"].Value = "false";
            }
            else
            {
                Config.Entry["ShowFolder"].Value = "true";
            }

            Config.Entry["LPCPlaybackMethod"].Value = comboBox_LPCplayback.SelectedIndex.ToString();

            if (comboBox_LPCASIODriver.Items.Count != 0)
            {
                Config.Entry["LPCUseASIODriver"].Value = comboBox_LPCASIODriver.Items[comboBox_LPCASIODriver.SelectedIndex]?.ToString()!;
            }
            else
            {
                Config.Entry["LPCUseASIODriver"].Value = "";
            }

            if (checkBox_MultisoundDontDS.Checked)
            {
                Config.Entry["LPCMultipleStreamAlwaysWASAPIorASIO"].Value = "true";
                Config.Entry["LPCMultipleStreamPlaybackMethod"].Value = comboBox_LPCMultisourcePlaybackmode.SelectedIndex.ToString();
            }
            else
            {
                Config.Entry["LPCMultipleStreamAlwaysWASAPIorASIO"].Value = "false";
                Config.Entry["LPCMultipleStreamPlaybackMethod"].Value = "65535";
            }

            if (!checkBox_Smoothsamples.Checked)
            {
                Config.Entry["DirectSoundBuffers"].Value = comboBox_DSBuffers.SelectedIndex.ToString();
                Config.Entry["DirectSoundBuffersValue"].Value = comboBox_DSBuffers.Text;
                Config.Entry["DirectSoundLatency"].Value = comboBox_DSLatencys.SelectedIndex.ToString();
                Config.Entry["DirectSoundLatencyValue"].Value = comboBox_DSLatencys.Text;
                Config.Entry["WASAPILatencyShared"].Value = comboBox_WASAPILatencysS.SelectedIndex.ToString();
                Config.Entry["WASAPILatencySharedValue"].Value = comboBox_WASAPILatencysS.Text;
                Config.Entry["WASAPILatencyExclusived"].Value = comboBox_WASAPILatencysE.SelectedIndex.ToString();
                Config.Entry["WASAPILatencyExclusivedValue"].Value = comboBox_WASAPILatencysE.Text;
            }

            if (checkBox_Usepal.Checked)
            {
                Config.Entry["UseParallelMethod"].Value = "true";
            }
            else
            {
                Config.Entry["UseParallelMethod"].Value = "false";
            }

            Config.Entry["PlaybackThreadCount"].Value = comboBox_PlaybackThreadCounts.SelectedIndex.ToString();

            Config.Save(xmlpath);

            if (checkBox_Splashimg.Checked == true && !string.IsNullOrEmpty(textBox_Splashimg.Text) && oldsplashimgpath != textBox_Splashimg.Text)
            {
                MessageBox.Show(this, Localization.CustomSplashCaption, Localization.MSGBoxSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Close();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CheckBox_Checkupdate_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox_Checkupdate.Checked)
            {
                checkBox_Checkupdate.Checked = false;
            }
            else
            {
                checkBox_Checkupdate.Checked = true;
            }
        }

        private void CheckBox_Splashimg_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox_Splashimg.Checked)
            {
                checkBox_Splashimg.Checked = false;
                textBox_Splashimg.Text = string.Empty;
                textBox_Splashimg.Enabled = false;
                button_Splashimg.Enabled = false;
            }
            else
            {
                checkBox_Splashimg.Checked = true;
                textBox_Splashimg.Text = string.Empty;
                textBox_Splashimg.Enabled = true;
                button_Splashimg.Enabled = true;
            }
        }

        private void Button_Splashimg_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filter = "All Supported Files (*.jpg, *.png, *.bmp)|*.jpg;*.png;*.bmp",
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using var img = Image.FromFile(ofd.FileName);
                if (img.Width == 800 && img.Height == 480)
                {
                    textBox_Splashimg.Text = ofd.FileName;
                }
                else if (img.Width == 400 && img.Height == 240)
                {
                    textBox_Splashimg.Text = ofd.FileName;
                }
                else
                {
                    MessageBox.Show(Localization.CustomSplashSizeErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox_Splashimg.Text = string.Empty;
                }
                return;
            }
            else
            {
                return;
            }
        }

        private void CheckBox_Fixconvert_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox_Fixconvert.Checked)
            {
                checkBox_Fixconvert.Checked = false;
                comboBox_Fixconvert.Enabled = false;
            }
            else
            {
                checkBox_Fixconvert.Checked = true;
                comboBox_Fixconvert.Enabled = true;
                comboBox_Fixconvert.SelectedIndex = 0;
            }
        }

        private void ComboBox_Fixconvert_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_Fixconvert.SelectedIndex)
            {
                default:
                    break;
            }
        }

        private void ComboBox_LPCplayback_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_LPCplayback.SelectedIndex)
            {
                case 0:
                    if (comboBox_LPCMultisourcePlaybackmode.SelectedIndex != 2)
                    {
                        label_LPC_ASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Items.Clear();
                    }
                    break;
                case 1:
                    if (comboBox_LPCMultisourcePlaybackmode.SelectedIndex != 2)
                    {
                        label_LPC_ASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Items.Clear();
                    }
                    break;
                case 2:
                    if (comboBox_LPCMultisourcePlaybackmode.SelectedIndex != 2)
                    {
                        label_LPC_ASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Items.Clear();
                    }
                    break;
                case 3:
                    {
                        if (comboBox_LPCMultisourcePlaybackmode.SelectedIndex != 2)
                        {
                            if (!FirstLoad)
                            {
                                MessageBox.Show(Localization.ASIOWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            GetandSetASIODrivers();
                        }

                        break;
                    }
                default:
                    label_LPC_ASIODriver.Enabled = false;
                    comboBox_LPCASIODriver.Enabled = false;
                    comboBox_LPCASIODriver.Items.Clear();
                    break;
            }
        }

        private void CheckBox_MultisoundDontDS_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_MultisoundDontDS.Checked)
            {
                label_LPC_MultiplesourcePlaybackmode.Enabled = true;
                comboBox_LPCMultisourcePlaybackmode.Enabled = true;
            }
            else
            {
                label_LPC_MultiplesourcePlaybackmode.Enabled = false;
                comboBox_LPCMultisourcePlaybackmode.Enabled = false;
            }
        }

        private void ComboBox_LPCMultisourcePlaybackmode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_LPCMultisourcePlaybackmode.SelectedIndex)
            {
                case 0:
                    if (comboBox_LPCASIODriver.Enabled && comboBox_LPCplayback.SelectedIndex != 3)
                    {
                        label_LPC_ASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Items.Clear();
                    }
                    break;
                case 1:
                    if (comboBox_LPCASIODriver.Enabled && comboBox_LPCplayback.SelectedIndex != 3)
                    {
                        label_LPC_ASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Enabled = false;
                        comboBox_LPCASIODriver.Items.Clear();
                    }
                    break;
                case 2:
                    if (!comboBox_LPCASIODriver.Enabled && comboBox_LPCplayback.SelectedIndex != 3)
                    {
                        if (!FirstLoad)
                        {
                            MessageBox.Show(Localization.ASIOWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        GetandSetASIODrivers();
                    }
                    break;
                default:
                    break;
            }
        }

        private void CheckBox_Smoothsamples_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_Smoothsamples.Checked)
            {
                label_DSBuffers.Enabled = false;
                label_DSLatency.Enabled = false;
                label_WASAPILatencyS.Enabled = false;
                label_WASAPILatencyE.Enabled = false;
                comboBox_DSBuffers.Enabled = false;
                comboBox_DSLatencys.Enabled = false;
                comboBox_WASAPILatencysS.Enabled = false;
                comboBox_WASAPILatencysE.Enabled = false;
            }
            else
            {
                label_DSBuffers.Enabled = true;
                label_DSLatency.Enabled = true;
                label_WASAPILatencyS.Enabled = true;
                label_WASAPILatencyE.Enabled = true;
                comboBox_DSBuffers.Enabled = true;
                comboBox_DSLatencys.Enabled = true;
                comboBox_WASAPILatencysS.Enabled = true;
                comboBox_WASAPILatencysE.Enabled = true;
            }
        }

        private void GetandSetASIODrivers()
        {
            label_LPC_ASIODriver.Enabled = true;
            comboBox_LPCASIODriver.Enabled = true;

            comboBox_LPCASIODriver.Items.Clear();
            string[] DriverList = AsioOut.GetDriverNames();
            foreach (string s in DriverList)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    continue;
                }
                else
                {
                    comboBox_LPCASIODriver.Items.Add(s);
                }
            }

            if (comboBox_LPCASIODriver.Items is null)
            {
                MessageBox.Show(Localization.ASIODriverNotFoundCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ASIODriverNotFound = true;
                label_LPC_ASIODriver.Enabled = false;
                comboBox_LPCASIODriver.Enabled = false;
            }
            else
            {
                ASIODriverNotFound = false;
                comboBox_LPCASIODriver.SelectedIndex = 0;
            }
        }

        private void FormPreferencesSettings_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain.DebugInfo("[FormPreferencesSettings] Closed.");
        }
    }
}
