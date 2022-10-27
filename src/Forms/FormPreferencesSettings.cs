using ATRACTool_Reloaded.Localizable;
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
            Config.Load(xmlpath);
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
            if (radioButton_spc.Checked == true && textBox_Path.Text == "")
            {
                MessageBox.Show(this, "Path not specified.", Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (radioButton_spc.Checked == true && checkBox_Subfolder.Checked == true && textBox_suffix.Text == "")
            {
                MessageBox.Show(this, "suffix is not specified.", Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
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

            Close();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
