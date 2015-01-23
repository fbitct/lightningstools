using System;
namespace F4Utils.Campaign
{
    public class GroundUnit : Unit
    {
        #region Public Fields
        public byte orders;
        public short division;
        public VU_ID aobj;
        #endregion

        protected GroundUnit()
            : base()
        {
        }
        public GroundUnit(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            orders = bytes[offset];
            offset++;
            division = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            aobj = new VU_ID();
            aobj.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            aobj.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

        }

    }
}