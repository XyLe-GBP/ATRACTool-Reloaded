namespace ATRACTool_Reloaded
{
    public partial class FormATWSelect : Form
    {
        public FormATWSelect()
        {
            InitializeComponent();
        }

        private void FormATWSelect_Load(object sender, EventArgs e)
        {
            comboBox_fmt.SelectedIndex = 0;
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            Common.Generic.WTAFlag = (sbyte)comboBox_fmt.SelectedIndex;
            Close();
        }
    }
}
