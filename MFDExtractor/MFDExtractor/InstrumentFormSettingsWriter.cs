using System;

namespace MFDExtractor
{
	internal interface IInstrumentFormSettingsWriter
	{
		void Write(string instrumentName, InstrumentFormSettings instrumentFormSettings);
	}

	internal class InstrumentFormSettingsWriter : IInstrumentFormSettingsWriter
	{
		private readonly ISettingWriter _settingWriter;

		public InstrumentFormSettingsWriter(ISettingWriter settingWriter = null)
		{
			_settingWriter = settingWriter ?? new SettingWriter();
		}

		public void Write(string instrumentName, InstrumentFormSettings instrumentFormSettings)
		{
			_settingWriter.WriteSetting(value:instrumentFormSettings.Enabled, settingName: String.Format("Enable{0}Output", instrumentName));
			_settingWriter.WriteSetting(value:instrumentFormSettings.OutputDisplay, settingName: String.Format("{0}_OutputDisplay", instrumentName));
			_settingWriter.WriteSetting(value:instrumentFormSettings.StretchToFit, settingName: String.Format("{0}_StretchToFit", instrumentName));
			_settingWriter.WriteSetting(value:instrumentFormSettings.ULY, settingName: String.Format("{0}_ULY", instrumentName));
			_settingWriter.WriteSetting(value:instrumentFormSettings.ULX, settingName: String.Format("{0}_ULX", instrumentName));
			_settingWriter.WriteSetting(value:instrumentFormSettings.LRX, settingName: String.Format("{0}_LRX", instrumentName));
			_settingWriter.WriteSetting(value:instrumentFormSettings.LRY, settingName: String.Format("{0}_LRY", instrumentName));
			_settingWriter.WriteSetting(value:instrumentFormSettings.AlwaysOnTop, settingName: String.Format("{0}_AlwaysOnTop", instrumentName));
			_settingWriter.WriteSetting(value:instrumentFormSettings.Monochrome, settingName: String.Format("{0}_Monochrome", instrumentName));
			_settingWriter.WriteSetting(value:instrumentFormSettings.RotateFlipType.ToString(), settingName: String.Format("{0}_RotateFlipType", instrumentName));
		}
	}
}
