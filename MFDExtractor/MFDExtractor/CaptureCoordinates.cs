using System.Drawing;

namespace MFDExtractor
{
    public class CaptureCoordinates 
    {
        public CaptureCoordinates()
        {
            Primary2D = new Rectangle();
            Secondary2D = new Rectangle();
            ThreeDee = new Rectangle();
            Output = new Rectangle();
            
        }
        public Rectangle Primary2D { get; set; }
        public Rectangle Secondary2D { get; set; }
        public Rectangle ThreeDee { get; set; }
        public Rectangle Output { get; set; }
    }
}
