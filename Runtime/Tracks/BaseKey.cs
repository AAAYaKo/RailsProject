using System;
using Unity.Properties;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public abstract class BaseKey : IKey
	{
		[SerializeField, DontCreateProperty] private int timePosition;

		/// <summary>
		/// Time position in frames
		/// </summary>
		[CreateProperty]
		public int TimePosition
		{
			get => timePosition;
			set => timePosition = value;
		}


		public void SetTimePositionWithoutNotify(int value)
		{
			timePosition = value;
		}
	}

	public interface IKey
	{
		public int TimePosition { get; set; }
		public void SetTimePositionWithoutNotify(int value);
	}
}
