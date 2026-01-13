using UnityEngine;

namespace Rails.Editor
{
	public readonly struct SelectionBoxCompleteEvent
	{
		public Rect SelectionWorldRect { get; }
		public bool ActionKey { get; }


		public SelectionBoxCompleteEvent(Rect selectionWorldRect, bool actionKey)
		{
			SelectionWorldRect = selectionWorldRect;
			ActionKey = actionKey;
		}
	}
}