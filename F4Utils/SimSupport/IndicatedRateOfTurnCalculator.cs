using Common.Statistics;
using F4SharedMem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F4Utils.SimSupport
{
    public interface IIndicatedRateOfTurnCalculator
    {
        float DetermineIndicatedRateOfTurn(float currentMagneticHeadingDecimalDegrees);
        void Reset();
    }
    public class IndicatedRateOfTurnCalculator : IIndicatedRateOfTurnCalculator
    {
        private const float MAX_INDICATED_RATE_OF_TURN_DECIMAL_DEGREES_PER_SECOND = 3.0f;
        private TimestampedFloatValue _lastHeadingSample;
        private List<TimestampedFloatValue> _lastInstantaneousRatesOfTurn = new List<TimestampedFloatValue>();
        private readonly IMedianOfSamplesCalculator _medianOfSamplesCalculator = new MedianOfSamplesCalculator();
        private float _lastIndicatedRateOfTurn =0;
        public float DetermineIndicatedRateOfTurn(float currentMagneticHeadingDecimalDegrees)
        {
            //capture the current time
            var curTime = DateTime.Now;

            //determine how many seconds it's been since our last "current heading" datum snapshot?
            var dT = (float)((curTime.Subtract(_lastHeadingSample.Timestamp)).TotalMilliseconds);

            //determine the change in heading since our last snapshot
            var currentHeading = currentMagneticHeadingDecimalDegrees;
            var headingDelta = Common.Math.Util.AngleDelta(_lastHeadingSample.Value, currentHeading);

            //now calculate the instantaneous rate of turn
            var currentInstantaneousRateOfTurn = (headingDelta / dT) * 1000;
            if (Math.Abs(currentInstantaneousRateOfTurn) > 30 || float.IsInfinity(currentInstantaneousRateOfTurn) ||
                float.IsNaN(currentInstantaneousRateOfTurn)) currentInstantaneousRateOfTurn = 0; //noise
            if (Math.Abs(currentInstantaneousRateOfTurn) >
                (MAX_INDICATED_RATE_OF_TURN_DECIMAL_DEGREES_PER_SECOND + 0.5f))
            {
                currentInstantaneousRateOfTurn = (MAX_INDICATED_RATE_OF_TURN_DECIMAL_DEGREES_PER_SECOND + 0.5f) *
                                                 Math.Sign(currentInstantaneousRateOfTurn);
            }

            var sample = new TimestampedFloatValue { Timestamp = curTime, Value = currentInstantaneousRateOfTurn };

            //cull historic rate-of-turn samples older than n seconds
            var replacementList = new List<TimestampedFloatValue>();
            for (var i = 0; i < _lastInstantaneousRatesOfTurn.Count; i++)
            {
                if (!(Math.Abs(curTime.Subtract(_lastInstantaneousRatesOfTurn[i].Timestamp).TotalMilliseconds) > 1000))
                {
                    replacementList.Add(_lastInstantaneousRatesOfTurn[i]);
                }
            }
            _lastInstantaneousRatesOfTurn = replacementList;

            _lastInstantaneousRatesOfTurn.Add(sample);

            var medianRateOfTurn = (float)Math.Round(_medianOfSamplesCalculator.MedianSampleValue(_lastInstantaneousRatesOfTurn), 1);
            var indicatedRateOfTurn = _lastIndicatedRateOfTurn;
            const float minIncrement = 0.1f;
            while (medianRateOfTurn < indicatedRateOfTurn - minIncrement)
            {
                indicatedRateOfTurn -= minIncrement;
            }
            while (medianRateOfTurn > indicatedRateOfTurn + minIncrement)
            {
                indicatedRateOfTurn += minIncrement;
            }

            if (Math.Round(medianRateOfTurn, 1) == 0)
            {
                indicatedRateOfTurn = 0;
            }
            else if (medianRateOfTurn == indicatedRateOfTurn - minIncrement)
            {
                indicatedRateOfTurn = medianRateOfTurn;
            }
            else if (medianRateOfTurn == indicatedRateOfTurn + minIncrement)
            {
                indicatedRateOfTurn = medianRateOfTurn;
            }

            _lastHeadingSample = new TimestampedFloatValue
            {
                Timestamp = curTime,
                Value = currentMagneticHeadingDecimalDegrees
            };
            _lastIndicatedRateOfTurn = LimitRateOfTurn(indicatedRateOfTurn);
            return indicatedRateOfTurn;
        }
        public void Reset()
        {
            _lastInstantaneousRatesOfTurn = new List<TimestampedFloatValue>();
            _lastIndicatedRateOfTurn = 0;
        }
        private static float LimitRateOfTurn(float rateOfTurnDegreesPerSecond)
        {
            var indicatedRateOfTurnDegreesPerSecond = rateOfTurnDegreesPerSecond;

            /*  LIMIT INDICATED RATE OF TURN TO BE WITHIN CERTAIN OUTER BOUNDARIES */
            const float maxIndicatedRateOfTurnDegreesPerSecond =
                MAX_INDICATED_RATE_OF_TURN_DECIMAL_DEGREES_PER_SECOND + 0.75f;
            const float minIndicatedRateOfTurnDegreesPerSecond = -maxIndicatedRateOfTurnDegreesPerSecond;

            if (rateOfTurnDegreesPerSecond < minIndicatedRateOfTurnDegreesPerSecond)
            {
                indicatedRateOfTurnDegreesPerSecond = minIndicatedRateOfTurnDegreesPerSecond;
            }
            else if (rateOfTurnDegreesPerSecond > maxIndicatedRateOfTurnDegreesPerSecond)
            {
                indicatedRateOfTurnDegreesPerSecond = maxIndicatedRateOfTurnDegreesPerSecond;
            }
            return indicatedRateOfTurnDegreesPerSecond;
        }
    }
}
