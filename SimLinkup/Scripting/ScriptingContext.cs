using System.Collections.Generic;
using Common.HardwareSupport;
using Common.MacroProgramming;
using Common.SimSupport;

namespace SimLinkup.Scripting
{
    public class ScriptingContext : Dictionary<object, object>
    {
        private IHardwareSupportModule[] _hsms;
        private SimSupportModule[] _ssms;
        public SimSupportModule[] SimSupportModules
        {
            get { return _ssms; }
            set
            {
                _ssms = value;
                AddSimAndHardwareInsAndOuts();
            }
        }

        public IHardwareSupportModule[] HardwareSupportModules
        {
            get { return _hsms; }
            set
            {
                _hsms = value;
                AddSimAndHardwareInsAndOuts();
            }
        }

        public DigitalSignal[] DigitalSignals
        {
            get
            {
                var toReturn = new List<DigitalSignal>();
                foreach (var value in Values)
                {
                    if (value is DigitalSignal)
                    {
                        toReturn.Add((DigitalSignal) value);
                    }
                }
                return toReturn.ToArray();
            }
        }

        public SignalList<AnalogSignal> AnalogSignals
        {
            get
            {
                var toReturn = new SignalList<AnalogSignal>();
                foreach (var value in Values)
                {
                    if (value is AnalogSignal)
                    {
                        toReturn.Add((AnalogSignal) value);
                    }
                }
                return toReturn;
            }
        }

        public SignalList<TextSignal> TextSignals
        {
            get
            {
                var toReturn = new SignalList<TextSignal>();
                foreach (var value in Values)
                {
                    if (value is TextSignal)
                    {
                        toReturn.Add((TextSignal) value);
                    }
                }
                return toReturn;
            }
        }

        public SignalList<Signal> AllSignals
        {
            get
            {
                var toReturn = new SignalList<Signal>();
                foreach (var value in Values)
                {
                    if (value is Signal)
                    {
                        toReturn.Add((Signal) value);
                    }
                }
                return toReturn;
            }
        }

        private void AddSimAndHardwareInsAndOuts()
        {
            Clear();
            AddSimInputsAndOutputs();
            AddHardwareInputsAndOutputs();
        }

        private void AddHardwareInputsAndOutputs()
        {
            if (HardwareSupportModules == null || HardwareSupportModules.Length == 0) return;

            foreach (var hsm in HardwareSupportModules)
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
            if (SimSupportModules == null || SimSupportModules.Length == 0) return;
            foreach (var ssm in SimSupportModules)
            {
                if (ssm == null) continue;
                if (ssm.SimOutputs != null)
                {
                    foreach (var output in ssm.SimOutputs.Values)
                    {
                        this[output.Id] = output;
                    }
                }
                if (ssm.SimCommands != null)
                {
                    foreach (var command in ssm.SimCommands.Values)
                    {
                        this[command.Id] = command;
                    }
                }
            }
        }
    }
}