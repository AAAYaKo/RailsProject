using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.Runtime.Drivers
{
	[Serializable]
	public class Random : BaseRailsDriver<float>, ISerializationCallbackReceiver
	{
		[SerializeField] private float min = 0;
		[SerializeField] private float max = 1;

		[NonSerialized] private UnityEngine.Object reference;


		public override float ComputeValue(UnityEngine.Object reference)
		{
			this.reference = reference;
			return UnityEngine.Random.Range(min, max);
		}

		public void OnAfterDeserialize()
		{
			if (reference is MaskableGraphic or CanvasGroup)
			{
				min = math.max(min, 0);
				max = math.min(max, 1);
				if (min > 1)
					min = 1;
				if (max < 0)
					max = 0;
				if (min > max)
					min = max;
			}
		}

		public void OnBeforeSerialize()
		{
		}
	}
}
