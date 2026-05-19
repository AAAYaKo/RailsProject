using System;
using Rails.Editor.Context;
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

		private TrackMoveDragManipulator manipulator;
		private int? timePosition;
		private float framePixelSize = 30;

		public event Action<int> TimePositionChanged;


		public TrackKeyView()
		{
			AddToClassList("track-key");
			SetBinding(TimePositionProperty, new ToTargetBinding(nameof(AnimationKeyViewModel.TimePosition)));
			manipulator = new TrackMoveDragManipulator();
			this.AddManipulator(manipulator);
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			manipulator.RightClick += OnRightClick;
			manipulator.Click += OnClick;
			manipulator.DragBegin += OnKeyDragBegin;
			manipulator.DragChanged += OnKeyDragChanged;
			manipulator.DragComplete += OnKeyDragComplete;
			EventBus.Subscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			OnFramePixelSizeChanged(EditorContext.Instance.FramePixelSize);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			manipulator.RightClick -= OnRightClick;
			manipulator.DragBegin -= OnKeyDragBegin;
			manipulator.DragChanged -= OnKeyDragChanged;
			manipulator.DragComplete -= OnKeyDragComplete;
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

		private void OnRightClick()
		{
			EventBus.Publish(new KeyRightClickEvent(this));
		}

		private void OnClick(bool actionKey)
		{
			EventBus.Publish(new KeyClickEvent(this, actionKey));
		}

		private void OnGeometryChanged(GeometryChangedEvent evt)
		{
			UpdatePosition();
		}

		private void OnKeyDragBegin(bool actionKey)
		{
			EventBus.Publish(new KeyDragBeginEvent(this));
		}

		private void OnKeyDragChanged(int deltaFrames, bool actionKey)
		{
			EventBus.Publish(new KeyDragChangedEvent(deltaFrames));
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