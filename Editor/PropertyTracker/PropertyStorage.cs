using System;
using System.Collections.Generic;
using Rails.Runtime;

namespace Rails.Editor.Property
{
	internal static class PropertyStorage<TContainer, TValue>
	{
		private readonly static Dictionary<SnapshotPropertyKey<TContainer>, TValue> store = new();

		public static bool CheckValueChanged(in SnapshotPropertyKey<TContainer> propertyKey, in TValue value)
		{
			if (!store.ContainsKey(propertyKey))
			{
				store.Add(propertyKey, value);
				return true;
			}

			TValue storedValue = store[propertyKey];

			bool changed = !EqualityComparer<TValue>.Default.Equals(storedValue, value);
			if (changed)
				store[propertyKey] = value;
			return changed;
		}
	}

	internal static class PropertyStorage<TContainer, TCollection, TElement>
		where TCollection : ICollection<TElement>
	{
		private static readonly CollectionComparer<TElement> comparer = new();
		private static readonly Dictionary<SnapshotPropertyKey<TContainer>, List<TElement>> store = new();


		public static bool CheckValueChanged(in SnapshotPropertyKey<TContainer> propertyKey, in TCollection collection)
		{
			if (!store.ContainsKey(propertyKey))
			{
				List<TElement> copy = collection == null ? null : new(collection);
				store.Add(propertyKey, copy);
				return true;
			}

			var storedValue = store[propertyKey];

			bool changed = !comparer.Equals(storedValue, collection);

			if (changed)
			{
				if (collection != null)
				{
					if (storedValue == null)
					{
						storedValue = new();
						store[propertyKey] = storedValue;
					}
					storedValue.Clear();
					storedValue.AddRange(collection);
				}
				else
				{
					store[propertyKey] = null;
				}
			}
			return changed;
		}
	}
}