using System;
using UnityEngine;

namespace Rails.Runtime.Drivers
{
	[Serializable]
	public class CopyTargetAnchorPosition : BaseRailsDriver<Vector2>
	{
		[SerializeField] private RectTransform target;


		public override Vector2 ComputeValue(UnityEngine.Object reference)
		{
			if (target == null)
				return Vector2.zero;
			return target.anchoredPosition;
		}
	}
}
