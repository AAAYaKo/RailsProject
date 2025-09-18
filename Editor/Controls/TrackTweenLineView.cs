using Rails.Editor.ViewModel;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
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