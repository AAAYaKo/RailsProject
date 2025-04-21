using System;
using System.Collections.Generic;
using Rails.Editor.ViewModel;
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

		[UxmlAttribute("clips"), CreateProperty]
		public List<RailsClipViewModel> Clips
		{
			get => clips;
			set
			{
				if (!Utils.ListEquals(clips, value))
				{
					clips = value;
					UpdateList();
				}
			}
		}
		[UxmlAttribute("selected"), CreateProperty]
		public int SelectedIndex
		{
			get => selectedIndex;
			set
			{
				if (selectedIndex == value)
					return;
				selectedIndex = value;
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

		private List<RailsClipViewModel> clips;

		private VisualTreeAsset templateMain;
		private VisualTreeAsset templateItem;
		private VisualElement clipsContainer;
		private VisualElement buttonContainer;
		private List<VisualElement> clipViews = new();
		private int selectedIndex;
		private bool? canAdd;

		public event Action AddClicked;
		public event Action<int> RemoveClicked;

		public ClipsListView()
		{
			templateMain = Resources.Load<VisualTreeAsset>("RailsClipsView");
			templateItem = Resources.Load<VisualTreeAsset>("RailsClip");
			var main = templateMain.Instantiate();
			hierarchy.Add(main);
			clipsContainer = main.Q<VisualElement>("clips-container");
			buttonContainer = main.Q<VisualElement>("button-container");
			buttonContainer.Q<Button>("add-button").clicked += () => AddClicked?.Invoke();
		}

		public void UpdateList()
		{
			if (Clips == null)
			{
				foreach (var view in clipViews)
				{
					clipsContainer.Remove(view);
					clipViews.Remove(view);
				}
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
						ChangeSelection(index);
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
			clipViews[SelectedIndex].RemoveFromClassList(SelectedClass);
			SelectedIndex = index;
			clipViews[SelectedIndex].AddToClassList(SelectedClass);
		}
	}
}