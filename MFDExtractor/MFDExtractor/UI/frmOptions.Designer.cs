using Common.UI.UserControls;
namespace MFDExtractor.UI
{
    partial class frmOptions
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
            if (disposing)
            {
                Common.Util.DisposeObject(components);
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOptions));
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.errControlErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.tabAllTabs = new System.Windows.Forms.TabControl();
            this.tabInstruments = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabOtherInstros = new System.Windows.Forms.TabControl();
            this.tabMfdsHud = new System.Windows.Forms.TabPage();
            this.chkEnableMFD4 = new System.Windows.Forms.CheckBox();
            this.chkEnableMFD3 = new System.Windows.Forms.CheckBox();
            this.cmdRecoverMfd4 = new System.Windows.Forms.Button();
            this.cmdRecoverRightMfd = new System.Windows.Forms.Button();
            this.cmdRecoverMfd3 = new System.Windows.Forms.Button();
            this.chkEnableRightMFD = new System.Windows.Forms.CheckBox();
            this.chkEnableLeftMFD = new System.Windows.Forms.CheckBox();
            this.cmdRecoverLeftMfd = new System.Windows.Forms.Button();
            this.cmdRecoverHud = new System.Windows.Forms.Button();
            this.chkEnableHud = new System.Windows.Forms.CheckBox();
            this.tabFlightInstruments = new System.Windows.Forms.TabPage();
            this.chkDED = new System.Windows.Forms.CheckBox();
            this.pbRecoverDED = new System.Windows.Forms.PictureBox();
            this.chkEHSI = new System.Windows.Forms.CheckBox();
            this.chkAccelerometer = new System.Windows.Forms.CheckBox();
            this.pbRecoverEHSI = new System.Windows.Forms.PictureBox();
            this.pbRecoverASI = new System.Windows.Forms.PictureBox();
            this.grpVVIOptions = new System.Windows.Forms.GroupBox();
            this.rdoVVIStyleNeedle = new System.Windows.Forms.RadioButton();
            this.rdoVVIStyleTape = new System.Windows.Forms.RadioButton();
            this.chkStandbyADI = new System.Windows.Forms.CheckBox();
            this.chkHSI = new System.Windows.Forms.CheckBox();
            this.pbRecoverCompass = new System.Windows.Forms.PictureBox();
            this.pbRecoverBackupADI = new System.Windows.Forms.PictureBox();
            this.pbRecoverHSI = new System.Windows.Forms.PictureBox();
            this.pbRecoverVVI = new System.Windows.Forms.PictureBox();
            this.chkCabinPress = new System.Windows.Forms.CheckBox();
            this.pbRecoverCabinPress = new System.Windows.Forms.PictureBox();
            this.chkAirspeedIndicator = new System.Windows.Forms.CheckBox();
            this.pbRecoverAccelerometer = new System.Windows.Forms.PictureBox();
            this.chkVVI = new System.Windows.Forms.CheckBox();
            this.chkAltimeter = new System.Windows.Forms.CheckBox();
            this.chkISIS = new System.Windows.Forms.CheckBox();
            this.chkCompass = new System.Windows.Forms.CheckBox();
            this.pbRecoverISIS = new System.Windows.Forms.PictureBox();
            this.chkADI = new System.Windows.Forms.CheckBox();
            this.pbRecoverADI = new System.Windows.Forms.PictureBox();
            this.chkAOAIndicator = new System.Windows.Forms.CheckBox();
            this.pbRecoverAOAIndicator = new System.Windows.Forms.PictureBox();
            this.pbRecoverAltimeter = new System.Windows.Forms.PictureBox();
            this.grpAltimeterStyle = new System.Windows.Forms.GroupBox();
            this.rdoAltimeterStyleDigital = new System.Windows.Forms.RadioButton();
            this.rdoAltimeterStyleElectromechanical = new System.Windows.Forms.RadioButton();
            this.tabEW = new System.Windows.Forms.TabPage();
            this.chkAzimuthIndicator = new System.Windows.Forms.CheckBox();
            this.pbRecoverAzimuthIndicator = new System.Windows.Forms.PictureBox();
            this.chkCMDSPanel = new System.Windows.Forms.CheckBox();
            this.pbRecoverCMDS = new System.Windows.Forms.PictureBox();
            this.grpAzimuthIndicatorStyle = new System.Windows.Forms.GroupBox();
            this.rdoAzimuthIndicatorStyleScope = new System.Windows.Forms.RadioButton();
            this.rdoAzimuthIndicatorStyleDigital = new System.Windows.Forms.RadioButton();
            this.grpAzimuthIndicatorBezelTypes = new System.Windows.Forms.GroupBox();
            this.rdoAzimuthIndicatorNoBezel = new System.Windows.Forms.RadioButton();
            this.rdoRWRHAFBezelType = new System.Windows.Forms.RadioButton();
            this.rdoRWRIP1310BezelType = new System.Windows.Forms.RadioButton();
            this.grpAzimuthIndicatorDigitalTypes = new System.Windows.Forms.GroupBox();
            this.rdoTTD = new System.Windows.Forms.RadioButton();
            this.rdoATDPlus = new System.Windows.Forms.RadioButton();
            this.tabEngineInstruments = new System.Windows.Forms.TabPage();
            this.chkFuelFlow = new System.Windows.Forms.CheckBox();
            this.pbRecoverFuelFlow = new System.Windows.Forms.PictureBox();
            this.gbEngine1Instros = new System.Windows.Forms.GroupBox();
            this.chkFTIT1 = new System.Windows.Forms.CheckBox();
            this.pbRecoverRPM1 = new System.Windows.Forms.PictureBox();
            this.pbRecoverOil1 = new System.Windows.Forms.PictureBox();
            this.pbRecoverNozPos1 = new System.Windows.Forms.PictureBox();
            this.pbRecoverFTIT1 = new System.Windows.Forms.PictureBox();
            this.chkRPM1 = new System.Windows.Forms.CheckBox();
            this.chkOIL1 = new System.Windows.Forms.CheckBox();
            this.chkNOZ1 = new System.Windows.Forms.CheckBox();
            this.gbEngine2Instros = new System.Windows.Forms.GroupBox();
            this.chkFTIT2 = new System.Windows.Forms.CheckBox();
            this.chkNOZ2 = new System.Windows.Forms.CheckBox();
            this.pbRecoverRPM2 = new System.Windows.Forms.PictureBox();
            this.pbRecoverOil2 = new System.Windows.Forms.PictureBox();
            this.chkOIL2 = new System.Windows.Forms.CheckBox();
            this.pbRecoverNozPos2 = new System.Windows.Forms.PictureBox();
            this.pbRecoverFTIT2 = new System.Windows.Forms.PictureBox();
            this.chkRPM2 = new System.Windows.Forms.CheckBox();
            this.gbFuelQuantityOptions = new System.Windows.Forms.GroupBox();
            this.rdoFuelQuantityDModel = new System.Windows.Forms.RadioButton();
            this.rdoFuelQuantityNeedleCModel = new System.Windows.Forms.RadioButton();
            this.chkFuelQty = new System.Windows.Forms.CheckBox();
            this.pbRecoverFuelQuantity = new System.Windows.Forms.PictureBox();
            this.tabHydraulics = new System.Windows.Forms.TabPage();
            this.chkHydA = new System.Windows.Forms.CheckBox();
            this.pbRecoverHydA = new System.Windows.Forms.PictureBox();
            this.pbRecoverHydB = new System.Windows.Forms.PictureBox();
            this.chkHydB = new System.Windows.Forms.CheckBox();
            this.tabFaults = new System.Windows.Forms.TabPage();
            this.chkPFL = new System.Windows.Forms.CheckBox();
            this.chkCautionPanel = new System.Windows.Forms.CheckBox();
            this.pbRecoverCautionPanel = new System.Windows.Forms.PictureBox();
            this.pbRecoverPFL = new System.Windows.Forms.PictureBox();
            this.tabIndexers = new System.Windows.Forms.TabPage();
            this.chkNWSIndexer = new System.Windows.Forms.CheckBox();
            this.pbRecoverNWS = new System.Windows.Forms.PictureBox();
            this.chkAOAIndexer = new System.Windows.Forms.CheckBox();
            this.pbRecoverAOAIndexer = new System.Windows.Forms.PictureBox();
            this.tabTrim = new System.Windows.Forms.TabPage();
            this.chkPitchTrim = new System.Windows.Forms.CheckBox();
            this.pbRecoverPitchTrim = new System.Windows.Forms.PictureBox();
            this.pbRecoverRollTrim = new System.Windows.Forms.PictureBox();
            this.chkRollTrim = new System.Windows.Forms.CheckBox();
            this.tabEPU = new System.Windows.Forms.TabPage();
            this.chkEPU = new System.Windows.Forms.CheckBox();
            this.pbRecoverEPU = new System.Windows.Forms.PictureBox();
            this.tabGearAndBrakes = new System.Windows.Forms.TabPage();
            this.chkSpeedbrake = new System.Windows.Forms.CheckBox();
            this.pbRecoverSpeedbrake = new System.Windows.Forms.PictureBox();
            this.chkGearLights = new System.Windows.Forms.CheckBox();
            this.pbRecoverGearLights = new System.Windows.Forms.PictureBox();
            this.tabHotkeys = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabHotkeysInner = new System.Windows.Forms.TabControl();
            this.tabGeneralKeys = new System.Windows.Forms.TabPage();
            this.lblNVIS = new System.Windows.Forms.Label();
            this.cmdNV = new System.Windows.Forms.Button();
            this.tabAccelerometerKeys = new System.Windows.Forms.TabPage();
            this.gbAccelerometer = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.cmdAccelerometerResetButtonPressed = new System.Windows.Forms.Button();
            this.lblAccelerometerResetButtonPressed = new System.Windows.Forms.Label();
            this.tabASI = new System.Windows.Forms.TabPage();
            this.gbAirspeedIndicator = new System.Windows.Forms.GroupBox();
            this.gbASIIndexKnob = new System.Windows.Forms.GroupBox();
            this.lblAirspeedIndexIncreaseHotkey = new System.Windows.Forms.Label();
            this.cmdAirspeedIndexIncreaseHotkey = new System.Windows.Forms.Button();
            this.lblAirspeedIndexDecreaseHotkey = new System.Windows.Forms.Label();
            this.cmdAirspeedIndexDecreaseHotkey = new System.Windows.Forms.Button();
            this.tabAzimuthIndicatorKeys = new System.Windows.Forms.TabPage();
            this.gbAzimuthIndicator = new System.Windows.Forms.GroupBox();
            this.gbAzimuthIndicatorBrightnessControl = new System.Windows.Forms.GroupBox();
            this.lblAzimuthIndicatorBrightnessDecrease = new System.Windows.Forms.Label();
            this.lblAzimuthIndicatorBrightnessIncrease = new System.Windows.Forms.Label();
            this.cmdAzimuthIndicatorBrightnessIncrease = new System.Windows.Forms.Button();
            this.cmdAzimuthIndicatorBrightnessDecrease = new System.Windows.Forms.Button();
            this.tabEHSIKeys = new System.Windows.Forms.TabPage();
            this.gbEHSI = new System.Windows.Forms.GroupBox();
            this.lblEHSIMenuButton = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblEHSICourseIncreaseHotkey = new System.Windows.Forms.Label();
            this.cmdEHSICourseIncreaseKey = new System.Windows.Forms.Button();
            this.cmdEHSICourseKnobDepressedKey = new System.Windows.Forms.Button();
            this.lblEHSICourseDecreaseHotkey = new System.Windows.Forms.Label();
            this.cmdEHSICourseDecreaseKey = new System.Windows.Forms.Button();
            this.label54 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblEHSIHeadingIncreaseButton = new System.Windows.Forms.Label();
            this.cmdEHSIHeadingIncreaseKey = new System.Windows.Forms.Button();
            this.label53 = new System.Windows.Forms.Label();
            this.cmdEHSIHeadingDecreaseKey = new System.Windows.Forms.Button();
            this.cmdEHSIMenuButtonHotkey = new System.Windows.Forms.Button();
            this.tabISISKeys = new System.Windows.Forms.TabPage();
            this.gbISIS = new System.Windows.Forms.GroupBox();
            this.lblISISBrightBrightnessButtonPressed = new System.Windows.Forms.Label();
            this.cmdISISBrightButtonPressed = new System.Windows.Forms.Button();
            this.cmdISISStandardBrightnessButtonPressed = new System.Windows.Forms.Button();
            this.lblISISStandardBrightnessButtonPressed = new System.Windows.Forms.Label();
            this.tabNetworking = new System.Windows.Forms.TabPage();
            this.grpNetworkMode = new System.Windows.Forms.GroupBox();
            this.grpServerOptions = new System.Windows.Forms.GroupBox();
            this.lblCompressionType = new System.Windows.Forms.Label();
            this.cboCompressionType = new System.Windows.Forms.ComboBox();
            this.lblImageFormat = new System.Windows.Forms.Label();
            this.cboImageFormat = new System.Windows.Forms.ComboBox();
            this.lblServerServerPortNum = new System.Windows.Forms.Label();
            this.txtNetworkServerUsePortNum = new System.Windows.Forms.TextBox();
            this.gbNetworkingMode = new System.Windows.Forms.GroupBox();
            this.rdoServer = new System.Windows.Forms.RadioButton();
            this.rdoStandalone = new System.Windows.Forms.RadioButton();
            this.rdoClient = new System.Windows.Forms.RadioButton();
            this.grpClientOptions = new System.Windows.Forms.GroupBox();
            this.lblClientServerPortNum = new System.Windows.Forms.Label();
            this.txtNetworkClientUseServerPortNum = new System.Windows.Forms.TextBox();
            this.lblServerIpAddress = new System.Windows.Forms.Label();
            this.ipaNetworkClientUseServerIpAddress = new Common.UI.UserControls.IPAddressControl();
            this.tabPerformance = new System.Windows.Forms.TabPage();
            this.grpPerformanceOptions = new System.Windows.Forms.GroupBox();
            this.cmdBMSOptions = new System.Windows.Forms.Button();
            this.chkOnlyUpdateImagesWhenDataChanges = new System.Windows.Forms.CheckBox();
            this.lblMilliseconds = new System.Windows.Forms.Label();
            this.cboThreadPriority = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.txtPollDelay = new System.Windows.Forms.TextBox();
            this.tabGraphics = new System.Windows.Forms.TabPage();
            this.gbGDIOptions = new System.Windows.Forms.GroupBox();
            this.lblCompositingQuality = new System.Windows.Forms.Label();
            this.cbCompositingQuality = new System.Windows.Forms.ComboBox();
            this.lblTextRenderingHint = new System.Windows.Forms.Label();
            this.cbTextRenderingHint = new System.Windows.Forms.ComboBox();
            this.chkHighlightOutputWindowsWhenContainMouseCursor = new System.Windows.Forms.CheckBox();
            this.lblSmoothingMode = new System.Windows.Forms.Label();
            this.cbSmoothingMode = new System.Windows.Forms.ComboBox();
            this.lblPixelOffsetMode = new System.Windows.Forms.Label();
            this.cbPixelOffsetMode = new System.Windows.Forms.ComboBox();
            this.lblInterpolationMode = new System.Windows.Forms.Label();
            this.cbInterpolationMode = new System.Windows.Forms.ComboBox();
            this.tabStartup = new System.Windows.Forms.TabPage();
            this.grpStartupOptions = new System.Windows.Forms.GroupBox();
            this.chkStartWithWindows = new System.Windows.Forms.CheckBox();
            this.chkStartOnLaunch = new System.Windows.Forms.CheckBox();
            this.cmdResetToDefaults = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label25 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label32 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label33 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.label36 = new System.Windows.Forms.Label();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cmdApply = new System.Windows.Forms.Button();
            this.globalEventProvider1 = new Common.InputSupport.GlobalEventProvider();
            this.globalEventProvider2 = new Common.InputSupport.GlobalEventProvider();
            ((System.ComponentModel.ISupportInitialize)(this.errControlErrorProvider)).BeginInit();
            this.tabAllTabs.SuspendLayout();
            this.tabInstruments.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabOtherInstros.SuspendLayout();
            this.tabMfdsHud.SuspendLayout();
            this.tabFlightInstruments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverDED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverEHSI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverASI)).BeginInit();
            this.grpVVIOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverCompass)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverBackupADI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverHSI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverVVI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverCabinPress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverAccelerometer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverISIS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverADI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverAOAIndicator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverAltimeter)).BeginInit();
            this.grpAltimeterStyle.SuspendLayout();
            this.tabEW.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverAzimuthIndicator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverCMDS)).BeginInit();
            this.grpAzimuthIndicatorStyle.SuspendLayout();
            this.grpAzimuthIndicatorBezelTypes.SuspendLayout();
            this.grpAzimuthIndicatorDigitalTypes.SuspendLayout();
            this.tabEngineInstruments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverFuelFlow)).BeginInit();
            this.gbEngine1Instros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverRPM1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverOil1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverNozPos1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverFTIT1)).BeginInit();
            this.gbEngine2Instros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverRPM2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverOil2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverNozPos2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverFTIT2)).BeginInit();
            this.gbFuelQuantityOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverFuelQuantity)).BeginInit();
            this.tabHydraulics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverHydA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverHydB)).BeginInit();
            this.tabFaults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverCautionPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverPFL)).BeginInit();
            this.tabIndexers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverNWS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverAOAIndexer)).BeginInit();
            this.tabTrim.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverPitchTrim)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverRollTrim)).BeginInit();
            this.tabEPU.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverEPU)).BeginInit();
            this.tabGearAndBrakes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverSpeedbrake)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverGearLights)).BeginInit();
            this.tabHotkeys.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabHotkeysInner.SuspendLayout();
            this.tabGeneralKeys.SuspendLayout();
            this.tabAccelerometerKeys.SuspendLayout();
            this.gbAccelerometer.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabASI.SuspendLayout();
            this.gbAirspeedIndicator.SuspendLayout();
            this.gbASIIndexKnob.SuspendLayout();
            this.tabAzimuthIndicatorKeys.SuspendLayout();
            this.gbAzimuthIndicator.SuspendLayout();
            this.gbAzimuthIndicatorBrightnessControl.SuspendLayout();
            this.tabEHSIKeys.SuspendLayout();
            this.gbEHSI.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabISISKeys.SuspendLayout();
            this.gbISIS.SuspendLayout();
            this.tabNetworking.SuspendLayout();
            this.grpNetworkMode.SuspendLayout();
            this.grpServerOptions.SuspendLayout();
            this.gbNetworkingMode.SuspendLayout();
            this.grpClientOptions.SuspendLayout();
            this.tabPerformance.SuspendLayout();
            this.grpPerformanceOptions.SuspendLayout();
            this.tabGraphics.SuspendLayout();
            this.gbGDIOptions.SuspendLayout();
            this.tabStartup.SuspendLayout();
            this.grpStartupOptions.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOk
            // 
            this.cmdOk.Location = new System.Drawing.Point(4, 771);
            this.cmdOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(160, 45);
            this.cmdOk.TabIndex = 150;
            this.cmdOk.Text = "&OK";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(170, 771);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(158, 45);
            this.cmdCancel.TabIndex = 151;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // errControlErrorProvider
            // 
            this.errControlErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errControlErrorProvider.ContainerControl = this;
            // 
            // tabAllTabs
            // 
            this.tabAllTabs.Controls.Add(this.tabInstruments);
            this.tabAllTabs.Controls.Add(this.tabHotkeys);
            this.tabAllTabs.Controls.Add(this.tabNetworking);
            this.tabAllTabs.Controls.Add(this.tabPerformance);
            this.tabAllTabs.Controls.Add(this.tabGraphics);
            this.tabAllTabs.Controls.Add(this.tabStartup);
            this.tabAllTabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.errControlErrorProvider.SetIconAlignment(this.tabAllTabs, System.Windows.Forms.ErrorIconAlignment.TopRight);
            this.tabAllTabs.Location = new System.Drawing.Point(0, 0);
            this.tabAllTabs.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabAllTabs.Multiline = true;
            this.tabAllTabs.Name = "tabAllTabs";
            this.tabAllTabs.SelectedIndex = 0;
            this.tabAllTabs.Size = new System.Drawing.Size(792, 759);
            this.tabAllTabs.TabIndex = 1;
            // 
            // tabInstruments
            // 
            this.tabInstruments.Controls.Add(this.panel1);
            this.tabInstruments.Location = new System.Drawing.Point(4, 29);
            this.tabInstruments.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabInstruments.Name = "tabInstruments";
            this.tabInstruments.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabInstruments.Size = new System.Drawing.Size(784, 726);
            this.tabInstruments.TabIndex = 11;
            this.tabInstruments.Text = "Instruments";
            this.tabInstruments.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tabOtherInstros);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(4, 5);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(776, 716);
            this.panel1.TabIndex = 156;
            // 
            // tabOtherInstros
            // 
            this.tabOtherInstros.Controls.Add(this.tabMfdsHud);
            this.tabOtherInstros.Controls.Add(this.tabFlightInstruments);
            this.tabOtherInstros.Controls.Add(this.tabEW);
            this.tabOtherInstros.Controls.Add(this.tabEngineInstruments);
            this.tabOtherInstros.Controls.Add(this.tabHydraulics);
            this.tabOtherInstros.Controls.Add(this.tabFaults);
            this.tabOtherInstros.Controls.Add(this.tabIndexers);
            this.tabOtherInstros.Controls.Add(this.tabTrim);
            this.tabOtherInstros.Controls.Add(this.tabEPU);
            this.tabOtherInstros.Controls.Add(this.tabGearAndBrakes);
            this.tabOtherInstros.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabOtherInstros.Location = new System.Drawing.Point(0, 0);
            this.tabOtherInstros.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabOtherInstros.Multiline = true;
            this.tabOtherInstros.Name = "tabOtherInstros";
            this.tabOtherInstros.SelectedIndex = 0;
            this.tabOtherInstros.Size = new System.Drawing.Size(776, 716);
            this.tabOtherInstros.TabIndex = 156;
            // 
            // tabMfdsHud
            // 
            this.tabMfdsHud.Controls.Add(this.chkEnableMFD4);
            this.tabMfdsHud.Controls.Add(this.chkEnableMFD3);
            this.tabMfdsHud.Controls.Add(this.cmdRecoverMfd4);
            this.tabMfdsHud.Controls.Add(this.cmdRecoverRightMfd);
            this.tabMfdsHud.Controls.Add(this.cmdRecoverMfd3);
            this.tabMfdsHud.Controls.Add(this.chkEnableRightMFD);
            this.tabMfdsHud.Controls.Add(this.chkEnableLeftMFD);
            this.tabMfdsHud.Controls.Add(this.cmdRecoverLeftMfd);
            this.tabMfdsHud.Controls.Add(this.cmdRecoverHud);
            this.tabMfdsHud.Controls.Add(this.chkEnableHud);
            this.tabMfdsHud.Location = new System.Drawing.Point(4, 54);
            this.tabMfdsHud.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabMfdsHud.Name = "tabMfdsHud";
            this.tabMfdsHud.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabMfdsHud.Size = new System.Drawing.Size(768, 658);
            this.tabMfdsHud.TabIndex = 17;
            this.tabMfdsHud.Text = "MFDs & HUD";
            this.tabMfdsHud.UseVisualStyleBackColor = true;
            // 
            // chkEnableMFD4
            // 
            this.chkEnableMFD4.AutoSize = true;
            this.chkEnableMFD4.Location = new System.Drawing.Point(14, 128);
            this.chkEnableMFD4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkEnableMFD4.Name = "chkEnableMFD4";
            this.chkEnableMFD4.Size = new System.Drawing.Size(196, 24);
            this.chkEnableMFD4.TabIndex = 163;
            this.chkEnableMFD4.Text = "Enable MFD #4 output";
            this.chkEnableMFD4.UseVisualStyleBackColor = true;
            this.chkEnableMFD4.CheckedChanged += new System.EventHandler(this.chkEnableMFD4_CheckedChanged);
            // 
            // chkEnableMFD3
            // 
            this.chkEnableMFD3.AutoSize = true;
            this.chkEnableMFD3.Location = new System.Drawing.Point(14, 91);
            this.chkEnableMFD3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkEnableMFD3.Name = "chkEnableMFD3";
            this.chkEnableMFD3.Size = new System.Drawing.Size(196, 24);
            this.chkEnableMFD3.TabIndex = 159;
            this.chkEnableMFD3.Text = "Enable MFD #3 output";
            this.chkEnableMFD3.UseVisualStyleBackColor = true;
            this.chkEnableMFD3.CheckedChanged += new System.EventHandler(this.chkEnableMFD3_CheckedChanged);
            // 
            // cmdRecoverMfd4
            // 
            this.cmdRecoverMfd4.Location = new System.Drawing.Point(231, 123);
            this.cmdRecoverMfd4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRecoverMfd4.Name = "cmdRecoverMfd4";
            this.cmdRecoverMfd4.Size = new System.Drawing.Size(120, 35);
            this.cmdRecoverMfd4.TabIndex = 166;
            this.cmdRecoverMfd4.Text = "&Recover";
            this.cmdRecoverMfd4.UseVisualStyleBackColor = true;
            this.cmdRecoverMfd4.Click += new System.EventHandler(this.cmdRecoverMfd4_Click);
            // 
            // cmdRecoverRightMfd
            // 
            this.cmdRecoverRightMfd.Location = new System.Drawing.Point(231, 48);
            this.cmdRecoverRightMfd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRecoverRightMfd.Name = "cmdRecoverRightMfd";
            this.cmdRecoverRightMfd.Size = new System.Drawing.Size(120, 35);
            this.cmdRecoverRightMfd.TabIndex = 158;
            this.cmdRecoverRightMfd.Text = "&Recover";
            this.cmdRecoverRightMfd.UseVisualStyleBackColor = true;
            this.cmdRecoverRightMfd.Click += new System.EventHandler(this.cmdRecoverRightMfd_Click);
            // 
            // cmdRecoverMfd3
            // 
            this.cmdRecoverMfd3.Location = new System.Drawing.Point(231, 85);
            this.cmdRecoverMfd3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRecoverMfd3.Name = "cmdRecoverMfd3";
            this.cmdRecoverMfd3.Size = new System.Drawing.Size(120, 35);
            this.cmdRecoverMfd3.TabIndex = 162;
            this.cmdRecoverMfd3.Text = "&Recover";
            this.cmdRecoverMfd3.UseVisualStyleBackColor = true;
            this.cmdRecoverMfd3.Click += new System.EventHandler(this.cmdRecoverMfd3_Click);
            // 
            // chkEnableRightMFD
            // 
            this.chkEnableRightMFD.AutoSize = true;
            this.chkEnableRightMFD.Location = new System.Drawing.Point(14, 54);
            this.chkEnableRightMFD.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkEnableRightMFD.Name = "chkEnableRightMFD";
            this.chkEnableRightMFD.Size = new System.Drawing.Size(216, 24);
            this.chkEnableRightMFD.TabIndex = 155;
            this.chkEnableRightMFD.Text = "Enable Right MFD output";
            this.chkEnableRightMFD.UseVisualStyleBackColor = true;
            this.chkEnableRightMFD.CheckedChanged += new System.EventHandler(this.chkEnableRightMFD_CheckedChanged);
            // 
            // chkEnableLeftMFD
            // 
            this.chkEnableLeftMFD.AutoSize = true;
            this.chkEnableLeftMFD.Location = new System.Drawing.Point(14, 15);
            this.chkEnableLeftMFD.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkEnableLeftMFD.Name = "chkEnableLeftMFD";
            this.chkEnableLeftMFD.Size = new System.Drawing.Size(206, 24);
            this.chkEnableLeftMFD.TabIndex = 2;
            this.chkEnableLeftMFD.Text = "Enable Left MFD output";
            this.chkEnableLeftMFD.UseVisualStyleBackColor = true;
            this.chkEnableLeftMFD.CheckedChanged += new System.EventHandler(this.chkEnableLeftMFD_CheckedChanged);
            // 
            // cmdRecoverLeftMfd
            // 
            this.cmdRecoverLeftMfd.Location = new System.Drawing.Point(231, 9);
            this.cmdRecoverLeftMfd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRecoverLeftMfd.Name = "cmdRecoverLeftMfd";
            this.cmdRecoverLeftMfd.Size = new System.Drawing.Size(120, 35);
            this.cmdRecoverLeftMfd.TabIndex = 9;
            this.cmdRecoverLeftMfd.Text = "&Recover";
            this.cmdRecoverLeftMfd.UseVisualStyleBackColor = true;
            this.cmdRecoverLeftMfd.Click += new System.EventHandler(this.cmdRecoverLeftMfd_Click);
            // 
            // cmdRecoverHud
            // 
            this.cmdRecoverHud.Location = new System.Drawing.Point(231, 162);
            this.cmdRecoverHud.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRecoverHud.Name = "cmdRecoverHud";
            this.cmdRecoverHud.Size = new System.Drawing.Size(120, 35);
            this.cmdRecoverHud.TabIndex = 170;
            this.cmdRecoverHud.Text = "&Recover";
            this.cmdRecoverHud.UseVisualStyleBackColor = true;
            this.cmdRecoverHud.Click += new System.EventHandler(this.cmdRecoverHud_Click);
            // 
            // chkEnableHud
            // 
            this.chkEnableHud.AutoSize = true;
            this.chkEnableHud.Location = new System.Drawing.Point(14, 168);
            this.chkEnableHud.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkEnableHud.Name = "chkEnableHud";
            this.chkEnableHud.Size = new System.Drawing.Size(175, 24);
            this.chkEnableHud.TabIndex = 167;
            this.chkEnableHud.Text = "Enable HUD output";
            this.chkEnableHud.UseVisualStyleBackColor = true;
            this.chkEnableHud.CheckedChanged += new System.EventHandler(this.chkEnableHud_CheckedChanged);
            // 
            // tabFlightInstruments
            // 
            this.tabFlightInstruments.Controls.Add(this.chkDED);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverDED);
            this.tabFlightInstruments.Controls.Add(this.chkEHSI);
            this.tabFlightInstruments.Controls.Add(this.chkAccelerometer);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverEHSI);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverASI);
            this.tabFlightInstruments.Controls.Add(this.grpVVIOptions);
            this.tabFlightInstruments.Controls.Add(this.chkStandbyADI);
            this.tabFlightInstruments.Controls.Add(this.chkHSI);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverCompass);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverBackupADI);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverHSI);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverVVI);
            this.tabFlightInstruments.Controls.Add(this.chkCabinPress);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverCabinPress);
            this.tabFlightInstruments.Controls.Add(this.chkAirspeedIndicator);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverAccelerometer);
            this.tabFlightInstruments.Controls.Add(this.chkVVI);
            this.tabFlightInstruments.Controls.Add(this.chkAltimeter);
            this.tabFlightInstruments.Controls.Add(this.chkISIS);
            this.tabFlightInstruments.Controls.Add(this.chkCompass);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverISIS);
            this.tabFlightInstruments.Controls.Add(this.chkADI);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverADI);
            this.tabFlightInstruments.Controls.Add(this.chkAOAIndicator);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverAOAIndicator);
            this.tabFlightInstruments.Controls.Add(this.pbRecoverAltimeter);
            this.tabFlightInstruments.Controls.Add(this.grpAltimeterStyle);
            this.tabFlightInstruments.Location = new System.Drawing.Point(4, 54);
            this.tabFlightInstruments.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabFlightInstruments.Name = "tabFlightInstruments";
            this.tabFlightInstruments.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabFlightInstruments.Size = new System.Drawing.Size(768, 658);
            this.tabFlightInstruments.TabIndex = 8;
            this.tabFlightInstruments.Text = "Flight & Navigation Instruments";
            this.tabFlightInstruments.UseVisualStyleBackColor = true;
            // 
            // chkDED
            // 
            this.chkDED.AutoSize = true;
            this.chkDED.Location = new System.Drawing.Point(9, 358);
            this.chkDED.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkDED.Name = "chkDED";
            this.chkDED.Size = new System.Drawing.Size(215, 24);
            this.chkDED.TabIndex = 72;
            this.chkDED.Text = "Data Entry Display (DED)";
            this.chkDED.UseVisualStyleBackColor = true;
            this.chkDED.CheckedChanged += new System.EventHandler(this.chkDED_CheckedChanged);
            // 
            // pbRecoverDED
            // 
            this.pbRecoverDED.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverDED.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverDED.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverDED.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverDED.Location = new System.Drawing.Point(400, 358);
            this.pbRecoverDED.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverDED.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverDED.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverDED.Name = "pbRecoverDED";
            this.pbRecoverDED.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverDED.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverDED.TabIndex = 73;
            this.pbRecoverDED.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverDED, "Recover");
            // 
            // chkEHSI
            // 
            this.chkEHSI.AutoSize = true;
            this.chkEHSI.Location = new System.Drawing.Point(9, 393);
            this.chkEHSI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkEHSI.Name = "chkEHSI";
            this.chkEHSI.Size = new System.Drawing.Size(367, 24);
            this.chkEHSI.TabIndex = 19;
            this.chkEHSI.Text = "Electronic Horizontal Situation Indicator (EHSI)";
            this.chkEHSI.UseVisualStyleBackColor = true;
            this.chkEHSI.CheckedChanged += new System.EventHandler(this.chkEHSI_CheckedChanged);
            // 
            // chkAccelerometer
            // 
            this.chkAccelerometer.AutoSize = true;
            this.chkAccelerometer.Location = new System.Drawing.Point(9, 9);
            this.chkAccelerometer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkAccelerometer.Name = "chkAccelerometer";
            this.chkAccelerometer.Size = new System.Drawing.Size(211, 24);
            this.chkAccelerometer.TabIndex = 0;
            this.chkAccelerometer.Text = "Accelerometer (G-meter)";
            this.chkAccelerometer.UseVisualStyleBackColor = true;
            this.chkAccelerometer.CheckedChanged += new System.EventHandler(this.chkAccelerometer_CheckedChanged);
            // 
            // pbRecoverEHSI
            // 
            this.pbRecoverEHSI.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverEHSI.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverEHSI.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverEHSI.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverEHSI.Location = new System.Drawing.Point(400, 393);
            this.pbRecoverEHSI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverEHSI.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverEHSI.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverEHSI.Name = "pbRecoverEHSI";
            this.pbRecoverEHSI.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverEHSI.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverEHSI.TabIndex = 86;
            this.pbRecoverEHSI.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverEHSI, "Recover");
            // 
            // pbRecoverASI
            // 
            this.pbRecoverASI.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverASI.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverASI.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverASI.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverASI.Location = new System.Drawing.Point(400, 48);
            this.pbRecoverASI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverASI.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverASI.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverASI.Name = "pbRecoverASI";
            this.pbRecoverASI.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverASI.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverASI.TabIndex = 67;
            this.pbRecoverASI.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverASI, "Recover");
            // 
            // grpVVIOptions
            // 
            this.grpVVIOptions.Controls.Add(this.rdoVVIStyleNeedle);
            this.grpVVIOptions.Controls.Add(this.rdoVVIStyleTape);
            this.grpVVIOptions.Location = new System.Drawing.Point(44, 573);
            this.grpVVIOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpVVIOptions.Name = "grpVVIOptions";
            this.grpVVIOptions.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpVVIOptions.Size = new System.Drawing.Size(280, 57);
            this.grpVVIOptions.TabIndex = 44;
            this.grpVVIOptions.TabStop = false;
            this.grpVVIOptions.Text = "Style";
            // 
            // rdoVVIStyleNeedle
            // 
            this.rdoVVIStyleNeedle.AutoSize = true;
            this.rdoVVIStyleNeedle.Location = new System.Drawing.Point(96, 23);
            this.rdoVVIStyleNeedle.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoVVIStyleNeedle.Name = "rdoVVIStyleNeedle";
            this.rdoVVIStyleNeedle.Size = new System.Drawing.Size(84, 24);
            this.rdoVVIStyleNeedle.TabIndex = 1;
            this.rdoVVIStyleNeedle.TabStop = true;
            this.rdoVVIStyleNeedle.Text = "Needle";
            this.rdoVVIStyleNeedle.UseVisualStyleBackColor = true;
            this.rdoVVIStyleNeedle.CheckedChanged += new System.EventHandler(this.rdoVVIStyleNeedle_CheckedChanged);
            // 
            // rdoVVIStyleTape
            // 
            this.rdoVVIStyleTape.AutoSize = true;
            this.rdoVVIStyleTape.Location = new System.Drawing.Point(16, 23);
            this.rdoVVIStyleTape.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoVVIStyleTape.Name = "rdoVVIStyleTape";
            this.rdoVVIStyleTape.Size = new System.Drawing.Size(70, 24);
            this.rdoVVIStyleTape.TabIndex = 0;
            this.rdoVVIStyleTape.TabStop = true;
            this.rdoVVIStyleTape.Text = "Tape";
            this.rdoVVIStyleTape.UseVisualStyleBackColor = true;
            this.rdoVVIStyleTape.CheckedChanged += new System.EventHandler(this.rdoVVIStyleTape_CheckedChanged);
            // 
            // chkStandbyADI
            // 
            this.chkStandbyADI.AutoSize = true;
            this.chkStandbyADI.Location = new System.Drawing.Point(9, 499);
            this.chkStandbyADI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkStandbyADI.Name = "chkStandbyADI";
            this.chkStandbyADI.Size = new System.Drawing.Size(220, 24);
            this.chkStandbyADI.TabIndex = 91;
            this.chkStandbyADI.Text = "Standby Attitude Indicator";
            this.chkStandbyADI.UseVisualStyleBackColor = true;
            this.chkStandbyADI.CheckedChanged += new System.EventHandler(this.chkStandbyADI_CheckedChanged);
            // 
            // chkHSI
            // 
            this.chkHSI.AutoSize = true;
            this.chkHSI.Location = new System.Drawing.Point(9, 429);
            this.chkHSI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkHSI.Name = "chkHSI";
            this.chkHSI.Size = new System.Drawing.Size(282, 24);
            this.chkHSI.TabIndex = 33;
            this.chkHSI.Text = "Horizontal Situation Indicator (HSI)";
            this.chkHSI.UseVisualStyleBackColor = true;
            this.chkHSI.CheckedChanged += new System.EventHandler(this.chkHSI_CheckedChanged);
            // 
            // pbRecoverCompass
            // 
            this.pbRecoverCompass.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCompass.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCompass.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCompass.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCompass.Location = new System.Drawing.Point(400, 323);
            this.pbRecoverCompass.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverCompass.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverCompass.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverCompass.Name = "pbRecoverCompass";
            this.pbRecoverCompass.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverCompass.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverCompass.TabIndex = 90;
            this.pbRecoverCompass.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverCompass, "Recover");
            // 
            // pbRecoverBackupADI
            // 
            this.pbRecoverBackupADI.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverBackupADI.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverBackupADI.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverBackupADI.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverBackupADI.Location = new System.Drawing.Point(400, 499);
            this.pbRecoverBackupADI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverBackupADI.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverBackupADI.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverBackupADI.Name = "pbRecoverBackupADI";
            this.pbRecoverBackupADI.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverBackupADI.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverBackupADI.TabIndex = 92;
            this.pbRecoverBackupADI.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverBackupADI, "Recover");
            // 
            // pbRecoverHSI
            // 
            this.pbRecoverHSI.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHSI.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHSI.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHSI.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHSI.Location = new System.Drawing.Point(400, 429);
            this.pbRecoverHSI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverHSI.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverHSI.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverHSI.Name = "pbRecoverHSI";
            this.pbRecoverHSI.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverHSI.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverHSI.TabIndex = 51;
            this.pbRecoverHSI.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverHSI, "Recover");
            // 
            // pbRecoverVVI
            // 
            this.pbRecoverVVI.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverVVI.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverVVI.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverVVI.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverVVI.Location = new System.Drawing.Point(400, 535);
            this.pbRecoverVVI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverVVI.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverVVI.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverVVI.Name = "pbRecoverVVI";
            this.pbRecoverVVI.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverVVI.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverVVI.TabIndex = 50;
            this.pbRecoverVVI.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverVVI, "Recover");
            // 
            // chkCabinPress
            // 
            this.chkCabinPress.AutoSize = true;
            this.chkCabinPress.Location = new System.Drawing.Point(9, 287);
            this.chkCabinPress.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkCabinPress.Name = "chkCabinPress";
            this.chkCabinPress.Size = new System.Drawing.Size(210, 24);
            this.chkCabinPress.TabIndex = 62;
            this.chkCabinPress.Text = "Cabin Pressure Altimeter";
            this.chkCabinPress.UseVisualStyleBackColor = true;
            this.chkCabinPress.CheckedChanged += new System.EventHandler(this.chkCabinPress_CheckedChanged);
            // 
            // pbRecoverCabinPress
            // 
            this.pbRecoverCabinPress.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCabinPress.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCabinPress.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCabinPress.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCabinPress.Location = new System.Drawing.Point(400, 287);
            this.pbRecoverCabinPress.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverCabinPress.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverCabinPress.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverCabinPress.Name = "pbRecoverCabinPress";
            this.pbRecoverCabinPress.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverCabinPress.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverCabinPress.TabIndex = 63;
            this.pbRecoverCabinPress.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverCabinPress, "Recover");
            // 
            // chkAirspeedIndicator
            // 
            this.chkAirspeedIndicator.AutoSize = true;
            this.chkAirspeedIndicator.Location = new System.Drawing.Point(9, 48);
            this.chkAirspeedIndicator.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkAirspeedIndicator.Name = "chkAirspeedIndicator";
            this.chkAirspeedIndicator.Size = new System.Drawing.Size(252, 24);
            this.chkAirspeedIndicator.TabIndex = 1;
            this.chkAirspeedIndicator.Text = "Airspeed Indicator/Mach Meter";
            this.chkAirspeedIndicator.UseVisualStyleBackColor = true;
            this.chkAirspeedIndicator.CheckedChanged += new System.EventHandler(this.chkAirspeedIndicator_CheckedChanged);
            // 
            // pbRecoverAccelerometer
            // 
            this.pbRecoverAccelerometer.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAccelerometer.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAccelerometer.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAccelerometer.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAccelerometer.Location = new System.Drawing.Point(400, 9);
            this.pbRecoverAccelerometer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverAccelerometer.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverAccelerometer.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverAccelerometer.Name = "pbRecoverAccelerometer";
            this.pbRecoverAccelerometer.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverAccelerometer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverAccelerometer.TabIndex = 88;
            this.pbRecoverAccelerometer.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverAccelerometer, "Recover");
            // 
            // chkVVI
            // 
            this.chkVVI.AutoSize = true;
            this.chkVVI.Location = new System.Drawing.Point(9, 535);
            this.chkVVI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkVVI.Name = "chkVVI";
            this.chkVVI.Size = new System.Drawing.Size(254, 24);
            this.chkVVI.TabIndex = 43;
            this.chkVVI.Text = "Vertical Velocity Indicator (VVI)";
            this.chkVVI.UseVisualStyleBackColor = true;
            this.chkVVI.CheckedChanged += new System.EventHandler(this.chkVVI_CheckedChanged);
            // 
            // chkAltimeter
            // 
            this.chkAltimeter.AutoSize = true;
            this.chkAltimeter.Location = new System.Drawing.Point(9, 83);
            this.chkAltimeter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkAltimeter.Name = "chkAltimeter";
            this.chkAltimeter.Size = new System.Drawing.Size(98, 24);
            this.chkAltimeter.TabIndex = 2;
            this.chkAltimeter.Text = "Altimeter";
            this.chkAltimeter.UseVisualStyleBackColor = true;
            this.chkAltimeter.CheckedChanged += new System.EventHandler(this.chkAltimeter_CheckedChanged);
            // 
            // chkISIS
            // 
            this.chkISIS.AutoSize = true;
            this.chkISIS.Location = new System.Drawing.Point(9, 464);
            this.chkISIS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkISIS.Name = "chkISIS";
            this.chkISIS.Size = new System.Drawing.Size(356, 24);
            this.chkISIS.TabIndex = 36;
            this.chkISIS.Text = "Integrated Standby Instrument System (ISIS)";
            this.chkISIS.UseVisualStyleBackColor = true;
            this.chkISIS.CheckedChanged += new System.EventHandler(this.chkISIS_CheckedChanged);
            // 
            // chkCompass
            // 
            this.chkCompass.AutoSize = true;
            this.chkCompass.Location = new System.Drawing.Point(9, 323);
            this.chkCompass.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkCompass.Name = "chkCompass";
            this.chkCompass.Size = new System.Drawing.Size(102, 24);
            this.chkCompass.TabIndex = 89;
            this.chkCompass.Text = "Compass";
            this.chkCompass.UseVisualStyleBackColor = true;
            this.chkCompass.CheckedChanged += new System.EventHandler(this.chkCompass_CheckedChanged);
            // 
            // pbRecoverISIS
            // 
            this.pbRecoverISIS.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverISIS.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverISIS.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverISIS.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverISIS.Location = new System.Drawing.Point(400, 464);
            this.pbRecoverISIS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverISIS.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverISIS.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverISIS.Name = "pbRecoverISIS";
            this.pbRecoverISIS.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverISIS.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverISIS.TabIndex = 84;
            this.pbRecoverISIS.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverISIS, "Recover");
            // 
            // chkADI
            // 
            this.chkADI.AutoSize = true;
            this.chkADI.Location = new System.Drawing.Point(9, 252);
            this.chkADI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkADI.Name = "chkADI";
            this.chkADI.Size = new System.Drawing.Size(259, 24);
            this.chkADI.TabIndex = 8;
            this.chkADI.Text = "Attitude Director Indicator (ADI)";
            this.chkADI.UseVisualStyleBackColor = true;
            this.chkADI.CheckedChanged += new System.EventHandler(this.chkADI_CheckedChanged);
            // 
            // pbRecoverADI
            // 
            this.pbRecoverADI.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverADI.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverADI.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverADI.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverADI.Location = new System.Drawing.Point(400, 252);
            this.pbRecoverADI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverADI.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverADI.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverADI.Name = "pbRecoverADI";
            this.pbRecoverADI.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverADI.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverADI.TabIndex = 28;
            this.pbRecoverADI.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverADI, "Recover");
            // 
            // chkAOAIndicator
            // 
            this.chkAOAIndicator.AutoSize = true;
            this.chkAOAIndicator.Location = new System.Drawing.Point(9, 216);
            this.chkAOAIndicator.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkAOAIndicator.Name = "chkAOAIndicator";
            this.chkAOAIndicator.Size = new System.Drawing.Size(258, 24);
            this.chkAOAIndicator.TabIndex = 7;
            this.chkAOAIndicator.Text = "Angle of Attack (AOA) Indicator";
            this.chkAOAIndicator.UseVisualStyleBackColor = true;
            this.chkAOAIndicator.CheckedChanged += new System.EventHandler(this.chkAOAIndicator_CheckedChanged);
            // 
            // pbRecoverAOAIndicator
            // 
            this.pbRecoverAOAIndicator.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAOAIndicator.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAOAIndicator.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAOAIndicator.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAOAIndicator.Location = new System.Drawing.Point(400, 216);
            this.pbRecoverAOAIndicator.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverAOAIndicator.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverAOAIndicator.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverAOAIndicator.Name = "pbRecoverAOAIndicator";
            this.pbRecoverAOAIndicator.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverAOAIndicator.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverAOAIndicator.TabIndex = 32;
            this.pbRecoverAOAIndicator.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverAOAIndicator, "Recover");
            // 
            // pbRecoverAltimeter
            // 
            this.pbRecoverAltimeter.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAltimeter.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAltimeter.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAltimeter.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAltimeter.Location = new System.Drawing.Point(400, 88);
            this.pbRecoverAltimeter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverAltimeter.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverAltimeter.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverAltimeter.Name = "pbRecoverAltimeter";
            this.pbRecoverAltimeter.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverAltimeter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverAltimeter.TabIndex = 30;
            this.pbRecoverAltimeter.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverAltimeter, "Recover");
            // 
            // grpAltimeterStyle
            // 
            this.grpAltimeterStyle.Controls.Add(this.rdoAltimeterStyleDigital);
            this.grpAltimeterStyle.Controls.Add(this.rdoAltimeterStyleElectromechanical);
            this.grpAltimeterStyle.Location = new System.Drawing.Point(32, 123);
            this.grpAltimeterStyle.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpAltimeterStyle.Name = "grpAltimeterStyle";
            this.grpAltimeterStyle.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpAltimeterStyle.Size = new System.Drawing.Size(292, 72);
            this.grpAltimeterStyle.TabIndex = 4;
            this.grpAltimeterStyle.TabStop = false;
            this.grpAltimeterStyle.Text = "Style";
            // 
            // rdoAltimeterStyleDigital
            // 
            this.rdoAltimeterStyleDigital.AutoSize = true;
            this.rdoAltimeterStyleDigital.Location = new System.Drawing.Point(186, 29);
            this.rdoAltimeterStyleDigital.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoAltimeterStyleDigital.Name = "rdoAltimeterStyleDigital";
            this.rdoAltimeterStyleDigital.Size = new System.Drawing.Size(78, 24);
            this.rdoAltimeterStyleDigital.TabIndex = 1;
            this.rdoAltimeterStyleDigital.TabStop = true;
            this.rdoAltimeterStyleDigital.Text = "Digital";
            this.rdoAltimeterStyleDigital.UseVisualStyleBackColor = true;
            this.rdoAltimeterStyleDigital.CheckedChanged += new System.EventHandler(this.rdoAltimeterStyleDigital_CheckedChanged);
            // 
            // rdoAltimeterStyleElectromechanical
            // 
            this.rdoAltimeterStyleElectromechanical.AutoSize = true;
            this.rdoAltimeterStyleElectromechanical.Location = new System.Drawing.Point(9, 29);
            this.rdoAltimeterStyleElectromechanical.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoAltimeterStyleElectromechanical.Name = "rdoAltimeterStyleElectromechanical";
            this.rdoAltimeterStyleElectromechanical.Size = new System.Drawing.Size(164, 24);
            this.rdoAltimeterStyleElectromechanical.TabIndex = 0;
            this.rdoAltimeterStyleElectromechanical.TabStop = true;
            this.rdoAltimeterStyleElectromechanical.Text = "Electromechanical";
            this.rdoAltimeterStyleElectromechanical.UseVisualStyleBackColor = true;
            this.rdoAltimeterStyleElectromechanical.CheckedChanged += new System.EventHandler(this.rdoAltimeterStyleElectromechanical_CheckedChanged);
            // 
            // tabEW
            // 
            this.tabEW.Controls.Add(this.chkAzimuthIndicator);
            this.tabEW.Controls.Add(this.pbRecoverAzimuthIndicator);
            this.tabEW.Controls.Add(this.chkCMDSPanel);
            this.tabEW.Controls.Add(this.pbRecoverCMDS);
            this.tabEW.Controls.Add(this.grpAzimuthIndicatorStyle);
            this.tabEW.Location = new System.Drawing.Point(4, 54);
            this.tabEW.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabEW.Name = "tabEW";
            this.tabEW.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabEW.Size = new System.Drawing.Size(768, 658);
            this.tabEW.TabIndex = 11;
            this.tabEW.Text = "EW Suite";
            this.tabEW.UseVisualStyleBackColor = true;
            // 
            // chkAzimuthIndicator
            // 
            this.chkAzimuthIndicator.AutoSize = true;
            this.chkAzimuthIndicator.Location = new System.Drawing.Point(9, 9);
            this.chkAzimuthIndicator.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkAzimuthIndicator.Name = "chkAzimuthIndicator";
            this.chkAzimuthIndicator.Size = new System.Drawing.Size(212, 24);
            this.chkAzimuthIndicator.TabIndex = 76;
            this.chkAzimuthIndicator.Text = "Azimuth Indicator (RWR)";
            this.chkAzimuthIndicator.UseVisualStyleBackColor = true;
            this.chkAzimuthIndicator.CheckedChanged += new System.EventHandler(this.chkAzimuthIndicator_CheckedChanged);
            // 
            // pbRecoverAzimuthIndicator
            // 
            this.pbRecoverAzimuthIndicator.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAzimuthIndicator.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAzimuthIndicator.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAzimuthIndicator.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAzimuthIndicator.Location = new System.Drawing.Point(396, 11);
            this.pbRecoverAzimuthIndicator.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverAzimuthIndicator.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverAzimuthIndicator.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverAzimuthIndicator.Name = "pbRecoverAzimuthIndicator";
            this.pbRecoverAzimuthIndicator.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverAzimuthIndicator.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverAzimuthIndicator.TabIndex = 78;
            this.pbRecoverAzimuthIndicator.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverAzimuthIndicator, "Recover");
            // 
            // chkCMDSPanel
            // 
            this.chkCMDSPanel.AutoSize = true;
            this.chkCMDSPanel.Location = new System.Drawing.Point(9, 231);
            this.chkCMDSPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkCMDSPanel.Name = "chkCMDSPanel";
            this.chkCMDSPanel.Size = new System.Drawing.Size(126, 24);
            this.chkCMDSPanel.TabIndex = 79;
            this.chkCMDSPanel.Text = "CMDS Panel";
            this.chkCMDSPanel.UseVisualStyleBackColor = true;
            this.chkCMDSPanel.CheckedChanged += new System.EventHandler(this.chkCMDSPanel_CheckedChanged);
            // 
            // pbRecoverCMDS
            // 
            this.pbRecoverCMDS.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCMDS.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCMDS.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCMDS.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCMDS.Location = new System.Drawing.Point(396, 231);
            this.pbRecoverCMDS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverCMDS.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverCMDS.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverCMDS.Name = "pbRecoverCMDS";
            this.pbRecoverCMDS.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverCMDS.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverCMDS.TabIndex = 80;
            this.pbRecoverCMDS.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverCMDS, "Recover");
            // 
            // grpAzimuthIndicatorStyle
            // 
            this.grpAzimuthIndicatorStyle.Controls.Add(this.rdoAzimuthIndicatorStyleScope);
            this.grpAzimuthIndicatorStyle.Controls.Add(this.rdoAzimuthIndicatorStyleDigital);
            this.grpAzimuthIndicatorStyle.Controls.Add(this.grpAzimuthIndicatorBezelTypes);
            this.grpAzimuthIndicatorStyle.Controls.Add(this.grpAzimuthIndicatorDigitalTypes);
            this.grpAzimuthIndicatorStyle.Location = new System.Drawing.Point(16, 43);
            this.grpAzimuthIndicatorStyle.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpAzimuthIndicatorStyle.Name = "grpAzimuthIndicatorStyle";
            this.grpAzimuthIndicatorStyle.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpAzimuthIndicatorStyle.Size = new System.Drawing.Size(404, 178);
            this.grpAzimuthIndicatorStyle.TabIndex = 77;
            this.grpAzimuthIndicatorStyle.TabStop = false;
            this.grpAzimuthIndicatorStyle.Text = "Style";
            // 
            // rdoAzimuthIndicatorStyleScope
            // 
            this.rdoAzimuthIndicatorStyleScope.AutoSize = true;
            this.rdoAzimuthIndicatorStyleScope.Location = new System.Drawing.Point(9, 25);
            this.rdoAzimuthIndicatorStyleScope.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoAzimuthIndicatorStyleScope.Name = "rdoAzimuthIndicatorStyleScope";
            this.rdoAzimuthIndicatorStyleScope.Size = new System.Drawing.Size(116, 24);
            this.rdoAzimuthIndicatorStyleScope.TabIndex = 11;
            this.rdoAzimuthIndicatorStyleScope.TabStop = true;
            this.rdoAzimuthIndicatorStyleScope.Text = "CRT Scope";
            this.rdoAzimuthIndicatorStyleScope.UseVisualStyleBackColor = true;
            this.rdoAzimuthIndicatorStyleScope.CheckedChanged += new System.EventHandler(this.rdoAzimuthIndicatorStyleScope_CheckedChanged);
            // 
            // rdoAzimuthIndicatorStyleDigital
            // 
            this.rdoAzimuthIndicatorStyleDigital.AutoSize = true;
            this.rdoAzimuthIndicatorStyleDigital.Location = new System.Drawing.Point(8, 60);
            this.rdoAzimuthIndicatorStyleDigital.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoAzimuthIndicatorStyleDigital.Name = "rdoAzimuthIndicatorStyleDigital";
            this.rdoAzimuthIndicatorStyleDigital.Size = new System.Drawing.Size(117, 24);
            this.rdoAzimuthIndicatorStyleDigital.TabIndex = 12;
            this.rdoAzimuthIndicatorStyleDigital.TabStop = true;
            this.rdoAzimuthIndicatorStyleDigital.Text = "TFT Display";
            this.rdoAzimuthIndicatorStyleDigital.UseVisualStyleBackColor = true;
            this.rdoAzimuthIndicatorStyleDigital.CheckedChanged += new System.EventHandler(this.rdoAzimuthIndicatorStyleDigital_CheckedChanged);
            // 
            // grpAzimuthIndicatorBezelTypes
            // 
            this.grpAzimuthIndicatorBezelTypes.Controls.Add(this.rdoAzimuthIndicatorNoBezel);
            this.grpAzimuthIndicatorBezelTypes.Controls.Add(this.rdoRWRHAFBezelType);
            this.grpAzimuthIndicatorBezelTypes.Controls.Add(this.rdoRWRIP1310BezelType);
            this.grpAzimuthIndicatorBezelTypes.Location = new System.Drawing.Point(27, 103);
            this.grpAzimuthIndicatorBezelTypes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpAzimuthIndicatorBezelTypes.Name = "grpAzimuthIndicatorBezelTypes";
            this.grpAzimuthIndicatorBezelTypes.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpAzimuthIndicatorBezelTypes.Size = new System.Drawing.Size(358, 66);
            this.grpAzimuthIndicatorBezelTypes.TabIndex = 13;
            this.grpAzimuthIndicatorBezelTypes.TabStop = false;
            this.grpAzimuthIndicatorBezelTypes.Text = "Bezel Type";
            // 
            // rdoAzimuthIndicatorNoBezel
            // 
            this.rdoAzimuthIndicatorNoBezel.AutoSize = true;
            this.rdoAzimuthIndicatorNoBezel.Location = new System.Drawing.Point(274, 29);
            this.rdoAzimuthIndicatorNoBezel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoAzimuthIndicatorNoBezel.Name = "rdoAzimuthIndicatorNoBezel";
            this.rdoAzimuthIndicatorNoBezel.Size = new System.Drawing.Size(72, 24);
            this.rdoAzimuthIndicatorNoBezel.TabIndex = 3;
            this.rdoAzimuthIndicatorNoBezel.TabStop = true;
            this.rdoAzimuthIndicatorNoBezel.Text = "None";
            this.rdoAzimuthIndicatorNoBezel.UseVisualStyleBackColor = true;
            this.rdoAzimuthIndicatorNoBezel.CheckedChanged += new System.EventHandler(this.rdoAzimuthIndicatorNoBezel_CheckedChanged);
            // 
            // rdoRWRHAFBezelType
            // 
            this.rdoRWRHAFBezelType.AutoSize = true;
            this.rdoRWRHAFBezelType.Location = new System.Drawing.Point(196, 29);
            this.rdoRWRHAFBezelType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoRWRHAFBezelType.Name = "rdoRWRHAFBezelType";
            this.rdoRWRHAFBezelType.Size = new System.Drawing.Size(67, 24);
            this.rdoRWRHAFBezelType.TabIndex = 2;
            this.rdoRWRHAFBezelType.TabStop = true;
            this.rdoRWRHAFBezelType.Text = "HAF";
            this.rdoRWRHAFBezelType.UseVisualStyleBackColor = true;
            this.rdoRWRHAFBezelType.CheckedChanged += new System.EventHandler(this.rdoRWRHAFBezelType_CheckedChanged);
            // 
            // rdoRWRIP1310BezelType
            // 
            this.rdoRWRIP1310BezelType.AutoSize = true;
            this.rdoRWRIP1310BezelType.Location = new System.Drawing.Point(0, 29);
            this.rdoRWRIP1310BezelType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoRWRIP1310BezelType.Name = "rdoRWRIP1310BezelType";
            this.rdoRWRIP1310BezelType.Size = new System.Drawing.Size(174, 24);
            this.rdoRWRIP1310BezelType.TabIndex = 1;
            this.rdoRWRIP1310BezelType.TabStop = true;
            this.rdoRWRIP1310BezelType.Text = "IP-1310/ALR (USA)";
            this.rdoRWRIP1310BezelType.UseVisualStyleBackColor = true;
            this.rdoRWRIP1310BezelType.CheckedChanged += new System.EventHandler(this.rdoRWRIP1310BezelType_CheckedChanged);
            // 
            // grpAzimuthIndicatorDigitalTypes
            // 
            this.grpAzimuthIndicatorDigitalTypes.Controls.Add(this.rdoTTD);
            this.grpAzimuthIndicatorDigitalTypes.Controls.Add(this.rdoATDPlus);
            this.grpAzimuthIndicatorDigitalTypes.Location = new System.Drawing.Point(27, 103);
            this.grpAzimuthIndicatorDigitalTypes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpAzimuthIndicatorDigitalTypes.Name = "grpAzimuthIndicatorDigitalTypes";
            this.grpAzimuthIndicatorDigitalTypes.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpAzimuthIndicatorDigitalTypes.Size = new System.Drawing.Size(358, 66);
            this.grpAzimuthIndicatorDigitalTypes.TabIndex = 13;
            this.grpAzimuthIndicatorDigitalTypes.TabStop = false;
            this.grpAzimuthIndicatorDigitalTypes.Text = "Display Type";
            // 
            // rdoTTD
            // 
            this.rdoTTD.AutoSize = true;
            this.rdoTTD.Enabled = false;
            this.rdoTTD.Location = new System.Drawing.Point(98, 29);
            this.rdoTTD.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoTTD.Name = "rdoTTD";
            this.rdoTTD.Size = new System.Drawing.Size(64, 24);
            this.rdoTTD.TabIndex = 2;
            this.rdoTTD.TabStop = true;
            this.rdoTTD.Text = "TTD";
            this.rdoTTD.UseVisualStyleBackColor = true;
            // 
            // rdoATDPlus
            // 
            this.rdoATDPlus.AutoSize = true;
            this.rdoATDPlus.Location = new System.Drawing.Point(9, 29);
            this.rdoATDPlus.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoATDPlus.Name = "rdoATDPlus";
            this.rdoATDPlus.Size = new System.Drawing.Size(75, 24);
            this.rdoATDPlus.TabIndex = 1;
            this.rdoATDPlus.TabStop = true;
            this.rdoATDPlus.Text = "ATD+";
            this.rdoATDPlus.UseVisualStyleBackColor = true;
            this.rdoATDPlus.CheckedChanged += new System.EventHandler(this.rdoATDPlus_CheckedChanged);
            // 
            // tabEngineInstruments
            // 
            this.tabEngineInstruments.Controls.Add(this.chkFuelFlow);
            this.tabEngineInstruments.Controls.Add(this.pbRecoverFuelFlow);
            this.tabEngineInstruments.Controls.Add(this.gbEngine1Instros);
            this.tabEngineInstruments.Controls.Add(this.gbEngine2Instros);
            this.tabEngineInstruments.Controls.Add(this.gbFuelQuantityOptions);
            this.tabEngineInstruments.Controls.Add(this.chkFuelQty);
            this.tabEngineInstruments.Controls.Add(this.pbRecoverFuelQuantity);
            this.tabEngineInstruments.Location = new System.Drawing.Point(4, 54);
            this.tabEngineInstruments.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabEngineInstruments.Name = "tabEngineInstruments";
            this.tabEngineInstruments.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabEngineInstruments.Size = new System.Drawing.Size(768, 658);
            this.tabEngineInstruments.TabIndex = 10;
            this.tabEngineInstruments.Text = "Engine & Fuel Instruments";
            this.tabEngineInstruments.UseVisualStyleBackColor = true;
            // 
            // chkFuelFlow
            // 
            this.chkFuelFlow.AutoSize = true;
            this.chkFuelFlow.Location = new System.Drawing.Point(18, 400);
            this.chkFuelFlow.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkFuelFlow.Name = "chkFuelFlow";
            this.chkFuelFlow.Size = new System.Drawing.Size(169, 24);
            this.chkFuelFlow.TabIndex = 29;
            this.chkFuelFlow.Text = "Fuel Flow Indicator";
            this.chkFuelFlow.UseVisualStyleBackColor = true;
            this.chkFuelFlow.CheckedChanged += new System.EventHandler(this.chkFuelFlow_CheckedChanged);
            // 
            // pbRecoverFuelFlow
            // 
            this.pbRecoverFuelFlow.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFuelFlow.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFuelFlow.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFuelFlow.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFuelFlow.Location = new System.Drawing.Point(410, 400);
            this.pbRecoverFuelFlow.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverFuelFlow.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverFuelFlow.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverFuelFlow.Name = "pbRecoverFuelFlow";
            this.pbRecoverFuelFlow.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverFuelFlow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverFuelFlow.TabIndex = 46;
            this.pbRecoverFuelFlow.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverFuelFlow, "Recover");
            // 
            // gbEngine1Instros
            // 
            this.gbEngine1Instros.Controls.Add(this.chkFTIT1);
            this.gbEngine1Instros.Controls.Add(this.pbRecoverRPM1);
            this.gbEngine1Instros.Controls.Add(this.pbRecoverOil1);
            this.gbEngine1Instros.Controls.Add(this.pbRecoverNozPos1);
            this.gbEngine1Instros.Controls.Add(this.pbRecoverFTIT1);
            this.gbEngine1Instros.Controls.Add(this.chkRPM1);
            this.gbEngine1Instros.Controls.Add(this.chkOIL1);
            this.gbEngine1Instros.Controls.Add(this.chkNOZ1);
            this.gbEngine1Instros.Location = new System.Drawing.Point(9, 9);
            this.gbEngine1Instros.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbEngine1Instros.Name = "gbEngine1Instros";
            this.gbEngine1Instros.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbEngine1Instros.Size = new System.Drawing.Size(446, 186);
            this.gbEngine1Instros.TabIndex = 56;
            this.gbEngine1Instros.TabStop = false;
            this.gbEngine1Instros.Text = "Engine 1 Instruments";
            // 
            // chkFTIT1
            // 
            this.chkFTIT1.AutoSize = true;
            this.chkFTIT1.Location = new System.Drawing.Point(9, 34);
            this.chkFTIT1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkFTIT1.Name = "chkFTIT1";
            this.chkFTIT1.Size = new System.Drawing.Size(121, 24);
            this.chkFTIT1.TabIndex = 20;
            this.chkFTIT1.Text = "FTIT Gauge";
            this.chkFTIT1.UseVisualStyleBackColor = true;
            this.chkFTIT1.CheckedChanged += new System.EventHandler(this.chkFTIT1_CheckedChanged);
            // 
            // pbRecoverRPM1
            // 
            this.pbRecoverRPM1.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRPM1.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRPM1.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRPM1.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRPM1.Location = new System.Drawing.Point(400, 140);
            this.pbRecoverRPM1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverRPM1.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverRPM1.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverRPM1.Name = "pbRecoverRPM1";
            this.pbRecoverRPM1.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverRPM1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverRPM1.TabIndex = 40;
            this.pbRecoverRPM1.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverRPM1, "Recover");
            // 
            // pbRecoverOil1
            // 
            this.pbRecoverOil1.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverOil1.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverOil1.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverOil1.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverOil1.Location = new System.Drawing.Point(400, 105);
            this.pbRecoverOil1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverOil1.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverOil1.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverOil1.Name = "pbRecoverOil1";
            this.pbRecoverOil1.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverOil1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverOil1.TabIndex = 39;
            this.pbRecoverOil1.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverOil1, "Recover");
            // 
            // pbRecoverNozPos1
            // 
            this.pbRecoverNozPos1.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNozPos1.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNozPos1.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNozPos1.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNozPos1.Location = new System.Drawing.Point(400, 69);
            this.pbRecoverNozPos1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverNozPos1.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverNozPos1.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverNozPos1.Name = "pbRecoverNozPos1";
            this.pbRecoverNozPos1.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverNozPos1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverNozPos1.TabIndex = 38;
            this.pbRecoverNozPos1.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverNozPos1, "Recover");
            // 
            // pbRecoverFTIT1
            // 
            this.pbRecoverFTIT1.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFTIT1.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFTIT1.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFTIT1.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFTIT1.Location = new System.Drawing.Point(400, 34);
            this.pbRecoverFTIT1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverFTIT1.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverFTIT1.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverFTIT1.Name = "pbRecoverFTIT1";
            this.pbRecoverFTIT1.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverFTIT1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverFTIT1.TabIndex = 37;
            this.pbRecoverFTIT1.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverFTIT1, "Recover");
            // 
            // chkRPM1
            // 
            this.chkRPM1.AutoSize = true;
            this.chkRPM1.Location = new System.Drawing.Point(9, 140);
            this.chkRPM1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkRPM1.Name = "chkRPM1";
            this.chkRPM1.Size = new System.Drawing.Size(123, 24);
            this.chkRPM1.TabIndex = 23;
            this.chkRPM1.Text = "RPM Gauge";
            this.chkRPM1.UseVisualStyleBackColor = true;
            this.chkRPM1.CheckedChanged += new System.EventHandler(this.chkRPM1_CheckedChanged);
            // 
            // chkOIL1
            // 
            this.chkOIL1.AutoSize = true;
            this.chkOIL1.Location = new System.Drawing.Point(9, 105);
            this.chkOIL1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkOIL1.Name = "chkOIL1";
            this.chkOIL1.Size = new System.Drawing.Size(173, 24);
            this.chkOIL1.TabIndex = 22;
            this.chkOIL1.Text = "Oil Pressure Gauge";
            this.chkOIL1.UseVisualStyleBackColor = true;
            this.chkOIL1.CheckedChanged += new System.EventHandler(this.chkOIL1_CheckedChanged);
            // 
            // chkNOZ1
            // 
            this.chkNOZ1.AutoSize = true;
            this.chkNOZ1.Location = new System.Drawing.Point(9, 69);
            this.chkNOZ1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkNOZ1.Name = "chkNOZ1";
            this.chkNOZ1.Size = new System.Drawing.Size(209, 24);
            this.chkNOZ1.TabIndex = 21;
            this.chkNOZ1.Text = "Nozzle Position Indicator";
            this.chkNOZ1.UseVisualStyleBackColor = true;
            this.chkNOZ1.CheckedChanged += new System.EventHandler(this.chkNOZ1_CheckedChanged);
            // 
            // gbEngine2Instros
            // 
            this.gbEngine2Instros.Controls.Add(this.chkFTIT2);
            this.gbEngine2Instros.Controls.Add(this.chkNOZ2);
            this.gbEngine2Instros.Controls.Add(this.pbRecoverRPM2);
            this.gbEngine2Instros.Controls.Add(this.pbRecoverOil2);
            this.gbEngine2Instros.Controls.Add(this.chkOIL2);
            this.gbEngine2Instros.Controls.Add(this.pbRecoverNozPos2);
            this.gbEngine2Instros.Controls.Add(this.pbRecoverFTIT2);
            this.gbEngine2Instros.Controls.Add(this.chkRPM2);
            this.gbEngine2Instros.Location = new System.Drawing.Point(9, 205);
            this.gbEngine2Instros.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbEngine2Instros.Name = "gbEngine2Instros";
            this.gbEngine2Instros.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbEngine2Instros.Size = new System.Drawing.Size(446, 186);
            this.gbEngine2Instros.TabIndex = 57;
            this.gbEngine2Instros.TabStop = false;
            this.gbEngine2Instros.Text = "Engine 2 Instruments";
            // 
            // chkFTIT2
            // 
            this.chkFTIT2.AutoSize = true;
            this.chkFTIT2.Location = new System.Drawing.Point(9, 32);
            this.chkFTIT2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkFTIT2.Name = "chkFTIT2";
            this.chkFTIT2.Size = new System.Drawing.Size(121, 24);
            this.chkFTIT2.TabIndex = 24;
            this.chkFTIT2.Text = "FTIT Gauge";
            this.chkFTIT2.UseVisualStyleBackColor = true;
            this.chkFTIT2.CheckedChanged += new System.EventHandler(this.chkFTIT2_CheckedChanged);
            // 
            // chkNOZ2
            // 
            this.chkNOZ2.AutoSize = true;
            this.chkNOZ2.Location = new System.Drawing.Point(9, 68);
            this.chkNOZ2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkNOZ2.Name = "chkNOZ2";
            this.chkNOZ2.Size = new System.Drawing.Size(209, 24);
            this.chkNOZ2.TabIndex = 25;
            this.chkNOZ2.Text = "Nozzle Position Indicator";
            this.chkNOZ2.UseVisualStyleBackColor = true;
            this.chkNOZ2.CheckedChanged += new System.EventHandler(this.chkNOZ2_CheckedChanged);
            // 
            // pbRecoverRPM2
            // 
            this.pbRecoverRPM2.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRPM2.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRPM2.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRPM2.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRPM2.Location = new System.Drawing.Point(400, 138);
            this.pbRecoverRPM2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverRPM2.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverRPM2.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverRPM2.Name = "pbRecoverRPM2";
            this.pbRecoverRPM2.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverRPM2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverRPM2.TabIndex = 44;
            this.pbRecoverRPM2.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverRPM2, "Recover");
            // 
            // pbRecoverOil2
            // 
            this.pbRecoverOil2.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverOil2.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverOil2.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverOil2.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverOil2.Location = new System.Drawing.Point(400, 103);
            this.pbRecoverOil2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverOil2.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverOil2.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverOil2.Name = "pbRecoverOil2";
            this.pbRecoverOil2.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverOil2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverOil2.TabIndex = 43;
            this.pbRecoverOil2.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverOil2, "Recover");
            // 
            // chkOIL2
            // 
            this.chkOIL2.AutoSize = true;
            this.chkOIL2.Location = new System.Drawing.Point(9, 103);
            this.chkOIL2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkOIL2.Name = "chkOIL2";
            this.chkOIL2.Size = new System.Drawing.Size(173, 24);
            this.chkOIL2.TabIndex = 26;
            this.chkOIL2.Text = "Oil Pressure Gauge";
            this.chkOIL2.UseVisualStyleBackColor = true;
            this.chkOIL2.CheckedChanged += new System.EventHandler(this.chkOIL2_CheckedChanged);
            // 
            // pbRecoverNozPos2
            // 
            this.pbRecoverNozPos2.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNozPos2.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNozPos2.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNozPos2.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNozPos2.Location = new System.Drawing.Point(400, 68);
            this.pbRecoverNozPos2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverNozPos2.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverNozPos2.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverNozPos2.Name = "pbRecoverNozPos2";
            this.pbRecoverNozPos2.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverNozPos2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverNozPos2.TabIndex = 42;
            this.pbRecoverNozPos2.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverNozPos2, "Recover");
            // 
            // pbRecoverFTIT2
            // 
            this.pbRecoverFTIT2.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFTIT2.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFTIT2.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFTIT2.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFTIT2.Location = new System.Drawing.Point(400, 32);
            this.pbRecoverFTIT2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverFTIT2.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverFTIT2.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverFTIT2.Name = "pbRecoverFTIT2";
            this.pbRecoverFTIT2.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverFTIT2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverFTIT2.TabIndex = 41;
            this.pbRecoverFTIT2.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverFTIT2, "Recover");
            // 
            // chkRPM2
            // 
            this.chkRPM2.AutoSize = true;
            this.chkRPM2.Location = new System.Drawing.Point(9, 138);
            this.chkRPM2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkRPM2.Name = "chkRPM2";
            this.chkRPM2.Size = new System.Drawing.Size(123, 24);
            this.chkRPM2.TabIndex = 27;
            this.chkRPM2.Text = "RPM Gauge";
            this.chkRPM2.UseVisualStyleBackColor = true;
            this.chkRPM2.CheckedChanged += new System.EventHandler(this.chkRPM2_CheckedChanged);
            // 
            // gbFuelQuantityOptions
            // 
            this.gbFuelQuantityOptions.Controls.Add(this.rdoFuelQuantityDModel);
            this.gbFuelQuantityOptions.Controls.Add(this.rdoFuelQuantityNeedleCModel);
            this.gbFuelQuantityOptions.Location = new System.Drawing.Point(54, 471);
            this.gbFuelQuantityOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbFuelQuantityOptions.Name = "gbFuelQuantityOptions";
            this.gbFuelQuantityOptions.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbFuelQuantityOptions.Size = new System.Drawing.Size(292, 63);
            this.gbFuelQuantityOptions.TabIndex = 67;
            this.gbFuelQuantityOptions.TabStop = false;
            this.gbFuelQuantityOptions.Text = "Needle Style";
            // 
            // rdoFuelQuantityDModel
            // 
            this.rdoFuelQuantityDModel.AutoSize = true;
            this.rdoFuelQuantityDModel.Location = new System.Drawing.Point(124, 29);
            this.rdoFuelQuantityDModel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoFuelQuantityDModel.Name = "rdoFuelQuantityDModel";
            this.rdoFuelQuantityDModel.Size = new System.Drawing.Size(93, 24);
            this.rdoFuelQuantityDModel.TabIndex = 1;
            this.rdoFuelQuantityDModel.TabStop = true;
            this.rdoFuelQuantityDModel.Text = "D Model";
            this.rdoFuelQuantityDModel.UseVisualStyleBackColor = true;
            this.rdoFuelQuantityDModel.CheckedChanged += new System.EventHandler(this.rdoFuelQuantityDModel_CheckedChanged);
            // 
            // rdoFuelQuantityNeedleCModel
            // 
            this.rdoFuelQuantityNeedleCModel.AutoSize = true;
            this.rdoFuelQuantityNeedleCModel.Location = new System.Drawing.Point(20, 29);
            this.rdoFuelQuantityNeedleCModel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoFuelQuantityNeedleCModel.Name = "rdoFuelQuantityNeedleCModel";
            this.rdoFuelQuantityNeedleCModel.Size = new System.Drawing.Size(92, 24);
            this.rdoFuelQuantityNeedleCModel.TabIndex = 0;
            this.rdoFuelQuantityNeedleCModel.TabStop = true;
            this.rdoFuelQuantityNeedleCModel.Text = "C Model";
            this.rdoFuelQuantityNeedleCModel.UseVisualStyleBackColor = true;
            this.rdoFuelQuantityNeedleCModel.CheckedChanged += new System.EventHandler(this.rdoFuelQuantityNeedleCModel_CheckedChanged);
            // 
            // chkFuelQty
            // 
            this.chkFuelQty.AutoSize = true;
            this.chkFuelQty.Location = new System.Drawing.Point(18, 435);
            this.chkFuelQty.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkFuelQty.Name = "chkFuelQty";
            this.chkFuelQty.Size = new System.Drawing.Size(195, 24);
            this.chkFuelQty.TabIndex = 66;
            this.chkFuelQty.Text = "Fuel Quantity Indicator";
            this.chkFuelQty.UseVisualStyleBackColor = true;
            this.chkFuelQty.CheckedChanged += new System.EventHandler(this.chkFuelQty_CheckedChanged);
            // 
            // pbRecoverFuelQuantity
            // 
            this.pbRecoverFuelQuantity.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFuelQuantity.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFuelQuantity.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFuelQuantity.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverFuelQuantity.Location = new System.Drawing.Point(410, 435);
            this.pbRecoverFuelQuantity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverFuelQuantity.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverFuelQuantity.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverFuelQuantity.Name = "pbRecoverFuelQuantity";
            this.pbRecoverFuelQuantity.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverFuelQuantity.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverFuelQuantity.TabIndex = 68;
            this.pbRecoverFuelQuantity.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverFuelQuantity, "Recover");
            // 
            // tabHydraulics
            // 
            this.tabHydraulics.Controls.Add(this.chkHydA);
            this.tabHydraulics.Controls.Add(this.pbRecoverHydA);
            this.tabHydraulics.Controls.Add(this.pbRecoverHydB);
            this.tabHydraulics.Controls.Add(this.chkHydB);
            this.tabHydraulics.Location = new System.Drawing.Point(4, 54);
            this.tabHydraulics.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabHydraulics.Name = "tabHydraulics";
            this.tabHydraulics.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabHydraulics.Size = new System.Drawing.Size(768, 658);
            this.tabHydraulics.TabIndex = 14;
            this.tabHydraulics.Text = "Hydraulic Instruments";
            this.tabHydraulics.UseVisualStyleBackColor = true;
            // 
            // chkHydA
            // 
            this.chkHydA.AutoSize = true;
            this.chkHydA.Location = new System.Drawing.Point(9, 9);
            this.chkHydA.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkHydA.Name = "chkHydA";
            this.chkHydA.Size = new System.Drawing.Size(252, 24);
            this.chkHydA.TabIndex = 69;
            this.chkHydA.Text = "Hydraulic Pressure Indicator A ";
            this.chkHydA.UseVisualStyleBackColor = true;
            this.chkHydA.CheckedChanged += new System.EventHandler(this.chkHydA_CheckedChanged);
            // 
            // pbRecoverHydA
            // 
            this.pbRecoverHydA.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHydA.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHydA.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHydA.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHydA.Location = new System.Drawing.Point(399, 9);
            this.pbRecoverHydA.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverHydA.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverHydA.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverHydA.Name = "pbRecoverHydA";
            this.pbRecoverHydA.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverHydA.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverHydA.TabIndex = 71;
            this.pbRecoverHydA.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverHydA, "Recover");
            // 
            // pbRecoverHydB
            // 
            this.pbRecoverHydB.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHydB.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHydB.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHydB.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverHydB.Location = new System.Drawing.Point(399, 45);
            this.pbRecoverHydB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverHydB.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverHydB.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverHydB.Name = "pbRecoverHydB";
            this.pbRecoverHydB.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverHydB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverHydB.TabIndex = 72;
            this.pbRecoverHydB.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverHydB, "Recover");
            // 
            // chkHydB
            // 
            this.chkHydB.AutoSize = true;
            this.chkHydB.Location = new System.Drawing.Point(9, 43);
            this.chkHydB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkHydB.Name = "chkHydB";
            this.chkHydB.Size = new System.Drawing.Size(248, 24);
            this.chkHydB.TabIndex = 70;
            this.chkHydB.Text = "Hydraulic Pressure Indicator B";
            this.chkHydB.UseVisualStyleBackColor = true;
            this.chkHydB.CheckedChanged += new System.EventHandler(this.chkHydB_CheckedChanged);
            // 
            // tabFaults
            // 
            this.tabFaults.Controls.Add(this.chkPFL);
            this.tabFaults.Controls.Add(this.chkCautionPanel);
            this.tabFaults.Controls.Add(this.pbRecoverCautionPanel);
            this.tabFaults.Controls.Add(this.pbRecoverPFL);
            this.tabFaults.Location = new System.Drawing.Point(4, 54);
            this.tabFaults.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabFaults.Name = "tabFaults";
            this.tabFaults.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabFaults.Size = new System.Drawing.Size(768, 658);
            this.tabFaults.TabIndex = 7;
            this.tabFaults.Text = "Fault Indicators";
            this.tabFaults.UseVisualStyleBackColor = true;
            // 
            // chkPFL
            // 
            this.chkPFL.AutoSize = true;
            this.chkPFL.Location = new System.Drawing.Point(9, 45);
            this.chkPFL.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkPFL.Name = "chkPFL";
            this.chkPFL.Size = new System.Drawing.Size(177, 24);
            this.chkPFL.TabIndex = 74;
            this.chkPFL.Text = "Pilot Fault List (PFL)";
            this.chkPFL.UseVisualStyleBackColor = true;
            this.chkPFL.CheckedChanged += new System.EventHandler(this.chkPFL_CheckedChanged);
            // 
            // chkCautionPanel
            // 
            this.chkCautionPanel.AutoSize = true;
            this.chkCautionPanel.Location = new System.Drawing.Point(9, 9);
            this.chkCautionPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkCautionPanel.Name = "chkCautionPanel";
            this.chkCautionPanel.Size = new System.Drawing.Size(134, 24);
            this.chkCautionPanel.TabIndex = 70;
            this.chkCautionPanel.Text = "Caution Panel";
            this.chkCautionPanel.UseVisualStyleBackColor = true;
            this.chkCautionPanel.CheckedChanged += new System.EventHandler(this.chkCautionPanel_CheckedChanged);
            // 
            // pbRecoverCautionPanel
            // 
            this.pbRecoverCautionPanel.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCautionPanel.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCautionPanel.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCautionPanel.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverCautionPanel.Location = new System.Drawing.Point(400, 9);
            this.pbRecoverCautionPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverCautionPanel.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverCautionPanel.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverCautionPanel.Name = "pbRecoverCautionPanel";
            this.pbRecoverCautionPanel.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverCautionPanel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverCautionPanel.TabIndex = 71;
            this.pbRecoverCautionPanel.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverCautionPanel, "Recover");
            // 
            // pbRecoverPFL
            // 
            this.pbRecoverPFL.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverPFL.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverPFL.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverPFL.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverPFL.Location = new System.Drawing.Point(400, 45);
            this.pbRecoverPFL.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverPFL.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverPFL.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverPFL.Name = "pbRecoverPFL";
            this.pbRecoverPFL.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverPFL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverPFL.TabIndex = 75;
            this.pbRecoverPFL.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverPFL, "Recover");
            // 
            // tabIndexers
            // 
            this.tabIndexers.Controls.Add(this.chkNWSIndexer);
            this.tabIndexers.Controls.Add(this.pbRecoverNWS);
            this.tabIndexers.Controls.Add(this.chkAOAIndexer);
            this.tabIndexers.Controls.Add(this.pbRecoverAOAIndexer);
            this.tabIndexers.Location = new System.Drawing.Point(4, 54);
            this.tabIndexers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabIndexers.Name = "tabIndexers";
            this.tabIndexers.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabIndexers.Size = new System.Drawing.Size(768, 658);
            this.tabIndexers.TabIndex = 13;
            this.tabIndexers.Text = "Indexers";
            this.tabIndexers.UseVisualStyleBackColor = true;
            // 
            // chkNWSIndexer
            // 
            this.chkNWSIndexer.AutoSize = true;
            this.chkNWSIndexer.Location = new System.Drawing.Point(9, 45);
            this.chkNWSIndexer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkNWSIndexer.Name = "chkNWSIndexer";
            this.chkNWSIndexer.Size = new System.Drawing.Size(285, 24);
            this.chkNWSIndexer.TabIndex = 95;
            this.chkNWSIndexer.Text = "Nosewheel Steering (NWS) Indexer";
            this.chkNWSIndexer.UseVisualStyleBackColor = true;
            this.chkNWSIndexer.CheckedChanged += new System.EventHandler(this.chkNWSIndexer_CheckedChanged);
            // 
            // pbRecoverNWS
            // 
            this.pbRecoverNWS.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNWS.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNWS.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNWS.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverNWS.Location = new System.Drawing.Point(400, 45);
            this.pbRecoverNWS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverNWS.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverNWS.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverNWS.Name = "pbRecoverNWS";
            this.pbRecoverNWS.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverNWS.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverNWS.TabIndex = 96;
            this.pbRecoverNWS.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverNWS, "Recover");
            // 
            // chkAOAIndexer
            // 
            this.chkAOAIndexer.AutoSize = true;
            this.chkAOAIndexer.Location = new System.Drawing.Point(9, 9);
            this.chkAOAIndexer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkAOAIndexer.Name = "chkAOAIndexer";
            this.chkAOAIndexer.Size = new System.Drawing.Size(249, 24);
            this.chkAOAIndexer.TabIndex = 93;
            this.chkAOAIndexer.Text = "Angle of Attack (AOA) Indexer";
            this.chkAOAIndexer.UseVisualStyleBackColor = true;
            this.chkAOAIndexer.CheckedChanged += new System.EventHandler(this.chkAOAIndexer_CheckedChanged);
            // 
            // pbRecoverAOAIndexer
            // 
            this.pbRecoverAOAIndexer.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAOAIndexer.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAOAIndexer.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAOAIndexer.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverAOAIndexer.Location = new System.Drawing.Point(400, 9);
            this.pbRecoverAOAIndexer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverAOAIndexer.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverAOAIndexer.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverAOAIndexer.Name = "pbRecoverAOAIndexer";
            this.pbRecoverAOAIndexer.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverAOAIndexer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverAOAIndexer.TabIndex = 94;
            this.pbRecoverAOAIndexer.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverAOAIndexer, "Recover");
            // 
            // tabTrim
            // 
            this.tabTrim.Controls.Add(this.chkPitchTrim);
            this.tabTrim.Controls.Add(this.pbRecoverPitchTrim);
            this.tabTrim.Controls.Add(this.pbRecoverRollTrim);
            this.tabTrim.Controls.Add(this.chkRollTrim);
            this.tabTrim.Location = new System.Drawing.Point(4, 54);
            this.tabTrim.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabTrim.Name = "tabTrim";
            this.tabTrim.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabTrim.Size = new System.Drawing.Size(768, 658);
            this.tabTrim.TabIndex = 6;
            this.tabTrim.Text = "Manual Trim";
            this.tabTrim.UseVisualStyleBackColor = true;
            // 
            // chkPitchTrim
            // 
            this.chkPitchTrim.AutoSize = true;
            this.chkPitchTrim.Location = new System.Drawing.Point(9, 9);
            this.chkPitchTrim.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkPitchTrim.Name = "chkPitchTrim";
            this.chkPitchTrim.Size = new System.Drawing.Size(170, 24);
            this.chkPitchTrim.TabIndex = 39;
            this.chkPitchTrim.Text = "Pitch Trim Indicator";
            this.chkPitchTrim.UseVisualStyleBackColor = true;
            this.chkPitchTrim.CheckedChanged += new System.EventHandler(this.chkPitchTrim_CheckedChanged);
            // 
            // pbRecoverPitchTrim
            // 
            this.pbRecoverPitchTrim.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverPitchTrim.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverPitchTrim.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverPitchTrim.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverPitchTrim.Location = new System.Drawing.Point(406, 9);
            this.pbRecoverPitchTrim.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverPitchTrim.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverPitchTrim.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverPitchTrim.Name = "pbRecoverPitchTrim";
            this.pbRecoverPitchTrim.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverPitchTrim.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverPitchTrim.TabIndex = 65;
            this.pbRecoverPitchTrim.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverPitchTrim, "Recover");
            // 
            // pbRecoverRollTrim
            // 
            this.pbRecoverRollTrim.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRollTrim.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRollTrim.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRollTrim.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverRollTrim.Location = new System.Drawing.Point(406, 45);
            this.pbRecoverRollTrim.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverRollTrim.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverRollTrim.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverRollTrim.Name = "pbRecoverRollTrim";
            this.pbRecoverRollTrim.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverRollTrim.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverRollTrim.TabIndex = 63;
            this.pbRecoverRollTrim.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverRollTrim, "Recover");
            // 
            // chkRollTrim
            // 
            this.chkRollTrim.AutoSize = true;
            this.chkRollTrim.Location = new System.Drawing.Point(9, 45);
            this.chkRollTrim.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkRollTrim.Name = "chkRollTrim";
            this.chkRollTrim.Size = new System.Drawing.Size(162, 24);
            this.chkRollTrim.TabIndex = 40;
            this.chkRollTrim.Text = "Roll Trim Indicator";
            this.chkRollTrim.UseVisualStyleBackColor = true;
            this.chkRollTrim.CheckedChanged += new System.EventHandler(this.chkRollTrim_CheckedChanged);
            // 
            // tabEPU
            // 
            this.tabEPU.Controls.Add(this.chkEPU);
            this.tabEPU.Controls.Add(this.pbRecoverEPU);
            this.tabEPU.Location = new System.Drawing.Point(4, 54);
            this.tabEPU.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabEPU.Name = "tabEPU";
            this.tabEPU.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabEPU.Size = new System.Drawing.Size(768, 658);
            this.tabEPU.TabIndex = 15;
            this.tabEPU.Text = "EPU";
            this.tabEPU.UseVisualStyleBackColor = true;
            // 
            // chkEPU
            // 
            this.chkEPU.AutoSize = true;
            this.chkEPU.Location = new System.Drawing.Point(9, 9);
            this.chkEPU.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkEPU.Name = "chkEPU";
            this.chkEPU.Size = new System.Drawing.Size(156, 24);
            this.chkEPU.TabIndex = 64;
            this.chkEPU.Text = "EPU Fuel Gauge";
            this.chkEPU.UseVisualStyleBackColor = true;
            this.chkEPU.CheckedChanged += new System.EventHandler(this.chkEPU_CheckedChanged);
            // 
            // pbRecoverEPU
            // 
            this.pbRecoverEPU.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverEPU.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverEPU.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverEPU.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverEPU.Location = new System.Drawing.Point(400, 9);
            this.pbRecoverEPU.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverEPU.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverEPU.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverEPU.Name = "pbRecoverEPU";
            this.pbRecoverEPU.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverEPU.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverEPU.TabIndex = 65;
            this.pbRecoverEPU.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverEPU, "Recover");
            // 
            // tabGearAndBrakes
            // 
            this.tabGearAndBrakes.Controls.Add(this.chkSpeedbrake);
            this.tabGearAndBrakes.Controls.Add(this.pbRecoverSpeedbrake);
            this.tabGearAndBrakes.Controls.Add(this.chkGearLights);
            this.tabGearAndBrakes.Controls.Add(this.pbRecoverGearLights);
            this.tabGearAndBrakes.Location = new System.Drawing.Point(4, 54);
            this.tabGearAndBrakes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabGearAndBrakes.Name = "tabGearAndBrakes";
            this.tabGearAndBrakes.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabGearAndBrakes.Size = new System.Drawing.Size(768, 658);
            this.tabGearAndBrakes.TabIndex = 16;
            this.tabGearAndBrakes.Text = "Gear & Brakes";
            this.tabGearAndBrakes.UseVisualStyleBackColor = true;
            // 
            // chkSpeedbrake
            // 
            this.chkSpeedbrake.AutoSize = true;
            this.chkSpeedbrake.Location = new System.Drawing.Point(9, 45);
            this.chkSpeedbrake.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkSpeedbrake.Name = "chkSpeedbrake";
            this.chkSpeedbrake.Size = new System.Drawing.Size(254, 24);
            this.chkSpeedbrake.TabIndex = 67;
            this.chkSpeedbrake.Text = "Speed Brake Position Indicator";
            this.chkSpeedbrake.UseVisualStyleBackColor = true;
            this.chkSpeedbrake.CheckedChanged += new System.EventHandler(this.chkSpeedbrake_CheckedChanged);
            // 
            // pbRecoverSpeedbrake
            // 
            this.pbRecoverSpeedbrake.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverSpeedbrake.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverSpeedbrake.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverSpeedbrake.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverSpeedbrake.Location = new System.Drawing.Point(396, 45);
            this.pbRecoverSpeedbrake.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverSpeedbrake.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverSpeedbrake.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverSpeedbrake.Name = "pbRecoverSpeedbrake";
            this.pbRecoverSpeedbrake.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverSpeedbrake.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverSpeedbrake.TabIndex = 68;
            this.pbRecoverSpeedbrake.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverSpeedbrake, "Recover");
            // 
            // chkGearLights
            // 
            this.chkGearLights.AutoSize = true;
            this.chkGearLights.Location = new System.Drawing.Point(9, 9);
            this.chkGearLights.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkGearLights.Name = "chkGearLights";
            this.chkGearLights.Size = new System.Drawing.Size(180, 24);
            this.chkGearLights.TabIndex = 66;
            this.chkGearLights.Text = "Wheels Down Lights";
            this.chkGearLights.UseVisualStyleBackColor = true;
            this.chkGearLights.CheckedChanged += new System.EventHandler(this.chkGearLights_CheckedChanged);
            // 
            // pbRecoverGearLights
            // 
            this.pbRecoverGearLights.BackgroundImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverGearLights.ErrorImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverGearLights.Image = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverGearLights.InitialImage = global::MFDExtractor.Properties.Resources.restore;
            this.pbRecoverGearLights.Location = new System.Drawing.Point(396, 9);
            this.pbRecoverGearLights.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbRecoverGearLights.MaximumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverGearLights.MinimumSize = new System.Drawing.Size(24, 25);
            this.pbRecoverGearLights.Name = "pbRecoverGearLights";
            this.pbRecoverGearLights.Size = new System.Drawing.Size(24, 25);
            this.pbRecoverGearLights.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRecoverGearLights.TabIndex = 69;
            this.pbRecoverGearLights.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRecoverGearLights, "Recover");
            // 
            // tabHotkeys
            // 
            this.tabHotkeys.Controls.Add(this.panel2);
            this.tabHotkeys.Location = new System.Drawing.Point(4, 29);
            this.tabHotkeys.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabHotkeys.Name = "tabHotkeys";
            this.tabHotkeys.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabHotkeys.Size = new System.Drawing.Size(784, 726);
            this.tabHotkeys.TabIndex = 6;
            this.tabHotkeys.Text = "Hotkeys";
            this.tabHotkeys.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tabHotkeysInner);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(4, 5);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(776, 716);
            this.panel2.TabIndex = 156;
            // 
            // tabHotkeysInner
            // 
            this.tabHotkeysInner.Controls.Add(this.tabGeneralKeys);
            this.tabHotkeysInner.Controls.Add(this.tabAccelerometerKeys);
            this.tabHotkeysInner.Controls.Add(this.tabASI);
            this.tabHotkeysInner.Controls.Add(this.tabAzimuthIndicatorKeys);
            this.tabHotkeysInner.Controls.Add(this.tabEHSIKeys);
            this.tabHotkeysInner.Controls.Add(this.tabISISKeys);
            this.tabHotkeysInner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabHotkeysInner.Location = new System.Drawing.Point(0, 0);
            this.tabHotkeysInner.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabHotkeysInner.Name = "tabHotkeysInner";
            this.tabHotkeysInner.SelectedIndex = 0;
            this.tabHotkeysInner.Size = new System.Drawing.Size(776, 716);
            this.tabHotkeysInner.TabIndex = 156;
            // 
            // tabGeneralKeys
            // 
            this.tabGeneralKeys.Controls.Add(this.lblNVIS);
            this.tabGeneralKeys.Controls.Add(this.cmdNV);
            this.tabGeneralKeys.Location = new System.Drawing.Point(4, 29);
            this.tabGeneralKeys.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabGeneralKeys.Name = "tabGeneralKeys";
            this.tabGeneralKeys.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabGeneralKeys.Size = new System.Drawing.Size(768, 736);
            this.tabGeneralKeys.TabIndex = 0;
            this.tabGeneralKeys.Text = "General";
            this.tabGeneralKeys.UseVisualStyleBackColor = true;
            // 
            // lblNVIS
            // 
            this.lblNVIS.AutoSize = true;
            this.lblNVIS.Location = new System.Drawing.Point(15, 15);
            this.lblNVIS.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNVIS.Name = "lblNVIS";
            this.lblNVIS.Size = new System.Drawing.Size(147, 20);
            this.lblNVIS.TabIndex = 7;
            this.lblNVIS.Text = "NVIS Mode Toggle:";
            // 
            // cmdNV
            // 
            this.cmdNV.Location = new System.Drawing.Point(176, 8);
            this.cmdNV.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdNV.Name = "cmdNV";
            this.cmdNV.Size = new System.Drawing.Size(112, 35);
            this.cmdNV.TabIndex = 8;
            this.cmdNV.Text = "View/Set...";
            this.cmdNV.UseVisualStyleBackColor = true;
            this.cmdNV.Click += new System.EventHandler(this.cmdNV_Click);
            // 
            // tabAccelerometerKeys
            // 
            this.tabAccelerometerKeys.Controls.Add(this.gbAccelerometer);
            this.tabAccelerometerKeys.Location = new System.Drawing.Point(4, 29);
            this.tabAccelerometerKeys.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabAccelerometerKeys.Name = "tabAccelerometerKeys";
            this.tabAccelerometerKeys.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabAccelerometerKeys.Size = new System.Drawing.Size(768, 736);
            this.tabAccelerometerKeys.TabIndex = 4;
            this.tabAccelerometerKeys.Text = "Accelerometer";
            this.tabAccelerometerKeys.UseVisualStyleBackColor = true;
            // 
            // gbAccelerometer
            // 
            this.gbAccelerometer.Controls.Add(this.groupBox5);
            this.gbAccelerometer.Location = new System.Drawing.Point(9, 9);
            this.gbAccelerometer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbAccelerometer.Name = "gbAccelerometer";
            this.gbAccelerometer.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbAccelerometer.Size = new System.Drawing.Size(286, 137);
            this.gbAccelerometer.TabIndex = 11;
            this.gbAccelerometer.TabStop = false;
            this.gbAccelerometer.Text = "Accelerometer Hotkeys";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.cmdAccelerometerResetButtonPressed);
            this.groupBox5.Controls.Add(this.lblAccelerometerResetButtonPressed);
            this.groupBox5.Location = new System.Drawing.Point(14, 29);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox5.Size = new System.Drawing.Size(260, 86);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Reset Button";
            // 
            // cmdAccelerometerResetButtonPressed
            // 
            this.cmdAccelerometerResetButtonPressed.Location = new System.Drawing.Point(116, 29);
            this.cmdAccelerometerResetButtonPressed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdAccelerometerResetButtonPressed.Name = "cmdAccelerometerResetButtonPressed";
            this.cmdAccelerometerResetButtonPressed.Size = new System.Drawing.Size(112, 35);
            this.cmdAccelerometerResetButtonPressed.TabIndex = 5;
            this.cmdAccelerometerResetButtonPressed.Text = "View/Set...";
            this.cmdAccelerometerResetButtonPressed.UseVisualStyleBackColor = true;
            this.cmdAccelerometerResetButtonPressed.Click += new System.EventHandler(this.cmdAccelerometerResetButtonPressed_Click);
            // 
            // lblAccelerometerResetButtonPressed
            // 
            this.lblAccelerometerResetButtonPressed.AutoSize = true;
            this.lblAccelerometerResetButtonPressed.Location = new System.Drawing.Point(16, 37);
            this.lblAccelerometerResetButtonPressed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAccelerometerResetButtonPressed.Name = "lblAccelerometerResetButtonPressed";
            this.lblAccelerometerResetButtonPressed.Size = new System.Drawing.Size(71, 20);
            this.lblAccelerometerResetButtonPressed.TabIndex = 4;
            this.lblAccelerometerResetButtonPressed.Text = "Pressed:";
            // 
            // tabASI
            // 
            this.tabASI.Controls.Add(this.gbAirspeedIndicator);
            this.tabASI.Location = new System.Drawing.Point(4, 29);
            this.tabASI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabASI.Name = "tabASI";
            this.tabASI.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabASI.Size = new System.Drawing.Size(768, 736);
            this.tabASI.TabIndex = 2;
            this.tabASI.Text = "Airspeed Indicator";
            this.tabASI.UseVisualStyleBackColor = true;
            // 
            // gbAirspeedIndicator
            // 
            this.gbAirspeedIndicator.Controls.Add(this.gbASIIndexKnob);
            this.gbAirspeedIndicator.Location = new System.Drawing.Point(9, 9);
            this.gbAirspeedIndicator.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbAirspeedIndicator.Name = "gbAirspeedIndicator";
            this.gbAirspeedIndicator.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbAirspeedIndicator.Size = new System.Drawing.Size(288, 168);
            this.gbAirspeedIndicator.TabIndex = 14;
            this.gbAirspeedIndicator.TabStop = false;
            this.gbAirspeedIndicator.Text = "Airspeed Indicator Hotkeys";
            // 
            // gbASIIndexKnob
            // 
            this.gbASIIndexKnob.Controls.Add(this.lblAirspeedIndexIncreaseHotkey);
            this.gbASIIndexKnob.Controls.Add(this.cmdAirspeedIndexIncreaseHotkey);
            this.gbASIIndexKnob.Controls.Add(this.lblAirspeedIndexDecreaseHotkey);
            this.gbASIIndexKnob.Controls.Add(this.cmdAirspeedIndexDecreaseHotkey);
            this.gbASIIndexKnob.Location = new System.Drawing.Point(9, 29);
            this.gbASIIndexKnob.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbASIIndexKnob.Name = "gbASIIndexKnob";
            this.gbASIIndexKnob.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbASIIndexKnob.Size = new System.Drawing.Size(267, 120);
            this.gbASIIndexKnob.TabIndex = 7;
            this.gbASIIndexKnob.TabStop = false;
            this.gbASIIndexKnob.Text = "Index Knob";
            // 
            // lblAirspeedIndexIncreaseHotkey
            // 
            this.lblAirspeedIndexIncreaseHotkey.AutoSize = true;
            this.lblAirspeedIndexIncreaseHotkey.Location = new System.Drawing.Point(9, 37);
            this.lblAirspeedIndexIncreaseHotkey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAirspeedIndexIncreaseHotkey.Name = "lblAirspeedIndexIncreaseHotkey";
            this.lblAirspeedIndexIncreaseHotkey.Size = new System.Drawing.Size(85, 20);
            this.lblAirspeedIndexIncreaseHotkey.TabIndex = 0;
            this.lblAirspeedIndexIncreaseHotkey.Text = "Increment:";
            // 
            // cmdAirspeedIndexIncreaseHotkey
            // 
            this.cmdAirspeedIndexIncreaseHotkey.Location = new System.Drawing.Point(123, 29);
            this.cmdAirspeedIndexIncreaseHotkey.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdAirspeedIndexIncreaseHotkey.Name = "cmdAirspeedIndexIncreaseHotkey";
            this.cmdAirspeedIndexIncreaseHotkey.Size = new System.Drawing.Size(112, 35);
            this.cmdAirspeedIndexIncreaseHotkey.TabIndex = 1;
            this.cmdAirspeedIndexIncreaseHotkey.Text = "View/Set...";
            this.cmdAirspeedIndexIncreaseHotkey.UseVisualStyleBackColor = true;
            this.cmdAirspeedIndexIncreaseHotkey.Click += new System.EventHandler(this.cmdAirspeedIndexIncreaseHotkey_Click);
            // 
            // lblAirspeedIndexDecreaseHotkey
            // 
            this.lblAirspeedIndexDecreaseHotkey.AutoSize = true;
            this.lblAirspeedIndexDecreaseHotkey.Location = new System.Drawing.Point(9, 82);
            this.lblAirspeedIndexDecreaseHotkey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAirspeedIndexDecreaseHotkey.Name = "lblAirspeedIndexDecreaseHotkey";
            this.lblAirspeedIndexDecreaseHotkey.Size = new System.Drawing.Size(92, 20);
            this.lblAirspeedIndexDecreaseHotkey.TabIndex = 2;
            this.lblAirspeedIndexDecreaseHotkey.Text = "Decrement:";
            // 
            // cmdAirspeedIndexDecreaseHotkey
            // 
            this.cmdAirspeedIndexDecreaseHotkey.Location = new System.Drawing.Point(123, 74);
            this.cmdAirspeedIndexDecreaseHotkey.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdAirspeedIndexDecreaseHotkey.Name = "cmdAirspeedIndexDecreaseHotkey";
            this.cmdAirspeedIndexDecreaseHotkey.Size = new System.Drawing.Size(112, 35);
            this.cmdAirspeedIndexDecreaseHotkey.TabIndex = 3;
            this.cmdAirspeedIndexDecreaseHotkey.Text = "View/Set...";
            this.cmdAirspeedIndexDecreaseHotkey.UseVisualStyleBackColor = true;
            this.cmdAirspeedIndexDecreaseHotkey.Click += new System.EventHandler(this.cmdAirspeedIndexDecreaseHotkey_Click);
            // 
            // tabAzimuthIndicatorKeys
            // 
            this.tabAzimuthIndicatorKeys.Controls.Add(this.gbAzimuthIndicator);
            this.tabAzimuthIndicatorKeys.Location = new System.Drawing.Point(4, 29);
            this.tabAzimuthIndicatorKeys.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabAzimuthIndicatorKeys.Name = "tabAzimuthIndicatorKeys";
            this.tabAzimuthIndicatorKeys.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabAzimuthIndicatorKeys.Size = new System.Drawing.Size(768, 736);
            this.tabAzimuthIndicatorKeys.TabIndex = 3;
            this.tabAzimuthIndicatorKeys.Text = "Azimuth Indicator";
            this.tabAzimuthIndicatorKeys.UseVisualStyleBackColor = true;
            // 
            // gbAzimuthIndicator
            // 
            this.gbAzimuthIndicator.Controls.Add(this.gbAzimuthIndicatorBrightnessControl);
            this.gbAzimuthIndicator.Location = new System.Drawing.Point(9, 9);
            this.gbAzimuthIndicator.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbAzimuthIndicator.Name = "gbAzimuthIndicator";
            this.gbAzimuthIndicator.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbAzimuthIndicator.Size = new System.Drawing.Size(282, 165);
            this.gbAzimuthIndicator.TabIndex = 15;
            this.gbAzimuthIndicator.TabStop = false;
            this.gbAzimuthIndicator.Text = "Azimuth Indicator Hotkeys";
            // 
            // gbAzimuthIndicatorBrightnessControl
            // 
            this.gbAzimuthIndicatorBrightnessControl.Controls.Add(this.lblAzimuthIndicatorBrightnessDecrease);
            this.gbAzimuthIndicatorBrightnessControl.Controls.Add(this.lblAzimuthIndicatorBrightnessIncrease);
            this.gbAzimuthIndicatorBrightnessControl.Controls.Add(this.cmdAzimuthIndicatorBrightnessIncrease);
            this.gbAzimuthIndicatorBrightnessControl.Controls.Add(this.cmdAzimuthIndicatorBrightnessDecrease);
            this.gbAzimuthIndicatorBrightnessControl.Location = new System.Drawing.Point(9, 29);
            this.gbAzimuthIndicatorBrightnessControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbAzimuthIndicatorBrightnessControl.Name = "gbAzimuthIndicatorBrightnessControl";
            this.gbAzimuthIndicatorBrightnessControl.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbAzimuthIndicatorBrightnessControl.Size = new System.Drawing.Size(254, 118);
            this.gbAzimuthIndicatorBrightnessControl.TabIndex = 0;
            this.gbAzimuthIndicatorBrightnessControl.TabStop = false;
            this.gbAzimuthIndicatorBrightnessControl.Text = "Brightness Control";
            // 
            // lblAzimuthIndicatorBrightnessDecrease
            // 
            this.lblAzimuthIndicatorBrightnessDecrease.AutoSize = true;
            this.lblAzimuthIndicatorBrightnessDecrease.Location = new System.Drawing.Point(9, 82);
            this.lblAzimuthIndicatorBrightnessDecrease.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAzimuthIndicatorBrightnessDecrease.Name = "lblAzimuthIndicatorBrightnessDecrease";
            this.lblAzimuthIndicatorBrightnessDecrease.Size = new System.Drawing.Size(82, 20);
            this.lblAzimuthIndicatorBrightnessDecrease.TabIndex = 2;
            this.lblAzimuthIndicatorBrightnessDecrease.Text = "Decrease:";
            // 
            // lblAzimuthIndicatorBrightnessIncrease
            // 
            this.lblAzimuthIndicatorBrightnessIncrease.AutoSize = true;
            this.lblAzimuthIndicatorBrightnessIncrease.Location = new System.Drawing.Point(9, 37);
            this.lblAzimuthIndicatorBrightnessIncrease.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAzimuthIndicatorBrightnessIncrease.Name = "lblAzimuthIndicatorBrightnessIncrease";
            this.lblAzimuthIndicatorBrightnessIncrease.Size = new System.Drawing.Size(75, 20);
            this.lblAzimuthIndicatorBrightnessIncrease.TabIndex = 0;
            this.lblAzimuthIndicatorBrightnessIncrease.Text = "Increase:";
            // 
            // cmdAzimuthIndicatorBrightnessIncrease
            // 
            this.cmdAzimuthIndicatorBrightnessIncrease.Location = new System.Drawing.Point(123, 29);
            this.cmdAzimuthIndicatorBrightnessIncrease.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdAzimuthIndicatorBrightnessIncrease.Name = "cmdAzimuthIndicatorBrightnessIncrease";
            this.cmdAzimuthIndicatorBrightnessIncrease.Size = new System.Drawing.Size(112, 35);
            this.cmdAzimuthIndicatorBrightnessIncrease.TabIndex = 1;
            this.cmdAzimuthIndicatorBrightnessIncrease.Text = "View/Set...";
            this.cmdAzimuthIndicatorBrightnessIncrease.UseVisualStyleBackColor = true;
            this.cmdAzimuthIndicatorBrightnessIncrease.Click += new System.EventHandler(this.cmdAzimuthIndicatorBrightnessIncrease_Click);
            // 
            // cmdAzimuthIndicatorBrightnessDecrease
            // 
            this.cmdAzimuthIndicatorBrightnessDecrease.Location = new System.Drawing.Point(123, 74);
            this.cmdAzimuthIndicatorBrightnessDecrease.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdAzimuthIndicatorBrightnessDecrease.Name = "cmdAzimuthIndicatorBrightnessDecrease";
            this.cmdAzimuthIndicatorBrightnessDecrease.Size = new System.Drawing.Size(112, 35);
            this.cmdAzimuthIndicatorBrightnessDecrease.TabIndex = 3;
            this.cmdAzimuthIndicatorBrightnessDecrease.Text = "View/Set...";
            this.cmdAzimuthIndicatorBrightnessDecrease.UseVisualStyleBackColor = true;
            this.cmdAzimuthIndicatorBrightnessDecrease.Click += new System.EventHandler(this.cmdAzimuthIndicatorBrightnessDecrease_Click);
            // 
            // tabEHSIKeys
            // 
            this.tabEHSIKeys.Controls.Add(this.gbEHSI);
            this.tabEHSIKeys.Location = new System.Drawing.Point(4, 29);
            this.tabEHSIKeys.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabEHSIKeys.Name = "tabEHSIKeys";
            this.tabEHSIKeys.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabEHSIKeys.Size = new System.Drawing.Size(768, 736);
            this.tabEHSIKeys.TabIndex = 1;
            this.tabEHSIKeys.Text = "EHSI";
            this.tabEHSIKeys.UseVisualStyleBackColor = true;
            // 
            // gbEHSI
            // 
            this.gbEHSI.Controls.Add(this.lblEHSIMenuButton);
            this.gbEHSI.Controls.Add(this.groupBox4);
            this.gbEHSI.Controls.Add(this.groupBox3);
            this.gbEHSI.Controls.Add(this.cmdEHSIMenuButtonHotkey);
            this.gbEHSI.Location = new System.Drawing.Point(9, 9);
            this.gbEHSI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbEHSI.Name = "gbEHSI";
            this.gbEHSI.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbEHSI.Size = new System.Drawing.Size(608, 262);
            this.gbEHSI.TabIndex = 8;
            this.gbEHSI.TabStop = false;
            this.gbEHSI.Text = "EHSI Hotkeys";
            // 
            // lblEHSIMenuButton
            // 
            this.lblEHSIMenuButton.AutoSize = true;
            this.lblEHSIMenuButton.Location = new System.Drawing.Point(18, 25);
            this.lblEHSIMenuButton.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEHSIMenuButton.Name = "lblEHSIMenuButton";
            this.lblEHSIMenuButton.Size = new System.Drawing.Size(78, 20);
            this.lblEHSIMenuButton.TabIndex = 0;
            this.lblEHSIMenuButton.Text = "M Button:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lblEHSICourseIncreaseHotkey);
            this.groupBox4.Controls.Add(this.cmdEHSICourseIncreaseKey);
            this.groupBox4.Controls.Add(this.cmdEHSICourseKnobDepressedKey);
            this.groupBox4.Controls.Add(this.lblEHSICourseDecreaseHotkey);
            this.groupBox4.Controls.Add(this.cmdEHSICourseDecreaseKey);
            this.groupBox4.Controls.Add(this.label54);
            this.groupBox4.Location = new System.Drawing.Point(302, 71);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Size = new System.Drawing.Size(286, 174);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "CRS/BRT Knob";
            // 
            // lblEHSICourseIncreaseHotkey
            // 
            this.lblEHSICourseIncreaseHotkey.AutoSize = true;
            this.lblEHSICourseIncreaseHotkey.Location = new System.Drawing.Point(18, 37);
            this.lblEHSICourseIncreaseHotkey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEHSICourseIncreaseHotkey.Name = "lblEHSICourseIncreaseHotkey";
            this.lblEHSICourseIncreaseHotkey.Size = new System.Drawing.Size(85, 20);
            this.lblEHSICourseIncreaseHotkey.TabIndex = 0;
            this.lblEHSICourseIncreaseHotkey.Text = "Increment:";
            // 
            // cmdEHSICourseIncreaseKey
            // 
            this.cmdEHSICourseIncreaseKey.Location = new System.Drawing.Point(147, 29);
            this.cmdEHSICourseIncreaseKey.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdEHSICourseIncreaseKey.Name = "cmdEHSICourseIncreaseKey";
            this.cmdEHSICourseIncreaseKey.Size = new System.Drawing.Size(112, 35);
            this.cmdEHSICourseIncreaseKey.TabIndex = 1;
            this.cmdEHSICourseIncreaseKey.Text = "View/Set...";
            this.cmdEHSICourseIncreaseKey.UseVisualStyleBackColor = true;
            this.cmdEHSICourseIncreaseKey.Click += new System.EventHandler(this.cmdEHSICourseIncreaseKey_Click);
            // 
            // cmdEHSICourseKnobDepressedKey
            // 
            this.cmdEHSICourseKnobDepressedKey.Location = new System.Drawing.Point(147, 118);
            this.cmdEHSICourseKnobDepressedKey.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdEHSICourseKnobDepressedKey.Name = "cmdEHSICourseKnobDepressedKey";
            this.cmdEHSICourseKnobDepressedKey.Size = new System.Drawing.Size(112, 35);
            this.cmdEHSICourseKnobDepressedKey.TabIndex = 5;
            this.cmdEHSICourseKnobDepressedKey.Text = "View/Set...";
            this.cmdEHSICourseKnobDepressedKey.UseVisualStyleBackColor = true;
            this.cmdEHSICourseKnobDepressedKey.Click += new System.EventHandler(this.cmdEHSICourseKnobDepressedKey_Click);
            // 
            // lblEHSICourseDecreaseHotkey
            // 
            this.lblEHSICourseDecreaseHotkey.AutoSize = true;
            this.lblEHSICourseDecreaseHotkey.Location = new System.Drawing.Point(18, 82);
            this.lblEHSICourseDecreaseHotkey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEHSICourseDecreaseHotkey.Name = "lblEHSICourseDecreaseHotkey";
            this.lblEHSICourseDecreaseHotkey.Size = new System.Drawing.Size(92, 20);
            this.lblEHSICourseDecreaseHotkey.TabIndex = 2;
            this.lblEHSICourseDecreaseHotkey.Text = "Decrement:";
            // 
            // cmdEHSICourseDecreaseKey
            // 
            this.cmdEHSICourseDecreaseKey.Location = new System.Drawing.Point(147, 74);
            this.cmdEHSICourseDecreaseKey.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdEHSICourseDecreaseKey.Name = "cmdEHSICourseDecreaseKey";
            this.cmdEHSICourseDecreaseKey.Size = new System.Drawing.Size(112, 35);
            this.cmdEHSICourseDecreaseKey.TabIndex = 3;
            this.cmdEHSICourseDecreaseKey.Text = "View/Set...";
            this.cmdEHSICourseDecreaseKey.UseVisualStyleBackColor = true;
            this.cmdEHSICourseDecreaseKey.Click += new System.EventHandler(this.cmdEHSICourseDecreaseKey_Click);
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(18, 126);
            this.label54.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(71, 20);
            this.label54.TabIndex = 4;
            this.label54.Text = "Pressed:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblEHSIHeadingIncreaseButton);
            this.groupBox3.Controls.Add(this.cmdEHSIHeadingIncreaseKey);
            this.groupBox3.Controls.Add(this.label53);
            this.groupBox3.Controls.Add(this.cmdEHSIHeadingDecreaseKey);
            this.groupBox3.Location = new System.Drawing.Point(9, 71);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox3.Size = new System.Drawing.Size(278, 174);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "HDG/ATT Knob";
            // 
            // lblEHSIHeadingIncreaseButton
            // 
            this.lblEHSIHeadingIncreaseButton.AutoSize = true;
            this.lblEHSIHeadingIncreaseButton.Location = new System.Drawing.Point(9, 37);
            this.lblEHSIHeadingIncreaseButton.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEHSIHeadingIncreaseButton.Name = "lblEHSIHeadingIncreaseButton";
            this.lblEHSIHeadingIncreaseButton.Size = new System.Drawing.Size(85, 20);
            this.lblEHSIHeadingIncreaseButton.TabIndex = 0;
            this.lblEHSIHeadingIncreaseButton.Text = "Increment:";
            // 
            // cmdEHSIHeadingIncreaseKey
            // 
            this.cmdEHSIHeadingIncreaseKey.Location = new System.Drawing.Point(146, 29);
            this.cmdEHSIHeadingIncreaseKey.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdEHSIHeadingIncreaseKey.Name = "cmdEHSIHeadingIncreaseKey";
            this.cmdEHSIHeadingIncreaseKey.Size = new System.Drawing.Size(112, 35);
            this.cmdEHSIHeadingIncreaseKey.TabIndex = 1;
            this.cmdEHSIHeadingIncreaseKey.Text = "View/Set...";
            this.cmdEHSIHeadingIncreaseKey.UseVisualStyleBackColor = true;
            this.cmdEHSIHeadingIncreaseKey.Click += new System.EventHandler(this.cmdEHSIHeadingIncreaseKey_Click);
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Location = new System.Drawing.Point(9, 82);
            this.label53.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(92, 20);
            this.label53.TabIndex = 2;
            this.label53.Text = "Decrement:";
            // 
            // cmdEHSIHeadingDecreaseKey
            // 
            this.cmdEHSIHeadingDecreaseKey.Location = new System.Drawing.Point(146, 74);
            this.cmdEHSIHeadingDecreaseKey.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdEHSIHeadingDecreaseKey.Name = "cmdEHSIHeadingDecreaseKey";
            this.cmdEHSIHeadingDecreaseKey.Size = new System.Drawing.Size(112, 35);
            this.cmdEHSIHeadingDecreaseKey.TabIndex = 3;
            this.cmdEHSIHeadingDecreaseKey.Text = "View/Set...";
            this.cmdEHSIHeadingDecreaseKey.UseVisualStyleBackColor = true;
            this.cmdEHSIHeadingDecreaseKey.Click += new System.EventHandler(this.cmdEHSIHeadingDecreaseKey_Click);
            // 
            // cmdEHSIMenuButtonHotkey
            // 
            this.cmdEHSIMenuButtonHotkey.Location = new System.Drawing.Point(154, 17);
            this.cmdEHSIMenuButtonHotkey.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdEHSIMenuButtonHotkey.Name = "cmdEHSIMenuButtonHotkey";
            this.cmdEHSIMenuButtonHotkey.Size = new System.Drawing.Size(112, 35);
            this.cmdEHSIMenuButtonHotkey.TabIndex = 1;
            this.cmdEHSIMenuButtonHotkey.Text = "View/Set...";
            this.cmdEHSIMenuButtonHotkey.UseVisualStyleBackColor = true;
            this.cmdEHSIMenuButtonHotkey.Click += new System.EventHandler(this.cmdEHSIMenuButtonHotkey_Click);
            // 
            // tabISISKeys
            // 
            this.tabISISKeys.Controls.Add(this.gbISIS);
            this.tabISISKeys.Location = new System.Drawing.Point(4, 29);
            this.tabISISKeys.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabISISKeys.Name = "tabISISKeys";
            this.tabISISKeys.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabISISKeys.Size = new System.Drawing.Size(768, 683);
            this.tabISISKeys.TabIndex = 5;
            this.tabISISKeys.Text = "ISIS";
            this.tabISISKeys.UseVisualStyleBackColor = true;
            // 
            // gbISIS
            // 
            this.gbISIS.Controls.Add(this.lblISISBrightBrightnessButtonPressed);
            this.gbISIS.Controls.Add(this.cmdISISBrightButtonPressed);
            this.gbISIS.Controls.Add(this.cmdISISStandardBrightnessButtonPressed);
            this.gbISIS.Controls.Add(this.lblISISStandardBrightnessButtonPressed);
            this.gbISIS.Location = new System.Drawing.Point(9, 9);
            this.gbISIS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbISIS.Name = "gbISIS";
            this.gbISIS.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbISIS.Size = new System.Drawing.Size(292, 114);
            this.gbISIS.TabIndex = 12;
            this.gbISIS.TabStop = false;
            this.gbISIS.Text = "ISIS Hotkeys";
            // 
            // lblISISBrightBrightnessButtonPressed
            // 
            this.lblISISBrightBrightnessButtonPressed.AutoSize = true;
            this.lblISISBrightBrightnessButtonPressed.Location = new System.Drawing.Point(18, 29);
            this.lblISISBrightBrightnessButtonPressed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblISISBrightBrightnessButtonPressed.Name = "lblISISBrightBrightnessButtonPressed";
            this.lblISISBrightBrightnessButtonPressed.Size = new System.Drawing.Size(97, 20);
            this.lblISISBrightBrightnessButtonPressed.TabIndex = 0;
            this.lblISISBrightBrightnessButtonPressed.Text = "BRT Button:";
            // 
            // cmdISISBrightButtonPressed
            // 
            this.cmdISISBrightButtonPressed.Location = new System.Drawing.Point(132, 22);
            this.cmdISISBrightButtonPressed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdISISBrightButtonPressed.Name = "cmdISISBrightButtonPressed";
            this.cmdISISBrightButtonPressed.Size = new System.Drawing.Size(112, 35);
            this.cmdISISBrightButtonPressed.TabIndex = 1;
            this.cmdISISBrightButtonPressed.Text = "View/Set...";
            this.cmdISISBrightButtonPressed.UseVisualStyleBackColor = true;
            this.cmdISISBrightButtonPressed.Click += new System.EventHandler(this.cmdISISBrightButtonPressed_Click);
            // 
            // cmdISISStandardBrightnessButtonPressed
            // 
            this.cmdISISStandardBrightnessButtonPressed.Location = new System.Drawing.Point(132, 65);
            this.cmdISISStandardBrightnessButtonPressed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdISISStandardBrightnessButtonPressed.Name = "cmdISISStandardBrightnessButtonPressed";
            this.cmdISISStandardBrightnessButtonPressed.Size = new System.Drawing.Size(112, 35);
            this.cmdISISStandardBrightnessButtonPressed.TabIndex = 3;
            this.cmdISISStandardBrightnessButtonPressed.Text = "View/Set...";
            this.cmdISISStandardBrightnessButtonPressed.UseVisualStyleBackColor = true;
            this.cmdISISStandardBrightnessButtonPressed.Click += new System.EventHandler(this.cmdISISStandardBrightnessButtonPressed_Click);
            // 
            // lblISISStandardBrightnessButtonPressed
            // 
            this.lblISISStandardBrightnessButtonPressed.AutoSize = true;
            this.lblISISStandardBrightnessButtonPressed.Location = new System.Drawing.Point(18, 72);
            this.lblISISStandardBrightnessButtonPressed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblISISStandardBrightnessButtonPressed.Name = "lblISISStandardBrightnessButtonPressed";
            this.lblISISStandardBrightnessButtonPressed.Size = new System.Drawing.Size(97, 20);
            this.lblISISStandardBrightnessButtonPressed.TabIndex = 2;
            this.lblISISStandardBrightnessButtonPressed.Text = "STD Button:";
            // 
            // tabNetworking
            // 
            this.tabNetworking.Controls.Add(this.grpNetworkMode);
            this.tabNetworking.Location = new System.Drawing.Point(4, 29);
            this.tabNetworking.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabNetworking.Name = "tabNetworking";
            this.tabNetworking.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabNetworking.Size = new System.Drawing.Size(784, 726);
            this.tabNetworking.TabIndex = 16;
            this.tabNetworking.Text = "Networking";
            this.tabNetworking.UseVisualStyleBackColor = true;
            // 
            // grpNetworkMode
            // 
            this.grpNetworkMode.Controls.Add(this.grpServerOptions);
            this.grpNetworkMode.Controls.Add(this.gbNetworkingMode);
            this.grpNetworkMode.Controls.Add(this.grpClientOptions);
            this.grpNetworkMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpNetworkMode.Location = new System.Drawing.Point(4, 5);
            this.grpNetworkMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpNetworkMode.Name = "grpNetworkMode";
            this.grpNetworkMode.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpNetworkMode.Size = new System.Drawing.Size(776, 716);
            this.grpNetworkMode.TabIndex = 58;
            this.grpNetworkMode.TabStop = false;
            this.grpNetworkMode.Text = "Networking";
            // 
            // grpServerOptions
            // 
            this.grpServerOptions.Controls.Add(this.lblCompressionType);
            this.grpServerOptions.Controls.Add(this.cboCompressionType);
            this.grpServerOptions.Controls.Add(this.lblImageFormat);
            this.grpServerOptions.Controls.Add(this.cboImageFormat);
            this.grpServerOptions.Controls.Add(this.lblServerServerPortNum);
            this.grpServerOptions.Controls.Add(this.txtNetworkServerUsePortNum);
            this.grpServerOptions.Location = new System.Drawing.Point(174, 25);
            this.grpServerOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpServerOptions.Name = "grpServerOptions";
            this.grpServerOptions.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpServerOptions.Size = new System.Drawing.Size(350, 146);
            this.grpServerOptions.TabIndex = 45;
            this.grpServerOptions.TabStop = false;
            this.grpServerOptions.Text = "Server Options";
            // 
            // lblCompressionType
            // 
            this.lblCompressionType.AutoSize = true;
            this.lblCompressionType.Location = new System.Drawing.Point(6, 100);
            this.lblCompressionType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCompressionType.Name = "lblCompressionType";
            this.lblCompressionType.Size = new System.Drawing.Size(144, 20);
            this.lblCompressionType.TabIndex = 92;
            this.lblCompressionType.Text = "Compression Type:";
            // 
            // cboCompressionType
            // 
            this.cboCompressionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCompressionType.FormattingEnabled = true;
            this.cboCompressionType.Location = new System.Drawing.Point(160, 95);
            this.cboCompressionType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cboCompressionType.Name = "cboCompressionType";
            this.cboCompressionType.Size = new System.Drawing.Size(128, 28);
            this.cboCompressionType.TabIndex = 48;
            // 
            // lblImageFormat
            // 
            this.lblImageFormat.AutoSize = true;
            this.lblImageFormat.Location = new System.Drawing.Point(40, 63);
            this.lblImageFormat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblImageFormat.Name = "lblImageFormat";
            this.lblImageFormat.Size = new System.Drawing.Size(113, 20);
            this.lblImageFormat.TabIndex = 90;
            this.lblImageFormat.Text = "Image Format:";
            // 
            // cboImageFormat
            // 
            this.cboImageFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImageFormat.FormattingEnabled = true;
            this.cboImageFormat.Location = new System.Drawing.Point(160, 57);
            this.cboImageFormat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cboImageFormat.Name = "cboImageFormat";
            this.cboImageFormat.Size = new System.Drawing.Size(128, 28);
            this.cboImageFormat.TabIndex = 47;
            this.cboImageFormat.SelectedIndexChanged += new System.EventHandler(this.cboImageFormat_SelectedIndexChanged);
            // 
            // lblServerServerPortNum
            // 
            this.lblServerServerPortNum.AutoSize = true;
            this.lblServerServerPortNum.Location = new System.Drawing.Point(51, 25);
            this.lblServerServerPortNum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblServerServerPortNum.Name = "lblServerServerPortNum";
            this.lblServerServerPortNum.Size = new System.Drawing.Size(102, 20);
            this.lblServerServerPortNum.TabIndex = 5;
            this.lblServerServerPortNum.Text = "Port Number:";
            this.lblServerServerPortNum.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtNetworkServerUsePortNum
            // 
            this.txtNetworkServerUsePortNum.Location = new System.Drawing.Point(160, 20);
            this.txtNetworkServerUsePortNum.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtNetworkServerUsePortNum.MaxLength = 5;
            this.txtNetworkServerUsePortNum.Name = "txtNetworkServerUsePortNum";
            this.txtNetworkServerUsePortNum.Size = new System.Drawing.Size(128, 26);
            this.txtNetworkServerUsePortNum.TabIndex = 46;
            // 
            // gbNetworkingMode
            // 
            this.gbNetworkingMode.Controls.Add(this.rdoServer);
            this.gbNetworkingMode.Controls.Add(this.rdoStandalone);
            this.gbNetworkingMode.Controls.Add(this.rdoClient);
            this.gbNetworkingMode.Location = new System.Drawing.Point(24, 25);
            this.gbNetworkingMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbNetworkingMode.Name = "gbNetworkingMode";
            this.gbNetworkingMode.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbNetworkingMode.Size = new System.Drawing.Size(141, 143);
            this.gbNetworkingMode.TabIndex = 58;
            this.gbNetworkingMode.TabStop = false;
            this.gbNetworkingMode.Text = "Mode";
            // 
            // rdoServer
            // 
            this.rdoServer.AutoSize = true;
            this.rdoServer.Location = new System.Drawing.Point(9, 95);
            this.rdoServer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoServer.Name = "rdoServer";
            this.rdoServer.Size = new System.Drawing.Size(80, 24);
            this.rdoServer.TabIndex = 41;
            this.rdoServer.TabStop = true;
            this.rdoServer.Text = "Server";
            this.rdoServer.UseVisualStyleBackColor = true;
            this.rdoServer.CheckedChanged += new System.EventHandler(this.rdoServer_CheckedChanged);
            // 
            // rdoStandalone
            // 
            this.rdoStandalone.AutoSize = true;
            this.rdoStandalone.Location = new System.Drawing.Point(9, 25);
            this.rdoStandalone.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoStandalone.Name = "rdoStandalone";
            this.rdoStandalone.Size = new System.Drawing.Size(116, 24);
            this.rdoStandalone.TabIndex = 39;
            this.rdoStandalone.TabStop = true;
            this.rdoStandalone.Text = "Standalone";
            this.rdoStandalone.UseVisualStyleBackColor = true;
            this.rdoStandalone.CheckedChanged += new System.EventHandler(this.rdoStandalone_CheckedChanged);
            // 
            // rdoClient
            // 
            this.rdoClient.AutoSize = true;
            this.rdoClient.Location = new System.Drawing.Point(9, 60);
            this.rdoClient.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoClient.Name = "rdoClient";
            this.rdoClient.Size = new System.Drawing.Size(74, 24);
            this.rdoClient.TabIndex = 40;
            this.rdoClient.TabStop = true;
            this.rdoClient.Text = "Client";
            this.rdoClient.UseVisualStyleBackColor = true;
            this.rdoClient.CheckedChanged += new System.EventHandler(this.rdoClient_CheckedChanged);
            // 
            // grpClientOptions
            // 
            this.grpClientOptions.Controls.Add(this.lblClientServerPortNum);
            this.grpClientOptions.Controls.Add(this.txtNetworkClientUseServerPortNum);
            this.grpClientOptions.Controls.Add(this.lblServerIpAddress);
            this.grpClientOptions.Controls.Add(this.ipaNetworkClientUseServerIpAddress);
            this.grpClientOptions.Location = new System.Drawing.Point(174, 25);
            this.grpClientOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpClientOptions.Name = "grpClientOptions";
            this.grpClientOptions.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpClientOptions.Size = new System.Drawing.Size(350, 105);
            this.grpClientOptions.TabIndex = 42;
            this.grpClientOptions.TabStop = false;
            this.grpClientOptions.Text = "Client Options";
            // 
            // lblClientServerPortNum
            // 
            this.lblClientServerPortNum.AutoSize = true;
            this.lblClientServerPortNum.Location = new System.Drawing.Point(48, 66);
            this.lblClientServerPortNum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblClientServerPortNum.Name = "lblClientServerPortNum";
            this.lblClientServerPortNum.Size = new System.Drawing.Size(102, 20);
            this.lblClientServerPortNum.TabIndex = 3;
            this.lblClientServerPortNum.Text = "Port Number:";
            this.lblClientServerPortNum.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtNetworkClientUseServerPortNum
            // 
            this.txtNetworkClientUseServerPortNum.Location = new System.Drawing.Point(162, 62);
            this.txtNetworkClientUseServerPortNum.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtNetworkClientUseServerPortNum.MaxLength = 5;
            this.txtNetworkClientUseServerPortNum.Name = "txtNetworkClientUseServerPortNum";
            this.txtNetworkClientUseServerPortNum.Size = new System.Drawing.Size(127, 26);
            this.txtNetworkClientUseServerPortNum.TabIndex = 44;
            // 
            // lblServerIpAddress
            // 
            this.lblServerIpAddress.AutoSize = true;
            this.lblServerIpAddress.Location = new System.Drawing.Point(9, 25);
            this.lblServerIpAddress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblServerIpAddress.Name = "lblServerIpAddress";
            this.lblServerIpAddress.Size = new System.Drawing.Size(141, 20);
            this.lblServerIpAddress.TabIndex = 1;
            this.lblServerIpAddress.Text = "Server IP Address:";
            this.lblServerIpAddress.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ipaNetworkClientUseServerIpAddress
            // 
            this.ipaNetworkClientUseServerIpAddress.AllowInternalTab = true;
            this.ipaNetworkClientUseServerIpAddress.AutoHeight = true;
            this.ipaNetworkClientUseServerIpAddress.BackColor = System.Drawing.SystemColors.Window;
            this.ipaNetworkClientUseServerIpAddress.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ipaNetworkClientUseServerIpAddress.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ipaNetworkClientUseServerIpAddress.Location = new System.Drawing.Point(160, 20);
            this.ipaNetworkClientUseServerIpAddress.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ipaNetworkClientUseServerIpAddress.MinimumSize = new System.Drawing.Size(126, 26);
            this.ipaNetworkClientUseServerIpAddress.Name = "ipaNetworkClientUseServerIpAddress";
            this.ipaNetworkClientUseServerIpAddress.ReadOnly = false;
            this.ipaNetworkClientUseServerIpAddress.Size = new System.Drawing.Size(189, 26);
            this.ipaNetworkClientUseServerIpAddress.TabIndex = 43;
            this.ipaNetworkClientUseServerIpAddress.Text = "...";
            // 
            // tabPerformance
            // 
            this.tabPerformance.Controls.Add(this.grpPerformanceOptions);
            this.tabPerformance.Location = new System.Drawing.Point(4, 29);
            this.tabPerformance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPerformance.Name = "tabPerformance";
            this.tabPerformance.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPerformance.Size = new System.Drawing.Size(784, 726);
            this.tabPerformance.TabIndex = 18;
            this.tabPerformance.Text = "Performance";
            this.tabPerformance.UseVisualStyleBackColor = true;
            // 
            // grpPerformanceOptions
            // 
            this.grpPerformanceOptions.Controls.Add(this.cmdBMSOptions);
            this.grpPerformanceOptions.Controls.Add(this.chkOnlyUpdateImagesWhenDataChanges);
            this.grpPerformanceOptions.Controls.Add(this.lblMilliseconds);
            this.grpPerformanceOptions.Controls.Add(this.cboThreadPriority);
            this.grpPerformanceOptions.Controls.Add(this.label19);
            this.grpPerformanceOptions.Controls.Add(this.label18);
            this.grpPerformanceOptions.Controls.Add(this.txtPollDelay);
            this.grpPerformanceOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpPerformanceOptions.Location = new System.Drawing.Point(4, 5);
            this.grpPerformanceOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpPerformanceOptions.Name = "grpPerformanceOptions";
            this.grpPerformanceOptions.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpPerformanceOptions.Size = new System.Drawing.Size(776, 716);
            this.grpPerformanceOptions.TabIndex = 57;
            this.grpPerformanceOptions.TabStop = false;
            this.grpPerformanceOptions.Text = "Performance";
            // 
            // cmdBMSOptions
            // 
            this.cmdBMSOptions.Location = new System.Drawing.Point(18, 146);
            this.cmdBMSOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdBMSOptions.Name = "cmdBMSOptions";
            this.cmdBMSOptions.Size = new System.Drawing.Size(312, 49);
            this.cmdBMSOptions.TabIndex = 52;
            this.cmdBMSOptions.Text = "Falcon BMS Advanced Options...";
            this.cmdBMSOptions.UseVisualStyleBackColor = true;
            this.cmdBMSOptions.Click += new System.EventHandler(this.cmdBMSOptions_Click);
            // 
            // chkOnlyUpdateImagesWhenDataChanges
            // 
            this.chkOnlyUpdateImagesWhenDataChanges.AutoSize = true;
            this.chkOnlyUpdateImagesWhenDataChanges.Location = new System.Drawing.Point(27, 111);
            this.chkOnlyUpdateImagesWhenDataChanges.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkOnlyUpdateImagesWhenDataChanges.Name = "chkOnlyUpdateImagesWhenDataChanges";
            this.chkOnlyUpdateImagesWhenDataChanges.Size = new System.Drawing.Size(478, 24);
            this.chkOnlyUpdateImagesWhenDataChanges.TabIndex = 53;
            this.chkOnlyUpdateImagesWhenDataChanges.Text = "Give rendering priority to instruments with new or changed data";
            this.chkOnlyUpdateImagesWhenDataChanges.UseVisualStyleBackColor = true;
            this.chkOnlyUpdateImagesWhenDataChanges.CheckedChanged += new System.EventHandler(this.chkOnlyUpdateImagesWhenDataChanges_CheckedChanged);
            // 
            // lblMilliseconds
            // 
            this.lblMilliseconds.AutoSize = true;
            this.lblMilliseconds.Location = new System.Drawing.Point(280, 34);
            this.lblMilliseconds.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMilliseconds.Name = "lblMilliseconds";
            this.lblMilliseconds.Size = new System.Drawing.Size(94, 20);
            this.lblMilliseconds.TabIndex = 77;
            this.lblMilliseconds.Text = "milliseconds";
            // 
            // cboThreadPriority
            // 
            this.cboThreadPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboThreadPriority.FormattingEnabled = true;
            this.cboThreadPriority.Location = new System.Drawing.Point(230, 69);
            this.cboThreadPriority.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cboThreadPriority.Name = "cboThreadPriority";
            this.cboThreadPriority.Size = new System.Drawing.Size(236, 28);
            this.cboThreadPriority.TabIndex = 51;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(24, 74);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(113, 20);
            this.label19.TabIndex = 29;
            this.label19.Text = "Thread priority:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(24, 34);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(167, 20);
            this.label18.TabIndex = 27;
            this.label18.Text = "Poll for new data every";
            // 
            // txtPollDelay
            // 
            this.txtPollDelay.Location = new System.Drawing.Point(230, 29);
            this.txtPollDelay.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPollDelay.Name = "txtPollDelay";
            this.txtPollDelay.Size = new System.Drawing.Size(40, 26);
            this.txtPollDelay.TabIndex = 50;
            // 
            // tabGraphics
            // 
            this.tabGraphics.Controls.Add(this.gbGDIOptions);
            this.tabGraphics.Location = new System.Drawing.Point(4, 29);
            this.tabGraphics.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabGraphics.Name = "tabGraphics";
            this.tabGraphics.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabGraphics.Size = new System.Drawing.Size(784, 726);
            this.tabGraphics.TabIndex = 15;
            this.tabGraphics.Text = "Graphics";
            this.tabGraphics.UseVisualStyleBackColor = true;
            // 
            // gbGDIOptions
            // 
            this.gbGDIOptions.Controls.Add(this.lblCompositingQuality);
            this.gbGDIOptions.Controls.Add(this.cbCompositingQuality);
            this.gbGDIOptions.Controls.Add(this.lblTextRenderingHint);
            this.gbGDIOptions.Controls.Add(this.cbTextRenderingHint);
            this.gbGDIOptions.Controls.Add(this.chkHighlightOutputWindowsWhenContainMouseCursor);
            this.gbGDIOptions.Controls.Add(this.lblSmoothingMode);
            this.gbGDIOptions.Controls.Add(this.cbSmoothingMode);
            this.gbGDIOptions.Controls.Add(this.lblPixelOffsetMode);
            this.gbGDIOptions.Controls.Add(this.cbPixelOffsetMode);
            this.gbGDIOptions.Controls.Add(this.lblInterpolationMode);
            this.gbGDIOptions.Controls.Add(this.cbInterpolationMode);
            this.gbGDIOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbGDIOptions.Location = new System.Drawing.Point(4, 5);
            this.gbGDIOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbGDIOptions.Name = "gbGDIOptions";
            this.gbGDIOptions.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbGDIOptions.Size = new System.Drawing.Size(776, 716);
            this.gbGDIOptions.TabIndex = 56;
            this.gbGDIOptions.TabStop = false;
            this.gbGDIOptions.Text = "Graphics";
            // 
            // lblCompositingQuality
            // 
            this.lblCompositingQuality.AutoSize = true;
            this.lblCompositingQuality.Location = new System.Drawing.Point(24, 25);
            this.lblCompositingQuality.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCompositingQuality.Name = "lblCompositingQuality";
            this.lblCompositingQuality.Size = new System.Drawing.Size(153, 20);
            this.lblCompositingQuality.TabIndex = 1;
            this.lblCompositingQuality.Text = "Compositing Quality:";
            // 
            // cbCompositingQuality
            // 
            this.cbCompositingQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCompositingQuality.FormattingEnabled = true;
            this.cbCompositingQuality.Location = new System.Drawing.Point(200, 20);
            this.cbCompositingQuality.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbCompositingQuality.Name = "cbCompositingQuality";
            this.cbCompositingQuality.Size = new System.Drawing.Size(252, 28);
            this.cbCompositingQuality.TabIndex = 2;
            // 
            // lblTextRenderingHint
            // 
            this.lblTextRenderingHint.AutoSize = true;
            this.lblTextRenderingHint.Location = new System.Drawing.Point(24, 189);
            this.lblTextRenderingHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTextRenderingHint.Name = "lblTextRenderingHint";
            this.lblTextRenderingHint.Size = new System.Drawing.Size(154, 20);
            this.lblTextRenderingHint.TabIndex = 9;
            this.lblTextRenderingHint.Text = "Text Rendering Hint:";
            // 
            // cbTextRenderingHint
            // 
            this.cbTextRenderingHint.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTextRenderingHint.FormattingEnabled = true;
            this.cbTextRenderingHint.Location = new System.Drawing.Point(200, 185);
            this.cbTextRenderingHint.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbTextRenderingHint.Name = "cbTextRenderingHint";
            this.cbTextRenderingHint.Size = new System.Drawing.Size(252, 28);
            this.cbTextRenderingHint.TabIndex = 10;
            // 
            // chkHighlightOutputWindowsWhenContainMouseCursor
            // 
            this.chkHighlightOutputWindowsWhenContainMouseCursor.AutoSize = true;
            this.chkHighlightOutputWindowsWhenContainMouseCursor.Location = new System.Drawing.Point(28, 226);
            this.chkHighlightOutputWindowsWhenContainMouseCursor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkHighlightOutputWindowsWhenContainMouseCursor.Name = "chkHighlightOutputWindowsWhenContainMouseCursor";
            this.chkHighlightOutputWindowsWhenContainMouseCursor.Size = new System.Drawing.Size(261, 24);
            this.chkHighlightOutputWindowsWhenContainMouseCursor.TabIndex = 0;
            this.chkHighlightOutputWindowsWhenContainMouseCursor.Text = "Highlight output window borders";
            this.chkHighlightOutputWindowsWhenContainMouseCursor.UseVisualStyleBackColor = true;
            this.chkHighlightOutputWindowsWhenContainMouseCursor.CheckedChanged += new System.EventHandler(this.chkHighlightOutputWindowsWhenContainMouseCursor_CheckedChanged);
            // 
            // lblSmoothingMode
            // 
            this.lblSmoothingMode.AutoSize = true;
            this.lblSmoothingMode.Location = new System.Drawing.Point(24, 148);
            this.lblSmoothingMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSmoothingMode.Name = "lblSmoothingMode";
            this.lblSmoothingMode.Size = new System.Drawing.Size(134, 20);
            this.lblSmoothingMode.TabIndex = 7;
            this.lblSmoothingMode.Text = "Smoothing Mode:";
            // 
            // cbSmoothingMode
            // 
            this.cbSmoothingMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSmoothingMode.FormattingEnabled = true;
            this.cbSmoothingMode.Location = new System.Drawing.Point(200, 143);
            this.cbSmoothingMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbSmoothingMode.Name = "cbSmoothingMode";
            this.cbSmoothingMode.Size = new System.Drawing.Size(252, 28);
            this.cbSmoothingMode.TabIndex = 8;
            // 
            // lblPixelOffsetMode
            // 
            this.lblPixelOffsetMode.AutoSize = true;
            this.lblPixelOffsetMode.Location = new System.Drawing.Point(24, 106);
            this.lblPixelOffsetMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPixelOffsetMode.Name = "lblPixelOffsetMode";
            this.lblPixelOffsetMode.Size = new System.Drawing.Size(137, 20);
            this.lblPixelOffsetMode.TabIndex = 5;
            this.lblPixelOffsetMode.Text = "Pixel Offset Mode:";
            // 
            // cbPixelOffsetMode
            // 
            this.cbPixelOffsetMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPixelOffsetMode.FormattingEnabled = true;
            this.cbPixelOffsetMode.Location = new System.Drawing.Point(200, 102);
            this.cbPixelOffsetMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbPixelOffsetMode.Name = "cbPixelOffsetMode";
            this.cbPixelOffsetMode.Size = new System.Drawing.Size(252, 28);
            this.cbPixelOffsetMode.TabIndex = 6;
            // 
            // lblInterpolationMode
            // 
            this.lblInterpolationMode.AutoSize = true;
            this.lblInterpolationMode.Location = new System.Drawing.Point(24, 66);
            this.lblInterpolationMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInterpolationMode.Name = "lblInterpolationMode";
            this.lblInterpolationMode.Size = new System.Drawing.Size(146, 20);
            this.lblInterpolationMode.TabIndex = 3;
            this.lblInterpolationMode.Text = "Interpolation Mode:";
            // 
            // cbInterpolationMode
            // 
            this.cbInterpolationMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbInterpolationMode.FormattingEnabled = true;
            this.cbInterpolationMode.Location = new System.Drawing.Point(200, 62);
            this.cbInterpolationMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbInterpolationMode.Name = "cbInterpolationMode";
            this.cbInterpolationMode.Size = new System.Drawing.Size(252, 28);
            this.cbInterpolationMode.TabIndex = 4;
            // 
            // tabStartup
            // 
            this.tabStartup.Controls.Add(this.grpStartupOptions);
            this.tabStartup.Location = new System.Drawing.Point(4, 29);
            this.tabStartup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabStartup.Name = "tabStartup";
            this.tabStartup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabStartup.Size = new System.Drawing.Size(784, 726);
            this.tabStartup.TabIndex = 17;
            this.tabStartup.Text = "Startup";
            this.tabStartup.UseVisualStyleBackColor = true;
            // 
            // grpStartupOptions
            // 
            this.grpStartupOptions.Controls.Add(this.chkStartWithWindows);
            this.grpStartupOptions.Controls.Add(this.chkStartOnLaunch);
            this.grpStartupOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpStartupOptions.Location = new System.Drawing.Point(4, 5);
            this.grpStartupOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpStartupOptions.Name = "grpStartupOptions";
            this.grpStartupOptions.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpStartupOptions.Size = new System.Drawing.Size(776, 716);
            this.grpStartupOptions.TabIndex = 55;
            this.grpStartupOptions.TabStop = false;
            this.grpStartupOptions.Text = "Startup";
            // 
            // chkStartWithWindows
            // 
            this.chkStartWithWindows.AutoSize = true;
            this.chkStartWithWindows.Location = new System.Drawing.Point(24, 60);
            this.chkStartWithWindows.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkStartWithWindows.Name = "chkStartWithWindows";
            this.chkStartWithWindows.Size = new System.Drawing.Size(354, 24);
            this.chkStartWithWindows.TabIndex = 55;
            this.chkStartWithWindows.Text = "Launch automatically during Windows startup";
            this.chkStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // chkStartOnLaunch
            // 
            this.chkStartOnLaunch.AutoSize = true;
            this.chkStartOnLaunch.Location = new System.Drawing.Point(24, 25);
            this.chkStartOnLaunch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkStartOnLaunch.Name = "chkStartOnLaunch";
            this.chkStartOnLaunch.Size = new System.Drawing.Size(361, 24);
            this.chkStartOnLaunch.TabIndex = 54;
            this.chkStartOnLaunch.Text = "Start extracting when this program is launched";
            this.chkStartOnLaunch.UseVisualStyleBackColor = true;
            // 
            // cmdResetToDefaults
            // 
            this.cmdResetToDefaults.Location = new System.Drawing.Point(584, 774);
            this.cmdResetToDefaults.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdResetToDefaults.Name = "cmdResetToDefaults";
            this.cmdResetToDefaults.Size = new System.Drawing.Size(189, 35);
            this.cmdResetToDefaults.TabIndex = 152;
            this.cmdResetToDefaults.Text = "Rese&t to Defaults";
            this.cmdResetToDefaults.UseVisualStyleBackColor = true;
            this.cmdResetToDefaults.Click += new System.EventHandler(this.cmdResetToDefaults_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(330, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "&Recover";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label25);
            this.groupBox1.Controls.Add(this.label26);
            this.groupBox1.Controls.Add(this.label27);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.label32);
            this.groupBox1.Controls.Add(this.textBox3);
            this.groupBox1.Controls.Add(this.textBox4);
            this.groupBox1.Location = new System.Drawing.Point(6, 110);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(404, 75);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Secondary 2D Cockpit View Image Source Coordinates";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(6, 48);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(104, 20);
            this.label25.TabIndex = 21;
            this.label25.Text = "Upper Left Y:";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(149, 48);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(113, 20);
            this.label26.TabIndex = 22;
            this.label26.Text = "Lower Right Y:";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(6, 22);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(104, 20);
            this.label27.TabIndex = 19;
            this.label27.Text = "Upper Left X:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(82, 19);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(61, 26);
            this.textBox1.TabIndex = 9;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(232, 45);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(61, 26);
            this.textBox2.TabIndex = 12;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(149, 22);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(113, 20);
            this.label32.TabIndex = 20;
            this.label32.Text = "Lower Right X:";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(82, 45);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(61, 26);
            this.textBox3.TabIndex = 10;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(232, 19);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(61, 26);
            this.textBox4.TabIndex = 11;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(6, 6);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(139, 17);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "Enable Left MFD output";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label33);
            this.groupBox2.Controls.Add(this.label34);
            this.groupBox2.Controls.Add(this.label35);
            this.groupBox2.Controls.Add(this.textBox5);
            this.groupBox2.Controls.Add(this.textBox6);
            this.groupBox2.Controls.Add(this.label36);
            this.groupBox2.Controls.Add(this.textBox7);
            this.groupBox2.Controls.Add(this.textBox8);
            this.groupBox2.Location = new System.Drawing.Point(6, 29);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(404, 75);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Primary 2D Cockpit View Image Source Coordinates";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(6, 48);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(104, 20);
            this.label33.TabIndex = 21;
            this.label33.Text = "Upper Left Y:";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(149, 48);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(113, 20);
            this.label34.TabIndex = 22;
            this.label34.Text = "Lower Right Y:";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(6, 22);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(104, 20);
            this.label35.TabIndex = 19;
            this.label35.Text = "Upper Left X:";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(82, 19);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(61, 26);
            this.textBox5.TabIndex = 4;
            // 
            // textBox6
            // 
            this.textBox6.Location = new System.Drawing.Point(232, 45);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(61, 26);
            this.textBox6.TabIndex = 7;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(149, 22);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(113, 20);
            this.label36.TabIndex = 20;
            this.label36.Text = "Lower Right X:";
            // 
            // textBox7
            // 
            this.textBox7.Location = new System.Drawing.Point(82, 45);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(61, 26);
            this.textBox7.TabIndex = 5;
            // 
            // textBox8
            // 
            this.textBox8.Location = new System.Drawing.Point(232, 19);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(61, 26);
            this.textBox8.TabIndex = 6;
            // 
            // cmdApply
            // 
            this.cmdApply.Location = new System.Drawing.Point(358, 771);
            this.cmdApply.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdApply.Name = "cmdApply";
            this.cmdApply.Size = new System.Drawing.Size(160, 45);
            this.cmdApply.TabIndex = 155;
            this.cmdApply.Text = "&Apply";
            this.cmdApply.UseVisualStyleBackColor = true;
            this.cmdApply.Click += new System.EventHandler(this.cmdApply_Click);
            // 
            // frmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(792, 826);
            this.ControlBox = false;
            this.Controls.Add(this.tabAllTabs);
            this.Controls.Add(this.cmdApply);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdResetToDefaults);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOptions";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Options";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmOptions_FormClosing);
            this.Load += new System.EventHandler(this.frmOptions_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errControlErrorProvider)).EndInit();
            this.tabAllTabs.ResumeLayout(false);
            this.tabInstruments.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tabOtherInstros.ResumeLayout(false);
            this.tabMfdsHud.ResumeLayout(false);
            this.tabMfdsHud.PerformLayout();
            this.tabFlightInstruments.ResumeLayout(false);
            this.tabFlightInstruments.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverDED)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverEHSI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverASI)).EndInit();
            this.grpVVIOptions.ResumeLayout(false);
            this.grpVVIOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverCompass)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverBackupADI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverHSI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverVVI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverCabinPress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverAccelerometer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverISIS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverADI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverAOAIndicator)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverAltimeter)).EndInit();
            this.grpAltimeterStyle.ResumeLayout(false);
            this.grpAltimeterStyle.PerformLayout();
            this.tabEW.ResumeLayout(false);
            this.tabEW.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverAzimuthIndicator)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverCMDS)).EndInit();
            this.grpAzimuthIndicatorStyle.ResumeLayout(false);
            this.grpAzimuthIndicatorStyle.PerformLayout();
            this.grpAzimuthIndicatorBezelTypes.ResumeLayout(false);
            this.grpAzimuthIndicatorBezelTypes.PerformLayout();
            this.grpAzimuthIndicatorDigitalTypes.ResumeLayout(false);
            this.grpAzimuthIndicatorDigitalTypes.PerformLayout();
            this.tabEngineInstruments.ResumeLayout(false);
            this.tabEngineInstruments.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverFuelFlow)).EndInit();
            this.gbEngine1Instros.ResumeLayout(false);
            this.gbEngine1Instros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverRPM1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverOil1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverNozPos1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverFTIT1)).EndInit();
            this.gbEngine2Instros.ResumeLayout(false);
            this.gbEngine2Instros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverRPM2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverOil2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverNozPos2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverFTIT2)).EndInit();
            this.gbFuelQuantityOptions.ResumeLayout(false);
            this.gbFuelQuantityOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverFuelQuantity)).EndInit();
            this.tabHydraulics.ResumeLayout(false);
            this.tabHydraulics.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverHydA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverHydB)).EndInit();
            this.tabFaults.ResumeLayout(false);
            this.tabFaults.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverCautionPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverPFL)).EndInit();
            this.tabIndexers.ResumeLayout(false);
            this.tabIndexers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverNWS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverAOAIndexer)).EndInit();
            this.tabTrim.ResumeLayout(false);
            this.tabTrim.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverPitchTrim)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverRollTrim)).EndInit();
            this.tabEPU.ResumeLayout(false);
            this.tabEPU.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverEPU)).EndInit();
            this.tabGearAndBrakes.ResumeLayout(false);
            this.tabGearAndBrakes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverSpeedbrake)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRecoverGearLights)).EndInit();
            this.tabHotkeys.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tabHotkeysInner.ResumeLayout(false);
            this.tabGeneralKeys.ResumeLayout(false);
            this.tabGeneralKeys.PerformLayout();
            this.tabAccelerometerKeys.ResumeLayout(false);
            this.gbAccelerometer.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tabASI.ResumeLayout(false);
            this.gbAirspeedIndicator.ResumeLayout(false);
            this.gbASIIndexKnob.ResumeLayout(false);
            this.gbASIIndexKnob.PerformLayout();
            this.tabAzimuthIndicatorKeys.ResumeLayout(false);
            this.gbAzimuthIndicator.ResumeLayout(false);
            this.gbAzimuthIndicatorBrightnessControl.ResumeLayout(false);
            this.gbAzimuthIndicatorBrightnessControl.PerformLayout();
            this.tabEHSIKeys.ResumeLayout(false);
            this.gbEHSI.ResumeLayout(false);
            this.gbEHSI.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabISISKeys.ResumeLayout(false);
            this.gbISIS.ResumeLayout(false);
            this.gbISIS.PerformLayout();
            this.tabNetworking.ResumeLayout(false);
            this.grpNetworkMode.ResumeLayout(false);
            this.grpServerOptions.ResumeLayout(false);
            this.grpServerOptions.PerformLayout();
            this.gbNetworkingMode.ResumeLayout(false);
            this.gbNetworkingMode.PerformLayout();
            this.grpClientOptions.ResumeLayout(false);
            this.grpClientOptions.PerformLayout();
            this.tabPerformance.ResumeLayout(false);
            this.grpPerformanceOptions.ResumeLayout(false);
            this.grpPerformanceOptions.PerformLayout();
            this.tabGraphics.ResumeLayout(false);
            this.gbGDIOptions.ResumeLayout(false);
            this.gbGDIOptions.PerformLayout();
            this.tabStartup.ResumeLayout(false);
            this.grpStartupOptions.ResumeLayout(false);
            this.grpStartupOptions.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.ErrorProvider errControlErrorProvider;
        private System.Windows.Forms.Button cmdResetToDefaults;
		private System.Windows.Forms.TabControl tabAllTabs;
		private System.Windows.Forms.CheckBox chkEnableLeftMFD;
		private System.Windows.Forms.TabPage tabHotkeys;
        private System.Windows.Forms.Button cmdRecoverLeftMfd;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.TextBox textBox7;
		private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.TabPage tabInstruments;
        private System.Windows.Forms.CheckBox chkAltimeter;
        private System.Windows.Forms.CheckBox chkADI;
        private System.Windows.Forms.CheckBox chkAOAIndicator;
        private System.Windows.Forms.CheckBox chkHSI;
        private System.Windows.Forms.CheckBox chkFuelFlow;
        private System.Windows.Forms.CheckBox chkFTIT1;
        private System.Windows.Forms.CheckBox chkOIL1;
        private System.Windows.Forms.CheckBox chkNOZ1;
        private System.Windows.Forms.CheckBox chkRPM1;
        private System.Windows.Forms.CheckBox chkVVI;
        private System.Windows.Forms.CheckBox chkRPM2;
        private System.Windows.Forms.CheckBox chkOIL2;
        private System.Windows.Forms.CheckBox chkNOZ2;
        private System.Windows.Forms.CheckBox chkFTIT2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PictureBox pbRecoverADI;
        private System.Windows.Forms.PictureBox pbRecoverAltimeter;
        private System.Windows.Forms.PictureBox pbRecoverAOAIndicator;
        private System.Windows.Forms.PictureBox pbRecoverFTIT1;
        private System.Windows.Forms.PictureBox pbRecoverNozPos1;
        private System.Windows.Forms.PictureBox pbRecoverOil1;
        private System.Windows.Forms.PictureBox pbRecoverRPM1;
        private System.Windows.Forms.PictureBox pbRecoverFTIT2;
        private System.Windows.Forms.PictureBox pbRecoverNozPos2;
        private System.Windows.Forms.PictureBox pbRecoverOil2;
        private System.Windows.Forms.PictureBox pbRecoverRPM2;
        private System.Windows.Forms.PictureBox pbRecoverFuelFlow;
        private System.Windows.Forms.PictureBox pbRecoverVVI;
        private System.Windows.Forms.PictureBox pbRecoverHSI;
        private System.Windows.Forms.PictureBox pbRecoverRollTrim;
        private System.Windows.Forms.CheckBox chkRollTrim;
        private System.Windows.Forms.PictureBox pbRecoverPitchTrim;
        private System.Windows.Forms.CheckBox chkPitchTrim;
        private System.Windows.Forms.PictureBox pbRecoverASI;
        private System.Windows.Forms.CheckBox chkAirspeedIndicator;
        private System.Windows.Forms.GroupBox grpAltimeterStyle;
        private System.Windows.Forms.RadioButton rdoAltimeterStyleElectromechanical;
        private System.Windows.Forms.RadioButton rdoAltimeterStyleDigital;
        private System.Windows.Forms.GroupBox grpVVIOptions;
        private System.Windows.Forms.RadioButton rdoVVIStyleNeedle;
        private System.Windows.Forms.RadioButton rdoVVIStyleTape;
        private Common.InputSupport.GlobalEventProvider globalEventProvider1;
        private Common.InputSupport.GlobalEventProvider globalEventProvider2;
        private System.Windows.Forms.PictureBox pbRecoverISIS;
        private System.Windows.Forms.CheckBox chkISIS;
        private System.Windows.Forms.PictureBox pbRecoverEHSI;
        private System.Windows.Forms.CheckBox chkEHSI;
        private System.Windows.Forms.CheckBox chkAccelerometer;
        private System.Windows.Forms.PictureBox pbRecoverAccelerometer;
        private System.Windows.Forms.Button cmdApply;
		private System.Windows.Forms.Button cmdRecoverMfd4;
		private System.Windows.Forms.CheckBox chkEnableMFD4;
		private System.Windows.Forms.Button cmdRecoverMfd3;
		private System.Windows.Forms.CheckBox chkEnableMFD3;
		private System.Windows.Forms.Button cmdRecoverRightMfd;
		private System.Windows.Forms.CheckBox chkEnableRightMFD;
		private System.Windows.Forms.Button cmdRecoverHud;
		private System.Windows.Forms.CheckBox chkEnableHud;
        private System.Windows.Forms.GroupBox gbEngine2Instros;
        private System.Windows.Forms.GroupBox gbEngine1Instros;
        private System.Windows.Forms.CheckBox chkGearLights;
        private System.Windows.Forms.PictureBox pbRecoverGearLights;
        private System.Windows.Forms.CheckBox chkSpeedbrake;
        private System.Windows.Forms.PictureBox pbRecoverSpeedbrake;
        private System.Windows.Forms.CheckBox chkCompass;
        private System.Windows.Forms.PictureBox pbRecoverCompass;
        private System.Windows.Forms.CheckBox chkStandbyADI;
        private System.Windows.Forms.PictureBox pbRecoverBackupADI;
        private System.Windows.Forms.CheckBox chkEPU;
        private System.Windows.Forms.PictureBox pbRecoverEPU;
        private System.Windows.Forms.CheckBox chkCabinPress;
        private System.Windows.Forms.PictureBox pbRecoverCabinPress;
        private System.Windows.Forms.GroupBox gbFuelQuantityOptions;
        private System.Windows.Forms.RadioButton rdoFuelQuantityDModel;
        private System.Windows.Forms.RadioButton rdoFuelQuantityNeedleCModel;
        private System.Windows.Forms.CheckBox chkFuelQty;
        private System.Windows.Forms.PictureBox pbRecoverFuelQuantity;
        private System.Windows.Forms.PictureBox pbRecoverNWS;
        private System.Windows.Forms.CheckBox chkNWSIndexer;
        private System.Windows.Forms.CheckBox chkAOAIndexer;
        private System.Windows.Forms.PictureBox pbRecoverAOAIndexer;
        private System.Windows.Forms.PictureBox pbRecoverHydB;
        private System.Windows.Forms.CheckBox chkHydB;
        private System.Windows.Forms.PictureBox pbRecoverHydA;
        private System.Windows.Forms.CheckBox chkHydA;
        private System.Windows.Forms.PictureBox pbRecoverPFL;
        private System.Windows.Forms.CheckBox chkPFL;
        private System.Windows.Forms.CheckBox chkDED;
        private System.Windows.Forms.PictureBox pbRecoverDED;
        private System.Windows.Forms.CheckBox chkCautionPanel;
        private System.Windows.Forms.PictureBox pbRecoverCautionPanel;
        private System.Windows.Forms.GroupBox gbAirspeedIndicator;
        private System.Windows.Forms.GroupBox gbASIIndexKnob;
        private System.Windows.Forms.Label lblAirspeedIndexIncreaseHotkey;
        private System.Windows.Forms.Button cmdAirspeedIndexIncreaseHotkey;
        private System.Windows.Forms.Label lblAirspeedIndexDecreaseHotkey;
        private System.Windows.Forms.Button cmdAirspeedIndexDecreaseHotkey;
        private System.Windows.Forms.GroupBox gbISIS;
        private System.Windows.Forms.Label lblISISBrightBrightnessButtonPressed;
        private System.Windows.Forms.Button cmdISISBrightButtonPressed;
        private System.Windows.Forms.Button cmdISISStandardBrightnessButtonPressed;
        private System.Windows.Forms.Label lblISISStandardBrightnessButtonPressed;
        private System.Windows.Forms.GroupBox gbAccelerometer;
        private System.Windows.Forms.Label lblAccelerometerResetButtonPressed;
        private System.Windows.Forms.Button cmdAccelerometerResetButtonPressed;
        private System.Windows.Forms.GroupBox gbEHSI;
        private System.Windows.Forms.Label lblEHSIMenuButton;
        private System.Windows.Forms.Button cmdEHSIMenuButtonHotkey;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblEHSICourseIncreaseHotkey;
        private System.Windows.Forms.Button cmdEHSICourseIncreaseKey;
        private System.Windows.Forms.Button cmdEHSICourseKnobDepressedKey;
        private System.Windows.Forms.Label lblEHSICourseDecreaseHotkey;
        private System.Windows.Forms.Button cmdEHSICourseDecreaseKey;
        private System.Windows.Forms.Label label54;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblEHSIHeadingIncreaseButton;
        private System.Windows.Forms.Button cmdEHSIHeadingIncreaseKey;
        private System.Windows.Forms.Label label53;
        private System.Windows.Forms.Button cmdEHSIHeadingDecreaseKey;
        private System.Windows.Forms.GroupBox gbAzimuthIndicator;
        private System.Windows.Forms.GroupBox gbAzimuthIndicatorBrightnessControl;
        private System.Windows.Forms.Label lblAzimuthIndicatorBrightnessDecrease;
        private System.Windows.Forms.Label lblAzimuthIndicatorBrightnessIncrease;
        private System.Windows.Forms.Button cmdAzimuthIndicatorBrightnessIncrease;
        private System.Windows.Forms.Button cmdAzimuthIndicatorBrightnessDecrease;
        private System.Windows.Forms.Label lblNVIS;
        private System.Windows.Forms.Button cmdNV;
        private System.Windows.Forms.GroupBox grpPerformanceOptions;
        private System.Windows.Forms.Button cmdBMSOptions;
        private System.Windows.Forms.CheckBox chkOnlyUpdateImagesWhenDataChanges;
        private System.Windows.Forms.Label lblMilliseconds;
        private System.Windows.Forms.ComboBox cboThreadPriority;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtPollDelay;
        private System.Windows.Forms.TabPage tabGraphics;
        private System.Windows.Forms.GroupBox grpNetworkMode;
        private System.Windows.Forms.GroupBox gbNetworkingMode;
        private System.Windows.Forms.RadioButton rdoServer;
        private System.Windows.Forms.RadioButton rdoStandalone;
        private System.Windows.Forms.RadioButton rdoClient;
        private System.Windows.Forms.GroupBox grpClientOptions;
        private System.Windows.Forms.Label lblClientServerPortNum;
        private System.Windows.Forms.TextBox txtNetworkClientUseServerPortNum;
        private System.Windows.Forms.Label lblServerIpAddress;
        private IPAddressControl ipaNetworkClientUseServerIpAddress;
        private System.Windows.Forms.GroupBox grpServerOptions;
        private System.Windows.Forms.Label lblCompressionType;
        private System.Windows.Forms.ComboBox cboCompressionType;
        private System.Windows.Forms.Label lblImageFormat;
        private System.Windows.Forms.ComboBox cboImageFormat;
        private System.Windows.Forms.Label lblServerServerPortNum;
        private System.Windows.Forms.TextBox txtNetworkServerUsePortNum;
        private System.Windows.Forms.GroupBox grpStartupOptions;
        private System.Windows.Forms.CheckBox chkStartWithWindows;
        private System.Windows.Forms.CheckBox chkStartOnLaunch;
        private System.Windows.Forms.GroupBox gbGDIOptions;
        private System.Windows.Forms.Label lblCompositingQuality;
        private System.Windows.Forms.ComboBox cbCompositingQuality;
        private System.Windows.Forms.Label lblTextRenderingHint;
        private System.Windows.Forms.ComboBox cbTextRenderingHint;
        private System.Windows.Forms.CheckBox chkHighlightOutputWindowsWhenContainMouseCursor;
        private System.Windows.Forms.Label lblSmoothingMode;
        private System.Windows.Forms.ComboBox cbSmoothingMode;
        private System.Windows.Forms.Label lblPixelOffsetMode;
        private System.Windows.Forms.ComboBox cbPixelOffsetMode;
        private System.Windows.Forms.Label lblInterpolationMode;
        private System.Windows.Forms.ComboBox cbInterpolationMode;
        private System.Windows.Forms.CheckBox chkAzimuthIndicator;
        private System.Windows.Forms.GroupBox grpAzimuthIndicatorStyle;
        private System.Windows.Forms.RadioButton rdoAzimuthIndicatorStyleScope;
        private System.Windows.Forms.RadioButton rdoAzimuthIndicatorStyleDigital;
        private System.Windows.Forms.GroupBox grpAzimuthIndicatorBezelTypes;
        private System.Windows.Forms.RadioButton rdoAzimuthIndicatorNoBezel;
        private System.Windows.Forms.RadioButton rdoRWRHAFBezelType;
        private System.Windows.Forms.RadioButton rdoRWRIP1310BezelType;
        private System.Windows.Forms.GroupBox grpAzimuthIndicatorDigitalTypes;
        private System.Windows.Forms.RadioButton rdoTTD;
        private System.Windows.Forms.RadioButton rdoATDPlus;
        private System.Windows.Forms.PictureBox pbRecoverAzimuthIndicator;
        private System.Windows.Forms.CheckBox chkCMDSPanel;
        private System.Windows.Forms.PictureBox pbRecoverCMDS;
        private System.Windows.Forms.TabPage tabNetworking;
        private System.Windows.Forms.TabPage tabStartup;
        private System.Windows.Forms.TabPage tabPerformance;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TabControl tabOtherInstros;
        private System.Windows.Forms.TabPage tabTrim;
        private System.Windows.Forms.TabPage tabFaults;
        private System.Windows.Forms.TabPage tabFlightInstruments;
        private System.Windows.Forms.TabPage tabEngineInstruments;
        private System.Windows.Forms.TabPage tabEW;
        private System.Windows.Forms.TabPage tabIndexers;
        private System.Windows.Forms.TabPage tabHydraulics;
        private System.Windows.Forms.TabPage tabEPU;
        private System.Windows.Forms.TabPage tabGearAndBrakes;
        private System.Windows.Forms.TabControl tabHotkeysInner;
        private System.Windows.Forms.TabPage tabGeneralKeys;
        private System.Windows.Forms.TabPage tabAccelerometerKeys;
        private System.Windows.Forms.TabPage tabASI;
        private System.Windows.Forms.TabPage tabAzimuthIndicatorKeys;
        private System.Windows.Forms.TabPage tabEHSIKeys;
        private System.Windows.Forms.TabPage tabISISKeys;
        private System.Windows.Forms.TabPage tabMfdsHud;

    }
}