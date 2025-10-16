using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public static class EditorUtils
	{
		private static readonly Dictionary<Type, string> shortNames = new()
		{
			{ typeof(bool), "bool" },
			{ typeof(float), "float" },
			{ typeof(int), "int" },
			{ typeof(string), "string" },
		};

		public static DisplayStyle ToDisplay(this bool value) => value ? DisplayStyle.Flex : DisplayStyle.None;

		public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

		public static string GetPreviewName(this Type type)
		{
			if (shortNames.TryGetValue(type, out string name))
				return name;
			return type.Name;
		}
	}
}