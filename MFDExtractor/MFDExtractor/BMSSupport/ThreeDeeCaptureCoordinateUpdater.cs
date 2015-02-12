using System;
using System.Drawing;
using System.IO;
using log4net;

namespace MFDExtractor.BMSSupport
{
	internal interface IThreeDeeCaptureCoordinateUpdater
	{
		void Update3DCoordinatesFromCurrentBmsDatFile(int vehicleACD);
	}

	class ThreeDeeCaptureCoordinateUpdater : IThreeDeeCaptureCoordinateUpdater
	{
		private readonly ILog _log;
	    private readonly TexturesSharedMemoryImageCoordinates _coordinates;
		private readonly IThreeDeeCockpitFileFinder2 _threeDeeCockpitFileFinder;
		private readonly IDoubleResolutionRTTChecker _doubleResolutionRTTChecker;

		public ThreeDeeCaptureCoordinateUpdater(
            TexturesSharedMemoryImageCoordinates coordinates,
            IThreeDeeCockpitFileFinder2 threeDeeCockpitFileFinder=null,
			IDoubleResolutionRTTChecker doubleResolutionRTTChecker = null,
			ILog log=null)
		{
		    _coordinates = coordinates;
			_threeDeeCockpitFileFinder = threeDeeCockpitFileFinder ?? new ThreeDeeCockpitFileFinder();
			_doubleResolutionRTTChecker = doubleResolutionRTTChecker ?? new DoubleResolutionRTTChecker();

			_log = log ?? LogManager.GetLogger(GetType());
		}
        public void Update3DCoordinatesFromCurrentBmsDatFile(int vehicleACD)
		{

			var threeDeeCockpitFile = _threeDeeCockpitFileFinder.FindThreeDeeCockpitFile(vehicleACD);
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
					if (currentLine.ToLowerInvariant().StartsWith("hud\t"))
					{
						_coordinates.HUD = ReadCaptureCoordinates(currentLine);
					}
                    else if (currentLine.ToLowerInvariant().StartsWith("mfd4\t"))
					{
						_coordinates.MFD4 = ReadCaptureCoordinates(currentLine);
					}
                    else if (currentLine.ToLowerInvariant().StartsWith("mfd3\t"))
					{
						_coordinates.MFD3= ReadCaptureCoordinates(currentLine);
					}
                    else if (currentLine.ToLowerInvariant().StartsWith("mfdleft\t"))
					{
						_coordinates.LMFD= ReadCaptureCoordinates(currentLine);
					}
                    else if (currentLine.ToLowerInvariant().StartsWith("mfdright\t"))
					{
						_coordinates.RMFD= ReadCaptureCoordinates(currentLine);
					}
				}
			}
			if (isDoubleResolution)
			{
                _coordinates.LMFD = Common.UI.Layout.Util.MultiplyRectangle(_coordinates.LMFD, 2);
                _coordinates.RMFD = Common.UI.Layout.Util.MultiplyRectangle(_coordinates.RMFD, 2);
                _coordinates.MFD3 = Common.UI.Layout.Util.MultiplyRectangle(_coordinates.MFD3, 2);
                _coordinates.MFD4 = Common.UI.Layout.Util.MultiplyRectangle(_coordinates.MFD4, 2);
                _coordinates.HUD = Common.UI.Layout.Util.MultiplyRectangle(_coordinates.HUD, 2);
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
			    return rectangle;
			}
			catch (Exception e)
			{
				_log.Error(e.Message, e);
			}
			return Rectangle.Empty;
		}
	}
}
