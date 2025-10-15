using Rails.Runtime;
using Rails.Runtime.Tracks;

namespace Rails.Editor.ViewModel
{
	public class EventTrackViewModel : BaseTrackViewModel<EventsTrack, EventKey, EventKeyViewModel>
	{
		public override string TrackClass => "event";


		public EventTrackViewModel() : base()
		{
			storedSelectedIndexes = new StoredIntList(StoreKey + "event");
		}

		protected override EventKeyViewModel CreateKey(int index)
		{
			return new(TrackClass, index);
		}
	}
}