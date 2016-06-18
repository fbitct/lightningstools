﻿using System;
using System.Runtime.InteropServices;

namespace SDI
{
    /// <summary>
    ///   URC modes
    /// </summary>
    [ComVisible(true)]
    [Flags]
    public enum UpdateRateControlModes: byte
    {
        /// <summary>
        /// Limit 
        /// </summary>
        Limit = 0x00,
        /// <summary>
        /// Smooth
        /// </summary>
        Smooth = 0x01,
        /// <summary>
        /// Speed
        /// </summary>
        Speed = 0x02,
        /// <summary>
        /// Miscellaneous
        /// </summary>
        Miscellaneous = 0x03,
    }
}
