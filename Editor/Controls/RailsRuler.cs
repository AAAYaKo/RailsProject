using System.Collections.Generic;
using Rails.Editor.Manipulator;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class RailsRuler : BaseView
	{
		public static readonly BindingId TimeHeadPositionProperty = nameof(TimeHeadPosition);

		private const float MinStepSize = 120;
		private const float MaxStepSize = 240;

		[UxmlAttribute("duration"), CreateProperty]
		public int Duration
		{
			get => duration;
			set
			{
				if (duration == value)
					return;
				duration = value;
				Repaint();
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

		private float framePixelSize = 30;
		private float stepSize => framePixelSize * stepFrames;
		private int stepFrames = 0;
		private int fps = RailsClip.Fps;
		private float timePosition;
		private int duration;
		private int timeHeadPosition;
		private List<RulerStep> stepList = new();
		private Stack<RulerStep> pool = new();


		public RailsRuler()
		{
			AddToClassList("ruler");
			this.AddManipulator(new RulerDragManipulator(x =>
			{
				float globalPixelsPosition = x - TrackLinesView.StartAdditional + timePosition * framePixelSize;
				int frames = Mathf.RoundToInt(globalPixelsPosition / framePixelSize);
				TimeHeadPosition = frames;
			}));
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			EventBus.Subscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			EventBus.Subscribe<TimePositionChangedEvent>(OnTimePositionChanged);
			OnFramePixelSizeChanged(EditorContext.Instance.FramePixelSize);
			OnTimePositionChanged(EditorContext.Instance.TimePosition);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			EventBus.Unsubscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			EventBus.Unsubscribe<TimePositionChangedEvent>(OnTimePositionChanged);
		}

		private void OnFramePixelSizeChanged(FramePixelSizeChangedEvent evt)
		{
			OnFramePixelSizeChanged(evt.FramePixelSize);
		}

		private void OnFramePixelSizeChanged(float framePixelSize)
		{
			float length = layout.width;
			this.framePixelSize = framePixelSize;
			while (stepSize < MinStepSize && stepSize < length)
			{
				stepFrames = stepFrames switch
				{
					< 2 => stepFrames + 1,
					< 5 => stepFrames + 3,
					< 20 => stepFrames + 5,
					< 30 => stepFrames + 10,
					< 60 => stepFrames + 30,
					_ => stepFrames + 60,
				};
			}

			while (stepSize > MaxStepSize && stepFrames > 0)
			{
				stepFrames = stepFrames switch
				{
					> 60 => stepFrames - 60,
					> 30 => stepFrames - 30,
					> 20 => stepFrames - 10,
					> 5 => stepFrames - 5,
					> 2 => stepFrames - 3,
					_ => stepFrames - 1,
				};
				if (stepFrames <= 0)
				{
					stepFrames = 1;
					break;
				}
			}
			Repaint();
		}

		private void OnTimePositionChanged(TimePositionChangedEvent evt)
		{
			OnTimePositionChanged(evt.TimePosition);
		}

		private void OnTimePositionChanged(float timePosition)
		{
			this.timePosition = timePosition;
			Repaint();
		}

		private void Repaint()
		{
			if (stepFrames <= 0)
				return;
			float length = layout.width;
			int firstFrame = Mathf.FloorToInt(timePosition / stepFrames) * stepFrames;

			int currentFrame = firstFrame - stepFrames;
			float shift = -timePosition * framePixelSize + TrackLinesView.StartAdditional;
			float currenShift = shift;
			int i = 0;
			bool hasExtra = false;

			for (; i < stepList.Count; i++)
			{
				var step = stepList[i];
				bool isOverflow = InitStep(step);

				if (currenShift > length && i < stepList.Count - 1 || isOverflow)
				{
					hasExtra = true;
					break;
				}
			}
			if (hasExtra)
			{
				int countToRemove = stepList.Count - i;
				for (int j = 0; j < countToRemove; j++)
				{
					pool.Push(stepList[^1]);
					stepList[^1].style.display = DisplayStyle.None;
					stepList.RemoveAt(stepList.Count - 1);
				}
			}
			while (currenShift < length)
			{
				if (currentFrame == Duration)
					break;
				var step = GetNextStep();
				InitStep(step);
				step.style.display = DisplayStyle.Flex;

				stepList.Add(step);
			}

			bool InitStep(RulerStep step)
			{
				bool result = false;
				currentFrame += stepFrames;
				if (currentFrame > Duration)
				{
					currentFrame = Duration;
					result = true;
				}
				step.Frame = currentFrame;

				currenShift = shift + currentFrame * framePixelSize;
				step.style.left = currenShift;
				return result;
			}

			RulerStep GetNextStep()
			{
				if (pool.Count > 0)
					return pool.Pop();
				RulerStep step = new();
				hierarchy.Add(step);
				step.Fps = fps;
				return step;
			}
		}

		public class RulerStep : VisualElement
		{
			public int Frame
			{
				get => frame;
				set
				{
					if (frame == value)
						return;
					frame = value;
					label.text = EditorUtils.FormatTime(frame, fps);
				}
			}
			public int Fps
			{
				get => fps;
				set
				{
					if (fps == value)
						return;
					fps = value;
					label.text = EditorUtils.FormatTime(frame, fps);
				}
			}
			private int frame = 1;
			private int fps = 60;
			private Label label;

			public RulerStep()
			{
				VisualElement mark = new();
				label = new Label();
				hierarchy.Add(mark);
				hierarchy.Add(label);
				AddToClassList("ruler-step");
				mark.AddToClassList("ruler-step-mark");
				label.AddToClassList("ruler-step-time");
			}
		}
	}
}