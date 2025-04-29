using System;
using System.Collections.Generic;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class ClipsListView : VisualElement
	{
		public static readonly BindingId SelectedIndexProperty = nameof(SelectedIndex);
		public static readonly BindingId CanAddProperty = nameof(CanAdd);
		private const string SelectedClass = "rails-clip-view--selected";

		[CreateProperty]
		public ObservableList<RailsClipViewModel> Clips
		{
			get => clips;
			set
			{
				if (clips == value)
					return;

				if (clips != null)
					clips.ListChanged -= UpdateList;

				clips = value;
				clips.ListChanged += UpdateList;
				UpdateList();
			}
		}
		[UxmlAttribute("selected"), CreateProperty]
		public int SelectedIndex
		{
			get => selectedIndex ?? 0;
			set
			{
				if (selectedIndex == value)
					return;

				ChangeSelection(value);

				NotifyPropertyChanged(SelectedIndexProperty);
			}
		}
		[UxmlAttribute("can-add"), CreateProperty]
		public bool CanAdd
		{
			get => canAdd ?? false;
			set
			{
				if (canAdd == value)
					return;

				canAdd = value;
				buttonContainer.style.display = canAdd ?? false ? DisplayStyle.Flex : DisplayStyle.None;
				NotifyPropertyChanged(CanAddProperty);
			}
		}

		private ObservableList<RailsClipViewModel> clips;

		private static VisualTreeAsset templateMain;
		private static VisualTreeAsset templateItem;
		private VisualElement clipsContainer;
		private VisualElement buttonContainer;
		private List<VisualElement> clipViews = new();
		private VisualElement selected;
		private int? selectedIndex;
		private bool? canAdd;

		public event Action AddClicked;
		public event Action<int> RemoveClicked;

		public ClipsListView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsClipsView");
			if (templateItem == null)
				templateItem = Resources.Load<VisualTreeAsset>("RailsClip");
			templateMain.CloneTree(this);

			clipsContainer = this.Q<VisualElement>("clips-container");
			buttonContainer = this.Q<VisualElement>("button-container");
			buttonContainer.Q<Button>("add-button").clicked += () => AddClicked?.Invoke();
		}

		public void UpdateList()
		{
			if (Clips == null)
			{
				clipsContainer.Clear();
				clipViews.Clear();
				SelectedIndex = 0;
				return;
			}
			while (Clips.Count > clipViews.Count)
			{
				var view = templateItem.Instantiate();
				clipsContainer.Add(view);
				clipViews.Add(view);
				view.AddManipulator(new ContextualMenuManipulator(x =>
				{
					x.menu.AppendAction("Remove", x =>
					{
						RemoveClicked?.Invoke(clipViews.IndexOf(view));
					}, DropdownMenuAction.Status.Normal);
				}));
				view.RegisterCallback<ClickEvent>(x =>
				{
					if (x.button == 0)
					{
						int index = clipViews.IndexOf(view);
						SelectedIndex = index;
					}
				});
			}
			while (Clips.Count < clipViews.Count)
			{
				var view = clipViews[^1];
				clipsContainer.Remove(view);
				clipViews.Remove(view);
			}
			for (int i = 0; i < clipViews.Count; i++)
			{
				clipViews[i].dataSource = Clips[i];
			}
			if (Clips.Count > 0)
			{
				if (SelectedIndex >= Clips.Count)
					SelectedIndex = 0;
				ChangeSelection(SelectedIndex);
			}
		}

		private void ChangeSelection(int index)
		{
			selectedIndex = index;
			if (clipViews.Count > 0)
			{
				selected?.RemoveFromClassList(SelectedClass);
				selected = clipViews[index];
				selected?.AddToClassList(SelectedClass);
			}
		}
	}
}