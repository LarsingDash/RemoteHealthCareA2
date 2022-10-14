using System.Windows.Input;

namespace ClientApplication.ViewModel;

public class ChatViewModel:ViewModelBase
{
	public ChatViewModel()
	{
		startChat = new ViewModelCommand(startChatExecute);
	}

	private static void startChatExecute(object obj)
	{
		
	}

	public ICommand startChat
	{
		get;
	}
}