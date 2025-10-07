using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Manipulator
{
	public class SelectionBoxDragManipulator : MouseManipulator
	{
		public event Action<Rect, MouseDownEvent> SelectionBegin;
		public event Action<Rect, MouseMoveEvent> SelectionChanged;
		public event Action<Rect, MouseUpEvent> SelectionComplete;
		private bool isDragging = false;
		private Vector2 startPosition;
		private VisualElement selectionBoxContainer;
		private VisualElement selectionBox;

		public SelectionBoxDragManipulator(VisualElement selectionBoxContainer)
		{
			this.selectionBoxContainer = selectionBoxContainer;
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });

			selectionBox = new VisualElement();
			selectionBox.style.position = Position.Absolute;
			selectionBox.pickingMode = PickingMode.Ignore;
			selectionBox.AddToClassList("selection-box");
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
			startPosition = evt.localMousePosition;
			isDragging = true;
			target.CaptureMouse();
			evt.StopPropagation();

			selectionBoxContainer.Add(selectionBox);
			selectionBox.style.left = startPosition.x;
			selectionBox.style.top = startPosition.y;
			selectionBox.style.width = 0;
			selectionBox.style.height = 0;
			SelectionBegin?.Invoke(selectionBox.layout, evt);
		}

		private void OnMouseMove(MouseMoveEvent evt)
		{
			if (!isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;

			Vector2 position = evt.localMousePosition;
			Vector2 delta = position - startPosition;
			Offsets rectTemplate = new();
			if (delta.x < 0)
			{
				rectTemplate.Left = position.x;
				rectTemplate.Right = startPosition.x;
			}
			else
			{
				rectTemplate.Left = startPosition.x;
				rectTemplate.Right = position.x;
			}
			if (delta.y < 0)
			{
				rectTemplate.Top = position.y;
				rectTemplate.Bottom = startPosition.y;
			}
			else
			{
				rectTemplate.Top = startPosition.y;
				rectTemplate.Bottom = position.y;
			}

			if (rectTemplate.Left < 0)
				rectTemplate.Left = 0;
			if (rectTemplate.Right > target.resolvedStyle.width)
				rectTemplate.Right = target.resolvedStyle.width;
			if (rectTemplate.Top < 0)
				rectTemplate.Top = 0;
			if (rectTemplate.Bottom > target.resolvedStyle.height)
				rectTemplate.Bottom = target.resolvedStyle.height;

			selectionBox.style.left = rectTemplate.Left;
			selectionBox.style.top = rectTemplate.Top;
			selectionBox.style.width = rectTemplate.Right - rectTemplate.Left;
			selectionBox.style.height = rectTemplate.Bottom - rectTemplate.Top;
			SelectionChanged?.Invoke(selectionBox.layout, evt);
			evt.StopPropagation();
		}

		private void OnMouseUp(MouseUpEvent evt)
		{
			if (!isDragging || !target.HasMouseCapture() || !CanStartManipulation(evt))
				return;
			isDragging = false;
			target.ReleaseMouse();
			evt.StopPropagation();

			SelectionComplete?.Invoke(selectionBox.layout, evt);
			selectionBoxContainer.Remove(selectionBox);
		}
	}
}