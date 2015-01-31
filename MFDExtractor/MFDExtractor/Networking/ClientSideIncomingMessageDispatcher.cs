using MFDExtractor.EventSystem.Handlers;

namespace MFDExtractor.Networking
{
	internal interface IClientSideIncomingMessageDispatcher
	{
		void ProcessPendingMessages();
	}

	class ClientSideIncomingMessageDispatcher : IClientSideIncomingMessageDispatcher
	{
		private readonly IExtractorClient _extractorClient;
		private readonly IInputEvents _inputEvents;

		public ClientSideIncomingMessageDispatcher(
			IInputEvents inputEvents,
			IExtractorClient extractorClient)
		{
			_inputEvents = inputEvents;
			_extractorClient = extractorClient;
		}

		public void ProcessPendingMessages()
		{
			var pendingMessage = _extractorClient.GetNextMessageToClientFromServer();
			while (pendingMessage != null)
			{
				var messageType = pendingMessage.MessageType;
				switch (messageType)
				{
					case MessageType.ToggleNightMode:
						_inputEvents.NightVisionModeToggled.Handle();
						break;
					case MessageType.AirspeedIndexIncrease:
						_inputEvents.AirspeedIndexIncreasedByOne.Handle();
						break;
					case MessageType.AirspeedIndexDecrease:
						_inputEvents.AirspeedIndexDecreasedByOne.Handle();
						break;
					case MessageType.EHSILeftKnobIncrease:
						_inputEvents.EHSILeftKnobIncreasedByOne.Handle();
						break;
					case MessageType.EHSILeftKnobDecrease:
						_inputEvents.EHSILeftKnobDecreasedByOne.Handle();
						break;
					case MessageType.EHSIRightKnobIncrease:
						_inputEvents.EHSIRightKnobIncreasedByOne.Handle();
						break;
					case MessageType.EHSIRightKnobDecrease:
						_inputEvents.EHSIRightKnobDecreasedByOne.Handle();
						break;
					case MessageType.EHSIRightKnobDepressed:
						_inputEvents.EHSIRightKnobDepressed.Handle();
						break;
					case MessageType.EHSIRightKnobReleased:
						_inputEvents.EHSIRightKnobReleased.Handle();
						break;
					case MessageType.EHSIMenuButtonDepressed:
						_inputEvents.EHSIMenuButtonDepressed.Handle();
						break;
					case MessageType.AccelerometerIsReset:
						_inputEvents.AccelerometerReset.Handle();
						break;
				}
				pendingMessage = _extractorClient.GetNextMessageToClientFromServer();
			}
		}
	}
}
