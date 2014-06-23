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
        public void CreateOrRecycle(ref Thread renderThread, ThreadPriority priority, string threadName, Func<bool> preCondition, ThreadStart threadStart)
        {
            Common.Threading.Util.AbortThread(ref renderThread);
            if (preCondition())
            {
                renderThread = new Thread(threadStart) {IsBackground = true, Name = threadName};
            }
        }
    }
}