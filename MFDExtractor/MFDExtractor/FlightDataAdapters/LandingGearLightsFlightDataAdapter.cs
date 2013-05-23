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
            landingGearLights.InstrumentState.LeftGearDown = ((flightData.lightBits3 & (int)LightBits3.LeftGearDown) == (int)LightBits3.LeftGearDown);
            landingGearLights.InstrumentState.NoseGearDown = ((flightData.lightBits3 & (int)LightBits3.NoseGearDown) == (int)LightBits3.NoseGearDown);
            landingGearLights.InstrumentState.RightGearDown = ((flightData.lightBits3 & (int)LightBits3.RightGearDown) == (int)LightBits3.RightGearDown);
        }
    }
}
