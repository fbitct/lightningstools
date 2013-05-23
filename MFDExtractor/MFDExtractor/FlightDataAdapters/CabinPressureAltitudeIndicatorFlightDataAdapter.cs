using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface ICabinPressureAltitudeIndicatorFlightDataAdapter
    {
        void Adapt(IF16CabinPressureAltitudeIndicator cabinPressureAltitudeIndicator, FlightData flightData);
    }

    class CabinPressureAltitudeIndicatorFlightDataAdapter : ICabinPressureAltitudeIndicatorFlightDataAdapter
    {
        public void Adapt(IF16CabinPressureAltitudeIndicator cabinPressureAltitudeIndicator, FlightData flightData)
        {
            cabinPressureAltitudeIndicator.InstrumentState.CabinPressureAltitudeFeet = flightData.cabinAlt;
        }
    }
}
