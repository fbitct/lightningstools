using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IEPUFuelFlightDataAdapter
    {
        void Adapt(IF16EPUFuelGauge epuFuelGauge, FlightData flightData);
    }

    class EPUFuelFlightDataAdapter : IEPUFuelFlightDataAdapter
    {
        public void Adapt(IF16EPUFuelGauge epuFuelGauge, FlightData flightData)
        {
            epuFuelGauge.InstrumentState.FuelRemainingPercent = flightData.epuFuel;
        }
    }
}
