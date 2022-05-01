namespace ATRACTool_Reloaded
{
    public partial class FormUpdateApplicationType : Form
    {
        public FormUpdateApplicationType()
        {
            InitializeComponent();
        }

        private void FormUpdateApplicationType_Load(object sender, EventArgs e)
        {
            comboBox_type.SelectedIndex = 0;
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            if (comboBox_type.SelectedIndex != 1)
            {
                Common.Generic.ApplicationPortable = false;
            }
            else
            {
                Common.Generic.ApplicationPortable = true;
            }
            Close();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
