using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Common.Threading
{
    public static class Util
    {
        public static void AbortThread(ref Thread t)
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
        public static void WaitAllHandlesInListAndClearList(List<WaitHandle> toWait, int millisecondsTimeout)
        {
            if (toWait != null && toWait.Count > 0)
            {
                try
                {
                    WaitHandle[] handles = toWait.ToArray();
                    if (handles != null && handles.Length > 0)
                    {
                        WaitHandle.WaitAll(handles, millisecondsTimeout);
                    }
                }
                catch (TimeoutException)
                {
                }
                catch (DuplicateWaitObjectException) //this can happen somehow if our list is not cleared 
                {
                }
            }
            toWait.Clear();
        }
    }
}
