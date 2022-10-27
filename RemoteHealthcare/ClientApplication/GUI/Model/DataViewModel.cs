using System.Collections.ObjectModel;
using Caliburn.Micro;
using DoctorApplication.Core;

namespace ClientApplication.Model;

public class DataViewModel : ObservableObject
{
	public DataViewModel(ObservableCollection<MessageModel> messages)
	{

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
		messages.Add(new MessageModel("Doktor", message));
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