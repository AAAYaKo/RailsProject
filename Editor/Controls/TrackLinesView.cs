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

		public ScrollView Scroll => scrollView;
		public Scroller VerticalScroller => verticalScroller;
		public Scroller HorizontalScroller => horizontalScroller;
		public MinMaxSlider Slider => slider;
		public event Action<float> FramePixelSizeChanged;
		public event Action<float> TimePositionChanged;

		private static VisualTreeAsset templateMain;
		private ScrollView scrollView;
		private Scroller horizontalScroller;
		private Scroller verticalScroller;
		private MinMaxSlider slider;
		private int duration = -1;
		private bool? canEdit;
		private float currentDelta = float.NaN;
		private float framePixelSize = 30;


		public TrackLinesView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsTrackView");
			templateMain.CloneTree(this);
			scrollView = this.Q<ScrollView>();
			horizontalScroller = scrollView.horizontalScroller;
			verticalScroller = scrollView.verticalScroller;
			slider = this.Q<MinMaxSlider>();
			slider.lowLimit = 0;
			container = scrollView.Q<VisualElement>("tracks-container");

			scrollView.contentViewport.RegisterCallback<GeometryChangedEvent>(x =>
			{
				AdjustFramePixelSize();
			});
			slider.RegisterCallback<ChangeEvent<Vector2>>(SliderChangedHandler);
			horizontalScroller.valueChanged += HorizontalScrollerValueChangedHandler;
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
				horizontalScroller.lowValue, horizontalScroller.highValue,
				value.x);
			if (!Mathf.Approximately(horizontalScroller.value, position))
				horizontalScroller.slider.SetValueWithoutNotify(position);
		}

		private void HorizontalScrollerValueChangedHandler(float value)
		{
			if (float.IsNaN(value) || Mathf.Approximately(horizontalScroller.lowValue, horizontalScroller.highValue))
				return;
			float position = math.remap(
				horizontalScroller.lowValue, horizontalScroller.highValue,
				slider.lowLimit, slider.highLimit - currentDelta,
				value);
			if (Mathf.Approximately(slider.value.x, position))
				return;
			slider.value = new Vector2(position, position + currentDelta);
		}

		private void AdjustContainer(float frameSize)
		{
			container.style.width = duration * frameSize + additional;
		}

		private void AdjustFramePixelSize()
		{
			FramePixelSize = (scrollView.contentViewport.contentRect.width - additional) / currentDelta;
			AdjustContainer(FramePixelSize);
		}
	}
}