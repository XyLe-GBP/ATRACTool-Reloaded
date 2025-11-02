using ATRACTool_Reloaded.Localizable;
using Microsoft.VisualBasic;
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
        }

        private static string Path = null!;

        private void FormPreferencesSettings_Load(object sender, EventArgs e)
        {
            comboBox_Fixconvert.SelectedIndex = 0;

            Config.Load(xmlpath);

            try
            {
                switch (bool.Parse(Config.Entry["Check_Update"].Value))
                {
                    case true:
                        checkBox_Checkupdate.Checked = true;
                        break;
                    case false:
                        checkBox_Checkupdate.Checked = false;
                        break;
                }

                switch (bool.Parse(Config.Entry["SmoothSamples"].Value))
                {
                    case true:
                        checkBox_Smoothsamples.Checked = true;
                        label_DSBuffers.Enabled = false;
                        label_DSLatency.Enabled = false;
                        label_WASAPILatencyS.Enabled = false;
                        label_WASAPILatencyE.Enabled = false;
                        comboBox_DSBuffers.Enabled = false;
                        comboBox_DSLatencys.Enabled = false;
                        comboBox_WASAPILatencysS.Enabled = false;
                        comboBox_WASAPILatencysE.Enabled = false;
                        break;
                    case false:
                        checkBox_Smoothsamples.Checked = false;
                        label_DSBuffers.Enabled = true;
                        label_DSLatency.Enabled = true;
                        label_WASAPILatencyS.Enabled = true;
                        label_WASAPILatencyE.Enabled = true;
                        comboBox_DSBuffers.Enabled = true;
                        comboBox_DSLatencys.Enabled = true;
                        comboBox_WASAPILatencysS.Enabled = true;
                        comboBox_WASAPILatencysE.Enabled = true;
                        break;
                }

                switch (bool.Parse(Config.Entry["PlaybackATRAC"].Value))
                {
                    case true:
                        checkBox_EnableATRACPlayback.Checked = true;
                        break;
                    case false:
                        checkBox_EnableATRACPlayback.Checked = false;
                        break;
                }

                switch (bool.Parse(Config.Entry["DisablePreviewWarning"].Value))
                {
                    case true:
                        checkBox_DisablePreviewWarning.Checked = true;
                        break;
                    case false:
                        checkBox_DisablePreviewWarning.Checked = false;
                        break;
                }

                switch (bool.Parse(Config.Entry["SplashImage"].Value))
                {
                    case true:
                        checkBox_Splashimg.Checked = true;
                        textBox_Splashimg.Enabled = true;
                        button_Splashimg.Enabled = true;
                        if (!string.IsNullOrEmpty(Config.Entry["SplashImage_Path"].Value))
                        {
                            textBox_Splashimg.Text = Config.Entry["SplashImage_Path"].Value;
                        }
                        else
                        {
                            textBox_Splashimg.Text = string.Empty;
                        }
                        break;
                    case false:
                        checkBox_Splashimg.Checked = false;
                        textBox_Splashimg.Enabled = false;
                        button_Splashimg.Enabled = false;
                        break;
                }

                switch (bool.Parse(Config.Entry["Oldmode"].Value))
                {
                    case true:
                        checkBox_Oldmode.Checked = true;
                        break;
                    case false:
                        checkBox_Oldmode.Checked = false;
                        break;
                }

                switch (bool.Parse(Config.Entry["HideSplash"].Value))
                {
                    case true:
                        checkBox_Hidesplash.Checked = true;
                        break;
                    case false:
                        checkBox_Hidesplash.Checked = false;
                        break;
                }

                switch (bool.Parse(Config.Entry["FasterATRAC"].Value))
                {
                    case true:
                        checkBox_FasterATRAC.Checked = true;
                        break;
                    case false:
                        checkBox_FasterATRAC.Checked = false;
                        break;
                }

                switch (bool.Parse(Config.Entry["FixedConvert"].Value))
                {
                    case true:
                        checkBox_Fixconvert.Checked = true;
                        comboBox_Fixconvert.Enabled = true;
                        comboBox_Fixconvert.SelectedIndex = int.Parse(Config.Entry["ConvertType"].Value) switch
                        {
                            0 => 0,
                            1 => 1,
                            2 => 2,
                            3 => 3,
                            _ => 0,
                        };
                        break;
                    case false:
                        checkBox_Fixconvert.Checked = false;
                        comboBox_Fixconvert.Enabled = false;
                        break;
                }

                switch (bool.Parse(Config.Entry["ForceConvertWaveOnly"].Value))
                {
                    case true:
                        checkBox_ForceConvertWaveOnly.Checked = true;
                        break;
                    case false:
                        checkBox_ForceConvertWaveOnly.Checked = false;
                        break;
                }

                switch (bool.Parse(Config.Entry["ATRACEncodeSource"].Value))
                {
                    case true:
                        checkBox_ATRACEncodeSource.Checked = true;
                        break;
                    case false:
                        checkBox_ATRACEncodeSource.Checked = false;
                        break;
                }

                switch (bool.Parse(Config.Entry["Save_IsManual"].Value))
                {
                    case true:
                        {
                            radioButton_spc.Checked = true;
                            radioButton_nml.Checked = false;
                            label_IO_Path.Enabled = true;
                            button_Clear.Enabled = true;
                            button_Browse.Enabled = true;
                            textBox_Path.Enabled = true;
                            checkBox_Subfolder.Enabled = true;

                            if (Config.Entry["Save_Isfolder"].Value != "")
                            {
                                textBox_Path.Text = Config.Entry["Save_Isfolder"].Value;
                            }
                            else
                            {
                                textBox_Path.Text = null;
                            }

                            switch (bool.Parse(Config.Entry["Save_IsSubfolder"].Value))
                            {
                                case true:
                                    {
                                        label_IO_SubfolderSuffix.Enabled = true;
                                        textBox_suffix.Enabled = true;
                                        checkBox_Subfolder.Checked = true;

                                        if (Config.Entry["Save_Subfolder_Suffix"].Value != "")
                                        {
                                            textBox_suffix.Text = Config.Entry["Save_Subfolder_Suffix"].Value;
                                        }
                                        else
                                        {
                                            textBox_suffix.Text = null;
                                        }
                                        break;
                                    }
                                case false:
                                    {
                                        label_IO_SubfolderSuffix.Enabled = false;
                                        textBox_suffix.Text = null;
                                        textBox_suffix.Enabled = false;
                                        checkBox_Subfolder.Checked = false;
                                        break;
                                    }
                            }

                            break;
                        }
                    case false:
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
                            break;
                        }
                }

                switch (bool.Parse(Config.Entry["Save_NestFolderSource"].Value))
                {
                    case true:
                        {
                            checkBox_IO_SaveSourcesnest.Checked = true;
                            break;
                        }
                    case false:
                        {
                            checkBox_IO_SaveSourcesnest.Checked = false;
                            break;
                        }
                }

                switch (bool.Parse(Config.Entry["Save_DeleteHzSuffix"].Value))
                {
                    case true:
                        {
                            checkBox_SaveDeleteHzSuffix.Checked = true;
                            break;
                        }
                    case false:
                        {
                            checkBox_SaveDeleteHzSuffix.Checked = false;
                            break;
                        }
                }

                switch (bool.Parse(Config.Entry["ShowFolder"].Value))
                {
                    case true:
                        {
                            checkBox_ShowFolder.Checked = true;
                            break;
                        }
                    case false:
                        {
                            checkBox_ShowFolder.Checked = false;
                            break;
                        }
                }

                switch (uint.Parse(Config.Entry["LPCPlaybackMethod"].Value))
                {
                    case 0:
                        {
                            comboBox_LPCplayback.SelectedIndex = 0;
                            label_LPC_ASIODriver.Enabled = false;
                            comboBox_LPCASIODriver.Enabled = false;
                            break;
                        }
                    case 1:
                        {
                            comboBox_LPCplayback.SelectedIndex = 1;
                            label_LPC_ASIODriver.Enabled = false;
                            comboBox_LPCASIODriver.Enabled = false;
                            break;
                        }
                    case 2:
                        {
                            comboBox_LPCplayback.SelectedIndex = 2;
                            label_LPC_ASIODriver.Enabled = false;
                            comboBox_LPCASIODriver.Enabled = false;
                            break;
                        }
                    case 3:
                        {
                            comboBox_LPCplayback.SelectedIndex = 3;
                            label_LPC_ASIODriver.Enabled = true;
                            comboBox_LPCASIODriver.Enabled = true;
                            break;
                        }
                    default:
                        {
                            comboBox_LPCplayback.SelectedIndex = 0;
                            label_LPC_ASIODriver.Enabled = false;
                            comboBox_LPCASIODriver.Enabled = false;
                            break;
                        }
                }

                switch (string.IsNullOrWhiteSpace(Config.Entry["LPCUseASIODriver"].Value))
                {
                    case true:
                        {
                            label_LPC_ASIODriver.Enabled = false;
                            comboBox_LPCASIODriver.Enabled = false;
                            break;
                        }
                    case false:
                        {
                            label_LPC_ASIODriver.Enabled = false;
                            comboBox_LPCASIODriver.Enabled = false;
                            break;
                        }
                }

                switch (bool.Parse(Config.Entry["LPCMultipleStreamAlwaysWASAPIorASIO"].Value))
                {
                    case true:
                        {
                            checkBox_MultisoundDontDS.Checked = true;
                            break;
                        }
                    case false:
                        {
                            checkBox_MultisoundDontDS.Checked = false;
                            break;
                        }
                }

                switch (uint.Parse(Config.Entry["LPCMultipleStreamPlaybackMethod"].Value))
                {
                    case 0:
                        {
                            comboBox_LPCMultisourcePlaybackmode.SelectedIndex = 0;
                            label_LPC_MultiplesourcePlaybackmode.Enabled = true;
                            comboBox_LPCMultisourcePlaybackmode.Enabled = true;
                            break;
                        }
                    case 1:
                        {
                            comboBox_LPCMultisourcePlaybackmode.SelectedIndex = 1;
                            label_LPC_MultiplesourcePlaybackmode.Enabled = true;
                            comboBox_LPCMultisourcePlaybackmode.Enabled = true;
                            break;
                        }
                    case 2:
                        {
                            comboBox_LPCMultisourcePlaybackmode.SelectedIndex = 2;
                            label_LPC_MultiplesourcePlaybackmode.Enabled = true;
                            comboBox_LPCMultisourcePlaybackmode.Enabled = true;
                            break;
                        }
                    default:
                        {
                            comboBox_LPCMultisourcePlaybackmode.SelectedIndex = 0;
                            label_LPC_MultiplesourcePlaybackmode.Enabled = false;
                            comboBox_LPCMultisourcePlaybackmode.Enabled = false;
                            break;
                        }
                }

                comboBox_DSBuffers.SelectedIndex = int.Parse(Config.Entry["DirectSoundBuffers"].Value);
                comboBox_DSLatencys.SelectedIndex = int.Parse(Config.Entry["DirectSoundLatency"].Value);
                comboBox_WASAPILatencysS.SelectedIndex = int.Parse(Config.Entry["WASAPILatencyShared"].Value);
                comboBox_WASAPILatencysE.SelectedIndex = int.Parse(Config.Entry["WASAPILatencyExclusived"].Value);
                comboBox_PlaybackThreadCounts.SelectedIndex = int.Parse(Config.Entry["PlaybackThreadCount"].Value);

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
                Config.Entry["LPCUseASIODriver"].Value = comboBox_LPCASIODriver.Items[comboBox_LPCASIODriver.SelectedIndex].ToString();
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
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    break;
            }
        }

        private void ComboBox_LPCplayback_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_LPCplayback.SelectedIndex)
            {
                case 0:
                    label_LPC_ASIODriver.Enabled = false;
                    comboBox_LPCASIODriver.Enabled = false;
                    comboBox_LPCASIODriver.Items.Clear();
                    break;
                case 1:
                    label_LPC_ASIODriver.Enabled = false;
                    comboBox_LPCASIODriver.Enabled = false;
                    comboBox_LPCASIODriver.Items.Clear();
                    break;
                case 2:
                    label_LPC_ASIODriver.Enabled = false;
                    comboBox_LPCASIODriver.Enabled = false;
                    comboBox_LPCASIODriver.Items.Clear();
                    break;
                case 3:
                    MessageBox.Show(Localization.ASIOWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    label_LPC_ASIODriver.Enabled = true;
                    comboBox_LPCASIODriver.Enabled = true;
                    break;
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
                    break;
                case 1:
                    break;
                case 2:
                    MessageBox.Show(Localization.ASIOWarningCaption, Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                default:
                    break;
            }
        }

        private void tabPageAdvanced_Click(object sender, EventArgs e)
        {

        }

        private void checkBox_Smoothsamples_CheckedChanged(object sender, EventArgs e)
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
    }
}
