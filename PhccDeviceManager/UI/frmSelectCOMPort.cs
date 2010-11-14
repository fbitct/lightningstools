using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace Phcc.DeviceManager.UI
{
    public partial class frmSelectCOMPort : Form
    {
        private List<string> _comPorts = new List<string>();
        private string _comPort = null;
        public frmSelectCOMPort()
        {
            InitializeComponent();
            _comPorts = EnumerateSerialPorts();
        }
        public string COMPort
        {
            get
            {
                return _comPort;
            }
            set
            {
                _comPort = value;

            }
        }
        private void SelectSuppliedComPortInList()
        {
            foreach (var item in cbComPort.Items)
            {
                if (item.ToString() == _comPort)
                {
                    cbComPort.SelectedItem = item;
                }
            }
        }
        private void UpdateComPort()
        {
            _comPort=cbComPort.SelectedItem != null ? cbComPort.SelectedItem.ToString() : null;
        }
        public List<string> COMPorts
        {
            get
            {
                return _comPorts;
            }
            set
            {
                _comPorts = value;
            }
        }
        private void frmSelectCOMPort_Load(object sender, EventArgs e)
        {
            AddComPortsToList(); 
            EnableDisableOKButton();
        }

        private void AddComPortsToList()
        {
            if (_comPorts == null) return;
            IComparer<string> comparer = new Common.Strings.NumericComparer();
            _comPorts.Sort(comparer);
            cbComPort.Items.Clear();
            foreach (string portName in _comPorts)
            {
                cbComPort.Items.Add(portName);
            }
            SelectSuppliedComPortInList();
        }
        private List<string>EnumerateSerialPorts()
        {
            Microsoft.VisualBasic.Devices.Ports ports = new Microsoft.VisualBasic.Devices.Ports();
            List<string> toReturn = new List<string>();
            foreach (var portName in ports.SerialPortNames)
            {
                toReturn.Add(portName);
            }
            return toReturn;
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void cbComPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableOKButton();
            UpdateComPort();
        }
        private void EnableDisableOKButton()
        {
            string selectedText = cbComPort.SelectedItem != null ? cbComPort.SelectedItem.ToString() : null;
            cmdOk.Enabled = !(string.IsNullOrEmpty(selectedText));
        }
    }
}
