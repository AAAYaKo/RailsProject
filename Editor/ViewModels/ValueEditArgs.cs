using UnityEngine;

namespace Rails.Editor.ViewModel
{
	public class ValueEditArgs
	{
		public float SingleValue { get; }
		public Vector2 Vector2Value { get; }
		public Vector3 Vector3Value { get; }


		public ValueEditArgs(float singleValue)
		{
			SingleValue = singleValue;
		}

		public ValueEditArgs(Vector2 vector2Value)
		{
			Vector2Value = vector2Value;
		}

		public ValueEditArgs(Vector3 vector3Value)
		{
			Vector3Value = vector3Value;
		}
	}
}