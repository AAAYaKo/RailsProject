using System;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TracksListView : ListObserverElement<AnimationTrackViewModel, AnimationTrackView>
	{
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

		public ScrollView Scroll => scrollView;

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
				foreach (var type in AnimationTrackViewModel.TrackTypes.Keys)
					menu.AddItem(type.Name, false, () => OnAddClicked(type));

				menu.DropDown(button.worldBound, button, true);
			};
		}

		protected override AnimationTrackView CreateElement()
		{
			var view = new AnimationTrackView();
			view.SetBinding(nameof(AnimationTrackView.ValueType), new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.ValueType)),
				bindingMode = BindingMode.ToTarget,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			view.SetBinding(nameof(AnimationTrackView.TrackClass), new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.TrackClass)),
				bindingMode = BindingMode.ToTarget,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			view.RemoveClicked += OnRemoveClicked;
			view.KeyFrameClicked += OnKeyFrameClicked;
			return view;
		}

		protected override void ResetElement(AnimationTrackView element)
		{
			element.RemoveClicked -= OnRemoveClicked;
			element.KeyFrameClicked -= OnKeyFrameClicked;
		}

		private void OnRemoveClicked(AnimationTrackView view)
		{
			EditorContext.Instance.SelectedClip?.RemoveTrack(views.IndexOf(view));
		}

		private void OnKeyFrameClicked(AnimationTrackView view)
		{
			int index = views.IndexOf(view);
			if (index < 0)
				return;
			Values[index].OnKeyFrameClicked();
		}
		private void OnAddClicked(Type type)
		{
			EditorContext.Instance.SelectedClip?.AddTrack(type);
		}
	}
}