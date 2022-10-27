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
	public static LoginViewModel Model;
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
		Model = this;
		LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
		RecoverPasswordCommand = new ViewModelCommand(p =>ExecuteRecoveryPasswordCommand("",""));
	}

	private void ExecuteRecoveryPasswordCommand(string username, string email)
	{
		//TODO : Implement recovery password logic
		throw new NotImplementedException();
	}

	/// <summary>
	/// If the phone number is not null or white space, and the phone number is 10 characters long, and the password is not
	/// null, and the password is at least 3 characters long, then the data is valid
	/// </summary>
	/// <param name="obj">The parameter is used to pass the data from the view to the view model.</param>
	/// <returns>
	/// The return value is a boolean.
	/// </returns>
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

	/// <summary>
	/// It sends a login request to the server, and if the server responds with a status of "ok", then the login view is hidden
	/// </summary>
	/// <param name="obj">The object that was passed to the command.</param>
	private void ExecuteLoginCommand(object obj)
	{
		if (ErrorMessage == "Could not connect with server.")
			return;
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