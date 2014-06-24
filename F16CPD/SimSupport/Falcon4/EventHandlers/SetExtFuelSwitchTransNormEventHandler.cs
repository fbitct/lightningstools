using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ISetExtFuelSwitchTransNormEventHandler
    {
        void SetExtFuelSwitchTransNorm();
    }
    class SetExtFuelSwitchTransNormEventHandler : ISetExtFuelSwitchTransNormEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public SetExtFuelSwitchTransNormEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void SetExtFuelSwitchTransNorm()
        {
            _falconCallbackSender.SendCallbackToFalcon("SimFuelTransNorm");
        }
    }
}
