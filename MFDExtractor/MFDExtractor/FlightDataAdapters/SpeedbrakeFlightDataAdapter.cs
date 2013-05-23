using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface ISpeedbrakeFlightDataAdapter
    {
        void Adapt(IF16SpeedbrakeIndicator speedbrakeIndicator, FlightData flightData);
    }

    class SpeedbrakeFlightDataAdapter : ISpeedbrakeFlightDataAdapter
    {
        public void Adapt(IF16SpeedbrakeIndicator speedbrakeIndicator, FlightData flightData)
        {
            speedbrakeIndicator.InstrumentState.PercentOpen = flightData.speedBrake * 100.0f;
            speedbrakeIndicator.InstrumentState.PowerLoss = ((flightData.lightBits3 & (int)Bms4LightBits3.Power_Off) == (int)Bms4LightBits3.Power_Off);
        }
    }
}
