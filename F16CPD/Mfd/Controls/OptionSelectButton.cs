
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using F16CPD.Mfd.Menus;
namespace F16CPD.Mfd.Controls
{
    public enum HAlignment
    {
        Center,
        Left,
        Right,
    }
    public enum VAlignment
    {
        Center,
        Top,
        Bottom
    }
    public class OptionSelectButton:MomentaryButtonMfdInputControl
    {
        protected MfdMenuPage _page = null;
        protected OptionSelectButton():base()
        {
        }
        public OptionSelectButton(MfdMenuPage page)
            : this()
        {
            _page = page;
            this.ForeColor = Color.FromArgb(0, 255, 0);
            this.BackColor= Color.Transparent;
            this.TextFont = new Font(FontFamily.GenericMonospace, 10, FontStyle.Bold, GraphicsUnit.Point);
            this.TextHAlignment = HAlignment.Center;
            this.TextVAlignment = VAlignment.Center;
            this.TriangleLegLength = 15;
            this.Visible = true;
        }
        public int TriangleLegLength
        {
            get;
            set;
        }
        public string LabelText
        {
            get;
            set;
        }
        public bool InvertLabelText
        {
            get;
            set;
        }
        public Size LabelSize
        {
            get;
            set;
        }
        public Point LabelLocation
        {
            get;
            set;
        }
        public HAlignment TextHAlignment
        {
            get;
            set;
        }
        public VAlignment TextVAlignment
        {
            get;
            set;
        }
        public void DrawLabel (Graphics g) 
        {
            Matrix origTransform = g.Transform;
            g.TranslateTransform(this.LabelLocation.X, this.LabelLocation.Y);
            string text = this.LabelText;
            Size labelSize = this.LabelSize;
            Font font = this.TextFont;

            Color foreColor = this.InvertLabelText ? this.BackColor : this.ForeColor;
            Color backColor = this.InvertLabelText ? this.ForeColor : this.BackColor;

            if (foreColor == Color.Transparent) foreColor = Color.Black;
            SolidBrush forecolorBrush = new SolidBrush(foreColor);
            SolidBrush backcolorBrush = new SolidBrush(backColor);

            Rectangle backgroundRectangle = new Rectangle(new Point(0, 0), labelSize);

            int maxTextAreaWidth = backgroundRectangle.Width;
            int numLinesOfText = 1;
            foreach (char thisChar in text.ToCharArray())
            {
                if (thisChar == '\n')
                {
                    numLinesOfText++;
                }
            }
            Size textSize = new Size(maxTextAreaWidth, (font.Height * numLinesOfText));

            StringFormat textFormat = new StringFormat();
            textFormat.Trimming = StringTrimming.EllipsisCharacter;
            textFormat.LineAlignment = StringAlignment.Center;
            textFormat.Alignment = StringAlignment.Center;

            int textX = 0;
            int textY = 0;
            if (this.TextVAlignment == VAlignment.Top)
            {
                textFormat.LineAlignment = StringAlignment.Near;
                textY = backgroundRectangle.Top;
            }
            else if (this.TextVAlignment == VAlignment.Bottom)
            {
                textFormat.LineAlignment = StringAlignment.Far;
                textY = backgroundRectangle.Bottom - textSize.Height;
            }
            else if (this.TextVAlignment == VAlignment.Center)
            {
                textFormat.LineAlignment = StringAlignment.Center;
                textY = backgroundRectangle.Top + (((backgroundRectangle.Bottom - backgroundRectangle.Top)-textSize.Height)/2) ;
            }

            if (this.TextHAlignment == HAlignment.Left)
            {
                textFormat.Alignment = StringAlignment.Near;
                textX = backgroundRectangle.Left;
            }
            else if (this.TextHAlignment == HAlignment.Right)
            {
                textFormat.Alignment = StringAlignment.Far;
                textX = backgroundRectangle.Right - textSize.Width;
            }
            else if (this.TextHAlignment == HAlignment.Center)
            {
                textFormat.Alignment = StringAlignment.Center;
                textX = backgroundRectangle.Left + ((backgroundRectangle.Width - textSize.Width) / 2);
            }
            Rectangle textBoundingRectangle = new Rectangle(new Point(textX, textY), textSize);
            if (!String.IsNullOrEmpty(text.Trim()))
            {
                g.FillRectangle(backcolorBrush, textBoundingRectangle);
                if (text.Trim() == "^" || text.Trim() == @"\/")
                {
                    int xCoordinate = 0;
                    int yCoordinate = 0;
                    
                    if (this.TextHAlignment == HAlignment.Left)
                    {
                        xCoordinate = 0;
                    }
                    else if (this.TextHAlignment == HAlignment.Right)
                    {
                        xCoordinate = backgroundRectangle.Width - this.TriangleLegLength;
                    }
                    else if (this.TextHAlignment == HAlignment.Center)
                    {
                        xCoordinate = ((backgroundRectangle.Width - this.TriangleLegLength) / 2);
                    }

                    if (this.TextVAlignment == VAlignment.Top)
                    {
                        yCoordinate = 0;
                    }
                    else if (this.TextVAlignment == VAlignment.Center)
                    {
                        yCoordinate = ((backgroundRectangle.Height - this.TriangleLegLength) / 2);
                    }
                    else if (this.TextVAlignment == VAlignment.Bottom)
                    {
                        yCoordinate = backgroundRectangle.Height - this.TriangleLegLength;
                    }

                    Point[] points = new Point[3];
                    Rectangle boundingRectangle = new Rectangle(new Point(0, 0), new Size(this.TriangleLegLength, this.TriangleLegLength));
                    if (text.Trim() == "^")
                    {
                        points[0] = new Point(boundingRectangle.Left + boundingRectangle.Width / 2, ((boundingRectangle.Height - this.TriangleLegLength) / 2)); //top point of triangle
                        points[1] = new Point(((boundingRectangle.Width - this.TriangleLegLength) / 2), ((boundingRectangle.Height - this.TriangleLegLength) / 2) + this.TriangleLegLength); //lower left point of triangle
                        points[2] = new Point(((boundingRectangle.Width - this.TriangleLegLength) / 2) + this.TriangleLegLength, ((boundingRectangle.Height - this.TriangleLegLength) / 2) + this.TriangleLegLength); //lower right point of triangle
                    }
                    if (text.Trim() == @"\/")
                    {
                        points[0] = new Point(boundingRectangle.Left + boundingRectangle.Width / 2, ((boundingRectangle.Height - this.TriangleLegLength) / 2) + this.TriangleLegLength); //bottom point of triangle
                        points[1] = new Point(((boundingRectangle.Width - this.TriangleLegLength) / 2), ((boundingRectangle.Height - this.TriangleLegLength) / 2) ); //upper left point of triangle
                        points[2] = new Point(((boundingRectangle.Width - this.TriangleLegLength) / 2) + this.TriangleLegLength, ((boundingRectangle.Height - this.TriangleLegLength) / 2) ); //upper right point of triangle
                    }
                    g.TranslateTransform(xCoordinate, yCoordinate);
                    g.FillPolygon(forecolorBrush, points);
                    g.TranslateTransform(-xCoordinate, -yCoordinate);

                }
                else
                {
                    g.DrawString(text, font, forecolorBrush, textBoundingRectangle, textFormat);
                }
                /*
                Pen p = new Pen(Color.Red);
                p.Width = 1;
                g.DrawRectangle(p, textBoundingRectangle);
                g.DrawRectangle(p, backgroundRectangle);
                */
                g.Transform = origTransform;
            }
        }
        public Color ForeColor
        {
            get;
            set;
        }
        public Color BackColor
        {
            get;
            set;
        }
        public Font TextFont
        {
            get;
            set;
        }
        public float PositionNumber
        {
            get;
            set;
        }
        public MfdMenuPage Page
        {
            get
            {
                return _page;
            }
        }
        public string FunctionName
        {
            get;
            set;
        }
        public bool Visible
        {
            get;
            set;
        }
    }
}
