using System;
using Rails.Editor.Context;
using Rails.Editor.Manipulator;
using Rails.Editor.ViewModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackLinesView : ListObserverElement<AnimationTrackViewModel, AnimationTrackLineView>
	{
		private static VisualTreeAsset templateMain;

		public event Action<Vector2> DoubleClicked;
		public event Action<Vector2> ScrollPerformed;
		public VisualElement Viewport { get; set; }

		private new VisualElement contentContainer;
		private VisualElement tracksBackgroundContainer;
		private VisualElement selectionBoxContainer;
		private VisualElement selectionBoxManipulatorLayer;
		private SelectionBoxDragManipulator selectionBoxManipulator;
		private Vector2 currentMousePosition;
		private IVisualElementScheduledItem mouseScroll;
		private bool mustScroll;


		public TrackLinesView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsTrackView");
			templateMain.CloneTree(this);
			tracksBackgroundContainer = this.Q<VisualElement>("tracks-back");
			contentContainer = this.Q<VisualElement>("content-container");
			container = this.Q<VisualElement>("tracks-container");
			selectionBoxContainer = this.Q<VisualElement>("selection-box-container");
			selectionBoxManipulatorLayer = this.Q<VisualElement>("selection-box-manipulator");

			selectionBoxManipulator = new SelectionBoxDragManipulator(selectionBoxContainer);
			selectionBoxManipulatorLayer.AddManipulator(selectionBoxManipulator);
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			selectionBoxManipulatorLayer.RegisterCallback<ClickEvent>(OnMouseClick);
			selectionBoxManipulator.SelectionBegin += OnSelectionBegin;
			selectionBoxManipulator.SelectionChanged += OnSelectionChanged;
			selectionBoxManipulator.SelectionComplete += OnSelectionComplete;
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			selectionBoxManipulatorLayer.UnregisterCallback<ClickEvent>(OnMouseClick);
			selectionBoxManipulator.SelectionBegin -= OnSelectionBegin;
			selectionBoxManipulator.SelectionChanged -= OnSelectionChanged;
			selectionBoxManipulator.SelectionComplete -= OnSelectionComplete;
		}

		protected override AnimationTrackLineView CreateElement()
		{
			AnimationTrackLineView line = new();
			VisualElement trackBack = new();
			trackBack.AddToClassList("animation-track-line-background");
			tracksBackgroundContainer.Add(trackBack);
			return line;
		}

		protected override void ResetElement(AnimationTrackLineView element)
		{
			tracksBackgroundContainer.RemoveAt(0);
		}

		private void OnMouseClick(ClickEvent evt)
		{
			if (evt.button == 0 && evt.clickCount == 2)
				DoubleClicked?.Invoke(evt.localPosition);
		}

		private void OnSelectionBegin(Rect selectionRect, MouseDownEvent evt)
		{
			Rect selectionWorldRect = selectionBoxContainer.LocalToWorld(selectionRect);
			EventBus.Publish(new SelectionBoxBeginEvent(selectionWorldRect, evt.actionKey));
			currentMousePosition = evt.mousePosition;
			mustScroll = true;
			mouseScroll = schedule.Execute(x =>
			{
				Vector2 mousePosition = Viewport.WorldToLocal(currentMousePosition);
				if (!Viewport.ContainsPoint(mousePosition))
				{
					Vector2 delta = mousePosition;
					if (mousePosition.x < 0)
						delta.x = mousePosition.x;
					else if (mousePosition.x > Viewport.layout.size.x)
						delta.x = mousePosition.x - Viewport.layout.size.x;
					else
						delta.x = 0;

					if (mousePosition.y < 0)
						delta.y = mousePosition.y;
					else if (mousePosition.y > Viewport.layout.size.y)
						delta.y = mousePosition.y - Viewport.layout.size.y;
					else
						delta.y = 0;

					delta *= x.deltaTime / 1000f;

					ScrollPerformed?.Invoke(delta);
				}
			}).Until(() => !mustScroll);
		}

		private void OnSelectionChanged(Rect selectionRect, MouseMoveEvent evt)
		{
			Rect selectionWorldRect = selectionBoxContainer.LocalToWorld(selectionRect);
			EventBus.Publish(new SelectionBoxChangeEvent(selectionWorldRect, evt.actionKey));
			currentMousePosition = evt.mousePosition;
		}

		private void OnSelectionComplete(Rect selectionRect, MouseUpEvent evt)
		{
			Rect selectionWorldRect = selectionBoxContainer.LocalToWorld(selectionRect);
			EventBus.Publish(new SelectionBoxCompleteEvent(selectionWorldRect, evt.actionKey));
			currentMousePosition = evt.mousePosition;
			mouseScroll.Pause();
			mouseScroll = null;
			mustScroll = false;
		}
	}
}