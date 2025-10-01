using Rails.Editor.Controls;

namespace Rails.Editor
{
	public struct TimePositionChangedEvent
	{
		public float TimePosition { get; }


		public TimePositionChangedEvent(float timePosition)
		{
			TimePosition = timePosition;
		}
	}

	public struct DeselectAllKeysEvent
	{
		public TrackKeyView Key { get; }


		public DeselectAllKeysEvent(TrackKeyView key)
		{
			Key = key;
		}
	}
}