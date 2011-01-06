using System;
using System.Collections.Generic;
using Common.MacroProgramming;
using Common.HardwareSupport;
using System.IO;
using log4net;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;

namespace SimLinkup.HardwareSupport.Lilbern
{
    //Lilbern 3239 F-16A Fuel Flow Indicator
    public class Lilbern3239HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables
        private static ILog _log = LogManager.GetLogger(typeof(Lilbern3239HardwareSupportModule));
        #endregion

        #region Instance variables
        private bool _isDisposed = false;
        private AnalogSignal _fuelFlowInputSignal = null;
        private AnalogSignal.AnalogSignalChangedEventHandler _fuelFlowInputSignalChangedEventHandler = null;
        private AnalogSignal _fuelFlowOutputSignal = null;

        #endregion

        #region Constructors
        private Lilbern3239HardwareSupportModule()
            : base()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();

        }

        public override string FriendlyName
        {
            get
            {
                return "Lilbern M/N 3239 - F-16A Fuel Flow Indicator";
            }
        }
        public static IHardwareSupportModule[] GetInstances()
        {
            List<IHardwareSupportModule> toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Lilbern3239HardwareSupportModule());
            try
            {
                string hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory, "Lilbern3239HardwareSupportModuleConfig.config");
                Lilbern3239HardwareSupportModuleConfig hsmConfig = Lilbern3239HardwareSupportModuleConfig.Load(hsmConfigFilePath);

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
                return new AnalogSignal[] { _fuelFlowInputSignal };
            }
        }
        public override DigitalSignal[] DigitalInputs
        {
            get
            {
                return null;
            }
        }
        public override AnalogSignal[] AnalogOutputs
        {
            get
            {
                return new AnalogSignal[] { _fuelFlowOutputSignal};
            }
        }
        public override DigitalSignal[] DigitalOutputs
        {
            get
            {
                return null;
            }
        }
        #endregion

        #region Signals Handling
        #region Signals Event Handling
        private void CreateInputEventHandlers()
        {
            _fuelFlowInputSignalChangedEventHandler = new AnalogSignal.AnalogSignalChangedEventHandler(fuelFlow_InputSignalChanged);
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
                catch (RemotingException ex)
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
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Fuel Flow Pounds Per Hour Value from Simulation";
            thisSignal.Id = "3239_Fuel_Flow_Pounds_Per_Hour_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }

        private void CreateOutputSignals()
        {
            _fuelFlowOutputSignal = CreateFuelFlowPoundsPerHourOutputSignal();
        }
        
        private AnalogSignal CreateFuelFlowPoundsPerHourOutputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Fuel Flow Pounds Per Hour Signal To Instrument";
            thisSignal.Id = "3239_Fuel_Flow_Pounds_Per_Hour_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = ((-10.00 + 10.0)/20.0);
            return thisSignal;
        }

        private void fuelFlow_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateFuelFlowOutputValues();
        }
        private void UpdateFuelFlowOutputValues()
        {
            if (_fuelFlowInputSignal != null)
            {
                double fuelFlowInput = _fuelFlowInputSignal.State;
                double fuelFlowOutputValue = 0;

                if (fuelFlowInput <= 0)
                {
                    fuelFlowOutputValue = -10.00;
                }
                else
                {
                    fuelFlowOutputValue = -10.00 + ((fuelFlowInput / 80000.0000) * 20.0000);
                }


                if (_fuelFlowOutputSignal != null)
                {

                    if (fuelFlowOutputValue < -10)
                    {
                        fuelFlowOutputValue = -10;
                    }
                    else if (fuelFlowOutputValue > 10)
                    {
                        fuelFlowOutputValue = 10;
                    }

                    _fuelFlowOutputSignal.State = ((fuelFlowOutputValue + 10.0000) / 20.0000);
                }

            }

        }
        #endregion

        #endregion

        #region Destructors
        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object 
        /// is not manually disposed.  Ordinarily called only 
        /// by the garbage collector.
        /// </summary>
        ~Lilbern3239HardwareSupportModule()
        {
            Dispose();
        }
        /// <summary>
        /// Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">flag to indicate if we should actually
        /// perform disposal.  Distinguishes the private method signature 
        /// from the public signature.</param>
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
        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up 
        /// managed and unmanaged resources used by this 
        /// object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
