using System;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class ScaleTrack : AnimationTrack<Transform, Vector3>
	{
		protected override Tween CreateInstantTween(Vector3 end)
		{
			return Reference.DOScale(end, 0.0001f);
		}

		protected override Tween CreateTween(Vector3 start, Vector3 end, float duration)
		{
			return Reference
				.DOScale(end, duration)
				.From(start);
		}

		protected override Vector3 GetCurrentValue_Internal()
		{
			return Reference.localScale;
		}

		protected override void InstantChange(Vector3 value)
		{
			Reference.localScale = value;
		}
	}
}
