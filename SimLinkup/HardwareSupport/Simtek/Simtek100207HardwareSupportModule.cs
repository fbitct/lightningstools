using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-0207 F-16 RPM Indicator
    public class Simtek100207HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Simtek100207HardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;
        private AnalogSignal _rpmInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _rpmInputSignalChangedEventHandler;
        private AnalogSignal _rpmOutputSignal;

        #endregion

        #region Constructors

        private Simtek100207HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-0207 - Indicator, Simulated Tachometer"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek100207HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "Simtek100207HardwareSupportModule.config");
                var hsmConfig =
                    Simtek100207HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] {_rpmInputSignal}; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] {_rpmOutputSignal}; }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get { return null; }
        }

        #endregion

        #region Signals Handling

        #region Signals Event Handling

        private void CreateInputEventHandlers()
        {
            _rpmInputSignalChangedEventHandler = rpm_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _rpmInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_rpmInputSignal != null)
            {
                _rpmInputSignal.SignalChanged += _rpmInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_rpmInputSignalChangedEventHandler != null && _rpmInputSignal != null)
            {
                try
                {
                    _rpmInputSignal.SignalChanged -= _rpmInputSignalChangedEventHandler;
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
            _rpmInputSignal = CreateRPMInputSignal();
        }

        private void CreateOutputSignals()
        {
            _rpmOutputSignal = CreateRPMOutputSignal();
        }

        private AnalogSignal CreateRPMOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "RPM";
            thisSignal.Id = "100207_RPM_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = -10.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateRPMInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "RPM";
            thisSignal.Id = "100207_RPM_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.IsPercentage = true;
            thisSignal.State = 0;
            thisSignal.MinValue = 110;
            return thisSignal;
        }

        private void rpm_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void UpdateOutputValues()
        {
            if (_rpmInputSignal != null)
            {
                var rpmInput = _rpmInputSignal.State;
                double rpmOutputValue = 0;
                if (_rpmOutputSignal != null)
                {
                    if (rpmInput < 10)
                    {
                        rpmOutputValue = Math.Max(-10, -10.0 + ((rpmInput/10.0)*1.25));
                    }
                    else if (rpmInput >= 10 && rpmInput < 20)
                    {
                        rpmOutputValue = -8.75 + (((rpmInput - 10)/10.0)*1.25);
                    }
                    else if (rpmInput >= 20 && rpmInput < 30)
                    {
                        rpmOutputValue = -7.50 + (((rpmInput - 20)/10.0)*1.25);
                    }
                    else if (rpmInput >= 30 && rpmInput < 40)
                    {
                        rpmOutputValue = -6.25 + (((rpmInput - 30)/10.0)*1.25);
                    }
                    else if (rpmInput >= 40 && rpmInput < 50)
                    {
                        rpmOutputValue = -5.00 + (((rpmInput - 40)/10.0)*1.25);
                    }
                    else if (rpmInput >= 50 && rpmInput < 60)
                    {
                        rpmOutputValue = -3.75 + (((rpmInput - 50)/10.0)*1.25);
                    }
                    else if (rpmInput >= 60 && rpmInput < 65)
                    {
                        rpmOutputValue = -2.50 + (((rpmInput - 60)/5.0)*1.562);
                    }
                    else if (rpmInput >= 65 && rpmInput < 68)
                    {
                        rpmOutputValue = -0.938 + (((rpmInput - 65)/3.0)*0.938);
                    }
                    else if (rpmInput >= 68 && rpmInput < 70)
                    {
                        rpmOutputValue = 0.00 + (((rpmInput - 68)/2.0)*0.625);
                    }
                    else if (rpmInput >= 70 && rpmInput < 75)
                    {
                        rpmOutputValue = 0.625 + (((rpmInput - 70)/5.0)*1.563);
                    }
                    else if (rpmInput >= 75 && rpmInput < 80)
                    {
                        rpmOutputValue = 2.188 + (((rpmInput - 75)/5.0)*1.562);
                    }
                    else if (rpmInput >= 80 && rpmInput < 85)
                    {
                        rpmOutputValue = 3.750 + (((rpmInput - 80)/5.0)*1.563);
                    }
                    else if (rpmInput >= 85 && rpmInput < 90)
                    {
                        rpmOutputValue = 5.313 + (((rpmInput - 85)/5.0)*1.562);
                    }
                    else if (rpmInput >= 90 && rpmInput < 95)
                    {
                        rpmOutputValue = 6.875 + (((rpmInput - 90)/5.0)*1.563);
                    }
                    else if (rpmInput >= 95)
                    {
                        rpmOutputValue = Math.Min(10, 8.438 + (Math.Min(1, ((rpmInput - 95)/5.0))*1.562));
                    }

                    if (rpmOutputValue < -10)
                    {
                        rpmOutputValue = -10;
                    }
                    else if (rpmOutputValue > 10)
                    {
                        rpmOutputValue = 10;
                    }
                    _rpmOutputSignal.State = rpmOutputValue;
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
        ~Simtek100207HardwareSupportModule()
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