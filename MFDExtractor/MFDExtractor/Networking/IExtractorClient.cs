using System.Drawing;
using F4SharedMem;

namespace MFDExtractor.Networking
{
    public interface IExtractorClient
    {
        bool IsConnected { get; }
        Image GetInstrumentImagesSprite();
        FlightData GetFlightData();
        void SendMessageToServer(Message message);
        void ClearPendingMessagesToClientFromServer();
        Message GetNextMessageToClientFromServer();
    }
}