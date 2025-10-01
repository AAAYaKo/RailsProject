using System;

namespace Rails.Editor
{
	public class StoredValue<TTable, TValue> where TTable : DataTable<TValue>
	{
		public TValue Value
		{
			get => value;
			set
			{
				table.Set(key, value);
			}
		}

		public event Action<TValue> ValueChanged;

		private string key;
		private TValue value;
		private TTable table;


		public StoredValue(string key)
		{
			this.key = key;
		}

		public void Bind(TTable table)
		{
			value = table.Get(key);
			this.table = table;
			table.RecordChanged += OnRecordChanged;
		}

		public void Unbind()
		{
			table.RecordChanged -= OnRecordChanged;
			table = null;
		}

		private void OnRecordChanged(string key)
		{
			if (key != this.key)
				return;
			value = table.Get(key);
			ValueChanged?.Invoke(value);
		}
	}

	public class StoredInt : StoredValue<IntDataTable, int>
	{
		public StoredInt(string key) : base(key)
		{
		}
	}
}