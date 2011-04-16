using System.Drawing;
using F4SharedMem;

namespace MFDExtractor.Networking
{
    public interface IExtractorClient
    {
        bool IsConnected { get; }
        Image GetInstrumentImage(string instrumentName);
        FlightData GetFlightData();
        void SendMessageToServer(Message message);
        void ClearPendingMessagesToClientFromServer();
        Message GetNextMessageToClientFromServer();
    }
}