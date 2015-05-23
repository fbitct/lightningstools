using System;
using System.Collections.Generic;
using Common.HardwareSupport;
using Common.InputSupport.DirectInput;
using Common.MacroProgramming;
using log4net;
using System.Runtime.Remoting;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Linq;
using System.Text;
namespace SimLinkup.HardwareSupport.Powell
{
    public class PowellIP1310ALRHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof(PowellIP1310ALRHardwareSupportModule));
        private const int MAX_RWR_SYMBOLS_AS_INPUTS = 64;
        private const int MAX_RWR_SYMBOLS_AS_OUTPUTS = 31;
        private const string DEFAULT_DEVICE_ID = "RWR00";
        private const int BAUD_RATE = 2400;
        private const int DATA_BITS = 8;
        private const Parity PARITY = Parity.None;
        private const StopBits STOP_BITS = StopBits.One;
        private const Handshake HANDSHAKE = Handshake.RequestToSend;
        private const bool RTS_ENABLE = true;
        private const int RECEIVED_BYTES_THRESHOLD = 1;
        private const int SERIAL_READ_TIMEOUT = 500;
        private const int SERIAL_WRITE_TIMEOUT = 500;
        private const int MAX_UNSUCCESSFUL_CONNECTION_ATTEMPTS = 5;
        #endregion

        #region Instance variables

        private readonly AnalogSignal[] _analogInputSignals;
        private readonly DigitalSignal[] _digitalInputSignals;
        private string _deviceID;
        private Common.IO.Ports.ISerialPort _serialPort;
        private object _serialPortLock = new object();
        private int _unsuccessfulConnectionAttempts = 0;
        private string _comPort;
        private bool _resetNeeded = true;
        private bool _isDisposed;
        private AnalogSignal _rwrSymbolCountInputSignal;
        private AnalogSignal[] _rwrObjectSymbolIDInputSignals = new AnalogSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private AnalogSignal[] _rwrObjectBearingInputSignals = new AnalogSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private AnalogSignal[] _rwrObjectLethalityInputSignals = new AnalogSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private DigitalSignal[] _rwrObjectMissileActivityFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private DigitalSignal[] _rwrObjectMissileLaunchFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private DigitalSignal[] _rwrObjectSelectedFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private DigitalSignal[] _rwrObjectNewDetectionFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private IFalconRWRSymbolTranslator _falconRWRSymbolTranslator = new FalconRWRSymbolTranslator();
        #endregion

        #region Constructors

        private PowellIP1310ALRHardwareSupportModule(string comPort,string deviceID=DEFAULT_DEVICE_ID)
        {
            _deviceID = deviceID;
            _comPort = comPort;
            CreateInputSignals(deviceID, out _analogInputSignals, out _digitalInputSignals);
            RegisterForInputEvents();
        }

        public override string FriendlyName
        {
            get { return string.Format("Powell IP-1310/ALR Azimuth Indicator (RWR) Driver: {0}", _deviceID); }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory,
                    "PowellIP1310ALRHardwareSupportModule.config");
                var hsmConfig =
                    PowellIP1310ALRHardwareSupportModuleConfig.Load(hsmConfigFilePath);
                
                IHardwareSupportModule thisHsm = new PowellIP1310ALRHardwareSupportModule(
                    comPort: hsmConfig.COMPort, deviceID: hsmConfig.DeviceID ?? DEFAULT_DEVICE_ID);
                
                toReturn.Add(thisHsm);
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

        #region Signals Handling
        #region Signals Event Handling

        private void RegisterForInputEvents()
        {
            foreach (var analogSignal in _analogInputSignals)
            {
                analogSignal.SignalChanged += analogSignal_SignalChanged;
            }
            foreach (var digitalSignal in _digitalInputSignals)
            {
                digitalSignal.SignalChanged += digitalSignal_SignalChanged;
            }
        }

        private void digitalSignal_SignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            UpdateOutputs();
        }

        private void analogSignal_SignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputs();
        }
        private void UpdateOutputs()
        {
            bool connected = EnsureSerialPortConnected();
            if (connected)
            {
                var commandList = GenerateCommandList();
                SendCommandList(commandList);
            }
        }
        private bool EnsureSerialPortConnected()
        {
            lock (_serialPortLock)
            {
                if (_serialPort == null)
                {
                    try
                    {
                        _serialPort = new Common.IO.Ports.SerialPort(_comPort, BAUD_RATE, PARITY, DATA_BITS, STOP_BITS);
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                        return false;
                    }
                }
                if (_serialPort !=null && !_serialPort.IsOpen && _unsuccessfulConnectionAttempts <MAX_UNSUCCESSFUL_CONNECTION_ATTEMPTS)
                {
                    try
                    {
                        _serialPort.Handshake = HANDSHAKE;
                        _serialPort.ReceivedBytesThreshold = RECEIVED_BYTES_THRESHOLD;
                        _serialPort.RtsEnable = RTS_ENABLE;
                        _serialPort.ReadTimeout = SERIAL_READ_TIMEOUT;
                        _serialPort.WriteTimeout = SERIAL_WRITE_TIMEOUT;
                        _serialPort.ErrorReceived += _serialPort_ErrorReceived;
                        _log.DebugFormat("Opening serial port {0}: Handshake:{1}, ReceivedBytesThreshold:{2}, RtsEnable:{3}, ReadTimeout:{4}, WriteTimeout:{5}", _comPort, HANDSHAKE.ToString(), RTS_ENABLE.ToString(), SERIAL_READ_TIMEOUT, SERIAL_WRITE_TIMEOUT);

                        _serialPort.Open();
                        _unsuccessfulConnectionAttempts = 0;
                    }
                    catch (Exception e)
                    {
                        _unsuccessfulConnectionAttempts++;
                        _log.Error(e.Message, e);
                        return false;
                    }
                }
                return true;
            }
           
        }

        void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            _log.ErrorFormat("A serial port error occurred communicating with {0} on COM port {1}.\r\nError Type: {2}\r\nError Description:{3}", _deviceID, _comPort, e.EventType.ToString(), e.ToString());
        }

        private void CloseSerialPortConnection()
        {
            lock (_serialPortLock)
            {
                if (_serialPort != null)
                {
                    try
                    {
                        if (_serialPort.IsOpen)
                        {
                            _log.DebugFormat("Closing serial port {0}", _comPort);
                            _serialPort.Close();
                        }
                        _serialPort.Dispose();
                    }
                    catch {}
                    _serialPort = null;
                }
                Thread.Sleep(500);
            }
        }
        private IEnumerable<RWRCommand> GenerateCommandList()
        {
            var rwrCommands = new List<RWRCommand>();
            if (_resetNeeded)
            {
                rwrCommands.Add(new ResetCommand());
            }
            var numInputSymbols = (int)Math.Truncate( _rwrSymbolCountInputSignal.State);
            var blipList = new List<Blip>();
            for (var i = 0; i < numInputSymbols; i++)
            {
                var falconRWRSymbol = new FalconRWRSymbol
                {
                    SymbolID = (int)Math.Truncate(_rwrObjectSymbolIDInputSignals[i].State),
                    Bearing = _rwrObjectBearingInputSignals[i].State,
                    Lethality = _rwrObjectLethalityInputSignals[i].State,
                    MissileActivity = _rwrObjectMissileActivityFlagInputSignals[i].State,
                    MissileLaunch = _rwrObjectMissileLaunchFlagInputSignals[i].State,
                    Selected = _rwrObjectSelectedFlagInputSignals[i].State,
                    NewDetection = _rwrObjectNewDetectionFlagInputSignals[i].State
                };
                var blips = _falconRWRSymbolTranslator.Translate(falconRWRSymbol, primarySymbol:true);
                blipList.AddRange(blips);
            }
            rwrCommands.Add(new DrawBlipsCommand { Blips = blipList });
            return rwrCommands;
        }
        
        private void SendCommandList(IEnumerable<RWRCommand> commandList)
        {
            lock (_serialPortLock)
            {
                using (var ms = new MemoryStream())
                {
                    bool clearResetFlag = false;
                    int totalBytes = 0;
                    foreach (var command in commandList)
                    {
                        if (command is ResetCommand)
                        {
                            clearResetFlag = true; ;
                        }
                        var deviceIdBytes = Encoding.ASCII.GetBytes(_deviceID);
                        ms.Write(deviceIdBytes, 0, deviceIdBytes.Length);
                        totalBytes += deviceIdBytes.Length;

                        var thisCommandBytes = command.ToBytes();
                        ms.Write(thisCommandBytes, 0, thisCommandBytes.Length);
                        totalBytes += thisCommandBytes.Length;
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    var bytesToWrite = ms.GetBuffer();
                    try
                    {
                        _log.DebugFormat("Sending bytes to serial port {0}:{1}", _comPort, Encoding.UTF8.GetString(bytesToWrite, 0, totalBytes));
                        _serialPort.Write(bytesToWrite, 0, totalBytes);
                        if (_resetNeeded && clearResetFlag)
                        {
                            _resetNeeded = false;
                        }

                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                    }


                }
            }
        }
        
        private void UnregisterForInputEvents()
        {
            foreach (var analogSignal in _analogInputSignals)
            {
                try
                {
                    analogSignal.SignalChanged -= analogSignal_SignalChanged;
                }
                catch (RemotingException) { }
            }
            foreach (var digitalSignal in _digitalInputSignals)
            {
                try
                {
                    digitalSignal.SignalChanged -= digitalSignal_SignalChanged;
                }
                catch (RemotingException) { }
            }
        }

        #endregion

        #region Signal Creation

        private void CreateInputSignals(string deviceID, out AnalogSignal[] analogSignals,
            out DigitalSignal[] digitalSignals)
        {
            var analogSignalsToReturn = new List<AnalogSignal>();
            var digitalSignalsToReturn = new List<DigitalSignal>();

            {
                var thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = "RWR Symbol Count";
                thisSignal.Id = "IP1310ALR__RWR_Symbol_Count";
                thisSignal.Index = 0;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                _rwrSymbolCountInputSignal = thisSignal;
                analogSignalsToReturn.Add(thisSignal);
            }


            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Symbol ID", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Symbol_ID[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                _rwrObjectSymbolIDInputSignals[i] = thisSignal;
                analogSignalsToReturn.Add(thisSignal);
            }
            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Bearing (degrees)", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Bearing_Degrees[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                _rwrObjectBearingInputSignals[i] = thisSignal;
                analogSignalsToReturn.Add(thisSignal);
            }

            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Lethality", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Lethality[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                _rwrObjectLethalityInputSignals[i] = thisSignal;
                analogSignalsToReturn.Add(thisSignal);
            }

            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.CollectionName = "Digital Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Missile Activity Flag", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Missile_Activity_Flag[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = false;
                _rwrObjectMissileActivityFlagInputSignals[i] = thisSignal;
                digitalSignalsToReturn.Add(thisSignal);
            }
            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.CollectionName = "Digital Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Missile Launch Flag", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Missile_Launch_Flag[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = false;
                _rwrObjectMissileLaunchFlagInputSignals[i] = thisSignal;
                digitalSignalsToReturn.Add(thisSignal);
            }
            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.CollectionName = "Digital Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Selected Flag", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Selected_Flag[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = false;
                _rwrObjectSelectedFlagInputSignals[i] = thisSignal;
                digitalSignalsToReturn.Add(thisSignal);
            }
            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.CollectionName = "Digital Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} New Detection Flag", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_New_Detection_Flag[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = false;
                _rwrObjectNewDetectionFlagInputSignals[i] = thisSignal;
                digitalSignalsToReturn.Add(thisSignal);
            }

            analogSignals = analogSignalsToReturn.ToArray();
            digitalSignals = digitalSignalsToReturn.ToArray();
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
        ~PowellIP1310ALRHardwareSupportModule()
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
                }

                try
                {
                    CloseSerialPortConnection();
                }
                catch { }
            }
            _isDisposed = true;
        }

        #endregion
    }
}