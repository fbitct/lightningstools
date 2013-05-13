namespace MFDExtractor.FlightDataAdapters
{
    internal interface IFlightDataAdapterSet
    {
        ICMDSFlightDataAdapter CMDS { get; }
        INWSFlightDataAdapter NWS { get; }
        IDEDFlightDataAdapter DED { get; }
    }

    class FlightDataAdapterSet : IFlightDataAdapterSet
    {
        private readonly ICMDSFlightDataAdapter _cmds;
        private readonly INWSFlightDataAdapter _nws;
        private readonly IDEDFlightDataAdapter _ded;

        public FlightDataAdapterSet()
        {
            _cmds = new CMDSFlightDataAdapter();
            _nws = new NWSFlightDataAdapter();
            _ded = new DEDFlightDataAdapter();
        }
        public ICMDSFlightDataAdapter CMDS { get { return _cmds; } }
        public INWSFlightDataAdapter NWS { get { return _nws; } }
        public IDEDFlightDataAdapter DED { get { return _ded; } }
    }
}
