using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class EaseControl : VisualElement
	{
		public static readonly BindingId HasHandlesProperty = nameof(HasHandles);
		public static readonly BindingId HasFunctionProperty = nameof(HasFunction);

		[CreateProperty]
		public bool HasHandles
		{
			get => _hasHandles ?? false;
			set
			{
				if (_hasHandles != value)
				{
					_hasHandles = value;
					DisplayStyle visibility = (_hasHandles ?? false) ? DisplayStyle.Flex : DisplayStyle.None;
					_firstPoint.style.display = visibility;
					_secondPoint.style.display = visibility;
					NotifyPropertyChanged(HasHandlesProperty);
				}
			}
		}

		[CreateProperty]
		public bool HasFunction
		{
			get => _hasFunction ?? false;
			set
			{
				if (_hasFunction != value)
				{
					_hasFunction = value;
					DisplayStyle visibility = (_hasFunction ?? false) ? DisplayStyle.Flex : DisplayStyle.None;
					_easeFunction.style.display = visibility;
					NotifyPropertyChanged(HasFunctionProperty);
				}
			}
		}

		private EaseView _easeView;
		private DropdownField _easeFunction;
		private Vector2Field _firstPoint;
		private Vector2Field _secondPoint;
		private ToggleButtonGroup _easeType;

		private bool? _hasHandles;
		private bool? _hasFunction;


		public EaseControl()
		{
			_easeType = new ToggleButtonGroup();
			var button1 = new Button();
			var button2 = new Button();
			var button3 = new Button();
			button1.text = "No Animation";
			button2.text = "Curve";
			button3.text = "EaseFunc";
			_easeType.Add(button1);
			_easeType.Add(button2);
			_easeType.Add(button3);
			_easeType.AddToClassList("ease-type-field");
			_easeType.name = "ease-type-field";
			hierarchy.Add(_easeType);

			Foldout fold = new();
			fold.text = "Ease";
			_easeView = new EaseView();
			fold.Add(_easeView);
			hierarchy.Add(fold);

			_firstPoint = new Vector2Field();
			_secondPoint = new Vector2Field();
			hierarchy.Add(_firstPoint);
			hierarchy.Add(_secondPoint);
			_firstPoint.AddToClassList("ease-first-point");
			_firstPoint.name = "ease-first-point";
			_secondPoint.AddToClassList("ease-second-point");
			_secondPoint.name = "ease-second-point";

			_easeFunction = new DropdownField();
			_easeFunction.name = "ease-function";
			hierarchy.Add(_easeFunction);

			AddBinding();
		}

		private void AddBinding()
		{
			_easeType.SetBinding("value", new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.EaseType)),
				bindingMode = BindingMode.TwoWay,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			_firstPoint.SetBinding("value", new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.FirstPoint)),
				bindingMode = BindingMode.TwoWay,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			_secondPoint.SetBinding("value", new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.SecondPoint)),
				bindingMode = BindingMode.TwoWay,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			_easeFunction.SetBinding("choices", new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.EaseVariants)),
				bindingMode = BindingMode.ToTargetOnce,
			});
			_easeFunction.SetBinding("index", new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.SelectedVariant)),
				bindingMode = BindingMode.TwoWay,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			_easeView.SetBinding(EaseView.FirstPointProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.FirstPoint)),
				bindingMode = BindingMode.TwoWay,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			_easeView.SetBinding(EaseView.SecondPointProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.SecondPoint)),
				bindingMode = BindingMode.TwoWay,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			_easeView.SetBinding(EaseView.SplinePointProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.Spline)),
				bindingMode = BindingMode.ToTarget,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			_easeView.SetBinding(EaseView.HasHandlesProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.HasHandles)),
				bindingMode = BindingMode.ToTarget,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			SetBinding(HasHandlesProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.HasHandles)),
				bindingMode = BindingMode.ToTarget,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			SetBinding(HasFunctionProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(EaseViewModel.HasFunction)),
				bindingMode = BindingMode.ToTarget,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
		}
	}
}