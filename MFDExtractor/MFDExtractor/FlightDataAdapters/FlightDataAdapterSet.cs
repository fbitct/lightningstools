namespace MFDExtractor.FlightDataAdapters
{
    internal interface IFlightDataAdapterSet
    {
        ICMDSFlightDataAdapter CMDS { get; }
        INWSFlightDataAdapter NWS { get; }
        IDEDFlightDataAdapter DED { get; }
        IPFLFlightDataAdapter PFL { get; }
        ISpeedbrakeFlightDataAdapter Speedbrake { get; }
        IVVIFlightDataAdapter VVI { get; }
        IAltimeterFlightDataAdapter Altimeter { get; }
        IEPUFuelFlightDataAdapter EPUFuel { get; }
        ICautionPanelFlightDataAdapter CautionPanel { get; }
        ILandingGearLightsFlightDataAdapter LandingGearLights { get; }
        IAzimuthIndicatorFlightDataAdapter AzimuthIndicator { get; }
        IAirspeedIndicatorFlightDataAdapter AirspeedIndicator { get; }
        ICompassFlightDataAdapter Compass { get; }
        IAngleOfAttackIndicatorFlightDataAdapter AOAIndicator { get; }
        IAngleOfAttackIndexerFlightDataAdapter AOAIndexer { get; }
        IISISFlightDataAdapter ISIS { get; }
        IRPM1FlightDataAdapter RPM1 { get; }
        IRPM2FlightDataAdapter RPM2 { get; }
        ICabinPressureAltitudeIndicatorFlightDataAdapter CabinPress { get; }
        IRollTrimIndicatorFlightDataAdapter RollTrim { get; }
        IPitchTrimIndicatorFlightDataAdapter PitchTrim { get; }
        IFuelFlowFlightDataAdapter FuelFlow { get; }
    }

    class FlightDataAdapterSet : IFlightDataAdapterSet
    {
        public FlightDataAdapterSet()
        {
            CMDS = new CMDSFlightDataAdapter();
            NWS = new NWSFlightDataAdapter();
            DED = new DEDFlightDataAdapter();
            PFL = new PFLFlightDataAdapter();
            Speedbrake = new SpeedbrakeFlightDataAdapter();
            VVI = new VVIFlightDataAdapter();
            Altimeter = new AltimeterFlightDataAdapter();
            EPUFuel = new EPUFuelFlightDataAdapter();
            CautionPanel = new CautionPanelFlightDataAdapter();
            LandingGearLights = new LandingGearLightsFlightDataAdapter();
            AzimuthIndicator = new AzimuthIndicatorFlightDataAdapter();
            AirspeedIndicator = new AirspeedIndicatorFlightDataAdapter();
            Compass = new CompassFlightDataAdapter();
            AOAIndicator = new AngleOfAttackIndicatorFlightDataAdapter();
            AOAIndexer = new AngleOfAttackIndexerFlightDataAdapter(); 
            ISIS = new ISISFlightDataAdapter();
            RPM1 = new RPM1FlightDataAdapter();
            RPM2 = new RPM2FlightDataAdapter();
            CabinPress = new CabinPressureAltitudeIndicatorFlightDataAdapter();
            RollTrim = new RollTrimIndicatorFlightDataAdapter();
            PitchTrim = new PitchTrimIndicatorFlightDataAdapter();
            FuelFlow = new FuelFlowFlightDataAdapter();
        }

        public ICMDSFlightDataAdapter CMDS { get; private set; }
        public INWSFlightDataAdapter NWS { get; private set; }
        public IDEDFlightDataAdapter DED { get; private set; }
        public ISpeedbrakeFlightDataAdapter Speedbrake { get; private set; }
        public IVVIFlightDataAdapter VVI { get; private set; }
        public IAltimeterFlightDataAdapter Altimeter { get; private set; }
        public IPFLFlightDataAdapter PFL { get; private set; }
        public IEPUFuelFlightDataAdapter EPUFuel { get; private set; }
        public ICautionPanelFlightDataAdapter CautionPanel { get; private set; }
        public ILandingGearLightsFlightDataAdapter LandingGearLights { get; private set; }
        public IAzimuthIndicatorFlightDataAdapter AzimuthIndicator { get; private set; }
        public IAirspeedIndicatorFlightDataAdapter AirspeedIndicator { get; private set; }
        public ICompassFlightDataAdapter Compass { get; private set; }
        public IAngleOfAttackIndicatorFlightDataAdapter AOAIndicator { get; private set; }
        public IAngleOfAttackIndexerFlightDataAdapter AOAIndexer { get; private set; }
        public IISISFlightDataAdapter ISIS { get; private set; }
        public IRPM1FlightDataAdapter RPM1 { get; private set; }
        public IRPM2FlightDataAdapter RPM2 { get; private set; }
        public ICabinPressureAltitudeIndicatorFlightDataAdapter CabinPress { get; private set; }
        public IRollTrimIndicatorFlightDataAdapter RollTrim { get; private set; }
        public IPitchTrimIndicatorFlightDataAdapter PitchTrim { get; private set; }
        public IFuelFlowFlightDataAdapter FuelFlow { get; private set; }
    }
}
