using System;
using System.Drawing;
using System.IO;
using log4net;

namespace MFDExtractor.BMSSupport
{
	internal interface IThreeDeeCaptureCoordinateReader
	{
		void Read3DCoordinatesFromCurrentBmsDatFile(
			out Rectangle mfd4CaptureCoordinates,
			out Rectangle mfd3CaptureCoordinates,
			out Rectangle leftMfdCaptureCoordinates,
			out Rectangle rightMfdCaptureCoordinates,
			out Rectangle hudCaptureCoordinates);
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
		public void Read3DCoordinatesFromCurrentBmsDatFile(out Rectangle mfd4CaptureCoordinates,
																   out Rectangle mfd3CaptureCoordinates,
																   out Rectangle leftMfdCaptureCoordinates,
																   out Rectangle rightMfdCaptureCoordinates,
																   out Rectangle hudCaptureCoordinates)
		{

			mfd4CaptureCoordinates = Rectangle.Empty;
			mfd3CaptureCoordinates = Rectangle.Empty;
			leftMfdCaptureCoordinates = Rectangle.Empty;
			rightMfdCaptureCoordinates = Rectangle.Empty;
			hudCaptureCoordinates = Rectangle.Empty;

			var threeDeeCockpitFile = _threeDeeCockpitFileFinder.FindThreeDeeCockpitFile();
			if (threeDeeCockpitFile == null)
			{
				return;
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
						hudCaptureCoordinates = ReadCaptureCoordinates(currentLine);
					}
					else if (currentLine.ToLowerInvariant().StartsWith("mfd4"))
					{
						mfd4CaptureCoordinates = ReadCaptureCoordinates(currentLine);
					}
					else if (currentLine.ToLowerInvariant().StartsWith("mfd3"))
					{
						mfd3CaptureCoordinates= ReadCaptureCoordinates(currentLine);
					}
					else if (currentLine.ToLowerInvariant().StartsWith("mfdleft"))
					{
						rightMfdCaptureCoordinates= ReadCaptureCoordinates(currentLine);
					}
					else if (currentLine.ToLowerInvariant().StartsWith("mfdright"))
					{
						hudCaptureCoordinates= ReadCaptureCoordinates(currentLine);
					}
				}
			}
			if (isDoubleResolution)
			{
				leftMfdCaptureCoordinates = Common.Math.Util.MultiplyRectangle(leftMfdCaptureCoordinates, 2);
				rightMfdCaptureCoordinates = Common.Math.Util.MultiplyRectangle(rightMfdCaptureCoordinates, 2);
				mfd3CaptureCoordinates = Common.Math.Util.MultiplyRectangle(mfd3CaptureCoordinates, 2);
				mfd4CaptureCoordinates = Common.Math.Util.MultiplyRectangle(mfd4CaptureCoordinates, 2);
				hudCaptureCoordinates = Common.Math.Util.MultiplyRectangle(hudCaptureCoordinates, 2);
			}
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
