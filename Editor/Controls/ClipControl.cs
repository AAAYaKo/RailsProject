using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class ClipControl : VisualElement
	{
		private static VisualTreeAsset templateMain;

		public ClipControl()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsClipControl");

			var main = templateMain.Instantiate();
			hierarchy.Add(main);

			Button button = main.Q<Button>("time");
			RailsClipPopupContent windowContent = new();

			button.clicked += () =>
			{
				windowContent.DataSource = EditorContext.Instance.SelectedClip;
				UnityEditor.PopupWindow.Show(button.worldBound, windowContent);
			};
		}
	}
}