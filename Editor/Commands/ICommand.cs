namespace Rails.Editor
{
	public interface ICommand
	{
		void Execute();
	}

	public interface ICommand<T>
	{
		void Execute(T parameter);
	}
}