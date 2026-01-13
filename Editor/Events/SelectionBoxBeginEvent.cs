using UnityEngine;

namespace Rails.Editor
{
	public readonly struct SelectionBoxBeginEvent
	{
		public Rect SelectionWorldRect { get; }
		public bool ActionKey { get; }


		public SelectionBoxBeginEvent(Rect selectionWorldRect, bool actionKey)
		{
			SelectionWorldRect = selectionWorldRect;
			ActionKey = actionKey;
		}
	}
}