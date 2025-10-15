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
	}
}
