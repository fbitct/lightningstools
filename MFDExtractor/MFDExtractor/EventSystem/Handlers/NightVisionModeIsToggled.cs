namespace MFDExtractor.EventSystem.Handlers
{
	class NightVisionModeIsToggled : IInputEventHandler
	{
		private readonly Extractor _extractor;

		public NightVisionModeIsToggled(Extractor extractor)
		{
			_extractor = extractor;
		}

		public void Raise()
		{
			_extractor.State.NightMode = !_extractor.State.NightMode;
		}
	}
}
