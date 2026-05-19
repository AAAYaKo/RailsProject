using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Manipulator
{
	public class TrackMoveDragManipulator : MouseManipulator
	{
		public event Action RightClick;
		public event Action<bool> Click;
		public event Action<bool> DragBegin;
		public event Action<int, bool> DragChanged;
		public event Action<int, bool> DragComplete;

		private bool isDragging = false;
		private bool isDragBegan = false;
		private Vector2 startPosition;
		private float framePixelSize = 30;


		public TrackMoveDragManipulator()
		{
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
			if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
				activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Command });
			else
				activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Control });
		}

		public void OnFramePixelSizeChanged(float framePixels)
		{
			framePixelSize = framePixels;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<MouseDownEvent>(OnMouseDown);
			target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
			target.RegisterCallback<MouseUpEvent>(OnMouseUp);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
			target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
			target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
		}

		private void OnMouseDown(MouseDownEvent evt)
		{
			if (!CanStartManipulation(evt))
				return;
			if (evt.button == 1)
			{
				RightClick.Invoke();
				return;
			}
			startPosition = evt.mousePosition;
			isDragging = true;
			isDragBegan = false;
			target.CaptureMouse();
			evt.StopPropagation();
		}

		private void OnMouseMove(MouseMoveEvent evt)
		{
			if (!isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;

			float delta = evt.mousePosition.x - startPosition.x;

			if (!isDragBegan && Mathf.Abs(delta) > 0.01f)
			{
				DragBegin?.Invoke(evt.actionKey);
				target.CaptureMouse();
				isDragBegan = true;
			}

			int deltaFrames = Mathf.RoundToInt(Mathf.Abs(delta / framePixelSize));
			if (delta < 0)
				deltaFrames = -deltaFrames;
			if (deltaFrames != 0)
			{
				DragChanged?.Invoke(deltaFrames, evt.actionKey);
			}
			evt.StopPropagation();
		}

		private void OnMouseUp(MouseUpEvent evt)
		{
			if (!isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;

			float delta = evt.mousePosition.x - startPosition.x;
			int deltaFrames = Mathf.RoundToInt(Mathf.Abs(delta / framePixelSize));
			if (delta < 0)
				deltaFrames = -deltaFrames;
			if (deltaFrames != 0)
				DragComplete?.Invoke(deltaFrames, evt.actionKey);

			if (!isDragBegan)
				Click?.Invoke(evt.actionKey);

			isDragBegan = false;
			isDragging = false;
			target.ReleaseMouse();
			evt.StopPropagation();
		}
	}
}