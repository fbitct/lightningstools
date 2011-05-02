using System;
using System.Windows.Forms;
using log4net;
using MFDExtractor.Runtime.Settings;
using MFDExtractor.UI;

namespace MFDExtractor.Runtime
{
    internal class FormManager
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (Extractor));

        /// <summary>
        /// Reference to the application's main form (for supplying to DirectInput)
        /// </summary>
        private static Form _applicationForm;

        private readonly InstrumentRenderers _renderers;
        private readonly SettingsManager _settingsManager;

        public FormManager()
        {
        }

        public FormManager(SettingsManager settingsManager, InstrumentRenderers renderers)
        {
            _settingsManager = settingsManager;
            _renderers = renderers;
        }


        /// <summary>
        /// Gets/sets a reference to the application's main form (if there is one) -- required for DirectInput event notifications
        /// </summary>
        public static Form ApplicationForm
        {
            get { return _applicationForm; }
            set { _applicationForm = value; }
        }

        #region Forms Setup

        public void SetupOutputForms()
        {
            var mainForm = _applicationForm;
            CreateMFD4Form(mainForm);
            CreateMFD3Form(mainForm);
            CreateLMFDForm(mainForm);
            CreateRMFDForm(mainForm);
            CreateHUDForm(mainForm);
            CreateNWSIndexerForm(mainForm);
            CreateAOAIndexerForm(mainForm);
            CreateAOAIndicatorForm(mainForm);
            CreateVVIForm(mainForm);
            CreateADIForm(mainForm);
            CreateStandbyADIForm(mainForm);
            CreateASIForm(mainForm);
            CreateAltimeterForm(mainForm);
            CreateCautionPanelForm(mainForm);
            CreateCMDSForm(mainForm);
            CreateCompassForm(mainForm);
            CreateDEDForm(mainForm);
            CreatePFLForm(mainForm);
            CreateEPUFuelForm(mainForm);
            CreateAccelerometerForm(mainForm);
            CreateFTIT1Form(mainForm);
            CreateFTIT2Form(mainForm);
            CreateFuelFlowForm(mainForm);
            CreateISISForm(mainForm);
            CreateFuelQuantityForm(mainForm);
            CreateHSIForm(mainForm);
            CreateEHSIForm(mainForm);
            CreateGearLightsForm(mainForm);
            CreateNOZ1Form(mainForm);
            CreateNOZ2Form(mainForm);
            CreateOIL1Form(mainForm);
            CreateOIL2Form(mainForm);
            CreateRWRForm(mainForm);
            CreateSpeedbrakeForm(mainForm);
            CreateRPM1Form(mainForm);
            CreateRPM2Form(mainForm);
            CreateHYDAForm(mainForm);
            CreateHYDBForm(mainForm);
            CreateCabinPressForm(mainForm);
            CreateRollTrimForm(mainForm);
            CreatePitchTrimForm(mainForm);
        }

        private void CreatePitchTrimForm(Form mainForm)
        {
            InstrumentFormController.Create("PitchTrim", "Pitch Trim Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.PitchTrimRenderer);
        }

        private void CreateRollTrimForm(Form mainForm)
        {
            InstrumentFormController.Create("RollTrim", "Roll Trim Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.RollTrimRenderer);
        }

        private void CreateCabinPressForm(Form mainForm)
        {
            InstrumentFormController.Create("CabinPress", "Cabin Pressure Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.CabinPressRenderer);
        }

        private void CreateHYDBForm(Form mainForm)
        {
            InstrumentFormController.Create("HYDB", "HYD B", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.HYDBRenderer);
        }

        private void CreateHYDAForm(Form mainForm)
        {
            InstrumentFormController.Create("HYDA", "HYD A", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.HYDARenderer);
        }

        private void CreateRPM2Form(Form mainForm)
        {
            InstrumentFormController.Create("RPM2", "Engine 2 - Tachometer", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.RPM2Renderer);
        }

        private void CreateRPM1Form(Form mainForm)
        {
            InstrumentFormController.Create("RPM1", "Engine 1 - Tachometer", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.RPM1Renderer);
        }

        private void CreateSpeedbrakeForm(Form mainForm)
        {
            InstrumentFormController.Create("Speedbrake", "Speedbrake", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.SpeedbrakeRenderer);
        }

        private void CreateRWRForm(Form mainForm)
        {
            InstrumentFormController.Create("RWR", "RWR", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.RWRRenderer);
        }

        private void CreateOIL2Form(Form mainForm)
        {
            InstrumentFormController.Create("OIL2", "Engine 2 - Oil Pressure Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.OIL2Renderer);
        }

        private void CreateOIL1Form(Form mainForm)
        {
            InstrumentFormController.Create("OIL1", "Engine 1 - Oil Pressure Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.OIL1Renderer);
        }

        private void CreateNOZ2Form(Form mainForm)
        {
            InstrumentFormController.Create("NOZ2", "Engine 2 - Nozzle Position Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.NOZ2Renderer);
        }

        private void CreateNOZ1Form(Form mainForm)
        {
            InstrumentFormController.Create("NOZ1", "Engine 1 - Nozzle Position Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.NOZ1Renderer);
        }

        private void CreateGearLightsForm(Form mainForm)
        {
            InstrumentFormController.Create("GearLights", "Gear Lights", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.LandingGearLightsRenderer);
        }

        private void CreateEHSIForm(Form mainForm)
        {
            InstrumentFormController.Create("EHSI", "EHSI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.EHSIRenderer);
        }

        private void CreateHSIForm(Form mainForm)
        {
            InstrumentFormController.Create("HSI", "HSI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.HSIRenderer);
        }

        private void CreateFuelQuantityForm(Form mainForm)
        {
            InstrumentFormController.Create("FuelQuantity", "Fuel Quantity", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.FuelQuantityRenderer);
        }

        private void CreateISISForm(Form mainForm)
        {
            InstrumentFormController.Create("ISIS", "ISIS", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.ISISRenderer);
        }

        private void CreateFuelFlowForm(Form mainForm)
        {
            InstrumentFormController.Create("FuelFlow", "Fuel Flow", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.FuelFlowRenderer);
        }

        private void CreateFTIT2Form(Form mainForm)
        {
            InstrumentFormController.Create("FTIT2", "Engine 2 FTIT", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.FTIT2Renderer);
        }

        private void CreateFTIT1Form(Form mainForm)
        {
            InstrumentFormController.Create("FTIT1", "Engine 1 FTIT", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.FTIT1Renderer);
        }

        private void CreateAccelerometerForm(Form mainForm)
        {
            InstrumentFormController.Create("Accelerometer", "Accelerometer", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.AccelerometerRenderer);
        }

        private void CreateEPUFuelForm(Form mainForm)
        {
            InstrumentFormController.Create("EPUFuel", "EPU Fuel", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.EPUFuelRenderer);
        }

        private void CreatePFLForm(Form mainForm)
        {
            InstrumentFormController.Create("PFL", "PFL", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.PFLRenderer);
        }

        private void CreateDEDForm(Form mainForm)
        {
            InstrumentFormController.Create("DED", "DED", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.DEDRenderer);
        }

        private void CreateCompassForm(Form mainForm)
        {
            InstrumentFormController.Create("Compass", "Compass", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.CompassRenderer);
        }

        private void CreateCMDSForm(Form mainForm)
        {
            InstrumentFormController.Create("CMDS", "CMDS", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.CMDSPanelRenderer);
        }

        private void CreateCautionPanelForm(Form mainForm)
        {
            InstrumentFormController.Create("CautionPanel", "Caution Panel", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.CautionPanelRenderer);
        }

        private void CreateAltimeterForm(Form mainForm)
        {
            InstrumentFormController.Create("Altimeter", "Altimeter", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.AltimeterRenderer);
        }

        private void CreateASIForm(Form mainForm)
        {
            InstrumentFormController.Create("ASI", "ASI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.ASIRenderer);
        }

        private void CreateStandbyADIForm(Form mainForm)
        {
            InstrumentFormController.Create("StandbyADI", "StandbyADI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.StandbyADIRenderer);
        }

        private void CreateADIForm(Form mainForm)
        {
            InstrumentFormController.Create("ADI", "ADI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.ADIRenderer);
        }

        private void CreateVVIForm(Form mainForm)
        {
            InstrumentFormController.Create("VVI", "VVI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.VVIRenderer);
        }

        private void CreateAOAIndicatorForm(Form mainForm)
        {
            InstrumentFormController.Create("AOAIndicator", "AOA Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.AOAIndicatorRenderer);
        }

        private void CreateAOAIndexerForm(Form mainForm)
        {
            InstrumentFormController.Create("AOAIndexer", "AOA Indexer", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.AOAIndexerRenderer);
        }

        private void CreateNWSIndexerForm(Form mainForm)
        {
            InstrumentFormController.Create("NWSIndexer", "NWS Indexer", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.NWSIndexerRenderer);
        }

        private void CreateHUDForm(Form mainForm)
        {
            InstrumentFormController.Create("HUD", "HUD", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.HUDRenderer);
        }

        private void CreateRMFDForm(Form mainForm)
        {
            InstrumentFormController.Create("RMFD", "Right MFD", mainForm, BlankAndTestImages.RightMfdBlankImage,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.RMFDRenderer);
        }

        private void CreateLMFDForm(Form mainForm)
        {
            InstrumentFormController.Create("LMFD", "Left MFD", mainForm, BlankAndTestImages.LeftMfdBlankImage,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.LMFDRenderer);
        }

        private void CreateMFD3Form(Form mainForm)
        {
            InstrumentFormController.Create("MFD3", "MFD #3", mainForm, BlankAndTestImages.Mfd3BlankImage,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.MFD3Renderer);
        }

        private void CreateMFD4Form(Form mainForm)
        {
            InstrumentFormController.Create("MFD4", "MFD #4", mainForm, BlankAndTestImages.Mfd4BlankImage,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.MFD4Renderer);
        }

        #endregion

        #region Form Teardown

        private void CloseOutputWindowForm(Form form)
        {
            if (form != null)
            {
                try
                {
                    form.Close();
                }
                catch (InvalidOperationException e)
                {
                    _log.Error(e.Message, e);
                }
            }
        }

        #endregion
    }
}