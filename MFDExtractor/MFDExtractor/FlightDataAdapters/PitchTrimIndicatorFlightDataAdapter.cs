using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IPitchTrimIndicatorFlightDataAdapter
    {
        void Adapt(IF16PitchTrimIndicator pitchTrimIndicator, FlightData flightData);
    }

    class PitchTrimIndicatorFlightDataAdapter : IPitchTrimIndicatorFlightDataAdapter
    {
        public void Adapt(IF16PitchTrimIndicator pitchTrimIndicator, FlightData flightData)
        {
            var pitchTrim = flightData.TrimPitch;
            pitchTrimIndicator.InstrumentState.PitchTrimPercent = pitchTrim * 2.0f * 100.0f;        }
    }
}
