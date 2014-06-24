using F16CPD.Networking;
using F16CPD.Properties;
using F16CPD.SimSupport.Falcon4.EventHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.Networking
{
    internal interface IServerSideInboundMessageProcessor
    {
        bool ProcessPendingMessage(Message message);
    }
    class ServerSideInboundMessageProcessor : IServerSideInboundMessageProcessor
    {
        private F16CpdMfdManager _mfdManager;
        private IFalconCallbackSender _falconCallbackSender;
        private IIncreaseAlowEventHandler _increaseAlowEventHandler;
        private IDecreaseAlowEventHandler _decreaseAlowEventHandler;
        private IIncreaseBaroEventHandler _increaseBaroEventHandler;
        private IDecreaseBaroEventHandler _decreaseBaroEventHandler;
        public ServerSideInboundMessageProcessor(
            F16CpdMfdManager mfdManager,
            IFalconCallbackSender falconCallbackSender=null, 
            IIncreaseAlowEventHandler increaseAlowEventHandler=null,
            IDecreaseAlowEventHandler decreaseAlowEventHandler=null,
            IIncreaseBaroEventHandler increaseBaroEventHandler=null,
            IDecreaseBaroEventHandler decreaseBaroEventHandler=null)
        {
            _mfdManager = mfdManager;
            _falconCallbackSender = falconCallbackSender ?? new FalconCallbackSender(_mfdManager);
            _increaseAlowEventHandler = increaseAlowEventHandler ?? new IncreaseAlowEventHandler(_mfdManager, _falconCallbackSender);
            _decreaseAlowEventHandler = decreaseAlowEventHandler ?? new DecreaseAlowEventHandler(_mfdManager, _falconCallbackSender);
            _increaseBaroEventHandler = increaseBaroEventHandler ?? new IncreaseBaroEventHandler(_mfdManager,_falconCallbackSender);
            _decreaseBaroEventHandler = decreaseBaroEventHandler ?? new DecreaseBaroEventHandler(_mfdManager, _falconCallbackSender);

        }
        public bool ProcessPendingMessage(Message message)
        {
            if (!Settings.Default.RunAsServer) return false;
            var toReturn = false;
            if (message != null)
            {
                var messageType = message.MessageType;
                if (messageType != null)
                {
                    switch (messageType)
                    {
                        case "Falcon4SendCallbackMessage":
                            var callback = (string)message.Payload;
                            _falconCallbackSender.SendCallbackToFalcon(callback);
                            toReturn = true;
                            break;
                        case "Falcon4IncreaseALOW":
                            {
                                _increaseAlowEventHandler.IncreaseAlow();
                                toReturn = true;
                            }
                            break;

                        case "Falcon4DecreaseALOW":
                            {
                                _decreaseAlowEventHandler.DecreaseAlow();
                                toReturn = true;
                            }
                            break;
                        case "Falcon4IncreaseBaro":
                            {
                                _increaseBaroEventHandler.IncreaseBaro();
                                toReturn = true;
                            }
                            break;

                        case "Falcon4DecreaseBaro":
                            {
                                _decreaseBaroEventHandler.DecreaseBaro();
                                toReturn = true;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return toReturn;
        }
    }
}
