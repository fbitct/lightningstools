﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;
using SDI;
using log4net;

namespace SDITestTool
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
            SetupDefaultDemoParameters();
            SetupDefaultURCParameters();
            SetupDefaultMoveIndicatorInQuadrantParameters();
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
        private void SetupDefaultDemoParameters()
        {
            cboDemoMovementSpeed.SelectedIndex = 0;
            cboDemoMovementStepSize.SelectedIndex = 0;
            rdoModusStartToEndToStart.Checked = true;
        }
        private void SetupDefaultURCParameters()
        {
            cboURCSmoothModeSmoothUpdates.SelectedIndex = 0;
        }
        private void SetupDefaultMoveIndicatorInQuadrantParameters()
        {
            cboMoveIndicatorInQuadrant.SelectedIndex = 0;
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
                DisposeSDIDevice();
                lblIdentification.Text = "Identification:";
                _log.Debug(ex);
            }
            SetInitialBaseAngles();
            SetDefaultMovementLimits();
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

        private void rdoLEDAlwaysOff_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoLEDAlwaysOff.Checked && DeviceIsValid)
            {
                try
                {
                    _sdiDevice.ConfigureDiagnosticLEDBehavior(DiagnosticLEDMode.Off);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void rdoLEDAlwaysON_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoLEDAlwaysOn.Checked && DeviceIsValid)
            {
                try
                {
                    _sdiDevice.ConfigureDiagnosticLEDBehavior(DiagnosticLEDMode.On);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void rdoLEDFlashesAtHeartbeatRate_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoLEDFlashesAtHeartbeatRate.Checked && DeviceIsValid)
            {
                try
                {
                    _sdiDevice.ConfigureDiagnosticLEDBehavior(DiagnosticLEDMode.Heartbeat);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void rdoToggleLEDPerAcceptedCommand_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoToggleLEDPerAcceptedCommand.Checked && DeviceIsValid)
            {
                try
                {
                    _sdiDevice.ConfigureDiagnosticLEDBehavior(DiagnosticLEDMode.ToggleOnAcceptedCommand);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void btnDisableWatchdog_Click(object sender, EventArgs e)
        {
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.DisableWatchdog();
                    chkWatchdogEnabled.CheckState = CheckState.Unchecked;
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void chkWatchdogEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (chkWatchdogEnabled.CheckState == CheckState.Indeterminate)
            {
                return;
            }

            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.Watchdog(chkWatchdogEnabled.CheckState == CheckState.Checked, (byte)nudWatchdogCountdown.Value);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void nudWatchdogCountdown_ValueChanged(object sender, EventArgs e)
        {
            if (chkWatchdogEnabled.CheckState == CheckState.Indeterminate)
            {
                chkWatchdogEnabled.Checked = true;
            }

            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.Watchdog(chkWatchdogEnabled.CheckState == CheckState.Checked, (byte)nudWatchdogCountdown.Value);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }
        private void chkPowerDownEnabled_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePowerDownValues();
        }
        private void rdoPowerDownLevelFull_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoPowerDownLevelFull.Checked)
            {
                UpdatePowerDownValues();
            }
        }
        private void rdoPowerDownLevelHalf_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoPowerDownLevelHalf.Checked)
            {
                UpdatePowerDownValues();
            }
        }
        private void nudPowerDownDelay_ValueChanged(object sender, EventArgs e)
        {
            UpdatePowerDownValues();
        }
        private void UpdatePowerDownValues()
        {
            if (chkPowerDownEnabled.CheckState == CheckState.Indeterminate)
            {
                UpdateUIControlsEnabledOrDisabledState();
                return;
            }
            if (DeviceIsValid)
            {
                try
                {
                    var powerDownState = chkPowerDownEnabled.Checked ? PowerDownState.Enabled : PowerDownState.Disabled;
                    var powerDownLevel = rdoPowerDownLevelFull.Checked ? PowerDownLevel.Full : PowerDownLevel.Half;
                    var delayTimeMilliseconds = nudPowerDownDelay.Value;
                    _sdiDevice.ConfigurePowerDown(powerDownState, powerDownLevel, (short)delayTimeMilliseconds);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void nudStatorS1BaseAngle_ValueChanged(object sender, EventArgs e)
        {
            var offset = (short)nudStatorS1BaseAngle.Value;
            lblStatorS1BaseAngleDegrees.Text = string.Format("Degrees:{0}", Decimal.Round((decimal)(offset * 360.000 / 1024.000), 1).ToString());
            lblStatorS1BaseAngleHex.Text = string.Format("Hex:0x{0}", offset.ToString("X").PadLeft(3, '0'));
            lblStatorS1BaseAngleLSB.Text = string.Format("LSB:0x{0}", (offset & 0xFF).ToString("X").PadLeft(2,'0'));
            lblStatorS1BaseAngleMSB.Text = string.Format("MSB:0x{0}", ((offset & 0x300)>>8).ToString("X").PadLeft(2,'0'));
            btnUpdateStatorBaseAngles.Enabled = true;
        }

        private void nudStatorS2BaseAngle_ValueChanged(object sender, EventArgs e)
        {
            var offset = (short)nudStatorS2BaseAngle.Value;
            lblStatorS2BaseAngleDegrees.Text = string.Format("Degrees:{0}", Decimal.Round((decimal)(offset * 360.000 / 1024.000), 1).ToString());
            lblStatorS2BaseAngleHex.Text = string.Format("Hex:0x{0}", offset.ToString("X").PadLeft(3, '0'));
            lblStatorS2BaseAngleLSB.Text = string.Format("LSB:0x{0}", (offset & 0xFF).ToString("X").PadLeft(2, '0'));
            lblStatorS2BaseAngleMSB.Text = string.Format("MSB:0x{0}", ((offset & 0x300) >> 8).ToString("X").PadLeft(2, '0'));
            btnUpdateStatorBaseAngles.Enabled = true;
        }

        private void nudStatorS3BaseAngle_ValueChanged(object sender, EventArgs e)
        {
            var offset = (short)nudStatorS3BaseAngle.Value;
            lblStatorS3BaseAngleDegrees.Text = string.Format("Degrees:{0}", Decimal.Round((decimal)(offset * 360.000 / 1024.000), 1).ToString());
            lblStatorS3BaseAngleHex.Text = string.Format("Hex:0x{0}", offset.ToString("X").PadLeft(3, '0'));
            lblStatorS3BaseAngleLSB.Text = string.Format("LSB:0x{0}", (offset & 0xFF).ToString("X").PadLeft(2, '0'));
            lblStatorS3BaseAngleMSB.Text = string.Format("MSB:0x{0}", ((offset & 0x300) >> 8).ToString("X").PadLeft(2, '0'));
            btnUpdateStatorBaseAngles.Enabled = true;
        }
        private void UpdateUIControlsEnabledOrDisabledState()
        {
#if (!DEBUG)


            gbLED.Enabled = DeviceIsValid;
            gbUSBDebug.Enabled = DeviceIsValid;
            gbWatchdog.Enabled = DeviceIsValid;
            gbPowerDown.Enabled = DeviceIsValid;
            gbRawDataControl.Enabled = DeviceIsValid;
            gbStatorBaseAngles.Enabled = DeviceIsValid;
            gbDemo.Enabled = DeviceIsValid;
            gbMovementLimits.Enabled = DeviceIsValid;
            gbUpdateRateControl.Enabled = DeviceIsValid;
#endif
            rdoPowerDownLevelFull.Enabled = chkPowerDownEnabled.CheckState != CheckState.Indeterminate;
            rdoPowerDownLevelHalf.Enabled = chkPowerDownEnabled.CheckState != CheckState.Indeterminate;
            nudPowerDownDelay.Enabled = chkPowerDownEnabled.CheckState != CheckState.Indeterminate;

            lblURCLimitThreshold.Enabled = rdoURCLimitMode.Checked;
            lblURCLimitThresholdDegrees.Enabled = rdoURCLimitMode.Checked;
            lblURCLimitThresholdHex.Enabled = rdoURCLimitMode.Checked;
            nudURCLimitThresholdDecimal.Enabled = rdoURCLimitMode.Checked;

            lblURCSmoothModeThreshold.Enabled = rdoURCSmoothMode.Checked;
            lblURCSmoothModeThresholdDegrees.Enabled = rdoURCSmoothMode.Checked;
            lblURCSmoothModeThresholdHex.Enabled = rdoURCSmoothMode.Checked;
            nudURCSmoothModeThresholdDecimal.Enabled = rdoURCSmoothMode.Checked;
            lblURCSmoothModeSmoothUpdates.Enabled = rdoURCSmoothMode.Checked;
            cboURCSmoothModeSmoothUpdates.Enabled = rdoURCSmoothMode.Checked;
#if (!DEBUG)
            chkUpdateRateControlShortestPath.Enabled = IsPitch;
#endif

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
                return DeviceIsValid && lblIdentification.Text.ToLowerInvariant().EndsWith("30");
            }
        }
        private bool IsRoll
        {
            get 
            {
                return DeviceIsValid && lblIdentification.Text.ToLowerInvariant().EndsWith("32");
            }
        }
        private void SetInitialBaseAngles()
        {
            if (IsPitch)
            {
                nudStatorS1BaseAngle.Value = 682;
                nudStatorS2BaseAngle.Value = 0;
                nudStatorS3BaseAngle.Value = 341;
            }
            else if (IsRoll)
            {
                nudStatorS1BaseAngle.Value = 597;
                nudStatorS2BaseAngle.Value = 938;
                nudStatorS3BaseAngle.Value = 256;
            }
            btnUpdateStatorBaseAngles.Enabled = true;
        }
        private void SetDefaultMovementLimits()
        {
            if (IsPitch)
            {
                nudLimitMin.Value = 35;
                nudLimitMax.Value = 175;
            }
            else if (IsRoll)
            {
                nudLimitMin.Value = 0;
                nudLimitMax.Value = 255;
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

        private void btnUpdateStatorBaseAngles_Click(object sender, EventArgs e)
        {
            if (DeviceIsValid)
            {
                try
                {
                    var offset = (short)nudStatorS1BaseAngle.Value;
                    _sdiDevice.SetStatorBaseAngle(StatorSignals.S1, offset);
                    offset = (short)nudStatorS2BaseAngle.Value;
                    _sdiDevice.SetStatorBaseAngle(StatorSignals.S2, offset);
                    offset = (short)nudStatorS3BaseAngle.Value;
                    _sdiDevice.SetStatorBaseAngle(StatorSignals.S3, offset);
                    btnUpdateStatorBaseAngles.Enabled = false;
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void nudLimitMin_ValueChanged(object sender, EventArgs e)
        {
            var position = (byte)nudLimitMin.Value;
            lblLimitMinimumDegrees.Text =
                position > 0
                    ? string.Format("Degrees:{0}", Decimal.Round((decimal)(position * 360.000 / 255.000), 1).ToString())
                    : "N/A";
            lblLimitMinimumHex.Text = string.Format("Hex:0x{0}", position.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetIndicatorMovementLimitMinimum(position);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void nudLimitMax_ValueChanged(object sender, EventArgs e)
        {
            var position = (byte)nudLimitMax.Value;
            lblLimitMaximumDegrees.Text =
                position < 255
                    ? string.Format("Degrees:{0}", Decimal.Round((decimal)(position * 360.000 / 255.000), 1).ToString())
                    : "N/A";
            lblLimitMaximumHex.Text = string.Format("Hex:0x{0}", position.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetIndicatorMovementLimitMaximum(position);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void nudDemoStartPositionDecimal_ValueChanged(object sender, EventArgs e)
        {
            var position = (short)nudDemoStartPositionDecimal.Value;
            lblDemoStartPositionDegrees.Text = string.Format("Degrees:{0}", Decimal.Round((decimal)(position *360.000/ 255.000), 1).ToString());
            lblDemoStartPositionHex.Text = string.Format("Hex:0x{0}", position.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetDemoModeStartPosition((byte)position);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void nudDemoEndPositionDecimal_ValueChanged(object sender, EventArgs e)
        {
            var position = (short)nudDemoEndPositionDecimal.Value;
            lblDemoEndPositionDegrees.Text = string.Format("Degrees:{0}", Decimal.Round((decimal)(position *360.000/ 255.000), 1).ToString());
            lblDemoEndPositionHex.Text = string.Format("Hex:0x{0}", position.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetDemoModeEndPosition((byte)position);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }
        
        private void cboDemoMovementSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDemo();
        }

        private void cboDemoMovementStepSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDemo();
        }
        private void rdoModusStartToEndToStart_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoModusStartToEndToStart.Checked)
            {
                UpdateDemo();
            }
        }
        private void rdoDemoModusStartToEndJumpToStart_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoDemoModusStartToEndJumpToStart.Checked)
            {
                UpdateDemo();
            }
        }
        private void chkStartDemo_CheckedChanged(object sender, EventArgs e)
        {
            if (chkStartDemo.Checked)
            {
                chkStartDemo.Text = "Stop Demo";
            }
            else
            {
                chkStartDemo.Text = "Start Demo";
            }
            UpdateDemo();
        }
        private DemoMovementSpeeds SelectedDemoMovementSpeed
        {
            get
            {
                var selectedSpeedString = cboDemoMovementSpeed.SelectedItem as string;
                var demoMovementSpeed = DemoMovementSpeeds.x100ms;
                if (selectedSpeedString == "100 ms")
                {
                    demoMovementSpeed = DemoMovementSpeeds.x100ms;
                }
                else if (selectedSpeedString == "500 ms")
                {
                    demoMovementSpeed = DemoMovementSpeeds.x500ms;
                }
                else if (selectedSpeedString == "1 second")
                {
                    demoMovementSpeed = DemoMovementSpeeds.x1sec;
                }
                else if (selectedSpeedString == "2 seconds")
                {
                    demoMovementSpeed = DemoMovementSpeeds.x2sec;
                }
                return demoMovementSpeed;
            }
        }
        private byte SelectedDemoMovementStepSize
        {
            get
            {
                byte demoMovementStepSize = 1;
                var selectedDemoMovementStepSizeString = cboDemoMovementStepSize.SelectedItem as string;
                if (!string.IsNullOrWhiteSpace(selectedDemoMovementStepSizeString))
                {
                    byte.TryParse(selectedDemoMovementStepSizeString.Replace(" steps", ""), out demoMovementStepSize);
                }
               
                return demoMovementStepSize == 1 
                    ? demoMovementStepSize
                    :(byte)(demoMovementStepSize/2);

            }
        }
        private DemoModus SelectedDemoModus
        {
            get
            {
                return rdoDemoModusStartToEndJumpToStart.Checked
                    ? DemoModus.UpFromStartToEndThenRestart
                    : DemoModus.UpFromStartToEndThenDown;
            }
        }
      

        private void UpdateDemo()
        {
            UpdateUIControlsEnabledOrDisabledState();
            var speed = SelectedDemoMovementSpeed;
            var stepSize = SelectedDemoMovementStepSize;
            var modus = SelectedDemoModus;
            var start = chkStartDemo.Checked;
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.Demo(speed, stepSize, modus, start);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
        }

        private void nudURCLimitThresholdDecimal_ValueChanged(object sender, EventArgs e)
        {
            UpdateUrcLimitMode();
        }
        private void rdoURCLimitMode_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUrcLimitMode();
        }
        private void UpdateUrcLimitMode()
        {
            var threshold = (byte)nudURCLimitThresholdDecimal.Value;
            lblURCLimitThresholdDegrees.Text = string.Format("Degrees:{0}", Decimal.Round((decimal)((threshold+1) * 360.000 / 1024.000), 1).ToString());
            lblURCLimitThresholdHex.Text = string.Format("Hex:0x{0}", threshold.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetUpdateRateControlModeLimit(threshold);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }
        private void rdoURCSmoothMode_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUrcSmoothMode();
        }
        private void cboSmoothModeSmoothUpdates_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUrcSmoothMode();
        }
        private void nudURCSmoothModeThresholdDecimal_ValueChanged(object sender, EventArgs e)
        {
            UpdateUrcSmoothMode();
        }
        private UpdateRateControlSmoothingMode SmoothingMode
        {
            get
            {
                var selectedSmoothingMode = cboURCSmoothModeSmoothUpdates.SelectedItem as string;
                if (selectedSmoothingMode == "Adaptive")
                {
                    return UpdateRateControlSmoothingMode.Adaptive;
                }
                else if (selectedSmoothingMode == "2 steps")
                {
                    return UpdateRateControlSmoothingMode.TwoSteps;
                }
                else if (selectedSmoothingMode == "4 steps")
                {
                    return UpdateRateControlSmoothingMode.FourSteps;
                }
                else if (selectedSmoothingMode == "8 steps")
                {
                    return UpdateRateControlSmoothingMode.EightSteps;
                }
                return UpdateRateControlSmoothingMode.Adaptive;
            }
        }
        private void UpdateUrcSmoothMode()
        {
            var threshold = (byte)nudURCSmoothModeThresholdDecimal.Value;
            var thresholdRaw = (byte)((threshold - 4) / 4);
            lblURCSmoothModeThresholdDegrees.Text = string.Format("Degrees:{0}", Decimal.Round((decimal)(threshold * 360.000 / 1024.000), 1).ToString());
            lblURCSmoothModeThresholdHex.Text = string.Format("Hex:0x{0}", thresholdRaw.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetUpdateRateControlModeSmooth(thresholdRaw, SmoothingMode );
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void nudUpdateRateControlSpeed_ValueChanged(object sender, EventArgs e)
        {
            var stepUpdateDelayMillis = (ushort)nudUpdateRateControlSpeed.Value;
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetUpdateRateControlSpeed(stepUpdateDelayMillis);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void chkUpdateRateControlShortestPath_CheckedChanged(object sender, EventArgs e)
        {
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetUpdateRateControlMiscellaneous(chkUpdateRateControlShortestPath.Checked);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void chkUSBDebugEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.UsbDebug(chkUSBDebugEnabled.Checked);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void nudMoveIndicatorToPosition_ValueChanged(object sender, EventArgs e)
        {
            UpdateMoveIndicatorInQuadrant();
        }

        private void cboMoveIndicatorInQuadrant_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMoveIndicatorInQuadrant();
        }
        private void UpdateMoveIndicatorInQuadrant()
        {
            var position = (byte)nudMoveIndicatorToPositionDecimal.Value;
            var quadrant = (Quadrant)byte.Parse((string)cboMoveIndicatorInQuadrant.SelectedItem);
            lblMoveIndicatorToPositionDegrees.Text = string.Format("Degrees:{0}", Decimal.Round((decimal)(position * 90.000 / 255), 1).ToString());
            lblMoveIndicatorToPositionHex.Text = string.Format("Hex:0x{0}", position.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.MoveIndicatorFine(quadrant, position);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void nudMoveIndicatorCoarseResolutionDecimal_ValueChanged(object sender, EventArgs e)
        {
            var position = (byte)nudMoveIndicatorCoarseResolutionDecimal.Value;
            lblMoveIndicatorCoarseResolutionDegrees.Text = string.Format("Degrees:{0}", Decimal.Round((decimal)(position * 359.000 / 255), 1).ToString());
            lblMoveIndicatorCoarseResolutionHex.Text = string.Format("Hex:0x{0}", position.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.MoveIndicatorCoarse(position);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void nudS1AmplitudeImmediateDecimal_ValueChanged(object sender, EventArgs e)
        {
            var amplitude = (byte)nudS1AmplitudeImmediateDecimal.Value;
            lblS1AmplitudeImmediateHex.Text = string.Format("Hex:0x{0}", amplitude.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetStatorSignalAmplitudeImmediate(StatorSignals.S1, amplitude);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void nudS2AmplitudeImmediateDecimal_ValueChanged(object sender, EventArgs e)
        {
            var amplitude = (byte)nudS2AmplitudeImmediateDecimal.Value;
            lblS2AmplitudeImmediateHex.Text = string.Format("Hex:0x{0}", amplitude.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetStatorSignalAmplitudeImmediate(StatorSignals.S2, amplitude);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void nudS3AmplitudeImmediateDecimal_ValueChanged(object sender, EventArgs e)
        {
            var amplitude = (byte)nudS3AmplitudeImmediateDecimal.Value;
            lblS3AmplitudeImmediateHex.Text = string.Format("Hex:0x{0}", amplitude.ToString("X").PadLeft(2, '0'));
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetStatorSignalAmplitudeImmediate(StatorSignals.S3, amplitude);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void UpdatePolarityImmediate()
        {
            var s1Polarity = chkS1PolarityImmediate.Checked;
            var s2Polarity = chkS2PolarityImmediate.Checked;
            var s3Polarity = chkS3PolarityImmediate.Checked;
            var polarities = StatorSignals.Unknown;

            if (s1Polarity) polarities |= StatorSignals.S1;
            if (s2Polarity) polarities |= StatorSignals.S2;
            if (s3Polarity) polarities |= StatorSignals.S3;
            
            if (DeviceIsValid)
            {
                try
                {
                    _sdiDevice.SetStatorSignalsPolarityImmediate(polarities);
                }
                catch (Exception ex)
                {
                    _log.Debug(ex);
                }
            }
            UpdateUIControlsEnabledOrDisabledState();
        }

        private void chkS1PolarityImmediate_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePolarityImmediate();
        }

        private void chkS2PolarityImmediate_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePolarityImmediate();
        }

        private void chkS3PolarityImmediate_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePolarityImmediate();
        }
    }
}