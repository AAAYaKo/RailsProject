using System;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public abstract class AnimationTrack<TReference, TValue> : BaseTrack<IAnimationKey>, IAnimationTrack
		where TReference : UnityEngine.Object
	{
		[SerializeField] private TReference sceneReference;

		public UnityEngine.Object SceneReference
		{
			get => sceneReference;
			set
			{
				if (value is TReference reference)
					SetProperty(ref sceneReference, reference);
				else
					throw new InvalidCastException($"Cannot cast {value} to {typeof(TReference).Name}");
			}
		}
		protected TReference Reference => sceneReference;

#if UNITY_EDITOR
		[NonSerialized] private TReference sceneReferenceCopy;
#endif


		public override void InsertInSequence(Sequence sequence, float frameTime)
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
				InsertNewKey(null, null, frame);
				return;
			}
			int nextIndex = previousIndex + 1;
			if (nextIndex >= AnimationKeys.Count)
			{
				InsertNewKey(AnimationKeys[previousIndex], null, frame);
				return;
			}
			InsertNewKey(AnimationKeys[previousIndex], AnimationKeys[nextIndex], frame);
		}

		public void InsertNewKeyAt(int frame, object value)
		{
			int previousIndex = AnimationKeys.FindLastIndex(x =>
			{
				return x.TimePosition <= frame;
			});
			if (previousIndex == -1)
			{
				InsertNewKey(null, null, frame, value);
				return;
			}
			int nextIndex = previousIndex + 1;
			if (nextIndex >= AnimationKeys.Count)
			{
				InsertNewKey(AnimationKeys[previousIndex], null, frame, value);
				return;
			}
			InsertNewKey(AnimationKeys[previousIndex], AnimationKeys[nextIndex], frame, value);
		}

		protected IAnimationKey CreateKey(int frame, TValue value = default)
		{
			return new AnimationKey<TValue>()
			{
				TimePosition = frame,
				Value = value,
			};
		}

		private void InsertNewKey(IAnimationKey previousKey, IAnimationKey nextKey, int frame)
		{
			if (previousKey == null)
			{
				AddKey(CreateKey(frame));
				return;
			}
			if (nextKey == null)
			{
				AddKey(CreateKey(frame, (TValue)previousKey.Value));
				return;
			}
			AddKey(CreateKey(frame, (TValue)previousKey.Ease.EasedValue(previousKey.Value, nextKey.Value, Time())));

			float Time()
			{
				return math.remap(previousKey.TimePosition, nextKey.TimePosition, 0f, 1f, frame);
			}
		}

		private void InsertNewKey(IAnimationKey previousKey, IAnimationKey nextKey, int frame, object value)
		{
			if (value is TValue valueT)
			{
				if (previousKey == null)
				{
					AddKey(CreateKey(frame, valueT));
					return;
				}
				if (nextKey == null)
				{
					AddKey(CreateKey(frame, valueT));
					return;
				}
				AddKey(CreateKey(frame, valueT));
			}
			else
				throw new InvalidCastException($"Cannot cast {value} to {typeof(TValue).Name}");
		}

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

		public override void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			base.OnBeforeSerialize();
			sceneReferenceCopy = sceneReference;
#endif
		}

		public override void OnAfterDeserialize()
		{
#if UNITY_EDITOR
			try
			{
				base.OnAfterDeserialize();
				if (NotifyIfChanged(SceneReference, sceneReferenceCopy, nameof(SceneReference)))
					sceneReferenceCopy = sceneReference;
			}
			catch
			{

			}
#endif
		}
	}

	public interface IAnimationTrack : IBaseTrack<IAnimationKey>
	{
		public UnityEngine.Object SceneReference { get; set; }


		public void InsertNewKeyAt(int frame, object value);

		public enum ValueType
		{
			Single,
			Vector2,
			Vector3,
		}
	}
}
