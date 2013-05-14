using System;
using System.Drawing;

namespace MFDExtractor
{
	[Serializable]
	class InstrumentFormSettings
	{
		public bool Enabled { get; set; }
		public string OutputDisplay { get; set; }
		public bool StretchToFit { get; set; }
		public int ULX { get; set; }
		public int ULY { get; set; }
		public int LRX { get; set; }
		public int LRY { get; set; }
		public bool AlwaysOnTop { get; set; }
		public bool Monochrome { get; set; }
		public RotateFlipType RotateFlipType { get; set; }
	}
}
