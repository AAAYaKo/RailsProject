using UnityEngine;

namespace Rails.Editor
{
	public static class Utils
	{
		public static bool Approximately(Vector2 first, Vector2 second)
		{
			return Mathf.Approximately(first.x, second.x) && Mathf.Approximately(first.y, second.y);
		}

		public static bool SpliteEquals(Vector2[] first, Vector2[] second)
		{
			if (first == null && second != null || first != null && second == null)
				return false;

			if (first == null && second == null)
				return true;

			if (first?.Length != second.Length)
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