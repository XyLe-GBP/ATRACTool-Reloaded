using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormAtWSelectTarget : Form
    {
        public FormAtWSelectTarget()
        {
            InitializeComponent();
        }

        private void FormAtWSelectTarget_Load(object sender, EventArgs e)
        {
            Common.Generic.WTAmethod = 0;
            comboBox_method.SelectedIndex = 5;
        }

        private void ComboBox_method_SelectedIndexChanged(object sender, EventArgs e)
        {
            Common.Generic.WTAmethod = comboBox_method.SelectedIndex switch
            {
                0 => Constants.WTAType.Hz8000,
                1 => Constants.WTAType.Hz12000,
                2 => Constants.WTAType.Hz16000,
                3 => Constants.WTAType.Hz24000,
                4 => Constants.WTAType.Hz32000,
                5 => Constants.WTAType.Hz44100,
                6 => Constants.WTAType.Hz48000,
                _ => Constants.WTAType.Hz44100,
            };
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
