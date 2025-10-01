using System;
using System.Collections.Generic;
using Rails.Runtime;

namespace Rails.Editor
{
	[Serializable]
	public class IntListDataTable : DataTable<List<int>>
	{
		protected override bool Changed(List<int> value, List<int> next)
		{
			return !Utils.ListEquals(value, next);
		}
	}
}