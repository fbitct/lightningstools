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
    //Simtek 10-1081 F-16 Altimeter
    public class Simtek101081HardwareSupportModule:HardwareSupportModuleBase, IDisposable
    {
        #region Class variables
        private static ILog _log = LogManager.GetLogger(typeof(Simtek101081HardwareSupportModule));
        #endregion

        #region Instance variables
        private bool _isDisposed=false;
        private AnalogSignal _altitudeInputSignal= null;
        private AnalogSignal.AnalogSignalChangedEventHandler _altitudeInputSignalChangedEventHandler = null;
        private AnalogSignal _altitudeFineSinOutputSignal = null;
        private AnalogSignal _altitudeFineCosOutputSignal = null;
        private AnalogSignal _altitudeCoarseOutputSignal = null;

        #endregion

        #region Constructors
        private Simtek101081HardwareSupportModule():base()
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
                return "Simtek P/N 10-1081 - Altimeter";
            }
        }
        public static IHardwareSupportModule[] GetInstances()
        {
            List<IHardwareSupportModule> toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek101081HardwareSupportModule());
            try
            {
                string hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory, "Simtek101081HardwareSupportModuleConfig.config");
                Simtek101081HardwareSupportModuleConfig hsmConfig = Simtek101081HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
                return new AnalogSignal[] { _altitudeInputSignal };
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
                return new AnalogSignal[] { _altitudeFineSinOutputSignal, _altitudeFineCosOutputSignal, _altitudeCoarseOutputSignal};
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
            _altitudeInputSignalChangedEventHandler = new AnalogSignal.AnalogSignalChangedEventHandler(altitude_InputSignalChanged);
        }
        private void AbandonInputEventHandlers()
        {
            _altitudeInputSignalChangedEventHandler = null;
        }
        private void RegisterForInputEvents()
        {
            if (_altitudeInputSignal != null)
            {
                _altitudeInputSignal.SignalChanged += _altitudeInputSignalChangedEventHandler;
            }
        }
        private void UnregisterForInputEvents()
        {
            if (_altitudeInputSignalChangedEventHandler != null && _altitudeInputSignal !=null)
            {
                try
                {
                    _altitudeInputSignal.SignalChanged -= _altitudeInputSignalChangedEventHandler;
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
            _altitudeInputSignal= CreateAltitudeInputSignal();
        }
        private AnalogSignal CreateAltitudeInputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Altitude (Indicated) Value from Simulation";
            thisSignal.Id = "101081_Altitude_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }

        private void CreateOutputSignals()
        {
            _altitudeFineSinOutputSignal= CreateAltitudeFineSinOutputSignal();
            _altitudeFineCosOutputSignal = CreateAltitudeFineCosOutputSignal();
            _altitudeCoarseOutputSignal = CreateAltitudeCoarseSinOutputSignal();
        }
        private AnalogSignal CreateAltitudeFineSinOutputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Altitude Fine SIN Signal To Instrument";
            thisSignal.Id = "101081_Altitude_Fine_SIN_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }
        private AnalogSignal CreateAltitudeFineCosOutputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Altitude Fine COS Signal To Instrument";
            thisSignal.Id = "101081_Altitude_Fine_COS_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 10;
            return thisSignal;
        }
        private AnalogSignal CreateAltitudeCoarseSinOutputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Altitude Coarse Signal To Instrument";
            thisSignal.Id = "101081_Altitude_Coarse_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }
        private void altitude_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateAltitudeOutputValues();
        }
        private void UpdateAltitudeOutputValues()
        {
            if (_altitudeInputSignal != null)
            {
                double altitudeInput = _altitudeInputSignal.State;
                double altitudeFineSinOutputValue = 0;
                double altitudeFineCosOutputValue = 0;
                double altitudeCoarseOutputValue = 0;

                double numRevolutionsOfFineResolver = altitudeInput / 1000.0000;
                double fineResolverDegrees = numRevolutionsOfFineResolver * 360.0000;
                altitudeFineSinOutputValue = 10.0000 * Math.Sin(fineResolverDegrees * Common.Math.Constants.RADIANS_PER_DEGREE);
                altitudeFineCosOutputValue = 10.0000 * Math.Cos(fineResolverDegrees * Common.Math.Constants.RADIANS_PER_DEGREE);

                if (altitudeInput < -1000)
                {
                    altitudeCoarseOutputValue = -10.0000;
                }
                else if (altitudeInput >= -1000 && altitudeInput <0)
                {
                    altitudeCoarseOutputValue = -10.0000 + (((altitudeInput - (-1000.0000)) / 1000.0000) * 0.2500);
                }
                else if (altitudeInput >= 0 && altitudeInput < 80000)
                {
                    altitudeCoarseOutputValue = -9.7500 + ((altitudeInput / 1000.0000) * (19.7500 / 80.0000));
                }
                else if (altitudeInput >= 80000)
                {
                    altitudeCoarseOutputValue = 10;
                }

                if (_altitudeFineSinOutputSignal != null)
                {

                    if (altitudeFineSinOutputValue < -10)
                    {
                        altitudeFineSinOutputValue = -10;
                    }
                    else if (altitudeFineSinOutputValue > 10)
                    {
                        altitudeFineSinOutputValue = 10;
                    }

                    _altitudeFineSinOutputSignal.State = ((altitudeFineSinOutputValue +10.0000)/20.0000);
                }

                if (_altitudeFineCosOutputSignal != null)
                {

                    if (altitudeFineCosOutputValue < -10)
                    {
                        altitudeFineCosOutputValue = -10;
                    }
                    else if (altitudeFineCosOutputValue > 10)
                    {
                        altitudeFineCosOutputValue = 10;
                    }

                    _altitudeFineCosOutputSignal.State = ((altitudeFineCosOutputValue + 10.0000) / 20.0000);
                }

                if (_altitudeCoarseOutputSignal != null)
                {

                    if (altitudeCoarseOutputValue < -10)
                    {
                        altitudeCoarseOutputValue = -10;
                    }
                    else if (altitudeCoarseOutputValue > 10)
                    {
                        altitudeCoarseOutputValue = 10;
                    }

                    _altitudeCoarseOutputSignal.State = ((altitudeCoarseOutputValue + 10.0000) / 20.0000);
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
        ~Simtek101081HardwareSupportModule()
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
