using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    internal sealed class FormNus3BankMultiEncode : Form
    {
        private readonly ComboBox comboBoxCodec;
        private readonly ComboBox comboBoxSamplingRate;
        private readonly ListView listViewStreams;
        private readonly TextBox textBoxStreamName;
        private readonly TextBox textBoxLoopStart;
        private readonly TextBox textBoxLoopEnd;
        private readonly Button buttonMoveUp;
        private readonly Button buttonMoveDown;
        private readonly Button buttonOk;
        private readonly Dictionary<Nus3BankEncodeStreamSetting, (int? LoopStart, int? LoopEnd)> originalLoopValues = [];
        private readonly Dictionary<Nus3BankEncodeStreamSetting, long> totalSamplesByStream = [];
        private readonly Dictionary<int, int> samplingRateByCodec = [];
        private bool updatingSelection;
        private int activeCodecIndex = -1;

        public FormNus3BankMultiEncode(
            IReadOnlyList<Nus3BankEncodeStreamSetting> streams,
            sbyte initialCodecFlag,
            int initialSamplingRate)
        {
            FormMain.DebugInfo($"[FormNus3BankMultiEncode] Initialized. streams={streams.Count}, initialCodecFlag={initialCodecFlag}, initialSamplingRate={initialSamplingRate}");
            Text = "NUSound Multi Encode";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            Size = new Size(880, 540);
            MinimumSize = new Size(720, 420);

            comboBoxCodec = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 180,
            };
            comboBoxCodec.Items.AddRange(["ATRAC3 / ATRAC3+", "ATRAC9"]);

            comboBoxSamplingRate = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 180,
            };

            samplingRateByCodec[0] = Utils.GetInt("ATRAC3_Console", (int)Constants.ATRAC3ConsoleType.PSP) == (int)Constants.ATRAC3ConsoleType.PS3
                ? 48000
                : 44100;
            int configuredAtrac9Rate = Utils.GetInt("ATRAC9_SamplingValue", 48000);
            samplingRateByCodec[1] = GetSupportedSamplingRates(codecIndex: 1).Contains(configuredAtrac9Rate)
                ? configuredAtrac9Rate
                : 48000;

            int initialCodecIndex = initialCodecFlag == 0 ? 0 : 1;
            if (GetSupportedSamplingRates(initialCodecIndex).Contains(initialSamplingRate))
                samplingRateByCodec[initialCodecIndex] = initialSamplingRate;

            comboBoxCodec.SelectedIndexChanged += ComboBoxCodec_SelectedIndexChanged;
            comboBoxCodec.SelectedIndex = initialCodecIndex;

            textBoxStreamName = new TextBox
            {
                Dock = DockStyle.Fill,
            };
            textBoxStreamName.TextChanged += TextBoxStreamName_TextChanged;

            textBoxLoopStart = new TextBox
            {
                Width = 120,
            };
            textBoxLoopStart.KeyPress += TextBoxLoopSample_KeyPress;
            textBoxLoopStart.TextChanged += TextBoxLoopStart_TextChanged;

            textBoxLoopEnd = new TextBox
            {
                Width = 120,
            };
            textBoxLoopEnd.KeyPress += TextBoxLoopSample_KeyPress;
            textBoxLoopEnd.TextChanged += TextBoxLoopEnd_TextChanged;

            buttonMoveUp = new Button
            {
                Text = "Up",
                Width = 76,
                Anchor = AnchorStyles.Right,
            };
            buttonMoveUp.Click += (_, _) => MoveSelectedItem(-1);

            buttonMoveDown = new Button
            {
                Text = "Down",
                Width = 76,
                Anchor = AnchorStyles.Right,
            };
            buttonMoveDown.Click += (_, _) => MoveSelectedItem(1);

            var editPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                ColumnCount = 6,
                RowCount = 2,
                Padding = new Padding(8, 8, 8, 4),
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            };
            editPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            editPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            editPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            editPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            editPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
            editPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));
            editPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
            editPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));

            editPanel.Controls.Add(new Label
            {
                Text = "Codec",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 8, 0),
            }, 0, 0);
            editPanel.Controls.Add(comboBoxCodec, 1, 0);
            editPanel.Controls.Add(new Label
            {
                Text = "Stream name",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(8, 6, 8, 0),
            }, 2, 0);
            editPanel.Controls.Add(textBoxStreamName, 3, 0);
            editPanel.Controls.Add(buttonMoveUp, 4, 0);
            editPanel.Controls.Add(buttonMoveDown, 5, 0);

            editPanel.Controls.Add(new Label
            {
                Text = "Sampling rate",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 8, 0),
            }, 0, 1);
            editPanel.Controls.Add(comboBoxSamplingRate, 1, 1);
            editPanel.Controls.Add(new Label
            {
                Text = "LoopStart",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(8, 6, 8, 0),
            }, 2, 1);
            editPanel.Controls.Add(textBoxLoopStart, 3, 1);
            editPanel.Controls.Add(new Label
            {
                Text = "LoopEnd",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(8, 6, 8, 0),
            }, 4, 1);
            editPanel.Controls.Add(textBoxLoopEnd, 5, 1);

            listViewStreams = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                MultiSelect = false,
            };
            listViewStreams.Columns.Add("#", 48, HorizontalAlignment.Right);
            listViewStreams.Columns.Add("Stream", 210, HorizontalAlignment.Left);
            listViewStreams.Columns.Add("LoopStart", 96, HorizontalAlignment.Right);
            listViewStreams.Columns.Add("LoopEnd", 96, HorizontalAlignment.Right);
            listViewStreams.Columns.Add("Source", 410, HorizontalAlignment.Left);
            listViewStreams.SelectedIndexChanged += ListViewStreams_SelectedIndexChanged;
            listViewStreams.Resize += (_, _) => ResizeColumns();

            AddStreamItems(streams);

            buttonOk = new Button
            {
                Text = "OK",
                Width = 96,
            };
            buttonOk.Click += ButtonOk_Click;

            var buttonCancel = new Button
            {
                Text = "Cancel",
                Width = 96,
            };
            buttonCancel.Click += ButtonCancel_Click;

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(8),
            };
            buttonPanel.Controls.Add(buttonCancel);
            buttonPanel.Controls.Add(buttonOk);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            layout.Controls.Add(editPanel, 0, 0);
            layout.Controls.Add(listViewStreams, 0, 1);
            layout.Controls.Add(buttonPanel, 0, 2);

            Controls.Add(layout);
            AcceptButton = buttonOk;
            CancelButton = buttonCancel;

            ModernUI.ModernTheme.Apply(this);
            UpdateEditControls();
        }

        public sbyte SelectedAtracFlag => comboBoxCodec.SelectedIndex == 0 ? (sbyte)0 : (sbyte)1;
        public int SelectedSamplingRate => comboBoxSamplingRate.SelectedItem is SamplingRateOption option
            ? option.Value
            : 48000;
        public List<Nus3BankEncodeStreamSetting> StreamSettings { get; } = [];

        private void ComboBoxCodec_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (activeCodecIndex >= 0 && comboBoxSamplingRate.SelectedItem is SamplingRateOption currentOption)
                samplingRateByCodec[activeCodecIndex] = currentOption.Value;

            activeCodecIndex = comboBoxCodec.SelectedIndex;
            int selectedRate = samplingRateByCodec.TryGetValue(activeCodecIndex, out int rememberedRate)
                ? rememberedRate
                : GetSupportedSamplingRates(activeCodecIndex)[0];

            comboBoxSamplingRate.BeginUpdate();
            try
            {
                comboBoxSamplingRate.Items.Clear();
                foreach (int samplingRate in GetSupportedSamplingRates(activeCodecIndex))
                    comboBoxSamplingRate.Items.Add(new SamplingRateOption(samplingRate));

                comboBoxSamplingRate.SelectedIndex = Math.Max(
                    0,
                    comboBoxSamplingRate.Items.Cast<SamplingRateOption>().ToList().FindIndex(option => option.Value == selectedRate));
            }
            finally
            {
                comboBoxSamplingRate.EndUpdate();
            }

            FormMain.DebugInfo($"[FormNus3BankMultiEncode] Codec changed. atracFlag={SelectedAtracFlag}, samplingRate={SelectedSamplingRate}");
        }

        private static int[] GetSupportedSamplingRates(int codecIndex)
        {
            return codecIndex == 0
                ? [44100, 48000]
                : [48000, 24000, 12000];
        }

        public List<Nus3BankEncodeStreamSetting> GetCurrentStreamSettings()
        {
            var settings = new List<Nus3BankEncodeStreamSetting>();

            foreach (ListViewItem item in listViewStreams.Items)
            {
                if (item.Tag is not Nus3BankEncodeStreamSetting setting)
                    continue;

                string streamName = string.IsNullOrWhiteSpace(setting.StreamName)
                    ? $"stream_{settings.Count:D4}"
                    : setting.StreamName.Trim();

                settings.Add(new Nus3BankEncodeStreamSetting
                {
                    SourceIndex = setting.SourceIndex,
                    StreamName = streamName,
                    LoopStart = setting.LoopStart,
                    LoopEnd = setting.LoopEnd,
                });
            }

            return settings;
        }

        private void AddStreamItems(IReadOnlyList<Nus3BankEncodeStreamSetting> streams)
        {
            for (int i = 0; i < streams.Count; i++)
            {
                var source = streams[i];
                var setting = new Nus3BankEncodeStreamSetting
                {
                    SourceIndex = source.SourceIndex,
                    StreamName = source.StreamName,
                    LoopStart = source.LoopStart,
                    LoopEnd = source.LoopEnd,
                };
                originalLoopValues[setting] = (source.LoopStart, source.LoopEnd);
                long? totalSamples = ResolveTotalSamples(setting.SourceIndex);
                if (totalSamples.HasValue)
                    totalSamplesByStream[setting] = totalSamples.Value;

                var item = new ListViewItem((i + 1).ToString())
                {
                    Tag = setting,
                };
                item.SubItems.Add(setting.StreamName);
                item.SubItems.Add(FormatLoopValue(setting.LoopStart));
                item.SubItems.Add(FormatLoopValue(setting.LoopEnd));
                item.SubItems.Add(ResolveSourcePath(setting.SourceIndex));
                listViewStreams.Items.Add(item);
            }

            if (listViewStreams.Items.Count > 0)
                listViewStreams.Items[0].Selected = true;

            ResizeColumns();
        }

        private void ListViewStreams_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateEditControls();
        }

        private void TextBoxStreamName_TextChanged(object? sender, EventArgs e)
        {
            if (updatingSelection || listViewStreams.SelectedItems.Count == 0)
                return;

            ListViewItem item = listViewStreams.SelectedItems[0];
            if (item.Tag is not Nus3BankEncodeStreamSetting setting)
                return;

            setting.StreamName = textBoxStreamName.Text;
            item.SubItems[1].Text = setting.StreamName;
        }

        private static void TextBoxLoopSample_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void TextBoxLoopStart_TextChanged(object? sender, EventArgs e)
        {
            UpdateSelectedLoopValue(textBoxLoopStart, subItemIndex: 2, isStart: true);
        }

        private void TextBoxLoopEnd_TextChanged(object? sender, EventArgs e)
        {
            UpdateSelectedLoopValue(textBoxLoopEnd, subItemIndex: 3, isStart: false);
        }

        private void UpdateSelectedLoopValue(TextBox textBox, int subItemIndex, bool isStart)
        {
            if (updatingSelection || listViewStreams.SelectedItems.Count == 0)
                return;

            ListViewItem item = listViewStreams.SelectedItems[0];
            if (item.Tag is not Nus3BankEncodeStreamSetting setting)
                return;

            string loopText = textBox.Text.Trim();
            int? value = null;
            string fieldName = isStart ? "LoopStart" : "LoopEnd";

            if (!string.IsNullOrWhiteSpace(loopText))
            {
                if (!long.TryParse(loopText, out long sampleValue))
                {
                    FormMain.DebugWarn($"[FormNus3BankMultiEncode] Invalid loop value. field={fieldName}, value={loopText}");
                    WarnAndResetLoopValue(item, setting, fieldName);
                    return;
                }

                if (totalSamplesByStream.TryGetValue(setting, out long totalSamples) &&
                    sampleValue > totalSamples)
                {
                    FormMain.DebugWarn($"[FormNus3BankMultiEncode] Loop value exceeds total samples. field={fieldName}, value={sampleValue}, totalSamples={totalSamples}");
                    WarnAndResetLoopValue(item, setting, fieldName, totalSamples);
                    return;
                }

                if (sampleValue > int.MaxValue)
                {
                    FormMain.DebugWarn($"[FormNus3BankMultiEncode] Loop value exceeds int range. field={fieldName}, value={sampleValue}");
                    WarnAndResetLoopValue(item, setting, fieldName);
                    return;
                }

                value = (int)sampleValue;
            }

            if (isStart)
                setting.LoopStart = value;
            else
                setting.LoopEnd = value;

            item.SubItems[subItemIndex].Text = FormatLoopValue(value);
        }

        private void WarnAndResetLoopValue(
            ListViewItem item,
            Nus3BankEncodeStreamSetting setting,
            string fieldName,
            long? totalSamples = null)
        {
            string message = totalSamples.HasValue
                ? $"Incorrect loop value.\r\n{fieldName} cannot be greater than total samples ({totalSamples.Value})."
                : $"Incorrect loop value.\r\n{fieldName} is too large.";

            MessageBox.Show(
                this,
                message,
                Localizable.Localization.MSGBoxErrorCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            ResetLoopValues(item, setting);
            item.Selected = true;
            item.Focused = true;
            item.EnsureVisible();
            UpdateEditControls();
        }

        private void ResetLoopValues(ListViewItem item, Nus3BankEncodeStreamSetting setting)
        {
            (int? loopStart, int? loopEnd) = originalLoopValues.TryGetValue(setting, out var originalLoop)
                ? originalLoop
                : (null, null);

            setting.LoopStart = loopStart;
            setting.LoopEnd = loopEnd;
            item.SubItems[2].Text = FormatLoopValue(setting.LoopStart);
            item.SubItems[3].Text = FormatLoopValue(setting.LoopEnd);
        }

        private void MoveSelectedItem(int direction)
        {
            if (listViewStreams.SelectedItems.Count == 0)
                return;

            int oldIndex = listViewStreams.SelectedItems[0].Index;
            int newIndex = oldIndex + direction;
            if (newIndex < 0 || newIndex >= listViewStreams.Items.Count)
                return;

            FormMain.DebugInfo($"[FormNus3BankMultiEncode] Move stream. from={oldIndex}, to={newIndex}");
            ListViewItem item = listViewStreams.SelectedItems[0];
            listViewStreams.Items.RemoveAt(oldIndex);
            listViewStreams.Items.Insert(newIndex, item);
            item.Selected = true;
            item.Focused = true;
            ReindexRows();
            UpdateEditControls();
        }

        private void ButtonOk_Click(object? sender, EventArgs e)
        {
            if (WarnAndResetInvalidLoopValues())
            {
                FormMain.DebugWarn("[FormNus3BankMultiEncode] OK blocked: invalid loop range.");
                return;
            }

            List<Nus3BankEncodeStreamSetting> currentSettings = GetCurrentStreamSettings();
            if (currentSettings.Any(setting => ContainsMultibyteCharacters(setting.StreamName)))
            {
                const string warningMessage = "Streamにマルチバイト文字(日本語、記号等)が含まれています。このままエンコードした場合、マルチバイト文字が含まれたStreamはtone_00**のようにリネームされます。続行しますか？";
                FormMain.DebugWarn("[FormNus3BankMultiEncode] Multibyte stream name detected. Confirmation requested.");

                DialogResult confirmation = MessageBox.Show(
                    this,
                    warningMessage,
                    Localizable.Localization.MSGBoxWarningCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (confirmation != DialogResult.Yes)
                {
                    FormMain.DebugWarn("[FormNus3BankMultiEncode] OK cancelled by multibyte stream name confirmation.");
                    return;
                }

                FormMain.DebugInfo("[FormNus3BankMultiEncode] Multibyte stream name encoding confirmed.");
            }

            StreamSettings.Clear();
            StreamSettings.AddRange(currentSettings);
            FormMain.DebugInfo($"[FormNus3BankMultiEncode] OK. streams={StreamSettings.Count}, atracFlag={SelectedAtracFlag}, samplingRate={SelectedSamplingRate}");

            DialogResult = DialogResult.OK;
            Close();
        }

        private static bool ContainsMultibyteCharacters(string text)
        {
            return text.Any(character => character > 0x7F);
        }

        private void ButtonCancel_Click(object? sender, EventArgs e)
        {
            WarnAndResetInvalidLoopValues();

            FormMain.DebugWarn("[FormNus3BankMultiEncode] Cancelled.");
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool WarnAndResetInvalidLoopValues()
        {
            ListViewItem? firstInvalidItem = null;

            foreach (ListViewItem item in listViewStreams.Items)
            {
                if (item.Tag is not Nus3BankEncodeStreamSetting setting ||
                    !setting.LoopStart.HasValue ||
                    !setting.LoopEnd.HasValue ||
                    setting.LoopStart.Value <= setting.LoopEnd.Value)
                {
                    continue;
                }

                firstInvalidItem ??= item;
                ResetLoopValues(item, setting);
            }

            if (firstInvalidItem is null)
                return false;

            FormMain.DebugWarn("[FormNus3BankMultiEncode] Invalid loop range detected.");
            MessageBox.Show(
                this,
                "Incorrect loop value.\r\nLoopStart cannot be greater than LoopEnd.",
                Localizable.Localization.MSGBoxErrorCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            listViewStreams.SelectedItems.Clear();
            firstInvalidItem.Selected = true;
            firstInvalidItem.Focused = true;
            firstInvalidItem.EnsureVisible();
            UpdateEditControls();
            return true;
        }

        private void UpdateEditControls()
        {
            updatingSelection = true;
            try
            {
                if (listViewStreams.SelectedItems.Count == 0 ||
                    listViewStreams.SelectedItems[0].Tag is not Nus3BankEncodeStreamSetting setting)
                {
                    textBoxStreamName.Text = string.Empty;
                    textBoxStreamName.Enabled = false;
                    textBoxLoopStart.Text = string.Empty;
                    textBoxLoopStart.Enabled = false;
                    textBoxLoopEnd.Text = string.Empty;
                    textBoxLoopEnd.Enabled = false;
                    buttonMoveUp.Enabled = false;
                    buttonMoveDown.Enabled = false;
                    return;
                }

                int index = listViewStreams.SelectedItems[0].Index;
                textBoxStreamName.Enabled = true;
                textBoxStreamName.Text = setting.StreamName;
                textBoxLoopStart.Enabled = true;
                textBoxLoopStart.Text = setting.LoopStart?.ToString() ?? string.Empty;
                textBoxLoopEnd.Enabled = true;
                textBoxLoopEnd.Text = setting.LoopEnd?.ToString() ?? string.Empty;
                buttonMoveUp.Enabled = index > 0;
                buttonMoveDown.Enabled = index < listViewStreams.Items.Count - 1;
            }
            finally
            {
                updatingSelection = false;
            }
        }

        private void ReindexRows()
        {
            for (int i = 0; i < listViewStreams.Items.Count; i++)
                listViewStreams.Items[i].SubItems[0].Text = (i + 1).ToString();
        }

        private void ResizeColumns()
        {
            if (listViewStreams.Columns.Count < 5)
                return;

            int available = Math.Max(500, listViewStreams.ClientSize.Width - 56);
            listViewStreams.Columns[0].Width = 48;
            listViewStreams.Columns[2].Width = 96;
            listViewStreams.Columns[3].Width = 96;
            listViewStreams.Columns[1].Width = Math.Min(240, Math.Max(160, available / 3));
            listViewStreams.Columns[4].Width = Math.Max(260, available - listViewStreams.Columns[1].Width - 192);
        }

        private static string FormatLoopValue(int? value)
        {
            return value.HasValue ? value.Value.ToString() : "-";
        }

        private static long? ResolveTotalSamples(int sourceIndex)
        {
            if (sourceIndex >= 0 && sourceIndex < Generic.InputJobs.Count)
            {
                InputJob job = Generic.InputJobs[sourceIndex];
                long? totalSamples = TryReadWaveTotalSamples(job.WorkPath);
                if (totalSamples.HasValue)
                    return totalSamples;

                if (Generic.pATRACOpenFilePaths is not null &&
                    sourceIndex < Generic.pATRACOpenFilePaths.Length)
                {
                    totalSamples = TryReadWaveTotalSamples(Generic.pATRACOpenFilePaths[sourceIndex]);
                    if (totalSamples.HasValue)
                        return totalSamples;
                }

                return TryReadWaveTotalSamples(job.OriginPath);
            }

            return null;
        }

        private static long? TryReadWaveTotalSamples(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;

            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new NAudio.Wave.WaveFileReader(stream);
                return reader.SampleCount;
            }
            catch
            {
                FormMain.DebugWarn($"[FormNus3BankMultiEncode] Failed to read WAV samples. path={path}");
                return null;
            }
        }

        private static string ResolveSourcePath(int sourceIndex)
        {
            if (sourceIndex >= 0 && sourceIndex < Generic.InputJobs.Count)
            {
                InputJob job = Generic.InputJobs[sourceIndex];
                return string.IsNullOrWhiteSpace(job.OriginPath) ? job.WorkPath : job.OriginPath;
            }

            return string.Empty;
        }

        private sealed class SamplingRateOption(int value)
        {
            public int Value { get; } = value;

            public override string ToString()
            {
                return $"{Value} Hz";
            }
        }
    }
}
