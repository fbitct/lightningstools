using F4SharedMem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ISetFuelSelectExtCtrEventHandler
    {
        void SetFuelSelectExtCtr();
    }
    class SetFuelSelectExtCtrEventHandler:ISetFuelSelectExtCtrEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public SetFuelSelectExtCtrEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
            
        public void SetFuelSelectExtCtr()
        {
            _falconCallbackSender.SendCallbackToFalcon("SimFuelSwitchCenterExt");
        }
    }
}
