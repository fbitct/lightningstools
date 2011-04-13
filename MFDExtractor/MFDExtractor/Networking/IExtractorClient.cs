using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using F4SharedMem;

namespace MFDExtractor.Networking
{
    public interface IExtractorClient
    {
        Image GetInstrumentImage(string instrumentName);
        FlightData GetFlightData();
        void SendMessageToServer(Message message);
        void ClearPendingMessagesToClientFromServer();
        Message GetNextMessageToClientFromServer();
        bool IsConnected
        {
            get;
        }
    }
}
