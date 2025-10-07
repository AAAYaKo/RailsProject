using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public abstract class BaseTrack<TKey> : INotifyPropertyChanged
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
		where TKey : BaseKey
	{
		public static readonly CollectionComparer<TKey> comparer = new();

		[SerializeField] private List<TKey> animationKeys = new();

		public List<TKey> AnimationKeys
		{
			get => animationKeys;
			set
			{
				if (comparer.Equals(animationKeys, value))
					return;
				animationKeys = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

#if UNITY_EDITOR
		private readonly List<TKey> animationKeysCopy = new();
#endif

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

		protected void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

#if UNITY_EDITOR
		public virtual void OnBeforeSerialize()
		{
			animationKeysCopy.Clear();
			animationKeysCopy.AddRange(AnimationKeys);
		}

		public virtual void OnAfterDeserialize()
		{
			if (!comparer.Equals(animationKeysCopy, AnimationKeys))
				NotifyPropertyChanged(nameof(AnimationKeys));
			animationKeysCopy.Clear();
			animationKeysCopy.AddRange(AnimationKeys);
		}
#endif
	}
}
