using Rails.Editor.Controls;

namespace Rails.Editor
{
	public struct ClipClickEvent
	{
		public ClipItemView Clip { get; }

		public ClipClickEvent(ClipItemView clip)
		{
			Clip = clip;
		}
	}
}