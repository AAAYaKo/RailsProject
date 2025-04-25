using UnityEngine;
using UnityEngine.Events;

namespace Rails.Runtime
{
	public class EventKey
	{
		[SerializeField] private int timePosition;
		[SerializeField] private UnityEvent animationEvent = new();

		/// <summary>
		/// Time position in frames
		/// </summary>
		public int TimePosition { get => timePosition; set => timePosition = value; }
		public UnityEvent AnimationEvent { get => animationEvent; set => animationEvent = value; }
	}
}
