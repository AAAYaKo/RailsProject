using System.ComponentModel;
using Rails.Editor.Context;
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


		public EventKeyViewModel(int keyIndex, ICommand<AnimationTime> moveKeyCommand) : base(keyIndex, moveKeyCommand)
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

		protected override void OnModelPropertyChanged(object sender, string propertyName)
		{
			base.OnModelPropertyChanged(sender, propertyName);
			if (propertyName == nameof(EventKey.AnimationEvent))
			{
				AnimationEvent.UnbindModel();
				AnimationEvent.BindModel(model.AnimationEvent);
			}

		}
	}
}