using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;
using ATRACTool_Reloaded.Localizable;

namespace ATRACTool_Reloaded
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                pictureBox2.ImageLocation = "https://avatars.githubusercontent.com/u/59692068?v=4";
            }
            else
            {
                pictureBox2.Image = Properties.Resources.IMG_1540;
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Common.Utils.OpenURI("https://github.com/AydinAdn/MediaToolkit");
            return;
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Common.Utils.OpenURI("https://github.com/XyLe-GBP");
            return;
        }

        private void LinkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Common.Utils.OpenURI("https://xyle-official.com");
            return;
        }

        private void LinkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Common.Utils.OpenURI("https://secure.xyle-official.com/payment/donate/");
            return;
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            Close();
            return;
        }

        private void PictureBox2_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);
                Bitmap canvas = new(pictureBox2.Width, pictureBox2.Height);
                Graphics g = Graphics.FromImage(canvas);
                GraphicsPath gp = new();
                gp.AddEllipse(g.VisibleClipBounds);
                Region rgn = new(gp);
                pictureBox2.Region = rgn;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Localization.UnExpectedCaption, ex.ToString()), Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
        }
    }
}
