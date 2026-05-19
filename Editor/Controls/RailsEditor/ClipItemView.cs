using System.Collections;
using Rails.Editor.Context;
using Unity.EditorCoroutines.Editor;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class ClipItemView : BaseView
	{
		public static readonly BindingId RemoveCommandProperty = nameof(RemoveCommand);
		public static readonly BindingId NameProperty = nameof(Name);

		private static VisualTreeAsset template;

		[CreateProperty]
		public ICommand RemoveCommand { get; set; }
		[CreateProperty]
		public string Name { get; set; }

		private TextField nameField;
		private Label nameLabel;
		private IVisualElementScheduledItem delayedClick;


		static ClipItemView()
		{
			template = Resources.Load<VisualTreeAsset>("RailsClip");
		}

		public ClipItemView()
		{
			template.CloneTree(this);

			nameLabel = this.Q<Label>("name");
			nameField = this.Q<TextField>("nameField");
			nameField.style.display = DisplayStyle.None;

			this.AddManipulator(new ContextualMenuManipulator(x =>
			{
				x.menu.AppendAction("Rename", x =>
				{
					nameField.style.display = DisplayStyle.Flex;
					nameLabel.style.display = DisplayStyle.None;
					nameField.Focus();
				}, DropdownMenuAction.Status.Normal);
				x.menu.AppendAction("Remove", x =>
				{
					bool choice = EditorUtility.DisplayDialog("Remove this Clip?",
	$"Are you sure you want to delete {Name}", "Delete", "Cancel");
					if (choice)
					{
						RemoveCommand.Execute();
					}
				}, DropdownMenuAction.Status.Normal);
			}));
			RegisterCallback<ClickEvent>(x =>
			{
				if (x.button == 0)
				{
					if (x.clickCount == 1)
					{
						delayedClick = schedule.Execute(() =>
						{
							EventBus.Publish(new ClipClickEvent(this));
							delayedClick = null;
						}).StartingIn(200);
					}
					else
					{
						delayedClick?.Pause();
						delayedClick = null;
						nameField.style.display = DisplayStyle.Flex;
						nameLabel.style.display = DisplayStyle.None;
						nameField.Focus();
					}
				}
			});

			nameField.RegisterValueChangedCallback(OnNameFieldChanged);
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			nameField.RegisterCallback<FocusOutEvent>(OnNameFieldFocusOut);
			nameField.RegisterValueChangedCallback(OnNameFieldChanged);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			nameField.UnregisterCallback<FocusOutEvent>(OnNameFieldFocusOut);
			nameField.UnregisterValueChangedCallback(OnNameFieldChanged);
		}

		private void OnNameFieldFocusOut(FocusOutEvent evt)
		{
			nameField.style.display = DisplayStyle.None;
			nameLabel.style.display = DisplayStyle.Flex;
		}

		private void OnNameFieldChanged(ChangeEvent<string> evt)
		{
			nameField.Blur();
		}
	}
}