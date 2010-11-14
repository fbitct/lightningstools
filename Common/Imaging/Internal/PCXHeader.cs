using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Imaging.Internal
{
    // Standard PCX header
    public struct PCXHead
    {
        public byte ID;
        public byte Version;
        public byte Encoding;
        public byte BitPerPixel;
        public short X1;
        public short Y1;
        public short X2;
        public short Y2;
        public short HRes;
        public short VRes;
        public byte[] ClrMap;//[16*3];
        public byte Reserved1;
        public byte NumPlanes;
        public short BPL;
        public short Pal_t;
        public byte[] Filler;//[58];
    }
}
