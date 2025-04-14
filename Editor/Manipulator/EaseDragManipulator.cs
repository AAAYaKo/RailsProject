using System;
using Rails.Editor.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Manipulator
{
	public class EaseDragManipulator : MouseManipulator
	{
		private bool _isDragging = false;
		private Vector2 _startPosition;
		private Action<Vector2> _updateValue;


		public EaseDragManipulator(Action<Vector2> updateValue)
		{
			_updateValue = updateValue;
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
		}

		protected override void RegisterCallbacksOnTarget()
		{
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
			if (target is not DragHandler)
				return;
			if (!CanStartManipulation(evt))
				return;
			_startPosition = evt.localMousePosition;
			_isDragging = true;
			target.CaptureMouse();
			evt.StopPropagation();
		}

		private void OnMouseMove(MouseMoveEvent evt)
		{
			if (target is not DragHandler)
				return;
			if (!_isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;

			Vector2 delta = evt.localMousePosition - _startPosition;
			Vector2 position = target.layout.position + (Vector2)target.transform.position + delta;

			_updateValue(position);
			target.MarkDirtyRepaint();
			evt.StopPropagation();
		}

		private void OnMouseUp(MouseUpEvent evt)
		{
			if (target is not DragHandler)
				return;
			if (!_isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;
			_isDragging = false;
			target.ReleaseMouse();
			evt.StopPropagation();
		}
	}
}