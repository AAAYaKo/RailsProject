using Rails.Editor.ViewModel;
using Rails.Runtime;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class AnimationValueControl : BaseView
	{
		public static readonly BindingId FloatValueProperty = nameof(SingleValue);
		public static readonly BindingId Vector2ValueProperty = nameof(Vector2Value);
		public static readonly BindingId Vector3ValueProperty = nameof(Vector3Value);
		public static readonly BindingId ValueEditCommandProperty = nameof(ValueEditCommand);
		public static readonly BindingId ConstrainedProportionsProperty = nameof(ConstrainedProportions);
		public static readonly BindingId ConstrainedProportionsChangeCommandProperty = nameof(ConstrainedProportionsChangeCommand);

		[UxmlAttribute("label"), CreateProperty]
		public string Label
		{
			get => label;
			set
			{
				if (label == value)
					return;
				label = value;
				if (singleField != null)
					singleField.label = label;
				if (vector2Field != null)
					vector2Field.label = label;
				if (vector3Field != null)
					vector3Field.label = label;
			}
		}
		[UxmlAttribute("type"), CreateProperty]
		public AnimationTrack.ValueType ValueType
		{
			get => type;
			set
			{
				if (type == value)
					return;
				type = value;
				UpdateValueView();
				constrainedToggle.style.display = value is AnimationTrack.ValueType.Single ? DisplayStyle.None : DisplayStyle.Flex;
			}
		}
		[UxmlAttribute("singleValue"), CreateProperty]
		public float SingleValue
		{
			get => singleValue ?? 0;
			set
			{
				if (singleValue == value)
					return;
				singleValue = value;
				singleField?.SetValueWithoutNotify(singleValue.Value);
				NotifyPropertyChanged(FloatValueProperty);
			}
		}
		[UxmlAttribute("vector2Value"), CreateProperty]
		public Vector2 Vector2Value
		{
			get => vector2Value ?? Vector2.zero;
			set
			{
				if (vector2Value == value)
					return;
				vector2Value = value;
				vector2Field?.SetValueWithoutNotify(vector2Value.Value);
				NotifyPropertyChanged(Vector2ValueProperty);
			}
		}
		[UxmlAttribute("vector3Value"), CreateProperty]
		public Vector3 Vector3Value
		{
			get => vector3Value ?? Vector3.zero;
			set
			{
				if (vector3Value == value)
					return;
				vector3Value = value;
				vector3Field?.SetValueWithoutNotify(vector3Value.Value);
				NotifyPropertyChanged(Vector3ValueProperty);
			}
		}
		[UxmlAttribute("constrained"), CreateProperty]
		public bool ConstrainedProportions
		{
			get => constrainedProportions;
			set
			{
				if (constrainedProportions == value)
					return;
				constrainedProportions = value;
				UpdateFields();
				NotifyPropertyChanged(ConstrainedProportionsProperty);
			}
		}

		[CreateProperty]
		public ICommand<ValueEditArgs> ValueEditCommand { get; set; }
		[CreateProperty]
		public ICommand<bool> ConstrainedProportionsChangeCommand { get; set; }

		private string label;
		private float? singleValue;
		private Vector2? vector2Value;
		private Vector3? vector3Value;
		private AnimationTrack.ValueType type = AnimationTrack.ValueType.Single;
		private FloatField singleField;
		private Vector2Field vector2Field;
		private Vector3Field vector3Field;
		private Toggle constrainedToggle;
		private Vector2 vector2Proportions;
		private Vector3 vector3Proportions;
		private bool constrainedProportions;

		public AnimationValueControl()
		{
			style.flexDirection = FlexDirection.RowReverse;

			constrainedToggle = new Toggle();
			constrainedToggle.AddToClassList("constrained-toggle");
			constrainedToggle.style.display = DisplayStyle.None;

			singleField = new FloatField();
			singleField.style.flexGrow = 1;
			singleField.style.flexShrink = 1;
			singleField.RegisterValueChangedCallback(OnValueChanged);
			singleField.SetValueWithoutNotify(SingleValue);

			hierarchy.Add(constrainedToggle);
			hierarchy.Add(singleField);
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			singleField?.RegisterValueChangedCallback(OnValueChanged);
			vector2Field?.RegisterValueChangedCallback(OnValueChanged);
			vector3Field?.RegisterValueChangedCallback(OnValueChanged);
			constrainedToggle.RegisterValueChangedCallback(OnConstrainedToggle);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			singleField?.UnregisterValueChangedCallback(OnValueChanged);
			vector2Field?.UnregisterValueChangedCallback(OnValueChanged);
			vector3Field?.UnregisterValueChangedCallback(OnValueChanged);
			constrainedToggle.UnregisterValueChangedCallback(OnConstrainedToggle);
		}

		private void UpdateValueView()
		{
			if (ValueType is AnimationTrack.ValueType.Single)
			{
				if (singleField == null)
				{
					RemoveVector2Field();
					RemoveVector3Field();
					singleField = new FloatField(Label);
					singleField.style.flexGrow = 1;
					singleField.style.flexShrink = 1;
					hierarchy.Add(singleField);
					singleField.RegisterValueChangedCallback(OnValueChanged);
					this.Query<FloatField>().ForEach(x => x.isDelayed = true);
				}
				singleField.SetValueWithoutNotify(SingleValue);
			}
			else if (ValueType is AnimationTrack.ValueType.Vector2)
			{
				if (vector2Field == null)
				{
					RemoveSingleField();
					RemoveVector3Field();
					vector2Field = new Vector2Field(Label);
					vector2Field.style.flexGrow = 1;
					vector2Field.style.flexShrink = 1;
					vector2Field
						.Q(className: "unity-composite-field__field-spacer")
						.RemoveFromHierarchy();
					hierarchy.Add(vector2Field);
					vector2Field.RegisterValueChangedCallback(OnValueChanged);
					this.Query<FloatField>().ForEach(x => x.isDelayed = true);
				}
				vector2Field.SetValueWithoutNotify(Vector2Value);
			}
			else if (ValueType is AnimationTrack.ValueType.Vector3)
			{
				if (vector3Field == null)
				{
					RemoveSingleField();
					RemoveVector2Field();
					vector3Field = new Vector3Field(Label);
					vector3Field.style.flexGrow = 1;
					vector3Field.style.flexShrink = 1;
					hierarchy.Add(vector3Field);
					vector3Field.RegisterValueChangedCallback(OnValueChanged);
					this.Query<FloatField>().ForEach(x => x.isDelayed = true);
				}
				vector3Field.SetValueWithoutNotify(Vector3Value);
			}
		}

		private void RemoveSingleField()
		{
			if (singleField != null)
			{
				singleField.UnregisterValueChangedCallback(OnValueChanged);
				hierarchy.Remove(singleField);
			}
		}

		private void RemoveVector2Field()
		{
			if (vector2Field != null)
			{
				vector2Field.UnregisterValueChangedCallback(OnValueChanged);
				hierarchy.Remove(vector2Field);
			}
		}

		private void RemoveVector3Field()
		{
			if (vector3Field != null)
			{
				vector3Field.UnregisterValueChangedCallback(OnValueChanged);
				hierarchy.Remove(vector3Field);
			}
		}

		private void OnValueChanged(ChangeEvent<float> evt)
		{
			ValueEditCommand.Execute(new ValueEditArgs(evt.newValue));
		}

		private void OnValueChanged(ChangeEvent<Vector2> evt)
		{
			Vector2 newValue = evt.newValue;
			if (ConstrainedProportions)
			{
				float factor = 0;
				factor = SelectConstrainedFactor(evt.previousValue.x, evt.newValue.x, vector2Proportions.x, factor);
				factor = SelectConstrainedFactor(evt.previousValue.y, evt.newValue.y, vector2Proportions.y, factor);
				newValue = factor * vector2Proportions;
			}
			ValueEditCommand.Execute(new ValueEditArgs(newValue));
		}

		private void OnValueChanged(ChangeEvent<Vector3> evt)
		{
			Vector3 newValue = evt.newValue;
			if (ConstrainedProportions)
			{
				float factor = 0;
				factor = SelectConstrainedFactor(evt.previousValue.x, evt.newValue.x, vector3Proportions.x, factor);
				factor = SelectConstrainedFactor(evt.previousValue.y, evt.newValue.y, vector3Proportions.y, factor);
				factor = SelectConstrainedFactor(evt.previousValue.z, evt.newValue.z, vector3Proportions.z, factor);
				newValue = factor * vector3Proportions;
			}
			ValueEditCommand.Execute(new ValueEditArgs(newValue));
		}

		private float SelectConstrainedFactor(float previousValue, float newValue, float proportion, float currentFactor)
		{
			if (Mathf.Approximately(previousValue, newValue))
				return currentFactor;
			return newValue / proportion;
		}

		private void OnConstrainedToggle(ChangeEvent<bool> evt)
		{
			ConstrainedProportionsChangeCommand.Execute(evt.newValue);
			UpdateFields();
		}

		private void UpdateFields()
		{
			constrainedToggle.SetValueWithoutNotify(ConstrainedProportions);
			if (vector2Field != null)
			{
				if (ConstrainedProportions)
				{
					vector2Proportions = Vector2Value.normalized;
					if (Utils.Approximately(vector2Proportions, Vector2.zero))
						vector2Proportions = Vector2.one;
				}
				vector2Field.Q<FloatField>("unity-x-input").SetEnabled(!IsLocked(vector2Proportions.x));
				vector2Field.Q<FloatField>("unity-y-input").SetEnabled(!IsLocked(vector2Proportions.y));
			}
			else if (vector3Field != null)
			{
				if (ConstrainedProportions)
				{
					vector3Proportions = Vector3Value.normalized;
					if (Utils.Approximately(vector3Proportions, Vector3.zero))
						vector3Proportions = Vector3.one;
				}
				vector3Field.Q<FloatField>("unity-x-input").SetEnabled(!IsLocked(vector3Proportions.x));
				vector3Field.Q<FloatField>("unity-y-input").SetEnabled(!IsLocked(vector3Proportions.y));
				vector3Field.Q<FloatField>("unity-z-input").SetEnabled(!IsLocked(vector3Proportions.z));
			}

			bool IsLocked(float proportion)
			{
				return ConstrainedProportions && Mathf.Approximately(proportion, 0);
			}
		}
	}
}