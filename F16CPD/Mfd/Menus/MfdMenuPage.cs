using System.Collections.Generic;
using System.Drawing;
using F16CPD.Mfd.Controls;

namespace F16CPD.Mfd.Menus
{
    public class MfdMenuPage
    {
        protected MfdManager _manager;

        protected MfdMenuPage()
        {
        }

        public MfdMenuPage(MfdManager manager) : this()
        {
            _manager = manager;
        }

        public List<OptionSelectButton> OptionSelectButtons { get; set; }
        public string Name { get; set; }

        public MfdManager Manager
        {
            get { return _manager; }
        }

        /// <summary>
        /// Checks a set of x/y coordinates to see if that corresponds 
        /// to a screen position occupied by an Option Select Buttonlabel.  If so, 
        /// returns the corresponding <see cref="OptionSelectButton"/> object.  
        /// If not, returns <see langword="null/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public OptionSelectButton GetOptionSelectButtonByLocation(int x, int y)
        {
            foreach (OptionSelectButton button in OptionSelectButtons)
            {
                if (!button.Visible) continue;
                var origLabelRectangle = new Rectangle(button.LabelLocation, button.LabelSize);
                var labelX =
                    (int) (((Manager.ScreenBoundsPixels.Width)/Constants.F_NATIVE_RES_WIDTH)*origLabelRectangle.X);
                var labelY =
                    (int) (((Manager.ScreenBoundsPixels.Height)/Constants.F_NATIVE_RES_HEIGHT)*origLabelRectangle.Y);
                var labelWidth =
                    (int) (((Manager.ScreenBoundsPixels.Width)/Constants.F_NATIVE_RES_WIDTH)*origLabelRectangle.Width);
                var labelHeight =
                    (int)
                    (((Manager.ScreenBoundsPixels.Height)/Constants.F_NATIVE_RES_HEIGHT)*origLabelRectangle.Height);

                var labelRectangle = new Rectangle(labelX, labelY, labelWidth, labelHeight);
                if (x >= labelRectangle.X && y >= labelRectangle.Y && x <= (labelRectangle.X + labelRectangle.Width) &&
                    y <= (labelRectangle.Y + labelRectangle.Height))
                {
                    return button;
                }
            }
            return null;
        }

        public OptionSelectButton FindOptionSelectButtonByPositionNumber(float positionNumber)
        {
            foreach (OptionSelectButton button in OptionSelectButtons)
            {
                if (button.PositionNumber == positionNumber)
                {
                    return button;
                }
            }
            return null;
        }

        public OptionSelectButton FindOptionSelectButtonByFunctionName(string functionName)
        {
            foreach (OptionSelectButton button in OptionSelectButtons)
            {
                if (button.FunctionName == functionName) return button;
            }
            return null;
        }

        public OptionSelectButton FindOptionSelectButtonByLabelText(string labelText)
        {
            foreach (OptionSelectButton button in OptionSelectButtons)
            {
                if (button.LabelText == labelText) return button;
            }
            return null;
        }
    }
}