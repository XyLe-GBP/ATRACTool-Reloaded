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
            comboBox_method.SelectedIndex = 0;
        }

        private void ComboBox_method_SelectedIndexChanged(object sender, EventArgs e)
        {
            Common.Generic.WTAmethod = comboBox_method.SelectedIndex switch
            {
                0 => 0,
                1 => 1,
                2 => 2,
                3 => 3,
                _ => 0,
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
