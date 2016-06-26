using System;
using System.Collections.Generic;
using Common.HardwareSupport;
using Common.InputSupport;
using Common.InputSupport.DirectInput;
using Common.MacroProgramming;
using log4net;
using System.Linq;
using System.Windows.Forms;

namespace SimLinkup.HardwareSupport.DirectInput
{
    public class DirectInputHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (DirectInputHardwareSupportModule));

        #endregion

        #region Signals Handling

        #region Signal Creation

        private void CreateSignals(out DigitalSignal[] buttons, out AnalogSignal[] axes, out AnalogSignal[] povs)
        {
            var device = _deviceMonitor.DeviceInfo;
            var buttonsToReturn = new List<DigitalSignal>();
            var axesToReturn = new List<AnalogSignal>();
            var povsToReturn = new List<AnalogSignal>();

            foreach (var button in device.Buttons)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.Category = "Controls";
                thisSignal.CollectionName = "Buttons";
                thisSignal.FriendlyName = button.Alias;
                thisSignal.Id = button.ToString();
                thisSignal.Index = button.ControlNum;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceFriendlyName = this.FriendlyName;
                thisSignal.SourceAddress = device.Guid.ToString();
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                var currentState = _mediator.GetPhysicalControlValue(button, StateType.Current);
                thisSignal.State = currentState.HasValue ? currentState.Value==1 : false;
                buttonsToReturn.Add(thisSignal);
            }

            foreach (var axis in device.Axes)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.Category = "Controls";
                thisSignal.CollectionName = "Axes";
                thisSignal.FriendlyName = axis.Alias;
                thisSignal.Id = axis.ToString();
                thisSignal.Index = axis.ControlNum;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceFriendlyName = this.FriendlyName;
                thisSignal.SourceAddress = device.Guid.ToString();
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                var currentState = _mediator.GetPhysicalControlValue(axis, StateType.Current);
                thisSignal.State = currentState.HasValue ? currentState.Value : _deviceMonitor.AxisRangeMin;
                thisSignal.MinValue = _deviceMonitor.AxisRangeMin;
                thisSignal.MaxValue = _deviceMonitor.AxisRangeMax;

                axesToReturn.Add(thisSignal);
            }

            foreach (var pov in device.Povs)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.Category = "Controls";
                thisSignal.CollectionName = "POVs";
                thisSignal.FriendlyName = pov.Alias;
                thisSignal.Id = pov.ToString();
                thisSignal.Index = pov.ControlNum;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceFriendlyName = this.FriendlyName;
                thisSignal.SourceAddress = device.Guid.ToString();
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                var currentState = _mediator.GetPhysicalControlValue(pov, StateType.Current);
                thisSignal.State = currentState.HasValue ? currentState.Value : -1;
                thisSignal.MinValue = -1;
                thisSignal.MaxValue = _deviceMonitor.AxisRangeMax;
                povsToReturn.Add(thisSignal);
            }

            buttons = buttonsToReturn.ToArray();
            axes = axesToReturn.ToArray();
            povs = povsToReturn.ToArray();
        }


        #endregion

        #endregion

        #region Instance variables

        private readonly DigitalSignal[] _buttons;
        private readonly AnalogSignal[] _axes;
        private readonly AnalogSignal[] _povs;
        private readonly Mediator _mediator;
        private readonly DIDeviceMonitor _deviceMonitor;
        private bool _isDisposed=false;

        #endregion

        #region Constructors

        private DirectInputHardwareSupportModule(){}

        private DirectInputHardwareSupportModule(Mediator mediator, DIDeviceMonitor deviceMonitor)
            : this()
        {
            if (mediator == null) throw new ArgumentNullException("mediator");
            _mediator = mediator;
            if (deviceMonitor == null) throw new ArgumentNullException("deviceMonitor");
            _deviceMonitor = deviceMonitor;

            CreateSignals(out _buttons, out _axes, out _povs);
            _mediator.PhysicalControlStateChanged += _mediator_PhysicalControlStateChanged;
        }


        private void _mediator_PhysicalControlStateChanged(object sender, PhysicalControlStateChangedEventArgs e)
        {
            if (e.Control.Parent.Key != _deviceMonitor.DeviceInfo.Key) return;
            switch (e.Control.ControlType)
            {
                case ControlType.Axis:
                    var thisAxis = _axes.Where(x => x.Id == e.Control.ToString()).SingleOrDefault();
                    thisAxis.State = e.CurrentState;
                    break;
                case ControlType.Button:
                    var thisButton = _buttons.Where(x => x.Id == e.Control.ToString()).SingleOrDefault();
                    thisButton.State = e.CurrentState == 1 ? true: false;
                    break;
                case ControlType.Pov:
                    var thisPov = _povs.Where(x => x.Id == e.Control.ToString()).SingleOrDefault();
                    thisPov.State = e.CurrentState;
                    break;
                default:
                    break;
            }
        }

        public override string FriendlyName
        {
            get
            {
                var alias = _deviceMonitor.DeviceInfo.Alias;
                var guid = _deviceMonitor.DeviceInfo.Guid;
                var deviceNum = _deviceMonitor.DeviceInfo.DeviceNum;
                return string.Format("DirectInput Device {0} - {1} [GUID:{2}]", deviceNum, alias, guid);
            }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            try
            {
                var mediator = new Mediator(Application.OpenForms[0]);
                mediator.RaiseEvents = true;
                foreach (var deviceMonitor in mediator.DeviceMonitors)
                {
                    IHardwareSupportModule thisHsm = new DirectInputHardwareSupportModule(mediator, deviceMonitor.Value);
                    toReturn.Add(thisHsm);
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
            get { return null; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return null; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return _axes.Union(_povs).ToArray(); }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get { return _buttons.ToArray(); }
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
                }
            }
            _isDisposed = true;
        }

        #endregion
    }
}