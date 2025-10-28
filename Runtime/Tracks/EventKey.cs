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
		private SerializableEvent animationEventCopy = new();
#endif

#if UNITY_EDITOR
		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			animationEventCopy.Copy(AnimationEvent);
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			if (animationEventCopy != AnimationEvent)
				NotifyPropertyChanged(nameof(AnimationEvent));

			animationEventCopy.Copy(AnimationEvent);
		}
#endif
	}
}
