using System;
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
				var messageType = (MessageTypes)Enum.Parse(typeof(MessageTypes), pendingMessage.MessageType);
				switch (messageType)
				{
					case MessageTypes.ToggleNightMode:
						_inputEvents.NightVisionModeToggled.Handle();
						break;
					case MessageTypes.AirspeedIndexIncrease:
						_inputEvents.AirspeedIndexIncreasedByOne.Handle();
						break;
					case MessageTypes.AirspeedIndexDecrease:
						_inputEvents.AirspeedIndexDecreasedByOne.Handle();
						break;
					case MessageTypes.EHSILeftKnobIncrease:
						_inputEvents.EHSILeftKnobIncreasedByOne.Handle();
						break;
					case MessageTypes.EHSILeftKnobDecrease:
						_inputEvents.EHSILeftKnobDecreasedByOne.Handle();
						break;
					case MessageTypes.EHSIRightKnobIncrease:
						_inputEvents.EHSIRightKnobIncreasedByOne.Handle();
						break;
					case MessageTypes.EHSIRightKnobDecrease:
						_inputEvents.EHSIRightKnobDecreasedByOne.Handle();
						break;
					case MessageTypes.EHSIRightKnobDepressed:
						_inputEvents.EHSIRightKnobDepressed.Handle();
						break;
					case MessageTypes.EHSIRightKnobReleased:
						_inputEvents.EHSIRightKnobReleased.Handle();
						break;
					case MessageTypes.EHSIMenuButtonDepressed:
						_inputEvents.EHSIMenuButtonDepressed.Handle();
						break;
					case MessageTypes.AccelerometerIsReset:
						_inputEvents.AccelerometerReset.Handle();
						break;
				}
				pendingMessage = _extractorClient.GetNextMessageToClientFromServer();
			}
		}
	}
}
