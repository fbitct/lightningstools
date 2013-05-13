using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IRollTrimIndicatorFlightDataAdapter
    {
        void Adapt(IF16RollTrimIndicator rollTrimIndicator, FlightData flightData);
    }

    class RollTrimIndicatorFlightDataAdapter : IRollTrimIndicatorFlightDataAdapter
    {
        public void Adapt(IF16RollTrimIndicator rollTrimIndicator, FlightData flightData)
        {
            var rolltrim = flightData.TrimRoll;
            rollTrimIndicator.InstrumentState.RollTrimPercent = rolltrim * 2.0f * 100.0f;

        }
    }
}
