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
		private readonly IInputEventHandler
			_nightVisionModeIsToggled,
			_airspeedIndexDecreasedByOne, _airspeedIndexIncreasedByOne,
			_ehsiLeftKnobDecreasedByOne, _ehsiLeftKnobIncreasedByOne,
			_ehsiRightKnobDecreasedByOne, _ehsiRightKnobIncreasedByOne,
			_ehsiRightKnobDepressed, _ehsiRightKnobReleased,
			_ehsiMenuButtonDepressed,
			_accelerometerIsReset;

		public ClientSideIncomingMessageDispatcher(
			IInputEventHandler nightVisionModeIsToggled, 
			IInputEventHandler airspeedIndexDecreasedByOne, IInputEventHandler airspeedIndexIncreasedByOne, 
			IInputEventHandler ehsiLeftKnobDecreasedByOne, IInputEventHandler ehsiLeftKnobIncreasedByOne, 
			IInputEventHandler ehsiRightKnobDecreasedByOne, IInputEventHandler ehsiRightKnobIncreasedByOne, 
			IInputEventHandler ehsiRightKnobDepressed, IInputEventHandler ehsiRightKnobReleased, 
			IInputEventHandler ehsiMenuButtonDepressed, 
			IInputEventHandler accelerometerIsReset, 
			IExtractorClient extractorClient)
		{
			_nightVisionModeIsToggled = nightVisionModeIsToggled;
			_airspeedIndexDecreasedByOne = airspeedIndexDecreasedByOne;
			_airspeedIndexIncreasedByOne = airspeedIndexIncreasedByOne;
			_ehsiLeftKnobDecreasedByOne = ehsiLeftKnobDecreasedByOne;
			_ehsiLeftKnobIncreasedByOne = ehsiLeftKnobIncreasedByOne;
			_ehsiRightKnobDecreasedByOne = ehsiRightKnobDecreasedByOne;
			_ehsiRightKnobIncreasedByOne = ehsiRightKnobIncreasedByOne;
			_ehsiRightKnobDepressed = ehsiRightKnobDepressed;
			_ehsiRightKnobReleased = ehsiRightKnobReleased;
			_ehsiMenuButtonDepressed = ehsiMenuButtonDepressed;
			_accelerometerIsReset = accelerometerIsReset;
			_extractorClient = extractorClient;
		}

		public void ProcessPendingMessages()
		{
			Message pendingMessage = _extractorClient.GetNextMessageToClientFromServer();
			while (pendingMessage != null)
			{
				var messageType = (MessageTypes)Enum.Parse(typeof(MessageTypes), pendingMessage.MessageType);
				switch (messageType)
				{
					case MessageTypes.ToggleNightMode:
						_nightVisionModeIsToggled.Raise();
						break;
					case MessageTypes.AirspeedIndexIncrease:
						_airspeedIndexIncreasedByOne.Raise();
						break;
					case MessageTypes.AirspeedIndexDecrease:
						_airspeedIndexDecreasedByOne.Raise();
						break;
					case MessageTypes.EHSILeftKnobIncrease:
						_ehsiLeftKnobIncreasedByOne.Raise();
						break;
					case MessageTypes.EHSILeftKnobDecrease:
						_ehsiLeftKnobDecreasedByOne.Raise();
						break;
					case MessageTypes.EHSIRightKnobIncrease:
						_ehsiRightKnobIncreasedByOne.Raise();
						break;
					case MessageTypes.EHSIRightKnobDecrease:
						_ehsiRightKnobDecreasedByOne.Raise();
						break;
					case MessageTypes.EHSIRightKnobDepressed:
						_ehsiRightKnobDepressed.Raise();
						break;
					case MessageTypes.EHSIRightKnobReleased:
						_ehsiRightKnobReleased.Raise();
						break;
					case MessageTypes.EHSIMenuButtonDepressed:
						_ehsiMenuButtonDepressed.Raise();
						break;
					case MessageTypes.AccelerometerIsReset:
						_accelerometerIsReset.Raise();
						break;
				}
				pendingMessage = _extractorClient.GetNextMessageToClientFromServer();
			}
		}
	}
}
