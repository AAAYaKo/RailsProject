using UnityEngine;

namespace Rails.Editor
{
	public struct TimePositionChangedEvent
	{
		public float TimePosition { get; }


		public TimePositionChangedEvent(float timePosition)
		{
			TimePosition = timePosition;
		}
	}

	public struct SelectionBoxBeginEvent
	{
		public Rect SelectionWorldRect { get; }
		public bool ActionKey { get; }


		public SelectionBoxBeginEvent(Rect selectionWorldRect, bool actionKey)
		{
			SelectionWorldRect = selectionWorldRect;
			ActionKey = actionKey;
		}
	}

	public struct SelectionBoxChangeEvent
	{
		public Rect SelectionWorldRect { get; }
		public bool ActionKey { get; }


		public SelectionBoxChangeEvent(Rect selectionWorldRect, bool actionKey)
		{
			SelectionWorldRect = selectionWorldRect;
			ActionKey = actionKey;
		}
	}

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