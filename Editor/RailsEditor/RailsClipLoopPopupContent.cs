using Rails.Editor.Controls;
using Rails.Editor.ViewModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class RailsClipLoopPopupContent : PopupWindowContent 
	{
		public BindingId Choices = nameof(LoopTypeField.choices);

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


		static RailsClipLoopPopupContent()
		{
			mainTree = Resources.Load<VisualTreeAsset>("RailsLoopPopup");
		}

		public override VisualElement CreateGUI()
		{
			if (root == null)
			{
				root = mainTree.CloneTree();
				var type =root.Q<LoopTypeField>();
				type.SetBinding(Choices, new ToTargetBinding(nameof(RailsClipViewModel.LoopTypes)));
				root.dataSource = DataSource;
			}
			return root;
		}
	}
}