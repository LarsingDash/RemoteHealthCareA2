using System;
using System.Collections.Generic;
using System.Security;
using System.Windows.Input;
using ClientApplication.ServerConnection;
using ClientApplication.Util;
using Shared;

namespace ClientApplication.ViewModel;

public class LoginViewModel: ViewModelBase
{
	//Fields
	private string phoneNumber;
	private SecureString password;
	private string errorMessage;
	private bool isViewVisible = true;

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
		LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
		RecoverPasswordCommand = new ViewModelCommand(p =>ExecuteRecoveryPasswordCommand("",""));
	}

	private void ExecuteRecoveryPasswordCommand(string username, string email)
	{
		//TODO : Implement recovery password logic
		throw new NotImplementedException();
	}

	private bool CanExecuteLoginCommand(object obj)
	{
		bool validData;
		if (string.IsNullOrWhiteSpace(PhoneNumber) || PhoneNumber.Length < 10 || PhoneNumber.Length > 10 ||
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
		Client client = App.GetClientConnectedToServerInstance();
		var serial = Shared.Util.RandomString();
		var pass = new System.Net.NetworkCredential(string.Empty, password).Password;
		client.SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
		{
			{"_serial_", serial},
			{"_username_", phoneNumber},
			{"_type_", "Client"},
			{"_password_", pass},
			{"_bikeId_", App.GetBikeHandlerInstance().Bike.BikeId}
		}, JsonFolder.ServerConnection.Path));

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