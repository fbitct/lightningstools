using System;
using System.Threading;

namespace MFDExtractor
{
    public sealed class RenderThreadSetupHelper
    {
        private readonly ThreadAbortion _threadAbortion;

        public RenderThreadSetupHelper(ThreadAbortion threadAbortion=null)
        {
            _threadAbortion = threadAbortion ?? new ThreadAbortion();
        }

        public void SetupThread(ref Thread renderThread, ThreadPriority priority, string threadName, Func<bool> preCondition, ThreadStart threadStart)
        {
            _threadAbortion.AbortThread(ref renderThread);
            if (preCondition())
            {
                renderThread = new Thread(threadStart) {IsBackground = true, Name = threadName};
            }
        }
    }
}