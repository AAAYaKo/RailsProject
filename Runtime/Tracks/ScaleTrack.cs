using System;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class ScaleTrack : AnimationTrack
	{
		public Transform Reference => (Transform)SceneReference;

		protected override Tween CreateTween(AnimationKey keyStart, AnimationKey keyEnd, float frameTime)
		{
			float duration = (keyEnd.TimePosition - keyStart.TimePosition) * frameTime;
			return Reference
				.DOScale(keyEnd.Vector3Value, duration)
				.From(keyStart.Vector3Value);
		}

		protected override void InstantChange(AnimationKey key)
		{
			Reference.localScale = key.Vector3Value;
		}
	}
}
