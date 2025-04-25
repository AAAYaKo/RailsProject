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
		private RailsAnimatorViewModel viewModel => EditorContext.Instance.ViewModel;


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

			VisualElement firstPage = Resources.Load<VisualTreeAsset>("RailsFirstPage").Instantiate();
			var clips = firstPage.Q<ClipsListView>("clips-view");
			threePanels.FirstPanel.Add(firstPage);

			clips.AddClicked += viewModel.AddClip;
			clips.RemoveClicked += RemoveClipClicked;

			VisualElement secondPage = Resources.Load<VisualTreeAsset>("RailsSecondPage").Instantiate();
			threePanels.SecondPanel.Add(secondPage);

			root.dataSource = viewModel;
		}

		private void TargetChangedHandler(RailsAnimator target)
		{

		}

		private void RemoveClipClicked(int index)
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