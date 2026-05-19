using Rails.Editor.Controls;

namespace Rails.Editor
{
	public readonly struct KeyDragBeginEvent
	{
		public TrackKeyView[] Keys { get; }


		public KeyDragBeginEvent(params TrackKeyView[] keys)
		{
			Keys = keys;
		}
	}
}