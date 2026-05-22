using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Manipulator
{
	public class EaseDragManipulator : MouseManipulator
	{
		private VisualElement container;
		private bool isDragging = false;
		private Action<Vector2> updateValue;
		private Vector2 startTransition;
		private Vector2 startPosition;
		private Vector2 currentPosition;


		public EaseDragManipulator(VisualElement container, Action<Vector2> updateValue)
		{
			this.container = container;
			this.updateValue = updateValue;
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
			target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
			target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
			target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
		}

		private void OnMouseDown(MouseDownEvent evt)
		{
			if (!CanStartManipulation(evt))
				return;
			startPosition = evt.mousePosition;
			startTransition = target.resolvedStyle.translate;
			isDragging = true;

			currentPosition = evt.mousePosition;
			UpdateValue(currentPosition - startPosition);

			target.CaptureMouse();
			evt.StopPropagation();
		}

		private void OnMouseMove(MouseMoveEvent evt)
		{
			if (!isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;

			currentPosition = evt.mousePosition;

			UpdateValue(currentPosition - startPosition);
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

		private void UpdateValue(Vector2 delta)
		{
			Vector2 position = target.layout.position + startTransition + delta;

			updateValue(position);
			target.MarkDirtyRepaint();
		}
	}
}