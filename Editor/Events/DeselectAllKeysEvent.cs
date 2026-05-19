using Rails.Editor.Controls;

namespace Rails.Editor
{
	public readonly struct DeselectAllKeysEvent
	{
		public TrackKeyView[] IgnoreKeys { get; }


		public DeselectAllKeysEvent(params TrackKeyView[] ignoreKeys)
		{
			IgnoreKeys = ignoreKeys;
		}
	}
}