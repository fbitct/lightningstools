using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;
using LightningGauges.Renderers.F16;
using System.Drawing;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-0216 F-16 FTIT Indicator
    public class Simtek100216HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Simtek100216HardwareSupportModule));

        #endregion

        #region Instance variables

        private AnalogSignal _ftitInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _ftitInputSignalChangedEventHandler;
        private AnalogSignal _ftitOutputSignal;

        private IFanTurbineInletTemperature _renderer = new FanTurbineInletTemperature();

        private bool _isDisposed;

        #endregion

        #region Constructors

        private Simtek100216HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-0216 - Indicator, Simulated FTIT"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek100216HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.CurrentMappingProfileDirectory,
                    "Simtek100216HardwareSupportModule.config");
                var hsmConfig =
                    Simtek100216HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] {_ftitInputSignal}; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] {_ftitOutputSignal}; }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get { return null; }
        }

        #endregion

        #region Visualization
        public override void Render(Graphics g, Rectangle destinationRectangle)
        {
            _renderer.InstrumentState.InletTemperatureDegreesCelcius = (float)_ftitInputSignal.State;
            _renderer.Render(g, destinationRectangle);
        }
        #endregion


        #region Signals Handling

        #region Signals Event Handling

        private void CreateInputEventHandlers()
        {
            _ftitInputSignalChangedEventHandler =
                ftit_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _ftitInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_ftitInputSignal != null)
            {
                _ftitInputSignal.SignalChanged += _ftitInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_ftitInputSignalChangedEventHandler != null && _ftitInputSignal != null)
            {
                try
                {
                    _ftitInputSignal.SignalChanged -= _ftitInputSignalChangedEventHandler;
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
            _ftitInputSignal = CreateFTITInputSignal();
        }

        private void CreateOutputSignals()
        {
            _ftitOutputSignal = CreateFTITOutputSignal();
        }

        private AnalogSignal CreateFTITOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "FTIT";
            thisSignal.Id = "100216_FTIT_To_Instrument";
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

        private AnalogSignal CreateFTITInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "FTIT";
            thisSignal.Id = "100216_FTIT_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 1200;
            return thisSignal;
        }

        private void ftit_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void UpdateOutputValues()
        {
            if (_ftitInputSignal != null)
            {
                var ftitInput = _ftitInputSignal.State;
                double ftitOutputValue = 0;
                if (_ftitOutputSignal != null)
                {
                    if (ftitInput <= 200)
                    {
                        ftitOutputValue = -10;
                    }
                    else if (ftitInput >= 200 && ftitInput < 700)
                    {
                        ftitOutputValue = -10 + (((ftitInput - 200)/500)*6.25);
                    }
                    else if (ftitInput >= 700 && ftitInput < 1000)
                    {
                        ftitOutputValue = -3.75 + (((ftitInput - 700)/300.0)*11.25);
                    }
                    else if (ftitInput >= 1000 && ftitInput < 1200)
                    {
                        ftitOutputValue = 7.5 + (((ftitInput - 1000)/200.0)*2.5);
                    }
                    else if (ftitInput >= 1200)
                    {
                        ftitOutputValue = 10;
                    }


                    if (ftitOutputValue < -10)
                    {
                        ftitOutputValue = -10;
                    }
                    else if (ftitOutputValue > 10)
                    {
                        ftitOutputValue = 10;
                    }
                    _ftitOutputSignal.State = ftitOutputValue;
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
        ~Simtek100216HardwareSupportModule()
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