using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Rails.Runtime
{
	public static class Utils
	{
		public static bool Approximately(Vector2 first, Vector2 second)
		{
			return Mathf.Approximately(first.x, second.x) && Mathf.Approximately(first.y, second.y);
		}

		public static bool Approximately(float4 a, float4 b)
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

		public static bool ListEquals<T>(List<T> first, List<T> second) where T : class
		{
			if (first == null && second != null || first != null && second == null)
				return false;

			if (first == null && second == null)
				return true;

			if (first.Count != second.Count)
				return false;

			for (int i = 0; i < first.Count; i++)
			{
				if (first[i] != second[i])
					return false;
			}
			return true;
		}

		public static bool ListEquals(List<int> first, List<int> second)
		{
			if (first == null && second != null || first != null && second == null)
				return false;

			if (first == null && second == null)
				return true;

			if (first.Count != second.Count)
				return false;

			for (int i = 0; i < first.Count; i++)
			{
				if (first[i] != second[i])
					return false;
			}
			return true;
		}
	}
}