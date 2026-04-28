using System;
using System.Collections.Generic;

namespace Rails.Editor.Property
{
	internal readonly struct SnapshotPropertyKey<TContainer> : IEquatable<SnapshotPropertyKey<TContainer>>
	{
		public TContainer Container { get; }
		public string Property { get; }


		public SnapshotPropertyKey(TContainer container, string property)
		{
			Container = container;
			Property = property;
		}

		public override bool Equals(object obj)
		{
			return obj is SnapshotPropertyKey<TContainer> key && Equals(key);
		}

		public bool Equals(SnapshotPropertyKey<TContainer> other)
		{
			return EqualityComparer<TContainer>.Default.Equals(Container, other.Container) &&
				   Property == other.Property;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Container, Property);
		}

		public static bool operator ==(SnapshotPropertyKey<TContainer> left, SnapshotPropertyKey<TContainer> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SnapshotPropertyKey<TContainer> left, SnapshotPropertyKey<TContainer> right)
		{
			return !(left == right);
		}
	}
}