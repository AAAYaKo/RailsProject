using System;
using Unity.Properties;

namespace Rails.Runtime.Drivers
{
	[Serializable]
	public abstract class BaseRailsDriver<T> : IRailsDriver<T>
		where T : struct
	{
		[NonSerialized, DontCreateProperty] protected T? storedValue;


		public T Value(UnityEngine.Object reference)
		{
			storedValue ??= ComputeValue(reference);
			return storedValue.Value;
		}

		public abstract T ComputeValue(UnityEngine.Object reference);

		public void UpdateValue(UnityEngine.Object reference)
		{
			storedValue = ComputeValue(reference);
		}
	}
}
