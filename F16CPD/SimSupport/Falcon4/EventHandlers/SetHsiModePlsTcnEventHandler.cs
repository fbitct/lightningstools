using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ISetHsiModePlsTcnEventHandler
    {
        void SetHsiModePlsTcn();
    }
    class SetHsiModePlsTcnEventHandler : ISetHsiModePlsTcnEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public SetHsiModePlsTcnEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void SetHsiModePlsTcn()
        {
            _falconCallbackSender.SendCallbackToFalcon("SimHSIIlsTcn");
        }
    }
}
