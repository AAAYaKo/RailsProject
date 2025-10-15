using Rails.Runtime;
using Unity.Properties;

namespace Rails.Editor.ViewModel
{
	public class EventKeyViewModel : BaseKeyViewModel<EventKey>
	{
		[CreateProperty]
		public SerializableEventViewModel AnimationEvent
		{
			get => animationEvent;
			set => SetProperty(ref animationEvent, value);
		}

		public override string TrackName => "Events";

		private SerializableEventViewModel animationEvent = new();


		public EventKeyViewModel(string trackClass, int keyIndex) : base(trackClass, keyIndex)
		{
		}

		protected override void OnBind()
		{
			base.OnBind();
			AnimationEvent.BindModel(model.AnimationEvent);
		}

		protected override void OnUnbind()
		{
			base.OnUnbind();
			AnimationEvent.UnbindModel();
		}
	}
}