using SimLinkup.Scripting;
using System.Windows.Forms;
using System;
using System.IO;
using Common.MacroProgramming;
using SimLinkup.Signals;
using log4net;
using System.Reflection;
using System.Text;

class Loop
{
    public static ILog _logger = LogManager.GetLogger(typeof(Loop));
    public static void SomeMethod(ScriptingContext context)
    {
        _logger.Debug(MethodInfo.GetCurrentMethod().Name + " called");
       
        Signal adiPitchValueFromSim = context.AllSignals["F4_STBY_ADI__PITCH_DEGREES"];
        Signal adiPitchSignalInputIntoSimtek = context.AllSignals["10033501_Pitch_From_Sim"];
        Signal adiPitchSinSignalOutputFromSimtek = context.AllSignals["10033501_Pitch_SIN_To_Instrument"];
        Signal adiPitchCosSignalOutputFromSimtek = context.AllSignals["10033501_Pitch_COS_To_Instrument"];
        Signal analogDevicesInput0 = context.AllSignals["AnalogDevices_AD536x/537x__DAC_OUTPUT[0][0]"];
        Signal analogDevicesInput1 = context.AllSignals["AnalogDevices_AD536x/537x__DAC_OUTPUT[0][1]"];
        Signal analogDevicesInput2 = context.AllSignals["AnalogDevices_AD536x/537x__DAC_OUTPUT[0][2]"];
        Signal analogDevicesInput3 = context.AllSignals["AnalogDevices_AD536x/537x__DAC_OUTPUT[0][3]"]; 
        MessageBox.Show("ADI pitch val from sim:" + ((AnalogSignal)adiPitchValueFromSim).State);
        MessageBox.Show("ADI pitch val into Simtek:" + ((AnalogSignal)adiPitchSignalInputIntoSimtek).State);
        MessageBox.Show("ADI pitch SIN val from Simtek:" + ((AnalogSignal)adiPitchSinSignalOutputFromSimtek).State);
        MessageBox.Show("ADI pitch COS val from Simtek:" + ((AnalogSignal)adiPitchCosSignalOutputFromSimtek).State);
        MessageBox.Show("val going into AD chan 0:" + ((AnalogSignal)analogDevicesInput0).State);
        MessageBox.Show("val going into AD chan 1:" + ((AnalogSignal)analogDevicesInput1).State);
        MessageBox.Show("val going into AD chan 2:" + ((AnalogSignal)analogDevicesInput2).State);
        MessageBox.Show("val going into AD chan 3:" + ((AnalogSignal)analogDevicesInput3).State);

    }
}
