using Unity.Mathematics;
using UnityEngine;

namespace Rails.Runtime
{
	public static class Utils
	{
		public static bool Approximately(in Vector2 first, in Vector2 second)
		{
			return Mathf.Approximately(first.x, second.x) && Mathf.Approximately(first.y, second.y);
		}

		public static bool Approximately(in Vector3 first, in Vector3 second)
		{
			return Mathf.Approximately(first.x, second.x) && Mathf.Approximately(first.y, second.y) && Mathf.Approximately(first.z, second.z);
		}

		public static bool Approximately(in float4 a, in float4 b)
		{
			for (int i = 0; i < 4; i++)
			{
				if (!Mathf.Approximately(a[i], b[i]))
					return false;
			}
			return true;
		}

		public static bool SplineEquals(Vector2[] first, Vector2[] second)
		{
			if (first == null && second != null || first != null && second == null)
				return false;

			if (first == null && second == null)
				return true;

			if (first.Length != second.Length)
				return false;

			for (int i = 0; i < first.Length; i++)
			{
				if (!Approximately(first[i], second[i]))
					return false;
			}
			return true;
		}
	}
}