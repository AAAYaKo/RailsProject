﻿using UnityEngine;

namespace Rails.Editor
{
	public struct SelectionBoxCompleteEvent
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