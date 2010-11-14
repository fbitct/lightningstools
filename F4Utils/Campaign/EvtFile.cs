using System;
using System.IO;
namespace F4Utils.Campaign
{
    public class EvtFile
    {
        #region Public Fields
        public short numEvents;
        public CampEvent[] campEvents;
        #endregion

        protected EvtFile()
            : base()
        {
        }
        public EvtFile(byte[] bytes, int version)
            : this()
        {
            int offset = 0;
            numEvents = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            campEvents = new CampEvent[numEvents];
            for (int i = 0; i < numEvents; i++)
            {
                CampEvent thisEvent = new CampEvent();
                thisEvent.id = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                thisEvent.flags = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                campEvents[i] = thisEvent;
            }
        }
    }
}