﻿using Common.Math;
using F4SharedMem;
using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface ICompassFlightDataAdapter
    {
        void Adapt(ICompass compass, FlightData flightData);
    }

    class CompassFlightDataAdapter : ICompassFlightDataAdapter
    {
        public void Adapt(ICompass compass, FlightData flightData)
        {
            compass.InstrumentState.MagneticHeadingDegrees = (360 + (flightData.yaw / Constants.RADIANS_PER_DEGREE)) % 360;
        }
    }
}
