using System;
using System.Collections.Generic;
using DG.Tweening;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class EaseControl : VisualElement
	{
		public static readonly BindingId FirstPointProperty = nameof(FirstPoint);
		public static readonly BindingId SecondPointProperty = nameof(SecondPoint);
		public static readonly BindingId SplineProperty = nameof(Spline);
		public static readonly BindingId HasHandlesProperty = nameof(HasHandles);
		public static readonly BindingId HasFunctionProperty = nameof(HasFunction);
		public static readonly BindingId TypeProperty = nameof(Type);
		public static readonly BindingId EaseFunctionProperty = nameof(EaseFunction);
		public static readonly BindingId EaseOptionsProperty = nameof(EaseOptions);
		public static readonly BindingId ShowEaseFoldoutProperty = nameof(ShowEaseFoldout);
		public static readonly BindingId EaseFunctionChangeCommandProperty = nameof(EaseFunctionChangeCommand);
		public static readonly BindingId EaseTypeChangeCommandProperty = nameof(EaseTypeChangeCommand);
		public static readonly BindingId FirstPointChangeCommandProperty = nameof(FirstPointChangeCommand);
		public static readonly BindingId SecondPointChangeCommandProperty = nameof(SecondPointChangeCommand);

		private static readonly CollectionComparer<Ease> easeComparer = new();
		private static readonly CollectionComparer<Vector2> comparer = new(VectorComparer.Instance);
		private static readonly VisualTreeAsset template;

		[CreateProperty]
		public bool HasHandles
		{
			get => hasHandles ?? false;
			set
			{
				if (hasHandles == value)
					return;
				hasHandles = value;
				DisplayStyle visibility = (hasHandles ?? false) ? DisplayStyle.Flex : DisplayStyle.None;
				firstPointField.style.display = visibility;
				secondPointField.style.display = visibility;
				easeView.HasHandles = value;
				NotifyPropertyChanged(HasHandlesProperty);
			}
		}
		[CreateProperty]
		public bool HasFunction
		{
			get => hasFunction ?? false;
			set
			{
				if (hasFunction == value)
					return;
				hasFunction = value;
				DisplayStyle visibility = (hasFunction ?? false) ? DisplayStyle.Flex : DisplayStyle.None;
				easeFunctionField.style.display = visibility;
				NotifyPropertyChanged(HasFunctionProperty);
			}
		}
		[CreateProperty]
		public Vector2[] Spline
		{
			get => spline;
			set
			{
				if (comparer.Equals(spline, value))
					return;
				spline = value;
				easeView.Spline = spline;

				NotifyPropertyChanged(SplineProperty);
			}
		}
		[CreateProperty]
		public Vector2 FirstPoint
		{
			get => firstPoint ?? Vector2.zero;
			set
			{
				if (firstPoint != null && Utils.Approximately(firstPoint.Value, value))
					return;

				value.x = Mathf.Clamp(value.x, 0, 1);
				firstPoint = value;
				easeView.FirstPoint = value;
				firstPointField.SetValueWithoutNotify(value);

				NotifyPropertyChanged(FirstPointProperty);
			}
		}
		[CreateProperty]
		public Vector2 SecondPoint
		{
			get => secondPoint ?? Vector2.zero;
			set
			{
				if (secondPoint != null && Utils.Approximately(secondPoint.Value, value))
					return;

				value.x = Mathf.Clamp(value.x, 0, 1);
				secondPoint = value;
				easeView.SecondPoint = value;
				secondPointField.SetValueWithoutNotify(value);

				NotifyPropertyChanged(SecondPointProperty);
			}
		}
		[CreateProperty]
		public RailsEase.EaseType Type
		{
			get => type ?? RailsEase.EaseType.NoAnimation;
			set
			{
				if (type == value)
					return;
				type = value;

				int length = Enum.GetNames(typeof(RailsEase.EaseType)).Length;
				ToggleButtonGroupState result = new(0, length);
				result.ResetAllOptions();
				result[(int)value] = true;
				easeType.SetValueWithoutNotify(result);

				NotifyPropertyChanged(TypeProperty);
			}
		}
		[CreateProperty]
		public Ease EaseFunction
		{
			get => easeFunction ?? Ease.Linear;
			set
			{
				if (easeFunction == value)
					return;
				easeFunction = value;

				easeFunctionField.SetValueWithoutNotify(value);
				NotifyPropertyChanged(EaseFunctionProperty);
			}
		}
		[CreateProperty]
		public List<Ease> EaseOptions
		{
			get => easeOptions;
			set
			{
				if (easeComparer.Equals(easeOptions, value))
					return;
				easeOptions = value;

				easeFunctionField.choices = value;
				NotifyPropertyChanged(EaseOptionsProperty);
			}
		}
		[CreateProperty]
		public bool ShowEaseFoldout
		{
			get => showEaseFoldout ?? false;
			set
			{
				if (showEaseFoldout == value)
					return;
				showEaseFoldout = value;
				content.style.display = ShowEaseFoldout ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}
		[CreateProperty]
		public ICommand<Ease> EaseFunctionChangeCommand { get; set; }
		[CreateProperty]
		public ICommand<RailsEase.EaseType> EaseTypeChangeCommand { get; set; }
		[CreateProperty]
		public ICommand<Vector2> FirstPointChangeCommand { get; set; }
		[CreateProperty]
		public ICommand<Vector2> SecondPointChangeCommand { get; set; }

		private Toggle foldoutToggle;
		private VisualElement content;
		private EaseView easeView;
		private EaseFunctionField easeFunctionField;
		private Vector2Field firstPointField;
		private Vector2Field secondPointField;
		private ToggleButtonGroup easeType;

		private bool? hasHandles;
		private bool? hasFunction;
		private bool? showEaseFoldout;
		private Vector2[] spline;
		private Vector2? firstPoint;
		private Vector2? secondPoint;
		private RailsEase.EaseType? type;
		private Ease? easeFunction;
		private List<Ease> easeOptions;


		static EaseControl()
		{
			template = Resources.Load<VisualTreeAsset>("EaseControl");
		}

		public EaseControl()
		{
			template.CloneTree(this);
			foldoutToggle = this.Q<Toggle>("foldout-toggle");
			easeType = this.Q<ToggleButtonGroup>("ease-type-field");
			content = this.Q<VisualElement>("content");
			easeView = content.Q<EaseView>();
			firstPointField = this.Q<Vector2Field>("ease-first-point");
			secondPointField = this.Q<Vector2Field>("ease-second-point");
			easeFunctionField = this.Q<EaseFunctionField>("ease-function");

			foldoutToggle.RegisterValueChangedCallback(OnFoldoutChanged);
			firstPointField.RegisterValueChangedCallback(OnFirstPointChanged);
			secondPointField.RegisterValueChangedCallback(OnSecondPointChanged);
			easeType.RegisterValueChangedCallback(OnTypeChanged);
			easeFunctionField.RegisterValueChangedCallback(OnFunctionChanged);
			easeView.FirstPointChanged += OnFirstPointChanged;
			easeView.SecondPointChanged += OnSecondPointChanged;

			AddBinding();
		}

		private void AddBinding()
		{
			SetBinding(TypeProperty, new TwoWayBinding(nameof(EaseViewModel.EaseType)));
			SetBinding(EaseOptionsProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.EaseVariants)),
				bindingMode = BindingMode.ToTargetOnce,
			});
			SetBinding(EaseFunctionProperty, new TwoWayBinding(nameof(EaseViewModel.SelectedVariant)));
			SetBinding(HasHandlesProperty, new ToTargetBinding(nameof(EaseViewModel.HasHandles)));
			SetBinding(HasFunctionProperty, new ToTargetBinding(nameof(EaseViewModel.HasFunction)));
			SetBinding(SplineProperty, new ToTargetBinding(nameof(EaseViewModel.Spline)));
			SetBinding(FirstPointProperty, new TwoWayBinding(nameof(EaseViewModel.FirstPoint)));
			SetBinding(SecondPointProperty, new TwoWayBinding(nameof(EaseViewModel.SecondPoint)));

			SetBinding(EaseFunctionChangeCommandProperty, new CommandBinding(nameof(EaseViewModel.EaseFunctionChangeCommand)));
			SetBinding(EaseTypeChangeCommandProperty, new CommandBinding(nameof(EaseViewModel.EaseTypeChangeCommand)));
			SetBinding(FirstPointChangeCommandProperty, new CommandBinding(nameof(EaseViewModel.FirstPointChangeCommand)));
			SetBinding(SecondPointChangeCommandProperty, new CommandBinding(nameof(EaseViewModel.SecondPointChangeCommand)));
		}

		private void OnFoldoutChanged(ChangeEvent<bool> evt)
		{
			ShowEaseFoldout = evt.newValue;
		}

		private void OnFirstPointChanged(ChangeEvent<Vector2> evt)
		{
			OnFirstPointChanged(evt.newValue);
		}

		private void OnFirstPointChanged(Vector2 value)
		{
			FirstPoint = value;
			FirstPointChangeCommand.Execute(value);
		}

		private void OnSecondPointChanged(ChangeEvent<Vector2> evt)
		{
			OnSecondPointChanged(evt.newValue);
		}

		private void OnSecondPointChanged(Vector2 value)
		{
			SecondPoint = value;
			SecondPointChangeCommand.Execute(value);
		}

		private void OnTypeChanged(ChangeEvent<ToggleButtonGroupState> evt)
		{
			RailsEase.EaseType value = ConvertToEaseType(evt.newValue);
			Type = value;
			EaseTypeChangeCommand.Execute(value);
		}

		private void OnFunctionChanged(ChangeEvent<Ease> evt)
		{
			EaseFunction = evt.newValue;
			EaseFunctionChangeCommand.Execute(EaseFunction);
		}

		private RailsEase.EaseType ConvertToEaseType(ToggleButtonGroupState state)
		{
			for (int i = 0; i < state.length; i++)
			{
				if (state[i])
					return (RailsEase.EaseType)i;
			}
			return RailsEase.EaseType.NoAnimation;
		}
	}
}