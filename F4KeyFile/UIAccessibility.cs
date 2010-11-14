using System;
using System.Runtime.InteropServices;
namespace F4KeyFile
{
    [ComVisible(true)]
    [Serializable]
    public enum UIAcccessibility
    {
        VisibleWithChangesAllowed = 1,
        VisibleNoChangesAllowed = -1,
        Invisible = -2
    }
}
