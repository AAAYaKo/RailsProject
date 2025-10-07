using System.ComponentModel;
using Rails.Runtime;
using Rails.Runtime.Tracks;
using Unity.Properties;

namespace Rails.Editor.ViewModel
{
	public abstract class BaseKeyViewModel<TKey> : BaseNotifyPropertyViewModel<TKey>
		where TKey : BaseKey
	{
		[CreateProperty]
		public int TimePosition
		{
			get => timePosition ?? 0;
			set => SetProperty(ref timePosition, value);
		}

		private int? timePosition;


		protected override void OnModelChanged()
		{
			if (model == null)
				return;

			TimePosition = model.TimePosition;
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AnimationKey.TimePosition))
				TimePosition = model.TimePosition;
		}
	}
}