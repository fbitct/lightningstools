using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ISetHsiModeTcnEventHandler
    {
        void SetHsiModeTcn();
    }
    class SetHsiModeTcnEventHandler : ISetHsiModeTcnEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public SetHsiModeTcnEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void SetHsiModeTcn()
        {
            _falconCallbackSender.SendCallbackToFalcon("SimHSITcn");
        }
    }
}
