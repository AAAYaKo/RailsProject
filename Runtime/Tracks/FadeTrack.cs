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
				.DOFade(start, duration)
				.From(end);
		}

		protected override void InstantChange(float value)
		{
			Reference.alpha = value;
		}
	}
}
