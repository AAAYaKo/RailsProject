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
		public static readonly BindingId AddTrackCommandProperty = nameof(AddTrackCommand);

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
		[CreateProperty]
		public ICommand<Type> AddTrackCommand;

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
			SetBinding(AddTrackCommandProperty, new CommandBinding(nameof(RailsClipViewModel.AddTrackCommand)));
		}

		protected override AnimationTrackView CreateElement()
		{
			var view = new AnimationTrackView();
			view.SetBinding(AnimationTrackView.TrackClassProperty, new ToTargetBinding(nameof(AnimationTrackViewModel.TrackClass)));
			return view;
		}

		protected override void ResetElement(AnimationTrackView element)
		{
		}

		private void OnAddClicked(Type type)
		{
			AddTrackCommand.Execute(type);
		}
	}
}