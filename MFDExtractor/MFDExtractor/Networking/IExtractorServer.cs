using System.Drawing;
using F4SharedMem;

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