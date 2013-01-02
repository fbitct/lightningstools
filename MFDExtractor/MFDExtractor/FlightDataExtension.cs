using System;

namespace MFDExtractor
{
    [Serializable]
    public class FlightDataExtension
    {
        public float RadarAltitudeFeetAGL { get; set; }
        public float IndicatedAltitude { get; set; }
    }
}