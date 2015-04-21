using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using Common.Math;
using log4net;

namespace SimLinkup.HardwareSupport.Gould
{
    //GOULD F-16 COMPASS
    public class GouldHS070D51341HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof(GouldHS070D51341HardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;
        private AnalogSignal _compassInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _compassInputSignalChangedEventHandler;
        private AnalogSignal _compassSINOutputSignal;
        private AnalogSignal _compassCOSOutputSignal;

        #endregion

        #region Constructors

        private GouldHS070D51341HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Gould P/N HS070D5134-1 - Standby Compass"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new GouldHS070D51341HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "GouldHS070D51341HardwareSupportModule.config");
                var hsmConfig =
                    GouldHS070D51341HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] { _compassInputSignal }; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] { _compassSINOutputSignal, _compassCOSOutputSignal }; }
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
            _compassInputSignalChangedEventHandler =
                compass_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _compassInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_compassInputSignal != null)
            {
                _compassInputSignal.SignalChanged += _compassInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_compassInputSignalChangedEventHandler != null && _compassInputSignal != null)
            {
                try
                {
                    _compassInputSignal.SignalChanged -= _compassInputSignalChangedEventHandler;
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
            _compassInputSignal = CreateCompassInputSignal();
        }

        private void CreateOutputSignals()
        {
            _compassSINOutputSignal = CreateCompassSINOutputSignal();
            _compassCOSOutputSignal = CreateCompassCOSOutputSignal();
        }

        private AnalogSignal CreateCompassSINOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Compass (SIN)";
            thisSignal.Id = "HS070D51341_Compass__SIN_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 1.00f;
            return thisSignal;
        }

        private AnalogSignal CreateCompassCOSOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Compass (SIN)";
            thisSignal.Id = "HS070D51341_Compass__COS_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.5f;
            return thisSignal;
        }
        private AnalogSignal CreateCompassInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Magnetic Heading (Degrees)";
            thisSignal.Id = "HS070D51341_Compass__Altitude_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;

            return thisSignal;
        }

        private void compass_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void UpdateOutputValues()
        {
            if (_compassInputSignal != null)
            {
                var compassInput = Math.Abs(_compassInputSignal.State % 360.000);
                double compassSINOutputValue = 0;
                if (_compassSINOutputSignal != null)
                {
                    compassSINOutputValue = 10.0000 * Math.Sin(compassInput * Constants.RADIANS_PER_DEGREE);
                    
                    if (compassSINOutputValue < -10)
                    {
                        compassSINOutputValue = -10;
                    }
                    else if (compassSINOutputValue > 10)
                    {
                        compassSINOutputValue = 10;
                    }

                    _compassSINOutputSignal.State = ((compassSINOutputValue + 10.0000) / 20.0000);
                }

                if (_compassCOSOutputSignal != null)
                {
                    double compassCOSOutputValue = 0;
                   
                    compassCOSOutputValue = 10.0000 * Math.Cos(compassInput * Constants.RADIANS_PER_DEGREE);

                    if (compassCOSOutputValue < -10)
                    {
                        compassCOSOutputValue = -10;
                    }
                    else if (compassCOSOutputValue > 10)
                    {
                        compassCOSOutputValue = 10;
                    }

                    _compassCOSOutputSignal.State = ((compassCOSOutputValue + 10.0000) / 20.0000);
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
        ~GouldHS070D51341HardwareSupportModule()
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