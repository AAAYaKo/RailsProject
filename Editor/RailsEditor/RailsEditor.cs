using Rails.Editor.Controls;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class RailsEditor : EditorWindow
	{
		[SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

		private ThreePanelsView threePanels;
		private RailsAnimatorViewModel viewModel = new();


		[MenuItem("Window/RailsEditor")]
		public static void ShowExample()
		{
			RailsEditor wnd = GetWindow<RailsEditor>();
			var logo = Resources.Load<Texture>("Icons/logo_small");
			wnd.titleContent = new GUIContent("RailsEditor", logo);
		}

		private void OnEnable()
		{
			EditorContext.Instance.CurrentTargetChanged += TargetChangedHandler;
			TargetChangedHandler(EditorContext.Instance.CurrentTarget);
		}

		private void CreateGUI()
		{
			// Each editor window contains a root VisualElement object
			VisualElement root = rootVisualElement;

			// Instantiate UXML
			VisualElement uxml = m_VisualTreeAsset.Instantiate();
			uxml.style.flexBasis = new Length(100, LengthUnit.Percent);
			root.Add(uxml);

			threePanels = root.Q<ThreePanelsView>();

			var clips = new ClipsListView();
			threePanels.FirstPanel.Add(clips);
			clips.SetBinding("Clips", new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(RailsAnimatorViewModel.Clips)),
				bindingMode = BindingMode.ToTarget,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			clips.SetBinding("CanAdd", new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(RailsAnimatorViewModel.CanAddClip)),
				bindingMode = BindingMode.ToTarget,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			clips.AddClicked += viewModel.AddClip;
			clips.RemoveClicked += RemoveClicked;

			root.dataSource = viewModel;
		}

		private void TargetChangedHandler(RailsAnimator target)
		{
			viewModel.UnbindModel();
			viewModel.BindModel(target);
		}

		private void RemoveClicked(int index)
		{
			bool choice = EditorUtility.DisplayDialog("Remove this Clip?",
				$"Are you sure you want to delete {viewModel.Clips[index].Name}", "Delete", "Cancel");
			if (choice)
			{
				viewModel.RemoveClip(index);
			}
		}
	}
}