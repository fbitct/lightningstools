namespace MFDExtractor.FlightDataAdapters
{
    internal interface IFlightDataAdapterSet
    {
        ICMDSFlightDataAdapter CMDS { get; }
        INWSFlightDataAdapter NWS { get; }
        IDEDFlightDataAdapter DED { get; }
        ISpeedbrakeFlightDataAdapter Speedbrake { get; }
        IVVIFlightDataAdapter VVI { get; }
        IAltimeterFlightDataAdapter Altimeter { get; }
    }

    class FlightDataAdapterSet : IFlightDataAdapterSet
    {
        private readonly ICMDSFlightDataAdapter _cmds;
        private readonly INWSFlightDataAdapter _nws;
        private readonly IDEDFlightDataAdapter _ded;
        private readonly ISpeedbrakeFlightDataAdapter _speedbrake;
        private readonly IVVIFlightDataAdapter _vvi;
        private readonly IAltimeterFlightDataAdapter _altimeter;
        public FlightDataAdapterSet()
        {
            _cmds = new CMDSFlightDataAdapter();
            _nws = new NWSFlightDataAdapter();
            _ded = new DEDFlightDataAdapter();
            _speedbrake = new SpeedbrakeFlightDataAdapter();
            _vvi = new VVIFlightDataAdapter();
            _altimeter = new AltimeterFlightDataAdapter();
        }
        public ICMDSFlightDataAdapter CMDS { get { return _cmds; } }
        public INWSFlightDataAdapter NWS { get { return _nws; } }
        public IDEDFlightDataAdapter DED { get { return _ded; } }
        public ISpeedbrakeFlightDataAdapter Speedbrake { get { return _speedbrake; } }
        public IVVIFlightDataAdapter VVI { get { return _vvi; } }
        public IAltimeterFlightDataAdapter Altimeter { get { return _altimeter; } }
    }
}
