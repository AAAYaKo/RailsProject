using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class RailsClipPopupContent : PopupWindowContent
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
		private VisualElement root;
		private object dataSource;


		public override void OnOpen()
		{
			Debug.Log("Popup opened: " + this);
		}

		public override VisualElement CreateGUI()
		{
			if (root == null)
			{
				var visualTreeAsset = Resources.Load<VisualTreeAsset>("RailsPopup");
				root = visualTreeAsset.CloneTree();
				root.dataSource = DataSource;
			}
			return root;
		}

		public override void OnClose()
		{
			Debug.Log("Popup closed: " + this);
		}
	}
}