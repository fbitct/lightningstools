using Common.Networking;

namespace MFDExtractor.EventSystem
{
	public interface IMessagePublisher
	{
		void Publish(NetworkMode networkMode, MessageTypes messageType);
	}
	class MessagePublisher:IMessagePublisher
	{
		private readonly IExtractorClient _extractorClient;
		public MessagePublisher(IExtractorClient extractorClient)
		{
			_extractorClient = extractorClient;
		}
		public virtual void Publish(NetworkMode networkMode, MessageTypes messageType)
		{
			var msg = new Message(messageType.ToString(), null);
			switch (networkMode)
			{
				case NetworkMode.Server:
					ExtractorServer.SubmitMessageToClientFromServer(msg);
					break;
				case NetworkMode.Client:
					_extractorClient.SendMessageToServer(msg);
					break;
			}
		}
	}
}
