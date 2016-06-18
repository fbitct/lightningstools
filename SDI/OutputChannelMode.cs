﻿using System;
using System.Runtime.InteropServices;

namespace SDI
{
    /// <summary>
    ///   User defined output channel modes
    /// </summary>
    [ComVisible(true)]
    public enum OutputChannelMode : byte
    {
        /// <summary>
        ///  Digital
        /// </summary>
        Digital = 0,
        /// <summary>
        ///  PWM
        /// </summary>
        PWM = 1,
    }
}