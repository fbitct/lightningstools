using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;
using SDI;
using log4net;

namespace ADITestTool
{
    public partial class frmMain : Form
    {
        private Device _sdiDevice = new Device();
        private ReadOnlyCollection<string> _serialPorts;
        private ILog _log = LogManager.GetLogger(typeof(frmMain));
        public frmMain()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        public ReadOnlyCollection<string> SerialPorts
        {
            get { return _serialPorts; }
        }



        private void frmMain_Load(object sender, EventArgs e)
        {
            epErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            Text = Application.ProductName + " v" + Application.ProductVersion;
            SetupSerialPorts();
            ResetErrors();
            Activate();
        }

        private void SetupSerialPorts()
        {
            EnumerateSerialPorts();
            cbSerialPort.Sorted = true;
            foreach (var port in _serialPorts)
            {
                cbSerialPort.Items.Add(port);
                cbSerialPort.Text = port;
                Application.DoEvents();
            }
            ChangeSerialPort();
        }

        private void EnumerateSerialPorts()
        {
            var ports = new Ports();
            _serialPorts = ports.SerialPortNames;
        }

        private void cbSerialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeSerialPort();
        }

        private void ChangeSerialPort()
        {
            var selectedPort = cbSerialPort.Text;
            if (String.IsNullOrWhiteSpace(selectedPort)) return;
            try
            {
                if (_sdiDevice != null) DisposeSDIDevice();
            }
            catch (Exception ex)
            {
                _log.Debug(ex);
            }
            try
            {
                _sdiDevice = new Device(selectedPort);
                var identification = _sdiDevice.Identify().TrimEnd();
                if (!string.IsNullOrWhiteSpace(identification))
                {
                    lblIdentification.Text = "Identification:" + identification;
                }
            }
            catch (Exception ex)
            {
#if (!DEBUG)
                DisposeSDIDevice();
#endif
                lblIdentification.Text = "Identification:";
                _log.Debug(ex);
            }
            ResetErrors();
        }

       private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeSDIDevice();
        }

        private void DisposeSDIDevice()
        {
            if (_sdiDevice != null)
            {
                try
                {
                    _sdiDevice.Dispose();
                    _sdiDevice = null;
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void ResetErrors()
        {
            epErrorProvider.Clear();
            if (String.IsNullOrEmpty(cbSerialPort.Text) || lblIdentification.Text == "Identification:")
            {
                epErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                epErrorProvider.SetError(cbSerialPort,
                                         "No serial port is selected, or no SDI device is detected on the selected serial port.");
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void tcTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetErrors();
        }

        private void txtSubAddr_Leave(object sender, EventArgs e)
        {
            byte val = 0;
            var valid = ValidateHexTextControl(txtSubAddr, out val);
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            byte subAddr = 0;
            byte data = 0;
            bool valid = ValidateHexTextControl(txtSubAddr, out subAddr);
            if (!valid) return;
            valid = ValidateHexTextControl(txtDataByte, out data);
            if (valid)
            {
                if (DeviceIsValid)
                {
                    try
                    {
                        _sdiDevice.SendCommand((CommandSubaddress)subAddr, data);
                    }
                    catch (Exception ex)
                    {
                        _log.Debug(ex);
                    }
                }
            }
        }

        private void txtDataByte_Leave(object sender, EventArgs e)
        {
            byte val = 0;
            var valid = ValidateHexTextControl(txtDataByte, out val);
        }


        private void UpdateUIControlsEnabledOrDisabledState()
        {
            gbRawDataControl.Enabled = DeviceIsValid;
        }
        private bool DeviceIsValid
        {
            get
            {
                return _sdiDevice != null && !string.IsNullOrWhiteSpace(_sdiDevice.PortName);
            }
        }
        private bool IsPitch
        {
            get
            {
                return DeviceIsValid &&
                    (
                        lblIdentification.Text.ToLowerInvariant().EndsWith("30") 
                            ||
                        lblIdentification.Text.ToLowerInvariant().EndsWith("48")
                    );
            }
        }
        private bool IsRoll
        {
            get
            {
                return DeviceIsValid &&
                    (
                        lblIdentification.Text.ToLowerInvariant().EndsWith("32")
                            ||
                        lblIdentification.Text.ToLowerInvariant().EndsWith("50")
                    );

            }
        }
        private bool ValidateHexTextControl(TextBox textControl, out byte val)
        {
            // First create an instance of the call stack   
            var callStack = new StackTrace();
            var frames = callStack.GetFrames();
            for (var i = 1; i < frames.Length; i++)
            {
                var frame = frames[i];
                var method = frame.GetMethod();
                // Get the declaring type and method names    
                var declaringType = method.DeclaringType.Name;
                var methodName = method.Name;
                if (methodName != null && methodName.Contains("ValidateHex"))
                {
                    val = 0x00;
                    return true;
                }
            }

            ResetErrors();
            var text = textControl.Text.Trim().ToUpperInvariant();
            textControl.Text = text;
            var parsed = false;
            if (text.StartsWith("0X"))
            {
                text = text.Substring(2, text.Length - 2);
                textControl.Text = "0x" + text;
                parsed = Byte.TryParse(text, NumberStyles.HexNumber, null, out val);
            }
            else
            {
                parsed = Byte.TryParse(text, out val);
            }
            if (!parsed)
            {
                epErrorProvider.SetError(textControl,
                                         "Invalid hexadecimal or decimal byte value.\nHex values should be preceded by the\ncharacters '0x' (zero, x) and\nshould be in the range 0x00-0xFF.\nDecimal values should be in the range 0-255.");
            }
            return parsed;
        }




    }
}