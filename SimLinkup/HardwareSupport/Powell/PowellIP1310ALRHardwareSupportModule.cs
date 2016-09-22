﻿using System;
using System.Collections.Generic;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLinkup.HardwareSupport.Powell
{
    public class PowellIP1310ALRHardwareSupportModule: HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof(PowellIP1310ALRHardwareSupportModule));
        private const int MAX_RWR_SYMBOLS_AS_INPUTS = 64;
        private const string DEFAULT_DEVICE_ID = "RWR00";
        private const int BAUD_RATE = 2400;
        private const int DATA_BITS = 8;
        private const Parity PARITY = Parity.None;
        private const StopBits STOP_BITS = StopBits.One;
        private const Handshake HANDSHAKE = Handshake.None;
        private const int WRITE_BUFFER_SIZE = 2048;
        private const int SERIAL_WRITE_TIMEOUT = 3000;

        //limits exceptions when we don't have the RWR plugged into the serial port
        private const int MAX_UNSUCCESSFUL_PORT_OPEN_ATTEMPTS = 5;
        private const int MAX_REFRESH_RATE_HZ = 3; //prevents more than this many occurrences per second of Synchronize() event from generating traffic
        #endregion

        #region Instance variables

        private readonly AnalogSignal[] _analogInputSignals;
        private readonly DigitalSignal[] _digitalInputSignals;
        private AnalogSignal _magneticHeadingDegreesInputSignal;
        private AnalogSignal _rwrSymbolCountInputSignal;
        private readonly AnalogSignal[] _rwrObjectSymbolIDInputSignals = new AnalogSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private readonly AnalogSignal[] _rwrObjectBearingInputSignals = new AnalogSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private readonly AnalogSignal[] _rwrObjectLethalityInputSignals = new AnalogSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private readonly DigitalSignal[] _rwrObjectMissileActivityFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private readonly DigitalSignal[] _rwrObjectMissileLaunchFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private readonly DigitalSignal[] _rwrObjectSelectedFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private readonly DigitalSignal[] _rwrObjectNewDetectionFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private readonly IFalconRWRSymbolTranslator _falconRWRSymbolTranslator = new FalconRWRSymbolTranslator();

        private readonly string _deviceID;
        
        private Common.IO.Ports.ISerialPort _serialPort;
        private readonly object _serialPortLock = new object();
        private readonly string _comPort;
        private readonly uint _delayBetweenCharactersMillis = 0;
        private readonly uint _delayBetweenCommandsMillis = 0;
        private int _unsuccessfulConnectionAttempts = 3;

        private DateTime _lastSynchronizedAt = DateTime.MinValue;        
        private bool _isDisposed;

        
        #endregion

        #region Constructors

        private PowellIP1310ALRHardwareSupportModule(string comPort,string deviceID=DEFAULT_DEVICE_ID, uint delayBetweenCharactersMillis=0, uint delayBetweenCommandsMillis=0)
        {
            _deviceID = deviceID;
            _comPort = comPort;
            _delayBetweenCharactersMillis = delayBetweenCharactersMillis;
            _delayBetweenCommandsMillis = delayBetweenCharactersMillis;
            CreateInputSignals(deviceID, out _analogInputSignals, out _digitalInputSignals);
        }

        public override string FriendlyName => $"Powell IP-1310/ALR Azimuth Indicator (RWR) Driver: {_deviceID}";

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.CurrentMappingProfileDirectory,
                    "PowellIP1310ALRHardwareSupportModule.config");
                var hsmConfig =
                    PowellIP1310ALRHardwareSupportModuleConfig.Load(hsmConfigFilePath);
                
                IHardwareSupportModule thisHsm = new PowellIP1310ALRHardwareSupportModule(
                    comPort: hsmConfig.COMPort, deviceID: hsmConfig.DeviceID ?? DEFAULT_DEVICE_ID, 
                    delayBetweenCharactersMillis: hsmConfig.DelayBetweenCharactersMillis, 
                    delayBetweenCommandsMillis: hsmConfig.DelayBetweenCommandsMillis);
                
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

        public override AnalogSignal[] AnalogInputs => _analogInputSignals;

        public override DigitalSignal[] DigitalInputs => _digitalInputSignals;

        public override AnalogSignal[] AnalogOutputs => null;

        public override DigitalSignal[] DigitalOutputs => null;

        public override async Task SynchronizeAsync()
        {
            await base.SynchronizeAsync().ConfigureAwait(false);
            var timeSinceLastSynchronizedInMillis=DateTime.UtcNow.Subtract(_lastSynchronizedAt).TotalMilliseconds;
            if (timeSinceLastSynchronizedInMillis > (1000.00 / MAX_REFRESH_RATE_HZ))
            {
                await UpdateOutputsAsync().ConfigureAwait(false);
                _lastSynchronizedAt = DateTime.UtcNow;
            }
        }

        #endregion

        #region Signals Handling
        #region Signals Event Handling

        
        private async Task UpdateOutputsAsync()
        {
            var connected = EnsureSerialPortConnected();
            if (connected)
            {
                var commandList = GenerateCommandList();
                await SendCommandListAsync(commandList).ConfigureAwait(false);
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
                if (_serialPort !=null && !_serialPort.IsOpen && _unsuccessfulConnectionAttempts <MAX_UNSUCCESSFUL_PORT_OPEN_ATTEMPTS)
                {
                    try
                    {
                        _serialPort.Handshake = HANDSHAKE;
                        _serialPort.WriteTimeout = SERIAL_WRITE_TIMEOUT;
                        _serialPort.ErrorReceived += _serialPort_ErrorReceived;
                        _serialPort.WriteBufferSize = WRITE_BUFFER_SIZE;
                        _log.DebugFormat(
                            $"Opening serial port {_comPort}: Handshake:{HANDSHAKE.ToString()}, WriteTimeout:{SERIAL_WRITE_TIMEOUT}, WriteBufferSize:{WRITE_BUFFER_SIZE}");
                        _serialPort.Open();
                        GC.SuppressFinalize(_serialPort.BaseStream);
                        _serialPort.DiscardOutBuffer();
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

        private void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            _log.ErrorFormat(
                $"A serial port error occurred communicating with {_deviceID} on COM port {_comPort}.\r\nError Type: {e.EventType.ToString()}\r\nError Description:{e}");
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
                            _log.DebugFormat($"Closing serial port {_comPort}");
                            _serialPort.DiscardOutBuffer();
                            _serialPort.Close();
                        }
                        GC.ReRegisterForFinalize(_serialPort.BaseStream);
                        _serialPort.Dispose();
                    }
                    catch {}
                    _serialPort = null;
                }
                _unsuccessfulConnectionAttempts = 0; //reset unsuccessful connection attempts counter
            }
        }

        private IEnumerable<RWRCommand> GenerateCommandList()
        {
            var rwrCommands = new List<RWRCommand>();
            var blipList = GenerateBlipList();
            var enumerable = blipList as Blip[] ?? blipList.ToArray();
            rwrCommands.Add(new DrawBlipsCommand { Blips = enumerable });
            return rwrCommands;
        }

        private IEnumerable<Blip> GenerateBlipList()
        {
            var numInputSymbols = (int)Math.Truncate(_rwrSymbolCountInputSignal.State);
            var blipList = new List<Blip>();
            var usePrimarySymbol = DateTime.UtcNow.Millisecond < 500;
            for (var i = 0; i < numInputSymbols; i++)
            {
                var falconRwrSymbol = new FalconRWRSymbol
                {
                    SymbolId = (int)Math.Truncate(_rwrObjectSymbolIDInputSignals[i].State),
                    BearingDegrees = _rwrObjectBearingInputSignals[i].State,
                    Lethality = _rwrObjectLethalityInputSignals[i].State,
                    MissileActivity = _rwrObjectMissileActivityFlagInputSignals[i].State,
                    MissileLaunch = _rwrObjectMissileLaunchFlagInputSignals[i].State,
                    Selected = _rwrObjectSelectedFlagInputSignals[i].State,
                    NewDetection = _rwrObjectNewDetectionFlagInputSignals[i].State
                };
                var blips = _falconRWRSymbolTranslator.Translate(falconRwrSymbol, _magneticHeadingDegreesInputSignal.State, usePrimarySymbol);
                blipList.AddRange(blips);
            }
            return blipList;
        }

        private byte[] _lastBytesWritten=new byte[0];
        private async Task SendCommandListAsync(IEnumerable<RWRCommand> commandList)
        {
            var rwrCommands = commandList as RWRCommand[] ?? commandList.ToArray();
            if (!rwrCommands.Any()) return;
            lock (_serialPortLock)
            {
                using (var ms = new MemoryStream())
                {
                    var totalBytes = 0;
                    foreach (var command in rwrCommands)
                    {
                        //write out device identifier to memory stream
                        var deviceIdBytes = Encoding.ASCII.GetBytes(_deviceID);
                        ms.Write(deviceIdBytes, 0, deviceIdBytes.Length);
                        totalBytes += deviceIdBytes.Length;

                        //write out the bytes for this command to memory stream
                        var thisCommandBytes = command.ToBytes();
                        if (thisCommandBytes != null && thisCommandBytes.Length > 0)
                        {
                            ms.Write(thisCommandBytes, 0, thisCommandBytes.Length);
                            totalBytes += thisCommandBytes.Length;
                        }

                        //write contents of memory stream to COM port
                        ms.Seek(0, SeekOrigin.Begin);
                        var bytesToWrite = new byte[totalBytes];
                        Array.Copy(ms.GetBuffer(), 0, bytesToWrite, 0, totalBytes);
                        if (bytesToWrite.SequenceEqual(_lastBytesWritten)) continue;
                        try
                        {
                            _log.DebugFormat(
                                $"Sending bytes to serial port {_comPort}:{BytesToString(bytesToWrite, 0, totalBytes)}");
                            for (var i = 0; i < bytesToWrite.Length; i++)
                            {
                                _serialPort.Write(bytesToWrite, i, 1);
                                Thread.Sleep((int)_delayBetweenCharactersMillis);
                                _serialPort.BaseStream.Flush();
                            }
                            _lastBytesWritten = bytesToWrite;
                        }
                        catch (Exception e)
                        {
                            _log.Error(e.Message, e);
                        }
                        Thread.Sleep((int)_delayBetweenCommandsMillis);
                    }
                }
            }
        }
        private static string BytesToString(byte[] bytes, int offset, int count)
        {
            var sb = new StringBuilder();
            sb.Append($"{count} bytes:");
            for (var i = offset; i < offset + count; i++)
            {
                sb.Append("0x");
                sb.Append($"{bytes[i]:X} ");
            }
            return sb.ToString();
        }
        

        #endregion

        #region Signal Creation

        private void CreateInputSignals(string deviceID, out AnalogSignal[] analogSignals,
            out DigitalSignal[] digitalSignals)
        {
            var analogSignalsToReturn = new List<AnalogSignal>();
            var digitalSignalsToReturn = new List<DigitalSignal>();

            {
                var thisSignal = new AnalogSignal
                {
                    Category = "Inputs",
                    CollectionName = "Analog Inputs",
                    FriendlyName = "Magnetic Heading (Degrees)",
                    Id = "IP1310ALR__Magnetic_Heading_Degrees",
                    Index = 0,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = FriendlyName,
                    SourceAddress = _deviceID,
                    SubSource = null,
                    SubSourceFriendlyName = null,
                    SubSourceAddress = null,
                    State = 0,
                    IsAngle = true,
                    MinValue = 0,
                    MaxValue = 360
                };
                _magneticHeadingDegreesInputSignal = thisSignal;
                analogSignalsToReturn.Add(thisSignal);
            }
            {
                var thisSignal = new AnalogSignal
                {
                    Category = "Inputs",
                    CollectionName = "Analog Inputs",
                    FriendlyName = "RWR Symbol Count",
                    Id = "IP1310ALR__RWR_Symbol_Count",
                    Index = 0,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = FriendlyName,
                    SourceAddress = _deviceID,
                    SubSource = null,
                    SubSourceFriendlyName = null,
                    SubSourceAddress = null,
                    State = 0,
                    MinValue = 0,
                    MaxValue = 64
                };
                _rwrSymbolCountInputSignal = thisSignal;
                analogSignalsToReturn.Add(thisSignal);
            }


            for (var i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new AnalogSignal
                {
                    Category = "Inputs",
                    CollectionName = "Analog Inputs",
                    FriendlyName = $"RWR Object #{i} Symbol ID",
                    Id = $"IP1310ALR__RWR_Object_Symbol_ID[{i}]",
                    Index = i,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = FriendlyName,
                    SourceAddress = _deviceID,
                    SubSource = null,
                    SubSourceFriendlyName = null,
                    SubSourceAddress = null,
                    State = 0,
                    MinValue = 0,
                    MaxValue = 64
                };
                _rwrObjectSymbolIDInputSignals[i] = thisSignal;
                analogSignalsToReturn.Add(thisSignal);
            }
            for (var i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new AnalogSignal
                {
                    Category = "Inputs",
                    CollectionName = "Analog Inputs",
                    FriendlyName = $"RWR Object #{i} Bearing (degrees)",
                    Id = $"IP1310ALR__RWR_Object_Bearing_Degrees[{i}]",
                    Index = i,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = FriendlyName,
                    SourceAddress = _deviceID,
                    SubSource = null,
                    SubSourceFriendlyName = null,
                    SubSourceAddress = null,
                    State = 0,
                    IsAngle = true,
                    MinValue = 0,
                    MaxValue = 360
                };
                _rwrObjectBearingInputSignals[i] = thisSignal;
                analogSignalsToReturn.Add(thisSignal);
            }

            for (var i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new AnalogSignal
                {
                    Category = "Inputs",
                    CollectionName = "Analog Inputs",
                    FriendlyName = $"RWR Object #{i} Lethality",
                    Id = $"IP1310ALR__RWR_Object_Lethality[{i}]",
                    Index = i,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = FriendlyName,
                    SourceAddress = _deviceID,
                    SubSource = null,
                    SubSourceFriendlyName = null,
                    SubSourceAddress = null,
                    State = 0,
                    MinValue = -1,
                    MaxValue = 3
                };
                _rwrObjectLethalityInputSignals[i] = thisSignal;
                analogSignalsToReturn.Add(thisSignal);
            }

            for (var i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal
                {
                    Category = "Inputs",
                    CollectionName = "Digital Inputs",
                    FriendlyName = $"RWR Object #{i} Missile Activity Flag",
                    Id = $"IP1310ALR__RWR_Object_Missile_Activity_Flag[{i}]",
                    Index = i,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = FriendlyName,
                    SourceAddress = _deviceID,
                    SubSource = null,
                    SubSourceFriendlyName = null,
                    SubSourceAddress = null,
                    State = false
                };
                _rwrObjectMissileActivityFlagInputSignals[i] = thisSignal;
                digitalSignalsToReturn.Add(thisSignal);
            }
            for (var i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal
                {
                    Category = "Inputs",
                    CollectionName = "Digital Inputs",
                    FriendlyName = $"RWR Object #{i} Missile Launch Flag",
                    Id = $"IP1310ALR__RWR_Object_Missile_Launch_Flag[{i}]",
                    Index = i,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = FriendlyName,
                    SourceAddress = _deviceID,
                    SubSource = null,
                    SubSourceFriendlyName = null,
                    SubSourceAddress = null,
                    State = false
                };
                _rwrObjectMissileLaunchFlagInputSignals[i] = thisSignal;
                digitalSignalsToReturn.Add(thisSignal);
            }
            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal
                {
                    Category = "Inputs",
                    CollectionName = "Digital Inputs",
                    FriendlyName = $"RWR Object #{i} Selected Flag",
                    Id = $"IP1310ALR__RWR_Object_Selected_Flag[{i}]",
                    Index = i,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = FriendlyName,
                    SourceAddress = _deviceID,
                    SubSource = null,
                    SubSourceFriendlyName = null,
                    SubSourceAddress = null,
                    State = false
                };
                _rwrObjectSelectedFlagInputSignals[i] = thisSignal;
                digitalSignalsToReturn.Add(thisSignal);
            }
            for (var i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal
                {
                    Category = "Inputs",
                    CollectionName = "Digital Inputs",
                    FriendlyName = $"RWR Object #{i} New Detection Flag",
                    Id = $"IP1310ALR__RWR_Object_New_Detection_Flag[{i}]",
                    Index = i,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = FriendlyName,
                    SourceAddress = _deviceID,
                    SubSource = null,
                    SubSourceFriendlyName = null,
                    SubSourceAddress = null,
                    State = false
                };
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
                    //dispose of managed resources here
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