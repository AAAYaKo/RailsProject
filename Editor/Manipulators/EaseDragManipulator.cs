using System;
using Rails.Editor.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Manipulator
{
	public class EaseDragManipulator : MouseManipulator
	{
		private bool isDragging = false;
		private Vector2 startPosition;
		private Action<Vector2> updateValue;


		public EaseDragManipulator(Action<Vector2> updateValue)
		{
			this.updateValue = updateValue;
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
		}

		protected override void RegisterCallbacksOnTarget()
		{
			if (target is not DragHandler)
				return;
			target.RegisterCallback<MouseDownEvent>(OnMouseDown);
			target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
			target.RegisterCallback<MouseUpEvent>(OnMouseUp);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			if (target is not DragHandler)
				return;
			target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
			target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
			target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
		}

		private void OnMouseDown(MouseDownEvent evt)
		{
			if (!CanStartManipulation(evt))
				return;
			startPosition = evt.localMousePosition;
			isDragging = true;
			target.CaptureMouse();
			evt.StopPropagation();
		}

		private void OnMouseMove(MouseMoveEvent evt)
		{
			if (!isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;

			Vector2 delta = evt.localMousePosition - startPosition;
			Vector2 position = target.layout.position + (Vector2)target.transform.position + delta;

			updateValue(position);
			target.MarkDirtyRepaint();
			evt.StopPropagation();
		}

		private void OnMouseUp(MouseUpEvent evt)
		{
			if (!isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;
			isDragging = false;
			target.ReleaseMouse();
			evt.StopPropagation();
		}
	}
}