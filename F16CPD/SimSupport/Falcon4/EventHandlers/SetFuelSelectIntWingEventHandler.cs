using F4SharedMem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ISetFuelSelectIntWingEventHandler
    {
        void SetFuelSelectIntWing();
    }
    class SetFuelSelectIntWingEventHandler : ISetFuelSelectIntWingEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public SetFuelSelectIntWingEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void SetFuelSelectIntWing()
        {
            _falconCallbackSender.SendCallbackToFalcon("SimFuelSwitchWingInt");
        }
    }
}
