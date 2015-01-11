using System.Drawing;

namespace F16CPD.FlightInstruments.Pfd
{
    internal class AdiPitchLadderRenderer
    {
        internal static void DrawAdiPitchLadder(Graphics g, int centerYPfd, int centerXPfd,
            float pitchAngleDegrees, float rollAngleDegrees, bool adiOffFlag)
        {
            //obtain pitch ladder Bitmap
            var adiPitchLadder = AdiPitchLadderFactory.GetAdiPitchLadder(pitchAngleDegrees, rollAngleDegrees);
            var centerYAdiBars = (adiPitchLadder.Height / 2) + 1;
            var adiBarsXpos = centerXPfd - (adiPitchLadder.Width / 2);
            var adiBarsYpos = centerYPfd - centerYAdiBars;

            //draw pitch ladder
            if (!adiOffFlag)
            {
                g.DrawImage(adiPitchLadder, adiBarsXpos, adiBarsYpos);
            }
        }
    }
}