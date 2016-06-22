﻿using System;
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
        private Device _pitchSdiDevice = new Device();
        private Device _rollSdiDevice = new Device();
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
            cbPitchDeviceSerialPort.Sorted = true;
            cbRollDeviceSerialPort.Sorted = true;
            foreach (var port in _serialPorts)
            {
                cbPitchDeviceSerialPort.Items.Add(port);
                cbPitchDeviceSerialPort.Text = port;
                cbRollDeviceSerialPort.Items.Add(port);
                cbRollDeviceSerialPort.Text = port;
                Application.DoEvents();
            }
            ChangePitchDeviceSerialPort();
            ChangeRollDeviceSerialPort();

        }

        private void EnumerateSerialPorts()
        {
            var ports = new Ports();
            _serialPorts = ports.SerialPortNames;
        }

        private void cbPitchDeviceSerialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangePitchDeviceSerialPort();
        }
        private void cbRollDeviceSerialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeRollDeviceSerialPort();
        }
        private void ChangePitchDeviceSerialPort()
        {
            ChangeSerialPort(cbPitchDeviceSerialPort, ref _pitchSdiDevice, lblPitchDeviceIdentification);
        }
        private void ChangeRollDeviceSerialPort()
        {
            ChangeSerialPort(cbRollDeviceSerialPort, ref _rollSdiDevice, lblRollDeviceIdentification);
        }
        private void ChangeSerialPort(ComboBox serialPortSelectionComboBox, ref Device sdiDevice, Label deviceIdentificationLabel)
        {
            var selectedPort = serialPortSelectionComboBox.Text;
            if (String.IsNullOrWhiteSpace(selectedPort)) return;
            try
            {
                if (sdiDevice != null) DisposeSDIDevice(ref sdiDevice);
            }
            catch (Exception ex)
            {
                _log.Debug(ex);
            }
            try
            {
                sdiDevice = new Device(selectedPort);
                var identification = sdiDevice.Identify().TrimEnd();
                if (!string.IsNullOrWhiteSpace(identification))
                {
                    deviceIdentificationLabel.Text = "Identification:" + identification;
                }
            }
            catch (Exception ex)
            {
#if (!DEBUG)
                DisposeSDIDevice(ref sdiDevice);
#endif
                deviceIdentificationLabel.Text = "Identification:";
                _log.Debug(ex);
            }
            ResetErrors();
        }

       private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeSDIDevice(ref _pitchSdiDevice);
            DisposeSDIDevice(ref _rollSdiDevice);
        }

        private void DisposeSDIDevice(ref Device sdiDevice)
        {
            if (sdiDevice != null)
            {
                try
                {
                    sdiDevice.Dispose();
                    sdiDevice = null;
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
            SetErrorsForSerialDeviceSelection(cbPitchDeviceSerialPort, lblPitchDeviceIdentification);
            SetErrorsForSerialDeviceSelection(cbRollDeviceSerialPort, lblRollDeviceIdentification);
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void SetErrorsForSerialDeviceSelection(ComboBox serialPortSelectionComboBox, Label deviceIdentificationLabel)
        {
            if (String.IsNullOrEmpty(serialPortSelectionComboBox.Text) || deviceIdentificationLabel.Text == "Identification:")
            {
                epErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                epErrorProvider.SetError(serialPortSelectionComboBox,
                                         "No serial port is selected, or no SDI device is detected on the selected serial port.");
            }
        }

        private void tcTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetErrors();
        }

        private void txtPitchSubAddr_Leave(object sender, EventArgs e)
        {
            byte val = 0;
            var valid = ValidateHexTextControl(txtPitchSubAddr, out val);
        }
        private void txtRollSubAddr_Leave(object sender, EventArgs e)
        {
            byte val = 0;
            var valid = ValidateHexTextControl(txtRollSubAddr, out val);
        }
        private void btnPitchSendRaw_Click(object sender, EventArgs e)
        {
            SendRaw(txtPitchSubAddr, txtPitchDataByte, _pitchSdiDevice, ()=>PitchDeviceIsValid);
        }
        private void btnRollSendRaw_Click(object sender, EventArgs e)
        {
            SendRaw(txtRollSubAddr, txtRollDataByte, _rollSdiDevice, ()=>RollDeviceIsValid);
        }
        private void SendRaw(TextBox subAddressTextBox, TextBox dataByteTextBox, Device sdiDevice, Func<bool> deviceValidationFunction)
        {
            byte subAddr = 0;
            byte data = 0;
            bool valid = ValidateHexTextControl(subAddressTextBox, out subAddr);
            if (!valid) return;
            valid = ValidateHexTextControl(dataByteTextBox, out data);
            if (valid)
            {
                if (deviceValidationFunction())
                {
                    try
                    {
                        sdiDevice.SendCommand((CommandSubaddress)subAddr, data);
                    }
                    catch (Exception ex)
                    {
                        _log.Debug(ex);
                    }
                }
            }
        }

        private void txtPitchDataByte_Leave(object sender, EventArgs e)
        {
            byte val = 0;
            var valid = ValidateHexTextControl(txtPitchDataByte, out val);
        }
        private void txtRollDataByte_Leave(object sender, EventArgs e)
        {
            byte val = 0;
            var valid = ValidateHexTextControl(txtRollDataByte, out val);
        }

        private void UpdateUIControlsEnabledOrDisabledState()
        {
            gbPitchRawDataControl.Enabled = PitchDeviceIsValid;
            gbRollRawDataControl.Enabled = RollDeviceIsValid;
        }
        private bool PitchDeviceIsValid
        {
            get
            {
                return _pitchSdiDevice != null && !string.IsNullOrWhiteSpace(_pitchSdiDevice.PortName);
            }
        }
        private bool RollDeviceIsValid
        {
            get
            {
                return _rollSdiDevice != null && !string.IsNullOrWhiteSpace(_rollSdiDevice.PortName);
            }
        }
        private bool IsPitch(string deviceIdentification)
        {
            return PitchDeviceIsValid &&
                    (
                        deviceIdentification.ToLowerInvariant().EndsWith("30") 
                            ||
                        deviceIdentification.ToLowerInvariant().EndsWith("48")
                    );
        }
        private bool IsRoll(string deviceIdentification)
        {
            return RollDeviceIsValid &&
                (
                    deviceIdentification.ToLowerInvariant().EndsWith("32")
                        ||
                    deviceIdentification.ToLowerInvariant().EndsWith("50")
                );
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