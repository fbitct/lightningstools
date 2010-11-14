using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JoyMapper
{
    public partial class frmPPJoyDevicesToCreate : Form
    {
        private int _maxNumCreateableDevices = 16;
        private int _numExistingDevices = 0;

        public frmPPJoyDevicesToCreate()
        {
            InitializeComponent();
        }

        private void PPJoyDevicesToCreateForm_Load(object sender, EventArgs e)
        {
            _maxNumCreateableDevices = Util.GetMaxPPJoyVirtualDevicesAllowed();
            _numExistingDevices = Util.CountPPJoyVirtualDevices();
            lblNumExistingDevices.Text  = _numExistingDevices.ToString();
            udDevicesToCreate.Minimum = 1;
            udDevicesToCreate.Maximum = _maxNumCreateableDevices- _numExistingDevices;
            udDevicesToCreate.Value = 1;
        }
        public int NumDevicesToCreate
        {
            get
            {
                return Convert.ToInt32(udDevicesToCreate.Value);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
