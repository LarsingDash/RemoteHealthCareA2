using DoctorApplication.Core;
using System;
using System.ComponentModel;
using System.Security;
using System.Windows.Input;

namespace DoctorApplication.ViewModel;

public class LoginViewModel: INotifyPropertyChanged
{
	//Fields
	private string phoneNumber;
	private SecureString password;
	private string errorMessage;
	private bool isViewVisible = true;

    /* This is a method that is used to update the view when the model changes. */
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public string PhoneNumber
	{
		get { return phoneNumber; }
		set { phoneNumber = value; OnPropertyChanged("PhoneNumber"); }
	}

	public SecureString Password
	{
		get { return password; }
		set { password = value; OnPropertyChanged("Password"); }
	}

	public string ErrorMessage
	{
		get { return errorMessage; }
		set { errorMessage = value; OnPropertyChanged("ErrorMessage"); }
	}

	public bool IsViewVisible
	{
		get { return isViewVisible; }
		set { isViewVisible = value; OnPropertyChanged("IsViewVisible"); }
	}
	
	//Commands ->
	public ICommand LoginCommand { get; }
	public ICommand RecoverPasswordCommand { get; }
	public ICommand ShowPasswordCommand { get; }
	public ICommand RememberPasswordCommand { get; }
	
	//Constructor
	public LoginViewModel()
	{
		LoginCommand = new RelayCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
		RecoverPasswordCommand = new RelayCommand(p =>ExecuteRecoveryPasswordCommand("",""));
	}

	private void ExecuteRecoveryPasswordCommand(string username, string email)
	{
		//TODO : Implement recovery password logic
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
		var canLogin = true;
		if (canLogin)
		{
			IsViewVisible = false;
		}
		else
		{
			ErrorMessage = "Invalid phone number or password";
		}
	}
}