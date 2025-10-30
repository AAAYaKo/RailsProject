using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
using Object = UnityEngine.Object;


//Based on https://gist.github.com/adammyhre/e5318c8c9811264f0cabdd793b796529
namespace Rails.Runtime.Callback
{

	[Serializable]
	public class SerializableCallback : BaseSerializableNotifier
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
			set => SetProperty(ref targetObject, value);
		}
		[CreateProperty]
		public string MethodName
		{
			get => methodName;
			set => SetProperty(ref methodName, value);
		}
		[CreateProperty]
		public SerializableCallbackState State
		{
			get => state;
			set => SetProperty(ref state, value);
		}
		[CreateProperty]
		public AnyValue[] Parameters
		{
			get => parameters;
			set => SetProperty(ref parameters, value, comparer);
		}

		private Delegate cachedDelegate;
#if UNITY_EDITOR
		private Object targetObjectCopy;
		private string methodNameCopy;
		private SerializableCallbackState stateCopy;
		private AnyValue[] parametersCopy;
#endif

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

		public override void OnBeforeSerialize()
		{
			targetObjectCopy = TargetObject;
			methodNameCopy = MethodName;
			stateCopy = State;
			CopyArray(Parameters, ref parametersCopy);
		}

		public override void OnAfterDeserialize()
		{
			cachedDelegate = null;
			if (NotifyIfChanged(TargetObject, targetObjectCopy, nameof(TargetObject)))
				targetObjectCopy = TargetObject;
			if (NotifyIfChanged(MethodName, methodNameCopy, nameof(MethodName)))
				methodNameCopy = MethodName;
			if (NotifyIfChanged(State, stateCopy, nameof(State)))
				stateCopy = State;
			if (NotifyIfChanged(Parameters, parametersCopy, nameof(Parameters), comparer))
				CopyArray(Parameters, ref parametersCopy);
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
	}

	[Serializable] //Not full realized
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