using System;
using System.Collections.Generic;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TracksListView : VisualElement
	{
		public static readonly Type[] TrackTypes = new[]
		{
			typeof(MoveAnchorTrack),
			typeof(FadeTrack),
		};

		[CreateProperty]
		public ObservableList<AnimationTrackViewModel> Tracks
		{
			get => tracks;
			set
			{
				if (tracks == value)
					return;

				if (tracks != null)
					tracks.ListChanged -= UpdateList;

				tracks = value;
				tracks.ListChanged += UpdateList;
				UpdateList();
			}
		}
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
		private VisualElement tracksContainer;
		private VisualElement buttonContainer;
		private List<AnimationTrackView> trackViews = new();
		private ObservableList<AnimationTrackViewModel> tracks = new();
		private bool? canEdit;


		public TracksListView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsTracksView");
			var main = templateMain.Instantiate();
			main.style.flexBasis = new Length(100, LengthUnit.Percent);
			main.style.flexGrow = 1;
			hierarchy.Add(main);
			tracksContainer = main.Q<VisualElement>("tracks-container");
			buttonContainer = main.Q<VisualElement>("button-container");
			var button = buttonContainer.Q<Button>("add-button");
			button.clicked += () =>
			{
				GenericDropdownMenu menu = new();
				foreach (var type in TrackTypes)
					menu.AddItem(type.Name, false, () => OnAddClicked(type));
				
				menu.DropDown(button.worldBound, button, true);
			};
		}

		public void UpdateList()
		{
			if (Tracks == null)
			{
				tracksContainer.Clear();
				trackViews.Clear();
				return;
			}
			while (Tracks.Count > trackViews.Count)
			{
				var view = new AnimationTrackView();
				view.SetBinding("ValueType", new DataBinding
				{
					dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.ValueType)),
					bindingMode = BindingMode.ToTarget,
					updateTrigger = BindingUpdateTrigger.OnSourceChanged,
				});
				view.RemoveClicked += OnRemoveClicked;
				tracksContainer.Add(view);
				trackViews.Add(view);
			}
			while (Tracks.Count < trackViews.Count)
			{
				var view = trackViews[^1];
				view.RemoveClicked -= OnRemoveClicked;
				tracksContainer.Remove(view);
				trackViews.Remove(view);
			}
			for (int i = 0; i < trackViews.Count; i++)
			{
				trackViews[i].dataSource = Tracks[i];
			}
		}

		private void OnRemoveClicked(AnimationTrackView view)
		{
			EditorContext.Instance.SelectedClip?.RemoveTrack(trackViews.IndexOf(view));
		}

		private void OnAddClicked(Type type)
		{
			EditorContext.Instance.SelectedClip?.AddTrack(type);
		}
	}
}