﻿using System;
using System.Collections.Generic;
using Common.MacroProgramming;
using Common.HardwareSupport;
using System.IO;
using log4net;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-0216 F-16 VVI Indicator
    public class Simtek10058102HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables
        private static ILog _log = LogManager.GetLogger(typeof(Simtek10058102HardwareSupportModule ));
        #endregion

        #region Instance variables
        private bool _isDisposed = false;
        private AnalogSignal _vviInputSignal = null;
        private AnalogSignal.AnalogSignalChangedEventHandler _vviInputSignalChangedEventHandler = null;

        private DigitalSignal _vviPowerInputSignal = null;
        private DigitalSignal.SignalChangedEventHandler _vviPowerInputSignalChangedEventHandler = null;

        private AnalogSignal _vviOutputSignal = null;
        #endregion

        #region Constructors
        private Simtek10058102HardwareSupportModule ()
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
                return "Simtek P/N 10-0581-02 - Indicator - Simulated Vertical Velocity";
            }
        }
        public static IHardwareSupportModule[] GetInstances()
        {
            List<IHardwareSupportModule> toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek10058102HardwareSupportModule ());
            try
            {
                string hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory, "Simtek10058102HardwareSupportModule.config");
                Simtek10058102HardwareSupportModuleConfig hsmConfig = Simtek10058102HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
                return new AnalogSignal[] { _vviInputSignal };
            }
        }
        public override DigitalSignal[] DigitalInputs
        {
            get
            {
                return new DigitalSignal[] { _vviPowerInputSignal};
            }
        }
        public override AnalogSignal[] AnalogOutputs
        {
            get
            {
                return new AnalogSignal[] { _vviOutputSignal };
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
            _vviInputSignalChangedEventHandler = new AnalogSignal.AnalogSignalChangedEventHandler(vvi_InputSignalChanged);
            _vviPowerInputSignalChangedEventHandler = new DigitalSignal.SignalChangedEventHandler(vviPower_InputSignalChanged);
        }
        private void AbandonInputEventHandlers()
        {
            _vviInputSignalChangedEventHandler = null;
            _vviPowerInputSignalChangedEventHandler = null;
        }
        private void RegisterForInputEvents()
        {
            if (_vviInputSignal != null)
            {
                _vviInputSignal.SignalChanged += _vviInputSignalChangedEventHandler;
            }
            if (_vviPowerInputSignal != null)
            {
                _vviPowerInputSignal.SignalChanged += _vviPowerInputSignalChangedEventHandler;
            }
        }
        private void UnregisterForInputEvents()
        {
            if (_vviInputSignalChangedEventHandler != null && _vviInputSignal != null)
            {
                try
                {
                    _vviInputSignal.SignalChanged -= _vviInputSignalChangedEventHandler;
                }
                catch (RemotingException ex)
                {
                }
            }
            if (_vviPowerInputSignalChangedEventHandler != null && _vviPowerInputSignal != null)
            {
                try
                {
                    _vviPowerInputSignal.SignalChanged -= _vviPowerInputSignalChangedEventHandler;
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
            _vviInputSignal = CreateVVIInputSignal();
            _vviPowerInputSignal = CreateVVIPowerInputSignal();
        }
        private void CreateOutputSignals()
        {
            _vviOutputSignal = CreateVVIOutputSignal();
        }
        private AnalogSignal CreateVVIOutputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "VVI Signal To Instrument";
            thisSignal.Id = "10058102_VVI_To_Instrument";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }

        private AnalogSignal CreateVVIInputSignal()
        {
            AnalogSignal thisSignal = new AnalogSignal();
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "VVI Value from Simulation";
            thisSignal.Id = "10058102_VVI_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            return thisSignal;
        }

        private DigitalSignal CreateVVIPowerInputSignal()
        {
            DigitalSignal thisSignal = new DigitalSignal();
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "VVI Power Off Flag Value from Simulation";
            thisSignal.Id = "10058102_VVI_Power_Off_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = this.FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;
            return thisSignal;
        }
        private void vvi_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }
        private void vviPower_InputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }
        private void UpdateOutputValues()
        {
            bool vviPowerOff = false;
            if (_vviPowerInputSignal != null)
            {
                vviPowerOff = _vviPowerInputSignal.State;
            }

            if (_vviInputSignal != null)
            {
                double vviInput = _vviInputSignal.State;
                double vviOutputValue = 0;

                if (_vviOutputSignal != null)
                {
                    if (vviPowerOff)
                    {
                        vviOutputValue = -10;
                    }
                    else
                    {
                        if (vviInput <-6000)
                        {
                            vviOutputValue = -10;
                        }
                        else if (vviInput >= -6000 && vviInput < -3000)
                        {
                            vviOutputValue = -6.37 + (((vviInput - -6000) / 3000) * 1.66);
                        }
                        else if (vviInput >= -3000 && vviInput < -1000)
                        {
                            vviOutputValue = -4.71 + (((vviInput - -3000) / 2000) * 2.90);
                        }
                        else if (vviInput >= -1000 && vviInput < -400)
                        {
                            vviOutputValue = -1.81 + (((vviInput - -1000) / 600) * 1.81);
                        }
                        else if (vviInput >= -400 && vviInput < 0)
                        {
                            vviOutputValue = 0 + (((vviInput - -400) / 400) * 1.83);
                        }
                        else if (vviInput >= 0 && vviInput < 1000)
                        {
                            vviOutputValue = 1.83 + ((vviInput  / 1000) * 3.65);
                        }
                        else if (vviInput >= 1000 && vviInput < 3000)
                        {
                            vviOutputValue = 5.48 + (((vviInput - 1000) / 2000) * 2.9);
                        }
                        else if (vviInput >= 3000 && vviInput < 6000)
                        {
                            vviOutputValue = 8.38 + (((vviInput - 3000) / 3000) * 1.62);
                        }
                        else if (vviInput >= 6000)
                        {
                            vviOutputValue = 10;
                        }
                    }

                    if (vviOutputValue < -10)
                    {
                        vviOutputValue = -10;
                    }
                    else if (vviOutputValue > 10)
                    {
                        vviOutputValue = 10;
                    }

                    _vviOutputSignal.State = ((vviOutputValue + 10.0000) / 20.0000);

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
        ~Simtek10058102HardwareSupportModule()
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
