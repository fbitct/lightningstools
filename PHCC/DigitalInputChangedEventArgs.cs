using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace Phcc
{
    /// <summary>
    /// <see cref="DigitalInputChangedEventArgs"/> objects hold data that the PHCC motherboard provides whenever a digital 
    /// input value changes.
    /// The <see cref="Device.DigitalInputChanged"/> event  
    /// provides <see cref="DigitalInputChangedEventArgs"/> event-args 
    /// objects during the raising of each event.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public sealed class DigitalInputChangedEventArgs : EventArgs
    {
        private short _index;
        private bool _newValue;

        /// <summary>
        /// Creates an instance of the <see cref="DigitalInputChangedEventArgs"/> class.
        /// </summary>
        public DigitalInputChangedEventArgs()
            : base()
        {
        }
        /// <summary>
        /// Creates an instance of the <see cref="DigitalInputChangedEventArgs"/> class.
        /// </summary>
        /// <param name="index">The index of the digital input 
        /// whose value has changed.</param>
        /// <param name="newValue">The new value of the digital input 
        /// indicated by the <paramref name="Address"/> parameter.</param>
        public DigitalInputChangedEventArgs(short index, bool newValue)
        {
            _index = index;
            _newValue = newValue;
        }
        /// <summary>
        /// Gets/sets the index of the digital input whose 
        /// value has changed.
        /// </summary>
        public short Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }
        /// <summary>
        /// Gets/sets the new value of the indicated digital input.
        /// </summary>
        public bool NewValue
        {
            get
            {
                return _newValue;
            }
            set
            {
                _newValue = value;
            }
        }

    }
}
