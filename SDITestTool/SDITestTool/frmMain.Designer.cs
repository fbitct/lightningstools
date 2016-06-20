namespace SDITestTool
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.lblDeviceAddress = new System.Windows.Forms.Label();
            this.epErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.gbRawDataControl = new System.Windows.Forms.GroupBox();
            this.lblSubAddr = new System.Windows.Forms.Label();
            this.txtSubAddr = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblDataByte = new System.Windows.Forms.Label();
            this.txtDataByte = new System.Windows.Forms.TextBox();
            this.lblSerialPort = new System.Windows.Forms.Label();
            this.cbSerialPort = new System.Windows.Forms.ComboBox();
            this.lblIdentification = new System.Windows.Forms.Label();
            this.gbLED = new System.Windows.Forms.GroupBox();
            this.rdoToggleLEDPerAcceptedCommand = new System.Windows.Forms.RadioButton();
            this.rdoLEDFlashesAtHeartbeatRate = new System.Windows.Forms.RadioButton();
            this.rdoLEDAlwaysOn = new System.Windows.Forms.RadioButton();
            this.rdoLEDAlwaysOff = new System.Windows.Forms.RadioButton();
            this.gbWatchdog = new System.Windows.Forms.GroupBox();
            this.lblCountdownDesc = new System.Windows.Forms.Label();
            this.lblWatchdogCountdown = new System.Windows.Forms.Label();
            this.nudWatchdogCountdown = new System.Windows.Forms.NumericUpDown();
            this.chkWatchdogEnabled = new System.Windows.Forms.CheckBox();
            this.btnDisableWatchdog = new System.Windows.Forms.Button();
            this.gbPowerDown = new System.Windows.Forms.GroupBox();
            this.lblDelayDescr = new System.Windows.Forms.Label();
            this.lblPowerDownDelayTime = new System.Windows.Forms.Label();
            this.nudPowerDownDelay = new System.Windows.Forms.NumericUpDown();
            this.gbPowerDownLevel = new System.Windows.Forms.GroupBox();
            this.rdoPowerDownLevelHalf = new System.Windows.Forms.RadioButton();
            this.rdoPowerDownLevelFull = new System.Windows.Forms.RadioButton();
            this.chkPowerDownEnabled = new System.Windows.Forms.CheckBox();
            this.gbUSBDebug = new System.Windows.Forms.GroupBox();
            this.chkUSBDebugEnabled = new System.Windows.Forms.CheckBox();
            this.gbDemo = new System.Windows.Forms.GroupBox();
            this.gbDemoSpeedAndStepping = new System.Windows.Forms.GroupBox();
            this.lblDemoMovementSpeed = new System.Windows.Forms.Label();
            this.cboDemoMovementStepSize = new System.Windows.Forms.ComboBox();
            this.cboDemoMovementSpeed = new System.Windows.Forms.ComboBox();
            this.lblDemoMovementStepSizeDesc = new System.Windows.Forms.Label();
            this.lblDemoMovementStepSize = new System.Windows.Forms.Label();
            this.lblDemoMovementSpeedDesc = new System.Windows.Forms.Label();
            this.gbModus = new System.Windows.Forms.GroupBox();
            this.rdoDemoModusStartToEndJumpToStart = new System.Windows.Forms.RadioButton();
            this.rdoModusStartToEndToStart = new System.Windows.Forms.RadioButton();
            this.gbDemoStartAndEndPositions = new System.Windows.Forms.GroupBox();
            this.lblDemoEndPositionHex = new System.Windows.Forms.Label();
            this.lblDemoStartPositionHex = new System.Windows.Forms.Label();
            this.lblDemoEndPositionDegrees = new System.Windows.Forms.Label();
            this.lblDemoStartPositionDegrees = new System.Windows.Forms.Label();
            this.nudDemoEndPositionDecimal = new System.Windows.Forms.NumericUpDown();
            this.lblDemoEndPositionDecimal = new System.Windows.Forms.Label();
            this.nudDemoStartPositionDecimal = new System.Windows.Forms.NumericUpDown();
            this.lblDemoStartPosition = new System.Windows.Forms.Label();
            this.gbStatorBaseAngles = new System.Windows.Forms.GroupBox();
            this.btnUpdateStatorBaseAngles = new System.Windows.Forms.Button();
            this.lblStatorS3BaseAngleHex = new System.Windows.Forms.Label();
            this.lblStatorS2BaseAngleHex = new System.Windows.Forms.Label();
            this.lblStatorS1BaseAngleHex = new System.Windows.Forms.Label();
            this.lblStatorS3BaseAngleDegrees = new System.Windows.Forms.Label();
            this.lblStatorS2BaseAngleDegrees = new System.Windows.Forms.Label();
            this.lblStatorS1BaseAngleDegrees = new System.Windows.Forms.Label();
            this.lblStatorS3BaseAngleMSB = new System.Windows.Forms.Label();
            this.lblStatorS2BaseAngleMSB = new System.Windows.Forms.Label();
            this.lblStatorS1BaseAngleMSB = new System.Windows.Forms.Label();
            this.lblStatorS3BaseAngleLSB = new System.Windows.Forms.Label();
            this.lblStatorS2BaseAngleLSB = new System.Windows.Forms.Label();
            this.lblStatorS1BaseAngleLSB = new System.Windows.Forms.Label();
            this.nudStatorS3BaseAngle = new System.Windows.Forms.NumericUpDown();
            this.lblStatorS3BaseAngle = new System.Windows.Forms.Label();
            this.nudStatorS2BaseAngle = new System.Windows.Forms.NumericUpDown();
            this.lblStatorS2BaseAngle = new System.Windows.Forms.Label();
            this.nudStatorS1BaseAngle = new System.Windows.Forms.NumericUpDown();
            this.lblStatorS1BaseAngle = new System.Windows.Forms.Label();
            this.gbMovementLimits = new System.Windows.Forms.GroupBox();
            this.lblLimitMaximumHex = new System.Windows.Forms.Label();
            this.lblLimitMaximumDegrees = new System.Windows.Forms.Label();
            this.lblLimitMinimumHex = new System.Windows.Forms.Label();
            this.lblLimitMinimumDegrees = new System.Windows.Forms.Label();
            this.lblLimitMinDesc = new System.Windows.Forms.Label();
            this.lblLimitMaxDesc = new System.Windows.Forms.Label();
            this.nudLimitMax = new System.Windows.Forms.NumericUpDown();
            this.lblLimitMax = new System.Windows.Forms.Label();
            this.nudLimitMin = new System.Windows.Forms.NumericUpDown();
            this.lblLimitMin = new System.Windows.Forms.Label();
            this.chkStartDemo = new System.Windows.Forms.CheckBox();
            this.gbMain = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabDeviceSetup = new System.Windows.Forms.TabPage();
            this.tabRawData = new System.Windows.Forms.TabPage();
            this.tabSynchroSetup = new System.Windows.Forms.TabPage();
            this.tabDemoMode = new System.Windows.Forms.TabPage();
            this.tabSynchroControl = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.epErrorProvider)).BeginInit();
            this.gbRawDataControl.SuspendLayout();
            this.gbLED.SuspendLayout();
            this.gbWatchdog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudWatchdogCountdown)).BeginInit();
            this.gbPowerDown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPowerDownDelay)).BeginInit();
            this.gbPowerDownLevel.SuspendLayout();
            this.gbUSBDebug.SuspendLayout();
            this.gbDemo.SuspendLayout();
            this.gbDemoSpeedAndStepping.SuspendLayout();
            this.gbModus.SuspendLayout();
            this.gbDemoStartAndEndPositions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDemoEndPositionDecimal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDemoStartPositionDecimal)).BeginInit();
            this.gbStatorBaseAngles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStatorS3BaseAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStatorS2BaseAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStatorS1BaseAngle)).BeginInit();
            this.gbMovementLimits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLimitMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLimitMin)).BeginInit();
            this.gbMain.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabDeviceSetup.SuspendLayout();
            this.tabRawData.SuspendLayout();
            this.tabSynchroSetup.SuspendLayout();
            this.tabDemoMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDeviceAddress
            // 
            this.lblDeviceAddress.Location = new System.Drawing.Point(0, 0);
            this.lblDeviceAddress.Name = "lblDeviceAddress";
            this.lblDeviceAddress.Size = new System.Drawing.Size(100, 23);
            this.lblDeviceAddress.TabIndex = 0;
            // 
            // epErrorProvider
            // 
            this.epErrorProvider.ContainerControl = this;
            // 
            // gbRawDataControl
            // 
            this.gbRawDataControl.Controls.Add(this.lblSubAddr);
            this.gbRawDataControl.Controls.Add(this.txtSubAddr);
            this.gbRawDataControl.Controls.Add(this.btnSend);
            this.gbRawDataControl.Controls.Add(this.lblDataByte);
            this.gbRawDataControl.Controls.Add(this.txtDataByte);
            this.gbRawDataControl.Location = new System.Drawing.Point(6, 6);
            this.gbRawDataControl.Margin = new System.Windows.Forms.Padding(6);
            this.gbRawDataControl.Name = "gbRawDataControl";
            this.gbRawDataControl.Padding = new System.Windows.Forms.Padding(6);
            this.gbRawDataControl.Size = new System.Drawing.Size(304, 293);
            this.gbRawDataControl.TabIndex = 0;
            this.gbRawDataControl.TabStop = false;
            this.gbRawDataControl.Text = "Raw Data Control";
            // 
            // lblSubAddr
            // 
            this.lblSubAddr.AutoSize = true;
            this.lblSubAddr.Location = new System.Drawing.Point(37, 118);
            this.lblSubAddr.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblSubAddr.Name = "lblSubAddr";
            this.lblSubAddr.Size = new System.Drawing.Size(133, 25);
            this.lblSubAddr.TabIndex = 2;
            this.lblSubAddr.Text = "S&ubaddress:";
            this.lblSubAddr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSubAddr
            // 
            this.txtSubAddr.Location = new System.Drawing.Point(181, 112);
            this.txtSubAddr.Margin = new System.Windows.Forms.Padding(6);
            this.txtSubAddr.MaxLength = 4;
            this.txtSubAddr.Name = "txtSubAddr";
            this.txtSubAddr.Size = new System.Drawing.Size(88, 31);
            this.txtSubAddr.TabIndex = 3;
            this.txtSubAddr.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtSubAddr.Leave += new System.EventHandler(this.txtSubAddr_Leave);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(42, 224);
            this.btnSend.Margin = new System.Windows.Forms.Padding(6);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(227, 44);
            this.btnSend.TabIndex = 6;
            this.btnSend.Text = "&Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblDataByte
            // 
            this.lblDataByte.AutoSize = true;
            this.lblDataByte.Location = new System.Drawing.Point(55, 168);
            this.lblDataByte.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblDataByte.Name = "lblDataByte";
            this.lblDataByte.Size = new System.Drawing.Size(112, 25);
            this.lblDataByte.TabIndex = 4;
            this.lblDataByte.Text = "Data &Byte:";
            this.lblDataByte.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDataByte
            // 
            this.txtDataByte.Location = new System.Drawing.Point(181, 162);
            this.txtDataByte.Margin = new System.Windows.Forms.Padding(6);
            this.txtDataByte.MaxLength = 4;
            this.txtDataByte.Name = "txtDataByte";
            this.txtDataByte.Size = new System.Drawing.Size(88, 31);
            this.txtDataByte.TabIndex = 5;
            this.txtDataByte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDataByte.Leave += new System.EventHandler(this.txtDataByte_Leave);
            // 
            // lblSerialPort
            // 
            this.lblSerialPort.AutoSize = true;
            this.lblSerialPort.Location = new System.Drawing.Point(10, 17);
            this.lblSerialPort.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblSerialPort.Name = "lblSerialPort";
            this.lblSerialPort.Size = new System.Drawing.Size(118, 25);
            this.lblSerialPort.TabIndex = 5;
            this.lblSerialPort.Text = "Serial &Port:";
            // 
            // cbSerialPort
            // 
            this.cbSerialPort.FormattingEnabled = true;
            this.cbSerialPort.Location = new System.Drawing.Point(140, 13);
            this.cbSerialPort.Margin = new System.Windows.Forms.Padding(6);
            this.cbSerialPort.Name = "cbSerialPort";
            this.cbSerialPort.Size = new System.Drawing.Size(238, 33);
            this.cbSerialPort.TabIndex = 4;
            // 
            // lblIdentification
            // 
            this.lblIdentification.AutoSize = true;
            this.lblIdentification.Location = new System.Drawing.Point(432, 17);
            this.lblIdentification.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblIdentification.Name = "lblIdentification";
            this.lblIdentification.Size = new System.Drawing.Size(139, 25);
            this.lblIdentification.TabIndex = 6;
            this.lblIdentification.Text = "Identification:";
            // 
            // gbLED
            // 
            this.gbLED.Controls.Add(this.rdoToggleLEDPerAcceptedCommand);
            this.gbLED.Controls.Add(this.rdoLEDFlashesAtHeartbeatRate);
            this.gbLED.Controls.Add(this.rdoLEDAlwaysOn);
            this.gbLED.Controls.Add(this.rdoLEDAlwaysOff);
            this.gbLED.Location = new System.Drawing.Point(6, 6);
            this.gbLED.Name = "gbLED";
            this.gbLED.Size = new System.Drawing.Size(489, 188);
            this.gbLED.TabIndex = 7;
            this.gbLED.TabStop = false;
            this.gbLED.Text = "Onboard Diagnostic LED";
            // 
            // rdoToggleLEDPerAcceptedCommand
            // 
            this.rdoToggleLEDPerAcceptedCommand.AutoSize = true;
            this.rdoToggleLEDPerAcceptedCommand.Location = new System.Drawing.Point(6, 135);
            this.rdoToggleLEDPerAcceptedCommand.Name = "rdoToggleLEDPerAcceptedCommand";
            this.rdoToggleLEDPerAcceptedCommand.Size = new System.Drawing.Size(471, 29);
            this.rdoToggleLEDPerAcceptedCommand.TabIndex = 11;
            this.rdoToggleLEDPerAcceptedCommand.TabStop = true;
            this.rdoToggleLEDPerAcceptedCommand.Text = "Toggle LED ON/OFF per accepted command";
            this.rdoToggleLEDPerAcceptedCommand.UseVisualStyleBackColor = true;
            this.rdoToggleLEDPerAcceptedCommand.CheckedChanged += new System.EventHandler(this.rdoToggleLEDPerAcceptedCommand_CheckedChanged);
            // 
            // rdoLEDFlashesAtHeartbeatRate
            // 
            this.rdoLEDFlashesAtHeartbeatRate.AutoSize = true;
            this.rdoLEDFlashesAtHeartbeatRate.Location = new System.Drawing.Point(6, 100);
            this.rdoLEDFlashesAtHeartbeatRate.Name = "rdoLEDFlashesAtHeartbeatRate";
            this.rdoLEDFlashesAtHeartbeatRate.Size = new System.Drawing.Size(318, 29);
            this.rdoLEDFlashesAtHeartbeatRate.TabIndex = 10;
            this.rdoLEDFlashesAtHeartbeatRate.TabStop = true;
            this.rdoLEDFlashesAtHeartbeatRate.Text = "Flash LED at Heartbeat Rate";
            this.rdoLEDFlashesAtHeartbeatRate.UseVisualStyleBackColor = true;
            this.rdoLEDFlashesAtHeartbeatRate.CheckedChanged += new System.EventHandler(this.rdoLEDFlashesAtHeartbeatRate_CheckedChanged);
            // 
            // rdoLEDAlwaysOn
            // 
            this.rdoLEDAlwaysOn.AutoSize = true;
            this.rdoLEDAlwaysOn.Location = new System.Drawing.Point(6, 65);
            this.rdoLEDAlwaysOn.Name = "rdoLEDAlwaysOn";
            this.rdoLEDAlwaysOn.Size = new System.Drawing.Size(195, 29);
            this.rdoLEDAlwaysOn.TabIndex = 9;
            this.rdoLEDAlwaysOn.TabStop = true;
            this.rdoLEDAlwaysOn.Text = "LED Always ON";
            this.rdoLEDAlwaysOn.UseVisualStyleBackColor = true;
            this.rdoLEDAlwaysOn.CheckedChanged += new System.EventHandler(this.rdoLEDAlwaysON_CheckedChanged);
            // 
            // rdoLEDAlwaysOff
            // 
            this.rdoLEDAlwaysOff.AutoSize = true;
            this.rdoLEDAlwaysOff.Location = new System.Drawing.Point(6, 30);
            this.rdoLEDAlwaysOff.Name = "rdoLEDAlwaysOff";
            this.rdoLEDAlwaysOff.Size = new System.Drawing.Size(206, 29);
            this.rdoLEDAlwaysOff.TabIndex = 8;
            this.rdoLEDAlwaysOff.TabStop = true;
            this.rdoLEDAlwaysOff.Text = "LED Always OFF";
            this.rdoLEDAlwaysOff.UseVisualStyleBackColor = true;
            this.rdoLEDAlwaysOff.CheckedChanged += new System.EventHandler(this.rdoLEDAlwaysOff_CheckedChanged);
            // 
            // gbWatchdog
            // 
            this.gbWatchdog.Controls.Add(this.lblCountdownDesc);
            this.gbWatchdog.Controls.Add(this.lblWatchdogCountdown);
            this.gbWatchdog.Controls.Add(this.nudWatchdogCountdown);
            this.gbWatchdog.Controls.Add(this.chkWatchdogEnabled);
            this.gbWatchdog.Controls.Add(this.btnDisableWatchdog);
            this.gbWatchdog.Location = new System.Drawing.Point(319, 89);
            this.gbWatchdog.Name = "gbWatchdog";
            this.gbWatchdog.Size = new System.Drawing.Size(372, 210);
            this.gbWatchdog.TabIndex = 9;
            this.gbWatchdog.TabStop = false;
            this.gbWatchdog.Text = "Watchdog Timer";
            // 
            // lblCountdownDesc
            // 
            this.lblCountdownDesc.AutoSize = true;
            this.lblCountdownDesc.BackColor = System.Drawing.SystemColors.Info;
            this.lblCountdownDesc.Location = new System.Drawing.Point(11, 151);
            this.lblCountdownDesc.Name = "lblCountdownDesc";
            this.lblCountdownDesc.Size = new System.Drawing.Size(339, 25);
            this.lblCountdownDesc.TabIndex = 7;
            this.lblCountdownDesc.Text = "0=use firmware default countdown";
            // 
            // lblWatchdogCountdown
            // 
            this.lblWatchdogCountdown.AutoSize = true;
            this.lblWatchdogCountdown.Location = new System.Drawing.Point(11, 117);
            this.lblWatchdogCountdown.Name = "lblWatchdogCountdown";
            this.lblWatchdogCountdown.Size = new System.Drawing.Size(120, 25);
            this.lblWatchdogCountdown.TabIndex = 3;
            this.lblWatchdogCountdown.Text = "Countdown";
            // 
            // nudWatchdogCountdown
            // 
            this.nudWatchdogCountdown.Location = new System.Drawing.Point(137, 117);
            this.nudWatchdogCountdown.Maximum = new decimal(new int[] {
            63,
            0,
            0,
            0});
            this.nudWatchdogCountdown.Name = "nudWatchdogCountdown";
            this.nudWatchdogCountdown.Size = new System.Drawing.Size(120, 31);
            this.nudWatchdogCountdown.TabIndex = 2;
            this.nudWatchdogCountdown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudWatchdogCountdown.ValueChanged += new System.EventHandler(this.nudWatchdogCountdown_ValueChanged);
            // 
            // chkWatchdogEnabled
            // 
            this.chkWatchdogEnabled.AutoSize = true;
            this.chkWatchdogEnabled.Checked = true;
            this.chkWatchdogEnabled.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.chkWatchdogEnabled.Location = new System.Drawing.Point(13, 81);
            this.chkWatchdogEnabled.Name = "chkWatchdogEnabled";
            this.chkWatchdogEnabled.Size = new System.Drawing.Size(123, 29);
            this.chkWatchdogEnabled.TabIndex = 1;
            this.chkWatchdogEnabled.Text = "Enabled";
            this.chkWatchdogEnabled.UseVisualStyleBackColor = true;
            this.chkWatchdogEnabled.CheckedChanged += new System.EventHandler(this.chkWatchdogEnabled_CheckedChanged);
            // 
            // btnDisableWatchdog
            // 
            this.btnDisableWatchdog.Location = new System.Drawing.Point(6, 30);
            this.btnDisableWatchdog.Name = "btnDisableWatchdog";
            this.btnDisableWatchdog.Size = new System.Drawing.Size(227, 43);
            this.btnDisableWatchdog.TabIndex = 0;
            this.btnDisableWatchdog.Text = "&Disable Watchdog";
            this.btnDisableWatchdog.UseVisualStyleBackColor = true;
            this.btnDisableWatchdog.Click += new System.EventHandler(this.btnDisableWatchdog_Click);
            // 
            // gbPowerDown
            // 
            this.gbPowerDown.Controls.Add(this.lblDelayDescr);
            this.gbPowerDown.Controls.Add(this.lblPowerDownDelayTime);
            this.gbPowerDown.Controls.Add(this.nudPowerDownDelay);
            this.gbPowerDown.Controls.Add(this.gbPowerDownLevel);
            this.gbPowerDown.Controls.Add(this.chkPowerDownEnabled);
            this.gbPowerDown.Location = new System.Drawing.Point(3, 431);
            this.gbPowerDown.Name = "gbPowerDown";
            this.gbPowerDown.Size = new System.Drawing.Size(907, 155);
            this.gbPowerDown.TabIndex = 9;
            this.gbPowerDown.TabStop = false;
            this.gbPowerDown.Text = "Synchro Power Down Control";
            // 
            // lblDelayDescr
            // 
            this.lblDelayDescr.AutoSize = true;
            this.lblDelayDescr.BackColor = System.Drawing.SystemColors.Info;
            this.lblDelayDescr.Location = new System.Drawing.Point(419, 106);
            this.lblDelayDescr.Name = "lblDelayDescr";
            this.lblDelayDescr.Size = new System.Drawing.Size(323, 25);
            this.lblDelayDescr.TabIndex = 6;
            this.lblDelayDescr.Text = "0=use firmware default of 512ms";
            // 
            // lblPowerDownDelayTime
            // 
            this.lblPowerDownDelayTime.AutoSize = true;
            this.lblPowerDownDelayTime.Location = new System.Drawing.Point(291, 65);
            this.lblPowerDownDelayTime.Name = "lblPowerDownDelayTime";
            this.lblPowerDownDelayTime.Size = new System.Drawing.Size(121, 25);
            this.lblPowerDownDelayTime.TabIndex = 5;
            this.lblPowerDownDelayTime.Text = "Delay (ms) ";
            // 
            // nudPowerDownDelay
            // 
            this.nudPowerDownDelay.Increment = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.nudPowerDownDelay.Location = new System.Drawing.Point(292, 106);
            this.nudPowerDownDelay.Maximum = new decimal(new int[] {
            2016,
            0,
            0,
            0});
            this.nudPowerDownDelay.Name = "nudPowerDownDelay";
            this.nudPowerDownDelay.Size = new System.Drawing.Size(120, 31);
            this.nudPowerDownDelay.TabIndex = 4;
            this.nudPowerDownDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudPowerDownDelay.Value = new decimal(new int[] {
            512,
            0,
            0,
            0});
            this.nudPowerDownDelay.ValueChanged += new System.EventHandler(this.nudPowerDownDelay_ValueChanged);
            // 
            // gbPowerDownLevel
            // 
            this.gbPowerDownLevel.Controls.Add(this.rdoPowerDownLevelHalf);
            this.gbPowerDownLevel.Controls.Add(this.rdoPowerDownLevelFull);
            this.gbPowerDownLevel.Location = new System.Drawing.Point(33, 65);
            this.gbPowerDownLevel.Name = "gbPowerDownLevel";
            this.gbPowerDownLevel.Size = new System.Drawing.Size(239, 72);
            this.gbPowerDownLevel.TabIndex = 5;
            this.gbPowerDownLevel.TabStop = false;
            this.gbPowerDownLevel.Text = "Power Down Level";
            // 
            // rdoPowerDownLevelHalf
            // 
            this.rdoPowerDownLevelHalf.AutoSize = true;
            this.rdoPowerDownLevelHalf.Location = new System.Drawing.Point(90, 30);
            this.rdoPowerDownLevelHalf.Name = "rdoPowerDownLevelHalf";
            this.rdoPowerDownLevelHalf.Size = new System.Drawing.Size(81, 29);
            this.rdoPowerDownLevelHalf.TabIndex = 7;
            this.rdoPowerDownLevelHalf.Text = "Half";
            this.rdoPowerDownLevelHalf.UseVisualStyleBackColor = true;
            this.rdoPowerDownLevelHalf.CheckedChanged += new System.EventHandler(this.rdoPowerDownLevelHalf_CheckedChanged);
            // 
            // rdoPowerDownLevelFull
            // 
            this.rdoPowerDownLevelFull.AutoSize = true;
            this.rdoPowerDownLevelFull.Checked = true;
            this.rdoPowerDownLevelFull.Location = new System.Drawing.Point(6, 30);
            this.rdoPowerDownLevelFull.Name = "rdoPowerDownLevelFull";
            this.rdoPowerDownLevelFull.Size = new System.Drawing.Size(78, 29);
            this.rdoPowerDownLevelFull.TabIndex = 6;
            this.rdoPowerDownLevelFull.TabStop = true;
            this.rdoPowerDownLevelFull.Text = "Full";
            this.rdoPowerDownLevelFull.UseVisualStyleBackColor = true;
            this.rdoPowerDownLevelFull.CheckedChanged += new System.EventHandler(this.rdoPowerDownLevelFull_CheckedChanged);
            // 
            // chkPowerDownEnabled
            // 
            this.chkPowerDownEnabled.AutoSize = true;
            this.chkPowerDownEnabled.Checked = true;
            this.chkPowerDownEnabled.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.chkPowerDownEnabled.Location = new System.Drawing.Point(6, 30);
            this.chkPowerDownEnabled.Name = "chkPowerDownEnabled";
            this.chkPowerDownEnabled.Size = new System.Drawing.Size(123, 29);
            this.chkPowerDownEnabled.TabIndex = 4;
            this.chkPowerDownEnabled.Text = "Enabled";
            this.chkPowerDownEnabled.UseVisualStyleBackColor = true;
            this.chkPowerDownEnabled.CheckedChanged += new System.EventHandler(this.chkPowerDownEnabled_CheckedChanged);
            // 
            // gbUSBDebug
            // 
            this.gbUSBDebug.Controls.Add(this.chkUSBDebugEnabled);
            this.gbUSBDebug.Location = new System.Drawing.Point(319, 6);
            this.gbUSBDebug.Name = "gbUSBDebug";
            this.gbUSBDebug.Size = new System.Drawing.Size(369, 77);
            this.gbUSBDebug.TabIndex = 9;
            this.gbUSBDebug.TabStop = false;
            this.gbUSBDebug.Text = "USB Debug";
            // 
            // chkUSBDebugEnabled
            // 
            this.chkUSBDebugEnabled.AutoSize = true;
            this.chkUSBDebugEnabled.Checked = true;
            this.chkUSBDebugEnabled.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.chkUSBDebugEnabled.Location = new System.Drawing.Point(6, 30);
            this.chkUSBDebugEnabled.Name = "chkUSBDebugEnabled";
            this.chkUSBDebugEnabled.Size = new System.Drawing.Size(123, 29);
            this.chkUSBDebugEnabled.TabIndex = 8;
            this.chkUSBDebugEnabled.Text = "Enabled";
            this.chkUSBDebugEnabled.UseVisualStyleBackColor = true;
            // 
            // gbDemo
            // 
            this.gbDemo.Controls.Add(this.chkStartDemo);
            this.gbDemo.Controls.Add(this.gbDemoSpeedAndStepping);
            this.gbDemo.Controls.Add(this.gbModus);
            this.gbDemo.Controls.Add(this.gbDemoStartAndEndPositions);
            this.gbDemo.Location = new System.Drawing.Point(3, 3);
            this.gbDemo.Name = "gbDemo";
            this.gbDemo.Size = new System.Drawing.Size(907, 459);
            this.gbDemo.TabIndex = 9;
            this.gbDemo.TabStop = false;
            this.gbDemo.Text = "Demo Mode";
            // 
            // gbDemoSpeedAndStepping
            // 
            this.gbDemoSpeedAndStepping.Controls.Add(this.lblDemoMovementSpeed);
            this.gbDemoSpeedAndStepping.Controls.Add(this.cboDemoMovementStepSize);
            this.gbDemoSpeedAndStepping.Controls.Add(this.cboDemoMovementSpeed);
            this.gbDemoSpeedAndStepping.Controls.Add(this.lblDemoMovementStepSizeDesc);
            this.gbDemoSpeedAndStepping.Controls.Add(this.lblDemoMovementStepSize);
            this.gbDemoSpeedAndStepping.Controls.Add(this.lblDemoMovementSpeedDesc);
            this.gbDemoSpeedAndStepping.Location = new System.Drawing.Point(17, 144);
            this.gbDemoSpeedAndStepping.Name = "gbDemoSpeedAndStepping";
            this.gbDemoSpeedAndStepping.Size = new System.Drawing.Size(879, 119);
            this.gbDemoSpeedAndStepping.TabIndex = 11;
            this.gbDemoSpeedAndStepping.TabStop = false;
            this.gbDemoSpeedAndStepping.Text = "Speed and Stepping";
            // 
            // lblDemoMovementSpeed
            // 
            this.lblDemoMovementSpeed.AutoSize = true;
            this.lblDemoMovementSpeed.Location = new System.Drawing.Point(9, 37);
            this.lblDemoMovementSpeed.Name = "lblDemoMovementSpeed";
            this.lblDemoMovementSpeed.Size = new System.Drawing.Size(217, 25);
            this.lblDemoMovementSpeed.TabIndex = 31;
            this.lblDemoMovementSpeed.Text = "MOVEMENT SPEED:";
            // 
            // cboDemoMovementStepSize
            // 
            this.cboDemoMovementStepSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDemoMovementStepSize.FormattingEnabled = true;
            this.cboDemoMovementStepSize.Items.AddRange(new object[] {
            " 1 step",
            " 2 steps",
            " 4 steps",
            " 6 steps",
            " 8 steps",
            "10 steps",
            "12 steps",
            "14 steps",
            "16 steps",
            "18 steps",
            "20 steps",
            "22 steps",
            "24 steps",
            "26 steps",
            "28 steps",
            "30 steps"});
            this.cboDemoMovementStepSize.Location = new System.Drawing.Point(268, 73);
            this.cboDemoMovementStepSize.Name = "cboDemoMovementStepSize";
            this.cboDemoMovementStepSize.Size = new System.Drawing.Size(150, 33);
            this.cboDemoMovementStepSize.TabIndex = 51;
            this.cboDemoMovementStepSize.SelectedIndexChanged += new System.EventHandler(this.cboDemoMovementStepSize_SelectedIndexChanged);
            // 
            // cboDemoMovementSpeed
            // 
            this.cboDemoMovementSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDemoMovementSpeed.FormattingEnabled = true;
            this.cboDemoMovementSpeed.Items.AddRange(new object[] {
            "100 ms",
            "500 ms",
            "1 second",
            "2 seconds"});
            this.cboDemoMovementSpeed.Location = new System.Drawing.Point(268, 34);
            this.cboDemoMovementSpeed.Name = "cboDemoMovementSpeed";
            this.cboDemoMovementSpeed.Size = new System.Drawing.Size(150, 33);
            this.cboDemoMovementSpeed.TabIndex = 10;
            this.cboDemoMovementSpeed.SelectedIndexChanged += new System.EventHandler(this.cboDemoMovementSpeed_SelectedIndexChanged);
            // 
            // lblDemoMovementStepSizeDesc
            // 
            this.lblDemoMovementStepSizeDesc.AutoSize = true;
            this.lblDemoMovementStepSizeDesc.Location = new System.Drawing.Point(424, 79);
            this.lblDemoMovementStepSizeDesc.Name = "lblDemoMovementStepSizeDesc";
            this.lblDemoMovementStepSizeDesc.Size = new System.Drawing.Size(340, 25);
            this.lblDemoMovementStepSizeDesc.TabIndex = 50;
            this.lblDemoMovementStepSizeDesc.Text = "(Increment in Position Per Update)";
            // 
            // lblDemoMovementStepSize
            // 
            this.lblDemoMovementStepSize.AutoSize = true;
            this.lblDemoMovementStepSize.Location = new System.Drawing.Point(9, 76);
            this.lblDemoMovementStepSize.Name = "lblDemoMovementStepSize";
            this.lblDemoMovementStepSize.Size = new System.Drawing.Size(253, 25);
            this.lblDemoMovementStepSize.TabIndex = 33;
            this.lblDemoMovementStepSize.Text = "MOVEMENT STEP SIZE:";
            // 
            // lblDemoMovementSpeedDesc
            // 
            this.lblDemoMovementSpeedDesc.AutoSize = true;
            this.lblDemoMovementSpeedDesc.Location = new System.Drawing.Point(424, 37);
            this.lblDemoMovementSpeedDesc.Name = "lblDemoMovementSpeedDesc";
            this.lblDemoMovementSpeedDesc.Size = new System.Drawing.Size(339, 25);
            this.lblDemoMovementSpeedDesc.TabIndex = 49;
            this.lblDemoMovementSpeedDesc.Text = "(Delay Between Position Updates)";
            // 
            // gbModus
            // 
            this.gbModus.Controls.Add(this.rdoDemoModusStartToEndJumpToStart);
            this.gbModus.Controls.Add(this.rdoModusStartToEndToStart);
            this.gbModus.Location = new System.Drawing.Point(17, 269);
            this.gbModus.Name = "gbModus";
            this.gbModus.Size = new System.Drawing.Size(879, 109);
            this.gbModus.TabIndex = 11;
            this.gbModus.TabStop = false;
            this.gbModus.Text = "Modus";
            // 
            // rdoDemoModusStartToEndJumpToStart
            // 
            this.rdoDemoModusStartToEndJumpToStart.AutoSize = true;
            this.rdoDemoModusStartToEndJumpToStart.Location = new System.Drawing.Point(12, 65);
            this.rdoDemoModusStartToEndJumpToStart.Name = "rdoDemoModusStartToEndJumpToStart";
            this.rdoDemoModusStartToEndJumpToStart.Size = new System.Drawing.Size(717, 29);
            this.rdoDemoModusStartToEndJumpToStart.TabIndex = 54;
            this.rdoDemoModusStartToEndJumpToStart.Text = "Sweep \"up\" from start to end position, then jumps back to start position";
            this.rdoDemoModusStartToEndJumpToStart.UseVisualStyleBackColor = true;
            this.rdoDemoModusStartToEndJumpToStart.CheckedChanged += new System.EventHandler(this.rdoDemoModusStartToEndJumpToStart_CheckedChanged);
            // 
            // rdoModusStartToEndToStart
            // 
            this.rdoModusStartToEndToStart.AutoSize = true;
            this.rdoModusStartToEndToStart.Location = new System.Drawing.Point(12, 30);
            this.rdoModusStartToEndToStart.Name = "rdoModusStartToEndToStart";
            this.rdoModusStartToEndToStart.Size = new System.Drawing.Size(842, 29);
            this.rdoModusStartToEndToStart.TabIndex = 53;
            this.rdoModusStartToEndToStart.Text = "Sweep \"up\" from start to end position, then sweeps \"down\" from end to start posit" +
    "ion";
            this.rdoModusStartToEndToStart.UseVisualStyleBackColor = true;
            this.rdoModusStartToEndToStart.CheckedChanged += new System.EventHandler(this.rdoModusStartToEndToStart_CheckedChanged);
            // 
            // gbDemoStartAndEndPositions
            // 
            this.gbDemoStartAndEndPositions.Controls.Add(this.lblDemoEndPositionHex);
            this.gbDemoStartAndEndPositions.Controls.Add(this.lblDemoStartPositionHex);
            this.gbDemoStartAndEndPositions.Controls.Add(this.lblDemoEndPositionDegrees);
            this.gbDemoStartAndEndPositions.Controls.Add(this.lblDemoStartPositionDegrees);
            this.gbDemoStartAndEndPositions.Controls.Add(this.nudDemoEndPositionDecimal);
            this.gbDemoStartAndEndPositions.Controls.Add(this.lblDemoEndPositionDecimal);
            this.gbDemoStartAndEndPositions.Controls.Add(this.nudDemoStartPositionDecimal);
            this.gbDemoStartAndEndPositions.Controls.Add(this.lblDemoStartPosition);
            this.gbDemoStartAndEndPositions.Location = new System.Drawing.Point(17, 30);
            this.gbDemoStartAndEndPositions.Name = "gbDemoStartAndEndPositions";
            this.gbDemoStartAndEndPositions.Size = new System.Drawing.Size(879, 103);
            this.gbDemoStartAndEndPositions.TabIndex = 9;
            this.gbDemoStartAndEndPositions.TabStop = false;
            this.gbDemoStartAndEndPositions.Text = "Start and End Positions";
            // 
            // lblDemoEndPositionHex
            // 
            this.lblDemoEndPositionHex.AutoSize = true;
            this.lblDemoEndPositionHex.Location = new System.Drawing.Point(522, 65);
            this.lblDemoEndPositionHex.Name = "lblDemoEndPositionHex";
            this.lblDemoEndPositionHex.Size = new System.Drawing.Size(56, 25);
            this.lblDemoEndPositionHex.TabIndex = 47;
            this.lblDemoEndPositionHex.Text = "Hex:";
            // 
            // lblDemoStartPositionHex
            // 
            this.lblDemoStartPositionHex.AutoSize = true;
            this.lblDemoStartPositionHex.Location = new System.Drawing.Point(522, 28);
            this.lblDemoStartPositionHex.Name = "lblDemoStartPositionHex";
            this.lblDemoStartPositionHex.Size = new System.Drawing.Size(56, 25);
            this.lblDemoStartPositionHex.TabIndex = 46;
            this.lblDemoStartPositionHex.Text = "Hex:";
            // 
            // lblDemoEndPositionDegrees
            // 
            this.lblDemoEndPositionDegrees.AutoSize = true;
            this.lblDemoEndPositionDegrees.Location = new System.Drawing.Point(352, 65);
            this.lblDemoEndPositionDegrees.Name = "lblDemoEndPositionDegrees";
            this.lblDemoEndPositionDegrees.Size = new System.Drawing.Size(99, 25);
            this.lblDemoEndPositionDegrees.TabIndex = 45;
            this.lblDemoEndPositionDegrees.Text = "Degrees:";
            // 
            // lblDemoStartPositionDegrees
            // 
            this.lblDemoStartPositionDegrees.AutoSize = true;
            this.lblDemoStartPositionDegrees.Location = new System.Drawing.Point(352, 28);
            this.lblDemoStartPositionDegrees.Name = "lblDemoStartPositionDegrees";
            this.lblDemoStartPositionDegrees.Size = new System.Drawing.Size(99, 25);
            this.lblDemoStartPositionDegrees.TabIndex = 44;
            this.lblDemoStartPositionDegrees.Text = "Degrees:";
            // 
            // nudDemoEndPositionDecimal
            // 
            this.nudDemoEndPositionDecimal.Location = new System.Drawing.Point(233, 62);
            this.nudDemoEndPositionDecimal.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudDemoEndPositionDecimal.Name = "nudDemoEndPositionDecimal";
            this.nudDemoEndPositionDecimal.Size = new System.Drawing.Size(95, 31);
            this.nudDemoEndPositionDecimal.TabIndex = 39;
            this.nudDemoEndPositionDecimal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudDemoEndPositionDecimal.ValueChanged += new System.EventHandler(this.nudDemoEndPositionDecimal_ValueChanged);
            // 
            // lblDemoEndPositionDecimal
            // 
            this.lblDemoEndPositionDecimal.AutoSize = true;
            this.lblDemoEndPositionDecimal.Location = new System.Drawing.Point(12, 65);
            this.lblDemoEndPositionDecimal.Name = "lblDemoEndPositionDecimal";
            this.lblDemoEndPositionDecimal.Size = new System.Drawing.Size(150, 25);
            this.lblDemoEndPositionDecimal.TabIndex = 38;
            this.lblDemoEndPositionDecimal.Text = "End (decimal):";
            // 
            // nudDemoStartPositionDecimal
            // 
            this.nudDemoStartPositionDecimal.Location = new System.Drawing.Point(233, 25);
            this.nudDemoStartPositionDecimal.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudDemoStartPositionDecimal.Name = "nudDemoStartPositionDecimal";
            this.nudDemoStartPositionDecimal.Size = new System.Drawing.Size(95, 31);
            this.nudDemoStartPositionDecimal.TabIndex = 37;
            this.nudDemoStartPositionDecimal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudDemoStartPositionDecimal.ValueChanged += new System.EventHandler(this.nudDemoStartPositionDecimal_ValueChanged);
            // 
            // lblDemoStartPosition
            // 
            this.lblDemoStartPosition.AutoSize = true;
            this.lblDemoStartPosition.Location = new System.Drawing.Point(12, 28);
            this.lblDemoStartPosition.Name = "lblDemoStartPosition";
            this.lblDemoStartPosition.Size = new System.Drawing.Size(157, 25);
            this.lblDemoStartPosition.TabIndex = 36;
            this.lblDemoStartPosition.Text = "Start (decimal):";
            // 
            // gbStatorBaseAngles
            // 
            this.gbStatorBaseAngles.Controls.Add(this.btnUpdateStatorBaseAngles);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS3BaseAngleHex);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS2BaseAngleHex);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS1BaseAngleHex);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS3BaseAngleDegrees);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS2BaseAngleDegrees);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS1BaseAngleDegrees);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS3BaseAngleMSB);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS2BaseAngleMSB);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS1BaseAngleMSB);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS3BaseAngleLSB);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS2BaseAngleLSB);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS1BaseAngleLSB);
            this.gbStatorBaseAngles.Controls.Add(this.nudStatorS3BaseAngle);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS3BaseAngle);
            this.gbStatorBaseAngles.Controls.Add(this.nudStatorS2BaseAngle);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS2BaseAngle);
            this.gbStatorBaseAngles.Controls.Add(this.nudStatorS1BaseAngle);
            this.gbStatorBaseAngles.Controls.Add(this.lblStatorS1BaseAngle);
            this.gbStatorBaseAngles.Location = new System.Drawing.Point(3, 3);
            this.gbStatorBaseAngles.Name = "gbStatorBaseAngles";
            this.gbStatorBaseAngles.Size = new System.Drawing.Size(907, 226);
            this.gbStatorBaseAngles.TabIndex = 10;
            this.gbStatorBaseAngles.TabStop = false;
            this.gbStatorBaseAngles.Text = "Stator Base Angles";
            // 
            // btnUpdateStatorBaseAngles
            // 
            this.btnUpdateStatorBaseAngles.Location = new System.Drawing.Point(34, 166);
            this.btnUpdateStatorBaseAngles.Name = "btnUpdateStatorBaseAngles";
            this.btnUpdateStatorBaseAngles.Size = new System.Drawing.Size(150, 44);
            this.btnUpdateStatorBaseAngles.TabIndex = 8;
            this.btnUpdateStatorBaseAngles.Text = "&Update";
            this.btnUpdateStatorBaseAngles.UseVisualStyleBackColor = true;
            this.btnUpdateStatorBaseAngles.Click += new System.EventHandler(this.btnUpdateStatorBaseAngles_Click);
            // 
            // lblStatorS3BaseAngleHex
            // 
            this.lblStatorS3BaseAngleHex.AutoSize = true;
            this.lblStatorS3BaseAngleHex.Location = new System.Drawing.Point(539, 126);
            this.lblStatorS3BaseAngleHex.Name = "lblStatorS3BaseAngleHex";
            this.lblStatorS3BaseAngleHex.Size = new System.Drawing.Size(56, 25);
            this.lblStatorS3BaseAngleHex.TabIndex = 24;
            this.lblStatorS3BaseAngleHex.Text = "Hex:";
            // 
            // lblStatorS2BaseAngleHex
            // 
            this.lblStatorS2BaseAngleHex.AutoSize = true;
            this.lblStatorS2BaseAngleHex.Location = new System.Drawing.Point(539, 89);
            this.lblStatorS2BaseAngleHex.Name = "lblStatorS2BaseAngleHex";
            this.lblStatorS2BaseAngleHex.Size = new System.Drawing.Size(56, 25);
            this.lblStatorS2BaseAngleHex.TabIndex = 23;
            this.lblStatorS2BaseAngleHex.Text = "Hex:";
            // 
            // lblStatorS1BaseAngleHex
            // 
            this.lblStatorS1BaseAngleHex.AutoSize = true;
            this.lblStatorS1BaseAngleHex.Location = new System.Drawing.Point(539, 52);
            this.lblStatorS1BaseAngleHex.Name = "lblStatorS1BaseAngleHex";
            this.lblStatorS1BaseAngleHex.Size = new System.Drawing.Size(56, 25);
            this.lblStatorS1BaseAngleHex.TabIndex = 22;
            this.lblStatorS1BaseAngleHex.Text = "Hex:";
            // 
            // lblStatorS3BaseAngleDegrees
            // 
            this.lblStatorS3BaseAngleDegrees.AutoSize = true;
            this.lblStatorS3BaseAngleDegrees.Location = new System.Drawing.Point(369, 126);
            this.lblStatorS3BaseAngleDegrees.Name = "lblStatorS3BaseAngleDegrees";
            this.lblStatorS3BaseAngleDegrees.Size = new System.Drawing.Size(99, 25);
            this.lblStatorS3BaseAngleDegrees.TabIndex = 21;
            this.lblStatorS3BaseAngleDegrees.Text = "Degrees:";
            // 
            // lblStatorS2BaseAngleDegrees
            // 
            this.lblStatorS2BaseAngleDegrees.AutoSize = true;
            this.lblStatorS2BaseAngleDegrees.Location = new System.Drawing.Point(369, 89);
            this.lblStatorS2BaseAngleDegrees.Name = "lblStatorS2BaseAngleDegrees";
            this.lblStatorS2BaseAngleDegrees.Size = new System.Drawing.Size(99, 25);
            this.lblStatorS2BaseAngleDegrees.TabIndex = 20;
            this.lblStatorS2BaseAngleDegrees.Text = "Degrees:";
            // 
            // lblStatorS1BaseAngleDegrees
            // 
            this.lblStatorS1BaseAngleDegrees.AutoSize = true;
            this.lblStatorS1BaseAngleDegrees.Location = new System.Drawing.Point(369, 52);
            this.lblStatorS1BaseAngleDegrees.Name = "lblStatorS1BaseAngleDegrees";
            this.lblStatorS1BaseAngleDegrees.Size = new System.Drawing.Size(99, 25);
            this.lblStatorS1BaseAngleDegrees.TabIndex = 19;
            this.lblStatorS1BaseAngleDegrees.Text = "Degrees:";
            // 
            // lblStatorS3BaseAngleMSB
            // 
            this.lblStatorS3BaseAngleMSB.AutoSize = true;
            this.lblStatorS3BaseAngleMSB.Location = new System.Drawing.Point(771, 126);
            this.lblStatorS3BaseAngleMSB.Name = "lblStatorS3BaseAngleMSB";
            this.lblStatorS3BaseAngleMSB.Size = new System.Drawing.Size(64, 25);
            this.lblStatorS3BaseAngleMSB.TabIndex = 18;
            this.lblStatorS3BaseAngleMSB.Text = "MSB:";
            // 
            // lblStatorS2BaseAngleMSB
            // 
            this.lblStatorS2BaseAngleMSB.AutoSize = true;
            this.lblStatorS2BaseAngleMSB.Location = new System.Drawing.Point(771, 89);
            this.lblStatorS2BaseAngleMSB.Name = "lblStatorS2BaseAngleMSB";
            this.lblStatorS2BaseAngleMSB.Size = new System.Drawing.Size(64, 25);
            this.lblStatorS2BaseAngleMSB.TabIndex = 17;
            this.lblStatorS2BaseAngleMSB.Text = "MSB:";
            // 
            // lblStatorS1BaseAngleMSB
            // 
            this.lblStatorS1BaseAngleMSB.AutoSize = true;
            this.lblStatorS1BaseAngleMSB.Location = new System.Drawing.Point(771, 52);
            this.lblStatorS1BaseAngleMSB.Name = "lblStatorS1BaseAngleMSB";
            this.lblStatorS1BaseAngleMSB.Size = new System.Drawing.Size(64, 25);
            this.lblStatorS1BaseAngleMSB.TabIndex = 16;
            this.lblStatorS1BaseAngleMSB.Text = "MSB:";
            // 
            // lblStatorS3BaseAngleLSB
            // 
            this.lblStatorS3BaseAngleLSB.AutoSize = true;
            this.lblStatorS3BaseAngleLSB.Location = new System.Drawing.Point(665, 126);
            this.lblStatorS3BaseAngleLSB.Name = "lblStatorS3BaseAngleLSB";
            this.lblStatorS3BaseAngleLSB.Size = new System.Drawing.Size(58, 25);
            this.lblStatorS3BaseAngleLSB.TabIndex = 15;
            this.lblStatorS3BaseAngleLSB.Text = "LSB:";
            // 
            // lblStatorS2BaseAngleLSB
            // 
            this.lblStatorS2BaseAngleLSB.AutoSize = true;
            this.lblStatorS2BaseAngleLSB.Location = new System.Drawing.Point(665, 89);
            this.lblStatorS2BaseAngleLSB.Name = "lblStatorS2BaseAngleLSB";
            this.lblStatorS2BaseAngleLSB.Size = new System.Drawing.Size(58, 25);
            this.lblStatorS2BaseAngleLSB.TabIndex = 14;
            this.lblStatorS2BaseAngleLSB.Text = "LSB:";
            // 
            // lblStatorS1BaseAngleLSB
            // 
            this.lblStatorS1BaseAngleLSB.AutoSize = true;
            this.lblStatorS1BaseAngleLSB.Location = new System.Drawing.Point(665, 52);
            this.lblStatorS1BaseAngleLSB.Name = "lblStatorS1BaseAngleLSB";
            this.lblStatorS1BaseAngleLSB.Size = new System.Drawing.Size(58, 25);
            this.lblStatorS1BaseAngleLSB.TabIndex = 13;
            this.lblStatorS1BaseAngleLSB.Text = "LSB:";
            // 
            // nudStatorS3BaseAngle
            // 
            this.nudStatorS3BaseAngle.Location = new System.Drawing.Point(233, 123);
            this.nudStatorS3BaseAngle.Maximum = new decimal(new int[] {
            1023,
            0,
            0,
            0});
            this.nudStatorS3BaseAngle.Name = "nudStatorS3BaseAngle";
            this.nudStatorS3BaseAngle.Size = new System.Drawing.Size(95, 31);
            this.nudStatorS3BaseAngle.TabIndex = 12;
            this.nudStatorS3BaseAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudStatorS3BaseAngle.ValueChanged += new System.EventHandler(this.nudStatorS3BaseAngle_ValueChanged);
            // 
            // lblStatorS3BaseAngle
            // 
            this.lblStatorS3BaseAngle.AutoSize = true;
            this.lblStatorS3BaseAngle.Location = new System.Drawing.Point(29, 126);
            this.lblStatorS3BaseAngle.Name = "lblStatorS3BaseAngle";
            this.lblStatorS3BaseAngle.Size = new System.Drawing.Size(197, 25);
            this.lblStatorS3BaseAngle.TabIndex = 11;
            this.lblStatorS3BaseAngle.Text = "S3 offset (decimal):";
            // 
            // nudStatorS2BaseAngle
            // 
            this.nudStatorS2BaseAngle.Location = new System.Drawing.Point(233, 86);
            this.nudStatorS2BaseAngle.Maximum = new decimal(new int[] {
            1023,
            0,
            0,
            0});
            this.nudStatorS2BaseAngle.Name = "nudStatorS2BaseAngle";
            this.nudStatorS2BaseAngle.Size = new System.Drawing.Size(95, 31);
            this.nudStatorS2BaseAngle.TabIndex = 10;
            this.nudStatorS2BaseAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudStatorS2BaseAngle.ValueChanged += new System.EventHandler(this.nudStatorS2BaseAngle_ValueChanged);
            // 
            // lblStatorS2BaseAngle
            // 
            this.lblStatorS2BaseAngle.AutoSize = true;
            this.lblStatorS2BaseAngle.Location = new System.Drawing.Point(29, 89);
            this.lblStatorS2BaseAngle.Name = "lblStatorS2BaseAngle";
            this.lblStatorS2BaseAngle.Size = new System.Drawing.Size(197, 25);
            this.lblStatorS2BaseAngle.TabIndex = 9;
            this.lblStatorS2BaseAngle.Text = "S2 offset (decimal):";
            // 
            // nudStatorS1BaseAngle
            // 
            this.nudStatorS1BaseAngle.Location = new System.Drawing.Point(233, 49);
            this.nudStatorS1BaseAngle.Maximum = new decimal(new int[] {
            1023,
            0,
            0,
            0});
            this.nudStatorS1BaseAngle.Name = "nudStatorS1BaseAngle";
            this.nudStatorS1BaseAngle.Size = new System.Drawing.Size(95, 31);
            this.nudStatorS1BaseAngle.TabIndex = 8;
            this.nudStatorS1BaseAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudStatorS1BaseAngle.ValueChanged += new System.EventHandler(this.nudStatorS1BaseAngle_ValueChanged);
            // 
            // lblStatorS1BaseAngle
            // 
            this.lblStatorS1BaseAngle.AutoSize = true;
            this.lblStatorS1BaseAngle.Location = new System.Drawing.Point(29, 52);
            this.lblStatorS1BaseAngle.Name = "lblStatorS1BaseAngle";
            this.lblStatorS1BaseAngle.Size = new System.Drawing.Size(197, 25);
            this.lblStatorS1BaseAngle.TabIndex = 0;
            this.lblStatorS1BaseAngle.Text = "S1 offset (decimal):";
            // 
            // gbMovementLimits
            // 
            this.gbMovementLimits.Controls.Add(this.lblLimitMaximumHex);
            this.gbMovementLimits.Controls.Add(this.lblLimitMaximumDegrees);
            this.gbMovementLimits.Controls.Add(this.lblLimitMinimumHex);
            this.gbMovementLimits.Controls.Add(this.lblLimitMinimumDegrees);
            this.gbMovementLimits.Controls.Add(this.lblLimitMinDesc);
            this.gbMovementLimits.Controls.Add(this.lblLimitMaxDesc);
            this.gbMovementLimits.Controls.Add(this.nudLimitMax);
            this.gbMovementLimits.Controls.Add(this.lblLimitMax);
            this.gbMovementLimits.Controls.Add(this.nudLimitMin);
            this.gbMovementLimits.Controls.Add(this.lblLimitMin);
            this.gbMovementLimits.Location = new System.Drawing.Point(3, 235);
            this.gbMovementLimits.Name = "gbMovementLimits";
            this.gbMovementLimits.Size = new System.Drawing.Size(907, 190);
            this.gbMovementLimits.TabIndex = 10;
            this.gbMovementLimits.TabStop = false;
            this.gbMovementLimits.Text = "Movement Limits";
            // 
            // lblLimitMaximumHex
            // 
            this.lblLimitMaximumHex.AutoSize = true;
            this.lblLimitMaximumHex.Location = new System.Drawing.Point(539, 114);
            this.lblLimitMaximumHex.Name = "lblLimitMaximumHex";
            this.lblLimitMaximumHex.Size = new System.Drawing.Size(56, 25);
            this.lblLimitMaximumHex.TabIndex = 30;
            this.lblLimitMaximumHex.Text = "Hex:";
            // 
            // lblLimitMaximumDegrees
            // 
            this.lblLimitMaximumDegrees.AutoSize = true;
            this.lblLimitMaximumDegrees.Location = new System.Drawing.Point(369, 114);
            this.lblLimitMaximumDegrees.Name = "lblLimitMaximumDegrees";
            this.lblLimitMaximumDegrees.Size = new System.Drawing.Size(99, 25);
            this.lblLimitMaximumDegrees.TabIndex = 29;
            this.lblLimitMaximumDegrees.Text = "Degrees:";
            // 
            // lblLimitMinimumHex
            // 
            this.lblLimitMinimumHex.AutoSize = true;
            this.lblLimitMinimumHex.Location = new System.Drawing.Point(539, 33);
            this.lblLimitMinimumHex.Name = "lblLimitMinimumHex";
            this.lblLimitMinimumHex.Size = new System.Drawing.Size(56, 25);
            this.lblLimitMinimumHex.TabIndex = 26;
            this.lblLimitMinimumHex.Text = "Hex:";
            // 
            // lblLimitMinimumDegrees
            // 
            this.lblLimitMinimumDegrees.AutoSize = true;
            this.lblLimitMinimumDegrees.Location = new System.Drawing.Point(369, 33);
            this.lblLimitMinimumDegrees.Name = "lblLimitMinimumDegrees";
            this.lblLimitMinimumDegrees.Size = new System.Drawing.Size(99, 25);
            this.lblLimitMinimumDegrees.TabIndex = 25;
            this.lblLimitMinimumDegrees.Text = "Degrees:";
            // 
            // lblLimitMinDesc
            // 
            this.lblLimitMinDesc.AutoSize = true;
            this.lblLimitMinDesc.BackColor = System.Drawing.SystemColors.Info;
            this.lblLimitMinDesc.Location = new System.Drawing.Point(238, 70);
            this.lblLimitMinDesc.Name = "lblLimitMinDesc";
            this.lblLimitMinDesc.Size = new System.Drawing.Size(195, 25);
            this.lblLimitMinDesc.TabIndex = 8;
            this.lblLimitMinDesc.Text = "0=no limit minimum";
            // 
            // lblLimitMaxDesc
            // 
            this.lblLimitMaxDesc.AutoSize = true;
            this.lblLimitMaxDesc.BackColor = System.Drawing.SystemColors.Info;
            this.lblLimitMaxDesc.Location = new System.Drawing.Point(233, 150);
            this.lblLimitMaxDesc.Name = "lblLimitMaxDesc";
            this.lblLimitMaxDesc.Size = new System.Drawing.Size(225, 25);
            this.lblLimitMaxDesc.TabIndex = 11;
            this.lblLimitMaxDesc.Text = "255=no limit maximum";
            // 
            // nudLimitMax
            // 
            this.nudLimitMax.Location = new System.Drawing.Point(233, 111);
            this.nudLimitMax.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudLimitMax.Name = "nudLimitMax";
            this.nudLimitMax.Size = new System.Drawing.Size(95, 31);
            this.nudLimitMax.TabIndex = 28;
            this.nudLimitMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudLimitMax.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudLimitMax.ValueChanged += new System.EventHandler(this.nudLimitMax_ValueChanged);
            // 
            // lblLimitMax
            // 
            this.lblLimitMax.AutoSize = true;
            this.lblLimitMax.Location = new System.Drawing.Point(12, 114);
            this.lblLimitMax.Name = "lblLimitMax";
            this.lblLimitMax.Size = new System.Drawing.Size(223, 25);
            this.lblLimitMax.TabIndex = 27;
            this.lblLimitMax.Text = "LIMIT_MAX (decimal):";
            // 
            // nudLimitMin
            // 
            this.nudLimitMin.Location = new System.Drawing.Point(238, 30);
            this.nudLimitMin.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudLimitMin.Name = "nudLimitMin";
            this.nudLimitMin.Size = new System.Drawing.Size(95, 31);
            this.nudLimitMin.TabIndex = 26;
            this.nudLimitMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudLimitMin.ValueChanged += new System.EventHandler(this.nudLimitMin_ValueChanged);
            // 
            // lblLimitMin
            // 
            this.lblLimitMin.AutoSize = true;
            this.lblLimitMin.Location = new System.Drawing.Point(12, 33);
            this.lblLimitMin.Name = "lblLimitMin";
            this.lblLimitMin.Size = new System.Drawing.Size(215, 25);
            this.lblLimitMin.TabIndex = 25;
            this.lblLimitMin.Text = "LIMIT_MIN (decimal):";
            // 
            // chkStartDemo
            // 
            this.chkStartDemo.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStartDemo.AutoSize = true;
            this.chkStartDemo.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.chkStartDemo.Location = new System.Drawing.Point(32, 404);
            this.chkStartDemo.Name = "chkStartDemo";
            this.chkStartDemo.Size = new System.Drawing.Size(129, 35);
            this.chkStartDemo.TabIndex = 12;
            this.chkStartDemo.Text = "Start Demo";
            this.chkStartDemo.UseVisualStyleBackColor = true;
            this.chkStartDemo.CheckedChanged += new System.EventHandler(this.chkStartDemo_CheckedChanged);
            // 
            // gbMain
            // 
            this.gbMain.Controls.Add(this.tabControl1);
            this.gbMain.Location = new System.Drawing.Point(3, 51);
            this.gbMain.Name = "gbMain";
            this.gbMain.Size = new System.Drawing.Size(950, 710);
            this.gbMain.TabIndex = 11;
            this.gbMain.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabDeviceSetup);
            this.tabControl1.Controls.Add(this.tabSynchroSetup);
            this.tabControl1.Controls.Add(this.tabSynchroControl);
            this.tabControl1.Controls.Add(this.tabDemoMode);
            this.tabControl1.Controls.Add(this.tabRawData);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(944, 680);
            this.tabControl1.TabIndex = 0;
            // 
            // tabDeviceSetup
            // 
            this.tabDeviceSetup.Controls.Add(this.gbLED);
            this.tabDeviceSetup.Location = new System.Drawing.Point(4, 34);
            this.tabDeviceSetup.Name = "tabDeviceSetup";
            this.tabDeviceSetup.Padding = new System.Windows.Forms.Padding(3);
            this.tabDeviceSetup.Size = new System.Drawing.Size(936, 642);
            this.tabDeviceSetup.TabIndex = 0;
            this.tabDeviceSetup.Text = "Device Setup";
            this.tabDeviceSetup.UseVisualStyleBackColor = true;
            // 
            // tabRawData
            // 
            this.tabRawData.Controls.Add(this.gbUSBDebug);
            this.tabRawData.Controls.Add(this.gbRawDataControl);
            this.tabRawData.Controls.Add(this.gbWatchdog);
            this.tabRawData.Location = new System.Drawing.Point(4, 34);
            this.tabRawData.Name = "tabRawData";
            this.tabRawData.Size = new System.Drawing.Size(936, 642);
            this.tabRawData.TabIndex = 4;
            this.tabRawData.Text = "Raw Data";
            this.tabRawData.UseVisualStyleBackColor = true;
            // 
            // tabSynchroSetup
            // 
            this.tabSynchroSetup.Controls.Add(this.gbMovementLimits);
            this.tabSynchroSetup.Controls.Add(this.gbPowerDown);
            this.tabSynchroSetup.Controls.Add(this.gbStatorBaseAngles);
            this.tabSynchroSetup.Location = new System.Drawing.Point(4, 34);
            this.tabSynchroSetup.Name = "tabSynchroSetup";
            this.tabSynchroSetup.Size = new System.Drawing.Size(936, 642);
            this.tabSynchroSetup.TabIndex = 5;
            this.tabSynchroSetup.Text = "Synchro Setup";
            this.tabSynchroSetup.UseVisualStyleBackColor = true;
            // 
            // tabDemoMode
            // 
            this.tabDemoMode.Controls.Add(this.gbDemo);
            this.tabDemoMode.Location = new System.Drawing.Point(4, 34);
            this.tabDemoMode.Name = "tabDemoMode";
            this.tabDemoMode.Size = new System.Drawing.Size(936, 642);
            this.tabDemoMode.TabIndex = 6;
            this.tabDemoMode.Text = "Demo Mode";
            this.tabDemoMode.UseVisualStyleBackColor = true;
            // 
            // tabSynchroControl
            // 
            this.tabSynchroControl.Location = new System.Drawing.Point(4, 34);
            this.tabSynchroControl.Name = "tabSynchroControl";
            this.tabSynchroControl.Size = new System.Drawing.Size(936, 642);
            this.tabSynchroControl.TabIndex = 7;
            this.tabSynchroControl.Text = "Synchro Control";
            this.tabSynchroControl.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(958, 761);
            this.Controls.Add(this.gbMain);
            this.Controls.Add(this.lblSerialPort);
            this.Controls.Add(this.cbSerialPort);
            this.Controls.Add(this.lblIdentification);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SDI Test Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.epErrorProvider)).EndInit();
            this.gbRawDataControl.ResumeLayout(false);
            this.gbRawDataControl.PerformLayout();
            this.gbLED.ResumeLayout(false);
            this.gbLED.PerformLayout();
            this.gbWatchdog.ResumeLayout(false);
            this.gbWatchdog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudWatchdogCountdown)).EndInit();
            this.gbPowerDown.ResumeLayout(false);
            this.gbPowerDown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPowerDownDelay)).EndInit();
            this.gbPowerDownLevel.ResumeLayout(false);
            this.gbPowerDownLevel.PerformLayout();
            this.gbUSBDebug.ResumeLayout(false);
            this.gbUSBDebug.PerformLayout();
            this.gbDemo.ResumeLayout(false);
            this.gbDemo.PerformLayout();
            this.gbDemoSpeedAndStepping.ResumeLayout(false);
            this.gbDemoSpeedAndStepping.PerformLayout();
            this.gbModus.ResumeLayout(false);
            this.gbModus.PerformLayout();
            this.gbDemoStartAndEndPositions.ResumeLayout(false);
            this.gbDemoStartAndEndPositions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDemoEndPositionDecimal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDemoStartPositionDecimal)).EndInit();
            this.gbStatorBaseAngles.ResumeLayout(false);
            this.gbStatorBaseAngles.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStatorS3BaseAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStatorS2BaseAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStatorS1BaseAngle)).EndInit();
            this.gbMovementLimits.ResumeLayout(false);
            this.gbMovementLimits.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLimitMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLimitMin)).EndInit();
            this.gbMain.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabDeviceSetup.ResumeLayout(false);
            this.tabRawData.ResumeLayout(false);
            this.tabSynchroSetup.ResumeLayout(false);
            this.tabDemoMode.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblDeviceAddress;
        private System.Windows.Forms.ErrorProvider epErrorProvider;
        private System.Windows.Forms.Label lblSerialPort;
        private System.Windows.Forms.ComboBox cbSerialPort;
        private System.Windows.Forms.Label lblIdentification;
        private System.Windows.Forms.GroupBox gbRawDataControl;
        private System.Windows.Forms.Label lblSubAddr;
        private System.Windows.Forms.TextBox txtSubAddr;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblDataByte;
        private System.Windows.Forms.TextBox txtDataByte;
        private System.Windows.Forms.GroupBox gbLED;
        private System.Windows.Forms.RadioButton rdoToggleLEDPerAcceptedCommand;
        private System.Windows.Forms.RadioButton rdoLEDFlashesAtHeartbeatRate;
        private System.Windows.Forms.RadioButton rdoLEDAlwaysOn;
        private System.Windows.Forms.RadioButton rdoLEDAlwaysOff;
        private System.Windows.Forms.GroupBox gbWatchdog;
        private System.Windows.Forms.Label lblWatchdogCountdown;
        private System.Windows.Forms.NumericUpDown nudWatchdogCountdown;
        private System.Windows.Forms.CheckBox chkWatchdogEnabled;
        private System.Windows.Forms.Button btnDisableWatchdog;
        private System.Windows.Forms.GroupBox gbPowerDown;
        private System.Windows.Forms.Label lblPowerDownDelayTime;
        private System.Windows.Forms.NumericUpDown nudPowerDownDelay;
        private System.Windows.Forms.GroupBox gbPowerDownLevel;
        private System.Windows.Forms.RadioButton rdoPowerDownLevelHalf;
        private System.Windows.Forms.RadioButton rdoPowerDownLevelFull;
        private System.Windows.Forms.CheckBox chkPowerDownEnabled;
        private System.Windows.Forms.Label lblDelayDescr;
        private System.Windows.Forms.Label lblCountdownDesc;
        private System.Windows.Forms.GroupBox gbUSBDebug;
        private System.Windows.Forms.CheckBox chkUSBDebugEnabled;
        private System.Windows.Forms.GroupBox gbDemo;
        private System.Windows.Forms.GroupBox gbStatorBaseAngles;
        private System.Windows.Forms.Label lblStatorS3BaseAngleMSB;
        private System.Windows.Forms.Label lblStatorS2BaseAngleMSB;
        private System.Windows.Forms.Label lblStatorS1BaseAngleMSB;
        private System.Windows.Forms.Label lblStatorS3BaseAngleLSB;
        private System.Windows.Forms.Label lblStatorS2BaseAngleLSB;
        private System.Windows.Forms.Label lblStatorS1BaseAngleLSB;
        private System.Windows.Forms.NumericUpDown nudStatorS3BaseAngle;
        private System.Windows.Forms.Label lblStatorS3BaseAngle;
        private System.Windows.Forms.NumericUpDown nudStatorS2BaseAngle;
        private System.Windows.Forms.Label lblStatorS2BaseAngle;
        private System.Windows.Forms.NumericUpDown nudStatorS1BaseAngle;
        private System.Windows.Forms.Label lblStatorS1BaseAngle;
        private System.Windows.Forms.Label lblStatorS3BaseAngleDegrees;
        private System.Windows.Forms.Label lblStatorS2BaseAngleDegrees;
        private System.Windows.Forms.Label lblStatorS1BaseAngleDegrees;
        private System.Windows.Forms.Label lblStatorS3BaseAngleHex;
        private System.Windows.Forms.Label lblStatorS2BaseAngleHex;
        private System.Windows.Forms.Label lblStatorS1BaseAngleHex;
        private System.Windows.Forms.Button btnUpdateStatorBaseAngles;
        private System.Windows.Forms.GroupBox gbMovementLimits;
        private System.Windows.Forms.NumericUpDown nudLimitMax;
        private System.Windows.Forms.Label lblLimitMax;
        private System.Windows.Forms.NumericUpDown nudLimitMin;
        private System.Windows.Forms.Label lblLimitMin;
        private System.Windows.Forms.Label lblLimitMinDesc;
        private System.Windows.Forms.Label lblLimitMaxDesc;
        private System.Windows.Forms.GroupBox gbDemoStartAndEndPositions;
        private System.Windows.Forms.Label lblDemoEndPositionHex;
        private System.Windows.Forms.Label lblDemoStartPositionHex;
        private System.Windows.Forms.Label lblDemoEndPositionDegrees;
        private System.Windows.Forms.Label lblDemoStartPositionDegrees;
        private System.Windows.Forms.NumericUpDown nudDemoEndPositionDecimal;
        private System.Windows.Forms.Label lblDemoEndPositionDecimal;
        private System.Windows.Forms.NumericUpDown nudDemoStartPositionDecimal;
        private System.Windows.Forms.Label lblDemoStartPosition;
        private System.Windows.Forms.Label lblLimitMaximumHex;
        private System.Windows.Forms.Label lblLimitMaximumDegrees;
        private System.Windows.Forms.Label lblLimitMinimumHex;
        private System.Windows.Forms.Label lblLimitMinimumDegrees;
        private System.Windows.Forms.Label lblDemoMovementSpeed;
        private System.Windows.Forms.ComboBox cboDemoMovementSpeed;
        private System.Windows.Forms.Label lblDemoMovementStepSize;
        private System.Windows.Forms.GroupBox gbModus;
        private System.Windows.Forms.RadioButton rdoDemoModusStartToEndJumpToStart;
        private System.Windows.Forms.RadioButton rdoModusStartToEndToStart;
        private System.Windows.Forms.ComboBox cboDemoMovementStepSize;
        private System.Windows.Forms.Label lblDemoMovementStepSizeDesc;
        private System.Windows.Forms.Label lblDemoMovementSpeedDesc;
        private System.Windows.Forms.GroupBox gbDemoSpeedAndStepping;
        private System.Windows.Forms.CheckBox chkStartDemo;
        private System.Windows.Forms.GroupBox gbMain;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabDeviceSetup;
        private System.Windows.Forms.TabPage tabSynchroSetup;
        private System.Windows.Forms.TabPage tabDemoMode;
        private System.Windows.Forms.TabPage tabSynchroControl;
        private System.Windows.Forms.TabPage tabRawData;
    }
}

