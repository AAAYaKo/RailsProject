using System;

namespace Rails.Editor
{
	public class RelayCommand : ICommand
	{
		private Action execute;


		public RelayCommand(Action execute)
		{
			this.execute = execute;
		}

		public void Execute()
		{
			execute();
		}
	}

	public class RelayCommand<T> : ICommand<T>
	{
		public Action<T> execute;


		public RelayCommand(Action<T> execute)
		{
			this.execute = execute;
		}

		public void Execute(T parameter)
		{
			execute(parameter);
		}
	}
}