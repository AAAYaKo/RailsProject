using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class EventsTrack
	{
		[SerializeField] private List<EventKey> keys = new();

		public void InsertInSequence(Sequence sequence, float frameTime)
		{
			foreach (var key in keys)
			{
				sequence.InsertCallback(key.TimePosition * frameTime, () =>
				{
					key.AnimationEvent.Invoke();
				});
			}
		}

		public void AddKey(EventKey key)
		{
			keys.Add(key);
		}

		public void RemoveKey(EventKey key)
		{
			keys.Remove(key);
		}
	}
}
