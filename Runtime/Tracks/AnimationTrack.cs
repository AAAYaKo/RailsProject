using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public abstract class AnimationTrack : INotifyPropertyChanged
	{
		[SerializeField] protected List<AnimationKey> animationKeys = new();
		[SerializeField] private UnityEngine.Object sceneReference;

		public List<AnimationKey> AnimationKeys
		{
			get => animationKeys;
			set
			{
				if (animationKeys == value)
					return;
				animationKeys = value;
				NotifyPropertyChanged();
			}
		}
		public UnityEngine.Object SceneReference
		{
			get => sceneReference;
			set
			{
				if (sceneReference == value)
					return;
				sceneReference = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;


		public void InsertInSequence(Sequence sequence, float frameTime)
		{
			if (animationKeys.Count == 0)
				return;
			if (SceneReference == null)
			{
				Debug.LogWarning("Track hasn't Scene Reference");
				return;
			}
			if (animationKeys.Count == 1)
			{
				InsertInstantChange(animationKeys[0], sequence, frameTime);
				return;
			}

			var sorted = animationKeys.OrderBy(x => x.TimePosition).ToArray();

			for (int i = 0; i < sorted.Length - 1; i++)
			{
				var current = sorted[i];
				if (current.Ease.Type is RailsEase.EaseType.NoAnimation)
				{
					InsertInstantChange(current, sequence, frameTime);
					continue;
				}
				var next = sorted[i + 1];
				InsertTween(current, next, sequence, frameTime);
			}
			if (sorted[^2].Ease.Type is RailsEase.EaseType.NoAnimation)
			{
				InsertInstantChange(sorted[^1], sequence, frameTime);
			}
		}

		public void AddKey(AnimationKey key)
		{
			bool inserted = false;
			for (int i = 0; i < animationKeys.Count; i++)
			{
				if (animationKeys[i].TimePosition > key.TimePosition) //to maintain order
				{
					animationKeys.Insert(i, key);
					inserted = true;
					break;
				}
				else if (animationKeys[i].TimePosition == key.TimePosition) //replace the key with the same time position
				{
					animationKeys[i] = key;
					inserted = true;
					break;
				}
			}
			if (!inserted) //add to the end if the key is not already in the list
				animationKeys.Add(key);

			NotifyPropertyChanged(nameof(AnimationKeys));
		}

		public void RemoveKey(AnimationKey key)
		{
			animationKeys.Remove(key);
			NotifyPropertyChanged(nameof(AnimationKeys));
		}

		public void InsertNewKeyAt(int frame)
		{
			int previousIndex = animationKeys.FindLastIndex(x =>
			{
				return x.TimePosition <= frame;
			});
			if (previousIndex == -1)
			{
				InsertKey(null, null, frame);
				return;
			}
			int nextIndex = previousIndex + 1;
			if (nextIndex >= animationKeys.Count)
			{
				InsertKey(animationKeys[previousIndex], null, frame);
				return;
			}
			InsertKey(animationKeys[previousIndex], animationKeys[nextIndex], frame);
		}

		private void InsertKey(AnimationKey previousKey, AnimationKey nextKey, int frame)
		{
			if (previousKey == null)
			{
				AddKey(new AnimationKey()
				{
					TimePosition = frame,
				});
				return;
			}
			if (nextKey == null)
			{
				AddKey(new AnimationKey()
				{
					SingleValue = previousKey.SingleValue,
					Vector2Value = previousKey.Vector2Value,
					Vector3Value = previousKey.Vector3Value,
					TimePosition = frame,
				});
				return;
			}
			AddKey(new AnimationKey()
			{
				SingleValue = previousKey.Ease.EasedValue(previousKey.SingleValue, nextKey.SingleValue, T()),
				Vector2Value = previousKey.Ease.EasedValue(previousKey.Vector2Value, nextKey.Vector2Value, T()),
				Vector3Value = previousKey.Ease.EasedValue(previousKey.Vector3Value, nextKey.Vector3Value, T()),
				TimePosition = frame,
			});

			float T()
			{
				return math.remap(previousKey.TimePosition, nextKey.TimePosition, 0f, 1f, frame);
			}
		}

		protected void InsertInstantChange(AnimationKey key, Sequence sequence, float frameTime)
		{
			sequence.InsertCallback(key.TimePosition * frameTime, () =>
			{
				InstantChange(key);
			});
		}

		protected void InsertTween(AnimationKey keyStart, AnimationKey keyEnd, Sequence sequence, float frameTime)
		{
			Tween tween = CreateTween(keyStart, keyEnd, frameTime);
			if (keyStart.Ease.Type is RailsEase.EaseType.EaseFunction)
			{
				tween.SetEase(keyStart.Ease.EaseFunc);
			}
			else if (keyStart.Ease.Type is RailsEase.EaseType.EaseCurve)
			{
				tween.SetEase(keyStart.Ease.CurveFunction);
			}

			sequence.Insert(keyStart.TimePosition * frameTime, tween);
		}

		protected abstract Tween CreateTween(AnimationKey keyStart, AnimationKey keyEnd, float frameTime);
		protected abstract void InstantChange(AnimationKey key);

		protected void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

		public enum ValueType
		{
			Single,
			Vector2,
			Vector3,
		}
	}
}
