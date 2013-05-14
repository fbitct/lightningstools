using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IFuelQuantityFlightDataAdapter
    {
        void Adapt(IF16FuelQuantityIndicator fuelQuantityIndicator, FlightData flightData);
    }

    class FuelQuantityFlightDataAdapter : IFuelQuantityFlightDataAdapter
    {
        public void Adapt(IF16FuelQuantityIndicator fuelQuantityIndicator, FlightData flightData)
        {
            fuelQuantityIndicator.InstrumentState.AftLeftFuelQuantityPounds =flightData.aft/10.0f;
            fuelQuantityIndicator.InstrumentState.ForeRightFuelQuantityPounds =flightData.fwd/10.0f;
            fuelQuantityIndicator.InstrumentState.TotalFuelQuantityPounds =flightData.total;

        }
    }
}
