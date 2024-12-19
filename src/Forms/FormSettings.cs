using ATRACTool_Reloaded.Localizable;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormSettings : Form
    {
        private string bitrateAT3 = null!;
        private string wholeloopAT3 = null!;
        private string looppointAT3 = null!;
        private string loopstartAT3 = null!;
        private string loopendAT3 = null!;
        private string looptimeAT3 = null!;
        private string looptimesAT3 = null!;
        private string paramAT3 = "at3tool -e";

        private string bitrateAT9 = null!;
        private string samplingAT9 = null!;
        private string wholeloopAT9 = null!;
        private string looppointAT9 = null!;
        private string loopstartAT9 = null!;
        private string loopendAT9 = null!;
        private string looptimeAT9 = null!;
        private string looptimesAT9 = null!;
        private string looplistAT9 = null!;
        private string looplistfileAT9 = null!;
        private string enctypeAT9 = null!;
        private string dualencAT9 = null!;
        private string supframeAT9 = null!;
        private string nbandAT9 = null!;
        private string isbandAT9 = null!;
        private string bex = null!;
        private string wband = null!;
        private string LFE = null!;
        private string paramAT9 = "at9tool -e";

        private string walkmanfmt = null!;
        private string title = null!;
        private string sorttitle = null!;
        private string sortsubtitle = null!;
        private string subtitle = null!;
        private string artist = null!;
        private string sortartist = null!;
        private string album = null!;
        private string sortalbum = null!;
        private string albumartist = null!;
        private string sortalbumartist = null!;
        private string genre = null!;
        private string composer = null!;
        private string lyricist = null!;
        private string lyrics = null!;
        private string lyricsmode = null!;
        private string linernotes = null!;
        private string linernotesmode = null!;
        private string jacketmode = null!;
        private string jacket = null!;
        private string tracknumber = null!;
        private string totaltracks = null!;
        private string duration = null!;
        private string milliseconds = null!;
        private string bitrates = null!;
        private string release = null!;
        private string import = null!;

        private string paramWalkman = "traconv";

        private bool Setloopingpoint = false;

        public FormSettings(bool IsSetLoopingPoint)
        {
            InitializeComponent();

            if (IsSetLoopingPoint)
            {
                Setloopingpoint = true;
            }
            else
            {
                Setloopingpoint = false;
            }
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            Common.Config.Load(Common.xmlpath);

            comboBox_at9_enctype.SelectedIndex = 5;
            comboBox_at9_startband.SelectedIndex = 0;
            comboBox_at9_useband.SelectedIndex = 0;
            comboBox_DecodeFormats.SelectedIndex = 0;
            comboBox_OutputFormats.SelectedIndex = 1;
            comboBox_Lyricsmode.SelectedIndex = 3;
            comboBox_Linermode.SelectedIndex = 3;
            comboBox_Jacketmode.SelectedIndex = 3;
            radioButton_each.Checked = true;

            if (Generic.lpcreatev2 != false)
            {
                checkBox_lpcreate.Enabled = false;
            }
            else
            {
                checkBox_lpcreate.Enabled = true;
            }

            

            switch (int.Parse(Config.Entry["ATRAC3_Console"].Value))
            {
                case 0:
                    radioButton_PSP.Checked = true;
                    comboBox_at3_encmethod.Items.Clear();
                    comboBox_at3_encmethod.Items.Add("32kbps, mono");
                    comboBox_at3_encmethod.Items.Add("48kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("52kbps, mono");
                    comboBox_at3_encmethod.Items.Add("64kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("66kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("96kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("105kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("128kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("132kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("160kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("192kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("256kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("320kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("352kbps, stereo");
                    switch (int.Parse(Config.Entry["ATRAC3_Bitrate"].Value))
                    {
                        case 0: // 32k
                            comboBox_at3_encmethod.SelectedIndex = 0;
                            bitrateAT3 = " -br 32";
                            break;
                        case 1: // 48k
                            comboBox_at3_encmethod.SelectedIndex = 1;
                            bitrateAT3 = " -br 48";
                            break;
                        case 2: // 52k
                            comboBox_at3_encmethod.SelectedIndex = 2;
                            bitrateAT3 = " -br 52";
                            break;
                        case 3: // 64k mono / stereo
                            comboBox_at3_encmethod.SelectedIndex = 3;
                            bitrateAT3 = " -br 64";
                            break;
                        case 4: // 66k mono / stereo
                            comboBox_at3_encmethod.SelectedIndex = 4;
                            bitrateAT3 = " -br 66";
                            break;
                        case 5: // 96k mono / stereo
                            comboBox_at3_encmethod.SelectedIndex = 5;
                            bitrateAT3 = " -br 96";
                            break;
                        case 6: // 105k stereo
                            comboBox_at3_encmethod.SelectedIndex = 6;
                            bitrateAT3 = " -br 105";
                            break;
                        case 7: // 128k mono / stereo
                            comboBox_at3_encmethod.SelectedIndex = 7;
                            bitrateAT3 = " -br 128";
                            break;
                        case 8: // 132k
                            comboBox_at3_encmethod.SelectedIndex = 8;
                            bitrateAT3 = " -br 132";
                            break;
                        case 9: // 160k
                            comboBox_at3_encmethod.SelectedIndex = 9;
                            bitrateAT3 = " -br 160";
                            break;
                        case 10: // 192k
                            comboBox_at3_encmethod.SelectedIndex = 10;
                            bitrateAT3 = " -br 192";
                            break;
                        case 11: // 256k
                            comboBox_at3_encmethod.SelectedIndex = 11;
                            bitrateAT3 = " -br 256";
                            break;
                        case 12: // 320k
                            comboBox_at3_encmethod.SelectedIndex = 12;
                            bitrateAT3 = " -br 320";
                            break;
                        case 13: // 352k
                            comboBox_at3_encmethod.SelectedIndex = 13;
                            bitrateAT3 = " -br 352";
                            break;
                        default:
                            comboBox_at3_encmethod.SelectedIndex = 7;
                            bitrateAT3 = " -br 128";
                            break;
                    }
                    break;
                case 1:
                    radioButton_PS3.Checked = true;
                    comboBox_at3_encmethod.Items.Clear();
                    comboBox_at3_encmethod.Items.Add("32kbps, mono");
                    comboBox_at3_encmethod.Items.Add("48kbps, mono");
                    comboBox_at3_encmethod.Items.Add("57kbps, mono");
                    comboBox_at3_encmethod.Items.Add("64kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("72kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("96kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("114kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("128kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("144kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("160kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("192kbps, stereo / 6ch");
                    comboBox_at3_encmethod.Items.Add("256kbps, stereo / 6ch");
                    comboBox_at3_encmethod.Items.Add("320kbps, stereo / 6ch");
                    comboBox_at3_encmethod.Items.Add("384kbps, 6ch / 8ch");
                    comboBox_at3_encmethod.Items.Add("512kbps, 6ch");
                    comboBox_at3_encmethod.Items.Add("768kbps, 8ch");
                    switch (int.Parse(Config.Entry["ATRAC3_Bitrate"].Value))
                    {
                        case 0: // 32k
                            comboBox_at3_encmethod.SelectedIndex = 0;
                            bitrateAT3 = " -br 32";
                            break;
                        case 1: // 48k
                            comboBox_at3_encmethod.SelectedIndex = 1;
                            bitrateAT3 = " -br 48";
                            break;
                        case 2: // 57k
                            comboBox_at3_encmethod.SelectedIndex = 2;
                            bitrateAT3 = " -br 57";
                            break;
                        case 3: // 64k mono / stereo
                            comboBox_at3_encmethod.SelectedIndex = 3;
                            bitrateAT3 = " -br 64";
                            break;
                        case 4: // 72k mono / stereo
                            comboBox_at3_encmethod.SelectedIndex = 4;
                            bitrateAT3 = " -br 72";
                            break;
                        case 5: // 96k mono / stereo
                            comboBox_at3_encmethod.SelectedIndex = 5;
                            bitrateAT3 = " -br 96";
                            break;
                        case 6: // 114k stereo
                            comboBox_at3_encmethod.SelectedIndex = 6;
                            bitrateAT3 = " -br 114";
                            break;
                        case 7: // 128k mono / stereo
                            comboBox_at3_encmethod.SelectedIndex = 7;
                            bitrateAT3 = " -br 128";
                            break;
                        case 8: // 144k
                            comboBox_at3_encmethod.SelectedIndex = 8;
                            bitrateAT3 = " -br 144";
                            break;
                        case 9: // 160k
                            comboBox_at3_encmethod.SelectedIndex = 9;
                            bitrateAT3 = " -br 160";
                            break;
                        case 10: // 192k
                            comboBox_at3_encmethod.SelectedIndex = 10;
                            bitrateAT3 = " -br 192";
                            break;
                        case 11: // 256k
                            comboBox_at3_encmethod.SelectedIndex = 11;
                            bitrateAT3 = " -br 256";
                            break;
                        case 12: // 320k
                            comboBox_at3_encmethod.SelectedIndex = 12;
                            bitrateAT3 = " -br 320";
                            break;
                        case 13: // 384k
                            comboBox_at3_encmethod.SelectedIndex = 13;
                            bitrateAT3 = " -br 384";
                            break;
                        case 14: // 512k
                            comboBox_at3_encmethod.SelectedIndex = 14;
                            bitrateAT3 = " -br 512";
                            break;
                        case 15: // 768k
                            comboBox_at3_encmethod.SelectedIndex = 15;
                            bitrateAT3 = " -br 768";
                            break;
                        default:
                            comboBox_at3_encmethod.SelectedIndex = 10;
                            bitrateAT3 = " -br 192";
                            break;
                    }
                    break;
                default:
                    radioButton_PSP.Checked = true;
                    comboBox_at3_encmethod.Items.Clear();
                    comboBox_at3_encmethod.Items.Add("32kbps, mono");
                    comboBox_at3_encmethod.Items.Add("48kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("52kbps, mono");
                    comboBox_at3_encmethod.Items.Add("64kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("66kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("96kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("105kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("128kbps, mono / stereo");
                    comboBox_at3_encmethod.Items.Add("132kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("160kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("192kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("256kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("320kbps, stereo");
                    comboBox_at3_encmethod.Items.Add("352kbps, stereo");
                    comboBox_at3_encmethod.SelectedIndex = 7;
                    bitrateAT3 = " -br 128";
                    break;
            }

            switch (int.Parse(Config.Entry["ATRAC9_Console"].Value))
            {
                case 0:
                    radioButton_PSV.Checked = true;
                    comboBox_at9_bitrate.Items.Clear();
                    comboBox_at9_bitrate.Items.Add("48kbps, mono");
                    comboBox_at9_bitrate.Items.Add("60kbps, mono");
                    comboBox_at9_bitrate.Items.Add("72kbps, mono");
                    comboBox_at9_bitrate.Items.Add("84kbps, mono");
                    comboBox_at9_bitrate.Items.Add("96kbps, mono");
                    comboBox_at9_bitrate.Items.Add("120kbps, mono");
                    comboBox_at9_bitrate.Items.Add("144kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("168kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("192kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("256kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("320kbps, stereo");
                    switch (int.Parse(Config.Entry["ATRAC9_Bitrate"].Value))
                    {
                        case 0:
                            comboBox_at9_bitrate.SelectedIndex = 0;
                            bitrateAT9 = " -br 48";
                            break;
                        case 1:
                            comboBox_at9_bitrate.SelectedIndex = 1;
                            bitrateAT9 = " -br 60";
                            break;
                        case 2:
                            comboBox_at9_bitrate.SelectedIndex = 2;
                            bitrateAT9 = " -br 72";
                            break;
                        case 3:
                            comboBox_at9_bitrate.SelectedIndex = 3;
                            bitrateAT9 = " -br 84";
                            break;
                        case 4:
                            comboBox_at9_bitrate.SelectedIndex = 4;
                            bitrateAT9 = " -br 96";
                            break;
                        case 5:
                            comboBox_at9_bitrate.SelectedIndex = 5;
                            bitrateAT9 = " -br 120";
                            break;
                        case 6:
                            comboBox_at9_bitrate.SelectedIndex = 6;
                            bitrateAT9 = " -br 144";
                            break;
                        case 7:
                            comboBox_at9_bitrate.SelectedIndex = 7;
                            bitrateAT9 = " -br 168";
                            break;
                        case 8:
                            comboBox_at9_bitrate.SelectedIndex = 8;
                            bitrateAT9 = " -br 192";
                            break;
                        case 9:
                            comboBox_at9_bitrate.SelectedIndex = 9;
                            bitrateAT9 = " -br 256";
                            break;
                        case 10:
                            comboBox_at9_bitrate.SelectedIndex = 10;
                            bitrateAT9 = " -br 320";
                            break;
                        default:
                            comboBox_at9_bitrate.SelectedIndex = 7;
                            bitrateAT9 = " -br 168";
                            break;
                    }
                    break;
                case 1:
                    radioButton_PS4.Checked = true;
                    comboBox_at9_bitrate.Items.Clear();
                    comboBox_at9_bitrate.Items.Add("48kbps, mono");
                    comboBox_at9_bitrate.Items.Add("60kbps, mono");
                    comboBox_at9_bitrate.Items.Add("72kbps, mono");
                    comboBox_at9_bitrate.Items.Add("84kbps, mono");
                    comboBox_at9_bitrate.Items.Add("96kbps, mono");
                    comboBox_at9_bitrate.Items.Add("120kbps, mono");
                    comboBox_at9_bitrate.Items.Add("144kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("168kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("192kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("240kbps, 4ch");
                    comboBox_at9_bitrate.Items.Add("256kbps, 4ch");
                    comboBox_at9_bitrate.Items.Add("300kbps, 5.1ch");
                    comboBox_at9_bitrate.Items.Add("320kbps, 5.1ch");
                    comboBox_at9_bitrate.Items.Add("384kbps, 5.1ch");
                    comboBox_at9_bitrate.Items.Add("420kbps, 7.1ch");
                    switch (int.Parse(Config.Entry["ATRAC9_Bitrate"].Value))
                    {
                        case 0:
                            comboBox_at9_bitrate.SelectedIndex = 0;
                            bitrateAT9 = " -br 48";
                            break;
                        case 1:
                            comboBox_at9_bitrate.SelectedIndex = 1;
                            bitrateAT9 = " -br 60";
                            break;
                        case 2:
                            comboBox_at9_bitrate.SelectedIndex = 2;
                            bitrateAT9 = " -br 72";
                            break;
                        case 3:
                            comboBox_at9_bitrate.SelectedIndex = 3;
                            bitrateAT9 = " -br 84";
                            break;
                        case 4:
                            comboBox_at9_bitrate.SelectedIndex = 4;
                            bitrateAT9 = " -br 96";
                            break;
                        case 5:
                            comboBox_at9_bitrate.SelectedIndex = 5;
                            bitrateAT9 = " -br 120";
                            break;
                        case 6:
                            comboBox_at9_bitrate.SelectedIndex = 6;
                            bitrateAT9 = " -br 144";
                            break;
                        case 7:
                            comboBox_at9_bitrate.SelectedIndex = 7;
                            bitrateAT9 = " -br 168";
                            break;
                        case 8:
                            comboBox_at9_bitrate.SelectedIndex = 8;
                            bitrateAT9 = " -br 192";
                            break;
                        case 9:
                            comboBox_at9_bitrate.SelectedIndex = 9;
                            bitrateAT9 = " -br 240";
                            break;
                        case 10:
                            comboBox_at9_bitrate.SelectedIndex = 10;
                            bitrateAT9 = " -br 256";
                            break;
                        case 11:
                            comboBox_at9_bitrate.SelectedIndex = 11;
                            bitrateAT9 = " -br 300";
                            break;
                        case 12:
                            comboBox_at9_bitrate.SelectedIndex = 12;
                            bitrateAT9 = " -br 320";
                            break;
                        case 13:
                            comboBox_at9_bitrate.SelectedIndex = 13;
                            bitrateAT9 = " -br 384";
                            break;
                        case 14:
                            comboBox_at9_bitrate.SelectedIndex = 14;
                            bitrateAT9 = " -br 420";
                            break;
                        default:
                            comboBox_at9_bitrate.SelectedIndex = 7;
                            bitrateAT9 = " -br 168";
                            break;
                    }
                    break;
                default:
                    radioButton_PSV.Checked = true;
                    comboBox_at9_bitrate.Items.Clear();
                    comboBox_at9_bitrate.Items.Add("48kbps, mono");
                    comboBox_at9_bitrate.Items.Add("60kbps, mono");
                    comboBox_at9_bitrate.Items.Add("72kbps, mono");
                    comboBox_at9_bitrate.Items.Add("84kbps, mono");
                    comboBox_at9_bitrate.Items.Add("96kbps, mono");
                    comboBox_at9_bitrate.Items.Add("120kbps, mono");
                    comboBox_at9_bitrate.Items.Add("144kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("168kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("192kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("256kbps, stereo");
                    comboBox_at9_bitrate.Items.Add("320kbps, stereo");
                    comboBox_at9_bitrate.SelectedIndex = 7;
                    bitrateAT9 = " -br 168";
                    break;
            }

            switch (int.Parse(Config.Entry["ATRAC9_Sampling"].Value))
            {
                case 0:
                    comboBox_at9_sampling.SelectedIndex = 0;
                    samplingAT9 = " -fs 12000";
                    break;
                case 1:
                    comboBox_at9_sampling.SelectedIndex = 1;
                    samplingAT9 = " -fs 24000";
                    break;
                case 2:
                    comboBox_at9_sampling.SelectedIndex = 2;
                    samplingAT9 = " -fs 48000";
                    break;
                default:
                    comboBox_at9_sampling.SelectedIndex = 2;
                    samplingAT9 = " -fs 48000";
                    break;
            }


            switch (bool.Parse(Config.Entry["ATRAC3_LoopPoint"].Value))
            {
                case true:
                    {
                        wholeloopAT3 = "";
                        checkBox_at3_loopsound.Enabled = false;

                        checkBox_at3_looppoint.Checked = true;
                        looppointAT3 = " -loop";
                        label_at3_loopstart.Enabled = true;
                        label_at3_loopend.Enabled = true;
                        label_at3_samples.Enabled = true;
                        textBox_at3_loopstart.Enabled = true;
                        textBox_at3_loopend.Enabled = true;
                        if (Common.Config.Entry["ATRAC3_LoopStart_Samples"].Value != "")
                        {
                            loopstartAT3 = Common.Config.Entry["ATRAC3_LoopStart_Samples"].Value;
                            textBox_at3_loopstart.Text = loopstartAT3;
                        }
                        if (Common.Config.Entry["ATRAC3_LoopEnd_Samples"].Value != "")
                        {
                            loopendAT3 = Common.Config.Entry["ATRAC3_LoopEnd_Samples"].Value;
                            textBox_at3_loopend.Text = loopendAT3;
                        }
                        break;
                    }
                case false:
                    {
                        checkBox_at3_loopsound.Enabled = true;

                        looppointAT3 = "";
                        loopstartAT3 = "";
                        loopendAT3 = "";
                        checkBox_at3_looppoint.Checked = false;
                        label_at3_loopstart.Enabled = false;
                        label_at3_loopend.Enabled = false;
                        label_at3_samples.Enabled = false;
                        textBox_at3_loopstart.Enabled = false;
                        textBox_at3_loopstart.Text = null;
                        textBox_at3_loopend.Enabled = false;
                        textBox_at3_loopend.Text = null;
                        break;
                    }
            }

            switch (bool.Parse(Common.Config.Entry["ATRAC9_LoopPoint"].Value))
            {
                case true:
                    {
                        checkBox_at9_loopsound.Enabled = false;
                        checkBox_at9_looppoint.Checked = true;
                        looppointAT9 = " -loop";
                        label_at9_loopstart.Enabled = true;
                        label_at9_loopend.Enabled = true;
                        label_at9_samples.Enabled = true;
                        textBox_at9_loopstart.Enabled = true;
                        textBox_at9_loopend.Enabled = true;
                        if (Common.Config.Entry["ATRAC9_LoopStart_Samples"].Value != "")
                        {
                            loopstartAT9 = Common.Config.Entry["ATRAC9_LoopStart_Samples"].Value;
                            textBox_at9_loopstart.Text = loopstartAT9;
                        }
                        if (Common.Config.Entry["ATRAC9_LoopEnd_Samples"].Value != "")
                        {
                            loopendAT9 = Common.Config.Entry["ATRAC9_LoopEnd_Samples"].Value;
                            textBox_at9_loopend.Text = loopendAT9;
                        }
                        break;
                    }
                case false:
                    {
                        looppointAT9 = "";
                        loopstartAT9 = "";
                        loopendAT9 = "";
                        checkBox_at9_looppoint.Checked = false;
                        label_at9_loopstart.Enabled = false;
                        label_at9_loopend.Enabled = false;
                        label_at9_samples.Enabled = false;
                        textBox_at9_loopstart.Enabled = false;
                        textBox_at9_loopstart.Text = null;
                        textBox_at9_loopend.Enabled = false;
                        textBox_at9_loopend.Text = null;
                        checkBox_at9_loopsound.Enabled = true;
                        break;
                    }
            }

            switch (bool.Parse(Common.Config.Entry["ATRAC3_LoopSound"].Value))
            {
                case true:
                    {
                        wholeloopAT3 = " -wholeloop";
                        checkBox_at3_loopsound.Checked = true;
                        checkBox_at3_looppoint.Enabled = false;
                        break;
                    }
                case false:
                    {
                        wholeloopAT3 = "";
                        checkBox_at3_loopsound.Checked = false;
                        checkBox_at3_looppoint.Enabled = true;
                        break;
                    }
            }

            switch (bool.Parse(Common.Config.Entry["ATRAC9_LoopSound"].Value))
            {
                case true:
                    {
                        wholeloopAT9 = " -wholeloop";
                        checkBox_at9_loopsound.Checked = true;
                        checkBox_at9_looppoint.Enabled = false;
                        break;
                    }
                case false:
                    {
                        wholeloopAT9 = "";
                        checkBox_at9_loopsound.Checked = false;
                        checkBox_at9_looppoint.Enabled = true;
                        break;
                    }
            }

            switch (bool.Parse(Common.Config.Entry["ATRAC3_LoopTime"].Value))
            {
                case true:
                    {
                        if (Common.Config.Entry["ATRAC3_LoopTimes"].Value != "")
                        {
                            checkBox_at3_looptimes.Checked = true;
                            looptimeAT3 = " -repeat";
                            label_at3_nol.Enabled = true;
                            label_at3_times.Enabled = true;
                            textBox_at3_looptimes.Enabled = true;
                            textBox_at3_looptimes.Text = Common.Config.Entry["ATRAC3_LoopTimes"].Value;
                        }
                        else
                        {
                            checkBox_at3_looptimes.Checked = true;
                            looptimeAT3 = " -repeat";
                            label_at3_nol.Enabled = true;
                            label_at3_times.Enabled = true;
                            textBox_at3_looptimes.Enabled = true;
                            textBox_at3_looptimes.Text = Common.Config.Entry["ATRAC3_LoopTimes"].Value;
                        }
                        break;
                    }
                case false:
                    {
                        looptimeAT3 = "";
                        looptimesAT3 = "";
                        checkBox_at3_looptimes.Checked = false;
                        label_at3_nol.Enabled = false;
                        label_at3_times.Enabled = false;
                        textBox_at3_looptimes.Enabled = false;
                        textBox_at3_looptimes.Text = null;
                        break;
                    }
            }

            switch (bool.Parse(Common.Config.Entry["ATRAC9_LoopTime"].Value))
            {
                case true:
                    {
                        if (Common.Config.Entry["ATRAC9_LoopTimes"].Value != "")
                        {
                            checkBox_at9_looptimes.Checked = true;
                            looptimeAT9 = " -repeat";
                            label_at9_nol.Enabled = true;
                            label_at9_times.Enabled = true;
                            textBox_at9_looptimes.Enabled = true;
                            textBox_at9_looptimes.Text = Common.Config.Entry["ATRAC9_LoopTimes"].Value;
                        }
                        else
                        {
                            checkBox_at9_looptimes.Checked = true;
                            looptimeAT9 = " -repeat";
                            label_at9_nol.Enabled = true;
                            label_at9_times.Enabled = true;
                            textBox_at9_looptimes.Enabled = true;
                            textBox_at9_looptimes.Text = Common.Config.Entry["ATRAC9_LoopTimes"].Value;
                        }
                        break;
                    }
                case false:
                    {
                        looptimeAT9 = "";
                        looptimesAT9 = "";
                        checkBox_at9_looptimes.Checked = false;
                        label_at9_nol.Enabled = false;
                        label_at9_times.Enabled = false;
                        textBox_at9_looptimes.Enabled = false;
                        textBox_at9_looptimes.Text = null;
                        break;
                    }
            }

            switch (bool.Parse(Common.Config.Entry["ATRAC9_LoopList"].Value))
            {
                case true:
                    {
                        checkBox_at9_looplist.Checked = true;
                        looplistAT9 = " -looplist";
                        textBox_at9_looplist.Enabled = true;
                        button_at9_looplist.Enabled = true;
                        if (Common.Config.Entry["ATRAC9_LoopListFile"].Value != "")
                        {
                            textBox_at9_looplist.Text = Common.Config.Entry["ATRAC9_LoopListFile"].Value;
                        }
                        else
                        {
                            textBox_at9_looplist.Text = "";
                        }
                        break;
                    }
                case false:
                    {
                        checkBox_at9_looplist.Checked = false;
                        looplistAT9 = "";
                        textBox_at9_looplist.Enabled = false;
                        textBox_at9_looplist.Text = null;
                        button_at9_looplist.Enabled = false;
                        break;
                    }
            }

            switch (bool.Parse(Common.Config.Entry["ATRAC9_Advanced"].Value))
            {
                case true:
                    {
                        switch (int.Parse(Common.Config.Entry["ATRAC9_Console"].Value))
                        {
                            case 0:
                                {
                                    checkBox_bex.Enabled = false;
                                    checkBox_wband.Enabled = false;
                                    checkBox_LFE.Enabled = false;
                                    break;
                                }
                            case 1:
                                {
                                    checkBox_bex.Enabled = true;
                                    checkBox_wband.Enabled = true;
                                    checkBox_LFE.Enabled = true;

                                    switch (bool.Parse(Common.Config.Entry["ATRAC9_BandExtension"].Value))
                                    {
                                        case true:
                                            {
                                                checkBox_bex.Checked = true;
                                                bex = " -bex";
                                                break;
                                            }
                                        case false:
                                            {
                                                checkBox_bex.Checked = false;
                                                bex = "";
                                                break;
                                            }
                                    }

                                    switch (bool.Parse(Common.Config.Entry["ATRAC9_WideBand"].Value))
                                    {
                                        case true:
                                            {
                                                checkBox_wband.Checked = true;
                                                wband = " -wband";
                                                break;
                                            }
                                        case false:
                                            {
                                                checkBox_wband.Checked = false;
                                                wband = "";
                                                break;
                                            }
                                    }

                                    switch (bool.Parse(Common.Config.Entry["ATRAC9_LFE_SuperLowCut"].Value))
                                    {
                                        case true:
                                            {
                                                checkBox_LFE.Checked = true;
                                                LFE = " -slc";
                                                break;
                                            }
                                        case false:
                                            {
                                                checkBox_LFE.Checked = false;
                                                LFE = "";
                                                break;
                                            }
                                    }
                                    break;
                                }
                            default:
                                {
                                    checkBox_bex.Enabled = false;
                                    checkBox_wband.Enabled = false;
                                    checkBox_LFE.Enabled = false;
                                    break;
                                }
                        }
                        checkBox_at9_advanced.Checked = true;
                        checkBox_at9_enctype.Enabled = true;
                        checkBox_at9_advband.Enabled = true;
                        checkBox_at9_dualenc.Enabled = true;
                        checkBox_at9_supframe.Enabled = true;

                        switch (bool.Parse(Common.Config.Entry["ATRAC9_EncodeType"].Value))
                        {
                            case true:
                                {
                                    checkBox_at9_enctype.Checked = true;
                                    label_at9_enctype.Enabled = true;
                                    comboBox_at9_enctype.Enabled = true;
                                    break;
                                }
                            case false:
                                {
                                    checkBox_at9_enctype.Checked = false;
                                    label_at9_enctype.Enabled = false;
                                    comboBox_at9_enctype.Enabled = false;
                                    break;
                                }
                        }

                        switch (int.Parse(Common.Config.Entry["ATRAC9_EncodeTypeIndex"].Value))
                        {
                            case 0:
                                comboBox_at9_enctype.SelectedIndex = 0;
                                enctypeAT9 = " -gradmode 0";
                                break;
                            case 1:
                                comboBox_at9_enctype.SelectedIndex = 1;
                                enctypeAT9 = " -gradmode 1";
                                break;
                            case 2:
                                comboBox_at9_enctype.SelectedIndex = 2;
                                enctypeAT9 = " -gradmode 2";
                                break;
                            case 3:
                                comboBox_at9_enctype.SelectedIndex = 3;
                                enctypeAT9 = " -gradmode 3";
                                break;
                            case 4:
                                comboBox_at9_enctype.SelectedIndex = 4;
                                enctypeAT9 = " -gradmode 4";
                                break;
                            case 5:
                                comboBox_at9_enctype.SelectedIndex = 5;
                                enctypeAT9 = "";
                                break;
                            default:
                                comboBox_at9_enctype.SelectedIndex = 5;
                                enctypeAT9 = "";
                                break;
                        }

                        switch (bool.Parse(Common.Config.Entry["ATRAC9_DualEncode"].Value))
                        {
                            case true:
                                {
                                    checkBox_at9_dualenc.Checked = true;
                                    dualencAT9 = " -dual";
                                    break;
                                }
                            case false:
                                {
                                    checkBox_at9_dualenc.Checked = false;
                                    dualencAT9 = "";
                                    break;
                                }
                        }

                        switch (bool.Parse(Common.Config.Entry["ATRAC9_SuperFrameEncode"].Value))
                        {
                            case true:
                                {
                                    checkBox_at9_supframe.Checked = true;
                                    supframeAT9 = " -supframeon";
                                    break;
                                }
                            case false:
                                {
                                    checkBox_at9_supframe.Checked = false;
                                    supframeAT9 = " -supframeoff";
                                    break;
                                }
                        }

                        switch (bool.Parse(Common.Config.Entry["ATRAC9_AdvancedBand"].Value))
                        {
                            case true:
                                {
                                    checkBox_at9_advband.Checked = true;
                                    label_at9_useband.Enabled = true;
                                    comboBox_at9_useband.Enabled = true;
                                    label_at9_startband.Enabled = true;
                                    comboBox_at9_startband.Enabled = true;
                                    switch (int.Parse(Common.Config.Entry["ATRAC9_NbandsIndex"].Value))
                                    {
                                        case 0:
                                            comboBox_at9_useband.SelectedIndex = 0;
                                            nbandAT9 = " -nbands 3";
                                            break;
                                        case 1:
                                            comboBox_at9_useband.SelectedIndex = 1;
                                            nbandAT9 = " -nbands 4";
                                            break;
                                        case 2:
                                            comboBox_at9_useband.SelectedIndex = 2;
                                            nbandAT9 = " -nbands 5";
                                            break;
                                        case 3:
                                            comboBox_at9_useband.SelectedIndex = 3;
                                            nbandAT9 = " -nbands 6";
                                            break;
                                        case 4:
                                            comboBox_at9_useband.SelectedIndex = 4;
                                            nbandAT9 = " -nbands 7";
                                            break;
                                        case 5:
                                            comboBox_at9_useband.SelectedIndex = 5;
                                            nbandAT9 = " -nbands 8";
                                            break;
                                        case 6:
                                            comboBox_at9_useband.SelectedIndex = 6;
                                            nbandAT9 = " -nbands 9";
                                            break;
                                        case 7:
                                            comboBox_at9_useband.SelectedIndex = 7;
                                            nbandAT9 = " -nbands 10";
                                            break;
                                        case 8:
                                            comboBox_at9_useband.SelectedIndex = 8;
                                            nbandAT9 = " -nbands 11";
                                            break;
                                        case 9:
                                            comboBox_at9_useband.SelectedIndex = 9;
                                            nbandAT9 = " -nbands 12";
                                            break;
                                        case 10:
                                            comboBox_at9_useband.SelectedIndex = 10;
                                            nbandAT9 = " -nbands 13";
                                            break;
                                        case 11:
                                            comboBox_at9_useband.SelectedIndex = 11;
                                            nbandAT9 = " -nbands 14";
                                            break;
                                        case 12:
                                            comboBox_at9_useband.SelectedIndex = 12;
                                            nbandAT9 = " -nbands 15";
                                            break;
                                        case 13:
                                            comboBox_at9_useband.SelectedIndex = 13;
                                            nbandAT9 = " -nbands 16";
                                            break;
                                        case 14:
                                            comboBox_at9_useband.SelectedIndex = 14;
                                            nbandAT9 = " -nbands 17";
                                            break;
                                        case 15:
                                            comboBox_at9_useband.SelectedIndex = 15;
                                            nbandAT9 = " -nbands 18";
                                            break;
                                        default:
                                            comboBox_at9_useband.SelectedIndex = 15;
                                            nbandAT9 = "";
                                            break;
                                    }

                                    switch (int.Parse(Common.Config.Entry["ATRAC9_IsbandIndex"].Value))
                                    {
                                        case 0:
                                            comboBox_at9_startband.SelectedIndex = 0;
                                            isbandAT9 = " -isband -1";
                                            break;
                                        case 1:
                                            comboBox_at9_startband.SelectedIndex = 1;
                                            isbandAT9 = " -isband 3";
                                            break;
                                        default:
                                            comboBox_at9_startband.SelectedIndex = 0;
                                            isbandAT9 = "";
                                            break;
                                    }

                                    break;
                                }
                            case false:
                                {
                                    nbandAT9 = "";
                                    isbandAT9 = "";
                                    checkBox_at9_advband.Checked = false;
                                    label_at9_useband.Enabled = false;
                                    comboBox_at9_useband.Enabled = false;
                                    label_at9_startband.Enabled = false;
                                    comboBox_at9_startband.Enabled = false;
                                    break;
                                }
                        }

                        break;
                    }
                case false:
                    {
                        enctypeAT9 = "";
                        nbandAT9 = "";
                        isbandAT9 = "";
                        dualencAT9 = "";
                        supframeAT9 = "";
                        checkBox_at9_enctype.Checked = false;
                        checkBox_at9_enctype.Enabled = false;
                        checkBox_at9_advband.Checked = false;
                        checkBox_at9_advband.Enabled = false;
                        checkBox_at9_dualenc.Checked = false;
                        checkBox_at9_dualenc.Enabled = false;
                        checkBox_at9_supframe.Checked = false;
                        checkBox_at9_supframe.Enabled = false;
                        checkBox_bex.Checked = false;
                        checkBox_bex.Enabled = false;
                        checkBox_wband.Checked = false;
                        checkBox_wband.Enabled = false;
                        checkBox_LFE.Checked = false;
                        checkBox_LFE.Enabled = false;
                        comboBox_at9_enctype.Enabled = false;
                        comboBox_at9_useband.Enabled = false;
                        comboBox_at9_startband.Enabled = false;
                        label_at9_enctype.Enabled = false;
                        label_at9_startband.Enabled = false;
                        label_at9_useband.Enabled = false;
                        break;
                    }
            }

            if (Common.Config.Entry["ATRAC3_Params"].Value != "")
            {
                textBox_at3_cmd.Text = Common.Config.Entry["ATRAC3_Params"].Value;
                paramAT3 = Common.Config.Entry["ATRAC3_Params"].Value;
            }
            else
            {
                paramAT3 = RefleshParamAT3();
                textBox_at3_cmd.Text = paramAT3;
            }
            if (Common.Config.Entry["ATRAC9_Params"].Value != "")
            {
                textBox_at9_cmd.Text = Common.Config.Entry["ATRAC9_Params"].Value;
                paramAT9 = Common.Config.Entry["ATRAC9_Params"].Value;
            }
            else
            {
                paramAT9 = RefleshParamAT9();
                textBox_at9_cmd.Text = paramAT9;
            }
            if (Common.Config.Entry["Walkman_Params"].Value != "")
            {
                textBox_cmd_walkman.Text = Common.Config.Entry["Walkman_Params"].Value;
                paramWalkman = Common.Config.Entry["Walkman_Params"].Value;
            }
            else
            {
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramAT9;
            }

            switch (bool.Parse(Config.Entry["LPC_Create"].Value))
            {
                case true:
                    checkBox_lpcreate.Checked = true;
                    checkBox_at3_looppoint.Checked = false;
                    checkBox_at3_loopsound.Checked = false;
                    checkBox_at3_looppoint.Enabled = false;
                    checkBox_at3_loopsound.Enabled = false;
                    textBox_at3_loopend.Text = null;
                    textBox_at3_loopend.Enabled = false;
                    textBox_at3_loopstart.Text = null;
                    textBox_at3_loopstart.Enabled = false;
                    textBox_at9_loopend.Text = null;
                    textBox_at9_loopend.Enabled = false;
                    textBox_at9_loopstart.Text = null;
                    textBox_at9_loopstart.Enabled = false;
                    checkBox_at9_looppoint.Checked = false;
                    checkBox_at9_looppoint.Enabled = false;
                    checkBox_at9_loopsound.Checked = false;
                    checkBox_at9_loopsound.Enabled = false;
                    checkBox_at9_looplist.Checked = false;
                    checkBox_at9_looplist.Enabled = false;
                    looppointAT3 = "";
                    loopstartAT3 = "";
                    loopendAT3 = "";
                    looppointAT9 = "";
                    loopstartAT9 = "";
                    loopendAT9 = "";
                    Config.Entry["ATRAC3_LoopPoint"].Value = "false";
                    Config.Entry["ATRAC3_LoopStart_Samples"].Value = "";
                    Config.Entry["ATRAC3_LoopEnd_Samples"].Value = "";
                    Config.Entry["ATRAC9_LoopPoint"].Value = "false";
                    Config.Entry["ATRAC9_LoopStart_Samples"].Value = "";
                    Config.Entry["ATRAC9_LoopEnd_Samples"].Value = "";
                    Config.Save(xmlpath);
                    Config.Load(xmlpath);
                    break;
                case false:
                    checkBox_lpcreate.Checked = false;
                    checkBox_at3_looppoint.Enabled = true;
                    checkBox_at3_loopsound.Enabled = true;
                    textBox_at3_loopend.Enabled = true;
                    textBox_at3_loopstart.Enabled = true;
                    textBox_at9_loopend.Enabled = true;
                    textBox_at9_loopstart.Enabled = true;
                    checkBox_at9_looppoint.Enabled = true;
                    checkBox_at9_loopsound.Enabled = true;
                    break;
            }


            // Walkman Tabs

            if (bool.Parse(Config.Entry["Walkman_EveryFmt"].Value))
            {
                checkBox_everyFormats.Checked = true;
                label_Decodeformat.Enabled = false;
                label_Outputformat.Enabled = false;
                comboBox_DecodeFormats.Enabled = false;
                comboBox_OutputFormats.Enabled = false;
            }
            else
            {
                checkBox_everyFormats.Checked = false;
                label_Decodeformat.Enabled = true;
                label_Outputformat.Enabled = true;
                comboBox_DecodeFormats.Enabled = true;
                comboBox_OutputFormats.Enabled = true;
            }

            if (Config.Entry["Walkman_EveryFmt_OutputFmt"].Value is not null)
            {
                comboBox_OutputFormats.SelectedIndex = int.Parse(Config.Entry["Walkman_EveryFmt_OutputFmt"].Value);
            }
            else
            {
                comboBox_OutputFormats.SelectedIndex = 1;
            }

            if (Config.Entry["Walkman_EveryFmt_DecodeFmt"].Value is not null)
            {
                comboBox_DecodeFormats.SelectedIndex = int.Parse(Config.Entry["Walkman_EveryFmt_DecodeFmt"].Value);
            }
            else
            {
                comboBox_DecodeFormats.SelectedIndex = 0;
            }

            if (bool.Parse(Config.Entry["Walkman_FixSongInformation"].Value))
            {
                radioButton_each.Checked = false;
                radioButton_specified.Checked = true;
                groupBox_walkman_others.Enabled = true;
            }
            else
            {
                radioButton_each.Checked = true;
                radioButton_specified.Checked = false;
                groupBox_walkman_others.Enabled = false;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Bitrate"].Value))
            {
                bitrates = " --Bitrate -1";
                textBox_Bitrates.Text = "-1";
            }
            else
            {
                textBox_Bitrates.Text = Config.Entry["Walkman_Bitrate"].Value;
                bitrates = " --Bitrate " + textBox_Bitrates.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Title"].Value))
            {
                title = "";
                textBox_Title.Text = "";
            }
            else
            {
                textBox_Title.Text = Config.Entry["Walkman_Title"].Value;
                title = " --Title " + textBox_Title.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_SortTitle"].Value))
            {
                sorttitle = "";
                textBox_SortTitle.Text = "";
            }
            else
            {
                textBox_SortTitle.Text = Config.Entry["Walkman_SortTitle"].Value;
                sorttitle = " --SortTitle " + textBox_SortTitle.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_SubTitle"].Value))
            {
                subtitle = "";
                textBox_Subtitle.Text = "";
            }
            else
            {
                textBox_Subtitle.Text = Config.Entry["Walkman_SubTitle"].Value;
                subtitle = " --Subtitle" + textBox_Subtitle.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_SortSubTitle"].Value))
            {
                sortsubtitle = "";
                textBox_SortSubtitle.Text = "";
            }
            else
            {
                textBox_SortSubtitle.Text = Config.Entry["Walkman_SortSubTitle"].Value;
                sortsubtitle = " --SortSubtitle " + textBox_SortSubtitle.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Artist"].Value))
            {
                artist = "";
                textBox_Artist.Text = "";
            }
            else
            {
                textBox_Artist.Text = Config.Entry["Walkman_Artist"].Value;
                artist = " --Artist " + textBox_Artist.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_SortArtist"].Value))
            {
                sortartist = "";
                textBox_SortArtist.Text = "";
            }
            else
            {
                textBox_SortArtist.Text = Config.Entry["Walkman_SortArtist"].Value;
                sortartist = " --SortArtist" + textBox_SortArtist.Text;
            }

            /*if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_ArtistURL"].Value))
            {
                artisturl = "";
                textBox_ArtistURL.Text = "";
            }
            else
            {
                textBox_ArtistURL.Text = Config.Entry["Walkman_ArtistURL"].Value;
                artisturl = textBox_ArtistURL.Text;
            }*/

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Album"].Value))
            {
                album = "";
                textBox_Album.Text = "";
            }
            else
            {
                textBox_Album.Text = Config.Entry["Walkman_Album"].Value;
                album = " --Album " + textBox_Album.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_SortAlbum"].Value))
            {
                sortalbum = "";
                textBox_SortAlbum.Text = "";
            }
            else
            {
                textBox_SortAlbum.Text = Config.Entry["Walkman_SortAlbum"].Value;
                sortalbum = " --SortAlbum " + textBox_SortAlbum.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_AlbumArtist"].Value))
            {
                albumartist = "";
                textBox_AlbumArtist.Text = "";
            }
            else
            {
                textBox_AlbumArtist.Text = Config.Entry["Walkman_AlbumArtist"].Value;
                albumartist = " --AlbumArtist " + textBox_AlbumArtist.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_SortAlbumArtist"].Value))
            {
                sortalbumartist = "";
                textBox_SortAlbumArtist.Text = "";
            }
            else
            {
                textBox_SortAlbumArtist.Text = Config.Entry["Walkman_SortAlbumArtist"].Value;
                sortalbumartist = " --SortAlbumArtist " + textBox_SortAlbumArtist.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Genre"].Value))
            {
                genre = "";
                textBox_Genre.Text = "";
            }
            else
            {
                textBox_Genre.Text = Config.Entry["Walkman_Genre"].Value;
                genre = " --Genre " + textBox_Genre.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Composer"].Value))
            {
                composer = "";
                textBox_Composer.Text = "";
            }
            else
            {
                textBox_Composer.Text = Config.Entry["Walkman_Composer"].Value;
                composer = " --Composer " + textBox_Composer.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Lyricist"].Value))
            {
                lyricist = "";
                textBox_Lyricist.Text = "";
            }
            else
            {
                textBox_Lyricist.Text = Config.Entry["Walkman_Lyricist"].Value;
                lyricist = " --Lyricist " + textBox_Lyricist.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_TrackNumber"].Value))
            {
                tracknumber = "";
                textBox_TrackNumber.Text = "";
            }
            else
            {
                textBox_TrackNumber.Text = Config.Entry["Walkman_TrackNumber"].Value;
                tracknumber = " --TrackNumber " + textBox_TrackNumber.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_TotalTracks"].Value))
            {
                totaltracks = "";
                textBox_TotalTracks.Text = "";
            }
            else
            {
                textBox_TotalTracks.Text = Config.Entry["Walkman_TotalTracks"].Value;
                totaltracks = " --TotalTracks " + textBox_TotalTracks.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Release"].Value))
            {
                release = " --Release " + DateTime.Now.ToShortDateString().ToString();
                dateTimePicker_Release.Value = DateTime.Now;
            }
            else
            {
                dateTimePicker_Release.Value = DateTime.Parse(Config.Entry["Walkman_Release"].Value);
                release = " --Release " + dateTimePicker_Release.Value.ToShortDateString().ToString();
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Import"].Value))
            {
                import = " --Import " + DateTime.Now.ToShortDateString().ToString();
                dateTimePicker_Import.Value = DateTime.Now;
            }
            else
            {
                dateTimePicker_Import.Value = DateTime.Parse(Config.Entry["Walkman_Import"].Value);
                import = " --Import " + dateTimePicker_Import.Value.ToShortDateString().ToString();
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Duration"].Value))
            {
                duration = "";
                textBox_Duration.Text = "";
            }
            else
            {
                textBox_Duration.Text = Config.Entry["Walkman_Duration"].Value;
                duration = " --Duration " + textBox_Duration.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_MilliSecond"].Value))
            {
                milliseconds = "";
                textBox_MilliSecond.Text = "";
            }
            else
            {
                textBox_MilliSecond.Text = Config.Entry["Walkman_MilliSecond"].Value;
                milliseconds = " --MilliSecond " + textBox_MilliSecond.Text;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Lyrics"].Value))
            {
                lyrics = "";
                label_Lyricspath.Text = "";
            }
            else
            {
                lyrics = Config.Entry["Walkman_Lyrics"].Value;
                label_Lyricspath.Text = Config.Entry["Walkman_Lyrics"].Value.Replace(" --Lyrics \"", "").Replace("\"", "");
            }

            if (Config.Entry["Walkman_LyricsMode"].Value is not null)
            {
                comboBox_Lyricsmode.SelectedIndex = int.Parse(Config.Entry["Walkman_LyricsMode"].Value);
            }
            else
            {
                comboBox_Lyricsmode.SelectedIndex = 3;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_LinerNotes"].Value))
            {
                linernotes = "";
                label_Linerpath.Text = "";
            }
            else
            {
                linernotes = Config.Entry["Walkman_LinerNotes"].Value;
                label_Linerpath.Text = Config.Entry["Walkman_LinerNotes"].Value.Replace(" --LinerNotes \"", "").Replace("\"", ""); ;
            }

            if (Config.Entry["Walkman_LinerNotesMode"].Value is not null)
            {
                comboBox_Linermode.SelectedIndex = int.Parse(Config.Entry["Walkman_LinerNotesMode"].Value);
            }
            else
            {
                comboBox_Linermode.SelectedIndex = 3;
            }

            if (string.IsNullOrWhiteSpace(Config.Entry["Walkman_Jacket"].Value))
            {
                jacket = "";
                label_Jacketpath.Text = "";
            }
            else
            {
                jacket = Config.Entry["Walkman_Jacket"].Value;
                label_Jacketpath.Text = Config.Entry["Walkman_Jacket"].Value.Replace(" --Jacket \"", "").Replace("\"", "");
                pictureBox_Jacket.ImageLocation = Config.Entry["Walkman_Jacket"].Value.Replace(" --Jacket \"", "").Replace("\"", "");
            }

            if (Config.Entry["Walkman_JacketMode"].Value is not null)
            {
                comboBox_Jacketmode.SelectedIndex = int.Parse(Config.Entry["Walkman_JacketMode"].Value);
            }
            else
            {
                comboBox_Jacketmode.SelectedIndex = 3;
            }

            paramWalkman = RefleshParamWalkman();
            textBox_cmd_walkman.Text = paramWalkman;

            if (Setloopingpoint)
            {
                if (Generic.ATRACFlag == 0) // ATRAC3
                {
                    if (FormLPC.FormLPCInstance.SampleRate == 44100)
                    {
                        radioButton_PSP.Checked = true;
                        radioButton_PS3.Enabled = false;
                    }
                    else if (FormLPC.FormLPCInstance.SampleRate == 48000)
                    {
                        radioButton_PS3.Checked = true;
                        radioButton_PSP.Enabled = false;
                    }
                    checkBox_lpcreate.Enabled = false;
                    groupBox_at9.Enabled = false;
                    groupBox_at9_advanced.Enabled = false;
                    checkBox_at3_looppoint.Enabled = false;
                    checkBox_at3_loopsound.Enabled = false;
                    label_at3_cmd.Enabled = false;
                    textBox_at3_cmd.Enabled = false;
                    label_at9_cmd.Enabled = false;
                    textBox_at9_cmd.Enabled = false;
                    tabControl_Main.TabPages.Remove(tabPage2);
                    button_Cancel.Enabled = false;
                }
                else if (Generic.ATRACFlag == 1) // ATRAC9
                {
                    checkBox_lpcreate.Enabled = false;
                    groupBox_at3.Enabled = false;
                    checkBox_at9_looppoint.Enabled = false;
                    checkBox_at9_loopsound.Enabled = false;
                    checkBox_at9_looplist.Enabled = false;
                    textBox_at9_looplist.Enabled = false;
                    button_at9_looplist.Enabled = false;

                    label_at3_cmd.Enabled = false;
                    textBox_at3_cmd.Enabled = false;
                    label_at9_cmd.Enabled = false;
                    textBox_at9_cmd.Enabled = false;
                    tabControl_Main.TabPages.Remove(tabPage2);
                    button_Cancel.Enabled = false;
                }
                else
                {
                    throw new NotSupportedException("Not supported formats.");
                }
            }
            else
            {
                return;
            }
        }

        // ATRAC3

        private void ComboBox_at3_encmethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioButton_PSP.Checked == true)
            {
                switch (comboBox_at3_encmethod.SelectedIndex)
                {
                    case 0: // 32k
                        comboBox_at3_encmethod.SelectedIndex = 0;
                        bitrateAT3 = " -br 32";
                        break;
                    case 1: // 48k
                        comboBox_at3_encmethod.SelectedIndex = 1;
                        bitrateAT3 = " -br 48";
                        break;
                    case 2: // 52k
                        comboBox_at3_encmethod.SelectedIndex = 2;
                        bitrateAT3 = " -br 52";
                        break;
                    case 3: // 64k mono / stereo
                        comboBox_at3_encmethod.SelectedIndex = 3;
                        bitrateAT3 = " -br 64";
                        break;
                    case 4: // 66k mono / stereo
                        comboBox_at3_encmethod.SelectedIndex = 4;
                        bitrateAT3 = " -br 66";
                        break;
                    case 5: // 96k mono / stereo
                        comboBox_at3_encmethod.SelectedIndex = 5;
                        bitrateAT3 = " -br 96";
                        break;
                    case 6: // 105k stereo
                        comboBox_at3_encmethod.SelectedIndex = 6;
                        bitrateAT3 = " -br 105";
                        break;
                    case 7: // 128k mono / stereo
                        comboBox_at3_encmethod.SelectedIndex = 7;
                        bitrateAT3 = " -br 128";
                        break;
                    case 8: // 132k
                        comboBox_at3_encmethod.SelectedIndex = 8;
                        bitrateAT3 = " -br 132";
                        break;
                    case 9: // 160k
                        comboBox_at3_encmethod.SelectedIndex = 9;
                        bitrateAT3 = " -br 160";
                        break;
                    case 10: // 192k
                        comboBox_at3_encmethod.SelectedIndex = 10;
                        bitrateAT3 = " -br 192";
                        break;
                    case 11: // 256k
                        comboBox_at3_encmethod.SelectedIndex = 11;
                        bitrateAT3 = " -br 256";
                        break;
                    case 12: // 320k
                        comboBox_at3_encmethod.SelectedIndex = 12;
                        bitrateAT3 = " -br 320";
                        break;
                    case 13: // 352k
                        comboBox_at3_encmethod.SelectedIndex = 13;
                        bitrateAT3 = " -br 352";
                        break;
                    default:
                        comboBox_at3_encmethod.SelectedIndex = 7;
                        bitrateAT3 = " -br 128";
                        break;
                }
            }
            else if (radioButton_PS3.Checked == true)
            {
                switch (comboBox_at3_encmethod.SelectedIndex)
                {
                    case 0: // 32k
                        comboBox_at3_encmethod.SelectedIndex = 0;
                        bitrateAT3 = " -br 32";
                        break;
                    case 1: // 48k
                        comboBox_at3_encmethod.SelectedIndex = 1;
                        bitrateAT3 = " -br 48";
                        break;
                    case 2: // 57k
                        comboBox_at3_encmethod.SelectedIndex = 2;
                        bitrateAT3 = " -br 57";
                        break;
                    case 3: // 64k mono / stereo
                        comboBox_at3_encmethod.SelectedIndex = 3;
                        bitrateAT3 = " -br 64";
                        break;
                    case 4: // 72k mono / stereo
                        comboBox_at3_encmethod.SelectedIndex = 4;
                        bitrateAT3 = " -br 72";
                        break;
                    case 5: // 96k mono / stereo
                        comboBox_at3_encmethod.SelectedIndex = 5;
                        bitrateAT3 = " -br 96";
                        break;
                    case 6: // 114k stereo
                        comboBox_at3_encmethod.SelectedIndex = 6;
                        bitrateAT3 = " -br 114";
                        break;
                    case 7: // 128k mono / stereo
                        comboBox_at3_encmethod.SelectedIndex = 7;
                        bitrateAT3 = " -br 128";
                        break;
                    case 8: // 144k
                        comboBox_at3_encmethod.SelectedIndex = 8;
                        bitrateAT3 = " -br 144";
                        break;
                    case 9: // 160k
                        comboBox_at3_encmethod.SelectedIndex = 9;
                        bitrateAT3 = " -br 160";
                        break;
                    case 10: // 192k
                        comboBox_at3_encmethod.SelectedIndex = 10;
                        bitrateAT3 = " -br 192";
                        break;
                    case 11: // 256k
                        comboBox_at3_encmethod.SelectedIndex = 11;
                        bitrateAT3 = " -br 256";
                        break;
                    case 12: // 320k
                        comboBox_at3_encmethod.SelectedIndex = 12;
                        bitrateAT3 = " -br 320";
                        break;
                    case 13: // 384k
                        comboBox_at3_encmethod.SelectedIndex = 13;
                        bitrateAT3 = " -br 384";
                        break;
                    case 14: // 512k
                        comboBox_at3_encmethod.SelectedIndex = 14;
                        bitrateAT3 = " -br 512";
                        break;
                    case 15: // 768k
                        comboBox_at3_encmethod.SelectedIndex = 15;
                        bitrateAT3 = " -br 768";
                        break;
                    default:
                        comboBox_at3_encmethod.SelectedIndex = 10;
                        bitrateAT3 = " -br 192";
                        break;
                }
            }
            else
            {
                throw new Exception("An error occured.");
            }

            paramAT3 = RefleshParamAT3();
            textBox_at3_cmd.Text = paramAT3;
        }

        private void CheckBox_at3_loopsound_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at3_loopsound.Checked != false)
            {
                wholeloopAT3 = " -wholeloop";
                looppointAT3 = "";
                loopstartAT3 = "";
                loopendAT3 = "";
                checkBox_at3_looppoint.Enabled = false;
                label_at3_loopstart.Enabled = false;
                label_at3_loopend.Enabled = false;
                label_at3_samples.Enabled = false;
                textBox_at3_loopstart.Enabled = false;
                textBox_at3_loopstart.Text = null;
                textBox_at3_loopend.Enabled = false;
                textBox_at3_loopend.Text = null;
            }
            else
            {
                checkBox_at3_looppoint.Enabled = true;
                wholeloopAT3 = "";
            }
            paramAT3 = RefleshParamAT3();
            textBox_at3_cmd.Text = paramAT3;
        }

        private void CheckBox_at3_looppoint_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at3_looppoint.Checked != false)
            {
                looppointAT3 = " -loop";
                label_at3_loopstart.Enabled = true;
                label_at3_loopend.Enabled = true;
                label_at3_samples.Enabled = true;
                textBox_at3_loopstart.Enabled = true;
                textBox_at3_loopend.Enabled = true;
                wholeloopAT3 = "";
                checkBox_at3_loopsound.Checked = false;
                checkBox_at3_loopsound.Enabled = false;
            }
            else
            {
                looppointAT3 = "";
                loopstartAT3 = "";
                loopendAT3 = "";
                label_at3_loopstart.Enabled = false;
                label_at3_loopend.Enabled = false;
                label_at3_samples.Enabled = false;
                textBox_at3_loopstart.Enabled = false;
                textBox_at3_loopstart.Text = null;
                textBox_at3_loopend.Enabled = false;
                textBox_at3_loopend.Text = null;
                checkBox_at3_loopsound.Enabled = true;
            }
            paramAT3 = RefleshParamAT3();
            textBox_at3_cmd.Text = paramAT3;
        }

        private void TextBox_at3_loopstart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void TextBox_at3_loopend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void TextBox_at3_looptimes_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void CheckBox_at3_looptimes_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at3_looptimes.Checked != false)
            {
                looptimeAT3 = " -repeat";
                label_at3_nol.Enabled = true;
                label_at3_times.Enabled = true;
                textBox_at3_looptimes.Enabled = true;
            }
            else
            {
                looptimeAT3 = "";
                looptimesAT3 = "";
                label_at3_nol.Enabled = false;
                label_at3_times.Enabled = false;
                textBox_at3_looptimes.Enabled = false;
                textBox_at3_looptimes.Text = null;
            }
            paramAT3 = RefleshParamAT3();
            textBox_at3_cmd.Text = paramAT3;
        }

        private void TextBox_at3_loopstart_TextChanged(object sender, EventArgs e)
        {
            if (textBox_at3_loopstart.TextLength != 0)
            {
                loopstartAT3 = " " + textBox_at3_loopstart.Text;
            }
            else
            {
                loopstartAT3 = "";
            }
            paramAT3 = RefleshParamAT3();
            textBox_at3_cmd.Text = paramAT3;
        }

        private void TextBox_at3_loopend_TextChanged(object sender, EventArgs e)
        {
            if (textBox_at3_loopend.TextLength != 0)
            {
                loopendAT3 = " " + textBox_at3_loopend.Text;
            }
            else
            {
                loopendAT3 = "";
            }
            paramAT3 = RefleshParamAT3();
            textBox_at3_cmd.Text = paramAT3;
        }

        private void TextBox_at3_looptimes_TextChanged(object sender, EventArgs e)
        {
            if (textBox_at3_looptimes.TextLength != 0)
            {
                looptimesAT3 = " " + textBox_at3_looptimes.Text;
            }
            else
            {
                looptimesAT3 = "";
            }
            paramAT3 = RefleshParamAT3();
            textBox_at3_cmd.Text = paramAT3;
        }

        private void TextBox_at3_cmd_TextChanged(object sender, EventArgs e)
        {
            paramAT3 = textBox_at3_cmd.Text;
            textBox_at3_cmd.Text = paramAT3;
        }

        // ATRAC9

        private void ComboBox_at9_bitrate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioButton_PSV.Checked == true)
            {
                bitrateAT9 = comboBox_at9_bitrate.SelectedIndex switch
                {
                    0 => " -br 48",
                    1 => " -br 60",
                    2 => " -br 72",
                    3 => " -br 84",
                    4 => " -br 96",
                    5 => " -br 120",
                    6 => " -br 144",
                    7 => " -br 168",
                    8 => " -br 192",
                    9 => " -br 256",
                    10 => " -br 320",
                    _ => " -br 168",
                };
            }
            else if (radioButton_PS4.Checked == true)
            {
                bitrateAT9 = comboBox_at9_bitrate.SelectedIndex switch
                {
                    0 => " -br 48",
                    1 => " -br 60",
                    2 => " -br 72",
                    3 => " -br 84",
                    4 => " -br 96",
                    5 => " -br 120",
                    6 => " -br 144",
                    7 => " -br 168",
                    8 => " -br 192",
                    9 => " -br 240",
                    10 => " -br 256",
                    11 => " -br 300",
                    12 => " -br 320",
                    13 => " -br 384",
                    14 => " -br 420",
                    _ => " -br 168",
                };
            }
            else
            {
                throw new Exception("An error occured.");
            }

            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void ComboBox_at9_sampling_SelectedIndexChanged(object sender, EventArgs e)
        {
            samplingAT9 = comboBox_at9_sampling.SelectedIndex switch
            {
                0 => " -fs 12000",
                1 => " -fs 24000",
                2 => " -fs 48000",
                _ => " -fs 48000",
            };
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_at9_loopsound_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_loopsound.Checked != false)
            {
                wholeloopAT9 = " -wholeloop";
                looppointAT9 = "";
                loopstartAT9 = "";
                loopendAT9 = "";
                label_at9_loopstart.Enabled = false;
                label_at9_loopend.Enabled = false;
                label_at9_samples.Enabled = false;
                textBox_at9_loopstart.Enabled = false;
                textBox_at9_loopstart.Text = null;
                textBox_at9_loopend.Enabled = false;
                textBox_at9_loopend.Text = null;
                checkBox_at9_looppoint.Checked = false;
                checkBox_at9_looppoint.Enabled = false;
            }
            else
            {
                wholeloopAT9 = "";
                checkBox_at9_looppoint.Enabled = true;
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_at9_looppoint_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_looppoint.Checked != false)
            {
                wholeloopAT9 = "";
                looppointAT9 = " -loop";
                label_at9_loopstart.Enabled = true;
                label_at9_loopend.Enabled = true;
                label_at9_samples.Enabled = true;
                textBox_at9_loopstart.Enabled = true;
                textBox_at9_loopend.Enabled = true;
                checkBox_at9_loopsound.Checked = false;
                checkBox_at9_loopsound.Enabled = false;
            }
            else
            {
                looppointAT9 = "";
                loopstartAT9 = "";
                loopendAT9 = "";
                label_at9_loopstart.Enabled = false;
                label_at9_loopend.Enabled = false;
                label_at9_samples.Enabled = false;
                textBox_at9_loopstart.Enabled = false;
                textBox_at9_loopstart.Text = null;
                textBox_at9_loopend.Enabled = false;
                textBox_at9_loopend.Text = null;
                checkBox_at9_loopsound.Enabled = true;
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void TextBox_at9_loopstart_TextChanged(object sender, EventArgs e)
        {
            if (textBox_at9_loopstart.TextLength != 0)
            {
                loopstartAT9 = " " + textBox_at9_loopstart.Text;
            }
            else
            {
                loopstartAT9 = "";
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void TextBox_at9_loopend_TextChanged(object sender, EventArgs e)
        {
            if (textBox_at9_loopend.TextLength != 0)
            {
                loopendAT9 = " " + textBox_at9_loopend.Text;
            }
            else
            {
                loopendAT9 = "";
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void TextBox_at9_looptimes_TextChanged(object sender, EventArgs e)
        {
            if (textBox_at9_looptimes.TextLength != 0)
            {
                looptimesAT9 = " " + textBox_at9_looptimes.Text;
            }
            else
            {
                looptimesAT9 = "";
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void TextBox_at9_loopstart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void TextBox_at9_loopend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void TextBox_at9_looptimes_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void TextBox_bex_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void CheckBox_at9_looptimes_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_looptimes.Checked != false)
            {
                looptimeAT9 = " -repeat";
                label_at9_nol.Enabled = true;
                label_at9_times.Enabled = true;
                textBox_at9_looptimes.Enabled = true;
            }
            else
            {
                looptimeAT9 = "";
                looptimesAT9 = "";
                label_at9_nol.Enabled = false;
                label_at9_times.Enabled = false;
                textBox_at9_looptimes.Enabled = false;
                textBox_at9_looptimes.Text = null;
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_at9_looplist_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_looplist.Checked != false)
            {
                looplistAT9 = " -looplist";
                textBox_at9_looplist.Enabled = true;
                button_at9_looplist.Enabled = true;
            }
            else
            {
                looplistAT9 = "";
                looplistfileAT9 = "";
                textBox_at9_looplist.Enabled = false;
                textBox_at9_looplist.Text = null;
                button_at9_looplist.Enabled = false;
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void Button_at9_looplist_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = Localization.ListFilters,
                FilterIndex = 11,
                Title = Localization.OpenListDialogTitle,
                Multiselect = true,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox_at9_looplist.Text = ofd.FileName;
                looplistfileAT9 = " " + ofd.FileName;
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_at9_advanced_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_advanced.Checked != false)
            {
                if (radioButton_PSV.Checked != false)
                {
                    checkBox_at9_enctype.Enabled = true;
                    checkBox_at9_advband.Enabled = true;
                    checkBox_at9_dualenc.Enabled = true;
                    checkBox_at9_supframe.Enabled = true;
                    checkBox_bex.Checked = false;
                    checkBox_bex.Enabled = false;
                    checkBox_wband.Checked = false;
                    checkBox_wband.Enabled = false;
                    checkBox_LFE.Checked = false;
                    checkBox_LFE.Enabled = false;
                }
                else if (radioButton_PS4.Checked != false)
                {
                    checkBox_at9_enctype.Enabled = true;
                    checkBox_at9_advband.Enabled = true;
                    checkBox_at9_dualenc.Enabled = true;
                    checkBox_at9_supframe.Enabled = true;
                    checkBox_bex.Enabled = true;
                    checkBox_wband.Enabled = true;
                    checkBox_LFE.Enabled = true;
                }
            }
            else
            {
                enctypeAT9 = "";
                nbandAT9 = "";
                isbandAT9 = "";
                dualencAT9 = "";
                supframeAT9 = "";
                bex = "";
                wband = "";
                LFE = "";
                checkBox_at9_enctype.Checked = false;
                checkBox_at9_enctype.Enabled = false;
                checkBox_at9_advband.Checked = false;
                checkBox_at9_advband.Enabled = false;
                checkBox_at9_dualenc.Checked = false;
                checkBox_at9_dualenc.Enabled = false;
                checkBox_at9_supframe.Checked = false;
                checkBox_at9_supframe.Enabled = false;
                comboBox_at9_enctype.Enabled = false;
                comboBox_at9_useband.Enabled = false;
                comboBox_at9_startband.Enabled = false;
                label_at9_enctype.Enabled = false;
                label_at9_startband.Enabled = false;
                label_at9_useband.Enabled = false;
                checkBox_bex.Checked = false;
                checkBox_bex.Enabled = false;
                checkBox_wband.Checked = false;
                checkBox_wband.Enabled = false;
                checkBox_LFE.Checked = false;
                checkBox_LFE.Enabled = false;
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_at9_enctype_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_enctype.Checked != false)
            {
                comboBox_at9_enctype.Enabled = true;
                label_at9_enctype.Enabled = true;
            }
            else
            {
                enctypeAT9 = "";
                comboBox_at9_enctype.Enabled = false;
                label_at9_enctype.Enabled = false;
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_at9_dualenc_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_dualenc.Checked != false)
            {
                dualencAT9 = " -dual";
            }
            else
            {
                dualencAT9 = "";
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_at9_supframe_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_supframe.Checked != false)
            {
                supframeAT9 = " -supframeon";
            }
            else
            {
                supframeAT9 = " -supframeoff";
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_at9_advband_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_advband.Checked != false)
            {
                label_at9_useband.Enabled = true;
                comboBox_at9_useband.Enabled = true;
                label_at9_startband.Enabled = true;
                comboBox_at9_startband.Enabled = true;
            }
            else
            {
                nbandAT9 = "";
                isbandAT9 = "";
                label_at9_useband.Enabled = false;
                comboBox_at9_useband.Enabled = false;
                label_at9_startband.Enabled = false;
                comboBox_at9_startband.Enabled = false;
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_wband_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_wband.Checked != false)
            {
                wband = " -wband";
            }
            else
            {
                wband = "";
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_bex_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_bex.Checked != false)
            {
                bex = " -bex";
            }
            else
            {
                bex = "";
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void CheckBox_LFE_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_LFE.Checked != false)
            {
                LFE = " -slc";
            }
            else
            {
                LFE = "";
            }
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void ComboBox_at9_useband_SelectedIndexChanged(object sender, EventArgs e)
        {
            nbandAT9 = comboBox_at9_useband.SelectedIndex switch
            {
                0 => " -nbands 3",
                1 => " -nbands 4",
                2 => " -nbands 5",
                3 => " -nbands 6",
                4 => " -nbands 7",
                5 => " -nbands 8",
                6 => " -nbands 9",
                7 => " -nbands 10",
                8 => " -nbands 11",
                9 => " -nbands 12",
                10 => " -nbands 13",
                11 => " -nbands 14",
                12 => " -nbands 15",
                13 => " -nbands 16",
                14 => " -nbands 17",
                15 => " -nbands 18",
                _ => "",
            };
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void ComboBox_at9_startband_SelectedIndexChanged(object sender, EventArgs e)
        {
            isbandAT9 = comboBox_at9_startband.SelectedIndex switch
            {
                0 => " -isband -1",
                1 => " -isband 3",
                _ => "",
            };
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void ComboBox_at9_enctype_SelectedIndexChanged(object sender, EventArgs e)
        {
            enctypeAT9 = comboBox_at9_enctype.SelectedIndex switch
            {
                0 => " -gradmode 0",
                1 => " -gradmode 1",
                2 => " -gradmode 2",
                3 => " -gradmode 3",
                4 => " -gradmode 4",
                5 => "",
                _ => "",
            };
            paramAT9 = RefleshParamAT9();
            textBox_at9_cmd.Text = paramAT9;
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            if (paramAT3.Contains("$InFile $OutFile") != true || paramAT3.Contains("$InFile") != true || paramAT3.Contains("$OutFile") != true)
            {
                MessageBox.Show(this, Localization.NotFoundIOStringCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                paramAT3 = RefleshParamAT3();
                textBox_at3_cmd.Text = paramAT3;
                return;
            }

            if (paramAT9.Contains("$InFile $OutFile") != true || paramAT9.Contains("$InFile") != true || paramAT9.Contains("$OutFile") != true)
            {
                MessageBox.Show(this, Localization.NotFoundIOStringCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                paramAT9 = RefleshParamAT9();
                textBox_at9_cmd.Text = paramAT9;
                return;
            }

            if (paramWalkman.Contains("$OutFile $InFile") != true || paramWalkman.Contains("$InFile") != true || paramWalkman.Contains("$OutFile") != true)
            {
                MessageBox.Show(this, Localization.NotFoundIOStringCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
                return;
            }

            if (checkBox_at3_looppoint.Checked != false)
            {
                if (textBox_at3_loopstart.TextLength == 0)
                {
                    MessageBox.Show(this, Localization.LoopStartErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (textBox_at3_loopend.TextLength == 0)
                {
                    MessageBox.Show(this, Localization.LoopEndErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (checkBox_at3_looptimes.Checked != false)
            {
                if (textBox_at3_looptimes.TextLength == 0)
                {
                    MessageBox.Show(this, Localization.LoopTimesErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (checkBox_at9_looppoint.Checked != false)
            {
                if (textBox_at9_loopstart.TextLength == 0)
                {
                    MessageBox.Show(this, Localization.LoopStartErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (textBox_at9_loopend.TextLength == 0)
                {
                    MessageBox.Show(this, Localization.LoopEndErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (checkBox_at9_looptimes.Checked != false)
            {
                if (textBox_at9_looptimes.TextLength == 0)
                {
                    MessageBox.Show(this, Localization.LoopTimesErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (checkBox_at9_looplist.Checked != false)
            {
                if (textBox_at9_looplist.TextLength == 0)
                {
                    MessageBox.Show(this, Localization.LoopListErrorCaption, Localization.MSGBoxErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (radioButton_PSP.Checked != false)
            {
                Common.Config.Entry["ATRAC3_Console"].Value = "0";
            }
            else if (radioButton_PS3.Checked != false)
            {
                Common.Config.Entry["ATRAC3_Console"].Value = "1";
            }
            else
            {
                Common.Config.Entry["ATRAC3_Console"].Value = "-1";
            }

            if (radioButton_PSV.Checked != false)
            {
                Common.Config.Entry["ATRAC9_Console"].Value = "0";
            }
            else if (radioButton_PS4.Checked != false)
            {
                Common.Config.Entry["ATRAC9_Console"].Value = "1";
            }
            else
            {
                Common.Config.Entry["ATRAC9_Console"].Value = "-1";
            }

            Common.Config.Entry["ATRAC3_Bitrate"].Value = comboBox_at3_encmethod.SelectedIndex.ToString();
            Common.Config.Entry["ATRAC9_Bitrate"].Value = comboBox_at9_bitrate.SelectedIndex.ToString();
            Common.Config.Entry["ATRAC9_Sampling"].Value = comboBox_at9_sampling.SelectedIndex.ToString();

            if (checkBox_at3_loopsound.Checked != false)
            {
                Common.Config.Entry["ATRAC3_LoopSound"].Value = "true";
            }
            else
            {
                Common.Config.Entry["ATRAC3_LoopSound"].Value = "false";
            }
            if (checkBox_at3_looppoint.Checked != false)
            {
                Common.Config.Entry["ATRAC3_LoopPoint"].Value = "true";
                Common.Config.Entry["ATRAC3_LoopStart_Samples"].Value = textBox_at3_loopstart.Text;
                Common.Config.Entry["ATRAC3_LoopEnd_Samples"].Value = textBox_at3_loopend.Text;
            }
            else
            {
                Common.Config.Entry["ATRAC3_LoopPoint"].Value = "false";
                Common.Config.Entry["ATRAC3_LoopStart_Samples"].Value = "";
                Common.Config.Entry["ATRAC3_LoopEnd_Samples"].Value = "";
            }
            if (checkBox_at3_looptimes.Checked != false)
            {
                Common.Config.Entry["ATRAC3_LoopTime"].Value = "true";
                Common.Config.Entry["ATRAC3_LoopTimes"].Value = textBox_at3_looptimes.Text;
            }
            else
            {
                Common.Config.Entry["ATRAC3_LoopTime"].Value = "false";
                Common.Config.Entry["ATRAC3_LoopTimes"].Value = "";
            }

            if (checkBox_at9_loopsound.Checked != false)
            {
                Common.Config.Entry["ATRAC9_LoopSound"].Value = "true";
            }
            else
            {
                Common.Config.Entry["ATRAC9_LoopSound"].Value = "false";
            }
            if (checkBox_at9_looppoint.Checked != false)
            {
                Common.Config.Entry["ATRAC9_LoopPoint"].Value = "true";
                Common.Config.Entry["ATRAC9_LoopStart_Samples"].Value = textBox_at9_loopstart.Text;
                Common.Config.Entry["ATRAC9_LoopEnd_Samples"].Value = textBox_at9_loopend.Text;
            }
            else
            {
                Common.Config.Entry["ATRAC9_LoopPoint"].Value = "false";
                Common.Config.Entry["ATRAC9_LoopStart_Samples"].Value = "";
                Common.Config.Entry["ATRAC9_LoopEnd_Samples"].Value = "";
            }
            if (checkBox_at9_looptimes.Checked != false)
            {
                Common.Config.Entry["ATRAC9_LoopTime"].Value = "true";
                Common.Config.Entry["ATRAC9_LoopTimes"].Value = textBox_at9_looptimes.Text;
            }
            else
            {
                Common.Config.Entry["ATRAC9_LoopTime"].Value = "false";
                Common.Config.Entry["ATRAC9_LoopTimes"].Value = "";
            }
            if (checkBox_at9_looplist.Checked != false)
            {
                Common.Config.Entry["ATRAC9_LoopTime"].Value = "true";
                Common.Config.Entry["ATRAC9_LoopTimes"].Value = textBox_at9_looplist.Text;
            }
            else
            {
                Common.Config.Entry["ATRAC9_LoopList"].Value = "false";
                Common.Config.Entry["ATRAC9_LoopListFile"].Value = "";
            }
            if (checkBox_at9_advanced.Checked != false)
            {
                Common.Config.Entry["ATRAC9_Advanced"].Value = "true";
                if (checkBox_at9_enctype.Checked != false)
                {
                    Common.Config.Entry["ATRAC9_EncodeType"].Value = "true";
                    Common.Config.Entry["ATRAC9_EncodeTypeIndex"].Value = comboBox_at9_enctype.SelectedIndex.ToString();
                }
                else
                {
                    Common.Config.Entry["ATRAC9_EncodeType"].Value = "false";
                    Common.Config.Entry["ATRAC9_EncodeTypeIndex"].Value = "5";
                }
                if (checkBox_at9_advband.Checked != false)
                {
                    Common.Config.Entry["ATRAC9_AdvancedBand"].Value = "true";
                    Common.Config.Entry["ATRAC9_NbandsIndex"].Value = comboBox_at9_useband.SelectedIndex.ToString();
                    Common.Config.Entry["ATRAC9_IsbandIndex"].Value = comboBox_at9_startband.SelectedIndex.ToString();
                }
                else
                {
                    Common.Config.Entry["ATRAC9_AdvancedBand"].Value = "false";
                    Common.Config.Entry["ATRAC9_NbandsIndex"].Value = "";
                    Common.Config.Entry["ATRAC9_IsbandIndex"].Value = "";
                }
                if (checkBox_at9_dualenc.Checked != false)
                {
                    Common.Config.Entry["ATRAC9_DualEncode"].Value = "true";
                }
                else
                {
                    Common.Config.Entry["ATRAC9_DualEncode"].Value = "false";
                }
                if (checkBox_at9_supframe.Checked != false)
                {
                    Common.Config.Entry["ATRAC9_SuperFrameEncode"].Value = "true";
                }
                else
                {
                    Common.Config.Entry["ATRAC9_SuperFrameEncode"].Value = "false";
                }
                if (checkBox_wband.Checked != false)
                {
                    Common.Config.Entry["ATRAC9_WideBand"].Value = "true";
                }
                else
                {
                    Common.Config.Entry["ATRAC9_WideBand"].Value = "false";
                }
                if (checkBox_bex.Checked != false)
                {
                    Common.Config.Entry["ATRAC9_BandExtension"].Value = "true";
                }
                else
                {
                    Common.Config.Entry["ATRAC9_BandExtension"].Value = "false";
                }
                if (checkBox_LFE.Checked != false)
                {
                    Common.Config.Entry["ATRAC9_LFE_SuperLowCut"].Value = "true";
                }
                else
                {
                    Common.Config.Entry["ATRAC9_LFE_SuperLowCut"].Value = "false";
                }
            }
            else
            {
                Common.Config.Entry["ATRAC9_Advanced"].Value = "false";
                Common.Config.Entry["ATRAC9_EncodeType"].Value = "false";
                Common.Config.Entry["ATRAC9_EncodeTypeIndex"].Value = "";
                Common.Config.Entry["ATRAC9_AdvancedBand"].Value = "false";
                Common.Config.Entry["ATRAC9_NbandsIndex"].Value = "";
                Common.Config.Entry["ATRAC9_IsbandIndex"].Value = "";
                Common.Config.Entry["ATRAC9_DualEncode"].Value = "false";
                Common.Config.Entry["ATRAC9_SuperFrameEncode"].Value = "false";
            }

            if (checkBox_lpcreate.Checked != false)
            {
                Common.Config.Entry["LPC_Create"].Value = "true";
            }
            else
            {
                Common.Config.Entry["LPC_Create"].Value = "false";
            }

            if (checkBox_everyFormats.Checked)
            {
                Config.Entry["Walkman_EveryFmt"].Value = "true";
            }
            else
            {
                Config.Entry["Walkman_EveryFmt"].Value = "false";
            }

            Config.Entry["Walkman_EveryFmt_DecodeFmt"].Value = comboBox_DecodeFormats.SelectedIndex.ToString();
            Config.Entry["Walkman_EveryFmt_OutputFmt"].Value = comboBox_OutputFormats.SelectedIndex.ToString();
            switch (comboBox_OutputFormats.SelectedIndex)
            {
                case 0:
                    Common.Generic.WalkmanEveryFilter = "PCM ATRAC (*.oma)|*.oma;";
                    break;
                case 1:
                    Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.oma)|*.oma;";
                    break;
                case 2:
                    Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3 (*.omg)|*.omg;";
                    break;
                case 3:
                    Common.Generic.WalkmanEveryFilter = "ATRAC3 Advanced Lossless (*.oma)|*.oma;";
                    break;
                case 4:
                    Common.Generic.WalkmanEveryFilter = "ATRAC3 Video Clip (*.kdr)|*.kdr;";
                    break;
                case 5:
                    Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3+ (*.oma)|*.oma;";
                    break;
                case 6:
                    Common.Generic.WalkmanEveryFilter = "OpenMG ATRAC3+ (*.omg)|*.omg;";
                    break;
                case 7:
                    Common.Generic.WalkmanEveryFilter = "ATRAC3+ Advanced Lossless (*.oma)|*.oma;";
                    break;
                case 8:
                    Common.Generic.WalkmanEveryFilter = "ATRAC3+ Video Clip (*.kdr)|*.kdr;";
                    break;
            }

            if (radioButton_each.Checked)
            {
                Config.Entry["Walkman_FixSongInformation"].Value = "false";
            }
            else
            {
                Config.Entry["Walkman_FixSongInformation"].Value = "true";
            }

            if (string.IsNullOrWhiteSpace(textBox_Bitrates.Text))
            {
                Config.Entry["Walkman_Bitrate"].Value = "-1";
            }
            else
            {
                Config.Entry["Walkman_Bitrate"].Value = textBox_Bitrates.Text;
            }

            // Walkman Tags
            if (string.IsNullOrWhiteSpace(textBox_Title.Text))
            {
                Config.Entry["Walkman_Title"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_Title"].Value = textBox_Title.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_SortTitle.Text))
            {
                Config.Entry["Walkman_SortTitle"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_SortTitle"].Value = textBox_SortTitle.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_Subtitle.Text))
            {
                Config.Entry["Walkman_SubTitle"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_SubTitle"].Value = textBox_Subtitle.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_SortSubtitle.Text))
            {
                Config.Entry["Walkman_SortSubTitle"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_SortSubTitle"].Value = textBox_SortSubtitle.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_Artist.Text))
            {
                Config.Entry["Walkman_Artist"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_Artist"].Value = textBox_Artist.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_SortArtist.Text))
            {
                Config.Entry["Walkman_SortArtist"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_SortArtist"].Value = textBox_SortArtist.Text;
            }

            /*if (string.IsNullOrWhiteSpace(textBox_ArtistURL.Text))
            {
                Config.Entry["Walkman_ArtistURL"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_ArtistURL"].Value = textBox_ArtistURL.Text;
            }*/

            if (string.IsNullOrWhiteSpace(textBox_Album.Text))
            {
                Config.Entry["Walkman_Album"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_Album"].Value = textBox_Album.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_SortAlbum.Text))
            {
                Config.Entry["Walkman_SortAlbum"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_SortAlbum"].Value = textBox_SortAlbum.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_AlbumArtist.Text))
            {
                Config.Entry["Walkman_AlbumArtist"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_AlbumArtist"].Value = textBox_AlbumArtist.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_SortAlbumArtist.Text))
            {
                Config.Entry["Walkman_SortAlbumArtist"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_SortAlbumArtist"].Value = textBox_SortAlbumArtist.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_Genre.Text))
            {
                Config.Entry["Walkman_Genre"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_Genre"].Value = textBox_Genre.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_Composer.Text))
            {
                Config.Entry["Walkman_Composer"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_Composer"].Value = textBox_Composer.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_Lyricist.Text))
            {
                Config.Entry["Walkman_Lyricist"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_Lyricist"].Value = textBox_Lyricist.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_TrackNumber.Text))
            {
                Config.Entry["Walkman_TrackNumber"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_TrackNumber"].Value = textBox_TrackNumber.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_TotalTracks.Text))
            {
                Config.Entry["Walkman_TotalTracks"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_TotalTracks"].Value = textBox_TotalTracks.Text;
            }

            Config.Entry["Walkman_Release"].Value = dateTimePicker_Release.Value.ToShortDateString();
            Config.Entry["Walkman_Import"].Value = dateTimePicker_Import.Value.ToShortDateString();

            if (string.IsNullOrWhiteSpace(textBox_Duration.Text))
            {
                Config.Entry["Walkman_Duration"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_Duration"].Value = textBox_Duration.Text;
            }

            if (string.IsNullOrWhiteSpace(textBox_MilliSecond.Text))
            {
                Config.Entry["Walkman_MilliSecond"].Value = "";
            }
            else
            {
                Config.Entry["Walkman_MilliSecond"].Value = textBox_MilliSecond.Text;
            }

            Config.Entry["Walkman_Lyrics"].Value = lyrics;
            Config.Entry["Walkman_LyricsMode"].Value = comboBox_Lyricsmode.SelectedIndex.ToString();
            Config.Entry["Walkman_LinerNotes"].Value = linernotes;
            Config.Entry["Walkman_LinerMode"].Value = comboBox_Linermode.SelectedIndex.ToString();
            Config.Entry["Walkman_Jacket"].Value = jacket;
            Config.Entry["Walkman_JacketMode"].Value = comboBox_Jacketmode.SelectedIndex.ToString();

            Common.Config.Entry["ATRAC3_Params"].Value = paramAT3;
            Common.Config.Entry["ATRAC9_Params"].Value = paramAT9;
            Config.Entry["Walkman_Params"].Value = paramWalkman;

            Config.Save(xmlpath);

            Utils.PictureboxImageDispose(pictureBox_Jacket);
            Close();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CheckBox_lpcreate_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_lpcreate.Checked != false)
            {
                if (Common.Generic.lpcreatev2 != false)
                {
                    switch (Common.Generic.ATRACFlag)
                    {
                        case 0:
                            checkBox_at3_looppoint.Enabled = true;
                            checkBox_at3_loopsound.Enabled = false;
                            textBox_at3_loopend.Enabled = true;
                            textBox_at3_loopstart.Enabled = true;
                            textBox_at9_loopend.Enabled = false;
                            textBox_at9_loopstart.Enabled = false;
                            checkBox_at9_looppoint.Enabled = false;
                            checkBox_at9_loopsound.Enabled = false;
                            break;
                        case 1:
                            checkBox_at3_looppoint.Enabled = false;
                            checkBox_at3_loopsound.Enabled = false;
                            textBox_at3_loopend.Enabled = false;
                            textBox_at3_loopstart.Enabled = false;
                            textBox_at9_loopend.Enabled = true;
                            textBox_at9_loopstart.Enabled = true;
                            checkBox_at9_looppoint.Enabled = true;
                            checkBox_at9_loopsound.Enabled = false;
                            break;
                    }
                }
                else
                {
                    checkBox_at3_looppoint.Checked = false;
                    checkBox_at3_loopsound.Checked = false;
                    checkBox_at3_looppoint.Enabled = false;
                    checkBox_at3_loopsound.Enabled = false;
                    textBox_at3_loopend.Text = null;
                    textBox_at3_loopend.Enabled = false;
                    textBox_at3_loopstart.Text = null;
                    textBox_at3_loopstart.Enabled = false;
                    textBox_at9_loopend.Text = null;
                    textBox_at9_loopend.Enabled = false;
                    textBox_at9_loopstart.Text = null;
                    textBox_at9_loopstart.Enabled = false;
                    checkBox_at9_looppoint.Checked = false;
                    checkBox_at9_looppoint.Enabled = false;
                    checkBox_at9_loopsound.Checked = false;
                    checkBox_at9_loopsound.Enabled = false;
                    looppointAT3 = "";
                    loopstartAT3 = "";
                    loopendAT3 = "";
                    looppointAT9 = "";
                    loopstartAT9 = "";
                    loopendAT9 = "";
                }
            }
            else
            {
                checkBox_at3_looppoint.Enabled = true;
                checkBox_at3_loopsound.Enabled = true;
                textBox_at3_loopend.Enabled = true;
                textBox_at3_loopstart.Enabled = true;
                textBox_at9_loopend.Enabled = true;
                textBox_at9_loopstart.Enabled = true;
                checkBox_at9_looppoint.Enabled = true;
                checkBox_at9_loopsound.Enabled = true;
            }
        }

        private void RadioButton_PSP_CheckedChanged(object sender, EventArgs e)
        {
            comboBox_at3_encmethod.Items.Clear();
            comboBox_at3_encmethod.Items.Add("32kbps, mono");
            comboBox_at3_encmethod.Items.Add("48kbps, mono / stereo");
            comboBox_at3_encmethod.Items.Add("52kbps, mono");
            comboBox_at3_encmethod.Items.Add("64kbps, mono / stereo");
            comboBox_at3_encmethod.Items.Add("66kbps, mono / stereo");
            comboBox_at3_encmethod.Items.Add("96kbps, mono / stereo");
            comboBox_at3_encmethod.Items.Add("105kbps, stereo");
            comboBox_at3_encmethod.Items.Add("128kbps, mono / stereo");
            comboBox_at3_encmethod.Items.Add("132kbps, stereo");
            comboBox_at3_encmethod.Items.Add("160kbps, stereo");
            comboBox_at3_encmethod.Items.Add("192kbps, stereo");
            comboBox_at3_encmethod.Items.Add("256kbps, stereo");
            comboBox_at3_encmethod.Items.Add("320kbps, stereo");
            comboBox_at3_encmethod.Items.Add("352kbps, stereo");
            comboBox_at3_encmethod.SelectedIndex = 7;
            bitrateAT3 = " -br 128";
        }

        private void RadioButton_PS3_CheckedChanged(object sender, EventArgs e)
        {
            comboBox_at3_encmethod.Items.Clear();
            comboBox_at3_encmethod.Items.Add("32kbps, mono");
            comboBox_at3_encmethod.Items.Add("48kbps, mono");
            comboBox_at3_encmethod.Items.Add("57kbps, mono");
            comboBox_at3_encmethod.Items.Add("64kbps, mono / stereo");
            comboBox_at3_encmethod.Items.Add("72kbps, mono / stereo");
            comboBox_at3_encmethod.Items.Add("96kbps, mono / stereo");
            comboBox_at3_encmethod.Items.Add("114kbps, stereo");
            comboBox_at3_encmethod.Items.Add("128kbps, mono / stereo");
            comboBox_at3_encmethod.Items.Add("144kbps, stereo");
            comboBox_at3_encmethod.Items.Add("160kbps, stereo");
            comboBox_at3_encmethod.Items.Add("192kbps, stereo / 6ch");
            comboBox_at3_encmethod.Items.Add("256kbps, stereo / 6ch");
            comboBox_at3_encmethod.Items.Add("320kbps, stereo / 6ch");
            comboBox_at3_encmethod.Items.Add("384kbps, 6ch / 8ch");
            comboBox_at3_encmethod.Items.Add("512kbps, 6ch");
            comboBox_at3_encmethod.Items.Add("768kbps, 8ch");
            comboBox_at3_encmethod.SelectedIndex = 10;
            bitrateAT3 = " -br 192";
        }

        private void RadioButton_PSV_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_advanced.Checked != false)
            {
                bex = "";
                wband = "";
                LFE = "";
                checkBox_bex.Checked = false;
                checkBox_bex.Enabled = false;
                checkBox_wband.Checked = false;
                checkBox_wband.Enabled = false;
                checkBox_LFE.Checked = false;
                checkBox_LFE.Enabled = false;
            }
            comboBox_at9_bitrate.Items.Clear();
            comboBox_at9_bitrate.Items.Add("48kbps, mono");
            comboBox_at9_bitrate.Items.Add("60kbps, mono");
            comboBox_at9_bitrate.Items.Add("72kbps, mono");
            comboBox_at9_bitrate.Items.Add("84kbps, mono");
            comboBox_at9_bitrate.Items.Add("96kbps, mono");
            comboBox_at9_bitrate.Items.Add("120kbps, mono");
            comboBox_at9_bitrate.Items.Add("144kbps, stereo");
            comboBox_at9_bitrate.Items.Add("168kbps, stereo");
            comboBox_at9_bitrate.Items.Add("192kbps, stereo");
            comboBox_at9_bitrate.Items.Add("256kbps, stereo");
            comboBox_at9_bitrate.Items.Add("320kbps, stereo");
            comboBox_at9_bitrate.SelectedIndex = 7;
            bitrateAT9 = " -br 168";
        }

        private void RadioButton_PS4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_at9_advanced.Checked != false)
            {
                checkBox_bex.Enabled = true;
                checkBox_wband.Enabled = true;
                checkBox_LFE.Enabled = true;
            }
            comboBox_at9_bitrate.Items.Clear();
            comboBox_at9_bitrate.Items.Add("48kbps, mono");
            comboBox_at9_bitrate.Items.Add("60kbps, mono");
            comboBox_at9_bitrate.Items.Add("72kbps, mono");
            comboBox_at9_bitrate.Items.Add("84kbps, mono");
            comboBox_at9_bitrate.Items.Add("96kbps, mono");
            comboBox_at9_bitrate.Items.Add("120kbps, mono");
            comboBox_at9_bitrate.Items.Add("144kbps, stereo");
            comboBox_at9_bitrate.Items.Add("168kbps, stereo");
            comboBox_at9_bitrate.Items.Add("192kbps, stereo");
            comboBox_at9_bitrate.Items.Add("240kbps, 4ch");
            comboBox_at9_bitrate.Items.Add("256kbps, 4ch");
            comboBox_at9_bitrate.Items.Add("300kbps, 5.1ch");
            comboBox_at9_bitrate.Items.Add("320kbps, 5.1ch");
            comboBox_at9_bitrate.Items.Add("384kbps, 5.1ch");
            comboBox_at9_bitrate.Items.Add("420kbps, 7.1ch");
            comboBox_at9_bitrate.SelectedIndex = 7;
            bitrateAT9 = " -br 168";
        }

        private string RefleshParamAT3()
        {
            return "at3tool -e" + bitrateAT3 + looppointAT3 + loopstartAT3 + loopendAT3 + wholeloopAT3 + looptimeAT3 + looptimesAT3 + " $InFile $OutFile";
        }

        private string RefleshParamAT9()
        {
            return "at9tool -e" + bitrateAT9 + samplingAT9 + looppointAT9 + loopstartAT9 + loopendAT9 + wholeloopAT9 + looptimeAT9 + looptimesAT9 + looplistAT9 + looplistfileAT9 + supframeAT9 + dualencAT9 + enctypeAT9 + nbandAT9 + isbandAT9 + wband + bex + LFE + " $InFile $OutFile";
        }

        private string RefleshParamWalkman()
        {
            return "traconv --Convert" + walkmanfmt + bitrates + title + sorttitle + subtitle + sortsubtitle + artist + sortartist + album + sortalbum + albumartist + sortalbumartist + genre + composer + lyricist + tracknumber + totaltracks + release + import + duration + milliseconds + lyrics + lyricsmode + linernotes + linernotesmode + jacket + jacketmode + " --Output $OutFile $InFile";
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        // Walkman Functions

        private void checkBox_everyFormats_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_everyFormats.Checked)
            {
                label_Decodeformat.Enabled = false;
                label_Outputformat.Enabled = false;
                comboBox_DecodeFormats.Enabled = false;
                comboBox_OutputFormats.Enabled = false;
            }
            else
            {
                label_Decodeformat.Enabled = true;
                label_Outputformat.Enabled = true;
                comboBox_DecodeFormats.Enabled = true;
                comboBox_OutputFormats.Enabled = true;
            }
        }

        private void radioButton_each_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_each.Checked)
            {
                groupBox_walkman_others.Enabled = false;
            }
            else
            {
                groupBox_walkman_others.Enabled = true;
            }
        }

        private void radioButton_specified_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_specified.Checked)
            {
                groupBox_walkman_others.Enabled = true;
            }
            else
            {
                groupBox_walkman_others.Enabled = false;
            }
        }

        private void comboBox_DecodeFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_DecodeFormats.SelectedIndex)
            {
                case 0:
                    walkmanfmt = " --FileType WAV";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
                case 1:
                    walkmanfmt = " --FileType AACL";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
                case 2:
                    walkmanfmt = " --FileType AACH";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
            }
        }

        private void comboBox_OutputFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_OutputFormats.SelectedIndex)
            {
                case 0:
                    walkmanfmt = " --FileType PCM";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
                case 1:
                    walkmanfmt = " --FileType OMA3";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
                case 2:
                    walkmanfmt = " --FileType OMG3";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
                case 3:
                    walkmanfmt = " --FileType AAL3";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
                case 4:
                    walkmanfmt = " --FileType KDR3";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
                case 5:
                    walkmanfmt = " --FileType OMAP";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
                case 6:
                    walkmanfmt = " --FileType OMGP";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
                case 7:
                    walkmanfmt = " --FileType AALP";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
                case 8:
                    walkmanfmt = " --FileType KDRP";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                    break;
            }
        }

        private void textBox_Bitrates_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Bitrates.Text))
            {
                bitrates = " --Bitrate -1";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                bitrates = " --Bitrate " + textBox_Bitrates.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_Bitrates_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void textBox_TrackNumber_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_TrackNumber.Text))
            {
                tracknumber = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                tracknumber = " --TrackNumber " + textBox_TrackNumber.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_TotalTracks_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_TotalTracks.Text))
            {
                totaltracks = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                totaltracks = " --TotalTracks " + textBox_TotalTracks.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_Duration_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Duration.Text))
            {
                duration = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                duration = " --Duration " + textBox_Duration.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_MilliSecond_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_MilliSecond.Text))
            {
                milliseconds = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                milliseconds = " --MilliSecond " + textBox_MilliSecond.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_TrackNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void textBox_TotalTracks_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void textBox_Duration_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void textBox_MilliSecond_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void button_Lyricspath_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = "Lyrics file (*.lrc)|*.lrc;|JPEG Image (*.jpg)|*.jpg;|PNG Image (*.png)|*.png;|All Files (*.*)|*.*;",
                FilterIndex = 0,
                Title = "Open Lyrics file",
                Multiselect = true,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                label_Lyricspath.Text = ofd.FileName;
                lyrics = " --Lyrics \"" + ofd.FileName + "\"";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                label_Lyricspath.Text = "";
                lyrics = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void comboBox_Lyricsmode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_Lyricsmode.SelectedIndex)
            {
                case 0:
                    lyricsmode = " --LyricsMode Delete";
                    break;
                case 1:
                    lyricsmode = " --LyricsMode Text";
                    break;
                case 2:
                    lyricsmode = " --LyricsMode Picture";
                    break;
                case 3:
                    lyricsmode = " --LyricsMode Auto";
                    break;
                case 4:
                    lyricsmode = " --LyricsMode Hybrid";
                    break;
                default:
                    break;
            }
            paramWalkman = RefleshParamWalkman();
            textBox_cmd_walkman.Text = paramWalkman;
        }

        private void button_Linerpath_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = "Text file (*.txt)|*.txt;|JPEG Image (*.jpg)|*.jpg;|PNG Image (*.png)|*.png;|All Files (*.*)|*.*;",
                FilterIndex = 0,
                Title = "Open Liner Notes file",
                Multiselect = true,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                label_Linerpath.Text = ofd.FileName;
                linernotes = " --LinerNotes \"" + ofd.FileName + "\"";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                label_Linerpath.Text = "";
                linernotes = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void comboBox_Linermode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_Linermode.SelectedIndex)
            {
                case 0:
                    linernotesmode = " --LinerNotesMode Delete";
                    break;
                case 1:
                    linernotesmode = " --LinerNotesMode Text";
                    break;
                case 2:
                    linernotesmode = " --LinerNotesMode Picture";
                    break;
                case 3:
                    linernotesmode = " --LinerNotesMode Auto";
                    break;
                case 4:
                    linernotesmode = " --LinerNotesMode Hybrid";
                    break;
                default:
                    break;
            }
            paramWalkman = RefleshParamWalkman();
            textBox_cmd_walkman.Text = paramWalkman;
        }

        private void button_Jacketpath_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = "JPEG Image (*.jpg)|*.jpg;|PNG Image (*.png)|*.png;|All Files (*.*)|*.*;",
                FilterIndex = 0,
                Title = "Open Jacket image",
                Multiselect = true,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox_Jacket.ImageLocation = ofd.FileName;
                label_Jacketpath.Text = ofd.FileName;
                jacket = " --Jacket \"" + ofd.FileName + "\"";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                Utils.PictureboxImageDispose(pictureBox_Jacket);
                label_Jacketpath.Text = "";
                jacket = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void comboBox_Jacketmode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_Jacketmode.SelectedIndex)
            {
                case 0:
                    jacketmode = " --JacketMode Delete";
                    break;
                case 1:
                    jacketmode = " --JacketMode Text";
                    break;
                case 2:
                    jacketmode = " --JacketMode Picture";
                    break;
                case 3:
                    jacketmode = " --JacketMode Auto";
                    break;
                default:
                    break;
            }
            paramWalkman = RefleshParamWalkman();
            textBox_cmd_walkman.Text = paramWalkman;
        }

        // Tags

        private void textBox_Title_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Title.Text))
            {
                title = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                title = " --Title " + textBox_Title.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_SortTitle_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_SortTitle.Text))
            {
                sorttitle = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                sorttitle = " --SortTitle " + textBox_SortTitle.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_Subtitle_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Subtitle.Text))
            {
                subtitle = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                subtitle = " --Subtitle " + textBox_Subtitle.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_SortSubtitle_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_SortSubtitle.Text))
            {
                sortsubtitle = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                sortsubtitle = " --SortSubtitle " + textBox_SortSubtitle.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_Artist_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Artist.Text))
            {
                artist = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                artist = " --Artist " + textBox_Artist.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_SortArtist_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_SortArtist.Text))
            {
                sortartist = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                sortartist = " --SortArtist " + textBox_SortArtist.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_Album_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Album.Text))
            {
                album = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                album = " --Album " + textBox_Album.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_SortAlbum_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_SortAlbum.Text))
            {
                sortalbum = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                sortalbum = " --SortAlbum " + textBox_SortAlbum.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_AlbumArtist_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_AlbumArtist.Text))
            {
                albumartist = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                albumartist = " --AlbumArtist " + textBox_AlbumArtist.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_SortAlbumArtist_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_SortAlbumArtist.Text))
            {
                sortalbumartist = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                sortalbumartist = " --SortAlbumArtist " + textBox_SortAlbumArtist.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_Genre_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Genre.Text))
            {
                genre = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                genre = " --Genre " + textBox_Genre.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_Composer_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Composer.Text))
            {
                composer = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                composer = " --Composer " + textBox_Composer.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void textBox_Lyricist_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Lyricist.Text))
            {
                lyricist = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                lyricist = " --Lyricist " + textBox_Lyricist.Text;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void dateTimePicker_Release_ValueChanged(object sender, EventArgs e)
        {
            release = " --Release " + dateTimePicker_Release.Value.ToShortDateString();
            paramWalkman = RefleshParamWalkman();
            textBox_cmd_walkman.Text = paramWalkman;
        }

        private void dateTimePicker_Import_ValueChanged(object sender, EventArgs e)
        {
            import = " --Import " + dateTimePicker_Import.Value.ToShortDateString();
            paramWalkman = RefleshParamWalkman();
            textBox_cmd_walkman.Text = paramWalkman;
        }

        private void tabControl_Main_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == 1 && Setloopingpoint)
            {
                e.Cancel = true;
            }
        }
    }
}
