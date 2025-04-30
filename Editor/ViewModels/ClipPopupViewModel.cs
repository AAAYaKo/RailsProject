using System.ComponentModel;
using Rails.Runtime;
using Unity.Properties;

namespace Rails.Editor.ViewModel
{
	public class ClipPopupViewModel : BaseNotifyPropertyViewModel<RailsClip>
	{
		[CreateProperty]
		public string Duration
		{
			get => duration;
			set
			{
				if (duration == value)
					return;

				value = value.Replace(" ", "");

				if (TimeUtils.TryReadValue(value, RailsClip.Fps, out int frames))
				{
					EditorContext.Instance.Record("Clip Duration Changed");
					model.Duration = frames;
					duration = TimeUtils.FormatTime(frames, RailsClip.Fps);
				}
				NotifyPropertyChanged();
			}
		}

		private string duration;


		protected override void OnModelChanged()
		{
			duration = TimeUtils.FormatTime(model.Duration, RailsClip.Fps);
			NotifyPropertyChanged(nameof(Duration));
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RailsClip.Duration))
			{
				duration = TimeUtils.FormatTime(model.Duration, RailsClip.Fps);
				NotifyPropertyChanged(nameof(Duration));
			}
		}
	}
}