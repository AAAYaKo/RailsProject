using System;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class RotateTrack : AnimationTrack<Transform, Vector3>
	{
		protected override Tween CreateTween(Vector3 start, Vector3 end, float duration)
		{
			return Reference
				.DORotate(start, duration)
				.From(end);			
		}

		protected override void InstantChange(Vector3 value)
		{
			Reference.eulerAngles = value;
		}
	}
}
