using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.Mfd.Controls
{
    internal interface IParamSelectKnobFactory
    {
        RotaryEncoderMfdInputControl BuildParamSelectKnob();
    }
    class ParamSelectKnobFactory:IParamSelectKnobFactory
    {
        private F16CpdMfdManager _mfdManager;
        public ParamSelectKnobFactory(F16CpdMfdManager mfdManager)
        {
            _mfdManager = mfdManager;
        }

        public RotaryEncoderMfdInputControl BuildParamSelectKnob()
        {
            return new RotaryEncoderMfdInputControl();
        }
    }
}
