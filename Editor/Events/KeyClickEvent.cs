using Rails.Editor.Controls;

namespace Rails.Editor
{
	public class KeyClickEvent
	{
		public TrackKeyView Key { get; }
		public bool ActionKey { get; }


		public KeyClickEvent(TrackKeyView key, bool actionKey)
		{
			Key = key;
			ActionKey = actionKey;
		}
	}

	public class ClipClickEvent
	{
		public ClipItemView Clip { get; }

		public ClipClickEvent(ClipItemView clip)
		{
			Clip = clip;
		}
	}
}