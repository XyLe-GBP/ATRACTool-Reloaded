using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormSetWalkmanInformations : Form
    {
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

        bool iseveryfmt = false;

        public FormSetWalkmanInformations()
        {
            InitializeComponent();
        }

        private void FormSetWalkmanInformations_Load(object sender, EventArgs e)
        {
            Common.Config.Load(Common.xmlpath);

            iseveryfmt = bool.Parse(Config.Entry["Walkman_EveryFmt"].Value);
            if (iseveryfmt)
            {
                button_Cancel.Enabled = false;
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
                label_Linerpath.Text = Config.Entry["Walkman_LinerNotes"].Value.Replace(" --LinerNotes \"", "").Replace("\"", "");
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
        }

        private void button_OK_Click(object sender, EventArgs e)
        {

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

            Config.Entry["Walkman_Params"].Value = paramWalkman;

            Config.Save(xmlpath);

            Common.Utils.PictureboxImageDispose(pictureBox_Jacket);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private string RefleshParamWalkman()
        {
            return "traconv --Convert" + walkmanfmt + bitrates + title + sorttitle + subtitle + sortsubtitle + artist + sortartist + album + sortalbum + albumartist + sortalbumartist + genre + composer + lyricist + tracknumber + totaltracks + release + import + duration + milliseconds + lyrics + lyricsmode + linernotes + linernotesmode + jacket + jacketmode + " --Output $OutFile $InFile";
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
            if (iseveryfmt)
            {
                DialogResult dr = DialogResult.None;
                Thread thread = new(() =>
                {
                    dr = ofd.ShowDialog();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                if (dr == DialogResult.OK)
                {
                    pictureBox_Jacket.ImageLocation = ofd.FileName;
                    label_Jacketpath.Text = ofd.FileName;
                    jacket = " --Jacket \"" + ofd.FileName + "\"";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                }
                else
                {
                    Common.Utils.PictureboxImageDispose(pictureBox_Jacket);
                    label_Jacketpath.Text = "";
                    jacket = "";
                    paramWalkman = RefleshParamWalkman();
                    textBox_cmd_walkman.Text = paramWalkman;
                }
            }
            else if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox_Jacket.ImageLocation = ofd.FileName;
                label_Jacketpath.Text = ofd.FileName;
                jacket = " --Jacket \"" + ofd.FileName + "\"";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                Common.Utils.PictureboxImageDispose(pictureBox_Jacket);
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

        private void groupBox_walkman_others_Enter(object sender, EventArgs e)
        {

        }
    }
}
