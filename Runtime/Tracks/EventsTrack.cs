using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class EventsTrack : BaseTrack<EventKey>
	{
		public override void InsertInSequence(Sequence sequence, float frameTime)
		{
			foreach (var key in AnimationKeys)
			{
				sequence.InsertCallback(key.TimePosition * frameTime, () =>
				{
					key.AnimationEvent.Invoke();
				});
			}
		}

		public override void InsertNewKeyAt(int frame)
		{
			AddKey(new EventKey()
			{
				TimePosition = frame,
			});
		}
	}
}
