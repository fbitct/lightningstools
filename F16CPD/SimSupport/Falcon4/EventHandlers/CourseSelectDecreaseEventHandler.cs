using F4KeyFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ICourseSelectDecreaseEventHandler
    {
        void CourseSelectDecrease();
    }
    class CourseSelectDecreaseEventHandler:ICourseSelectDecreaseEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public CourseSelectDecreaseEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void CourseSelectDecrease()
        {
            var useDecrementByOne = false;
            KeyBinding decByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiCrsDecBy1");
            if (decByOneCallback != null &&
                decByOneCallback.Key.ScanCode != (int)ScanCodes.NotAssigned)
            {
                useDecrementByOne = true;
            }
            _falconCallbackSender.SendCallbackToFalcon(useDecrementByOne ? "SimHsiCrsDecBy1" : "SimHsiCourseDec");
        }
    }
}
