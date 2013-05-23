using System;
using System.Drawing;
using System.IO;
using log4net;

namespace MFDExtractor.BMSSupport
{
	internal interface IThreeDeeCaptureCoordinateReader
	{
		SharedMemorySpriteCoordinates Read3DCoordinatesFromCurrentBmsDatFile();
	}

	class ThreeDeeCaptureCoordinateReader : IThreeDeeCaptureCoordinateReader
	{
		private readonly ILog _log;
		private readonly IThreeDeeCockpitFileFinder _threeDeeCockpitFileFinder;
		private readonly IDoubleResolutionRTTChecker _doubleResolutionRTTChecker;

		public ThreeDeeCaptureCoordinateReader(
			IThreeDeeCockpitFileFinder threeDeeCockpitFileFinder=null,
			IDoubleResolutionRTTChecker doubleResolutionRTTChecker = null,
			ILog log=null)
		{
			_threeDeeCockpitFileFinder = threeDeeCockpitFileFinder ?? new ThreeDeeCockpitFileFinder();
			_doubleResolutionRTTChecker = doubleResolutionRTTChecker ?? new DoubleResolutionRTTChecker();

			_log = log ?? LogManager.GetLogger(GetType());
		}
		public SharedMemorySpriteCoordinates Read3DCoordinatesFromCurrentBmsDatFile()
		{
			var coordinates = new SharedMemorySpriteCoordinates();

			var threeDeeCockpitFile = _threeDeeCockpitFileFinder.FindThreeDeeCockpitFile();
			if (threeDeeCockpitFile == null)
			{
				return coordinates;
			}
			var isDoubleResolution = _doubleResolutionRTTChecker.IsDoubleResolutionRtt;

			using (var filestream = threeDeeCockpitFile.OpenRead())
			using (var reader = new StreamReader(filestream))
			{
				while (!reader.EndOfStream)
				{
					var currentLine = reader.ReadLine() ?? string.Empty;
					if (currentLine.ToLowerInvariant().StartsWith("hud"))
					{
						coordinates.HUD = ReadCaptureCoordinates(currentLine);
					}
					else if (currentLine.ToLowerInvariant().StartsWith("mfd4"))
					{
						coordinates.MFD4 = ReadCaptureCoordinates(currentLine);
					}
					else if (currentLine.ToLowerInvariant().StartsWith("mfd3"))
					{
						coordinates.MFD3= ReadCaptureCoordinates(currentLine);
					}
					else if (currentLine.ToLowerInvariant().StartsWith("mfdleft"))
					{
						coordinates.RMFD= ReadCaptureCoordinates(currentLine);
					}
					else if (currentLine.ToLowerInvariant().StartsWith("mfdright"))
					{
						coordinates.LMFD= ReadCaptureCoordinates(currentLine);
					}
				}
			}
			if (isDoubleResolution)
			{
				coordinates.LMFD = Common.Math.Util.MultiplyRectangle(coordinates.LMFD, 2);
				coordinates.RMFD = Common.Math.Util.MultiplyRectangle(coordinates.RMFD, 2);
				coordinates.MFD3 = Common.Math.Util.MultiplyRectangle(coordinates.MFD3, 2);
				coordinates.MFD4 = Common.Math.Util.MultiplyRectangle(coordinates.MFD4, 2);
				coordinates.HUD = Common.Math.Util.MultiplyRectangle(coordinates.HUD, 2);
			}
			return coordinates;
		}

		private Rectangle ReadCaptureCoordinates(string configLine)
		{
			var tokens = Common.Strings.Util.Tokenize(configLine);
			if (tokens.Count <= 12) return Rectangle.Empty;
			try
			{
				var rectangle = new Rectangle {X = Convert.ToInt32(tokens[10]), Y = Convert.ToInt32(tokens[11])};
				rectangle.Width = Math.Abs(Convert.ToInt32(tokens[12]) - rectangle.X);
				rectangle.Height = Math.Abs(Convert.ToInt32(tokens[13]) - rectangle.Y);
			}
			catch (Exception e)
			{
				_log.Error(e.Message, e);
			}
			return Rectangle.Empty;
		}
	}
}
