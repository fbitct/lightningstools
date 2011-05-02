﻿using System;
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
            InstrumentFormController.Create("MFD4", "MFD #4", mainForm, BlankAndTestImages.Mfd4BlankImage,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.MFD4Renderer);
            InstrumentFormController.Create("MFD3", "MFD #3", mainForm, BlankAndTestImages.Mfd3BlankImage,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.MFD3Renderer);
            InstrumentFormController.Create("LMFD", "Left MFD", mainForm, BlankAndTestImages.LeftMfdBlankImage,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.LMFDRenderer);
            InstrumentFormController.Create("RMFD", "Right MFD", mainForm, BlankAndTestImages.RightMfdBlankImage,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.RMFDRenderer);
            InstrumentFormController.Create("HUD", "HUD", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.HUDRenderer);
            InstrumentFormController.Create("NWSIndexer", "NWS Indexer", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.NWSIndexerRenderer);
            InstrumentFormController.Create("AOAIndexer", "AOA Indexer", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.AOAIndexerRenderer);
            InstrumentFormController.Create("AOAIndicator", "AOA Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.AOAIndicatorRenderer);
            InstrumentFormController.Create("VVI", "VVI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.VVIRenderer);
            InstrumentFormController.Create("ADI", "ADI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.ADIRenderer);
            InstrumentFormController.Create("StandbyADI", "StandbyADI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.StandbyADIRenderer);
            InstrumentFormController.Create("ASI", "ASI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.ASIRenderer);
            InstrumentFormController.Create("Altimeter", "Altimeter", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.AltimeterRenderer);
            InstrumentFormController.Create("CautionPanel", "Caution Panel", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.CautionPanelRenderer);
            InstrumentFormController.Create("CMDS", "CMDS", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.CMDSPanelRenderer);
            InstrumentFormController.Create("Compass", "Compass", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.CompassRenderer);
            InstrumentFormController.Create("DED", "DED", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.DEDRenderer);
            InstrumentFormController.Create("PFL", "PFL", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.PFLRenderer);
            InstrumentFormController.Create("EPUFuel", "EPU Fuel", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.EPUFuelRenderer);
            InstrumentFormController.Create("Accelerometer", "Accelerometer", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.AccelerometerRenderer);
            InstrumentFormController.Create("FTIT1", "Engine 1 FTIT", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.FTIT1Renderer);
            InstrumentFormController.Create("FTIT2", "Engine 2 FTIT", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.FTIT2Renderer);
            InstrumentFormController.Create("FuelFlow", "Fuel Flow", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.FuelFlowRenderer);
            InstrumentFormController.Create("ISIS", "ISIS", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.ISISRenderer);
            InstrumentFormController.Create("FuelQuantity", "Fuel Quantity", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.FuelQuantityRenderer);
            InstrumentFormController.Create("HSI", "HSI", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.HSIRenderer);
            InstrumentFormController.Create("EHSI", "EHSI", mainForm, null,
                                            (s, e) => { _settingsManager.SaveSettingsAsync(); },
                                            Properties.Settings.Default, _renderers.EHSIRenderer);
            InstrumentFormController.Create("GearLights", "Gear Lights", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.LandingGearLightsRenderer);
            InstrumentFormController.Create("NOZ1", "Engine 1 - Nozzle Position Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.NOZ1Renderer);
            InstrumentFormController.Create("NOZ2", "Engine 2 - Nozzle Position Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.NOZ2Renderer);
            InstrumentFormController.Create("OIL1", "Engine 1 - Oil Pressure Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.OIL1Renderer);
            InstrumentFormController.Create("OIL2", "Engine 2 - Oil Pressure Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.OIL2Renderer);
            InstrumentFormController.Create("RWR", "RWR", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.RWRRenderer);
            InstrumentFormController.Create("Speedbrake", "Speedbrake", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.SpeedbrakeRenderer);
            InstrumentFormController.Create("RPM1", "Engine 1 - Tachometer", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.RPM1Renderer);
            InstrumentFormController.Create("RPM2", "Engine 2 - Tachometer", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.RPM2Renderer);
            InstrumentFormController.Create("HYDA", "HYD A", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.HYDARenderer);
            InstrumentFormController.Create("HYDB", "HYD B", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.HYDBRenderer);
            InstrumentFormController.Create("CabinPress", "Cabin Pressure Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.CabinPressRenderer);
            InstrumentFormController.Create("RollTrim", "Roll Trim Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.RollTrimRenderer);
            InstrumentFormController.Create("PitchTrim", "Pitch Trim Indicator", mainForm, null,
                                            (s, e) => _settingsManager.SaveSettingsAsync(),
                                            Properties.Settings.Default, _renderers.PitchTrimRenderer);

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