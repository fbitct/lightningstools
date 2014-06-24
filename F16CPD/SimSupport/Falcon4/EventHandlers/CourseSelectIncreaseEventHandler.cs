using F4KeyFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface ICourseSelectIncreaseEventHandler
    {
        void CourseSelectIncrease();
    }
    class CourseSelectIncreaseEventHandler:ICourseSelectIncreaseEventHandler
    {
        private IFalconCallbackSender _falconCallbackSender;
        public CourseSelectIncreaseEventHandler(IFalconCallbackSender falconCallbackSender)
        {
            _falconCallbackSender = falconCallbackSender;
        }
        public void CourseSelectIncrease()
        {
            var useIncrementByOne = false;
            KeyBinding incByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiCrsIncBy1");
            if (incByOneCallback != null &&
                incByOneCallback.Key.ScanCode != (int)ScanCodes.NotAssigned)
            {
                useIncrementByOne = true;
            }
            _falconCallbackSender.SendCallbackToFalcon(useIncrementByOne ? "SimHsiCrsIncBy1" : "SimHsiCourseInc");
        }
    }
}
