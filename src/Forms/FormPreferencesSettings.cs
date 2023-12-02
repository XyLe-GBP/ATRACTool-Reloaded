using ATRACTool_Reloaded.Localizable;
using Microsoft.VisualBasic;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormPreferencesSettings : Form
    {
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

                switch (bool.Parse(Config.Entry["Save_IsManual"].Value))
                {
                    case true:
                        {
                            radioButton_spc.Checked = true;
                            radioButton_nml.Checked = false;
                            label1.Enabled = true;
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
                                        label2.Enabled = true;
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
                                        label2.Enabled = false;
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
                            label1.Enabled = false;
                            label2.Enabled = false;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, string.Format("An error has occurred.\n{0}\nThe configuration file is incorrect.", ex), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void RadioButton_nml_CheckedChanged(object sender, EventArgs e)
        {
            label1.Enabled = false;
            label2.Enabled = false;
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
            label1.Enabled = true;
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
                label2.Enabled = false;
                textBox_suffix.Text = null;
                textBox_suffix.Enabled = false;
            }
            else
            {
                label2.Enabled = true;
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

            if (checkBox_ShowFolder.Checked != true)
            {
                Config.Entry["ShowFolder"].Value = "false";
            }
            else
            {
                Config.Entry["ShowFolder"].Value = "true";
            }

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
    }
}
