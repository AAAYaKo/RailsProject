using UnityEngine;

namespace Rails.Editor
{
	public readonly struct SelectionBoxChangeEvent
	{
		public Rect SelectionWorldRect { get; }
		public bool ActionKey { get; }


		public SelectionBoxChangeEvent(Rect selectionWorldRect, bool actionKey)
		{
			SelectionWorldRect = selectionWorldRect;
			ActionKey = actionKey;
		}
	}
}