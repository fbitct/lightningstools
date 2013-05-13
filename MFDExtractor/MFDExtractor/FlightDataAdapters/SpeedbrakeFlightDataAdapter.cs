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

            if (flightData.DataFormat == FalconDataFormats.BMS4)
            {
                speedbrakeIndicator.InstrumentState.PowerLoss = ((flightData.lightBits3 & (int)Bms4LightBits3.Power_Off) == (int)Bms4LightBits3.Power_Off);
            }
            else
            {
                speedbrakeIndicator.InstrumentState.PowerLoss = ((flightData.lightBits3 & (int)LightBits3.Power_Off) == (int)LightBits3.Power_Off);
            }
        }
    }
}
