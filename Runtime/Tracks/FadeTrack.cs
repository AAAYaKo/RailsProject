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


		protected override void InsertTween(AnimationKey keyStart, AnimationKey keyEnd, Sequence sequence, float frameTime)
		{
			float duration = (keyEnd.TimePosition - keyStart.TimePosition) * frameTime;
			Tween tween = Reference
				.DOFade(keyEnd.SingleValue, duration)
				.From(keyStart.SingleValue);
			sequence.Insert(keyStart.TimePosition * frameTime, tween);
		}

		protected override void InstantChange(AnimationKey key)
		{
			Reference.alpha = key.SingleValue;
		}
	}
}
