using Common.Drawing;

namespace F4Utils.SimSupport
{
	public class TexturesSharedMemoryImageCoordinates
	{
		public TexturesSharedMemoryImageCoordinates()
		{
			HUD = Rectangle.Empty;
			LMFD = Rectangle.Empty;
			RMFD = Rectangle.Empty;
			MFD3 = Rectangle.Empty;
			MFD4 = Rectangle.Empty;
            DED = Rectangle.Empty;
            PFL = Rectangle.Empty;
            RWR = Rectangle.Empty;
            HMS = Rectangle.Empty;
		}
		public Rectangle HUD { get; set; }
		public Rectangle LMFD { get; set; }
		public Rectangle RMFD { get; set; }
		public Rectangle MFD3 { get; set; }
		public Rectangle MFD4 { get; set; }
        public Rectangle DED { get; set; }
        public Rectangle PFL { get; set; }
        public Rectangle RWR { get; set; }
        public Rectangle HMS { get; set; }
    }
}
