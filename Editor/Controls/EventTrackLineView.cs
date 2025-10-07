using Rails.Editor.ViewModel;
using Rails.Runtime;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class EventTrackLineView : BaseTrackLineView<EventKeyViewModel, EventKey>
	{
		public EventTrackLineView() : base()
		{
			AddToClassList("event-track-line");
		}
	}
}