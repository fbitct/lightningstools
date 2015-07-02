using System;
using F4Utils.Campaign.F4Structs;
using System.IO;
using System.Text;

namespace F4Utils.Campaign
{
    public class EvtFile
    {
        #region Public Fields
        public short numEvents;
        public CampEvent[] campEvents;
        #endregion

        public EvtFile(Stream stream, int version)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                numEvents = reader.ReadInt16();

                campEvents = new CampEvent[numEvents];
                for (int i = 0; i < numEvents; i++)
                {
                    CampEvent thisEvent = new CampEvent();
                    thisEvent.id = reader.ReadInt16();
                    thisEvent.flags = reader.ReadInt16();
                    campEvents[i] = thisEvent;
                }
            }
        }
    }
}