using System.Collections.Generic;

namespace Common.Statistics
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
