using System;
using System.Collections.Generic;
using Common.MacroProgramming;
using Common.HardwareSupport;
using System.IO;
using log4net;
using System.Runtime.Remoting;
using AnalogDevices;
using Microsoft.DirectX.DirectInput;
using Common.InputSupport.DirectInput;
namespace SimLinkup.HardwareSupport.DirectInput
{
    public class DirectInputHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables
        private static ILog _log = LogManager.GetLogger(typeof(DirectInputHardwareSupportModule));
        #endregion

        #region Instance variables
        private DIPhysicalDeviceInfo _device = null;
        private bool _isDisposed = false;
        private AnalogSignal[] _analogInputSignals = null;
        private DigitalSignal[] _digitalInputSignals = null;
        #endregion

        #region Constructors
        private DirectInputHardwareSupportModule()
            : base()
        {
        }
        private DirectInputHardwareSupportModule(DIPhysicalDeviceInfo device)
            : this()
        {
            if (device == null) throw new ArgumentNullException("device");
            _device = device;
            CreateInputSignals(_device, out _analogInputSignals, out _digitalInputSignals );
        }

        public override string FriendlyName
        {
            get
            {
                return string.Format("DirectInput Device: {0}", _device.Alias);

            }
        }
        public static IHardwareSupportModule[] GetInstances()
        {
            List<IHardwareSupportModule> toReturn = new List<IHardwareSupportModule>();
            try
            {
                using (Common.InputSupport.DirectInput.Mediator mediator = new Common.InputSupport.DirectInput.Mediator(null))
                {
                    foreach (var device in mediator.DeviceMonitors) 
                    {
                        IHardwareSupportModule thisHsm = new DirectInputHardwareSupportModule(device.Value.DeviceInfo);
                        toReturn.Add(thisHsm);
                    }
                }
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
                return _analogInputSignals;
            }
        }
        public override DigitalSignal[] DigitalInputs
        {
            get
            {
                return _digitalInputSignals;

            }
        }
        public override AnalogSignal[] AnalogOutputs
        {
            get
            {
                return null;
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
        #region Signal Creation

        private void CreateInputSignals(DIPhysicalDeviceInfo device, out AnalogSignal[] analogSignals, out DigitalSignal[] digitalSignals)
        {
            if (device == null) throw new ArgumentNullException("device");
            List<AnalogSignal> analogSignalsToReturn = new List<AnalogSignal>();
            List<DigitalSignal> digitalSignalsToReturn = new List<DigitalSignal>();

            foreach (var button in device.Buttons)
            {
                DigitalSignal thisSignal = new DigitalSignal();
                thisSignal.CollectionName = "Digital Inputs";
                thisSignal.FriendlyName = button.Alias;
                thisSignal.Id = button.ToString();
                thisSignal.Index = button.ControlNum;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceFriendlyName = device.Alias;
                thisSignal.SourceAddress = device.Guid.ToString();
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = false;
                digitalSignalsToReturn.Add(thisSignal);
            }

            foreach(var axis in device.Axes) 
            {
                AnalogSignal thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = axis.Alias;
                thisSignal.Id = axis.ToString();
                thisSignal.Index = axis.ControlNum;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceFriendlyName = device.Alias;
                thisSignal.SourceAddress = device.Guid.ToString();
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                analogSignalsToReturn.Add(thisSignal);
            }

            foreach (var axis in device.Povs)
            {
                AnalogSignal thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = axis.Alias;
                thisSignal.Id = axis.ToString();
                thisSignal.Index = axis.ControlNum;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceFriendlyName = device.Alias;
                thisSignal.SourceAddress = device.Guid.ToString();
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                analogSignalsToReturn.Add(thisSignal);
            }
            analogSignals = analogSignalsToReturn.ToArray();
            digitalSignals = digitalSignalsToReturn.ToArray();
        }

        #endregion
        #endregion

        #region Destructors
        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object 
        /// is not manually disposed.  Ordinarily called only 
        /// by the garbage collector.
        /// </summary>
        ~DirectInputHardwareSupportModule()
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
                    Common.Util.DisposeObject(_device); //disconnect 
                    _device = null;
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
