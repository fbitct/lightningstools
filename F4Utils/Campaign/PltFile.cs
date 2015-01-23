using System;
namespace F4Utils.Campaign
{
    public class PltFile
    {
        #region Public Fields

        public short NumPilots;
        public PilotInfoClass[] PilotInfo;
        public short NumCallsigns;
        public byte[] CallsignData;
        #endregion

        protected PltFile()
            : base()
        {
        }
        public PltFile(byte[] bytes, int version)
            : this()
        {
            Decode(bytes, version);
        }
        protected void Decode(byte[] bytes, int version)
        {
            if (version < 60)
            {
                return;
            }
            int offset = 0;
            NumPilots = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            PilotInfo = new PilotInfoClass[NumPilots];
            for (int j = 0; j < PilotInfo.Length; j++)
            {
                PilotInfoClass thisPilot = new PilotInfoClass();
                thisPilot.usage = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                thisPilot.voice_id = bytes[offset];
                offset++;
                thisPilot.photo_id = bytes[offset];
                offset++;
            }

            NumCallsigns = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            CallsignData = new byte[NumCallsigns];
            for (int j = 0; j < NumCallsigns; j++)
            {
                CallsignData[j] = bytes[offset];
                offset++;
            }
        }
    }
}