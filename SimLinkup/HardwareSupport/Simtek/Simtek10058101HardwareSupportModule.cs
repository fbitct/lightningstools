using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-0581-01 F-16 AOA Indicator
    public class Simtek10058101HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Simtek10058101HardwareSupportModule));

        #endregion

        #region Instance variables

        private AnalogSignal _aoaInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _aoaInputSignalChangedEventHandler;

        private AnalogSignal _aoaOutputSignal;
        private DigitalSignal.SignalChangedEventHandler _aoaPowerInputSignalChangedEventHandler;
        private DigitalSignal _aoaPowerOffInputSignal;
        private bool _isDisposed;

        #endregion

        #region Constructors

        private Simtek10058101HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-0581-01 - Indicator - Simulated Angle Of Attack Indicator"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek10058101HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                                                     "Simtek10058101HardwareSupportModule.config");
                var hsmConfig =
                    Simtek10058101HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] {_aoaInputSignal}; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return new[] {_aoaPowerOffInputSignal}; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] {_aoaOutputSignal}; }
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
            _aoaInputSignalChangedEventHandler = new AnalogSignal.AnalogSignalChangedEventHandler(AOA_InputSignalChanged);
            _aoaPowerInputSignalChangedEventHandler =
                new DigitalSignal.SignalChangedEventHandler(AOAPower_InputSignalChanged);
        }

        private void AbandonInputEventHandlers()
        {
            _aoaInputSignalChangedEventHandler = null;
            _aoaPowerInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_aoaInputSignal != null)
            {
                _aoaInputSignal.SignalChanged += _aoaInputSignalChangedEventHandler;
            }
            if (_aoaPowerOffInputSignal != null)
            {
                _aoaPowerOffInputSignal.SignalChanged += _aoaPowerInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_aoaInputSignalChangedEventHandler != null && _aoaInputSignal != null)
            {
                try
                {
                    _aoaInputSignal.SignalChanged -= _aoaInputSignalChangedEventHandler;
                }
                catch (RemotingException )
                {
                }
            }
            if (_aoaPowerInputSignalChangedEventHandler != null && _aoaPowerOffInputSignal != null)
            {
                try
                {
                    _aoaPowerOffInputSignal.SignalChanged -= _aoaPowerInputSignalChangedEventHandler;
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
            _aoaInputSignal = CreateAOAInputSignal();
            _aoaPowerOffInputSignal = CreateAOAPowerInputSignal();
        }

        private void CreateOutputSignals()
        {
            _aoaOutputSignal = CreateAOAOutputSignal();
        }

        private AnalogSignal CreateAOAOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "AOA";
            thisSignal.Id = "10058101_AOA_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = (-10.00 + 10.00)/20.00;
            return thisSignal;
        }

        private AnalogSignal CreateAOAInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "AOA";
            thisSignal.Id = "10058101_AOA_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }

        private DigitalSignal CreateAOAPowerInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "AOA Power Off Flag";
            thisSignal.Id = "10058101_AOA_Power_Off_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;
            return thisSignal;
        }

        private void AOA_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void AOAPower_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void UpdateOutputValues()
        {
            var aoaPowerOff = false;
            if (_aoaPowerOffInputSignal != null)
            {
                aoaPowerOff = _aoaPowerOffInputSignal.State;
            }

            if (_aoaInputSignal != null)
            {
                var aoaInput = _aoaInputSignal.State;
                double aoaOutputValue = 0;

                if (_aoaOutputSignal != null)
                {
                    if (aoaPowerOff)
                    {
                        aoaOutputValue = -10;
                    }
                    else
                    {
                        if (aoaInput < -5)
                        {
                            aoaOutputValue = -6.37;
                        }
                        else if (aoaInput >= -5 && aoaInput <= 40)
                        {
                            aoaOutputValue = -6.37 + (((aoaInput + 5)/45)*16.37);
                        }
                    }

                    if (aoaOutputValue < -10)
                    {
                        aoaOutputValue = -10;
                    }
                    else if (aoaOutputValue > 10)
                    {
                        aoaOutputValue = 10;
                    }

                    _aoaOutputSignal.State = ((aoaOutputValue + 10.0000)/20.0000);
                }
            }
        }

        #endregion

        #endregion

        #region Destructors

        /// <summary>
        ///   Public implementation of IDisposable.Dispose().  Cleans up 
        ///   managed and unmanaged resources used by this 
        ///   object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Standard finalizer, which will call Dispose() if this object 
        ///   is not manually disposed.  Ordinarily called only 
        ///   by the garbage collector.
        /// </summary>
        ~Simtek10058101HardwareSupportModule()
        {
            Dispose();
        }

        /// <summary>
        ///   Private implementation of Dispose()
        /// </summary>
        /// <param name = "disposing">flag to indicate if we should actually
        ///   perform disposal.  Distinguishes the private method signature 
        ///   from the public signature.</param>
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