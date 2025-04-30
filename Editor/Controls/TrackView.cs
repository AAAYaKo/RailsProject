using System;
using System.Collections.Generic;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackView : ListObserverElement<AnimationTrackViewModel, TrackLine>
	{
		private const int additional = 30;
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
					slider.minValue = 0;
					slider.maxValue = duration;
					currentDelta = duration;
					AdjustFramePixelSize();
				}
				else if (currentDelta > duration)
				{
					currentDelta = duration;
					AdjustFramePixelSize();
				}
				else if (currentDelta < 2 && duration >= 2)
				{
					currentDelta = 2;
					slider.maxValue = slider.minValue + 2;
					AdjustFramePixelSize();
				}
				else
				{
					AdjustContainer(EditorContext.Instance.FramePixelSize);
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
			}
		}

		private static VisualTreeAsset templateMain;
		private ScrollView scrollView;
		private Scroller horizontalScroller;
		private MinMaxSlider slider;
		private int duration = -1;
		private bool? canEdit;
		private float currentDelta = float.NaN;


		public TrackView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsTrackView");
			templateMain.CloneTree(this);
			scrollView = this.Q<ScrollView>();
			horizontalScroller = scrollView.horizontalScroller;
			slider = this.Q<MinMaxSlider>();
			slider.lowLimit = 0;
			container = scrollView.Q<VisualElement>("tracks-container");

			RegisterCallback<WheelEvent>(ScrollHandler, TrickleDown.TrickleDown);
			RegisterCallback<AttachToPanelEvent>(x =>
			{
				EditorContext.Instance.TrackScrollPerformed += ScrollPerformedHandler;
				EditorContext.Instance.FramePixelSizeChanged += FramePixelSizeChangedHandler;
			});
			RegisterCallback<DetachFromPanelEvent>(x =>
			{
				EditorContext.Instance.TrackScrollPerformed -= ScrollPerformedHandler;
				EditorContext.Instance.FramePixelSizeChanged -= FramePixelSizeChangedHandler;
			});
			scrollView.contentViewport.RegisterCallback<GeometryChangedEvent>(x =>
			{
				AdjustFramePixelSize();
			});
			slider.RegisterCallback<ChangeEvent<Vector2>>(SliderChangedHandler);
			horizontalScroller.valueChanged += ScrollerValueChangedHandler;
		}

		protected override TrackLine CreateElement()
		{
			return new TrackLine();
		}

		protected override void ResetElement(TrackLine element)
		{

		}

		private void ScrollPerformedHandler(Vector2 delta)
		{
			scrollView.scrollOffset += delta;
		}

		private void ScrollHandler(WheelEvent evt)
		{
			float num2 = scrollView.mouseWheelScrollSize;
			float y = evt.delta.y * ((scrollView.verticalScroller.lowValue < scrollView.verticalScroller.highValue) ? 1f : (-1f)) * num2;
			float x = evt.delta.x * ((scrollView.horizontalScroller.lowValue < scrollView.horizontalScroller.highValue) ? 1f : (-1f)) * num2;

			EditorContext.Instance.PerformTrackScroll(new Vector2(x, y));

			evt.StopPropagation();
		}

		private void SliderChangedHandler(ChangeEvent<Vector2> evt)
		{
			float delta = evt.newValue.y - evt.newValue.x;
			Vector2 value = evt.newValue;
			if (delta < 2)
			{
				value = Mathf.Approximately(evt.newValue.x, evt.previousValue.x) ?
				new(evt.newValue.x, evt.newValue.x + 2) : new(evt.newValue.y - 2, evt.newValue.y);
				slider.SetValueWithoutNotify(value);
			}
			if (Utils.Approximately(value, evt.previousValue))
				return;
			float position = math.remap(
				slider.lowLimit, slider.highLimit - delta,
				horizontalScroller.lowValue, horizontalScroller.highValue,
				value.x);
			if (!Mathf.Approximately(horizontalScroller.value, position))
				horizontalScroller.value = position;

			if (!Mathf.Approximately(delta, currentDelta))
			{
				currentDelta = delta;
				AdjustFramePixelSize();
			}
		}

		private void ScrollerValueChangedHandler(float value)
		{
			float position = math.remap(
				horizontalScroller.lowValue, horizontalScroller.highValue,
				slider.lowLimit, slider.highLimit - currentDelta,
				value);
			if (Mathf.Approximately(slider.value.x, position))
				return;
			slider.minValue = position;
			slider.maxValue = position + currentDelta;
		}

		private void AdjustContainer(float frameSize)
		{
			container.style.width = duration * frameSize + additional;
		}

		private void FramePixelSizeChangedHandler(float frameSize)
		{
			AdjustContainer(frameSize);
		}

		private void AdjustFramePixelSize()
		{
			EditorContext.Instance.FramePixelSize = (scrollView.contentViewport.contentRect.width - additional) / currentDelta;
		}
	}
}