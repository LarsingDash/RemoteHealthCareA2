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
	public static ObservableCollection<MessageModel>? Messages { get; set; }
    public ChatViewModel()
	{
		Messages = new ObservableCollection<MessageModel>();
		DataViewModel = new DataViewModel(Messages);
		startChat = new ViewModelCommand(startChatExecute);
	}

	public ObservableCollection<MessageModel> getMessages()
	{
		ObservableCollection<MessageModel> messages = new ObservableCollection<MessageModel>();

		return messages;
	}
	/// <summary>
	/// > This function is called when the user clicks the "Start Chat" button
	/// </summary>
	/// <param name="obj">The object that was passed to the thread.</param>
	private static void startChatExecute(object obj)
	{
		
	}

	public ICommand startChat
	{
		get;
	}
}