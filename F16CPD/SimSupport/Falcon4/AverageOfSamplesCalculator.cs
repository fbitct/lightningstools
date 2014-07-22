using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Statistics;

namespace F16CPD.SimSupport.Falcon4
{
    internal interface IAverageOfSamplesCalculator
    {
        float AverageSampleValue(List<TimestampedFloatValue> values);
    }
    class AverageOfSamplesCalculator:IAverageOfSamplesCalculator
    {
        public float AverageSampleValue(List<TimestampedFloatValue> values)
        {
            float sum = 0;
            for (var i = 0; i < values.Count; i++)
            {
                sum += values[i].Value;
            }

            var avg = values.Count > 0 ? sum / values.Count : 0;
            return avg;
        }

    }
}
