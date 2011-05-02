using System.Reflection;
using System.Windows.Forms;
using Common.MacroProgramming;
using log4net;
using SimLinkup.Scripting;

internal class Loop
{
    public static ILog _logger = LogManager.GetLogger(typeof (Loop));

    public static void SomeMethod(ScriptingContext context)
    {
        _logger.Debug(MethodBase.GetCurrentMethod().Name + " called");

        var adiPitchValueFromSim = context.AllSignals["F4_STBY_ADI__PITCH_DEGREES"];
        var adiPitchSignalInputIntoSimtek = context.AllSignals["10033501_Pitch_From_Sim"];
        var adiPitchSinSignalOutputFromSimtek = context.AllSignals["10033501_Pitch_SIN_To_Instrument"];
        var adiPitchCosSignalOutputFromSimtek = context.AllSignals["10033501_Pitch_COS_To_Instrument"];
        var analogDevicesInput0 = context.AllSignals["AnalogDevices_AD536x/537x__DAC_OUTPUT[0][0]"];
        var analogDevicesInput1 = context.AllSignals["AnalogDevices_AD536x/537x__DAC_OUTPUT[0][1]"];
        var analogDevicesInput2 = context.AllSignals["AnalogDevices_AD536x/537x__DAC_OUTPUT[0][2]"];
        var analogDevicesInput3 = context.AllSignals["AnalogDevices_AD536x/537x__DAC_OUTPUT[0][3]"];
        MessageBox.Show("ADI pitch val from sim:" + ((AnalogSignal) adiPitchValueFromSim).State);
        MessageBox.Show("ADI pitch val into Simtek:" + ((AnalogSignal) adiPitchSignalInputIntoSimtek).State);
        MessageBox.Show("ADI pitch SIN val from Simtek:" + ((AnalogSignal) adiPitchSinSignalOutputFromSimtek).State);
        MessageBox.Show("ADI pitch COS val from Simtek:" + ((AnalogSignal) adiPitchCosSignalOutputFromSimtek).State);
        MessageBox.Show("val going into AD chan 0:" + ((AnalogSignal) analogDevicesInput0).State);
        MessageBox.Show("val going into AD chan 1:" + ((AnalogSignal) analogDevicesInput1).State);
        MessageBox.Show("val going into AD chan 2:" + ((AnalogSignal) analogDevicesInput2).State);
        MessageBox.Show("val going into AD chan 3:" + ((AnalogSignal) analogDevicesInput3).State);
    }
}