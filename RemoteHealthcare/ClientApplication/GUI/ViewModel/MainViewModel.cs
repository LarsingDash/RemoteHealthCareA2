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

    /// <summary>
    /// > The function sets the CurrentChildView property to the ChatViewModel property, sets the Caption property to
    /// "Chat", and sets the Icon property to the IconChar.Message property
    /// </summary>
    /// <param name="obj">The parameter passed to the command.</param>
    private void ExecuteShowChatViewCommand(object obj)
    {
        CurrentChildView = ChatViewModel;
        Caption = "Chat";
        Icon = IconChar.Message;
    }

    /// <summary>
    /// It sets the CurrentChildView property to a new instance of the HomeViewModel class, and sets the Caption and Icon
    /// properties to the values that will be displayed in the navigation bar
    /// </summary>
    /// <param name="obj">The object that is passed to the command.</param>
    private void ExecuteShowHomeViewCommand(object obj)
    {
        CurrentChildView = new HomeViewModel();
        Caption = "Dashboard";
        Icon = IconChar.Home;
    }

    /// <summary>
    /// It creates a new instance of the VRViewModel class and assigns it to the CurrentChildView property
    /// </summary>
    /// <param name="obj">The object that is passed to the command. In this case, it's null.</param>
    private void ExecuteShowVRViewCommand(object obj)
    {
        CurrentChildView = new VRViewModel();
        Caption = "VR Session";
        Icon = IconChar.Glasses;
    }
}