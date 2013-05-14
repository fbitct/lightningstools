using F4SharedMem;
using F4Utils.SimSupport;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface INOZ2FlightDataAdapter
    {
        void Adapt(IF16NozzlePositionIndicator nozzlePositionIndicator, FlightData flightData);
    }

    class NOZ2FlightDataAdapter : INOZ2FlightDataAdapter
    {
        public void Adapt(IF16NozzlePositionIndicator nozzlePositionIndicator, FlightData flightData)
        {
            switch (flightData.DataFormat)
            {
                case FalconDataFormats.OpenFalcon:
                    nozzlePositionIndicator.InstrumentState.NozzlePositionPercent = NonImplementedGaugeCalculations.NOZ(flightData.rpm2, flightData.z, flightData.fuelFlow);
                    break;
                case FalconDataFormats.BMS4:
                    nozzlePositionIndicator.InstrumentState.NozzlePositionPercent = flightData.nozzlePos2 * 100.0f;
                    break;
                default:
                    nozzlePositionIndicator.InstrumentState.NozzlePositionPercent = flightData.nozzlePos2;
                    break;
            }
        }
    }
}
