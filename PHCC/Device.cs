using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Diagnostics;
namespace Phcc
{
    //Source interface for events to be exposed.
    //Add GuidAttribute to the source interface to supply an explicit System.Guid.
    //Add InterfaceTypeAttribute to indicate that interface is IDispatch interface.
    /// <summary>
    /// COM Event Source Interface
    /// </summary>
    [GuidAttribute("8709CA5D-79FA-4a63-ACF4-C99475990BC3")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
    [ComVisible(true)]
    public interface PhccEvents
    {
        /// <summary>
        /// The <see cref="DigitalInputChanged"/> event is raised when 
        /// the PHCC motherboard detects that one of the digital inputs
        /// has changed (i.e. whenever a button that is wired 
        /// into the digital input key matrix is pressed or released). 
        /// </summary>
        [DispId(1)]
        void DigitalInputChanged(object sender, DigitalInputChangedEventArgs e);

        /// <summary>
        /// The <see cref="AnalogInputChanged"/> event is raised when
        /// the PHCC motherboard detects that one of the analog inputs 
        /// has changed values (i.e. whenever an analog input signal 
        /// changes state). 
        /// </summary>
        [DispId(2)]
        void AnalogInputChanged(object sender, AnalogInputChangedEventArgs e);

        /// <summary>
        /// The <see cref="I2CDataReceived"/> event is raised when
        /// the PHCC motherboard receives data from one of the attached 
        /// I2C peripherals (if any).
        /// </summary>
        [DispId(3)]
        void I2CDataReceived(object sender, I2CDataReceivedEventArgs e);
    }


    /// <summary>
    /// Enumeration of PHCC command codes
    /// </summary>
    internal enum Host2PhccCommands : byte
    {
        /// <summary>
        /// Set the PHCC to IDLE mode.
        /// </summary>
        Idle = 0x00,
        /// <summary>
        /// Command the PHCC to perform a software reset on itself.
        /// </summary>
        Reset = 0x01,
        /// <summary>
        /// Command the PHCC to start sending automatic digital input and 
        /// analog input change notifications.
        /// </summary>
        StartTalking = 0x02,
        /// <summary>
        /// Command the PHCC to stop sending automatic digital input and 
        /// analog input change notifications
        /// </summary>
        StopTalking = 0x03,
        /// <summary>
        /// Command the PHCC to send a full list of the current values 
        /// of all digital inputs.
        /// </summary>
        GetCurrentDigitalInputValues = 0x04,
        /// <summary>
        /// Command the PHCC to send a full list of the current values 
        /// of all prioritized and non-prioritized analog inputs.
        /// </summary>
        GetCurrentAnalogInputValues = 0x05,
        /// <summary>
        /// Command the PHCC to send data to an attached I2C peripheral.
        /// </summary>
        I2CSend = 0x06,
        /// <summary>
        /// Command the PHCC to send data to an attached
        /// Digital Output Type A (DOA) peripheral.
        /// </summary>
        DoaSend = 0x07,
        /// <summary>
        /// Command the PHCC to send data to an attached 
        /// Digital Output Type B (DOB) peripheral.
        /// </summary>
        DobSend = 0x08
    }
    /// <summary>
    /// Enumeration of stepper motor directions.
    /// </summary>
    public enum MotorDirections : byte
    {
        /// <summary>
        /// Specifies clockwise stepper motor movement.
        /// </summary>
        Clockwise = 0x00,
        /// <summary>
        /// Specifies counterclockwise stepper motor movement.
        /// </summary>
        Counterclockwise= 0x80

    }
    /// <summary>
    /// Enumeration of possible stepper motor step types.
    /// </summary>
    public enum MotorStepTypes : byte
    {
        /// <summary>
        /// Indicates that the step count refers to the number of full steps that the stepper motor should move.
        /// </summary>
        FullStep= 0x00,
        /// <summary>
        /// Indicates that the step count refers to the number of half steps that the stepper motor should move.
        /// </summary>
        HalfStep = 0x01
    }
    /// <summary>
    /// Enumeration of LCD data modes.
    /// </summary>
    public enum LcdDataModes : byte
    {
        /// <summary>
        /// Specifies that the data being sent to the LCD is to be considered as Display Data 
        /// </summary>
        DisplayData = 0x07,
        /// <summary>
        /// Specifies that the data being sent to the LCD is to be considered as Control Data 
        /// </summary>
        ControlData = 0x0F
    }

    /// <summary>
    /// Enumeration of packet types that PHCC returns over the serial 
    /// interface
    /// </summary>
    internal enum Phcc2HostPacketTypes : byte
    {
        /// <summary>
        /// Packet terminator containing all zeros
        /// </summary>
        AllBitsZero = 0x00,
        /// <summary>
        /// Packet header indicating a digital input change notification 
        /// packet
        /// </summary>
        DigitalInputUpdatePacket = 0x20,
        /// <summary>
        /// Packet header indicating an analog input change notification 
        /// packet
        /// </summary>
        AnalogInputUpdatePacket = 0x40,
        /// <summary>
        /// Packet header indicating data received from an attached 
        /// I2C peripheral
        /// </summary>
        I2CDataReceivedPacket = 0x60,
        /// <summary>
        /// Packet header indicating a packet containing the values of all 
        /// digital inputs
        /// </summary>
        DigitalInputsFullDumpPacket = 0x80,
        /// <summary>
        /// Packet header indicating a packet containing the values of all 
        /// prioritized and non-prioritized analog inputs
        /// </summary>
        AnalogInputsFullDumpPacket = 0xA0,
        /// <summary>
        /// Packet terminator containing all ones
        /// </summary>
        AllBitsOne = 0xFF,
        /// <summary>
        /// Bitmask for determining the packet type from the
        /// 3 most-significant bits of the first byte of a data packet
        /// </summary>
        PacketTypeMask = 0xE0,
    }
    /// <summary>
    /// Event handler delegate for the <see cref="Device.DigitalInputChanged"/> event.
    /// </summary>
    /// <param name="sender">the object raising the event.</param>
    /// <param name="e">a <see cref="DigitalInputChangedEventArgs"/> object containing detailed information about the event.</param>
    [ComVisible(false)]
    public delegate void DigitalInputChangedEventHandler(object sender, DigitalInputChangedEventArgs e);
    /// <summary>
    /// Event handler delegate for the <see cref="Device.AnalogInputChanged"/> event.
    /// </summary>
    /// <param name="sender">the object raising the event.</param>
    /// <param name="e">a <see cref="AnalogInputChangedEventArgs"/> object containing detailed information about the event.</param>
    [ComVisible(false)]
    public delegate void AnalogInputChangedEventHandler(object sender, AnalogInputChangedEventArgs e);
    /// <summary>
    /// Event handler delegate for the <see cref="Device.I2CDataReceived"/> event.
    /// </summary>
    /// <param name="sender">the object raising the event.</param>
    /// <param name="e">an <see cref="I2CDataReceivedEventArgs "/> object containing detailed information about the event.</param>
    [ComVisible(false)]
    public delegate void I2CDataReceivedEventHandler(object sender, I2CDataReceivedEventArgs e);

    //Seven-segment display bitmasks
    [ComVisible(true)]
    [Flags]
    public enum SevenSegmentBits : byte
    {
        /// <summary>
        /// Bitmask for all segments off
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Bitmask for segment A
        /// </summary>
        SegmentA = 0x80,
        /// <summary>
        /// Bitmask for segment B
        /// </summary>
        SegmentB = 0x40,
        /// <summary>
        /// Bitmask for segment C
        /// </summary>
        SegmentC = 0x20,
        /// <summary>
        /// Bitmask for segment D
        /// </summary>
        SegmentD = 0x08,
        /// <summary>
        /// Bitmask for segment E
        /// </summary>
        SegmentE = 0x04,
        /// <summary>
        /// Bitmask for segment F
        /// </summary>
        SegmentF = 0x02,
        /// <summary>
        /// Bitmask for segment G
        /// </summary>
        SegmentG = 0x01,
        /// <summary>
        /// Bitmask for decimal point segment
        /// </summary>
        SegmentDP = 0x10,
        /// <summary>
        /// Bitmask for all segments on
        /// </summary>
        All = 0xFF,
    }


    /// <summary>
    /// The <see cref="Device"/> class provides methods for
    /// communicating with the PHCC motherboard and 
    /// any attached peripherals, via RS232.  
    /// The PHCC USB interface also appears to Windows as 
    /// a standard RS232 COM port.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    [ComSourceInterfaces(typeof(PhccEvents))]
    [Synchronization]
    public sealed class Device : ContextBoundObject,IDisposable
    {
        /// <summary>
        /// The <see cref="DigitalInputChanged"/> event is raised when 
        /// the PHCC motherboard detects that one of the digital inputs
        /// has changed (i.e. whenever a button that is wired 
        /// into the digital input key matrix is pressed or released). 
        /// </summary>
        public event DigitalInputChangedEventHandler DigitalInputChanged;

        /// <summary>
        /// The <see cref="AnalogInputChanged"/> event is raised when
        /// the PHCC motherboard detects that one of the analog inputs 
        /// has changed values (i.e. whenever an analog input signal 
        /// changes state). 
        /// </summary>
        public event AnalogInputChangedEventHandler AnalogInputChanged;

        /// <summary>
        /// The <see cref="I2CDataReceived"/> event is raised when
        /// the PHCC motherboard receives data from one of the attached 
        /// I2C peripherals (if any).
        /// </summary>
        public event I2CDataReceivedEventHandler I2CDataReceived;

        /// <summary>
        /// Bitmask for determining the index of the digital 
        /// input whose value has changed.  
        /// This is useful when interpreting PHCC update data packets 
        /// that are received when the PHCC is in "talking" mode.
        /// </summary>
        private const ushort DigitalInputUpdatedIndexMask = 0x07FE;
        /// <summary>
        /// Bitmask for determining the new value of the digital 
        /// input whose value has 
        /// changed.  This is useful when interpreting PHCC update 
        /// data packets that are received when the PHCC is in "talking" 
        /// mode.  
        /// </summary>
        private const ushort DigitalInputNewValueMask = 0x0001;

        /// <summary>
        /// Bitmask for determining the index of the analog input 
        /// whose value has changed.  This is useful when 
        /// interpreting PHCC update data packets
        /// that are received when the PHCC is in "talking" mode.
        /// </summary>
        private const ushort AnalogInputUpdatedIndexMask = 0xFC00;

        /// <summary>
        /// Bitmask for determining the new value of the analog 
        /// input whose value has changed.  This is useful when 
        /// interpreting PHCC update data packets that are received 
        /// when the PHCC is in "talking" mode.  
        /// </summary>
        private const ushort AnalogInputNewValueMask = 0x03FF;

        /// <summary>
        /// Bitmask for determining the high-order bits of the address of the I2C peripheral which is sending data.  
        /// </summary>
        private const ushort I2CDataReceivedAddressHighOrderBitsMask = 0x03;

        /// <summary>
        /// A <see cref="System.IO.Ports.SerialPort"/> object that
        /// hides the details of communicating via RS232 over a COM port.
        /// </summary>
        private volatile SerialPort _serialPort;

        /// <summary>
        /// The name of the COM port to communicate over 
        /// (i.e. "COM1", "COM2", etc.)
        /// </summary>
        private volatile string _portName;

        /// <summary>
        /// A byte buffer to use when reading data from the PHCC.
        /// </summary>
        private volatile byte[] _readBuffer = new byte[20];

        /// <summary>
        /// A byte buffer to use when writing data to the PHCC.
        /// </summary>
        private volatile byte[] _writeBuffer = new byte[4];

        /// <summary>
        /// Flag for preventing the read buffer from being read 
        /// during certain operations
        /// </summary>
        private volatile bool _dontRead;

        /// <summary>
        /// Flag to keep track of whether the PHCC motherboard is sending automatic change
        /// notifications or not
        /// </summary>
        private volatile bool _talking;

        /// <summary>
        /// A byte buffer to store the current values of the 
        /// digital inputs.
        /// </summary>
        private volatile byte[] _currentDigitalInputValues = new byte[128];

        /// <summary>
        /// A byte buffer to store the un-parsed values of the 
        /// current analog input values list.
        /// </summary>
        /* Commenting out this declaration, which matches the implementation in PHCC2HostProtocol, but which does not match the actual implementation in Firmware14 *?
        //private volatile byte[] _currentAnalogInputsRaw = new byte[45];
        */
        private volatile byte[] _currentAnalogInputsRaw = new byte[70]; //this matches the implementation in Firmware18
        
        
        /// <summary>
        /// A byte buffer to store the parsed values of the 
        /// current analog input values list.
        /// </summary>
        private volatile short[] _currentAnalogInputsParsed = new short[35];

        /// <summary>
        /// Boolean flag indicating whether this object instance has 
        /// been <see cref="IDisposable.Dispose()"/>d.
        /// </summary>
        private bool _isDisposed;

        private object _rs232lock = new object();

        /// <summary>
        /// Creates an instance of the <see cref="Device"/> class.
        /// </summary>
        public Device()
        {
        }
        /// <summary>
        /// Creates an instance of the <see cref="Device"/> class.
        /// </summary>
        /// <param name="portName">The name of the COM port to use for 
        /// communicating with the PHCC motherboard (i.e. "COM1", "COM2",
        /// etc.)</param>
        public Device(string portName):this(portName,true)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="Device"/> class.
        /// </summary>
        /// <param name="portName">The name of the COM port to use for 
        /// communicating with the PHCC motherboard (i.e. "COM1", "COM2",
        /// etc.)</param>
        /// <param name="openPort">Specifies whether to open the COM port immediately or wait till the first operation that requires doing so.</param>
        public Device(string portName, bool openPort)
            : this()
        {
            _portName = portName;
            if (openPort)
            {
                EnsurePortIsReady();
            }

        }
        /// <summary>
        /// Gets the underlying <see cref="System.IO.Ports.SerialPort"/> object,
        /// which allows direct communication with the PHCC motherboard via
        /// RS232.
        /// </summary>
        public SerialPort SerialPort
        {
            get
            {
                return _serialPort;
            }
        }

        /// <summary>
        /// Gets/sets the name of the COM port to use for 
        /// communicating with the PHCC motherboard (i.e. "COM1", "COM2", 
        /// etc.)
        /// </summary>
        public string PortName
        {
            get
            {
                return _portName;
            }
            set
            {
                if (value == null || value.Trim().Equals(string.Empty))
                {
                    throw new ArgumentException("must contain a string that identifies a valid serial port on the machine (i.e. COM1, COM2, etc.)", "value");
                }
                ClosePort();
                _portName = value;
            }
        }

        /// <summary>
        /// Closes the serial port connection.
        /// </summary>
        private void ClosePort()
        {
            lock (_rs232lock)
            {
                if (_serialPort != null)
                {
                    try
                    {
                        if (_serialPort.IsOpen)
                        {
                            _serialPort.Close();
                        }
                        _serialPort.Dispose();
                    }
                    catch (Exception)
                    {
                    }

                    _serialPort = null;
                }
            }
            Thread.Sleep(500);
        }
        /// <summary>
        /// Establishes a serial port connection to the PHCC motherboard.
        /// </summary>
        private void InitializeSerialPort()
        {
            lock (_rs232lock)
            {
                ClosePort();
                _serialPort = new SerialPort();
                _serialPort.PortName = _portName;
                _serialPort.BaudRate = 115200;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;
                //_serialPort.Handshake = Handshake.None;
                _serialPort.Handshake = Handshake.RequestToSend;
                _serialPort.ReceivedBytesThreshold = 1;
                _serialPort.RtsEnable = true;
                _serialPort.ReadTimeout = 500; 
                _serialPort.WriteTimeout = 500; 
                //_serialPort.ReadTimeout = -1; //infinite
                //_serialPort.WriteTimeout = -1; //infinite
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
                _serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(_serialPort_ErrorReceived);
                _serialPort.Open();
                GC.SuppressFinalize(_serialPort.BaseStream);

            }

        }

        void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
          //do nothing   
        }
        /// <summary>
        /// Instructs the PHCC motherboard to perform a software reset 
        /// on itself.
        /// </summary>
        public void Reset()
        {
            EnsurePortIsReady(); 
            _writeBuffer[0] = (byte)Host2PhccCommands.Reset;
            _writeBuffer[1] = (byte)Host2PhccCommands.Reset;
            _writeBuffer[2] = (byte)Host2PhccCommands.Reset;
            Debug.WriteLine("In Reset, Writing 3 bytes to the serial port.");
            RS232Write(_writeBuffer, 0, 3);
        }
        /// <summary>
        /// Sends data to an individual HD44780-compatible character LCD 
        /// display wired to a PHCC DOA_char_lcd character LCD driver 
        /// daughtercard.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific 
        /// DOA_char_lcd character LCD driver daughtercard to send data to.</param>
        /// <param name="displayNum">The display number of the individual
        /// HD44780-compatible character LCD wired to the indicated 
        /// DOA_char_lcd character LCD driver daughtercard, to which, 
        /// the specified <paramref name="data"/> will be sent. </param>
        /// <param name="mode">A value from the <see cref="LcdDataModes"/> 
        /// enumeration, specifying whether the value contained in 
        /// the <paramref name="data"/> parameter is to be considered 
        /// Display Data or Control Data.</param>
        /// <param name="data">The actual data value to send to the 
        /// indicated  HD44780-compatible character LCD display.</param>
        public void DoaSendCharLcd(byte deviceAddr, byte displayNum, LcdDataModes mode, byte data)
        {
            if (displayNum == 0 || displayNum > 8)
            {
                throw new ArgumentOutOfRangeException("displayNum", "must be between 1 and 8");
            }
            DoaSendRaw(deviceAddr, (byte)(displayNum & ((byte)mode)), data);
        }
        /// <summary>
        /// Sends data to a DOA_AnOut1 analog output daughtercard, to
        /// control the gain parameter which is in effect for all 
        /// channels simultaneously.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific 
        /// DOA_AnOut1 daughtercard to send data to.</param>
        /// <param name="gain">A byte whose integer value specifies the new
        /// value to set for the all-channels gain parameter.</param>
        public void DoaSendAnOut1GainAllChannels(byte deviceAddr, byte gain)
        {
            DoaSendRaw(deviceAddr, 0x10, gain);
        }
        /// <summary>
        /// Sends data to a DOA_AnOut1 analog output daughtercard, to 
        /// control the RMS voltage (using PWM) being supplied by 
        /// that channel.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific 
        /// DOA_AnOut1 daughtercard to send data to.</param>
        /// <param name="channelNum">The channel number of the channel 
        /// on the DOA_AnOut1 daughtercard whose PWM output voltage is 
        /// to be set.</param>
        /// <param name="value">A byte whose integer value controls the PWM pulse width (
        /// delay time), which, in turn, dictates the RMS (average) voltage between
        /// the control pins on the specified channel.</param>
        public void DoaSendAnOut1(byte deviceAddr, byte channelNum, byte value)
        {
            if (channelNum == 0 || channelNum> 16)
            {
                throw new ArgumentOutOfRangeException("channelNum", "must be between 1 and 16");
            }
            DoaSendRaw(deviceAddr, (byte)((int)channelNum - 1), value);
        }
        /// <summary>
        /// Sends data to a DOA_servo daughtercard to control the gain 
        /// parameter of an individual servo wired to the card.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific 
        /// DOA_servo daughtercard to send data to.</param>
        /// <param name="servoNum">The number of the specific servo on the
        /// DOA_servo daughtercard, to which this gain parameter value will be applied.</param>
        /// <param name="gain">The new gain value to use for the specified 
        /// servo.</param>
        public void DoaSend8ServoGain(byte deviceAddr, byte servoNum, byte gain)
        {
            if (servoNum == 0 || servoNum > 8)
            {
                throw new ArgumentOutOfRangeException("servoNum", "must be between 1 and 8");
            }
            DoaSendRaw(deviceAddr, (byte)(((int)servoNum - 1) + 24), gain);
        }

        /// <summary>
        /// Sends data to a DOA_servo daughtercard to control the 
        /// calibration of an individual servo wired to the card.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific 
        /// DOA_servo daughtercard to send data to.</param>
        /// <param name="servoNum">The number of the specific servo on the
        /// DOA_servo daughtercard, to which this calibration data will 
        /// apply.</param>
        /// <param name="calibrationOffset">The new 16-bit calibration 
        /// offset value to use with this specific servo.</param>
        public void DoaSend8ServoCalibration(byte deviceAddr, byte servoNum, Int16 calibrationOffset)
        {
            if (servoNum == 0 || servoNum > 8)
            {
                throw new ArgumentOutOfRangeException("servoNum", "must be between 1 and 8");
            }
            byte[] calibrationBytes = BitConverter.GetBytes(calibrationOffset);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(calibrationBytes);
            }
            DoaSendRaw(deviceAddr, (byte)(((int)servoNum - 1) + 8), calibrationBytes[0]);
            DoaSendRaw(deviceAddr, (byte)((int)(servoNum - 1) + 16), calibrationBytes[1]);
        }
        /// <summary>
        /// Sends data to a DOA_servo daughtercard to control the position 
        /// of an individual servo wired to the card.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific 
        /// DOA_servo daughtercard to send data to.</param>
        /// <param name="servoNum">The number of the specific servo on the
        /// DOA_servo daughtercard, to which this position update will 
        /// be sent.</param>
        /// <param name="position">The position value to set this servo 
        /// to.</param>
        public void DoaSend8ServoPosition(byte deviceAddr, byte servoNum, byte position)
        {
            if (servoNum == 0 || servoNum > 8)
            {
                throw new ArgumentOutOfRangeException("servoNum", "must be between 1 and 8");
            }

            DoaSendRaw(deviceAddr, (byte)((int)servoNum - 1), position);
        }
        /// <summary>
        /// Sends data to a DOA_877_4067 daughtercard to control the 
        /// 7-segment LCDs (or individual LEDs) wired to the card.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific 
        /// DOA_877_4067 daughtercard to send data to.</param>
        /// <param name="displayNum">The number of the 7-segment display 
        /// (1-48), on the specified daughtercard, that 
        /// the <paramref name="bits"/> parameter will control.</param>
        /// <param name="bits">A byte, whose bits correspond to individual 
        /// segments of the specified 7-segment display 
        /// (including the decimal point).  
        /// Each bit in this byte that is set to <see langref="true"/> will 
        /// result in a logic HIGH being sent to the 
        /// corresponding segment (or LED).</param>
        public void DoaSend7Seg8774067(byte deviceAddr, byte displayNum, byte bits)
        {
            if (displayNum == 0 || displayNum > 48)
            {
                throw new ArgumentOutOfRangeException("displayNum", "must be between 1 and 48");
            }
            DoaSendRaw(deviceAddr, (byte)((int)displayNum - 1), bits);
        }

        /// <summary>
        /// Sends data to a DOA_7Seg daughtercard to control the 7-segment 
        /// LCDs (or individual LEDs) wired to the card.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific
        /// DOA_7Seg daughtercard to send data to.</param>
        /// <param name="displayNum">The number of the 7-segment display 
        /// (1-32), on the specified daughtercard, that 
        /// the <paramref name="bits"/> parameter will control.</param>
        /// <param name="bits">A byte, whose bits correspond to individual 
        /// segments of the specified 7-segment display (including the 
        /// decimal point).  Each bit in this byte that is set 
        /// to <see langref="true"/> will result in a logic HIGH 
        /// being sent to the corresponding segment (or LED).</param>
        public void DoaSend7Seg(byte deviceAddr, byte displayNum, byte bits)
        {
            if (displayNum == 0 || displayNum > 32)
            {
                throw new ArgumentOutOfRangeException("displayNum", "must be between 1 and 32");
            }
            DoaSendRaw(deviceAddr, (byte)((int)displayNum-1), bits);
        }

        /// <summary>
        /// Produces a 7-segment bitmask with the appropriate bits set, to represent a specific Latin character 
        /// </summary>
        /// <param name="charToConvert">a Latin character to produce a seven-segment display bitmask from</param>
        /// <returns>a byte whose bits are set appropriately for sending to a seven-segment display</returns>
        public byte CharTo7Seg(char charToConvert)
        {
            switch(charToConvert)
            {
                case '0': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case '1': return (byte) (SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC);
                case '2': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentG | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentD);
                case '3': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentG);
                case '4': return (byte) (SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC);
                case '5': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD);
                case '6': return (byte) (SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentG);
                case '7': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC);
                case '8': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case '9': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case '.': return (byte) (SevenSegmentBits.SegmentDP);
                case '-': return (byte) (SevenSegmentBits.SegmentG);
                case '+': return (byte) (SevenSegmentBits.SegmentG | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentE);
                case '!': return (byte)(SevenSegmentBits.SegmentB |  SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentDP);
                case '@': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                case '#': return (byte)(SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case '$': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case '%': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentG);
                case '^': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentF);
                case '&': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case '*': return (byte)(SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case '(': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case ')': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD);
                case '_': return (byte)(SevenSegmentBits.SegmentD);
                case '=': return (byte)(SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentG);
                case '[': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case ']': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD);
                case '{': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case '}': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD);
                case '|': return (byte)(SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case '\\': return (byte)(SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case ':': return (byte)(SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case ';': return (byte)(SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentD);
                case '\'': return (byte)(SevenSegmentBits.SegmentB );
                case '"': return (byte)(SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentF);
                case '<': return (byte)(SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                case '>': return (byte)(SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentG);
                case ',': return (byte)(SevenSegmentBits.SegmentE);
                case '/': return (byte)(SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                case '?': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG | SevenSegmentBits.SegmentDP);
                case '`': return (byte)(SevenSegmentBits.SegmentF);
                case '~': return (byte)(SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);

                case 'A': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'a': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                case 'B': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'b': return (byte) (SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'C': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case 'c': return (byte) (SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                case 'D': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case 'd': return (byte) (SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                case 'E': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'e': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'F': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'f': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'G': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case 'g': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'H': return (byte)(SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'h': return (byte) (SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'I': return (byte) (SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF );
                case 'i': return (byte) (SevenSegmentBits.SegmentC);
                case 'J': return (byte) (SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE);
                case 'j': return (byte) (SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD );
                case 'K': return (byte) (SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'k': return (byte) (SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'L': return (byte) (SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case 'l': return (byte) (SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE );
                case 'M': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentF);
                case 'm': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentF);
                case 'N': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case 'n': return (byte) (SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                case 'O': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case 'o': return (byte) (SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                case 'P': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'p': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'Q': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'q': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'R': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case 'r': return (byte) (SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                case 'S': return (byte)(SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 's': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'T': return (byte) (SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 't': return (byte) (SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'U': return (byte)(SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF);
                case 'u': return (byte) (SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE );
                case 'V': return (byte)(SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentF); 
                case 'v': return (byte)(SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'W': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE);
                case 'w': return (byte) (SevenSegmentBits.SegmentA | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentE);
                case 'X': return (byte) (SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'x': return (byte) (SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'Y': return (byte) (SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'y': return (byte) (SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentC | SevenSegmentBits.SegmentD | SevenSegmentBits.SegmentF | SevenSegmentBits.SegmentG);
                case 'Z': return (byte)(SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                case 'z': return (byte) (SevenSegmentBits.SegmentB | SevenSegmentBits.SegmentE | SevenSegmentBits.SegmentG);
                default:
                return 0x00;
            }
        }

        /// <summary>
        /// Sends data to a DOA_40DO digital output daughtercard.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific 
        /// DOA_40DO daughtercard to send data to.</param>
        /// <param name="connectorNum">The output connector number to 
        /// send data to (3=CON3, 4=CON4, 5=CON5, 6=CON6, 7=CON7)</param>
        /// <param name="bits">A byte, whose bits correspond to the pins on 
        /// the specified connector.  Each bit in this byte that is set to 
        /// TRUE will result in a logic HIGH on the corresponding output 
        /// pin on the specified connector.</param>
        public void DoaSend40DO(byte deviceAddr, byte connectorNum, byte bits)
        {
            if (connectorNum <3 || connectorNum > 7)
            {
                throw new ArgumentOutOfRangeException("connectorNum", "must be between 3 and 7");
            }
            DoaSendRaw(deviceAddr, (byte)((int)connectorNum -3), bits);
        }

        /// <summary>
        /// Sends data to an air core motor daughtercard.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific 
        /// air core motor daughtercard to send data to.</param>
        /// <param name="motorNum">The motor number (1-4) of the motor to control.</param>
        /// <param name="position">A value from 0-1023 indicating the position to move the motor to.  Zero is straight north and the scale proceeds clockwise.</param>
        public void DoaSendAirCoreMotor(byte deviceAddr, byte motorNum, int position)
        {
            if (motorNum < 1 || motorNum > 4)
            {
                throw new ArgumentOutOfRangeException("motorNum", "must be between 1 and 4");
            }
            if (position < 0 || position > 1023)
            {
                throw new ArgumentOutOfRangeException("position", "must be between 0 and 1023");
            }

            byte quadrant = 0;
            byte pos = 0;
            if (position >= 0 && position <= 255)
            {
                quadrant = 1;
                pos = (byte)(255 - position);
            }
            else if (position >= 256 && position <= 511)
            {
                quadrant = 3;
                pos = (byte)(position - 256);
            }
            else if (position >= 512 && position <= 767)
            {
                quadrant = 4;
                pos = (byte)(255 - (position - 512));
            }
            else if (position >= 768 && position <= 1023)
            {
                quadrant = 2;
                pos = (byte)(position - 768);
            }
            byte motorNumMask = (byte)((motorNum-1) << 2);
            byte subAddress = (byte)(((byte)(quadrant-1)) | motorNumMask);

            DoaSendRaw(deviceAddr, subAddress, pos);
        }

        /// <summary>
        /// Sends data to a stepper motor daughtercard.
        /// </summary>
        /// <param name="deviceAddr">The device address of the specific 
        /// stepper motor daughtercard to send data to.</param>
        /// <param name="motorNum">The motor number (1-4) of the motor to control.</param>
        /// <param name="direction">A value from the <see cref="MotorDirections"/> enumeration, indicating the direction (clockwise or counterclockwise) to move the motor (this ultimately depends on how the motor is wired to the card).</param>
        /// <param name="numSteps">A byte, whose value (0-127) represents the number of discrete steps to command the stepper motor to move, in the indicated direction.</param>
        /// <param name="stepType">A value from the <see cref="MotorStepTypes"/> enumeration, indicating whether to move the motor in full-steps or in half-steps.</param>
        public void DoaSendStepperMotor(byte deviceAddr, byte motorNum, MotorDirections direction, byte numSteps, MotorStepTypes stepType)
        {
            if (motorNum< 1 || motorNum > 4)
            {
                throw new ArgumentOutOfRangeException("motorNum", "must be between 1 and 4");
            }
            if (numSteps < 0 || numSteps > 127)
            {
                throw new ArgumentOutOfRangeException("numSteps", "must be between 0 and 127");
            }
            if (stepType == MotorStepTypes.HalfStep)
            {
                motorNum += 4;
            }
            DoaSendRaw(deviceAddr, (byte)((int)motorNum-1), ((byte)((byte)direction | numSteps)));
        }

        
        
        /// <summary>
        /// Sends data to a Digital Output Type A (DOA) peripheral attached to
        /// the PHCC motherboard.
        /// </summary>
        /// <param name="addr">The device address of the specific 
        /// Digital Output Type A (DOA)
        /// peripheral to send data to.</param>
        /// <param name="subAddr">The sub-address of the 
        /// Digital Output Type A (DOA) peripheral to send data to.</param>
        /// <param name="data">The data to send to the specified 
        /// Output Type A (DOA) peripheral.</param>
        public void DoaSendRaw(byte addr, byte subAddr, byte data)
        {
            EnsurePortIsReady();
            _writeBuffer[0] = (byte)Host2PhccCommands.DoaSend;
            _writeBuffer[1] = addr;
            _writeBuffer[2] = subAddr;
            _writeBuffer[3] = data;
            Debug.WriteLine("In DoaSendRaw, Writing 4 bytes to the serial port.");
            RS232Write(_writeBuffer, 0, 4);
        }
        /// <summary>
        /// Sends data to a Digital Output Type B (DOB) peripheral attached 
        /// to the PHCC motherboard.
        /// </summary>
        /// <param name="addr">The address of the 
        /// Digital Output Type B (DOB) peripheral to send data to.</param>
        /// <param name="data">The data to send to the specified 
        /// Digital Output Type B (DOB) peripheral.</param>
        public void DobSendRaw(byte addr, byte data)
        {
            EnsurePortIsReady();
            _writeBuffer[0] = (byte)Host2PhccCommands.DobSend;
            _writeBuffer[1] = addr;
            _writeBuffer[2] = data;
            Debug.WriteLine("In DobSendRaw, Writing 3 bytes to the serial port.");
            RS232Write(_writeBuffer, 0, 3);
        }
        /// <summary>
        /// Sends data to an I2C peripheral attached to the 
        /// PHCC motherboard.
        /// </summary>
        /// <param name="addr">The address of the I2C peripheral 
        /// to send data to.</param>
        /// <param name="subAddr">The sub-address of the I2C 
        /// peripheral to send data to.</param>
        /// <param name="data">The data to send to the specified 
        /// I2C peripheral.</param>
        public void I2CSend(byte addr, byte subAddr, byte data)
        {
            EnsurePortIsReady();
            _writeBuffer[0] = (byte)Host2PhccCommands.I2CSend;
            _writeBuffer[1] = addr;
            _writeBuffer[2] = subAddr;
            _writeBuffer[3] = data;
            Debug.WriteLine("In I2CSend, Writing 4 bytes to the serial port.");
            RS232Write(_writeBuffer, 0, 4);
        }
        /// <summary>
        /// Informs the PHCC motherboard to stop sending change notification events
        /// </summary>
        public void StopTalking()
        {
            Debug.WriteLine("StopTalking entered.");
            /*
            if (!_talking)
            {
                return;
            }
             */
            EnsurePortIsReady();
            bool oldDontRead = _dontRead;
            try
            {
                WaitForInputBufferQuiesce();
                _writeBuffer[0] = (byte)Host2PhccCommands.StopTalking;
                Debug.WriteLine("In StopTalking, writing 1 byte to the serial port.");
                RS232Write(_writeBuffer, 0, 1);
                WaitForInputBufferQuiesce();
                _talking = false;
            }
            finally
            {
                _dontRead = oldDontRead;
            }
        }
        private void Rs232DiscardInBuffer()
        {
            lock (_rs232lock)
            {
                Debug.WriteLine("Discarding RS232 port's input buffer contents.");
                _serialPort.DiscardInBuffer();
            }
        }
        private void WaitForInputBufferQuiesce()
        {
            Debug.WriteLine("Waiting for input buffer to quiesce...");
            EnsurePortIsReady();
            bool oldDontRead = _dontRead;
            try
            {
                bool done = false;
                while (!done)
                {
                    Rs232DiscardInBuffer(); //now discard whatever's in the buffer
                    if (Rs232BytesAvailable() == 0) //if there's nothing in the buffer then we have quiesced, otherwise wait some more
                    {
                        done = true;
                    }
                    //System.Windows.Forms.Application.DoEvents();
                }
                Rs232DiscardInBuffer(); //now discard whatever's left in the buffer
            }
            finally
            {
                _dontRead = oldDontRead;
            }
            Debug.WriteLine("Done waiting for input buffer to quiesce.");
        }
        /// <summary>
        /// Reads a packet from the RS232 serial port containing a report on 
        /// a single byte of data received from an attached I2C peripheral
        /// </summary>
        private void ReadI2CDataReceivedPacket()
        {
            Debug.WriteLine("In ReadI2CDataReceivedPacket, about to read 70 bytes from the serial port.");
            Rs232Read(_readBuffer, 1, 2);
            byte addressLowOrderBits = _readBuffer[1];
            byte addressHighOrderBits = (byte)(_readBuffer[0] | I2CDataReceivedAddressHighOrderBitsMask);

            ushort address= 0;
            if (BitConverter.IsLittleEndian)
            {
                address = BitConverter.ToUInt16(new byte[] { addressLowOrderBits, addressHighOrderBits }, 0);
            }
            else
            {
                address = BitConverter.ToUInt16(new byte[] { addressHighOrderBits, addressLowOrderBits }, 0);
            }

            byte data = _readBuffer[2];
            if (I2CDataReceived != null)
            {
                I2CDataReceived(this,
                    new I2CDataReceivedEventArgs((short)address, data));
            }
        }
        /// <summary>
        /// Reads a packet from the RS232 serial port containing a report on 
        /// a single analog input value change event
        /// </summary>
        private void ReadAnalogInputUpdatePacket()
        {
            Debug.WriteLine("In ReadAnalogInputUpdatePacket, about to read 2 bytes from the serial port.");
            Rs232Read(_readBuffer, 1, 2);
            //ushort bits = ConvertBytesToUShort(_readBuffer, 0);
            ushort bits = (ushort)_readBuffer[1];
            //byte index = (byte)((bits & AnalogInputUpdatedIndexMask)>>10);
            byte index = (byte)(bits >>2);
            bits = ConvertBytesToUShort(_readBuffer, 1);
            ushort newValue = (ushort)(bits & AnalogInputNewValueMask);
            if (AnalogInputChanged != null)
            {
                AnalogInputChanged(this,
                    new AnalogInputChangedEventArgs(index, (short)newValue));
            }
        }
        /// <summary>
        /// Reads a packet from the RS232 serial port containing a report on 
        /// a single digital input value change event
        /// </summary>
        private void ReadDigitalInputUpdatePacket()
        {
            Debug.WriteLine("In ReadDigitalInputUpdatePacket, about to read 1 bytes from the serial port.");
            Rs232Read(_readBuffer, 1, 1);
            ushort bits = ConvertBytesToUShort(_readBuffer, 0);
            ushort index = (ushort)((bits & DigitalInputUpdatedIndexMask) >>1);
            bool newVal = ((bits & DigitalInputNewValueMask) != 0);
            if (DigitalInputChanged != null)
            {
                DigitalInputChanged(this,
                    new DigitalInputChangedEventArgs((short)index, newVal));
            }
        }
        /// <summary>
        ///Reads a packet from the RS232 serial port containing a full update of
        /// all digital input values.
        /// </summary>
        private void ReadDigitalInputFullDumpPacket()
        {
            bool oldDontRead = _dontRead;
            try
            {
                _dontRead = true;
                Debug.WriteLine("In ReadDigitalInputFullDumpPacket, about to read 128 bytes from the serial port.");
                Rs232Read(_currentDigitalInputValues, 0, 128);
            }
            finally
            {
                _dontRead = oldDontRead;
            }
        }
        /// <summary>
        /// Reads a packet from the RS232 serial port containing a full update of
        /// all analog input values.
        /// </summary>
        private void ReadAnalogInputFullDumpPacket()
        {
            bool oldDontRead = _dontRead;
            try
            {
                _dontRead = true;
                /* 
                //this is the implementation that matches the PHCC2HostProtocol documentation, but this is not how it is implemented in Firmware18
                Debug.WriteLine("In ReadAnalogInputFullDumpPacket, about to read 45 bytes from the serial port.");
                Rs232Read(_currentAnalogInputsRaw, 0, 45);
                */
                Debug.WriteLine("In ReadAnalogInputFullDumpPacket, about to read 70 bytes from the serial port.");
                Rs232Read(_currentAnalogInputsRaw, 0, 70);

            }
            finally
            {
                _dontRead = oldDontRead;
            }
            ParseRawAnalogInputs(_currentAnalogInputsRaw, out _currentAnalogInputsParsed);

        }
        /// <summary>
        /// Informs the PHCC motherboard to start sending automatic change notification events.
        /// </summary>
        public void StartTalking()
        {
            Debug.Write("StartTalking entered.");
            EnsurePortIsReady();
            _writeBuffer[0] = (byte)Host2PhccCommands.StartTalking;
            Debug.WriteLine("In StartTalking, Writing 1 byte to the serial port.");
            RS232Write(_writeBuffer, 0, 1);
            _talking = true;
        }
        /// <summary>
        /// Event handler responsible for reading data from the serial port when it arrives
        /// </summary>
        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_rs232lock)
            {
                try
                {
                    ProcessBufferContents();
                }
                catch (Exception)
                {
                }
            }
        }
        /// <summary>
        /// Processes the contents of the UART buffer.
        /// </summary>
        private void ProcessBufferContents()
        {
            if (_dontRead) //if reading has been temporarily disabled here so 
                            //that bytes don't disappear from the buffer 
                            //when needed elsewhere, then yield before reading any
                            //here
            {
                Debug.WriteLine("In ProcessBufferContents, the dontRead flag is enabled so skipping");
                return;
            }
            _readBuffer.Initialize();
            try
            {
                while (_serialPort.BytesToRead > 0)
                {
                    Debug.WriteLine("In ProcessBufferContents, about to read one byte from the serial port.");
                    Rs232Read(_readBuffer, 0, 1);
                    Debug.WriteLine("In ProcessBufferContents, just finished reading one byte from the serial port."); 
                    switch ((byte)(_readBuffer[0] & (byte)Phcc2HostPacketTypes.PacketTypeMask))
                    {
                        case (byte)Phcc2HostPacketTypes.I2CDataReceivedPacket:
                            Debug.WriteLine("I2CDataReceived packet received");
                            ReadI2CDataReceivedPacket();
                            break;
                        case (byte)Phcc2HostPacketTypes.AnalogInputUpdatePacket:
                            Debug.WriteLine("AnalogInputUpdate packet received");
                            ReadAnalogInputUpdatePacket();
                            break;
                        case (byte)Phcc2HostPacketTypes.DigitalInputUpdatePacket:
                            Debug.WriteLine("DigitalInputUpdate packet received");
                            ReadDigitalInputUpdatePacket();
                            break;
                        case (byte)Phcc2HostPacketTypes.DigitalInputsFullDumpPacket:
                            Debug.WriteLine("DigitalInputFullDump packet received");
                            ReadDigitalInputFullDumpPacket();
                            break;
                        case (byte)Phcc2HostPacketTypes.AnalogInputsFullDumpPacket:
                            Debug.WriteLine("AnalogInputFullDump packet received");
                            ReadAnalogInputFullDumpPacket();
                            break;
                        case (byte)Phcc2HostPacketTypes.AllBitsOne:
                            Debug.WriteLine("AllBitsOne packet received");
                            break;
                        case (byte)Phcc2HostPacketTypes.AllBitsZero:
                            Debug.WriteLine("AllBitsZero packet received");
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (TimeoutException)
            {
            }
        }
        /// <summary>
        /// Gets a bool array containing the current values of 
        /// all digital inputs.
        /// </summary>
        /// <returns>A bool array containing the current values of 
        /// all digital inputs.  Each value in the array represents
        /// a single discrete digital input, out of a total of 
        /// 1024 inputs.</returns>
        public bool[] DigitalInputs
        {
            get
            {
                bool[] toReturn = new bool[1024];
                PollDigitalInputs();
                for (int i = 0; i < toReturn.Length; i++)
                {
                    byte relevantByte = _currentDigitalInputValues[(int)((i) / 8)];
                    toReturn[i]=((relevantByte & (byte)(Math.Pow(2, (i % 8)))) != 0);
                }
                return toReturn;
            }
        }
        /// <summary>
        /// Commands the PHCC motherboard to send a full report of the current
        /// digital input values.
        /// </summary>
        private void PollDigitalInputs()
        {
            bool wasTalking = _talking;
            EnsurePortIsReady();
            if (_talking)
            {
                StopTalking();
            }
            _writeBuffer[0] = (byte)Host2PhccCommands.GetCurrentDigitalInputValues;
            Debug.WriteLine("In PollDigitalInputs, Writing 1 byte to the serial port.");
            RS232Write(_writeBuffer, 0, 1);
            ProcessBufferContents();
            if (wasTalking) {
                StartTalking();
            }
        }
        /// <summary>
        /// Gets an array of 16-bit signed integers containing the current values 
        /// of all analog inputs.  Only the low 10 bits contain information; 
        /// the high 6 bits are always zero because the precision of the 
        /// analog inputs is currently limited to 10 bits.
        /// </summary>
        /// <returns>An array of 16-bit signed integers containing the current
        /// values of all analog inputs.  Only the low 10 bits 
        /// in each array element contain useful information; 
        /// the high 6 bits are always zero because the 
        /// precision of the PHCC analog inputs is currently 
        /// limited to 10 bits.</returns>
        public short[] AnalogInputs
        {
            get
            {
                PollAnalogInputs();
                return _currentAnalogInputsParsed;
            }
        }
        /// <summary>
        /// Commands the PHCC motherboard to send a full report of the current
        /// analog input values
        /// </summary>
        private void PollAnalogInputs()
        {
            bool wasTalking = _talking;
            EnsurePortIsReady();
            if (_talking)
            {
                StopTalking();
            }
            _writeBuffer[0] = (byte)Host2PhccCommands.GetCurrentAnalogInputValues;
            Debug.WriteLine("In PollAnalogInputs, Writing 1 byte to the serial port.");
            RS232Write(_writeBuffer, 0, 1);
            ProcessBufferContents();
            if (wasTalking)
            {
                StartTalking();
            }
        }
        private void RS232Write(string toWrite)
        {
            EnsurePortIsReady();
            lock (_rs232lock)
            {
                _serialPort.Write(toWrite);
            }
        }
        private void RS232Write(byte[] buffer, int index, int count)
        {
            EnsurePortIsReady();
            lock (_rs232lock)
            {
                _serialPort.Write(buffer, index, count);
            }
        }
        /// <summary>
        /// Parses the raw analog input values list that the P
        /// HCC motherboard provides, by combining the low and 
        /// high bytes into a single 16-bit value for 
        /// each analog input value in the "raw" analog values list.
        /// </summary>
        /// <param name="raw">A byte array containing 2 bytes for each 
        /// analog input.</param>
        /// <param name="processed">An array of 16-bit signed integers to 
        /// hold the result of combining corresponding pairs of bytes
        /// from the "raw" analog input values list.</param>
        private static void ParseRawAnalogInputs(byte[] raw, out short[] processed)
        {
            /* This is the algorithm for processing the analog inputs per the current PHCC documentation, which DOES NOT match the actual implementation in Firmware18
            int curInput = 1;
            for (int i = 0; i < raw.Length; i++)
            {
                byte curByte = raw[i];
                if ((i +1) % 5 == 0 && i > 0)
                {
                    //this byte contains the high-order 2 bits of the previous 4 axes values
                    for (int j = 1; j <= 4; j++) //for each of the previously-processed 4 axes,
                    {
                        if ((curInput - j) < processed.Length)
                        {
                            int mask = 0x3 << (2 * (j - 1)); // calculate a bitmask that will pass the 2 bits of the current byte that represent the high-order 2 bits of the axis but which will block the other bits
                            byte highOrderByte = (byte)(curByte & mask); //mask out any other bits in the current byte
                            byte lowOrderByte = (byte)processed[curInput - j]; //retrieve the low-order bits from the axes-values array where they've already been placed
                            ushort combined = 0;
                            if (BitConverter.IsLittleEndian)
                            {
                                combined = BitConverter.ToUInt16(new byte[] { lowOrderByte, highOrderByte }, 0);
                            }
                            else
                            {
                                combined = BitConverter.ToUInt16(new byte[] { highOrderByte, lowOrderByte }, 0);
                            }
                            processed[curInput - j] = (short)combined;
                        }
                    }
                    continue;
                }
                else
                {
                    //this byte contains the low-order 8 bits of the current axis
                    if (curInput <= processed.Length)
                    {
                        processed[curInput - 1] = (short)curByte;
                    }
                    curInput++;
                }
            }*/
            //this is the implementation for firmware18
            processed = new short[35];
            for (int i = 0; i < raw.Length; i += 2)
            {
                processed[i / 2] = (short)ConvertBytesToUShort(raw, i);
            }

        }
        /// <summary>
        /// Establishes a connection to the PHCC motherboard via RS232.
        /// </summary>
        private void EnsurePortIsReady()
        {
            lock (_rs232lock)
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    InitializeSerialPort();
                }
            }
        }
        /// <summary>
        /// Instructs the PHCC motherboard to enter the IDLE state.
        /// </summary>
        public void SetIdle()
        {
            EnsurePortIsReady(); 
            _writeBuffer[0] = (byte)Host2PhccCommands.Idle;
            Debug.WriteLine("In SetIdle, Writing 1 byte to the serial port.");
            RS232Write(_writeBuffer, 0, 1);
        }
        /// <summary>
        /// Gets a string containing the PHCC motherboard's firmware 
        /// version.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the PHCC motherboard's
        /// firmware version.</returns>
        public string FirmwareVersion
        {
            get
            {
                string toReturn = null;
                bool wasTalking = _talking;
                bool oldDontRead = _dontRead;
                try
                {
                    EnsurePortIsReady();
                    _dontRead = true; //temporarily disable the buffer-reader event handler
                    if (_talking)
                    {
                        StopTalking();
                    }
                    else
                    {
                        WaitForInputBufferQuiesce();
                    }
                    Debug.WriteLine("In FirmwareVersion, Writing 1 byte to the serial port.");
                    RS232Write(" ");
                    _readBuffer.Initialize();
                    Rs232Read(_readBuffer, 0, 10);
                    toReturn = Encoding.ASCII.GetString(_readBuffer, 0, 10);
                    Debug.WriteLine("In FirmwareVersion, bytes read = " + toReturn);
                }
                finally
                {
                    _dontRead = oldDontRead; 
                    if (wasTalking) StartTalking();
                }
                return toReturn;
            }
        }
        private int Rs232BytesAvailable()
        {
            lock (_rs232lock)
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    return _serialPort.BytesToRead;
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// Reads the speficied number of bytes synchronously, from the 
        /// RS232 COM port interface, into the specified buffer, 
        /// filling the buffer starting at the specified index.
        /// </summary>
        /// <param name="buffer">a byte array that will store the 
        /// results of the read operation.</param>
        /// <param name="index">the index in 
        /// the <paramref name="buffer"/> where the bytes which are 
        /// being read from the serial port, will be written.</param>
        /// <param name="count">the number of bytes to read
        /// from the serial port.</param>
        private void Rs232Read(byte[] buffer, int index, int count)
        {
            bool oldDontRead = _dontRead;
            EnsurePortIsReady();
            lock (_rs232lock)
            {
                try
                {
                    _dontRead = true;
                    DateTime startTime = DateTime.Now;
                    int timeOut = 0;
                    lock (_rs232lock)
                    {
                        timeOut = _serialPort.ReadTimeout;
                    }

                    Debug.WriteLine("Waiting for " + count + " bytes to appear at the serial port.");
                    int bytesAvailable = Rs232BytesAvailable();
                    while (bytesAvailable < count)
                    {
                        Debug.WriteLine("There are currently " + bytesAvailable + " bytes already at the serial port.");
                        bytesAvailable = Rs232BytesAvailable();
                        if (DateTime.Now > startTime.AddMilliseconds(timeOut) && timeOut != Timeout.Infinite)
                        {
                            Debug.WriteLine("A timeout occurred waiting for data on the serial port.");
                            throw new TimeoutException();
                        }
                        System.Windows.Forms.Application.DoEvents();
                    }
                    Debug.WriteLine("There are currently " + bytesAvailable + " bytes already at the serial port.");
                    _serialPort.Read(buffer, index, count);
                    Debug.WriteLine("Read these bytes:");
                    for (int i = index; i < (index + count); i++)
                    {
                        System.Diagnostics.Debug.Write(buffer[i].ToString() + " ");
                    }
                    Debug.WriteLine("");
                    Debug.WriteLine("Finished reading " + count + " bytes from the serial port.");
                    if (Rs232BytesAvailable() > 0)
                    {
                        Debug.WriteLine("There are still " + Rs232BytesAvailable() + " bytes waiting at the serial port.");
                    }
                }
                finally
                {
                    _dontRead = oldDontRead;
                }
            }
        }
        /// <summary>
        /// Converts a pair of bytes to a 16-bit, unsigned integer
        /// </summary>
        /// <param name="value">a byte array containing bytes to combine.</param>
        /// <param name="startIndex">the index within the byte array 
        /// indicating the first byte of the pair to combine.</param>
        /// <returns></returns>
        private static ushort ConvertBytesToUShort(byte[] value, int startIndex)
        {
            ushort toReturn = 0;
            if (BitConverter.IsLittleEndian)
            {
                byte[] toSwap = new byte[2];
                toSwap[0] = value[startIndex];
                toSwap[1] = value[startIndex + 1];
                Array.Reverse(toSwap);
                toReturn = BitConverter.ToUInt16(toSwap, 0);
            }
            else
            {
                toReturn = BitConverter.ToUInt16(value, startIndex);
            }
            return toReturn;
        }
        #region Destructors
        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object 
        /// is not manually disposed.  Ordinarily called only 
        /// by the garbage collector.
        /// </summary>
        ~Device()
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
                    ClosePort(); //disconnect from the PHCC
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
