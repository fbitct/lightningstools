using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ISetFuelSelectTestEventHandler 
    {
        void SetFuelSelectTest();
    }
    class SetFuelSelectTestEventHandler : ISetFuelSelectTestEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public SetFuelSelectTestEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void SetFuelSelectTest()
        {
            _falconCallbackSender.SendCallbackToFalcon("SimFuelSwitchTest");
        }
    }
}
