using Rails.Editor.Controls;

namespace Rails.Editor
{
	public struct KeyRightClickEvent
	{
		public TrackKeyView Key { get; }

		public KeyRightClickEvent(TrackKeyView key)
		{
			Key = key;
		}
	}
}