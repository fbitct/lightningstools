using F16CPD.Networking;
using F16CPD.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface IIncreaseBaroEventHandler
    {
        void IncreaseBaro();
    }
    class IncreaseBaroEventHandler : IIncreaseBaroEventHandler
    {
        private F16CpdMfdManager _mfdManager;
        private IFalconCallbackSender _falconCallbackSender;
        public IncreaseBaroEventHandler(F16CpdMfdManager mfdManager, IFalconCallbackSender falconCallbackSender = null)
        {
            _mfdManager = mfdManager;
            _falconCallbackSender = falconCallbackSender ?? new FalconCallbackSender(_mfdManager);
        }
        public void IncreaseBaro()
        {
            if (Settings.Default.RunAsClient)
            {
                var message = new Message("Falcon4IncreaseBaro", null);
                _mfdManager.Client.SendMessageToServer(message);
            }
            else
            {
                _falconCallbackSender.SendCallbackToFalcon("SimAltPressInc");
            }
        }
    }
}
