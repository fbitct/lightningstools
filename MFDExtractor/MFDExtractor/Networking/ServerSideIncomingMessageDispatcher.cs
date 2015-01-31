using MFDExtractor.EventSystem.Handlers;

namespace MFDExtractor.Networking
{
	internal interface IServerSideIncomingMessageDispatcher
	{
		void ProcessPendingMessages();
	}

	class ServerSideIncomingMessageDispatcher : IServerSideIncomingMessageDispatcher
	{
		private readonly IInputEvents _inputEvents;

		public ServerSideIncomingMessageDispatcher(IInputEvents inputEvents)
		{
			_inputEvents = inputEvents;
		}

		public void ProcessPendingMessages()
		{
			var pendingMessage = ExtractorServer.GetNextPendingMessageToServerFromClient();
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
				pendingMessage = ExtractorServer.GetNextPendingMessageToServerFromClient();
			}
		}
	}
}
