using System.Linq;
using Rails.Editor.Manipulator;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class ClipView : FocusableView
	{
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
		private float timePosition;
		private float framePixelSize = 30;


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
			trackView.VerticalScroller.valueChanged += OnVerticalScroller;
			split.FixedPanelDimensionChanged += OnDimensionChanged;
			selectionBoxManipulator.SelectionBegin += OnSelectionBegin;
			selectionBoxManipulator.SelectionChanged += OnSelectionChanged;
			selectionBoxManipulator.SelectionComplete += OnSelectionComplete;
			EventBus.Subscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			EventBus.Subscribe<TimePositionChangedEvent>(OnTimePositionChanged);
			EventBus.Subscribe<KeyDragEvent>(OnKeyDragged);
			OnFramePixelSizeChanged(EditorContext.Instance.FramePixelSize);
			OnTimePositionChanged(EditorContext.Instance.TimePosition);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			UnregisterCallback<WheelEvent>(ScrollHandler, TrickleDown.TrickleDown);
			UnregisterCallback<KeyUpEvent>(OnKeyClick, TrickleDown.TrickleDown);
			trackView.VerticalScroller.valueChanged -= OnVerticalScroller;
			split.FixedPanelDimensionChanged -= OnDimensionChanged;
			selectionBoxManipulator.SelectionBegin -= OnSelectionBegin;
			selectionBoxManipulator.SelectionChanged -= OnSelectionChanged;
			selectionBoxManipulator.SelectionComplete -= OnSelectionComplete;
			EventBus.Unsubscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			EventBus.Unsubscribe<TimePositionChangedEvent>(OnTimePositionChanged);
			EventBus.Unsubscribe<KeyDragEvent>(OnKeyDragged);
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
			int deltaFrames = evt.DragFrames;
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
				where TKey : BaseKey
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
				where TKey : BaseKey
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