using System.Linq;
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
	public partial class TrackLinesView : ListObserverElement<AnimationTrackViewModel, TrackLineView>
	{
		public static readonly BindingId TimeHeadPositionProperty = nameof(TimeHeadPosition);

		public const int EndAdditional = 60;
		public const int StartAdditional = 10;
		public static readonly BindingId DurationProperty = nameof(Duration);

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
					slider.maxValue = slider.minValue + 2;
					CurrentDelta = 2;
				}
				else
				{
					AdjustContainer(FramePixelSize);
				}
				NotifyPropertyChanged(DurationProperty);
			}
		}
		[UxmlAttribute("timePosition"), CreateProperty]
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

		public ScrollView ScrollView => scrollView;
		public Scroller VerticalScroller => verticalScroller;
		public MinMaxSlider Slider => slider;

		private static VisualTreeAsset templateMain;
		private ScrollView scrollView;
		private Scroller verticalScroller;
		private MinMaxSlider slider;
		private new VisualElement contentContainer;
		private VisualElement viewport;
		private VisualElement tracksBackgroundContainer;
		private VisualElement selectionBoxContainer;
		private VisualElement selectionBoxManipulatorLayer;
		private SelectionBoxDragManipulator selectionBoxManipulator;
		private int duration = -1;
		private int timeHeadPosition;
		private bool? canEdit;
		private float currentDelta = float.NaN;
		private float framePixelSize = 30;
		private float containerSize = 0;
		private float maxOffset => containerSize - viewport.layout.width;


		public TrackLinesView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsTrackView");
			templateMain.CloneTree(this);
			scrollView = this.Q<ScrollView>();
			verticalScroller = scrollView.verticalScroller;
			slider = this.Q<MinMaxSlider>();
			slider.lowLimit = 0;
			tracksBackgroundContainer = scrollView.Q<VisualElement>("tracks-back");
			contentContainer = scrollView.Q<VisualElement>("content-container");
			container = scrollView.Q<VisualElement>("tracks-container");
			selectionBoxContainer = scrollView.Q<VisualElement>("selection-box-container");
			selectionBoxManipulatorLayer = scrollView.Q<VisualElement>("selection-box-manipulator");

			viewport = scrollView.contentViewport;

			selectionBoxManipulator = new SelectionBoxDragManipulator(selectionBoxContainer);
			selectionBoxManipulatorLayer.AddManipulator(selectionBoxManipulator);
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			viewport.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			slider.RegisterCallback<ChangeEvent<Vector2>>(SliderChangedHandler);
			selectionBoxManipulatorLayer.RegisterCallback<ClickEvent>(OnMouseClick);
			selectionBoxManipulator.SelectionBegin += OnSelectionBegin;
			selectionBoxManipulator.SelectionChanged += OnSelectionChanged;
			selectionBoxManipulator.SelectionComplete += OnSelectionComplete;
			EventBus.Subscribe<KeyDragEvent>(OnKeyDragged);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			viewport.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			slider.UnregisterCallback<ChangeEvent<Vector2>>(SliderChangedHandler);
			selectionBoxManipulatorLayer.UnregisterCallback<ClickEvent>(OnMouseClick);
			selectionBoxManipulator.SelectionBegin -= OnSelectionBegin;
			selectionBoxManipulator.SelectionChanged -= OnSelectionChanged;
			selectionBoxManipulator.SelectionComplete -= OnSelectionComplete;
			EventBus.Unsubscribe<KeyDragEvent>(OnKeyDragged);
		}

		public void Scroll(Vector2 delta)
		{
			scrollView.scrollOffset += new Vector2(0, delta.y);
			if (maxOffset <= 0)
				return;

			float positionDelta = math.remap(
				0, maxOffset,
				slider.lowLimit, slider.highLimit - currentDelta,
				delta.x);
			if (slider.minValue + positionDelta < slider.lowLimit)
				positionDelta = slider.lowLimit - slider.minValue;
			else if (slider.maxValue + positionDelta > slider.highLimit)
				positionDelta = slider.highLimit - slider.maxValue;
			if (Mathf.Approximately(0, positionDelta))
				return;
			slider.value += new Vector2(positionDelta, positionDelta);
		}

		protected override TrackLineView CreateElement()
		{
			TrackLineView line = new();
			VisualElement trackBack = new();
			trackBack.AddToClassList("track-line-background");
			tracksBackgroundContainer.Add(trackBack);
			return line;
		}

		protected override void ResetElement(TrackLineView element)
		{
			tracksBackgroundContainer.RemoveAt(0);
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
			if (!Mathf.Approximately(contentContainer.layout.position.x, position))
				contentContainer.style.left = -position;
		}

		private void OnGeometryChanged(GeometryChangedEvent evt)
		{
			AdjustFramePixelSize();
		}

		private void AdjustContainer(float frameSize)
		{
			containerSize = StartAdditional + duration * frameSize + EndAdditional;
			contentContainer.style.width = containerSize;
		}

		private void AdjustFramePixelSize()
		{
			FramePixelSize = (scrollView.contentViewport.contentRect.width - EndAdditional - StartAdditional) / currentDelta;
			AdjustContainer(FramePixelSize);
		}

		private void OnMouseClick(ClickEvent evt)
		{
			if (evt.button == 0 && evt.clickCount == 2)
			{
				float x = evt.localPosition.x;
				float globalPixelsPosition = x - TrackLinesView.StartAdditional + slider.value.x * framePixelSize;
				int frames = Mathf.RoundToInt(globalPixelsPosition / framePixelSize);
				TimeHeadPosition = frames;
			}
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

			Vector2 mousePosition = viewport.WorldToLocal(evt.mousePosition);
			if (!viewport.ContainsPoint(mousePosition))
			{
				Vector2 delta = mousePosition - viewport.layout.size;
				Scroll(delta);
			}
		}

		private void OnSelectionComplete(Rect selectionRect, MouseUpEvent evt)
		{
			Rect selectionWorldRect = selectionBoxContainer.LocalToWorld(selectionRect);
			EventBus.Publish(new SelectionBoxCompleteEvent(selectionWorldRect, evt.actionKey));
		}

		private void OnKeyDragged(KeyDragEvent evt)
		{
			int deltaFrames = evt.DragFrames;
			foreach (var line in views)
			{
				if (line.SelectedKeysFrames.IsNullOrEmpty())
					continue;
				if (line.FirstSelectedKeyFrame + deltaFrames < 0
					|| line.LastSelectedKeyFrame + deltaFrames > Duration)
					return;
			}

			EventBus.Publish(new KeyMoveEvent(deltaFrames));
		}
	}
}