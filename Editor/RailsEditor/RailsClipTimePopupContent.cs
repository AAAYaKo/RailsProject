using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class RailsClipTimePopupContent : PopupWindowContent
	{
		public object DataSource
		{
			get => dataSource;
			set
			{
				dataSource = value;
				if (root != null)
					root.dataSource = dataSource;
			}
		}
		private static VisualTreeAsset mainTree;
		private VisualElement root;
		private object dataSource;


		static RailsClipTimePopupContent()
		{
			mainTree = Resources.Load<VisualTreeAsset>("RailsTimePopup");
		}

		public override VisualElement CreateGUI()
		{
			if (root == null)
			{
				root = mainTree.CloneTree();
				root.dataSource = DataSource;
			}
			return root;
		}
	}
}