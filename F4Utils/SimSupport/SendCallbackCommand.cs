using Common.SimSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F4Utils.SimSupport
{
    public class SendCallbackCommand:SimCommand
    {
        public string Callback { get; set; }
        public override void Execute()
        {
            F4Utils.Process.KeyFileUtils.SendCallbackToFalcon(Callback);
        }
    }
}
