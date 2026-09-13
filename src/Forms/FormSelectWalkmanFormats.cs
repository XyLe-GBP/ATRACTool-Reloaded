using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATRACTool_Reloaded
{
    public partial class FormSelectWalkmanFormats : Form
    {
        private bool _flag = false;

        // Decode: false, Encode: true
        public FormSelectWalkmanFormats(bool flag)
        {
            InitializeComponent();
            ModernUI.ModernTheme.Apply(this);
            FormMain.DebugInfo("[FormSelectWalkmanFormats] Initialized.");

            if (flag)
            {
                this._flag = true;
                label_DecodeFmt.Enabled = false;
                label_DecodeFmt.Visible = false;
                label_OutputFmt.Enabled = true;
                comboBox_DecodeFormats.Enabled = false;
                comboBox_DecodeFormats.Visible = false;
                comboBox_OutputFormats.Enabled = true;
                comboBox_OutputFormats.Visible = true;
                comboBox_OutputFormats.SelectedIndex = 1;
            }
            else
            {
                this._flag = false;
                label_DecodeFmt.Enabled = true;
                label_OutputFmt.Enabled = false;
                label_OutputFmt.Visible = false;
                comboBox_DecodeFormats.Enabled = true;
                comboBox_DecodeFormats.Visible = true;
                comboBox_OutputFormats.Enabled = false;
                comboBox_OutputFormats.Visible = false;
                comboBox_DecodeFormats.SelectedIndex = 0;
            }
        }

        private void FormSelectWalkmanFormats_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // Decode: _flag == false（デコードフォーマット）
            if (_flag)
            {
                int idx = Common.Utils.NormalizeWalkmanOutputFormatIndex(comboBox_OutputFormats.SelectedIndex);
                FormMain.DebugInfo($"[FormSelectWalkmanFormats] Output format selected. index={idx}");

                // 既存の Generic も更新（即時反映のため）
                Common.Generic.WalkmanMultiConvFmt = idx.ToString();

                // ★変換が参照しているのはココ（Config/Utils側）なので必ず更新する
                Common.Config.Entry["Walkman_EveryFmt_OutputFmt"].Value = idx.ToString();

                Common.Config.Entry["Walkman_FileType"].Value = Common.Utils.GetWalkmanFileType(idx);
                Common.Generic.WalkmanMultiConvExt = Common.Utils.GetWalkmanExtension(idx);
                Common.Generic.WalkmanEveryFilter = Common.Utils.GetWalkmanSaveFilter(idx);
            }
            else
            {
                int idx = comboBox_DecodeFormats.SelectedIndex;
                FormMain.DebugInfo($"[FormSelectWalkmanFormats] Decode format selected. index={idx}");

                // 既存の Generic も更新
                Common.Generic.WalkmanMultiConvFmt = idx.ToString();

                // Decode 側も Config を更新
                Common.Config.Entry["Walkman_EveryFmt_DecodeFmt"].Value = idx.ToString();
            }

            // 設定を永続化（この Save/Load の有無で「次回起動」や「変換開始時のロード」に差が出ます）
            Common.Config.Save(Common.xmlpath);
            Common.Config.Load(Common.xmlpath);
            FormMain.DebugInfo($"[FormSelectWalkmanFormats] Selection saved. encodeMode={_flag}, format={Common.Generic.WalkmanMultiConvFmt}, ext={Common.Generic.WalkmanMultiConvExt}");

            DialogResult = DialogResult.OK;
            Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            FormMain.DebugWarn("[FormSelectWalkmanFormats] Cancelled.");
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormSelectWalkmanFormats_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain.DebugInfo("[FormSelectWalkmanFormats] Closed.");
        }
    }
}
