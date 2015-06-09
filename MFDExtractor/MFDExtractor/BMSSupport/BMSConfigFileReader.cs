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
				var bmsExePath = _bmsRunningExecutableLocator.BMSExePath;
				if (string.IsNullOrEmpty(bmsExePath)) return Enumerable.Empty<string>();
                var bmsBasePath = new FileInfo(bmsExePath).Directory.Parent.Parent.FullName;
                var configFilePath = bmsBasePath + Path.DirectorySeparatorChar + "\\User\\config\\Falcon BMS.cfg";
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
		
	}
}
