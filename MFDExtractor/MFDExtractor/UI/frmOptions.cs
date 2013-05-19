using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Common.InputSupport.UI;
using Common.Strings;
using LightningGauges.Renderers;
using MFDExtractor.Properties;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using log4net;
using Common.Networking;

namespace MFDExtractor.UI
{
    public enum VVIStyles
    {
        Tape,
        Needle
    }

    /// <summary>
    ///     Code-behind for the Options form
    /// </summary>
    public partial class frmOptions : Form
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (frmOptions));

        /// <summary>
        ///     the current Extractor engine instance
        /// </summary>
        private readonly Extractor _extractor = Extractor.GetInstance();

        /// <summary>
        ///     specifies whether the extractor should be running when the Options form exits (because
        ///     the Options form specifically stops the Extractor, so it should re-start it if
        ///     it was already started prior to entering the Options form)
        /// </summary>
        private bool _extractorRunningStateOnFormOpen;

        private bool _formLoading = true;
        private volatile bool _restartScheduled = false;

        /// <summary>
        ///     Default constructor for the Options Form
        /// </summary>
        public frmOptions()
        {
            InitializeComponent();
        }


        /// <summary>
        ///     Event handler for the form's Load event
        /// </summary>
        /// <param name="sender">the object raising this event</param>
        /// <param name="e">Event arguments for the form's Load event</param>
        private void frmOptions_Load(object sender, EventArgs e)
        {
            _extractorRunningStateOnFormOpen = _extractor.State.Running; //store current running
            //state of the Extractor engine
            //stop the Extractor engine
			if (_extractor.State.Running)
            {
                _extractor.Stop();
            }

            //register for the Extractor's DataChanged event
            _extractor.DataChanged += extractor_DataChanged;

            //put the Extractor into Test mode (displays the Test/Blank images)
			_extractor.State.TestMode = true;
            //set the titlebar for the Options form
            Text = Application.ProductName + " v" + Application.ProductVersion + " Options";

            //inform the shortcut-input controls that allow the user to input a hotkey for 2D mode 
            //and 3D mode, that there are "modifier" keys required (i.e. you're allowed to use "A" as
            //a hotkey without requiring something like "CTRL-A" or "ALT-A" or "SHIFT-A"
            sciPrimary2DModeHotkey.MinModifiers = 0;
            sciSecondary2DModeHotkey.MinModifiers = 0;
            sci3DModeHotkey.MinModifiers = 0;

            //add list of possible thread priority values to the thread priority drop-down list
            string[] names = Enum.GetNames(typeof (ThreadPriority));
            Array.Reverse(names);
            cboThreadPriority.Items.AddRange(names);

            cboImageFormat.Items.Add("BMP");
            cboImageFormat.Items.Add("GIF");
            cboImageFormat.Items.Add("JPEG");
            cboImageFormat.Items.Add("PNG");
            cboImageFormat.Items.Add("TIFF");

            PopulateGDIPlusOptionsCombos();

            //force a reload of the user settings from the in-memory user-config
            LoadSettings();

            _extractor.Start();
            _formLoading = false;
        }

        private void PopulateGDIPlusOptionsCombos()
        {
            cbInterpolationMode.Items.Clear();
            var interpolationModes = new List<InterpolationMode>();
            foreach (object val in Enum.GetValues(typeof (InterpolationMode)))
            {
                if ((InterpolationMode) val != InterpolationMode.Invalid)
                {
                    interpolationModes.Add((InterpolationMode) val);
                }
            }
            cbInterpolationMode.DataSource = interpolationModes;

            cbSmoothingMode.Items.Clear();
            var smoothingModes = new List<SmoothingMode>();
            Array vals = Enum.GetValues(typeof (SmoothingMode));
            Array.Sort(vals);
            foreach (object val in vals)
            {
                if ((SmoothingMode) val != SmoothingMode.Invalid)
                {
                    smoothingModes.Add((SmoothingMode) val);
                }
            }
            cbSmoothingMode.DataSource = smoothingModes;


            cbPixelOffsetMode.Items.Clear();
            var pixelOffsetModes = new List<PixelOffsetMode>();
            vals = Enum.GetValues(typeof (PixelOffsetMode));
            Array.Sort(vals);
            foreach (object val in vals)
            {
                if ((PixelOffsetMode) val != PixelOffsetMode.Invalid)
                {
                    pixelOffsetModes.Add((PixelOffsetMode) val);
                }
            }
            cbPixelOffsetMode.DataSource = pixelOffsetModes;

            cbTextRenderingHint.Items.Clear();
            Array vals2 = Enum.GetValues(typeof (TextRenderingHint));
            Array.Sort(vals2);
            cbTextRenderingHint.DataSource = vals2;

            cbCompositingQuality.Items.Clear();
            var compositingQualities = new List<CompositingQuality>();
            vals = Enum.GetValues(typeof (CompositingQuality));
            Array.Sort(vals);
            foreach (object val in vals)
            {
                if ((CompositingQuality) val != CompositingQuality.Invalid)
                {
                    compositingQualities.Add((CompositingQuality) val);
                }
            }
            cbCompositingQuality.DataSource = compositingQualities;
        }

        private void LoadGDIPlusSettings()
        {
            cbInterpolationMode.SelectedItem = Settings.Default.InterpolationMode;
            cbSmoothingMode.SelectedItem = Settings.Default.SmoothingMode;
            cbPixelOffsetMode.SelectedItem = Settings.Default.PixelOffsetMode;
            cbTextRenderingHint.SelectedItem = Settings.Default.TextRenderingHint;
            cbCompositingQuality.SelectedItem = Settings.Default.CompositingQuality;
        }

        private void SaveGDIPlusSettings()
        {
            Settings.Default.InterpolationMode = (InterpolationMode) cbInterpolationMode.SelectedItem;
            Settings.Default.SmoothingMode = (SmoothingMode) cbSmoothingMode.SelectedItem;
            Settings.Default.PixelOffsetMode = (PixelOffsetMode) cbPixelOffsetMode.SelectedItem;
            Settings.Default.TextRenderingHint = (TextRenderingHint) cbTextRenderingHint.SelectedItem;
            Settings.Default.CompositingQuality = (CompositingQuality) cbCompositingQuality.SelectedItem;
        }

        private void UpdateCompressionTypeList()
        {
            cboCompressionType.Items.Clear();
            switch (cboImageFormat.SelectedItem.ToString())
            {
                case "BMP":
                    cboCompressionType.Items.Add("None");
                    cboCompressionType.Items.Add("RLE");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("RLE");
                    cboCompressionType.Enabled = true;
                    lblCompressionType.Enabled = true;
                    break;
                case "GIF":
                    cboCompressionType.Items.Add("None");
                    cboCompressionType.Items.Add("RLE");
                    cboCompressionType.Items.Add("LZW");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("LZW");
                    cboCompressionType.Enabled = true;
                    lblCompressionType.Enabled = true;
                    break;
                case "JPEG":
                    cboCompressionType.Items.Add("Implied");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("Implied");
                    cboCompressionType.Enabled = false;
                    lblCompressionType.Enabled = false;
                    break;
                case "PNG":
                    cboCompressionType.Items.Add("None");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("None");
                    cboCompressionType.Enabled = false;
                    lblCompressionType.Enabled = false;
                    break;
                case "TIFF":
                    cboCompressionType.Items.Add("None");
                    cboCompressionType.Items.Add("LZW");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("LZW");
                    cboCompressionType.Enabled = true;
                    lblCompressionType.Enabled = true;
                    break;
                default:
                    cboCompressionType.Items.Add("None");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("");
                    cboCompressionType.Enabled = false;
                    lblCompressionType.Enabled = false;
                    break;
            }
        }

        /// <summary>
        ///     Event handler for the Extractor engine's DataChanged event
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">EventArgs class for the Extractor engine's DataChanged event</param>
        private void extractor_DataChanged(object sender, EventArgs e)
        {
            //store the currently-selected user control on the Options form (required because
            //when we reload the user settings in the next step, the currenty-selected
            //control will go out of focus
            Control currentControl = ActiveControl;

            //reload user settings from the in-memory user config
            LoadSettings();

            //refocus the control that was in focus before we reloaded the user settings
            ActiveControl = currentControl;
        }

        /// <summary>
        ///     Reloads user settings from the in-memory user settings class (Properties.Settings)
        /// </summary>
        private void LoadSettings()
        {
            //load all committed user settings from memory (these may not mirror the settings
            //which have been persisted to the user-config file on disk; that only happens 
            //when Properties.Settings.Default.Save() is called)
            Settings settings = Settings.Default;
            settings.UpgradeNeeded = false;

            //update the UI elements with the corresponding settings values that we just read in
            ipaNetworkClientUseServerIpAddress.Text = settings.ClientUseServerIpAddress;
            txtNetworkClientUseServerPortNum.Text =
                settings.ClientUseServerPortNum.ToString(CultureInfo.InvariantCulture);
            txtNetworkServerUsePortNum.Text = settings.ServerUsePortNumber.ToString(CultureInfo.InvariantCulture);

            //set the current Extractor instance's network mode as per the user-config
            var networkMode = (NetworkMode) settings.NetworkingMode;
            if (networkMode == NetworkMode.Client)
            {
                EnableClientModeOptions();
            }
            else if (networkMode == NetworkMode.Server)
            {
                EnableServerModeOptions();
            }
            else
            {
                EnableStandaloneModeOptions();
            }

            cboThreadPriority.SelectedItem = settings.ThreadPriority.ToString();


            txtPrimaryViewMFD4_ULX.Text = "" + settings.Primary_MFD4_2D_ULX;
            txtPrimaryViewMFD4_ULY.Text = "" + settings.Primary_MFD4_2D_ULY;
            txtPrimaryViewMFD4_LRX.Text = "" + settings.Primary_MFD4_2D_LRX;
            txtPrimaryViewMFD4_LRY.Text = "" + settings.Primary_MFD4_2D_LRY;
            txtPrimaryViewMFD3_ULX.Text = "" + settings.Primary_MFD3_2D_ULX;
            txtPrimaryViewMFD3_ULY.Text = "" + settings.Primary_MFD3_2D_ULY;
            txtPrimaryViewMFD3_LRX.Text = "" + settings.Primary_MFD3_2D_LRX;
            txtPrimaryViewMFD3_LRY.Text = "" + settings.Primary_MFD3_2D_LRY;
            txtPrimaryViewLMFD_ULX.Text = "" + settings.Primary_LMFD_2D_ULX;
            txtPrimaryViewLMFD_ULY.Text = "" + settings.Primary_LMFD_2D_ULY;
            txtPrimaryViewLMFD_LRX.Text = "" + settings.Primary_LMFD_2D_LRX;
            txtPrimaryViewLMFD_LRY.Text = "" + settings.Primary_LMFD_2D_LRY;
            txtPrimaryViewRMFD_ULX.Text = "" + settings.Primary_RMFD_2D_ULX;
            txtPrimaryViewRMFD_ULY.Text = "" + settings.Primary_RMFD_2D_ULY;
            txtPrimaryViewRMFD_LRX.Text = "" + settings.Primary_RMFD_2D_LRX;
            txtPrimaryViewRMFD_LRY.Text = "" + settings.Primary_RMFD_2D_LRY;
            txtPrimaryViewHUD_ULX.Text = "" + settings.Primary_HUD_2D_ULX;
            txtPrimaryViewHUD_ULY.Text = "" + settings.Primary_HUD_2D_ULY;
            txtPrimaryViewHUD_LRX.Text = "" + settings.Primary_HUD_2D_LRX;
            txtPrimaryViewHUD_LRY.Text = "" + settings.Primary_HUD_2D_LRY;


            txtSecondaryViewMFD4_ULX.Text = "" + settings.Secondary_MFD4_2D_ULX;
            txtSecondaryViewMFD4_ULY.Text = "" + settings.Secondary_MFD4_2D_ULY;
            txtSecondaryViewMFD4_LRX.Text = "" + settings.Secondary_MFD4_2D_LRX;
            txtSecondaryViewMFD4_LRY.Text = "" + settings.Secondary_MFD4_2D_LRY;
            txtSecondaryViewMFD3_ULX.Text = "" + settings.Secondary_MFD3_2D_ULX;
            txtSecondaryViewMFD3_ULY.Text = "" + settings.Secondary_MFD3_2D_ULY;
            txtSecondaryViewMFD3_LRX.Text = "" + settings.Secondary_MFD3_2D_LRX;
            txtSecondaryViewMFD3_LRY.Text = "" + settings.Secondary_MFD3_2D_LRY;
            txtSecondaryViewLMFD_ULX.Text = "" + settings.Secondary_LMFD_2D_ULX;
            txtSecondaryViewLMFD_ULY.Text = "" + settings.Secondary_LMFD_2D_ULY;
            txtSecondaryViewLMFD_LRX.Text = "" + settings.Secondary_LMFD_2D_LRX;
            txtSecondaryViewLMFD_LRY.Text = "" + settings.Secondary_LMFD_2D_LRY;
            txtSecondaryViewRMFD_ULX.Text = "" + settings.Secondary_RMFD_2D_ULX;
            txtSecondaryViewRMFD_ULY.Text = "" + settings.Secondary_RMFD_2D_ULY;
            txtSecondaryViewRMFD_LRX.Text = "" + settings.Secondary_RMFD_2D_LRX;
            txtSecondaryViewRMFD_LRY.Text = "" + settings.Secondary_RMFD_2D_LRY;
            txtSecondaryViewHUD_ULX.Text = "" + settings.Secondary_HUD_2D_ULX;
            txtSecondaryViewHUD_ULY.Text = "" + settings.Secondary_HUD_2D_ULY;
            txtSecondaryViewHUD_LRX.Text = "" + settings.Secondary_HUD_2D_LRX;
            txtSecondaryViewHUD_LRY.Text = "" + settings.Secondary_HUD_2D_LRY;


            chkEnableMFD4.Checked = settings.EnableMfd4Output;
            chkEnableMFD3.Checked = settings.EnableMfd3Output;
            chkEnableLeftMFD.Checked = settings.EnableLeftMFDOutput;
            chkEnableRightMFD.Checked = settings.EnableRightMFDOutput;
            chkEnableHud.Checked = settings.EnableHudOutput;

            cmdRecoverHud.Enabled = chkEnableHud.Checked;
            cmdRecoverMfd3.Enabled = chkEnableMFD3.Checked;
            cmdRecoverMfd4.Enabled = chkEnableMFD4.Checked;
            cmdRecoverLeftMfd.Enabled = chkEnableLeftMFD.Checked;
            cmdRecoverRightMfd.Enabled = chkEnableRightMFD.Checked;

            sciPrimary2DModeHotkey.Keys = settings.TwoDPrimaryHotkey;
            sciPrimary2DModeHotkey.Refresh();

            sciSecondary2DModeHotkey.Keys = settings.TwoDSecondaryHotkey;
            sciSecondary2DModeHotkey.Refresh();

            sci3DModeHotkey.Keys = settings.ThreeDHotkey;
            sci3DModeHotkey.Refresh();
            chkStartOnLaunch.Checked = settings.StartOnLaunch;
            chkStartWithWindows.Checked = settings.LaunchWithWindows;

            txtPollDelay.Text = "" + settings.PollingDelay;
            cboThreadPriority.SelectedItem = settings.ThreadPriority;
            cboImageFormat.SelectedIndex = cboImageFormat.FindString(settings.NetworkImageFormat);
            UpdateCompressionTypeList();
            cboCompressionType.SelectedIndex = cboCompressionType.FindString(settings.CompressionType);

            chkAzimuthIndicator.Checked = settings.EnableRWROutput;
            chkADI.Checked = settings.EnableADIOutput;
            chkStandbyADI.Checked = settings.EnableBackupADIOutput;
            chkAirspeedIndicator.Checked = settings.EnableASIOutput;
            chkAltimeter.Checked = settings.EnableAltimeterOutput;
            chkAOAIndexer.Checked = settings.EnableAOAIndexerOutput;
            chkAOAIndicator.Checked = settings.EnableAOAIndicatorOutput;
            chkCautionPanel.Checked = settings.EnableCautionPanelOutput;
            chkCMDSPanel.Checked = settings.EnableCMDSOutput;
            chkCompass.Checked = settings.EnableCompassOutput;
            chkDED.Checked = settings.EnableDEDOutput;
            chkFTIT1.Checked = settings.EnableFTIT1Output;
            chkFTIT2.Checked = settings.EnableFTIT2Output;
            chkAccelerometer.Checked = settings.EnableAccelerometerOutput;
            chkNOZ1.Checked = settings.EnableNOZ1Output;
            chkNOZ2.Checked = settings.EnableNOZ2Output;
            chkOIL1.Checked = settings.EnableOIL1Output;
            chkOIL2.Checked = settings.EnableOIL2Output;
            chkRPM1.Checked = settings.EnableRPM1Output;
            chkRPM2.Checked = settings.EnableRPM2Output;
            chkEPU.Checked = settings.EnableEPUFuelOutput;
            chkFuelFlow.Checked = settings.EnableFuelFlowOutput;
            chkISIS.Checked = settings.EnableISISOutput;
            chkFuelQty.Checked = settings.EnableFuelQuantityOutput;
            chkGearLights.Checked = settings.EnableGearLightsOutput;
            chkHSI.Checked = settings.EnableHSIOutput;
            chkEHSI.Checked = settings.EnableEHSIOutput;
            chkNWSIndexer.Checked = settings.EnableNWSIndexerOutput;
            chkPFL.Checked = settings.EnablePFLOutput;
            chkSpeedbrake.Checked = settings.EnableSpeedbrakeOutput;
            chkVVI.Checked = settings.EnableVVIOutput;
            chkHydA.Checked = settings.EnableHYDAOutput;
            chkHydB.Checked = settings.EnableHYDBOutput;
            chkCabinPress.Checked = settings.EnableCabinPressOutput;
            chkRollTrim.Checked = settings.EnableRollTrimOutput;
            chkPitchTrim.Checked = settings.EnablePitchTrimOutput;


            string azimuthIndicatorType = settings.AzimuthIndicatorType;
            var azimuthIndicatorStyle =
                (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle)
                Enum.Parse(typeof (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle), azimuthIndicatorType);
            switch (azimuthIndicatorStyle)
            {
                case F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.IP1310ALR:
                    if (settings.AzimuthIndicator_ShowBezel)
                    {
                        rdoAzimuthIndicatorStyleDigital.Checked = false;
                        rdoRWRHAFBezelType.Checked = false;
                        rdoATDPlus.Checked = false;
                        rdoAzimuthIndicatorNoBezel.Checked = false;
                        rdoRWRIP1310BezelType.Checked = true;
                        rdoAzimuthIndicatorStyleScope.Checked = true;
                    }
                    else
                    {
                        rdoAzimuthIndicatorStyleDigital.Checked = false;
                        rdoRWRIP1310BezelType.Checked = false;
                        rdoRWRHAFBezelType.Checked = false;
                        rdoATDPlus.Checked = false;
                        rdoAzimuthIndicatorNoBezel.Checked = true;
                        rdoAzimuthIndicatorStyleScope.Checked = true;
                    }
                    break;
                case F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.HAF:
                    rdoAzimuthIndicatorStyleDigital.Checked = false;
                    rdoRWRIP1310BezelType.Checked = false;
                    rdoATDPlus.Checked = false;
                    rdoAzimuthIndicatorNoBezel.Checked = false;
                    rdoRWRHAFBezelType.Checked = true;
                    rdoAzimuthIndicatorStyleScope.Checked = true;
                    break;
                case F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.AdvancedThreatDisplay:
                    rdoAzimuthIndicatorStyleScope.Checked = false;
                    rdoRWRIP1310BezelType.Checked = false;
                    rdoRWRHAFBezelType.Checked = false;
                    rdoAzimuthIndicatorNoBezel.Checked = false;
                    rdoAzimuthIndicatorStyleDigital.Checked = true;
                    rdoATDPlus.Checked = true;
                    break;
                default:
                    break;
            }

            string altimeterStyleString = Settings.Default.Altimeter_Style;
            var altimeterStyle =
                (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle)
                Enum.Parse(typeof (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle), altimeterStyleString);
            switch (altimeterStyle)
            {
                case F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electromechanical:
                    rdoAltimeterStyleElectromechanical.Checked = true;
                    rdoAltimeterStyleDigital.Checked = false;
                    break;
                case F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electronic:
                    rdoAltimeterStyleElectromechanical.Checked = false;
                    rdoAltimeterStyleDigital.Checked = true;
                    break;
                default:
                    break;
            }

            string pressureUnitsString = Settings.Default.Altimeter_PressureUnits;
            var pressureUnits =
                (F16Altimeter.F16AltimeterOptions.PressureUnits)
                Enum.Parse(typeof (F16Altimeter.F16AltimeterOptions.PressureUnits), pressureUnitsString);
            switch (pressureUnits)
            {
                case F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury:
                    rdoInchesOfMercury.Checked = true;
                    rdoMillibars.Checked = false;
                    break;
                case F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars:
                    rdoInchesOfMercury.Checked = false;
                    rdoMillibars.Checked = true;
                    break;
                default:
                    break;
            }

            string vviStyleString = Settings.Default.VVI_Style;
            var vviStyle = (VVIStyles) Enum.Parse(typeof (VVIStyles), vviStyleString);
            switch (vviStyle)
            {
                case VVIStyles.Tape:
                    rdoVVIStyleNeedle.Checked = false;
                    rdoVVIStyleTape.Checked = true;
                    break;
                case VVIStyles.Needle:
                    rdoVVIStyleNeedle.Checked = true;
                    rdoVVIStyleTape.Checked = false;
                    break;
                default:
                    break;
            }
            grpVVIOptions.Enabled = chkVVI.Checked;
            grpAltimeterStyle.Enabled = chkAltimeter.Checked;
            //grpPressureAltitudeSettings.Enabled = chkAltimeter.Checked;
            grpAzimuthIndicatorStyle.Enabled = chkAzimuthIndicator.Checked;

            rdoFuelQuantityNeedleCModel.Checked = Settings.Default.FuelQuantityIndicator_NeedleCModel;
            rdoFuelQuantityDModel.Checked = !Settings.Default.FuelQuantityIndicator_NeedleCModel;

            gbFuelQuantityOptions.Enabled = chkFuelQty.Checked;
            LoadGDIPlusSettings();
            chkHighlightOutputWindowsWhenContainMouseCursor.Checked = Settings.Default.HighlightOutputWindows;
            chkOnlyUpdateImagesWhenDataChanges.Checked = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
        }

        /// <summary>
        ///     Update's the Form's ErrorProvider to notify the user that a user-input item has
        ///     an error.  This method is called during user-input validation in order to provide
        ///     feedback to the user, via the Form's ErrorProvider.
        /// </summary>
        /// <param name="control">
        ///     the <see cref="Control" /> that contains a user-input error.
        /// </param>
        /// <param name="errorString">
        ///     a descriptive message to display to the user when they hover
        ///     over the error symbol which the ErrorProvider places next to the <see cref="Control" /> containing
        ///     the error
        /// </param>
        private void SetError(Control control, string errorString)
        {
            //inform the Form's ErrorProvider of the error
            errControlErrorProvider.SetError(control, errorString);

            //locate the tab on which the specified Control has been placed
            Control parent = control.Parent;
            while (parent != null && parent.GetType() != typeof (TabPage))
            {
                parent = parent.Parent;
            }
            //make the errored control's parent Tab control visible
            tabAllTabs.SelectedTab = ((TabPage) parent);

            //now set focus to the errored control itself
            control.Focus();
        }

        /// <summary>
        ///     Validate all user input on all tabs
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if validation succeeds, or <see langword="false" />, if validation fails due to
        ///     a user-input error
        /// </returns>
        private bool ValidateSettings()
        {
            bool isValid = true; //start with the assumption that all user input is already valid

            errControlErrorProvider.Clear();
            //clear any errors from in the Form's ErrorProvider (leftovers from previous validation attempts)

            if (!sciPrimary2DModeHotkey.IsValid)
            {
                SetError(sciPrimary2DModeHotkey, "Please select a valid hotkey for 2D Mode (primary view).");
                isValid = false;
            }
            else if (!sciSecondary2DModeHotkey.IsValid)
            {
                SetError(sciSecondary2DModeHotkey, "Please select a valid hotkey for 2D Mode (seoncdary view).");
                isValid = false;
            }
            else if (!sci3DModeHotkey.IsValid)
            {
                SetError(sci3DModeHotkey, "Please select a valid hotkey for 3D Mode.");
                isValid = false;
            }
            else if (sciPrimary2DModeHotkey.Keys == sci3DModeHotkey.Keys)
            {
                SetError(sciPrimary2DModeHotkey,
                         "Please select a different hotkey for 2D Mode (primary view) and 3D Mode.");
                SetError(sci3DModeHotkey, "Please select a different hotkey for 2D Mode (primary view) and 3D Mode.");
                isValid = false;
            }
            else if (sciSecondary2DModeHotkey.Keys == sci3DModeHotkey.Keys)
            {
                SetError(sciSecondary2DModeHotkey,
                         "Please select a different hotkey for 2D Mode (secondary view) and 3D Mode.");
                SetError(sci3DModeHotkey, "Please select a different hotkey for 2D Mode (secondary view) and 3D Mode.");
                isValid = false;
            }
            else if (sciPrimary2DModeHotkey.Keys == sciSecondary2DModeHotkey.Keys)
            {
                SetError(sciPrimary2DModeHotkey,
                         "Please select a different hotkey for 2D Mode (primary view) and 2D Mode (secondary view).");
                SetError(sciSecondary2DModeHotkey,
                         "Please select a different hotkey for 2D Mode (primary view) and 2D Mode (secondary view).");
                isValid = false;
            }
            if (isValid && (rdoServer.Checked || rdoStandalone.Checked))
            {
                if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD4_ULX.Text))
                {
                    SetError(txtPrimaryViewMFD4_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD4_ULY.Text))
                {
                    SetError(txtPrimaryViewMFD4_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD4_LRX.Text))
                {
                    SetError(txtPrimaryViewMFD4_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD4_LRY.Text))
                {
                    SetError(txtPrimaryViewMFD4_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD3_ULX.Text))
                {
                    SetError(txtPrimaryViewMFD3_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD3_ULY.Text))
                {
                    SetError(txtPrimaryViewMFD3_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD3_LRX.Text))
                {
                    SetError(txtPrimaryViewMFD3_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD3_LRY.Text))
                {
                    SetError(txtPrimaryViewMFD3_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewLMFD_ULX.Text))
                {
                    SetError(txtPrimaryViewLMFD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewLMFD_ULY.Text))
                {
                    SetError(txtPrimaryViewLMFD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewLMFD_LRX.Text))
                {
                    SetError(txtPrimaryViewLMFD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewLMFD_LRY.Text))
                {
                    SetError(txtPrimaryViewLMFD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewRMFD_ULX.Text))
                {
                    SetError(txtPrimaryViewRMFD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewRMFD_ULY.Text))
                {
                    SetError(txtPrimaryViewRMFD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewRMFD_LRX.Text))
                {
                    SetError(txtPrimaryViewRMFD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewRMFD_LRY.Text))
                {
                    SetError(txtPrimaryViewRMFD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewHUD_ULX.Text))
                {
                    SetError(txtPrimaryViewHUD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewHUD_ULY.Text))
                {
                    SetError(txtPrimaryViewHUD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewHUD_LRX.Text))
                {
                    SetError(txtPrimaryViewHUD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewHUD_LRY.Text))
                {
                    SetError(txtPrimaryViewHUD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD4_ULX.Text))
                {
                    SetError(txtSecondaryViewMFD4_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD4_ULY.Text))
                {
                    SetError(txtSecondaryViewMFD4_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD4_LRX.Text))
                {
                    SetError(txtSecondaryViewMFD4_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD4_LRY.Text))
                {
                    SetError(txtSecondaryViewMFD4_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD3_ULX.Text))
                {
                    SetError(txtSecondaryViewMFD3_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD3_ULY.Text))
                {
                    SetError(txtSecondaryViewMFD3_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD3_LRX.Text))
                {
                    SetError(txtSecondaryViewMFD3_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD3_LRY.Text))
                {
                    SetError(txtSecondaryViewMFD3_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewLMFD_ULX.Text))
                {
                    SetError(txtSecondaryViewLMFD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewLMFD_ULY.Text))
                {
                    SetError(txtSecondaryViewLMFD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewLMFD_LRX.Text))
                {
                    SetError(txtSecondaryViewLMFD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewLMFD_LRY.Text))
                {
                    SetError(txtSecondaryViewLMFD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewRMFD_ULX.Text))
                {
                    SetError(txtSecondaryViewRMFD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewRMFD_ULY.Text))
                {
                    SetError(txtSecondaryViewRMFD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewRMFD_LRX.Text))
                {
                    SetError(txtSecondaryViewRMFD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewRMFD_LRY.Text))
                {
                    SetError(txtSecondaryViewRMFD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewHUD_ULX.Text))
                {
                    SetError(txtSecondaryViewHUD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewHUD_ULY.Text))
                {
                    SetError(txtSecondaryViewHUD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewHUD_LRX.Text))
                {
                    SetError(txtSecondaryViewHUD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewHUD_LRY.Text))
                {
                    SetError(txtSecondaryViewHUD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }

                if (isValid)
                {
                    if (Convert.ToInt32(txtPrimaryViewMFD4_ULX.Text, CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtPrimaryViewMFD4_LRX.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewMFD4_ULX, "Must be <= the lower right X value.");
                        SetError(txtPrimaryViewMFD4_LRX, "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (Convert.ToInt32(txtPrimaryViewMFD4_ULY.Text, CultureInfo.InvariantCulture) >
                             Convert.ToInt32(txtPrimaryViewMFD4_LRY.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewMFD4_ULY, "Must be <= the lower right Y value.");
                        SetError(txtPrimaryViewMFD4_LRY, "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (Convert.ToInt32(txtPrimaryViewMFD3_ULX.Text, CultureInfo.InvariantCulture) >
                             Convert.ToInt32(txtPrimaryViewMFD3_LRX.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewMFD3_ULX, "Must be <= the lower right X value.");
                        SetError(txtPrimaryViewMFD3_LRX, "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (Convert.ToInt32(txtPrimaryViewMFD3_ULY.Text, CultureInfo.InvariantCulture) >
                             Convert.ToInt32(txtPrimaryViewMFD3_LRY.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewMFD3_ULY, "Must be <= the lower right Y value.");
                        SetError(txtPrimaryViewMFD3_LRY, "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (Convert.ToInt32(txtPrimaryViewLMFD_ULX.Text, CultureInfo.InvariantCulture) >
                             Convert.ToInt32(txtPrimaryViewLMFD_LRX.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewLMFD_ULX, "Must be <= the lower right X value.");
                        SetError(txtPrimaryViewLMFD_LRX, "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (Convert.ToInt32(txtPrimaryViewLMFD_ULY.Text, CultureInfo.InvariantCulture) >
                             Convert.ToInt32(txtPrimaryViewLMFD_LRY.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewLMFD_ULY, "Must be <= the lower right Y value.");
                        SetError(txtPrimaryViewLMFD_LRY, "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtPrimaryViewRMFD_ULX.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtPrimaryViewRMFD_LRX.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewRMFD_ULX, "Must be <= the lower right X value.");
                        SetError(txtPrimaryViewRMFD_LRX, "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtPrimaryViewRMFD_ULY.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtPrimaryViewRMFD_LRY.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewRMFD_ULY,
                                 "Must be <= the lower right Y value.");
                        SetError(txtPrimaryViewRMFD_LRY,
                                 "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtPrimaryViewHUD_ULX.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtPrimaryViewHUD_LRX.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewHUD_ULX,
                                 "Must be <= the lower right X value.");
                        SetError(txtPrimaryViewHUD_LRX,
                                 "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtPrimaryViewHUD_ULY.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtPrimaryViewHUD_LRY.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewHUD_ULY,
                                 "Must be <= the lower right Y value.");
                        SetError(txtPrimaryViewHUD_LRY,
                                 "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtSecondaryViewMFD4_ULX.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtSecondaryViewMFD4_LRX.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewMFD4_ULX,
                                 "Must be <= the lower right X value.");
                        SetError(txtSecondaryViewMFD4_LRX,
                                 "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtSecondaryViewMFD4_ULY.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtSecondaryViewMFD4_LRY.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewMFD4_ULY,
                                 "Must be <= the lower right Y value.");
                        SetError(txtSecondaryViewMFD4_LRY,
                                 "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtSecondaryViewMFD3_ULX.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtSecondaryViewMFD3_LRX.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewMFD3_ULX,
                                 "Must be <= the lower right X value.");
                        SetError(txtSecondaryViewMFD3_LRX,
                                 "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewMFD3_ULY.Text,
                            CultureInfo.InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewMFD3_LRY.Text,
                            CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewMFD3_ULY,
                                 "Must be <= the lower right Y value.");
                        SetError(txtSecondaryViewMFD3_LRY,
                                 "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewLMFD_ULX.Text,
                            CultureInfo.InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewLMFD_LRX.Text,
                            CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewLMFD_ULX,
                                 "Must be <= the lower right X value.");
                        SetError(txtSecondaryViewLMFD_LRX,
                                 "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewLMFD_ULY.Text,
                            CultureInfo.InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewLMFD_LRY.Text,
                            CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewLMFD_ULY,
                                 "Must be <= the lower right Y value.");
                        SetError(txtSecondaryViewLMFD_LRY,
                                 "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewRMFD_ULX
                                .Text,
                            CultureInfo.InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewRMFD_LRX
                                .Text,
                            CultureInfo.InvariantCulture))
                    {
                        SetError(
                            txtSecondaryViewRMFD_ULX,
                            "Must be <= the lower right X value.");
                        SetError(
                            txtSecondaryViewRMFD_LRX,
                            "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewRMFD_ULY
                                .Text,
                            CultureInfo
                                .InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewRMFD_LRY
                                .Text,
                            CultureInfo
                                .InvariantCulture))
                    {
                        SetError(
                            txtSecondaryViewRMFD_ULY,
                            "Must be <= the lower right Y value.");
                        SetError(
                            txtSecondaryViewRMFD_LRY,
                            "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewHUD_ULX
                                .Text,
                            CultureInfo
                                .InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewHUD_LRX
                                .Text,
                            CultureInfo
                                .InvariantCulture))
                    {
                        SetError(
                            txtSecondaryViewHUD_ULX,
                            "Must be <= the lower right X value.");
                        SetError(
                            txtSecondaryViewHUD_LRX,
                            "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewHUD_ULY
                                .Text,
                            CultureInfo
                                .InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewHUD_LRY
                                .Text,
                            CultureInfo
                                .InvariantCulture))
                    {
                        SetError(
                            txtSecondaryViewHUD_ULY,
                            "Must be <= the lower right Y value.");
                        SetError(
                            txtSecondaryViewHUD_LRY,
                            "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                }
            }
            if (isValid && rdoServer.Checked)
            {
                int serverPortNum = -1;
                if (Int32.TryParse(txtNetworkServerUsePortNum.Text, out serverPortNum))
                {
                    if (serverPortNum < 0 || serverPortNum > 65535)
                    {
                        SetError(txtNetworkServerUsePortNum, "Must be in the range 0 to 65535");
                        isValid = false;
                    }
                }
                else
                {
                    SetError(txtNetworkServerUsePortNum, "Must be in the range 0 to 65535");
                    isValid = false;
                }
            }
            if (isValid && rdoClient.Checked)
            {
                int clientUseServerPortNum = -1;
                if (Int32.TryParse(txtNetworkClientUseServerPortNum.Text, out clientUseServerPortNum))
                {
                    if (clientUseServerPortNum < 0 || clientUseServerPortNum > 65535)
                    {
                        SetError(txtNetworkClientUseServerPortNum, "Must be in the range 0 to 65535");
                        isValid = false;
                    }
                }
                else
                {
                    SetError(txtNetworkClientUseServerPortNum, "Must be in the range 0 to 65535");
                    isValid = false;
                }
            }
            if (isValid && rdoClient.Checked)
            {
                IPAddress ipAddress;
                string serverIpAddress = ipaNetworkClientUseServerIpAddress.Text;
                if (!IPAddress.TryParse(serverIpAddress, out ipAddress))
                {
                    SetError(ipaNetworkClientUseServerIpAddress, "Please enter a valid IP address.");
                    isValid = false;
                }
            }

            if (isValid)
            {
                int pollDelay = -1;
                if (Int32.TryParse(txtPollDelay.Text, out pollDelay))
                {
                    if (pollDelay <= 0)
                    {
                        SetError(txtPollDelay, "Must be an integer > 0.");
                        isValid = false;
                    }
                }
                else
                {
                    SetError(txtPollDelay, "Must be an integer > 0.");
                    isValid = false;
                }
            }
            return isValid;
        }

        /// <summary>
        ///     Applies the current settings to the current Extractor instance, optionally
        ///     saving those settings to disk
        /// </summary>
        /// <param name="persist">
        ///     if <see langword="true" />, the current settings will be persisited to
        ///     the on-disk user-config file.  If <see langword="false" />, the settings will not be
        ///     persisited to disk.  In either case, the current Extractor instance will be
        ///     updated with the new settings.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if validation succeeds, or <see langword="false" />, if
        ///     validation fails due to a user-input error
        /// </returns>
        private bool ApplySettings(bool persist)
        {
            //Validate user settings
            if (!ValidateSettings())
            {
                return false;
            }
            //Commit the current settings to in-memory user-config settings storage (and persist them to 
            //the on-disk user-config file, if we're supposed to do that)
            SaveSettings(persist);
            return true; //if we've got this far, then validation was successful, so return true to indicate that
        }

        private void cmdApply_Click(object sender, EventArgs e)
        {
            ValidateAndApplySettings();
        }

        /// <summary>
        ///     Event handler for the OK button's Click event
        /// </summary>
        /// <param name="sender">the object raising this event</param>
        /// <param name="args">event arguments for the OK button's Click event</param>
        private void cmdOk_Click(object sender, EventArgs args)
        {
            bool valid = ValidateAndApplySettings();
            if (valid)
            {
                CloseThisDialog();
            }
        }

        private void CloseThisDialog()
        {
            try
            {
                Close(); //TODO: handle this kind of stuff centrally
            }
            catch (Exception e)
            {
            }
        }

        private bool ValidateAndApplySettings()
        {
            bool valid = false; //assume all user input is *invalid*
            try
            {
                valid = ApplySettings(true); //try to commit user settings to disk (will perform validation as well)
                if (valid)
                    //if validation succeeds, we can close the form (if not, then the form's ErrorProvider will display errors to the user)
                {
                    if (_extractorRunningStateOnFormOpen)
                    {
                        StopAndRestartExtractor();
                    }
                    else
                    {
						if (_extractor.State.Running)
                        {
                            _extractor.Stop(); //stop the Extractor if it's currently running
                        }
                        _extractor.LoadSettings(); //tell the Extractor to reload its settings (will use the in-
                    }
                }
                else
                {
                    MessageBox.Show(
                        "One or more settings are currently marked as invalid.\n\nYou must correct any settings that are marked as invalid before you can apply changes.",
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
            catch (Exception e) //exceptions will cause the Options form to close
            {
                _log.Error(e.Message, e);
            }
            return valid;
        }

        /// <summary>
        ///     Saves user settings to the in-memory user-config cache (and optionally, to disk as well)
        /// </summary>
        /// <param name="persist">
        ///     if <see langword="true" />, the current settings will be persisited to
        ///     the on-disk user-config file.  If <see langword="false" />, the settings will not be
        ///     persisited to disk.  In either case, the current Extractor instance will be
        ///     updated with the new settings.
        /// </param>
        private void SaveSettings(bool persist)
        {
            Settings settings = Settings.Default;
            settings.UpgradeNeeded = false;
            settings.NetworkImageFormat = cboImageFormat.SelectedItem.ToString();
            settings.CompressionType = cboCompressionType.SelectedItem.ToString();
            if (rdoServer.Checked || rdoStandalone.Checked)
            {
                settings.Primary_MFD4_2D_ULX = Convert.ToInt32(txtPrimaryViewMFD4_ULX.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD4_2D_ULY = Convert.ToInt32(txtPrimaryViewMFD4_ULY.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD4_2D_LRX = Convert.ToInt32(txtPrimaryViewMFD4_LRX.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD4_2D_LRY = Convert.ToInt32(txtPrimaryViewMFD4_LRY.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD3_2D_ULX = Convert.ToInt32(txtPrimaryViewMFD3_ULX.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD3_2D_ULY = Convert.ToInt32(txtPrimaryViewMFD3_ULY.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD3_2D_LRX = Convert.ToInt32(txtPrimaryViewMFD3_LRX.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD3_2D_LRY = Convert.ToInt32(txtPrimaryViewMFD3_LRY.Text, CultureInfo.InvariantCulture);
                settings.Primary_LMFD_2D_ULX = Convert.ToInt32(txtPrimaryViewLMFD_ULX.Text, CultureInfo.InvariantCulture);
                settings.Primary_LMFD_2D_ULY = Convert.ToInt32(txtPrimaryViewLMFD_ULY.Text, CultureInfo.InvariantCulture);
                settings.Primary_LMFD_2D_LRX = Convert.ToInt32(txtPrimaryViewLMFD_LRX.Text, CultureInfo.InvariantCulture);
                settings.Primary_LMFD_2D_LRY = Convert.ToInt32(txtPrimaryViewLMFD_LRY.Text, CultureInfo.InvariantCulture);
                settings.Primary_RMFD_2D_ULX = Convert.ToInt32(txtPrimaryViewRMFD_ULX.Text, CultureInfo.InvariantCulture);
                settings.Primary_RMFD_2D_ULY = Convert.ToInt32(txtPrimaryViewRMFD_ULY.Text, CultureInfo.InvariantCulture);
                settings.Primary_RMFD_2D_LRX = Convert.ToInt32(txtPrimaryViewRMFD_LRX.Text, CultureInfo.InvariantCulture);
                settings.Primary_RMFD_2D_LRY = Convert.ToInt32(txtPrimaryViewRMFD_LRY.Text, CultureInfo.InvariantCulture);
                settings.Primary_HUD_2D_ULX = Convert.ToInt32(txtPrimaryViewHUD_ULX.Text, CultureInfo.InvariantCulture);
                settings.Primary_HUD_2D_ULY = Convert.ToInt32(txtPrimaryViewHUD_ULY.Text, CultureInfo.InvariantCulture);
                settings.Primary_HUD_2D_LRX = Convert.ToInt32(txtPrimaryViewHUD_LRX.Text, CultureInfo.InvariantCulture);
                settings.Primary_HUD_2D_LRY = Convert.ToInt32(txtPrimaryViewHUD_LRY.Text, CultureInfo.InvariantCulture);


                settings.Secondary_MFD4_2D_ULX = Convert.ToInt32(txtSecondaryViewMFD4_ULX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD4_2D_ULY = Convert.ToInt32(txtSecondaryViewMFD4_ULY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD4_2D_LRX = Convert.ToInt32(txtSecondaryViewMFD4_LRX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD4_2D_LRY = Convert.ToInt32(txtSecondaryViewMFD4_LRY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD3_2D_ULX = Convert.ToInt32(txtSecondaryViewMFD3_ULX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD3_2D_ULY = Convert.ToInt32(txtSecondaryViewMFD3_ULY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD3_2D_LRX = Convert.ToInt32(txtSecondaryViewMFD3_LRX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD3_2D_LRY = Convert.ToInt32(txtSecondaryViewMFD3_LRY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_LMFD_2D_ULX = Convert.ToInt32(txtSecondaryViewLMFD_ULX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_LMFD_2D_ULY = Convert.ToInt32(txtSecondaryViewLMFD_ULY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_LMFD_2D_LRX = Convert.ToInt32(txtSecondaryViewLMFD_LRX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_LMFD_2D_LRY = Convert.ToInt32(txtSecondaryViewLMFD_LRY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_RMFD_2D_ULX = Convert.ToInt32(txtSecondaryViewRMFD_ULX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_RMFD_2D_ULY = Convert.ToInt32(txtSecondaryViewRMFD_ULY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_RMFD_2D_LRX = Convert.ToInt32(txtSecondaryViewRMFD_LRX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_RMFD_2D_LRY = Convert.ToInt32(txtSecondaryViewRMFD_LRY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_HUD_2D_ULX = Convert.ToInt32(txtSecondaryViewHUD_ULX.Text,
                                                                CultureInfo.InvariantCulture);
                settings.Secondary_HUD_2D_ULY = Convert.ToInt32(txtSecondaryViewHUD_ULY.Text,
                                                                CultureInfo.InvariantCulture);
                settings.Secondary_HUD_2D_LRX = Convert.ToInt32(txtSecondaryViewHUD_LRX.Text,
                                                                CultureInfo.InvariantCulture);
                settings.Secondary_HUD_2D_LRY = Convert.ToInt32(txtSecondaryViewHUD_LRY.Text,
                                                                CultureInfo.InvariantCulture);
            }
            settings.EnableMfd4Output = chkEnableMFD4.Checked;
            settings.EnableMfd3Output = chkEnableMFD3.Checked;
            settings.EnableLeftMFDOutput = chkEnableLeftMFD.Checked;
            settings.EnableRightMFDOutput = chkEnableRightMFD.Checked;
            settings.EnableHudOutput = chkEnableHud.Checked;

            settings.EnableRWROutput = chkAzimuthIndicator.Checked;
            settings.EnableADIOutput = chkADI.Checked;
            settings.EnableBackupADIOutput = chkStandbyADI.Checked;
            settings.EnableASIOutput = chkAirspeedIndicator.Checked;
            settings.EnableAltimeterOutput = chkAltimeter.Checked;
            settings.EnableAOAIndexerOutput = chkAOAIndexer.Checked;
            settings.EnableAOAIndicatorOutput = chkAOAIndicator.Checked;
            settings.EnableCautionPanelOutput = chkCautionPanel.Checked;
            settings.EnableCMDSOutput = chkCMDSPanel.Checked;
            settings.EnableCompassOutput = chkCompass.Checked;
            settings.EnableDEDOutput = chkDED.Checked;
            settings.EnableAccelerometerOutput = chkAccelerometer.Checked;
            settings.EnableFTIT1Output = chkFTIT1.Checked;
            settings.EnableFTIT2Output = chkFTIT2.Checked;
            settings.EnableNOZ1Output = chkNOZ1.Checked;
            settings.EnableNOZ2Output = chkNOZ2.Checked;
            settings.EnableOIL1Output = chkOIL1.Checked;
            settings.EnableOIL2Output = chkOIL2.Checked;
            settings.EnableRPM1Output = chkRPM1.Checked;
            settings.EnableRPM2Output = chkRPM2.Checked;
            settings.EnableEPUFuelOutput = chkEPU.Checked;
            settings.EnableFuelFlowOutput = chkFuelFlow.Checked;
            settings.EnableISISOutput = chkISIS.Checked;
            settings.EnableFuelQuantityOutput = chkFuelQty.Checked;
            settings.EnableGearLightsOutput = chkGearLights.Checked;
            settings.EnableHSIOutput = chkHSI.Checked;
            settings.EnableEHSIOutput = chkEHSI.Checked;
            settings.EnableNWSIndexerOutput = chkNWSIndexer.Checked;
            settings.EnablePFLOutput = chkPFL.Checked;
            settings.EnableSpeedbrakeOutput = chkSpeedbrake.Checked;
            settings.EnableVVIOutput = chkVVI.Checked;
            settings.EnableHYDAOutput = chkHydA.Checked;
            settings.EnableHYDBOutput = chkHydB.Checked;
            settings.EnableCabinPressOutput = chkCabinPress.Checked;
            settings.EnableRollTrimOutput = chkRollTrim.Checked;
            settings.EnablePitchTrimOutput = chkPitchTrim.Checked;


            if (rdoMillibars.Checked)
            {
                settings.Altimeter_PressureUnits = F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars.ToString();
            }
            else if (rdoInchesOfMercury.Checked)
            {
                settings.Altimeter_PressureUnits =
                    F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury.ToString();
            }

            if (rdoAltimeterStyleDigital.Checked)
            {
                settings.Altimeter_Style = F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electronic.ToString();
            }
            else if (rdoAltimeterStyleElectromechanical.Checked)
            {
                settings.Altimeter_Style =
                    F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electromechanical.ToString();
            }

            if (rdoAzimuthIndicatorStyleScope.Checked)
            {
                if (rdoRWRIP1310BezelType.Checked)
                {
                    settings.AzimuthIndicatorType =
                        F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.IP1310ALR.ToString();
                    settings.AzimuthIndicator_ShowBezel = true;
                }
                else if (rdoRWRHAFBezelType.Checked)
                {
                    settings.AzimuthIndicatorType =
                        F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.HAF.ToString();
                    settings.AzimuthIndicator_ShowBezel = true;
                }
                else if (rdoAzimuthIndicatorNoBezel.Checked)
                {
                    settings.AzimuthIndicatorType =
                        F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.IP1310ALR.ToString();
                    settings.AzimuthIndicator_ShowBezel = false;
                }
            }
            else if (rdoAzimuthIndicatorStyleDigital.Checked)
            {
                settings.AzimuthIndicatorType =
                    F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.AdvancedThreatDisplay.ToString();
            }

            if (rdoVVIStyleNeedle.Checked)
            {
                settings.VVI_Style = VVIStyles.Needle.ToString();
            }
            else if (rdoVVIStyleTape.Checked)
            {
                settings.VVI_Style = VVIStyles.Tape.ToString();
            }

            if (rdoFuelQuantityNeedleCModel.Checked)
            {
                settings.FuelQuantityIndicator_NeedleCModel = true;
            }
            else if (rdoFuelQuantityDModel.Checked)
            {
                settings.FuelQuantityIndicator_NeedleCModel = false;
            }

            settings.StartOnLaunch = chkStartOnLaunch.Checked;
            if (rdoStandalone.Checked || rdoServer.Checked)
            {
                settings.TwoDPrimaryHotkey = sciPrimary2DModeHotkey.Keys;
                settings.TwoDSecondaryHotkey = sciSecondary2DModeHotkey.Keys;
                settings.ThreeDHotkey = sci3DModeHotkey.Keys;
            }
            settings.ThreadPriority =
                (ThreadPriority) Enum.Parse(typeof (ThreadPriority), cboThreadPriority.SelectedItem.ToString());
            settings.PollingDelay = Convert.ToInt32(txtPollDelay.Text, CultureInfo.InvariantCulture);
            settings.LaunchWithWindows = chkStartWithWindows.Checked;

            if (rdoClient.Checked)
            {
                settings.NetworkingMode = (int) NetworkMode.Client;
                settings.ClientUseServerPortNum = Convert.ToInt32(txtNetworkClientUseServerPortNum.Text,
                                                                  CultureInfo.InvariantCulture);
                settings.ClientUseServerIpAddress = ipaNetworkClientUseServerIpAddress.Text;
            }
            else if (rdoServer.Checked)
            {
                settings.NetworkingMode = (int) NetworkMode.Server;
                settings.ServerUsePortNumber = Convert.ToInt32(txtNetworkServerUsePortNum.Text,
                                                               CultureInfo.InvariantCulture);
            }
            else if (rdoStandalone.Checked)
            {
                settings.NetworkingMode = (int) NetworkMode.Standalone;
            }

            SaveGDIPlusSettings();

            Settings.Default.HighlightOutputWindows = chkHighlightOutputWindowsWhenContainMouseCursor.Checked;
            Settings.Default.RenderInstrumentsOnlyOnStatechanges = chkOnlyUpdateImagesWhenDataChanges.Checked;
            if (persist) //persist the user settings to disk
            {
				bool testMode = _extractor.State.TestMode; //store whether we're in test mode
                settings.TestMode = false; //clear test mode flag from the current settings cache (in-memory)
				_extractorRunningStateOnFormOpen = _extractor.State.Running;
                //store current Extractor instance's "isRunning" state
                settings.Save(); //save our new settings to the current settings cache (on-disk)
                settings.TestMode = testMode; //reset the test-mode flag in the current settings cache

                //we have to do the above steps because we don't want the test-mode flag
                //to be persisted to disk, but we do need it in memory to signal the Extractor
            }
            if (persist)
            {
                //update the Windows Registry's Run-at-startup applications list according
                //to the new user settings
                if (chkStartWithWindows.Checked)
                {
                    var c = new Computer();
                    try
                    {
                        using (
                            RegistryKey startupKey =
                                c.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)
                            )
                        {
                            startupKey.SetValue(Application.ProductName, Application.ExecutablePath,
                                                RegistryValueKind.String);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                    }
                }
                else
                {
                    var c = new Computer();
                    try
                    {
                        using (
                            RegistryKey startupKey =
                                c.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)
                            )
                        {
                            startupKey.DeleteValue(Application.ProductName, false);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                    }
                }
            }
        }

        /// <summary>
        ///     Tests a string to see if it contains (only) a valid integer (positive or negative)
        /// </summary>
        /// <param name="coordinateString">
        ///     <see cref="String" /> to test
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the supplied string contains
        ///     a valid integer, or <see langword="false" /> if the supplied string does
        ///     not contain a valid integer.
        /// </returns>
        private static bool xyCoordinateIsValid(string coordinateString)
        {
            int coordinateValue = -1;
            if (Int32.TryParse(coordinateString, out coordinateValue))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Tests a string to see if it contains (only) a valid *positive* integer
        /// </summary>
        /// <param name="coordinateString">
        ///     <see cref="String" /> to test
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the supplied string contains
        ///     a valid positive integer, or <see langword="false" /> if the supplied string does
        ///     not contain a valid positive integer.
        /// </returns>
        private static bool PositiveXyCoordinateIsValid(string coordinateString)
        {
            int coordinateValue = -1;
            if (Int32.TryParse(coordinateString, out coordinateValue))
            {
                if (coordinateValue >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Event handler for the Cancel button's Click event
        /// </summary>
        /// <param name="sender">object raising this event</param>
        /// <param name="e">Event arguments for the Cancel button's Click event</param>
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.Default.Reload(); //re-load the on-disk user settings into the in-memory user config cache
				if (_extractor.State.Running)
                {
                    _extractor.Stop(); //stop the Extractor engine if it's running
                }
                _extractor.LoadSettings(); //tell the Extractor engine to reload its settings
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            Close(); //user has cancelled out of the Options form, so close the form now
        }

        /// <summary>
        ///     Event handler for the "Test Changes" button's Click event
        /// </summary>
        /// <param name="sender">the object raising this event</param>
        /// <param name="e">Event arguments for the Click event</param>
        private void frmOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
			_extractor.State.TestMode = false;
            if (_extractorRunningStateOnFormOpen)
            {
				if (!_extractor.State.Running)
                {
                    _extractor.Start();
                }
            }
            else
            {
				if (_extractor.State.Running)
                {
                    _extractor.Stop();
                }
            }
        }

        /// <summary>
        ///     Event handler for the rdoStandalone control's CheckChanged event
        /// </summary>
        /// <param name="sender">the object raising this event</param>
        /// <param name="e">Event arguments for the rdoStandalone control's CheckChanged event</param>
        private void rdoStandalone_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoStandalone.Checked)
            {
                EnableStandaloneModeOptions();
            }
        }

        /// <summary>
        ///     Event handler for the rdoClient control's CheckChanged event
        /// </summary>
        /// <param name="sender">the object raising this event</param>
        /// <param name="e">Event arguments for the rdoClient control's CheckChanged event</param>
        private void rdoClient_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoClient.Checked)
            {
                EnableClientModeOptions();
            }
        }

        /// <summary>
        ///     Event handler for the rdoServer control's CheckChanged event
        /// </summary>
        /// <param name="sender">the object raising this event</param>
        /// <param name="e">Event arguments for the rdoServer control's CheckChanged event</param>
        private void rdoServer_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoServer.Checked)
            {
                EnableServerModeOptions();
            }
        }

        /// <summary>
        ///     Enables options that are applicable for Server Mode (and disables options that are not
        ///     relevant to this mode)
        /// </summary>
        private void EnableServerModeOptions()
        {
            rdoServer.Checked = true;
            rdoClient.Checked = false;
            rdoStandalone.Checked = false;
            grpServerOptions.Enabled = true;
            grpServerOptions.Visible = true;
            grpServerOptions.BringToFront();
            grpClientOptions.Enabled = false;
            grpClientOptions.Visible = false;
            grpClientOptions.SendToBack();
            grpPrimaryViewMfd4ImageSourceCoordinates.Enabled = true;
            grpPrimaryViewMfd3ImageSourceCoordinates.Enabled = true;
            grpPrimaryViewLeftMfdImageSourceCoordinates.Enabled = true;
            grpPrimaryViewRightMfdImageSourceCoordinates.Enabled = true;
            grpSecondaryViewMfd4ImageSourceCoordinates.Enabled = true;
            grpSecondaryViewMfd3ImageSourceCoordinates.Enabled = true;
            grpSecondaryViewLeftMfdImageSourceCoordinates.Enabled = true;
            grpSecondaryViewRightMfdImageSourceCoordinates.Enabled = true;
            grpPrimaryViewHudImageSourceCoordinates.Enabled = true;
            grpSecondaryViewHudImageSourceCoordinates.Enabled = true;
//            tabHotkeysInner.Enabled = true;
            cmdBMSOptions.Enabled = true;
            errControlErrorProvider.Clear();
        }

        /// <summary>
        ///     Enables options that are applicable for Client Mode (and disables options that are not
        ///     relevant to this mode)
        /// </summary>
        private void EnableClientModeOptions()
        {
            rdoClient.Checked = true;
            rdoStandalone.Checked = false;
            rdoServer.Checked = false;
            grpClientOptions.Enabled = true;
            grpClientOptions.Visible = true;
            grpClientOptions.BringToFront();
            grpServerOptions.Enabled = false;
            grpServerOptions.Visible = false;
            grpServerOptions.SendToBack();
            grpPrimaryViewMfd4ImageSourceCoordinates.Enabled = false;
            grpPrimaryViewMfd3ImageSourceCoordinates.Enabled = false;
            grpPrimaryViewLeftMfdImageSourceCoordinates.Enabled = false;
            grpPrimaryViewRightMfdImageSourceCoordinates.Enabled = false;
            grpSecondaryViewMfd4ImageSourceCoordinates.Enabled = false;
            grpSecondaryViewMfd3ImageSourceCoordinates.Enabled = false;
            grpSecondaryViewLeftMfdImageSourceCoordinates.Enabled = false;
            grpSecondaryViewRightMfdImageSourceCoordinates.Enabled = false;
            grpPrimaryViewHudImageSourceCoordinates.Enabled = false;
            grpSecondaryViewHudImageSourceCoordinates.Enabled = false;
//            tabHotkeysInner.Enabled = false;
            cmdBMSOptions.Enabled = false;
            errControlErrorProvider.Clear();
        }

        /// <summary>
        ///     Enables options that are applicable for Standalone Mode (and disables options that are not
        ///     relevant to this mode)
        /// </summary>
        private void EnableStandaloneModeOptions()
        {
            rdoServer.Checked = false;
            rdoClient.Checked = false;
            rdoStandalone.Checked = true;
            grpServerOptions.Enabled = false;
            grpClientOptions.Enabled = false;
            grpServerOptions.Visible = false;
            grpClientOptions.Visible = false;
            grpPrimaryViewMfd4ImageSourceCoordinates.Enabled = true;
            grpPrimaryViewMfd3ImageSourceCoordinates.Enabled = true;
            grpPrimaryViewLeftMfdImageSourceCoordinates.Enabled = true;
            grpPrimaryViewRightMfdImageSourceCoordinates.Enabled = true;
            grpSecondaryViewMfd4ImageSourceCoordinates.Enabled = true;
            grpSecondaryViewMfd3ImageSourceCoordinates.Enabled = true;
            grpSecondaryViewLeftMfdImageSourceCoordinates.Enabled = true;
            grpSecondaryViewRightMfdImageSourceCoordinates.Enabled = true;
            grpPrimaryViewHudImageSourceCoordinates.Enabled = true;
            grpSecondaryViewHudImageSourceCoordinates.Enabled = true;
//            tabHotkeysInner.Enabled = true;
            cmdBMSOptions.Enabled = true;
            errControlErrorProvider.Clear();
        }

        private void StopAndRestartExtractor()
        {
            if (_formLoading) return;
			if (_extractor.State.Running)
            {
                _extractor.Stop();
                _extractor.LoadSettings();
                _extractor.Start();
            }
        }

        private void chkEnableMFD4_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableMfd4Output = chkEnableMFD4.Checked;
            cmdRecoverMfd4.Enabled = chkEnableMFD4.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkEnableMFD3_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableMfd3Output = chkEnableMFD3.Checked;
            cmdRecoverMfd3.Enabled = chkEnableMFD3.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkEnableLeftMFD_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableLeftMFDOutput = chkEnableLeftMFD.Checked;
            cmdRecoverLeftMfd.Enabled = chkEnableLeftMFD.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkEnableRightMFD_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableRightMFDOutput = chkEnableRightMFD.Checked;
            cmdRecoverRightMfd.Enabled = chkEnableRightMFD.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkEnableHud_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableHudOutput = chkEnableHud.Checked;
            cmdRecoverHud.Enabled = chkEnableHud.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void cmdBMSOptions_Click(object sender, EventArgs e)
        {
            var options = new frmBMSOptions();
            options.BmsPath = GetBmsPath();
            options.ShowDialog(this);
            if (!options.Cancelled)
            {
                string newBmsPath = options.BmsPath;
                if (newBmsPath != null && newBmsPath != string.Empty)
                {
                    Settings.Default.BmsPath = options.BmsPath;
                }
            }
        }

        private string GetBmsPath()
        {
            string bmsPath = Settings.Default.BmsPath;
            if (bmsPath != null) bmsPath = bmsPath.Trim();
            bool exists = false;
            if (!String.IsNullOrEmpty(bmsPath))
            {
                var bmsDirInfo = new DirectoryInfo(bmsPath);
                if (bmsDirInfo.Exists &&
                    ((bmsDirInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory))
                {
                    exists = true;
                }
            }
            if (!exists)
            {
                RegistryKey mainKey = null;
                try
                {
                    mainKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\MicroProse\Falcon\4.0\", false);
                }
                catch (Exception)
                {
                }
                if (mainKey != null)
                {
                    string baseDir = null;
                    try
                    {
                        baseDir = (string) mainKey.GetValue("baseDir");
                    }
                    catch (Exception)
                    {
                    }
                    if (!String.IsNullOrEmpty(baseDir))
                    {
                        var dirInfo = new DirectoryInfo(baseDir);
                        if (dirInfo.Exists)
                        {
                            if ((dirInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                bmsPath = dirInfo.FullName;
                            }
                        }
                    }
                }
            }
            return bmsPath;
        }

        private void cboImageFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCompressionTypeList();
        }

        private void cmdResetToDefaults_Click(object sender, EventArgs e)
        {
            DialogResult result =
                MessageBox.Show(
                    "Warning: This will reset all MFD Extractor options to their defaults.  You will lose any customizations you have made.  Do you want to continue?",
                    "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.OK)
            {
                Settings.Default.Reset();
                Settings.Default.UpgradeNeeded = false;
				bool extractorRunning = _extractor.State.Running;
                if (extractorRunning)
                {
                    _extractor.Stop();
                    _extractor.LoadSettings();
					_extractor.State.TestMode = true;
                }
                if (extractorRunning)
                {
                    _extractor.Start();
                }
                LoadSettings();
            }
        }

        private void cmdImportCoordinates_Click(object sender, EventArgs e)
        {
            var file = new OpenFileDialog();
            file.AddExtension = true;
            file.AutoUpgradeEnabled = true;
            file.CheckFileExists = true;
            file.CheckPathExists = true;
            file.DefaultExt = "mfde";
            file.DereferenceLinks = true;
            file.Filter = "MFD Extractor Coordinates Files|*.mfde";
            file.FilterIndex = 0;
            file.InitialDirectory = Application.ExecutablePath;
            file.Multiselect = false;
            file.ReadOnlyChecked = true;
            file.RestoreDirectory = true;
            file.ShowHelp = false;
            file.ShowReadOnly = false;
            file.SupportMultiDottedExtensions = true;
            file.Title = "Load Coordinates";
            file.ValidateNames = true;
            DialogResult result = file.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                var selectForm = new frmSelectImportExportCoordinates();
                selectForm.Text = "Load Coordinates";
                selectForm.DisableAll();
                //determine which coordinate sets can be loaded
                using (StreamReader sr = File.OpenText(file.FileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string thisLine = sr.ReadLine();
                        if (String.IsNullOrEmpty(thisLine))
                        {
                            continue;
                        }
                        thisLine = thisLine.Trim();
                        if (String.IsNullOrEmpty(thisLine))
                        {
                            continue;
                        }
                        if (thisLine.StartsWith("//") || thisLine.StartsWith("REM") || thisLine.StartsWith("#"))
                        {
                            continue;
                        }

                        List<string> tokens = Util.Tokenize(thisLine);
                        if (tokens.Count == 3)
                        {
                            try
                            {
                                if (tokens[0].StartsWith("Primary_LMFD_2D_ULX"))
                                {
                                    selectForm.EnableSelectLeftMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_LMFD_2D_ULY"))
                                {
                                    selectForm.EnableSelectLeftMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_LMFD_2D_LRX"))
                                {
                                    selectForm.EnableSelectLeftMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_LMFD_2D_LRY"))
                                {
                                    selectForm.EnableSelectLeftMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_RMFD_2D_ULX"))
                                {
                                    selectForm.EnableSelectRightMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_RMFD_2D_ULY"))
                                {
                                    selectForm.EnableSelectRightMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_RMFD_2D_LRX"))
                                {
                                    selectForm.EnableSelectRightMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_RMFD_2D_LRY"))
                                {
                                    selectForm.EnableSelectRightMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD3_2D_ULX"))
                                {
                                    selectForm.EnableSelectMfd3Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD3_2D_ULY"))
                                {
                                    selectForm.EnableSelectMfd3Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD3_2D_LRX"))
                                {
                                    selectForm.EnableSelectMfd3Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD3_2D_LRY"))
                                {
                                    selectForm.EnableSelectMfd3Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD4_2D_ULX"))
                                {
                                    selectForm.EnableSelectMfd4Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD4_2D_ULY"))
                                {
                                    selectForm.EnableSelectMfd4Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD4_2D_LRX"))
                                {
                                    selectForm.EnableSelectMfd4Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD4_2D_LRY"))
                                {
                                    selectForm.EnableSelectMfd4Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_HUD_2D_ULX"))
                                {
                                    selectForm.EnableSelectHudPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_HUD_2D_ULY"))
                                {
                                    selectForm.EnableSelectHudPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_HUD_2D_LRX"))
                                {
                                    selectForm.EnableSelectHudPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_HUD_2D_LRY"))
                                {
                                    selectForm.EnableSelectHudPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_LMFD_2D_ULX"))
                                {
                                    selectForm.EnableSelectLeftMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_LMFD_2D_ULY"))
                                {
                                    selectForm.EnableSelectLeftMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_LMFD_2D_LRX"))
                                {
                                    selectForm.EnableSelectLeftMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_LMFD_2D_LRY"))
                                {
                                    selectForm.EnableSelectLeftMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_RMFD_2D_ULX"))
                                {
                                    selectForm.EnableSelectRightMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_RMFD_2D_ULY"))
                                {
                                    selectForm.EnableSelectRightMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_RMFD_2D_LRX"))
                                {
                                    selectForm.EnableSelectRightMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_RMFD_2D_LRY"))
                                {
                                    selectForm.EnableSelectRightMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD3_2D_ULX"))
                                {
                                    selectForm.EnableSelectMfd3Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD3_2D_ULY"))
                                {
                                    selectForm.EnableSelectMfd3Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD3_2D_LRX"))
                                {
                                    selectForm.EnableSelectMfd3Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD3_2D_LRY"))
                                {
                                    selectForm.EnableSelectMfd3Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD4_2D_ULX"))
                                {
                                    selectForm.EnableSelectMfd4Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD4_2D_ULY"))
                                {
                                    selectForm.EnableSelectMfd4Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD4_2D_LRX"))
                                {
                                    selectForm.EnableSelectMfd4Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD4_2D_LRY"))
                                {
                                    selectForm.EnableSelectMfd4Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_HUD_2D_ULX"))
                                {
                                    selectForm.EnableSelectHudSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_HUD_2D_ULY"))
                                {
                                    selectForm.EnableSelectHudSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_HUD_2D_LRX"))
                                {
                                    selectForm.EnableSelectHudSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_HUD_2D_LRY"))
                                {
                                    selectForm.EnableSelectHudSecondary = true;
                                }
                            }
                            catch (Exception f)
                            {
                            }
                        } //end if (tokens.Count == 2)
                    } //end while (!sr.EndOfStream)
                } //end using (StreamReader sr = File.OpenText(file.FileName))


                //select which coordinate sets to load
                selectForm.SelectAll();
                result = selectForm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    //load the selected coordinate sets from the file
                    using (StreamReader sr = File.OpenText(file.FileName))
                    {
                        while (!sr.EndOfStream)
                        {
                            string thisLine = sr.ReadLine();
                            if (String.IsNullOrEmpty(thisLine))
                            {
                                continue;
                            }
                            thisLine = thisLine.Trim();
                            if (String.IsNullOrEmpty(thisLine))
                            {
                                continue;
                            }
                            if (thisLine.StartsWith("//") || thisLine.StartsWith("REM") || thisLine.StartsWith("#"))
                            {
                                continue;
                            }

                            List<string> tokens = Util.Tokenize(thisLine);
                            if (tokens.Count == 3)
                            {
                                try
                                {
                                    if (selectForm.ExportLeftMfdPrimary)
                                    {
                                        if (tokens[0].StartsWith("Primary_LMFD_2D_ULX"))
                                        {
                                            Settings.Default.Primary_LMFD_2D_ULX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_LMFD_2D_ULY"))
                                        {
                                            Settings.Default.Primary_LMFD_2D_ULY = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_LMFD_2D_LRX"))
                                        {
                                            Settings.Default.Primary_LMFD_2D_LRX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_LMFD_2D_LRY"))
                                        {
                                            Settings.Default.Primary_LMFD_2D_LRY = (int) UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportRightMfdPrimary)
                                    {
                                        if (tokens[0].StartsWith("Primary_RMFD_2D_ULX"))
                                        {
                                            Settings.Default.Primary_RMFD_2D_ULX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_RMFD_2D_ULY"))
                                        {
                                            Settings.Default.Primary_RMFD_2D_ULY = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_RMFD_2D_LRX"))
                                        {
                                            Settings.Default.Primary_RMFD_2D_LRX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_RMFD_2D_LRY"))
                                        {
                                            Settings.Default.Primary_RMFD_2D_LRY = (int) UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportMfd3Primary)
                                    {
                                        if (tokens[0].StartsWith("Primary_MFD3_2D_ULX"))
                                        {
                                            Settings.Default.Primary_MFD3_2D_ULX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD3_2D_ULY"))
                                        {
                                            Settings.Default.Primary_MFD3_2D_ULY = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD3_2D_LRX"))
                                        {
                                            Settings.Default.Primary_MFD3_2D_LRX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD3_2D_LRY"))
                                        {
                                            Settings.Default.Primary_MFD3_2D_LRY = (int) UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportMfd4Primary)
                                    {
                                        if (tokens[0].StartsWith("Primary_MFD4_2D_ULX"))
                                        {
                                            Settings.Default.Primary_MFD4_2D_ULX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD4_2D_ULY"))
                                        {
                                            Settings.Default.Primary_MFD4_2D_ULY = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD4_2D_LRX"))
                                        {
                                            Settings.Default.Primary_MFD4_2D_LRX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD4_2D_LRY"))
                                        {
                                            Settings.Default.Primary_MFD4_2D_LRY = (int) UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportHudPrimary)
                                    {
                                        if (tokens[0].StartsWith("Primary_HUD_2D_ULX"))
                                        {
                                            Settings.Default.Primary_HUD_2D_ULX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_HUD_2D_ULY"))
                                        {
                                            Settings.Default.Primary_HUD_2D_ULY = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_HUD_2D_LRX"))
                                        {
                                            Settings.Default.Primary_HUD_2D_LRX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_HUD_2D_LRY"))
                                        {
                                            Settings.Default.Primary_HUD_2D_LRY = (int) UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportLeftMfdSecondary)
                                    {
                                        if (tokens[0].StartsWith("Secondary_LMFD_2D_ULX"))
                                        {
                                            Settings.Default.Secondary_LMFD_2D_ULX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_LMFD_2D_ULY"))
                                        {
                                            Settings.Default.Secondary_LMFD_2D_ULY = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_LMFD_2D_LRX"))
                                        {
                                            Settings.Default.Secondary_LMFD_2D_LRX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_LMFD_2D_LRY"))
                                        {
                                            Settings.Default.Secondary_LMFD_2D_LRY = (int) UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportRightMfdSecondary)
                                    {
                                        if (tokens[0].StartsWith("Secondary_RMFD_2D_ULX"))
                                        {
                                            Settings.Default.Secondary_RMFD_2D_ULX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_RMFD_2D_ULY"))
                                        {
                                            Settings.Default.Secondary_RMFD_2D_ULY = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_RMFD_2D_LRX"))
                                        {
                                            Settings.Default.Secondary_RMFD_2D_LRX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_RMFD_2D_LRY"))
                                        {
                                            Settings.Default.Secondary_RMFD_2D_LRY = (int) UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportMfd3Secondary)
                                    {
                                        if (tokens[0].StartsWith("Secondary_MFD3_2D_ULX"))
                                        {
                                            Settings.Default.Secondary_MFD3_2D_ULX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD3_2D_ULY"))
                                        {
                                            Settings.Default.Secondary_MFD3_2D_ULY = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD3_2D_LRX"))
                                        {
                                            Settings.Default.Secondary_MFD3_2D_LRX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD3_2D_LRY"))
                                        {
                                            Settings.Default.Secondary_MFD3_2D_LRY = (int) UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportMfd4Secondary)
                                    {
                                        if (tokens[0].StartsWith("Secondary_MFD4_2D_ULX"))
                                        {
                                            Settings.Default.Secondary_MFD4_2D_ULX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD4_2D_ULY"))
                                        {
                                            Settings.Default.Secondary_MFD4_2D_ULY = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD4_2D_LRX"))
                                        {
                                            Settings.Default.Secondary_MFD4_2D_LRX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD4_2D_LRY"))
                                        {
                                            Settings.Default.Secondary_MFD4_2D_LRY = (int) UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportHudSecondary)
                                    {
                                        if (tokens[0].StartsWith("Secondary_HUD_2D_ULX"))
                                        {
                                            Settings.Default.Secondary_HUD_2D_ULX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_HUD_2D_ULY"))
                                        {
                                            Settings.Default.Secondary_HUD_2D_ULY = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_HUD_2D_LRX"))
                                        {
                                            Settings.Default.Secondary_HUD_2D_LRX = (int) UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_HUD_2D_LRY"))
                                        {
                                            Settings.Default.Secondary_HUD_2D_LRY = (int) UInt32.Parse(tokens[2]);
                                        }
                                    }
                                }
                                catch (Exception f)
                                {
                                }
                            } //end if (tokens.Count == 2)
                        } //end while (!sr.EndOfStream)
                    } //end using (StreamReader sr = File.OpenText(file.FileName))
                    LoadSettings();
                } //end if (result == DialogResult.OK)
            } //end if (result == DialogResult.OK)
        }

        private void LoadInstrumentTimings()
        {
            //dgvInstrumentTimings.Rows.Add(n
        }

        private void cmdExportCoordinates_Click(object sender, EventArgs e)
        {
            if (!ValidateSettings())
            {
                MessageBox.Show(
                    "One or more settings are currently marked as invalid.\n\nYou must correct any settings that are marked as invalid before you can save coordinates to a file.",
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            var selectForm = new frmSelectImportExportCoordinates();
            selectForm.Text = "Save Coordinates";
            selectForm.EnableAll();
            selectForm.SelectAll();
            DialogResult result = selectForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                var file = new SaveFileDialog();
                file.CreatePrompt = false;
                file.OverwritePrompt = true;
                file.AddExtension = true;
                file.AutoUpgradeEnabled = true;
                file.CheckFileExists = false;
                file.CheckPathExists = false;
                file.DefaultExt = "mfde";
                file.DereferenceLinks = true;
                file.Filter = "MFD Extractor Coordinates Files|*.mfde";
                file.FilterIndex = 0;
                file.InitialDirectory = Application.ExecutablePath;
                file.RestoreDirectory = true;
                file.ShowHelp = false;
                file.SupportMultiDottedExtensions = true;
                file.Title = "Save Coordinates";
                file.ValidateNames = true;
                result = file.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    try
                    {
                        using (var sw = new StreamWriter(file.FileName))
                        {
                            if (selectForm.ExportLeftMfdPrimary)
                            {
                                sw.WriteLine("Primary_LMFD_2D_ULX" + " = " +
                                             Settings.Default.Primary_LMFD_2D_ULX.ToString());
                                sw.WriteLine("Primary_LMFD_2D_ULY" + " = " +
                                             Settings.Default.Primary_LMFD_2D_ULY.ToString());
                                sw.WriteLine("Primary_LMFD_2D_LRX" + " = " +
                                             Settings.Default.Primary_LMFD_2D_LRX.ToString());
                                sw.WriteLine("Primary_LMFD_2D_LRY" + " = " +
                                             Settings.Default.Primary_LMFD_2D_LRY.ToString());
                            }
                            if (selectForm.ExportRightMfdPrimary)
                            {
                                sw.WriteLine("Primary_RMFD_2D_ULX" + " = " +
                                             Settings.Default.Primary_RMFD_2D_ULX.ToString());
                                sw.WriteLine("Primary_RMFD_2D_ULY" + " = " +
                                             Settings.Default.Primary_RMFD_2D_ULY.ToString());
                                sw.WriteLine("Primary_RMFD_2D_LRX" + " = " +
                                             Settings.Default.Primary_RMFD_2D_LRX.ToString());
                                sw.WriteLine("Primary_RMFD_2D_LRY" + " = " +
                                             Settings.Default.Primary_RMFD_2D_LRY.ToString());
                            }
                            if (selectForm.ExportMfd3Primary)
                            {
                                sw.WriteLine("Primary_MFD3_2D_ULX" + " = " +
                                             Settings.Default.Primary_MFD3_2D_ULX.ToString());
                                sw.WriteLine("Primary_MFD3_2D_ULY" + " = " +
                                             Settings.Default.Primary_MFD3_2D_ULY.ToString());
                                sw.WriteLine("Primary_MFD3_2D_LRX" + " = " +
                                             Settings.Default.Primary_MFD3_2D_LRX.ToString());
                                sw.WriteLine("Primary_MFD3_2D_LRY" + " = " +
                                             Settings.Default.Primary_MFD3_2D_LRY.ToString());
                            }
                            if (selectForm.ExportMfd4Primary)
                            {
                                sw.WriteLine("Primary_MFD4_2D_ULX" + " = " +
                                             Settings.Default.Primary_MFD4_2D_ULX.ToString());
                                sw.WriteLine("Primary_MFD4_2D_ULY" + " = " +
                                             Settings.Default.Primary_MFD4_2D_ULY.ToString());
                                sw.WriteLine("Primary_MFD4_2D_LRX" + " = " +
                                             Settings.Default.Primary_MFD4_2D_LRX.ToString());
                                sw.WriteLine("Primary_MFD4_2D_LRY" + " = " +
                                             Settings.Default.Primary_MFD4_2D_LRY.ToString());
                            }
                            if (selectForm.ExportHudPrimary)
                            {
                                sw.WriteLine("Primary_HUD_2D_ULX" + " = " +
                                             Settings.Default.Primary_HUD_2D_ULX.ToString());
                                sw.WriteLine("Primary_HUD_2D_ULY" + " = " +
                                             Settings.Default.Primary_HUD_2D_ULY.ToString());
                                sw.WriteLine("Primary_HUD_2D_LRX" + " = " +
                                             Settings.Default.Primary_HUD_2D_LRX.ToString());
                                sw.WriteLine("Primary_HUD_2D_LRY" + " = " +
                                             Settings.Default.Primary_HUD_2D_LRY.ToString());
                            }
                            if (selectForm.ExportLeftMfdSecondary)
                            {
                                sw.WriteLine("Secondary_LMFD_2D_ULX" + " = " +
                                             Settings.Default.Secondary_LMFD_2D_ULX.ToString());
                                sw.WriteLine("Secondary_LMFD_2D_ULY" + " = " +
                                             Settings.Default.Secondary_LMFD_2D_ULY.ToString());
                                sw.WriteLine("Secondary_LMFD_2D_LRX" + " = " +
                                             Settings.Default.Secondary_LMFD_2D_LRX.ToString());
                                sw.WriteLine("Secondary_LMFD_2D_LRY" + " = " +
                                             Settings.Default.Secondary_LMFD_2D_LRY.ToString());
                            }
                            if (selectForm.ExportRightMfdSecondary)
                            {
                                sw.WriteLine("Secondary_RMFD_2D_ULX" + " = " +
                                             Settings.Default.Secondary_RMFD_2D_ULX.ToString());
                                sw.WriteLine("Secondary_RMFD_2D_ULY" + " = " +
                                             Settings.Default.Secondary_RMFD_2D_ULY.ToString());
                                sw.WriteLine("Secondary_RMFD_2D_LRX" + " = " +
                                             Settings.Default.Secondary_RMFD_2D_LRX.ToString());
                                sw.WriteLine("Secondary_RMFD_2D_LRY" + " = " +
                                             Settings.Default.Secondary_RMFD_2D_LRY.ToString());
                            }
                            if (selectForm.ExportMfd3Secondary)
                            {
                                sw.WriteLine("Secondary_MFD3_2D_ULX" + " = " +
                                             Settings.Default.Secondary_MFD3_2D_ULX.ToString());
                                sw.WriteLine("Secondary_MFD3_2D_ULY" + " = " +
                                             Settings.Default.Secondary_MFD3_2D_ULY.ToString());
                                sw.WriteLine("Secondary_MFD3_2D_LRX" + " = " +
                                             Settings.Default.Secondary_MFD3_2D_LRX.ToString());
                                sw.WriteLine("Secondary_MFD3_2D_LRY" + " = " +
                                             Settings.Default.Secondary_MFD3_2D_LRY.ToString());
                            }
                            if (selectForm.ExportMfd4Secondary)
                            {
                                sw.WriteLine("Secondary_MFD4_2D_ULX" + " = " +
                                             Settings.Default.Secondary_MFD4_2D_ULX.ToString());
                                sw.WriteLine("Secondary_MFD4_2D_ULY" + " = " +
                                             Settings.Default.Secondary_MFD4_2D_ULY.ToString());
                                sw.WriteLine("Secondary_MFD4_2D_LRX" + " = " +
                                             Settings.Default.Secondary_MFD4_2D_LRX.ToString());
                                sw.WriteLine("Secondary_MFD4_2D_LRY" + " = " +
                                             Settings.Default.Secondary_MFD4_2D_LRY.ToString());
                            }
                            if (selectForm.ExportHudSecondary)
                            {
                                sw.WriteLine("Secondary_HUD_2D_ULX" + " = " +
                                             Settings.Default.Secondary_HUD_2D_ULX.ToString());
                                sw.WriteLine("Secondary_HUD_2D_ULY" + " = " +
                                             Settings.Default.Secondary_HUD_2D_ULY.ToString());
                                sw.WriteLine("Secondary_HUD_2D_LRX" + " = " +
                                             Settings.Default.Secondary_HUD_2D_LRX.ToString());
                                sw.WriteLine("Secondary_HUD_2D_LRY" + " = " +
                                             Settings.Default.Secondary_HUD_2D_LRY.ToString());
                            }
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    catch (Exception f)
                    {
                        MessageBox.Show(f.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error,
                                        MessageBoxDefaultButton.Button1);
                    }
                }
            }
        }

        private void chkAOAIndicator_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableAOAIndicatorOutput = chkAOAIndicator.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkAzimuthIndicator_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableRWROutput = chkAzimuthIndicator.Checked;
            grpAzimuthIndicatorStyle.Enabled = chkAzimuthIndicator.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkADI_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableADIOutput = chkADI.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkAirspeedIndicator_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableASIOutput = chkAirspeedIndicator.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkAltimeter_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableAltimeterOutput = chkAltimeter.Checked;
            grpAltimeterStyle.Enabled = chkAltimeter.Checked;
            //grpPressureAltitudeSettings.Enabled = chkAltimeter.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkAOAIndexer_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableAOAIndexerOutput = chkAOAIndexer.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkCautionPanel_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableCautionPanelOutput = chkCautionPanel.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkCMDSPanel_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableCMDSOutput = chkCMDSPanel.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkCompass_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableCompassOutput = chkCompass.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkDED_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableDEDOutput = chkDED.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkFTIT1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableFTIT1Output = chkFTIT1.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkAccelerometer_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableAccelerometerOutput = chkAccelerometer.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkNOZ1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableNOZ1Output = chkNOZ1.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkOIL1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableOIL1Output = chkOIL1.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkRPM1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableRPM1Output = chkRPM1.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkFTIT2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableFTIT2Output = chkFTIT2.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkNOZ2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableNOZ2Output = chkNOZ2.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkOIL2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableOIL2Output = chkOIL2.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkRPM2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableRPM2Output = chkRPM2.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkEPU_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableEPUFuelOutput = chkEPU.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkFuelFlow_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableFuelFlowOutput = chkFuelFlow.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkISIS_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableISISOutput = chkISIS.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkFuelQty_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableFuelQuantityOutput = chkFuelQty.Checked;
            gbFuelQuantityOptions.Enabled = chkFuelQty.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkGearLights_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableGearLightsOutput = chkGearLights.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkHSI_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableHSIOutput = chkHSI.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkEHSI_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableEHSIOutput = chkEHSI.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkNWSIndexer_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableNWSIndexerOutput = chkNWSIndexer.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkPFL_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnablePFLOutput = chkPFL.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkSpeedbrake_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableSpeedbrakeOutput = chkSpeedbrake.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkStandbyADI_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableBackupADIOutput = chkStandbyADI.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkVVI_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableVVIOutput = chkVVI.Checked;
            grpVVIOptions.Enabled = chkVVI.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkHydA_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableHYDAOutput = chkHydA.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkHydB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableHYDBOutput = chkHydB.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkCabinPress_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableCabinPressOutput = chkCabinPress.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkRollTrim_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableRollTrimOutput = chkRollTrim.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }

        private void chkPitchTrim_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnablePitchTrimOutput = chkPitchTrim.Checked;
            //StopAndRestartExtractor();
            BringToFront();
        }


        private void cmdNV_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.NVISKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.NVISKey = serialized;
            }
        }

        private void rdoAzimuthIndicatorStyleScope_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAzimuthIndicatorStyleScope.Checked)
            {
                grpAzimuthIndicatorDigitalTypes.Visible = false;
                grpAzimuthIndicatorBezelTypes.Visible = true;
                if (!rdoRWRIP1310BezelType.Checked && !rdoRWRHAFBezelType.Checked)
                {
                    rdoRWRIP1310BezelType.Checked = true;
                    Settings.Default.AzimuthIndicator_ShowBezel = false;
                }
                else
                {
                    Settings.Default.AzimuthIndicator_ShowBezel = true;
                }
            }
        }

        private void rdoRWRIP1310BezelType_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoRWRIP1310BezelType.Checked)
            {
                Settings.Default.AzimuthIndicatorType =
                    F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.IP1310ALR.ToString();
                Settings.Default.AzimuthIndicator_ShowBezel = true;
                //StopAndRestartExtractor();
            }
        }

        private void rdoRWRHAFBezelType_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoRWRHAFBezelType.Checked)
            {
                Settings.Default.AzimuthIndicatorType =
                    F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.HAF.ToString();
                Settings.Default.AzimuthIndicator_ShowBezel = true;
                //StopAndRestartExtractor();
            }
        }

        private void rdoAzimuthIndicatorNoBezel_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAzimuthIndicatorNoBezel.Checked)
            {
                Settings.Default.AzimuthIndicatorType =
                    F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.IP1310ALR.ToString();
                Settings.Default.AzimuthIndicator_ShowBezel = false;
                //StopAndRestartExtractor();
            }
        }

        private void rdoAzimuthIndicatorStyleDigital_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAzimuthIndicatorStyleDigital.Checked)
            {
                grpAzimuthIndicatorDigitalTypes.Visible = true;
                grpAzimuthIndicatorBezelTypes.Visible = false;
                if (!rdoATDPlus.Checked)
                {
                    rdoATDPlus.Checked = true;
                }
            }
        }

        private void rdoATDPlus_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoATDPlus.Checked)
            {
                Settings.Default.AzimuthIndicatorType =
                    F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.AdvancedThreatDisplay.ToString();
                //StopAndRestartExtractor();
            }
        }


        private void rdoAltimeterStyleElectromechanical_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAltimeterStyleElectromechanical.Checked)
            {
                Settings.Default.Altimeter_Style =
                    F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electromechanical.ToString();
                //StopAndRestartExtractor();
            }
        }

        private void rdoAltimeterStyleDigital_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAltimeterStyleDigital.Checked)
            {
                Settings.Default.Altimeter_Style =
                    F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electronic.ToString();
                //StopAndRestartExtractor();
            }
        }

        private void rdoInchesOfMercury_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoInchesOfMercury.Checked)
            {
                Settings.Default.Altimeter_PressureUnits =
                    F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury.ToString();
                //StopAndRestartExtractor();
            }
        }

        private void rdoMillibars_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoMillibars.Checked)
            {
                Settings.Default.Altimeter_PressureUnits =
                    F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars.ToString();
                //StopAndRestartExtractor();
            }
        }

        private void rdoVVIStyleTape_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoVVIStyleTape.Checked)
            {
                Settings.Default.VVI_Style = VVIStyles.Tape.ToString();
                //StopAndRestartExtractor();
            }
        }

        private void rdoVVIStyleNeedle_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoVVIStyleNeedle.Checked)
            {
                Settings.Default.VVI_Style = VVIStyles.Needle.ToString();
                //StopAndRestartExtractor();
            }
        }

        private void rdoFuelQuantityNeedleCModel_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoFuelQuantityNeedleCModel.Checked)
            {
                Settings.Default.FuelQuantityIndicator_NeedleCModel = true;
                //StopAndRestartExtractor();
            }
        }

        private void rdoFuelQuantityDModel_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoFuelQuantityDModel.Checked)
            {
                Settings.Default.FuelQuantityIndicator_NeedleCModel = false;
                //StopAndRestartExtractor();
            }
        }


        private void cmdAirspeedIndexIncreaseHotkey_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.AirspeedIndexIncreaseKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.AirspeedIndexIncreaseKey = serialized;
            }
        }

        private void cmdAirspeedIndexDecreaseHotkey_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.AirspeedIndexDecreaseKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.AirspeedIndexDecreaseKey = serialized;
            }
        }

        private void cmdEHSIMenuButtonHotkey_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.EHSIMenuButtonKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.EHSIMenuButtonKey = serialized;
            }
        }

        private void cmdEHSIHeadingIncreaseKey_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.EHSIHeadingIncreaseKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.EHSIHeadingIncreaseKey = serialized;
            }
        }

        private void cmdEHSIHeadingDecreaseKey_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.EHSIHeadingDecreaseKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.EHSIHeadingDecreaseKey = serialized;
            }
        }

        private void cmdEHSICourseIncreaseKey_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.EHSICourseIncreaseKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.EHSICourseIncreaseKey = serialized;
            }
        }

        private void cmdEHSICourseDecreaseKey_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.EHSICourseDecreaseKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.EHSICourseDecreaseKey = serialized;
            }
        }

        private void cmdEHSICourseKnobDepressedKey_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.EHSICourseKnobDepressedKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.EHSICourseKnobDepressedKey = serialized;
            }
        }

        private void cmdAzimuthIndicatorBrightnessIncrease_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.AzimuthIndicatorBrightnessIncreaseKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.AzimuthIndicatorBrightnessIncreaseKey = serialized;
            }
        }

        private void cmdAzimuthIndicatorBrightnessDecrease_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.AzimuthIndicatorBrightnessDecreaseKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.AzimuthIndicatorBrightnessDecreaseKey = serialized;
            }
        }

        private void cmdISISBrightButtonPressed_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.ISISBrightButtonKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.ISISBrightButtonKey = serialized;
            }
        }

        private void cmdISISStandardBrightnessButtonPressed_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.ISISStandardButtonKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.ISISStandardButtonKey = serialized;
            }
        }

        private void cmdAccelerometerResetButtonPressed_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = _extractor.Mediator;
            string keyFromSettingsString = Settings.Default.AccelerometerResetKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch (Exception ex)
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            InputControlSelection selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.AccelerometerResetKey = serialized;
            }
        }

        private void chkHighlightOutputWindowsWhenContainMouseCursor_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.HighlightOutputWindows = chkHighlightOutputWindowsWhenContainMouseCursor.Checked;
        }

        private void chkOnlyUpdateImagesWhenDataChanges_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.RenderInstrumentsOnlyOnStatechanges = chkOnlyUpdateImagesWhenDataChanges.Checked;
        }
    }
}