using System;
using System.Collections.Generic;
using Common.MacroProgramming;
using Common.HardwareSupport;
using System.IO;
using log4net;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-0216 F-16 FTIT Indicator
    public class Simtek100216HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables
        private static ILog _log = LogManager.GetLogger(typeof(Simtek100216HardwareSupportModule ));
        #endregion

        #region Instance variables
        private bool _isDisposed = false;
        private AnalogSignal _ftitInputSignal = null;
        private AnalogSignal.AnalogSignalChangedEventHandler _ftitInputSignalChangedEventHandler = null;
        private AnalogSignal _ftitOutputSignal = null;
        #endregion

        #region Constructors
        private Simtek100216HardwareSupportModule ()
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
                return "Simtek P/N 10-0216 - Indicator, Simulated FTIT";
            }
        }
        public static IHardwareSupportModule[] GetInstances()
        {
            List<IHardwareSupportModule> toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek100216HardwareSupportModule ());
            try
            {
                string hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory, "Simtek100216HardwareSupportModule.config");
                Simtek100216HardwareSupportModuleConfig hsmConfig = Simtek100216HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
                return new AnalogSignal[] { _ftitInputSignal };
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
                return new AnalogSignal[] { _ftitOutputSignal };
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
            _ftitInputSignalChangedEventHandler = new AnalogSignal.AnalogSignalChangedEventHandler(ftit_InputSignalChanged);
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
                catch (RemotingException ex)
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
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "FTIT Signal To Instrument";
            thisSignal.Id = "100216_FTIT_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = (-10.00 + 10.00) / 20.00; ;
            return thisSignal;
        }

        private AnalogSignal CreateFTITInputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "FTIT Value from Simulation";
            thisSignal.Id = "100216_FTIT_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
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
                double ftitInput = _ftitInputSignal.State;
                double ftitOutputValue = 0;
                if (_ftitOutputSignal != null)
                {
                    if (ftitInput <=200)
                    {
                        ftitOutputValue = -10;
                    }
                    else if (ftitInput >= 200 && ftitInput <700)
                    {
                        ftitOutputValue = -10 + (((ftitInput - 200) / 500) * 6.25);
                    }
                    else if (ftitInput >= 700 && ftitInput < 1000)
                    {
                        ftitOutputValue = -3.75 + (((ftitInput - 700) / 300.0) * 11.25);
                    }
                    else if (ftitInput >= 1000 && ftitInput < 1200)
                    {
                        ftitOutputValue = 7.5 + (((ftitInput - 1000) / 200.0) * 2.5);
                    }
                    else if (ftitInput >=1200)
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

                    _ftitOutputSignal.State = ((ftitOutputValue + 10.0000) / 20.0000);

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
        ~Simtek100216HardwareSupportModule()
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
