using System.Collections.Generic;
using Rails.Runtime;
using Unity.Properties;
using UnityEditor;

namespace Rails.Editor.Property
{
	public static class ChangeChecker
	{
		private static readonly SnapshotVisitor visitor = new();
		private static readonly HashSet<RailsAnimator> entries = new();


		static ChangeChecker()
		{
			EditorApplication.update += OnUpdate;
		}

		public static void Register(RailsAnimator entry)
		{
			if (entries.Contains(entry))
				return;
			entries.Add(entry);
		}

		public static void Unregister(RailsAnimator entry)
		{
			entries.Remove(entry);
		}

		private static void OnUpdate()
		{
			foreach (var entry in entries)
			{
				if (entry != null)
					PropertyContainer.Accept(visitor, entry);
			}
		}
	}
}