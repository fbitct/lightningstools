using Common.Math;
using F4SharedMem;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface ICompassFlightDataAdapter
    {
        void Adapt(IF16Compass compass, FlightData flightData);
    }

    class CompassFlightDataAdapter : ICompassFlightDataAdapter
    {
        public void Adapt(IF16Compass compass, FlightData flightData)
        {
            compass.InstrumentState.MagneticHeadingDegrees = (360 + (flightData.yaw / Constants.RADIANS_PER_DEGREE)) % 360;
        }
    }
}
