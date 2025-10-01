using System;

namespace Rails.Editor
{
	[Serializable]
	public class IntDataTable : DataTable<int>
	{
		protected override bool Changed(int value, int next)
		{
			return value != next;
		}
	}
}