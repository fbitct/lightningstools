using System;
using System.Collections.Generic;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;
using a = AnalogDevices;
using System.IO;
using System.Threading.Tasks;

namespace SimLinkup.HardwareSupport.AnalogDevices
{
    public class AnalogDevicesHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (AnalogDevicesHardwareSupportModule));
        private List<Task> _tasks = new List<Task>();
        #endregion

        #region Instance variables

        private readonly AnalogSignal[] _analogOutputSignals;
        private a.IDenseDacEvalBoard _device;
        private bool _isDisposed;
        private int _deviceIndex;
        private a.DacChannelDataSource _dacChannelDataSourceForPendingData = a.DacChannelDataSource.DataValueA;
        #endregion

        #region Constructors

        private AnalogDevicesHardwareSupportModule()
        {
        }

        private AnalogDevicesHardwareSupportModule(a.IDenseDacEvalBoard device, int deviceIndex, DeviceConfig deviceConfig) : this()
        {
            _device = device ?? new a.DenseDacEvalBoard(null);
            _deviceIndex = deviceIndex;
            ConfigureDevice(_device, deviceConfig);
            CreateOutputSignals(_device, deviceIndex, out _analogOutputSignals);
        }
        private void ConfigureDevice(a.IDenseDacEvalBoard device, DeviceConfig deviceConfig)
        {
            device.Reset();
            if (device.IsOverTemperature)
            {
                device.IsThermalShutdownEnabled = false;
                //reset temperature shutdown after previous over-temperature event
            }
            device.IsThermalShutdownEnabled = true; //enable over-temperature auto shutdown

            device.SetDacChannelDataSourceAllChannels(a.DacChannelDataSource.DataValueA);
            device.DacPrecision =   deviceConfig !=null  && 
                                    deviceConfig.DACPrecision.HasValue 
                                        ? deviceConfig.DACPrecision.Value 
                                        : a.DacPrecision.SixteenBit;

            device.OffsetDAC0 = deviceConfig != null &&
                                    deviceConfig.Calibration != null &&
                                    deviceConfig.Calibration.OffsetDAC0.HasValue
                                        ? deviceConfig.Calibration.OffsetDAC0.Value
                                        : (ushort)0x2000;

            device.OffsetDAC1 = deviceConfig != null &&
                                    deviceConfig.Calibration != null &&
                                    deviceConfig.Calibration.OffsetDAC1.HasValue
                                        ? deviceConfig.Calibration.OffsetDAC1.Value
                                        : (ushort)0x2000;

            device.OffsetDAC2 = deviceConfig != null &&
                                    deviceConfig.Calibration != null &&
                                    deviceConfig.Calibration.OffsetDAC2.HasValue
                                        ? deviceConfig.Calibration.OffsetDAC2.Value
                                        : (ushort)0x2000;

            for (var channel = a.ChannelAddress.Dac0; channel <= a.ChannelAddress.Dac39; channel++)
            {
                var dacChannelConfiguration = GetDACChannelConfiguration(channel, deviceConfig);

                device.SetDacChannelDataValueA(channel,
                                dacChannelConfiguration != null &&
                                dacChannelConfiguration.InitialState != null &&
                                dacChannelConfiguration.InitialState.DataValueA.HasValue
                                    ? dacChannelConfiguration.InitialState.DataValueA.Value
                                    : (ushort)0x0000);

                device.SetDacChannelDataValueB(channel,
                                dacChannelConfiguration != null &&
                                dacChannelConfiguration.InitialState != null &&
                                dacChannelConfiguration.InitialState.DataValueB.HasValue
                                    ? dacChannelConfiguration.InitialState.DataValueB.Value
                                    : (ushort)0x0000);

                device.SetDacChannelOffset(channel,   
                                dacChannelConfiguration !=null && 
                                dacChannelConfiguration.Calibration !=null && 
                                dacChannelConfiguration.Calibration.Offset.HasValue 
                                    ? dacChannelConfiguration.Calibration.Offset.Value
                                    : (ushort)0x8000);

                device.SetDacChannelGain(channel, 
                                dacChannelConfiguration != null &&
                                dacChannelConfiguration.Calibration != null &&
                                dacChannelConfiguration.Calibration.Gain.HasValue
                                    ? dacChannelConfiguration.Calibration.Gain.Value
                                    : (ushort)0xFFFF);

            }
            device.UpdateAllDacOutputs();
        }
        private DACChannelConfiguration GetDACChannelConfiguration(a.ChannelAddress channel, DeviceConfig deviceConfig)
        {
            var type = typeof(DACChannelConfigurations);
            DACChannelConfiguration toReturn = null;
            try
            { 
                if (
                        deviceConfig != null && 
                        deviceConfig.DACChannelConfig != null
                    )
                {
                    var propInfo = type.GetProperty(string.Format("DAC{0}", ((int)channel) - 8));
                    toReturn = propInfo != null
                                ? propInfo.GetMethod.Invoke(deviceConfig.DACChannelConfig, null) as DACChannelConfiguration
                                : null;
                }
                
            }
            catch (Exception) { }
            
            return toReturn;

        }

        public override string FriendlyName
        {
            get
            {
                return String.Format("Analog Devices AD536x/AD537x on {0}",
                    _device == null ? string.Format("{{FAKE{0}}}", _deviceIndex) : _device.SymbolicName);
            }
        }

        public static IHardwareSupportModule[] GetInstances()
        {

            var toReturn = new List<IHardwareSupportModule>();
            try
            {
                var hsmConfigFilePath = Path.Combine(Path.Combine(Path.Combine(Util.ApplicationDirectory, "Content"), "Mapping"), "AnalogDevicesHardwareSupportModule.config");
                var hsmConfig = AnalogDevicesHardwareSupportModuleConfig.Load(hsmConfigFilePath);
                if (hsmConfig == null || hsmConfig.Devices ==null && hsmConfig.Devices.Length ==0)
                {
                    return toReturn.ToArray();
                }
                var index = 0;
                var devices = a.DenseDacEvalBoard.Enumerate();

                foreach (var deviceConfig in hsmConfig.Devices)
                {
                    var thisDeviceConfig = hsmConfig.Devices.Length > index
                                                    ? hsmConfig.Devices[index]
                                                    : null;

                    var device = devices != null && devices.Length > index ? devices[index] : new SimulatedAnalogDevicesDenseDacEvalBoard();
                    var hsmInstance = new AnalogDevicesHardwareSupportModule(device, index, thisDeviceConfig);
                    toReturn.Add(hsmInstance);
                    index++;
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
            get { return _analogOutputSignals; }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get { return null; }
        }

        #endregion

        #region Signals Handling

        #region Signal Creation

        private void CreateOutputSignals(a.IDenseDacEvalBoard device, int deviceIndex, out AnalogSignal[] analogSignals)
        {
            var analogSignalsToReturn = new List<AnalogSignal>();
            for (var i = 0; i < 40; i++)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.Category = "Outputs";
                thisSignal.CollectionName = "DAC Outputs";
                thisSignal.FriendlyName = string.Format("DAC #{0}", i);
                thisSignal.Id = string.Format("AnalogDevices_AD536x/537x__DAC_OUTPUT[{0}][{1}]", deviceIndex, i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceFriendlyName = this.FriendlyName;
                thisSignal.SourceAddress = device != null ? device.SymbolicName : null;
                thisSignal.SubSource = (a.ChannelAddress)i+8;
                thisSignal.SubSourceFriendlyName = ((a.ChannelAddress)thisSignal.SubSource).ToString();
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0; //O Volts
                thisSignal.SignalChanged += DAC_OutputSignalChanged;
                thisSignal.Precision = -1; //arbitrary decimal precision (limited to 14-16 bits output precision)
                thisSignal.IsVoltage = true;
                thisSignal.MinValue = -10;
                thisSignal.MaxValue = 10;
                analogSignalsToReturn.Add(thisSignal);
            }
            analogSignals = analogSignalsToReturn.ToArray();
            Initialize();
        }

        private void DAC_OutputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateDACOutput((AnalogSignal) sender, _dacChannelDataSourceForPendingData);
        }

        private void UpdateDACOutput(AnalogSignal outputSignal, a.DacChannelDataSource dacChannelDataSource)
        {
            if (outputSignal.Index.HasValue)
            {
                if (_device != null)
                {
                    var value = (ushort)(((outputSignal.State + 10.0000) / 20.0000) * 0xFFFF);
                    var channelAddress = (a.ChannelAddress)outputSignal.SubSource;
                    if (dacChannelDataSource == a.DacChannelDataSource.DataValueA) 
                    {
                        _tasks.Add(Task.Run(()=> _device.SetDacChannelDataValueA(channelAddress, value))); 
                    }
                    else
                    {
                        _tasks.Add(Task.Run(()=>_device.SetDacChannelDataValueB(channelAddress, value)));
                    }
                }
            }
        }

        private void Initialize()
        {
            foreach (var signal in _analogOutputSignals)
            {
                UpdateDACOutput(signal, _dacChannelDataSourceForPendingData);
            }
            Synchronize();
        }

        public override void Synchronize()
        {
            if (_device != null)
            {
                Task.WaitAll(_tasks.ToArray());
                _tasks.Clear();
                ToggleDACChannelDataSource();
                _device.UpdateAllDacOutputs();
            }
        }

        private void ToggleDACChannelDataSource()
        {
            //tell device to use pending data source as data source for all DAC channels
            _device.SetDacChannelDataSourceAllChannels(_dacChannelDataSourceForPendingData); 

            //now set pending data source to alternate data source so signal updates will accumulate there
            _dacChannelDataSourceForPendingData = _dacChannelDataSourceForPendingData == a.DacChannelDataSource.DataValueA
                                                                                            ? a.DacChannelDataSource.DataValueB
                                                                                            : a.DacChannelDataSource.DataValueA;
            //populate the pending data source with all current values
            foreach (var outputSignal in _analogOutputSignals)
            {
                UpdateDACOutput(outputSignal, _dacChannelDataSourceForPendingData);
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
        ~AnalogDevicesHardwareSupportModule()
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