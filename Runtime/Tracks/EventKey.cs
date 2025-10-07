using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Events;

namespace Rails.Runtime
{
	[Serializable]
	public class EventKey : BaseKey
	{
		[SerializeField] private UnityEvent animationEvent = new();

		[CreateProperty]
		public UnityEvent AnimationEvent
		{
			get => animationEvent;
			set
			{
				if (animationEvent == value)
					return;
				animationEvent = value;
				NotifyPropertyChanged(nameof(AnimationEvent));
			}
		}


#if UNITY_EDITOR
		private UnityEvent animationEventCopy;

		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			animationEventCopy = AnimationEvent;
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			if (animationEventCopy != AnimationEvent)
				NotifyPropertyChanged(nameof(AnimationEvent));
		}
#endif
	}
}
