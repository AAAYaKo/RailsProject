using System;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class MoveAnchorTrack : AnimationTrack<RectTransform, Vector2>
	{
		protected override Tween CreateTween(Vector2 start, Vector2 end, float duration)
		{
			return Reference
				.DOAnchorPos(end, duration)
				.From(start);
		}

		protected override Vector2 GetCurrentValue_Internal()
		{
			return Reference.anchoredPosition;
		}

		protected override void InstantChange(Vector2 value)
		{
			Reference.anchoredPosition = value;
		}
	}
}
