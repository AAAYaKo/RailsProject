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
				foreach (var type in TrackTypes)
					menu.AddItem(type.Name, false, () => OnAddClicked(type));

				menu.DropDown(button.worldBound, button, true);
			};
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
	}
}