using System.IO;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    public partial class FormWalkmanInformations : Form
    {
        private readonly InputJob _job;

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

        public FormWalkmanInformations(InputJob job)
        {
            InitializeComponent();
            ModernUI.ModernTheme.Apply(this);
            FormMain.DebugInfo("[FormWalkmanInformations] Initialized.");
            _job = job ?? throw new ArgumentNullException(nameof(job));
        }

        private void FormWalkmanInformations_Load(object sender, EventArgs e)
        {
            FormMain.DebugInfo($"[FormWalkmanInformations] Loaded. index={_job.Index}, origin={_job.OriginPath}, work={_job.WorkPath}");
            // This form edits the metadata already assigned to this input job.

            // 画面へ反映
            textBox_Title.Text = _job.Meta.Title ?? "";
            textBox_SortTitle.Text = _job.Meta.SortTitle ?? "";
            textBox_Subtitle.Text = _job.Meta.Subtitle ?? "";
            textBox_SortSubtitle.Text = _job.Meta.SortSubtitle ?? "";
            textBox_Artist.Text = _job.Meta.Artist ?? "";
            textBox_SortArtist.Text = _job.Meta.SortArtist ?? "";
            textBox_Album.Text = _job.Meta.Album ?? "";
            textBox_SortAlbum.Text = _job.Meta.SortAlbum ?? "";
            textBox_AlbumArtist.Text = _job.Meta.AlbumArtist ?? "";
            textBox_SortAlbumArtist.Text = _job.Meta.SortAlbumArtist ?? "";
            textBox_Genre.Text = _job.Meta.Genre ?? "";
            textBox_Composer.Text = _job.Meta.Composer ?? "";
            textBox_Lyricist.Text = _job.Meta.Lyricist ?? "";
            textBox_TrackNumber.Text = _job.Meta.TrackNumber ?? "";
            textBox_TotalTracks.Text = _job.Meta.TotalTracks ?? "";
            textBox_Duration.Text = _job.Meta.Duration ?? "";
            textBox_MilliSecond.Text = _job.Meta.MilliSecond ?? "";

            if (TryParseWalkmanDate(_job.Meta.ReleaseYear, out DateTime releaseDate))
            {
                dateTimePicker_Release.Value = releaseDate;
                dateTimePicker_Release.Checked = true;
            }
            else
            {
                // 未設定扱い
                dateTimePicker_Release.Value = new DateTime(DateTime.Now.Year, 1, 1);

                // ShowCheckBox を使っているなら未チェックで「未設定」を表現できる
                if (dateTimePicker_Release.ShowCheckBox)
                    dateTimePicker_Release.Checked = false;
            }

            if (TryParseWalkmanDate(_job.Meta.Import, out DateTime importDate))
            {
                dateTimePicker_Import.Value = importDate;
                dateTimePicker_Import.Checked = true;
            }
            else
            {
                // 未設定扱い
                dateTimePicker_Import.Value = new DateTime(DateTime.Now.Year, 1, 1);

                // ShowCheckBox を使っているなら未チェックで「未設定」を表現できる
                if (dateTimePicker_Import.ShowCheckBox)
                    dateTimePicker_Import.Checked = false;
            }

            // 任意：フォームのタイトル/ラベルに表示名
            this.Text = $"Walkman Metadata - {_job.DisplayName}";

            // ★追加：JacketPath → pictureBox/label に反映
            ApplyJacketToUIFromMeta();

            // The per-file dialog must preserve the same jacket mode used by
            // unattended conversion.  An extracted/selected image is explicit.
            comboBox_Jacketmode.SelectedIndex = JacketModeToSelectedIndex(
                string.IsNullOrWhiteSpace(_job.Meta.JacketPath)
                    ? _job.Meta.JacketMode
                    : "Picture");
            label_Lyricspath.Text = _job.Meta.LyricsPath ?? "";
            label_Linerpath.Text = _job.Meta.LinerNotesPath ?? "";
            comboBox_Lyricsmode.SelectedIndex = ContentModeToSelectedIndex(_job.Meta.LyricsMode);
            comboBox_Linermode.SelectedIndex = ContentModeToSelectedIndex(_job.Meta.LinerNotesMode);
            RefreshPathBackedCommandOptions();
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            SaveControlsToMeta(_job.Meta);
            if (!ValidateJacketSelection())
                return;

            Common.Utils.PictureboxImageDispose(pictureBox_Jacket);
            FormMain.DebugInfo($"[FormWalkmanInformations] OK. index={_job.Index}, title={_job.Meta.Title}, artist={_job.Meta.Artist}, jacket={_job.Meta.JacketPath}");
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            var dr = MessageBox.Show(this, Localizable.Localization.WalkmanMetadataConfirmCaption, Localizable.Localization.MSGBoxConfirmCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            FormMain.DebugInfo($"[FormWalkmanInformations] Cancel option selected. index={_job.Index}, dialogResult={dr}");

            if (dr == DialogResult.Cancel)
            {
                FormMain.DebugWarn($"[FormWalkmanInformations] Cancel dialog closed without decision. index={_job.Index}");
                return;
            }

            if (dr == DialogResult.No)
            {
                // 変換中止
                // ★ユーザー中止をジョブに記録
                _job.UserCancelled = true;
                FormMain.DebugWarn($"[FormWalkmanInformations] User cancelled conversion. index={_job.Index}");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            // Yes：タグ無しで続行（Meta 自体は差し替えず、中身だけ消す）
            _job.Meta.ClearTagFieldsKeepRequired();
            FormMain.DebugWarn($"[FormWalkmanInformations] Continue without tags. index={_job.Index}");

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ApplyJacketToUIFromMeta()
        {
            // 既存画像を解放（あなたの Common.Utils.PictureboxImageDispose がある前提）
            Common.Utils.PictureboxImageDispose(pictureBox_Jacket);

            string path = _job.Meta.JacketPath?.Trim() ?? "";
            label_Jacketpath.Text = path;
            if (path.Length == 0 || !System.IO.File.Exists(path))
            {
                jacket = "";
                return;
            }

            jacket = BuildStrOpt("--Jacket", path);

            // ★ファイルロック回避：ImageLocation を使わず、メモリに読み込んでから閉じる
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var img = Image.FromStream(fs))
            {
                pictureBox_Jacket.Image = new Bitmap(img);
            }
        }

        private void RefreshPathBackedCommandOptions()
        {
            lyrics = BuildStrOpt("--Lyrics", _job.Meta.LyricsPath);
            linernotes = BuildStrOpt("--LinerNotes", _job.Meta.LinerNotesPath);
            jacket = File.Exists(_job.Meta.JacketPath)
                ? BuildStrOpt("--Jacket", _job.Meta.JacketPath)
                : "";
            paramWalkman = RefleshParamWalkman();
            textBox_cmd_walkman.Text = paramWalkman;
        }

        private bool ValidateJacketSelection()
        {
            string path = _job.Meta.JacketPath?.Trim() ?? "";
            string mode = SelectedIndexToJacketMode(comboBox_Jacketmode.SelectedIndex);
            bool pathIsRequired = mode.Equals("Picture", StringComparison.OrdinalIgnoreCase);
            bool invalid = (pathIsRequired && path.Length == 0) ||
                (path.Length > 0 && !File.Exists(path));

            if (!invalid)
                return true;

            string message = Localizable.Localization.ResourceManager.GetString("WalkmanJacketPathErrorCaption")
                ?? "The jacket image path is empty or the file cannot be found.";
            MessageBox.Show(
                this,
                message,
                Localizable.Localization.MSGBoxErrorCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            FormMain.DebugWarn($"[FormWalkmanInformations] Invalid jacket path. index={_job.Index}, mode={mode}, path={path}");
            return false;
        }

        private string RefleshParamWalkman()
        {
            return "traconv --Convert" + walkmanfmt + bitrates + title + sorttitle + subtitle + sortsubtitle + artist + sortartist + album + sortalbum + albumartist + sortalbumartist + genre + composer + lyricist + tracknumber + totaltracks + release + import + duration + milliseconds + lyrics + lyricsmode + linernotes + linernotesmode + jacket + jacketmode + " --Output $OutFile $InFile";
        }

        private void TextBox_Bitrates_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Bitrates.Text))
            {
                bitrates = " --Bitrate -1";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                bitrates = BuildNumOpt("--Bitrate", textBox_Bitrates.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_Bitrates_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void TextBox_TrackNumber_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_TrackNumber.Text))
            {
                tracknumber = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                tracknumber = BuildNumOpt("--TrackNumber", textBox_TrackNumber.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_TotalTracks_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_TotalTracks.Text))
            {
                totaltracks = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                totaltracks = BuildNumOpt("--TotalTracks", textBox_TotalTracks.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_Duration_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Duration.Text))
            {
                duration = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                duration = BuildNumOpt("--Duration", textBox_Duration.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_MilliSecond_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_MilliSecond.Text))
            {
                milliseconds = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                milliseconds = BuildNumOpt("--MilliSecond", textBox_MilliSecond.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_TrackNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void TextBox_TotalTracks_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void TextBox_Duration_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void TextBox_MilliSecond_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void Button_Lyricspath_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = "Lyrics file (*.lrc)|*.lrc;|JPEG Image (*.jpg)|*.jpg;|PNG Image (*.png)|*.png;|All Files (*.*)|*.*;",
                FilterIndex = 1,
                Title = "Open Lyrics file",
                Multiselect = false,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                label_Lyricspath.Text = ofd.FileName;
                lyrics = " --Lyrics \"" + ofd.FileName + "\"";
                _job.Meta.LyricsPath = ofd.FileName;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void ComboBox_Lyricsmode_SelectedIndexChanged(object sender, EventArgs e)
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

        private void Button_Linerpath_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = "Text file (*.txt)|*.txt;|JPEG Image (*.jpg)|*.jpg;|PNG Image (*.png)|*.png;|All Files (*.*)|*.*;",
                FilterIndex = 1,
                Title = "Open Liner Notes file",
                Multiselect = false,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                label_Linerpath.Text = ofd.FileName;
                linernotes = " --LinerNotes \"" + ofd.FileName + "\"";
                _job.Meta.LinerNotesPath = ofd.FileName;
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void ComboBox_Linermode_SelectedIndexChanged(object sender, EventArgs e)
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

        private void Button_Jacketpath_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                FileName = "",
                InitialDirectory = "",
                Filter = "JPEG Image (*.jpg)|*.jpg;|PNG Image (*.png)|*.png;|All Files (*.*)|*.*;",
                FilterIndex = 1,
                Title = "Open Jacket image",
                Multiselect = false,
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                label_Jacketpath.Text = ofd.FileName;
                jacket = " --Jacket \"" + ofd.FileName + "\"";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;

                _job.Meta.JacketPath = ofd.FileName;
                _job.Meta.JacketMode = "Picture";
                comboBox_Jacketmode.SelectedIndex = JacketModeToSelectedIndex("Picture");

                ApplyJacketToUIFromMeta();
            }
        }

        private void ComboBox_Jacketmode_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMode = SelectedIndexToJacketMode(comboBox_Jacketmode.SelectedIndex);
            switch (selectedMode)
            {
                case "Delete":
                    jacketmode = " --JacketMode Delete";
                    break;
                case "Text":
                    jacketmode = " --JacketMode Text";
                    break;
                case "Picture":
                    jacketmode = " --JacketMode Picture";
                    break;
                case "Auto":
                    jacketmode = " --JacketMode Auto";
                    break;
                default:
                    break;
            }
            _job.Meta.JacketMode = selectedMode;
            paramWalkman = RefleshParamWalkman();
            textBox_cmd_walkman.Text = paramWalkman;
        }

        private static string SelectedIndexToJacketMode(int selectedIndex)
        {
            return selectedIndex switch
            {
                0 => "Delete",
                1 => "Text",
                2 => "Picture",
                _ => "Auto",
            };
        }

        private static int JacketModeToSelectedIndex(string? jacketMode)
        {
            return jacketMode?.Trim().ToUpperInvariant() switch
            {
                "DELETE" => 0,
                "TEXT" => 1,
                "PICTURE" => 2,
                _ => 3,
            };
        }

        private static int ContentModeToSelectedIndex(string? mode)
        {
            return mode?.Trim().ToUpperInvariant() switch
            {
                "DELETE" => 0,
                "TEXT" => 1,
                "PICTURE" => 2,
                "HYBRID" => 4,
                _ => 3,
            };
        }

        private static bool TryParseWalkmanDate(string? value, out DateTime date)
        {
            if (int.TryParse(value, out int year) && year is >= 1 and <= 9999)
            {
                date = new DateTime(year, 1, 1);
                return true;
            }

            return DateTime.TryParse(value, out date);
        }

        // Tags
        private void TextBox_Title_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Title.Text))
            {
                title = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                title = BuildStrOpt("--Title", textBox_Title.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_SortTitle_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_SortTitle.Text))
            {
                sorttitle = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                sorttitle = BuildStrOpt("--SortTitle", textBox_SortTitle.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_Subtitle_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Subtitle.Text))
            {
                subtitle = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                subtitle = BuildStrOpt("--Subtitle", textBox_Subtitle.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_SortSubtitle_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_SortSubtitle.Text))
            {
                sortsubtitle = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                sortsubtitle = BuildStrOpt("--SortSubtitle", textBox_SortSubtitle.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_Artist_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Artist.Text))
            {
                artist = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                artist = BuildStrOpt("--Artist", textBox_Artist.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_SortArtist_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_SortArtist.Text))
            {
                sortartist = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                sortartist = BuildStrOpt("--SortArtist", textBox_SortArtist.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_Album_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Album.Text))
            {
                album = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                album = BuildStrOpt("--Album", textBox_Album.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_SortAlbum_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_SortAlbum.Text))
            {
                sortalbum = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                sortalbum = BuildStrOpt("--SortAlbum", textBox_SortAlbum.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_AlbumArtist_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_AlbumArtist.Text))
            {
                albumartist = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                albumartist = BuildStrOpt("--AlbumArtist", textBox_AlbumArtist.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_SortAlbumArtist_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_SortAlbumArtist.Text))
            {
                sortalbumartist = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                sortalbumartist = BuildStrOpt("--SortAlbumArtist", textBox_SortAlbumArtist.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_Genre_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Genre.Text))
            {
                genre = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                genre = BuildStrOpt("--Genre", textBox_Genre.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_Composer_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Composer.Text))
            {
                composer = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                composer = BuildStrOpt("--Composer", textBox_Composer.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void TextBox_Lyricist_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_Lyricist.Text))
            {
                lyricist = "";
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
            else
            {
                lyricist = BuildStrOpt("--Lyricist", textBox_Lyricist.Text);
                paramWalkman = RefleshParamWalkman();
                textBox_cmd_walkman.Text = paramWalkman;
            }
        }

        private void groupBox_walkman_others_Enter(object sender, EventArgs e)
        {

        }

        private static string Q(string s)
        {
            // traconv へ渡す用：ダブルクォートはエスケープ
            return "\"" + (s ?? "").Replace("\"", "\\\"") + "\"";
        }

        private static string BuildStrOpt(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            return " " + key + " " + Q(value.Trim());
        }

        private static string BuildNumOpt(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            return " " + key + " " + value.Trim();
        }

        private void LoadMetaToControls(WalkmanMeta m)
        {
            textBox_Title.Text = m.Title ?? "";
            textBox_SortTitle.Text = m.SortTitle ?? "";
            textBox_Artist.Text = m.Artist ?? "";
            textBox_SortArtist.Text = m.SortArtist ?? "";
            textBox_Album.Text = m.Album ?? "";
            textBox_SortAlbum.Text = m.SortAlbum ?? "";
            textBox_AlbumArtist.Text = m.AlbumArtist ?? "";
            textBox_SortAlbumArtist.Text = m.SortAlbumArtist ?? "";
            textBox_Genre.Text = m.Genre ?? "";
            textBox_Composer.Text = m.Composer ?? "";
            textBox_Lyricist.Text = m.Lyricist ?? "";
            textBox_TrackNumber.Text = m.TrackNumber ?? "";
            textBox_TotalTracks.Text = m.TotalTracks ?? "";

            // DateTimePicker の場合（あなたの修正済みの系）
            if (DateTime.TryParse(m.Import, out var imp)) dateTimePicker_Import.Value = imp;
            if (int.TryParse(m.ReleaseYear, out var y)) dateTimePicker_Release.Value = new DateTime(y, 1, 1);

            pictureBox_Jacket.ImageLocation = m.JacketPath ?? "";
            //comboBox_FileType.Text = string.IsNullOrWhiteSpace(m.FileType) ? "OMA3" : m.FileType;
        }

        private void SaveControlsToMeta(WalkmanMeta m)
        {
            m.Title = textBox_Title.Text.Trim();
            m.SortTitle = textBox_SortTitle.Text.Trim();
            m.Subtitle = textBox_Subtitle.Text.Trim();
            m.SortSubtitle = textBox_SortSubtitle.Text.Trim();
            m.Artist = textBox_Artist.Text.Trim();
            m.SortArtist = textBox_SortArtist.Text.Trim();
            m.Album = textBox_Album.Text.Trim();
            m.SortAlbum = textBox_SortAlbum.Text.Trim();
            m.AlbumArtist = textBox_AlbumArtist.Text.Trim();
            m.SortAlbumArtist = textBox_SortAlbumArtist.Text.Trim();
            m.Genre = textBox_Genre.Text.Trim();
            m.Composer = textBox_Composer.Text.Trim();
            m.Lyricist = textBox_Lyricist.Text.Trim();
            m.TrackNumber = textBox_TrackNumber.Text.Trim();
            m.TotalTracks = textBox_TotalTracks.Text.Trim();
            m.Duration = textBox_Duration.Text.Trim();
            m.MilliSecond = textBox_MilliSecond.Text.Trim();

            m.Import = dateTimePicker_Import.ShowCheckBox && !dateTimePicker_Import.Checked
                ? ""
                : dateTimePicker_Import.Value.ToString("yyyy/MM/dd");
            m.ReleaseYear = dateTimePicker_Release.ShowCheckBox && !dateTimePicker_Release.Checked
                ? ""
                : dateTimePicker_Release.Value.ToString("yyyy/MM/dd");

            m.LyricsPath = label_Lyricspath.Text?.Trim() ?? "";
            m.LyricsMode = Common.Utils.GetWalkmanContentMode(comboBox_Lyricsmode.SelectedIndex);
            m.LinerNotesPath = label_Linerpath.Text?.Trim() ?? "";
            m.LinerNotesMode = Common.Utils.GetWalkmanContentMode(comboBox_Linermode.SelectedIndex);

            // JacketPath is updated when the file is loaded or selected. The
            // read-only path control is display-only, so long-path wrapping can
            // never alter the path passed to TraConv.
            m.JacketMode = SelectedIndexToJacketMode(comboBox_Jacketmode.SelectedIndex);

            // ★必須：FileType
            //m.FileType = comboBox_FileType.Text.Trim();
            if (string.IsNullOrWhiteSpace(m.FileType))
                m.FileType = Common.Utils.GetWalkmanFileType(Common.Utils.GetInt("Walkman_EveryFmt_OutputFmt", 1));
        }

        private void FormWalkmanInformations_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain.DebugInfo("[FormWalkmanInformations] Closed.");
        }
    }
}
