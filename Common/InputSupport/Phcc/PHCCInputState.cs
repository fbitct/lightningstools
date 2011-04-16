using System;

namespace Common.InputSupport.Phcc
{
    public struct PHCCInputState : ICloneable
    {
        public short[] analogInputs;
        public bool[] digitalInputs;

        public PHCCInputState(bool[] digitalInputs, short[] analogInputs) : this()
        {
            this.digitalInputs = digitalInputs;
            this.analogInputs = analogInputs;
        }

        #region ICloneable Members

        public object Clone()
        {
            var clone = new PHCCInputState();
            clone.digitalInputs = (bool[]) digitalInputs.Clone();
            clone.analogInputs = (short[]) analogInputs.Clone();
            return clone;
        }

        #endregion
    }
}