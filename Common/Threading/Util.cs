using System;
using System.Collections.Generic;
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
            catch
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
                    var handles = toWait.ToArray();
                    if (handles.Length > 0)
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
            if (toWait != null) toWait.Clear();
        }
    }
}