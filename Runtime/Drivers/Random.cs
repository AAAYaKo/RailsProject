using System;
using UnityEngine;

namespace Rails.Runtime.Drivers
{
	[Serializable]
	public class Random : BaseRailsDriver<float>
	{
		[SerializeField] private float min = 0;
		[SerializeField] private float max = 1;


		public override float ComputeValue()
		{
			return UnityEngine.Random.Range(min, max);
		}
	}
}
