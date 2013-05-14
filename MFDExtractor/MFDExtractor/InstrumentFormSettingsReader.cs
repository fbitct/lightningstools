using System;
using System.Drawing;

namespace MFDExtractor
{
	internal interface IInstrumentFormSettingsReader
	{
		InstrumentFormSettings Read(string instrumentName);
	}

	internal class InstrumentFormSettingsReader : IInstrumentFormSettingsReader
	{
		private readonly ISettingReader _settingReader;

		public InstrumentFormSettingsReader(ISettingReader settingReader = null)
		{
			_settingReader = settingReader ?? new SettingReader();
		}

		public InstrumentFormSettings Read(string instrumentName)
		{
			var toReturn = new InstrumentFormSettings
			{
				Enabled = (bool) _settingReader.ReadSetting(defaultValue: false, settingName: String.Format("Enable{0}Output", instrumentName)),
				OutputDisplay = (string)_settingReader.ReadSetting(defaultValue: string.Empty, settingName: String.Format("{0}_OutputDisplay", instrumentName)),
				StretchToFit = (bool) _settingReader.ReadSetting(defaultValue: false, settingName: String.Format("{0}_StretchToFit", instrumentName)),
				ULX = (int)_settingReader.ReadSetting(defaultValue: 0, settingName: String.Format("{0}_OutULX", instrumentName)),
                ULY = (int)_settingReader.ReadSetting(defaultValue: 0, settingName: String.Format("{0}_OutULY", instrumentName)),
                LRX = (int)_settingReader.ReadSetting(defaultValue: 0, settingName: String.Format("{0}_OutLRX", instrumentName)),
                LRY = (int)_settingReader.ReadSetting(defaultValue: 0, settingName: String.Format("{0}_OutLRY", instrumentName)),
				AlwaysOnTop = (bool)_settingReader.ReadSetting(defaultValue: false, settingName: String.Format("{0}_AlwaysOnTop", instrumentName)),
				Monochrome = (bool)_settingReader.ReadSetting(defaultValue: false, settingName: String.Format("{0}_Monochrome", instrumentName)),
				RotateFlipType = (RotateFlipType) _settingReader.ReadSetting(defaultValue: RotateFlipType.RotateNoneFlipNone, settingName: String.Format("{0}_RotateFlipType", instrumentName)),
			};
			return toReturn;
		}
	}
}
