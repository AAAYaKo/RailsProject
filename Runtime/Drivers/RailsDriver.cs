using System;
using Unity.Properties;

namespace Rails.Runtime.Drivers
{
	public abstract class RailsDriver<T> : IDriver
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

	public interface IDriver
	{
		public void UpdateValue();
	}
}
