﻿using System;
using System.Diagnostics;
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
			InstrumentState = new MfdRendererInstrumentState();
			Options = new MfdRendererOptions {SourceRectangle = Rectangle.Empty};
		}
		public override void Render(Graphics graphics, Rectangle bounds)
		{
			if (InstrumentState.Blank)
			{
				if (Options.BlankImage != null)
				{
					graphics.DrawImage(Options.BlankImage, bounds, new Rectangle(new Point(0, 0), Options.BlankImage.Size),GraphicsUnit.Pixel);
				}
			}
			else if (InstrumentState.TestMode)
			{
				if (Options.TestAlignmentImage != null)
				{
					graphics.DrawImage(Options.TestAlignmentImage, bounds, new Rectangle(new Point(0, 0), Options.TestAlignmentImage.Size), GraphicsUnit.Pixel);
				}
			}
			else
			{
				if (Options.SourceImage != null)
				{
				    RenderFromSharedmemSurface(graphics, bounds);
				}
			}
		}
	    private void RenderFromSharedmemSurface(Graphics graphics, Rectangle bounds)
	    {
            var image = Options.SourceImage;
	        try
	        {
                if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Undefined)
                {
                    graphics.DrawImage(image, bounds, Options.SourceRectangle, GraphicsUnit.Pixel);
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
			public Image SourceImage { get; set; }
			public Rectangle SourceRectangle { get; set; }
		}
		[Serializable]
		public class MfdRendererInstrumentState : InstrumentStateBase
		{
			public bool Blank { get; set; }
			public bool TestMode { get; set; }
		}
	}
}