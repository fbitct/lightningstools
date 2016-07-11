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
        private a.DacChannelDataSource _dacChannelDataSourceForPendingData = a.DacChannelDataSource.DataValueA;
        #endregion

        #region Constructors

        private AnalogDevicesHardwareSupportModule(){}

        public static async Task<AnalogDevicesHardwareSupportModule> CreateAsync(a.IDenseDacEvalBoard device, int deviceIndex, DeviceConfig deviceConfig) 
        {
            var module = new AnalogDevicesHardwareSupportModule();
            await module.InitializeAsync(device, deviceIndex, deviceConfig).ConfigureAwait(false);
            return module;
        }
        private async Task InitializeAsync(a.IDenseDacEvalBoard device, int deviceIndex, DeviceConfig deviceConfig)
        {
            _device = device ?? new SimulatedAnalogDevicesDenseDacEvalBoard();
            _deviceIndex = deviceIndex;
            await ConfigureDeviceAsync(_device, deviceConfig).ConfigureAwait(false);
            _analogOutputSignals = CreateOutputSignals(_device, _deviceIndex);
            await InitializeOutputsAsync().ConfigureAwait(false);
        }
        private async Task ConfigureDeviceAsync(a.IDenseDacEvalBoard device, DeviceConfig deviceConfig)
        {
            device.DacPrecision = deviceConfig != null &&
                                    deviceConfig.DACPrecision.HasValue
                                        ? deviceConfig.DACPrecision.Value
                                        : a.DacPrecision.SixteenBit;

            await device.ResetAsync().ConfigureAwait(false);
            if (await device.GetIsOverTemperatureAsync().ConfigureAwait(false))
            {
                await device.DisableThermalShutdownAsync().ConfigureAwait(false);
                //reset temperature shutdown after previous over-temperature event
            }
            await device.EnableThermalShutdownAsync().ConfigureAwait(false); //enable over-temperature auto shutdown

            await device.SetDacChannelDataSourceAllChannelsAsync(a.DacChannelDataSource.DataValueA).ConfigureAwait(false);

            await device.SetOffsetDAC0Async( deviceConfig != null &&
                                    deviceConfig.Calibration != null &&
                                    deviceConfig.Calibration.OffsetDAC0.HasValue
                                        ? deviceConfig.Calibration.OffsetDAC0.Value
                                        : (ushort)0x2000).ConfigureAwait(false);

            await device.SetOffsetDAC1Async(deviceConfig != null &&
                                    deviceConfig.Calibration != null &&
                                    deviceConfig.Calibration.OffsetDAC1.HasValue
                                        ? deviceConfig.Calibration.OffsetDAC1.Value
                                        : (ushort)0x2000).ConfigureAwait(false);

            await device.SetOffsetDAC2Async(deviceConfig != null &&
                                    deviceConfig.Calibration != null &&
                                    deviceConfig.Calibration.OffsetDAC2.HasValue
                                        ? deviceConfig.Calibration.OffsetDAC2.Value
                                        : (ushort)0x2000).ConfigureAwait(false);

            
            var tasks = new List<Task>();
            for (var channel = a.ChannelAddress.Dac0; channel <= a.ChannelAddress.Dac39; channel++)
            {
                var task = ConfigureDeviceChannelAsync(device, deviceConfig, channel);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            await device.UpdateAllDacOutputsAsync().ConfigureAwait(false);
        }

        private async Task ConfigureDeviceChannelAsync(a.IDenseDacEvalBoard device, DeviceConfig deviceConfig, a.ChannelAddress channel)
        {
            var dacChannelConfiguration = GetDACChannelConfigurationAsync(channel, deviceConfig);

            await device.SetDacChannelDataValueAAsync(channel,
                            dacChannelConfiguration != null &&
                            dacChannelConfiguration.InitialState != null &&
                            dacChannelConfiguration.InitialState.DataValueA.HasValue
                                ? dacChannelConfiguration.InitialState.DataValueA.Value
                                : (ushort)0x0000).ConfigureAwait(false);

            await device.SetDacChannelDataValueBAsync(channel,
                            dacChannelConfiguration != null &&
                            dacChannelConfiguration.InitialState != null &&
                            dacChannelConfiguration.InitialState.DataValueB.HasValue
                                ? dacChannelConfiguration.InitialState.DataValueB.Value
                                : (ushort)0x0000).ConfigureAwait(false);

            await device.SetDacChannelOffsetAsync(channel,
                            dacChannelConfiguration != null &&
                            dacChannelConfiguration.Calibration != null &&
                            dacChannelConfiguration.Calibration.Offset.HasValue
                                ? dacChannelConfiguration.Calibration.Offset.Value
                                : (ushort)0x8000).ConfigureAwait(false);

            await device.SetDacChannelGainAsync(channel,
                            dacChannelConfiguration != null &&
                            dacChannelConfiguration.Calibration != null &&
                            dacChannelConfiguration.Calibration.Gain.HasValue
                                ? dacChannelConfiguration.Calibration.Gain.Value
                                : (ushort)0xFFFF).ConfigureAwait(false);
        }

        private DACChannelConfiguration GetDACChannelConfigurationAsync(a.ChannelAddress channel, DeviceConfig deviceConfig)
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
                    var hsmInstance = CreateAsync(device, index, thisDeviceConfig).Result;
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

        private async void DAC_OutputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            await SetDACOutputAsync((AnalogSignal) sender, _dacChannelDataSourceForPendingData);
        }

        private async Task SetDACOutputAsync(AnalogSignal outputSignal, a.DacChannelDataSource dacChannelDataSource)
        {
            if (!outputSignal.Index.HasValue || _device == null) return;

            var value = (ushort)(((outputSignal.State + 10.0000) / 20.0000) * 0xFFFF);
            var channelAddress = (a.ChannelAddress)outputSignal.SubSource;
            await (dacChannelDataSource == a.DacChannelDataSource.DataValueA
                    ? _device.SetDacChannelDataValueAAsync(channelAddress, value)
                    : _device.SetDacChannelDataValueBAsync(channelAddress, value)
                ).ConfigureAwait(false);
        }

        private async Task InitializeOutputsAsync()
        {
            await Task.WhenAll(
                _analogOutputSignals.Select(signal => 
                    SetDACOutputAsync(signal, _dacChannelDataSourceForPendingData)
                )
            ).ConfigureAwait(false);
            await SynchronizeAsync().ConfigureAwait(false);
        }

        public override async Task SynchronizeAsync()
        {
            if (_device != null)
            {
                //await ToggleDACChannelDataSourceAsync().ConfigureAwait(false);
                await _device.UpdateAllDacOutputsAsync().ConfigureAwait(false);
            }
        }

        private async Task ToggleDACChannelDataSourceAsync()
        {
            //tell device to use pending data source as data source for all DAC channels
            await _device.SetDacChannelDataSourceAllChannelsAsync(_dacChannelDataSourceForPendingData).ConfigureAwait(false); 

            //now set pending data source to alternate data source so signal updates will accumulate there
            _dacChannelDataSourceForPendingData = _dacChannelDataSourceForPendingData == a.DacChannelDataSource.DataValueA
                                                                                            ? a.DacChannelDataSource.DataValueB
                                                                                            : a.DacChannelDataSource.DataValueA;
            //populate the pending data source with all current values
            await Task.WhenAll(
                _analogOutputSignals.Select(outputSignal => 
                    SetDACOutputAsync(outputSignal, _dacChannelDataSourceForPendingData))
            ).ConfigureAwait(false);
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