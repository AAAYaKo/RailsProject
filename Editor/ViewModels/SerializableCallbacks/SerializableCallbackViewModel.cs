using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Rails.Runtime;
using Rails.Runtime.Callback;
using Unity.Properties;
using UnityEngine;

namespace Rails.Editor.ViewModel
{
	public class SerializableCallbackViewModel : BaseNotifyPropertyViewModel<SerializableCallback>
	{
		private const string NoFunction = "No Function";
		private static readonly CollectionComparer<AnyValueViewModel> comparer = new();

		[CreateProperty]
		public UnityEngine.Object TargetObject
		{
			get => targetObject;
			set
			{
				if (SetProperty(ref targetObject, value))
				{
					EditorContext.Instance.Record("Changed Event Target");
					model.TargetObject = value;
				}
			}
		}
		[CreateProperty]
		public string SelectedMethod
		{
			get => selectedMethod;
			set => selectedMethod = value;
		}
		[CreateProperty]
		public List<string> MethodOptions
		{
			get => methodOptions;
			set => methodOptions = value;
		}
		[CreateProperty]
		public SerializableCallbackState State
		{
			get => state ?? SerializableCallbackState.Off;
			set
			{
				if (SetProperty(ref state, value))
				{
					EditorContext.Instance.Record("Changed Event State");
					model.State = value;
				}
			}
		}
		[CreateProperty]
		public ObservableList<AnyValueViewModel> Params => _params;
		[CreateProperty]
		public ICommand<string> SelectMethodCommand
		{
			get => selectMethodCommand;
			set => SetProperty(ref selectMethodCommand, value);
		}

		private UnityEngine.Object targetObject;
		private string selectedMethod = NoFunction;
		private List<string> methodOptions = new();
		private SerializableCallbackState? state;
		private ICommand<string> selectMethodCommand;
		private ObservableList<AnyValueViewModel> _params = new();
		private Dictionary<Type, MethodInfo[]> methodsTable = new();


		public SerializableCallbackViewModel()
		{
			SelectMethodCommand = new RelayCommand<string>(x =>
			{
				EditorContext.Instance.Record("Changed Event Method");

				if (x == NoFunction)
				{
					model.MethodName = null;
					model.Parameters = null;
					return;
				}

				string[] parts = x.Split('/');
				if (parts.Length != 2)
				{
					Debug.LogError("Event can't recognize type/method");
					return;
				}
				var record = methodsTable.First(x => x.Key.FullName == parts[0]);
				Type targetType = record.Key;
				MethodInfo method = record.Value.FirstOrDefault(x => x.Name == parts[1]);

				if (method == null)
				{
					Debug.LogError("Event can't change method, selected targe doesn't have such method");
					return;
				}

				if (targetType != targetObject.GetType())
				{
					if (targetObject is GameObject gameObject)
					{
						var component = gameObject.GetComponent(targetType);
						model.TargetObject = component;
					}
					else
					{
						Debug.LogError("Event can't change type, current targe is not GameObject");
						return;
					}
				}

				model.MethodName = method.Name;
				var parametersInfo = method.GetParameters();
				AnyValue[] parameters = new AnyValue[parametersInfo.Length];
				for (int i = 0; i < parameters.Length; i++)
					parameters[i].Type = AnyValue.ValueTypeOf(parametersInfo[i].ParameterType);
				model.Parameters = parameters;
			});
		}

		protected override void OnBind()
		{
			base.OnBind();
		}

		protected override void OnUnbind()
		{
			base.OnUnbind();
			ClearViewModels<AnyValueViewModel, AnyValue>(Params);
		}

		protected override void OnModelChanged()
		{
			methodOptions.Clear();
			if (model == null)
			{
				if (_params.Count > 0)
				{
					ClearViewModels<AnyValueViewModel, AnyValue>(Params);
				}
				return;
			}

			targetObject = model.TargetObject;
			NotifyPropertyChanged(nameof(TargetObject));
			state = model.State;
			NotifyPropertyChanged(nameof(State));
			UpdateParams();
			UpdateMethods();
			UpdateSelectedMethod();
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SerializableCallback.TargetObject))
			{
				targetObject = model.TargetObject;
				UpdateMethods();
				NotifyPropertyChanged(nameof(TargetObject));
			}
			else if (e.PropertyName == nameof(SerializableCallback.State))
			{
				state = model.State;
				NotifyPropertyChanged(nameof(State));
			}
			else if (e.PropertyName == nameof(SerializableCallback.Parameters))
			{
				UpdateParams();
			}
			else if (e.PropertyName == nameof(SerializableCallback.MethodName))
			{
				UpdateSelectedMethod();
			}

		}

		private void UpdateParams()
		{
			UpdateViewModels(Params, model.Parameters,
				createViewModel: x => new AnyValueViewModel());
		}

		private void UpdateMethods()
		{
			methodsTable.Clear();

			if (targetObject == null)
			{
				methodOptions.Clear();
				methodOptions.Add(NoFunction);
				NotifyPropertyChanged(nameof(MethodOptions));
				return;
			}

			Type targetType = TargetObject.GetType();
			methodsTable.Add(targetType, FindTargetMethods(targetType));

			if (TargetObject is GameObject gameObject)
			{
				var components = gameObject.GetComponents<UnityEngine.Component>();
				foreach (var component in components)
				{
					targetType = component.GetType();
					methodsTable.Add(targetType, FindTargetMethods(targetType));
				}
			}

			methodOptions.Clear();
			methodOptions.Add(NoFunction);
			methodsTable.ForEach(x => x.Value.ForEach(y => methodOptions.Add($"{x.Key.FullName}/{y.Name}")));

			NotifyPropertyChanged(nameof(MethodOptions));

			static MethodInfo[] FindTargetMethods(Type targetType)
			{
				var allMethods = targetType
					.GetMethods(BindingFlags.Instance | BindingFlags.Public)
					.Where(x =>
					{
						if (x.IsSpecialName)
							return false;
						ParameterInfo[] parameters = x.GetParameters();
						if (parameters.Length == 0)
							return true;
						return parameters.All(x => AnyValue.IsSupported(x.ParameterType));
					});

				var allProperties = targetType
					.GetProperties(BindingFlags.Instance | BindingFlags.Public)
					.Select(x => x.GetSetMethod())
					.Where(x => x != null && x.IsPublic)
					.Where(x =>
					{
						ParameterInfo[] parameters = x.GetParameters();
						if (parameters.Length == 0)
							return true;
						return parameters.All(x => AnyValue.IsSupported(x.ParameterType));
					});

				return allProperties
					.Union(allMethods)
					.ToArray();
			}
		}

		private void UpdateSelectedMethod()
		{
			if (targetObject == null)
			{
				selectedMethod = NoFunction;
				NotifyPropertyChanged(nameof(SelectedMethod));
				model.MethodName = null;
				return;
			}

			Type targetType = TargetObject.GetType();
			if (methodsTable[targetType].Any(x => x.Name == model.MethodName))
			{
				selectedMethod = $"{targetType.FullName}/{model.MethodName}";
			}
			else
			{
				selectedMethod = NoFunction;
				model.MethodName = null;
			}

			NotifyPropertyChanged(nameof(SelectedMethod));
		}
	}
}