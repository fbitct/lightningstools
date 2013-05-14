using System;
using Common.UI;
using LightningGauges.Renderers;
using MFDExtractor.Properties;
using MFDExtractor.UI;

namespace MFDExtractor
{
    internal interface IRendererSetInitializer
    {
        void Initialize(GDIPlusOptions gdiPlusOptions);
    }

    class RendererSetInitializer : IRendererSetInitializer
    {
        private readonly IInstrumentRendererSet _renderers;
        public RendererSetInitializer(IInstrumentRendererSet renderers)
        {
            _renderers = renderers ?? new InstrumentRendererSet();
        }
        public void Initialize(GDIPlusOptions gdiPlusOptions)
        {
            SetupADIRenderer();
            SetupBackupADIRenderer();
            SetupASIRenderer();
            SetupAltimeterRenderer();
            SetupAOAIndexerRenderer();
            SetupAOAIndicatorRenderer();
            SetupCautionPanelRenderer();
            SetupCMDSPanelRenderer();
            SetupCompassRenderer();
            SetupDEDRenderer();
            SetupPFLRenderer();
            SetupEPUFuelRenderer();
            SetupAccelerometerRenderer();
            SetupFTIT1Renderer();
            SetupFTIT2Renderer();
            SetupFuelFlowRenderer();
            SetupISISRenderer(gdiPlusOptions);
            SetupFuelQuantityRenderer();
            SetupHSIRenderer();
            SetupEHSIRenderer(gdiPlusOptions);
            SetupLandingGearLightsRenderer();
            SetupNWSIndexerRenderer();
            SetupNOZ1Renderer();
            SetupNOZ2Renderer();
            SetupOil1Renderer();
            SetupOil2Renderer();
            SetupRWRRenderer(gdiPlusOptions);
            SetupSpeedbrakeRenderer();
            SetupRPM1Renderer();
            SetupRPM2Renderer();
            SetupVVIRenderer();
            SetupHydARenderer();
            SetupHydBRenderer();
            SetupCabinPressRenderer();
            SetupRollTrimRenderer();
            SetupPitchTrimRenderer();
        }

        private void SetupVVIRenderer()
        {
            var vviStyleString = Settings.Default.VVI_Style;
            var vviStyle = (VVIStyles) Enum.Parse(typeof (VVIStyles), vviStyleString);
            switch (vviStyle)
            {
                case VVIStyles.Tape:
                    _renderers.VVI = new F16VerticalVelocityIndicatorUSA();
                    break;
                case VVIStyles.Needle:
                    _renderers.VVI = new F16VerticalVelocityIndicatorEU();
                    break;
            }
        }

        private void SetupRPM2Renderer()
        {
            _renderers.RPM2 = new F16Tachometer();
            ((F16Tachometer)_renderers.RPM2).Options.IsSecondary = true;
        }

        private void SetupRPM1Renderer()
        {
            _renderers.RPM1 = new F16Tachometer();
            ((F16Tachometer)_renderers.RPM1).Options.IsSecondary = false;
        }

        private void SetupSpeedbrakeRenderer()
        {
            _renderers.Speedbrake = new F16SpeedbrakeIndicator();
        }

        private void SetupRWRRenderer(GDIPlusOptions gdiPlusOptions)
        {
            _renderers.RWR = new F16AzimuthIndicator();
            var styleString = Settings.Default.AzimuthIndicatorType;
            var style = (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle) 
                Enum.Parse(typeof (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle), styleString);
            ((F16AzimuthIndicator)_renderers.RWR).Options.Style = style;
            ((F16AzimuthIndicator)_renderers.RWR).Options.HideBezel = !Settings.Default.AzimuthIndicator_ShowBezel;
            ((F16AzimuthIndicator)_renderers.RWR).Options.GDIPlusOptions = gdiPlusOptions;
        }

        private void SetupOil2Renderer()
        {
            _renderers.OIL2 = new F16OilPressureGauge();
            ((F16OilPressureGauge)_renderers.OIL2).Options.IsSecondary = true;
        }

        private void SetupOil1Renderer()
        {
            _renderers.OIL1 = new F16OilPressureGauge();
            ((F16OilPressureGauge)_renderers.OIL1).Options.IsSecondary = false;
        }

        private void SetupNOZ2Renderer()
        {
            _renderers.NOZ2 = new F16NozzlePositionIndicator();
            ((F16NozzlePositionIndicator)_renderers.NOZ2).Options.IsSecondary = true;
        }

        private void SetupNOZ1Renderer()
        {
            _renderers.NOZ1 = new F16NozzlePositionIndicator();
            ((F16NozzlePositionIndicator)_renderers.NOZ1).Options.IsSecondary = false;
        }

        private void SetupNWSIndexerRenderer()
        {
            _renderers.NWSIndexer = new F16NosewheelSteeringIndexer();
        }

        private void SetupLandingGearLightsRenderer()
        {
            _renderers.LandingGearLights = new F16LandingGearWheelsLights();
        }

        private void SetupHSIRenderer()
        {
            _renderers.HSI = new F16HorizontalSituationIndicator();
        }

        private void SetupEHSIRenderer(GDIPlusOptions gdiPlusOptions)
        {
            _renderers.EHSI = new F16EHSI();
            ((F16EHSI)_renderers.EHSI).Options.GDIPlusOptions = gdiPlusOptions;
        }

        private void SetupFuelQuantityRenderer()
        {
            _renderers.FuelQuantity = new F16FuelQuantityIndicator();
            if (Settings.Default.FuelQuantityIndicator_NeedleCModel)
            {
                ((F16FuelQuantityIndicator)_renderers.FuelQuantity).Options.NeedleType =
                    F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.CModel;
            }
            else
            {
                ((F16FuelQuantityIndicator)_renderers.FuelQuantity).Options.NeedleType =
                    F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.DModel;
            }
        }

        private void SetupFuelFlowRenderer()
        {
            _renderers.FuelFlow = new F16FuelFlow();
        }

        private void SetupISISRenderer(GDIPlusOptions gdiPlusOptions)
        {
            _renderers.ISIS = new F16ISIS();
            var pressureUnitsString = Settings.Default.ISIS_PressureUnits;
            if (!string.IsNullOrEmpty(pressureUnitsString))
            {
                try
                {
                    ((F16ISIS)_renderers.ISIS).Options.PressureAltitudeUnits =
                        (F16ISIS.F16ISISOptions.PressureUnits)
                        Enum.Parse(typeof (F16ISIS.F16ISISOptions.PressureUnits), pressureUnitsString);
                }
                catch (Exception )
                {
                    ((F16ISIS)_renderers.ISIS).Options.PressureAltitudeUnits =
                        F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury;
                }
            }
            ((F16ISIS)_renderers.ISIS).Options.GDIPlusOptions = gdiPlusOptions;
        }

        private void SetupAccelerometerRenderer()
        {
            _renderers.Accelerometer = new F16Accelerometer();
        }

        private void SetupFTIT2Renderer()
        {
            _renderers.FTIT2 = new F16FanTurbineInletTemperature();
            ((F16FanTurbineInletTemperature)_renderers.FTIT2).Options.IsSecondary = true;
        }

        private void SetupFTIT1Renderer()
        {
            _renderers.FTIT1 = new F16FanTurbineInletTemperature();
            ((F16FanTurbineInletTemperature)_renderers.FTIT1).Options.IsSecondary = false;
        }

        private void SetupEPUFuelRenderer()
        {
            _renderers.EPUFuel = new F16EPUFuelGauge();
        }

        private void SetupPFLRenderer()
        {
            _renderers.PFL = new F16DataEntryDisplayPilotFaultList();
        }

        private void SetupDEDRenderer()
        {
            _renderers.DED = new F16DataEntryDisplayPilotFaultList();
        }

        private void SetupCompassRenderer()
        {
            _renderers.Compass = new F16Compass();
        }

        private void SetupCMDSPanelRenderer()
        {
            _renderers.CMDSPanel = new F16CMDSPanel();
        }

        private void SetupCautionPanelRenderer()
        {
            _renderers.CautionPanel = new F16CautionPanel();
        }

        private void SetupAOAIndicatorRenderer()
        {
            _renderers.AOAIndicator = new F16AngleOfAttackIndicator();
        }

        private void SetupAOAIndexerRenderer()
        {
            _renderers.AOAIndexer = new F16AngleOfAttackIndexer();
        }

        private void SetupAltimeterRenderer()
        {
            _renderers.Altimeter = new F16Altimeter();

            var altimeterSyleString = Settings.Default.Altimeter_Style;
            var altimeterStyle = (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle) 
                Enum.Parse(typeof (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle), altimeterSyleString);
            ((F16Altimeter)_renderers.Altimeter).Options.Style = altimeterStyle;

            var pressureUnitsString = Settings.Default.Altimeter_PressureUnits;
            var pressureUnits = (F16Altimeter.F16AltimeterOptions.PressureUnits) Enum.Parse(typeof (F16Altimeter.F16AltimeterOptions.PressureUnits), pressureUnitsString);
            ((F16Altimeter)_renderers.Altimeter).Options.PressureAltitudeUnits = pressureUnits;
        }

        private void SetupASIRenderer()
        {
            _renderers.ASI = new F16AirspeedIndicator();
        }

        private void SetupADIRenderer()
        {
            _renderers.ADI = new F16ADI();
        }

        private void SetupBackupADIRenderer()
        {
            _renderers.BackupADI = new F16StandbyADI();
        }

        private void SetupHydARenderer()
        {
            _renderers.HYDA = new F16HydraulicPressureGauge();
        }

        private void SetupHydBRenderer()
        {
            _renderers.HYDB = new F16HydraulicPressureGauge();
        }

        private void SetupCabinPressRenderer()
        {
            _renderers.CabinPress = new F16CabinPressureAltitudeIndicator();
        }

        private void SetupRollTrimRenderer()
        {
            _renderers.RollTrim = new F16RollTrimIndicator();
        }

        private void SetupPitchTrimRenderer()
        {
            _renderers.PitchTrim = new F16PitchTrimIndicator();
        }
        
    }
}
