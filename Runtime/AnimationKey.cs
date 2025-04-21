using System;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public class AnimationKey
	{
		[SerializeField] private int timePosition;
		[SerializeField] private RailsEase ease;
		[SerializeField] private float singleValue;
		[SerializeField] private Vector3 vector3Value;
		[SerializeField] private Vector2 vector2Value;

		/// <summary>
		/// Time position in frames
		/// </summary>
		public int TimePosition { get => timePosition; set => timePosition = value; }
		public RailsEase Ease { get => ease; set => ease = value; }
		public float SingleValue { get => singleValue; set => singleValue = value; }
		public Vector3 Vector3Value { get => vector3Value; set => vector3Value = value; }
		public Vector2 Vector2Value { get => vector2Value; set => vector2Value = value; }
	}
}
