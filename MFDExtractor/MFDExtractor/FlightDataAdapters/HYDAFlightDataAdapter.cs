using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IHYDAFlightDataAdapter
    {
        void Adapt(IF16HydraulicPressureGauge hydraulicPressureGaugeA, FlightData flightData);
    }

    class HYDAFlightDataAdapter : IHYDAFlightDataAdapter
    {
        public void Adapt(IF16HydraulicPressureGauge hydraulicPressureGaugeA, FlightData flightData)
        {
            hydraulicPressureGaugeA.InstrumentState.HydraulicPressurePoundsPerSquareInch = flightData.hydPressureA;
        }
    }
}
