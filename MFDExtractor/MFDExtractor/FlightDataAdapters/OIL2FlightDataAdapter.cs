using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IOIL2FlightDataAdapter
    {
        void Adapt(IF16OilPressureGauge oil2, FlightData flightData);
    }

    class OIL2FlightDataAdapter : IOIL1FlightDataAdapter
    {
        public void Adapt(IF16OilPressureGauge oil2, FlightData flightData)
        {
            oil2.InstrumentState.OilPressurePercent = flightData.oilPressure2;
        }
    }
}
