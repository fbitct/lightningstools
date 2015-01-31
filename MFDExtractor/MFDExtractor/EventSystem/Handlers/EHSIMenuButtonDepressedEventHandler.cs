using Common.Networking;
using F4Utils.Process;
using LightningGauges.Renderers.F16.EHSI;
using MFDExtractor.Networking;

namespace MFDExtractor.EventSystem.Handlers
{
	internal interface IEHSIMenuButtonDepressedEventHandler:IInputEventHandlerEventHandler{}
	internal class EHSIMenuButtonDepressedEventHandler : IEHSIMenuButtonDepressedEventHandler
	{
		private readonly IEHSIStateTracker _ehsiStateTracker;
		public EHSIMenuButtonDepressedEventHandler(IEHSIStateTracker ehsiStateTracker)
		{
			_ehsiStateTracker = ehsiStateTracker;
		}

		public void Handle(bool forwardEvent)
		{
		    var ehsi = _ehsiStateTracker.EHSI;
		    var currentMode = ehsi.InstrumentState.InstrumentMode;
		    InstrumentModes? newMode = null;
		    switch (currentMode)
		    {
		        case InstrumentModes.Unknown:
		            break;
		        case InstrumentModes.PlsTacan:
		            newMode = InstrumentModes.Nav;
		            break;
		        case InstrumentModes.Tacan:
		            newMode = InstrumentModes.PlsTacan;
		            break;
		        case InstrumentModes.Nav:
		            newMode = InstrumentModes.PlsNav;
		            break;
		        case InstrumentModes.PlsNav:
		            newMode = InstrumentModes.Tacan;
		            break;
		    }
		    if (newMode.HasValue)
		    {
		        ehsi.InstrumentState.InstrumentMode = newMode.Value;
		    }

		    if (Extractor.State.NetworkMode != NetworkMode.Client)
		    {
		        KeyFileUtils.SendCallbackToFalcon("SimStepHSIMode");
		    }
		    if (!forwardEvent) return;

            switch (Extractor.State.NetworkMode)
            {
                case NetworkMode.Client:
                    ExtractorClient.SendMessageToServer(new Message(MessageType.EHSIMenuButtonDepressed));
                    break;
                case NetworkMode.Server:
                    ExtractorServer.SubmitMessageToClientFromServer(new Message(MessageType.EHSIMenuButtonDepressed));
                    break;
            }
		}
	}
}
