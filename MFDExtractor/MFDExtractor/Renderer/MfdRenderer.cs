﻿using System;
using Common.SimSupport;
using System.Drawing;

namespace MFDExtractor.Renderer
{
	public interface IMfdRenderer:IInstrumentRenderer
	{
		MfdRenderer.MfdRendererInstrumentState InstrumentState { get; set; }
		MfdRenderer.MfdRendererOptions Options { get; set; }
	}

	public class MfdRenderer:InstrumentRendererBase, IMfdRenderer
	{
		public MfdRenderer()
		{
			InstrumentState = new MfdRendererInstrumentState{SourceRectangle = Rectangle.Empty};
			Options = new MfdRendererOptions();
		}
		public override void Render(Graphics destinationGraphics, Rectangle destinationRectangle)
		{
			if (InstrumentState.Blank)
			{
				if (Options.BlankImage != null)
				{
					destinationGraphics.DrawImage(Options.BlankImage, destinationRectangle, new Rectangle(new Point(0, 0), Options.BlankImage.Size),GraphicsUnit.Pixel);
				}
			}
			else if (InstrumentState.TestMode)
			{
				if (Options.TestAlignmentImage != null)
				{
					destinationGraphics.DrawImage(Options.TestAlignmentImage, destinationRectangle, new Rectangle(new Point(0, 0), Options.TestAlignmentImage.Size), GraphicsUnit.Pixel);
				}
			}
			else
			{
				if (InstrumentState.SourceImage != null)
				{
				    RenderFromSharedmemSurface(destinationGraphics, destinationRectangle);
				}
			}
		}
	    private void RenderFromSharedmemSurface(Graphics destinationGraphics, Rectangle destinationRectangle)
	    {
            var mfdImage = InstrumentState.SourceImage;
	        try
	        {
                if (mfdImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Undefined)
                {
                    destinationGraphics.DrawImage(mfdImage, destinationRectangle, InstrumentState.SourceRectangle, GraphicsUnit.Pixel);
                }
	        }
	        catch (AccessViolationException){}
	        catch (InvalidOperationException){}
	    }

	    public MfdRendererInstrumentState InstrumentState { get; set; }
		public MfdRendererOptions Options { get; set; }
		public override void Dispose() {}
		~MfdRenderer()
        {
            Dispose(false);
        }

		[Serializable]
		public class MfdRendererOptions
		{
			public Image BlankImage { get; set; }
			public Image TestAlignmentImage { get; set; }
		}
		[Serializable]
		public class MfdRendererInstrumentState : InstrumentStateBase
		{
			public bool Blank { get; set; }
			public bool TestMode { get; set; }

		    [NonSerialized] 
            public Image SourceImage;

		    public object SourceImageHashCode
		    {
		        get { return SourceImage != null ? SourceImage.GetHashCode() : 0; }
		    }

		    public Rectangle SourceRectangle { get; set; }
		  
		}
	}
}
