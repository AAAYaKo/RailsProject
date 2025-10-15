using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using Object = UnityEngine.Object;


//Based on https://gist.github.com/adammyhre/e5318c8c9811264f0cabdd793b796529
namespace Rails.Runtime.Callback
{

	[Serializable]
	public class SerializableCallback : ISerializationCallbackReceiver, INotifyPropertyChanged
	{
		private static readonly CollectionComparer<AnyValue> comparer = new();

		[SerializeField] private Object targetObject;
		[SerializeField] private string methodName;
		[SerializeField] private SerializableCallbackState state = SerializableCallbackState.RuntimeOnly;
		[SerializeField] private AnyValue[] parameters;

		[CreateProperty]
		public Object TargetObject
		{
			get => targetObject;
			set
			{
				if (targetObject == value)
					return;
				targetObject = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public string MethodName
		{
			get => methodName;
			set
			{
				if (methodName == value)
					return;
				methodName = value;
				NotifyPropertyChanged();
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
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public AnyValue[] Parameters
		{
			get => parameters;
			set
			{
				if (comparer.Equals(parameters, value))
					return;
				parameters = value;
				NotifyPropertyChanged();
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;

		private Delegate cachedDelegate;


		public void Invoke() => Invoke(parameters);

		public void Invoke(params AnyValue[] args)
		{
			if (state is SerializableCallbackState.Off)
				return;
			if (state is SerializableCallbackState.RuntimeOnly && !Application.isPlaying)
				return;

			if (cachedDelegate == null)
				BuildDelegate();

			if (cachedDelegate != null)
			{
				cachedDelegate.DynamicInvoke(ConvertParameters(args));
				return;
			}
			if (targetObject != null && !string.IsNullOrEmpty(methodName))
				Debug.LogWarning($"Unable to invoke method {methodName} on {targetObject}");
			return;
		}

		public void OnBeforeSerialize()
		{

		}

		public void OnAfterDeserialize()
		{
			cachedDelegate = null;
		}

		private object[] ConvertParameters(AnyValue[] args)
		{
			if (args == null || args.Length == 0)
				return Array.Empty<object>();

			object[] convertedParams = new object[args.Length];
			for (int i = 0; i < args.Length; i++)
				convertedParams[i] = args[i].ConvertValue<object>();

			return convertedParams;
		}

		private void BuildDelegate()
		{
			cachedDelegate = null;

			if (targetObject == null || string.IsNullOrEmpty(methodName))
			{
				//Debug.LogWarning("Target object or method name is null, cannot rebuild delegate.");
				return;
			}

			Type targetType = targetObject.GetType();
			MethodInfo methodInfo = targetType
				.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (methodInfo == null)
			{
				Debug.LogWarning($"Method {methodName} not found on {targetObject}");
				return;
			}

			Type[] parameterTypes = methodInfo
				.GetParameters()
				.Select(p => p.ParameterType)
				.ToArray();
			if (parameters.Length != parameterTypes.Length)
			{
				Debug.LogWarning($"Parameter mismatch for method {methodName}");
				return;
			}

			Type delegateType = Expression.GetDelegateType(parameterTypes.Append(methodInfo.ReturnType).ToArray());
			cachedDelegate = methodInfo.CreateDelegate(delegateType, targetObject);
		}

		private void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}

	[Serializable]
	public class SerializableCallback<TReturn> : ISerializationCallbackReceiver
	{
		[SerializeField] private Object targetObject;
		[SerializeField] private string methodName;
		[SerializeField] private AnyValue[] parameters;

		[NonSerialized] Delegate cachedDelegate;
		[NonSerialized] bool isDelegateRebuilt;

		public TReturn Invoke() => Invoke(parameters);

		public TReturn Invoke(params AnyValue[] args)
		{
			if (!isDelegateRebuilt)
				BuildDelegate();

			if (cachedDelegate != null)
			{
				var result = cachedDelegate.DynamicInvoke(ConvertParameters(args));
				return (TReturn)Convert.ChangeType(result, typeof(TReturn));
			}
			Debug.LogWarning($"Unable to invoke method {methodName} on {targetObject}");
			return default;
		}

		public void OnBeforeSerialize()
		{

		}

		public void OnAfterDeserialize()
		{
			isDelegateRebuilt = false;
		}

		private object[] ConvertParameters(AnyValue[] args)
		{
			if (args == null || args.Length == 0)
				return Array.Empty<object>();

			object[] convertedParams = new object[args.Length];
			for (int i = 0; i < args.Length; i++)
				convertedParams[i] = args[i].ConvertValue<object>();

			return convertedParams;
		}

		private void BuildDelegate()
		{
			cachedDelegate = null;

			if (targetObject == null || string.IsNullOrEmpty(methodName))
			{
				Debug.LogWarning("Target object or method name is null, cannot rebuild delegate.");
				return;
			}

			Type targetType = targetObject.GetType();
			MethodInfo methodInfo = targetType
				.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (methodInfo == null)
			{
				Debug.LogWarning($"Method {methodName} not found on {targetObject}");
				return;
			}

			Type[] parameterTypes = methodInfo
				.GetParameters()
				.Select(p => p.ParameterType)
				.ToArray();
			if (parameters.Length != parameterTypes.Length)
			{
				Debug.LogWarning($"Parameter mismatch for method {methodName}");
				return;
			}

			Type delegateType = Expression.GetDelegateType(parameterTypes.Append(methodInfo.ReturnType).ToArray());
			cachedDelegate = methodInfo.CreateDelegate(delegateType, targetObject);
			isDelegateRebuilt = true;
		}
	}
}