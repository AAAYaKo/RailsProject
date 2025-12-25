using System;

namespace Rails.Editor
{
	public class RelayCommand : ICommand
	{
		private Func<bool> validate;
		private Action execute;


		public RelayCommand(Action execute)
		{
			this.execute = execute;
			this.validate = null;
		}

		public RelayCommand(Action execute, Func<bool> validate)
		{
			this.execute = execute;
			this.validate = validate;
		}

		public void Execute()
		{
			execute();
		}

		public bool Validate()
		{
			if (validate == null)
				return true;
			return validate();
		}
	}

	public class RelayCommand<T> : ICommand<T>
	{
		private Func<T, bool> validate;
		public Action<T> execute;


		public RelayCommand(Action<T> execute)
		{
			this.execute = execute;
			this.validate = null;
		}

		public RelayCommand(Action<T> execute, Func<T, bool> validate)
		{
			this.execute = execute;
			this.validate = validate;
		}

		public void Execute(in T parameter)
		{
			execute(parameter);
		}

		public bool Validate(in T parameter)
		{
			if (validate == null)
				return true;
			return validate(parameter);
		}
	}
}