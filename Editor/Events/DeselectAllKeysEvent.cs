using Rails.Editor.Controls;

namespace Rails.Editor
{
	public readonly struct DeselectAllKeysEvent
	{
		public TrackKeyView Key { get; }


		public DeselectAllKeysEvent(TrackKeyView key)
		{
			Key = key;
		}
	}
}