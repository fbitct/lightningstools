using System;
using System.Collections.Generic;
using Common.HardwareSupport;
using Common.InputSupport.DirectInput;
using Common.MacroProgramming;
using log4net;

namespace SimLinkup.HardwareSupport.DirectInput
{
    public class DirectInputHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (DirectInputHardwareSupportModule));

        #endregion

        #region Signals Handling

        #region Signal Creation

        private void CreateInputSignals(DIPhysicalDeviceInfo device, out AnalogSignal[] analogSignals,
            out DigitalSignal[] digitalSignals)
        {
            if (device == null) throw new ArgumentNullException("device");
            var analogSignalsToReturn = new List<AnalogSignal>();
            var digitalSignalsToReturn = new List<DigitalSignal>();

            foreach (var button in device.Buttons)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.Category = "Inputs";
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

            foreach (var axis in device.Axes)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.Category = "Inputs";
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
                thisSignal.MinValue = 0;
                thisSignal.MaxValue = 1024;

                analogSignalsToReturn.Add(thisSignal);
            }

            foreach (var axis in device.Povs)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.Category = "Inputs";
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

        #region Instance variables

        private readonly AnalogSignal[] _analogInputSignals;
        private readonly DigitalSignal[] _digitalInputSignals;
        private DIPhysicalDeviceInfo _device;
        private bool _isDisposed;

        #endregion

        #region Constructors

        private DirectInputHardwareSupportModule()
        {
        }

        private DirectInputHardwareSupportModule(DIPhysicalDeviceInfo device)
            : this()
        {
            if (device == null) throw new ArgumentNullException("device");
            _device = device;
            CreateInputSignals(_device, out _analogInputSignals, out _digitalInputSignals);
        }

        public override string FriendlyName
        {
            get { return string.Format("DirectInput Device: {0}", _device.Alias); }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            try
            {
                using (var mediator = new Mediator(null))
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
            get { return _analogInputSignals; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return _digitalInputSignals; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return null; }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get { return null; }
        }

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
        ~DirectInputHardwareSupportModule()
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
                    Common.Util.DisposeObject(_device); //disconnect 
                    _device = null;
                }
            }
            _isDisposed = true;
        }

        #endregion
    }
}