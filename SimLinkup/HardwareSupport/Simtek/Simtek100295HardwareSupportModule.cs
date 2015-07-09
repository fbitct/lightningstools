using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-0295 F-16 Simulated Fuel Flow Indicator
    public class Simtek100295HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Simtek100295HardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;
        private AnalogSignal _fuelFlowInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _fuelFlowInputSignalChangedEventHandler;
        private AnalogSignal _fuelFlowOutputSignal;

        #endregion

        #region Constructors

        private Simtek100295HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-0295 - Simulated Fuel Flow Indicator"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek100295HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "Simtek100295HardwareSupportModuleConfig.config");
                var hsmConfig =
                    Simtek100295HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] {_fuelFlowInputSignal}; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] {_fuelFlowOutputSignal}; }
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
            _fuelFlowInputSignalChangedEventHandler =
                fuelFlow_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _fuelFlowInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_fuelFlowInputSignal != null)
            {
                _fuelFlowInputSignal.SignalChanged += _fuelFlowInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_fuelFlowInputSignalChangedEventHandler != null && _fuelFlowInputSignal != null)
            {
                try
                {
                    _fuelFlowInputSignal.SignalChanged -= _fuelFlowInputSignalChangedEventHandler;
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
            _fuelFlowInputSignal = CreateFuelFlowInputSignal();
        }

        private AnalogSignal CreateFuelFlowInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Fuel Flow (pounds per hour)";
            thisSignal.Id = "100295_Fuel_Flow_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 99900;
            return thisSignal;
        }


        private void CreateOutputSignals()
        {
            _fuelFlowOutputSignal = CreateFuelFlowOutputSignal();
        }

        private AnalogSignal CreateFuelFlowOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Fuel Flow";
            thisSignal.Id = "100295_Fuel_Flow_To_Instrument";
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


        private void fuelFlow_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void UpdateOutputValues()
        {
            if (_fuelFlowOutputSignal != null)
            {
                var fuelFlow = _fuelFlowInputSignal.State;
                if (fuelFlow <= 10000)
                {
                    _fuelFlowOutputSignal.State = (Math.Min((_fuelFlowInputSignal.State / 9900.00), 1.00)) * 10.00;
                }
                else if (fuelFlow >=10000 && fuelFlow <80000)
                {
                    _fuelFlowOutputSignal.State = (_fuelFlowInputSignal.State / 99000.00) * 10.00;
                }
                else if (fuelFlow >=80000)
                {
                    _fuelFlowOutputSignal.State = (8000/9900.00) * 10.00;
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
        ~Simtek100295HardwareSupportModule()
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