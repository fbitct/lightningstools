using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Phcc.DeviceManager.UI
{
    public partial class frmSelectServo : Form
    {
        public frmSelectServo()
        {
            InitializeComponent();
        }
        public int SelectedServo { get; set; }
        private void cmdOK_Click(object sender, EventArgs e)
        {
            this.SelectedServo = -1;
            if (rdoServo1.Checked)
            {
                this.SelectedServo = 1;
            }
            else if (rdoServo2.Checked)
            {
                this.SelectedServo = 2;
            }
            else if (rdoServo3.Checked)
            {
                this.SelectedServo = 3;
            }
            else if (rdoServo4.Checked)
            {
                this.SelectedServo = 4;
            }
            else if (rdoServo5.Checked)
            {
                this.SelectedServo = 5;
            }
            else if (rdoServo6.Checked)
            {
                this.SelectedServo = 6;
            }
            else if (rdoServo7.Checked) 
            {
                this.SelectedServo = 7;
            }
            else if (rdoServo8.Checked)
            {
                this.SelectedServo = 8;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
