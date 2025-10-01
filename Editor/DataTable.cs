using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Editor
{
	public abstract class DataTable<T> : ISerializationCallbackReceiver
	{
		[SerializeField] protected List<Record> records = new();

		public event Action<string> RecordChanged;
		protected readonly Dictionary<string, T> cacheTable = new();


		public void Set(string key, T value)
		{
			if (cacheTable.ContainsKey(key))
			{
				if (!Changed(cacheTable[key], value))
					return;
				cacheTable[key] = value;
			}
			else
			{
				cacheTable.Add(key, value);
			}
			RecordChanged?.Invoke(key);
		}

		public T Get(string key)
		{
			if (!cacheTable.TryGetValue(key, out T value))
				return default;
			return value;
		}

		public void RemoveRecordIntList(string key)
		{
			if (cacheTable.ContainsKey(key))
				cacheTable.Remove(key);
		}

		public void OnBeforeSerialize()
		{
			records.Clear();

			foreach (var record in cacheTable)
				records.Add(new Record(record.Key, record.Value));
		}

		public void OnAfterDeserialize()
		{
			HashSet<string> keys = new(records.ConvertAll(x => x.Key));

			foreach (var record in records)
			{
				if (cacheTable.ContainsKey(record.Key))
				{
					if (!Changed(cacheTable[record.Key], record.Value))
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

		protected abstract bool Changed(T value, T next);

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