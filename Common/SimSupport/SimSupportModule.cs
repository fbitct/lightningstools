using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.SimSupport
{
    public abstract class SimSupportModule:IDisposable
    {
        public bool TestMode { get; set; }
        public abstract bool IsSimRunning { get; }
        public abstract Dictionary<string, ISimOutput> SimOutputs { get; }
        public abstract Dictionary<string, SimCommand> SimCommands { get; }
        public abstract string FriendlyName { get; }
        public abstract Task UpdateAsync();

        public abstract void Dispose();
    }
}