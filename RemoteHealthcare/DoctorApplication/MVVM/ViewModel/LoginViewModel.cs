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
	public static LoginViewModel Model;
	private string userName;
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

    public string UserName
	{
		get { return userName; }
		set { userName = value; OnPropertyChanged("UserName"); }
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
		LoginCommand = new RelayCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
		RecoverPasswordCommand = new RelayCommand(p =>ExecuteRecoveryPasswordCommand("",""));
	}

	private void ExecuteRecoveryPasswordCommand(string username, string email)
	{
		//TODO : Implement recovery password logic
		throw new NotImplementedException();
	}

	/// <summary>
	/// If the username is null or whitespace, or if the username is less than 1 character, or if the password is null, or if
	/// the password is less than 1 character, then the data is invalid. Otherwise, the data is valid
	/// </summary>
	/// <param name="obj">The parameter is used to pass a value to the command when it is executed.</param>
	/// <returns>
	/// A boolean value.
	/// </returns>
	private bool CanExecuteLoginCommand(object obj)
	{
		bool validData;
		if (string.IsNullOrWhiteSpace(UserName) || UserName.Length < 1 ||
		    Password == null || Password.Length < 1) {
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
	/// <param name="obj">The object that is passed to the command.</param>
	private void ExecuteLoginCommand(object obj)
	{
		Client client = App.GetClientInstance();
		var serial = Util.RandomString();
		var pass = new System.Net.NetworkCredential(string.Empty, password).Password;
		client.SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
		{
			{"_serial_", serial},
			{"_username_", userName},
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
				if (ob["data"]!["error"]!.ToObject<string>()! == "Already Logged In")
				{
					IsViewVisible = false;
				}
				else
				{
					ErrorMessage = ob["data"]!["error"]!.ToObject<string>()!;
				}
			}
		}, () =>
		{
			ErrorMessage = "No response from server";
		}, 200);
	}
}