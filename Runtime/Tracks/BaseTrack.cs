using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public abstract class BaseTrack<TKey> : BaseSerializableNotifier
		where TKey : BaseKey
	{
		public static readonly CollectionComparer<TKey> comparer = new();

		[SerializeField] private List<TKey> animationKeys = new();

		public List<TKey> AnimationKeys
		{
			get => animationKeys;
			set => SetProperty(ref animationKeys, value, comparer);
		}

#if UNITY_EDITOR
		private readonly List<TKey> animationKeysCopy = new();
#endif


		public override void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			CopyList(AnimationKeys, animationKeysCopy);
#endif
		}

		public override void OnAfterDeserialize()
		{
#if UNITY_EDITOR
			if (NotifyIfChanged(AnimationKeys, animationKeysCopy, nameof(AnimationKeys), comparer))
				CopyList(AnimationKeys, animationKeysCopy);
#endif
		}

		public abstract void InsertInSequence(Sequence sequence, float frameTime);

		public void AddKey(TKey key)
		{
			AddKeyWithoutNotify(key);
			NotifyPropertyChanged(nameof(AnimationKeys));
		}

		public void RemoveKey(TKey key)
		{
			RemoveKeyWithoutNotify(key);
			NotifyPropertyChanged(nameof(AnimationKeys));
		}

		public void RemoveKeys(IEnumerable<TKey> toRemove)
		{
			foreach (var key in toRemove)
				RemoveKeyWithoutNotify(key);
			NotifyPropertyChanged(nameof(AnimationKeys));
		}

		public void MoveMultipleKeys(Dictionary<int, int> keysFramePositions)
		{
			foreach (var request in keysFramePositions)
				animationKeys[request.Key].SetTimePositionWithoutNotify(request.Value);

			List<TKey> keysToRemove = new();
			foreach (var request in keysFramePositions)
			{
				var otherKey = animationKeys.Find(x => x != animationKeys[request.Key] && x.TimePosition == request.Value);
				if (otherKey != null)
					keysToRemove.Add(otherKey);
			}
			keysToRemove.ForEach(x => RemoveKeyWithoutNotify(x));

			animationKeys.Sort((x, y) => x.TimePosition.CompareTo(y.TimePosition));

			NotifyPropertyChanged(nameof(AnimationKeys));
		}

		public abstract void InsertNewKeyAt(int frame);

		private void AddKeyWithoutNotify(TKey key)
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
		}

		private void RemoveKeyWithoutNotify(TKey key)
		{
			animationKeys.Remove(key);
		}
	}
}
