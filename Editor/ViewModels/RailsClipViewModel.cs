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

				value = value.Replace(" ", "");

				if (TimeUtils.TryReadValue(value, RailsClip.Fps, out int frames))
				{
					if (frames < 1)
						frames = 1;
					if (DurationFrames != frames)
					{
						EditorContext.Instance.Record("Clip Duration Changed");
						DurationFrames = frames;
					}
					durationText = TimeUtils.FormatTime(frames, RailsClip.Fps);
				}
				NotifyPropertyChanged();
				notify?.Invoke();
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

				durationFrames = value;
				model.Duration = value;

				NotifyPropertyChanged();
				notify?.Invoke();
			}
		}

		private string durationText;
		private int durationFrames;
		private string name;
		private ObservableList<AnimationTrackViewModel> tracks = new();
		private bool canEdit = true;
		private Action notify;


		protected override void OnModelChanged()
		{
			if (model == null)
				return;
			Name = model.Name;
			UpdateViewModels(model.Tracks);
			DurationFrames = model.Duration;
			durationText = TimeUtils.FormatTime(model.Duration, RailsClip.Fps);
			NotifyPropertyChanged(nameof(DurationText));
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
				durationText = TimeUtils.FormatTime(model.Duration, RailsClip.Fps);
				NotifyPropertyChanged(nameof(DurationText));
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
				Tracks.AddWithoutNotify(new AnimationTrackViewModel());
			}
			while (Tracks.Count > model.Tracks.Count)
			{
				var clip = Tracks[^1];
				clip.UnbindModel();
				Tracks.RemoveWithoutNotify(clip);
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
	}
}