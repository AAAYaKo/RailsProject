using System;
using System.Collections.Generic;
using System.Linq;
using Rails.Runtime;
using UnityEngine;

namespace Rails.Editor
{
	[Serializable]
	public class DataStorage : ISerializationCallbackReceiver
	{
		[SerializeField] private List<RecordInt> recordsInt = new();
		[SerializeField] private List<RecordIntList> recordsIntList = new();

		private readonly Dictionary<string, int> cacheTableInt = new();
		private readonly Dictionary<string, List<int>> cacheTableIntList = new();

		public event Action<string> RecordIntListChanged;


		public void SetInt(string key, int value)
		{
			if (cacheTableInt.ContainsKey(key))
			{
				if (cacheTableInt[key] == value)
					return;
				int previous = cacheTableInt[key];
				cacheTableInt[key] = value;
				EventBus.Publish(new RecordIntChangedEvent(key, previous, value));
			}
			else
			{
				cacheTableInt.Add(key, value);
				EventBus.Publish(new RecordIntChangedEvent(key, 0, value));
			}
		}

		public int GetInt(string key, int defaultValue)
		{
			if (!cacheTableInt.TryGetValue(key, out int value))
				return defaultValue;
			return value;
		}

		public void RemoveRecordInt(string key)
		{
			if (cacheTableInt.ContainsKey(key))
			{
				int previous = cacheTableInt[key];
				cacheTableInt.Remove(key);
				EventBus.Publish(new RecordIntChangedEvent(key, previous, 0));
			}
		}

		public void SetIntList(string key, List<int> list)
		{
			if (cacheTableIntList.ContainsKey(key))
			{
				if (Utils.ListEquals(cacheTableIntList[key], list))
					return;
				cacheTableIntList[key] = list;
				RecordIntListChanged?.Invoke(key);
			}
			else
			{
				cacheTableIntList.Add(key, list);
			}
		}

		public List<int> GetIntList(string key)
		{
			if (!cacheTableIntList.TryGetValue(key, out List<int> list))
				return null;
			return list;
		}

		public void RemoveRecordIntList(string key)
		{
			if (cacheTableIntList.ContainsKey(key))
				cacheTableIntList.Remove(key);
		}

		public void OnBeforeSerialize()
		{
			recordsInt.Clear();
			recordsIntList.Clear();

			foreach (var record in cacheTableInt)
				recordsInt.Add(new RecordInt(record.Key, record.Value));
			foreach (var record in cacheTableIntList)
				recordsIntList.Add(new RecordIntList(record.Key, record.Value));
		}

		public void OnAfterDeserialize()
		{
			HashSet<string> keysInt = new(recordsInt.ConvertAll(x => x.Key));
			HashSet<string> keysIntList = new(recordsIntList.ConvertAll(x => x.Key));

			cacheTableInt
				.Where(x => !keysInt.Contains(x.Key))
				.ForEach(x => cacheTableInt.Remove(x.Key));
			cacheTableIntList
				.Where(x => !keysInt.Contains(x.Key))
				.ForEach(x => cacheTableInt.Remove(x.Key));

			foreach (var record in recordsInt)
			{
				if (cacheTableInt.ContainsKey(record.Key))
				{
					if (cacheTableInt[record.Key] == record.Value)
						continue;
					int previous = cacheTableInt[record.Key];
					cacheTableInt[record.Key] = record.Value;
					EventBus.Publish(new RecordIntChangedEvent(record.Key, previous, record.Value));
				}
				else
				{
					cacheTableInt.Add(record.Key, record.Value);
					EventBus.Publish(new RecordIntChangedEvent(record.Key, 0, record.Value));
				}
			}

			foreach (var record in recordsIntList)
			{
				if (cacheTableIntList.ContainsKey(record.Key))
				{
					if (cacheTableIntList[record.Key] == record.Value)
						continue;
					cacheTableIntList[record.Key] = record.Value;
					//EventBus.Publish(new RecordIntChangedEvent(record.Key));
				}
				else
				{
					cacheTableIntList.Add(record.Key, record.Value);
					//EventBus.Publish(new RecordIntChangedEvent(record.Key));
				}
			}
		}

		[Serializable]
		public class RecordInt
		{
			[SerializeField] private string key;
			[SerializeField] private int value;

			public string Key => key;
			public int Value => value;


			public RecordInt(string key, int value)
			{
				this.key = key;
				this.value = value;
			}
		}


		[Serializable]
		public class RecordIntList
		{
			[SerializeField] private string key;
			[SerializeField] private List<int> value;

			public string Key => key;
			public List<int> Value => value;


			public RecordIntList(string key, List<int> value)
			{
				this.key = key;
				this.value = value;
			}
		}
	}
}