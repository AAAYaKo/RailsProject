using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rails.Runtime;
using Rails.Runtime.Callback;

namespace Rails.Editor.ViewModel
{
	public class MethodOption : IEquatable<MethodOption>
	{
		private static readonly CollectionComparer<AnyValue> comparer = new();
		private static readonly StringBuilder builder = new(32);

		public static MethodOption NoFunction { get; } = new();

		public Type TargetType { get; }
		public string Method { get; }
		public string OptionName { get; }
		public AnyValue[] Parameters { get; }
		public string SelectedName => TargetType == null ? OptionName : $"{TargetType.Name}.{Method.Replace("set_", "")}";


		public MethodOption(Type targetType, string method, AnyValue[] parameters)
		{
			TargetType = targetType;
			Method = method;
			Parameters = parameters;

			builder.Clear();
			builder
				.Append(targetType.Name)
				.Append('/')
				.Append(method)
				.Append(" (");

			if (Parameters != null)
			{
				for (int i = 0; i < Parameters.Length; i++)
				{
					builder.Append(AnyValue.TypeOf(Parameters[i].Type).GetPreviewName());
					if (i < Parameters.Length - 1)
						builder.Append(", ");
				}
			}

			builder.Append(')');

			OptionName = builder.ToString();
		}

		public MethodOption(Type targetType, MethodInfo method)
		{
			TargetType = targetType;
			Method = method.Name;
			var parametersInfo = method.GetParameters();
			Parameters = new AnyValue[parametersInfo.Length];

			builder.Clear();
			builder
				.Append(targetType.Name)
				.Append('/')
				.Append(method.Name)
				.Append(" (");

			for (int i = 0; i < parametersInfo.Length; i++)
			{
				Parameters[i] = new AnyValue() { Type = AnyValue.ValueTypeOf(parametersInfo[i].ParameterType) };
				builder.Append(parametersInfo[i].ParameterType.GetPreviewName());
				if (i < parametersInfo.Length - 1)
					builder.Append(", ");
			}

			builder.Append(')');

			OptionName = builder.ToString();
		}

		public MethodOption(Type targetType, PropertyInfo property)
		{
			TargetType = targetType;
			var setter = property.GetSetMethod() ?? throw new ArgumentException("Property hasn't setter");
			Method = setter.Name;

			Parameters = new AnyValue[] { new AnyValue() { Type = AnyValue.ValueTypeOf(property.PropertyType) } };

			builder.Clear();
			OptionName = builder
				.Append(targetType.Name)
				.Append('/')
				.Append(property.PropertyType.GetPreviewName())
				.Append(' ')
				.Append(property.Name)
				.ToString();
		}

		private MethodOption()
		{
			OptionName = "No Function";
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as MethodOption);
		}

		public bool Equals(MethodOption other)
		{
			if (other is null)
				return false;
			bool equals = TargetType == other.TargetType &&
				   Method == other.Method;
			if (!equals)
				return false;

			if (Parameters == null && other?.Parameters != null || Parameters != null && other?.Parameters == null)
				return false;

			if (Parameters == other.Parameters)
				return true;

			if (Parameters.Length != other.Parameters.Length)
				return false;

			for (int i = 0; i < Parameters.Length; i++)
			{
				if (Parameters[i].Type != other.Parameters[i].Type)
					return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			HashCode hash = new();
			hash.Add(TargetType);
			hash.Add(Method);
			if (Parameters != null)
			{
				foreach (var parameter in Parameters.Select(x => x.Type))
					hash.Add(parameter);
			}
			return hash.ToHashCode();
		}

		public static bool operator ==(MethodOption left, MethodOption right)
		{
			return EqualityComparer<MethodOption>.Default.Equals(left, right);
		}

		public static bool operator !=(MethodOption left, MethodOption right)
		{
			return !(left == right);
		}
	}
}