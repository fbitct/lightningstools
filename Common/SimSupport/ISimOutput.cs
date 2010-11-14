using System;
using System.Collections.Generic;
using System.Text;

namespace Common.SimSupport
{
    public interface ISimOutput
    {
        string FriendlyName
        {
            get;
        }
        string Id
        {
            get;
        }
    }
}
