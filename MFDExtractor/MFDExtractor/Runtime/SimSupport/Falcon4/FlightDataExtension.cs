using System;
using System.Collections.Generic;
using System.Text;

namespace MFDExtractor.Runtime.SimSupport.Falcon4
{
    [Serializable]
    public class FlightDataExtension
    {
        public float RadarAltitudeFeetAGL
        {
            get;
            set;
        }
        public float IndicatedAltitude
        {
            get;
            set;
        }
    }
}
