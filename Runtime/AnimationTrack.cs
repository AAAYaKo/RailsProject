using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public abstract class AnimationTrack
	{
		[SerializeField] protected List<AnimationKey> animationKeys = new();


		public void InsertInSequence(Sequence sequence, float frameTime)
		{
			if (animationKeys.Count == 0)
				return;
			if (animationKeys.Count == 1)
			{
				InsertInstantChange(animationKeys[0], sequence, frameTime);
				return;
			}
			for (int i = 0; i < animationKeys.Count - 1; i++)
			{
				var current = animationKeys[i];
				if (current.Ease.Type is RailsEase.EaseType.NoAnimation)
				{
					InsertInstantChange(current, sequence, frameTime);
					continue;
				}
				var next = animationKeys[i + 1];
				InsertTween(current, next, sequence, frameTime);
			}
			if (animationKeys[^2].Ease.Type is RailsEase.EaseType.NoAnimation)
			{
				InsertInstantChange(animationKeys[^1], sequence, frameTime);
			}
		}

		public void AddKey(AnimationKey key)
		{
			bool inserted = false;
			for (int i = 0; i < animationKeys.Count; i++)
			{
				if (animationKeys[i].TimePosition > key.TimePosition) //to maintain order
				{
					animationKeys.Insert(i + 1, key);
					inserted = true;
					break;
				}
				else if (Mathf.Approximately(animationKeys[i].TimePosition, key.TimePosition)) //replace the key with the same time position
				{
					animationKeys[i] = key;
					inserted = true;
					break;
				}
			}
			if (!inserted) //add to the end if the key is not already in the list
				animationKeys.Add(key);
		}

		public void RemoveKey(AnimationKey key)
		{
			animationKeys.Remove(key);
		}

		protected abstract void InsertInstantChange(AnimationKey key, Sequence sequence, float frameTime);
		protected abstract void InsertTween(AnimationKey keyStart, AnimationKey keyEnd, Sequence sequence, float frameTime);
	}

	[Serializable]
	public class MoveAnchorTrack : AnimationTrack
	{
		[SerializeField] public RectTransform animationComponent;


		protected override void InsertInstantChange(AnimationKey key, Sequence sequence, float frameTime)
		{
			sequence.InsertCallback(key.TimePosition * frameTime, () =>
			{
				animationComponent.anchoredPosition = key.Vector2Value;
			});
		}

		protected override void InsertTween(AnimationKey keyStart, AnimationKey keyEnd, Sequence sequence, float frameTime)
		{
			float duration = (keyEnd.TimePosition - keyStart.TimePosition) * frameTime;
			var tween = animationComponent
				.DOAnchorPos(keyEnd.Vector2Value, duration)
				.From(keyStart.Vector2Value);
			sequence.Insert(keyStart.TimePosition * frameTime, tween);
		}
	}
}
