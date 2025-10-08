using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	public abstract class FocusableView : BaseView
	{
		protected FocusableView()
		{
			focusable = true;
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
		}

		private void OnMouseDown(MouseDownEvent evt)
		{
			Focus();
		}
	}
}