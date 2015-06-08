using System;
using System.Collections.Generic;

namespace Common.SimSupport
{
    public abstract class SimSupportModule:IDisposable
    {
        public bool TestMode { get; set; }
        public abstract bool IsSimRunning { get; }
        public abstract Dictionary<string, ISimOutput> SimOutputs { get; }
        public abstract Dictionary<string, SimCommand> SimCommands { get; }
        public abstract string FriendlyName { get; }
        public abstract void Update();

        public abstract void Dispose();
    }
}