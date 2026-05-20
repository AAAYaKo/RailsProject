using System;
using System.Collections.Generic;
using System.Linq;
using Rails.Editor.Context;
using Rails.Editor.Manipulator;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.Mathematics;
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

		public const int EndAdditional = 60;
		public const int StartAdditional = 10;

		[UxmlAttribute("duration"), CreateProperty]
		public int Duration
		{
			get => duration;
			set
			{
				if (duration == value)
					return;
				duration = value;
				slider.highLimit = duration;
				slider.enabledSelf = duration > 2;
				if (float.IsNaN(currentDelta))
				{
					slider.value = new Vector2(0, duration);
					CurrentDelta = duration;
				}
				else if (currentDelta > duration)
				{
					slider.value = new Vector2(0, duration);
					CurrentDelta = duration;
				}
				else if (currentDelta < 2 && duration >= 2)
				{
					slider.value = new Vector2(0, duration);
					CurrentDelta = duration;
				}
				else
				{
					AdjustContainer(FramePixelSize);
				}
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
		[UxmlAttribute("is-graph"), CreateProperty]
		public bool IsGraph
		{
			get => isGraph ?? false;
			set
			{
				if (isGraph == value)
					return;
				isGraph = value;
				//graphView.style.display = IsGraph? DisplayStyle.Flex : DisplayStyle.None;
				trackView.style.display = IsGraph ? DisplayStyle.None : DisplayStyle.Flex;
			}
		}
		[UxmlAttribute("can-edit"), CreateProperty]
		public bool CanEdit
		{
			get => canEdit ?? false;
			set
			{
				if (canEdit == value)
					return;
				canEdit = value;
				slider.enabledSelf = canEdit.Value;
			}
		}
		[CreateProperty]
		public ICommand RemoveSelectedKeysCommand { get; set; }

		public float FramePixelSize
		{
			get => framePixelSize;
			set
			{
				if (framePixelSize == value)
					return;
				framePixelSize = value;

				EventBus.Publish(new FramePixelSizeChangedEvent(framePixelSize));
			}
		}
		public float CurrentDelta
		{
			get => currentDelta;
			set
			{
				if (currentDelta == value)
					return;
				currentDelta = value;
				AdjustFramePixelSize();
			}
		}

		public MinMaxSlider Slider => slider;

		private TracksListView tracksListView;
		private ClipControl clipControl;
		private TrackLinesView trackView;
		private GraphView graphView;
		private RailsRuler ruler;
		private RailsPlayHead playHead;
		private TwoPanelsView split;
		private VisualElement events;
		private EventTrackLineView eventsTrackLine;
		private VisualElement eventsContainer;

		private ScrollView scrollView;
		private Scroller verticalScroller;
		private MinMaxSlider slider;
		private VisualElement keysContainer;
		private VisualElement viewport;

		private VisualElement selectionBoxContainer;
		private VisualElement selectionBoxManipulatorLayer;
		private SelectionBoxDragManipulator selectionBoxManipulator;

		private static VisualTreeAsset templateLeft;
		private static VisualTreeAsset templateRight;

		private int duration = -1;
		private int timeHeadPosition = -1;
		private float timePosition;
		private bool? canEdit;
		private float framePixelSize = 30;
		private float currentDelta = float.NaN;
		private float containerSize = 0;
		private bool isForwardMove;
		private bool isFastMove;
		private bool? isGraph;
		private Vector2 currentMousePosition;
		private IVisualElementScheduledItem moveWork;
		private IVisualElementScheduledItem mouseScroll;
		private static readonly HashSet<int> keyFrames = new();
		private float maxOffset => containerSize - viewport.layout.width;


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
			//graphView = split.SecondPanel.Q<GraphView>();
			ruler = split.SecondPanel.Q<RailsRuler>();
			playHead = split.SecondPanel.Q<RailsPlayHead>();

			scrollView = this.Q<ScrollView>("keys-scroll");
			verticalScroller = scrollView.verticalScroller;
			slider = this.Q<MinMaxSlider>();
			slider.lowLimit = 0;
			keysContainer = scrollView.Q<VisualElement>("content-container");
			viewport = scrollView.contentViewport;

			events = split.SecondPanel.Q<VisualElement>("events");
			eventsTrackLine = events.Q<EventTrackLineView>();
			eventsContainer = events.Q<VisualElement>("events-container");
			selectionBoxContainer = events.Q<VisualElement>("selection-box-container");
			selectionBoxManipulatorLayer = events.Q<VisualElement>("selection-box-manipulator");
			selectionBoxManipulator = new SelectionBoxDragManipulator(selectionBoxManipulatorLayer);
			selectionBoxManipulatorLayer.AddManipulator(selectionBoxManipulator);

			SetBinding(nameof(Duration), new ToTargetBinding("SelectedClip.Duration"));
			SetBinding(nameof(TimeHeadPosition), new TwoWayBinding("SelectedClip.TimeHeadPosition"));
			SetBinding(nameof(IsGraph), new ToTargetBinding("SelectedClip.IsGraph"));
			SetBinding(nameof(RemoveSelectedKeysCommand), new CommandBinding("SelectedClip.RemoveSelectedKeysCommand"));
			SetBinding(nameof(CanEdit), new ToTargetBinding("SelectedClip.CanEdit"));
			ContextualMenuManipulator contextMenuTracks = new(ShowContextMenu);
			ContextualMenuManipulator contextMenuEvents = new(ShowContextMenu);
			events.AddManipulator(contextMenuTracks);
			trackView.AddManipulator(contextMenuEvents);
			focusable = true;
			trackView.Viewport = viewport;
			//graphView.style.display = DisplayStyle.None;
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			RegisterCallback<WheelEvent>(ScrollHandler, TrickleDown.TrickleDown);
			RegisterCallback<KeyUpEvent>(OnKeyClick, TrickleDown.TrickleDown);
			RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
			RegisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
			viewport.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			slider.RegisterCallback<ChangeEvent<Vector2>>(SliderChangedHandler);
			trackView.DoubleClicked += OnDoubleClick;
			trackView.ScrollPerformed += ScrollPerformed;
			verticalScroller.valueChanged += OnVerticalScroller;
			split.FixedPanelDimensionChanged += OnDimensionChanged;
			selectionBoxManipulator.SelectionBegin += OnSelectionBegin;
			selectionBoxManipulator.SelectionChanged += OnSelectionChanged;
			selectionBoxManipulator.SelectionComplete += OnSelectionComplete;
			EventBus.Subscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			EventBus.Subscribe<TimePositionChangedEvent>(OnTimePositionChanged);
			EventBus.Subscribe<KeyDragChangedEvent>(OnKeyDragged);
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
			viewport.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			slider.RegisterCallback<ChangeEvent<Vector2>>(SliderChangedHandler);
			trackView.DoubleClicked -= OnDoubleClick;
			trackView.ScrollPerformed -= ScrollPerformed;
			verticalScroller.valueChanged -= OnVerticalScroller;
			split.FixedPanelDimensionChanged -= OnDimensionChanged;
			selectionBoxManipulator.SelectionBegin -= OnSelectionBegin;
			selectionBoxManipulator.SelectionChanged -= OnSelectionChanged;
			selectionBoxManipulator.SelectionComplete -= OnSelectionComplete;
			EventBus.Unsubscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			EventBus.Unsubscribe<TimePositionChangedEvent>(OnTimePositionChanged);
			EventBus.Unsubscribe<KeyDragChangedEvent>(OnKeyDragged);
			EventBus.Unsubscribe<PerformMove>(OnPerformMove);
		}

		private void ScrollPerformed(Vector2 delta)
		{
			Scroll(delta);
		}

		private void OnVerticalScroller(float value)
		{
			tracksListView.Scroll.verticalScroller.value = value;
		}

		private void AdjustFramePixelSize()
		{
			float width = scrollView.contentViewport.contentRect.width;
			if (width == float.NaN || width == 0)
				return;
			FramePixelSize = (scrollView.contentViewport.contentRect.width - EndAdditional - StartAdditional) / currentDelta;
			AdjustContainer(FramePixelSize);
		}

		private void OnDoubleClick(Vector2 localPosition)
		{
			float x = localPosition.x;
			float globalPixelsPosition = x - StartAdditional + slider.value.x * framePixelSize;
			int frames = Mathf.RoundToInt(globalPixelsPosition / framePixelSize);
			TimeHeadPosition = frames;
		}

		private void OnGeometryChanged(GeometryChangedEvent evt)
		{
			AdjustFramePixelSize();
		}

		private void ScrollHandler(WheelEvent evt)
		{
			 Scroll(evt.delta);

			evt.StopPropagation();
		}

		private void Scroll(Vector2 delta)
		{
			float size = scrollView.mouseWheelScrollSize;
			float y = delta.y * ((verticalScroller.lowValue < verticalScroller.highValue) ? 1f : (-1f)) * size;
			float x = delta.x * size;

			tracksListView.Scroll.scrollOffset += new Vector2(0, y);
			scrollView.scrollOffset += new Vector2(0, y);

			if (maxOffset <= 0)
				return;

			float positionDelta = math.remap(
				0, maxOffset,
				slider.lowLimit, slider.highLimit - currentDelta,
				x);
			if (slider.minValue + positionDelta < slider.lowLimit)
				positionDelta = slider.lowLimit - slider.minValue;
			else if (slider.maxValue + positionDelta > slider.highLimit)
				positionDelta = slider.highLimit - slider.maxValue;
			if (Mathf.Approximately(0, positionDelta))
				return;
			slider.value += new Vector2(positionDelta, positionDelta);
		}

		private void OnKeyClick(KeyUpEvent evt)
		{
			if (evt.keyCode is KeyCode.Delete or KeyCode.Backspace && HasAnySelected())
			{
				RemoveSelectedKeysCommand.Execute();
			}
		}

		private void SliderChangedHandler(ChangeEvent<Vector2> evt)
		{
			Vector2 value = evt.newValue;
			float delta = value.y - value.x;
			if (delta < 2)
			{
				value = Mathf.Approximately(value.x, evt.previousValue.x) ?
				new(value.x, value.x + 2) : new(value.y - 2, value.y);
				slider.SetValueWithoutNotify(value);
				delta = 2;
			}
			if (Utils.Approximately(value, evt.previousValue))
				return;

			if (!Mathf.Approximately(delta, currentDelta))
				CurrentDelta = delta;

			if (evt.previousValue.x != value.x)
				EventBus.Publish(new TimePositionChangedEvent(value.x));

			float position = math.remap(
				slider.lowLimit, slider.highLimit - delta,
				0, maxOffset,
				value.x);
			if (!Mathf.Approximately(keysContainer.layout.position.x, position))
				keysContainer.style.left = -position;
		}

		private void OnKeyDown(KeyDownEvent evt)
		{
			if (evt.keyCode is KeyCode.Period && !evt.actionKey && !evt.altKey && (!moveWork?.isActive ?? true))
			{
				MovePlayHeadSchedule(true, DeltaNextFrame);
			}
			else if (evt.keyCode is KeyCode.Comma && !evt.actionKey && !evt.altKey && (!moveWork?.isActive ?? true))
			{
				MovePlayHeadSchedule(false, DeltaNextFrame);
			}
			else if (evt.keyCode is KeyCode.Period && evt.actionKey && (!moveWork?.isActive ?? true))
			{
				MovePlayHeadSchedule(true, DeltaNextKeyFrame);
			}
			else if (evt.keyCode is KeyCode.Comma && evt.actionKey && (!moveWork?.isActive ?? true))
			{
				MovePlayHeadSchedule(false, DeltaNextKeyFrame);
			}
			else if (evt.keyCode is KeyCode.Period && evt.altKey && (!moveWork?.isActive ?? true))
			{
				MoveSelectedSchedule(true, DeltaNextKeyFrame);
			}
			else if (evt.keyCode is KeyCode.Comma && evt.altKey && (!moveWork?.isActive ?? true))
			{
				MoveSelectedSchedule(false, DeltaNextKeyFrame);
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

		private void MovePlayHeadSchedule(bool isForward, Func<bool, bool, int> delta)
		{
			isForwardMove = isForward;
			if (CanMove())
				TimeHeadPosition += delta(isForward, isFastMove);

			moveWork = schedule
				.Execute(() => TimeHeadPosition += delta(isForward, isFastMove))
				.Every(50)
				.Until(() => !CanMove())
				.StartingIn(150);

			bool CanMove() => (isForward && TimeHeadPosition < Duration) || (!isForward && TimeHeadPosition > 0);
		}

		private void MoveSelectedSchedule(bool isForward, Func<bool, bool, int> delta)
		{
			isForwardMove = isForward;
			if (HasAnySelected())
				Move();

			moveWork = schedule
				.Execute(Move)
				.Every(50)
				.Until(() => !HasAnySelected())
				.StartingIn(150);

			void Move()
			{
				MoveSelectedKeys(delta(isForward, isFastMove));
				EventBus.Publish(new KeyDragCompleteEvent());
			}
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
			if (evt.keyCode is KeyCode.Period && isForwardMove && moveWork != null)
			{
				moveWork.Pause();
				moveWork = null;
			}
			else if (evt.keyCode is KeyCode.Comma && !isForwardMove && moveWork != null)
			{
				moveWork.Pause();
				moveWork = null;
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
			containerSize = StartAdditional + duration * framePixelSize + EndAdditional;
			keysContainer.style.width = containerSize;
			eventsContainer.style.width = containerSize;
		}

		private void OnKeyDragged(KeyDragChangedEvent evt)
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
					float globalPixelsPosition = trackView.WorldToLocal(evt.mousePosition).x - StartAdditional + timePosition * framePixelSize;
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