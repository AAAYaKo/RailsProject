using System;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class ScaleTrack : AnimationTrack<Transform, Vector3>
	{
		protected override Tween CreateTween(Vector3 start, Vector3 end, float duration)
		{
			return Reference
				.DOScale(start, duration)
				.From(end);
		}

		protected override void InstantChange(Vector3 value)
		{
			Reference.localScale = value;
		}
	}
}
