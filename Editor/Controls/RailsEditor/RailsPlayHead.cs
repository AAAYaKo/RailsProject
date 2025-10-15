using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class RailsPlayHead : BaseView
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

		[UxmlAttribute("canEdit"), CreateProperty]
		public bool CanEdit
		{
			get => canEdit;
			private set
			{
				if (canEdit == value)
					return;
				canEdit = value;
				style.display = canEdit ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		private bool canEdit = true;
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
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			EventBus.Subscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			EventBus.Subscribe<TimePositionChangedEvent>(OnTimePositionChanged);
			OnFramePixelSizeChanged(EditorContext.Instance.FramePixelSize);
			OnTimePositionChanged(EditorContext.Instance.TimePosition);
			UpdatePosition();
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
			this.framePixelSize = framePixelSize;
			UpdatePosition();
		}

		private void OnTimePositionChanged(TimePositionChangedEvent evt)
		{
			OnTimePositionChanged(evt.TimePosition);
		}
		private void OnTimePositionChanged(float timePosition)
		{
			this.timePosition = timePosition;
			UpdatePosition();
		}

		private void UpdatePosition()
		{
			style.left = TrackLinesView.StartAdditional + (TimeHeadPosition - timePosition) * framePixelSize;
		}
	}
}