using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IOIL1FlightDataAdapter
    {
        void Adapt(IF16OilPressureGauge oil1, FlightData flightData);
    }

    class OIL1FlightDataAdapter : IOIL1FlightDataAdapter
    {
        public void Adapt(IF16OilPressureGauge oil1, FlightData flightData)
        {
            oil1.InstrumentState.OilPressurePercent = flightData.oilPressure;
        }
    }
}
