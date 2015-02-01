﻿using System;
namespace F4Utils.Campaign
{
    public class PltFile
    {
        public short NumPilots;
        public PilotInfoClass[] PilotInfo;
        public short NumCallsigns;
        public byte[] CallsignData;

        public PltFile(byte[] bytes, int version)
        {
            Decode(bytes, version);
        }
        protected void Decode(byte[] bytes, int version)
        {
            if (version < 60)
            {
                return;
            }
            var offset = 0;
            NumPilots = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            PilotInfo = new PilotInfoClass[NumPilots];
            for (var j = 0; j < PilotInfo.Length; j++)
            {
                var thisPilot = new PilotInfoClass();
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
            for (var j = 0; j < NumCallsigns; j++)
            {
                CallsignData[j] = bytes[offset];
                offset++;
            }
        }
    }
}