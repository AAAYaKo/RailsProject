using System;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackKeyView : VisualElement
	{
		public static readonly BindingId TimePositionProperty = nameof(TimePosition);

		[UxmlAttribute("timePosition"), CreateProperty]
		public int TimePosition
		{
			get => timePosition ?? 0;
			private set
			{
				if (timePosition == value)
					return;
				timePosition = value;
				UpdatePosition();
				NotifyPropertyChanged(TimePositionProperty);
			}
		}

		public event Action<TrackKeyView, ClickEvent> OnClick;

		private int? timePosition;
		private float framePixelSize = 30;


		public TrackKeyView()
		{
			AddToClassList("track-key");
			SetBinding(nameof(TimePosition), new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationKeyViewModel.TimePosition)),
				bindingMode = BindingMode.TwoWay,
			});
			RegisterCallback<GeometryChangedEvent>(x =>
			{
				UpdatePosition();
			});
			RegisterCallback<ClickEvent>(x =>
			{
				OnClick?.Invoke(this, x);
			});
		}

		public void OnFramePixelSizeChanged(float framePixelSize)
		{
			this.framePixelSize = framePixelSize;
			UpdatePosition();
		}

		private void UpdatePosition()
		{
			style.left = TrackLinesView.StartAdditional - layout.width / 2 + TimePosition * framePixelSize;
		}
	}

	[UxmlElement]
	public partial class TrackTweenLineView : VisualElement
	{
		private const string ColorClass = "-color";

		public int StartFrame
		{
			get => startFrame ?? 0;
			set
			{
				if (startFrame == value)
					return;
				startFrame = value;
				UpdateStartPosition();
			}
		}

		public int EndFrame
		{
			get => endFrame ?? 0;
			set
			{
				if (endFrame == value)
					return;
				endFrame = value;
				UpdateEndPosition();
			}
		}
		public string TrackClass
		{
			get => trackClass;
			set
			{
				if (trackClass == value)
					return;
				if (!trackClass.IsNullOrEmpty())
					line.RemoveFromClassList(trackClass + ColorClass);
				trackClass = value;
				line.AddToClassList(trackClass + ColorClass);
			}
		}

		private int? startFrame;
		private int? endFrame;
		private float framePixelSize = 30;
		private string trackClass;
		private VisualElement line;


		public TrackTweenLineView()
		{
			line = new();
			line.AddToClassList("track-tween-line");
			AddToClassList("track-tween-line-container");

			Add(line);
			pickingMode = PickingMode.Ignore;
			line.pickingMode = PickingMode.Ignore;
		}

		public void OnFramePixelSizeChanged(float framePixelSize)
		{
			this.framePixelSize = framePixelSize;
			UpdateStartPosition();
			UpdateEndPosition();
		}

		private void UpdateStartPosition()
		{
			style.left = framePixelSize * StartFrame + TrackLinesView.StartAdditional;
		}

		private void UpdateEndPosition()
		{
			style.width = framePixelSize * (EndFrame - StartFrame);
		}
	}
}