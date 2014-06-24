using F16CPD.Mfd.Controls;
using F16CPD.SimSupport.Falcon4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface IInputControlEventHandler
    {
        void HandleInputControlEvent(CpdInputControls eventSource, MfdInputControl control);
    }
    class InputControlEventHandler:IInputControlEventHandler
    {
        private F16CpdMfdManager _mfdManager;
        private IFalconCallbackSender _falconCallbackSender;
        private IOptionSelectButtonPressHandler _optionSelectButtonPressHandler;
        private ISetHsiModeTcnEventHandler _setHsiModeTcnEventHandler;
        private ISetHsiModePlsNavEventHandler _setHsiModePlsNavEventHandler;
        private ISetHsiModePlsTcnEventHandler _setHsiModePlsTcnEventHandler;
        private ISetHsiModeNavEventHandler _setHsiModeNavEventHandler;
        private ISetFuelSelectNormEventHandler _setFuelSelectNormEventHandler;
        private ISetFuelSelectTestEventHandler _setFuelSelectTestEventHandler;
        private ISetFuelSelectRsvrEventHandler _setFuelSelectRsvrEventHandler;
        private ISetFuelSelectIntWingEventHandler _setFuelSelectIntWingEventHandler;
        private ISetFuelSelectExtWingEventHandler _setFuelSelectExtWingEventHandler;
        private ISetFuelSelectExtCtrEventHandler _setFuelSelectExtCtrEventHandler;
        private ISetExtFuelSwitchTransNormEventHandler _setExtFuelSwitchTransNormEventHandler;
        private ISetExtFuelSwitchTransWingFirstEventHandler _setExtFuelSwitchTransWingFirstEventHandler;
        public InputControlEventHandler(
            F16CpdMfdManager mfdManager,
            IFalconCallbackSender falconCallbackSender = null,
            IOptionSelectButtonPressHandler optionSelectButtonPressHandler = null,
            ISetHsiModeTcnEventHandler setHsiModeTcnEventHandler = null,
            ISetHsiModePlsNavEventHandler setHsiModePlsNavEventHandler = null,
            ISetHsiModePlsTcnEventHandler setHsiModePlsTcnEventHandler = null,
            ISetHsiModeNavEventHandler setHsiModeNavEventHandler=null,
            ISetFuelSelectNormEventHandler setFuelSelectNormEventHandler=null,
            ISetFuelSelectTestEventHandler setFuelSelectTestEventHandler=null,
            ISetFuelSelectRsvrEventHandler setFuelSelectRsvrEventHandler=null,
            ISetFuelSelectIntWingEventHandler setFuelSelectIntWingEventHandler=null,
            ISetFuelSelectExtWingEventHandler setFuelSelectExtWingEventHandler=null,
            ISetFuelSelectExtCtrEventHandler setFuelSelectExtCtrEventHandler=null,
            ISetExtFuelSwitchTransNormEventHandler setExtFuelSwitchTransNormEventHandler=null,
            ISetExtFuelSwitchTransWingFirstEventHandler setExtFuelSwitchTransWingFirstEventHandler=null
            )
        {
            _mfdManager = mfdManager;
            _falconCallbackSender = falconCallbackSender ?? new FalconCallbackSender (_mfdManager);
            _optionSelectButtonPressHandler = optionSelectButtonPressHandler ?? new OptionSelectButtonPressHandler(_mfdManager, _falconCallbackSender);
            _setHsiModeTcnEventHandler = setHsiModeTcnEventHandler ?? new SetHsiModeTcnEventHandler(_falconCallbackSender);
            _setHsiModePlsNavEventHandler = setHsiModePlsNavEventHandler ?? new SetHsiModePlsNavEventHandler(_falconCallbackSender);
            _setHsiModePlsTcnEventHandler = setHsiModePlsTcnEventHandler ?? new SetHsiModePlsTcnEventHandler(_falconCallbackSender);
            _setHsiModeNavEventHandler = setHsiModeNavEventHandler ?? new SetHsiModeNavEventHandler(_falconCallbackSender);
            _setFuelSelectNormEventHandler = setFuelSelectNormEventHandler ?? new SetFuelSelectNormEventHandler(_falconCallbackSender);
            _setFuelSelectTestEventHandler = setFuelSelectTestEventHandler ?? new SetFuelSelectTestEventHandler(_falconCallbackSender);
            _setFuelSelectRsvrEventHandler = setFuelSelectRsvrEventHandler ?? new SetFuelSelectRsvrEventHandler(_falconCallbackSender);
            _setFuelSelectIntWingEventHandler = setFuelSelectIntWingEventHandler ?? new SetFuelSelectIntWingEventHandler(_falconCallbackSender);
            _setFuelSelectExtWingEventHandler = setFuelSelectExtWingEventHandler ?? new SetFuelSelectExtWingEventHandler(_falconCallbackSender);
            _setFuelSelectExtCtrEventHandler = setFuelSelectExtCtrEventHandler ?? new SetFuelSelectExtCtrEventHandler(_falconCallbackSender);
            _setExtFuelSwitchTransNormEventHandler = setExtFuelSwitchTransNormEventHandler ?? new SetExtFuelSwitchTransNormEventHandler(_falconCallbackSender);
            _setExtFuelSwitchTransWingFirstEventHandler = setExtFuelSwitchTransWingFirstEventHandler ?? new SetExtFuelSwitchTransWingFirstEventHandler(_falconCallbackSender);
        }
        public void HandleInputControlEvent(CpdInputControls eventSource, MfdInputControl control)
        {
            OptionSelectButton button;
            switch (eventSource)
            {
                case CpdInputControls.OsbButton1:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton2:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton3:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton4:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton5:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton6:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton7:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton8:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton9:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton10:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton11:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton12:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton13:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton14:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton15:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton16:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton17:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton18:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton19:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton20:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton21:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton22:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton23:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton24:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton25:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton26:
                    button = control as OptionSelectButton;
                    _optionSelectButtonPressHandler.HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.HsiModeTcn:
                    _setHsiModeTcnEventHandler.SetHsiModeTcn();
                    break;
                case CpdInputControls.HsiModeIlsTcn:
                    _setHsiModePlsTcnEventHandler.SetHsiModePlsTcn();
                    break;
                case CpdInputControls.HsiModeNav:
                    _setHsiModeNavEventHandler.SetHsiModeNav();
                    break;
                case CpdInputControls.HsiModeIlsNav:
                    _setHsiModePlsNavEventHandler.SetHsiModePlsNav();
                    break;
                case CpdInputControls.ParameterAdjustKnobIncrease:
                    break;
                case CpdInputControls.ParameterAdjustKnobDecrease:
                    break;
                case CpdInputControls.FuelSelectTest:
                    _setFuelSelectTestEventHandler.SetFuelSelectTest();
                    break;
                case CpdInputControls.FuelSelectNorm:
                    _setFuelSelectNormEventHandler.SetFuelSelectNorm();
                    break;
                case CpdInputControls.FuelSelectRsvr:
                    _setFuelSelectRsvrEventHandler.SetFuelSelectRsvr();
                    break;
                case CpdInputControls.FuelSelectIntWing:
                    _setFuelSelectIntWingEventHandler.SetFuelSelectIntWing();
                    break;
                case CpdInputControls.FuelSelectExtWing:
                    _setFuelSelectExtWingEventHandler.SetFuelSelectExtWing();
                    break;
                case CpdInputControls.FuelSelectExtCtr:
                    _setFuelSelectExtCtrEventHandler.SetFuelSelectExtCtr();
                    break;
                case CpdInputControls.ExtFuelSwitchTransNorm:
                    _setExtFuelSwitchTransNormEventHandler.SetExtFuelSwitchTransNorm();
                    break;
                case CpdInputControls.ExtFuelSwitchTransWingFirst:
                    _setExtFuelSwitchTransWingFirstEventHandler.SetExtFuelSwitchTransWingFirst();
                    break;
            }
        }
    }
}
