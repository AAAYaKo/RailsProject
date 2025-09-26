﻿using Rails.Editor.Controls;

namespace Rails.Editor
{
	public struct KeyClickEvent
	{
		public TrackKeyView Key { get; }
		public bool ActionKey { get; }


		public KeyClickEvent(TrackKeyView key, bool actionKey)
		{
			Key = key;
			ActionKey = actionKey;
		}
	}
}