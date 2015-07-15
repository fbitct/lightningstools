using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using Common.Math;
using log4net;
using System.Drawing;
using LightningGauges.Renderers.F16;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-0335-01 F-16 Standby ADI
    public class Simtek100335015HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Simtek100335015HardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;
        private AnalogSignal _pitchCosOutputSignal;
        private AnalogSignal _pitchInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _pitchInputSignalChangedEventHandler;
        private AnalogSignal _pitchSinOutputSignal;
        private AnalogSignal _rollCosOutputSignal;
        private AnalogSignal _rollInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _rollInputSignalChangedEventHandler;
        private AnalogSignal _rollSinOutputSignal;

        private DigitalSignal _offFlagInputSignal;
        private DigitalSignal.SignalChangedEventHandler _offFlagInputSignalChangedEventHandler;
        private DigitalSignal _offFlagOutputSignal;

        private IStandbyADI _renderer = new StandbyADI();
        #endregion

        #region Constructors

        private Simtek100335015HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-0335-01 - Indicator - Simulated Standby Attitude"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek100335015HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "Simtek100335015HardwareSupportModuleConfig.config");
                var hsmConfig =
                    Simtek100335015HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] {_pitchInputSignal, _rollInputSignal}; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return new[] { _offFlagInputSignal }; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get
            {
                return new[] {_pitchSinOutputSignal, _pitchCosOutputSignal, _rollSinOutputSignal, _rollCosOutputSignal};
            }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get { return new[] { _offFlagOutputSignal }; }
        }

        #endregion

        #region Visualization
        public override void Render(Graphics g, Rectangle destinationRectangle)
        {
            _renderer.InstrumentState.PitchDegrees = (float)_pitchInputSignal.State;
            _renderer.InstrumentState.RollDegrees = (float)_rollInputSignal.State;
            _renderer.InstrumentState.OffFlag = _offFlagInputSignal.State;
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
            _offFlagInputSignalChangedEventHandler =
                offFlag_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _pitchInputSignalChangedEventHandler = null;
            _rollInputSignalChangedEventHandler = null;
            _offFlagInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
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
        }

        private void UnregisterForInputEvents()
        {
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
        }

        #endregion

        #region Signal Creation

        private void CreateInputSignals()
        {
            _pitchInputSignal = CreatePitchInputSignal();
            _rollInputSignal = CreateRollInputSignal();
            _offFlagInputSignal = CreateOFFFlagInputSignal();
        }
        private DigitalSignal CreateOFFFlagInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "OFF Flag";
            thisSignal.Id = "10033501_OFF_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;
            return thisSignal;
        }


        private AnalogSignal CreatePitchInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Pitch (Degrees)";
            thisSignal.Id = "10033501_Pitch_From_Sim";
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
            thisSignal.Id = "10033501_Roll_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsAngle = true;
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 360;
            return thisSignal;
        }

        private void CreateOutputSignals()
        {
            _pitchSinOutputSignal = CreatePitchSinOutputSignal();
            _pitchCosOutputSignal = CreatePitchCosOutputSignal();
            _rollSinOutputSignal = CreateRollSinOutputSignal();
            _rollCosOutputSignal = CreateRollCosOutputSignal();
            _offFlagOutputSignal = CreateOFFFlagOutputSignal();
        }
        private DigitalSignal CreateOFFFlagOutputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital Outputs";
            thisSignal.FriendlyName = "OFF Flag";
            thisSignal.Id = "10033501_OFF_Flag_To_Instrument";
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
            thisSignal.Id = "10033501_Pitch_SIN_To_Instrument";
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
            thisSignal.Id = "10033501_Pitch_COS_To_Instrument";
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
            thisSignal.Id = "10033501_Roll_SIN_To_Instrument";
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

        private AnalogSignal CreateRollCosOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Roll (COS)";
            thisSignal.Id = "10033501_Roll_COS_To_Instrument";
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

        private void pitch_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdatePitchOutputValues();
        }

        private void roll_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateRollOutputValues();
        }
        private void offFlag_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateOFFFlagOutputValue();
        }
        private void UpdateOFFFlagOutputValue()
        {
            _offFlagOutputSignal.State = _offFlagInputSignal.State;
        }
        private void UpdatePitchOutputValues()
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

        private void UpdateRollOutputValues()
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
        ~Simtek100335015HardwareSupportModule()
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