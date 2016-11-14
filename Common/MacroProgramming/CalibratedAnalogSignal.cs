using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.MacroProgramming
{
    [Serializable]
    public class CalibratedAnalogSignal : AnalogSignal
    {
        [XmlArray("CalibrationData")]
        [XmlArrayItem("CalibrationPoint")]
        public List<CalibrationPoint> CalibrationData { get; set; }

        [XmlIgnore]
        public override double State
        {
            get
            {
                return base.State;
            }

            set
            {
                base.State = Calibrate(value);
            }
        }
        private double Calibrate(double value)
        {
            if (CalibrationData == null) return value;
            var lowerPoint = CalibrationData.OrderBy(x=>x.Input).Where(x => x.Input <= value).LastOrDefault() ?? new CalibrationPoint(MinValue, MinValue);
            var upperPoint = CalibrationData.OrderBy(x => x.Input).Where(x => x != lowerPoint && x.Input >= lowerPoint.Input).FirstOrDefault() ?? new CalibrationPoint(MaxValue, MaxValue);
            var inputRange = System.Math.Abs(upperPoint.Input - lowerPoint.Input);
            var outputRange = System.Math.Abs(upperPoint.Output - lowerPoint.Output);
            var pct = inputRange != 0
                ? (value - lowerPoint.Input) / inputRange
                : 1.00;
            return (pct * outputRange) + lowerPoint.Output;
        }
    }
}
