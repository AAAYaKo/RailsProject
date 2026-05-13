using System;
using Unity.Properties;

namespace Rails.Runtime.Drivers
{
	[Serializable]
	public abstract class BaseRailsDriver<T> : IRailsDriver<T>
		where T : struct
	{
		[NonSerialized, DontCreateProperty] protected T? storedValue;


		public T Value()
		{
			storedValue ??= ComputeValue();
			return storedValue.Value;
		}

		public abstract T ComputeValue();

		public void UpdateValue()
		{
			storedValue = ComputeValue();
		}
	}
}
