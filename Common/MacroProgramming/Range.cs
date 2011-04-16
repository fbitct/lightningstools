using System;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Range : DataPoint
    {
        private double _lowerInclusiveBound;
        private double _upperInclusiveBound;

        public double Width
        {
            get { return System.Math.Abs(_upperInclusiveBound) - System.Math.Abs(_lowerInclusiveBound); }
        }

        public double CenterPoint
        {
            get { return (Width/2) + _lowerInclusiveBound; }
        }

        public double LowerInclusiveBound
        {
            get { return _lowerInclusiveBound; }
            set
            {
                if (_lowerInclusiveBound > _upperInclusiveBound)
                {
                    double oldUpperBound = _upperInclusiveBound;
                    _upperInclusiveBound = value;
                    _lowerInclusiveBound = oldUpperBound;
                }
                else
                {
                    _lowerInclusiveBound = value;
                }
            }
        }

        public double UpperInclusiveBound
        {
            get { return _upperInclusiveBound; }
            set
            {
                if (UpperInclusiveBound < LowerInclusiveBound)
                {
                    double oldLowerBound = _lowerInclusiveBound;
                    _lowerInclusiveBound = value;
                    _upperInclusiveBound = oldLowerBound;
                }
                else
                {
                    _upperInclusiveBound = value;
                }
            }
        }
    }
}