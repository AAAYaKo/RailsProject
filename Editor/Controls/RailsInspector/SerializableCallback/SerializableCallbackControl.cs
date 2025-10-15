using System.Collections.Generic;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Rails.Runtime.Callback;
using Unity.Properties;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class SerializableCallbackControl : BaseView
	{
		public static readonly BindingId MethodOptionsProperty = nameof(MethodOptions);
		public static readonly BindingId SelectedMethodProperty = nameof(SelectedMethod);
		public static readonly BindingId TargetObjectProperty = nameof(TargetObject);
		public static readonly BindingId StateProperty = nameof(State);
		public static readonly BindingId ParamsProperty = nameof(Params);
		public static readonly BindingId SelectMethodCommandProperty = nameof(SelectMethodCommand);
		private static readonly CollectionComparer<string> methodsComparer = new();

		[CreateProperty]
		public List<string> MethodOptions
		{
			get => methodOptions;
			set
			{
				if (methodsComparer.Equals(methodOptions, value))
					return;
				methodOptions = value;
				methodField.choices = value;
			}
		}
		[CreateProperty]
		public string SelectedMethod
		{
			get => selectedMethod;
			set
			{
				if (selectedMethod == value)
					return;
				selectedMethod = value;
				methodField.SetValueWithoutNotify(value);
			}
		}
		[CreateProperty]
		public UnityEngine.Object TargetObject
		{
			get => targetObject;
			set
			{
				if (targetObject == value)
					return;
				targetObject = value;
				targetField.SetValueWithoutNotify(value);
				NotifyPropertyChanged(TargetObjectProperty);
			}
		}
		[CreateProperty]
		public SerializableCallbackState State
		{
			get => state;
			set
			{
				if (state == value)
					return;
				state = value;
				stateField.SetValueWithoutNotify(value);
				NotifyPropertyChanged(StateProperty);
			}
		}
		[CreateProperty]
		public ObservableList<AnyValueViewModel> Params
		{
			get => _params;
			set
			{
				if (value == _params)
					return;
				if (_params != null)
					_params.ListChanged -= OnParamsListChanged;
				_params = value;
				_params.ListChanged += OnParamsListChanged;
				paramsContainer.itemsSource = value;
			}
		}

		[CreateProperty]
		public ICommand<string> SelectMethodCommand { get; set; }

		private DropdownField methodField;
		private EnumField stateField;
		private ObjectField targetField;
		private ListView paramsContainer;
		private List<string> methodOptions;
		private string selectedMethod;
		private Object targetObject;
		private SerializableCallbackState state = SerializableCallbackState.RuntimeOnly;
		private ObservableList<AnyValueViewModel> _params;

		public SerializableCallbackControl()
		{
			style.flexDirection = FlexDirection.Row;
			VisualElement left = new();
			VisualElement right = new();

			methodField = new DropdownField();
			methodField.formatSelectedValueCallback = x =>
			{
				if (x.IsNullOrEmpty())
					return x;
				string[] parts = x.Split('/');
				return parts[^1];
			};

			paramsContainer = new ListView();
			paramsContainer.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
			paramsContainer.selectionType = SelectionType.None;
			paramsContainer.allowAdd = false;
			paramsContainer.allowRemove = false;
			paramsContainer.reorderable = false;
			paramsContainer.bindingSourceSelectionMode = BindingSourceSelectionMode.Manual;
			paramsContainer.makeItem = () =>
			{
				AnyValueControl control = new();
				control.SetBinding(AnyValueControl.ValueTypeProperty, new ToTargetBinding(nameof(AnyValueViewModel.Type)));
				control.SetBinding(AnyValueControl.BoolValueProperty, new ToTargetBinding(nameof(AnyValueViewModel.BoolValue)));
				control.SetBinding(AnyValueControl.IntValueProperty, new ToTargetBinding(nameof(AnyValueViewModel.IntValue)));
				control.SetBinding(AnyValueControl.FloatValueProperty, new ToTargetBinding(nameof(AnyValueViewModel.FloatValue)));
				control.SetBinding(AnyValueControl.StringValueProperty, new ToTargetBinding(nameof(AnyValueViewModel.StringValue)));
				control.SetBinding(AnyValueControl.Vector2ValueProperty, new ToTargetBinding(nameof(AnyValueViewModel.Vector2Value)));
				control.SetBinding(AnyValueControl.Vector3ValueProperty, new ToTargetBinding(nameof(AnyValueViewModel.Vector3Value)));
				control.SetBinding(AnyValueControl.ChangeParamCommandProperty, new ToTargetBinding(nameof(AnyValueViewModel.ChangeParamCommand)));
				return control;
			};
			paramsContainer.makeNoneElement = () =>
			{
				return new VisualElement();
			};
			paramsContainer.bindItem = (element, index) =>
			{
				element.dataSource = Params[index];
			};

			stateField = new EnumField();
			stateField.Init(SerializableCallbackState.RuntimeOnly);
			targetField = new ObjectField();
			targetField.objectType = typeof(UnityEngine.Object);

			left.Add(stateField);
			left.Add(targetField);
			left.style.width = new Length(35, LengthUnit.Percent);

			right.Add(methodField);
			right.Add(paramsContainer);
			right.style.width = new Length(65, LengthUnit.Percent);

			hierarchy.Add(left);
			hierarchy.Add(right);
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			if (_params != null)
				_params.ListChanged += OnParamsListChanged;

			targetField.RegisterValueChangedCallback(OnObjectChanged);
			methodField.RegisterValueChangedCallback(OnMethodChanged);
			stateField.RegisterValueChangedCallback(OnStateChanged);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			if (_params != null)
				_params.ListChanged -= OnParamsListChanged;

			targetField.UnregisterValueChangedCallback(OnObjectChanged);
			methodField.UnregisterValueChangedCallback(OnMethodChanged);
			stateField.UnregisterValueChangedCallback(OnStateChanged);
		}

		private void OnParamsListChanged()
		{
			paramsContainer.RefreshItems();
		}

		private void OnObjectChanged(ChangeEvent<Object> evt)
		{
			TargetObject = evt.newValue;
		}

		private void OnMethodChanged(ChangeEvent<string> evt)
		{
			SelectMethodCommand.Execute(evt.newValue);
		}

		private void OnStateChanged(ChangeEvent<System.Enum> evt)
		{
			State = (SerializableCallbackState)evt.newValue;
		}
	}
}