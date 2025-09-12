using System.Collections.Generic;
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
	public partial class RailsRuler : VisualElement
	{
		public static readonly BindingId TimePositionProperty = nameof(TimeHeadPosition);

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
				NotifyPropertyChanged(TimePositionProperty);
			}
		}

		private float framePixelSize = 30;
		private float stepSize => framePixelSize * stepFrames;
		private int stepFrames = 1;
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

		public void OnFramePixelSizeChanged(float framePixelSize)
		{
			float length = layout.width;
			this.framePixelSize = framePixelSize;
			while (stepSize < MinStepSize && stepSize < length)
				stepFrames++;

			while (stepSize > MaxStepSize && stepFrames > 0)
				stepFrames--;
			Repaint();
		}

		public void OnTimePositionChanged(float timePosition)
		{
			this.timePosition = timePosition;
			Repaint();
		}

		private void Repaint()
		{
			if (stepFrames <= 0)
				stepFrames = 1;
			float length = layout.width;
			int firstFrame = 0;
			while (firstFrame + stepSize < timePosition)
				firstFrame += stepFrames;
			int currentFrame = firstFrame - stepFrames;
			float shift = (firstFrame - timePosition) * framePixelSize + TrackLinesView.StartAdditional;
			float currenShift = shift;
			int i = 0;
			bool hasExtra = false;
			for (; i < stepList.Count; i++)
			{
				var step = stepList[i];
				InitStep(step);

				if (currenShift > length && i < stepList.Count - 1 || currentFrame == Duration)
				{
					hasExtra = true;
					break;
				}
			}
			if (hasExtra)
			{
				for (int j = i + 1; j < stepList.Count; j++)
				{
					pool.Push(stepList[^1]);
					if (Children().Contains(stepList[^1]))
						Remove(stepList[^1]);
					stepList.RemoveAt(stepList.Count - 1);
				}

				return;
			}
			while (currenShift < length)
			{
				var step = GetNextStep();
				InitStep(step);
				hierarchy.Add(step);

				stepList.Add(step);
				if (currentFrame == Duration)
					break;
			}

			void InitStep(RulerStep step)
			{
				currentFrame += stepFrames;
				if (currentFrame > Duration)
					currentFrame = Duration;
				step.Frame = currentFrame;

				currenShift = shift + currentFrame * framePixelSize;
				step.style.left = currenShift;
			}

			RulerStep GetNextStep()
			{
				if (pool.Count > 0)
					return pool.Pop();
				RulerStep step = new();
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
					label.text = ViewModel.EditorUtils.FormatTime(frame, fps);
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
					label.text = ViewModel.EditorUtils.FormatTime(frame, fps);
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