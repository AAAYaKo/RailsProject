using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Unity.Properties;
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
					animationKeys.Insert(i + 1, key);
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

		protected void InsertInstantChange(AnimationKey key, Sequence sequence, float frameTime)
		{
			sequence.InsertCallback(key.TimePosition * frameTime, () =>
			{
				InstantChange(key);
			});
		}

		protected abstract void InsertTween(AnimationKey keyStart, AnimationKey keyEnd, Sequence sequence, float frameTime);
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
