using System;
using F4Utils.Campaign.F4Structs;
using System.IO;
using System.Text;

namespace F4Utils.Campaign
{
    public class PltFile
    {
        public short NumPilots;
        public PilotInfoClass[] PilotInfo;
        public short NumCallsigns;
        public byte[] CallsignData;

        public PltFile(Stream stream, int version)
        {
            Decode(stream, version);
        }
        protected void Decode(Stream stream, int version)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                if (version < 60)
                {
                    return;
                }
                NumPilots = reader.ReadInt16();
                PilotInfo = new PilotInfoClass[NumPilots];
                for (var j = 0; j < PilotInfo.Length; j++)
                {
                    var thisPilot = new PilotInfoClass();
                    thisPilot.usage = reader.ReadInt16();
                    thisPilot.voice_id = reader.ReadByte();
                    thisPilot.photo_id = reader.ReadByte();
                }

                NumCallsigns = reader.ReadInt16();
                CallsignData = new byte[NumCallsigns];
                for (var j = 0; j < NumCallsigns; j++)
                {
                    CallsignData[j] = reader.ReadByte();
                }
            }
        }
    }
}