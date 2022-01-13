using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Common.Utils.OpenURI("https://github.com/AydinAdn/MediaToolkit");
            return;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Common.Utils.OpenURI("https://github.com/XyLe-GBP");
            return;
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Common.Utils.OpenURI("https://xyle-official.com");
            return;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            Close();
            return;
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);
                Bitmap canvas = new Bitmap(pictureBox2.Width, pictureBox2.Height);
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
