using System;
using System.Collections.Generic;
using System.Linq;

namespace Rails.Runtime
{
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
			foreach (var x in obj)
				hash.Add(x, comparer);
			return hash.ToHashCode();
		}
	}
}