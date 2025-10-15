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
		};
		private static readonly CollectionComparer<AnimationTrackViewModel> tracksComparer = new();
		private static readonly CollectionComparer<IKeyViewModel> keysComparer = new();

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
		public ObservableList<IKeyViewModel> SelectedKeys => selectedKeys;
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
			get => duration.FormatTime(RailsClip.Fps);
			set
			{
				if (AnimationTime.TryParse(value, RailsClip.Fps, out var duration))
				{
					if (Duration != duration)
					{
						EditorContext.Instance.Record("Clip Duration Changed");
						Duration = duration;
						model.Duration = duration;
					}
				}
			}
		}
		[CreateProperty]
		public string TimeHeadPositionText
		{
			get => timeHeadPosition.FormatTime(RailsClip.Fps);
			set
			{
				if (AnimationTime.TryParse(value, RailsClip.Fps, out var timeHeadPosition))
				{
					if (TimeHeadPosition != timeHeadPosition)
					{
						TimeHeadPosition = timeHeadPosition;
					}
				}
			}
		}
		[CreateProperty]
		public AnimationTime Duration
		{
			get => duration;
			set
			{
				value = ClampDuration(value);
				if (SetProperty(ref duration, value))
				{
					NotifyPropertyChanged(nameof(DurationText));
					TimeHeadPosition = ClampTimeHeadPosition(TimeHeadPosition);
				}

			}
		}
		[CreateProperty]
		public AnimationTime TimeHeadPosition
		{
			get => timeHeadPosition;
			set
			{
				value = ClampTimeHeadPosition(value);
				if (SetProperty(ref timeHeadPosition, value))
				{
					NotifyPropertyChanged(nameof(TimeHeadPositionText));
					Tracks.ForEach(x => x.OnTimeHeadPositionChanged(value.Frames));
				}
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

		private AnimationTime duration;
		private AnimationTime timeHeadPosition;
		private string name;
		private EventTrackViewModel eventTrack = new();
		private ObservableList<IKeyViewModel> selectedKeys = new();
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
			eventTrack.OnClipSelect(OnKeysSelectionChanged);
			tracks.ForEach(x => x.OnClipSelect(OnKeysSelectionChanged));
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

			Duration = new() { Frames = model.Duration };
			TimeHeadPosition = new AnimationTime() { Frames = 0 };
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RailsClip.Name))
				Name = model.Name;
			else if (e.PropertyName == nameof(RailsClip.Tracks))
				UpdateTracks();
			else if (e.PropertyName == nameof(RailsClip.Duration))
				Duration = new() { Frames = model.Duration };
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
			UpdateViewModels(Tracks, model.Tracks,
				createViewModel: i => new AnimationTrackViewModel(i),
				resetViewModel: vm => vm.OnClipDeselect(),
				viewModelBindCallback: (vm, m) =>
				{
					if (selected)
						vm.OnClipSelect(OnKeysSelectionChanged);
					vm.RemoveCommand = new RelayCommand(() =>
					{
						EditorContext.Instance.Record($"Removed {m.GetType().Name} from {name}");
						model.RemoveTrack(m);
					});
				}
			);
		}

		private AnimationTime ClampTimeHeadPosition(AnimationTime value)
		{
			if (value.Frames < 0)
				value.Frames = 0;
			else if (value.Frames > Duration.Frames)
				value.Frames = Duration.Frames;
			return value;
		}

		private AnimationTime ClampDuration(AnimationTime value)
		{
			if (value.Frames < 1)
				value.Frames = 1;
			return value;
		}

		private void RemoveKeys()
		{
			EditorContext.Instance.Record("Key Frames Removed");
			eventTrack.RemoveSelectedKeys();
			foreach (var track in tracks)
				track.RemoveSelectedKeys();
		}

		private void OnKeysSelectionChanged()
		{
			selectedKeys.ClearWithoutNotify();
			selectedKeys.AddRangeWithoutNotify(eventTrack.SelectedKeys);
			foreach (var track in tracks)
				selectedKeys.AddRangeWithoutNotify(track.SelectedKeys);
			selectedKeys.NotifyListChanged();
		}
	}
}