using System.Windows.Forms;
using Common.MacroProgramming;
using SimLinkup.Scripting;
using SimLinkup.Signals;

internal class Setup
{
    public static void SomeMethod(ScriptingContext context)
    {
        var profileToCreate = new MappingProfile();
        Signal vviValueFromSim = context.AllSignals["F4_VVI__VERTICAL_VELOCITY_FPS"];
        Signal vviSignalInputIntoSimtek = context.AllSignals["10058102_VVI_From_Sim"];
        var mapping1 = new SignalMapping();
        mapping1.Source = vviValueFromSim;
        mapping1.Destination = vviSignalInputIntoSimtek;
        profileToCreate.SignalMappings.Add(mapping1);
        Signal vviPowerOffFlagFromSim = context.AllSignals["F4_VVI__OFF_FLAG"];
        Signal vviPowerOffFlagInputIntoSimtek = context.AllSignals["10058102_VVI_Power_Off_Flag_From_Sim"];
        var mapping2 = new SignalMapping();
        mapping2.Source = vviPowerOffFlagFromSim;
        mapping2.Destination = vviPowerOffFlagInputIntoSimtek;
        profileToCreate.SignalMappings.Add(mapping2);


        Signal vviOutputFromSimtekModule = context.AllSignals["10058102_VVI_To_Instrument"];
        Signal analogDevicesChannelZero = FindSignal(context);
        var mapping3 = new SignalMapping();
        mapping3.Source = vviOutputFromSimtekModule;
        mapping3.Destination = analogDevicesChannelZero;
        profileToCreate.SignalMappings.Add(mapping3);


        profileToCreate.Save(@"c:\test.mapping");
        MessageBox.Show("new mapping file saved.");
    }

    private static Signal FindSignal(ScriptingContext context)
    {
        foreach (Signal signal in context.AllSignals)
        {
            if (signal.SourceFriendlyName.Contains("Analog Devices") && signal.FriendlyName.Contains("DAC"))
            {
                return signal;
            }
        }
        return null;
    }
}