using System.Windows.Forms;
using Common.Networking;

namespace MFDExtractor
{
	public class ExtractorState
	{
		public Form ApplicationForm { get; set; }
		public bool Running { get; set; }
        public bool KeepRunning { get; set; }
		public bool TestMode { get; set; }
		public bool NightMode { get; set; }
		public bool TwoDeePrimaryView { get; set; }
		public bool ThreeDeeMode { get; set; }
        public long RenderCycleNum { get; set; }
        public NetworkMode NetworkMode { get; set; }
        public bool SimRunning { get; set; }
	}
}
