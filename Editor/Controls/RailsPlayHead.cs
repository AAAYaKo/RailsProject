using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class RailsPlayHead : VisualElement
	{
		[UxmlAttribute("timePosition"), CreateProperty]
		public int TimeHeadPosition
		{
			get => timeHeadPosition;
			set
			{
				if (timeHeadPosition == value)
					return;
				timeHeadPosition = value;
				UpdatePosition();
			}
		}

		private int timeHeadPosition;
		private float framePixelSize = 30;
		private float timePosition;


		public RailsPlayHead()
		{
			VisualElement icon = new();
			VisualElement tail = new();

			icon.name = "play-head-icon";
			icon.AddToClassList("play-head-icon");
			icon.style.flexGrow = 0;
			icon.style.flexShrink = 0;
			icon.pickingMode = PickingMode.Ignore;
			tail.name = "play-head-tail";
			tail.AddToClassList("play-head-tail");
			tail.style.flexGrow = 1;
			tail.style.flexShrink = 1;
			tail.pickingMode = PickingMode.Ignore;
			Add(icon);
			Add(tail);
			style.alignItems = Align.Center;
			style.position = Position.Absolute;
			style.left = TrackLinesView.StartAdditional;
			style.height = new Length(100, LengthUnit.Percent);
			style.width = new Length(2);

			RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
		}

		public void OnFramePixelSizeChanged(float framePixelSize)
		{
			this.framePixelSize = framePixelSize;
			UpdatePosition();
		}

		public void OnTimePositionChanged(float timePosition)
		{
			this.timePosition = timePosition;
			UpdatePosition();
		}

		private void UpdatePosition()
		{
			style.left = TrackLinesView.StartAdditional + (TimeHeadPosition - timePosition) * framePixelSize;
		}

		private void OnGeometryChange(GeometryChangedEvent evt)
		{
			UpdatePosition();
		}
	}
}