using System.Windows.Input;
using ClientApplication.Model;
using FontAwesome.Sharp;

namespace ClientApplication.ViewModel;

public class MainViewModel: ViewModelBase
{
	//Fields
    private ViewModelBase _currentChildView;
    private string _caption;
    private IconChar _icon;
    //Properties
    
    public ViewModelBase CurrentChildView
    {
        get => _currentChildView;
        set
        {
            _currentChildView = value;
            OnPropertyChanged(nameof(CurrentChildView));
        }
    }
    public string Caption
    {
        get => _caption;
        set
        {
            _caption = value;
            OnPropertyChanged(nameof(Caption));
        }
    }
    public IconChar Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged(nameof(Icon));
        }
    }
    //--> Commands
    public ICommand ShowHomeViewCommand { get; }
    public ICommand ShowVRViewCommand { get; }
    
    public ICommand ShowChatViewCommand { get; }

    public ChatViewModel ChatViewModel { get; set; }

    public MainViewModel()
    {
        ChatViewModel = new ChatViewModel();

        //Initialize commands
        ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
        ShowVRViewCommand = new ViewModelCommand(ExecuteShowVRViewCommand);
        ShowChatViewCommand = new ViewModelCommand(ExecuteShowChatViewCommand);
        //Default view
        ExecuteShowHomeViewCommand(null);
    }

    private void ExecuteShowChatViewCommand(object obj)
    {
        CurrentChildView = ChatViewModel;
        Caption = "Chat";
        Icon = IconChar.Message;
    }

    private void ExecuteShowHomeViewCommand(object obj)
    {
        CurrentChildView = new HomeViewModel();
        Caption = "Dashboard";
        Icon = IconChar.Home;
    }

    private void ExecuteShowVRViewCommand(object obj)
    {
        CurrentChildView = new VRViewModel();
        Caption = "VR Session";
        Icon = IconChar.Glasses;
    }
}