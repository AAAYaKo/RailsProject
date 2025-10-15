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


		static ClipControl()
		{
			templateMain = Resources.Load<VisualTreeAsset>("RailsClipControl");
		}

		public ClipControl()
		{
			templateMain.CloneTree(this);
			controls = this.Q<VisualElement>("controls");

			VisualElement time = controls.Q<VisualElement>("time");
			RailsClipPopupContent windowContent = new();

			time.RegisterCallback<ClickEvent>(x =>
			{
				if (x.clickCount == 1 && x.button == 0)
				{
					windowContent.DataSource = EditorContext.Instance.SelectedClip;
					UnityEditor.PopupWindow.Show(time.worldBound, windowContent);
				}
			});
		}
	}
}