using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IAirspeedIndicatorFlightDataAdapter
    {
        void Adapt(IF16AirspeedIndicator airspeedIndicator, FlightData flightData);
    }

    class AirspeedIndicatorFlightDataAdapter : IAirspeedIndicatorFlightDataAdapter
    {
        public void Adapt(IF16AirspeedIndicator airspeedIndicator, FlightData flightData)
        {
            airspeedIndicator.InstrumentState.AirspeedKnots = flightData.kias;
            airspeedIndicator.InstrumentState.MachNumber = flightData.mach;
        }
    }
}
