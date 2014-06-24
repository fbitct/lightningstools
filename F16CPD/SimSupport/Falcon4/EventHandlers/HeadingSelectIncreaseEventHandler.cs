using F4KeyFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface IHeadingSelectIncreaseEventHandler
    {
        void HeadingSelectIncrease();
    }
    class HeadingSelectIncreaseEventHandler : IHeadingSelectIncreaseEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public HeadingSelectIncreaseEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void HeadingSelectIncrease()
        {
            var useIncrementByOne = false;
            KeyBinding incByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiHdgIncBy1");
            if (incByOneCallback != null &&
                incByOneCallback.Key.ScanCode != (int)ScanCodes.NotAssigned)
            {
                useIncrementByOne = true;
            }
            _falconCallbackSender.SendCallbackToFalcon(useIncrementByOne ? "SimHsiHdgIncBy1" : "SimHsiHeadingInc");

        }
    }
}
