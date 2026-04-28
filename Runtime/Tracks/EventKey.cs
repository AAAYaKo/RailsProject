using System;
using Rails.Runtime.Callback;
using Unity.Properties;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public class EventKey : BaseKey
	{
		[SerializeField, DontCreateProperty] private SerializableEvent animationEvent = new();

		[CreateProperty]
		public SerializableEvent AnimationEvent => animationEvent;
	}
}
