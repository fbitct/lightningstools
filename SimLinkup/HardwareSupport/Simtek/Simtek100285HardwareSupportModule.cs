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
    //Simtek 10-0285 F-16 Altimeter
    public class Simtek100285HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Simtek100285HardwareSupportModule));

        #endregion

        #region Instance variables

        private AnalogSignal _altitudeCoarseCosOutputSignal;
        private AnalogSignal _altitudeCoarseSinOutputSignal;
        private AnalogSignal _altitudeFineCosOutputSignal;
        private AnalogSignal _altitudeFineSinOutputSignal;
        private AnalogSignal _altitudeInputSignal;
        private AnalogSignal _barometricPressureInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _altitudeInputSignalChangedEventHandler;
        private AnalogSignal.AnalogSignalChangedEventHandler _barometricPressureInputSignalChangedEventHandler;
   
        private bool _isDisposed;

        #endregion

        #region Constructors

        private Simtek100285HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-0285 - Simulated Altimeter"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek100285HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "Simtek100285HardwareSupportModuleConfig.config");
                var hsmConfig =
                    Simtek100285HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] {_altitudeInputSignal, _barometricPressureInputSignal}; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get
            {
                return new[]
                {
                    _altitudeFineSinOutputSignal, _altitudeFineCosOutputSignal,
                    _altitudeCoarseSinOutputSignal,
                    _altitudeCoarseCosOutputSignal
                };
            }
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
            _altitudeInputSignalChangedEventHandler =
                altitude_InputSignalChanged;

            _barometricPressureInputSignalChangedEventHandler = barometricPressure_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _altitudeInputSignalChangedEventHandler = null;
            _barometricPressureInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_altitudeInputSignal != null)
            {
                _altitudeInputSignal.SignalChanged += _altitudeInputSignalChangedEventHandler;
            }
            if (_barometricPressureInputSignal != null)
            {
                _barometricPressureInputSignal.SignalChanged += _barometricPressureInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_altitudeInputSignalChangedEventHandler != null && _altitudeInputSignal != null)
            {
                try
                {
                    _altitudeInputSignal.SignalChanged -= _altitudeInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_barometricPressureInputSignalChangedEventHandler != null && _barometricPressureInputSignal != null)
            {
                try
                {
                    _barometricPressureInputSignal.SignalChanged -= _barometricPressureInputSignalChangedEventHandler;
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
            _altitudeInputSignal = CreateAltitudeInputSignal();
            _barometricPressureInputSignal = CreateBarometricPressureInputSignal();
        }

        private AnalogSignal CreateAltitudeInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Altitude (Indicated)";
            thisSignal.Id = "100285_Altitude_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }
        private AnalogSignal CreateBarometricPressureInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Barometric Pressure (Indicated), In. Hg.";
            thisSignal.Id = "100285_Barometric_Pressure_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 29.92;
            return thisSignal;
        }

        private void CreateOutputSignals()
        {
            _altitudeFineSinOutputSignal = CreateAltitudeFineSinOutputSignal();
            _altitudeFineCosOutputSignal = CreateAltitudeFineCosOutputSignal();
            _altitudeCoarseSinOutputSignal = CreateAltitudeCoarseSinOutputSignal();
            _altitudeCoarseCosOutputSignal = CreateAltitudeCoarseCosOutputSignal();
        }

        private AnalogSignal CreateAltitudeFineSinOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Altitude Fine SIN";
            thisSignal.Id = "100285_Altitude_Fine_SIN_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = (0.00 + 10.00)/20.00;
            return thisSignal;
        }

        private AnalogSignal CreateAltitudeFineCosOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Altitude Fine COS";
            thisSignal.Id = "100285_Altitude_Fine_COS_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = (10.00 + 10.00)/20.00;
            return thisSignal;
        }

        private AnalogSignal CreateAltitudeCoarseSinOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Altitude Coarse SIN";
            thisSignal.Id = "100285_Altitude_Coarse_SIN_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = (0.00 + 10.00)/20.00;
            return thisSignal;
        }

        private AnalogSignal CreateAltitudeCoarseCosOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Altitude Coarse COS";
            thisSignal.Id = "100285_Altitude_Coarse_COS_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = (10.00 + 10.00)/20.00;
            return thisSignal;
        }

        private void altitude_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateAltitudeOutputValues();
        }
        private void barometricPressure_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateAltitudeOutputValues();
        }

        private void UpdateAltitudeOutputValues()
        {
            if (_altitudeInputSignal != null)
            {
                var altitudeInput = _altitudeInputSignal.State;
                var baroInput = _barometricPressureInputSignal.State;
                var altCalDelta = baroInput - 29.92f;
                var altToAdd = -(1000 / 1.08) * altCalDelta;
                var altitudeOutput = altitudeInput + altToAdd;
               
                double altitudeFineSinOutputValue = 0;
                double altitudeFineCosOutputValue = 0;
                double altitudeCoarseSinOutputValue = 0;
                double altitudeCoarseCosOutputValue = 0;

                var numRevolutionsOfFineResolver = altitudeOutput / 4000;
                var numRevolutionsOfCoarseResolver = altitudeOutput / 100000;

                var fineResolverDegrees = numRevolutionsOfFineResolver*360;
                var coarseResolverDegrees = numRevolutionsOfCoarseResolver*360;

                altitudeFineSinOutputValue = 10.0000*Math.Sin(fineResolverDegrees*Constants.RADIANS_PER_DEGREE);
                altitudeFineCosOutputValue = 10.0000*Math.Cos(fineResolverDegrees*Constants.RADIANS_PER_DEGREE);

                altitudeCoarseSinOutputValue = 10.0000*Math.Sin(coarseResolverDegrees*Constants.RADIANS_PER_DEGREE);
                altitudeCoarseCosOutputValue = 10.0000*Math.Cos(coarseResolverDegrees*Constants.RADIANS_PER_DEGREE);


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

                    _altitudeFineSinOutputSignal.State = ((altitudeFineSinOutputValue + 10.0000)/20.0000);
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

                    _altitudeFineCosOutputSignal.State = ((altitudeFineCosOutputValue + 10.0000)/20.0000);
                }

                if (_altitudeCoarseSinOutputSignal != null)
                {
                    if (altitudeCoarseSinOutputValue < -10)
                    {
                        altitudeCoarseSinOutputValue = -10;
                    }
                    else if (altitudeCoarseSinOutputValue > 10)
                    {
                        altitudeCoarseSinOutputValue = 10;
                    }

                    _altitudeCoarseSinOutputSignal.State = ((altitudeCoarseSinOutputValue + 10.0000)/20.0000);
                }

                if (_altitudeCoarseCosOutputSignal != null)
                {
                    if (altitudeCoarseCosOutputValue < -10)
                    {
                        altitudeCoarseCosOutputValue = -10;
                    }
                    else if (altitudeCoarseCosOutputValue > 10)
                    {
                        altitudeCoarseCosOutputValue = 10;
                    }

                    _altitudeCoarseCosOutputSignal.State = ((altitudeCoarseCosOutputValue + 10.0000)/20.0000);
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
        ~Simtek100285HardwareSupportModule()
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