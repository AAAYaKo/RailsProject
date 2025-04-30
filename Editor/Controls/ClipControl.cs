using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class ClipControl : VisualElement
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
				controls.enabledSelf = value;
			}
		}

		private static VisualTreeAsset templateMain;
		private VisualElement controls;
		private bool? canEdit;


		public ClipControl()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsClipControl");

			templateMain.CloneTree(this);
			controls = this.Q<VisualElement>("controls");
			
			Button button = controls.Q<Button>("time");
			RailsClipPopupContent windowContent = new();

			button.clicked += () =>
			{
				windowContent.DataSource = EditorContext.Instance.SelectedClip;
				UnityEditor.PopupWindow.Show(button.worldBound, windowContent);
			};
		}
	}
}