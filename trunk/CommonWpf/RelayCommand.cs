using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ClientCommonWpf
{
	public class RelayCommand : ICommand
	{
		private readonly Predicate<object> m_canExecute;
		private readonly Action<object> m_execute;

		public RelayCommand(Action<object> _execute)
			: this(_execute, null)
		{
		}

		public RelayCommand(Action<object> _execute, Predicate<object> _canExecute)
		{
			if (_execute == null)
				throw new ArgumentNullException("_execute");

			m_execute = _execute;
			m_canExecute = _canExecute;
		}

		#region ICommand Members

		[DebuggerStepThrough]
		public bool CanExecute(object _parameter)
		{
			return m_canExecute == null || m_canExecute(_parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object _parameter)
		{
			m_execute(_parameter);
		}

		#endregion
	}
}