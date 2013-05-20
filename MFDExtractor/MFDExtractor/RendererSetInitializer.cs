using Common.UI;
using LightningGauges.Renderers;
using MFDExtractor.RendererFactories;

namespace MFDExtractor
{
    internal interface IRendererSetInitializer
    {
        void Initialize(GdiPlusOptions gdiPlusOptions);
    }

    class RendererSetInitializer : IRendererSetInitializer
    {
        private readonly IInstrumentRendererSet _renderers;
	    private readonly IAzimuthIndicatorFactory _azimuthIndicatorFactory;
	    private readonly IAltimeterRendererFactory _altimeterRendererFactory;
	    private readonly IFuelQualityIndicatorRendererFactory _fuelQualityIndicatorRendererFactory;
	    private readonly IISISRendererFactory _isisRendererFactory;
	    private readonly IVVIRendererFactory _vviRendererFactory;
        public RendererSetInitializer(
			IInstrumentRendererSet renderers = null, 
			IAzimuthIndicatorFactory azimuthIndicatorFactory = null, 
			IFuelQualityIndicatorRendererFactory fuelQualityIndicatorRendererFactory=null, 
			IAltimeterRendererFactory altimeterRendererFactory=null, 
			IISISRendererFactory isisRendererFactory=null, 
			IVVIRendererFactory vviRendererFactory=null)
        {
	        _renderers = renderers ?? new InstrumentRendererSet();
			_azimuthIndicatorFactory = azimuthIndicatorFactory ?? new AzimuthIndicatorFactory();
			_altimeterRendererFactory = altimeterRendererFactory ?? new AltimeterRendererFactory();
			_fuelQualityIndicatorRendererFactory = fuelQualityIndicatorRendererFactory ?? new FuelQualityIndicatorRendererFactory();
	        _isisRendererFactory = isisRendererFactory ?? new ISISRendererFactory();
			_vviRendererFactory = vviRendererFactory ?? new VVIRendererFactory();
		}

	    public void Initialize(GdiPlusOptions gdiPlusOptions)
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
	        _renderers.VVI = _vviRendererFactory.Create();
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

        private void SetupRWRRenderer(GdiPlusOptions gdiPlusOptions)
        {
	        _renderers.RWR = _azimuthIndicatorFactory.Create(gdiPlusOptions);
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

        private void SetupEHSIRenderer(GdiPlusOptions gdiPlusOptions)
        {
            _renderers.EHSI = new F16EHSI();
            ((F16EHSI)_renderers.EHSI).Options.GDIPlusOptions = gdiPlusOptions;
        }

        private void SetupFuelQuantityRenderer()
        {
	        _renderers.FuelQuantity = _fuelQualityIndicatorRendererFactory.Create();
        }

        private void SetupFuelFlowRenderer()
        {
            _renderers.FuelFlow = new F16FuelFlow();
        }

        private void SetupISISRenderer(GdiPlusOptions gdiPlusOptions)
        {
	        _renderers.ISIS = _isisRendererFactory.Create(gdiPlusOptions);
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
	        _renderers.Altimeter = _altimeterRendererFactory.Create();
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
