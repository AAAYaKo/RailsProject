using Rails.Editor.Controls;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class RailsEditor : EditorWindow
	{
		[SerializeField] private StyleSheet darkTheme = default;
		[SerializeField] private StyleSheet lightTheme = default;
		[SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
		[SerializeField] private int selectedClip = 0;

		private TwoPanelsView twoPanels;
		private RailsAnimatorViewModel viewModel => EditorContext.Instance.ViewModel;


		[MenuItem("Window/RailsEditor")]
		public static void OpenWindow()
		{
			RailsEditor wnd = GetWindow<RailsEditor>();
			var logo = EditorGUIUtility.pixelsPerPoint > 1 ? Resources.Load<Texture>("Icons/logo@2x") : Resources.Load<Texture>("Icons/logo");
			wnd.titleContent = new GUIContent("RailsEditor", logo);
		}

		private void OnEnable()
		{
			EditorContext.Instance.CurrentTargetChanged += TargetChangedHandler;
			EditorContext.Instance.SelectedClipChanged += SelectedClipChangedHandler;
			TargetChangedHandler(EditorContext.Instance.CurrentTarget);
		}

		private void OnDisable()
		{
			EditorContext.Instance.CurrentTargetChanged -= TargetChangedHandler;
			EditorContext.Instance.SelectedClipChanged -= SelectedClipChangedHandler;
		}

		private void OnValidate()
		{
			if (selectedClip != EditorContext.Instance.ViewModel.SelectedClipIndex &&
				selectedClip < EditorContext.Instance.ViewModel.Clips.Count && selectedClip > 0)
				EditorContext.Instance.ViewModel.SelectedClipIndex = selectedClip;
		}

		private void CreateGUI()
		{
			// Each editor window contains a root VisualElement object
			VisualElement root = rootVisualElement;

			// Instantiate UXML
			VisualElement uxml = m_VisualTreeAsset.Instantiate();
			uxml.styleSheets.Add(EditorGUIUtility.isProSkin ? darkTheme : lightTheme);
			uxml.style.flexBasis = new Length(100, LengthUnit.Percent);
			root.Add(uxml);

			twoPanels = root.Q<TwoPanelsView>();

			Resources.Load<VisualTreeAsset>("RailsFirstPage").CloneTree(twoPanels.FirstPanel);
			var clips = twoPanels.FirstPanel.Q<ClipsListView>("clips-view");

			ClipView clipView = new();
			clipView.style.width = new Length(100, LengthUnit.Percent);
			clipView.style.height = new Length(100, LengthUnit.Percent);
			clipView.style.flexGrow = 1;
			twoPanels.SecondPanel.Add(clipView);

			root.dataSource = viewModel;
		}

		private void TargetChangedHandler(RailsAnimator target)
		{
			if (selectedClip < EditorContext.Instance.ViewModel.Clips.Count && selectedClip > 0)
				EditorContext.Instance.ViewModel.SelectedClipIndex = selectedClip;
		}

		private void SelectedClipChangedHandler(int selected)
		{
			EditorContext.Instance.Record(this, "Selected Clip Changed");
			selectedClip = selected;
		}
	}
}