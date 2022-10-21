using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace ClientApplication.Model;

public class DataViewModel
{
	public DataViewModel()
	{

		//creating users (test data)
		this.messages = new ObservableCollection<MessageModel>();
	}
	
	public ObservableCollection<MessageModel> messages { get; set; }
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