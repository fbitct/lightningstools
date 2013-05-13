namespace LightningGauges.Renderers
{
    public class F16VerticalVelocityIndicatorInstrumentState
    {
        private const float MAX_VELOCITY = 6000;
        private const float MIN_VELOCITY = -6000;
        private float _verticalVelocityFeet;
        
        public F16VerticalVelocityIndicatorInstrumentState()
        {
                OffFlag = false;
                VerticalVelocityFeet = 0.0f;
        }

        public float VerticalVelocityFeet
        {
            get { return _verticalVelocityFeet; }
            set
            {
                var vv = value;
                if (vv < MIN_VELOCITY) vv = MIN_VELOCITY;
                if (vv > MAX_VELOCITY) vv = MAX_VELOCITY;
                _verticalVelocityFeet = vv;
            }
        }

        public bool OffFlag { get; set; }

    }
}