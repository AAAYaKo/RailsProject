using System.ComponentModel;
using Rails.Runtime;
using Rails.Runtime.Tracks;
using Unity.Properties;

namespace Rails.Editor.ViewModel
{
	public abstract class BaseKeyViewModel<TKey> : BaseNotifyPropertyViewModel<TKey>, IKeyViewModel
		where TKey : BaseKey
	{
		[CreateProperty]
		public AnimationTime TimePosition
		{
			get => timePosition;
			set
			{
				if (SetProperty(ref timePosition, value))
					NotifyPropertyChanged(nameof(TimePositionText));
			}
		}
		[CreateProperty]
		public string TimePositionText
		{
			get => timePosition.FormatTime(RailsClip.Fps);
			set
			{
				if (AnimationTime.TryParse(value, RailsClip.Fps, out var timePosition))
				{
					if (TimePosition != timePosition)
					{
						EditorContext.Instance.Record("Clip Duration Changed");
						TimePosition = timePosition;
					}
				}
			}
		}
		[CreateProperty]
		public string TrackClass { get; set; }
		[CreateProperty]
		public abstract string TrackName { get; }
		[CreateProperty]
		public int KeyIndex { get; }
		[CreateProperty]
		public bool ShowInspectorFoldout
		{
			get => showInspectorFoldout;
			set => SetProperty(ref showInspectorFoldout, value);
		}

		private bool showInspectorFoldout;
		private AnimationTime timePosition;


		public BaseKeyViewModel(string trackClass, int keyIndex)
		{
			TrackClass = trackClass;
			KeyIndex = keyIndex;
		}

		protected override void OnModelChanged()
		{
			if (model == null)
				return;

			TimePosition = new AnimationTime() { Frames = model.TimePosition };
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AnimationKey.TimePosition))
				TimePosition = new AnimationTime() { Frames = model.TimePosition };
		}
	}

	public interface IKeyViewModel
	{
		public AnimationTime TimePosition { get; set; }
		public string TimePositionText { get; set; }
		public string TrackClass { get; set; }
		public string TrackName { get; }
		public int KeyIndex { get; }
		public bool ShowInspectorFoldout { get; set; }
	}
}