using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using F4SharedMem;
using System.Drawing;

namespace MFDExtractor.Networking
{
    public interface IExtractorServer
    {
        byte[] GetInstrumentImageBytes(string instrumentName);
        FlightData GetFlightData();
        void SubmitMessageToServerFromClient(Message message);
        void SubmitMessageToClientFromServer(Message message);
        void ClearPendingMessagesToClientFromServer();
        Message GetNextPendingMessageToClientFromServer();
        Message GetNextPendingMessageToServerFromClient();
        void StoreInstrumentImage(string instrumentName, Image image);
        void StoreFlightData(FlightData flightData);
        bool TestConnection();
    }
}
