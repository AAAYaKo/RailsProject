using Rails.Runtime.Callback;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class AnyValueControl : BaseView
	{
		public static readonly BindingId ValueTypeProperty = new(nameof(ValueType));
		public static readonly BindingId BoolValueProperty = new(nameof(BoolValue));
		public static readonly BindingId IntValueProperty = new(nameof(IntValue));
		public static readonly BindingId FloatValueProperty = new(nameof(FloatValue));
		public static readonly BindingId StringValueProperty = new(nameof(StringValue));
		public static readonly BindingId Vector2ValueProperty = new(nameof(Vector2Value));
		public static readonly BindingId Vector3ValueProperty = new(nameof(Vector3Value));
		public static readonly BindingId ChangeParamCommandProperty = new(nameof(ChangeParamCommand));

		[CreateProperty]
		public ValueType ValueType
		{
			get => valueType ?? ValueType.Int;
			set
			{
				if (valueType == value)
					return;

				ClearField();
				valueType = value;
				switch (ValueType)
				{
					case ValueType.Int:
						intField = new IntegerField();
						hierarchy.Add(intField);
						intField.RegisterValueChangedCallback(ValueChangedHandler);
						intField.SetValueWithoutNotify(IntValue);
						break;
					case ValueType.Float:
						floatField = new FloatField();
						hierarchy.Add(floatField);
						floatField.RegisterValueChangedCallback(ValueChangedHandler);
						floatField.SetValueWithoutNotify(FloatValue);
						break;
					case ValueType.Bool:
						boolField = new Toggle();
						hierarchy.Add(boolField);
						boolField.RegisterValueChangedCallback(ValueChangedHandler);
						boolField.SetValueWithoutNotify(BoolValue);
						break;
					case ValueType.String:
						textField = new TextField();
						hierarchy.Add(textField);
						textField.RegisterValueChangedCallback(ValueChangedHandler);
						textField.SetValueWithoutNotify(StringValue);
						break;
					case ValueType.Vector2:
						vector2Field = new Vector2Field();
						hierarchy.Add(vector2Field);
						vector2Field.RegisterValueChangedCallback(ValueChangedHandler);
						vector2Field.SetValueWithoutNotify(Vector2Value);
						break;
					case ValueType.Vector3:
						vector3Field = new Vector3Field();
						hierarchy.Add(vector3Field);
						vector3Field.RegisterValueChangedCallback(ValueChangedHandler);
						vector3Field.SetValueWithoutNotify(Vector3Value);
						break;
					default:
						break;
				}
			}
		}
		[CreateProperty]
		public bool BoolValue
		{
			get => boolValue ?? false;
			set
			{
				if (boolValue == value)
					return;
				boolValue = value;
				boolField?.SetValueWithoutNotify(value);
			}
		}
		[CreateProperty]
		public int IntValue
		{
			get => intValue ?? 0;
			set
			{
				if (intValue == value)
					return;
				intValue = value;
				intField?.SetValueWithoutNotify(value);
			}
		}
		[CreateProperty]
		public float FloatValue
		{
			get => floatValue ?? 0;
			set
			{
				if (floatValue == value)
					return;
				floatValue = value;
				floatField?.SetValueWithoutNotify(value);
			}
		}
		[CreateProperty]
		public string StringValue
		{
			get => stringValue;
			set
			{
				if (stringValue == value)
					return;
				stringValue = value;
				textField?.SetValueWithoutNotify(stringValue);
			}
		}
		[CreateProperty]
		public Vector2 Vector2Value
		{
			get => vector2Value ?? Vector2.zero;
			set
			{
				if (vector2Value == value)
					return;
				vector2Value = value;
				vector2Field?.SetValueWithoutNotify(value);
			}
		}
		[CreateProperty]
		public Vector3 Vector3Value
		{
			get => vector3Value ?? Vector3.zero;
			set
			{
				if (vector3Value == value)
					return;
				vector3Value = value;
				vector3Field?.SetValueWithoutNotify(value);
			}
		}
		[CreateProperty]
		public ICommand<AnyValue> ChangeParamCommand { get; set; }

		private Toggle boolField;
		private IntegerField intField;
		private FloatField floatField;
		private TextField textField;
		private Vector2Field vector2Field;
		private Vector3Field vector3Field;
		private ValueType? valueType;
		private bool? boolValue;
		private int? intValue;
		private float? floatValue;
		private string stringValue;
		private Vector2? vector2Value;
		private Vector3? vector3Value;


		public AnyValueControl()
		{
			AddToClassList("any-value-control");
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			ClearField();
		}

		private void ClearField()
		{
			switch (ValueType)
			{
				case ValueType.Int:
					if (intField != null)
					{
						hierarchy.Remove(intField);
						intField.UnregisterValueChangedCallback(ValueChangedHandler);
						intField = null;
					}
					break;
				case ValueType.Float:
					if (floatField != null)
					{
						hierarchy.Remove(floatField);
						floatField.UnregisterValueChangedCallback(ValueChangedHandler);
						floatField = null;
					}
					break;
				case ValueType.Bool:
					if (boolField != null)
					{
						hierarchy.Remove(boolField);
						boolField.UnregisterValueChangedCallback(ValueChangedHandler);
						boolField = null;
					}
					break;
				case ValueType.String:
					if (textField != null)
					{
						hierarchy.Remove(textField);
						textField.UnregisterValueChangedCallback(ValueChangedHandler);
						textField = null;
					}
					break;
				case ValueType.Vector2:
					if (vector2Field != null)
					{
						hierarchy.Remove(vector2Field);
						vector2Field.UnregisterValueChangedCallback(ValueChangedHandler);
						vector2Field = null;
					}
					break;
				case ValueType.Vector3:
					if (vector3Field != null)
					{
						hierarchy.Remove(vector3Field);
						vector3Field.UnregisterValueChangedCallback(ValueChangedHandler);
						vector3Field = null;
					}
					break;
				default:
					break;
			}
		}

		private void ValueChangedHandler(ChangeEvent<int> evt)
		{
			ChangeParamCommand?.Execute(new AnyValue() { Type = ValueType, IntValue = evt.newValue });
		}

		private void ValueChangedHandler(ChangeEvent<float> evt)
		{
			ChangeParamCommand?.Execute(new AnyValue() { Type = ValueType, FloatValue = evt.newValue });
		}

		private void ValueChangedHandler(ChangeEvent<bool> evt)
		{
			ChangeParamCommand?.Execute(new AnyValue() { Type = ValueType, BoolValue = evt.newValue });
		}

		private void ValueChangedHandler(ChangeEvent<string> evt)
		{
			ChangeParamCommand?.Execute(new AnyValue() { Type = ValueType, StringValue = evt.newValue });
		}

		private void ValueChangedHandler(ChangeEvent<Vector2> evt)
		{
			ChangeParamCommand?.Execute(new AnyValue() { Type = ValueType, Vector2Value = evt.newValue });
		}

		private void ValueChangedHandler(ChangeEvent<Vector3> evt)
		{
			ChangeParamCommand?.Execute(new AnyValue() { Type = ValueType, Vector3Value = evt.newValue });
		}
	}
}