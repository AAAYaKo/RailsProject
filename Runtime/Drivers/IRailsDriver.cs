namespace Rails.Runtime.Drivers
{
	public interface IRailsDriver<T> : IDriver
	where T : struct
	{
		public T Value(UnityEngine.Object reference);
		public T ComputeValue(UnityEngine.Object reference);
	}

	public interface IDriver
	{
		public void UpdateValue(UnityEngine.Object reference);
	}
}
