using System;
using System.Text;
using UnityEngine;

namespace Rails.Editor.ViewModel
{
	public struct AnimationTime : IComparable<AnimationTime>, IEquatable<AnimationTime>
	{
		public const int MaxFrameValue = 100/*hours*/* 60/*minutes*/ * 60/*seconds*/ * 60/*frames*/;
		private const string format = "{0:00}";
		private static readonly StringBuilder builder = new(8);
		public int Frames
		{
			readonly get => frames;
			set
			{
				if (frames == value)
					return;
				if (value > MaxFrameValue)
					throw new ArgumentOutOfRangeException(nameof(Frames), $"Time position in frames must be less than {MaxFrameValue}");
				frames = value;
				//Formatted = FormatTime(Frames, FPS);
			}
		}
		//public int FPS
		//{
		//	get => fps;
		//	set
		//	{
		//		if (fps == value)
		//			return;
		//		if (value == 0)
		//			throw new ArgumentOutOfRangeException(nameof(FPS), "FPS can't be less than 1");
		//		fps = value;
		//		Formatted = FormatTime(Frames, FPS);
		//	}
		//}
		//public float FrameTimeInSeconds => 1f / FPS;
		//public string Formatted { get; private set; } = "00:00f";

		private int frames;
		//private int fps = 1;


		public readonly int CompareTo(AnimationTime other)
		{
			//if (this.FPS != other.FPS)
			//{
			//	float thisTime = this.Frames * FrameTimeInSeconds;
			//	float otherTime = other.Frames * FrameTimeInSeconds;
			//	return thisTime.CompareTo(otherTime);
			//}
			//else
			//{
			return this.Frames.CompareTo(other.Frames);
			//}
		}

		public readonly override bool Equals(object obj)
		{
			return obj is AnimationTime time && Equals(time);
		}

		public readonly bool Equals(AnimationTime other)
		{
			return Frames == other.Frames;
		}

		public readonly override int GetHashCode()
		{
			return HashCode.Combine(Frames);
		}

		public static bool operator ==(AnimationTime left, AnimationTime right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(AnimationTime left, AnimationTime right)
		{
			return !(left == right);
		}

		public static bool operator ==(AnimationTime left, int right)
		{
			return left.Frames == right;
		}

		public static bool operator !=(AnimationTime left, int right)
		{
			return !(left == right);
		}

		public static implicit operator int(AnimationTime param)
		{
			return param.Frames;
		}

		public readonly string FormatTime(in int fps)
		{
			int frames = Frames % fps;
			int rest = Frames / fps;
			int seconds = rest % 60;
			int minutes = rest / 60;

			builder.Clear();
			if (minutes != 0)
			{
				builder
					.AppendFormat(format, minutes)
					.Append(':');
			}
			builder
				.AppendFormat(format, seconds)
				.Append(':')
				.AppendFormat(format, frames)
				.Append('f');
			return builder.ToString();
		}

		public static bool TryParse(string input, int fps, out AnimationTime position)
		{
			position = new();

			input = input.Replace(" ", "").ToLower();
			if (input.IsNullOrEmpty() || fps <= 0)
				return false;

			string[] parts = input.Split(':');
			if (parts.Length > 3)
				return false;

			try
			{
				int minutes = 0;
				int seconds = 0;
				int frames = 0;

				bool parsed = parts.Length switch
				{
					3 => ParseThreeParts(parts, out minutes, out seconds, out frames),
					2 => ParseTwoParts(parts, out minutes, out seconds, out frames),
					1 => ParseOnePart(parts[0], out minutes, out seconds, out frames),
					_ => false
				};

				if (!parsed)
					return false;

				// Check valid values
				if (minutes < 0 || seconds < 0 || frames < 0)
					return false;

				seconds += minutes * 60;
				frames += seconds * fps;

				if (frames > MaxFrameValue)
				{
					Debug.LogWarning("Input value exceeds maximal animation duration!");
					return false;
				}

				position.Frames = frames;

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Formats: MM:SS:FF
		/// </summary>
		/// <param name="parts"></param>
		/// <param name="minutes"></param>
		/// <param name="seconds"></param>
		/// <param name="frames"></param>
		/// <returns></returns>
		private static bool ParseThreeParts(string[] parts, out int minutes, out int seconds, out int frames)
		{
			minutes = 0;
			seconds = 0;
			frames = 0;
			return ParsePart(parts[0], 'm', out minutes)
				&& ParsePart(parts[1], 's', out seconds)
				&& ParsePart(parts[2], 'f', out frames);
		}

		/// <summary>
		/// Formats: MM:SS, MM:FF, SS:FF
		/// </summary>
		/// <param name="parts"></param>
		/// <param name="minutes"></param>
		/// <param name="seconds"></param>
		/// <param name="frames"></param>
		/// <returns></returns>
		private static bool ParseTwoParts(string[] parts, out int minutes, out int seconds, out int frames)
		{
			minutes = seconds = frames = 0;

			if (parts[0].EndsWith("m"))
			{
				if (!ParsePart(parts[0], 'm', out minutes))
					return false;

				if (parts[1].EndsWith("f"))
					return ParsePart(parts[1], 'f', out frames); //MM:FF
				else
					return ParsePart(parts[1], 's', out seconds); //MM:SS
			}
			else
			{
				return ParsePart(parts[0], 's', out seconds)
					&& ParsePart(parts[1], 'f', out frames); //SS:FF
			}
		}

		/// <summary>
		/// Formats: MM, SS, FF
		/// </summary>
		/// <param name="part"></param>
		/// <param name="minutes"></param>
		/// <param name="seconds"></param>
		/// <param name="frames"></param>
		/// <returns></returns>
		private static bool ParseOnePart(string part, out int minutes, out int seconds, out int frames)
		{
			minutes = seconds = frames = 0;

			if (part.EndsWith("m"))
				return ParsePart(part, 'm', out minutes); //MM
			else if (part.EndsWith("s"))
				return ParsePart(part, 's', out seconds); //SS
			else
				return ParsePart(part, 'f', out frames); //FF
		}

		private static bool ParsePart(string part, char expectedSuffix, out int value)
		{
			value = 0;
			if (part.IsNullOrEmpty())
				return false;

			if (char.IsLetter(part[^1]))
			{
				char suffix = part[^1];
				if (suffix != expectedSuffix)
					return false;

				string numberPart = part[..^1];

				if (!int.TryParse(numberPart, out value))
					return false;
			}
			else
			{
				if (!int.TryParse(part, out value))
					return false;
			}

			return true;
		}
	}
}