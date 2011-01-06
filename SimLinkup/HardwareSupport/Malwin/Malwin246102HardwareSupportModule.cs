using System;
using System.Collections.Generic;
using Common.MacroProgramming;
using Common.HardwareSupport;
using System.IO;
using log4net;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using SimLinkup.HardwareSupport.Simtek;

namespace SimLinkup.HardwareSupport.Malwin
{
    //Malwin 246102 F-16 Cabin Pressure Altimeter
    public class Malwin246102HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables
        private static ILog _log = LogManager.GetLogger(typeof(Malwin246102HardwareSupportModule));
        #endregion

        #region Instance variables
        private bool _isDisposed = false;
        private AnalogSignal _cabinPressureAltitudeInputSignal = null;
        private AnalogSignal.AnalogSignalChangedEventHandler _cabinPressureAltitudeInputSignalChangedEventHandler = null;
        private AnalogSignal _cabinPressureAltitudeSinOutputSignal = null;
        private AnalogSignal _cabinPressureAltitudeCosOutputSignal = null;

        #endregion

        #region Constructors
        private Malwin246102HardwareSupportModule()
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
                return "Malwin P/N 246102 - Cabin Pressure Altimeter";
            }
        }
        public static IHardwareSupportModule[] GetInstances()
        {
            List<IHardwareSupportModule> toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Malwin246102HardwareSupportModule());
            try
            {
                string hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory, "Malwin246102HardwareSupportModuleConfig.config");
                Malwin246102HardwareSupportModuleConfig hsmConfig = Malwin246102HardwareSupportModuleConfig.Load(hsmConfigFilePath);
                
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
                return new AnalogSignal[] { _cabinPressureAltitudeInputSignal };
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
                return new AnalogSignal[] { _cabinPressureAltitudeSinOutputSignal, _cabinPressureAltitudeCosOutputSignal};
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
            _cabinPressureAltitudeInputSignalChangedEventHandler = new AnalogSignal.AnalogSignalChangedEventHandler(cabinPressureAltitude_InputSignalChanged);
        }
        private void AbandonInputEventHandlers()
        {
            _cabinPressureAltitudeInputSignalChangedEventHandler = null;
        }
        private void RegisterForInputEvents()
        {
            if (_cabinPressureAltitudeInputSignal != null)
            {
                _cabinPressureAltitudeInputSignal.SignalChanged += _cabinPressureAltitudeInputSignalChangedEventHandler;
            }
        }
        private void UnregisterForInputEvents()
        {
            if (_cabinPressureAltitudeInputSignalChangedEventHandler != null && _cabinPressureAltitudeInputSignal != null)
            {
                try
                {
                    _cabinPressureAltitudeInputSignal.SignalChanged -= _cabinPressureAltitudeInputSignalChangedEventHandler;
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
            _cabinPressureAltitudeInputSignal = CreateCabinPressureAltitudeInputSignal();
        }
        private AnalogSignal CreateCabinPressureAltitudeInputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Cabin Pressure Altitude Value from Simulation";
            thisSignal.Id = "246102_Cabin_Pressure_Altitude_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }

        private void CreateOutputSignals()
        {
            _cabinPressureAltitudeSinOutputSignal = CreateCabinPressureAltitudeSinOutputSignal();
            _cabinPressureAltitudeCosOutputSignal = CreateCabinPressureAltitudeCosOutputSignal();
        }
        private AnalogSignal CreateCabinPressureAltitudeSinOutputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Cabin Pressure Altitude SIN Signal To Instrument";
            thisSignal.Id = "246102_Cabin_Pressure_Altitude_SIN_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = (0.00 + 10.00)/20.00;
            return thisSignal;
        }
        private AnalogSignal CreateCabinPressureAltitudeCosOutputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Cabin Pressure Altitude COS Signal To Instrument";
            thisSignal.Id = "246102_Cabin_Pressure_Altitude_COS_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = (10.00 + 10.00) / 20.00;
            return thisSignal;
        }
        private void cabinPressureAltitude_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateCabinPressureAltitudeOutputValues();
        }
        private void UpdateCabinPressureAltitudeOutputValues()
        {
            if (_cabinPressureAltitudeInputSignal != null)
            {
                double cabinPressureAltitudeInput = _cabinPressureAltitudeInputSignal.State;
                double cabinPressureAltitudeSinOutputValue = 0.0000;
                double cabinPressureAltitudeCosOutputValue = 0.0000;
                double degrees = 0.00;


                if (cabinPressureAltitudeInput < 0.0000)
                {
                    degrees = 0.0000;
                }
                else if (cabinPressureAltitudeInput >= 0 && cabinPressureAltitudeInput <= 50000.0000)
                {
                    degrees = (cabinPressureAltitudeInput / 50000.0000) * 300.0000;
                }
                else 
                {
                    degrees = 300.0;
                }

                cabinPressureAltitudeSinOutputValue = 10.0000 * Math.Sin(degrees * Common.Math.Constants.RADIANS_PER_DEGREE);
                cabinPressureAltitudeCosOutputValue = 10.0000 * Math.Cos(degrees * Common.Math.Constants.RADIANS_PER_DEGREE);


                if (_cabinPressureAltitudeSinOutputSignal != null)
                {

                    if (cabinPressureAltitudeSinOutputValue < -10)
                    {
                        cabinPressureAltitudeSinOutputValue = -10;
                    }
                    else if (cabinPressureAltitudeSinOutputValue > 10)
                    {
                        cabinPressureAltitudeSinOutputValue = 10;
                    }

                    _cabinPressureAltitudeSinOutputSignal.State = ((cabinPressureAltitudeSinOutputValue + 10.0000) / 20.0000);
                }

                if (_cabinPressureAltitudeCosOutputSignal != null)
                {

                    if (cabinPressureAltitudeCosOutputValue < -10)
                    {
                        cabinPressureAltitudeCosOutputValue = -10;
                    }
                    else if (cabinPressureAltitudeCosOutputValue > 10)
                    {
                        cabinPressureAltitudeCosOutputValue = 10;
                    }

                    _cabinPressureAltitudeCosOutputSignal.State = ((cabinPressureAltitudeCosOutputValue + 10.0000) / 20.0000);
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
        ~Malwin246102HardwareSupportModule()
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
