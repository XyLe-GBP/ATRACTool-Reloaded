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
            if (_flag)
            {
                Common.Generic.WalkmanMultiConvFmt = comboBox_OutputFormats.SelectedIndex.ToString();
            }
            else
            {
                Common.Generic.WalkmanMultiConvFmt = comboBox_DecodeFormats.SelectedIndex.ToString();
            }
            
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
