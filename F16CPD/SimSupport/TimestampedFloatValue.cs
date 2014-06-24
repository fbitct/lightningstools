using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport
{
    [Serializable]
    public struct TimestampedFloatValue
    {
        public DateTime Timestamp;
        public float Value;
    }

}
