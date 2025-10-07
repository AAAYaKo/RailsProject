using System;
using System.ComponentModel;
using Rails.Runtime;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class RailsClipViewModel : BaseNotifyPropertyViewModel<RailsClip>
	{
		public static readonly RailsClipViewModel Empty = new()
		{
			CanEdit = false,
			durationText = "--:--",
		};
		private static readonly CollectionComparer<AnimationTrackViewModel> tracksComparer = new();

		[CreateProperty]
		public string Name
		{
			get => name;
			set => SetProperty(ref name, value);
		}
		[CreateProperty]
		public EventTrackViewModel EventTrack
		{
			get => eventTrack;
			set => SetProperty(ref eventTrack, value);
		}
		[CreateProperty]
		public ObservableList<AnimationTrackViewModel> Tracks
		{
			get => tracks;
			set => SetProperty(ref tracks, value, tracksComparer);
		}
		[CreateProperty]
		public bool CanEdit
		{
			get => canEdit;
			private set => SetProperty(ref canEdit, value);
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

				Tracks.ForEach(x => x.OnTimeHeadPositionChanged(frames));
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

				Tracks.ForEach(x => x.OnTimeHeadPositionChanged(value));
			}
		}
		[CreateProperty]
		public ICommand<Type> AddTrackCommand
		{
			get => addTrackCommand;
			set => SetProperty(ref addTrackCommand, value);
		}
		[CreateProperty]
		public ICommand RemoveCommand
		{
			get => removeCommand;
			set => SetProperty(ref removeCommand, value);
		}
		[CreateProperty]
		public ICommand RemoveSelectedKeysCommand
		{
			get => removeSelectedKeysCommand;
			set => SetProperty(ref removeSelectedKeysCommand, value);
		}

		private string durationText;
		private int durationFrames;
		private string timeHeadPositionText;
		private int? timeHeadPositionFrames;
		private string name;
		private EventTrackViewModel eventTrack = new();
		private ObservableList<AnimationTrackViewModel> tracks = new();
		private bool canEdit = true;
		private bool selected = false;
		private ICommand<Type> addTrackCommand;
		private ICommand removeCommand;
		private ICommand removeSelectedKeysCommand;


		public RailsClipViewModel()
		{
			AddTrackCommand = new RelayCommand<Type>(trackType =>
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
			});
			RemoveSelectedKeysCommand = new RelayCommand(RemoveKeys);
		}

		public void Select(EventHandler<BindablePropertyChangedEventArgs> propertyChangedCallback)
		{
			selected = true;
			propertyChanged += propertyChangedCallback;
			eventTrack.OnClipSelect();
			tracks.ForEach(x => x.OnClipSelect());
		}

		public void Deselect(EventHandler<BindablePropertyChangedEventArgs> propertyChangedCallback)
		{
			selected = false;
			propertyChanged += propertyChangedCallback;
			eventTrack.OnClipDeselect();
			tracks.ForEach(x => x.OnClipDeselect());
		}

		protected override void OnModelChanged()
		{
			if (model == null)
				return;
			Name = model.Name;
			UpdateTracks();

			DurationFrames = model.Duration;

			TimeHeadPositionFrames = 0;
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RailsClip.Name))
				Name = model.Name;
			else if (e.PropertyName == nameof(RailsClip.Tracks))
				UpdateTracks();
			else if (e.PropertyName == nameof(RailsClip.Duration))
				DurationFrames = model.Duration;
		}

		protected override void OnUnbind()
		{
			base.OnUnbind();
			ClearViewModels<AnimationTrackViewModel, AnimationTrack>(Tracks, vm => vm.OnClipDeselect());
			eventTrack.UnbindModel();
		}

		protected override void OnBind()
		{
			base.OnBind();
			eventTrack.BindModel(model.EventTrack);
		}

		private void UpdateTracks()
		{
			UpdateVieModels(Tracks, model.Tracks,
				createViewModel: i => new AnimationTrackViewModel(i),
				resetViewModel: vm => vm.OnClipDeselect(),
				viewModelBindCallback: (vm, m) =>
				{
					if (selected)
						vm.OnClipSelect();
					vm.RemoveCommand = new RelayCommand(() =>
					{
						EditorContext.Instance.Record($"Removed {m.GetType().Name} from {name}");
						model.RemoveTrack(m);
					});
				}
			);
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

		protected void RemoveKeys()
		{
			EditorContext.Instance.Record("Key Frames Removed");
			eventTrack.RemoveSelectedKeys();
			foreach (var track in tracks)
				track.RemoveSelectedKeys();
		}
	}
}