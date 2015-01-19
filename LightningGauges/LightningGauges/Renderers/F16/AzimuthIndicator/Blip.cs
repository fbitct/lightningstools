using System;

namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    [Serializable]
    public class Blip
    {
        private float _bearingDegrees;
        public int SymbolID { get; set; }

        public float BearingDegrees
        {
            get { return _bearingDegrees; }
            set
            {
                if (Single.IsNaN(value) || Single.IsInfinity(value)) value = 0;
                var bearing = value;
                bearing %= 360.0f;
                _bearingDegrees = bearing;
            }
        }

        public int MissileActivity { get; set; }
        public int MissileLaunch { get; set; }
        public int Selected { get; set; }
        private float _lethality;
        public float Lethality 
        { 
            get { return _lethality; } 
            set 
            { 
                if (Single.IsNaN(value) || Single.IsInfinity(value)) value = 0;
                _lethality = value;
            } 
        }
        public int NewDetection { get; set; }
        public bool Visible { get; set; }
    }
}