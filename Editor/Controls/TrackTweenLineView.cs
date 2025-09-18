using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackTweenLineView : VisualElement
	{
		public int StartFrame
		{
			get => startFrame ?? 0;
			set
			{
				if (startFrame == value)
					return;
				startFrame = value;
				UpdateStartPosition();
				UpdateEndPosition();
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
				UpdateStartPosition();
				UpdateEndPosition();
			}
		}

		private int? startFrame;
		private int? endFrame;
		private float framePixelSize = 30;


		public TrackTweenLineView()
		{
			VisualElement line = new();
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