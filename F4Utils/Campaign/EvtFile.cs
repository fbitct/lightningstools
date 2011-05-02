using System;

namespace F4Utils.Campaign
{
    public class EvtFile
    {
        #region Public Fields

        public CampEvent[] campEvents;
        public short numEvents;

        #endregion

        protected EvtFile()
        {
        }

        public EvtFile(byte[] bytes, int version)
            : this()
        {
            var offset = 0;
            numEvents = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            campEvents = new CampEvent[numEvents];
            for (var i = 0; i < numEvents; i++)
            {
                var thisEvent = new CampEvent();
                thisEvent.id = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                thisEvent.flags = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                campEvents[i] = thisEvent;
            }
        }
    }
}