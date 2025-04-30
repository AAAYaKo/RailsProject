using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Editor.ViewModel
{
	[Serializable]
	public class ObservableList<T> : IList<T>
	{
		[SerializeField] private List<T> list = new();
		public event Action ListChanged;

		public T this[int index]
		{
			get => list[index];
			set
			{
				list[index] = value;
				ListChanged?.Invoke();
			}
		}

		public int Count => list.Count;

		public bool IsReadOnly => false;

		public void Add(T item)
		{
			list.Add(item);
			ListChanged?.Invoke();
		}

		public void AddWithoutNotify(T item)
		{
			list.Add(item);
		}

		public void Clear()
		{
			list.Clear();
			ListChanged?.Invoke();
		}

		public void ClearWithoutNotify()
		{
			list.Clear();
		}

		public bool Contains(T item)
		{
			return list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			list.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		public int IndexOf(T item)
		{
			return list.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			list.Insert(index, item);
			ListChanged?.Invoke();
		}

		public void InsertWithoutNotify(int index, T item)
		{
			list.Insert(index, item);
		}

		public bool Remove(T item)
		{
			bool result = list.Remove(item);
			ListChanged.Invoke();
			return result;
		}

		public bool RemoveWithoutNotify(T item)
		{
			return list.Remove(item);
		}

		public void RemoveAt(int index)
		{
			list.RemoveAt(index);
			ListChanged?.Invoke();
		}

		public void RemoveAtWithoutNotify(int index)
		{
			list.RemoveAt(index);
		}

		public void RemoveAll(Predicate<T> predicate)
		{
			list.RemoveAll(predicate);
			ListChanged?.Invoke();
		}

		public void RemoveAllWithoutNotify(Predicate<T> predicate)
		{
			list.RemoveAll(predicate);
		}

		public void InsertRange(int index, IEnumerable<T> collection)
		{
			list.InsertRange(index, collection);
			ListChanged?.Invoke();
		}

		public void InsertRangeWithoutNotify(int index, IEnumerable<T> collection)
		{
			list.InsertRange(index, collection);
		}

		public void RemoveRange(int index, int count)
		{
			list.RemoveRange(index, count);
			ListChanged?.Invoke();
		}

		public void RemoveRangeWithoutNotify(int index, int count)
		{
			list.RemoveRange(index, count);
		}

		public void NotifyListChanged()
		{
			ListChanged?.Invoke();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public static class ListExtension
	{
		public static bool IsNullOrEmpty<T>(this IList<T> list)
		{
			return list == null || list.Count == 0;
		}
	}
}