using Rails.Editor.Context;
using Rails.Editor.Manipulator;
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
		private TrackMoveDragManipulator manipulator;


		public TrackTweenLineView()
		{
			VisualElement line = new();
			line.AddToClassList("track-tween-line");
			AddToClassList("track-tween-line-container");

			Add(line);
			//pickingMode = PickingMode.Ignore;
			line.pickingMode = PickingMode.Ignore;
			style.height = 10;
			style.alignSelf = Align.Center;
			manipulator = new TrackMoveDragManipulator();
			this.AddManipulator(manipulator);
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
			manipulator.Click += OnClick;
			manipulator.DragBegin += OnKeyDragBegin;
			manipulator.DragChanged += OnKeyDragChanged;
			manipulator.DragComplete += OnKeyDragComplete;
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			EventBus.Unsubscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			manipulator.Click -= OnClick;
			manipulator.DragBegin -= OnKeyDragBegin;
			manipulator.DragChanged -= OnKeyDragChanged;
			manipulator.DragComplete -= OnKeyDragComplete;
			Unbind();
		}

		private void OnClick(bool actionKey)
		{
			EventBus.Publish(new TweenLineClickEvent(start, end, actionKey));
		}

		private void OnKeyDragBegin(bool actionKey)
		{
			EventBus.Publish(new KeyDragBeginEvent(start, end));
		}

		private void OnKeyDragChanged(int deltaFrames, bool actionKey)
		{
			EventBus.Publish(new KeyDragChangedEvent(deltaFrames));
		}

		private void OnKeyDragComplete(int deltaFrames, bool actionKey)
		{
			EventBus.Publish(new KeyDragCompleteEvent());
		}

		private void OnFramePixelSizeChanged(FramePixelSizeChangedEvent evt)
		{
			OnFramePixelSizeChanged(evt.FramePixelSize);
		}

		private void OnFramePixelSizeChanged(float framePixelSize)
		{
			this.framePixelSize = framePixelSize;
			manipulator.OnFramePixelSizeChanged(framePixelSize);
			OnStartPositionChanged(startTimePosition);
		}

		private void OnStartPositionChanged(int position)
		{
			startTimePosition = position;
			style.left = framePixelSize * startTimePosition + ClipView.StartAdditional;
			OnEndPositionChanged(endTimePosition);
		}

		private void OnEndPositionChanged(int position)
		{
			endTimePosition = position;
			style.width = framePixelSize * (endTimePosition - startTimePosition);
		}
	}
}