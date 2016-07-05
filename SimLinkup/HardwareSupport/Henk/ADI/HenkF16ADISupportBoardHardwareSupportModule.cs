using System;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;
using LightningGauges.Renderers.F16;
using System.Drawing;

namespace SimLinkup.HardwareSupport.Henk
{
    //Henk F-16 ADI Support Board for ARU-50/A Primary ADI
    public class HenkF16ADISupportBoardHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private const float GLIDESLOPE_DEVIATION_LIMIT_DEGREES = 1.0F;
        private const float LOCALIZER_DEVIATION_LIMIT_DEGREES = 5.0F;

        private static readonly ILog _log = LogManager.GetLogger(typeof(HenkF16ADISupportBoardHardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;

        private AnalogSignal _pitchInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _pitchInputSignalChangedEventHandler;
        private AnalogSignal _rollInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _rollInputSignalChangedEventHandler;
        private AnalogSignal _horizontalCommandBarInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _horizontalCommandBarInputSignalChangedEventHandler;
        private AnalogSignal _verticalCommandBarInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _verticalCommandBarInputSignalChangedEventHandler;
        private AnalogSignal _rateOfTurnInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _rateOfTurnInputSignalChangedEventHandler;

        private DigitalSignal _commandBarsVisibleInputSignal;
        private DigitalSignal.SignalChangedEventHandler _commandBarsVisibleInputSignalChangedEventHandler;
        private DigitalSignal _auxFlagInputSignal;
        private DigitalSignal.SignalChangedEventHandler _auxFlagInputSignalChangedEventHandler;
        private DigitalSignal _gsFlagInputSignal;
        private DigitalSignal.SignalChangedEventHandler _gsFlagInputSignalChangedEventHandler;
        private DigitalSignal _locFlagInputSignal;
        private DigitalSignal.SignalChangedEventHandler _locFlagInputSignalChangedEventHandler;
        private DigitalSignal _offFlagInputSignal;
        private DigitalSignal.SignalChangedEventHandler _offFlagInputSignalChangedEventHandler;

        private DigitalSignal _pitchAndRollEnableInputSignal;
        private DigitalSignal.SignalChangedEventHandler _pitchAndRollEnableInputSignalChangedEventHandler;
        private DigitalSignal _glideslopeIndicatorsPowerOnOffInputSignal;
        private DigitalSignal.SignalChangedEventHandler _glideslopeIndicatorsPowerOnOffInputSignalChangedEventHandler;
        private DigitalSignal _rateOfTurnAndFlagsPowerOnOffInputSignal;
        private DigitalSignal.SignalChangedEventHandler _rateOfTurnAndFlagsPowerOnOffInputSignalChangedEventHandler;


        private AnalogSignal _pitchOutputSignal;
        private AnalogSignal _rollOutputSignal;
        private AnalogSignal _horizontalCommandBarOutputSignal;
        private AnalogSignal _verticalCommandBarOutputSignal;
        private AnalogSignal _rateOfTurnOutputSignal;

        private DigitalSignal _auxFlagOutputSignal;
        private DigitalSignal _gsFlagOutputSignal;
        private DigitalSignal _locFlagOutputSignal;

        private DigitalSignal _pitchAndRollEnableOutputSignal;
        private DigitalSignal _glideslopeIndicatorsPowerOnOffOutputSignal;
        private DigitalSignal _rateOfTurnAndFlagsPowerOnOffOutputSignal;

        private IADI _renderer = new ADI();
        #endregion

        #region Constructors

        private HenkF16ADISupportBoardHardwareSupportModule()
        {
            CreateInputSignals();
            CreateInputEventHandlers();
            CreateOutputSignals();
            SetInitialOutputValues();
            RegisterForInputEvents();
        }
        private void SetInitialOutputValues ()
        {
            UpdatePitchAndRollEnableOutputValue();
            UpdatePitchOutputValues();
            UpdateRollOutputValues();
            UpdateGSPowerOutputValue();
            UpdateHorizontalGSBarOutputValues();
            UpdateVerticalGSBarOutputValues();
            UpdateRateOfTurnAndFlagsPowerOnOffOutputValue();
            UpdateRateOfTurnOutputValues();
            UpdateAuxFlagOutputValue();
            UpdateGSFlagOutputValue();
            UpdateLOCFlagOutputValue();
        }
        public override string FriendlyName
        {
            get { return "Henk F-16 ADI Support Board for ARU-50/A Primary ADI"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            return new[] { new HenkF16ADISupportBoardHardwareSupportModule() };
        }

        #endregion

        #region Virtual Method Implementations

        public override AnalogSignal[] AnalogInputs
        {
            get
            {
                return new[]
                {
                    _pitchInputSignal, _rollInputSignal, _horizontalCommandBarInputSignal, _verticalCommandBarInputSignal,
                    _rateOfTurnInputSignal
                };
            }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get
            {
                return new[] 
                {
                    _commandBarsVisibleInputSignal, _auxFlagInputSignal, _gsFlagInputSignal, _locFlagInputSignal, _offFlagInputSignal,
                    _pitchAndRollEnableInputSignal, _glideslopeIndicatorsPowerOnOffInputSignal, _rateOfTurnAndFlagsPowerOnOffInputSignal
                };
            }
        }
        public override AnalogSignal[] AnalogOutputs
        {
            get
            {
                return new[]
                {
                    _pitchOutputSignal, _rollOutputSignal, _horizontalCommandBarOutputSignal,
                    _verticalCommandBarOutputSignal, _rateOfTurnOutputSignal,
                };
            }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get
            {
                return new[] 
                {
                    _auxFlagOutputSignal, _gsFlagOutputSignal, _locFlagOutputSignal,
                    _pitchAndRollEnableOutputSignal,_glideslopeIndicatorsPowerOnOffOutputSignal, _rateOfTurnAndFlagsPowerOnOffOutputSignal
                };
            }
        }
        #endregion

        #region Signals Handling

        #region Signals Event Handling

        private void CreateInputEventHandlers()
        {
            _pitchInputSignalChangedEventHandler = pitch_InputSignalChanged;
            _rollInputSignalChangedEventHandler = roll_InputSignalChanged;
            _horizontalCommandBarInputSignalChangedEventHandler = horizontalCommandBar_InputSignalChanged;
            _verticalCommandBarInputSignalChangedEventHandler = verticalCommandBar_InputSignalChanged;
            _rateOfTurnInputSignalChangedEventHandler = rateOfTurn_InputSignalChanged;
            _auxFlagInputSignalChangedEventHandler = auxFlag_InputSignalChanged;
            _gsFlagInputSignalChangedEventHandler = gsFlag_InputSignalChanged;
            _locFlagInputSignalChangedEventHandler = locFlag_InputSignalChanged;
            _offFlagInputSignalChangedEventHandler = offFlag_InputSignalChanged;
            _commandBarsVisibleInputSignalChangedEventHandler = commandBarsVisible_InputSignalChanged;
            _pitchAndRollEnableInputSignalChangedEventHandler = pitchAndRollEnable_InputSignalChanged;
            _glideslopeIndicatorsPowerOnOffInputSignalChangedEventHandler = glideslopeIndicatorsPowerOnOff_InputSignalChanged;
            _rateOfTurnAndFlagsPowerOnOffInputSignalChangedEventHandler = rateOfTurnAndFlagsPowerOnOff_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _pitchInputSignalChangedEventHandler = null;
            _rollInputSignalChangedEventHandler = null;
            _horizontalCommandBarInputSignalChangedEventHandler = null;
            _verticalCommandBarInputSignalChangedEventHandler = null;
            _rateOfTurnInputSignalChangedEventHandler = null;
            _auxFlagInputSignalChangedEventHandler = null;
            _gsFlagInputSignalChangedEventHandler = null;
            _locFlagInputSignalChangedEventHandler = null;
            _offFlagInputSignalChangedEventHandler = null;
            _commandBarsVisibleInputSignalChangedEventHandler = null;
            _pitchAndRollEnableInputSignalChangedEventHandler = null;
            _glideslopeIndicatorsPowerOnOffInputSignalChangedEventHandler = null;
            _rateOfTurnAndFlagsPowerOnOffInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_auxFlagInputSignal != null)
            {
                _auxFlagInputSignal.SignalChanged += _auxFlagInputSignalChangedEventHandler;
            }
            if (_gsFlagInputSignal != null)
            {
                _gsFlagInputSignal.SignalChanged += _gsFlagInputSignalChangedEventHandler;
            }
            if (_locFlagInputSignal != null)
            {
                _locFlagInputSignal.SignalChanged += _locFlagInputSignalChangedEventHandler;
            }
            if (_offFlagInputSignal != null)
            {
                _offFlagInputSignal.SignalChanged += _offFlagInputSignalChangedEventHandler;
            }
            if (_commandBarsVisibleInputSignal != null)
            {
                _commandBarsVisibleInputSignal.SignalChanged += _commandBarsVisibleInputSignalChangedEventHandler;
            }
            if (_pitchAndRollEnableInputSignal != null)
            {
                _pitchAndRollEnableInputSignal.SignalChanged += _pitchAndRollEnableInputSignalChangedEventHandler;
            }
            if (_glideslopeIndicatorsPowerOnOffInputSignal !=null)
            {
                _glideslopeIndicatorsPowerOnOffInputSignal.SignalChanged += _glideslopeIndicatorsPowerOnOffInputSignalChangedEventHandler;
            }
            if (_rateOfTurnAndFlagsPowerOnOffInputSignal !=null)
            {
                _rateOfTurnAndFlagsPowerOnOffInputSignal.SignalChanged += _rateOfTurnAndFlagsPowerOnOffInputSignalChangedEventHandler;
            }
            if (_pitchInputSignal != null)
            {
                _pitchInputSignal.SignalChanged += _pitchInputSignalChangedEventHandler;
            }
            if (_rollInputSignal != null)
            {
                _rollInputSignal.SignalChanged += _rollInputSignalChangedEventHandler;
            }
            if (_horizontalCommandBarInputSignal != null)
            {
                _horizontalCommandBarInputSignal.SignalChanged += _horizontalCommandBarInputSignalChangedEventHandler;
            }
            if (_verticalCommandBarInputSignal != null)
            {
                _verticalCommandBarInputSignal.SignalChanged += _verticalCommandBarInputSignalChangedEventHandler;
            }
            if (_rateOfTurnInputSignal != null)
            {
                _rateOfTurnInputSignal.SignalChanged += _rateOfTurnInputSignalChangedEventHandler;
            }

        }

        private void UnregisterForInputEvents()
        {
            if (_auxFlagInputSignalChangedEventHandler != null && _auxFlagInputSignal != null)
            {
                try
                {
                    _auxFlagInputSignal.SignalChanged -= _auxFlagInputSignalChangedEventHandler;
                }
                catch (RemotingException){}
            }
            if (_gsFlagInputSignalChangedEventHandler != null && _gsFlagInputSignal != null)
            {
                try
                {
                    _gsFlagInputSignal.SignalChanged -= _gsFlagInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_locFlagInputSignalChangedEventHandler != null && _locFlagInputSignal != null)
            {
                try
                {
                    _locFlagInputSignal.SignalChanged -= _locFlagInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_offFlagInputSignalChangedEventHandler != null && _offFlagInputSignal != null)
            {
                try
                {
                    _offFlagInputSignal.SignalChanged -= _offFlagInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_commandBarsVisibleInputSignalChangedEventHandler != null && _commandBarsVisibleInputSignal !=null)
            {
                try
                {
                    _commandBarsVisibleInputSignal.SignalChanged -= _commandBarsVisibleInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_pitchAndRollEnableInputSignalChangedEventHandler != null && _pitchAndRollEnableInputSignal != null)
            {
                try
                {
                    _pitchAndRollEnableInputSignal.SignalChanged -= _pitchAndRollEnableInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_glideslopeIndicatorsPowerOnOffInputSignalChangedEventHandler != null && _glideslopeIndicatorsPowerOnOffInputSignal != null)
            {
                try
                {
                    _glideslopeIndicatorsPowerOnOffInputSignal.SignalChanged -= _glideslopeIndicatorsPowerOnOffInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_rateOfTurnAndFlagsPowerOnOffInputSignalChangedEventHandler != null && _rateOfTurnAndFlagsPowerOnOffInputSignal != null)
            {
                try
                {
                    _rateOfTurnAndFlagsPowerOnOffInputSignal.SignalChanged -= _rateOfTurnAndFlagsPowerOnOffInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_pitchInputSignalChangedEventHandler != null && _pitchInputSignal != null)
            {
                try
                {
                    _pitchInputSignal.SignalChanged -= _pitchInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_rollInputSignalChangedEventHandler != null && _rollInputSignal != null)
            {
                try
                {
                    _rollInputSignal.SignalChanged -= _rollInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_horizontalCommandBarInputSignalChangedEventHandler != null && _horizontalCommandBarInputSignal != null)
            {
                try
                {
                    _horizontalCommandBarInputSignal.SignalChanged -=
                        _horizontalCommandBarInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_verticalCommandBarInputSignalChangedEventHandler != null && _verticalCommandBarInputSignal != null)
            {
                try
                {
                    _verticalCommandBarInputSignal.SignalChanged -= _verticalCommandBarInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
            if (_rateOfTurnInputSignalChangedEventHandler != null && _rateOfTurnInputSignal != null)
            {
                try
                {
                    _rateOfTurnInputSignal.SignalChanged -= _rateOfTurnInputSignalChangedEventHandler;
                }
                catch (RemotingException) { }
            }
        }

        #endregion
        #region Visualization
        public override void Render(Graphics g, Rectangle destinationRectangle)
        {
            _renderer.InstrumentState.AuxFlag = _auxFlagInputSignal.State;
            _renderer.InstrumentState.GlideslopeDeviationDegrees = (float)_horizontalCommandBarInputSignal.State;
            _renderer.InstrumentState.GlideslopeDeviationLimitDegrees = GLIDESLOPE_DEVIATION_LIMIT_DEGREES;
            _renderer.InstrumentState.GlideslopeFlag = _gsFlagInputSignal.State;
            _renderer.InstrumentState.LocalizerDeviationDegrees = (float)_verticalCommandBarInputSignal.State;
            _renderer.InstrumentState.LocalizerDeviationLimitDegrees = LOCALIZER_DEVIATION_LIMIT_DEGREES;
            _renderer.InstrumentState.LocalizerFlag = _locFlagInputSignal.State;
            _renderer.InstrumentState.OffFlag = _offFlagInputSignal.State;
            _renderer.InstrumentState.PitchDegrees = (float)_pitchInputSignal.State;
            _renderer.InstrumentState.RollDegrees = (float)_rollInputSignal.State;
            _renderer.InstrumentState.ShowCommandBars = _commandBarsVisibleInputSignal.State;
            _renderer.Render(g, destinationRectangle);
        }
        #endregion

        #region Signal Creation

        private void CreateInputSignals()
        {
            _pitchInputSignal = CreatePitchInputSignal();
            _rollInputSignal = CreateRollInputSignal();
            _horizontalCommandBarInputSignal = CreateHorizontalCommandBarInputSignal();
            _verticalCommandBarInputSignal = CreateVerticalCommandBarInputSignal();
            _rateOfTurnInputSignal = CreateRateOfTurnInputSignal();
            _commandBarsVisibleInputSignal = CreateCommandBarsVisibleInputSignal();
            _auxFlagInputSignal = CreateAuxFlagInputSignal();
            _gsFlagInputSignal = CreateGSFlagInputSignal();
            _locFlagInputSignal = CreateLOCFlagInputSignal();
            _offFlagInputSignal = CreateOFFFlagInputSignal();
            _pitchAndRollEnableInputSignal = CreatePitchAndRollEnableInputSignal();
            _glideslopeIndicatorsPowerOnOffInputSignal = CreateGlideslopeIndicatorsPowerOnOffInputSignal();
            _rateOfTurnAndFlagsPowerOnOffInputSignal = CreateRateOfTurnAndFlagsPowerOnOffInputSignal();
        }

        private AnalogSignal CreatePitchInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Pitch (Degrees, -90.0=nadir, 0.0=level, +90.0=zenith)";
            thisSignal.Id = "HenkF16ADISupportBoard_Pitch_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.0; //degees
            thisSignal.IsAngle = true;
            thisSignal.MinValue = -90.0; //degrees
            thisSignal.MaxValue = 90.0; //degrees
            return thisSignal;
        }

        private AnalogSignal CreateRollInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Roll (Degrees, -180.0=inverted left bank, -90.0=left bank, 0.0=wings level, +90.0=right bank, +180.0=inverted right bank)";
            thisSignal.Id = "HenkF16ADISupportBoard_Roll_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.0; //degrees
            thisSignal.IsAngle = true;
            thisSignal.MinValue = -180.0; //degrees
            thisSignal.MaxValue = 180.0; //degrees
            return thisSignal;
        }

        private AnalogSignal CreateHorizontalCommandBarInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Horizontal Command Bar (Degrees, -1.0=100% deflected up, 0.0=centered, +1.0=100% deflected down)";
            thisSignal.Id = "HenkF16ADISupportBoard_Horizontal_Command_Bar_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.0; //centered
            thisSignal.MinValue = -1.0; //percent deflected up
            thisSignal.MaxValue = 1.0; //percent deflected down
            return thisSignal;
        }

        private AnalogSignal CreateVerticalCommandBarInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Vertical Command Bar (Degrees, -1.0=100% deflected left; 0.0=centered, +1.0=100% deflected right )";
            thisSignal.Id = "HenkF16ADISupportBoard_Vertical_Command_Bar_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.0f; //centered
            thisSignal.MinValue = -1.0; //percent deflected left
            thisSignal.MaxValue = 1.0; //percent deflected right
            return thisSignal;
        }

        private AnalogSignal CreateRateOfTurnInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Rate of Turn Indicator (% Deflection, -1.0=100% deflected left; 0.0=centered, +1.0=100% deflected right)";
            thisSignal.Id = "HenkF16ADISupportBoard_Rate_Of_Turn_Indicator_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsPercentage = true;
            thisSignal.MinValue = -1.0; //-100% (left deflected)
            thisSignal.MaxValue = 1.0; //+100% (right deflected)
            return thisSignal;
        }

        private DigitalSignal CreateAuxFlagInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "AUX Flag Visible";
            thisSignal.Id = "HenkF16ADISupportBoard_AUX_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;
            return thisSignal;
        }

        private DigitalSignal CreateGSFlagInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "GS Flag Visible";
            thisSignal.Id = "HenkF16ADISupportBoard_GS_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;
            return thisSignal;
        }

        private DigitalSignal CreateLOCFlagInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "LOC Flag Visible";
            thisSignal.Id = "HenkF16ADISupportBoard_LOC_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;
            return thisSignal;
        }
        private DigitalSignal CreateOFFFlagInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "OFF Flag Visible";
            thisSignal.Id = "HenkF16ADISupportBoard_OFF_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;
            return thisSignal;
        }

        private DigitalSignal CreateCommandBarsVisibleInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "Command Bars Visible";
            thisSignal.Id = "HenkF16ADISupportBoard_Command_Bars_Visible_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;
            return thisSignal;
        }
        private DigitalSignal CreatePitchAndRollEnableInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "Pitch/Roll synchros ENABLED";
            thisSignal.Id = "HenkF16ADISupportBoard_ENABLE_PITCH_AND_ROLL_Input";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;
            return thisSignal;
        }
        private DigitalSignal CreateGlideslopeIndicatorsPowerOnOffInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "GS POWER";
            thisSignal.Id = "HenkF16ADISupportBoard_GS_POWER_Input";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;
            return thisSignal;
        }
        private DigitalSignal CreateRateOfTurnAndFlagsPowerOnOffInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "RT and Flags POWER";
            thisSignal.Id = "HenkF16ADISupportBoard_RT_AND_FLAGS_POWER_Input";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;
            return thisSignal;
        }

        private void CreateOutputSignals()
        {
            _pitchOutputSignal = CreatePitchOutputSignal();
            _rollOutputSignal = CreateRollOutputSignal();
            _horizontalCommandBarOutputSignal = CreateHorizontalCommandBarOutputSignal();
            _verticalCommandBarOutputSignal = CreateVerticalCommandBarOutputSignal();
            _rateOfTurnOutputSignal = CreateRateOfTurnOutputSignal();
            _auxFlagOutputSignal = CreateAuxFlagOutputSignal();
            _gsFlagOutputSignal = CreateGSFlagOutputSignal();
            _locFlagOutputSignal = CreateLOCFlagOutputSignal();

            _pitchAndRollEnableOutputSignal = CreatePitchAndRollEnableOutputSignal();
            _glideslopeIndicatorsPowerOnOffOutputSignal = CreateGlideslopeIndicatorsPowerOnOffOutputSignal();
            _rateOfTurnAndFlagsPowerOnOffOutputSignal = CreateRateOfTurnAndFlagsPowerOnOffOutputSignal();


        }
        private DigitalSignal CreateAuxFlagOutputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital Outputs";
            thisSignal.FriendlyName = "AUX Flag Hidden";
            thisSignal.Id = "HenkF16ADISupportBoard_AUX_Flag_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;

            return thisSignal;

        }
        private DigitalSignal CreateGSFlagOutputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital Outputs";
            thisSignal.FriendlyName = "GS Flag Hidden";
            thisSignal.Id = "HenkF16ADISupportBoard_GS_Flag_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;

            return thisSignal;
        }
        private DigitalSignal CreateLOCFlagOutputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital Outputs";
            thisSignal.FriendlyName = "LOC Flag Hidden";
            thisSignal.Id = "HenkF16ADISupportBoard_LOC_Flag_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;

            return thisSignal;
        }
        private DigitalSignal CreatePitchAndRollEnableOutputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital Outputs";
            thisSignal.FriendlyName = "Pitch/Roll synchros ENABLED";
            thisSignal.Id = "HenkF16ADISupportBoard_ENABLE_PITCH_AND_ROLL_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;

            return thisSignal;
        }

        private DigitalSignal CreateGlideslopeIndicatorsPowerOnOffOutputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital Outputs";
            thisSignal.FriendlyName = "Glideslope Indicators POWER";
            thisSignal.Id = "HenkF16ADISupportBoard_GS_POWER_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;

            return thisSignal;
        }
        private DigitalSignal CreateRateOfTurnAndFlagsPowerOnOffOutputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital Outputs";
            thisSignal.FriendlyName = "RT and Flags POWER";
            thisSignal.Id = "HenkF16ADISupportBoard_RT_AND_FLAGS_POWER_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;

            return thisSignal;
        }

    private AnalogSignal CreatePitchOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Pitch Synchro Position (0-1023)";
            thisSignal.Id = "HenkF16ADISupportBoard_Pitch_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 424;
            thisSignal.MinValue = 140; 
            thisSignal.MaxValue = 700; 

            return thisSignal;
        }

        private AnalogSignal CreateRollOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Roll Synchro Position (0-1023)";
            thisSignal.Id = "HenkF16ADISupportBoard_Roll_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 512; 
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 1023;
            return thisSignal;
        }

        private AnalogSignal CreateHorizontalCommandBarOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Horizontal Glideslope Indicator Position (Percent Deflection, 0.0=100% deflected down, 0.5=centered, 1.0=100% deflected up)";
            thisSignal.Id = "HenkF16ADISupportBoard_Horizontal_GS_Bar_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.IsPercentage = true;
            thisSignal.State = 0.5;//50%
            thisSignal.MinValue = 0.0; //0%
            thisSignal.MaxValue = 1.0; //100%
            return thisSignal;
        }

        private AnalogSignal CreateVerticalCommandBarOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Vertical Glideslope Indicator Position (Percent Deflection, 0.0=100% deflected righ, 0.5=centered, 1.0=100% deflected left)";
            thisSignal.Id = "HenkF16ADISupportBoard_Vertical_GS_Bar_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.IsPercentage = true;
            thisSignal.State = 0.5;//50%
            thisSignal.MinValue = 0.0; //0%
            thisSignal.MaxValue = 1.0; //100%
            return thisSignal;
        }

        private AnalogSignal CreateRateOfTurnOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Rate of Turn Indicator (Percent Deflection, 0.0=100% deflected left, 0.5=centered, 1.0=100% deflected right)";
            thisSignal.Id = "HenkF16ADISupportBoard_Rate_Of_Turn_To_SDI";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.IsPercentage = true;
            thisSignal.State = 0.50; //50%
            thisSignal.MinValue = 0.0; //0%
            thisSignal.MaxValue = 1.0; //100%
            return thisSignal;
        }

        

        private void auxFlag_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateAuxFlagOutputValue();
        }
        private void gsFlag_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateGSFlagOutputValue();
        }
        private void locFlag_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateLOCFlagOutputValue();
        }
        private void offFlag_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {

        }
        private void commandBarsVisible_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateHorizontalGSBarOutputValues();
            UpdateVerticalGSBarOutputValues();
        }
        private void pitchAndRollEnable_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdatePitchAndRollEnableOutputValue();
        }
        private void glideslopeIndicatorsPowerOnOff_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateGSPowerOutputValue();
        }
        private void rateOfTurnAndFlagsPowerOnOff_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateRateOfTurnAndFlagsPowerOnOffOutputValue();
        }


        private void pitch_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdatePitchOutputValues();
        }

        private void roll_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateRollOutputValues();
        }

        private void horizontalCommandBar_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateHorizontalGSBarOutputValues();
        }

        private void verticalCommandBar_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateVerticalGSBarOutputValues();
        }

        private void rateOfTurn_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateRateOfTurnOutputValues();
        }
        private void UpdateAuxFlagOutputValue()
        {
            if (_auxFlagInputSignal != null && _auxFlagOutputSignal != null)
            {
                _auxFlagOutputSignal.State = !_auxFlagInputSignal.State;
            }
        }
        private void UpdateGSFlagOutputValue()
        {
            if (_gsFlagInputSignal != null && _gsFlagOutputSignal != null)
            {
                _gsFlagOutputSignal.State = !_gsFlagInputSignal.State;
            }
        }
        private void UpdateLOCFlagOutputValue()
        {
            if (_locFlagInputSignal != null && _locFlagOutputSignal != null)
            {
                _locFlagOutputSignal.State = !_locFlagInputSignal.State;
            }
        }
        private void UpdateGSPowerOutputValue()
        {
            if (_glideslopeIndicatorsPowerOnOffOutputSignal != null && _glideslopeIndicatorsPowerOnOffInputSignal != null)
            {
                _glideslopeIndicatorsPowerOnOffOutputSignal.State = _glideslopeIndicatorsPowerOnOffInputSignal.State;
            }
        }
        private void UpdatePitchAndRollEnableOutputValue()
        {
            if (_pitchAndRollEnableOutputSignal != null && _pitchAndRollEnableInputSignal != null)
            {
                _pitchAndRollEnableOutputSignal.State = _pitchAndRollEnableInputSignal.State;
            }
        }
        private void UpdateRateOfTurnAndFlagsPowerOnOffOutputValue()
        {
            if (_rateOfTurnAndFlagsPowerOnOffOutputSignal != null && _rateOfTurnAndFlagsPowerOnOffInputSignal != null)
            {
                _rateOfTurnAndFlagsPowerOnOffOutputSignal.State = _rateOfTurnAndFlagsPowerOnOffInputSignal.State;
            }
        }
        
        private void UpdatePitchOutputValues()
        {
            if (_pitchInputSignal != null && _pitchOutputSignal !=null)
            {
                _pitchOutputSignal.State = 424 + ((_pitchInputSignal.State / 90.000) * 255.000); 
            }
        }

        private void UpdateRollOutputValues()
        {
            if (_rollInputSignal != null && _rollOutputSignal != null)
            {
                _rollOutputSignal.State = 512.000 + ((_rollInputSignal.State / 180.000) * 512.000);
            }
        }

        private void UpdateHorizontalGSBarOutputValues()
        {
            if (_horizontalCommandBarInputSignal != null && _horizontalCommandBarOutputSignal !=null)
            {
                _horizontalCommandBarOutputSignal.State = _commandBarsVisibleInputSignal.State ? (0.5 + (0.5 * (_horizontalCommandBarInputSignal.State))) : 1.0f;
            }
        }

        private void UpdateVerticalGSBarOutputValues()
        {
            if (_verticalCommandBarInputSignal != null && _verticalCommandBarOutputSignal !=null)
            {
                _verticalCommandBarOutputSignal.State = _commandBarsVisibleInputSignal.State ? 1.00-(0.5 + (0.5 * (_verticalCommandBarInputSignal.State))) : 0.0f;
            }
        }

        private void UpdateRateOfTurnOutputValues()
        {
            if (_rateOfTurnInputSignal != null && _rateOfTurnOutputSignal !=null)
            {
                _rateOfTurnOutputSignal.State = (_rateOfTurnInputSignal.State +1.000) /2.000;
            }
        }

        #endregion

        #endregion

        #region Destructors

        /// <summary>
        ///     Public implementation of IDisposable.Dispose().  Cleans up
        ///     managed and unmanaged resources used by this
        ///     object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Standard finalizer, which will call Dispose() if this object
        ///     is not manually disposed.  Ordinarily called only
        ///     by the garbage collector.
        /// </summary>
        ~HenkF16ADISupportBoardHardwareSupportModule()
        {
            Dispose();
        }

        /// <summary>
        ///     Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">
        ///     flag to indicate if we should actually
        ///     perform disposal.  Distinguishes the private method signature
        ///     from the public signature.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    UnregisterForInputEvents();
                    AbandonInputEventHandlers();
                }
            }
            _isDisposed = true;
        }

        #endregion
    }
}