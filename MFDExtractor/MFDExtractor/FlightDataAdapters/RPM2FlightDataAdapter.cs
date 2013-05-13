using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IRPM2FlightDataAdapter
    {
        void Adapt(IF16Tachometer rpm1, FlightData flightData);
    }

    class RPM2FlightDataAdapter : IRPM2FlightDataAdapter
    {
        public void Adapt(IF16Tachometer rpm2, FlightData flightData)
        {
            rpm2.InstrumentState.RPMPercent = flightData.rpm2;
        }
    }
}
