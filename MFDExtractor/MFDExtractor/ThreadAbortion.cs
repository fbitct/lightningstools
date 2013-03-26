using System;
using System.Threading;

namespace MFDExtractor
{
    public sealed class ThreadAbortion
    {
    
        public void AbortThread(ref Thread t)
        {
            if (t == null) return;
            try
            {
                t.Abort();
            }
            catch (Exception e)
            {
            }
            Common.Util.DisposeObject(t);
            t = null;
        }
    }
}