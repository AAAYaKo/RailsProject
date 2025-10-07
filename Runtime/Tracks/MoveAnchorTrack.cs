using System;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class MoveAnchorTrack : AnimationTrack
	{
		public RectTransform Reference => (RectTransform)SceneReference;


		protected override Tween CreateTween(AnimationKey keyStart, AnimationKey keyEnd, float frameTime)
		{
			float duration = (keyEnd.TimePosition - keyStart.TimePosition) * frameTime;
			return Reference
				.DOAnchorPos(keyEnd.Vector2Value, duration)
				.From(keyStart.Vector2Value);
		}

		protected override void InstantChange(AnimationKey key)
		{
			Reference.anchoredPosition = key.Vector2Value;
		}
	}
}
