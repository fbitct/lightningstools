using System.IO;

namespace MFDExtractor.BMSSupport
{
	internal interface IBMSRunningExecutableLocator
	{
		string BMSExePath { get; }
	}

	class BMSRunningExecutableLocator : IBMSRunningExecutableLocator
	{
		public string BMSExePath 
		{
			get
			{
				var exePath = F4Utils.Process.Util.GetFalconExePath();
				if (string.IsNullOrEmpty(exePath)) return null;
				var directoryInfo = new FileInfo(exePath).Directory;
				return directoryInfo != null ? directoryInfo.FullName : null;
			}
		}
	}
}
