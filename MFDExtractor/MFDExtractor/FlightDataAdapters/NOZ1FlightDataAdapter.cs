using F4SharedMem;
using F4Utils.SimSupport;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface INOZ1FlightDataAdapter
    {
        void Adapt(IF16NozzlePositionIndicator nozzlePositionIndicator, FlightData flightData);
    }

    class NOZ1FlightDataAdapter : INOZ1FlightDataAdapter
    {
        public void Adapt(IF16NozzlePositionIndicator nozzlePositionIndicator, FlightData flightData)
        {
            nozzlePositionIndicator.InstrumentState.NozzlePositionPercent = flightData.nozzlePos * 100.0f;
        }
    }
}
