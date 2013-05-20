using LightningGauges.Renderers;
using MFDExtractor.Properties;

namespace MFDExtractor.RendererFactories
{
	internal interface IFuelQualityIndicatorRendererFactory
	{
		IF16FuelQuantityIndicator Create();
	}

	class FuelQualityIndicatorRendererFactory : IFuelQualityIndicatorRendererFactory
	{
		public IF16FuelQuantityIndicator Create()
		{
			return new F16FuelQuantityIndicator
			{
				Options =
				{
					NeedleType =
						Settings.Default.FuelQuantityIndicator_NeedleCModel
							? F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.CModel
							: F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.DModel
				}
			};
		}
	}
}
