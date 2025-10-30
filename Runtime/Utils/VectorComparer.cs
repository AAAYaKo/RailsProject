using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Rails.Runtime
{
	public class VectorComparer : IEqualityComparer<float4>, IEqualityComparer<Vector2>, IEqualityComparer<Vector3>
	{
		public static VectorComparer Instance { get; } = new();

		public bool Equals(float4 x, float4 y)
		{
			return Utils.Approximately(x, y);
		}

		public bool Equals(Vector2 x, Vector2 y)
		{
			return Utils.Approximately(x, y);
		}

		public bool Equals(Vector3 x, Vector3 y)
		{
			return Utils.Approximately(x, y);
		}

		public int GetHashCode(float4 obj)
		{
			return obj.GetHashCode();
		}

		public int GetHashCode(Vector2 obj)
		{
			return obj.GetHashCode();
		}

		public int GetHashCode(Vector3 obj)
		{
			return obj.GetHashCode();
		}
	}
}