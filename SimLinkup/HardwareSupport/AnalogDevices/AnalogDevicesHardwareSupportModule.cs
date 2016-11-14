using System;
using System.Collections.Generic;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;
using a = AnalogDevices;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
namespace SimLinkup.HardwareSupport.AnalogDevices
{
    public class AnalogDevicesHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (AnalogDevicesHardwareSupportModule));
        #endregion

        #region Instance variables

        private AnalogSignal[] _analogOutputSignals;
        private a.IDenseDacEvalBoard _device;
        private bool _isDisposed;
        private int _deviceIndex;
        #endregion

        #region Constructors

        private AnalogDevicesHardwareSupportModule(){}

        public static AnalogDevicesHardwareSupportModule Create(a.IDenseDacEvalBoard device, int deviceIndex, DeviceConfig deviceConfig) 
        {
            var module = new AnalogDevicesHardwareSupportModule();
            module.Initialize(device, deviceIndex, deviceConfig);
            return module;
        }
        private void Initialize(a.IDenseDacEvalBoard device, int deviceIndex, DeviceConfig deviceConfig)
        {
            _device = device;
            _deviceIndex = deviceIndex;
            ConfigureDevice(_device, deviceConfig);
            _analogOutputSignals = CreateOutputSignals(_device, _deviceIndex);
            InitializeOutputs();
        }
        private void ConfigureDevice(a.IDenseDacEvalBoard device, DeviceConfig deviceConfig)
        {
            if (device == null) return;
            device.DacPrecision = deviceConfig != null &&
                                    deviceConfig.DACPrecision.HasValue
                                        ? deviceConfig.DACPrecision.Value
                                        : a.DacPrecision.SixteenBit;

            device.Reset();
            if (device.IsOverTemperature)
            {
                device.IsThermalShutdownEnabled=false;
                //reset temperature shutdown after previous over-temperature event
            }
            device.IsThermalShutdownEnabled=true; //enable over-temperature auto shutdown

            device.SetDacChannelDataSourceAllChannels(a.DacChannelDataSource.DataValueA);

            device.OffsetDAC0 = ( deviceConfig != null &&
                                    deviceConfig.Calibration != null &&
                                    deviceConfig.Calibration.OffsetDAC0.HasValue
                                        ? deviceConfig.Calibration.OffsetDAC0.Value
                                        : (ushort)0x2000);

            device.OffsetDAC1=(deviceConfig != null &&
                                    deviceConfig.Calibration != null &&
                                    deviceConfig.Calibration.OffsetDAC1.HasValue
                                        ? deviceConfig.Calibration.OffsetDAC1.Value
                                        : (ushort)0x2000);

            device.OffsetDAC2=(deviceConfig != null &&
                                    deviceConfig.Calibration != null &&
                                    deviceConfig.Calibration.OffsetDAC2.HasValue
                                        ? deviceConfig.Calibration.OffsetDAC2.Value
                                        : (ushort)0x2000);

            
            for (var channel = a.ChannelAddress.Dac0; channel <= a.ChannelAddress.Dac39; channel++)
            {
                ConfigureDeviceChannel(device, deviceConfig, channel);
            }
            device.UpdateAllDacOutputs();
        }

        private void ConfigureDeviceChannel(a.IDenseDacEvalBoard device, DeviceConfig deviceConfig, a.ChannelAddress channel)
        {
            var dacChannelConfiguration = GetDACChannelConfiguration(channel, deviceConfig);

            device.SetDacChannelDataValueA(channel,
                            dacChannelConfiguration != null &&
                            dacChannelConfiguration.InitialState != null &&
                            dacChannelConfiguration.InitialState.DataValueA.HasValue
                                ? dacChannelConfiguration.InitialState.DataValueA.Value
                                : (ushort)0x8000);

            device.SetDacChannelDataValueB(channel,
                            dacChannelConfiguration != null &&
                            dacChannelConfiguration.InitialState != null &&
                            dacChannelConfiguration.InitialState.DataValueB.HasValue
                                ? dacChannelConfiguration.InitialState.DataValueB.Value
                                : (ushort)0x8000);

            device.SetDacChannelOffset(channel,
                            dacChannelConfiguration != null &&
                            dacChannelConfiguration.Calibration != null &&
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
                var hsmConfigFilePath = Path.Combine(Util.CurrentMappingProfileDirectory, "AnalogDevicesHardwareSupportModule.config");
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

                    var device = devices != null && devices.Length > index ? devices[index] : null;
                    if (device != null)
                    {
                        var hsmInstance = Create(device, index, thisDeviceConfig);
                        toReturn.Add(hsmInstance);
                    }
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

        private AnalogSignal[] CreateOutputSignals(a.IDenseDacEvalBoard device, int deviceIndex)
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
            return analogSignalsToReturn.ToArray();
        }

        private void DAC_OutputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            Task.Run(()=>SetDACChannelOutputValue((AnalogSignal)sender, a.DacChannelDataSource.DataValueA)).ConfigureAwait(false);
        }

        private void SetDACChannelOutputValue(AnalogSignal outputSignal, a.DacChannelDataSource dacChannelDataSource)
        {
            if (!outputSignal.Index.HasValue || _device == null) return;

            var value = (ushort)(((outputSignal.State + 10.0000) / 20.0000) * 0xFFFF);
            var channelAddress = (a.ChannelAddress)outputSignal.SubSource;
            if (dacChannelDataSource == a.DacChannelDataSource.DataValueA)
            {
                _device.SetDacChannelDataValueA(channelAddress, value);
            }
            else
            {
                _device.SetDacChannelDataValueB(channelAddress, value);
            }
        }

        private void InitializeOutputs()
        {
            _analogOutputSignals.ToList().ForEach(signal =>
                SetDACChannelOutputValue(signal, a.DacChannelDataSource.DataValueA));
            _device.SetLDACPinLow();
            Synchronize();
        }

        public override void Synchronize()
        {
            if (_device != null)
            {
                //_device.UpdateAllDacOutputs();
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