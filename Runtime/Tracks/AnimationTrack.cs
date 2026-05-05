using System;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public abstract class AnimationTrack<TReference, TValue> : BaseTrack<IAnimationKey>, IAnimationTrack
		where TReference : UnityEngine.Object
		where TValue : struct
	{
		[SerializeField, DontCreateProperty] private TReference sceneReference;

		[CreateProperty]
		public UnityEngine.Object SceneReference
		{
			get => sceneReference;
			set
			{
				if (value is TReference reference)
					sceneReference = reference;
				else if (value == null)
					sceneReference = null;
				else
					throw new InvalidCastException($"Cannot cast {value} to {typeof(TReference).Name}");
			}
		}
		[CreateProperty]
		protected TReference Reference => sceneReference;

#if UNITY_EDITOR
		[NonSerialized] protected TValue storedValue;
#endif


		public void SaveCurrentValue()
		{
			if (Reference == null)
				return;
			storedValue = GetCurrentValue();
		}

		public void RestoreValue()
		{
			if (Reference == null)
				return;
			InstantChange(storedValue);
		}

		public override void InsertInSequence(Sequence sequence, float frameTime, bool recomputeDrivers)
		{
			if (AnimationKeys.Count == 0)
				return;
			if (SceneReference == null)
			{
				Debug.LogWarning("Track hasn't Scene Reference");
				return;
			}
			if (AnimationKeys.Count == 1)
			{
				InsertInstantChange(AnimationKeys[0], sequence, frameTime);
				return;
			}

			var sorted = AnimationKeys.OrderBy(x => x.TimePosition).ToArray();
			if (recomputeDrivers)
			{
				for (int i = 0; i < sorted.Length - 1; i++)
					sorted[i].Driver?.UpdateValue();
			}

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

		public override void InsertNewKeyAt(int frame)
		{
			int previousIndex = AnimationKeys.FindLastIndex(x =>
			{
				return x.TimePosition <= frame;
			});
			if (previousIndex == -1)
			{
				InsertNewKey(null, null, frame, false);
				return;
			}
			int nextIndex = previousIndex + 1;
			if (nextIndex >= AnimationKeys.Count)
			{
				InsertNewKey(AnimationKeys[previousIndex], null, frame, false);
				return;
			}
			InsertNewKey(AnimationKeys[previousIndex], AnimationKeys[nextIndex], frame, false);
		}

		public void InsertNewKeyAt(int frame, object value, bool constrainedProportions)
		{
			int previousIndex = AnimationKeys.FindLastIndex(x =>
			{
				return x.TimePosition <= frame;
			});
			if (previousIndex == -1)
			{
				InsertNewKey(null, null, frame, value, constrainedProportions);
				return;
			}
			int nextIndex = previousIndex + 1;
			if (nextIndex >= AnimationKeys.Count)
			{
				InsertNewKey(AnimationKeys[previousIndex], null, frame, value, constrainedProportions);
				return;
			}
			InsertNewKey(AnimationKeys[previousIndex], AnimationKeys[nextIndex], frame, value, constrainedProportions);
		}

		public void RecomputeDrivers()
		{
			AnimationKeys.ForEach(x => x.Driver?.UpdateValue());
		}

		protected IAnimationKey CreateKey(int frame, bool constrainedProportions, TValue value = default)
		{
			return new AnimationKey<TValue>()
			{
				TimePosition = frame,
				Value = value,
				ConstrainedProportions = constrainedProportions,
			};
		}

		private void InsertNewKey(IAnimationKey previousKey, IAnimationKey nextKey, int frame, bool constrainedProportions)
		{
			if (previousKey == null)
			{
				AddKey(CreateKey(frame, constrainedProportions));
				return;
			}
			if (nextKey == null)
			{
				AddKey(CreateKey(frame, constrainedProportions, (TValue)previousKey.Value));
				return;
			}
			AddKey(CreateKey(frame, constrainedProportions, (TValue)previousKey.Ease.EasedValue(previousKey.Value, nextKey.Value, Time())));

			float Time()
			{
				return math.remap(previousKey.TimePosition, nextKey.TimePosition, 0f, 1f, frame);
			}
		}

		private void InsertNewKey(IAnimationKey previousKey, IAnimationKey nextKey, int frame, object value, bool constrainedProportions)
		{
			if (value is TValue valueT)
			{
				if (previousKey == null)
				{
					AddKey(CreateKey(frame, constrainedProportions, valueT));
					return;
				}
				if (nextKey == null)
				{
					AddKey(CreateKey(frame, constrainedProportions, valueT));
					return;
				}
				AddKey(CreateKey(frame, constrainedProportions, valueT));
			}
			else
				throw new InvalidCastException($"Cannot cast {value} to {typeof(TValue).Name}");
		}

		protected abstract TValue GetCurrentValue();

		protected void InsertInstantChange(IAnimationKey key, Sequence sequence, float frameTime)
		{
			sequence.InsertCallback(key.TimePosition * frameTime, () =>
			{
				InstantChange(key);
			});
		}

		protected void InsertTween(IAnimationKey keyStart, IAnimationKey keyEnd, Sequence sequence, float frameTime)
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

		protected abstract Tween CreateTween(TValue start, TValue end, float duration);
		protected abstract void InstantChange(TValue value);

		protected Tween CreateTween(IAnimationKey keyStart, IAnimationKey keyEnd, float frameTime)
		{
			if (keyStart.Value is TValue start && keyEnd.Value is TValue end)
			{
				float duration = (keyEnd.TimePosition - keyStart.TimePosition) * frameTime;
				return CreateTween(start, end, duration);
			}
			else
				throw new InvalidCastException($"Cannot cast {keyStart.Value} and {keyEnd.Value} to float");
		}

		protected void InstantChange(IAnimationKey key)
		{
			if (key.Value is TValue value)
				InstantChange(value);
			else
				throw new InvalidCastException($"Cannot cast {key.Value} to {typeof(TValue).Name}");
		}
	}

	public interface IAnimationTrack : IBaseTrack<IAnimationKey>
	{
		public UnityEngine.Object SceneReference { get; set; }


		public void SaveCurrentValue();

		public void RestoreValue();

		public void InsertNewKeyAt(int frame, object value, bool constrainedProportions);

		public void RecomputeDrivers();

		public enum ValueType
		{
			Single,
			Vector2,
			Vector3,
		}
	}
}
