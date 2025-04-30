using System;
using Rails.Editor.ViewModel;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TracksListView : ListObserverElement<AnimationTrackViewModel, AnimationTrackView>
	{
		public static readonly Type[] TrackTypes = new[]
		{
			typeof(MoveAnchorTrack),
			typeof(FadeTrack),
		};

		[UxmlAttribute("can-edit"), CreateProperty]
		public bool CanEdit
		{
			get => canEdit ?? false;
			set
			{
				if (canEdit == value)
					return;
				canEdit = value;
				buttonContainer.style.display = CanEdit ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}
		private static VisualTreeAsset templateMain;
		private ScrollView scrollView;
		private VisualElement buttonContainer;
		private bool? canEdit;


		public TracksListView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsTracksListView");
			templateMain.CloneTree(this);

			scrollView = this.Q<ScrollView>();
			container = scrollView.Q<VisualElement>("tracks-container");
			buttonContainer = scrollView.Q<VisualElement>("button-container");
			var button = buttonContainer.Q<Button>("add-button");
			button.clicked += () =>
			{
				GenericDropdownMenu menu = new();
				foreach (var type in TrackTypes)
					menu.AddItem(type.Name, false, () => OnAddClicked(type));

				menu.DropDown(button.worldBound, button, true);
			};
			RegisterCallback<WheelEvent>(ScrollHandler, TrickleDown.TrickleDown);
			RegisterCallback<AttachToPanelEvent>(x =>
			{
				EditorContext.Instance.TrackScrollPerformed += ScrollPerformedHandler;
			});
			RegisterCallback<DetachFromPanelEvent>(x =>
			{
				EditorContext.Instance.TrackScrollPerformed -= ScrollPerformedHandler;
			});
		}

		protected override AnimationTrackView CreateElement()
		{
			var view = new AnimationTrackView();
			view.SetBinding("ValueType", new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.ValueType)),
				bindingMode = BindingMode.ToTarget,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			view.RemoveClicked += OnRemoveClicked;
			return view;
		}

		protected override void ResetElement(AnimationTrackView element)
		{
			element.RemoveClicked -= OnRemoveClicked;
		}

		private void OnRemoveClicked(AnimationTrackView view)
		{
			EditorContext.Instance.SelectedClip?.RemoveTrack(views.IndexOf(view));
		}

		private void OnAddClicked(Type type)
		{
			EditorContext.Instance.SelectedClip?.AddTrack(type);
		}

		private void ScrollPerformedHandler(Vector2 delta)
		{
			scrollView.scrollOffset += delta;
		}

		private void ScrollHandler(WheelEvent evt)
		{
			float num2 = scrollView.mouseWheelScrollSize;
			float y = evt.delta.y * ((scrollView.verticalScroller.lowValue < scrollView.verticalScroller.highValue) ? 1f : (-1f)) * num2;
			float x = evt.delta.x * ((scrollView.horizontalScroller.lowValue < scrollView.horizontalScroller.highValue) ? 1f : (-1f)) * num2;

			//scrollView.scrollOffset += new Vector2(x, y);
			EditorContext.Instance.PerformTrackScroll(new Vector2(x, y));

			evt.StopPropagation();
		}
	}
}