using System;
using System.Collections.Generic;

namespace Rails.Editor
{
	public class StoredValue<TTable, TValue> where TTable : DataTable<TValue>
	{
		public virtual TValue Value
		{
			get => value;
			set
			{
				table.Set(key, value);
			}
		}

		public event Action<TValue> ValueChanged;

		protected string key;
		protected TValue value;
		protected TTable table;


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
			if (table != null)
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

	public class StoredIntList : StoredValue<IntListDataTable, List<int>>
	{
		public override List<int> Value
		{
			get => value;
			set
			{
				table.Set(key, new List<int>(value));
			}
		}

		public StoredIntList(string key) : base(key)
		{
		}
	}

	public class StoredFloat : StoredValue<FloatDataTable, float>
	{
		public StoredFloat(string key) : base(key)
		{
		}
	}
}