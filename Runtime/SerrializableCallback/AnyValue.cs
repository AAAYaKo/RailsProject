using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;

//Based on https://gist.github.com/adammyhre/e5318c8c9811264f0cabdd793b796529
namespace Rails.Runtime.Callback
{
	[Serializable]
	public struct AnyValue : IEquatable<AnyValue>
	{
		[SerializeField, DontCreateProperty] private ValueType type;

		// Storage for different types of values
		[SerializeField, DontCreateProperty] private bool boolValue;
		[SerializeField, DontCreateProperty] private int intValue;
		[SerializeField, DontCreateProperty] private float floatValue;
		[SerializeField, DontCreateProperty] private string stringValue;
		[SerializeField, DontCreateProperty] private Vector2 vector2Value;
		[SerializeField, DontCreateProperty] private Vector3 vector3Value;

		public ValueType Type
		{
			readonly get => type;
			set => type = value;
		}
		public bool BoolValue
		{
			readonly get => boolValue;
			set => boolValue = value;
		}
		public int IntValue
		{
			readonly get => intValue;
			set => intValue = value;
		}
		public float FloatValue
		{
			readonly get => floatValue;
			set
			{
				if (!Mathf.Approximately(floatValue, value))
					floatValue = value;
			}
		}
		public string StringValue
		{
			readonly get => stringValue;
			set => stringValue = value;
		}
		public Vector2 Vector2Value
		{
			readonly get => vector2Value;
			set
			{
				if (!VectorComparer.Instance.Equals(vector2Value, value))
					vector2Value = value;
			}
		}
		public Vector3 Vector3Value
		{
			readonly get => vector3Value;
			set
			{
				if (!VectorComparer.Instance.Equals(vector3Value, value))
					vector3Value = value;
			}
		}


		// Implicit conversion operators to convert AnyValue to different types
		public static implicit operator bool(AnyValue value) => value.ConvertValue<bool>();
		public static implicit operator int(AnyValue value) => value.ConvertValue<int>();
		public static implicit operator float(AnyValue value) => value.ConvertValue<float>();
		public static implicit operator string(AnyValue value) => value.ConvertValue<string>();
		public static implicit operator Vector2(AnyValue value) => value.ConvertValue<Vector2>();
		public static implicit operator Vector3(AnyValue value) => value.ConvertValue<Vector3>();
		public static bool operator ==(AnyValue left, AnyValue right) => left.Equals(right);
		public static bool operator !=(AnyValue left, AnyValue right) => !(left == right);


		public readonly T ConvertValue<T>() => type switch
		{
			_ when typeof(T) == typeof(object) => CastToObject<T>(),
			ValueType.Int => AsInt<T>(intValue),
			ValueType.Float => AsFloat<T>(floatValue),
			ValueType.Bool => AsBool<T>(boolValue),
			ValueType.String => (T)(object)stringValue,
			ValueType.Vector2 => AsVector2<T>(vector2Value),
			ValueType.Vector3 => AsVector3<T>(vector3Value),
			_ => throw new InvalidCastException($"Cannot convert AnyValue of type {type} to {typeof(T).Name}")
		};

		public static Type TypeOf(ValueType valueType) => valueType switch
		{
			ValueType.Bool => typeof(bool),
			ValueType.Int => typeof(int),
			ValueType.Float => typeof(float),
			ValueType.String => typeof(string),
			ValueType.Vector3 => typeof(Vector3),
			_ => throw new NotSupportedException($"Unsupported ValueType: {valueType}")
		};

		public static ValueType ValueTypeOf(Type type) => type switch
		{
			_ when type == typeof(bool) => ValueType.Bool,
			_ when type == typeof(int) => ValueType.Int,
			_ when type == typeof(float) => ValueType.Float,
			_ when type == typeof(string) => ValueType.String,
			_ when type == typeof(Vector2) => ValueType.Vector2,
			_ when type == typeof(Vector3) => ValueType.Vector3,
			_ => throw new NotSupportedException($"Unsupported type: {type}"),
		};

		public static bool IsSupported(Type type) => type switch
		{
			_ when type == typeof(bool) => true,
			_ when type == typeof(int) => true,
			_ when type == typeof(float) => true,
			_ when type == typeof(string) => true,
			_ when type == typeof(Vector2) => true,
			_ when type == typeof(Vector3) => true,
			_ => false,
		};

		public readonly override bool Equals(object obj)
		{
			return obj is AnyValue value && Equals(value);
		}

		public readonly bool Equals(AnyValue other)
		{
			if (Type != other.Type)
				return false;

			return Type switch
			{
				ValueType.Int => IntValue == other.IntValue,
				ValueType.Float => Mathf.Approximately(FloatValue, other.FloatValue),
				ValueType.Bool => BoolValue == other.BoolValue,
				ValueType.String => StringValue == other.StringValue,
				ValueType.Vector2 => Utils.Approximately(Vector2Value, other.Vector2Value),
				ValueType.Vector3 => Utils.Approximately(Vector2Value, other.Vector2Value),
				_ => false,
			};
		}

		public readonly override int GetHashCode()
		{
			return HashCode.Combine(Type, BoolValue, IntValue, FloatValue, StringValue, Vector2Value, Vector3Value);
		}

		// Helper methods for safe type conversions of the value types without the cost of boxing
		private readonly T AsBool<T>(bool value) => typeof(T) == typeof(bool) && value is T correctType ? correctType : default;
		private readonly T AsInt<T>(int value) => typeof(T) == typeof(int) && value is T correctType ? correctType : default;
		private readonly T AsFloat<T>(float value) => typeof(T) == typeof(float) && value is T correctType ? correctType : default;
		private readonly T AsVector2<T>(Vector2 value) => typeof(T) == typeof(Vector2) && value is T correctType ? correctType : default;
		private readonly T AsVector3<T>(Vector3 value) => typeof(T) == typeof(Vector3) && value is T correctType ? correctType : default;

		private readonly T CastToObject<T>() => type switch
		{
			ValueType.Int => (T)(object)intValue,
			ValueType.Float => (T)(object)floatValue,
			ValueType.Bool => (T)(object)boolValue,
			ValueType.String => (T)(object)stringValue,
			ValueType.Vector3 => (T)(object)vector3Value,
			_ => throw new InvalidCastException($"Cannot convert AnyValue of type {type} to {typeof(T).Name}")
		};
	}

	public enum ValueType
	{
		None,
		Int,
		Float,
		Bool,
		String,
		Vector2,
		Vector3,
	}
}