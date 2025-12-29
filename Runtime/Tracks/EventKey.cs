using System;
using Rails.Runtime.Callback;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public class EventKey : BaseKey
	{
		[SerializeField] private SerializableEvent animationEvent = new();

		public SerializableEvent AnimationEvent => animationEvent;

#if UNITY_EDITOR
		[NonSerialized] private SerializableEvent animationEventCopy = new();
#endif

		public override void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			base.OnBeforeSerialize();
			animationEventCopy.Copy(AnimationEvent);
#endif
		}

		public override void OnAfterDeserialize()
		{
#if UNITY_EDITOR
			base.OnAfterDeserialize();
			if (NotifyIfChanged(AnimationEvent, animationEventCopy, nameof(AnimationEvent)))
				animationEventCopy.Copy(AnimationEvent);
#endif
		}
	}
}
