using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.SimSupport;
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
            var rpm = flightData.rpm;
            var mainGen = ((flightData.lightBits3 & (int)LightBits3.MainGen) == (int)LightBits3.MainGen);
            var stbyGen = ((flightData.lightBits3 & (int)LightBits3.StbyGen) == (int)LightBits3.StbyGen);
            var epuGen = ((flightData.lightBits3 & (int)LightBits3.EpuGen) == (int)LightBits3.EpuGen);
            var epuOn = ((flightData.lightBits2 & (int)LightBits2.EPUOn) == (int)LightBits2.EPUOn);
            var epuFuel = flightData.epuFuel;
            hydraulicPressureGaugeB.InstrumentState.HydraulicPressurePoundsPerSquareInch = NonImplementedGaugeCalculations.HydB(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
        }
    }
}
