using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DG.Tweening;
using Rails.Editor.Context;
using Rails.Runtime;
using Rails.Runtime.Tracks;
using Unity.EditorCoroutines.Editor;
using Unity.Properties;
using UnityEngine;
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
		private static readonly Dictionary<LoopType, string> styles = new()
		{
			{ LoopType.Restart, "loop-icon-cycle" },
			{ LoopType.Yoyo, "loop-icon-ping-pong" },
		};

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
				if (IsPreview)
				{
					var duration = GetLoopDuration();
					if (!IsFullDuration && value > duration)
						value.Frames = duration.Frames;
					if (timeHeadPosition != value)
					{
						int delta = value.Frames - timeHeadPosition.Frames;
						if (preview.IsLoopingOrExecutingBackwards())
							delta = -delta;
						Goto(delta);
					}
				}
				SetTimeHeadPosition(value);
			}
		}

		[CreateProperty]
		public LoopType LoopType
		{
			get => loopType ?? LoopType.Restart;
			set
			{
				if (SetProperty(ref loopType, value))
				{
					if (LoopType != model.LoopType)
					{
						EditorContext.Instance.Record("Changed Clip Loop Type");
						model.LoopType = LoopType;
					}
					NotifyPropertyChanged(nameof(LoopIconStyle));
				}
			}
		}
		[CreateProperty]
		public readonly List<LoopType> LoopTypes = new() { LoopType.Restart, LoopType.Yoyo };
		[CreateProperty]
		public int LoopCount
		{
			get => loopCount ?? 1;
			set
			{
				if (value < -1)
					value = -1;
				else if (value == 0)
					value = 1;
				if (SetProperty(ref loopCount, value))
				{
					if (LoopCount != model.LoopCount)
					{
						EditorContext.Instance.Record("Changed Clip Loop Count");
						model.LoopCount = LoopCount;
					}
				}
			}
		}
		[CreateProperty]
		public bool IsFullDuration
		{
			get => isFullDuration ?? false;
			set
			{
				if (SetProperty(ref isFullDuration, value))
				{
					EditorContext.Instance.Record("Changed Clip Looping Duration");
					model.IsFullDuration = IsFullDuration;
				}
			}
		}
		[CreateProperty]
		public string LoopIconStyle => styles[LoopType];
		[CreateProperty]
		public bool IsPreview
		{
			get => isPreview;
			set
			{
				if (SetProperty(ref isPreview, value))
				{
					if (IsPreview)
						StartPreview();
					else
						StopPreview();
					IsPlay = IsPreview;
				}
			}
		}

		[CreateProperty]
		public bool IsPlay
		{
			get => isPlay;
			set
			{
				if (SetProperty(ref isPlay, value))
				{
					if (!isPlay)
						preview.Pause();
					else
						preview.Play();
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
		[CreateProperty]
		public ICommand GotoNextFrameCommand
		{
			get => gotoNextFrameCommand;
			set => SetProperty(ref gotoNextFrameCommand, value);
		}

		private AnimationTime duration;
		private AnimationTime timeHeadPosition;
		private string name;
		private LoopType? loopType;
		private int? loopCount;
		private bool? isFullDuration;
		private EventTrackViewModel eventTrack = new();
		private ObservableList<IKeyViewModel> selectedKeys = new();
		private ObservableList<AnimationTrackViewModel> tracks = new();
		private Tween preview;
		private bool canEdit = true;
		private bool selected = false;
		private bool isPreview;
		private bool isPlay;
		private ICommand<Type> addTrackCommand;
		private ICommand removeCommand;
		private ICommand removeSelectedKeysCommand;
		private ICommand gotoNextFrameCommand;
		private EditorCoroutine reloadRoutine;


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
				else if (trackType == typeof(RotateTrack))
				{
					model.AddTrack(new RotateTrack());
				}
				else if (trackType == typeof(ScaleTrack))
				{
					model.AddTrack(new ScaleTrack());
				}
			});
			RemoveSelectedKeysCommand = new RelayCommand(RemoveKeys);
			GotoNextFrameCommand = new RelayCommand(GotoNextFrame);
		}

		public void Select(EventHandler<BindablePropertyChangedEventArgs> propertyChangedCallback)
		{
			selected = true;
			propertyChanged += propertyChangedCallback;
			eventTrack.OnClipSelect(OnKeysSelectionChanged);
			tracks.ForEach(x => x.OnClipSelect(OnKeysSelectionChanged));
			EventBus.Subscribe<ClipChangedEvent>(OnClipChanged);
		}

		public void Deselect(EventHandler<BindablePropertyChangedEventArgs> propertyChangedCallback)
		{
			selected = false;
			propertyChanged += propertyChangedCallback;
			eventTrack.OnClipDeselect();
			tracks.ForEach(x => x.OnClipDeselect());
			EventBus.Unsubscribe<ClipChangedEvent>(OnClipChanged);
			IsPreview = false;
		}

		protected override void OnModelChanged()
		{
			if (model == null)
				return;
			Name = model.Name;
			UpdateTracks();

			Duration = new() { Frames = model.Duration };
			TimeHeadPosition = new AnimationTime() { Frames = 0 };
			LoopType = model.LoopType;
			LoopCount = model.LoopCount;
			IsFullDuration = model.IsFullDuration;
			IsPreview = false;
			IsPlay = false;
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			bool mustReload = true;
			if (e.PropertyName == nameof(RailsClip.Name))
			{
				mustReload = false;
				Name = model.Name;
			}
			else if (e.PropertyName == nameof(RailsClip.Tracks))
				UpdateTracks();
			else if (e.PropertyName == nameof(RailsClip.Duration))
				Duration = new() { Frames = model.Duration };
			else if (e.PropertyName == nameof(RailsClip.LoopType))
				LoopType = model.LoopType;
			else if (e.PropertyName == nameof(RailsClip.LoopCount))
				LoopCount = model.LoopCount;
			else if (e.PropertyName == nameof(RailsClip.IsFullDuration))
				IsFullDuration = model.IsFullDuration;
			else
				mustReload = false;

			if (mustReload)
				EventBus.Publish(new ClipChangedEvent());
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
					vm.CheckReference = x =>
					{
						foreach (var track in Tracks)
						{
							if (track == vm)
								continue;
							if (track.TrackType == vm.TrackType && track.Reference == x)
								return false;
						}
						return true;
					};
				}
			);
		}

		private void OnClipChanged(ClipChangedEvent evt)
		{
			if (!IsPreview)
				return;
			if (reloadRoutine != null)
				return;
			reloadRoutine = EditorCoroutineUtility.StartCoroutineOwnerless(Routine());
			IEnumerator Routine()
			{
				yield return null;
				if (!IsPreview)
					yield break;
				ReloadPreview();
				reloadRoutine = null;
			}
		}

		private void StartPreview()
		{
			preview = model.BuildSequence();
			EditorPreviewer.PrepareTweenForPreview(preview, model.Tracks.Select(x => x.SceneReference).Where(x => x != null));
			EditorPreviewer.Start(RailsClip.FrameTime, x =>
			{
				float currentPosition = preview.ElapsedDirectionalPercentage();
				int frames = Mathf.RoundToInt(GetLoopDuration() * currentPosition);
				var position = TimeHeadPosition;
				position.Frames = frames;
				SetTimeHeadPosition(position);
			});
		}

		private void StopPreview()
		{
			EditorPreviewer.Stop();
		}

		private void ReloadPreview()
		{
			float elapsed = preview.Elapsed();
			StopPreview(); //Restart Preview
			StartPreview();
			preview.GotoWithCallbacks(elapsed, IsPlay);
		}

		private void SetTimeHeadPosition(AnimationTime value)
		{
			value = ClampTimeHeadPosition(value);
			if (SetProperty(ref timeHeadPosition, value, nameof(TimeHeadPosition)))
			{
				NotifyPropertyChanged(nameof(TimeHeadPositionText));
				Tracks.ForEach(x => x.OnTimeHeadPositionChanged(value.Frames));
			}
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

		private AnimationTime GetLoopDuration()
		{
			return IsFullDuration ? Duration : Tracks.Max(x => x.LastFrame);
		}

		private void GotoNextFrame()
		{
			Goto(1);
		}

		private void Goto(int frames)
		{
			if (IsPlay)
				IsPlay = false;

			float next = preview.Elapsed() + frames * RailsClip.FrameTime;
			preview.GotoWithCallbacks(next);
		}
	}
}