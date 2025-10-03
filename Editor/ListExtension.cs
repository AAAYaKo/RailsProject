using System;
using System.Collections.Generic;
using System.Linq;

namespace Rails.Editor
{
	public static class ListExtension
	{
		public static bool IsNullOrEmpty<T>(this IList<T> list)
		{
			return list == null || list.Count == 0;
		}

		public static void ForEach<T>(this IEnumerable<T> list, Action<T> callback)
		{
			foreach (var item in list)
				callback?.Invoke(item);
		}
	}
}