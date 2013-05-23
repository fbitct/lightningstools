using System;
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
					graphics.DrawImage(Options.BlankImage, 0, 0, new Rectangle(new Point(0, 0), Options.BlankImage.Size),GraphicsUnit.Pixel);
				}
			}
			else if (InstrumentState.TestMode)
			{
				if (Options.TestAlignmentImage != null)
				{
					graphics.DrawImage(Options.TestAlignmentImage, 0, 0, new Rectangle(new Point(0, 0), Options.TestAlignmentImage.Size), GraphicsUnit.Pixel);
				}
			}
			else
			{
				if (Options.SourceImage != null)
				{
					graphics.DrawImage(Options.SourceImage, 0, 0, Options.SourceRectangle, GraphicsUnit.Pixel);
				}
			}
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
