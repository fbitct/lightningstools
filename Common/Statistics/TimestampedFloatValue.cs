using System;

namespace Common.Statistics
{
    [Serializable]
    public struct TimestampedFloatValue
    {
        public DateTime Timestamp;
        public float Value;
    }

}
