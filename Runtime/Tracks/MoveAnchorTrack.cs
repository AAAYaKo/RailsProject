using System;
using DG.Tweening;
using Unity.Properties;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class MoveAnchorTrack : AnimationTrack
	{
		public RectTransform Reference => (RectTransform)SceneReference;


		protected override void InsertTween(AnimationKey keyStart, AnimationKey keyEnd, Sequence sequence, float frameTime)
		{
			float duration = (keyEnd.TimePosition - keyStart.TimePosition) * frameTime;
			var tween = Reference
				.DOAnchorPos(keyEnd.Vector2Value, duration)
				.From(keyStart.Vector2Value);
			sequence.Insert(keyStart.TimePosition * frameTime, tween);
		}

		protected override void InstantChange(AnimationKey key)
		{
			Reference.anchoredPosition = key.Vector2Value;
		}
	}
}
