using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public static class EditorUtils
	{
		private static readonly Regex regex = new(@"[^0-9:]", RegexOptions.Compiled);
		private static readonly StringBuilder builder = new();
		private const string format = "{0:00}";


		public static bool TryReadTimeValue(string value, int fps, out int frames)
		{
			frames = 0;
			if (regex.IsMatch(value))
				return false;
			string[] parts = value.Split(':');
			if (parts.Length == 1)
			{
				int time = int.Parse(parts[0]);
				if (time < 0)
					return false;
				frames = time;
				return true;
			}

			int[] times = new int[]
			{
				int.Parse(parts[0]),
				int.Parse(parts[1]),
			};

			if (times.Any(x => x < 0))
				return false;

			frames = times[0] * fps + times[1];
			return true;
		}

		public static string FormatTime(int timeFrames, int fps)
		{
			int frames = timeFrames % fps;
			int seconds = timeFrames / fps;
			builder
				.Clear()
				.AppendFormat(format, seconds)
				.Append(':')
				.AppendFormat(format, frames);

			return builder.ToString();
		}

		public static DisplayStyle ToDisplay(this bool value) => value ? DisplayStyle.Flex : DisplayStyle.None;

		public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
	}
}