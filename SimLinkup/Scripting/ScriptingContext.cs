using System;
using System.Collections.Generic;
using Common.HardwareSupport;
using Common.SimSupport;
using Common.MacroProgramming;

namespace SimLinkup.Scripting
{
    public class ScriptingContext:Dictionary<object, object>
    {
        private SimSupportModule[] _ssms = null;
        private IHardwareSupportModule[] _hsms = null;
        public SimSupportModule[] SimSupportModules
        {
            get
            {
                return _ssms;
            }
            set
            {
                _ssms = value;
                AddSimAndHardwareInsAndOuts();
            }
        }
        public IHardwareSupportModule[] HardwareSupportModules
        {
            get
            {
                return _hsms;
            }
            set
            {
                _hsms = value;
                AddSimAndHardwareInsAndOuts();
            }
        }
        
        private void AddSimAndHardwareInsAndOuts()
        {
            this.Clear();
            AddSimInputsAndOutputs();
            AddHardwareInputsAndOutputs();
        }
        public DigitalSignal[] DigitalSignals
        {
            get
            {
                List<DigitalSignal> toReturn = new List<DigitalSignal>();
                foreach (var value in this.Values)
                {
                    if (value is DigitalSignal)
                    {
                        toReturn.Add((DigitalSignal)value);
                    }
                }
                return toReturn.ToArray();
            }
        }

        public SignalList<AnalogSignal> AnalogSignals
        {
            get
            {
                SignalList<AnalogSignal> toReturn = new SignalList<AnalogSignal>();
                foreach (var value in this.Values)
                {
                    if (value is AnalogSignal)
                    {
                        toReturn.Add((AnalogSignal)value);
                    }
                }
                return toReturn;
            }
        }
        public SignalList<TextSignal> TextSignals
        {
            get
            {
                SignalList<TextSignal> toReturn = new SignalList<TextSignal>();
                foreach (var value in this.Values)
                {
                    if (value is TextSignal)
                    {
                        toReturn.Add((TextSignal)value);
                    }
                }
                return toReturn;
            }
        }
        public SignalList<Signal> AllSignals
        {
            get
            {
                SignalList<Signal> toReturn = new SignalList<Signal>();
                foreach (var value in this.Values)
                {
                    if (value is Signal)
                    {
                        toReturn.Add((Signal)value);
                    }
                }
                return toReturn;
            }
        }
        private void AddHardwareInputsAndOutputs()
        {
            if (this.HardwareSupportModules == null || this.HardwareSupportModules.Length ==0) return;

            foreach (var hsm in this.HardwareSupportModules)
            {
                if (hsm == null) continue;
                if (hsm.AnalogInputs != null && hsm.AnalogInputs.Length > 0)
                {
                    foreach (var analogInput in hsm.AnalogInputs)
                    {
                        this[analogInput.Id] = analogInput;
                    }
                }
                if (hsm.AnalogOutputs != null && hsm.AnalogOutputs.Length > 0)
                {
                    foreach (var analogOutput in hsm.AnalogOutputs)
                    {
                        this[analogOutput.Id] = analogOutput;
                    }
                }
                if (hsm.DigitalInputs != null && hsm.DigitalInputs.Length > 0)
                {
                    foreach (var digitalInput in hsm.DigitalInputs)
                    {
                        this[digitalInput.Id] = digitalInput;
                    }
                }
                if (hsm.DigitalOutputs != null && hsm.DigitalOutputs.Length > 0)
                {
                    foreach (var digitalOutput in hsm.DigitalOutputs)
                    {
                        this[digitalOutput.Id] = digitalOutput;
                    }
                }
            }
        }
        private void AddSimInputsAndOutputs()
        {
            if (this.SimSupportModules == null || this.SimSupportModules.Length ==0) return;
            foreach (var ssm in this.SimSupportModules)
            {
                if (ssm == null) continue;
                if (ssm.SimOutputs != null)
                {
                    foreach (ISimOutput output in ssm.SimOutputs.Values)
                    {
                        this[output.Id] = output;
                    }
                }
                if (ssm.SimCommands != null)
                {
                    foreach (SimCommand command in ssm.SimCommands.Values)
                    {
                        this[command.Id] = command;
                    }
                }
            }
        }

    }
}
