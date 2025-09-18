using System;
using Rails.Editor.Manipulator;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackKeyView : VisualElement
	{
		[UxmlAttribute("timePosition"), CreateProperty]
		public int TimePosition
		{
			get => timePosition ?? 0;
			set
			{
				SetTimePositionWithoutUpdate(value);
				UpdatePosition();
			}
		}

		public event Action<TrackKeyView, bool> OnClick;
		public event Action<int> KeyDragged;
		public event Action<TrackKeyView> KeyDragComplete;

		private TrackKeyMoveDragManipulator manipulator;
		private int? timePosition;
		private float framePixelSize = 30;


		public TrackKeyView()
		{
			AddToClassList("track-key");
			SetBinding(nameof(TimePosition), new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationKeyViewModel.TimePosition)),
				bindingMode = BindingMode.ToTarget,
			});
			RegisterCallback<GeometryChangedEvent>(x =>
			{
				UpdatePosition();
			});
			RegisterCallback<ClickEvent>(x =>
			{
				OnClick?.Invoke(this, x.actionKey);
			}, TrickleDown.TrickleDown);

			manipulator = new TrackKeyMoveDragManipulator();
			manipulator.KeyDragBegin += OnKeyDragBegin;
			manipulator.KeyDragChanged += OnKeyDragChanged;
			manipulator.KeyDragComplete += OnKeyDragComplete;
			this.AddManipulator(manipulator);
		}

		public void OnFramePixelSizeChanged(float framePixelSize)
		{
			this.framePixelSize = framePixelSize;
			manipulator.OnFramePixelSizeChanged(framePixelSize);
			UpdatePosition();
		}

		public void SetTimePositionWithoutUpdate(int value)
		{
			if (timePosition == value)
				return;
			timePosition = value;
		}

		private void OnKeyDragBegin(bool actionKey)
		{
			OnClick?.Invoke(this, actionKey);
		}

		private void OnKeyDragChanged(int deltaFrames, bool actionKey)
		{
			KeyDragged?.Invoke(deltaFrames);
		}

		private void OnKeyDragComplete(int deltaFrames, bool actionKey)
		{
			KeyDragComplete?.Invoke(this);
		}

		private void UpdatePosition()
		{
			style.left = TrackLinesView.StartAdditional - layout.width / 2 + TimePosition * framePixelSize;
		}
	}
}