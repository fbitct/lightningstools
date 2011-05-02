using System;

namespace F4Utils.Campaign
{
    public class WthFile
    {
        #region Public Fields

        public uint LastCheck;
        public float Temp;
        public byte TodaysBase;
        public byte TodaysConHigh;
        public byte TodaysConLow;
        public byte TodaysTemp;
        public byte TodaysWind;
        public float WindHeading;
        public float WindSpeed;
        public CellState[] cellState;
        public float fogAltitude;
        public float fogTodaysAlt;
        public float fogTodaysMax;
        public float fogTodaysMin;
        public uint nh;
        public uint nw;
        public float spare1;
        public float spare2;
        public float spare3;
        public float visMax;
        public float visMin;
        public float visibilityPct;
        public float xOffset;
        public float yOffset;
        // -------------------

        #endregion

        private const int AF_MIN_VERSION_NUM = 80;

        protected WthFile()
        {
        }

        public WthFile(byte[] bytes, int version)
            : this()
        {
            Decode(bytes, version);
        }

        protected void Decode(byte[] bytes, int version)
        {
            var offset = 0;
            WindHeading = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            WindSpeed = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            LastCheck = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            Temp = BitConverter.ToSingle(bytes, offset);
            offset += 4;

            TodaysTemp = bytes[offset];
            offset++;

            TodaysWind = bytes[offset];
            offset++;

            TodaysBase = bytes[offset];
            offset++;

            if (version >= 27)
            {
                TodaysConLow = bytes[offset];
                offset++;

                TodaysConHigh = bytes[offset];
                offset++;
            }

            xOffset = BitConverter.ToSingle(bytes, offset);
            offset += 4;

            yOffset = BitConverter.ToSingle(bytes, offset);
            offset += 4;

            if (version >= AF_MIN_VERSION_NUM)
            {
                visMin = BitConverter.ToSingle(bytes, offset);
                offset += 4;
                visMax = BitConverter.ToSingle(bytes, offset);
                fogAltitude = BitConverter.ToSingle(bytes, offset);
                offset += 4;
                visibilityPct = BitConverter.ToSingle(bytes, offset);
                offset += 4;
                fogTodaysMax = BitConverter.ToSingle(bytes, offset);
                offset += 4;
                fogTodaysMin = BitConverter.ToSingle(bytes, offset);
                offset += 4;
                fogTodaysAlt = BitConverter.ToSingle(bytes, offset);
                offset += 4;
                spare1 = BitConverter.ToSingle(bytes, offset);
                offset += 4;
                spare2 = BitConverter.ToSingle(bytes, offset);
                offset += 4;
                spare3 = BitConverter.ToSingle(bytes, offset);
                offset += 4;
            }

            nw = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            nh = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            cellState = new CellState[nh*nw];
            for (var j = 0; j < nh*nw; j++)
            {
                var thisCellState = new CellState();
                thisCellState.BaseAltitude = bytes[offset];
                offset++;
                thisCellState.Type = bytes[offset];
                offset++;
                cellState[j] = thisCellState;
            }
        }
    }
}