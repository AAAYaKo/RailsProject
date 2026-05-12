using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rails.Editor.Context;
using Rails.Editor.Manipulator;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.EditorCoroutines.Editor;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class ClipView : FocusableView
	{
		private static readonly BindingId TimeHeadPositionProperty = new("TimeHeadPosition");
		private const string FixedDimensionKey = "clipFixedDimension";

		[UxmlAttribute("duration"), CreateProperty]
		public int Duration
		{
			get => duration;
			set
			{
				if (duration == value)
					return;
				duration = value;
				AdjustContainer(framePixelSize);
			}
		}
		[UxmlAttribute("time-position"), CreateProperty]
		public int TimeHeadPosition
		{
			get => timeHeadPosition;
			set
			{
				if (timeHeadPosition == value)
					return;
				timeHeadPosition = value;
				NotifyPropertyChanged(TimeHeadPositionProperty);
			}
		}
		[CreateProperty]
		public ICommand RemoveSelectedKeysCommand { get; set; }

		private TracksListView tracksListView;
		private ClipControl clipControl;
		private TrackLinesView trackView;
		private RailsRuler ruler;
		private RailsPlayHead playHead;
		private TwoPanelsView split;
		private VisualElement events;
		private EventTrackLineView eventsTrackLine;
		private VisualElement eventsContainer;

		private VisualElement selectionBoxContainer;
		private VisualElement selectionBoxManipulatorLayer;
		private SelectionBoxDragManipulator selectionBoxManipulator;

		private static VisualTreeAsset templateLeft;
		private static VisualTreeAsset templateRight;

		private int duration = -1;
		private int timeHeadPosition = -1;
		private float timePosition;
		private float framePixelSize = 30;
		private bool isForwardMove;
		private bool isFastMove;
		private EditorCoroutine movingRoutine;
		private static readonly HashSet<int> keyFrames = new();


		static ClipView()
		{
			templateLeft = Resources.Load<VisualTreeAsset>("RailsSecondPage");
			templateRight = Resources.Load<VisualTreeAsset>("RailsThirdPage");
		}

		public ClipView()
		{
			split = new TwoPanelsView();
			split.style.width = new Length(100, LengthUnit.Percent);
			split.style.height = new Length(100, LengthUnit.Percent);
			split.style.flexGrow = 1;
			templateLeft.CloneTree(split.FirstPanel);
			templateRight.CloneTree(split.SecondPanel);
			split.FirstPanel.style.minWidth = 350;
			split.SecondPanel.style.minWidth = 200;

			split.FixedPaneInitialDimension = Storage.RecordsFloat.Get(FixedDimensionKey, 300);
			hierarchy.Add(split);

			clipControl = split.FirstPanel.Q<ClipControl>();
			tracksListView = split.FirstPanel.Q<TracksListView>();
			trackView = split.SecondPanel.Q<TrackLinesView>();
			ruler = split.SecondPanel.Q<RailsRuler>();
			playHead = split.SecondPanel.Q<RailsPlayHead>();

			events = split.SecondPanel.Q<VisualElement>("events");
			eventsTrackLine = events.Q<EventTrackLineView>();
			eventsContainer = events.Q<VisualElement>("events-container");
			selectionBoxContainer = events.Q<VisualElement>("selection-box-container");
			selectionBoxManipulatorLayer = events.Q<VisualElement>("selection-box-manipulator");
			selectionBoxManipulator = new SelectionBoxDragManipulator(selectionBoxManipulatorLayer);
			selectionBoxManipulatorLayer.AddManipulator(selectionBoxManipulator);

			SetBinding(nameof(Duration), new ToTargetBinding("SelectedClip.Duration"));
			SetBinding(nameof(TimeHeadPosition), new TwoWayBinding("SelectedClip.TimeHeadPosition"));
			SetBinding(nameof(RemoveSelectedKeysCommand), new CommandBinding("SelectedClip.RemoveSelectedKeysCommand"));
			ContextualMenuManipulator contextMenuTracks = new(ShowContextMenu);
			ContextualMenuManipulator contextMenuEvents = new(ShowContextMenu);
			events.AddManipulator(contextMenuTracks);
			trackView.AddManipulator(contextMenuEvents);
			focusable = true;
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			RegisterCallback<WheelEvent>(ScrollHandler, TrickleDown.TrickleDown);
			RegisterCallback<KeyUpEvent>(OnKeyClick, TrickleDown.TrickleDown);
			RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
			RegisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
			trackView.VerticalScroller.valueChanged += OnVerticalScroller;
			split.FixedPanelDimensionChanged += OnDimensionChanged;
			selectionBoxManipulator.SelectionBegin += OnSelectionBegin;
			selectionBoxManipulator.SelectionChanged += OnSelectionChanged;
			selectionBoxManipulator.SelectionComplete += OnSelectionComplete;
			EventBus.Subscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			EventBus.Subscribe<TimePositionChangedEvent>(OnTimePositionChanged);
			EventBus.Subscribe<KeyDragEvent>(OnKeyDragged);
			EventBus.Subscribe<PerformMove>(OnPerformMove);
			OnFramePixelSizeChanged(EditorContext.Instance.FramePixelSize);
			OnTimePositionChanged(EditorContext.Instance.TimePosition);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			UnregisterCallback<WheelEvent>(ScrollHandler, TrickleDown.TrickleDown);
			UnregisterCallback<KeyUpEvent>(OnKeyClick, TrickleDown.TrickleDown);
			UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
			UnregisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
			trackView.VerticalScroller.valueChanged -= OnVerticalScroller;
			split.FixedPanelDimensionChanged -= OnDimensionChanged;
			selectionBoxManipulator.SelectionBegin -= OnSelectionBegin;
			selectionBoxManipulator.SelectionChanged -= OnSelectionChanged;
			selectionBoxManipulator.SelectionComplete -= OnSelectionComplete;
			EventBus.Unsubscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			EventBus.Unsubscribe<TimePositionChangedEvent>(OnTimePositionChanged);
			EventBus.Unsubscribe<KeyDragEvent>(OnKeyDragged);
			EventBus.Unsubscribe<PerformMove>(OnPerformMove);
		}

		private void OnVerticalScroller(float value)
		{
			tracksListView.Scroll.verticalScroller.value = value;
		}

		private void ScrollHandler(WheelEvent evt)
		{
			float size = trackView.ScrollView.mouseWheelScrollSize;
			float y = evt.delta.y * ((trackView.VerticalScroller.lowValue < trackView.VerticalScroller.highValue) ? 1f : (-1f)) * size;
			float x = evt.delta.x * size;

			trackView.Scroll(new Vector2(x, y));
			tracksListView.Scroll.scrollOffset += new Vector2(0, y);

			evt.StopPropagation();
		}

		private void OnKeyClick(KeyUpEvent evt)
		{
			if (evt.keyCode is KeyCode.Delete or KeyCode.Backspace && HasAnySelected())
			{
				RemoveSelectedKeysCommand.Execute();
			}
		}

		private void OnKeyDown(KeyDownEvent evt)
		{
			if (evt.keyCode is KeyCode.Period && !evt.actionKey && !evt.altKey && movingRoutine == null)
			{
				movingRoutine = EditorCoroutineUtility.StartCoroutine(MovePlayHeadRoutine(true, DeltaNextFrame), EditorContext.Instance.Editor);
			}
			else if (evt.keyCode is KeyCode.Comma && !evt.actionKey && !evt.altKey && movingRoutine == null)
			{
				movingRoutine = EditorCoroutineUtility.StartCoroutine(MovePlayHeadRoutine(false, DeltaNextFrame), EditorContext.Instance.Editor);
			}
			if (evt.keyCode is KeyCode.Period && evt.actionKey && movingRoutine == null)
			{
				movingRoutine = EditorCoroutineUtility.StartCoroutine(MovePlayHeadRoutine(true, DeltaNextKeyFrame), EditorContext.Instance.Editor);
			}
			else if (evt.keyCode is KeyCode.Comma && evt.actionKey && movingRoutine == null)
			{
				movingRoutine = EditorCoroutineUtility.StartCoroutine(MovePlayHeadRoutine(false, DeltaNextKeyFrame), EditorContext.Instance.Editor);
			}
			if (evt.keyCode is KeyCode.Period && evt.altKey && movingRoutine == null)
			{
				movingRoutine = EditorCoroutineUtility.StartCoroutine(MoveSelectedKeysRoutine(true, DeltaNextFrame), EditorContext.Instance.Editor);
			}
			else if (evt.keyCode is KeyCode.Comma && evt.altKey && movingRoutine == null)
			{
				movingRoutine = EditorCoroutineUtility.StartCoroutine(MoveSelectedKeysRoutine(false, DeltaNextFrame), EditorContext.Instance.Editor);
			}
			else if (evt.shiftKey)
			{
				isFastMove = true;
			}
		}

		private void OnPerformMove(PerformMove evt)
		{
			int delta = evt.Mode switch
			{
				PerformMove.MoveMode.frame => DeltaNextFrame(evt.IsForward, false),
				PerformMove.MoveMode.frame10 => DeltaNextFrame(evt.IsForward, true),
				PerformMove.MoveMode.key => DeltaNextKeyFrame(evt.IsForward, false),
				PerformMove.MoveMode.startEnd => DeltaNextKeyFrame(evt.IsForward, true),
				_ => DeltaNextFrame(evt.IsForward, false),
			};
			if (evt.NeedMoveKey)
			{
				MoveSelectedKeys(delta);
				EventBus.Publish(new KeyDragCompleteEvent());
			}
			else
			{
				TimeHeadPosition += delta;
			}
		}

		private IEnumerator MovePlayHeadRoutine(bool isForward, Func<bool, bool, int> delta)
		{
			isForwardMove = isForward;
			while ((isForward && TimeHeadPosition < Duration) || (!isForward && TimeHeadPosition > 0))
			{
				TimeHeadPosition += delta(isForward, isFastMove);
				yield return new EditorWaitForSeconds(0.1f);
			}
			movingRoutine = null;
		}

		private IEnumerator MoveSelectedKeysRoutine(bool isForward, Func<bool, bool, int> delta)
		{
			isForwardMove = isForward;
			while (HasAnySelected())
			{
				MoveSelectedKeys(delta(isForward, isFastMove));
				EventBus.Publish(new KeyDragCompleteEvent());
				yield return new EditorWaitForSeconds(0.1f);
			}
			movingRoutine = null;
		}

		private int DeltaNextFrame(bool isForward, bool isFastMove)
		{
			int shift = !isFastMove ? 1 : 10;
			return isForward ? shift : -shift;
		}

		private int DeltaNextKeyFrame(bool isForward, bool isFastMove)
		{
			keyFrames.Clear();
			eventsTrackLine.KeyFrames.ForEach(x => keyFrames.Add(x));
			trackView.Views.ForEach(x => x.KeyFrames.ForEach(x => keyFrames.Add(x)));
			if (isForward && (keyFrames.Any(x => x > TimeHeadPosition) || isFastMove))
			{
				if (isFastMove)
					return Duration - TimeHeadPosition;
				else
					return keyFrames.Where(x => x > TimeHeadPosition).Min() - TimeHeadPosition;
			}
			else if (!isForward && (keyFrames.Any(x => x < TimeHeadPosition) || isFastMove))
			{
				if (isFastMove)
					return -TimeHeadPosition;
				else
					return keyFrames.Where(x => x < TimeHeadPosition).Max() - TimeHeadPosition;
			}
			else
				return 0;
		}

		private void OnKeyUp(KeyUpEvent evt)
		{
			if (evt.keyCode is KeyCode.Period && isForwardMove && movingRoutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(movingRoutine);
				movingRoutine = null;
			}
			else if (evt.keyCode is KeyCode.Comma && !isForwardMove && movingRoutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(movingRoutine);
				movingRoutine = null;
			}
			else if (evt.keyCode is KeyCode.LeftShift or KeyCode.RightShift)
			{
				isFastMove = false;
			}
		}

		private void OnDimensionChanged(float dimension)
		{
			Storage.RecordsFloat.Set(FixedDimensionKey, dimension);
		}

		private void OnSelectionBegin(Rect selectionRect, MouseDownEvent evt)
		{
			Rect selectionWorldRect = selectionBoxContainer.LocalToWorld(selectionRect);
			EventBus.Publish(new SelectionBoxBeginEvent(selectionWorldRect, evt.actionKey));
		}

		private void OnSelectionChanged(Rect selectionRect, MouseMoveEvent evt)
		{
			Rect selectionWorldRect = selectionBoxContainer.LocalToWorld(selectionRect);
			EventBus.Publish(new SelectionBoxChangeEvent(selectionWorldRect, evt.actionKey));

			Vector2 mousePosition = events.WorldToLocal(evt.mousePosition);
			if (!events.ContainsPoint(mousePosition))
			{
				Vector2 delta = new((mousePosition - events.layout.size).x, 0);
				trackView.Scroll(delta);
			}
		}

		private void OnSelectionComplete(Rect selectionRect, MouseUpEvent evt)
		{
			Rect selectionWorldRect = selectionBoxContainer.LocalToWorld(selectionRect);
			EventBus.Publish(new SelectionBoxCompleteEvent(selectionWorldRect, evt.actionKey));
		}

		private void OnTimePositionChanged(TimePositionChangedEvent evt)
		{
			OnTimePositionChanged(evt.TimePosition);
		}

		private void OnTimePositionChanged(float timePosition)
		{
			this.timePosition = timePosition;
			UpdatePosition();
		}

		private void OnFramePixelSizeChanged(FramePixelSizeChangedEvent evt)
		{
			OnFramePixelSizeChanged(evt.FramePixelSize);
		}

		private void OnFramePixelSizeChanged(float framePixelSize)
		{
			this.framePixelSize = framePixelSize;
			UpdatePosition();
			AdjustContainer(framePixelSize);
		}

		private void UpdatePosition()
		{
			eventsContainer.style.left = -framePixelSize * timePosition;
		}

		private void AdjustContainer(float framePixelSize)
		{
			eventsContainer.style.width = framePixelSize * Duration + TrackLinesView.StartAdditional;
		}

		private void OnKeyDragged(KeyDragEvent evt)
		{
			MoveSelectedKeys(evt.DragFrames);
		}

		private void MoveSelectedKeys(int deltaFrames)
		{
			foreach (var line in trackView.Views)
			{
				if (Check(line))
					return;
			}

			if (Check(eventsTrackLine))
				return;

			EventBus.Publish(new KeyMoveEvent(deltaFrames));

			bool Check<TKeyViewModel, TKey>(BaseTrackLineView<TKeyViewModel, TKey> line)
				where TKeyViewModel : BaseKeyViewModel<TKey>
				where TKey : IKey
			{
				if (line.SelectedKeysFrames.IsNullOrEmpty())
					return false;
				if (line.FirstSelectedKeyFrame + deltaFrames < 0
					|| line.LastSelectedKeyFrame + deltaFrames > Duration)
					return true;
				return false;
			}
		}

		private void ShowContextMenu(ContextualMenuPopulateEvent evt)
		{
			if (HasAnySelected())
			{
				evt.menu.AppendAction("Remove", x =>
				{
					RemoveSelectedKeysCommand.Execute();
				}, DropdownMenuAction.Status.Normal);
			}

			AddKeyAction(eventsTrackLine);
			trackView.Views.ForEach(AddKeyAction);

			void AddKeyAction<TKeyViewModel, TKey>(BaseTrackLineView<TKeyViewModel, TKey> track)
				where TKeyViewModel : BaseKeyViewModel<TKey>
				where TKey : IKey
			{
				Vector2 mousePosition = track.parent.WorldToLocal(evt.mousePosition);
				if (track.layout.Contains(mousePosition))
				{
					float globalPixelsPosition = trackView.WorldToLocal(evt.mousePosition).x - TrackLinesView.StartAdditional + timePosition * framePixelSize;
					int frames = Mathf.RoundToInt(globalPixelsPosition / framePixelSize);
					if (!track.Values.Any(x => x.TimePosition == frames))
					{
						evt.menu.AppendAction("Add Key", x =>
						{
							track.AddKey(frames);
						}, DropdownMenuAction.Status.Normal);
					}
				}
			}
		}

		private bool HasAnySelected()
		{
			if (!eventsTrackLine.SelectedIndexes.IsNullOrEmpty())
				return true;
			foreach (var line in trackView.Views)
			{
				if (!line.SelectedIndexes.IsNullOrEmpty())
					return true;
			}
			return false;
		}
	}
}