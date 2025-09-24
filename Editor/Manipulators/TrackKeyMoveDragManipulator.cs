using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Manipulator
{
	public class TrackKeyMoveDragManipulator : MouseManipulator
	{
		public Action<bool> KeyDragBegin;
		public Action<int, bool> KeyDragChanged;
		public Action<int, bool> KeyDragComplete;

		private bool isDragging = false;
		private bool isDragBegan = false;
		private Vector2 startPosition;
		private float framePixelSize = 30;


		public TrackKeyMoveDragManipulator()
		{
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
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
				KeyDragBegin?.Invoke(evt.actionKey);
				target.CaptureMouse();
				isDragBegan = true;
			}

			int deltaFrames = Mathf.RoundToInt(Mathf.Abs(delta / framePixelSize));
			if (delta < 0)
				deltaFrames = -deltaFrames;
			if (deltaFrames != 0)
			{
				KeyDragChanged?.Invoke(deltaFrames, evt.actionKey);
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
			{
				KeyDragComplete?.Invoke(deltaFrames, evt.actionKey);
			}

			isDragBegan = false;
			isDragging = false;
			target.ReleaseMouse();
			evt.StopPropagation();
		}
	}
}