using System;
using System.Collections.Generic;
using System.Text;

namespace Common.InputSupport.Phcc
{
    public struct PHCCInputState: ICloneable
    {
        public bool[] digitalInputs;
        public short[] analogInputs;
        public PHCCInputState(bool[] digitalInputs, short[] analogInputs):this()
        {
            this.digitalInputs = digitalInputs;
            this.analogInputs = analogInputs;
        }
        public object Clone()
        {
            PHCCInputState clone = new PHCCInputState();
            clone.digitalInputs = (bool[])this.digitalInputs.Clone();
            clone.analogInputs = (short[])this.analogInputs.Clone();
            return clone;
        }
    }
}
