using System;
using System.Collections.Generic;
using System.Linq;
using Rails.Runtime;
using UnityEngine;

namespace Rails.Editor.Context
{
	[Serializable]
	public class DataStorage
	{
		[SerializeField] private IntDataTable recordsInt = new();
		[SerializeField] private FloatDataTable recordsFloat = new();
		[SerializeField] private IntListDataTable recordsSelectedClips = new();

		public IntDataTable RecordsInt => recordsInt;
		public IntListDataTable RecordsSelectedClips => recordsSelectedClips;
		public FloatDataTable RecordsFloat => recordsFloat;
	}
}