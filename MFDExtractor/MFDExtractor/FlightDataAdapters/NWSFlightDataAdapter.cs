using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface INWSFlightDataAdapter
    {
        void Adapt(IF16NosewheelSteeringIndexer nwsIndexer, FlightData flightData);
    }

    class NWSFlightDataAdapter : INWSFlightDataAdapter
    {
        public void Adapt(IF16NosewheelSteeringIndexer nwsIndexer, FlightData flightData)

        {
            nwsIndexer.InstrumentState.DISC = ((flightData.lightBits & (int)LightBits.RefuelDSC) == (int)LightBits.RefuelDSC);
            nwsIndexer.InstrumentState.AR_NWS = ((flightData.lightBits & (int)LightBits.RefuelAR) == (int)LightBits.RefuelAR);
            nwsIndexer.InstrumentState.RDY = ((flightData.lightBits & (int)LightBits.RefuelRDY) == (int)LightBits.RefuelRDY);
        }
    }
}
