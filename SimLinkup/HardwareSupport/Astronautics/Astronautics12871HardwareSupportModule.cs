using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using Common.Math;
using log4net;
using LightningGauges.Renderers.F16;

namespace SimLinkup.HardwareSupport.Astronautics
{
    //Astronautics 12871 F-16 Primary ADI
    public class Astronautics12871HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Astronautics12871HardwareSupportModule));

        private const float GLIDESLOPE_DEVIATION_LIMIT_DEGREES = 1.0F;
        private const float LOCALIZER_DEVIATION_LIMIT_DEGREES = 5.0F;

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
        private AnalogSignal _inclinometerInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _inclinometerInputSignalChangedEventHandler;
        private DigitalSignal _showCommandBarsInputSignal;
        private DigitalSignal.SignalChangedEventHandler _showCommandBarsInputSignalChangedEventHandler;

        private DigitalSignal _auxFlagInputSignal;
        private DigitalSignal.SignalChangedEventHandler _auxFlagInputSignalChangedEventHandler;
        private DigitalSignal _gsFlagInputSignal;
        private DigitalSignal.SignalChangedEventHandler _gsFlagInputSignalChangedEventHandler;
        private DigitalSignal _locFlagInputSignal;
        private DigitalSignal.SignalChangedEventHandler _locFlagInputSignalChangedEventHandler;
        private DigitalSignal _offFlagInputSignal;
        private DigitalSignal.SignalChangedEventHandler _offFlagInputSignalChangedEventHandler;


        private AnalogSignal _pitchSinOutputSignal;
        private AnalogSignal _pitchCosOutputSignal;
        private AnalogSignal _rollCosOutputSignal;
        private AnalogSignal _rollSinOutputSignal;
        private AnalogSignal _horizontalCommandBarOutputSignal;
        private AnalogSignal _verticalCommandBarOutputSignal;
        private AnalogSignal _rateOfTurnOutputSignal;
        private AnalogSignal _inclinometerOutputSignal;

        private DigitalSignal _auxFlagOutputSignal;
        private DigitalSignal _gsFlagOutputSignal;
        private DigitalSignal _locFlagOutputSignal;
        private DigitalSignal _offFlagOutputSignal;

        private IADI _renderer = new ADI(); 
        #endregion

        #region Constructors

        private Astronautics12871HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Astronautics P/N 12871 - Indicator - Simulated Attitude Director Indicator"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Astronautics12871HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.CurrentMappingProfileDirectory,
                    "Astronautics12871HardwareSupportModuleConfig.config");
                var hsmConfig =
                    Astronautics12871HardwareSupportModuleConfig.Load(hsmConfigFilePath);
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return toReturn.ToArray();
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
                    _rateOfTurnInputSignal, _inclinometerInputSignal
                };
            }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return new[] {_showCommandBarsInputSignal, _auxFlagInputSignal, _gsFlagInputSignal, _locFlagInputSignal, _offFlagInputSignal}; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get
            {
                return new[]
                {
                    _pitchSinOutputSignal, _pitchCosOutputSignal, _rollSinOutputSignal, _rollCosOutputSignal,
                    _horizontalCommandBarOutputSignal, _verticalCommandBarOutputSignal, _rateOfTurnOutputSignal, _inclinometerOutputSignal
                };
            }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get { return new[] { _auxFlagOutputSignal, _gsFlagOutputSignal, _locFlagOutputSignal, _offFlagOutputSignal}; }
        }

        #endregion

        #region Visualization
        public override void Render(System.Drawing.Graphics g, System.Drawing.Rectangle destinationRectangle)
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
            _renderer.InstrumentState.ShowCommandBars = _showCommandBarsInputSignal.State;
            _renderer.Render(g, destinationRectangle);
        }
        #endregion

        #region Signals Handling

        #region Signals Event Handling

        private void CreateInputEventHandlers()
        {
            _pitchInputSignalChangedEventHandler =
                pitch_InputSignalChanged;
            _rollInputSignalChangedEventHandler =
                roll_InputSignalChanged;
            _horizontalCommandBarInputSignalChangedEventHandler =
                horizontalCommandBar_InputSignalChanged;
            _verticalCommandBarInputSignalChangedEventHandler =
                verticalCommandBar_InputSignalChanged;
            _rateOfTurnInputSignalChangedEventHandler =
                rateOfTurn_InputSignalChanged;
            _inclinometerInputSignalChangedEventHandler =
                inclinometer_InputSignalChanged;
            _showCommandBarsInputSignalChangedEventHandler = showCommandBars_InputSignalChanged;
            _auxFlagInputSignalChangedEventHandler = auxFlag_InputSignalChanged;
            _gsFlagInputSignalChangedEventHandler = gsFlag_InputSignalChanged;
            _locFlagInputSignalChangedEventHandler = locFlag_InputSignalChanged;
            _offFlagInputSignalChangedEventHandler = offFlag_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _pitchInputSignalChangedEventHandler = null;
            _rollInputSignalChangedEventHandler = null;
            _horizontalCommandBarInputSignalChangedEventHandler = null;
            _verticalCommandBarInputSignalChangedEventHandler = null;
            _rateOfTurnInputSignalChangedEventHandler = null;
            _inclinometerInputSignalChangedEventHandler = null;
            _showCommandBarsInputSignalChangedEventHandler = null;
            _auxFlagInputSignalChangedEventHandler = null;
            _gsFlagInputSignalChangedEventHandler = null;
            _locFlagInputSignalChangedEventHandler = null;
            _offFlagInputSignalChangedEventHandler = null;
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
            if (_inclinometerInputSignal != null)
            {
                _inclinometerInputSignal.SignalChanged += _inclinometerInputSignalChangedEventHandler;
            }
            if (_showCommandBarsInputSignal != null)
            {
                _showCommandBarsInputSignal.SignalChanged += _showCommandBarsInputSignalChangedEventHandler;
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
                catch (RemotingException)
                {
                }
            }
            if (_gsFlagInputSignalChangedEventHandler != null && _gsFlagInputSignal != null)
            {
                try
                {
                    _gsFlagInputSignal.SignalChanged -= _gsFlagInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_locFlagInputSignalChangedEventHandler != null && _locFlagInputSignal != null)
            {
                try
                {
                    _locFlagInputSignal.SignalChanged -= _locFlagInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_offFlagInputSignalChangedEventHandler != null && _offFlagInputSignal != null)
            {
                try
                {
                    _offFlagInputSignal.SignalChanged -= _offFlagInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_pitchInputSignalChangedEventHandler != null && _pitchInputSignal != null)
            {
                try
                {
                    _pitchInputSignal.SignalChanged -= _pitchInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_rollInputSignalChangedEventHandler != null && _rollInputSignal != null)
            {
                try
                {
                    _rollInputSignal.SignalChanged -= _rollInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_horizontalCommandBarInputSignalChangedEventHandler != null && _horizontalCommandBarInputSignal != null)
            {
                try
                {
                    _horizontalCommandBarInputSignal.SignalChanged -=
                        _horizontalCommandBarInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_verticalCommandBarInputSignalChangedEventHandler != null && _verticalCommandBarInputSignal != null)
            {
                try
                {
                    _verticalCommandBarInputSignal.SignalChanged -= _verticalCommandBarInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_rateOfTurnInputSignalChangedEventHandler != null && _rateOfTurnInputSignal != null)
            {
                try
                {
                    _rateOfTurnInputSignal.SignalChanged -= _rateOfTurnInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_inclinometerInputSignalChangedEventHandler != null && _inclinometerInputSignal != null)
            {
                try
                {
                    _inclinometerInputSignal.SignalChanged -= _inclinometerInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_showCommandBarsInputSignalChangedEventHandler != null && _showCommandBarsInputSignal != null)
            {
                try
                {
                    _showCommandBarsInputSignal.SignalChanged -= _showCommandBarsInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
        }

        #endregion

        #region Signal Creation

        private void CreateInputSignals()
        {
            _auxFlagInputSignal = CreateAuxFlagInputSignal();
            _gsFlagInputSignal = CreateGSFlagInputSignal();
            _locFlagInputSignal = CreateLOCFlagInputSignal();
            _offFlagInputSignal = CreateOffFlagInputSignal();
            _pitchInputSignal = CreatePitchInputSignal();
            _rollInputSignal = CreateRollInputSignal();
            _horizontalCommandBarInputSignal = CreateHorizontalCommandBarInputSignal();
            _verticalCommandBarInputSignal = CreateVerticalCommandBarInputSignal();
            _rateOfTurnInputSignal = CreateRateOfTurnInputSignal();
            _showCommandBarsInputSignal = CreateShowCommandBarsInputSignal();
            _inclinometerInputSignal = CreateInclinometerInputSignal();
        }

        private AnalogSignal CreatePitchInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Pitch (Degrees)";
            thisSignal.Id = "12871_Pitch_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsAngle = true;
            thisSignal.MinValue = -90;
            thisSignal.MaxValue = 90;
            return thisSignal;
        }

        private AnalogSignal CreateRollInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Roll (Degrees)";
            thisSignal.Id = "12871_Roll_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsAngle = true;
            thisSignal.MinValue = -180;
            thisSignal.MaxValue = 180;

            return thisSignal;
        }

        private AnalogSignal CreateHorizontalCommandBarInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Horizontal Command Bar (% Deflection)";
            thisSignal.Id = "12871_Horizontal_Command_Bar_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsPercentage = true;
            thisSignal.MinValue = -1;
            thisSignal.MaxValue = 1;

            return thisSignal;
        }

        private AnalogSignal CreateVerticalCommandBarInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Vertical Command Bar (% Deflection)";
            thisSignal.Id = "12871_Vertical_Command_Bar_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsPercentage = true;
            thisSignal.MinValue = -1;
            thisSignal.MaxValue = 1;

            return thisSignal;
        }

        private AnalogSignal CreateRateOfTurnInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Rate of Turn Indicator (% Deflection)";
            thisSignal.Id = "12871_Rate_Of_Turn_Indicator_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsPercentage = true;
            thisSignal.MinValue = -1;
            thisSignal.MaxValue = 1;

            return thisSignal;
        }

        private AnalogSignal CreateInclinometerInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Inclinometer Position (% Deflection)";
            thisSignal.Id = "12871_Inclinometer_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsPercentage = true;
            thisSignal.MinValue = -1;
            thisSignal.MaxValue = 1;

            return thisSignal;
        }

        private DigitalSignal CreateAuxFlagInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "AUX Flag";
            thisSignal.Id = "12871_AUX_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;
            return thisSignal;
        }

        private DigitalSignal CreateOffFlagInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "OFF Flag";
            thisSignal.Id = "12871_OFF_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;
            return thisSignal;
        }

        private DigitalSignal CreateGSFlagInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "GS Flag";
            thisSignal.Id = "12871_GS_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;
            return thisSignal;
        }

        private DigitalSignal CreateLOCFlagInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "LOC Flag";
            thisSignal.Id = "12871_LOC_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;
            return thisSignal;
        }

        private DigitalSignal CreateShowCommandBarsInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "Command Bars Visible Flag";
            thisSignal.Id = "12871_Show_Command_Bars_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;
            return thisSignal;
        }

        private void CreateOutputSignals()
        {
            _auxFlagOutputSignal = CreateAUXFlagOutputSignal();
            _gsFlagOutputSignal = CreateGSFlagOutputSignal();
            _locFlagOutputSignal = CreateLOCFlagOutputSignal();
            _offFlagOutputSignal = CreateOFFFlagOutputSignal();
            _pitchSinOutputSignal = CreatePitchSinOutputSignal();
            _pitchCosOutputSignal = CreatePitchCosOutputSignal();
            _rollSinOutputSignal = CreateRollSinOutputSignal();
            _rollCosOutputSignal = CreateRollCosOutputSignal();
            _horizontalCommandBarOutputSignal = CreateHorizontalCommandBarOutputSignal();
            _verticalCommandBarOutputSignal = CreateVerticalCommandBarOutputSignal();
            _rateOfTurnOutputSignal = CreateRateOfTurnOutputSignal();
            _inclinometerOutputSignal = CreateInclinometerOutputSignal();
        }
        private DigitalSignal CreateAUXFlagOutputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital Outputs";
            thisSignal.FriendlyName = "AUX Flag";
            thisSignal.Id = "12871_AUX_Flag_To_Instrument";
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
            thisSignal.FriendlyName = "GS Flag";
            thisSignal.Id = "12871_GS_Flag_To_Instrument";
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
            thisSignal.FriendlyName = "LOC Flag";
            thisSignal.Id = "12871_LOC_Flag_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;
            return thisSignal;
        }
        private DigitalSignal CreateOFFFlagOutputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital Outputs";
            thisSignal.FriendlyName = "OFF Flag";
            thisSignal.Id = "12871_OFF_Flag_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;
            return thisSignal;
        }
        private AnalogSignal CreatePitchSinOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Pitch (SIN)";
            thisSignal.Id = "12871_Pitch_SIN_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.00; //volts;
            thisSignal.IsVoltage = true;
            thisSignal.IsSine = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;

            return thisSignal;
        }

        private AnalogSignal CreatePitchCosOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Pitch (COS)";
            thisSignal.Id = "12871_Pitch_COS_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = +10.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.IsCosine = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateRollSinOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Roll (SIN)";
            thisSignal.Id = "12871_Roll_SIN_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.00; //volts;
            thisSignal.IsVoltage = true;
            thisSignal.IsSine = true;
            thisSignal.MinValue = -1;
            thisSignal.MaxValue = 1;
            return thisSignal;
        }

        private AnalogSignal CreateRollCosOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Roll (COS)";
            thisSignal.Id = "12871_Roll_COS_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = +10.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.IsCosine = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateHorizontalCommandBarOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Horizontal Command Bar";
            thisSignal.Id = "12871_Horizontal_Command_Bar_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 6.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateVerticalCommandBarOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Vertical Command Bar";
            thisSignal.Id = "12871_Vertical_Command_Bar_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 6.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateRateOfTurnOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Rate of Turn Indicator";
            thisSignal.Id = "12871_Rate_Of_Turn_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateInclinometerOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Inclinometer Indicator";
            thisSignal.Id = "12871_Inclinometer_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }
        private void pitch_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdatePitchOutputValues(args.CurrentState);
        }

        private void roll_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateRollOutputValues(args.CurrentState);
        }
        private void auxFlag_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateAUXFlagOutputValue();
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
            UpdateOFFFlagOutputValue();
        }
        private void showCommandBars_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateHorizontalCommandBarOutputValues();
            UpdateVerticalCommandBarOutputValues();
        }
        private void horizontalCommandBar_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateHorizontalCommandBarOutputValues();
        }

        private void verticalCommandBar_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateVerticalCommandBarOutputValues();
        }

        private void rateOfTurn_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateRateOfTurnOutputValues();
        }

        private void inclinometer_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateInclinometerOutputValues();
        }
        private void UpdateAUXFlagOutputValue()
        {
            _auxFlagOutputSignal.State = _auxFlagInputSignal.State;
        }
        private void UpdateGSFlagOutputValue()
        {
            _gsFlagOutputSignal.State = _gsFlagInputSignal.State;
        }
        private void UpdateLOCFlagOutputValue()
        {
            _locFlagOutputSignal.State = _locFlagInputSignal.State;
        }
        private void UpdateOFFFlagOutputValue()
        {
            _offFlagOutputSignal.State = _offFlagInputSignal.State;
        }

        private void UpdatePitchOutputValues(double pitchDegrees)
        {
            if (_pitchInputSignal != null)
            {
                var pitchInputDegrees = _pitchInputSignal.State;

                double pitchSinOutputValue = 0;
                double pitchCosOutputValue = 0;

                pitchSinOutputValue = 10.0000*Math.Sin(pitchInputDegrees*Constants.RADIANS_PER_DEGREE);
                pitchCosOutputValue = 10.0000*Math.Cos(pitchInputDegrees*Constants.RADIANS_PER_DEGREE);

                if (_pitchSinOutputSignal != null)
                {
                    if (pitchSinOutputValue < -10)
                    {
                        pitchSinOutputValue = -10;
                    }
                    else if (pitchSinOutputValue > 10)
                    {
                        pitchSinOutputValue = 10;
                    }

                    _pitchSinOutputSignal.State = pitchSinOutputValue;
                }

                if (_pitchCosOutputSignal != null)
                {
                    if (pitchCosOutputValue < -10)
                    {
                        pitchCosOutputValue = -10;
                    }
                    else if (pitchCosOutputValue > 10)
                    {
                        pitchCosOutputValue = 10;
                    }

                    _pitchCosOutputSignal.State = pitchCosOutputValue;
                }
            }
        }

        private void UpdateRollOutputValues(double rollDegrees)
        {
            if (_rollInputSignal != null)
            {
                var rollInputDegrees = _rollInputSignal.State;

                double rollSinOutputValue = 0;
                double rollCosOutputValue = 0;

                rollSinOutputValue = 10.0000*Math.Sin(rollInputDegrees*Constants.RADIANS_PER_DEGREE);
                rollCosOutputValue = 10.0000*Math.Cos(rollInputDegrees*Constants.RADIANS_PER_DEGREE);

                if (_rollSinOutputSignal != null)
                {
                    if (rollSinOutputValue < -10)
                    {
                        rollSinOutputValue = -10;
                    }
                    else if (rollSinOutputValue > 10)
                    {
                        rollSinOutputValue = 10;
                    }

                    _rollSinOutputSignal.State = rollSinOutputValue;
                }

                if (_rollCosOutputSignal != null)
                {
                    if (rollCosOutputValue < -10)
                    {
                        rollCosOutputValue = -10;
                    }
                    else if (rollCosOutputValue > 10)
                    {
                        rollCosOutputValue = 10;
                    }

                    _rollCosOutputSignal.State = rollCosOutputValue;
                }
            }
        }

        private void UpdateHorizontalCommandBarOutputValues()
        {
            if (_horizontalCommandBarInputSignal != null)
            {
                var percentDeflection = _horizontalCommandBarInputSignal.State;

                double outputValue = 0;

                if (_showCommandBarsInputSignal.State)
                {
                    outputValue = percentDeflection * 2.25f;
                }
                else
                {
                    outputValue = 10;
                }

                if (_horizontalCommandBarOutputSignal != null)
                {
                    if (outputValue < -10)
                    {
                        outputValue = -10;
                    }
                    else if (outputValue > 10)
                    {
                        outputValue = 10;
                    }

                    _horizontalCommandBarOutputSignal.State = outputValue;
                }
            }
        }

        private void UpdateVerticalCommandBarOutputValues()
        {
            if (_verticalCommandBarInputSignal != null)
            {
                var percentDeflection = _verticalCommandBarInputSignal.State;

                double outputValue = 0;

                if (_showCommandBarsInputSignal.State)
                {
                    outputValue = percentDeflection * 2.25f;
                }
                else
                {
                    outputValue = 10;
                }

                if (_verticalCommandBarOutputSignal != null)
                {
                    if (outputValue < -10)
                    {
                        outputValue = -10;
                    }
                    else if (outputValue > 10)
                    {
                        outputValue = 10;
                    }

                    _verticalCommandBarOutputSignal.State = outputValue;
                }
            }
        }

        private void UpdateRateOfTurnOutputValues()
        {
            if (_rateOfTurnInputSignal != null)
            {
                var percentDeflection = _rateOfTurnInputSignal.State;

                double outputValue = 0;

                outputValue = 10.0000*percentDeflection;

                if (_rateOfTurnOutputSignal != null)
                {
                    if (outputValue < -10)
                    {
                        outputValue = -10;
                    }
                    else if (outputValue > 10)
                    {
                        outputValue = 0;
                    }

                    _rateOfTurnOutputSignal.State = outputValue;
                }
            }
        }


        private void UpdateInclinometerOutputValues()
        {
            if (_inclinometerInputSignal != null)
            {
                var percentDeflection = _inclinometerInputSignal.State;

                double outputValue = 0;

                outputValue = 10.0000 * percentDeflection;

                if (_inclinometerOutputSignal != null)
                {
                    if (outputValue < -10)
                    {
                        outputValue = -10;
                    }
                    else if (outputValue > 10)
                    {
                        outputValue = 0;
                    }

                    _inclinometerOutputSignal.State = outputValue;
                }
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
        ~Astronautics12871HardwareSupportModule()
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