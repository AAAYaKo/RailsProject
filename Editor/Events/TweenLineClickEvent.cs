using Rails.Editor.Controls;

namespace Rails.Editor
{
	public readonly struct TweenLineClickEvent
	{
		public TrackKeyView Start { get; }
		public TrackKeyView End { get; }
		public bool ActionKey { get; }


		public TweenLineClickEvent(TrackKeyView start, TrackKeyView end, bool actionKey)
		{
			Start = start;
			End = end;
			ActionKey = actionKey;
		}
	}
}