using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;

namespace SDI
{
    /// <summary>
    ///   The <see cref = "Device" /> class provides methods for
    ///   communicating with the Synchro-to-Digital Interface (SDI) card.  
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComSourceInterfaces(typeof(IDeviceEvents))]
    [Synchronization]
    public sealed class Device : ContextBoundObject, IDisposable
    {
        private bool _isDisposed;
        private ICommandDispatcher _commandDispatcher;

        /// <summary>
        ///   Creates an instance of the <see cref = "Device" /> class.
        /// </summary>
        public Device(){}

        /// <summary>
        ///   Creates an instance of the <see cref = "Device" /> class.
        /// </summary>
        /// <param name = "portName">The name of the COM port to use for 
        ///   communicating with the device (i.e. "COM1", "COM2",
        ///   etc.)</param>
        public Device(string portName) : this(portName, true){}

        /// <summary>
        ///   Creates an instance of the <see cref = "Device" /> class.
        /// </summary>
        /// <param name = "portName">The name of the COM port to use for 
        ///   communicating with the device (i.e. "COM1", "COM2",
        ///   etc.)</param>
        /// <param name = "openPort">Specifies whether to open the COM port immediately or wait till the first operation that requires doing so.</param>
        public Device(string portName, bool openPort): this()
        {
            _commandDispatcher = new UsbCommandDispatcher(portName, openPort);
        }

        /// <summary>
        ///   Creates an instance of the <see cref = "Device" /> class.
        /// </summary>
        /// <param name = "portName">The name of the COM port to use for 
        ///   communicating with the device (i.e. "COM1", "COM2",
        ///   etc.)</param>
        /// <param name = "openPort">Specifies whether to open the COM port immediately or wait till the first operation that requires doing so.</param>
        /// <param name = "doaAddress">Specifies the address of the SDI device on the PHCC DOA bus.</param>

        public Device(string portName, bool openPort, byte doaAddress): this()
        {
            _commandDispatcher = new PhccCommandDispatcher(portName, doaAddress, openPort);
        }

        /// <summary>
        ///   The <see cref = "DeviceDataReceived" /> event is raised when
        ///   the device transmits data back to the host (PC).
        /// </summary>
        public event EventHandler<DeviceDataReceivedEventArgs> DeviceDataReceived;

        #region Protocol

        
        public void MoveIndicatorFine(Quadrant quadrant, byte position)
        {
            switch (quadrant)
            {
                case Quadrant.One:
                    SendCommand(CommandSubAddress.SSYNQ1, position);
                    break;
                case Quadrant.Two:
                    SendCommand(CommandSubAddress.SSYNQ2, position);
                    break;
                case Quadrant.Three:
                    SendCommand(CommandSubAddress.SSYNQ3, position);
                    break;
                case Quadrant.Four:
                    SendCommand(CommandSubAddress.SSYNQ4, position);
                    break;
                default:
                    throw new ArgumentException("Uknown quadrant.", "quadrant");
            }          
        }
        public void MoveIndicatorCoarse(byte position)
        {
            SendCommand(CommandSubAddress.SYN8BIT, position);
        }


        public void ConfigurePowerDown(PowerDownState enabled, PowerDownLevel level, short delayTimeMilliseconds)
        {
            const uint MAX_POWER_DOWN_DELAY = 2047;
            if (delayTimeMilliseconds <0 || delayTimeMilliseconds > MAX_POWER_DOWN_DELAY)
            {
                throw new ArgumentOutOfRangeException("delayTimeMilliseconds", delayTimeMilliseconds, string.Format(CultureInfo.InvariantCulture, "Value must be >=0 and <= {0}", MAX_POWER_DOWN_DELAY));
            };
            var data =(byte)
                (
                    (byte)enabled |
                    (byte)level |
                    (delayTimeMilliseconds / 32)
                );

            SendCommand(CommandSubAddress.POWER_DOWN,data);
        }

        public void SetStatorBaseAngle(StatorSignals statorSignal, short offset)
        {
            const ushort MAX_OFFSET = 1023; //10 bits of precision allowed
            const ushort LSB_BITMASK = 0xFF; //bits 0-7
            const ushort MSB_BITMASK = 0x300; //bits 8-9

            if (offset <0 || offset > MAX_OFFSET)
            {
                throw new ArgumentOutOfRangeException("offset", string.Format(CultureInfo.InvariantCulture, "Must be >=0 and <= {0}", MAX_OFFSET));
            }
            byte lsb = (byte)(offset & LSB_BITMASK);
            byte msb = (byte)(offset & MSB_BITMASK);
            switch (statorSignal)
            {
                case StatorSignals.S1:
                    SendCommand(CommandSubAddress.S1_BASE_ANGLE_LSB, lsb);
                    SendCommand(CommandSubAddress.S1_BASE_ANGLE_MSB, msb);
                    break;
                case StatorSignals.S2:
                    SendCommand(CommandSubAddress.S2_BASE_ANGLE_LSB, lsb);
                    SendCommand(CommandSubAddress.S2_BASE_ANGLE_MSB, msb);
                    break;
                case StatorSignals.S3:
                    SendCommand(CommandSubAddress.S3_BASE_ANGLE_LSB, lsb);
                    SendCommand(CommandSubAddress.S3_BASE_ANGLE_MSB, msb);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("statorSignal");
            }
        }
        public void SetStatorSignalAmplitudeImmediate(StatorSignals statorSignal, byte amplitude)
        {
            switch (statorSignal)
            {
                case StatorSignals.S1:
                    SendCommand(CommandSubAddress.S1PWM, amplitude);
                    break;
                case StatorSignals.S2:
                    SendCommand(CommandSubAddress.S2PWM, amplitude);
                    break;
                case StatorSignals.S3:
                    SendCommand(CommandSubAddress.S3PWM, amplitude);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("statorSignal");
            }
        }
        public void SetStatorSignalsPolarityImmediate(StatorSignals statorSignalPolarities)
        {           
            SendCommand(CommandSubAddress.SXPOL, (byte)statorSignalPolarities);
        }
        public void SetStatorSignalAmplitudeDeferred(StatorSignals statorSignal, byte amplitude)
        {
            switch (statorSignal)
            {
                case StatorSignals.S1:
                    SendCommand(CommandSubAddress.S1PWMD, amplitude);
                    break;
                case StatorSignals.S2:
                    SendCommand(CommandSubAddress.S2PWMD, amplitude);
                    break;
                case StatorSignals.S3:
                    SendCommand(CommandSubAddress.S3PWMD, amplitude);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("statorSignal");
            }
        }
        public void SetStatorSignalsPolarityAndLoadDeferred(StatorSignals statorSignalPolarities)
        {
            SendCommand(CommandSubAddress.SXPOLD, (byte)statorSignalPolarities);
        }

        public void SetIndicatorMovementLimitMinimum(byte limitMinimum)
        {
            SendCommand(CommandSubAddress.LIMIT_MIN, limitMinimum);
        }
        public void SetIndicatorMovementLimitMaximum(byte limitMaximum)
        {
            SendCommand(CommandSubAddress.LIMIT_MAX, limitMaximum);
        }

   
        public void ConfigureOutputChannels(OutputChannels outputChannel)
        {
            if (
                (outputChannel & OutputChannels.Unknown) == OutputChannels.Unknown ||
                (outputChannel & OutputChannels.ONBOARD_OPAMP_BUFFERED_PWM) == OutputChannels.ONBOARD_OPAMP_BUFFERED_PWM
               )
            {
                throw new ArgumentOutOfRangeException("outputChannel");
            }
            SendCommand(CommandSubAddress.DIG_PWM, (byte)outputChannel);
        }

        public void SetOutputChannelValue(OutputChannels outputChannel, byte value)
        {
            switch (outputChannel)
            {
                case OutputChannels.DIG_PWM_1:
                    SendCommand(CommandSubAddress.DIG_PWM_1, value);
                    break;
                case OutputChannels.DIG_PWM_2:
                    SendCommand(CommandSubAddress.DIG_PWM_2, value);
                    break;
                case OutputChannels.DIG_PWM_3:
                    SendCommand(CommandSubAddress.DIG_PWM_3, value);
                    break;
                case OutputChannels.DIG_PWM_4:
                    SendCommand(CommandSubAddress.DIG_PWM_4, value);
                    break;
                case OutputChannels.DIG_PWM_5:
                    SendCommand(CommandSubAddress.DIG_PWM_5, value);
                    break;
                case OutputChannels.DIG_PWM_6:
                    SendCommand(CommandSubAddress.DIG_PWM_6, value);
                    break;
                case OutputChannels.DIG_PWM_7:
                    SendCommand(CommandSubAddress.DIG_PWM_7, value);
                    break;
                case OutputChannels.ONBOARD_OPAMP_BUFFERED_PWM:
                    SendCommand(CommandSubAddress.ONBOARD_PWM, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("outputChannel");
            }
            
        }

        public void SetUpdateRateControlMode_Limit(byte limitThreshold)
        {
            const byte MAX_LIMIT_THRESHOLD = 63; //6 bits
            if (limitThreshold > MAX_LIMIT_THRESHOLD)
            {
                throw new ArgumentOutOfRangeException("limitThreshold", string.Format(CultureInfo.InvariantCulture, "Must be <= {0}", MAX_LIMIT_THRESHOLD));
            }
            var data = (byte)((byte)UpdateRateControlModes.Limit | limitThreshold);
            SendCommand(CommandSubAddress.UPDATE_RATE_CONTROL, data);
        }
        public void SetUpdateRateControlMode_Smooth(byte smoothingMinimumThresholdValue, UpdateRateControlSmoothingMode smoothingMode)
        {
            const byte MAX_SMOOTHING_MINIMUM_THRESHOLD = 15; //4 bits
            if ((smoothingMinimumThresholdValue > MAX_SMOOTHING_MINIMUM_THRESHOLD))
            {
                throw new ArgumentOutOfRangeException("smoothingMinimumThresholdValue", string.Format(CultureInfo.InvariantCulture, "Must be <= {0}", MAX_SMOOTHING_MINIMUM_THRESHOLD));
            }
            var data = (byte)((byte)UpdateRateControlModes.Smooth  | (byte)(smoothingMinimumThresholdValue <<2) | (byte)smoothingMode);
            SendCommand(CommandSubAddress.UPDATE_RATE_CONTROL, data);
        }

        public void SetUpdateRateControlMode_Speed(short stepUpdateDelayMillis)
        {
            const byte MAX_STEP_UPDATE_DELAY = 31; //5 bits
           
            if (stepUpdateDelayMillis <0 || stepUpdateDelayMillis > MAX_STEP_UPDATE_DELAY)
            {
                throw new ArgumentOutOfRangeException("stepUpdateDelayMillis", string.Format(CultureInfo.InvariantCulture, "Must be >=0 and <= {0}", MAX_STEP_UPDATE_DELAY));
            }
            var data = (byte)((byte)UpdateRateControlModes.Smooth | stepUpdateDelayMillis);
            SendCommand(CommandSubAddress.UPDATE_RATE_CONTROL, data);
        }

        public void SetUpdateRateControlMode_Miscellaneous(bool shortPath)
        {
            var data = (byte)((byte)UpdateRateControlModes.Miscellaneous | (byte)(shortPath ? 1: 0));
            SendCommand(CommandSubAddress.UPDATE_RATE_CONTROL, data);
        }
        public void ConfigureDiagnosticLEDBehavior(DiagnosticLEDMode mode)
        {
            SendCommand(CommandSubAddress.DIAG_LED, (byte)mode);
        }
        public void Demo(DemoMovementSpeeds movementSpeed, byte movementStepSize, DemoModus modus, bool start)
        {
            const byte MAX_MOVEMENT_STEP_SIZE = 15; //4 bits
            if (movementStepSize >MAX_MOVEMENT_STEP_SIZE )
            {
                throw new ArgumentOutOfRangeException("movementStepSize", string.Format(CultureInfo.InvariantCulture, "Must be <= {0}", MAX_MOVEMENT_STEP_SIZE));
            }
            var data = (byte)
                       (
                            (((byte)movementSpeed & (byte)DemoBits.MovementSpeed) << 6) |
                           (byte)(movementStepSize << 2) |
                           (byte)modus |
                           (byte)(start ? 1 : 0)
                        );
            SendCommand(CommandSubAddress.DEMO_MODE, data);
        }
        public void SetDemoModeStartPosition(byte data)
        {
            SendCommand(CommandSubAddress.DEMO_MODE_START_POSITION, data);
        }
        public void SetDemoModeEndPosition(byte data)
        {
            SendCommand(CommandSubAddress.DEMO_MODE_END_POSITION, data);
        }

        public string Identify()
        {
            return SendQuery(CommandSubAddress.IDENTIFY, 0x00);
        }

        public void DisableWatchdog()
        {
            SendCommand(CommandSubAddress.DISABLE_WATCHDOG, 0x00);
        }
        public void Watchdog(bool enable, byte countdown)
        {
            const ushort MAX_COUNTDOWN = 63; //6 bits
            if (countdown > MAX_COUNTDOWN)
            {
                throw new ArgumentOutOfRangeException("countdown", string.Format(CultureInfo.InvariantCulture, "Must be <= {0}", MAX_COUNTDOWN));
            }
            var data = (byte)((enable ? 1 : 0) << 7) | countdown;
            SendCommand(CommandSubAddress.WATCHDOG_CONTROL, (byte)data);
        }

        public void UsbDebug(bool enable)
        {
            SendCommand(CommandSubAddress.USB_DEBUG, enable ? Convert.ToByte('Y') : Convert.ToByte('N'));
        }
       

        private void SendCommand(CommandSubAddress subAddress, byte data)
        {
            _commandDispatcher.SendCommand(subAddress, data);
        }
        private string SendQuery (CommandSubAddress subAddress, byte data)
        {
            return _commandDispatcher.SendQuery(subAddress, data);
        }
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
        ~Device()
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
            if (!_isDisposed && disposing)
            {
                if (_commandDispatcher !=null)
                {
                    _commandDispatcher.Dispose(); 
                }
            }
            _isDisposed = true;
        }

        #endregion
    }
}