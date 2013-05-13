using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IAccelerometerFlightDataAdapter
    {
        void Adapt(F16Accelerometer accelerometer, FlightData flightData);
    }

    class AccelerometerFlightDataAdapter : IAccelerometerFlightDataAdapter
    {
        public void Adapt(F16Accelerometer accelerometer, FlightData flightData)
        {
            var gs = flightData.gs;
            if (gs == 0) //ignore exactly zero g's
            {
                gs = 1;
            }
            accelerometer.InstrumentState.AccelerationInGs = gs;
        }
    }
}
