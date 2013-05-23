using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IHYDBFlightDataAdapter
    {
        void Adapt(IF16HydraulicPressureGauge hydraulicPressureGaugeB, FlightData flightData);
    }

    class HYDBFlightDataAdapter : IHYDBFlightDataAdapter
    {
        public void Adapt(IF16HydraulicPressureGauge hydraulicPressureGaugeB, FlightData flightData)
        {
            hydraulicPressureGaugeB.InstrumentState.HydraulicPressurePoundsPerSquareInch = flightData.hydPressureB;
        }
    }
}
