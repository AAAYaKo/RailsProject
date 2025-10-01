using System;
using System.Collections.Generic;
using System.Linq;
using Rails.Runtime;
using UnityEngine;

namespace Rails.Editor
{
	[Serializable]
	public class DataStorage
	{
		[SerializeField] private IntDataTable recordsInt = new();
		//[SerializeField] private List<RecordIntList> recordsIntList = new();

		public IntDataTable RecordsInt => recordsInt;
	}
}