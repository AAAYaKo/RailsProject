using Unity.Mathematics;
using UnityEngine;

namespace Rails.Editor.Controls
{
	public struct Scope
	{
		public Vector2 MinValue { get; set; }
		public Vector2 MaxValue { get; set; }
		public Vector2 MinPosition { get; set; }
		public Vector2 MaxPosition { get; set; }


		public readonly Vector2 CalculatePlace(Vector2 value, Offsets paddings)
		{
			float x = math.remap(MinValue.x, MaxValue.x, MinPosition.x + paddings.Left, MaxPosition.x + paddings.Right, value.x);
			float y = math.remap(MinValue.y, MaxValue.y, MaxPosition.y + paddings.Bottom, MinPosition.y + paddings.Top, value.y);
			return new Vector2(x, y);
		}

		public readonly Vector2 CalculateValue(Vector2 position, Offsets paddings)
		{
			float x = math.remap(MinPosition.x + paddings.Left, MaxPosition.x + paddings.Right, MinValue.x, MaxValue.x, position.x);
			float y = math.remap(MaxPosition.y + paddings.Bottom, MinPosition.y + paddings.Top, MinValue.y, MaxValue.y, position.y);
			return new Vector2(x, y);
		}
	}
}