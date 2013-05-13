using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.SimSupport;
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
            var z = flightData.z;
            var origCabinAlt = cabinPressureAltitudeIndicator.InstrumentState.CabinPressureAltitudeFeet;
            var pressurization = ((flightData.lightBits & (int)LightBits.CabinPress) == (int)LightBits.CabinPress);
            cabinPressureAltitudeIndicator.InstrumentState.CabinPressureAltitudeFeet = NonImplementedGaugeCalculations.CabinAlt(origCabinAlt, z, pressurization);
        }
    }
}
