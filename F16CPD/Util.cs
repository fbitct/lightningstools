using System;
using System.Collections.Generic;

using System.Text;

namespace F16CPD
{
    internal static class Util
    {
        internal static void SaveCurrentProperties()
        {
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

    }
}
