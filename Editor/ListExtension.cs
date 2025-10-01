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

	public class CollectionComparer<T> : IEqualityComparer<IEnumerable<T>>
	{
		private IEqualityComparer<T> comparer;


		public CollectionComparer(IEqualityComparer<T> comparer = null)
		{
			this.comparer = comparer ?? EqualityComparer<T>.Default;
		}

		public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
		{
			if (ReferenceEquals(x, y))
				return true;
			if (x == null || y == null)
				return false;
			return x.SequenceEqual(y, comparer);
		}

		public int GetHashCode(IEnumerable<T> obj)
		{
			HashCode hash = new();
			obj.ForEach(x => hash.Add(x, comparer));
			return hash.ToHashCode();
		}
	}
}