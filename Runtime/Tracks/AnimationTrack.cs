using System;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public abstract class AnimationTrack : BaseTrack<AnimationKey>
	{
		[SerializeField] private UnityEngine.Object sceneReference;

		public UnityEngine.Object SceneReference
		{
			get => sceneReference;
			set => SetProperty(ref sceneReference, value);
		}

#if UNITY_EDITOR
		[NonSerialized] private UnityEngine.Object sceneReferenceCopy;
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

		public void InsertNewKeyAt(int frame, float singleValue, Vector2 vector2Value, Vector3 vector3Value)
		{
			int previousIndex = AnimationKeys.FindLastIndex(x =>
			{
				return x.TimePosition <= frame;
			});
			if (previousIndex == -1)
			{
				InsertNewKey(null, null, frame, singleValue, vector2Value, vector3Value);
				return;
			}
			int nextIndex = previousIndex + 1;
			if (nextIndex >= AnimationKeys.Count)
			{
				InsertNewKey(AnimationKeys[previousIndex], null, frame, singleValue, vector2Value, vector3Value);
				return;
			}
			InsertNewKey(AnimationKeys[previousIndex], AnimationKeys[nextIndex], frame, singleValue, vector2Value, vector3Value);
		}

		private void InsertNewKey(AnimationKey previousKey, AnimationKey nextKey, int frame)
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

		private void InsertNewKey(AnimationKey previousKey, AnimationKey nextKey, int frame, float singleValue, Vector2 vector2Value, Vector3 vector3Value)
		{
			if (previousKey == null)
			{
				AddKey(new AnimationKey()
				{
					SingleValue = singleValue,
					Vector2Value = vector2Value,
					Vector3Value = vector3Value,
					TimePosition = frame,
				});
				return;
			}
			if (nextKey == null)
			{
				AddKey(new AnimationKey()
				{
					SingleValue = singleValue,
					Vector2Value = vector2Value,
					Vector3Value = vector3Value,
					TimePosition = frame,
				});
				return;
			}
			AddKey(new AnimationKey()
			{
				SingleValue = singleValue,
				Vector2Value = vector2Value,
				Vector3Value = vector3Value,
				TimePosition = frame,
			});
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

		public enum ValueType
		{
			Single,
			Vector2,
			Vector3,
		}
	}
}
