using ATRACTool_Reloaded.Localizable;
using static ATRACTool_Reloaded.Common;
using System.ComponentModel;

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
            Config.Load(Common.xmlpath);
            Screen screen = null!;
            if (Screen.PrimaryScreen is not null)
            {
                screen = Screen.PrimaryScreen;
            }
            else
            {
                MessageBox.Show(this, "PrimaryScreen is null.", Localization.MSGBoxWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                label_log.BackColor = Color.Transparent;
                if (bool.Parse(Config.Entry["SplashImage"].Value))
                {
                    Bitmap bimg = new(Properties.Resources.Splash);
                    Bitmap cimg = new(Config.Entry["SplashImage_Path"].Value);
                    using Graphics g = Graphics.FromImage(cimg);
                    g.DrawImage(bimg, 0, 0, cimg.Width, cimg.Height);
                    Width = cimg.Width;
                    Height = cimg.Height;
                    Left = (screen.Bounds.Width - Width) / 2;
                    Top = (screen.Bounds.Height - Height) / 2;
                    BackgroundImage = cimg;
                }
                else
                {
                    Bitmap bimg = new(Properties.Resources.Splash);
                    Bitmap cimg = new(Properties.Resources.Splash_SIE_Default);
                    using Graphics g = Graphics.FromImage(cimg);
                    g.DrawImage(bimg, 0, 0, cimg.Width, cimg.Height);
                    Width = cimg.Width;
                    Height = cimg.Height;
                    Left = (screen.Bounds.Width - Width) / 2;
                    Top = (screen.Bounds.Height - Height) / 2;
                    BackgroundImage = cimg;
                }
            }
            catch (Exception ex)
            {
                Generic.GlobalException = ex;
                Bitmap bimg = new(Properties.Resources.Splash);
                Bitmap cimg = new(Properties.Resources.Splash_SIE_Default);
                using Graphics g = Graphics.FromImage(cimg);
                g.DrawImage(bimg, 0, 0, cimg.Width, cimg.Height);
                Width = cimg.Width;
                Height = cimg.Height;
                Left = (screen.Bounds.Width - Width) / 2;
                Top = (screen.Bounds.Height - Height) / 2;
                BackgroundImage = cimg;
            }


            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 25;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ProgressMsg
        {
            set
            {
                label_log.Text = value;
            }
        }
    }
}
