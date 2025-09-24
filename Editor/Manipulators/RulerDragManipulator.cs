using System;
using Rails.Editor.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Manipulator
{
	public class RulerDragManipulator : MouseManipulator
	{
		private bool isDragging = false;
		private Action<float> updateTime;


		public RulerDragManipulator(Action<float> updateTime)
		{
			this.updateTime = updateTime;
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
		}

		protected override void RegisterCallbacksOnTarget()
		{
			if (target is not RailsRuler)
				return;
			target.RegisterCallback<MouseDownEvent>(OnMouseDown);
			target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
			target.RegisterCallback<MouseUpEvent>(OnMouseUp);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			if (target is not RailsRuler)
				return;
			target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
			target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
			target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
		}

		private void OnMouseDown(MouseDownEvent evt)
		{
			if (!CanStartManipulation(evt))
				return;
			isDragging = true;
			target.CaptureMouse();
			evt.StopPropagation();
		}

		private void OnMouseMove(MouseMoveEvent evt)
		{
			if (!isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;

			updateTime(evt.localMousePosition.x);
			evt.StopPropagation();
		}

		private void OnMouseUp(MouseUpEvent evt)
		{
			if (!isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;
			isDragging = false;
			updateTime(evt.localMousePosition.x);
			target.ReleaseMouse();
			evt.StopPropagation();
		}
	}
}