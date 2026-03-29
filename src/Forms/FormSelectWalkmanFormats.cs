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
                int idx = comboBox_OutputFormats.SelectedIndex;

                // 既存の Generic も更新（即時反映のため）
                Common.Generic.WalkmanMultiConvFmt = idx.ToString();

                // ★変換が参照しているのはココ（Config/Utils側）なので必ず更新する
                Common.Config.Entry["Walkman_EveryFmt_OutputFmt"].Value = idx.ToString();

                // ★FileType もここで揃えておく（traconv の --FileType に直結させる想定）
                // FormSettings の switch と同等のマッピング
                Common.Config.Entry["Walkman_FileType"].Value = idx switch
                {
                    0 => "PCM",
                    1 => "OMA3",
                    2 => "OMG3",
                    3 => "AAL3",
                    4 => "KDR3",
                    5 => "OMAP",
                    6 => "OMGP",
                    7 => "AALP",
                    8 => "KDRP",
                    _ => "OMA3",
                };

                // 必要なら拡張子もここで更新（あなたの既存設計に合わせて調整）
                Common.Generic.WalkmanMultiConvExt = idx switch
                {
                    2 => ".omg",
                    4 => ".kdr",
                    6 => ".omg",
                    8 => ".kdr",
                    _ => ".oma",
                };
            }
            else
            {
                int idx = comboBox_DecodeFormats.SelectedIndex;

                // 既存の Generic も更新
                Common.Generic.WalkmanMultiConvFmt = idx.ToString();

                // Decode 側も Config を更新
                Common.Config.Entry["Walkman_EveryFmt_DecodeFmt"].Value = idx.ToString();
            }

            // 設定を永続化（この Save/Load の有無で「次回起動」や「変換開始時のロード」に差が出ます）
            Common.Config.Save(Common.xmlpath);
            Common.Config.Load(Common.xmlpath);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormSelectWalkmanFormats_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain.DebugInfo("[FormSelectWalkmanFormats] Closed.");
        }
    }
}
