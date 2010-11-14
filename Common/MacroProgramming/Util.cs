using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
namespace Common.MacroProgramming
{
    internal static class Util
    {
      
        public static ThreadState SimpleThreadState(ThreadState ts)
        {
            return ts & (ThreadState.Aborted | ThreadState.AbortRequested |
                         ThreadState.Stopped | ThreadState.Unstarted |
                         ThreadState.WaitSleepJoin);
        }
        public static void AbortThread(Thread t)
        {
            if (t == null)
                return;
            try
            {
                t.Abort();
            }
            catch (ThreadStateException)
            {
                t.Resume(); //you can ignore the compiler warning about this line, this
                            //is the one case where you want to use Thread.Resume()
                // Now t will abort.
            }
        }
    }
}
