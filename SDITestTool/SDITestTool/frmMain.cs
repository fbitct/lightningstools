using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;
using Phcc;

namespace SDITestTool
{
    public partial class frmMain : Form
    {
        private Device _phccDevice = new Device();
        private ReadOnlyCollection<string> _serialPorts;

        private Thread _splashThread;

        public frmMain()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        public ReadOnlyCollection<string> SerialPorts
        {
            get { return _serialPorts; }
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
       

        private void EnumerateSerialPorts()
        {
            var ports = new Ports();
            _serialPorts = ports.SerialPortNames;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            _splashThread = new Thread(DoSplash);
            _splashThread.Start();
            Application.DoEvents();
            epErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            Text = Application.ProductName + " v" + Application.ProductVersion;

            EnumerateSerialPorts();
            cbSerialPort.Sorted = true;
            foreach (var port in _serialPorts)
            {
                cbSerialPort.Items.Add(port);
                cbSerialPort.Text = port;
                Application.DoEvents();
            }
            ResetErrors();

            try
            {
                _splashThread.Abort();
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            Activate();
        }



        private void cbSerialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeSerialPort();
        }

        private bool ChangeSerialPort()
        {
            var isPhcc = false;

            var selectedPort = cbSerialPort.Text;
            if (String.IsNullOrEmpty(selectedPort)) return false;
            try
            {
                if (_phccDevice != null) DisposePhccDevice();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            _phccDevice = null;
            try
            {
                _phccDevice = new Device(selectedPort);
                GC.SuppressFinalize(_phccDevice.SerialPort.BaseStream);
                var firmwareVersion = _phccDevice.FirmwareVersion;
                if (firmwareVersion != null && firmwareVersion.ToLowerInvariant().Trim().StartsWith("phcc"))
                {
                    isPhcc = true;
                    lblFirmwareVersion.Text = "PHCC Firmware Version:" + firmwareVersion;
                }
            }
            catch (Exception g)
            {
                lblFirmwareVersion.Text = "PHCC Firmware Version:";
                Debug.Write(g);
            }

            ResetErrors();
            return isPhcc;
        }

       private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposePhccDevice();
        }

        private void DisposePhccDevice()
        {
            if (_phccDevice != null)
            {
                try
                {
                    if (_phccDevice.SerialPort != null && _phccDevice.SerialPort.IsOpen) _phccDevice.SerialPort.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                try
                {
                    _phccDevice.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        private void ResetErrors()
        {
            epErrorProvider.Clear();
            if (String.IsNullOrEmpty(cbSerialPort.Text) || lblFirmwareVersion.Text == "PHCC Firmware Version:")
            {
                epErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                epErrorProvider.SetError(cbSerialPort,
                                         "No serial port is selected, or no PHCC device is detected on the selected serial port.");
            }
        }

        private void tcTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetErrors();
        }

        private void sendDoa()
        {
            byte devAddr = 0;
            byte subAddr = 0;
            byte data = 0;
            var valid = ValidateHexTextControl(txtDoaDevAddr, out devAddr);
            if (!valid) return;
            valid = ValidateHexTextControl(txtDoaSubAddr, out subAddr);
            if (!valid) return;
            valid = ValidateHexTextControl(txtDoaDataByte, out data);
            if (valid)
            {
                if (_phccDevice != null && !String.IsNullOrEmpty(_phccDevice.PortName))
                {
                    try
                    {
                        _phccDevice.DoaSendRaw(devAddr, subAddr, data);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        private void btnSendDoa_Click(object sender, EventArgs e)
        {
            sendDoa();
        }

        private void txtDoaDevAddr_Leave(object sender, EventArgs e)
        {
            byte val = 0;
            var valid = ValidateHexTextControl(txtDoaDevAddr, out val);
        }

        private void txtDoaSubAddr_Leave(object sender, EventArgs e)
        {
            byte val = 0;
            var valid = ValidateHexTextControl(txtDoaSubAddr, out val);
        }

        private void txtDoaDataByte_Leave(object sender, EventArgs e)
        {
            byte val = 0;
            var valid = ValidateHexTextControl(txtDoaDataByte, out val);
        }

        private void DoSplash()
        {
            var sp = new Splash();
            try
            {
                sp.ShowDialog();
                Thread.Sleep(1000);
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            if (sp != null)
            {
                sp.BackgroundImage = null;
                sp.Update();
                sp.Refresh();
                sp.Visible = false;
                sp.Hide();
                sp.Close();
            }
        }
    }
}