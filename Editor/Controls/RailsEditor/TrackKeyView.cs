using System;
using Rails.Editor.Manipulator;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackKeyView : BaseView
	{
		public static readonly BindingId TimePositionProperty = nameof(TimePosition);

		[UxmlAttribute("timePosition"), CreateProperty]
		public int TimePosition
		{
			get => timePosition ?? 0;
			set
			{
				if (timePosition == value)
					return;
				timePosition = value;
				UpdatePosition();
				TimePositionChanged?.Invoke(value);
			}
		}

		private TrackKeyMoveDragManipulator manipulator;
		private int? timePosition;
		private float framePixelSize = 30;

		public event Action<int> TimePositionChanged;


		public TrackKeyView()
		{
			AddToClassList("track-key");
			SetBinding(TimePositionProperty, new ToTargetBinding(nameof(AnimationKeyViewModel.TimePosition)));
			manipulator = new TrackKeyMoveDragManipulator();
			this.AddManipulator(manipulator);
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			RegisterCallback<MouseDownEvent>(OnClick, TrickleDown.TrickleDown);
			manipulator.KeyDragBegin += OnKeyDragBegin;
			manipulator.KeyDragChanged += OnKeyDragChanged;
			manipulator.KeyDragComplete += OnKeyDragComplete;
			EventBus.Subscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			OnFramePixelSizeChanged(EditorContext.Instance.FramePixelSize);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			UnregisterCallback<MouseDownEvent>(OnClick, TrickleDown.TrickleDown);
			manipulator.KeyDragBegin -= OnKeyDragBegin;
			manipulator.KeyDragChanged -= OnKeyDragChanged;
			manipulator.KeyDragComplete -= OnKeyDragComplete;
			EventBus.Unsubscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
		}

		private void OnFramePixelSizeChanged(FramePixelSizeChangedEvent evt)
		{
			OnFramePixelSizeChanged(evt.FramePixelSize);
		}
		private void OnFramePixelSizeChanged(float framePixelSize)
		{
			this.framePixelSize = framePixelSize;
			manipulator.OnFramePixelSizeChanged(framePixelSize);
			UpdatePosition();
		}

		private void OnClick(MouseDownEvent evt)
		{
			if (evt.button == 0)
				EventBus.Publish(new KeyClickEvent(this, evt.actionKey));
			else if (evt.button == 1)
				EventBus.Publish(new KeyRightClickEvent(this));
		}

		private void OnGeometryChanged(GeometryChangedEvent evt)
		{
			UpdatePosition();
		}

		private void OnKeyDragBegin(bool actionKey)
		{
			EventBus.Publish(new KeyClickEvent(this, actionKey));
		}

		private void OnKeyDragChanged(int deltaFrames, bool actionKey)
		{
			EventBus.Publish(new KeyDragEvent(deltaFrames));
		}

		private void OnKeyDragComplete(int deltaFrames, bool actionKey)
		{
			EventBus.Publish(new KeyDragCompleteEvent());
		}

		private void UpdatePosition()
		{
			style.left = TrackLinesView.StartAdditional - layout.width / 2 + TimePosition * framePixelSize;
		}
	}
}