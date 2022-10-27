using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Input;
using ClientApplication.Model;
using ClientApplication.ServerConnection;
using DoctorApplication.Core;

namespace ClientApplication.ViewModel;

public class ChatViewModel: ViewModelBase
{
	public string TestText { get; set; }
	DataViewModel DataViewModel { get; set; }
	public ObservableCollection<MessageModel> Messages { get; set; }
    public ChatViewModel()
	{
		Messages = new ObservableCollection<MessageModel>();
		Messages.Add(new MessageModel("Doctor", "Hello"));
		DataViewModel = new DataViewModel(Messages);
		startChat = new ViewModelCommand(startChatExecute);
	}

	public ObservableCollection<MessageModel> getMessages()
	{
		ObservableCollection<MessageModel> messages = new ObservableCollection<MessageModel>();

		return messages;
	}
private static void startChatExecute(object obj)
	{
		
	}

	public ICommand startChat
	{
		get;
	}
}