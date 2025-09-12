using System.ComponentModel;
using Rails.Runtime.Tracks;
using Unity.Properties;

namespace Rails.Editor.ViewModel
{
	public class AnimationKeyViewModel : BaseNotifyPropertyViewModel<AnimationKey>
	{
		[CreateProperty]
		public int TimePosition 
		{
			get => timePosition;
			set
			{
				if (timePosition == value)
					return;
				timePosition = value;
				NotifyPropertyChanged();
			}
		}

		private int timePosition;


		protected override void OnModelChanged()
		{
			if (model == null)
				return;

			TimePosition = model.TimePosition;
			NotifyPropertyChanged(nameof(TimePosition));
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AnimationKey.TimePosition))
				TimePosition = model.TimePosition;
		}
	}
}