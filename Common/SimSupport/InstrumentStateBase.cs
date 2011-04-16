using System;

namespace Common.SimSupport
{
    [Serializable]
    public abstract class InstrumentStateBase
    {
        public override string ToString()
        {
            return Serialization.Util.ToRawBytes(this);
        }

        public override bool Equals(object obj)
        {
            return (
                       obj != null &&
                       obj is InstrumentStateBase &&
                       obj.GetHashCode() == GetHashCode()
                   );
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}