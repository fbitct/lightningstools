using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ISetFuelSelectRsvrEventHandler
    {
        void SetFuelSelectRsvr();
    }
    class SetFuelSelectRsvrEventHandler : ISetFuelSelectRsvrEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public SetFuelSelectRsvrEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void SetFuelSelectRsvr()
        {
            _falconCallbackSender.SendCallbackToFalcon("SimFuelSwitchResv");
        }
    }
}
