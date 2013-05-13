﻿using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IVVIFlightDataAdapter
    {
        void Adapt(IF16VerticalVelocityIndicator verticalVelocityIndicator, FlightData flightData);
    }

    class VVIFlightDataAdapter : IVVIFlightDataAdapter
    {
        public void Adapt(IF16VerticalVelocityIndicator verticalVelocityIndicator, FlightData flightData)
        {
            float verticalVelocity;
            var hsiBits = (HsiBits) flightData.hsiBits;
            if (((hsiBits & HsiBits.VVI) == HsiBits.VVI))
            {
                verticalVelocity = 0;
            }
            else
            {
                verticalVelocity = -flightData.zDot * 60.0f;
            }

            verticalVelocityIndicator.InstrumentState.OffFlag = ((hsiBits & HsiBits.VVI) == HsiBits.VVI);
            verticalVelocityIndicator.InstrumentState.VerticalVelocityFeet = verticalVelocity;

        }
    }
}
