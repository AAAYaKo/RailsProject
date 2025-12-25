using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Editor
{
	public class DataTable<T> : ISerializationCallbackReceiver
	{
		[SerializeField] protected List<Record> records = new();

		public event Action<string> RecordChanged;
		protected readonly Dictionary<string, T> cacheTable = new();
		protected virtual IEqualityComparer<T> Comparer => defaultComparer;
		private static readonly IEqualityComparer<T> defaultComparer = EqualityComparer<T>.Default;

		public void Set(string key, T value)
		{
			if (cacheTable.ContainsKey(key))
			{
				if (Comparer.Equals(cacheTable[key], value))
					return;
				cacheTable[key] = value;
			}
			else
			{
				cacheTable.Add(key, value);
			}
			RecordChanged?.Invoke(key);
		}

		public T Get(string key, T defaultValue = default)
		{
			if (!cacheTable.TryGetValue(key, out T value))
				return defaultValue;
			return value;
		}

		public void RemoveRecordIntList(string key)
		{
			if (cacheTable.ContainsKey(key))
				cacheTable.Remove(key);
		}

		public virtual void OnBeforeSerialize()
		{
			records.Clear();

			foreach (var record in cacheTable)
				records.Add(new Record(record.Key, record.Value));
		}

		public virtual void OnAfterDeserialize()
		{
			HashSet<string> keys = new(records.ConvertAll(x => x.Key));

			foreach (var record in records)
			{
				if (cacheTable.ContainsKey(record.Key))
				{
					if (Comparer.Equals(cacheTable[record.Key], record.Value))
						continue;
					T previous = cacheTable[record.Key];
					cacheTable[record.Key] = record.Value;
					RecordChanged?.Invoke(record.Key);
				}
				else
				{
					cacheTable.Add(record.Key, record.Value);
					RecordChanged?.Invoke(record.Key);
				}
			}
		}

		[Serializable]
		public class Record
		{
			[SerializeField] private string key;
			[SerializeField] private T value;

			public string Key => key;
			public T Value => value;


			public Record(string key, T value)
			{
				this.key = key;
				this.value = value;
			}
		}
	}
}