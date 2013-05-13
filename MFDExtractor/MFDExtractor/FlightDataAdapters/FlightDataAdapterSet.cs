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
    }
}
