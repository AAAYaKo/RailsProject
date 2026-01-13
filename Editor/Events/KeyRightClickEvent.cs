using Rails.Editor.Controls;

namespace Rails.Editor
{
	public readonly struct KeyRightClickEvent
	{
		public TrackKeyView Key { get; }

		public KeyRightClickEvent(TrackKeyView key)
		{
			Key = key;
		}
	}
}