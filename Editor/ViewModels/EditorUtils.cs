using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public static class EditorUtils
	{
		private static readonly Regex vector2Check = new(
			@"^Vector2\(\s*(-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?),\s*(-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)\s*\)$",
			RegexOptions.Compiled);
		private static readonly Regex vector3Check = new(
			@"^Vector3\(\s*(-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?),\s*(-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?),\s*(-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)\s*\)$",
			RegexOptions.Compiled);
		private static readonly Regex singleCheck = new(
			@"^-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?$",
			RegexOptions.Compiled);


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

		public static string ToCopyBuffer<TValue>(in TValue value)
		{
			if (value is float single)
				return FormattableString.Invariant($"{single}");
			if (value is Vector2 vector2)
				return FormattableString.Invariant($"Vector2({vector2.x},{vector2.y})");
			if (value is Vector3 vector3)
				return FormattableString.Invariant($"Vector3({vector3.x},{vector3.y},{vector3.z})");

			return FormattableString.Invariant($"{value}");
		}

		public static bool IsOfSerializedType<TValue>(in string serrialized, out Match match)
		{
			match = singleCheck.Match(serrialized);
			if (match.Success)
				return typeof(TValue) == typeof(float);

			match = vector2Check.Match(serrialized);
			if (match.Success)
				return typeof(TValue) == typeof(Vector2);

			match = vector3Check.Match(serrialized);
			if (match.Success)
				return typeof(TValue) == typeof(Vector3);

			return false;
		}

		public static TValue FromCopyBuffer<TValue>(in string serrialized, in Match match)
		{
			if (typeof(TValue) == typeof(float))
			{
				return (TValue)(object)float.Parse(serrialized, System.Globalization.CultureInfo.InvariantCulture);
			}
			if (typeof(TValue) == typeof(Vector2))
			{
				float x = float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
				float y = float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);

				return (TValue)(object)new Vector2(x, y);
			}
			if (typeof(TValue) == typeof(Vector3))
			{
				float x = float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
				float y = float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
				float z = float.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);

				return (TValue)(object)new Vector3(x, y, z);
			}
			return default;
		}
	}
}