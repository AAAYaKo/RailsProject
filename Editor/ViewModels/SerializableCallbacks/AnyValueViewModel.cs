using System.ComponentModel;
using Rails.Runtime;
using Rails.Runtime.Callback;
using Unity.Properties;
using UnityEngine;
using ValueType = Rails.Runtime.Callback.ValueType;

namespace Rails.Editor.ViewModel
{
	public class AnyValueViewModel : BaseNotifyPropertyViewModel<AnyValue>
	{
		[CreateProperty]
		public ValueType Type
		{
			get => type ?? ValueType.Int;
			set => SetProperty(ref type, value);
		}
		[CreateProperty]
		public bool BoolValue
		{
			get => boolValue ?? false;
			set => SetProperty(ref boolValue, value);
		}
		[CreateProperty]
		public int IntValue
		{
			get => intValue ?? 0;
			set => SetProperty(ref intValue, value);
		}
		[CreateProperty]
		public float FloatValue
		{
			get => floatValue ?? 0;
			set
			{
				if (Mathf.Approximately(value, FloatValue) && floatValue != null)
					return;
				floatValue = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public string StringValue
		{
			get => stringValue;
			set => SetProperty(ref stringValue, value);
		}
		[CreateProperty]
		public Vector2 Vector2Value
		{
			get => vector2Value ?? Vector2.zero;
			set
			{
				if (Utils.Approximately(Vector2Value, value) && vector2Value != null)
					return;
				vector2Value = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public Vector3 Vector3Value
		{
			get => vector3Value ?? Vector3.zero;
			set
			{
				if (Utils.Approximately(Vector3Value, value) && vector3Value != null)
					return;
				vector3Value = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public ICommand<AnyValue> ChangeParamCommand { get; }

		private ValueType? type;
		private bool? boolValue;
		private int? intValue;
		private float? floatValue;
		private string stringValue;
		private Vector2? vector2Value;
		private Vector3? vector3Value;


		public AnyValueViewModel()
		{
			ChangeParamCommand = new RelayCommand<AnyValue>(x =>
			{
				if (x.Type != Type)
				{
					Debug.LogError("Something went wrong! Param has different type");
					return;
				}

				EditorContext.Instance.Record("Changed Event Parameter");
				switch (type)
				{
					case ValueType.Int:
						if (x.IntValue == IntValue)
							return;
						model.IntValue = x.IntValue;
						break;
					case ValueType.Float:
						if (Mathf.Approximately(x.FloatValue, FloatValue))
							return;
						model.FloatValue = x.FloatValue;
						break;
					case ValueType.Bool:
						if (x.BoolValue == BoolValue)
							return;
						model.BoolValue = x.BoolValue;
						break;
					case ValueType.String:
						if (x.StringValue == StringValue)
							return;
						model.StringValue = x.StringValue;
						break;
					case ValueType.Vector2:
						if (Utils.Approximately(x.Vector2Value, Vector2Value))
							return;
						model.Vector2Value = x.Vector2Value;
						break;
					case ValueType.Vector3:
						if (Utils.Approximately(x.Vector3Value, Vector3Value))
							return;
						model.Vector3Value = x.Vector3Value;
						break;
					default:
						break;
				}
			});
		}

		protected override void OnModelChanged()
		{
			Type = model.Type;
			BoolValue = model.BoolValue;
			IntValue = model.IntValue;
			FloatValue = model.FloatValue;
			StringValue = model.StringValue;
			Vector2Value = model.Vector2Value;
			Vector3Value = model.Vector3Value;
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AnyValue.Type))
				Type = model.Type;
			else if (e.PropertyName == nameof(AnyValue.BoolValue))
				BoolValue = model.BoolValue;
			else if (e.PropertyName == nameof(AnyValue.IntValue))
				IntValue = model.IntValue;
			else if (e.PropertyName == nameof(AnyValue.FloatValue))
				FloatValue = model.FloatValue;
			else if (e.PropertyName == nameof(AnyValue.StringValue))
				StringValue = model.StringValue;
			else if (e.PropertyName == nameof(AnyValue.Vector2Value))
				Vector2Value = model.Vector2Value;
			else if (e.PropertyName == nameof(AnyValue.Vector3Value))
				Vector3Value = model.Vector3Value;
		}
	}
}