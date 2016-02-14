using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;
using LightningGauges.Renderers.F16;
using System.Drawing;

namespace SimLinkup.HardwareSupport.AMI
{
    //AMI 9001584 F-16 Simulated Fuel Quantity Indicator
    public class AMI9001584HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof(AMI9001584HardwareSupportModule));

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

        private IFuelQuantityIndicator _renderer = new FuelQuantityIndicator();
        #endregion

        #region Constructors

        private AMI9001584HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
            UpdateOutputValues();
        }

        public override string FriendlyName
        {
            get { return "AMI P/N 9001584 - Indicator - Simulated Fuel Qty"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new AMI9001584HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "AMI9001584HardwareSupportModuleConfig.config");
                var hsmConfig =
                    AMI9001584HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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

        #region Visualization
        public override void Render(Graphics g, Rectangle destinationRectangle)
        {
            _renderer.InstrumentState.AftLeftFuelQuantityPounds = (float)_aftLeftFuelInputSignal.State;
            _renderer.InstrumentState.ForeRightFuelQuantityPounds = (float)_foreRightFuelInputSignal.State;
            _renderer.InstrumentState.TotalFuelQuantityPounds = (float)_totalFuelInputSignal.State;
            _renderer.Render(g, destinationRectangle);
        }
        #endregion

        #region Signals Handling

        #region Signals Event Handling

        private void CreateInputEventHandlers()
        {
            _totalFuelInputSignalChangedEventHandler =
                fuel_InputSignalChanged;
            _aftLeftFuelInputSignalChangedEventHandler =
                fuel_InputSignalChanged;
            _foreRightFuelInputSignalChangedEventHandler =
                fuel_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _totalFuelInputSignalChangedEventHandler = null;
            _foreRightFuelInputSignalChangedEventHandler = null;
            _aftLeftFuelInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_totalFuelInputSignal != null)
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
            if (_totalFuelInputSignalChangedEventHandler != null && _totalFuelInputSignal != null)
            {
                try
                {
                    _totalFuelInputSignal.SignalChanged -= _totalFuelInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }


            if (_aftLeftFuelInputSignalChangedEventHandler != null && _aftLeftFuelInputSignal != null)
            {
                try
                {
                    _aftLeftFuelInputSignal.SignalChanged -= _aftLeftFuelInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_foreRightFuelInputSignalChangedEventHandler != null && _foreRightFuelInputSignal != null)
            {
                try
                {
                    _foreRightFuelInputSignal.SignalChanged -= _foreRightFuelInputSignalChangedEventHandler;
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
            _totalFuelInputSignal = CreateTotalFuelInputSignal();
            _aftLeftFuelInputSignal = CreateAftLeftFuelInputSignal();
            _foreRightFuelInputSignal = CreateForeRightFuelInputSignal();
        }

        private AnalogSignal CreateTotalFuelInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Total Fuel (Pounds)";
            thisSignal.Id = "9001584_Total_Fuel_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 18000;
            return thisSignal;
        }

        private AnalogSignal CreateAftLeftFuelInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "A/L Fuel";
            thisSignal.Id = "9001584_AftAndLeft_Fuel_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 42000;
            return thisSignal;
        }

        private AnalogSignal CreateForeRightFuelInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "F/R Fuel";
            thisSignal.Id = "9001584_ForeAndRight_Fuel_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 42000;
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
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "F/R";
            thisSignal.Id = "9001584_FR_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = -7.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateAftLeftOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "A/L";
            thisSignal.Id = "9001584_AL_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = -7.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateCounterOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Counter";
            thisSignal.Id = "9001584_Counter_To_Instrument";
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

        private void fuel_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void UpdateOutputValues()
        {
            //NOTE: these values are correct for Nigel's modification to the AMI 9001584 to replace the 1-turn pot with a 3-turn pot for the needles to widen the range of indicated values
            if (_foreRightOutputSignal != null)
            {
                _foreRightOutputSignal.State = (((_foreRightFuelInputSignal.State / 100.00) / 42.00) * 14.00) - 7.00; //zero indicated at -7.00V; 4200 lbs indicated at +7V
            }
            if (_aftLeftOutputSignal != null)
            {
                _aftLeftOutputSignal.State = (((_aftLeftFuelInputSignal.State / 100.00) / 42.00) * 14.00) - 7.00;//zero indicated at -7.00V; 4200 lbs indicated at +7V
            }
            if (_counterOutputSignal != null)
            {
                _counterOutputSignal.State = ((_totalFuelInputSignal.State / 18000) * 20.00) - 10.00;
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
        ~AMI9001584HardwareSupportModule()
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