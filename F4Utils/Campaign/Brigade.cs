using System;
using F4Utils.Campaign.F4Structs;

namespace F4Utils.Campaign
{

    public class Brigade : GroundUnit
    {
        #region Public Fields
        public VU_ID[] element;
        public byte elements;
        #endregion

        protected Brigade()
            : base()
        {
        }
        public Brigade(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            elements = bytes[offset];
            offset++;
            element = new VU_ID[elements];
            for (int i = 0; i < elements; i++)
            {
                VU_ID thisElement = new VU_ID();
                thisElement.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                thisElement.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                element[i] = thisElement;
            }

        }
    }
}