using F4SharedMem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ISetFuelSelectExtWingEventHandler
    {
        void SetFuelSelectExtWing();
    }
    class SetFuelSelectExtWingEventHandler : ISetFuelSelectExtWingEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public SetFuelSelectExtWingEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void SetFuelSelectExtWing()
        {
            _falconCallbackSender.SendCallbackToFalcon("SimFuelSwitchWingExt");
        }
    }
}
