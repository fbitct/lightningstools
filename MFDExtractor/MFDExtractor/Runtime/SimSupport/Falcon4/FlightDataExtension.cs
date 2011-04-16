using System;

namespace MFDExtractor.Runtime.SimSupport.Falcon4
{
    [Serializable]
    public class FlightDataExtension
    {
        public float RadarAltitudeFeetAGL { get; set; }
        public float IndicatedAltitude { get; set; }
    }
}