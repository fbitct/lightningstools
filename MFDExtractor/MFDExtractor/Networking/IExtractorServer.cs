using F4SharedMem;

namespace MFDExtractor.Networking
{
    public interface IExtractorServer
    {
        byte[] GetInstrumentImagesSpriteBytes();
        FlightData GetFlightData();
        void SubmitMessageToServerFromClient(Message message);
        void ClearPendingMessagesToClientFromServer();
        Message GetNextPendingMessageToClientFromServer();
        bool TestConnection();
    }
}