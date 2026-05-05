namespace Rails.Runtime.Drivers
{
	public interface IRailsDriver<T> : IDriver
	where T : struct
	{
		public T Value();
		public T ComputeValue();
	}

	public interface IDriver
	{
		public void UpdateValue();
	}
}
