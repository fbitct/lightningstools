using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using Common.Math;
using log4net;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-1091 F-16 ENGINE OIL PRESSURE IND
    public class Simtek101091HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Simtek101091HardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;
        private AnalogSignal _oilPressureInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _oilPressureInputSignalChangedEventHandler;
        private AnalogSignal _oilPressureOutputSignal;

        #endregion

        #region Constructors

        private Simtek101091HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-1091 - Engine Oil Pressure Ind"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek101091HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                                                     "Simtek101091HardwareSupportModule.config");
                var hsmConfig =
                    Simtek101091HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] {_oilPressureInputSignal}; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] {_oilPressureOutputSignal}; }
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
            _oilPressureInputSignalChangedEventHandler =
                new AnalogSignal.AnalogSignalChangedEventHandler(oil_InputSignalChanged);
        }

        private void AbandonInputEventHandlers()
        {
            _oilPressureInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_oilPressureInputSignal != null)
            {
                _oilPressureInputSignal.SignalChanged += _oilPressureInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_oilPressureInputSignalChangedEventHandler != null && _oilPressureInputSignal != null)
            {
                try
                {
                    _oilPressureInputSignal.SignalChanged -= _oilPressureInputSignalChangedEventHandler;
                }
                catch (RemotingException )
                {
                }
            }
        }

        #endregion

        #region Signal Creation

        private void CreateInputSignals()
        {
            _oilPressureInputSignal = CreateOilInputSignal();
        }

        private void CreateOutputSignals()
        {
            _oilPressureOutputSignal = CreateOilOutputSignal();
        }

        private AnalogSignal CreateOilOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Oil  Pressure";
            thisSignal.Id = "101091_Oil_Pressure_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = (-10.00 + 10.00)/20.00;
            return thisSignal;
        }

        private AnalogSignal CreateOilInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Oil Pressure";
            thisSignal.Id = "101091_Oil_Pressure_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;

            return thisSignal;
        }

        private void oil_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void UpdateOutputValues()
        {
            if (_oilPressureInputSignal != null)
            {
                var oilPressureInput = _oilPressureInputSignal.State;
                double oilPressureOutputValue = 0;
                if (_oilPressureOutputSignal != null)
                {
                    if (oilPressureInput < 0)
                    {
                        oilPressureOutputValue = 0;
                    }
                    else if (oilPressureInput > 100)
                    {
                        oilPressureOutputValue = 10.0000*Math.Sin(320.0000*Constants.RADIANS_PER_DEGREE);
                    }
                    else
                    {
                        oilPressureOutputValue = 10.0000*
                                                 Math.Sin(((oilPressureInput/100.0000)*320.0000)*
                                                          Constants.RADIANS_PER_DEGREE);
                    }

                    if (oilPressureOutputValue < -10)
                    {
                        oilPressureOutputValue = -10;
                    }
                    else if (oilPressureOutputValue > 10)
                    {
                        oilPressureOutputValue = 10;
                    }

                    _oilPressureOutputSignal.State = ((oilPressureOutputValue + 10.0000)/20.0000);
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
        ~Simtek101091HardwareSupportModule()
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