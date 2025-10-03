using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	public abstract class BaseView : VisualElement
	{
		protected static DataStorage Storage => EditorContext.Instance.DataStorage;

		protected BaseView()
		{
			RegisterCallback<AttachToPanelEvent>(OnAttach);
			RegisterCallback<DetachFromPanelEvent>(OnDetach);
		}

		protected virtual void OnAttach(AttachToPanelEvent evt)
		{
		}

		protected virtual void OnDetach(DetachFromPanelEvent evt)
		{
		}
	}
}