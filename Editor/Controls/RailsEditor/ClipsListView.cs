using Rails.Editor.Context;
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
		public static readonly BindingId ClipAddCommandProperty = nameof(ClipAddCommand);
		public static readonly BindingId ClipSelectCommandProperty = nameof(ClipSelectCommand);
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
		[CreateProperty]
		public ICommand ClipAddCommand { get; set; }
		[CreateProperty]
		public ICommand ClipRemoveCommand { get; set; }
		[CreateProperty]
		public ICommand<int> ClipSelectCommand
		{
			get => clipSelectCommand;
			set
			{
				clipSelectCommand = value;
			}
		}

		[CreateProperty]
		private ICommand<int> clipSelectCommand;

		private static VisualTreeAsset template;
		private VisualElement buttonContainer;
		private VisualElement selected;
		private int? selectedIndex;
		private bool? canAdd;


		static ClipsListView()
		{
			template = Resources.Load<VisualTreeAsset>("RailsClipsView");
		}

		public ClipsListView()
		{
			template.CloneTree(this);

			container = this.Q<VisualElement>("clips-container");
			buttonContainer = this.Q<VisualElement>("button-container");
			buttonContainer.Q<Button>("add-button").clicked += () => ClipAddCommand.Execute();
			SetBinding(ClipAddCommandProperty, new CommandBinding(nameof(RailsAnimatorViewModel.ClipAddCommand)));
			SetBinding(ClipSelectCommandProperty, new CommandBinding(nameof(RailsAnimatorViewModel.ClipSelectCommand)));
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			EventBus.Subscribe<ClipClickEvent>(OnClipClick);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			EventBus.Unsubscribe<ClipClickEvent>(OnClipClick);
		}

		protected override VisualElement CreateElement()
		{
			ClipItemView view = new();
			view.SetBinding(ClipItemView.NameProperty, new ToTargetBinding(nameof(RailsClip.Name)));
			view.SetBinding(ClipItemView.RemoveCommandProperty, new CommandBinding(nameof(RailsClipViewModel.RemoveCommand)));
			return view;
		}

		protected override void UpdateList()
		{
			base.UpdateList();
			if (SelectedIndex >= (Values?.Count ?? 0) || SelectedIndex < 0)
				SelectedIndex = 0;
			ChangeSelection(SelectedIndex);
		}

		private void OnClipClick(ClipClickEvent evt)
		{
			int index = views.IndexOf(evt.Clip);
			ClipSelectCommand.Execute(index);
		}

		private void ChangeSelection(int index)
		{
			selectedIndex = index;
			if (views.Count > 0)
			{
				selected?.RemoveFromClassList(SelectedClass);
				selected = views[index];
				selected.AddToClassList(SelectedClass);
			}
		}
	}
}