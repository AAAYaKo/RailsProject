using Rails.Editor.Controls;

namespace Rails.Editor
{
	public readonly struct ClipClickEvent
	{
		public ClipItemView Clip { get; }

		public ClipClickEvent(ClipItemView clip)
		{
			Clip = clip;
		}
	}
}