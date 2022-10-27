using System.Collections.ObjectModel;
using Caliburn.Micro;
using DoctorApplication.Core;

namespace ClientApplication.Model;

public class DataViewModel : ObservableObject
{
	public static DataViewModel model;
	public DataViewModel(ObservableCollection<MessageModel> messages)
	{
		model = this;

		//creating users (test data)
		this.messages = messages;
		
	}
	private ObservableCollection<MessageModel> messages;
	public ObservableCollection<MessageModel> Messages {
		get { return messages; } set
		{
			messages = value;
			OnPropertyChanged();
		}
	}
	public void AddMessage(string message)
	{
		Messages.Add(new MessageModel("", message));
		Messages = Messages;
		OnPropertyChanged(nameof(Messages));
		OnPropertyChanged(nameof(messages));
	}
	
	private BindableCollection<UserModel> users;
	

	public BindableCollection<UserModel> Users
	{
		get { return users; }
		set
		{
			users = value;
		}
	}
}