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
namespace SimLinkup.HardwareSupport.Powell
{
    public class PowellIP1310ALRHardwareSupportModule : HardwareSupportModuleBase, IDisposable
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
        private const int SERIAL_WRITE_TIMEOUT = 500;
        private const bool FLUSH_WRITE_BUFFER = true;
        private const bool DISCARD_WRITE_BUFFER_AFTER_OPEN = true;

        //limits exceptions when we don't have the RWR plugged into the serial port
        private const int MAX_UNSUCCESSFUL_PORT_OPEN_ATTEMPTS = 5;
        private const bool RESET_UNSUCCESSFUL_CONNECTION_ATTEMPT_COUNTER_AFTER_CLOSING_PORT = true;

        //allows slowing down the rate of sending data across the bus
        private const int DELAY_AFTER_WRITES_MILLIS = 20; //delay, when set, occurs after *each* byte!
        private const int MAX_REFRESH_RATE_HZ = 25; //prevents more than this many occurrences per second of Synchronize() event from generating traffic

        //enable hack which forces pending data across the RS232 bus - despite all reason, this seems to work, where other approaches have failed.
        //The combination of the FTDI 232BL chip and the PIC microcontroller the way they're wired up right now seems to have flow control issues
        //which are mitigated by this approach
        private const bool CLOSE_AND_REOPEN_CONNECTION_AFTER_EACH_BYTE_SENT = false;
        private const int DELAY_AFTER_CLOSING_SERIAL_PORT = 20;

        #endregion

        #region Instance variables

        private readonly AnalogSignal[] _analogInputSignals;
        private readonly DigitalSignal[] _digitalInputSignals;
        private AnalogSignal _magneticHeadingDegreesInputSignal;
        private AnalogSignal _rwrSymbolCountInputSignal;
        private AnalogSignal[] _rwrObjectSymbolIDInputSignals = new AnalogSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private AnalogSignal[] _rwrObjectBearingInputSignals = new AnalogSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private AnalogSignal[] _rwrObjectLethalityInputSignals = new AnalogSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private DigitalSignal[] _rwrObjectMissileActivityFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private DigitalSignal[] _rwrObjectMissileLaunchFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private DigitalSignal[] _rwrObjectSelectedFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private DigitalSignal[] _rwrObjectNewDetectionFlagInputSignals = new DigitalSignal[MAX_RWR_SYMBOLS_AS_INPUTS];
        private IFalconRWRSymbolTranslator _falconRWRSymbolTranslator = new FalconRWRSymbolTranslator();

        private string _deviceID;
        
        private Common.IO.Ports.ISerialPort _serialPort;
        private object _serialPortLock = new object();
        private string _comPort;
        private int _unsuccessfulConnectionAttempts = 0;

        private DateTime _lastSynchronizedAt = DateTime.MinValue;
        private bool _resetNeeded = true;
        
        private bool _isDisposed;

        
        #endregion

        #region Constructors

        private PowellIP1310ALRHardwareSupportModule(string comPort,string deviceID=DEFAULT_DEVICE_ID)
        {
            _deviceID = deviceID;
            _comPort = comPort;
            CreateInputSignals(deviceID, out _analogInputSignals, out _digitalInputSignals);
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
        public override void Synchronize()
        {
            base.Synchronize();
            var timeSinceLastSynchronizedInMillis=DateTime.Now.Subtract(_lastSynchronizedAt).TotalMilliseconds;
            if (timeSinceLastSynchronizedInMillis > (1000.00 / MAX_REFRESH_RATE_HZ))
            {
                UpdateOutputs();
                _lastSynchronizedAt = DateTime.Now;
            }
        }

        #endregion

        #region Signals Handling
        #region Signals Event Handling

        
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
                if (_serialPort !=null && !_serialPort.IsOpen && _unsuccessfulConnectionAttempts <MAX_UNSUCCESSFUL_PORT_OPEN_ATTEMPTS)
                {
                    try
                    {
                        _serialPort.Handshake = HANDSHAKE;
                        _serialPort.WriteTimeout = SERIAL_WRITE_TIMEOUT;
                        _serialPort.ErrorReceived += _serialPort_ErrorReceived;
                        _serialPort.WriteBufferSize = WRITE_BUFFER_SIZE;
                        _log.DebugFormat("Opening serial port {0}: Handshake:{1}, WriteTimeout:{2}, WriteBufferSize:{3}", _comPort, HANDSHAKE.ToString(), SERIAL_WRITE_TIMEOUT, WRITE_BUFFER_SIZE);
                        _serialPort.Open();
                        if (DISCARD_WRITE_BUFFER_AFTER_OPEN)
                        {
                            _serialPort.DiscardOutBuffer();
                        }
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

                            Thread.Sleep(DELAY_AFTER_CLOSING_SERIAL_PORT);
                            _serialPort.DiscardOutBuffer();
                        }
                        _serialPort.Dispose();
                    }
                    catch {}
                    _serialPort = null;
                }
                if (RESET_UNSUCCESSFUL_CONNECTION_ATTEMPT_COUNTER_AFTER_CLOSING_PORT)
                {
                    _unsuccessfulConnectionAttempts = 0; //reset unsuccessful connection attempts counter
                }
            }
        }

        private IEnumerable<RWRCommand> GenerateCommandList()
        {
            var rwrCommands = new List<RWRCommand>();
            if (_resetNeeded)
            {
                //**IF** this is necessary at all, it should only be necessary when we first open the COM port and send our initial command.  
                //This will cause the device to reinitialize itself.  Once we run the command, we clear the _resetNeeded flag so we don't run
                //it a second time.
                rwrCommands.Add(new ResetCommand()); 
            }

            var blipList = GenerateBlipList();
            if (blipList.Count() > 0)
            {
                rwrCommands.Add(new DrawBlipsCommand { Blips = blipList });
            }
            return rwrCommands;
        }

        private IEnumerable<Blip> GenerateBlipList()
        {
            var numInputSymbols = (int)Math.Truncate(_rwrSymbolCountInputSignal.State);
            var blipList = new List<Blip>();
            var usePrimarySymbol = DateTime.Now.Millisecond < 500;
            for (var i = 0; i < numInputSymbols; i++)
            {
                var falconRWRSymbol = new FalconRWRSymbol
                {
                    SymbolID = (int)Math.Truncate(_rwrObjectSymbolIDInputSignals[i].State),
                    BearingDegrees = _rwrObjectBearingInputSignals[i].State,
                    Lethality = _rwrObjectLethalityInputSignals[i].State,
                    MissileActivity = _rwrObjectMissileActivityFlagInputSignals[i].State,
                    MissileLaunch = _rwrObjectMissileLaunchFlagInputSignals[i].State,
                    Selected = _rwrObjectSelectedFlagInputSignals[i].State,
                    NewDetection = _rwrObjectNewDetectionFlagInputSignals[i].State
                };
                var blips = _falconRWRSymbolTranslator.Translate(falconRWRSymbol, magneticHeadingDegrees: _magneticHeadingDegreesInputSignal.State, usePrimarySymbol: usePrimarySymbol, inverted: false);
                blipList.AddRange(blips);
            }
            return blipList;
        }
        private void SendCommandList(IEnumerable<RWRCommand> commandList)
        {
            if (commandList.Count() == 0) return;
            lock (_serialPortLock)
            {
                using (var ms = new MemoryStream())
                {
                    var totalBytes = 0;
                    foreach (var command in commandList)
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
                        var bytesToWrite = ms.GetBuffer();
                        try
                        {
                            _log.DebugFormat("Sending bytes to serial port {0}:{1}", _comPort, BytesToString(bytesToWrite, 0, totalBytes));
                            
                            //write out each byte, optionally flushing the buffer and optionally delaying between each byte to allow for slow remote end processing
                            for (int i = 0; i < totalBytes; i++)
                            {
                                EnsureSerialPortConnected();

                                _serialPort.Write(bytesToWrite, i, 1);
                                if (FLUSH_WRITE_BUFFER)
                                {
                                    _serialPort.BaseStream.Flush();
                                }
                                
                                Thread.Sleep(DELAY_AFTER_WRITES_MILLIS);
                                
                                if (CLOSE_AND_REOPEN_CONNECTION_AFTER_EACH_BYTE_SENT)
                                {
                                    CloseSerialPortConnection();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Error(e.Message, e);
                        }

                        //if we've just run a RESET command, ensure we don't run it again unless flagged to do so elsewhere
                        if (command is ResetCommand)
                        {
                            _resetNeeded = false;
                        }

                    }
                }
            }
        }
        private string BytesToString(byte[] bytes, int offset, int count)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} bytes:", count);
            for (var i = offset; i < offset + count; i++)
            {
                sb.Append("0x");
                sb.AppendFormat("{0:X} ", bytes[i]);
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
                var thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = "Magnetic Heading (Degrees)";
                thisSignal.Id = "IP1310ALR__Magnetic_Heading_Degrees";
                thisSignal.Index = 0;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                _magneticHeadingDegreesInputSignal = thisSignal;
                analogSignalsToReturn.Add(thisSignal);
            }
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