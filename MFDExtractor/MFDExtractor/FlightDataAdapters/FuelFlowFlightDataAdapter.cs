using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IFuelFlowFlightDataAdapter
    {
        void Adapt(IF16FuelFlow fuelFlow, FlightData flightData);
    }

    class FuelFlowFlightDataAdapter : IFuelFlowFlightDataAdapter
    {
        public void Adapt(IF16FuelFlow fuelFlow, FlightData flightData)
        {
            fuelFlow.InstrumentState.FuelFlowPoundsPerHour = flightData.fuelFlow;
        }
    }
}
