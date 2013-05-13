using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IRPM1FlightDataAdapter
    {
        void Adapt(IF16Tachometer rpm1, FlightData flightData);
    }

    class RPM1FlightDataAdapter : IRPM1FlightDataAdapter
    {
        public void Adapt(IF16Tachometer rpm1, FlightData flightData)
        {
            rpm1.InstrumentState.RPMPercent = flightData.rpm;
        }
    }
}
