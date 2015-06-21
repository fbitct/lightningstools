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
    //Simtek 10-1088 F-16 NOZZLE POSITION IND
    public class Simtek101088HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof(Simtek101088HardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;
        private AnalogSignal _nozzlePositionInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _nozzlePositionInputSignalChangedEventHandler;
        private AnalogSignal _nozzlePositionSINOutputSignal;
        private AnalogSignal _nozzlePositionCOSOutputSignal;

        #endregion

        #region Constructors

        private Simtek101088HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-1088 - Nozzle Position Ind"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek101088HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "Simtek101088HardwareSupportModule.config");
                var hsmConfig =
                    Simtek101088HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] { _nozzlePositionInputSignal }; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] { _nozzlePositionSINOutputSignal, _nozzlePositionCOSOutputSignal }; }
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
            _nozzlePositionInputSignalChangedEventHandler =
                nozzlePosition_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _nozzlePositionInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_nozzlePositionInputSignal != null)
            {
                _nozzlePositionInputSignal.SignalChanged += _nozzlePositionInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_nozzlePositionInputSignalChangedEventHandler != null && _nozzlePositionInputSignal != null)
            {
                try
                {
                    _nozzlePositionInputSignal.SignalChanged -= _nozzlePositionInputSignalChangedEventHandler;
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
            _nozzlePositionInputSignal = CreateNozzlePositionInputSignal();
        }

        private void CreateOutputSignals()
        {
            _nozzlePositionSINOutputSignal = CreateNozzlePositionSINOutputSignal();
            _nozzlePositionCOSOutputSignal = CreateNozzlePositionCOSOutputSignal();
        }

        private AnalogSignal CreateNozzlePositionSINOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Nozzle Position (SIN)";
            thisSignal.Id = "101088_Nozzle_Position_SIN_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.00; //volts
            thisSignal.IsVoltage = true;
            return thisSignal;
        }

        private AnalogSignal CreateNozzlePositionCOSOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Nozzle Position (COS)";
            thisSignal.Id = "101088_Nozzle_Position_COS_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 10.00; //volts
            thisSignal.IsVoltage = true;
            return thisSignal;
        }
        private AnalogSignal CreateNozzlePositionInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Nozzle Position";
            thisSignal.Id = "101088_Nozzle_Position_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;

            return thisSignal;
        }

        private void nozzlePosition_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }
        private void UpdateOutputValues()
        {
            if (_nozzlePositionInputSignal != null)
            {
                var nozzlePositionInput = _nozzlePositionInputSignal.State *100.000;

                double nozzlePositionSINOutputValue = 0;
                if (_nozzlePositionSINOutputSignal != null)
                {
                    if (nozzlePositionInput < 0)
                    {
                        nozzlePositionSINOutputValue = 0;
                    }
                    else if (nozzlePositionInput > 100)
                    {
                        nozzlePositionSINOutputValue = 10.0000 * Math.Sin(225.0000 * Constants.RADIANS_PER_DEGREE);
                    }
                    else
                    {
                        nozzlePositionSINOutputValue = 10.0000 *
                                                 Math.Sin(((nozzlePositionInput / 100.0000) * 225.0000) *
                                                          Constants.RADIANS_PER_DEGREE);
                    }

                    if (nozzlePositionSINOutputValue < -10)
                    {
                        nozzlePositionSINOutputValue = -10;
                    }
                    else if (nozzlePositionSINOutputValue > 10)
                    {
                        nozzlePositionSINOutputValue = 10;
                    }

                    _nozzlePositionSINOutputSignal.State = nozzlePositionSINOutputValue;
                }

                if (_nozzlePositionCOSOutputSignal != null)
                {
                    double nozzlePositionCOSOutputValue = 0;
                    if (nozzlePositionInput < 0)
                    {
                        nozzlePositionCOSOutputValue = 0;
                    }
                    else if (nozzlePositionInput > 100)
                    {
                        nozzlePositionCOSOutputValue = 10.0000 * Math.Cos(225.0000 * Constants.RADIANS_PER_DEGREE);
                    }
                    else
                    {
                        nozzlePositionCOSOutputValue = 10.0000 *
                                                 Math.Cos(((nozzlePositionInput / 100.0000) * 225.0000) *
                                                          Constants.RADIANS_PER_DEGREE);
                    }

                    if (nozzlePositionCOSOutputValue < -10)
                    {
                        nozzlePositionCOSOutputValue = -10;
                    }
                    else if (nozzlePositionCOSOutputValue > 10)
                    {
                        nozzlePositionCOSOutputValue = 10;
                    }

                    _nozzlePositionCOSOutputSignal.State = nozzlePositionCOSOutputValue;
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
        ~Simtek101088HardwareSupportModule()
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