using System;
using System.Collections.Generic;
using System.Text;

namespace Common.InputSupport
{
    /// <summary>
    /// Enumeration of states that can be retrieved from a call to a DeviceMonitor's GetPhysicalControlValue method
    /// </summary>
    public enum StateType : int
    {
        Current = 0,
        Previous = 1
    }
}
