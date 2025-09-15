using System;
using DG.Tweening;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class FadeTrack : AnimationTrack
	{
		public CanvasGroup Reference => (CanvasGroup)SceneReference;


		protected override Tween CreateTween(AnimationKey keyStart, AnimationKey keyEnd, float frameTime)
		{
			float duration = (keyEnd.TimePosition - keyStart.TimePosition) * frameTime;
			return Reference
				.DOFade(keyEnd.SingleValue, duration)
				.From(keyStart.SingleValue);
		}

		protected override void InstantChange(AnimationKey key)
		{
			Reference.alpha = key.SingleValue;
		}
	}
}
