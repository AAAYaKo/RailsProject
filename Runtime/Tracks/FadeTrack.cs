using System;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class FadeTrack : AnimationTrack<CanvasGroup, float>
	{
		protected override Tween CreateTween(float start, float end, float duration)
		{
			return Reference
				.DOFade(end, duration)
				.From(start);
		}

		protected override float GetCurrentValue_Internal()
		{
			return Reference.alpha;
		}

		protected override void InstantChange(float value)
		{
			Reference.alpha = value;
		}
	}
}
