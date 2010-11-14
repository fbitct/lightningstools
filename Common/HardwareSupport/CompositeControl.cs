using System;
using System.Collections.Generic;
using System.Text;
using Common.MacroProgramming;

namespace Common.HardwareSupport
{
    [Serializable]
    public abstract class CompositeControl:Chainable
    {
        public CompositeControl():base()
        {
        }
    }
}
