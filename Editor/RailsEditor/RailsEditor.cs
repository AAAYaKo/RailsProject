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
		[SerializeField] private DataStorage dataStorage = new();
		[SerializeField] private RailsAnimator currentTarget;

		private TwoPanelsView twoPanels;


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
			EditorContext.Instance.DataStorage = dataStorage;
			EditorContext.Instance.EditorWindow = this;
			EditorContext.Instance.ViewModel = new();
			if (currentTarget != null && EditorContext.Instance.CurrentTarget == null)
				EditorContext.Instance.CurrentTarget = currentTarget;
			TargetChangedHandler(EditorContext.Instance.CurrentTarget);
		}

		private void OnDisable()
		{
			EditorContext.Instance.CurrentTargetChanged -= TargetChangedHandler;
			EditorContext.Instance.DataStorage = null;
			EditorContext.Instance.EditorWindow = null;
			EditorContext.Instance.ViewModel.UnbindModel();
			EditorContext.Instance.ViewModel = null;
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

			root.dataSource = EditorContext.Instance.ViewModel;
		}

		private void TargetChangedHandler(RailsAnimator target)
		{
			EditorContext.Instance.ViewModel.UnbindModel();
			EditorContext.Instance.ViewModel.BindModel(target);
			this.currentTarget = target;
		}
	}
}