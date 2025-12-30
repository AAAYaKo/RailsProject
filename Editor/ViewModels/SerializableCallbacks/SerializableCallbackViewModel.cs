using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Rails.Editor.Context;
using Rails.Runtime;
using Rails.Runtime.Callback;
using Unity.Properties;
using UnityEngine;

namespace Rails.Editor.ViewModel
{
	public class SerializableCallbackViewModel : BaseNotifyPropertyViewModel<SerializableCallback>
	{
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
		public MethodOption SelectedMethod
		{
			get => selectedMethod;
			set => selectedMethod = value;
		}
		[CreateProperty]
		public List<MethodOption> MethodOptions
		{
			get => methodOptions.ToList();
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
		public ICommand<MethodOption> SelectMethodCommand
		{
			get => selectMethodCommand;
			set => SetProperty(ref selectMethodCommand, value);
		}

		private UnityEngine.Object targetObject;
		private MethodOption selectedMethod = MethodOption.NoFunction;
		private HashSet<MethodOption> methodOptions = new();
		private SerializableCallbackState? state;
		private ICommand<MethodOption> selectMethodCommand;
		private ObservableList<AnyValueViewModel> _params = new();


		public SerializableCallbackViewModel()
		{
			SelectMethodCommand = new RelayCommand<MethodOption>(x =>
			{
				EditorContext.Instance.Record("Changed Event Method");

				if (x == MethodOption.NoFunction)
				{
					model.MethodName = null;
					model.Parameters = null;
					return;
				}
				Type targetType = x.TargetType;

				if (targetType != targetObject.GetType())
				{
					if (targetObject is GameObject gameObject)
					{
						var component = gameObject.GetComponent(targetType);
						model.TargetObject = component;
					}
					else if (targetObject is UnityEngine.Component otherComponent)
					{
						if (targetType != typeof(GameObject))
						{
							var component = otherComponent.gameObject.GetComponent(targetType);
							model.TargetObject = component;
						}
						else
						{
							model.TargetObject = otherComponent.gameObject;
						}
					}
					else
					{
						Debug.LogError("Event can't change to this type");
						return;
					}
				}

				model.Parameters = x.Parameters;
				model.MethodName = x.Method;
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
			methodOptions.Clear();
			methodOptions.Add(MethodOption.NoFunction);

			if (targetObject == null)
			{
				NotifyPropertyChanged(nameof(MethodOptions));
				return;
			}

			Type targetType = TargetObject.GetType();

			if (TargetObject is GameObject gameObject)
			{
				AddMethods(targetType, FindTargetMethods(targetType));
				AddAllComponents(gameObject);
			}
			else if (TargetObject is UnityEngine.Component component)
			{
				targetType = typeof(GameObject);
				AddMethods(targetType, FindTargetMethods(targetType));
				AddAllComponents(component.gameObject);
			}
			else
			{
				AddMethods(targetType, FindTargetMethods(targetType));
			}

			NotifyPropertyChanged(nameof(MethodOptions));

			void AddAllComponents(GameObject gameObject)
			{
				var components = gameObject.GetComponents<UnityEngine.Component>();
				foreach (var component in components)
				{
					targetType = component.GetType();
					AddMethods(targetType, FindTargetMethods(targetType));
				}
			}

			void AddMethods(Type targetType, IEnumerable<MethodOption> methods)
			{
				methods.ForEach(x => methodOptions.Add(x));
			}

			static IEnumerable<MethodOption> FindTargetMethods(Type targetType)
			{
				var allMethods = targetType
					.GetMethods(BindingFlags.Instance | BindingFlags.Public)
					.Where(x => x.ReturnType == typeof(void))
					.Where(x =>
					{
						if (x.IsSpecialName)
							return false;
						ParameterInfo[] parameters = x.GetParameters();
						if (parameters.Length == 0)
							return true;
						return parameters.All(x => AnyValue.IsSupported(x.ParameterType));
					})
					.Select(x => new MethodOption(targetType, x));

				var allProperties = targetType
					.GetProperties(BindingFlags.Instance | BindingFlags.Public)
					.Where(x =>
					{
						var setter = x.GetSetMethod();
						if (setter == null || !setter.IsPublic)
							return false;

						return AnyValue.IsSupported(x.PropertyType);
					})
					.Select(x => new MethodOption(targetType, x));

				return allProperties
					.Union(allMethods);
			}
		}

		private void UpdateSelectedMethod()
		{
			if (targetObject == null)
			{
				selectedMethod = MethodOption.NoFunction;
				NotifyPropertyChanged(nameof(SelectedMethod));
				model.MethodName = null;
				model.Parameters = null;
				return;
			}

			Type targetType = TargetObject.GetType();

			if (model.MethodName.IsNullOrEmpty())
			{
				selectedMethod = MethodOption.NoFunction;
				model.Parameters = null;
			}
			else
			{
				MethodOption searchToken = new MethodOption(targetType, model.MethodName, model.Parameters);
				if (methodOptions.TryGetValue(searchToken, out MethodOption option))
				{
					selectedMethod = option;
				}
				else
				{
					selectedMethod = MethodOption.NoFunction;
					model.MethodName = null;
					model.Parameters = null;
				}
			}

			NotifyPropertyChanged(nameof(SelectedMethod));
		}
	}
}