using System;
using System.Windows.Input;
namespace ClientApplication.ViewModel;

public class ViewModelCommand: ICommand
{
	//Fields
	private readonly Action<object> _executeAction;
	private readonly Predicate<object> _canExecuteAction;
	
	//Constructor
	public ViewModelCommand(Action<object> executeAction)
	{
		_executeAction = executeAction;
		_canExecuteAction = null;
	}

	public ViewModelCommand(Action<object> executeAction, Predicate<object> canExecuteAction)
	{
		_executeAction = executeAction;
		_canExecuteAction = canExecuteAction;
	}

	//Events
	public event EventHandler? CanExecuteChanged
	{
		add { CommandManager.RequerySuggested += value; }
		remove { CommandManager.RequerySuggested -= value; }
	}

	//Methods
	/// <summary>
	/// If the _canExecuteAction is null, then return true, otherwise return the result of the _canExecuteAction
	/// </summary>
	/// <param name="parameter">The parameter passed to the command.</param>
	/// <returns>
	/// The return value is a boolean.
	/// </returns>
	public bool CanExecute(object? parameter)
	{
		return _canExecuteAction == null ? true : _canExecuteAction(parameter);
	}

	/// <summary>
	/// If the command can execute, then execute it
	/// </summary>
	/// <param name="parameter">The parameter passed to the command.</param>
	public void Execute(object? parameter)
	{
		_executeAction(parameter);
	}
}