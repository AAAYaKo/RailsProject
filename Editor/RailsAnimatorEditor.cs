using System;
using Rails.Editor.Context;
using Rails.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	[CustomEditor(typeof(RailsAnimator))]
	public class RailsAnimatorEditor : UnityEditor.Editor
	{
		[SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
		public static event Action AnimatorDestroyed;


		private void OnDisable()
		{
			if (target == null)
				AnimatorDestroyed?.Invoke();
		}

		public override VisualElement CreateInspectorGUI()
		{
			VisualElement myInspector = new();

			if (m_VisualTreeAsset != null)
				m_VisualTreeAsset.CloneTree(myInspector);

			myInspector.Q<Button>().clicked += () =>
			{
				if (EditorContext.Instance.Editor == null)
					RailsEditor.OpenWindow();
				else
					EditorWindow.FocusWindowIfItsOpen<RailsEditor>();
			};

			return myInspector;
		}
	}
}