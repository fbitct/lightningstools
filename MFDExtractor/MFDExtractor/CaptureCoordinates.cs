using System.Drawing;
using System.Windows.Forms;

namespace MFDExtractor
{
    public class CaptureCoordinates
    {
        public Rectangle Primary2DModeCoords { get; set; }
        public Rectangle Secondary2DModeCoords { get; set; }
        public Rectangle RTTSourceCoords { get; set; }
        public Rectangle OutputWindowCoords { get; set; }
        public Screen OutputScreen { get; set; }
    }
}