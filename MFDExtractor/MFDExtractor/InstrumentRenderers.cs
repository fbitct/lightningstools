using System;
using Common.Generic;
using Common.SimSupport;
using Common.UI;
using LightningGauges.Renderers;
using MFDExtractor.UI;

namespace MFDExtractor
{
    public class InstrumentRenderers
    {
        private InstrumentRenderers()
        {
        }

        public InstrumentRenderers(InitializationParams parms) : this()
        {
            Initialize(parms);
        }

        public IInstrumentRenderer ADIRenderer { get; set; }
        public IInstrumentRenderer StandbyADIRenderer { get; set; }
        public IInstrumentRenderer ASIRenderer { get; set; }
        public IInstrumentRenderer AltimeterRenderer { get; set; }
        public IInstrumentRenderer AOAIndexerRenderer { get; set; }
        public IInstrumentRenderer AOAIndicatorRenderer { get; set; }
        public IInstrumentRenderer CautionPanelRenderer { get; set; }
        public IInstrumentRenderer CMDSPanelRenderer { get; set; }
        public IInstrumentRenderer CompassRenderer { get; set; }
        public IInstrumentRenderer DEDRenderer { get; set; }
        public IInstrumentRenderer PFLRenderer { get; set; }
        public IInstrumentRenderer EPUFuelRenderer { get; set; }
        public IInstrumentRenderer AccelerometerRenderer { get; set; }
        public IInstrumentRenderer FTIT1Renderer { get; set; }
        public IInstrumentRenderer FTIT2Renderer { get; set; }
        public IInstrumentRenderer FuelFlowRenderer { get; set; }
        public IInstrumentRenderer ISISRenderer { get; set; }
        public IInstrumentRenderer FuelQuantityRenderer { get; set; }
        public IInstrumentRenderer HSIRenderer { get; set; }
        public IInstrumentRenderer EHSIRenderer { get; set; }
        public IInstrumentRenderer LandingGearLightsRenderer { get; set; }
        public IInstrumentRenderer NWSIndexerRenderer { get; set; }
        public IInstrumentRenderer NOZ1Renderer { get; set; }
        public IInstrumentRenderer NOZ2Renderer { get; set; }
        public IInstrumentRenderer OIL1Renderer { get; set; }
        public IInstrumentRenderer OIL2Renderer { get; set; }
        public IInstrumentRenderer RWRRenderer { get; set; }
        public IInstrumentRenderer SpeedbrakeRenderer { get; set; }
        public IInstrumentRenderer RPM1Renderer { get; set; }
        public IInstrumentRenderer RPM2Renderer { get; set; }
        public IInstrumentRenderer VVIRenderer { get; set; }
        public IInstrumentRenderer HYDARenderer { get; set; }
        public IInstrumentRenderer HYDBRenderer { get; set; }
        public IInstrumentRenderer CabinPressRenderer { get; set; }
        public IInstrumentRenderer RollTrimRenderer { get; set; }
        public IInstrumentRenderer PitchTrimRenderer { get; set; }
        public IInstrumentRenderer LMFDRenderer { get; set; }
        public IInstrumentRenderer RMFDRenderer { get; set; }
        public IInstrumentRenderer MFD3Renderer { get; set; }
        public IInstrumentRenderer MFD4Renderer { get; set; }
        public IInstrumentRenderer HUDRenderer { get; set; }

        private void Initialize(InitializationParams parms)
        {
            SetupVVIRenderer(parms.VVIStyleProperty);
            SetupRPM2Renderer();
            SetupRPM1Renderer();
            SetupSpeedbrakeRenderer();
            SetupRWRRenderer(parms.AzimuthIndicatorTypeProperty, parms.ShowAzimuthIndicatorBezelProperty,
                             parms.GDIPlusOptions);
            SetupOil1Renderer();
            SetupOil2Renderer();
            SetupNOZ1Renderer();
            SetupNOZ2Renderer();
            SetupNWSIndexerRenderer();
            SetupLandingGearLightsRenderer();
            SetupHSIRenderer();
            SetupEHSIRenderer(parms.GDIPlusOptions);
            SetupFuelQuantityRenderer(parms.FuelQuantityIndicatorNeedleIsCModelProperty);
            SetupFuelFlowRenderer();
            SetupISISRenderer(parms.ISISPressureUnitsProperty, parms.GDIPlusOptions);
            SetupAccelerometerRenderer();
            SetupFTIT2Renderer();
            SetupFTIT1Renderer();
            SetupEPUFuelRenderer();
            SetupPFLRenderer();
            SetupDEDRenderer();
            SetupCompassRenderer();
            SetupCMDSPanelRenderer();
            SetupCautionPanelRenderer();
            SetupAOAIndicatorRenderer();
            SetupAOAIndexerRenderer();
            SetupAltimeterRenderer(parms.AltimeterStyleProperty, parms.AltimeterPressureUnitsProperty);
            SetupASIRenderer();
            SetupADIRenderer();
            SetupStandbyADIRenderer();
            SetupHydARenderer();
            SetupHydBRenderer();
            SetupCabinPressRenderer();
            SetupRollTrimRenderer();
            SetupPitchTrimRenderer();
            SetupLMFDRenderer();
            SetupRMFDRenderer();
            SetupMFD3Renderer();
            SetupMFD4Renderer();
            SetupHUDRenderer();
        }

        private void SetupLMFDRenderer()
        {
            LMFDRenderer = new CanvasRenderer();
        }

        private void SetupRMFDRenderer()
        {
            RMFDRenderer = new CanvasRenderer();
        }

        private void SetupMFD3Renderer()
        {
            MFD3Renderer = new CanvasRenderer();
        }

        private void SetupMFD4Renderer()
        {
            MFD4Renderer = new CanvasRenderer();
        }

        private void SetupHUDRenderer()
        {
            HUDRenderer = new CanvasRenderer();
        }

        private void SetupVVIRenderer(PropertyInvoker<string> vviStyleProperty)
        {
            string vviStyleString = vviStyleProperty.GetProperty();
            var vviStyle = (VVIStyles) Enum.Parse(typeof (VVIStyles), vviStyleString);
            switch (vviStyle)
            {
                case VVIStyles.Tape:
                    VVIRenderer = new F16VerticalVelocityIndicatorUSA();
                    break;
                case VVIStyles.Needle:
                    VVIRenderer = new F16VerticalVelocityIndicatorEU();
                    break;
                default:
                    break;
            }
        }

        private void SetupRPM2Renderer()
        {
            RPM2Renderer = new F16Tachometer();
            ((F16Tachometer) RPM2Renderer).Options.IsSecondary = true;
        }

        private void SetupRPM1Renderer()
        {
            RPM1Renderer = new F16Tachometer();
            ((F16Tachometer) RPM1Renderer).Options.IsSecondary = false;
        }

        private void SetupSpeedbrakeRenderer()
        {
            SpeedbrakeRenderer = new F16SpeedbrakeIndicator();
        }

        private void SetupRWRRenderer(PropertyInvoker<string> azimuthIndicatorTypeProperty,
                                      PropertyInvoker<bool> ShowAzimuthIndicatorBezelProperty,
                                      GDIPlusOptions gdiPlusOptions)
        {
            RWRRenderer = new F16AzimuthIndicator();
            string styleString = azimuthIndicatorTypeProperty.GetProperty();
            var style =
                (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle)
                Enum.Parse(typeof (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle), styleString);
            ((F16AzimuthIndicator) RWRRenderer).Options.Style = style;
            ((F16AzimuthIndicator) RWRRenderer).Options.HideBezel = !ShowAzimuthIndicatorBezelProperty.GetProperty();
            ((F16AzimuthIndicator) RWRRenderer).Options.GDIPlusOptions = gdiPlusOptions;
        }

        private void SetupOil2Renderer()
        {
            OIL2Renderer = new F16OilPressureGauge();
            ((F16OilPressureGauge) OIL2Renderer).Options.IsSecondary = true;
        }

        private void SetupOil1Renderer()
        {
            OIL1Renderer = new F16OilPressureGauge();
            ((F16OilPressureGauge) OIL1Renderer).Options.IsSecondary = false;
        }

        private void SetupNOZ2Renderer()
        {
            NOZ2Renderer = new F16NozzlePositionIndicator();
            ((F16NozzlePositionIndicator) NOZ2Renderer).Options.IsSecondary = true;
        }

        private void SetupNOZ1Renderer()
        {
            NOZ1Renderer = new F16NozzlePositionIndicator();
            ((F16NozzlePositionIndicator) NOZ1Renderer).Options.IsSecondary = false;
        }

        private void SetupNWSIndexerRenderer()
        {
            NWSIndexerRenderer = new F16NosewheelSteeringIndexer();
        }

        private void SetupLandingGearLightsRenderer()
        {
            LandingGearLightsRenderer = new F16LandingGearWheelsLights();
        }

        private void SetupHSIRenderer()
        {
            HSIRenderer = new F16HorizontalSituationIndicator();
        }

        private void SetupEHSIRenderer(GDIPlusOptions gdiPlusOptions)
        {
            EHSIRenderer = new F16EHSI();
            ((F16EHSI) EHSIRenderer).Options.GDIPlusOptions = gdiPlusOptions;
        }

        private void SetupFuelQuantityRenderer(PropertyInvoker<bool> fuelQuantityIndicatorNeedleIsCModelProperty)
        {
            FuelQuantityRenderer = new F16FuelQuantityIndicator();
            if (fuelQuantityIndicatorNeedleIsCModelProperty.GetProperty())
            {
                ((F16FuelQuantityIndicator) FuelQuantityRenderer).Options.NeedleType =
                    F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.CModel;
            }
            else
            {
                ((F16FuelQuantityIndicator) FuelQuantityRenderer).Options.NeedleType =
                    F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.DModel;
            }
        }

        private void SetupFuelFlowRenderer()
        {
            FuelFlowRenderer = new F16FuelFlow();
        }

        private void SetupISISRenderer(PropertyInvoker<string> ISISPressureUnitsProperty, GDIPlusOptions gdiPlusOptions)
        {
            ISISRenderer = new F16ISIS();
            string pressureUnitsString = ISISPressureUnitsProperty.GetProperty();
            if (!string.IsNullOrEmpty(pressureUnitsString))
            {
                try
                {
                    ((F16ISIS) ISISRenderer).Options.PressureAltitudeUnits =
                        (F16ISIS.F16ISISOptions.PressureUnits)
                        Enum.Parse(typeof (F16ISIS.F16ISISOptions.PressureUnits), pressureUnitsString);
                }
                catch
                {
                    ((F16ISIS) ISISRenderer).Options.PressureAltitudeUnits =
                        F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury;
                }
            }
            ((F16ISIS) ISISRenderer).Options.GDIPlusOptions = gdiPlusOptions;
        }

        private void SetupAccelerometerRenderer()
        {
            AccelerometerRenderer = new F16Accelerometer();
        }

        private void SetupFTIT2Renderer()
        {
            FTIT2Renderer = new F16FanTurbineInletTemperature();
            ((F16FanTurbineInletTemperature) FTIT2Renderer).Options.IsSecondary = true;
        }

        private void SetupFTIT1Renderer()
        {
            FTIT1Renderer = new F16FanTurbineInletTemperature();
            ((F16FanTurbineInletTemperature) FTIT1Renderer).Options.IsSecondary = false;
        }

        private void SetupEPUFuelRenderer()
        {
            EPUFuelRenderer = new F16EPUFuelGauge();
        }

        private void SetupPFLRenderer()
        {
            PFLRenderer = new F16DataEntryDisplayPilotFaultList();
        }

        private void SetupDEDRenderer()
        {
            DEDRenderer = new F16DataEntryDisplayPilotFaultList();
        }

        private void SetupCompassRenderer()
        {
            CompassRenderer = new F16Compass();
        }

        private void SetupCMDSPanelRenderer()
        {
            CMDSPanelRenderer = new F16CMDSPanel();
        }

        private void SetupCautionPanelRenderer()
        {
            CautionPanelRenderer = new F16CautionPanel();
        }

        private void SetupAOAIndicatorRenderer()
        {
            AOAIndicatorRenderer = new F16AngleOfAttackIndicator();
        }

        private void SetupAOAIndexerRenderer()
        {
            AOAIndexerRenderer = new F16AngleOfAttackIndexer();
        }

        private void SetupAltimeterRenderer(PropertyInvoker<string> altimeterStyleProperty,
                                            PropertyInvoker<string> altimeterPressureUnitsProperty)
        {
            AltimeterRenderer = new F16Altimeter();

            string altimeterSyleString = altimeterStyleProperty.GetProperty();
            var altimeterStyle =
                (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle)
                Enum.Parse(typeof (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle), altimeterSyleString);
            ((F16Altimeter) AltimeterRenderer).Options.Style = altimeterStyle;

            string pressureUnitsString = altimeterPressureUnitsProperty.GetProperty();
            var pressureUnits =
                (F16Altimeter.F16AltimeterOptions.PressureUnits)
                Enum.Parse(typeof (F16Altimeter.F16AltimeterOptions.PressureUnits), pressureUnitsString);
            ((F16Altimeter) AltimeterRenderer).Options.PressureAltitudeUnits = pressureUnits;
        }

        private void SetupASIRenderer()
        {
            ASIRenderer = new F16AirspeedIndicator();
        }

        private void SetupADIRenderer()
        {
            ADIRenderer = new F16ADI();
        }

        private void SetupStandbyADIRenderer()
        {
            StandbyADIRenderer = new F16StandbyADI();
        }

        private void SetupHydARenderer()
        {
            HYDARenderer = new F16HydraulicPressureGauge();
        }

        private void SetupHydBRenderer()
        {
            HYDBRenderer = new F16HydraulicPressureGauge();
        }

        private void SetupCabinPressRenderer()
        {
            CabinPressRenderer = new F16CabinPressureAltitudeIndicator();
        }

        private void SetupRollTrimRenderer()
        {
            RollTrimRenderer = new F16RollTrimIndicator();
        }

        private void SetupPitchTrimRenderer()
        {
            PitchTrimRenderer = new F16PitchTrimIndicator();
        }

        #region Nested type: InitializationParams

        public class InitializationParams
        {
            public PropertyInvoker<string> VVIStyleProperty { get; set; }
            public PropertyInvoker<string> AzimuthIndicatorTypeProperty { get; set; }
            public PropertyInvoker<bool> ShowAzimuthIndicatorBezelProperty { get; set; }
            public PropertyInvoker<bool> FuelQuantityIndicatorNeedleIsCModelProperty { get; set; }
            public PropertyInvoker<string> ISISPressureUnitsProperty { get; set; }
            public PropertyInvoker<string> AltimeterStyleProperty { get; set; }
            public PropertyInvoker<string> AltimeterPressureUnitsProperty { get; set; }
            public GDIPlusOptions GDIPlusOptions { get; set; }
        }

        #endregion
    }
}