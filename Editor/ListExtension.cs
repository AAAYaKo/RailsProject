using System.Collections.Generic;

namespace Rails.Editor
{
	public static class ListExtension
	{
		public static bool IsNullOrEmpty<T>(this IList<T> list)
		{
			return list == null || list.Count == 0;
		}
	}
}