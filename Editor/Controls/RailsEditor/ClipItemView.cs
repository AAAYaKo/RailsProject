using Rails.Editor.Context;
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


		static ClipItemView()
		{
			template = Resources.Load<VisualTreeAsset>("RailsClip");
		}

		public ClipItemView()
		{
			template.CloneTree(this);

			this.AddManipulator(new ContextualMenuManipulator(x =>
			{
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
					EventBus.Publish(new ClipClickEvent(this));
			});
		}
	}
}