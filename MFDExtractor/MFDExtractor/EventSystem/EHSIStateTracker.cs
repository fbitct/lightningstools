using System;
using LightningGauges.Renderers;

namespace MFDExtractor.EventSystem
{
	internal interface IEHSIStateTracker
	{
		DateTime? RightKnobDepressedTime { get; set; }
		DateTime? RightKnobLastActivityTime { get; set; }
		DateTime? RightKnobReleasedTime { get; set; }
		bool RightKnobIsPressed { get; }
		void UpdateEHSIBrightnessLabelVisibility();
	}

	class EHSIStateTracker : IEHSIStateTracker
	{
		public DateTime? RightKnobDepressedTime { get; set; }
		public DateTime? RightKnobLastActivityTime { get; set; }
		public DateTime? RightKnobReleasedTime { get; set; }
		public bool RightKnobIsPressed {  get { return RightKnobDepressedTime.HasValue; } }
		private readonly IF16EHSI _ehsi;

		public EHSIStateTracker(IF16EHSI ehsi)
		{
			_ehsi = ehsi;
		}

		public void UpdateEHSIBrightnessLabelVisibility()
		{
			var showBrightnessLabel = false;
			if (RightKnobIsPressed)
			{
				if (RightKnobDepressedTime.HasValue)
				{
					var howLongPressed = DateTime.Now.Subtract(RightKnobDepressedTime.Value);
					if (howLongPressed.TotalMilliseconds > 2000)
					{
						showBrightnessLabel = true;
					}
				}
			}
			else
			{
				if (RightKnobReleasedTime.HasValue && RightKnobLastActivityTime.HasValue)
				{
					var howLongAgoReleased = DateTime.Now.Subtract(RightKnobReleasedTime.Value);
					var howLongAgoLastActivity = DateTime.Now.Subtract(RightKnobLastActivityTime.Value);
					if (howLongAgoReleased.TotalMilliseconds < 2000 || howLongAgoLastActivity.TotalMilliseconds < 2000)
					{
						showBrightnessLabel = _ehsi.InstrumentState.ShowBrightnessLabel;
					}
				}
			}
			_ehsi.InstrumentState.ShowBrightnessLabel = showBrightnessLabel;
		}
	}
}
