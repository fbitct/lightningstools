using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ISetHsiModePlsNavEventHandler
    {
        void SetHsiModePlsNav();
    }
    class SetHsiModePlsNavEventHandler:ISetHsiModePlsNavEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public SetHsiModePlsNavEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void SetHsiModePlsNav()
        {
            _falconCallbackSender.SendCallbackToFalcon("SimHSIIlsNav");
        }
    }
}
