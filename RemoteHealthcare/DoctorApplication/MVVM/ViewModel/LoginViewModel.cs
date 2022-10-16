using DoctorApplication.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using System.Windows.Input;
using ClientApplication.ServerConnection;
using ClientApplication.ServerConnection.Communication;
using DoctorApplication.Communication;
using Shared;

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
		Client client = App.GetClientInstance();
		var serial = Util.RandomString();
		var pass = new System.Net.NetworkCredential(string.Empty, password).Password;
		client.SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
		{
			{"_serial_", serial},
			{"_username_", phoneNumber},
			{"_type_", "Doctor"},
			{"_password_", pass}
		}, JsonFolder.Json.Path));

		client.AddSerialCallbackTimeout(serial, ob =>
		{
			var canLogin = ob["data"]!["status"]!.ToObject<string>()!.Equals("ok");
			if (canLogin)
			{
				IsViewVisible = false;
			}
			else
			{
				ErrorMessage = ob["data"]!["error"]!.ToObject<string>()!;
			}
		}, () =>
		{
			ErrorMessage = "No response from server";
		}, 200);
	}
}