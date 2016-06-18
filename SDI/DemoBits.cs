using System;
using System.Runtime.InteropServices;

namespace SDI
{
    /// <summary>
    ///   DEMO feature control bit masks
    /// </summary>
    [ComVisible(false)]
    [Flags]
    internal enum DemoBits : byte
    {
        /// <summary>
        ///  Start (bit 0)
        /// </summary>
        Start = 0x00,
        /// <summary>
        ///  Modus (bit 1)
        /// </summary>
        Level = 0x01,
        /// <summary>
        ///  Movement step size (bits 2-5)
        /// </summary>
        MovementStepSize = 0x3C,
        /// <summary>
        ///  Movement speed (bits 6-7)
        /// </summary>
        MovementSpeed = 0xC0,
    }
}
