using System;
using System.Threading;

namespace MFDExtractor
{
    public interface IRenderThreadFactory
    {
        void CreateOrRecycle(ref Thread renderThread, ThreadPriority priority, string threadName, Func<bool> preCondition, ThreadStart threadStart);
    }

    public class RenderThreadFactory : IRenderThreadFactory
    {
        private readonly ThreadAbortion _threadAbortion;

        public RenderThreadFactory(ThreadAbortion threadAbortion=null)
        {
            _threadAbortion = threadAbortion ?? new ThreadAbortion();
        }

        public void CreateOrRecycle(ref Thread renderThread, ThreadPriority priority, string threadName, Func<bool> preCondition, ThreadStart threadStart)
        {
            _threadAbortion.AbortThread(ref renderThread);
            if (preCondition())
            {
                renderThread = new Thread(threadStart) {IsBackground = true, Name = threadName};
            }
        }
    }
}