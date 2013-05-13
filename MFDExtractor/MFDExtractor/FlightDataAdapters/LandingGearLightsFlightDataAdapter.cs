using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface ILandingGearLightsFlightDataAdapter
    {
        void Adapt(IF16LandingGearWheelsLights landingGearLights, FlightData flightData);
    }

    class LandingGearLightsFlightDataAdapter : ILandingGearLightsFlightDataAdapter
    {
        public void Adapt(IF16LandingGearWheelsLights landingGearLights, FlightData flightData)
        {
            if (flightData.DataFormat == FalconDataFormats.OpenFalcon || flightData.DataFormat == FalconDataFormats.BMS4)
            {
                landingGearLights.InstrumentState.LeftGearDown = ((flightData.lightBits3 & (int)LightBits3.LeftGearDown) == (int)LightBits3.LeftGearDown);
                landingGearLights.InstrumentState.NoseGearDown = ((flightData.lightBits3 & (int)LightBits3.NoseGearDown) == (int)LightBits3.NoseGearDown);
                landingGearLights.InstrumentState.RightGearDown = ((flightData.lightBits3 & (int)LightBits3.RightGearDown) == (int)LightBits3.RightGearDown);
            }
            else
            {
                landingGearLights.InstrumentState.LeftGearDown = ((flightData.LeftGearPos == 1.0f));
                landingGearLights.InstrumentState.NoseGearDown = ((flightData.NoseGearPos == 1.0f));
                landingGearLights.InstrumentState.RightGearDown = ((flightData.RightGearPos == 1.0f));
            }
        }
    }
}
