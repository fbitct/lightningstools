using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4
{
    internal interface IMedianOfSamplesCalculator
    {
        float MedianSampleValue(List<TimestampedFloatValue> values);
    }
    class MedianOfSamplesCalculator:IMedianOfSamplesCalculator
    {
        public float MedianSampleValue(List<TimestampedFloatValue> values)
        {
            if (values.Count == 0)
            {
                return 0;
            }

            var justTheValues = new float[values.Count];
            for (var i = 0; i < values.Count; i++)
            {
                justTheValues[i] = values[i].Value;
            }

            Array.Sort(justTheValues);

            var itemIndex = justTheValues.Length / 2;

            if (justTheValues.Length % 2 == 0)
            {
                // Even number of items.
                return (justTheValues[itemIndex] + justTheValues[itemIndex - 1]) / 2;
            }
            // Odd number of items.
            return justTheValues[itemIndex];
        }

    }
}
