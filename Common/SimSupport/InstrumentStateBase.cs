using System;
using System.Collections.Generic;
using System.Text;

namespace Common.SimSupport
{
    [Serializable]
    public abstract class InstrumentStateBase
    {
        public override string ToString()
        {
            return Common.Serialization.Util.ToRawBytes(this);
        }
        public override bool Equals(object obj)
        {
            return (
                obj != null &&
                obj is InstrumentStateBase &&
                obj.GetHashCode() == this.GetHashCode()
            );
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}
