namespace Rails.Editor
{
	public interface ICommand
	{
		bool Validate();
		void Execute();
	}

	public interface ICommand<T>
	{
		bool Validate(in T parameter);
		void Execute(in T parameter);
	}
}