using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface INOZ2FlightDataAdapter
    {
        void Adapt(IF16NozzlePositionIndicator nozzlePositionIndicator, FlightData flightData);
    }

    class NOZ2FlightDataAdapter : INOZ2FlightDataAdapter
    {
        public void Adapt(IF16NozzlePositionIndicator nozzlePositionIndicator, FlightData flightData)
        {
            nozzlePositionIndicator.InstrumentState.NozzlePositionPercent = flightData.nozzlePos2 * 100.0f;
        }
    }
}
