using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackTweenLineView : BaseView
	{
		private TrackKeyView start;
		private TrackKeyView end;
		private int startTimePosition;
		private int endTimePosition;
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

		public void Bind(TrackKeyView start, TrackKeyView end)
		{
			this.start = start;
			this.end = end;
			start.TimePositionChanged += OnStartPositionChanged;
			end.TimePositionChanged += OnEndPositionChanged;
			startTimePosition = start.TimePosition;
			endTimePosition = end.TimePosition;
			OnStartPositionChanged(start.TimePosition);
		}

		public void Unbind()
		{
			if (start != null)
				start.TimePositionChanged -= OnStartPositionChanged;
			if (end != null)
				end.TimePositionChanged -= OnEndPositionChanged;
			start = null;
			end = null;
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			EventBus.Subscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			OnFramePixelSizeChanged(EditorContext.Instance.FramePixelSize);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			EventBus.Unsubscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			Unbind();
		}

		private void OnFramePixelSizeChanged(FramePixelSizeChangedEvent evt)
		{
			OnFramePixelSizeChanged(evt.FramePixelSize);
		}

		private void OnFramePixelSizeChanged(float framePixelSize)
		{
			this.framePixelSize = framePixelSize;
			OnStartPositionChanged(startTimePosition);
		}

		private void OnStartPositionChanged(int position)
		{
			startTimePosition = position;
			style.left = framePixelSize * startTimePosition + TrackLinesView.StartAdditional;
			OnEndPositionChanged(endTimePosition);
		}

		private void OnEndPositionChanged(int position)
		{
			endTimePosition = position;
			style.width = framePixelSize * (endTimePosition - startTimePosition);
		}
	}
}