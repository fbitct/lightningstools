using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using Common.Math;
using log4net;

namespace SimLinkup.HardwareSupport.Malwin
{
    //Malwin 19581 F-16 HYDRAULIC PRESSURE IND
    public class Malwin19581HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof(Malwin19581HardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;
        private AnalogSignal _hydPressureAInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _hydPressureAInputSignalChangedEventHandler;
        private AnalogSignal _hydPressureASINOutputSignal;
        private AnalogSignal _hydPressureACOSOutputSignal;
        private AnalogSignal _hydPressureBInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _hydPressureBInputSignalChangedEventHandler;
        private AnalogSignal _hydPressureBSINOutputSignal;
        private AnalogSignal _hydPressureBCOSOutputSignal;

        #endregion

        #region Constructors

        private Malwin19581HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Malwin P/N 19581 - Hydraulic Pressure Ind"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Malwin19581HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "Malwin19581HardwareSupportModule.config");
                var hsmConfig =
                    Malwin19581HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] { _hydPressureAInputSignal, _hydPressureBInputSignal }; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] { _hydPressureASINOutputSignal, _hydPressureACOSOutputSignal, _hydPressureBSINOutputSignal, _hydPressureBCOSOutputSignal }; }
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
            _hydPressureAInputSignalChangedEventHandler =
                hydPressureA_InputSignalChanged;

            _hydPressureBInputSignalChangedEventHandler =
                hydPressureB_InputSignalChanged;

        }

        private void AbandonInputEventHandlers()
        {
            _hydPressureAInputSignalChangedEventHandler = null;
            _hydPressureBInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_hydPressureAInputSignal != null)
            {
                _hydPressureAInputSignal.SignalChanged += _hydPressureAInputSignalChangedEventHandler;
            }
            if (_hydPressureBInputSignal != null)
            {
                _hydPressureBInputSignal.SignalChanged += _hydPressureBInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_hydPressureAInputSignalChangedEventHandler != null && _hydPressureAInputSignal != null)
            {
                try
                {
                    _hydPressureAInputSignal.SignalChanged -= _hydPressureAInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }

            if (_hydPressureBInputSignalChangedEventHandler != null && _hydPressureBInputSignal != null)
            {
                try
                {
                    _hydPressureBInputSignal.SignalChanged -= _hydPressureBInputSignalChangedEventHandler;
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
            _hydPressureAInputSignal = CreateHydraulicPressureAInputSignal();
            _hydPressureBInputSignal = CreateHydraulicPressureBInputSignal();
        }

        private void CreateOutputSignals()
        {
            _hydPressureASINOutputSignal = CreateHydPressureASINOutputSignal();
            _hydPressureACOSOutputSignal = CreateHydPressureACOSOutputSignal();

            _hydPressureBSINOutputSignal = CreateHydPressureBSINOutputSignal();
            _hydPressureBCOSOutputSignal = CreateHydPressureBCOSOutputSignal();
        }

        private AnalogSignal CreateHydPressureASINOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Hydraulic Pressure A (SIN)";
            thisSignal.Id = "19581_Hydraulic_Pressure_A_SIN_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.IsSine = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }
        private AnalogSignal CreateHydPressureBSINOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Hydraulic Pressure B (SIN)";
            thisSignal.Id = "19581_Hydraulic_Pressure_B_SIN_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.IsSine = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateHydPressureACOSOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Hydraulic Pressure A (COS)";
            thisSignal.Id = "19581_Hydraulic_Pressure_A_COS_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 10.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.IsCosine = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateHydPressureBCOSOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Hydraulic Pressure B (COS)";
            thisSignal.Id = "19581_Hydraulic_Pressure_B_COS_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 10.00; //volts
            thisSignal.IsVoltage = true;
            thisSignal.IsCosine = true;
            thisSignal.MinValue = -10;
            thisSignal.MaxValue = 10;
            return thisSignal;
        }

        private AnalogSignal CreateHydraulicPressureAInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Hydraulic Pressure A";
            thisSignal.Id = "19581_Hydraulic_Pressure_A_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsSine = true;
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 4000;

            return thisSignal;
        }
        private AnalogSignal CreateHydraulicPressureBInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Hydraulic Pressure B";
            thisSignal.Id = "19581_Hydraulic_Pressure_B_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 4000;

            return thisSignal;
        }

        private void hydPressureA_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateHydAOutputValues();
        }
        private void hydPressureB_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateHydBOutputValues();
        }

        private void UpdateHydAOutputValues()
        {
            if (_hydPressureAInputSignal != null)
            {
                var hydPressureAInput = _hydPressureAInputSignal.State;
                double hydPressureASINOutputValue = 0;
                if (_hydPressureASINOutputSignal != null)
                {
                    if (hydPressureAInput < 0)
                    {
                        hydPressureASINOutputValue = 0;
                    }
                    else if (hydPressureAInput > 4000)
                    {
                        hydPressureASINOutputValue = 10.0000 * Math.Sin(320.0000 * Constants.RADIANS_PER_DEGREE);
                    }
                    else
                    {
                        hydPressureASINOutputValue = 10.0000 *
                                                 Math.Sin(((hydPressureAInput / 4000) * 320.0000) *
                                                          Constants.RADIANS_PER_DEGREE);
                    }

                    if (hydPressureASINOutputValue < -10)
                    {
                        hydPressureASINOutputValue = -10;
                    }
                    else if (hydPressureASINOutputValue > 10)
                    {
                        hydPressureASINOutputValue = 10;
                    }

                    _hydPressureASINOutputSignal.State = hydPressureASINOutputValue;
                }

                if (_hydPressureACOSOutputSignal != null)
                {
                    double hydPressureACOSOutputValue = 0;
                    if (hydPressureAInput < 0)
                    {
                        hydPressureACOSOutputValue = 0;
                    }
                    else if (hydPressureAInput > 4000)
                    {
                        hydPressureACOSOutputValue = 10.0000 * Math.Cos(320.0000 * Constants.RADIANS_PER_DEGREE);
                    }
                    else
                    {
                        hydPressureACOSOutputValue = 10.0000 *
                                                 Math.Cos(((hydPressureAInput / 4000.0000) * 320.0000) *
                                                          Constants.RADIANS_PER_DEGREE);
                    }

                    if (hydPressureACOSOutputValue < -10)
                    {
                        hydPressureACOSOutputValue = -10;
                    }
                    else if (hydPressureACOSOutputValue > 10)
                    {
                        hydPressureACOSOutputValue = 10;
                    }

                    _hydPressureACOSOutputSignal.State = hydPressureACOSOutputValue;
                }

            }
        }
        private void UpdateHydBOutputValues()
        {
            if (_hydPressureBInputSignal != null)
            {
                var hydPressureBInput = _hydPressureBInputSignal.State;
                double hydPressureBSINOutputValue = 0;
                if (_hydPressureBSINOutputSignal != null)
                {
                    if (hydPressureBInput < 0)
                    {
                        hydPressureBSINOutputValue = 0;
                    }
                    else if (hydPressureBInput > 4000)
                    {
                        hydPressureBSINOutputValue = 10.0000 * Math.Sin(320.0000 * Constants.RADIANS_PER_DEGREE);
                    }
                    else
                    {
                        hydPressureBSINOutputValue = 10.0000 *
                                                 Math.Sin(((hydPressureBInput / 4000.0000) * 320.0000) *
                                                          Constants.RADIANS_PER_DEGREE);
                    }

                    if (hydPressureBSINOutputValue < -10)
                    {
                        hydPressureBSINOutputValue = -10;
                    }
                    else if (hydPressureBSINOutputValue > 10)
                    {
                        hydPressureBSINOutputValue = 10;
                    }

                    _hydPressureBSINOutputSignal.State = hydPressureBSINOutputValue;
                }

                if (_hydPressureBCOSOutputSignal != null)
                {
                    double hydPressureBCOSOutputValue = 0;
                    if (hydPressureBInput < 0)
                    {
                        hydPressureBCOSOutputValue = 0;
                    }
                    else if (hydPressureBInput > 4000)
                    {
                        hydPressureBCOSOutputValue = 10.0000 * Math.Cos(320.0000 * Constants.RADIANS_PER_DEGREE);
                    }
                    else
                    {
                        hydPressureBCOSOutputValue = 10.0000 *
                                                 Math.Cos(((hydPressureBInput / 4000.0000) * 320.0000) *
                                                          Constants.RADIANS_PER_DEGREE);
                    }

                    if (hydPressureBCOSOutputValue < -10)
                    {
                        hydPressureBCOSOutputValue = -10;
                    }
                    else if (hydPressureBCOSOutputValue > 10)
                    {
                        hydPressureBCOSOutputValue = 10;
                    }

                    _hydPressureBCOSOutputSignal.State = hydPressureBCOSOutputValue;
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
        ~Malwin19581HardwareSupportModule()
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