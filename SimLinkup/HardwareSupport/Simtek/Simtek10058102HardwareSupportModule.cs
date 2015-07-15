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
    //Simtek 10-0581-02 F-16 VVI Indicator
    public class Simtek10058102HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Simtek10058102HardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;
        private AnalogSignal _verticalVelocityInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _verticalVelocityInputSignalChangedEventHandler;
        private AnalogSignal _verticalVelocityOutputSignal;

        private DigitalSignal _offFlagInputSignal;
        private DigitalSignal.SignalChangedEventHandler _offFlagInputSignalChangedEventHandler;

        private IVerticalVelocityIndicatorUSA _renderer = new VerticalVelocityIndicatorUSA();
        #endregion

        #region Constructors

        private Simtek10058102HardwareSupportModule()
        {
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return "Simtek P/N 10-0581-02 - Indicator - Simulated Vertical Velocity"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            toReturn.Add(new Simtek10058102HardwareSupportModule());
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "Simtek10058102HardwareSupportModule.config");
                var hsmConfig =
                    Simtek10058102HardwareSupportModuleConfig.Load(hsmConfigFilePath);
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
            get { return new[] {_verticalVelocityInputSignal}; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return new[] {_offFlagInputSignal}; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return new[] {_verticalVelocityOutputSignal}; }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get { return null; }
        }

        #endregion

        #region Visualization
        public override void Render(Graphics g, Rectangle destinationRectangle)
        {
            g.Clear(Color.Black);

            _renderer.InstrumentState.OffFlag = _offFlagInputSignal.State;
            _renderer.InstrumentState.VerticalVelocityFeet = (float)_verticalVelocityInputSignal.State;

            var vviWidth = (int)(destinationRectangle.Height * (102f / 227f));
            var vviHeight = destinationRectangle.Height;

            using (var vviBmp = new Bitmap(vviWidth, vviHeight))
            using (var vviBmpGraphics = Graphics.FromImage(vviBmp))
            {
                _renderer.Render(vviBmpGraphics, new Rectangle(0,0, vviWidth, vviHeight));
                var targetRectangle = new Rectangle(destinationRectangle.X + (int)((destinationRectangle.Width - vviWidth) / 2.0), destinationRectangle.Y, vviWidth, destinationRectangle.Height);
                g.DrawImage(vviBmp, targetRectangle);
            }
        }
        #endregion


        #region Signals Handling

        #region Signals Event Handling

        private void CreateInputEventHandlers()
        {
            _verticalVelocityInputSignalChangedEventHandler = vvi_InputSignalChanged;
            _offFlagInputSignalChangedEventHandler =
                vviPower_InputSignalChanged;
        }

        private void AbandonInputEventHandlers()
        {
            _verticalVelocityInputSignalChangedEventHandler = null;
            _offFlagInputSignalChangedEventHandler = null;
        }

        private void RegisterForInputEvents()
        {
            if (_verticalVelocityInputSignal != null)
            {
                _verticalVelocityInputSignal.SignalChanged += _verticalVelocityInputSignalChangedEventHandler;
            }
            if (_offFlagInputSignal != null)
            {
                _offFlagInputSignal.SignalChanged += _offFlagInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_verticalVelocityInputSignalChangedEventHandler != null && _verticalVelocityInputSignal != null)
            {
                try
                {
                    _verticalVelocityInputSignal.SignalChanged -= _verticalVelocityInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_offFlagInputSignalChangedEventHandler != null && _offFlagInputSignal != null)
            {
                try
                {
                    _offFlagInputSignal.SignalChanged -= _offFlagInputSignalChangedEventHandler;
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
            _verticalVelocityInputSignal = CreateVerticalVelocityInputSignal();
            _offFlagInputSignal = CreateOffFlagInputSignal();
        }

        private void CreateOutputSignals()
        {
            _verticalVelocityOutputSignal = CreateVerticalVelocityOutputSignal();
        }

        private AnalogSignal CreateVerticalVelocityOutputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Analog Outputs";
            thisSignal.FriendlyName = "Vertical Velocity";
            thisSignal.Id = "10058102_Vertical_Velocity_To_Instrument";
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

        private AnalogSignal CreateVerticalVelocityInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Analog Inputs";
            thisSignal.FriendlyName = "Vertical Velocity (feet per minute)";
            thisSignal.Id = "10058102_Vertical_Velocity_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.MinValue = -6000;
            thisSignal.MaxValue = 6000;
            return thisSignal;
        }

        private DigitalSignal CreateOffFlagInputSignal()
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Digital Inputs";
            thisSignal.FriendlyName = "OFF Flag";
            thisSignal.Id = "10058102_VVI_Power_Off_Flag_From_Sim";
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = true;
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
            var vviPowerOff = false;
            if (_offFlagInputSignal != null)
            {
                vviPowerOff = _offFlagInputSignal.State;
            }

            if (_verticalVelocityInputSignal != null)
            {
                var vviInput = _verticalVelocityInputSignal.State;
                double vviOutputValue = 0;

                if (_verticalVelocityOutputSignal != null)
                {
                    if (vviPowerOff)
                    {
                        vviOutputValue = -10;
                    }
                    else
                    {
                        if (vviInput < -6000)
                        {
                            vviOutputValue = -6.37;
                        }
                        else if (vviInput >= -6000 && vviInput < -3000)
                        {
                            vviOutputValue = -6.37 + (((vviInput - -6000)/3000)*1.66);
                        }
                        else if (vviInput >= -3000 && vviInput < -1000)
                        {
                            vviOutputValue = -4.71 + (((vviInput - -3000)/2000)*2.90);
                        }
                        else if (vviInput >= -1000 && vviInput < -400)
                        {
                            vviOutputValue = -1.81 + (((vviInput - -1000)/600)*1.81);
                        }
                        else if (vviInput >= -400 && vviInput < 0)
                        {
                            vviOutputValue = 0 + (((vviInput - -400)/400)*1.83);
                        }
                        else if (vviInput >= 0 && vviInput < 1000)
                        {
                            vviOutputValue = 1.83 + ((vviInput/1000)*3.65);
                        }
                        else if (vviInput >= 1000 && vviInput < 3000)
                        {
                            vviOutputValue = 5.48 + (((vviInput - 1000)/2000)*2.9);
                        }
                        else if (vviInput >= 3000 && vviInput < 6000)
                        {
                            vviOutputValue = 8.38 + (((vviInput - 3000)/3000)*1.62);
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

                    _verticalVelocityOutputSignal.State = vviOutputValue;
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
        ~Simtek10058102HardwareSupportModule()
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