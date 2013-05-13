using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface INWSFlightDataAdapter
    {
        void Adapt(IF16NosewheelSteeringIndexer nwsIndexer, FlightData fromFalcon);
    }

    class NWSFlightDataAdapter : INWSFlightDataAdapter
    {
        public void Adapt(IF16NosewheelSteeringIndexer nwsIndexer, FlightData fromFalcon)

        {
            nwsIndexer.InstrumentState.DISC = ((fromFalcon.lightBits & (int)LightBits.RefuelDSC) == (int)LightBits.RefuelDSC);
            nwsIndexer.InstrumentState.AR_NWS = ((fromFalcon.lightBits & (int)LightBits.RefuelAR) == (int)LightBits.RefuelAR);
            nwsIndexer.InstrumentState.RDY = ((fromFalcon.lightBits & (int)LightBits.RefuelRDY) == (int)LightBits.RefuelRDY);
        }
    }
}
