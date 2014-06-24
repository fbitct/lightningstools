using F4KeyFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface IHeadingSelectDecreaseEventHandler
    {
        void HeadingSelectDecrease();
    }
    class HeadingSelectDecreaseEventHandler : IHeadingSelectDecreaseEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public HeadingSelectDecreaseEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void HeadingSelectDecrease()
        {
            var useDecrementByOne = false;
            var decByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiHdgDecBy1");
            if (decByOneCallback != null &&
                decByOneCallback.Key.ScanCode != (int)ScanCodes.NotAssigned)
            {
                useDecrementByOne = true;
            }
            _falconCallbackSender.SendCallbackToFalcon(useDecrementByOne ? "SimHsiHdgDecBy1" : "SimHsiHeadingDec");

        }
    }
}
