﻿using System.Windows.Forms;

namespace MFDExtractor
{
	public class ExtractorState
	{
		public Form ApplicationForm { get; set; }
		public bool Running { get; set; }
		public bool TestMode { get; set; }
		public bool NightMode { get; set; }
		public bool TwoDeePrimaryView { get; set; }
		public bool ThreeDeeMode { get; set; }
	}
}
