using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MFDExtractor.BMSSupport
{
	internal interface IBMSConfigFileReader
	{
		IEnumerable<string> ConfigLines { get; }
	}

	class BMSConfigFileReader : IBMSConfigFileReader
	{
		private readonly IBMSRunningExecutableLocator _bmsRunningExecutableLocator;
		public BMSConfigFileReader(IBMSRunningExecutableLocator bmsRunningExecutableLocator = null)
		{
			_bmsRunningExecutableLocator = bmsRunningExecutableLocator ?? new BMSRunningExecutableLocator();
		}

		public IEnumerable<string> ConfigLines
		{
			get
			{
				var bmsPath = _bmsRunningExecutableLocator.BMSExePath;
				if (string.IsNullOrEmpty(bmsPath)) return Enumerable.Empty<string>();

				var configFilePath = ConfigFilePath(bmsPath);
				if (string.IsNullOrEmpty(configFilePath)) return Enumerable.Empty<string>();
				var allLines = new List<string>();
				using (var reader = new StreamReader(configFilePath))
				{
					while (!reader.EndOfStream)
					{
						allLines.Add(reader.ReadLine());
					}
					reader.Close();
				}
				return allLines;
			}
		}

		private static string ConfigFilePath(string exePath)
		{
			return new[]
			{
				Path.Combine(exePath, "FalconBMS.cfg"),
				Path.Combine(Path.Combine(exePath, "config"), "Falcon BMS.cfg"),
				Path.Combine(Path.Combine(exePath, @"..\..\User\config"), "Falcon BMS.cfg")
			}.FirstOrDefault(x => new FileInfo(x).Exists);
		}
	}
}
