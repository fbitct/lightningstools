using System;
using System.Globalization;
using System.IO;

namespace AnalogDevices
{
    public class IhxFile
    {
        private readonly short[] _ihxData = new short[65536];

        public IhxFile(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                int b, len, cs, addr;
                var buf = new byte[255];
                var eof = false;
                var line = 0;

                for (var i = 0; i < _ihxData.Length; i++)
                {
                    _ihxData[i] = -1;
                }

                while (!eof)
                {
                    do
                    {
                        b = stream.ReadByte();
                        if (b < 0)
                        {
                            throw new InvalidDataException("Inexpected end of file");
                        }
                    } while (b != (byte) ':');

                    line++;

                    len = ReadHexByte(stream); // length field 
                    cs = len;

                    b = ReadHexByte(stream); // address field 
                    cs += b;
                    addr = b << 8;
                    b = ReadHexByte(stream);
                    cs += b;
                    addr |= b;

                    b = ReadHexByte(stream); // record type field
                    cs += b;

                    for (var i = 0; i < len; i++)
                    {
                        // data
                        buf[i] = (byte) ReadHexByte(stream);
                        cs += buf[i];
                    }

                    cs += ReadHexByte(stream); // checksum
                    if ((cs & 0xff) != 0)
                    {
                        throw new InvalidDataException("Checksum error");
                    }

                    if (b == 0) // data record 
                    {
                        for (var i = 0; i < len; i++)
                        {
                            if (_ihxData[addr + i] >= 0)
                            {
                                Console.Error.WriteLine("Warning: Memory at position 0x" +
                                                        i.ToString("X8", CultureInfo.InvariantCulture) + " overwritten");
                            }
                            _ihxData[addr + i] = (short) (buf[i] & 255);
                        }
                    }
                    else if (b == 1) // eof record
                    {
                        eof = true;
                    }
                    else
                    {
                        throw new InvalidDataException("Invalid record type: " + b);
                    }
                }
                stream.Close();
            }
        }

        public short[] IhxData
        {
            get { return _ihxData; }
        }

        private static int ReadHexDigit(Stream stream)
        {
            var b = stream.ReadByte();
            if (b >= (byte) '0' && b <= (byte) '9') return b - (byte) '0';
            if (b >= (byte) 'a' && b <= (byte) 'f') return 10 + b - (byte) 'a';
            if (b >= (byte) 'A' && b <= (byte) 'F') return 10 + b - (byte) 'A';
            if (b == -1)
            {
                throw new InvalidDataException("Inexpected end of file");
            }
            throw new InvalidDataException("Hex digit expected: " + (char) b);
        }

        private static int ReadHexByte(Stream stream)
        {
            return (ReadHexDigit(stream) << 4) | ReadHexDigit(stream);
        }
    }
}