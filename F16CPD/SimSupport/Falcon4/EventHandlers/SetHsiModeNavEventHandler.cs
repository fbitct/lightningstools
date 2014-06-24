using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ISetHsiModeNavEventHandler
    {
        void SetHsiModeNav();
    }
    class SetHsiModeNavEventHandler : ISetHsiModeNavEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public SetHsiModeNavEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void SetHsiModeNav()
        {
            _falconCallbackSender.SendCallbackToFalcon("SimHSINav");
        }
    }
}
