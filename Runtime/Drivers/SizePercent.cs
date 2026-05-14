using System;
using UnityEngine;

namespace Rails.Runtime.Drivers
{
	[Serializable]
	public class SizePercent : BaseRailsDriver<Vector2>
	{
		[SerializeField] private Vector2 percent;

		public override Vector2 ComputeValue(UnityEngine.Object reference)
		{
			if (reference == null)
				return Vector2.zero;
			if (reference is RectTransform rect)
			{
				var size = rect.rect.size;
				return new Vector2(size.x * percent.x / 100, size.y * percent.y / 100);
			}
			return Vector2.zero;
		}
	}
}
