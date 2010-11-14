﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Common.SimSupport
{
    public abstract class SimSupportModule
    {
        public bool TestMode
        {
            get;
            set;
        }
        public abstract bool IsSimRunning
        {
            get;
        }
        public abstract Dictionary<string, ISimOutput> SimOutputs
        {
            get;
        }
        public abstract void Update();
        public abstract Dictionary<string, SimCommand> SimCommands 
        {
            get;
        }
        public abstract string FriendlyName { get; }
    }
}
