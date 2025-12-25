using System;
using System.Collections.Generic;
using Rails.Runtime;

namespace Rails.Editor
{
	[Serializable]
	public class IntListDataTable : DataTable<List<int>>
	{
		protected override IEqualityComparer<List<int>> Comparer => comparer;

		private static readonly IEqualityComparer<List<int>> comparer = new CollectionComparer<int>();


		public override void OnBeforeSerialize()
		{
			records.Clear();

			foreach (var record in cacheTable)
				records.Add(new Record(record.Key, new List<int>(record.Value)));
		}
	}
}