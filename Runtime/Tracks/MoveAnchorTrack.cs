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
				.DOAnchorPos(start, duration)
				.From(end);
		}

		protected override void InstantChange(Vector2 value)
		{
			Reference.anchoredPosition = value;
		}
	}
}
