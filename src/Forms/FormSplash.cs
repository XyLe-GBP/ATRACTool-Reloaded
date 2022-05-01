namespace ATRACTool_Reloaded
{
    public partial class FormSplash : Form
    {
        public FormSplash()
        {
            InitializeComponent();
        }

        private void FormSplash_Load(object sender, EventArgs e)
        {
            BackgroundImage = Properties.Resources.SIE;
        }

        public string ProgressMsg
        {
            set
            {
                label_log.Text = value;
            }
        }
    }
}
