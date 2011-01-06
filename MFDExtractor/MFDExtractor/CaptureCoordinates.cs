
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class CaptureCoordinatesSet
    {
        public CaptureCoordinates LMFD = new CaptureCoordinates();
        public CaptureCoordinates RMFD = new CaptureCoordinates();
        public CaptureCoordinates MFD3 = new CaptureCoordinates();
        public CaptureCoordinates MFD4 = new CaptureCoordinates();
        public CaptureCoordinates HUD = new CaptureCoordinates();

    }

}
