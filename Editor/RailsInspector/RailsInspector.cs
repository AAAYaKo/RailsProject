using Rails.Editor.Context;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class RailsInspector : EditorWindow
	{
		[SerializeField] private StyleSheet darkTheme = default;
		[SerializeField] private StyleSheet lightTheme = default;
		[SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;


		private void OnEnable()
		{
			EditorContext.Instance.Inspector = this;
		}

		private void OnDisable()
		{
			EditorContext.Instance.Inspector = null;
		}

		private void OnDestroy()
		{
			if (EditorContext.Instance.Editor != null)
				EditorContext.Instance.Editor.Close();
		}

		public void CreateGUI()
		{
			VisualElement root = rootVisualElement;

			// Instantiate UXML
			VisualElement uxml = m_VisualTreeAsset.Instantiate();
			uxml.styleSheets.Add(EditorGUIUtility.isProSkin ? darkTheme : lightTheme);
			uxml.style.flexBasis = new Length(100, LengthUnit.Percent);
			root.Add(uxml);

			root.dataSource = EditorContext.Instance.ViewModel;
		}
	}
}