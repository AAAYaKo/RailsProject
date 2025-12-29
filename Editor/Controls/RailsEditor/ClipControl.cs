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
		[UxmlAttribute("loop-icon-style"), CreateProperty]
		public string LoopIconStyle
		{
			get => loopIconStyle;
			set
			{
				if (loopIconStyle == value)
					return;
				loopIcon.RemoveFromClassList(loopIconStyle);
				loopIconStyle = value;
				loopIcon.AddToClassList(loopIconStyle);
			}
		}

		private static VisualTreeAsset templateMain;
		private VisualElement controls;
		private VisualElement loopIcon;
		private bool? canEdit;
		private string loopIconStyle;

		static ClipControl()
		{
			templateMain = Resources.Load<VisualTreeAsset>("RailsClipControl");
		}

		public ClipControl()
		{
			templateMain.CloneTree(this);
			controls = this.Q<VisualElement>("controls");

			VisualElement time = controls.Q<VisualElement>("time");
			VisualElement loop = controls.Q<VisualElement>("loop");
			loopIcon = loop.Q<Image>("loop-icon");
			RailsClipTimePopupContent timeContent = new();
			RailsClipLoopPopupContent loopContent = new();

			time.RegisterCallback<ClickEvent>(x =>
			{
				if (x is { clickCount: 1, button: 0 })
				{
					timeContent.DataSource = EditorContext.Instance.SelectedClip;
					UnityEditor.PopupWindow.Show(time.worldBound, timeContent);
				}
			});
			loop.RegisterCallback<ClickEvent>(x =>
			{
				if (x is { clickCount: 1, button: 0 })
				{
					loopContent.DataSource = EditorContext.Instance.SelectedClip;
					UnityEditor.PopupWindow.Show(loop.worldBound, loopContent);
				}
			});
		}
	}
}