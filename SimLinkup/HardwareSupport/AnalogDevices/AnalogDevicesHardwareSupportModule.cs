using System;
using System.Collections.Generic;
using AnalogDevices;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;
using a = AnalogDevices;

namespace SimLinkup.HardwareSupport.AnalogDevices
{
    public class AnalogDevicesHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (AnalogDevicesHardwareSupportModule));

        #endregion

        #region Instance variables

        private readonly AnalogSignal[] _analogOutputSignals;
        private DenseDacEvalBoard _device;
        private bool _isDisposed;

        #endregion

        #region Constructors

        private AnalogDevicesHardwareSupportModule()
        {
        }

        private AnalogDevicesHardwareSupportModule(DenseDacEvalBoard device, int deviceIndex) : this()
        {
            //if (device == null) throw new ArgumentNullException("device");
            _device = device;
            CreateOutputSignals(_device, deviceIndex, out _analogOutputSignals);
        }

        public override string FriendlyName
        {
            get { return "Analog Devices AD536x/AD537x"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            try
            {
                var index = 0;
                var boards = DenseDacEvalBoard.Enumerate();
                if (boards != null && boards.Length > 0)
                {
                    foreach (var device in boards)
                    {
                        if (device == null) continue;
                        device.Reset();
                        device.DacPrecision = DacPrecision.SixteenBit;
                        device.Group0Offset = 0x2000;
                        device.Group1Offset = 0x2000;
                        //device.Groups2Thru4Offset = 0x2000;
                        for (var j = 0; j < 40; j++)
                        {
                            device.SetDacChannelOffset((ChannelAddress) j + 8, 0x8000);
                            device.SetDacChannelGain((ChannelAddress) j + 8, 0xFFFF);
                            device.SetDacChannelDataSource((ChannelAddress) j + 8, DacChannelDataSource.DataValueA);
                            device.SetDacChannelDataValueA((ChannelAddress) j + 8, 0x7FFF);
                        }
                        device.UpdateAllDacOutputs();
                        //device.ResumeAllDacOutputs();
                        //device.UpdateAllDacOutputs();

                        IHardwareSupportModule thisHsm = new AnalogDevicesHardwareSupportModule(device, index);
                        toReturn.Add(thisHsm);
                        index++;
                    }
                }
                else
                {
                    IHardwareSupportModule fakeHsm = new AnalogDevicesHardwareSupportModule(null, index);
                    toReturn.Add(fakeHsm);
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

        private void CreateOutputSignals(DenseDacEvalBoard device, int deviceIndex, out AnalogSignal[] analogSignals)
        {
            //if (device == null) throw new ArgumentNullException("device");
            var analogSignalsToReturn = new List<AnalogSignal>();
            for (var i = 0; i < 40; i++)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "DAC Outputs";
                thisSignal.FriendlyName = string.Format("DAC Output #{0}", i);
                thisSignal.Id = string.Format("AnalogDevices_AD536x/537x__DAC_OUTPUT[{0}][{1}]", deviceIndex, i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceFriendlyName = "Analog Devices AD536x/AD537x";
                thisSignal.SourceAddress = device != null ? device.SymbolicName : null;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0.500;
                thisSignal.SignalChanged += DAC_OutputSignalChanged;
                analogSignalsToReturn.Add(thisSignal);
            }
            analogSignals = analogSignalsToReturn.ToArray();
        }

        private void DAC_OutputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputSignal((AnalogSignal) sender);
        }

        private void UpdateOutputSignal(AnalogSignal outputSignal)
        {
            if (outputSignal.Index.HasValue)
            {
                if (_device != null)
                {
                    _device.SetDacChannelDataValueA((ChannelAddress) outputSignal.Index.Value + 8,
                                                    (ushort) (outputSignal.State*0xFFFF));
                }
            }
        }

        public override void Synchronize()
        {
            foreach (var signal in _analogOutputSignals)
            {
                UpdateOutputSignal(signal);
            }
            if (_device != null)
            {
                _device.UpdateAllDacOutputs();
            }
        }

        #endregion

        #endregion

        #region Destructors

        /// <summary>
        ///   Public implementation of IDisposable.Dispose().  Cleans up 
        ///   managed and unmanaged resources used by this 
        ///   object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Standard finalizer, which will call Dispose() if this object 
        ///   is not manually disposed.  Ordinarily called only 
        ///   by the garbage collector.
        /// </summary>
        ~AnalogDevicesHardwareSupportModule()
        {
            Dispose();
        }

        /// <summary>
        ///   Private implementation of Dispose()
        /// </summary>
        /// <param name = "disposing">flag to indicate if we should actually
        ///   perform disposal.  Distinguishes the private method signature 
        ///   from the public signature.</param>
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