using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MFDExtractor.BMSSupport
{
	internal interface IThreeDeeCockpitFileFinder
	{
		FileInfo FindThreeDeeCockpitFile();
	}

	class ThreeDeeCockpitFileFinder : IThreeDeeCockpitFileFinder
	{
		private readonly IBMSRunningExecutableLocator _bmsRunningExecutableLocator;
		public ThreeDeeCockpitFileFinder(IBMSRunningExecutableLocator bmsRunningExecutableLocator=null)
		{
			_bmsRunningExecutableLocator = bmsRunningExecutableLocator ?? new BMSRunningExecutableLocator();
		}
		public FileInfo FindThreeDeeCockpitFile()
		{
			var basePath = _bmsRunningExecutableLocator.BMSExePath;
			return basePath == null ? null : Paths(basePath).FirstOrDefault();
        }
		private static IEnumerable<FileInfo> Paths(string basePath)
		{
			yield return SearchIn(Path.Combine(basePath, @"\art\ckptartn"), "3dckpit.dat");
			yield return SearchIn(Path.Combine(basePath, @"\art\ckptart"), "3dckpit.dat");
		}
		private static FileInfo SearchIn(string searchPath, string fileName)
		{
			var dir = new DirectoryInfo(searchPath);
			if (!dir.Exists) return null;
			var subdirectories = dir.GetDirectories();

			return subdirectories.Concat(new[] {dir})
				.Select(x => new FileInfo(Path.Combine(x.FullName, fileName)))
				.Where(FileExistsAndIsInUse)
				.FirstOrDefault();
		}
		private static bool FileExistsAndIsInUse(FileInfo file)
		{
			if (!file.Exists) return false;
			try
			{
				using (var filestream = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					filestream.Close();
				}
			}
			catch (IOException) { return true; }
			return false;
		}
	}
}
