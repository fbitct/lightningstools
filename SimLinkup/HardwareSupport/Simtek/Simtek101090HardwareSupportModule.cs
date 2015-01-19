using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-1090 F-16 EPU FUEL QTY IND
    public class Simtek101090HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Simtek101090HardwareSupportModule));

        #endregion

        #region Instance variables

        private AnalogSignal _epuFuelPercentageInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _epuFuelPercentageInputSignalChangedEventHandler;
        private AnalogSignal _epuFuelPercentageOutputSignal;
        private bool _isDisposed;

        #endregion

        #region Constructors

        private Simtek101090HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-1090 - EPU Fuel Quantity Ind"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek101090HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "Simtek101090HardwareSupportModule.config");
                var hsmConfig =
                    Simtek101090HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] {_epuFuelPercentageInputSignal}; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] {_epuFuelPercentageOutputSignal}; }
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
            _epuFuelPercentageInputSignalChangedEventHandler =
                epu_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _epuFuelPercentageInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_epuFuelPercentageInputSignal != null)
            {
                _epuFuelPercentageInputSignal.SignalChanged += _epuFuelPercentageInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_epuFuelPercentageInputSignalChangedEventHandler != null && _epuFuelPercentageInputSignal != null)
            {
                try
                {
                    _epuFuelPercentageInputSignal.SignalChanged -= _epuFuelPercentageInputSignalChangedEventHandler;
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
            _epuFuelPercentageInputSignal = CreateEPUInputSignal();
        }

        private void CreateOutputSignals()
        {
            _epuFuelPercentageOutputSignal = CreateEPUOutputSignal();
        }

        private AnalogSignal CreateEPUOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "EPU Fuel Quantity %";
            thisSignal.Id = "101090_EPU_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = (-10.00 + 10.00)/20.00;
            ;
            return thisSignal;
        }

        private AnalogSignal CreateEPUInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "EPU Fuel Quantity %";
            thisSignal.Id = "101090_EPU_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }

        private void epu_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void UpdateOutputValues()
        {
            if (_epuFuelPercentageInputSignal != null)
            {
                var epuInput = _epuFuelPercentageInputSignal.State;
                double epuOutputValue = 0;
                if (_epuFuelPercentageOutputSignal != null)
                {
                    if (epuInput < 0)
                    {
                        epuOutputValue = -10;
                    }
                    else if (epuInput > 100)
                    {
                        epuOutputValue = 10;
                    }
                    else
                    {
                        epuOutputValue = ((epuInput/100)*20) - 10;
                    }

                    if (epuOutputValue < -10)
                    {
                        epuOutputValue = -10;
                    }
                    else if (epuOutputValue > 10)
                    {
                        epuOutputValue = 10;
                    }

                    _epuFuelPercentageOutputSignal.State = ((epuOutputValue + 10.0000)/20.0000);
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
        ~Simtek101090HardwareSupportModule()
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