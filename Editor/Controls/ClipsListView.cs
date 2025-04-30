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
	public partial class ClipsListView : ListObserverElement<RailsClipViewModel, VisualElement>
	{
		public static readonly BindingId SelectedIndexProperty = nameof(SelectedIndex);
		public static readonly BindingId CanAddProperty = nameof(CanAdd);
		private const string SelectedClass = "rails-clip-view--selected";


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

		private static VisualTreeAsset templateMain;
		private static VisualTreeAsset templateItem;
		private VisualElement buttonContainer;
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

			container = this.Q<VisualElement>("clips-container");
			buttonContainer = this.Q<VisualElement>("button-container");
			buttonContainer.Q<Button>("add-button").clicked += () => AddClicked?.Invoke();
		}

		protected override VisualElement CreateElement()
		{
			var view = templateItem.Instantiate();
			view.AddManipulator(new ContextualMenuManipulator(x =>
			{
				x.menu.AppendAction("Remove", x =>
				{
					RemoveClicked?.Invoke(view.IndexOf(view));
				}, DropdownMenuAction.Status.Normal);
			}));
			view.RegisterCallback<ClickEvent>(x =>
			{
				if (x.button == 0)
				{
					int index = views.IndexOf(view);
					SelectedIndex = index;
				}
			});
			return view;
		}

		protected override void ResetElement(VisualElement element)
		{
			
		}

		protected override void UpdateList()
		{
			base.UpdateList();
			if (SelectedIndex >= (Values?.Count ?? 0))
				SelectedIndex = 0;
			ChangeSelection(SelectedIndex);
		}

		private void ChangeSelection(int index)
		{
			selectedIndex = index;
			if (views.Count > 0)
			{
				selected?.RemoveFromClassList(SelectedClass);
				selected = views[index];
				selected?.AddToClassList(SelectedClass);
			}
		}
	}
}