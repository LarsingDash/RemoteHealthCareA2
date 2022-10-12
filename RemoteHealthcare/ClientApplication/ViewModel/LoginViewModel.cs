using System;
using System.Windows.Input;

namespace ClientApplication.ViewModel;

public class LoginViewModel: ViewModelBase
{
	//Fields
	private string phoneNumber;
	private string password;
	private string errorMessage;
	private bool isViewVisable = true;

	public string PhoneNumber
	{
		get { return phoneNumber; }
		set { phoneNumber = value; OnPropertyChanged("PhoneNumber"); }
	}

	public string Password
	{
		get { return password; }
		set { password = value; OnPropertyChanged("Password"); }
	}

	public string ErrorMessage
	{
		get { return errorMessage; }
		set { errorMessage = value; OnPropertyChanged("ErrorMessage"); }
	}

	public bool IsViewVisable
	{
		get { return isViewVisable; }
		set { isViewVisable = value; OnPropertyChanged("IsViewVisable"); }
	}
	
	//Commands ->
	public ICommand LoginCommand { get; }
	public ICommand RecoverPasswordCommand { get; }
	public ICommand ShowPasswordCommand { get; }
	public ICommand RememberPasswordCommand { get; }
	
	//Constructor
	public LoginViewModel()
	{
		LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
		RecoverPasswordCommand = new ViewModelCommand(p =>ExecuteRecoveryPasswordCommand("",""));
	}

	private void ExecuteRecoveryPasswordCommand(string username, string email)
	{
		throw new NotImplementedException();
	}

	private bool CanExecuteLoginCommand(object obj)
	{
		bool validData;
		if (string.IsNullOrWhiteSpace(PhoneNumber) || PhoneNumber.Length < 3 ||
		    Password == null || Password.Length < 3) {
			validData = false;	
		}
		else {
			validData = true;
		}
		return validData;
	}

	private void ExecuteLoginCommand(object obj)
	{
		throw new NotImplementedException();
	}
}