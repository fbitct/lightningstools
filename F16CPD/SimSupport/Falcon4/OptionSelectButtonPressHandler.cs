using F16CPD.Mfd.Controls;
using F16CPD.SimSupport.Falcon4.EventHandlers;
using F4KeyFile;
using F4SharedMem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4
{
    internal interface IOptionSelectButtonPressHandler
    {
        void HandleOptionSelectButtonPress(OptionSelectButton button);
    }
    class OptionSelectButtonPressHandler:IOptionSelectButtonPressHandler
    {
        private F16CpdMfdManager _mfdManager;
        private IFalconCallbackSender _falconCallbackSender;
        private ICourseSelectIncreaseEventHandler _courseSelectIncreaseEventHandler;
        private ICourseSelectDecreaseEventHandler _courseSelectDecreaseEventHandler;
        private IHeadingSelectIncreaseEventHandler _headingSelectIncreaseEventHandler;
        private IHeadingSelectDecreaseEventHandler _headingSelectDecreaseEventHandler;
        private IIncreaseBaroEventHandler _increaseBaroEventHandler;
        private IDecreaseBaroEventHandler _decreaseBaroEventHandler;
        private IIncreaseAlowEventHandler _increaseAlowEventHandler;
        private IDecreaseAlowEventHandler _decreaseAlowEventHandler;
        public OptionSelectButtonPressHandler(
            F16CpdMfdManager mfdManager,
            IFalconCallbackSender falconCallbackSender=null,
            ICourseSelectIncreaseEventHandler courseSelectIncreaseEventHandler=null,
            ICourseSelectDecreaseEventHandler courseSelectDecreaseEventHandler=null,
            IHeadingSelectIncreaseEventHandler headingSelectIncreaseEventHandler=null,
            IHeadingSelectDecreaseEventHandler headingSelectDecreaseEventHandler =null,
            IIncreaseBaroEventHandler increaseBaroEventHandler=null,
            IDecreaseBaroEventHandler decreaseBaroEventHandler=null,
            IIncreaseAlowEventHandler increaseAlowEventHandler=null,
            IDecreaseAlowEventHandler decreaseAlowEventHandler=null)
        {
            _mfdManager = mfdManager;
            _falconCallbackSender = falconCallbackSender ?? new FalconCallbackSender(_mfdManager);
            _courseSelectIncreaseEventHandler=courseSelectIncreaseEventHandler ?? new CourseSelectIncreaseEventHandler(_falconCallbackSender);
            _courseSelectDecreaseEventHandler = courseSelectDecreaseEventHandler ?? new CourseSelectDecreaseEventHandler(_falconCallbackSender);
            _headingSelectIncreaseEventHandler = headingSelectIncreaseEventHandler ?? new HeadingSelectIncreaseEventHandler(_falconCallbackSender);
            _headingSelectDecreaseEventHandler = headingSelectDecreaseEventHandler ?? new HeadingSelectDecreaseEventHandler(_falconCallbackSender);
            _increaseBaroEventHandler = increaseBaroEventHandler ?? new IncreaseBaroEventHandler(_mfdManager, _falconCallbackSender);
            _decreaseBaroEventHandler = decreaseBaroEventHandler ?? new DecreaseBaroEventHandler(_mfdManager, _falconCallbackSender);
            _increaseAlowEventHandler = increaseAlowEventHandler ?? new IncreaseAlowEventHandler(_mfdManager, _falconCallbackSender);
            _decreaseAlowEventHandler = decreaseAlowEventHandler ?? new DecreaseAlowEventHandler(_mfdManager, _falconCallbackSender);
        }
        public void HandleOptionSelectButtonPress(OptionSelectButton button)
        {
            var functionName = button.FunctionName;
            if (!String.IsNullOrEmpty(functionName))
            {
                switch (functionName)
                {
                    case "CourseSelectIncrease":
                        _courseSelectIncreaseEventHandler.CourseSelectIncrease();
                        break;
                    case "CourseSelectDecrease":
                        _courseSelectDecreaseEventHandler.CourseSelectDecrease();
                        break;
                    case "HeadingSelectIncrease":
                        _headingSelectIncreaseEventHandler.HeadingSelectIncrease();
                        break;
                    case "HeadingSelectDecrease":
                        _headingSelectDecreaseEventHandler.HeadingSelectDecrease();
                        break;
                    case "BarometricPressureSettingIncrease":
                        _increaseBaroEventHandler.IncreaseBaro();
                        break;
                    case "BarometricPressureSettingDecrease":
                        _decreaseBaroEventHandler.DecreaseBaro();
                        break;
                    case "LowAltitudeWarningThresholdIncrease":
                        _increaseAlowEventHandler.IncreaseAlow();
                        break;
                    case "LowAltitudeWarningThresholdDecrease":
                        _decreaseAlowEventHandler.DecreaseAlow();
                        break;
                    case "AcknowledgeMessage":
                        //SendCallbackToFalcon("SimICPFAck");
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
