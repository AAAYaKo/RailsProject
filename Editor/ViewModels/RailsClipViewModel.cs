using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rails.Runtime;
using Rails.Runtime.Tracks;
using Unity.Properties;

namespace Rails.Editor.ViewModel
{
	public class RailsClipViewModel : BaseNotifyPropertyViewModel<RailsClip>
	{
		public static readonly RailsClipViewModel Empty = new()
		{
			CanEdit = false,
			durationText = "--:--",
		};

		[CreateProperty]
		public string Name
		{
			get => name;
			set
			{
				if (Name == value)
					return;
				name = value;
				NotifyPropertyChanged();
				notify?.Invoke();
			}
		}
		[CreateProperty]
		public ObservableList<AnimationTrackViewModel> Tracks
		{
			get => tracks;
			set
			{
				if (tracks == value)
					return;
				tracks = value;
				NotifyPropertyChanged();
				notify?.Invoke();
			}
		}
		[CreateProperty]
		public bool CanEdit
		{
			get => canEdit;
			private set
			{
				if (canEdit == value)
					return;
				canEdit = value;
				NotifyPropertyChanged();
				notify?.Invoke();
			}
		}
		[CreateProperty]
		public string DurationText
		{
			get => durationText;
			set
			{
				if (durationText == value)
					return;

				SetDurationTextWithoutNotify(value, out int frames);
				if (DurationFrames != frames)
					EditorContext.Instance.Record("Clip Duration Changed");
				SetDurationFramesWithoutNotify(frames);
				TimeHeadPositionFrames = ClampTimeHeadPosition(TimeHeadPositionFrames);

				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(DurationFrames));
				notify?.Invoke();
			}
		}
		[CreateProperty]
		public string TimeHeadPositionText
		{
			get => timeHeadPositionText;
			set
			{
				if (timeHeadPositionText == value)
					return;

				SetTimeHeadPositionTextWithoutNotify(value, out int frames);
				SetTimeHeadPositionFramesWithoutNotify(frames);

				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(TimeHeadPositionFrames));

				notify?.Invoke();
				timeHeadPositionFramesChanged?.Invoke(frames);
			}
		}
		[CreateProperty]
		public int DurationFrames
		{
			get => durationFrames;
			set
			{
				if (durationFrames == value)
					return;

				SetDurationFramesWithoutNotify(value);
				SetDurationTextWithoutNotify(EditorUtils.FormatTime(DurationFrames, RailsClip.Fps), out _);
				TimeHeadPositionFrames = ClampTimeHeadPosition(TimeHeadPositionFrames);

				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(DurationText));
			}
		}
		[CreateProperty]
		public int TimeHeadPositionFrames
		{
			get => timeHeadPositionFrames ?? 0;
			set
			{
				if (timeHeadPositionFrames == value)
					return;

				SetTimeHeadPositionFramesWithoutNotify(value);
				SetTimeHeadPositionTextWithoutNotify(EditorUtils.FormatTime(TimeHeadPositionFrames, RailsClip.Fps), out _);

				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(TimeHeadPositionText));

				notify?.Invoke();
				timeHeadPositionFramesChanged?.Invoke(value);
			}
		}

		public event Action SelectionChanged;

		private string durationText;
		private int durationFrames;
		private string timeHeadPositionText;
		private int? timeHeadPositionFrames;
		private string name;
		private ObservableList<AnimationTrackViewModel> tracks = new();
		private bool canEdit = true;
		private Action notify;
		private event Action<int> timeHeadPositionFramesChanged;


		protected override void OnModelChanged()
		{
			if (model == null)
				return;
			Name = model.Name;
			UpdateViewModels(model.Tracks);

			DurationFrames = model.Duration;

			TimeHeadPositionFrames = 0;
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RailsClip.Name))
			{
				Name = model.Name;
			}
			if (e.PropertyName == nameof(RailsClip.Tracks))
			{
				UpdateViewModels(model.Tracks);
			}
			if (e.PropertyName == nameof(RailsClip.Duration))
			{
				DurationFrames = model.Duration;
			}
		}

		public void AddTrack(Type trackType)
		{
			EditorContext.Instance.Record($"Added {trackType.Name} to {name}");
			if (trackType == typeof(MoveAnchorTrack))
			{
				model.AddTrack(new MoveAnchorTrack());
			}
			else if (trackType == typeof(FadeTrack))
			{
				model.AddTrack(new FadeTrack());
			}
		}

		public void RemoveTrack(int index)
		{
			EditorContext.Instance.Record($"Removed {model.Tracks[index].GetType().Name} from {name}");
			model.RemoveTrack(model.Tracks[index]);
		}

		public void Select(Action notify)
		{
			this.notify = notify;
		}

		public void Deselect()
		{
			notify = null;
		}

		private void UpdateViewModels(List<AnimationTrack> models)
		{
			if (models == null)
			{
				ClearViewModels();
				return;
			}

			while (Tracks.Count < model.Tracks.Count)
			{
				AnimationTrackViewModel track = new();
				timeHeadPositionFramesChanged += track.OnTimeHeadPositionChanged;
				Tracks.AddWithoutNotify(track);
			}
			while (Tracks.Count > model.Tracks.Count)
			{
				var track = Tracks[^1];
				track.UnbindModel();
				timeHeadPositionFramesChanged -= track.OnTimeHeadPositionChanged;
				Tracks.RemoveWithoutNotify(track);
			}
			for (int i = 0; i < model.Tracks.Count; i++)
			{
				var clip = model.Tracks[i];
				var viewModel = Tracks[i];

				viewModel.UnbindModel();
				viewModel.BindModel(clip);
			}

			Tracks.NotifyListChanged();
		}

		private void ClearViewModels()
		{
			foreach (var clip in Tracks)
				clip.UnbindModel();
			Tracks.Clear();
		}

		private int ClampTimeHeadPosition(int value)
		{
			if (value < 0)
				value = 0;
			else if (value > DurationFrames)
				value = DurationFrames;
			return value;
		}

		private int ClampDuration(int value)
		{
			if (value < 1)
				value = 1;
			return value;
		}

		private void SetTimeHeadPositionFramesWithoutNotify(int value)
		{
			value = ClampTimeHeadPosition(value);
			timeHeadPositionFrames = value;
		}

		private void SetTimeHeadPositionTextWithoutNotify(string value, out int frames)
		{
			value = value.Replace(" ", "");

			if (EditorUtils.TryReadTimeValue(value, RailsClip.Fps, out frames))
			{
				frames = ClampTimeHeadPosition(frames);
				timeHeadPositionText = EditorUtils.FormatTime(frames, RailsClip.Fps);
			}
		}

		private void SetDurationFramesWithoutNotify(int value)
		{
			value = ClampDuration(value);
			durationFrames = value;
			model.Duration = value;
		}

		private void SetDurationTextWithoutNotify(string value, out int frames)
		{
			value = value.Replace(" ", "");

			if (EditorUtils.TryReadTimeValue(value, RailsClip.Fps, out frames))
			{
				frames = ClampDuration(frames);
				durationText = EditorUtils.FormatTime(frames, RailsClip.Fps);
			}
		}
	}
}