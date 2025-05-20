using System;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackLinesView : ListObserverElement<AnimationTrackViewModel, TrackLine>
	{
		private const int additional = 60;
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
				FramePixelSizeChanged?.Invoke(framePixelSize);
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
		public event Action<float> FramePixelSizeChanged;
		public event Action<float> TimePositionChanged;

		private static VisualTreeAsset templateMain;
		private ScrollView scrollView;
		private Scroller verticalScroller;
		private MinMaxSlider slider;
		private VisualElement viewport;
		private int duration = -1;
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
			container = scrollView.Q<VisualElement>("tracks-container");

			viewport = scrollView.contentViewport;
			viewport.RegisterCallback<GeometryChangedEvent>(x =>
			{
				AdjustFramePixelSize();
			});
			slider.RegisterCallback<ChangeEvent<Vector2>>(SliderChangedHandler);
		}

		protected override TrackLine CreateElement()
		{
			return new TrackLine();
		}

		protected override void ResetElement(TrackLine element)
		{

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
				TimePositionChanged?.Invoke(value.x);

			float position = math.remap(
				slider.lowLimit, slider.highLimit - delta,
				0, maxOffset,
				value.x);
			if (!Mathf.Approximately(container.layout.position.x, position))
				container.style.left = -position;
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

		private void AdjustContainer(float frameSize)
		{
			containerSize = duration * frameSize + additional;
			container.style.width = containerSize;
		}

		private void AdjustFramePixelSize()
		{
			FramePixelSize = (scrollView.contentViewport.contentRect.width - additional) / currentDelta;
			AdjustContainer(FramePixelSize);
		}
	}
}