﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using Common.Math;
using log4net;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-0294 F-16 Simulated Fuel Quantity Indicator
    public class Simtek100294HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof(Simtek100294HardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;
        private AnalogSignal _totalFuelInputSignal;
        private AnalogSignal _foreRightFuelInputSignal;
        private AnalogSignal _aftLeftFuelInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _totalFuelInputSignalChangedEventHandler;
        private AnalogSignal.AnalogSignalChangedEventHandler _foreRightFuelInputSignalChangedEventHandler;
        private AnalogSignal.AnalogSignalChangedEventHandler _aftLeftFuelInputSignalChangedEventHandler;
        private AnalogSignal _aftLeftOutputSignal;
        private AnalogSignal _foreRightOutputSignal;
        private AnalogSignal _counterOutputSignal;

        #endregion

        #region Constructors

        private Simtek100294HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-0294 - Indicator - Simulated Fuel Qty"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek100294HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                                                     "Simtek100294HardwareSupportModuleConfig.config");
                var hsmConfig =
                    Simtek100294HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] { _totalFuelInputSignal, _foreRightFuelInputSignal, _aftLeftFuelInputSignal }; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] { _foreRightOutputSignal, _aftLeftOutputSignal, _counterOutputSignal }; }
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
            _totalFuelInputSignalChangedEventHandler =
                new AnalogSignal.AnalogSignalChangedEventHandler(fuel_InputSignalChanged);
            _aftLeftFuelInputSignalChangedEventHandler =
                new AnalogSignal.AnalogSignalChangedEventHandler(fuel_InputSignalChanged);
            _foreRightFuelInputSignalChangedEventHandler =
                new AnalogSignal.AnalogSignalChangedEventHandler(fuel_InputSignalChanged);
        }

        private void AbandonInputEventHandlers()
        {
            _totalFuelInputSignalChangedEventHandler = null;
            _foreRightFuelInputSignalChangedEventHandler = null;
            _aftLeftFuelInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_totalFuelInputSignal!= null)
            {
                _totalFuelInputSignal.SignalChanged += _totalFuelInputSignalChangedEventHandler;
            }
            if (_aftLeftFuelInputSignal != null)
            {
                _aftLeftFuelInputSignal.SignalChanged += _aftLeftFuelInputSignalChangedEventHandler;
            }
            if (_foreRightFuelInputSignal != null)
            {
                _foreRightFuelInputSignal.SignalChanged += _foreRightFuelInputSignalChangedEventHandler;
            }

        }

        private void UnregisterForInputEvents()
        {
            if (_totalFuelInputSignalChangedEventHandler != null && _totalFuelInputSignal !=null)
            {
                try
                {
                    _totalFuelInputSignal.SignalChanged -= _totalFuelInputSignalChangedEventHandler;
                }
                catch (RemotingException ex)
                {
                }
            }


            if (_aftLeftFuelInputSignalChangedEventHandler != null && _aftLeftFuelInputSignal!= null)
            {
                try
                {
                    _aftLeftFuelInputSignal.SignalChanged -= _aftLeftFuelInputSignalChangedEventHandler;
                }
                catch (RemotingException ex)
                {
                }
            }
            if (_foreRightFuelInputSignalChangedEventHandler  != null && _foreRightFuelInputSignal!= null)
            {
                try
                {
                    _foreRightFuelInputSignal.SignalChanged -= _foreRightFuelInputSignalChangedEventHandler;
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
            _totalFuelInputSignal = CreateTotalFuelInputSignal();
            _aftLeftFuelInputSignal = CreateAftLeftFuelInputSignal();
            _foreRightFuelInputSignal = CreateForeRightFuelInputSignal();
        }

        private AnalogSignal CreateTotalFuelInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Total Fuel (Pounds)";
            thisSignal.Id = "100294_Total_Fuel_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }
        private AnalogSignal CreateAftLeftFuelInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "A/L Fuel";
            thisSignal.Id = "100294_AftAndLeft_Fuel_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }
        private AnalogSignal CreateForeRightFuelInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "F/R Fuel";
            thisSignal.Id = "100294_ForeAndRight_Fuel_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }


        private void CreateOutputSignals()
        {
            _foreRightOutputSignal = CreateForeRightOutputSignal();
            _aftLeftOutputSignal = CreateAftLeftOutputSignal();
            _counterOutputSignal = CreateCounterOutputSignal();
        }

        private AnalogSignal CreateForeRightOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "F/R";
            thisSignal.Id = "100294_FR_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.00;
            return thisSignal;
        }

        private AnalogSignal CreateAftLeftOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "A/L";
            thisSignal.Id = "100294_AL_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.00;
            return thisSignal;
        }

        private AnalogSignal CreateCounterOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Counter";
            thisSignal.Id = "100294_Counter_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State =0.00;
            return thisSignal;
        }

        private void fuel_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void UpdateOutputValues()
        {
            if (_foreRightOutputSignal != null)
            {
                _foreRightOutputSignal.State = (_foreRightFuelInputSignal.State / 1000.00) / 42.00;
            }
            if (_aftLeftOutputSignal!= null)
            {
                _aftLeftOutputSignal.State = (_aftLeftFuelInputSignal.State / 1000.00) / 42.00;
            }
            if (_counterOutputSignal != null)
            {
                _counterOutputSignal.State = _totalFuelInputSignal.State / 9900;
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
        ~Simtek100294HardwareSupportModule()
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