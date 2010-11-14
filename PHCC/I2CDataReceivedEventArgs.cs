using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace Phcc
{
    /// <summary>
    /// <see cref="I2CDataReceivedEventArgs"/> objects hold I2C data 
    /// that is received when the PHCC motherboard signals that new
    /// I2C data has arrived.  This data is provided by the 
    /// <see cref="Device.I2CDataReceived"/> event.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public sealed class I2CDataReceivedEventArgs : EventArgs
    {
        private short _address;
        private byte _data;

        /// <summary>
        /// Creates an instance of 
        /// the <see cref="I2CDataReceivedEventArgs"/> class.
        /// </summary>
        public I2CDataReceivedEventArgs()
            : base()
        {
        }
        /// <summary>
        /// Creates an instance of
        /// the <see cref="I2CDataReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="address">The address of the I2C device that is 
        /// providing the data during this event.</param>
        /// <param name="data">The data being provided by the I2C device
        /// during this event.</param>
        public I2CDataReceivedEventArgs(short address, byte data)
        {
            _address = address;
            _data = data;
        }
        /// <summary>
        /// Gets/sets the address of the I2C device that is providing 
        /// the data during this event.
        /// </summary>
        public short Address
        {
            get
            {
                return _address;
            }
            set
            {
                _address = value;
            }
        }
        /// <summary>
        /// Gets/sets the data being provided by the I2C device 
        /// during this event.
        /// </summary>
        public byte Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }
    }
}
