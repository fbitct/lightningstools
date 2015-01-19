using Common.UI;
using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;
using LightningGauges.Renderers.F16.EHSI;
using LightningGauges.Renderers.F16.HSI;
using MFDExtractor.Properties;
using MFDExtractor.Renderer;
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
		    SetupLMFDRenderer();
			SetupRMFDRenderer();
			SetupMFD3Renderer();
			SetupMFD4Renderer();
			SetupHUDRenderer();
		}
		private void SetupLMFDRenderer()
		{
			_renderers.LMFD = new MfdRenderer
			{
				Options =
					new MfdRenderer.MfdRendererOptions
					{
						BlankImage = Resources.leftMFDBlankImage,
						TestAlignmentImage = Resources.leftMFDTestAlignmentImage
					}
			};
		}
		private void SetupRMFDRenderer()
		{
			_renderers.RMFD = new MfdRenderer
			{
				Options =
					new MfdRenderer.MfdRendererOptions
					{
						BlankImage = Resources.rightMFDBlankImage,
						TestAlignmentImage = Resources.rightMFDTestAlignmentImage
					}
			};
		}
		private void SetupMFD3Renderer()
		{
			_renderers.MFD3 = new MfdRenderer
			{
				Options =
					new MfdRenderer.MfdRendererOptions
					{
						BlankImage = Resources.leftMFDBlankImage,
						TestAlignmentImage = Resources.mfd3TestAlignmentImage
					}
			};
		}
		private void SetupMFD4Renderer()
		{
			_renderers.MFD4 = new MfdRenderer
			{
				Options =
					new MfdRenderer.MfdRendererOptions
					{
						BlankImage = Resources.rightMFDBlankImage,
						TestAlignmentImage = Resources.mfd4TestAlignmentImage
					}
			};
		}
		private void SetupHUDRenderer()
		{
			_renderers.HUD = new MfdRenderer
			{
				Options =
					new MfdRenderer.MfdRendererOptions
					{
						BlankImage = Resources.hudBlankImage,
						TestAlignmentImage = Resources.hudTestAlignmentImage
					}
			};
		}
        private void SetupVVIRenderer()
        {
	        _renderers.VVI = _vviRendererFactory.Create();
        }

        private void SetupRPM2Renderer()
        {
            _renderers.RPM2 = new Tachometer();
            ((Tachometer)_renderers.RPM2).Options.IsSecondary = true;
        }

        private void SetupRPM1Renderer()
        {
            _renderers.RPM1 = new Tachometer();
            ((Tachometer)_renderers.RPM1).Options.IsSecondary = false;
        }

        private void SetupSpeedbrakeRenderer()
        {
            _renderers.Speedbrake = new SpeedbrakeIndicator();
        }

        private void SetupRWRRenderer(GdiPlusOptions gdiPlusOptions)
        {
	        _renderers.RWR = _azimuthIndicatorFactory.Create(gdiPlusOptions);
        }

        private void SetupOil2Renderer()
        {
            _renderers.OIL2 = new OilPressureGauge();
            ((OilPressureGauge)_renderers.OIL2).Options.IsSecondary = true;
        }

        private void SetupOil1Renderer()
        {
            _renderers.OIL1 = new OilPressureGauge();
            ((OilPressureGauge)_renderers.OIL1).Options.IsSecondary = false;
        }

        private void SetupNOZ2Renderer()
        {
            _renderers.NOZ2 = new NozzlePositionIndicator();
            ((NozzlePositionIndicator)_renderers.NOZ2).Options.IsSecondary = true;
        }

        private void SetupNOZ1Renderer()
        {
            _renderers.NOZ1 = new NozzlePositionIndicator();
            ((NozzlePositionIndicator)_renderers.NOZ1).Options.IsSecondary = false;
        }

        private void SetupNWSIndexerRenderer()
        {
            _renderers.NWSIndexer = new NosewheelSteeringIndexer();
        }

        private void SetupLandingGearLightsRenderer()
        {
            _renderers.GearLights = new LandingGearWheelsLights();
        }

        private void SetupHSIRenderer()
        {
            _renderers.HSI = new HorizontalSituationIndicator();
        }

        private void SetupEHSIRenderer(GdiPlusOptions gdiPlusOptions)
        {
            _renderers.EHSI = new EHSI();
            ((EHSI)_renderers.EHSI).Options.GDIPlusOptions = gdiPlusOptions;
        }

        private void SetupFuelQuantityRenderer()
        {
	        _renderers.FuelQuantity = _fuelQualityIndicatorRendererFactory.Create();
        }

        private void SetupFuelFlowRenderer()
        {
            _renderers.FuelFlow = new FuelFlow();
        }

        private void SetupISISRenderer(GdiPlusOptions gdiPlusOptions)
        {
	        _renderers.ISIS = _isisRendererFactory.Create(gdiPlusOptions);
        }

        private void SetupAccelerometerRenderer()
        {
            _renderers.Accelerometer = new Accelerometer();
        }

        private void SetupFTIT2Renderer()
        {
            _renderers.FTIT2 = new FanTurbineInletTemperature();
            ((FanTurbineInletTemperature)_renderers.FTIT2).Options.IsSecondary = true;
        }

        private void SetupFTIT1Renderer()
        {
            _renderers.FTIT1 = new FanTurbineInletTemperature();
            ((FanTurbineInletTemperature)_renderers.FTIT1).Options.IsSecondary = false;
        }

        private void SetupEPUFuelRenderer()
        {
            _renderers.EPUFuel = new EPUFuelGauge();
        }

        private void SetupPFLRenderer()
        {
            _renderers.PFL = new DataEntryDisplayPilotFaultList();
        }

        private void SetupDEDRenderer()
        {
            _renderers.DED = new DataEntryDisplayPilotFaultList();
        }

        private void SetupCompassRenderer()
        {
            _renderers.Compass = new Compass();
        }

        private void SetupCMDSPanelRenderer()
        {
            _renderers.CMDS = new CMDSPanel();
        }

        private void SetupCautionPanelRenderer()
        {
            _renderers.CautionPanel = new CautionPanel();
        }

        private void SetupAOAIndicatorRenderer()
        {
            _renderers.AOAIndicator = new AngleOfAttackIndicator();
        }

        private void SetupAOAIndexerRenderer()
        {
            _renderers.AOAIndexer = new AngleOfAttackIndexer();
        }

        private void SetupAltimeterRenderer()
        {
	        _renderers.Altimeter = _altimeterRendererFactory.Create();
        }

        private void SetupASIRenderer()
        {
            _renderers.ASI = new AirspeedIndicator();
        }

        private void SetupADIRenderer()
        {
            _renderers.ADI = new ADI();;
        }

        private void SetupBackupADIRenderer()
        {
            _renderers.BackupADI = new StandbyADI();
        }

        private void SetupHydARenderer()
        {
            _renderers.HYDA = new HydraulicPressureGauge();
        }

        private void SetupHydBRenderer()
        {
            _renderers.HYDB = new HydraulicPressureGauge();
        }

        private void SetupCabinPressRenderer()
        {
            _renderers.CabinPress = new CabinPressureAltitudeIndicator();
        }

        private void SetupRollTrimRenderer()
        {
            _renderers.RollTrim = new RollTrimIndicator();
        }

        private void SetupPitchTrimRenderer()
        {
            _renderers.PitchTrim = new PitchTrimIndicator();
        }
        
    }
}
