using System.Runtime.InteropServices;

namespace SDI
{
    /// <summary>
    ///   Watchdog status
    /// </summary>
    [ComVisible(true)]
    public enum WatchdogStatus : byte
    {
        /// <summary>
        /// Disabled 
        /// </summary>
        Disabled = 0,
        /// <summary>
        /// Enabled
        /// </summary>
        Enabled = 1,
    }
}
